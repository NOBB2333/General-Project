<script lang="ts" setup>
import { Button, Card, Form, Input, Progress, Select, Tag } from 'ant-design-vue';


import { useProjectDetailContext } from '../context';

const {
  approvalStatusOptions,
  charterForm,
  collaborationMetrics,
  currentHealth,
  cyclePreview,
  detail,
  formatDate,
  handleSaveCharter,
  healthMetrics,
  milestonePreview,
  overviewDetailSections,
  riskAlerts,
  savingCharter,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <div class="project-detail__overview-hero">
    <div class="project-detail__overview-hero-main">
      <span class="project-detail__eyebrow">项目总览</span>
      <h3>{{ detail?.name || '项目详细信息' }}</h3>
      <div class="project-detail__overview-meta">
        <span>{{ detail?.projectCode || '-' }}</span>
        <span>{{ detail?.organizationUnitName || '-' }}</span>
        <span>{{ detail?.managerName || '-' }}</span>
        <span>{{ formatDate(detail?.plannedStartDate) }} - {{ formatDate(detail?.plannedEndDate) }}</span>
      </div>
    </div>
    <div class="project-detail__overview-kpis">
      <article class="project-detail__overview-kpi">
        <span>项目状态</span>
        <strong>{{ detail?.status || '-' }}</strong>
      </article>
      <article class="project-detail__overview-kpi">
        <span>优先级</span>
        <strong>{{ detail?.priority || '-' }}</strong>
      </article>
      <article class="project-detail__overview-kpi">
        <span>成员规模</span>
        <strong>{{ detail?.memberCount || 0 }} 人</strong>
      </article>
    </div>
  </div>
  <div class="project-detail__overview-grid project-detail__overview-grid--cards">
    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card project-detail__overview-card--detail" title="项目详细信息">
      <div class="project-detail__detail-sections">
        <section v-for="section in overviewDetailSections" :key="section.title" class="project-detail__detail-section">
          <header>{{ section.title }}</header>
          <div class="project-detail__info-grid">
            <div v-for="item in section.items" :key="item.label" class="project-detail__info-item">
              <span>{{ item.label }}</span>
              <strong>{{ item.value }}</strong>
            </div>
          </div>
        </section>
      </div>
      <div class="project-detail__summary-card">
        <span>项目说明</span>
        <p>{{ detail?.description || '当前项目暂无补充说明，可在后续迭代中补录项目背景、范围和约束。' }}</p>
      </div>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="项目章程">
      <Form class="project-detail__form" layout="vertical">
        <Form.Item label="审批状态">
          <Select v-model:value="charterForm.approvalStatus" :options="approvalStatusOptions" />
        </Form.Item>
        <Form.Item label="商业论证">
          <Input.TextArea v-model:value="charterForm.businessCase" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Form.Item label="项目目标">
          <Input.TextArea v-model:value="charterForm.objectives" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Form.Item label="范围边界">
          <Input.TextArea v-model:value="charterForm.scopeBoundary" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Form.Item label="假设条件">
          <Input.TextArea v-model:value="charterForm.assumptions" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Form.Item label="制约因素">
          <Input.TextArea v-model:value="charterForm.constraints" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Form.Item label="成功标准">
          <Input.TextArea v-model:value="charterForm.successCriteria" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
        </Form.Item>
        <Button :loading="savingCharter" type="primary" @click="handleSaveCharter">保存章程</Button>
      </Form>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="当前健康度">
      <div class="project-detail__health project-detail__health--panel">
        <Progress
          :percent="currentHealth.percent"
          :status="currentHealth.color === 'warning' ? 'active' : currentHealth.color as any"
          type="circle"
        />
        <div class="project-detail__health-copy">
          <strong>{{ currentHealth.label }}</strong>
          <span>项目当前处于 {{ detail?.status || '-' }} 状态，重点关注执行与问题闭环。</span>
        </div>
      </div>
      <div class="project-detail__overview-list project-detail__overview-list--metrics">
        <div v-for="item in healthMetrics" :key="item.label" class="project-detail__overview-item">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
      <div class="project-detail__inline-section">
        <header>重点风险</header>
        <div v-if="riskAlerts.length === 0" class="project-detail__muted">当前没有高等级或逾期预警。</div>
        <div v-else class="project-detail__stack-list">
          <article v-for="item in riskAlerts" :key="item.id" class="project-detail__stack-item">
            <div class="project-detail__item-head">
              <Tag :color="item.isOverdue ? 'error' : 'gold'">{{ item.type }}</Tag>
              <Tag>{{ item.level }}</Tag>
            </div>
            <strong>{{ item.title }}</strong>
            <span>{{ item.ownerName }} / {{ item.status }}</span>
          </article>
        </div>
      </div>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="关键里程碑 / 周期摘要">
      <div class="project-detail__split-section">
        <section class="project-detail__inline-section">
          <header>近期里程碑</header>
          <div v-if="milestonePreview.length === 0" class="project-detail__muted">暂无里程碑数据。</div>
          <div v-else class="project-detail__stack-list">
            <article v-for="item in milestonePreview" :key="item.id" class="project-detail__stack-item">
              <strong>{{ item.name }}</strong>
              <span>{{ item.ownerName }} / {{ formatDate(item.plannedCompletionDate) }} / {{ item.status }}</span>
            </article>
          </div>
        </section>
        <section class="project-detail__inline-section">
          <header>执行周期</header>
          <div v-if="cyclePreview.length === 0" class="project-detail__muted">暂无周期摘要。</div>
          <div v-else class="project-detail__stack-list">
            <article v-for="item in cyclePreview" :key="item.id" class="project-detail__stack-item">
              <strong>{{ item.type }} · {{ item.name }}</strong>
              <span>{{ item.ownerName }} / {{ formatDate(item.endDate) }} / {{ item.progress }}%</span>
            </article>
          </div>
        </section>
      </div>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="与我相关">
      <div class="project-detail__overview-list project-detail__overview-list--dense">
        <div v-for="item in collaborationMetrics" :key="item.label" class="project-detail__overview-item">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
    </Card>
  </div>
</section>
</template>
