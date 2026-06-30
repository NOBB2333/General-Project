<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import {
  Alert,
  Button,
  Card,
  Col,
  Collapse,
  Divider,
  Drawer,
  Empty,
  Form,
  Input,
  Popconfirm,
  Row,
  Select,
  Space,
  Tabs,
  Tag,
} from 'ant-design-vue';

import { computed, ref, watch } from 'vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';
import ProjectWorkflowCanvas from './ProjectWorkflowCanvas.vue';

const NEW_ISSUE_HIGHLIGHT_SECONDS = 3;

const {
  acceptanceCriteriaOptions,
  deletingIssueId,
  detail,
  editingIssueId,
  formatDate,
  getPriorityColor,
  handleCloseIssueForm,
  handleDeleteIssue,
  handleOpenIssueCreate,
  handleOpenIssueEdit,
  handleSaveIssue,
  issueColumns,
  issueForm,
  issueFormOpen,
  issueMetrics,
  loading,
  memberSelectOptions,
  projectIssueLevelOptions,
  projectIssueStatusOptions,
  projectIssueTypeOptions,
  qualityChecklistOptions,
  savingIssue,
  workflowPreviewGroups,
} = useProjectDetailContext();

const activeWorkflowKey = ref(workflowPreviewGroups.value[0]?.key || 'defect-flow');
const highlightedIssueId = ref<string | null>(null);

watch(
  () => detail.value?.id,
  () => {
    activeWorkflowKey.value = workflowPreviewGroups.value[0]?.key || 'defect-flow';
    highlightedIssueId.value = null;
  }
);

watch(
  () => workflowPreviewGroups.value.map((item) => item.key).join('|'),
  () => {
    if (!workflowPreviewGroups.value.some((item) => item.key === activeWorkflowKey.value)) {
      activeWorkflowKey.value = workflowPreviewGroups.value[0]?.key || 'defect-flow';
    }
  }
);

