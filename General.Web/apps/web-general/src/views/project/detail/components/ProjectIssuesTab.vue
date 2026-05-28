<script lang="ts" setup>
import { Alert, Button, Card, Col, Form, Input, InputNumber, Row, Select, Table, Tag } from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  detail,
  formatDate,
  handleUpdateRaidRisk,
  issueColumns,
  issueMetrics,
  loading,
  raidRiskRows,
  riskResponseOptions,
  savingRaidRiskId,
  workflowPreviewGroups,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <Alert
    class="project-detail__alert"
    message="缺陷 / 问题补充了需求、产品、开发、测试字段；下方工作流仅做前端预览，方便你确认是否保留，暂不接后端配置。"
    show-icon
    type="info"
  />
  <Row :gutter="[16, 16]">
    <Col :lg="16" :span="24">
      <Card :bordered="false" class="project-detail__inner-card" title="缺陷 / 问题统一入口">
        <Table
          :columns="issueColumns"
          :data-source="detail?.issues || []"
          :loading="loading"
          :pagination="false"
          :scroll="{ x: 1200 }"
          row-key="id"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'dueDate'">
              {{ formatDate((record as ProjectApi.IssueItem).dueDate) }}
            </template>
            <template v-else-if="['requirementTitle', 'productOwnerName', 'developerOwnerName', 'testerOwnerName'].includes(String(column.key))">
              {{ (record as Record<string, string | undefined | null>)[String(column.key)] || '-' }}
            </template>
            <template v-else-if="column.key === 'level'">
              <Tag :color="(record as ProjectApi.IssueItem).level === '高' ? 'error' : 'gold'">
                {{ (record as ProjectApi.IssueItem).level }}
              </Tag>
            </template>
          </template>
        </Table>
      </Card>
    </Col>
    <Col :lg="8" :span="24">
      <Card :bordered="false" class="project-detail__inner-card" title="处理概况">
        <div class="project-detail__issue-metrics">
          <article v-for="metric in issueMetrics" :key="metric.label" class="project-detail__issue-metric">
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
          </article>
        </div>
      </Card>
      <Card :bordered="false" class="project-detail__inner-card" title="风险联动">
        <div class="project-detail__stack-list">
          <article v-for="item in detail?.raidItems || []" :key="item.id" class="project-detail__stack-item">
            <div class="project-detail__item-head">
              <Tag :color="item.isOverdue ? 'error' : 'processing'">{{ item.type }}</Tag>
              <Tag>{{ item.level }}</Tag>
            </div>
            <strong>{{ item.title }}</strong>
            <span>{{ item.ownerName }} / {{ item.status }}</span>
          </article>
        </div>
      </Card>
    </Col>
  </Row>

  <Card :bordered="false" class="project-detail__inner-card" title="项目工作流视图">
    <div class="project-detail__workflow-grid">
      <article v-for="workflow in workflowPreviewGroups" :key="workflow.key" class="project-detail__workflow-card">
        <header class="project-detail__workflow-card-head">
          <strong>{{ workflow.title }}</strong>
          <Tag :color="workflow.tone === 'danger' ? 'error' : workflow.tone === 'warning' ? 'gold' : 'processing'">
            {{ workflow.steps.length }} 个节点
          </Tag>
        </header>
        <div class="project-detail__workflow-canvas">
          <div class="project-detail__workflow-chain">
            <template v-for="(step, index) in workflow.steps" :key="step.key">
              <section class="project-detail__workflow-node">
                <span class="project-detail__workflow-node-count">{{ step.count }}</span>
                <strong>{{ step.title }}</strong>
              </section>
              <div v-if="Number(index) < workflow.steps.length - 1" class="project-detail__workflow-arrow">→</div>
            </template>
          </div>
        </div>
      </article>
    </div>
  </Card>

  <Card :bordered="false" class="project-detail__inner-card" title="风险评分 / 应对">
    <div v-if="(detail?.raidItems || []).length === 0" class="project-detail__muted">暂无 RAID 数据。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="{ form, item } in raidRiskRows.slice(0, 6)" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag :color="(form.probability * form.impact) >= 15 ? 'error' : (form.probability * form.impact) >= 8 ? 'gold' : 'blue'">
            规则评分 {{ form.probability * form.impact }}
          </Tag>
          <Tag>{{ item.type }}</Tag>
        </div>
        <strong>{{ item.title }}</strong>
        <div class="project-detail__risk-grid">
          <Form.Item label="概率">
            <InputNumber :value="form.probability" :max="5" :min="0" @update:value="(val) => { form.probability = typeof val === 'number' ? val : 0; }" />
          </Form.Item>
          <Form.Item label="影响">
            <InputNumber :value="form.impact" :max="5" :min="0" @update:value="(val) => { form.impact = typeof val === 'number' ? val : 0; }" />
          </Form.Item>
          <Form.Item label="应对策略">
            <Select :value="form.responseStrategy" allow-clear :options="riskResponseOptions" @update:value="(val) => { form.responseStrategy = typeof val === 'string' ? val : ''; }" />
          </Form.Item>
        </div>
        <Form.Item label="触发条件">
          <Input :value="form.triggerCondition" :maxlength="512" @update:value="(val) => { form.triggerCondition = val; }" />
        </Form.Item>
        <Form.Item label="应急预案">
          <Input.TextArea :value="form.contingencyPlan" :auto-size="{ minRows: 2, maxRows: 3 }" :maxlength="1024" @update:value="(val) => { form.contingencyPlan = val; }" />
        </Form.Item>
        <Button :loading="savingRaidRiskId === item.id" size="small" type="primary" @click="handleUpdateRaidRisk(item.id)">保存风险参数</Button>
      </article>
    </div>
  </Card>
</section>
</template>
