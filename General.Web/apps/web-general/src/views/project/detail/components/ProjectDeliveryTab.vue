<script lang="ts" setup>
import { Button, Card, Col, Form, Input, Row, Select, Tag } from 'ant-design-vue';


import { useProjectDetailContext } from '../context';

const {
  acceptanceCriteriaForm,
  acceptanceStatusOptions,
  detail,
  formatDate,
  handleCreateAcceptanceCriteria,
  handleCreateQualityChecklist,
  handleCreateQualityInspection,
  memberSelectOptions,
  milestoneSelectOptions,
  qualityCategoryOptions,
  qualityChecklistForm,
  qualityChecklistOptions,
  qualityInspectionForm,
  qualityInspectionResultOptions,
  savingAcceptanceCriteria,
  savingQualityChecklist,
  savingQualityInspection,
  showAcceptanceForm,
  showChecklistForm,
  showInspectionForm,
  taskSelectOptions,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__overview-list project-detail__overview-list--metrics">
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
  </div>

  <Card :bordered="false" class="project-detail__inner-card" title="验收标准">
    <template #extra>
      <Button size="small" type="primary" @click="showAcceptanceForm = !showAcceptanceForm">{{ showAcceptanceForm ? '收起' : '新增' }}</Button>
    </template>
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
  </Card>

  <Card :bordered="false" class="project-detail__inner-card" title="交付检查单">
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

  <Card :bordered="false" class="project-detail__inner-card" title="检查记录">
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
