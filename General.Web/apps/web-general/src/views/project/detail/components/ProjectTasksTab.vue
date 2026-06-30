<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, ref } from 'vue';

import {
  Alert,
  Button,
  Card,
  Checkbox,
  Col,
  Descriptions,
  Divider,
  Drawer,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Progress,
  Row,
  Select,
  Space,
  Switch,
  Tabs,
  Tag,
  Upload,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { FileApi, ProjectApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useProjectDetailContext } from '../context';

type SprintTableRow = ProjectApi.SprintItem & { __editing?: boolean };
type DemandTableRow = ProjectApi.TaskItem & { __editing?: boolean };

const {
  sprintForm,
  sprintStatusOptions,
  createTaskPrdUploadRequest,
  deletingSprintId,
  detail,
  filteredTasks,
  formatDate,
  formatHours,
  dependencyTypeOptions,
  editingTaskId,
  getPriorityColor,
  getTaskStatusColor,
  handleCancelSprintEdit,
  handleCancelTaskEdit,
  handleChangeTaskStatus,
  handleCreateSprintDraft,
  handleCreateTaskDraft,
  handleDeleteSprint,
  handleEditSprint,
  handleEditTask,
  handleSaveSprint,
  handleSaveTask,
  handleCloseTaskDetail,
  handleCreateTaskDetailDependency,
  handleOpenTaskDetail,
  handleRecordTaskProgress,
  editingSprintId,
  kanbanColumns,
  loading,
  memberSelectOptions,
  openProjectFilePreview,
  requirementPriorityOptions,
  savingTask,
  savingSprint,
  savingTaskDependency,
  savingTaskProgress,
  sprintColumns,
  sprintRows,
  selectedTaskDependencies,
  selectedTaskDetail,
  selectedTaskRequirement,
  selectedTaskWorklogs,
  selectedTaskSprintMetrics,
  switchProjectDetailTab,
  taskCalendarAnchorDate,
  taskCalendarRows,
  taskCalendarTitle,
  taskColumns,
  taskDetailDependencyForm,
  taskProgressForm,
  taskProgressStatusOptions,
  taskSprintFilter,
  taskSprintMetrics,
  taskSprintOptions,
  taskScope,
  taskScopeOptions,
  taskTimelineDays,
  taskTimelineGridStyle,
  taskTimelineRows,
  taskView,
  taskViewOptions,
  taskWbsNodeOptions,
  taskSelectOptions,
  shiftTaskCalendarMonth,
  showSprintForm,
  showTaskInlineForm,
  taskForm,
} = useProjectDetailContext();

const activeTaskInnerTab = ref('tasks');
const draggingTaskId = ref('');

const sprintTableRows = computed<SprintTableRow[]>(() => {
  const rows = sprintRows.value as SprintTableRow[];
  if (!showSprintForm.value) {
    return rows;
  }

  const editingRow = {
    __editing: true,
    id: editingSprintId.value || '__new_sprint__',
  } as SprintTableRow;

  if (editingSprintId.value) {
    return rows.map((row) => (row.id === editingSprintId.value ? editingRow : row));
  }

  return [editingRow, ...rows];
});

const demandTableRows = computed<DemandTableRow[]>(() => {
  const rows = filteredTasks.value as DemandTableRow[];
  if (!showTaskInlineForm.value) {
    return rows;
  }

  const editingRow = {
    __editing: true,
    id: editingTaskId.value || '__new_demand__',
  } as DemandTableRow;

  if (editingTaskId.value) {
    return rows.map((row) => (row.id === editingTaskId.value ? editingRow : row));
  }

  return [editingRow, ...rows];
});

const participantSelectOptions = computed(() => {
  const options = new Map<string, { label: string; value: string }>();

  for (const option of memberSelectOptions.value) {
    options.set(option.value, option);
  }

  function addUser(userId?: null | string, userName?: null | string) {
    if (!userId || options.has(userId)) {
      return;
    }

    options.set(userId, {
      label: userName || '未在项目成员中',
      value: userId,
    });
  }

  for (const task of detail.value?.tasks || []) {
    addUser(task.ownerUserId, task.ownerName);
    for (const [index, userId] of (task.productOwnerUserIds || []).entries()) {
      addUser(userId, task.productOwnerNames?.[index] || task.productOwnerName);
    }
    for (const [index, userId] of (task.developerOwnerUserIds || []).entries()) {
      addUser(userId, task.developerOwnerNames?.[index] || task.developerOwnerName);
    }
    for (const [index, userId] of (task.testerOwnerUserIds || []).entries()) {
      addUser(userId, task.testerOwnerNames?.[index] || task.testerOwnerName);
    }
    addUser(task.productOwnerUserId, task.productOwnerName);
    addUser(task.developerOwnerUserId, task.developerOwnerName);
    addUser(task.testerOwnerUserId, task.testerOwnerName);
  }

  return [...options.values()];
});

const taskDetailDependencyOptions = computed(() =>
  taskSelectOptions.value.filter((item) => item.value !== selectedTaskDetail.value?.id)
);
const demandPagination = useLocalVxePagination(demandTableRows, { defaultPageSize: 10 });
const sprintPagination = useLocalVxePagination(sprintTableRows, { defaultPageSize: 10 });
const taskGridOptions = computed<VxeGridProps<DemandTableRow>>(() => ({
  cellConfig: {
    height: showTaskInlineForm.value ? 104 : 64,
  },
  cellClassName: ({ row }: { row: DemandTableRow }) =>
    isDemandEditingRow(row) ? 'project-detail__demand-edit-cell' : '',
  columns: taskColumns,
  data: demandPagination.pagedRows.value,
  loading: loading.value,
  maxHeight: showTaskInlineForm.value ? 640 : 560,
  pagerConfig: demandPagination.pagerConfig.value,
  rowClassName: ({ row }: { row: DemandTableRow }) =>
    isDemandEditingRow(row) ? 'project-detail__demand-edit-row' : '',
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));
const sprintGridOptions = computed<VxeGridProps<SprintTableRow>>(() => ({
  columns: sprintColumns,
  data: sprintPagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: sprintPagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
}));

const taskGridEvents: NativeVxeGridEvents<DemandTableRow> = {
  'page-change': demandPagination.handlePageChange,
};
const sprintGridEvents: NativeVxeGridEvents<SprintTableRow> = {
  'page-change': sprintPagination.handlePageChange,
};

const [TaskGrid] = useVbenVxeGrid<DemandTableRow>({
  gridEvents: taskGridEvents,
  gridOptions: {},
});
const [SprintGrid] = useVbenVxeGrid<SprintTableRow>({
  gridEvents: sprintGridEvents,
  gridOptions: {},
});

function isSprintEditingRow(record: ProjectApi.SprintItem | SprintTableRow) {
  return Boolean((record as SprintTableRow).__editing);
}

function isDemandEditingRow(record: ProjectApi.TaskItem | DemandTableRow) {
  return Boolean((record as DemandTableRow).__editing);
}

function readTaskField(record: ProjectApi.TaskItem, key: string) {
  return record[key as keyof ProjectApi.TaskItem] || '-';
}

function readMemberLabel(userId?: null | string, name?: null | string) {
  if (!userId) {
    return name || '-';
  }

  return memberSelectOptions.value.find((item) => item.value === userId)?.label || name || '-';
}

function resolveParticipantLabels(
  task: ProjectApi.TaskItem,
  role: 'developer' | 'product' | 'tester',
) {
  const roleData = {
    developer: {
      fallbackName: task.developerOwnerName,
      fallbackUserId: task.developerOwnerUserId,
      names: task.developerOwnerNames || [],
      userIds: task.developerOwnerUserIds || [],
    },
    product: {
      fallbackName: task.productOwnerName,
      fallbackUserId: task.productOwnerUserId,
      names: task.productOwnerNames || [],
      userIds: task.productOwnerUserIds || [],
    },
    tester: {
      fallbackName: task.testerOwnerName,
      fallbackUserId: task.testerOwnerUserId,
      names: task.testerOwnerNames || [],
      userIds: task.testerOwnerUserIds || [],
    },
  }[role];
  const names = roleData.names.filter(Boolean);
  if (names.length > 0) {
    return names;
  }

  const ids = roleData.userIds;
  if (ids.length > 0) {
    return ids.map((id) => readMemberLabel(id, roleData.fallbackName));
  }

  const label = readMemberLabel(roleData.fallbackUserId, roleData.fallbackName);
  return label === '-' ? [] : [label];
}

function handleOpenTaskDetailById(taskId: string) {
  const task = filteredTasks.value.find((item) => item.id === taskId);
  if (task) {
    handleOpenTaskDetail(task);
  }
}

function resolveDependencyTypeLabel(value: string) {
  return dependencyTypeOptions.find((item) => item.value === value)?.label || value;
}

function resolveTaskWbsLabel(item: ProjectApi.TaskItem) {
  return [item.wbsCode, item.wbsNodeName].filter(Boolean).join(' · ') || '-';
}

function resolveSprintCode(record: ProjectApi.SprintItem) {
  if (isSprintEditingRow(record as SprintTableRow)) {
    return '自动生成';
  }

  const code = record.sprintCode;
  if (!code) {
    return '-';
  }

  const matched = code.match(/^SPR[-_]?(\d+)$/i);
  if (matched?.[1]) {
    return `排期-${matched[1].padStart(3, '0')}`;
  }

  return code.replace(/Sprint/gi, '排期');
}

function formatSprintDisplayName(name?: null | string) {
  return (name || '')
    .replace(/第\s*(\d+)\s*迭代/g, '第 $1 排期')
    .replace(/Sprint/gi, '排期');
}

function resolveTaskPrdFiles(task: ProjectApi.TaskItem) {
  return createTaskPrdUploadRequest.files?.(task) || [];
}

function resolveCurrentTaskPrdFile(task: ProjectApi.TaskItem): FileApi.FileItem | undefined {
  const files = resolveTaskPrdFiles(task);
  return files.find((file) => file.isCurrentVersion) || files[0];
}

function resolveTaskPrdLabel(task: ProjectApi.TaskItem) {
  const files = resolveTaskPrdFiles(task);
  if (files.length === 0) {
    return '无 PRD';
  }

  const currentFile = files.find((file) => file.isCurrentVersion) || files[0];
  return `当前 V${currentFile?.versionNo || 1}`;
}

function readFirstSelectedUserId(value: unknown) {
  return Array.isArray(value) && typeof value[0] === 'string' ? value[0] : undefined;
}

function handleKanbanDragStart(item: ProjectApi.TaskItem) {
  draggingTaskId.value = item.id;
}

function handleKanbanDragEnd() {
  draggingTaskId.value = '';
}

async function handleDropTaskToStatus(status: string) {
  const task = filteredTasks.value.find((item) => item.id === draggingTaskId.value);
  draggingTaskId.value = '';
  if (task) {
    await handleChangeTaskStatus(task, status);
  }
}

</script>

<template>
  <section class="project-detail__section">
    <Card :bordered="false" class="project-detail__inner-card rounded-2xl">
      <Alert
        class="project-detail__alert"
        message="排期优先按需求所属排期查看；未进入排期的需求单独筛出，方便 PM 判断哪些 WBS 工作包还没有进入本期。"
        show-icon
        type="info"
      />
      <Tabs v-model:active-key="activeTaskInnerTab" class="project-detail__inner-tabs">
        <Tabs.TabPane key="tasks" tab="需求">
          <div class="project-detail__demand-workspace-head">
            <div
              class="project-detail__overview-list project-detail__overview-list--metrics project-detail__task-sprint-metrics project-detail__task-sprint-metrics--demand"
            >
              <div
                v-for="metric in selectedTaskSprintMetrics"
                :key="metric.label"
                class="project-detail__overview-item"
              >
                <span>{{ metric.label }}</span>
                <strong>{{ metric.value }}</strong>
              </div>
            </div>
            <div class="project-detail__demand-toolbar">
              <Space class="project-detail__demand-toolbar-left" wrap>
                <span class="project-detail__toolbar-label">筛选</span>
                <Select
                  v-model:value="taskSprintFilter"
                  :options="taskSprintOptions"
                  placeholder="排期"
                  style="width: 240px"
                />
                <Button
                  v-for="option in taskScopeOptions"
                  :key="option.key"
                  :type="taskScope === option.key ? 'primary' : 'default'"
                  @click="taskScope = option.key"
                >
                  {{ option.label }}
                </Button>
              </Space>
              <Space class="project-detail__demand-view-switch" wrap>
                <Button class="project-detail__primary-action" type="primary" @click="handleCreateTaskDraft()">新建需求</Button>
                <Button @click="switchProjectDetailTab('plan')">维护 WBS</Button>
                <Button
                  v-for="option in taskViewOptions"
                  :key="option.key"
                  :type="taskView === option.key ? 'primary' : 'default'"
                  @click="taskView = option.key"
                >
                  {{ option.label }}
                </Button>
              </Space>
            </div>
          </div>
          <template v-if="taskView === 'list'">
            <TaskGrid
              class="app-vxe-card-grid"
              grid-class="project-detail__demand-grid"
              :grid-events="taskGridEvents"
              :grid-options="taskGridOptions"
            >
              <template #taskCode="{ row }">
                <Tag v-if="isDemandEditingRow(row)" color="processing">{{ taskForm.taskCode || '自动生成' }}</Tag>
                <span v-else>{{ row.taskCode }}</span>
              </template>
              <template #title="{ row }">
                <Input v-if="isDemandEditingRow(row)" v-model:value="taskForm.title" :maxlength="256" placeholder="需求标题" />
                <span v-else>{{ row.title }}</span>
              </template>
              <template #description="{ row }">
                <Input.TextArea
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.description"
                  :maxlength="2000"
                  class="project-detail__demand-description-input"
                  placeholder="说明需求要解决什么、怎么验收、关键约束是什么"
                  :rows="4"
                />
                <div v-else class="project-detail__demand-description-cell">
                  {{ row.description || '-' }}
                </div>
              </template>
              <template #wbsNodeName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.wbsNodeId"
                  allow-clear
                  :options="taskWbsNodeOptions"
                  placeholder="所属 WBS"
                  show-search
                />
                <span v-else>{{ readTaskField(row, 'wbsNodeName') }}</span>
              </template>
              <template #sprintName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.sprintId"
                  allow-clear
                  :options="taskSprintOptions.filter((item) => !['all', 'current', 'unscheduled'].includes(String(item.value)))"
                  placeholder="所属排期"
                  show-search
                />
                <span v-else>{{ formatSprintDisplayName(String(readTaskField(row, 'sprintName'))) }}</span>
              </template>
              <template #ownerName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.ownerUserId"
                  :options="memberSelectOptions"
                  option-filter-prop="label"
                  placeholder="负责人"
                  show-search
                />
                <span v-else>{{ row.ownerName || '-' }}</span>
              </template>
              <template #productOwnerName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.productOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="participantSelectOptions"
                  option-filter-prop="label"
                  placeholder="产品"
                  show-search
                  @change="(value) => { taskForm.productOwnerUserId = readFirstSelectedUserId(value); }"
                />
                <div v-else class="project-detail__tag-list project-detail__tag-list--compact">
                  <Tag
                    v-for="name in resolveParticipantLabels(row, 'product')"
                    :key="name"
                  >
                    {{ name }}
                  </Tag>
                  <span
                    v-if="resolveParticipantLabels(row, 'product').length === 0"
                    class="project-detail__muted"
                  >
                    -
                  </span>
                </div>
              </template>
              <template #developerOwnerName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.developerOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="participantSelectOptions"
                  option-filter-prop="label"
                  placeholder="开发"
                  show-search
                  @change="(value) => { taskForm.developerOwnerUserId = readFirstSelectedUserId(value); }"
                />
                <div v-else class="project-detail__tag-list project-detail__tag-list--compact">
                  <Tag
                    v-for="name in resolveParticipantLabels(row, 'developer')"
                    :key="name"
                  >
                    {{ name }}
                  </Tag>
                  <span
                    v-if="resolveParticipantLabels(row, 'developer').length === 0"
                    class="project-detail__muted"
                  >
                    -
                  </span>
                </div>
              </template>
              <template #testerOwnerName="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.testerOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="participantSelectOptions"
                  option-filter-prop="label"
                  placeholder="测试"
                  show-search
                  @change="(value) => { taskForm.testerOwnerUserId = readFirstSelectedUserId(value); }"
                />
                <div v-else class="project-detail__tag-list project-detail__tag-list--compact">
                  <Tag
                    v-for="name in resolveParticipantLabels(row, 'tester')"
                    :key="name"
                  >
                    {{ name }}
                  </Tag>
                  <span
                    v-if="resolveParticipantLabels(row, 'tester').length === 0"
                    class="project-detail__muted"
                  >
                    -
                  </span>
                </div>
              </template>
              <template #plannedStartTime="{ row }">
                <Input v-if="isDemandEditingRow(row)" v-model:value="taskForm.plannedStartTime" type="date" />
                <span v-else>{{ formatDate(row.plannedStartTime) }}</span>
              </template>
              <template #plannedEndTime="{ row }">
                <Input v-if="isDemandEditingRow(row)" v-model:value="taskForm.plannedEndTime" type="date" />
                <span v-else>{{ formatDate(row.plannedEndTime) }}</span>
              </template>
              <template #estimatedWorkHours="{ row }">
                <InputNumber
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.estimatedWorkHours"
                  :min="0"
                  :precision="1"
                />
                <span v-else>{{ formatHours(row.estimatedWorkHours) }}</span>
              </template>
              <template #actualWorkHours="{ row }">
                <span v-if="isDemandEditingRow(row)" class="project-detail__muted">-</span>
                <span v-else>{{ formatHours(row.actualWorkHours) }}</span>
              </template>
              <template #priority="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.priority"
                  :options="requirementPriorityOptions"
                />
                <Tag v-else :color="getPriorityColor(row.priority)">
                  {{ row.priority }}
                </Tag>
              </template>
              <template #isOutOfContract="{ row }">
                <Switch v-if="isDemandEditingRow(row)" v-model:checked="taskForm.isOutOfContract" checked-children="是" un-checked-children="否" />
                <Tag v-else :color="row.isOutOfContract ? 'warning' : 'default'">
                  {{ row.isOutOfContract ? '是' : '否' }}
                </Tag>
              </template>
              <template #prd="{ row }">
                <span v-if="isDemandEditingRow(row)" class="project-detail__muted">-</span>
                <div v-else class="project-detail__prd-cell">
                  <Button
                    v-if="resolveCurrentTaskPrdFile(row)"
                    size="small"
                    type="link"
                    @click="openProjectFilePreview(resolveCurrentTaskPrdFile(row)!)"
                  >
                    {{ resolveTaskPrdLabel(row) }}
                  </Button>
                  <span v-else>{{ resolveTaskPrdLabel(row) }}</span>
                  <small v-if="resolveTaskPrdFiles(row).length > 1">
                    历史 {{ resolveTaskPrdFiles(row).length - 1 }} 份
                  </small>
                  <Upload
                    v-if="!row.id.startsWith('__')"
                    :custom-request="createTaskPrdUploadRequest(row)"
                    :show-upload-list="false"
                  >
                    <Button size="small" type="link">上传 PRD</Button>
                  </Upload>
                </div>
              </template>
              <template #status="{ row }">
                <Select
                  v-if="isDemandEditingRow(row)"
                  v-model:value="taskForm.status"
                  :options="taskProgressStatusOptions"
                />
                <Tag
                  v-else
                  :color="
                    getTaskStatusColor(
                      row.status,
                      row.isBlocked ? 'error' : 'blue',
                    )
                  "
                >
                  {{ row.status }}
                </Tag>
              </template>
              <template #actions="{ row }">
                <Space v-if="isDemandEditingRow(row)">
                  <Button :loading="savingTask" size="small" type="primary" @click="handleSaveTask">
                    保存
                  </Button>
                  <Button size="small" @click="handleCancelTaskEdit">取消</Button>
                </Space>
                <Space v-else>
                  <Button
                    size="small"
                    type="link"
                    @click="handleOpenTaskDetail(row)"
                  >
                    详情
                  </Button>
                  <Button
                    size="small"
                    type="link"
                    @click="handleEditTask(row)"
                  >
                    编辑
                  </Button>
                </Space>
              </template>
            </TaskGrid>
          </template>
          <template v-else-if="taskView === 'board'">
            <div class="project-detail__kanban">
              <article
                v-for="column in kanbanColumns"
                :key="column.status"
                class="project-detail__kanban-column"
                @dragover.prevent
                @drop="handleDropTaskToStatus(column.status)"
              >
                <header>
                  <strong>{{ column.status }}</strong>
                  <span>{{ column.items.length }}</span>
                </header>
                <div v-if="column.items.length === 0" class="project-detail__muted">暂无需求</div>
                <div v-else class="project-detail__kanban-list">
                  <section
                    v-for="item in column.items"
                    :key="item.id"
                    class="project-detail__kanban-card"
                    draggable="true"
                    @dragend="handleKanbanDragEnd"
                    @dragstart="handleKanbanDragStart(item)"
                  >
                    <div class="project-detail__item-head">
                      <span class="project-detail__kanban-code">{{ item.taskCode }}</span>
                      <Tag :color="getPriorityColor(item.priority)">{{ item.priority }}</Tag>
                      <Tag v-if="item.isBlocked" color="error">阻塞</Tag>
                    </div>
                    <strong>{{ item.title }}</strong>
                    <span>{{ item.ownerName }} / {{ formatSprintDisplayName(item.sprintName) || '未进排期' }}</span>
                    <span>{{ formatDate(item.plannedStartTime) }} - {{ formatDate(item.plannedEndTime) }}</span>
                    <div class="project-detail__kanban-card-actions">
                      <Button size="small" type="link" @click="handleOpenTaskDetail(item)">
                        详情
                      </Button>
                      <Button size="small" type="link" @click="handleEditTask(item)">
                        编辑
                      </Button>
                    </div>
                  </section>
                </div>
              </article>
            </div>
          </template>

          <template v-else-if="taskView === 'calendar'">
            <div class="project-detail__calendar-range-panel">
              <div class="project-detail__calendar-head">
                <div>
                  <strong>{{ taskCalendarTitle }}</strong>
                  <p class="project-detail__calendar-subtitle">
                    按周展示跨日事件条，适合看需求在自然日历中的连续占用区间。
                  </p>
                </div>
                <Space>
                  <Button size="small" @click="shiftTaskCalendarMonth(-1)">上月</Button>
                  <Button size="small" @click="taskCalendarAnchorDate = null">回到需求月份</Button>
                  <Button size="small" @click="shiftTaskCalendarMonth(1)">下月</Button>
                </Space>
              </div>
              <div
                class="project-detail__calendar-weekdays project-detail__calendar-weekdays--sticky"
              >
                <span>周区间</span>
                <span v-for="label in ['日', '一', '二', '三', '四', '五', '六']" :key="label">{{
                  label
                }}</span>
              </div>
              <div class="project-detail__calendar-range-rows">
                <article
                  v-for="row in taskCalendarRows"
                  :key="row.key"
                  class="project-detail__calendar-range-row"
                >
                  <div class="project-detail__calendar-range-label">
                    {{ row.label }}
                  </div>
                  <div class="project-detail__calendar-range-track">
                    <div class="project-detail__calendar-range-days">
                      <div
                        v-for="day in row.days"
                        :key="day.key"
                        :class="[
                          'project-detail__calendar-range-day',
                          {
                            'project-detail__calendar-range-day--muted': !day.inCurrentMonth,
                            'project-detail__calendar-range-day--today': day.isToday,
                          },
                        ]"
                      >
                        {{ day.dayLabel }}
                      </div>
                    </div>
                    <div v-if="row.bars.length === 0" class="project-detail__calendar-range-empty">
                      本周暂无跨日需求
                    </div>
                    <div v-else class="project-detail__calendar-range-bars">
                      <div
                        v-for="bar in row.bars"
                        :key="bar.key"
                        class="project-detail__calendar-range-bar"
                        role="button"
                        tabindex="0"
                        :style="{
                          background: bar.color,
                          gridColumn: `${bar.startColumn + 1} / span ${bar.span}`,
                        }"
                        @click="handleOpenTaskDetailById(bar.key)"
                        @keydown.enter="handleOpenTaskDetailById(bar.key)"
                      >
                        <strong>{{ bar.title }}</strong>
                        <span>{{ bar.subtitle }}</span>
                      </div>
                    </div>
                  </div>
                </article>
              </div>
            </div>
          </template>

          <template v-else>
            <div class="project-detail__timeline-title">
              <strong>需求甘特</strong>
              <span>按计划开始和计划完成展示需求占用区间，支持横向滚动查看。</span>
            </div>
            <div v-if="taskTimelineDays.length === 0" class="project-detail__muted">
              暂无可展示需求。
            </div>
            <div v-else class="project-detail__timeline-shell">
              <div class="project-detail__timeline-head">
                <div class="project-detail__timeline-side">需求</div>
                <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
                  <div
                    v-for="day in taskTimelineDays"
                    :key="day.toISOString()"
                    class="project-detail__timeline-day"
                  >
                    {{ day.getMonth() + 1 }}/{{ day.getDate() }}
                  </div>
                </div>
              </div>
              <div
                v-for="item in taskTimelineRows"
                :key="item.id"
                class="project-detail__timeline-row"
              >
                <div class="project-detail__timeline-side">
                  <strong>{{ item.title }}</strong>
                  <small>{{ item.ownerName }} / {{ item.status }}</small>
                  <Button size="small" type="link" @click="handleOpenTaskDetail(item)">
                    详情
                  </Button>
                </div>
                <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
                  <div
                    class="project-detail__timeline-bar"
                    :style="{
                      '--task-bar-color': item.color,
                      gridColumn: `${item.startOffset + 1} / span ${item.span}`,
                    }"
                  >
                    <span>{{ item.taskCode }}</span>
                    <strong>{{ formatDate(item.plannedStartTime) }} - {{ formatDate(item.plannedEndTime) }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </Tabs.TabPane>

        <Tabs.TabPane key="sprint" tab="排期">
          <div
            class="project-detail__overview-list project-detail__overview-list--metrics project-detail__task-sprint-metrics project-detail__task-sprint-metrics--sprint"
          >
            <div
              v-for="metric in taskSprintMetrics"
              :key="metric.label"
              class="project-detail__overview-item"
            >
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>

          <div class="project-detail__section-head project-detail__section-head--actions-only">
            <Button class="project-detail__primary-action" type="primary" @click="handleCreateSprintDraft()">新增排期</Button>
          </div>
          <SprintGrid class="app-vxe-card-grid" :grid-events="sprintGridEvents" :grid-options="sprintGridOptions">
            <template #sprintCode="{ row }">
              <Tag color="processing">{{ resolveSprintCode(row) }}</Tag>
            </template>
            <template #name="{ row }">
              <Input
                v-if="isSprintEditingRow(row)"
                v-model:value="sprintForm.name"
                :maxlength="128"
                placeholder="第 3 排期"
              />
              <span v-else>{{ formatSprintDisplayName(row.name) }}</span>
            </template>
            <template #ownerName="{ row }">
              <Select
                v-if="isSprintEditingRow(row)"
                v-model:value="sprintForm.ownerUserId"
                :options="memberSelectOptions"
                show-search
                style="width: 120px"
              />
              <span v-else>{{ row.ownerName || '-' }}</span>
            </template>
            <template #status="{ row }">
              <Select
                v-if="isSprintEditingRow(row)"
                v-model:value="sprintForm.status"
                :options="sprintStatusOptions"
                style="width: 100px"
              />
              <span v-else>{{ row.status }}</span>
            </template>
            <template #progress="{ row }">
              <InputNumber
                v-if="isSprintEditingRow(row)"
                v-model:value="sprintForm.progress"
                :max="100"
                :min="0"
                style="width: 150px"
              />
              <Progress v-else :percent="row.progress" size="small" />
            </template>
            <template #tasks="{ row }">
              <span v-if="isSprintEditingRow(row)" class="project-detail__muted">-</span>
              <span v-else>{{ row.closedTaskCount }} / {{ row.taskCount }}</span>
            </template>
            <template #overdueTaskCount="{ row }">
              <span v-if="isSprintEditingRow(row)" class="project-detail__muted">-</span>
              <Tag v-else :color="row.overdueTaskCount > 0 ? 'error' : 'success'">
                {{ row.overdueTaskCount }}
              </Tag>
            </template>
            <template #riskCount="{ row }">
              <span v-if="isSprintEditingRow(row)" class="project-detail__muted">-</span>
              <Tag
                v-else
                :color="row.highRiskCount > 0 ? 'error' : row.riskCount > 0 ? 'warning' : 'success'"
              >
                {{ row.riskCount }}
              </Tag>
            </template>
            <template #openIssueCount="{ row }">
              <span v-if="isSprintEditingRow(row)" class="project-detail__muted">-</span>
              <Tag v-else :color="row.openIssueCount > 0 ? 'warning' : 'success'">
                {{ row.openIssueCount }}
              </Tag>
            </template>
            <template #reportCount="{ row }">
              <span v-if="isSprintEditingRow(row)" class="project-detail__muted">-</span>
              <Space v-else>
                <span>{{ row.reportCount }}</span>
                <Tag :color="row.reportCount > 0 ? 'success' : 'default'">
                  {{ row.latestReportStatus || '未归档' }}
                </Tag>
              </Space>
            </template>
            <template #startDate="{ row }">
              <Input v-if="isSprintEditingRow(row)" v-model:value="sprintForm.startDate" type="date" />
              <span v-else>{{ formatDate(row.startDate) }}</span>
            </template>
            <template #endDate="{ row }">
              <Input v-if="isSprintEditingRow(row)" v-model:value="sprintForm.endDate" type="date" />
              <span v-else>{{ formatDate(row.endDate) }}</span>
            </template>
            <template #actions="{ row }">
              <Space v-if="isSprintEditingRow(row)">
                <Button :loading="savingSprint" size="small" type="primary" @click="handleSaveSprint">
                  保存
                </Button>
                <Button size="small" @click="handleCancelSprintEdit">取消</Button>
              </Space>
              <Space v-else>
                <Button
                  size="small"
                  type="link"
                  @click="handleEditSprint(row)"
                >
                  编辑
                </Button>
                <Popconfirm
                  title="确认删除这个排期？"
                  @confirm="handleDeleteSprint(row.id)"
                >
                  <Button
                    danger
                    :loading="deletingSprintId === row.id"
                    size="small"
                    type="link"
                  >
                    删除
                  </Button>
                </Popconfirm>
              </Space>
            </template>
          </SprintGrid>
        </Tabs.TabPane>
      </Tabs>
    </Card>

    <Drawer
      :open="Boolean(selectedTaskDetail)"
      :title="
        selectedTaskDetail
          ? `${selectedTaskDetail.taskCode} · ${selectedTaskDetail.title}`
          : '需求详情'
      "
      width="760"
      @close="handleCloseTaskDetail"
    >
      <div v-if="selectedTaskDetail" class="project-detail__task-detail">
        <Descriptions :column="2" bordered size="small">
          <Descriptions.Item label="状态">
            <Tag
              :color="
                getTaskStatusColor(
                  selectedTaskDetail.status,
                  selectedTaskDetail.isBlocked ? 'error' : 'blue'
                )
              "
            >
              {{ selectedTaskDetail.status }}
            </Tag>
            <Tag v-if="selectedTaskDetail.isBlocked" color="error">阻塞</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="优先级">
            <Tag :color="getPriorityColor(selectedTaskDetail.priority)">
              {{ selectedTaskDetail.priority }}
            </Tag>
          </Descriptions.Item>
          <Descriptions.Item label="合同外需求">
            <Tag :color="selectedTaskDetail.isOutOfContract ? 'warning' : 'default'">
              {{ selectedTaskDetail.isOutOfContract ? '是' : '否' }}
            </Tag>
          </Descriptions.Item>
          <Descriptions.Item label="负责人">
            {{ selectedTaskDetail.ownerName || '-' }}
          </Descriptions.Item>
          <Descriptions.Item label="产品">
            {{ resolveParticipantLabels(selectedTaskDetail, 'product').join('、') || '-' }}
          </Descriptions.Item>
          <Descriptions.Item label="开发">
            {{ resolveParticipantLabels(selectedTaskDetail, 'developer').join('、') || '-' }}
          </Descriptions.Item>
          <Descriptions.Item label="测试">
            {{ resolveParticipantLabels(selectedTaskDetail, 'tester').join('、') || '-' }}
          </Descriptions.Item>
          <Descriptions.Item label="计划日期">
            {{ formatDate(selectedTaskDetail.plannedStartTime) }} -
            {{ formatDate(selectedTaskDetail.plannedEndTime) }}
          </Descriptions.Item>
          <Descriptions.Item label="预计工时">
            {{ formatHours(selectedTaskDetail.estimatedWorkHours) }}
          </Descriptions.Item>
          <Descriptions.Item label="实际工时">
            {{ formatHours(selectedTaskDetail.actualWorkHours) }}
          </Descriptions.Item>
          <Descriptions.Item label="实际时间">
            {{ formatDate(selectedTaskDetail.actualStartTime) }} -
            {{ formatDate(selectedTaskDetail.actualEndTime) }}
          </Descriptions.Item>
          <Descriptions.Item label="所属排期">
            {{ formatSprintDisplayName(selectedTaskDetail.sprintName) || '-' }}
          </Descriptions.Item>
          <Descriptions.Item label="所属 WBS">
            {{ resolveTaskWbsLabel(selectedTaskDetail) }}
          </Descriptions.Item>
          <Descriptions.Item
            label="需求说明"
            :span="2"
          >
            <div class="project-detail__demand-description-cell">
              {{ selectedTaskDetail.description || '-' }}
            </div>
          </Descriptions.Item>
          <Descriptions.Item
            v-if="selectedTaskDetail.requirementTitle"
            label="历史需求关联"
            :span="2"
          >
            {{ selectedTaskDetail.requirementTitle }}
          </Descriptions.Item>
          <Descriptions.Item v-if="selectedTaskDetail.isBlocked" label="阻塞原因" :span="2">
            {{ selectedTaskDetail.blockReason || '-' }}
          </Descriptions.Item>
        </Descriptions>

        <Divider orientation="left">记录进展</Divider>
        <Form class="project-detail__inline-form-card" layout="vertical">
          <Row :gutter="12">
            <Col :span="6">
              <Form.Item label="工作日期">
                <Input v-model:value="taskProgressForm.workDate" type="date" />
              </Form.Item>
            </Col>
            <Col :span="6">
              <Form.Item label="本次工时">
                <InputNumber
                  v-model:value="taskProgressForm.hours"
                  :max="24"
                  :min="0.1"
                  :precision="1"
                  style="width: 100%"
                />
              </Form.Item>
            </Col>
            <Col :span="6">
              <Form.Item label="需求状态">
                <Select
                  v-model:value="taskProgressForm.status"
                  :options="taskProgressStatusOptions"
                />
              </Form.Item>
            </Col>
            <Col :span="6">
              <Form.Item label="阻塞">
                <Checkbox v-model:checked="taskProgressForm.isBlocked"> 存在阻塞 </Checkbox>
              </Form.Item>
            </Col>
          </Row>
          <Form.Item
            v-if="taskProgressForm.isBlocked || taskProgressForm.status === '阻塞'"
            label="阻塞原因"
          >
            <Input
              v-model:value="taskProgressForm.blockReason"
              :maxlength="512"
              placeholder="说明当前阻塞点和需要谁处理"
            />
          </Form.Item>
          <Form.Item label="进展说明">
            <Input.TextArea
              v-model:value="taskProgressForm.summary"
              :auto-size="{ minRows: 2, maxRows: 4 }"
              :maxlength="256"
              placeholder="填写本次完成内容、待跟进事项或交付物"
            />
          </Form.Item>
          <Space>
            <Button :loading="savingTaskProgress" type="primary" @click="handleRecordTaskProgress">
              保存进展
            </Button>
          </Space>
        </Form>

        <Divider v-if="selectedTaskRequirement" orientation="left">历史需求关联</Divider>
        <section v-if="selectedTaskRequirement" class="project-detail__stack-item">
          <strong>
            {{ selectedTaskRequirement.requirementCode }} ·
            {{ selectedTaskRequirement.title }}
          </strong>
          <span>{{ selectedTaskRequirement.description || '-' }}</span>
          <div class="project-detail__tag-list">
            <Tag>{{ selectedTaskRequirement.status }}</Tag>
            <Tag>{{ selectedTaskRequirement.type }}</Tag>
            <Tag>{{ selectedTaskRequirement.source }}</Tag>
            <Tag v-if="selectedTaskRequirement.wbsNodeName">
              {{ selectedTaskRequirement.wbsNodeName }}
            </Tag>
          </div>
        </section>

        <Divider orientation="left">依赖关系</Divider>
        <Form class="project-detail__inline-form-card" layout="vertical">
          <Row :gutter="12">
            <Col :span="8">
              <Form.Item label="前置需求">
                <Select
                  v-model:value="taskDetailDependencyForm.predecessorTaskId"
                  allow-clear
                  :options="taskDetailDependencyOptions"
                  placeholder="选择前置需求"
                  show-search
                />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="后续需求">
                <Select
                  v-model:value="taskDetailDependencyForm.successorTaskId"
                  allow-clear
                  :options="taskDetailDependencyOptions"
                  placeholder="选择后续需求"
                  show-search
                />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="依赖类型">
                <Select
                  v-model:value="taskDetailDependencyForm.dependencyType"
                  :options="dependencyTypeOptions"
                />
              </Form.Item>
            </Col>
          </Row>
          <Space>
            <Button
              :disabled="!taskDetailDependencyForm.predecessorTaskId"
              :loading="savingTaskDependency"
              size="small"
              @click="handleCreateTaskDetailDependency(selectedTaskDetail.id, 'predecessor')"
            >
              添加前置
            </Button>
            <Button
              :disabled="!taskDetailDependencyForm.successorTaskId"
              :loading="savingTaskDependency"
              size="small"
              @click="handleCreateTaskDetailDependency(selectedTaskDetail.id, 'successor')"
            >
              添加后续
            </Button>
          </Space>
        </Form>
        <div v-if="selectedTaskDependencies.length > 0" class="project-detail__stack-list">
          <section
            v-for="item in selectedTaskDependencies"
            :key="item.id"
            class="project-detail__stack-item"
          >
            <strong>{{ item.direction }} · {{ item.relatedTaskTitle }}</strong>
            <span>{{ resolveDependencyTypeLabel(item.dependencyType) }}</span>
          </section>
        </div>
        <div v-else class="project-detail__muted">暂无依赖关系</div>

        <Divider orientation="left">工时记录</Divider>
        <div v-if="selectedTaskWorklogs.length > 0" class="project-detail__stack-list">
          <section
            v-for="item in selectedTaskWorklogs"
            :key="item.id"
            class="project-detail__stack-item"
          >
            <strong>
              {{ formatDate(item.workDate) }} · {{ item.userName }} ·
              {{ formatHours(item.hours) }}
            </strong>
            <span>{{ item.summary }}</span>
          </section>
        </div>
        <div v-else class="project-detail__muted">暂无工时记录</div>
      </div>
    </Drawer>
  </section>
</template>
