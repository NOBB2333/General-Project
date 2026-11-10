<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  approveProjectChangeApi,
  createProjectChangeRequestApi,
  rejectProjectChangeApi,
  submitProjectChangeApprovalApi,
  withdrawProjectChangeApprovalApi,
  type ProjectApi,
} from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

const props = defineProps<{
  projectId: string;
  requests: ProjectApi.ChangeRequest[];
}>();
const emit = defineEmits<{ changed: [] }>();

const changeTypes = ['范围', '进度', '成本'];
const priorities = ['P0', 'P1', 'P2', 'P3'];
const blankForm = () => ({
  analysis: '',
  changeType: '范围',
  costDelta: '',
  expectedResult: '',
  impactScope: '',
  reason: '',
  scheduleDelta: '',
  severity: 'P1',
  title: '',
});

const form = reactive(blankForm());
const approvalComment = ref('');
const createOpen = ref(false);
const saving = ref(false);
const activeRequestId = ref('');
const error = ref('');
const notice = ref('');

const canManage = computed(() => hasAccessCode('Project.Governance.Manage'));
const canApprove = computed(() => hasAccessCode('Project.Workflow.Approve'));

function parseOptionalNumber(value: string): number | undefined {
  if (!value.trim()) return undefined;
  const number = Number(value);
  return Number.isFinite(number) ? number : undefined;
}

function isPending(request: ProjectApi.ChangeRequest): boolean {
  return request.status === '审批中';
}

function isApproved(request: ProjectApi.ChangeRequest): boolean {
  return request.status === '已批准';
}

async function runAction(action: () => Promise<unknown>, successMessage: string, requestId = ''): Promise<boolean> {
  saving.value = true;
  activeRequestId.value = requestId;
  error.value = '';
  notice.value = '';
  try {
    await action();
    notice.value = successMessage;
    emit('changed');
    return true;
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '变更请求操作失败，请重试。';
    return false;
  } finally {
    saving.value = false;
    activeRequestId.value = '';
  }
}

async function createRequest() {
  if (
    !form.title.trim() ||
    !form.reason.trim() ||
    !form.impactScope.trim() ||
    !form.expectedResult.trim() ||
    !form.analysis.trim()
  ) {
    error.value = '请补齐标题、原因、影响范围、预期结果和影响分析。';
    return;
  }

  const costDelta = parseOptionalNumber(form.costDelta);
  const scheduleDelta = parseOptionalNumber(form.scheduleDelta);
  if (
    (form.costDelta.trim() && costDelta === undefined) ||
    (form.scheduleDelta.trim() && scheduleDelta === undefined)
  ) {
    error.value = '成本和进度影响请输入有效数字。';
    return;
  }

  const created = await runAction(
    () =>
      createProjectChangeRequestApi(props.projectId, {
        changeType: form.changeType,
        expectedResult: form.expectedResult.trim(),
        impactScope: form.impactScope.trim(),
        impacts: [
          {
            analysis: form.analysis.trim(),
            estimatedCostDelta: costDelta,
            estimatedScheduleDeltaDays: scheduleDelta,
            impactType: form.changeType,
            severity: form.severity,
          },
        ],
        reason: form.reason.trim(),
        title: form.title.trim(),
      }),
    '已创建变更请求，请提交审批。',
  );
  if (created) {
    Object.assign(form, blankForm());
    createOpen.value = false;
  }
}

async function submitRequest(request: ProjectApi.ChangeRequest) {
  await runAction(
    () => submitProjectChangeApprovalApi(request.id, { comment: approvalComment.value.trim() }),
    '已提交变更审批。',
    request.id,
  );
}

async function withdrawRequest(request: ProjectApi.ChangeRequest) {
  const completed = await runAction(
    () => withdrawProjectChangeApprovalApi(request.id, { comment: approvalComment.value.trim() }),
    '已撤回变更审批。',
    request.id,
  );
  if (completed) approvalComment.value = '';
}

async function approveRequest(request: ProjectApi.ChangeRequest) {
  const completed = await runAction(
    () => approveProjectChangeApi(request.id, { comment: approvalComment.value.trim() }),
    '变更已批准。',
    request.id,
  );
  if (completed) approvalComment.value = '';
}

