<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import {
  Building2,
  ChevronRight,
  Network,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  TriangleAlert,
  UsersRound,
} from 'lucide-vue-next';
import {
  createOrganizationUnitApi,
  deleteOrganizationUnitApi,
  getOrganizationUnitTreeApi,
  moveOrganizationUnitApi,
  updateOrganizationUnitApi,
  type OrganizationApi,
} from '@/api/core/organization';
import { UiButton, UiDialog, toast, type UiSelectOption } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import PlatformOrganizationMembersPanel from './PlatformOrganizationMembersPanel.vue';
import PlatformOrganizationNodeDialog from './PlatformOrganizationNodeDialog.vue';
import PlatformOrganizationTreeTable from './PlatformOrganizationTreeTable.vue';

type DialogMode = 'create' | 'edit' | 'move';

const organizations = ref<OrganizationApi.TreeItem[]>([]);
const expandedIds = ref(new Set<string>());
const selectedId = ref('');
const search = ref('');
const loading = ref(false);
const loadError = ref('');
const canManageOrganizations = computed(() => canAccess('Platform.Organization.Manage'));
const selectedUnit = computed(() => findOrganization(organizations.value, selectedId.value));
const allOrganizations = computed(() => flattenOrganizations(organizations.value));
const searchable = computed(() => search.value.trim().length > 0);
const metrics = computed(
  () =>
    [
      {
        id: 'root',
        label: '顶级组织',
        value: organizations.value.length,
        icon: Building2,
        tone: 'primary',
      },
      {
        id: 'total',
        label: '组织总数',
        value: allOrganizations.value.length,
        icon: Network,
        tone: 'purple',
      },
      {
        id: 'memberships',
        label: '组织成员关系',
        value: allOrganizations.value.reduce((sum, unit) => sum + unit.directMemberCount, 0),
        icon: UsersRound,
        tone: 'success',
      },
      {
        id: 'readOnly',
        label: '只读组织',
        value: allOrganizations.value.filter((unit) => unit.disabled).length,
        icon: TriangleAlert,
        tone: 'warning',
      },
    ] as const,
);
const dialogOpen = ref(false);
const dialogMode = ref<DialogMode>('create');
const dialogUnit = ref<null | OrganizationApi.TreeItem>(null);
const dialogSaving = ref(false);
const dialogError = ref('');
const deleteOpen = ref(false);
const deleteUnit = ref<null | OrganizationApi.TreeItem>(null);
const deleteSaving = ref(false);
const deleteError = ref('');

function flattenOrganizations(units: readonly OrganizationApi.TreeItem[]): OrganizationApi.TreeItem[] {
  return units.flatMap((unit) => [unit, ...flattenOrganizations(unit.children)]);
}

function findOrganization(
  units: readonly OrganizationApi.TreeItem[],
  organizationUnitId: string,
): OrganizationApi.TreeItem | null {
  for (const unit of units) {
    if (unit.id === organizationUnitId) return unit;
    const child = findOrganization(unit.children, organizationUnitId);
    if (child) return child;
  }
  return null;
}

function collectDescendantIds(unit: OrganizationApi.TreeItem): Set<string> {
  const result = new Set<string>([unit.id]);
  for (const child of unit.children) {
    for (const id of collectDescendantIds(child)) result.add(id);
  }
  return result;
}

const parentOptions = computed<UiSelectOption[]>(() => {
  const excludedIds =
    dialogMode.value === 'move' && dialogUnit.value ? collectDescendantIds(dialogUnit.value) : new Set();
  return [
    { label: '顶级组织', value: '' },
    ...allOrganizations.value
      .filter((unit) => !unit.disabled && !excludedIds.has(unit.id))
      .map((unit) => ({ label: `${unit.displayName} · ${unit.code}`, value: unit.id })),
  ];
});

async function loadOrganizations(preferredSelection = selectedId.value) {
  loading.value = true;
  loadError.value = '';
  try {
    const result = await getOrganizationUnitTreeApi();
    organizations.value = result;
    const availableIds = new Set(flattenOrganizations(result).map((unit) => unit.id));
    selectedId.value = availableIds.has(preferredSelection) ? preferredSelection : (result[0]?.id ?? '');
    if (expandedIds.value.size === 0) expandedIds.value = new Set(result.map((unit) => unit.id));
  } catch (error) {
    organizations.value = [];
    selectedId.value = '';
    loadError.value = error instanceof Error ? error.message : '组织架构加载失败，请重试。';
  } finally {
    loading.value = false;
  }
}

