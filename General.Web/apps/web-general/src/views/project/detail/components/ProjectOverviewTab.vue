<script lang="ts" setup>
import { Button, Card, Collapse, Input, Progress, Space, Tag, Upload } from 'ant-design-vue';

import type { FileApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  canApproveCharterApproval,
  canSubmitCharterApproval,
  canWithdrawCharterApproval,
  charterApprovalComment,
  charterApprovalRecords,
  charterApprovalStatus,
  charterApprovalWorkflow,
  charterFiles,
  collaborationMetrics,
  currentCharterFile,
  currentHealth,
  sprintPreview,
  detail,
  formatDate,
  getApprovalStatusColor,
  handleApproveCharterApproval,
  handleRejectCharterApproval,
  handleHealthAction,
  handleSubmitCharterApproval,
  handleWithdrawCharterApproval,
  healthMetrics,
  healthReasonItems,
  healthRuleInfo,
  milestonePreview,
  overviewDetailSections,
  riskAlerts,
  processingCharterApproval,
  openProjectFilePreview,
  switchProjectDetailTab,
  uploadCharterFile,
} = useProjectDetailContext();

function formatSize(size?: number) {
  if (!size) {
    return '-';
  }

  if (size < 1024) {
    return `${size} B`;
  }

  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }

  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}

function formatSprintCode(code?: null | string) {
  if (!code) {
    return '-';
  }

  const matched = code.match(/^SPR[-_]?(\d+)$/i);
  if (matched?.[1]) {
    return `排期-${matched[1].padStart(3, '0')}`;
  }

  return code.replace(/Sprint/gi, '排期');
}

