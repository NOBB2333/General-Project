<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';

import { Button, Card, Checkbox, Col, Empty, Form, Input, Row, Select, Space, Tag, Upload, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { FileApi } from '#/api/core';
import { downloadPlatformFile } from '#/components/file/download-file';

import { useProjectDetailContext } from '../context';

const {
  acceptanceCriteriaForm,
  acceptanceCriteriaFiles,
  acceptanceCriteriaOptions,
  acceptanceFileOptions,
  acceptanceFiles,
  acceptanceRecordForm,
  acceptanceResultOptions,
  acceptanceStatusOptions,
  currentAcceptanceCriteriaFile,
  detail,
  formatDate,
  handleCreateAcceptanceCriteria,
  handleCreateAcceptanceRecord,
  handleCreateQualityChecklist,
  handleCreateQualityInspection,
  issueLevelOptions,
  memberSelectOptions,
  milestoneSelectOptions,
  qualityCategoryOptions,
  qualityChecklistForm,
  qualityChecklistOptions,
  qualityInspectionForm,
  qualityInspectionResultOptions,
  openProjectFilePreview,
  sprintOptions,
  requirementSelectOptions,
  savingAcceptanceCriteria,
  savingAcceptanceRecord,
  savingQualityChecklist,
  savingQualityInspection,
  selectedAcceptanceSprintId,
  showAcceptanceForm,
  showAcceptanceRecordForm,
  showChecklistForm,
  showInspectionForm,
  taskSelectOptions,
  uploadAcceptanceCriteriaFile,
  uploadAcceptanceFile,
  uploadDeliveryFile,
} = useProjectDetailContext();

const acceptanceCriteriaFileColumns = [
  { align: 'left', field: 'fileName', slots: { default: 'fileName' }, title: '标准文件', minWidth: 220 },
  { field: 'versionMarker', slots: { default: 'versionMarker' }, title: '版本', width: 90 },
  { field: 'uploadedBy', title: '上传人', width: 130 },
  { field: 'uploadedAt', slots: { default: 'uploadedAt' }, title: '上传时间', width: 150 },
  { field: 'category', slots: { default: 'category' }, title: '分类', width: 180 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 150 },
] satisfies VxeGridProps<FileApi.FileItem>['columns'];

const acceptanceFileColumns = [
  { align: 'left', field: 'fileName', slots: { default: 'fileName' }, title: '文件名称', minWidth: 220 },
  { field: 'category', slots: { default: 'category' }, title: '分类', width: 160 },
  { field: 'uploadedBy', title: '上传人', width: 130 },
  { field: 'uploadedAt', slots: { default: 'uploadedAt' }, title: '上传时间', width: 150 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 150 },
] satisfies VxeGridProps<FileApi.FileItem>['columns'];

const historicalAcceptanceCriteriaFiles = computed(() =>
  acceptanceCriteriaFiles.value.slice(1),
);
const acceptanceCriteriaFileGridOptions = computed<VxeGridProps<FileApi.FileItem>>(() => ({
  columns: acceptanceCriteriaFileColumns,
  data: acceptanceCriteriaFiles.value,
  emptyText: '暂无标准文件，可上传验收标准、验收模板或客户确认版本。',
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'fileKey' },
  scrollX: { enabled: true },
  size: 'medium',
}));
const acceptanceFileGridOptions = computed<VxeGridProps<FileApi.FileItem>>(() => ({
  columns: acceptanceFileColumns,
  data: acceptanceFiles.value,
  emptyText: '暂无验收文件，可上传验收报告、签字扫描件或交付物清单。',
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'fileKey' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const [AcceptanceCriteriaFileGrid] = useVbenVxeGrid<FileApi.FileItem>({ gridOptions: {} });
const [AcceptanceFileGrid] = useVbenVxeGrid<FileApi.FileItem>({ gridOptions: {} });

function resolveAcceptanceCriteriaVersionMarker(file: FileApi.FileItem) {
  if (typeof file.isCurrentVersion === 'boolean') {
    return {
      color: file.isCurrentVersion ? 'success' : 'default',
      text: `${file.isCurrentVersion ? '当前' : '历史'} V${file.versionNo || 1}`,
    };
  }

  return currentAcceptanceCriteriaFile.value?.fileKey === file.fileKey
    ? { color: 'success', text: '当前' }
    : { color: 'default', text: '历史' };
}

async function handleDownloadProjectFile(file: FileApi.FileItem) {
  try {
    await downloadPlatformFile(file);
  } catch (error) {
    const description = error instanceof Error ? error.message : '文件下载失败';
    message.error(description || '文件下载失败');
  }
}
</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__metric-strip project-detail__metric-strip--delivery">
    <div class="project-detail__overview-item">
      <span>验收标准</span>
      <strong>{{ detail?.acceptanceCriteria.length || 0 }}</strong>
    </div>
    <div class="project-detail__overview-item">
      <span>交付检查单</span>
      <strong>{{ detail?.qualityChecklists.length || 0 }}</strong>
    </div>
    <div class="project-detail__overview-item">
      <span>检查记录</span>
      <strong>{{ detail?.qualityInspections.length || 0 }}</strong>
    </div>
    <div class="project-detail__overview-item">
      <span>验收记录</span>
      <strong>{{ detail?.acceptanceRecords.length || 0 }}</strong>
    </div>
  </div>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="验收标准文件 / 检查项">
    <template #extra>
      <Space>
        <Upload :custom-request="uploadAcceptanceCriteriaFile" :show-upload-list="false">
          <Button size="small" type="primary">上传标准文件</Button>
        </Upload>
        <Button size="small" @click="showAcceptanceForm = !showAcceptanceForm">{{ showAcceptanceForm ? '收起' : '新增检查项' }}</Button>
      </Space>
    </template>
    <div class="project-detail__split-section project-detail__split-section--acceptance">
      <div class="project-detail__summary-card">
        <header>当前验收标准文件</header>
        <template v-if="currentAcceptanceCriteriaFile">
          <strong>{{ currentAcceptanceCriteriaFile.fileName }}</strong>
          <p>{{ currentAcceptanceCriteriaFile.uploadedBy || '-' }} / {{ formatDate(currentAcceptanceCriteriaFile.uploadedAt) }}</p>
          <Space>
            <Tag color="blue">{{ currentAcceptanceCriteriaFile.category || 'Project/AcceptanceCriteria' }}</Tag>
            <Button size="small" type="link" @click="openProjectFilePreview(currentAcceptanceCriteriaFile)">预览</Button>
            <Button size="small" type="link" @click="handleDownloadProjectFile(currentAcceptanceCriteriaFile)">下载</Button>
          </Space>
        </template>
        <Empty v-else description="暂无验收标准文件" />
      </div>
      <div class="project-detail__summary-card">
        <header>历史标准文件</header>
        <div v-if="historicalAcceptanceCriteriaFiles.length === 0" class="project-detail__muted">暂无历史版本。上传新的标准文件后，原文件会继续保留在这里和文档与知识中。</div>
        <div v-else class="project-detail__compact-file-list">
          <button
            v-for="file in historicalAcceptanceCriteriaFiles"
            :key="file.fileKey"
            class="project-detail__compact-file-item"
            type="button"
            @click="openProjectFilePreview(file)"
          >
            <span>{{ file.fileName }}</span>
            <small>{{ formatDate(file.uploadedAt) }}</small>
          </button>
        </div>
      </div>
    </div>
    <div v-if="showAcceptanceForm" class="project-detail__inline-form-card">
      <Form layout="vertical">
        <Row :gutter="12">
          <Col :span="8"><Form.Item label="验收标题"><Input v-model:value="acceptanceCriteriaForm.title" :maxlength="128" /></Form.Item></Col>
          <Col :span="4"><Form.Item label="状态"><Select v-model:value="acceptanceCriteriaForm.status" :options="acceptanceStatusOptions" /></Form.Item></Col>
          <Col :span="6"><Form.Item label="关联任务"><Select v-model:value="acceptanceCriteriaForm.taskId" allow-clear :options="taskSelectOptions" show-search /></Form.Item></Col>
          <Col :span="6"><Form.Item label="关联里程碑"><Select v-model:value="acceptanceCriteriaForm.milestoneId" allow-clear :options="milestoneSelectOptions" show-search /></Form.Item></Col>
        </Row>
        <Form.Item label="验收说明">
          <Input.TextArea v-model:value="acceptanceCriteriaForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Button :loading="savingAcceptanceCriteria" type="primary" @click="handleCreateAcceptanceCriteria">提交</Button>
      </Form>
    </div>
    <div v-if="(detail?.acceptanceCriteria || []).length === 0" class="project-detail__muted">暂无验收标准。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in detail?.acceptanceCriteria || []" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag :color="item.status === '已通过' ? 'success' : item.status === '已驳回' ? 'error' : 'processing'">{{ item.status }}</Tag>
          <span>{{ item.taskTitle !== '-' ? item.taskTitle : item.milestoneName }}</span>
        </div>
        <strong>{{ item.title }}</strong>
        <span v-if="item.description">{{ item.description }}</span>
      </article>
    </div>
    <AcceptanceCriteriaFileGrid
      class="app-vxe-card-grid project-detail__file-table"
      grid-class="project-detail__acceptance-criteria-file-grid"
      :grid-options="acceptanceCriteriaFileGridOptions"
    >
      <template #fileName="{ row }">
        <Button type="link" @click="openProjectFilePreview(row)">
          {{ row.fileName }}
        </Button>
      </template>
      <template #category="{ row }">
        <Tag color="blue">{{ row.category || 'Project/AcceptanceCriteria' }}</Tag>
      </template>
      <template #versionMarker="{ row }">
        <Tag :color="resolveAcceptanceCriteriaVersionMarker(row).color">
          {{ resolveAcceptanceCriteriaVersionMarker(row).text }}
        </Tag>
      </template>
      <template #uploadedAt="{ row }">
        {{ formatDate(row.uploadedAt) }}
      </template>
      <template #actions="{ row }">
        <Space>
          <Button size="small" type="link" @click="openProjectFilePreview(row)">预览</Button>
          <Button size="small" type="link" @click="handleDownloadProjectFile(row)">下载</Button>
        </Space>
      </template>
    </AcceptanceCriteriaFileGrid>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="验收文件 / 交付资料">
    <template #extra>
      <Space>
        <Select
          v-model:value="selectedAcceptanceSprintId"
          allow-clear
          :options="sprintOptions"
          placeholder="关联排期"
          show-search
          style="width: 220px"
        />
        <Upload :custom-request="uploadAcceptanceFile" :show-upload-list="false">
          <Button size="small" type="primary">上传验收文件</Button>
        </Upload>
        <Upload :custom-request="uploadDeliveryFile" :show-upload-list="false">
          <Button size="small">上传交付物</Button>
        </Upload>
      </Space>
    </template>
    <AcceptanceFileGrid
      class="app-vxe-card-grid"
      grid-class="project-detail__acceptance-file-grid"
      :grid-options="acceptanceFileGridOptions"
    >
      <template #fileName="{ row }">
        <Button type="link" @click="openProjectFilePreview(row)">
          {{ row.fileName }}
        </Button>
      </template>
      <template #category="{ row }">
        <Tag color="blue">{{ row.category || 'Project/AcceptanceRecord' }}</Tag>
      </template>
      <template #uploadedAt="{ row }">
        {{ formatDate(row.uploadedAt) }}
      </template>
      <template #actions="{ row }">
        <Space>
          <Button size="small" type="link" @click="openProjectFilePreview(row)">预览</Button>
          <Button size="small" type="link" @click="handleDownloadProjectFile(row)">下载</Button>
        </Space>
      </template>
    </AcceptanceFileGrid>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="验收记录">
    <template #extra>
      <Button size="small" type="primary" @click="showAcceptanceRecordForm = !showAcceptanceRecordForm">
        {{ showAcceptanceRecordForm ? '收起' : '新增验收记录' }}
      </Button>
    </template>
    <div v-if="showAcceptanceRecordForm" class="project-detail__inline-form-card">
      <Form layout="vertical">
        <Row :gutter="12">
          <Col :span="8"><Form.Item label="记录标题"><Input v-model:value="acceptanceRecordForm.title" :maxlength="128" /></Form.Item></Col>
          <Col :span="4"><Form.Item label="结论"><Select v-model:value="acceptanceRecordForm.acceptanceResult" :options="acceptanceResultOptions" /></Form.Item></Col>
          <Col :span="6"><Form.Item label="验收人"><Select v-model:value="acceptanceRecordForm.acceptorUserId" :options="memberSelectOptions" show-search /></Form.Item></Col>
          <Col :span="6"><Form.Item label="验收时间"><Input v-model:value="acceptanceRecordForm.acceptanceTime" type="date" /></Form.Item></Col>
        </Row>
        <Row :gutter="12">
          <Col :span="6"><Form.Item label="关联里程碑"><Select v-model:value="acceptanceRecordForm.milestoneId" allow-clear :options="milestoneSelectOptions" show-search /></Form.Item></Col>
          <Col :span="6"><Form.Item label="关联排期"><Select v-model:value="acceptanceRecordForm.sprintId" allow-clear :options="sprintOptions" show-search /></Form.Item></Col>
          <Col :span="6"><Form.Item label="关联验收标准"><Select v-model:value="acceptanceRecordForm.acceptanceCriteriaIds" mode="multiple" :options="acceptanceCriteriaOptions" show-search /></Form.Item></Col>
          <Col :span="6"><Form.Item label="关联验收文件"><Select v-model:value="acceptanceRecordForm.acceptanceFileId" allow-clear :options="acceptanceFileOptions" show-search /></Form.Item></Col>
        </Row>
        <Form.Item label="覆盖需求">
          <Select
            v-model:value="acceptanceRecordForm.requirementIds"
            mode="multiple"
            :options="requirementSelectOptions"
            placeholder="选择本次验收覆盖的需求"
            show-search
          />
        </Form.Item>
        <Row :gutter="12">
          <Col :span="12"><Form.Item label="整改要求"><Input.TextArea v-model:value="acceptanceRecordForm.rectificationRequirement" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" /></Form.Item></Col>
          <Col :span="12"><Form.Item label="备注"><Input.TextArea v-model:value="acceptanceRecordForm.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" /></Form.Item></Col>
        </Row>
        <div v-if="acceptanceRecordForm.acceptanceResult === '不通过'" class="project-detail__inline-form-card">
          <Checkbox v-model:checked="acceptanceRecordForm.createIssueWhenRejected">同步生成问题记录</Checkbox>
          <Row v-if="acceptanceRecordForm.createIssueWhenRejected" :gutter="12">
            <Col :span="8"><Form.Item label="问题责任人"><Select v-model:value="acceptanceRecordForm.issueOwnerUserId" :options="memberSelectOptions" show-search /></Form.Item></Col>
            <Col :span="8"><Form.Item label="问题级别"><Select v-model:value="acceptanceRecordForm.issueLevel" :options="issueLevelOptions" /></Form.Item></Col>
            <Col :span="8"><Form.Item label="要求完成日期"><Input v-model:value="acceptanceRecordForm.issueDueDate" type="date" /></Form.Item></Col>
          </Row>
        </div>
        <Button :loading="savingAcceptanceRecord" type="primary" @click="handleCreateAcceptanceRecord">提交</Button>
      </Form>
    </div>
    <div v-if="(detail?.acceptanceRecords || []).length === 0" class="project-detail__muted">暂无验收记录。先上传验收报告或签字文件，再创建记录关联标准、里程碑和文件。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in detail?.acceptanceRecords || []" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag :color="item.acceptanceResult === '通过' ? 'success' : item.acceptanceResult === '不通过' ? 'error' : 'warning'">{{ item.acceptanceResult }}</Tag>
          <span>{{ item.milestoneName }}</span>
          <span v-if="item.sprintName && item.sprintName !== '-'">{{ item.sprintName }}</span>
          <span>{{ item.acceptorName }} / {{ formatDate(item.acceptanceTime) }}</span>
        </div>
        <strong>{{ item.title }}</strong>
        <div class="project-detail__tag-list">
          <Tag v-for="title in item.acceptanceCriteriaTitles" :key="`${item.id}-${title}`" color="blue">{{ title }}</Tag>
          <Tag v-for="title in item.requirementTitles" :key="`${item.id}-requirement-${title}`" color="purple">{{ title }}</Tag>
        </div>
        <span v-if="item.acceptanceFileName && item.acceptanceFileName !== '-'">验收文件：{{ item.acceptanceFileName }}</span>
        <span v-if="item.rectificationRequirement">整改：{{ item.rectificationRequirement }}</span>
        <span v-if="item.remark">备注：{{ item.remark }}</span>
      </article>
    </div>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="交付检查单">
    <template #extra>
      <Button size="small" type="primary" @click="showChecklistForm = !showChecklistForm">{{ showChecklistForm ? '收起' : '新增' }}</Button>
    </template>
    <div v-if="showChecklistForm" class="project-detail__inline-form-card">
      <Form layout="inline">
        <Form.Item label="标题"><Input v-model:value="qualityChecklistForm.title" :maxlength="128" /></Form.Item>
        <Form.Item label="分类"><Select v-model:value="qualityChecklistForm.category" :options="qualityCategoryOptions" /></Form.Item>
        <Form.Item label="说明"><Input v-model:value="qualityChecklistForm.description" :maxlength="1024" /></Form.Item>
        <Form.Item><Button :loading="savingQualityChecklist" type="primary" @click="handleCreateQualityChecklist">提交</Button></Form.Item>
      </Form>
    </div>
    <div v-if="(detail?.qualityChecklists || []).length === 0" class="project-detail__muted">暂无检查单。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in detail?.qualityChecklists || []" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag>{{ item.category }}</Tag>
        </div>
        <strong>{{ item.title }}</strong>
        <span v-if="item.description">{{ item.description }}</span>
      </article>
    </div>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="检查记录">
    <template #extra>
      <Button size="small" type="primary" @click="showInspectionForm = !showInspectionForm">{{ showInspectionForm ? '收起' : '新增' }}</Button>
    </template>
    <div v-if="showInspectionForm" class="project-detail__inline-form-card">
      <Form layout="vertical">
        <Row :gutter="12">
          <Col :span="6"><Form.Item label="检查单"><Select v-model:value="qualityInspectionForm.qualityChecklistId" :options="qualityChecklistOptions" show-search /></Form.Item></Col>
          <Col :span="4"><Form.Item label="检查人"><Select v-model:value="qualityInspectionForm.inspectorUserId" :options="memberSelectOptions" show-search /></Form.Item></Col>
          <Col :span="4"><Form.Item label="结果"><Select v-model:value="qualityInspectionForm.inspectionResult" :options="qualityInspectionResultOptions" /></Form.Item></Col>
          <Col :span="4"><Form.Item label="日期"><Input v-model:value="qualityInspectionForm.inspectionTime" type="date" /></Form.Item></Col>
        </Row>
        <Row :gutter="12">
          <Col :span="12"><Form.Item label="发现摘要"><Input.TextArea v-model:value="qualityInspectionForm.findingSummary" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" /></Form.Item></Col>
          <Col :span="12"><Form.Item label="整改要求"><Input.TextArea v-model:value="qualityInspectionForm.rectificationRequirement" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" /></Form.Item></Col>
        </Row>
        <Button :loading="savingQualityInspection" type="primary" @click="handleCreateQualityInspection">提交</Button>
      </Form>
    </div>
    <div v-if="(detail?.qualityInspections || []).length === 0" class="project-detail__muted">暂无检查记录。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in detail?.qualityInspections || []" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag :color="item.inspectionResult === '通过' ? 'success' : 'error'">{{ item.inspectionResult }}</Tag>
          <span>{{ item.qualityChecklistTitle }}</span>
        </div>
        <strong>{{ item.inspectorName }} / {{ formatDate(item.inspectionTime) }}</strong>
        <span v-if="item.findingSummary">发现：{{ item.findingSummary }}</span>
        <span v-if="item.rectificationRequirement">整改：{{ item.rectificationRequirement }}</span>
      </article>
    </div>
  </Card>
</section>
</template>