function toggleOrganization(organizationUnitId: string) {
  const next = new Set(expandedIds.value);
  if (next.has(organizationUnitId)) next.delete(organizationUnitId);
  else next.add(organizationUnitId);
  expandedIds.value = next;
}

function expandAll() {
  expandedIds.value = new Set(allOrganizations.value.filter((unit) => unit.children.length > 0).map((unit) => unit.id));
}

function collapseAll() {
  expandedIds.value = new Set();
}

function openNodeDialog(mode: DialogMode, unit: null | OrganizationApi.TreeItem) {
  if (!canManageOrganizations.value || (unit?.disabled ?? false)) return;
  dialogMode.value = mode;
  dialogUnit.value = unit;
  dialogError.value = '';
  dialogOpen.value = true;
}

function openDeleteDialog(unit: OrganizationApi.TreeItem) {
  if (!canManageOrganizations.value || unit.disabled) return;
  deleteUnit.value = unit;
  deleteError.value = '';
  deleteOpen.value = true;
}

async function saveOrganization(input: OrganizationApi.SaveInput) {
  if (!canManageOrganizations.value) return;
  dialogSaving.value = true;
  dialogError.value = '';
  try {
    if (dialogMode.value === 'create') {
      await createOrganizationUnitApi(input);
      toast.success('组织已创建。');
    } else if (dialogMode.value === 'edit' && dialogUnit.value) {
      await updateOrganizationUnitApi(dialogUnit.value.id, input);
      toast.success('组织名称已保存。');
    } else {
      return;
    }
    dialogOpen.value = false;
    const selectedBeforeRefresh = dialogUnit.value?.id ?? selectedId.value;
    await loadOrganizations(selectedBeforeRefresh);
  } catch (error) {
    dialogError.value = error instanceof Error ? error.message : '组织保存失败，请检查输入后重试。';
  } finally {
    dialogSaving.value = false;
  }
}

async function moveOrganization(input: OrganizationApi.MoveInput) {
  if (!canManageOrganizations.value || !dialogUnit.value) return;
  dialogSaving.value = true;
  dialogError.value = '';
  try {
    await moveOrganizationUnitApi(dialogUnit.value.id, input);
    toast.success('组织层级已调整。');
    dialogOpen.value = false;
    await loadOrganizations(dialogUnit.value.id);
  } catch (error) {
    dialogError.value = error instanceof Error ? error.message : '组织移动失败，请检查目标组织和层级规则。';
  } finally {
    dialogSaving.value = false;
  }
}

async function confirmDeleteOrganization() {
  if (!deleteUnit.value || !canManageOrganizations.value) return;
  deleteSaving.value = true;
  deleteError.value = '';
  try {
    await deleteOrganizationUnitApi(deleteUnit.value.id);
    toast.success('组织已删除。');
    deleteOpen.value = false;
    const selectedBeforeDelete = deleteUnit.value.id === selectedId.value ? '' : selectedId.value;
    await loadOrganizations(selectedBeforeDelete);
  } catch (error) {
    deleteError.value = error instanceof Error ? error.message : '组织删除失败，请检查该组织是否仍有成员或下级组织。';
  } finally {
    deleteSaving.value = false;
  }
}