function formatSprintName(name?: null | string) {
  return (name || '')
    .replace(/第\s*(\d+)\s*迭代/g, '第 $1 排期')
    .replace(/Sprint/gi, '排期');
}
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
    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card project-detail__overview-card--detail rounded-2xl" title="项目详细信息">
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

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card rounded-2xl" title="项目章程">
      <div class="project-detail__inline-section">
        <header>章程文件</header>
        <div v-if="currentCharterFile" class="project-detail__charter-file">
          <div class="project-detail__charter-file-main">
            <strong class="project-detail__file-name-cell">{{ currentCharterFile.fileName }}</strong>
            <div class="project-detail__knowledge-meta project-detail__knowledge-meta--single-line">
              <span>当前有效</span>
              <span>{{ formatSize(currentCharterFile.size) }}</span>
              <span>{{ currentCharterFile.uploadedBy || '-' }}</span>
              <span>{{ formatDate(currentCharterFile.uploadedAt) }}</span>
            </div>
          </div>
          <Button type="link" @click="openProjectFilePreview(currentCharterFile as FileApi.FileItem)">预览</Button>
        </div>
        <div v-else class="project-detail__muted">暂无章程文件，建议上传 Word、PDF 或公司模板文档作为正式依据。</div>
        <Upload :custom-request="uploadCharterFile" :show-upload-list="false">
          <Button type="primary">{{ currentCharterFile ? '替换章程文件' : '上传章程文件' }}</Button>
        </Upload>
        <div class="project-detail__muted">
          请上传包含项目目标、范围边界、成功标准、关键制约因素和主要假设的 Word/PDF 章程文件；系统只保留必要审批状态，不要求在页面重复填写正文。
        </div>
        <div v-if="charterFiles.length > 1" class="project-detail__muted">
          已保留 {{ charterFiles.length }} 份章程文件，最新上传文件作为当前有效版本。
        </div>
      </div>
      <Collapse ghost>
        <Collapse.Panel key="approval" header="章程审批流转">
          <div class="project-detail__form">
            <div class="project-detail__approval-panel">
              <div class="project-detail__item-head">
                <Tag :color="getApprovalStatusColor(charterApprovalStatus)">
                  {{ charterApprovalStatus || '草稿' }}
                </Tag>
                <Tag v-if="charterApprovalWorkflow">
                  {{ charterApprovalWorkflow.currentNodeName || '审批' }}
                </Tag>
              </div>
              <div v-if="charterApprovalWorkflow" class="project-detail__muted">
                Elsa 实例：{{ charterApprovalWorkflow.elsaWorkflowInstanceId || charterApprovalWorkflow.id }}
              </div>
              <Input.TextArea
                v-model:value="charterApprovalComment"
                :auto-size="{ minRows: 2, maxRows: 3 }"
                :maxlength="512"
                placeholder="审批意见，可选"
              />
              <Space wrap>
                <Button
                  :disabled="!canSubmitCharterApproval"
                  :loading="processingCharterApproval === 'submit'"
                  type="primary"
                  @click="handleSubmitCharterApproval"
                >
                  提交审批
                </Button>
                <Button
                  :disabled="!canWithdrawCharterApproval"
                  :loading="processingCharterApproval === 'withdraw'"
                  @click="handleWithdrawCharterApproval"
                >
                  撤回
                </Button>
                <Button
                  :disabled="!canApproveCharterApproval"
                  :loading="processingCharterApproval === 'approve'"
                  type="primary"
                  @click="handleApproveCharterApproval"
                >
                  通过
                </Button>
                <Button
                  danger
                  :disabled="!canApproveCharterApproval"
                  :loading="processingCharterApproval === 'reject'"
                  @click="handleRejectCharterApproval"
                >
                  驳回
                </Button>
              </Space>
            </div>
          </div>
          <div class="project-detail__inline-section project-detail__approval-records">
            <header>审批记录</header>
            <div v-if="charterApprovalRecords.length === 0" class="project-detail__muted">暂无审批记录。</div>
            <div v-else class="project-detail__stack-list">
              <article v-for="record in charterApprovalRecords" :key="record.id" class="project-detail__stack-item">
                <div class="project-detail__item-head">
                  <Tag>{{ record.action }}</Tag>
                  <Tag :color="getApprovalStatusColor(record.toStatus)">
                    {{ record.fromStatus }} -> {{ record.toStatus }}
                  </Tag>
                </div>
                <strong>{{ record.actorUserName || '-' }} / {{ formatDate(record.creationTime) }}</strong>
                <span>{{ record.comment || '无审批意见' }}</span>
              </article>
            </div>
          </div>
        </Collapse.Panel>
      </Collapse>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card rounded-2xl" title="当前健康度">
      <div class="project-detail__health project-detail__health--panel">
        <Progress
          :percent="currentHealth.percent"
          :status="currentHealth.color === 'warning' ? 'active' : currentHealth.color as any"
          type="circle"
        />
        <div class="project-detail__health-copy">
          <strong>{{ currentHealth.label }}</strong>
          <span>规则计算，不是 AI 或机器学习评分。</span>
        </div>
      </div>
      <div class="project-detail__overview-list project-detail__overview-list--metrics">
        <div v-for="item in healthRuleInfo" :key="item.label" class="project-detail__overview-item">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
      <div class="project-detail__overview-list project-detail__overview-list--metrics">
        <div v-for="item in healthMetrics" :key="item.label" class="project-detail__overview-item">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>
      <div class="project-detail__inline-section">
        <header>健康度原因</header>
        <div class="project-detail__stack-list">
          <article v-for="item in healthReasonItems" :key="`${item.label}-${item.score}`" class="project-detail__stack-item">
            <div class="project-detail__item-head">
              <Tag :color="item.score > 0 ? 'gold' : 'green'">{{ item.label }}</Tag>
              <Tag>{{ item.score }} 分</Tag>
              <Tag>{{ item.ruleVersion }}</Tag>
            </div>
            <span>{{ item.reason }}</span>
            <small>数据来源：{{ item.dataSource }}</small>
            <Space v-if="item.actionKey">
              <Button size="small" type="link" @click="handleHealthAction(item.actionKey)">
                {{ item.actionLabel || '去处理' }}
              </Button>
            </Space>
          </article>
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

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card rounded-2xl" title="关键里程碑 / 排期摘要">
      <div class="project-detail__section-head">
        <span>此处只展示摘要，里程碑和 WBS 在「目标与计划」维护；需求与排期在「需求与排期」维护。</span>
        <Button size="small" type="link" @click="switchProjectDetailTab('plan')">进入目标与计划</Button>
      </div>
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
          <header>近期排期</header>
          <div v-if="sprintPreview.length === 0" class="project-detail__muted">暂无排期摘要。</div>
          <div v-else class="project-detail__stack-list">
            <article v-for="item in sprintPreview" :key="item.id" class="project-detail__stack-item">
              <strong>{{ formatSprintCode(item.sprintCode) }} · {{ formatSprintName(item.name) }}</strong>
              <span>{{ item.ownerName }} / {{ formatDate(item.endDate) }} / {{ item.progress }}%</span>
            </article>
          </div>
        </section>
      </div>
    </Card>

    <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card rounded-2xl" title="与我相关">
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
