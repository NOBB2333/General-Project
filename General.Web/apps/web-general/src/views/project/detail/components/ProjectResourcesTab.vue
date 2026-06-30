<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import {
  Button,
  Card,
  Checkbox,
  Drawer,
  Empty,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

import { useProjectDetailContext } from '../context';

type ProjectMemberResourceRow = ProjectApi.MemberItem & {
  estimatedWorkHours: number;
  loadHint: string;
  taskCount: number;
  worklogHours: number;
};

const {
  detail,
  editingMemberId,
  filledWorklogEntryCount,
  formatDate,
  formatHours,
  handleApplyWorklogDateToRows,
  handleCloseWorklogDrawer,
  handleEditMemberRoles,
  handleFillEmptyWorklogSummaries,
  handleOpenWorklogDrawer,
  handleSaveFilledWorklogEntries,
  handleSaveMemberRoles,
  handleSaveWorklogEntry,
  loading,
  loadingMyWorklogTasks,
  memberColumns,
  memberResourceRows,
  memberRoleForms,
  projectRoleOptions,
  savingMemberRoleId,
  savingWorklogBatch,
  savingWorklogEntry,
  savingWorklogTaskId,
  taskProgressStatusOptions,
  worklogBatchDate,
  worklogBatchSummary,
  worklogColumns,
  worklogDemandRows,
  worklogDrawerSummary,
  worklogDrawerOpen,
  worklogEntryForms,
} = useProjectDetailContext();

const worklogRows = computed(() => detail.value?.worklogs || []);
const worklogPagination = useLocalVxePagination(worklogRows, { defaultPageSize: 20 });

const memberGridOptions = computed<VxeGridProps<ProjectMemberResourceRow>>(() => ({
  columns: memberColumns,
  data: memberResourceRows.value,
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));
const worklogGridOptions = computed<VxeGridProps<ProjectApi.WorklogItem>>(() => ({
  columns: worklogColumns,
  data: worklogPagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: worklogPagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const worklogGridEvents: NativeVxeGridEvents<ProjectApi.WorklogItem> = {
  'page-change': worklogPagination.handlePageChange,
};

const [MemberGrid] = useVbenVxeGrid<ProjectMemberResourceRow>({ gridOptions: {} });
const [WorklogGrid] = useVbenVxeGrid<ProjectApi.WorklogItem>({ gridOptions: {} });

function getWorklogEntryForm(taskId: string) {
  return worklogEntryForms.value[taskId];
}

function formatSprintDisplayName(name?: null | string) {
  return (name || '')
    .replace(/第\s*(\d+)\s*迭代/g, '第 $1 排期')
    .replace(/Sprint/gi, '排期');
}
</script>

<template>
<section class="project-detail__section">
  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="项目成员">
    <div class="project-detail__section-head">
      <span>成员角色、关联需求和工时投入统一在一张表查看，投入提示只用于资源协调，不作为考核结论。</span>
    </div>
    <MemberGrid
      class="app-vxe-card-grid"
      grid-class="project-detail__member-grid"
      :grid-options="memberGridOptions"
    >
      <template #roleNames="{ row }">
        <template v-if="editingMemberId === row.id">
          <Select
            :value="memberRoleForms[row.id] || []"
            mode="tags"
            :options="projectRoleOptions"
            size="small"
            style="width: 100%"
            @update:value="(value) => { memberRoleForms[row.id] = value as string[]; }"
          />
        </template>
        <template v-else>
          <div class="project-detail__tag-list">
            <Tag v-for="role in row.roleNames" :key="role" color="blue">
              {{ role }}
            </Tag>
            <span v-if="!row.roleNames?.length" class="project-detail__muted">未设置</span>
          </div>
        </template>
      </template>
      <template #joinDate="{ row }">
        {{ formatDate(row.joinDate) }}
      </template>
      <template #taskCount="{ row }">
        {{ row.taskCount || 0 }}
      </template>
      <template #estimatedWorkHours="{ row }">
        {{ row.estimatedWorkHours || 0 }} 小时
      </template>
      <template #worklogHours="{ row }">
        {{ row.worklogHours || 0 }} 小时
      </template>
      <template #loadHint="{ row }">
        <Tag :color="row.loadHint === '投入较集中' ? 'gold' : row.loadHint === '暂无记录' ? 'default' : 'green'">
          {{ row.loadHint }}
        </Tag>
      </template>
      <template #actions="{ row }">
        <Space>
          <template v-if="editingMemberId === row.id">
            <Button size="small" type="primary" :loading="savingMemberRoleId === row.id" @click="handleSaveMemberRoles(row.id)">保存</Button>
            <Button size="small" @click="editingMemberId = null">取消</Button>
          </template>
          <template v-else>
            <Button size="small" type="link" @click="handleEditMemberRoles(row)">编辑角色</Button>
          </template>
        </Space>
      </template>
    </MemberGrid>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="工时记录（小时）">
    <template #extra>
      <Button type="primary" @click="handleOpenWorklogDrawer">填写工时</Button>
    </template>
    <WorklogGrid
      class="app-vxe-card-grid"
      grid-class="project-detail__worklog-grid"
      :grid-events="worklogGridEvents"
      :grid-options="worklogGridOptions"
    >
      <template #workDate="{ row }">
        {{ formatDate(row.workDate) }}
      </template>
    </WorklogGrid>
  </Card>

  <Drawer
    :open="worklogDrawerOpen"
    title="填写工时"
    width="min(1400px, 92vw)"
    @close="handleCloseWorklogDrawer"
  >
    <div class="project-detail__worklog-drawer">
      <div class="project-detail__worklog-quickbar">
        <div class="project-detail__worklog-quickbar-metrics">
          <span>可填需求 <strong>{{ worklogDrawerSummary.taskCount }}</strong></span>
          <span>已填写 <strong>{{ filledWorklogEntryCount }}</strong></span>
          <span>预计 <strong>{{ formatHours(worklogDrawerSummary.estimatedHours) }}</strong></span>
          <span>已填报 <strong>{{ formatHours(worklogDrawerSummary.actualHours) }}</strong></span>
        </div>
        <div class="project-detail__worklog-quickbar-actions">
          <Input
            v-model:value="worklogBatchDate"
            class="project-detail__worklog-date-input"
            type="date"
          />
          <Button @click="handleApplyWorklogDateToRows">应用日期</Button>
          <Input
            v-model:value="worklogBatchSummary"
            allow-clear
            class="project-detail__worklog-summary-input"
            placeholder="通用工作进展，可填入空白行"
          />
          <Button @click="handleFillEmptyWorklogSummaries">填入空白进展</Button>
          <Button
            :disabled="filledWorklogEntryCount === 0"
            :loading="savingWorklogBatch"
            type="primary"
            @click="handleSaveFilledWorklogEntries"
          >
            保存已填写行
          </Button>
        </div>
      </div>
      <Empty v-if="!loadingMyWorklogTasks && worklogDemandRows.length === 0" description="暂无可填写工时的需求" />
      <div v-else class="project-detail__worklog-entry-list">
        <section
          v-for="task in worklogDemandRows"
          :key="task.id"
          class="project-detail__worklog-entry-row"
        >
          <div class="project-detail__worklog-demand-info">
            <strong>{{ task.taskCode }} · {{ task.title }}</strong>
            <span>{{ task.projectName || '当前项目' }} / {{ formatSprintDisplayName(task.sprintName) || '未进排期' }}</span>
            <small>
              预计 {{ formatHours(task.estimatedWorkHours) }} / 已填报 {{ formatHours(task.actualWorkHours) }}
            </small>
          </div>
          <Form
            v-if="getWorklogEntryForm(task.id)"
            class="project-detail__worklog-row-form"
            layout="vertical"
          >
            <div class="project-detail__worklog-row-grid">
              <Form.Item label="日期">
                <Input v-model:value="getWorklogEntryForm(task.id)!.workDate" type="date" />
              </Form.Item>
              <Form.Item label="工时">
                <InputNumber
                  v-model:value="getWorklogEntryForm(task.id)!.hours"
                  :max="24"
                  :min="0.1"
                  :precision="1"
                  style="width: 100%"
                />
              </Form.Item>
              <Form.Item label="状态">
                <Select
                  v-model:value="getWorklogEntryForm(task.id)!.status"
                  :options="taskProgressStatusOptions"
                />
              </Form.Item>
              <Form.Item label="阻塞">
                <Checkbox v-model:checked="getWorklogEntryForm(task.id)!.isBlocked">存在阻塞</Checkbox>
              </Form.Item>
            </div>
            <Input
              v-if="getWorklogEntryForm(task.id)!.isBlocked || getWorklogEntryForm(task.id)!.status === '阻塞'"
              v-model:value="getWorklogEntryForm(task.id)!.blockReason"
              :maxlength="512"
              class="project-detail__worklog-block-reason"
              placeholder="说明当前阻塞点和需要谁处理"
            />
            <Input.TextArea
              v-model:value="getWorklogEntryForm(task.id)!.summary"
              :maxlength="1000"
              class="project-detail__worklog-progress-textarea"
              placeholder="填写当天完成内容、待跟进事项或交付物"
              :rows="4"
            />
            <div class="project-detail__worklog-row-actions">
              <Button
                :loading="savingWorklogEntry && savingWorklogTaskId === task.id"
                type="primary"
                @click="handleSaveWorklogEntry(task.id)"
              >
                保存本需求
              </Button>
            </div>
          </Form>
        </section>
      </div>
    </div>
  </Drawer>
</section>
</template>