async function rejectRequest(request: ProjectApi.ChangeRequest) {
  if (!approvalComment.value.trim()) {
    error.value = '驳回变更时请填写审批意见。';
    return;
  }
  const completed = await runAction(
    () => rejectProjectChangeApi(request.id, { comment: approvalComment.value.trim() }),
    '变更已驳回。',
    request.id,
  );
  if (completed) approvalComment.value = '';
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="范围进度成本变更"
  >
    <header :class="$style.header">
      <div>
        <h3 :class="$style.heading">范围、进度与成本变更</h3>
        <p :class="$style.description">项目审批覆盖范围、进度和成本基线变更；预算与合同变更请在经营模块处理。</p>
      </div>
      <UiButton
        v-if="canManage"
        variant="primary"
        @click="createOpen = true"
        >新建变更</UiButton
      >
    </header>

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
    <UiFormField
      v-if="canManage || canApprove"
      label="审批意见"
      hint="驳回时必须填写意见。"
    >
      <UiTextarea
        v-model="approvalComment"
        rows="2"
        :disabled="saving"
      />
    </UiFormField>

    <article
      v-for="request in requests"
      :key="request.id"
      :class="$style.request"
    >
      <header :class="$style.requestHeader">
        <div>
          <h4 :class="$style.subheading">{{ request.title }}</h4>
          <p>{{ request.requestCode }} · {{ request.changeType }} · {{ request.applicantName }}</p>
        </div>
        <span>{{ request.status }}</span>
      </header>
      <p>原因：{{ request.reason }}</p>
      <p>影响范围：{{ request.impactScope }}</p>
      <p>预期结果：{{ request.expectedResult }}</p>
      <ul
        v-if="request.impacts.length"
        :class="$style.impactList"
      >
        <li
          v-for="impact in request.impacts"
          :key="impact.id"
        >
          {{ impact.impactType }} · {{ impact.severity }} · {{ impact.analysis }}
          <span v-if="impact.estimatedCostDelta !== null"> · 成本影响 {{ impact.estimatedCostDelta }}</span>
          <span v-if="impact.estimatedScheduleDeltaDays !== null">
            · 进度影响 {{ impact.estimatedScheduleDeltaDays }} 天</span
          >
        </li>
      </ul>
      <div :class="$style.actions">
        <UiButton
          v-if="canManage && !isApproved(request) && !isPending(request)"
          variant="primary"
          :disabled="saving"
          :loading="saving && activeRequestId === request.id"
          @click="submitRequest(request)"
        >
          提交审批
        </UiButton>
        <UiButton
          v-if="canManage && isPending(request)"
          variant="secondary"
          :disabled="saving"
          :loading="saving && activeRequestId === request.id"
          @click="withdrawRequest(request)"
        >
          撤回审批
        </UiButton>
        <UiButton
          v-if="canApprove && isPending(request)"
          variant="primary"
          :disabled="saving"
          :loading="saving && activeRequestId === request.id"
          @click="approveRequest(request)"
        >
          批准
        </UiButton>
        <UiButton
          v-if="canApprove && isPending(request)"
          variant="danger"
          :disabled="saving"
          :loading="saving && activeRequestId === request.id"
          @click="rejectRequest(request)"
        >
          驳回
        </UiButton>
      </div>
      <div
        v-if="request.approvals.length"
        :class="$style.history"
      >
        <p :class="$style.subheading">审批记录</p>
        <p
          v-for="approval in request.approvals"
          :key="approval.id"
        >
          {{ approval.approverName }} · {{ approval.action }} · {{ approval.comment || '未填写意见' }} ·
          {{ approval.decisionTime.slice(0, 10) }}
        </p>
      </div>
    </article>
    <p v-if="requests.length === 0">当前没有变更请求。</p>

    <UiDialog
      :open="createOpen"
      title="新建项目变更"
      description="填写影响分析后创建变更请求，再提交工作流审批。"
      size="lg"
      @update:open="createOpen = $event"
    >
      <form
        :class="$style.formGrid"
        @submit.prevent="createRequest"
      >
        <UiFormField
          label="变更标题"
          required
        >
          <UiInput
            v-model="form.title"
            maxlength="128"
          />
        </UiFormField>
        <UiFormField
          label="变更类型"
          required
        >
          <UiSelect
            v-model="form.changeType"
            label="变更类型"
            :options="changeTypes.map((value) => ({ value, label: value }))"
          />
        </UiFormField>
        <UiFormField
          label="原因"
          required
          wide
        >
          <UiTextarea
            v-model="form.reason"
            rows="2"
          />
        </UiFormField>
        <UiFormField
          label="影响范围"
          required
        >
          <UiTextarea
            v-model="form.impactScope"
            rows="2"
          />
        </UiFormField>
        <UiFormField
          label="预期结果"
          required
        >
          <UiTextarea
            v-model="form.expectedResult"
            rows="2"
          />
        </UiFormField>
        <UiFormField
          label="影响分析"
          required
        >
          <UiTextarea
            v-model="form.analysis"
            rows="2"
          />
        </UiFormField>
        <UiFormField
          label="影响等级"
          required
        >
          <UiSelect
            v-model="form.severity"
            label="影响等级"
            :options="priorities.map((value) => ({ value, label: value }))"
          />
        </UiFormField>
        <UiFormField label="预计成本变化">
          <UiInput
            v-model="form.costDelta"
            type="number"
            step="0.01"
          />
        </UiFormField>
        <UiFormField label="预计进度变化（天）">
          <UiInput
            v-model="form.scheduleDelta"
            type="number"
            step="1"
          />
        </UiFormField>
        <p
          v-if="error"
          :class="$style.formNotice"
          role="alert"
        >
          {{ error }}
        </p>
      </form>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="saving"
          @click="createOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          :loading="saving"
          @click="createRequest"
          >创建变更</UiButton
        >
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply space-y-3 border-t pt-4;
}

.request {
  @apply space-y-2 border-b pb-3;
}

.header,
.requestHeader {
  @apply flex flex-wrap items-start justify-between gap-2;
}

.heading,
.subheading {
  @apply font-semibold;
}

.description {
  @apply text-sm;
}

.impactList {
  @apply list-disc pl-5;
}

.actions {
  @apply flex flex-wrap gap-2;
}

.history {
  @apply space-y-1;
}

.formGrid {
  @apply grid grid-cols-2 gap-3;
}

.formNotice {
  @apply col-span-2;
}
</style>
