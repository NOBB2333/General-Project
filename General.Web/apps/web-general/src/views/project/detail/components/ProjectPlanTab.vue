<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import { Alert, Button, Card, Input, Popconfirm, Select, Space, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';
import ProjectWbsPanel from './ProjectWbsPanel.vue';

type MilestonePlanRow = {
  __editing?: boolean;
  closeState: {
    acceptanceCriteria: ProjectApi.AcceptanceCriteriaItem[];
    acceptanceRecordCount: number;
    conditions: {
      count: number;
      done: boolean;
      key: string;
      label: string;
      targetTabKey: string;
    }[];
    sprintName: string;
  };
  form: ProjectApi.ProjectMilestoneStatusUpdateInput;
  id: string;
  item: ProjectApi.WorkspaceMilestoneItem;
};

type MilestoneTableRow = MilestonePlanRow | { __editing: true; id: string };

function formatAcceptanceCriteriaTitles(
  items: ProjectApi.AcceptanceCriteriaItem[],
  limit?: number,
) {
  const source = typeof limit === 'number' ? items.slice(0, limit) : items;
  return source.map((item) => item.title).join('、');
}

const {
  communicationItems,
  deletingMilestoneId,
  detail,
  editingMilestoneId,
  formatDate,
  handleCancelMilestoneEdit,
  handleCreateMilestoneDraft,
  handleDeleteMilestone,
  handleEditMilestone,
  handleMilestoneCloseAdviceAction,
  handleSaveMilestone,
  loading,
  memberSelectOptions,
  milestoneColumns,
  milestoneCloseAdvice,
  milestoneForm,
  milestoneRows,
  milestoneSelectOptions,
  refreshSelectedProjectDetail,
  sprintOptions,
  savingMilestone,
  selectedProjectId,
  switchProjectDetailTab,
  showMilestoneForm,
} = useProjectDetailContext();

const milestoneTableRows = computed<MilestoneTableRow[]>(() => {
  const rows = milestoneRows.value as MilestonePlanRow[];
  if (!showMilestoneForm.value) {
    return rows;
  }

  const editingRow = {
    __editing: true,
    id: editingMilestoneId.value || '__new_milestone__',
  } as const;

  if (editingMilestoneId.value) {
    return rows.map((row) => (row.item.id === editingMilestoneId.value ? editingRow : row));
  }

  return [editingRow, ...rows];
});

const milestoneGridOptions = computed<VxeGridProps<MilestoneTableRow>>(() => ({
  columns: milestoneColumns,
  data: milestoneTableRows.value,
  emptyText: '暂无里程碑',
  expandConfig: {
    visibleMethod: ({ row }) => !isMilestoneEditingRow(row as MilestoneTableRow),
  },
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
}));

const [MilestoneGrid] = useVbenVxeGrid<MilestoneTableRow>({ gridOptions: {} });

function isMilestoneEditingRow(record: MilestoneTableRow) {
  return Boolean(record.__editing);
}

function asMilestonePlanRow(record: MilestoneTableRow) {
  return record as MilestonePlanRow;
}

function resolveMilestoneStatusColor(status?: null | string) {
  if (status === '已关闭') {
    return 'success';
  }

  if (status === '已完成') {
    return 'processing';
  }

  return 'default';
}
</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__plan-stack">
    <Card :bordered="false" class="project-detail__inner-card project-detail__plan-card--milestones rounded-2xl" title="里程碑">
      <div class="project-detail__muted project-detail__card-note">
        里程碑用于管理关键节点；每个里程碑可展开维护对应 WBS 工作包，需求与排期在「需求与排期」里维护。
      </div>
      <Alert
        v-if="milestoneCloseAdvice"
        class="project-detail__alert"
        show-icon
        type="warning"
      >
        <template #message>
          {{ milestoneCloseAdvice.milestoneName }} 暂不能关闭
        </template>
        <template #description>
          <div class="project-detail__alert-stack">
            <span>{{ milestoneCloseAdvice.message }}</span>
            <span>
              待补齐：
              {{ milestoneCloseAdvice.missingItems.join('、') }}
            </span>
            <Space wrap>
              <Button
                v-for="action in milestoneCloseAdvice.actions"
                :key="action.tabKey"
                size="small"
                type="link"
                @click="handleMilestoneCloseAdviceAction(action.tabKey)"
              >
                {{ action.label }}
              </Button>
            </Space>
          </div>
        </template>
      </Alert>
      <div class="project-detail__section-head">
        <Button type="primary" @click="handleCreateMilestoneDraft">新增里程碑</Button>
      </div>
      <MilestoneGrid
        class="app-vxe-card-grid"
        grid-class="project-detail__milestone-grid"
        :grid-options="milestoneGridOptions"
      >
        <template #name="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Input
              v-model:value="milestoneForm.name"
              :maxlength="128"
              placeholder="需求确认 / UAT 完成 / 正式上线"
            />
          </template>
          <template v-else>
            {{ asMilestonePlanRow(row).item.name }}
          </template>
        </template>

        <template #ownerName="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Select
              v-model:value="milestoneForm.ownerUserId"
              :options="memberSelectOptions"
              show-search
              style="width: 120px"
            />
          </template>
          <template v-else>
            {{ asMilestonePlanRow(row).item.ownerName }}
          </template>
        </template>

        <template #sprintName="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Select
              v-model:value="milestoneForm.sprintId"
              allow-clear
              :options="sprintOptions"
              placeholder="所属排期"
              show-search
              style="width: 160px"
            />
          </template>
          <template v-else>
            {{ asMilestonePlanRow(row).closeState.sprintName }}
          </template>
        </template>

        <template #plannedCompletionDate="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Input v-model:value="milestoneForm.plannedCompletionDate" type="date" />
          </template>
          <template v-else>
            {{ formatDate(asMilestonePlanRow(row).item.plannedCompletionDate) }}
          </template>
        </template>

        <template #status="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Select
              v-model:value="milestoneForm.status"
              :options="[
                { label: '未开始', value: '未开始' },
                { label: '进行中', value: '进行中' },
                { label: '已完成', value: '已完成' },
                { label: '已关闭', value: '已关闭' },
              ]"
              style="width: 104px"
            />
          </template>
          <Tag v-else :color="resolveMilestoneStatusColor(asMilestonePlanRow(row).item.status)">
            {{ asMilestonePlanRow(row).item.status || '-' }}
          </Tag>
        </template>

        <template #actualCompletionDate="{ row }">
          <template v-if="isMilestoneEditingRow(row)">
            <Input v-model:value="milestoneForm.actualCompletionDate" type="date" />
          </template>
          <template v-else>
            {{ formatDate(asMilestonePlanRow(row).item.actualCompletionDate) }}
          </template>
        </template>

        <template #closeConditions="{ row }">
          <span v-if="isMilestoneEditingRow(row)" class="project-detail__muted">-</span>
          <template v-else>
            <div class="project-detail__tag-list">
              <Tag
                v-for="condition in asMilestonePlanRow(row).closeState.conditions"
                :key="condition.key"
                :color="condition.done ? 'success' : 'warning'"
              >
                {{ condition.label }} {{ condition.count }}
              </Tag>
              <Button
                size="small"
                type="link"
                @click="switchProjectDetailTab('delivery')"
              >
                验收与交付
              </Button>
              <Button
                size="small"
                type="link"
                @click="switchProjectDetailTab('docs')"
              >
                文件
              </Button>
            </div>
            <div
              v-if="asMilestonePlanRow(row).closeState.acceptanceCriteria.length > 0"
              class="project-detail__muted"
            >
              {{ formatAcceptanceCriteriaTitles(asMilestonePlanRow(row).closeState.acceptanceCriteria, 2) }}
            </div>
          </template>
        </template>

        <template #actions="{ row }">
          <Space v-if="isMilestoneEditingRow(row)">
            <Button :loading="savingMilestone" size="small" type="primary" @click="handleSaveMilestone">
              保存
            </Button>
            <Button size="small" @click="handleCancelMilestoneEdit">取消</Button>
          </Space>
          <Space v-else>
            <Button size="small" type="link" @click="handleEditMilestone(asMilestonePlanRow(row).item)">
              编辑
            </Button>
            <Popconfirm title="确认删除这个里程碑？" @confirm="handleDeleteMilestone(asMilestonePlanRow(row).item.id)">
              <Button
                danger
                :loading="deletingMilestoneId === asMilestonePlanRow(row).item.id"
                size="small"
                type="link"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>

        <template #wbs="{ row }">
          <ProjectWbsPanel
            v-if="!isMilestoneEditingRow(row)"
            embedded
            :sprint-options="sprintOptions"
            :member-options="memberSelectOptions"
            :milestone-id="asMilestonePlanRow(row).item.id"
            :milestone-name="asMilestonePlanRow(row).item.name"
            :milestone-options="milestoneSelectOptions"
            :project-id="selectedProjectId"
            :tasks="detail?.tasks || []"
            :wbs-nodes="detail?.wbsNodes || []"
            @refreshed="refreshSelectedProjectDetail"
          />
        </template>
      </MilestoneGrid>
    </Card>
  </div>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="近期项目报告 / 会议记录">
    <div class="project-detail__section-head">
      <span>计划页只展示近期摘要，完整日报、周报、月报和会议记录统一在「文档与知识」维护。</span>
      <Button size="small" type="link" @click="switchProjectDetailTab('docs')">查看全部</Button>
    </div>
    <div v-if="communicationItems.length === 0" class="project-detail__muted">
      当前未归档项目报告或会议记录。
    </div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in communicationItems.slice(0, 3)" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag>{{ item.type }}</Tag>
          <Tag>{{ item.status }}</Tag>
        </div>
        <strong>{{ item.title }}</strong>
        <span>{{ item.ownerName }} / {{ item.periodText }}</span>
      </article>
    </div>
  </Card>
</section>
</template>
