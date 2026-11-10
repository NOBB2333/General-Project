<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { ArrowLeftRight, RefreshCw, Search, UsersRound } from 'lucide-vue-next';
import type { OrganizationApi } from '@/api/core/organization';
import { transferOrganizationMembersApi } from '@/api/core/organization';
import { getUserListApi, type UserApi } from '@/api/core/user';
import { UiButton, UiDialog, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import PlatformUserTable from '../users/PlatformUserTable.vue';

const props = defineProps<{
  canManage: boolean;
  organizations: readonly OrganizationApi.TreeItem[];
  unit: null | OrganizationApi.TreeItem;
}>();

const emit = defineEmits<{
  changed: [];
}>();

const users = ref<UserApi.ListItem[]>([]);
const totalCount = ref(0);
const pageIndex = ref(0);
const pageSize = ref(20);
const keyword = ref('');
const loading = ref(false);
const errorMessage = ref('');
const selectedUserIds = ref(new Set<string>());
const transferOpen = ref(false);
const transferTargetId = ref('');
const transferError = ref('');
const transferSaving = ref(false);
let latestRequestId = 0;

const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)));
const maySelectUsers = computed(() => props.canManage && Boolean(props.unit && !props.unit.disabled));
const targetOptions = computed<UiSelectOption[]>(() => [
  { label: '请选择目标组织', value: '' },
  ...props.organizations
    .filter((organization) => organization.id !== props.unit?.id && !organization.disabled)
    .map((organization) => ({ label: `${organization.displayName} · ${organization.code}`, value: organization.id })),
]);

const neverWriteUser = () => false;

async function loadUsers(targetPage = pageIndex.value, clearSelection = false) {
  const unitId = props.unit?.id;
  const requestId = ++latestRequestId;
  if (clearSelection) selectedUserIds.value.clear();
  if (!unitId) {
    users.value = [];
    totalCount.value = 0;
    pageIndex.value = 0;
    loading.value = false;
    errorMessage.value = '';
    return;
  }

  pageIndex.value = Math.max(0, targetPage);
  loading.value = true;
  errorMessage.value = '';
  users.value = [];
  try {
    const result = await getUserListApi({
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize.value,
      organizationUnitId: unitId,
      skipCount: pageIndex.value * pageSize.value,
    });
    if (requestId !== latestRequestId) return;
    const lastPageIndex = Math.max(0, Math.ceil(result.totalCount / pageSize.value) - 1);
    if (pageIndex.value > lastPageIndex) {
      await loadUsers(lastPageIndex);
      return;
    }
    users.value = result.items;
    totalCount.value = result.totalCount;
  } catch (error) {
    if (requestId !== latestRequestId) return;
    users.value = [];
    totalCount.value = 0;
    errorMessage.value = error instanceof Error ? error.message : '成员列表加载失败，请重试。';
  } finally {
    if (requestId === latestRequestId) loading.value = false;
  }
}

function updateSelectedUser(userId: string, selected: boolean) {
  const next = new Set(selectedUserIds.value);
  if (selected) next.add(userId);
  else next.delete(userId);
  selectedUserIds.value = next;
}

function changePageSize(value: string) {
  const nextPageSize = Number(value);
  if (![10, 20, 50, 100].includes(nextPageSize)) return;
  pageSize.value = nextPageSize;
  void loadUsers(0, true);
}

function openTransfer() {
  if (!maySelectUsers.value || selectedUserIds.value.size === 0 || targetOptions.value.length < 2) return;
  transferTargetId.value = '';
  transferError.value = '';
  transferOpen.value = true;
}

async function submitTransfer() {
  if (!props.unit || !maySelectUsers.value) return;
  if (!transferTargetId.value) {
    transferError.value = '请选择目标组织。';
    return;
  }
  transferSaving.value = true;
  transferError.value = '';
  try {
    await transferOrganizationMembersApi(props.unit.id, {
      targetOrganizationUnitId: transferTargetId.value,
      userIds: [...selectedUserIds.value],
    });
    transferOpen.value = false;
    selectedUserIds.value.clear();
    toast.success('成员组织关系已转移。');
    await loadUsers(0);
    emit('changed');
  } catch (error) {
    transferError.value = error instanceof Error ? error.message : '成员转移失败，请检查组织范围后重试。';
  } finally {
    transferSaving.value = false;
  }
}

watch(
  () => props.unit?.id,
  () => {
    keyword.value = '';
    pageIndex.value = 0;
    void loadUsers(0, true);
  },
  { immediate: true },
);

