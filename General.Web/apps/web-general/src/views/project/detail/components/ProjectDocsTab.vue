<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import {
  Button,
  Card,
  Col,
  Form,
  Input,
  Row,
  Select,
  Space,
  Tag,
  Textarea,
  Upload,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { FileApi } from '#/api/core';
import { downloadPlatformFile } from '#/components/file/download-file';

import type { ProjectCommunicationItem } from '../composables/useProjectCommunication';
import { useProjectDetailContext } from '../context';

const router = useRouter();

const {
  communicationItems,
  communicationMetrics,
  sprintOptions,
  demandPriorityOptions,
  formatDate,
  handleAddMeetingActionItem,
  handleCancelMeetingDraft,
  handleCancelReportDraft,
  handleCreateMeetingDraft,
  handleCreateMeetingRecord,
  handleCreateReport,
  handleCreateReportDraft,
  handleGenerateReportDraft,
  handleRemoveMeetingActionItem,
  loading,
  loadingProjectFiles,
  meetingActionStatusOptions,
  meetingActionTypeOptions,
  meetingAttachmentOptions,
  meetingRecordForm,
  meetingStatusOptions,
  memberSelectOptions,
  activeRiskOptions,
  issueSelectOptions,
  openCommunicationItem,
  openProjectFilePreview,
  projectFiles,
  reportAttachmentOptions,
  reportDraftLoading,
  reportForm,
  reportTypeOptions,
  savingMeetingRecord,
  savingReport,
  showMeetingRecordForm,
  showReportForm,
  taskSelectOptions,
  taskPriorityOptions,
  uploadMeetingFile,
  uploadProjectFile,
  uploadReportFile,
} = useProjectDetailContext();

type ProjectFileGroupKey =
  | 'all'
  | 'acceptance'
  | 'business'
  | 'charter'
  | 'general'
  | 'plan'
  | 'report'
  | 'requirement';

type ProjectFileGroup = {
  count: number;
  description: string;
  key: ProjectFileGroupKey;
  title: string;
};

const projectFileColumns = [
  { align: 'left', field: 'fileName', slots: { default: 'fileName' }, title: '资料名称', width: 320 },
  { field: 'category', slots: { default: 'category' }, title: '分类', width: 150 },
  { field: 'versionMarker', slots: { default: 'versionMarker' }, title: '版本', width: 90 },
  { align: 'left', field: 'business', slots: { default: 'business' }, title: '来源对象', width: 200 },
  { field: 'size', slots: { default: 'size' }, title: '大小', width: 110 },
  { field: 'uploadedBy', title: '上传人', width: 130 },
  { field: 'uploadedAt', slots: { default: 'uploadedAt' }, title: '上传时间', width: 150 },
  { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 150 },
] satisfies VxeGridProps<FileApi.FileItem>['columns'];

const communicationColumns = [
  { field: 'type', title: '类型', width: 110 },
  { align: 'left' as const, field: 'title', slots: { default: 'title' }, title: '标题', width: 260 },
  { field: 'periodText', slots: { default: 'periodText' }, title: '周期', width: 180 },
  { field: 'ownerName', title: '负责人', width: 120 },
  { field: 'attachmentFileName', slots: { default: 'attachmentFileName' }, title: '附件', width: 180 },
  { field: 'actionCount', slots: { default: 'actionCount' }, title: '行动项', width: 90 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
] satisfies VxeGridProps<ProjectCommunicationItem>['columns'];

const communicationTypeFilter = ref<string | undefined>();
const communicationStatusFilter = ref<string | undefined>();
const communicationOwnerFilter = ref<string | undefined>();
const communicationSprintFilter = ref<string | undefined>();
const projectFileGroupFilter = ref<ProjectFileGroupKey>();
const communicationTypeOptions = ['日报', '周报', '月报', '阶段报告', '项目总结', '会议记录'].map((value) => ({
  label: value,
  value,
}));
const communicationStatusOptions = computed(() =>
  Array.from(new Set(communicationItems.value.map((item) => item.status).filter(Boolean)))
    .map((value) => ({ label: value, value })),
);
const communicationOwnerOptions = computed(() =>
  memberSelectOptions.value.filter((item) =>
    communicationItems.value.some((record) => record.ownerUserId === item.value),
  ),
);
const communicationSprintOptions = computed(() =>
  sprintOptions.value.filter((item) =>
    communicationItems.value.some((record) => record.sprintId === item.value),
  ),
);

const projectFileGroups = computed<ProjectFileGroup[]>(() => {
  const groups: Array<Omit<ProjectFileGroup, 'count'>> = [
    {
      description: '当前项目关联的全部文件',
      key: 'all',
      title: '全部文件',
    },
    {
      description: '章程、立项材料和审批依据文件',
      key: 'charter',
      title: '章程与立项',
    },
    {
      description: '计划、范围、WBS、里程碑和排期资料',
      key: 'plan',
      title: '计划与范围',
    },
    {
      description: 'WBS 工作包、需求说明和实现过程资料',
      key: 'requirement',
      title: '需求资料',
    },
    {
      description: '验收标准、验收记录、交付物和签字文件',
      key: 'acceptance',
      title: '验收与交付',
    },
    {
      description: '日报、周报、月报、项目总结和会议附件',
      key: 'report',
      title: '报告与会议',
    },
    {
      description: '合同、采购、预算、回款、发票和经营附件',
      key: 'business',
      title: '经营附件',
    },
    {
      description: '未归入上述业务对象的项目资料',
      key: 'general',
      title: '其他资料',
    },
  ];

  return groups.map((group) => ({
    ...group,
    count: group.key === 'all'
      ? projectFiles.value.length
      : projectFiles.value.filter((file) => resolveProjectFileGroup(file) === group.key).length,
  }));
});

const filteredProjectFiles = computed(() => {
  if (!projectFileGroupFilter.value || projectFileGroupFilter.value === 'all') {
    return projectFiles.value;
  }

  return projectFiles.value.filter(
    (file) => resolveProjectFileGroup(file) === projectFileGroupFilter.value,
  );
});

const activeProjectFileGroup = computed(() =>
  projectFileGroups.value.find((item) => item.key === projectFileGroupFilter.value),
);

const filteredCommunicationItems = computed(() => {
  return communicationItems.value.filter(
    (item) =>
      (!communicationTypeFilter.value || item.type === communicationTypeFilter.value) &&
      (!communicationStatusFilter.value || item.status === communicationStatusFilter.value) &&
      (!communicationOwnerFilter.value || item.ownerUserId === communicationOwnerFilter.value) &&
      (!communicationSprintFilter.value || item.sprintId === communicationSprintFilter.value),
  );
});
const projectFileGridOptions = computed<VxeGridProps<FileApi.FileItem>>(() => ({
  columns: projectFileColumns,
  data: filteredProjectFiles.value,
  emptyText: '暂无项目资料',
  loading: loadingProjectFiles.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'fileKey' },
  scrollX: { enabled: true },
  size: 'medium',
}));
const communicationGridOptions = computed<VxeGridProps<ProjectCommunicationItem>>(() => ({
  columns: communicationColumns,
  data: filteredCommunicationItems.value,
  emptyText: '暂无项目报告或会议记录',
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const [ProjectFileGrid] = useVbenVxeGrid<FileApi.FileItem>({ gridOptions: {} });
const [CommunicationGrid] = useVbenVxeGrid<ProjectCommunicationItem>({ gridOptions: {} });

function formatSize(size: number) {
  if (size < 1024) {
    return `${size} B`;
  }

  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }

  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}

function resolveBusinessSource(file: FileApi.FileItem) {
  return [
    file.businessObjectTypeName,
    file.businessObjectName || file.businessProjectName,
    file.businessObjectCode,
  ].filter(Boolean).join(' / ') || '当前项目';
}

function resolveFileVersionMarker(file: FileApi.FileItem) {
  const groupKey = resolveVersionedFileGroupKey(file);
  if (!groupKey) {
    return null;
  }

  if (typeof file.isCurrentVersion === 'boolean') {
    return {
      color: file.isCurrentVersion ? 'success' : 'default',
      text: `${file.isCurrentVersion ? '当前' : '历史'} V${file.versionNo || 1}`,
    };
  }

  const currentFile = projectFiles.value
    .filter((item) => resolveVersionedFileGroupKey(item) === groupKey)
    .sort(
      (left, right) =>
        new Date(right.uploadedAt).getTime() -
        new Date(left.uploadedAt).getTime(),
    )[0];

  return currentFile?.fileKey === file.fileKey
    ? { color: 'success', text: '当前' }
    : { color: 'default', text: '历史' };
}

function resolveVersionedFileGroupKey(file: FileApi.FileItem) {
  const category = file.category || '';
  const parentPath = file.parentPath || '';
  if (category === 'Project/Charter' || parentPath === 'documents/charter') {
    return 'Project/Charter';
  }

  if (
    category === 'Project/AcceptanceCriteria' ||
    parentPath.startsWith('documents/acceptance/criteria')
  ) {
    return 'Project/AcceptanceCriteria';
  }

  if (category === 'Project/RequirementPrd' || parentPath.includes('/prd')) {
    return `Project/RequirementPrd:${file.businessType || ''}:${file.businessId || parentPath}`;
  }

  return '';
}

function openBusinessSource(file: FileApi.FileItem) {
  if (!file.businessJumpPath) {
    message.info('该文件暂未关联可跳转的来源对象');
    return;
  }

  router.push({
    path: file.businessJumpPath,
    query: Object.fromEntries(
      Object.entries(file.businessJumpQuery || {}).filter(([, value]) => value),
    ) as Record<string, string>,
  });
}

async function handleDownloadProjectFile(file: FileApi.FileItem) {
  try {
    await downloadPlatformFile(file);
  } catch (error) {
    const description = error instanceof Error ? error.message : '文件下载失败';
    message.error(description || '文件下载失败');
  }
}

function resolveProjectFileGroup(file: FileApi.FileItem): ProjectFileGroupKey {
  const category = file.category || '';
  const businessType = file.businessType || '';
  const businessTypeName = file.businessObjectTypeName || '';
  const parentPath = file.parentPath || '';
  const merged = `${category} ${businessType} ${businessTypeName} ${parentPath}`;

  if (merged.includes('Charter') || category === 'Project/Charter') {
    return 'charter';
  }

  if (
    merged.includes('Acceptance') ||
    merged.includes('Delivery') ||
    category.startsWith('Project/Acceptance') ||
    category === 'Project/Delivery'
  ) {
    return 'acceptance';
  }

  if (
    merged.includes('Report') ||
    merged.includes('Meeting') ||
    category === 'Project/Report' ||
    category === 'Project/Meeting'
  ) {
    return 'report';
  }

  if (
    merged.includes('Requirement') ||
    merged.includes('Task') ||
    category === 'Project/Requirement'
  ) {
    return 'requirement';
  }

  if (
    merged.includes('Wbs') ||
    merged.includes('Milestone') ||
    category === 'Project/Plan' ||
    category === 'Project/Document'
  ) {
    return 'plan';
  }

  if (businessType.startsWith('Business') || category.startsWith('Business/')) {
    return 'business';
  }

  return 'general';
}

</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__knowledge-layout">
    <aside class="project-detail__knowledge-sidebar">
      <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="项目资料">
        <div class="project-detail__file-panel">
          <Upload :custom-request="uploadProjectFile" :show-upload-list="false">
            <Button block type="primary">上传项目资料</Button>
          </Upload>
          <p>项目级附件会关联到当前项目，上传后可直接在线预览；阶段、里程碑级资料后续按业务对象继续扩展。</p>
          <div class="project-detail__file-summary">
            <span>关联资料</span>
            <strong>{{ projectFiles.length }}</strong>
          </div>
        </div>
      </Card>
      <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="文件分组">
        <div class="project-detail__knowledge-groups">
          <article
            v-for="group in projectFileGroups"
            :key="group.key"
            class="project-detail__knowledge-group project-detail__knowledge-group--clickable"
            :class="{ 'is-active': projectFileGroupFilter === group.key || (!projectFileGroupFilter && group.key === 'all') }"
            @click="projectFileGroupFilter = group.key === 'all' ? undefined : projectFileGroupFilter === group.key ? undefined : group.key"
          >
            <div>
              <strong>{{ group.title }}</strong>
              <p>{{ group.description }}</p>
            </div>
            <span>{{ group.count }} 个</span>
          </article>
        </div>
      </Card>
    </aside>
    <div class="project-detail__knowledge-main">
      <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="文件资料">
        <div class="project-detail__section-head">
          <span>
            {{ activeProjectFileGroup ? `${activeProjectFileGroup.title} · ${activeProjectFileGroup.description}` : '全部项目关联文件，按来源对象和分类统一查看。' }}
          </span>
          <Space wrap>
            <Tag v-if="activeProjectFileGroup" color="blue">
              {{ activeProjectFileGroup.count }} 个文件
            </Tag>
            <Button v-if="projectFileGroupFilter" size="small" @click="projectFileGroupFilter = undefined">
              查看全部
            </Button>
          </Space>
        </div>
        <ProjectFileGrid
          class="app-vxe-card-grid"
          grid-class="project-detail__project-file-grid"
          :grid-options="projectFileGridOptions"
        >
          <template #fileName="{ row }">
            <Button
              class="project-detail__file-name-button"
              type="link"
              @click="openProjectFilePreview(row)"
            >
              {{ row.fileName }}
            </Button>
            <div class="project-detail__file-type">{{ row.contentType || '-' }}</div>
          </template>
          <template #category="{ row }">
            <Tag color="blue">{{ row.category || '项目资料' }}</Tag>
          </template>
          <template #versionMarker="{ row }">
            <template v-if="resolveFileVersionMarker(row)">
              <Tag :color="resolveFileVersionMarker(row)?.color">
                {{ resolveFileVersionMarker(row)?.text }}
              </Tag>
            </template>
            <span v-else class="project-detail__muted">-</span>
          </template>
          <template #business="{ row }">
            {{ resolveBusinessSource(row) }}
          </template>
          <template #size="{ row }">
            {{ formatSize(row.size) }}
          </template>
          <template #uploadedAt="{ row }">
            {{ formatDate(row.uploadedAt) }}
          </template>
          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openProjectFilePreview(row)">预览</Button>
              <Button size="small" type="link" @click="handleDownloadProjectFile(row)">下载</Button>
              <Button
                v-if="row.businessJumpPath"
                size="small"
                type="link"
                @click="openBusinessSource(row)"
              >
                来源
              </Button>
            </Space>
          </template>
        </ProjectFileGrid>
      </Card>
      <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="项目报告 / 会议记录">
        <div class="project-detail__section-head project-detail__communication-head">
          <span>日报、周报、月报、阶段报告、项目总结和会议记录统一归档在这里，不拆多个入口。</span>
          <Space wrap>
            <Button class="project-detail__primary-action" type="primary" @click="handleCreateReportDraft">新建报告</Button>
            <Button class="project-detail__primary-action" @click="handleCreateMeetingDraft">新建会议记录</Button>
          </Space>
        </div>
        <div class="project-detail__metric-strip project-detail__metric-strip--communication">
          <div v-for="metric in communicationMetrics" :key="metric.label" class="project-detail__overview-item">
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
          </div>
        </div>
        <div class="project-detail__filter-bar project-detail__filter-bar--communication">
          <Space wrap>
            <Select
              v-model:value="communicationTypeFilter"
              allow-clear
              placeholder="筛选类型"
              style="width: 140px"
              :options="communicationTypeOptions"
            />
            <Select
              v-model:value="communicationStatusFilter"
              allow-clear
              placeholder="筛选状态"
              style="width: 130px"
              :options="communicationStatusOptions"
            />
            <Select
              v-model:value="communicationOwnerFilter"
              allow-clear
              placeholder="筛选负责人"
              show-search
              style="width: 150px"
              :options="communicationOwnerOptions"
            />
            <Select
              v-model:value="communicationSprintFilter"
              allow-clear
              placeholder="筛选排期"
              show-search
              style="width: 150px"
              :options="communicationSprintOptions"
            />
          </Space>
        </div>
        <Form
          v-if="showReportForm"
          class="project-detail__inline-form project-detail__inline-form-card project-detail__communication-form"
          layout="vertical"
        >
          <Row :gutter="[12, 0]">
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="报告类型">
                <Select v-model:value="reportForm.reportType" :options="reportTypeOptions" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="报告人">
                <Select v-model:value="reportForm.authorUserId" :options="memberSelectOptions" show-search />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="关联排期">
                <Select v-model:value="reportForm.sprintId" allow-clear :options="sprintOptions" show-search />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="附件">
                <Select v-model:value="reportForm.attachmentFileId" allow-clear :options="reportAttachmentOptions" show-search />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="开始日期">
                <Input v-model:value="reportForm.periodStart" type="date" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="结束日期">
                <Input v-model:value="reportForm.periodEnd" type="date" />
              </Form.Item>
            </Col>
            <Col :lg="18" :md="12" :xs="24">
              <Form.Item label="标题">
                <Input v-model:value="reportForm.title" :maxlength="128" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24" class="project-detail__form-upload-action">
              <Upload :custom-request="uploadReportFile" :show-upload-list="false">
                <Button block>上传报告附件</Button>
              </Upload>
            </Col>
            <Col :span="12">
              <Form.Item label="本期完成">
                <Textarea v-model:value="reportForm.completedSummary" :auto-size="{ minRows: 3, maxRows: 6 }" />
              </Form.Item>
            </Col>
            <Col :span="12">
              <Form.Item label="下期计划">
                <Textarea v-model:value="reportForm.nextPlan" :auto-size="{ minRows: 3, maxRows: 6 }" />
              </Form.Item>
            </Col>
            <Col :span="12">
              <Form.Item label="风险 / 问题">
                <Textarea v-model:value="reportForm.riskIssueSummary" :auto-size="{ minRows: 3, maxRows: 6 }" />
              </Form.Item>
            </Col>
            <Col :span="12">
              <Form.Item label="里程碑 / 工时">
                <Textarea
                  :value="[reportForm.milestoneSummary, reportForm.hoursSummary].filter(Boolean).join('\n')"
                  :auto-size="{ minRows: 3, maxRows: 6 }"
                  @update:value="(value) => { reportForm.milestoneSummary = value; }"
                />
              </Form.Item>
            </Col>
            <Col :span="24">
              <Form.Item label="经营摘要">
                <Textarea v-model:value="reportForm.businessSummary" :auto-size="{ minRows: 2, maxRows: 5 }" />
              </Form.Item>
            </Col>
            <Col :span="24">
              <Space wrap>
                <Button :loading="reportDraftLoading" @click="handleGenerateReportDraft">按项目数据生成草稿</Button>
                <Button :loading="savingReport" type="primary" @click="handleCreateReport">保存报告</Button>
                <Button @click="handleCancelReportDraft">取消</Button>
              </Space>
            </Col>
          </Row>
        </Form>
        <Form
          v-else-if="showMeetingRecordForm"
          class="project-detail__inline-form project-detail__inline-form-card project-detail__communication-form"
          layout="vertical"
        >
          <Row :gutter="[12, 0]">
            <Col :lg="12" :md="12" :xs="24">
              <Form.Item label="会议主题">
                <Input v-model:value="meetingRecordForm.topic" :maxlength="128" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="会议时间">
                <Input v-model:value="meetingRecordForm.meetingTime" type="datetime-local" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="状态">
                <Select v-model:value="meetingRecordForm.status" :options="meetingStatusOptions" />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="主持人">
                <Select v-model:value="meetingRecordForm.facilitatorUserId" :options="memberSelectOptions" show-search />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="参会人">
                <Select
                  v-model:value="meetingRecordForm.participantUserIds"
                  mode="multiple"
                  :options="memberSelectOptions"
                  show-search
                />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24">
              <Form.Item label="附件">
                <Select v-model:value="meetingRecordForm.attachmentFileId" allow-clear :options="meetingAttachmentOptions" show-search />
              </Form.Item>
            </Col>
            <Col :lg="6" :md="12" :xs="24" class="project-detail__form-upload-action">
              <Upload :custom-request="uploadMeetingFile" :show-upload-list="false">
                <Button block>上传会议附件</Button>
              </Upload>
            </Col>
            <Col :span="12">
              <Form.Item label="会议纪要">
                <Textarea v-model:value="meetingRecordForm.minutes" :auto-size="{ minRows: 3, maxRows: 6 }" />
              </Form.Item>
            </Col>
            <Col :span="12">
              <Form.Item label="会议决议">
                <Textarea v-model:value="meetingRecordForm.decisions" :auto-size="{ minRows: 3, maxRows: 6 }" />
              </Form.Item>
            </Col>
            <Col :span="24">
              <div class="project-detail__section-head">
                <span>行动项</span>
                <Button size="small" @click="handleAddMeetingActionItem">新增行动项</Button>
              </div>
              <div class="project-detail__meeting-actions">
                <div
                  v-for="(action, index) in meetingRecordForm.actionItems"
                  :key="index"
                  class="project-detail__meeting-action-row"
                >
                  <Input v-model:value="action.title" placeholder="行动项内容" />
                  <Select v-model:value="action.actionType" :options="meetingActionTypeOptions" />
                  <Select v-model:value="action.assigneeUserId" allow-clear :options="memberSelectOptions" placeholder="负责人" show-search />
                  <Input v-model:value="action.dueDate" type="date" />
                  <Input
                    v-if="action.actionType === '转任务'"
                    v-model:value="action.taskCode"
                    placeholder="任务编号（可空）"
                  />
                  <Input
                    v-if="action.actionType === '转任务'"
                    v-model:value.number="action.estimatedWorkHours"
                    min="0"
                    placeholder="预计工时"
                    type="number"
                  />
                  <Select
                    v-if="action.actionType === '转任务'"
                    v-model:value="action.priority"
                    :options="demandPriorityOptions"
                    placeholder="优先级"
                  />
                  <Select
                    v-if="['生成风险', '生成问题'].includes(action.actionType)"
                    v-model:value="action.priority"
                    :options="taskPriorityOptions"
                    placeholder="优先级/级别"
                  />
                  <Select
                    v-if="action.actionType === '关联任务'"
                    v-model:value="action.linkedTaskId"
                    allow-clear
                    :options="taskSelectOptions"
                    placeholder="关联任务"
                    show-search
                  />
                  <Input
                    v-if="action.actionType === '生成风险'"
                    v-model:value="action.generatedRiskResponseStrategy"
                    placeholder="应对策略（可空）"
                  />
                  <Select
                    v-if="action.actionType === '生成问题'"
                    v-model:value="action.generatedIssueType"
                    :options="[
                      { label: '执行问题', value: '执行问题' },
                      { label: '质量问题', value: '质量问题' },
                      { label: '需求问题', value: '需求问题' },
                    ]"
                    placeholder="问题类型"
                  />
                  <Select v-if="action.actionType === '关联风险'" v-model:value="action.linkedRaidItemId" allow-clear :options="activeRiskOptions" placeholder="关联风险" show-search />
                  <Select v-if="action.actionType === '关联问题'" v-model:value="action.linkedIssueId" allow-clear :options="issueSelectOptions" placeholder="关联问题" show-search />
                  <Select v-model:value="action.status" :options="meetingActionStatusOptions" />
                  <Button danger type="link" @click="handleRemoveMeetingActionItem(index)">重置</Button>
                </div>
              </div>
            </Col>
            <Col :span="24">
              <Space>
                <Button :loading="savingMeetingRecord" type="primary" @click="handleCreateMeetingRecord">保存会议记录</Button>
                <Button @click="handleCancelMeetingDraft">取消</Button>
              </Space>
            </Col>
          </Row>
        </Form>
        <CommunicationGrid
          class="app-vxe-card-grid"
          grid-class="project-detail__communication-grid"
          :grid-options="communicationGridOptions"
        >
          <template #title="{ row }">
            <Button
              class="project-detail__file-name-button"
              type="link"
              @click="openCommunicationItem(row)"
            >
              {{ row.title }}
            </Button>
            <div v-if="row.summary" class="project-detail__file-type">
              {{ row.summary }}
            </div>
          </template>
          <template #attachmentFileName="{ row }">
            {{ row.attachmentFileName || '-' }}
          </template>
          <template #periodText="{ row }">
            <span>{{ row.periodText }}</span>
            <div v-if="row.sprintName" class="project-detail__file-type">
              {{ row.sprintName }}
            </div>
          </template>
          <template #actionCount="{ row }">
            <Tag :color="row.actionCount > 0 ? 'processing' : 'default'">
              {{ row.actionCount }}
            </Tag>
          </template>
          <template #status="{ row }">
            <Tag color="processing">{{ row.status }}</Tag>
          </template>
        </CommunicationGrid>
      </Card>
    </div>
  </div>
</section>
</template>
