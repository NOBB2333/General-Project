<script setup lang="ts">
import { PlugZap } from 'lucide-vue-next';
import { computed } from 'vue';
import { ListPageSurface } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
} from '@/components/ui';
import type { BindingTab } from './bindings.model';
import { useIntegrationBindings } from './useIntegrationBindings';

const props = withDefaults(defineProps<{ initialTab?: BindingTab }>(), { initialTab: 'accounts' });
const page = useIntegrationBindings({ initialTab: props.initialTab });
const {
  activeTab,
  changePage,
  connectorError,
  connectorOptions,
  connectors,
  connectorsLoading,
  dialogDescription,
  dialogError,
  dialogFields,
  dialogKind,
  dialogOpen,
  dialogTitle,
  draft,
  handleAction,
  handleFilterChange,
  handleSearch,
  handleUserSearch,
  importJson,
  loadError,
  loading,
  organizationError,
  organizationsLoading,
  pageModel,
  pageSurfaceKey,
  remoteFilterValues,
  remotePagination,
  saving,
  selectTab,
  submitDialog,
  tabItems,
  updateDraft,
  userError,
  userSearch,
  usersLoading,
  openRow,
} = page;

const confirmLabel = computed(() => {
  if (dialogKind.value === 'import') return '确认导入';
  if (dialogKind.value === 'auto-match') return '开始匹配';
  return '保存';
});

function closeDialog() {
  if (saving.value) return;
  dialogOpen.value = false;
}
</script>

<template>
  <ListPageSurface
    :key="`${activeTab}:${pageSurfaceKey}`"
    :model="pageModel"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @filter-change="handleFilterChange"
    @page-change="changePage"
    @row="openRow"
    @search-change="handleSearch"
  >
    <template #hero-icon>
      <PlugZap
        :size="24"
        :stroke-width="1.7"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <div>
        <UiTabs
          :model-value="activeTab"
          @update:model-value="selectTab"
        >
          <UiTabsList label="外部集成绑定类型">
            <UiTabsTrigger
              v-for="item in tabItems"
              :key="item.value"
              :value="item.value"
            >
              {{ item.label }}
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
        <p
          v-if="loading"
          role="status"
        >
          正在读取当前绑定列表…
        </p>
        <p
          v-else-if="loadError"
          role="alert"
        >
          {{ loadError }}
        </p>
        <p
          v-if="connectorError"
          role="alert"
        >
          连接器选项暂不可用：{{ connectorError }}
        </p>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="dialogOpen"
    :title="dialogTitle"
    :description="dialogDescription"
    size="lg"
    @close="closeDialog"
  >
    <section v-if="dialogKind === 'import'">
      <UiFormField
        label="目标连接器"
        required
      >
        <UiSelect
          :model-value="draft.connectorId"
          :options="connectorOptions"
          label="目标连接器"
          :disabled="connectorsLoading || connectors.length === 0"
          @update:model-value="updateDraft('connectorId', $event)"
        />
      </UiFormField>
      <p>
        相同外部用户 ID 会覆盖该连接器下已有资料。请确认 JSON
        不含密码、令牌或其他凭证；支持字段：externalUserId、displayName、email、mobile、externalUnionId、avatarUrl、departmentExternalId、status。
      </p>
      <UiFormField
        label="账号 JSON 数组"
        required
      >
        <UiTextarea
          v-model="importJson"
          rows="12"
          spellcheck="false"
          aria-label="外部账号 JSON 数组"
          placeholder="输入外部账号 JSON 数组"
        />
      </UiFormField>
    </section>

    <section v-else-if="dialogKind === 'auto-match'">
      <UiFormField
        label="目标连接器"
        required
      >
        <UiSelect
          :model-value="draft.connectorId"
          :options="connectorOptions"
          label="目标连接器"
          :disabled="connectorsLoading || connectors.length === 0"
          @update:model-value="updateDraft('connectorId', $event)"
        />
      </UiFormField>
      <div>
        自动匹配会扫描该连接器下所有未停用的外部账号，按手机号和邮箱查找当前租户内部用户：唯一匹配会绑定，多人匹配会标记冲突，没有匹配会保持待处理。再次执行会重算并覆盖该连接器已有匹配结果，同时更新账号状态和用户
        ID 映射。
      </div>
    </section>

    <section v-else>
      <UiFormField
        v-if="dialogKind === 'user-bind' || dialogKind === 'user-resolve'"
        label="搜索内部用户"
      >
        <UiInput
          :model-value="userSearch"
          placeholder="输入姓名、用户名、邮箱或手机号"
          aria-label="搜索内部用户"
          @update:model-value="handleUserSearch(String($event))"
        />
        <span
          v-if="usersLoading"
          role="status"
          >正在查找用户…</span
        >
        <span
          v-else-if="userError"
          role="alert"
          >{{ userError }}</span
        >
      </UiFormField>
      <UiFormField
        v-for="field in dialogFields"
        :key="field.key"
        :label="field.label"
        :required="field.required"
        :hint="field.hint"
        :wide="field.key === 'remark'"
      >
        <UiSelect
          v-if="field.kind === 'select'"
          :model-value="draft[field.key]"
          :options="field.options ?? []"
          :label="field.label"
          :disabled="
            (field.key === 'connectorId' && (connectorsLoading || connectors.length === 0)) ||
            (field.key === 'internalUserId' && usersLoading) ||
            (field.key === 'internalOrganizationUnitId' && organizationsLoading)
          "
          @update:model-value="updateDraft(field.key, $event)"
        />
        <UiInput
          v-else
          :model-value="draft[field.key]"
          :maxlength="field.maxLength"
          :placeholder="field.label"
          :aria-label="field.label"
          @update:model-value="updateDraft(field.key, String($event))"
        />
      </UiFormField>
      <p
        v-if="dialogKind === 'department' && organizationsLoading"
        role="status"
      >
        正在读取内部组织…
      </p>
    </section>

    <p
      v-if="dialogError"
      role="alert"
    >
      {{ dialogError }}
    </p>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="closeDialog"
      >
        取消
      </UiButton>
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="(dialogKind === 'import' || dialogKind === 'auto-match') && connectors.length === 0"
        @click="submitDialog"
      >
        {{ confirmLabel }}
      </UiButton>
    </template>
  </UiDialog>
</template>
