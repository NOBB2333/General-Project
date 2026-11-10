<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import {
  approveProjectCharterApi,
  getProjectDetailSectionApi,
  rejectProjectCharterApi,
  saveProjectCharterApi,
  submitProjectCharterApprovalApi,
  withdrawProjectCharterApprovalApi,
  type ProjectApi,
} from '@/api/core/project';
import { UiButton, UiFormField, UiTextarea } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

const props = defineProps<{ projectId: string }>();
const emit = defineEmits<{ changed: [] }>();

const charter = ref<ProjectApi.Charter | null>(null);
const form = reactive({
  assumptions: '',
  businessCase: '',
  constraints: '',
  objectives: '',
  scopeBoundary: '',
  successCriteria: '',
});
const approvalComment = ref('');
const loading = ref(false);
const saving = ref(false);
const error = ref('');
const notice = ref('');

const canManage = computed(() => hasAccessCode('Project.Governance.Manage'));
const canApprove = computed(() => hasAccessCode('Project.Workflow.Approve'));
const approvalStatus = computed(() => charter.value?.approvalStatus || '未创建');
const isPending = computed(() => approvalStatus.value === '待审批');
const isApproved = computed(() => approvalStatus.value === '已批准');
const canEdit = computed(() => canManage.value && !isPending.value && !isApproved.value);
const workflow = computed(() => charter.value?.approvalWorkflow ?? null);

function syncForm(value: ProjectApi.Charter | null) {
  Object.assign(form, {
    assumptions: value?.assumptions ?? '',
    businessCase: value?.businessCase ?? '',
    constraints: value?.constraints ?? '',
    objectives: value?.objectives ?? '',
    scopeBoundary: value?.scopeBoundary ?? '',
    successCriteria: value?.successCriteria ?? '',
  });
}

async function loadCharter() {
  if (!props.projectId) return;
  loading.value = true;
  error.value = '';
  try {
    const detail = await getProjectDetailSectionApi('overview', props.projectId);
    charter.value = detail.charter;
    syncForm(detail.charter);
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目章程读取失败，请重试。';
  } finally {
    loading.value = false;
  }
}

watch(
  () => props.projectId,
  () => void loadCharter(),
  { immediate: true },
);

async function runAction(action: () => Promise<unknown>, successMessage: string): Promise<boolean> {
  saving.value = true;
  error.value = '';
  notice.value = '';
  try {
    await action();
    await loadCharter();
    emit('changed');
    notice.value = successMessage;
    return true;
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目章程操作失败，请重试。';
    return false;
  } finally {
    saving.value = false;
  }
}

function saveInput(): ProjectApi.CharterSaveInput {
  return {
    ...form,
    approvalStatus: '草稿',
  };
}

async function saveCharter() {
  await runAction(() => saveProjectCharterApi(props.projectId, saveInput()), '已保存项目章程摘要。');
}

async function submitApproval() {
  await runAction(async () => {
    await saveProjectCharterApi(props.projectId, saveInput());
    await submitProjectCharterApprovalApi(props.projectId, { comment: approvalComment.value.trim() });
  }, '已提交章程审批。');
}

async function approve() {
  const completed = await runAction(
    () => approveProjectCharterApi(props.projectId, { comment: approvalComment.value.trim() }),
    '章程已批准。',
  );
  if (completed) approvalComment.value = '';
}

async function reject() {
  if (!approvalComment.value.trim()) {
    error.value = '驳回章程时请填写审批意见。';
    return;
  }
  const completed = await runAction(
    () => rejectProjectCharterApi(props.projectId, { comment: approvalComment.value.trim() }),
    '章程已驳回。',
  );
  if (completed) approvalComment.value = '';
}

async function withdraw() {
  const completed = await runAction(
    () => withdrawProjectCharterApprovalApi(props.projectId, { comment: approvalComment.value.trim() }),
    '已撤回章程审批。',
  );
  if (completed) approvalComment.value = '';
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目章程审批"
  >
    <header :class="$style.header">
      <div>
        <h3 :class="$style.heading">项目章程</h3>
        <p :class="$style.description">保存目标与范围后提交审批；只有章程审批通过后才能进入执行状态。</p>
      </div>
      <span>{{ approvalStatus }}</span>
    </header>

    <p
      v-if="loading"
      role="status"
    >
      正在读取章程…
    </p>
    <p
      v-if="error"
      role="alert"
    >
      {{ error }}
    </p>
    <p
      v-if="notice"
      role="status"
    >
      {{ notice }}
    </p>

    <div :class="$style.formGrid">
      <UiFormField label="商业论证">
        <UiTextarea
          v-model="form.businessCase"
          rows="3"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
      <UiFormField label="项目目标">
        <UiTextarea
          v-model="form.objectives"
          rows="3"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
      <UiFormField label="范围边界">
        <UiTextarea
          v-model="form.scopeBoundary"
          rows="3"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
      <UiFormField label="成功标准">
        <UiTextarea
          v-model="form.successCriteria"
          rows="3"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
      <UiFormField label="假设条件">
        <UiTextarea
          v-model="form.assumptions"
          rows="2"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
      <UiFormField label="约束条件">
        <UiTextarea
          v-model="form.constraints"
          rows="2"
          :disabled="!canEdit || saving"
        />
      </UiFormField>
    </div>

    <p
      v-if="isApproved"
      role="status"
    >
      已批准的章程当前不能直接修改，后端尚未提供新版本创建接口。
    </p>
    <p
      v-else-if="isPending"
      role="status"
    >
      章程审批处理中，撤回后才能修改摘要。
    </p>
    <div :class="$style.actions">
      <UiButton
        v-if="canEdit"
        variant="secondary"
        :loading="saving"
        @click="saveCharter"
        >保存摘要</UiButton
      >
      <UiButton
        v-if="canEdit"
        variant="primary"
        :loading="saving"
        @click="submitApproval"
        >保存并提交审批</UiButton
      >
      <UiButton
        v-if="canManage && isPending"
        variant="secondary"
        :loading="saving"
        @click="withdraw"
        >撤回审批</UiButton
      >
      <UiButton
        v-if="canApprove && isPending"
        variant="primary"
        :loading="saving"
        @click="approve"
        >批准</UiButton
      >
      <UiButton
        v-if="canApprove && isPending"
        variant="danger"
        :loading="saving"
        @click="reject"
        >驳回</UiButton
      >
    </div>

    <UiFormField
      v-if="(canApprove && isPending) || (canManage && isPending) || (canManage && !isApproved && !isPending)"
      label="审批意见"
      hint="驳回时必须填写意见。"
    >
      <UiTextarea
        v-model="approvalComment"
        rows="2"
        :disabled="saving"
      />
    </UiFormField>

    <div
      v-if="workflow?.approvalRecords.length"
      :class="$style.history"
    >
      <h4 :class="$style.subheading">审批记录</h4>
      <article
        v-for="record in workflow.approvalRecords"
        :key="record.id"
        :class="$style.record"
      >
        <p>{{ record.actorUserName }} · {{ record.action }} · {{ record.toStatus }}</p>
        <p>{{ record.comment || '未填写审批意见' }} · {{ record.creationTime.slice(0, 10) }}</p>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel,
.history {
  @apply space-y-3 border-t pt-4;
}

.header {
  @apply flex items-center justify-between gap-3;
}

.heading,
.subheading {
  @apply font-semibold;
}

.description {
  @apply text-sm;
}

.formGrid {
  @apply grid grid-cols-2 gap-3;
}

.actions {
  @apply flex flex-wrap items-center gap-2;
}

.record {
  @apply border-b pb-2;
}
</style>