const recentIssueIds = computed(() => {
  const items = detail.value?.issues || [];
  const threshold = new Date(Date.now() - NEW_ISSUE_HIGHLIGHT_SECONDS * 1000);
  return new Set(
    items
      .filter((item) => item.creationTime && new Date(item.creationTime) >= threshold)
      .map((item) => item.id)
  );
});
const issueGridOptions = computed<VxeGridProps<ProjectApi.IssueItem>>(() => ({
  columns: issueColumns,
  data: detail.value?.issues || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowClassName: ({ row }) => rowClassName(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const [IssueGrid] = useVbenVxeGrid<ProjectApi.IssueItem>({ gridOptions: {} });

function readIssueField(record: ProjectApi.IssueItem, key: string) {
  return record[key as keyof ProjectApi.IssueItem] || '-';
}

function resolveIssueStatusColor(status?: null | string) {
  if (!status) {
    return 'default';
  }

  if (status === '待处理') {
    return 'warning';
  }

  if (status === '处理中') {
    return 'warning';
  }

  if (status === '已解决') {
    return 'processing';
  }

  if (status === '已关闭' || status === '已取消') {
    return 'default';
  }

  return 'default';
}

function handleCreateIssue() {
  handleOpenIssueCreate();
  highlightedIssueId.value = null;
}

function isNewIssue(record: ProjectApi.IssueItem) {
  return highlightedIssueId.value === record.id || recentIssueIds.value.has(record.id);
}

async function handleSaveIssueAndHighlight() {
  const wasEditing = Boolean(editingIssueId.value);
  const savedIssueId = await handleSaveIssue();
  if (savedIssueId && !wasEditing) {
    highlightedIssueId.value = savedIssueId;
    window.setTimeout(() => {
      if (highlightedIssueId.value === savedIssueId) {
        highlightedIssueId.value = null;
      }
    }, NEW_ISSUE_HIGHLIGHT_SECONDS * 1000);
  }
}

function rowClassName(record: ProjectApi.IssueItem) {
  if (isNewIssue(record)) {
    return 'project-detail__issue-row--new';
  }

  return '';
}
</script>

<template>
  <section class="project-detail__section">
    <Alert
      class="project-detail__alert"
      message="缺陷 / 问题用于记录 Bug 反馈、定位、修复和验证；工作流视图仅保留轻量预览，真实流程接入后再深化。"
      show-icon
      type="info"
    />
    <div class="project-detail__overview-list project-detail__overview-list--metrics">
      <div v-for="metric in issueMetrics" :key="metric.label" class="project-detail__overview-item">
        <span>{{ metric.label }}</span>
        <strong>{{ metric.value }}</strong>
      </div>
    </div>
    <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="缺陷 / 问题统一入口">
      <template #extra>
        <Button type="primary" @click="handleCreateIssue"> 新建问题 </Button>
      </template>
      <IssueGrid
        class="app-vxe-card-grid"
        grid-class="project-detail__issue-grid"
        :grid-options="issueGridOptions"
      >
        <template #title="{ row }">
          <span class="project-detail__issue-title">
            {{ row.title }}
            <Tag
              v-if="isNewIssue(row)"
              class="project-detail__issue-new-tag"
              color="red"
            >
              New
            </Tag>
          </span>
        </template>
        <template #dueDate="{ row }">
          {{ formatDate(row.dueDate) }}
        </template>
        <template #requirementTitle="{ row }">
          {{ readIssueField(row, 'requirementTitle') }}
        </template>
        <template #productOwnerName="{ row }">
          {{ readIssueField(row, 'productOwnerName') }}
        </template>
        <template #developerOwnerName="{ row }">
          {{ readIssueField(row, 'developerOwnerName') }}
        </template>
        <template #testerOwnerName="{ row }">
          {{ readIssueField(row, 'testerOwnerName') }}
        </template>
        <template #level="{ row }">
          <Tag :color="getPriorityColor(row.level)">
            {{ row.level }}
          </Tag>
        </template>
        <template #status="{ row }">
          <Tag :color="resolveIssueStatusColor(row.status)">
            {{ row.status }}
          </Tag>
        </template>
        <template #actions="{ row }">
          <Space size="small">
            <Button
              size="small"
              type="link"
              @click="handleOpenIssueEdit(row)"
            >
              编辑
            </Button>
            <Popconfirm
              title="确认删除这个问题记录？"
              @confirm="handleDeleteIssue(row.id)"
            >
              <Button
                danger
                :loading="deletingIssueId === row.id"
                size="small"
                type="link"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </IssueGrid>
    </Card>

    <Drawer
      :open="issueFormOpen"
      :title="editingIssueId ? '编辑问题' : '新建问题'"
      destroy-on-close
      width="760px"
      @close="handleCloseIssueForm"
    >
      <Form layout="vertical">
        <Row :gutter="[12, 0]">
          <Col :span="16">
            <Form.Item label="问题标题" required>
              <Input v-model:value="issueForm.title" :maxlength="256" />
            </Form.Item>
          </Col>
          <Col :span="8">
            <Form.Item label="负责人" required>
              <Select
                v-model:value="issueForm.ownerUserId"
                :options="memberSelectOptions"
                show-search
              />
            </Form.Item>
          </Col>
          <Col :span="6">
            <Form.Item label="类型">
              <Select v-model:value="issueForm.type" :options="projectIssueTypeOptions" />
            </Form.Item>
          </Col>
          <Col :span="6">
            <Form.Item label="等级">
              <Select v-model:value="issueForm.level" :options="projectIssueLevelOptions" />
            </Form.Item>
          </Col>
          <Col :span="6">
            <Form.Item label="状态">
              <Select v-model:value="issueForm.status" :options="projectIssueStatusOptions" />
            </Form.Item>
          </Col>
          <Col :span="6">
            <Form.Item label="到期日">
              <Input v-model:value="issueForm.dueDate" type="date" />
            </Form.Item>
          </Col>
        </Row>

        <Collapse :bordered="false" class="project-detail__form-collapse" ghost>
          <Collapse.Panel key="responsibility" header="责任矩阵（可选）">
            <Row :gutter="[12, 0]">
              <Col :span="8">
                <Form.Item label="产品负责人">
                  <Select
                    v-model:value="issueForm.productOwnerUserId"
                    allow-clear
                    :options="memberSelectOptions"
                    show-search
                  />
                </Form.Item>
              </Col>
              <Col :span="8">
                <Form.Item label="开发负责人">
                  <Select
                    v-model:value="issueForm.developerOwnerUserId"
                    allow-clear
                    :options="memberSelectOptions"
                    show-search
                  />
                </Form.Item>
              </Col>
              <Col :span="8">
                <Form.Item label="测试负责人">
                  <Select
                    v-model:value="issueForm.testerOwnerUserId"
                    allow-clear
                    :options="memberSelectOptions"
                    show-search
                  />
                </Form.Item>
              </Col>
            </Row>
          </Collapse.Panel>
        </Collapse>

        <Divider orientation="left" plain>
          <span class="project-detail__form-section-title">关联信息</span>
        </Divider>
        <Row :gutter="[12, 0]">
          <Col :span="12">
            <Form.Item label="验收标准">
              <Select
                v-model:value="issueForm.acceptanceCriteriaId"
                allow-clear
                :options="acceptanceCriteriaOptions"
                show-search
              />
            </Form.Item>
          </Col>
          <Col :span="12">
            <Form.Item label="质量检查单">
              <Select
                v-model:value="issueForm.qualityChecklistId"
                allow-clear
                :options="qualityChecklistOptions"
                show-search
              />
            </Form.Item>
          </Col>
          <Col :span="24">
            <Form.Item label="需求 / 背景摘要">
              <Input.TextArea
                v-model:value="issueForm.requirementTitle"
                :auto-size="{ minRows: 2, maxRows: 4 }"
                :maxlength="256"
              />
            </Form.Item>
          </Col>
        </Row>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="handleCloseIssueForm">取消</Button>
          <Button :loading="savingIssue" type="primary" @click="handleSaveIssueAndHighlight">保存</Button>
        </div>
      </template>
    </Drawer>

    <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="项目工作流视图">
      <div class="project-detail__section-head">
        <span>仅做缺陷处理流程预览和节点拖拽排版，真实 Workflow 后续再接入。</span>
        <Tag color="processing">前端画布</Tag>
      </div>
      <Tabs
        v-if="workflowPreviewGroups.length > 0"
        v-model:active-key="activeWorkflowKey"
        class="project-detail__inner-tabs"
      >
        <Tabs.TabPane
          v-for="workflow in workflowPreviewGroups"
          :key="workflow.key"
          :tab="workflow.title"
        >
          <ProjectWorkflowCanvas :project-id="detail?.id" :workflows="[workflow]" />
        </Tabs.TabPane>
      </Tabs>
      <Empty v-else description="暂无可视化流程" />
    </Card>
  </section>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-detail__issue-title {
  position: relative;
  display: inline-flex;
  align-items: center;
  @apply gap-2;
}

.project-detail__issue-new-tag {
  @apply rounded-full;
  font-size: 11px;
  line-height: 1;
}

.project-detail__form-collapse {
  @apply mb-3 mt-0.5;
  background: transparent;
}

:deep(.project-detail__issue-row--new) td {
  animation: issue-row-highlight 3s ease-out;
}

@keyframes issue-row-highlight {
  0% {
    background-color: rgba(255, 77, 79, 0.12);
  }

  100% {
    background-color: transparent;
  }
}

.project-detail__form-section-title {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}
</style>
