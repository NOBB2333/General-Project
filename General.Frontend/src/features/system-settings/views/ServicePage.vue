<script setup lang="ts">
import { Building2, ChevronRight, Network, Plus, Search, TriangleAlert, UsersRound } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast, UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import { serviceReferenceFixture } from '../model/service.data';
import type { ServiceDepartmentDraft, ServiceDepartmentRow } from '../model/service.types';
import ServiceDepartmentTable from './service/ServiceDepartmentTable.vue';

const departments = ref<ServiceDepartmentRow[]>(cloneReferenceRows(serviceReferenceFixture.rows));
const expandedIds = ref(new Set(['rd', 'engineering', 'sales']));
const search = ref('');
const createOpen = ref(false);
const draft = ref<ServiceDepartmentDraft>(emptyDraft());

const childParentIds = computed(
  () => new Set(departments.value.map((row) => row.parentId).filter((id): id is string => Boolean(id))),
);
const searchable = computed(() => search.value.trim().length > 0);
const visibleRows = computed(() => {
  const query = search.value.trim().toLocaleLowerCase();
  if (query) {
    return departments.value.filter((row) =>
      [row.name, row.code, row.manager].some((value) => value.toLocaleLowerCase().includes(query)),
    );
  }
  return departments.value.filter((row) => row.parentId === null || expandedIds.value.has(row.parentId));
});
const rootCount = computed(() => departments.value.filter((row) => row.level === 1).length);
const headcount = computed(() =>
  departments.value.filter((row) => row.level === 1).reduce((sum, row) => sum + row.headcount, 0),
);
const formingCount = computed(() => departments.value.filter((row) => row.status === '筹建中').length);
const metrics = computed(
  () =>
    [
      { id: 'root', label: '一级部门', value: rootCount.value, icon: Building2, tone: 'primary' },
      { id: 'total', label: '全部部门', value: departments.value.length, icon: Network, tone: 'purple' },
      { id: 'people', label: '总人数', value: headcount.value, icon: UsersRound, tone: 'success' },
      { id: 'forming', label: '筹建中', value: formingCount.value, icon: TriangleAlert, tone: 'warning' },
    ] as const,
);

function emptyDraft(): ServiceDepartmentDraft {
  return { name: '', code: '', manager: '', headcount: '0' };
}

function toggleDepartment(departmentId: string) {
  const next = new Set(expandedIds.value);
  if (next.has(departmentId)) next.delete(departmentId);
  else next.add(departmentId);
  expandedIds.value = next;
}

function expandAll() {
  expandedIds.value = new Set(childParentIds.value);
}

function collapseAll() {
  expandedIds.value = new Set();
}

function openCreateDialog() {
  draft.value = emptyDraft();
  createOpen.value = true;
}

function saveDepartment() {
  const name = draft.value.name.trim();
  if (!name) {
    toast.warning('请填写部门名称');
    return;
  }
  const parsedHeadcount = Number.parseInt(draft.value.headcount, 10);
  departments.value.push({
    id: `local-department-${Date.now()}`,
    parentId: null,
    name,
    code: draft.value.code.trim().toUpperCase() || `DEP-${departments.value.length + 1}`,
    level: 1,
    manager: draft.value.manager.trim() || '待指定',
    headcount: Number.isFinite(parsedHeadcount) && parsedHeadcount > 0 ? parsedHeadcount : 0,
    status: '筹建中',
  });
  createOpen.value = false;
  toast.success('部门已保存到本地');
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="board"
    data-page-route="/system/service"
    data-system-service-page
  >
    <header :class="$style.pageHeader">
      <div>
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>系统设置</span>
          <ChevronRight
            :size="12"
            :stroke-width="1.8"
          />
          <strong>部门管理</strong>
        </nav>
        <h1>部门管理</h1>
      </div>
      <UiButton
        variant="primary"
        size="md"
        @click="openCreateDialog"
      >
        <template #leading>
          <Plus
            :size="16"
            :stroke-width="1.8"
          />
        </template>
        新建部门
      </UiButton>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="部门概览"
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
        <strong>{{ metric.value }}</strong>
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
          aria-label="搜索部门"
          placeholder="搜索部门名称、编码、负责人..."
        />
      </label>
      <UiButton
        size="sm"
        @click="expandAll"
        >全部展开</UiButton
      >
      <UiButton
        size="sm"
        @click="collapseAll"
        >全部折叠</UiButton
      >
    </div>

    <ServiceDepartmentTable
      :rows="visibleRows"
      :expanded-ids="expandedIds"
      :searchable="searchable"
      :child-parent-ids="childParentIds"
      @toggle="toggleDepartment"
    />

    <UiDialog
      v-model:open="createOpen"
      title="新建部门"
      description="新部门仅保存在本地复刻页面，初始状态为筹建中。"
      size="sm"
    >
      <form
        :class="$style.createForm"
        @submit.prevent="saveDepartment"
      >
        <UiFormField
          label="部门名称"
          required
        >
          <UiInput
            v-model="draft.name"
            autofocus
            placeholder="请输入部门名称"
          />
        </UiFormField>
        <UiFormField label="部门编码">
          <UiInput
            v-model="draft.code"
            placeholder="例如 OPS"
          />
        </UiFormField>
        <UiFormField label="负责人">
          <UiInput
            v-model="draft.manager"
            placeholder="请输入负责人"
          />
        </UiFormField>
        <UiFormField label="人数">
          <UiInput
            v-model="draft.headcount"
            type="number"
            min="0"
          />
        </UiFormField>
        <div :class="$style.dialogActions">
          <UiButton @click="createOpen = false">取消</UiButton>
          <UiButton
            type="submit"
            variant="primary"
            >保存</UiButton
          >
        </div>
      </form>
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
.createForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.dialogActions {
  @apply col-span-2 mt-2 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}

@media (max-width: 1100px) {
  .metricGrid {
    @apply grid-cols-2;
  }
}
</style>
