<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import type { PageAction } from '@/components/page';
import { ListPageSurface } from '@/components/page';
import { UiButton } from '@/components/ui';
import { getDictItemsApi } from '@/api/core/dict';
import { getProjectPagedListApi, type ProjectApi } from '@/api/core/project';
import { buildProjectListPageModel, type ProjectListRow } from '@/features/projects-new/model/project-api-page.models';
import { hasAccessCode } from '@/session/access-store';
import ProjectCreateDialog from '@/features/projects-new/views/ProjectCreateDialog.vue';
import ProjectDetailDialog from '@/features/projects-new/views/ProjectDetailDialog.vue';

const props = withDefaults(defineProps<{ mode?: 'center' | 'close' | 'init' }>(), { mode: 'center' });

const pageSize = 20;
const pageNumber = ref(1);
const searchQuery = ref('');
const statusFilter = ref('');
const onlyMyRelated = ref(false);
const statusOptions = ref<string[]>([]);
const result = ref<ProjectApi.PagedListResult | null>(null);
const loading = ref(false);
const error = ref('');
const selectedProject = ref<ProjectApi.ListItem | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const projectNotice = ref('');
const canCreate = computed(() => hasAccessCode('Project.Project.Create'));
const pageModel = computed(() =>
  buildProjectListPageModel(
    result.value,
    statusOptions.value,
    loading.value,
    error.value || null,
    canCreate.value,
    props.mode,
  ),
);
const remotePagination = computed(() => ({
  currentPage: pageNumber.value,
  pageSize,
  searchQuery: searchQuery.value,
  totalCount: result.value?.totalCount ?? 0,
}));
const remoteFilterValues = computed(() => ({
  scope: onlyMyRelated.value ? '我的相关项目' : '全部项目',
  status: statusFilter.value || '全部状态',
}));

let activeRequest: AbortController | null = null;
let searchTimer: ReturnType<typeof setTimeout> | undefined;
let requestVersion = 0;

async function loadPage() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';

  try {
    result.value = await getProjectPagedListApi(
      {
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        onlyMyRelated: onlyMyRelated.value,
        skipCount: (pageNumber.value - 1) * pageSize,
        status: statusFilter.value || undefined,
      },
      controller.signal,
    );
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '项目列表读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function handleSearchChange(value: string) {
  searchQuery.value = value;
  pageNumber.value = 1;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void loadPage(), 250);
}

function handleFilterChange(key: string, value: string) {
  if (key === 'status') statusFilter.value = value === '全部状态' ? '' : value;
  if (key === 'scope') onlyMyRelated.value = value === '我的相关项目';
  pageNumber.value = 1;
  void loadPage();
}

function handlePageChange(nextPage: number) {
  pageNumber.value = nextPage;
  void loadPage();
}

function openProject(row: ProjectListRow) {
  const item = result.value?.items.find((project) => project.id === row.id);
  if (!item) return;
  selectedProject.value = item;
  detailOpen.value = true;
}

function projectCell(row: ProjectListRow, key: string): string {
  return String(row[key] ?? '');
}

function exportRows(action: PageAction, selectedRows: ProjectListRow[]) {
  if (action.kind === 'create') {
    if (canCreate.value) createOpen.value = true;
    return;
  }
  if (action.kind === 'refresh') {
    void loadPage();
    return;
  }
  if (action.kind !== 'export') return;

  const rows = selectedRows.length ? selectedRows : pageModel.value.rows;
  const columns = pageModel.value.columns;
  const csv = [
    columns.map((column) => column.title),
    ...rows.map((row) => columns.map((column) => projectCell(row, column.key))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/gu, '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '项目列表-当前页.csv';
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

async function handleProjectCreated(projectId: string) {
  pageNumber.value = 1;
  searchQuery.value = '';
  statusFilter.value = '';
  onlyMyRelated.value = false;
  await loadPage();
  const item = result.value?.items.find((project) => project.id === projectId);
  if (item) {
    projectNotice.value = '项目已创建。';
    selectedProject.value = item;
    detailOpen.value = true;
  } else if (!error.value) {
    projectNotice.value = '项目已创建，但当前账号数据范围暂未返回该项目。';
  }
}

onMounted(() => {
  void loadPage();
  if (props.mode === 'init' && canCreate.value) createOpen.value = true;
  void getDictItemsApi('project_status')
    .then((items) => {
      statusOptions.value = items.map((item) => item.value).filter(Boolean);
    })
    .catch(() => {
      statusOptions.value = [];
    });
});

onBeforeUnmount(() => {
  requestVersion += 1;
  activeRequest?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :remote-pagination="remotePagination"
    :remote-filter-values="remoteFilterValues"
    @action="exportRows"
    @filter-change="handleFilterChange"
    @page-change="handlePageChange"
    @row="openProject"
    @search-change="handleSearchChange"
  >
    <template #notice>
      <div
        v-if="projectNotice"
        role="status"
      >
        {{ projectNotice }}
      </div>
      <div
        v-if="loading || error"
        role="status"
        aria-live="polite"
      >
        <span>{{ loading ? '正在读取当前账号可见的项目…' : error }}</span>
        <UiButton
          v-if="error"
          variant="secondary"
          size="sm"
          @click="loadPage"
        >
          重试
        </UiButton>
      </div>
      <div v-else-if="props.mode === 'close'">
        项目状态更新位于项目详情；收尾条件由服务端检查，当前没有单独的归档接口。
      </div>
      <div v-else>项目进度、风险和我的关系由后端返回；列表按服务端数据范围、筛选和分页展示。</div>
    </template>
  </ListPageSurface>

  <ProjectCreateDialog
    v-model:open="createOpen"
    @created="handleProjectCreated"
  />

  <ProjectDetailDialog
    v-model:open="detailOpen"
    :project-id="selectedProject?.id ?? ''"
    :project-name="selectedProject?.name ?? ''"
    @changed="loadPage"
  />
</template>
