<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { createProjectApi, type ProjectApi } from '@/api/core/project';
import { getOrganizationUnitTreeApi, type OrganizationApi } from '@/api/core/organization';
import { getUserListApi, type UserApi } from '@/api/core/user';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  created: [projectId: string];
}>();

const blankForm = () => ({
  description: '',
  isKeyProject: false,
  managerUserId: '',
  name: '',
  organizationUnitId: '',
  plannedEndDate: '',
  plannedStartDate: '',
  priority: 'P1',
  projectCode: '',
  projectSource: '',
  projectType: '',
  sponsorUserId: '',
  status: '待规划',
});

const form = reactive(blankForm());
const userOptions = ref<UiSelectOption[]>([]);
const organizationOptions = ref<UiSelectOption[]>([]);
const userKeyword = ref('');
const loadingOptions = ref(false);
const saving = ref(false);
const error = ref('');

const canSubmit = computed(
  () =>
    !saving.value &&
    !loadingOptions.value &&
    form.projectCode.trim().length > 0 &&
    form.name.trim().length > 0 &&
    form.organizationUnitId.length > 0 &&
    form.managerUserId.length > 0 &&
    form.sponsorUserId.length > 0,
);

function flattenOrganizations(
  nodes: OrganizationApi.TreeItem[],
  depth = 0,
  values: UiSelectOption[] = [],
): UiSelectOption[] {
  for (const node of nodes) {
    if (!node.disabled) {
      values.push({ value: node.id, label: `${'　'.repeat(depth)}${node.displayName}` });
    }
    flattenOrganizations(node.children, depth + 1, values);
  }
  return values;
}

async function loadOptions() {
  loadingOptions.value = true;
  error.value = '';
  try {
    const [users, organizations] = await Promise.all([
      getUserListApi({ maxResultCount: 200, skipCount: 0, isActive: true }),
      getOrganizationUnitTreeApi(),
    ]);
    setUserOptions(users.items);
    organizationOptions.value = flattenOrganizations(organizations);
    if (!userOptions.value.length) error.value = '当前账号没有可选的启用用户，请先检查用户权限和账号状态。';
    else if (!organizationOptions.value.length) error.value = '当前没有可选组织，请先维护组织架构。';
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '负责人或组织选项读取失败，请重试。';
  } finally {
    loadingOptions.value = false;
  }
}

function setUserOptions(users: UserApi.ListItem[]) {
  userOptions.value = users
    .filter((user) => user.isActive)
    .map((user) => ({ value: user.id, label: user.displayName || user.username }));
}

async function searchUsers() {
  loadingOptions.value = true;
  error.value = '';
  try {
    const users = await getUserListApi({
      keyword: userKeyword.value.trim() || undefined,
      maxResultCount: 200,
      skipCount: 0,
      isActive: true,
    });
    setUserOptions(users.items);
    if (!userOptions.value.length) error.value = '没有找到符合条件的启用用户。';
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '用户搜索失败，请重试。';
  } finally {
    loadingOptions.value = false;
  }
}

function resetForm() {
  Object.assign(form, blankForm());
  error.value = '';
}

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    resetForm();
    userKeyword.value = '';
    void loadOptions();
  },
);

async function submit() {
  if (!canSubmit.value) {
    error.value = '请补齐项目编号、名称、组织、项目经理和发起人。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    const projectId = await createProjectApi({
      ...form,
      description: form.description?.trim() || undefined,
      name: form.name.trim(),
      plannedEndDate: form.plannedEndDate || undefined,
      plannedStartDate: form.plannedStartDate || undefined,
      projectCode: form.projectCode.trim(),
      projectSource: form.projectSource?.trim() || undefined,
      projectType: form.projectType?.trim() || undefined,
      status: '待规划',
    });
    emit('created', projectId);
    emit('update:open', false);
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目创建失败，请稍后重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建项目"
    description="项目从待规划状态创建；通过章程审批后才能进入执行。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="项目编号"
        required
      >
        <UiInput
          v-model="form.projectCode"
          maxlength="64"
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField
        label="项目名称"
        required
      >
        <UiInput
          v-model="form.name"
          maxlength="128"
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField
        label="查找项目相关用户"
        hint="列表按当前账号数据范围返回，可输入姓名或用户名搜索。"
        wide
      >
        <div :class="$style.inline">
          <UiInput
            v-model="userKeyword"
            autocomplete="off"
            @keyup.enter.prevent="searchUsers"
          />
          <UiButton
            variant="secondary"
            :disabled="loadingOptions"
            @click="searchUsers"
            >搜索用户</UiButton
          >
        </div>
      </UiFormField>
      <UiFormField
        label="所属组织"
        required
      >
        <UiSelect
          v-model="form.organizationUnitId"
          label="所属组织"
          :options="organizationOptions"
          :disabled="loadingOptions || !organizationOptions.length"
          placeholder="请选择组织"
        />
      </UiFormField>
      <UiFormField
        label="项目经理"
        required
      >
        <UiSelect
          v-model="form.managerUserId"
          label="项目经理"
          :options="userOptions"
          :disabled="loadingOptions || !userOptions.length"
          placeholder="请选择项目经理"
        />
      </UiFormField>
      <UiFormField
        label="发起人"
        required
      >
        <UiSelect
          v-model="form.sponsorUserId"
          label="发起人"
          :options="userOptions"
          :disabled="loadingOptions || !userOptions.length"
          placeholder="请选择发起人"
        />
      </UiFormField>
      <UiFormField
        label="优先级"
        required
      >
        <UiSelect
          v-model="form.priority"
          label="优先级"
          :options="[
            { value: 'P0', label: 'P0' },
            { value: 'P1', label: 'P1' },
            { value: 'P2', label: 'P2' },
            { value: 'P3', label: 'P3' },
          ]"
        />
      </UiFormField>
      <UiFormField label="项目类型">
        <UiInput
          v-model="form.projectType"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField label="项目来源">
        <UiInput
          v-model="form.projectSource"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField label="计划开始日期">
        <UiInput
          v-model="form.plannedStartDate"
          type="date"
        />
      </UiFormField>
      <UiFormField label="计划结束日期">
        <UiInput
          v-model="form.plannedEndDate"
          type="date"
        />
      </UiFormField>
      <UiFormField
        label="项目说明"
        wide
      >
        <UiTextarea
          v-model="form.description"
          rows="3"
          maxlength="512"
        />
      </UiFormField>
      <p
        v-if="error"
        :class="$style.formNotice"
        role="alert"
      >
        {{ error }}
      </p>
      <p
        v-else-if="loadingOptions"
        :class="$style.formNotice"
        role="status"
      >
        正在读取用户和组织…
      </p>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="!canSubmit"
        @click="submit"
        >创建项目</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.formGrid {
  @apply grid grid-cols-2 gap-3;
}

.inline {
  @apply flex gap-2;
}

.formNotice {
  @apply col-span-2;
}
</style>