watch(
  () => props.organizations.map((organization) => organization.id).join('|'),
  () => {
    if (!targetOptions.value.some((option) => option.value === transferTargetId.value)) transferTargetId.value = '';
  },
);
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="组织成员"
    data-organization-members
  >
    <header :class="$style.panelHeader">
      <div :class="$style.heading">
        <span :class="$style.headingIcon">
          <UsersRound
            :size="17"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h2>{{ unit?.displayName ?? '组织成员' }}</h2>
          <p v-if="unit">查询当前组织及其下级组织中的用户，成员范围由服务端按组织树计算。</p>
          <p v-else>选择左侧组织后查看成员。</p>
        </div>
      </div>
      <div :class="$style.headerActions">
        <UiButton
          size="sm"
          :disabled="loading || !unit"
          @click="loadUsers()"
        >
          <template #leading>
            <RefreshCw
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          刷新
        </UiButton>
        <UiButton
          v-if="canManage"
          variant="primary"
          size="sm"
          :disabled="!maySelectUsers || selectedUserIds.size === 0 || targetOptions.length < 2"
          @click="openTransfer"
        >
          <template #leading>
            <ArrowLeftRight
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          转移成员<span v-if="selectedUserIds.size">（{{ selectedUserIds.size }}）</span>
        </UiButton>
      </div>
    </header>

    <form
      :class="$style.filterBar"
      role="search"
      @submit.prevent="loadUsers(0, true)"
    >
      <label :class="$style.searchField">
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <input
          v-model="keyword"
          type="search"
          aria-label="搜索组织成员"
          placeholder="搜索姓名、账号、邮箱…"
          :disabled="!unit"
        />
      </label>
      <UiButton
        type="submit"
        variant="primary"
        size="sm"
        :disabled="loading || !unit"
      >
        查询
      </UiButton>
      <UiButton
        size="sm"
        :disabled="loading || !unit"
        @click="
          keyword = '';
          loadUsers(0, true);
        "
        >重置</UiButton
      >
      <span
        v-if="selectedUserIds.size"
        :class="$style.selectionNote"
        aria-live="polite"
      >
        已选 {{ selectedUserIds.size }} 人；切换组织或查询条件会清除选择
      </span>
    </form>

    <div
      v-if="errorMessage"
      :class="$style.errorState"
      role="alert"
    >
      <span>{{ errorMessage }}</span>
      <UiButton
        size="sm"
        @click="loadUsers()"
        >重试</UiButton
      >
    </div>

    <PlatformUserTable
      v-if="unit"
      :rows="users"
      :loading="loading"
      :can-manage="false"
      :is-writable="neverWriteUser"
      :can-delete="neverWriteUser"
      :selectable="maySelectUsers"
      :selected-ids="selectedUserIds"
      @select="updateSelectedUser"
    />
    <p
      v-else
      :class="$style.noOrganization"
      role="status"
    >
      暂无可选组织。请先创建组织后查看成员。
    </p>

    <footer
      v-if="unit"
      :class="$style.pagination"
      aria-label="组织成员分页"
    >
      <span>共 {{ totalCount.toLocaleString('zh-CN') }} 人 · 第 {{ pageIndex + 1 }} / {{ pageCount }} 页</span>
      <div :class="$style.paginationActions">
        <UiSelect
          :model-value="String(pageSize)"
          label="每页条数"
          :options="[
            { label: '10 条/页', value: '10' },
            { label: '20 条/页', value: '20' },
            { label: '50 条/页', value: '50' },
            { label: '100 条/页', value: '100' },
          ]"
          size="sm"
          :disabled="loading || !unit"
          @update:model-value="changePageSize"
        />
        <UiButton
          size="sm"
          :disabled="loading || pageIndex === 0"
          @click="loadUsers(pageIndex - 1)"
          >上一页</UiButton
        >
        <UiButton
          size="sm"
          :disabled="loading || pageIndex + 1 >= pageCount"
          @click="loadUsers(pageIndex + 1)"
          >下一页</UiButton
        >
      </div>
    </footer>

    <UiDialog
      :open="transferOpen"
      title="转移组织成员"
      description="服务端会验证当前成员范围和目标组织权限。"
      size="md"
      @update:open="transferOpen = $event"
    >
      <div :class="$style.transferForm">
        <p :class="$style.transferWarning">
          将 {{ selectedUserIds.size }} 位成员从“{{
            unit?.displayName
          }}”转移到目标组织。提交后，这些成员在当前源组织子树内的其他组织关系也会被移除，并加入目标组织。
        </p>
        <label :class="$style.fieldLabel">
          <span>目标组织</span>
          <UiSelect
            v-model="transferTargetId"
            label="目标组织"
            :options="targetOptions"
            :disabled="transferSaving"
          />
        </label>
        <p
          v-if="transferError"
          :class="$style.errorMessage"
          role="alert"
        >
          {{ transferError }}
        </p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="transferSaving"
          @click="transferOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          :loading="transferSaving"
          :disabled="!transferTargetId"
          @click="submitTransfer"
          >确认转移</UiButton
        >
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply min-w-0 border bg-white p-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.panelHeader {
  @apply flex flex-wrap items-center justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border);
}
.heading {
  @apply flex min-w-0 items-start gap-3;
}
.headingIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.heading h2 {
  @apply m-0 truncate text-[15px] font-semibold;
  color: var(--ui-text);
}
.heading p {
  @apply mb-0 mt-1 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.headerActions {
  @apply flex items-center gap-2;
}
.filterBar {
  @apply mb-3 flex flex-wrap items-center gap-2 pt-4;
}
.searchField {
  @apply flex h-8 w-[min(355px,100%)] items-center gap-2 border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.searchField:focus-within {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}
.searchField input {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}
.searchField input::placeholder {
  color: #97a0af;
}
.selectionNote {
  @apply text-[11px];
  color: var(--ui-primary, var(--app-primary));
}
.noOrganization {
  @apply my-4 rounded border px-4 py-8 text-center text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.errorState {
  @apply mb-3 flex items-center justify-between gap-3 rounded border bg-white px-4 py-3 text-[12px];
  color: var(--ui-danger);
  border-color: var(--ui-danger);
}
.pagination {
  @apply flex flex-wrap items-center justify-between gap-3 border-t py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.paginationActions {
  @apply flex items-center gap-2;
}
.transferForm {
  @apply grid gap-4 p-5;
}
.transferWarning {
  @apply m-0 rounded border px-3 py-3 text-[12px] leading-5;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
  border-color: color-mix(in srgb, var(--ui-warning) 25%, transparent);
}
.fieldLabel {
  @apply grid gap-2 text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.errorMessage {
  @apply m-0 border px-3 py-2 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}
</style>