onMounted(() => void loadOrganizations(''));
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="board"
    data-page-route="/platform/organization"
    data-platform-organizations-page
  >
    <header :class="$style.pageHeader">
      <div>
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>平台中心</span>
          <ChevronRight
            :size="12"
            :stroke-width="1.8"
          />
          <strong>组织架构</strong>
        </nav>
        <h1>组织架构</h1>
      </div>
      <div :class="$style.headerActions">
        <UiButton
          size="sm"
          :disabled="loading"
          @click="loadOrganizations()"
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
          v-if="canManageOrganizations"
          variant="primary"
          size="sm"
          :disabled="loading"
          @click="openNodeDialog('create', null)"
        >
          <template #leading>
            <Plus
              :size="15"
              :stroke-width="1.8"
            />
          </template>
          新建组织
        </UiButton>
      </div>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="组织概览"
    >
      <article
        v-for="metric in metrics"
        :key="metric.id"
        :class="$style.metricCard"
        :data-tone="metric.tone"
      >
        <div :class="$style.metricLabel">
          <component
            :is="metric.icon"
            :size="16"
            :stroke-width="1.8"
          />
          <span>{{ metric.label }}</span>
        </div>
        <strong>{{ metric.value.toLocaleString('zh-CN') }}</strong>
      </article>
    </section>

    <div :class="$style.toolbar">
      <label :class="$style.searchField">
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <input
          v-model="search"
          type="search"
          aria-label="搜索组织"
          placeholder="搜索组织名称或编码..."
          :disabled="loading"
        />
      </label>
      <UiButton
        size="sm"
        :disabled="loading || allOrganizations.length === 0"
        @click="expandAll"
        >全部展开</UiButton
      >
      <UiButton
        size="sm"
        :disabled="loading || expandedIds.size === 0"
        @click="collapseAll"
        >全部折叠</UiButton
      >
      <span :class="$style.scopeHint">
        <ShieldCheck
          :size="14"
          :stroke-width="1.8"
        />
        {{ canManageOrganizations ? '组织管理权限已启用' : '当前为只读视图' }}
      </span>
    </div>

    <div
      v-if="loadError"
      :class="$style.errorState"
      role="alert"
    >
      <span>{{ loadError }}</span>
      <UiButton
        size="sm"
        @click="loadOrganizations()"
        >重试</UiButton
      >
    </div>

    <PlatformOrganizationTreeTable
      :rows="organizations"
      :expanded-ids="expandedIds"
      :selected-id="selectedId"
      :loading="loading"
      :can-manage="canManageOrganizations"
      :searchable="searchable"
      :search-term="search"
      @select="selectedId = $event.id"
      @toggle="toggleOrganization"
      @create-child="openNodeDialog('create', $event)"
      @edit="openNodeDialog('edit', $event)"
      @move="openNodeDialog('move', $event)"
      @delete="openDeleteDialog"
    />

    <PlatformOrganizationMembersPanel
      :unit="selectedUnit"
      :organizations="allOrganizations"
      :can-manage="canManageOrganizations"
      @changed="loadOrganizations()"
    />

    <PlatformOrganizationNodeDialog
      v-model:open="dialogOpen"
      :mode="dialogMode"
      :unit="dialogUnit"
      :parent-options="parentOptions"
      :saving="dialogSaving"
      :error-message="dialogError"
      @save="saveOrganization"
      @move="moveOrganization"
    />

    <UiDialog
      :open="deleteOpen"
      title="删除组织"
      description="确认后将调用服务端删除；存在成员或下级组织时服务端会拒绝。"
      size="sm"
      @update:open="deleteOpen = $event"
    >
      <div :class="$style.deleteDialog">
        <p>
          确认删除组织“<strong>{{ deleteUnit?.displayName }}</strong
          >”吗？
        </p>
        <p
          v-if="deleteError"
          :class="$style.deleteError"
          role="alert"
        >
          {{ deleteError }}
        </p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="deleteSaving"
          @click="deleteOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :loading="deleteSaving"
          @click="confirmDeleteOrganization"
          >确认删除</UiButton
        >
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-[960px] pb-6;
  color: var(--ui-text);
}
.pageHeader {
  @apply mb-5 flex items-end justify-between;
}
.breadcrumb {
  @apply mb-2 flex items-center gap-1 text-[11px];
  color: var(--ui-text-subtle);
}
.breadcrumb strong {
  @apply font-semibold;
  color: var(--ui-text-muted);
}
.pageHeader h1 {
  @apply m-0 text-[22px] font-bold leading-8;
}
.headerActions {
  @apply flex items-center gap-2;
}
.metricGrid {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metricCard {
  @apply flex min-h-[100px] flex-col border bg-white px-4 py-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.metricLabel {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply mt-3 text-[23px] font-bold leading-7;
  color: var(--app-primary);
}
.metricCard[data-tone='primary'] .metricLabel {
  color: var(--app-primary);
}
.metricCard[data-tone='purple'] .metricLabel,
.metricCard[data-tone='purple'] strong {
  color: #6554c0;
}
.metricCard[data-tone='success'] .metricLabel,
.metricCard[data-tone='success'] strong {
  color: var(--ui-success);
}
.metricCard[data-tone='warning'] .metricLabel,
.metricCard[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.toolbar {
  @apply mb-3 flex items-center gap-2;
}
.searchField {
  @apply flex h-9 w-[355px] items-center gap-2 border bg-white px-3;
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
.scopeHint {
  @apply ml-auto inline-flex items-center gap-1.5 rounded px-2 py-1 text-[10px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.errorState {
  @apply mb-3 flex items-center justify-between gap-3 rounded border bg-white px-4 py-3 text-[12px];
  color: var(--ui-danger);
  border-color: var(--ui-danger);
}
.deleteDialog {
  @apply space-y-3 p-5 text-[12px];
  color: #6b778c;
}
.deleteDialog p {
  @apply m-0 leading-5;
}
.deleteDialog strong {
  color: #172b4d;
}
.deleteError {
  @apply border px-3 py-2 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}

@media (max-width: 1100px) {
  .metricGrid {
    @apply grid-cols-2;
  }
}
</style>
