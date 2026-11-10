<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { createProjectRequirementApi, updateProjectRequirementApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  buildProjectRequirementSaveInput,
  createProjectRequirementForm,
  projectRequirementFormFromItem,
  type ProjectRequirementForm,
} from '../model/project-issues';

const props = defineProps<{
  detail: ProjectApi.Detail;
  open: boolean;
  projectId: string;
  requirement: null | ProjectApi.RequirementItem;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<ProjectRequirementForm>(createProjectRequirementForm());
const saving = ref(false);
const error = ref('');

const userOptions = computed<UiSelectOption[]>(() => {
  const options = new Map<string, string>();
  for (const member of props.detail.members) {
    if (member.isActive) options.set(member.userId, displayName(member.userName));
  }
  const requirement = props.requirement;
  if (requirement) {
    addCurrentUserOption(options, requirement.businessOwnerUserId, requirement.businessOwnerName);
    addCurrentUserOption(options, requirement.productOwnerUserId, requirement.productOwnerName);
  }
  return [...options].map(([value, label]) => ({ value, label }));
});

const ownerOptions = computed<UiSelectOption[]>(() => [{ value: '', label: '未指定' }, ...userOptions.value]);

const wbsOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '未关联WBS' },
  ...props.detail.wbsNodes.map((node) => ({ value: node.id, label: `${node.wbsCode} · ${node.name}` })),
]);

const milestoneOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '未关联里程碑' },
  ...props.detail.milestones.map((milestone) => ({
    value: milestone.id,
    label: milestone.name,
  })),
]);

const replacesOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不替代其他需求' },
  ...props.detail.requirements
    .filter((item) => item.id !== props.requirement?.id)
    .map((item) => ({
      value: item.id,
      label: `${item.requirementCode} · ${item.title}（V${item.versionNo}）`,
    })),
]);

const priorityOptions: UiSelectOption[] = [
  { value: 'P0', label: 'P0' },
  { value: 'P1', label: 'P1' },
  { value: 'P2', label: 'P2' },
  { value: 'P3', label: 'P3' },
];

const currentVersionOptions: UiSelectOption[] = [
  { value: 'true', label: '是' },
  { value: 'false', label: '否' },
];

watch(
  () => [props.open, props.requirement?.id] as const,
  ([open]) => {
    if (!open) return;
    Object.assign(
      form,
      props.requirement ? projectRequirementFormFromItem(props.requirement) : createProjectRequirementForm(),
    );
    error.value = '';
  },
);

function updateDialogOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

async function saveRequirement() {
  if (
    !form.requirementCode.trim() ||
    !form.title.trim() ||
    !form.status.trim() ||
    !form.priority.trim() ||
    !form.source.trim() ||
    !form.type.trim()
  ) {
    error.value = '请填写需求编号、标题、状态、优先级、来源和类型。';
    return;
  }
  const sizeEstimate = form.sizeEstimate.trim() ? Number(form.sizeEstimate) : null;
  if (sizeEstimate !== null && !Number.isFinite(sizeEstimate)) {
    error.value = '规模估算请输入有效数字，或留空。';
    return;
  }
  const versionNo = Number(form.versionNo);
  if (!Number.isInteger(versionNo) || versionNo < 1) {
    error.value = '版本号请输入大于0的整数。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    const input = buildProjectRequirementSaveInput(form);
    if (props.requirement) await updateProjectRequirementApi(props.requirement.id, input);
    else await createProjectRequirementApi(props.projectId, input);
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '需求保存失败，请检查信息后重试。';
  } finally {
    saving.value = false;
  }
}

function addCurrentUserOption(options: Map<string, string>, userId: null | string, userName: string) {
  if (userId && !options.has(userId)) options.set(userId, displayName(userName));
}

function displayName(value: null | string): string {
  return value && value !== '-' && value !== '—' ? value : '未提供姓名';
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="requirement ? '编辑项目需求' : '新增项目需求'"
    description="需求编号在项目内唯一；WBS、里程碑和版本关联由后端校验。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveRequirement"
    >
      <UiFormField
        label="需求编号"
        required
      >
        <UiInput
          v-model="form.requirementCode"
          :disabled="saving"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField
        label="需求标题"
        required
      >
        <UiInput
          v-model="form.title"
          :disabled="saving"
          maxlength="256"
        />
      </UiFormField>
      <UiFormField
        label="优先级"
        required
      >
        <UiSelect
          v-model="form.priority"
          label="需求优先级"
          :options="priorityOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        label="版本号"
        required
      >
        <UiInput
          v-model="form.versionNo"
          type="number"
          min="1"
          step="1"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        label="状态"
        hint="按后端现有状态文本填写；当前没有状态字典端点。"
        required
      >
        <UiInput
          v-model="form.status"
          :disabled="saving"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField
        label="类型"
        required
      >
        <UiInput
          v-model="form.type"
          :disabled="saving"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField
        label="来源"
        required
      >
        <UiInput
          v-model="form.source"
          :disabled="saving"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField label="规模估算">
        <UiInput
          v-model="form.sizeEstimate"
          type="number"
          step="any"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="产品负责人">
        <UiSelect
          v-model="form.productOwnerUserId"
          label="需求产品负责人"
          :options="ownerOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="业务负责人">
        <UiSelect
          v-model="form.businessOwnerUserId"
          label="需求业务负责人"
          :options="ownerOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="WBS节点">
        <UiSelect
          v-model="form.wbsNodeId"
          label="关联WBS节点"
          :options="wbsOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="里程碑">
        <UiSelect
          v-model="form.milestoneId"
          label="关联里程碑"
          :options="milestoneOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="替代需求">
        <UiSelect
          v-model="form.replacesRequirementId"
          label="被替代需求"
          :options="replacesOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="当前版本">
        <UiSelect
          :model-value="String(form.isCurrent)"
          label="当前版本状态"
          :options="currentVersionOptions"
          :disabled="saving"
          @update:model-value="form.isCurrent = $event === 'true'"
        />
      </UiFormField>
      <UiFormField
        label="需求描述"
        wide
      >
        <UiTextarea
          v-model="form.description"
          :disabled="saving"
          rows="3"
        />
      </UiFormField>
      <UiFormField
        label="验收标准"
        wide
      >
        <UiTextarea
          v-model="form.acceptanceCriteria"
          :disabled="saving"
          rows="3"
        />
      </UiFormField>
      <UiFormField
        label="影响摘要"
        wide
      >
        <UiTextarea
          v-model="form.impactSummary"
          :disabled="saving"
          rows="2"
        />
      </UiFormField>
      <UiFormField
        label="变更原因"
        wide
      >
        <UiTextarea
          v-model="form.changeReason"
          :disabled="saving"
          rows="2"
        />
      </UiFormField>
      <p
        v-if="error"
        role="alert"
        :class="$style.error"
      >
        {{ error }}
      </p>
      <div :class="$style.actions">
        <UiButton
          type="submit"
          variant="primary"
          :loading="saving"
        >
          {{ requirement ? '保存需求' : '创建需求' }}
        </UiButton>
        <UiButton
          type="button"
          variant="secondary"
          :disabled="saving"
          @click="updateDialogOpen(false)"
        >
          取消
        </UiButton>
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid gap-3 sm:grid-cols-2;
}

.error {
  @apply rounded border px-3 py-2 text-[11px] sm:col-span-2;
  border-color: var(--ui-border);
  color: var(--ui-danger);
}

.actions {
  @apply flex justify-end gap-2 pt-2 sm:col-span-2;
}
</style>
