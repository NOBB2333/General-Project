<script lang="ts" setup>
import {
  Button,
  Card,
  Col,
  Form,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Tag,
} from 'ant-design-vue';

import { useProjectDetailContext } from '../context';

const {
  activeBaselineSummary,
  baselineChangeOptions,
  baselineForm,
  canApproveBaselineChange,
  canSubmitBaselineChange,
  changeApprovalComment,
  changeControlMetrics,
  changeImpactTypeOptions,
  changeRequestForm,
  changeSeverityOptions,
  changeTypeOptions,
  detail,
  evmMetrics,
  formatDate,
  formatMoney,
  formatRatio,
  getBaselineChangeWorkflowStatus,
  getChangeStatusColor,
  getPriorityColor,
  handleAddChangeImpact,
  handleCreateBaseline,
  handleCreateChangeApproval,
  handleCreateChangeRequest,
  handleCreatePerformanceSnapshot,
  handleRejectChangeApproval,
  handleRemoveChangeImpact,
  handleWithdrawChangeApproval,
  memberSelectOptions,
  milestoneSelectOptions,
  performanceSnapshotForm,
  performanceTrendRows,
  savingBaseline,
  savingChangeWorkflowId,
  savingChangeRequest,
  savingPerformanceSnapshot,
  showBaselineForm,
  showChangeRequestForm,
  showPerformanceSnapshotForm,
  taskSelectOptions,
} = useProjectDetailContext();
</script>

<template>
  <section class="project-detail__section">
    <div class="project-detail__scope-note">
      <strong>项目控制</strong>
      <span>这里不是日常任务区，只在立项后固化范围/进度基线、发生重大基线变更或查看 SPI/CPI 快照时使用。普通需求、排期和缺陷处理仍在对应 Tab 完成。</span>
    </div>

    <div
      class="project-detail__overview-list project-detail__overview-list--metrics"
    >
      <div
        v-for="metric in changeControlMetrics"
        :key="metric.label"
        class="project-detail__overview-item"
      >
        <span>{{ metric.label }}</span>
        <strong>{{ metric.value }}</strong>
      </div>
    </div>

    <Row :gutter="[16, 16]">
      <Col :lg="14" :span="24">
        <Card
          :bordered="false"
          class="project-detail__inner-card rounded-2xl"
          title="重大基线变更"
        >
          <template #extra>
            <Button
              size="small"
              type="primary"
              @click="showChangeRequestForm = !showChangeRequestForm"
            >
              {{ showChangeRequestForm ? '收起' : '新增重大变更' }}
            </Button>
          </template>

          <div
            v-if="showChangeRequestForm"
            class="project-detail__inline-form-card"
          >
            <Form layout="vertical">
              <Row :gutter="12">
                <Col :lg="8" :span="24"
                  ><Form.Item label="基线变更标题"
                    ><Input
                      v-model:value="changeRequestForm.title"
                      :maxlength="128" /></Form.Item
                ></Col>
                <Col :lg="4" :span="12"
                  ><Form.Item label="类型"
                    ><Select
                      v-model:value="changeRequestForm.changeType"
                      :options="changeTypeOptions" /></Form.Item
                ></Col>
                <Col :lg="8" :span="24"
                  ><Form.Item label="申请人"
                    ><Select
                      v-model:value="changeRequestForm.applicantUserId"
                      allow-clear
                      :options="memberSelectOptions"
                      show-search /></Form.Item
                ></Col>
              </Row>
              <Row :gutter="12">
                <Col :lg="8" :span="24"
                  ><Form.Item label="变更原因"
                    ><Input.TextArea
                      v-model:value="changeRequestForm.reason"
                      :auto-size="{ minRows: 2, maxRows: 4 }"
                      :maxlength="1024" /></Form.Item
                ></Col>
                <Col :lg="8" :span="24"
                  ><Form.Item label="影响范围"
                    ><Input.TextArea
                      v-model:value="changeRequestForm.impactScope"
                      :auto-size="{ minRows: 2, maxRows: 4 }"
                      :maxlength="1024" /></Form.Item
                ></Col>
                <Col :lg="8" :span="24"
                  ><Form.Item label="预期结果"
                    ><Input.TextArea
                      v-model:value="changeRequestForm.expectedResult"
                      :auto-size="{ minRows: 2, maxRows: 4 }"
                      :maxlength="1024" /></Form.Item
                ></Col>
              </Row>

              <div class="project-detail__inline-section">
                <header>影响分析</header>
                <article
                  v-for="(impact, index) in changeRequestForm.impacts"
                  :key="index"
                  class="project-detail__change-impact-form"
                >
                  <Row :gutter="12">
                    <Col :lg="6" :span="12"
                      ><Form.Item label="影响类型"
                        ><Select
                          v-model:value="impact.impactType"
                          :options="changeImpactTypeOptions" /></Form.Item
                    ></Col>
                    <Col :lg="6" :span="12"
                      ><Form.Item label="严重度"
                        ><Select
                          v-model:value="impact.severity"
                          :options="changeSeverityOptions" /></Form.Item
                    ></Col>
                    <Col :lg="6" :span="12"
                      ><Form.Item label="进度影响(天)"
                        ><InputNumber
                          v-model:value="impact.estimatedScheduleDeltaDays"
                          style="width: 100%" /></Form.Item
                    ></Col>
                    <Col :lg="6" :span="12"
                      ><Form.Item label="成本影响"
                        ><InputNumber
                          v-model:value="impact.estimatedCostDelta"
                          style="width: 100%" /></Form.Item
                    ></Col>
                  </Row>
                  <Row :gutter="12">
                    <Col :lg="12" :span="24"
                      ><Form.Item label="关联任务"
                        ><Select
                          v-model:value="impact.relatedTaskId"
                          allow-clear
                          :options="taskSelectOptions"
                          show-search /></Form.Item
                    ></Col>
                    <Col :lg="12" :span="24"
                      ><Form.Item label="关联里程碑"
                        ><Select
                          v-model:value="impact.relatedMilestoneId"
                          allow-clear
                          :options="milestoneSelectOptions"
                          show-search /></Form.Item
                    ></Col>
                  </Row>
                  <Row :gutter="12">
                    <Col :span="24"
                      ><Form.Item label="分析说明"
                        ><Input
                          v-model:value="impact.analysis"
                          :maxlength="1024" /></Form.Item
                    ></Col>
                  </Row>
                  <Button
                    size="small"
                    type="link"
                    @click="handleRemoveChangeImpact(index)"
                    >删除影响项</Button
                  >
                </article>
              <Button size="small" @click="handleAddChangeImpact"
                  >增加影响项</Button
                >
              </div>
              <Form.Item label="审批意见">
                <Input.TextArea
                  v-model:value="changeApprovalComment"
                  :auto-size="{ minRows: 2, maxRows: 3 }"
                  :maxlength="512"
                  placeholder="提交、通过、驳回、撤回都可填写说明"
                />
              </Form.Item>
              <Button
                :loading="savingChangeRequest"
                type="primary"
                @click="handleCreateChangeRequest"
                >提交基线变更审批</Button
              >
            </Form>
          </div>

          <div
            v-if="(detail?.changeRequests || []).length === 0"
            class="project-detail__muted"
          >
            暂无重大基线变更。日常需求文字调整、排期内任务微调不需要在这里提交。
          </div>
          <div v-else class="project-detail__stack-list">
            <article
              v-for="item in detail?.changeRequests || []"
              :key="item.id"
              class="project-detail__stack-item"
            >
              <div class="project-detail__item-head">
                <Tag :color="getChangeStatusColor(item.status)">{{
                  item.status
                }}</Tag>
                <Tag>{{ item.changeType }}</Tag>
                <span>{{ item.requestCode || '未编号' }}</span>
              </div>
              <strong>{{ item.title }}</strong>
              <span>{{ item.applicantName }} / {{ item.reason }}</span>
              <div class="project-detail__change-impact-list">
                <section
                  v-for="impact in item.impacts"
                  :key="impact.id"
                  class="project-detail__change-impact-item"
                >
                  <Tag :color="getPriorityColor(impact.severity)">{{
                    impact.severity
                  }}</Tag>
                  <strong>{{ impact.impactType }}</strong>
                  <span>影响摘要：{{ impact.analysis }}</span>
                  <small>
                    {{
                      impact.relatedTaskTitle ||
                      impact.relatedMilestoneName ||
                      impact.relatedContractCode ||
                      impact.relatedBudgetCode ||
                      impact.relatedBusinessCode ||
                      '未关联对象'
                    }}
                    / 进度 {{ impact.estimatedScheduleDeltaDays ?? 0 }} 天 /
                    成本 {{ formatMoney(impact.estimatedCostDelta) }}
                  </small>
                </section>
              </div>
              <div class="project-detail__change-approval-panel">
                <div
                  v-if="item.approvals.length > 0"
                  class="project-detail__tag-list"
                >
                  <Tag
                    v-for="approval in item.approvals"
                    :key="approval.id"
                    color="blue"
                  >
                    {{ approval.action }} · {{ approval.approverName }} ·
                    {{ formatDate(approval.decisionTime) }}
                  </Tag>
                </div>
                <Space wrap>
                  <Button
                    v-if="canSubmitBaselineChange(item)"
                    :loading="savingChangeWorkflowId === item.id"
                    size="small"
                    type="primary"
                    @click="handleCreateChangeApproval(item.id)"
                    >提交审批</Button
                  >
                  <Button
                    v-else-if="canApproveBaselineChange(item)"
                    :loading="savingChangeWorkflowId === item.id"
                    size="small"
                    type="primary"
                    @click="handleCreateChangeApproval(item.id)"
                    >通过</Button
                  >
                  <Button
                    v-else
                    disabled
                    size="small"
                    >{{ getBaselineChangeWorkflowStatus(item) }}</Button
                  >
                  <Button
                    v-if="canApproveBaselineChange(item)"
                    :loading="savingChangeWorkflowId === item.id"
                    size="small"
                    @click="handleWithdrawChangeApproval(item.id)"
                    >撤回</Button
                  >
                  <Button
                    v-if="canApproveBaselineChange(item)"
                    :loading="savingChangeWorkflowId === item.id"
                    danger
                    size="small"
                    @click="handleRejectChangeApproval(item.id)"
                    >驳回</Button
                  >
                </Space>
              </div>
            </article>
          </div>
        </Card>
      </Col>

      <Col :lg="10" :span="24">
        <Card
          :bordered="false"
          class="project-detail__inner-card rounded-2xl"
          title="当前基线"
        >
          <template #extra>
            <Button
              size="small"
              type="primary"
              @click="showBaselineForm = !showBaselineForm"
              >{{ showBaselineForm ? '收起' : '新建基线' }}</Button
            >
          </template>
          <div
            class="project-detail__overview-list project-detail__overview-list--metrics"
          >
            <div
              v-for="metric in activeBaselineSummary"
              :key="metric.label"
              class="project-detail__overview-item"
            >
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>
          <div
            v-if="detail?.activeBaseline"
            class="project-detail__summary-card"
          >
            <span>范围 / 进度基线</span>
            <p>{{ detail.activeBaseline.scopeBaselineSummary || '-' }}</p>
            <p>{{ detail.activeBaseline.scheduleBaselineSummary || '-' }}</p>
            <p class="project-detail__muted">成本基线仅保留历史展示，预算执行以「项目经营」为准：{{ detail.activeBaseline.costBaselineSummary || '-' }}</p>
          </div>
          <div v-if="showBaselineForm" class="project-detail__inline-form-card">
            <Form layout="vertical">
              <Form.Item label="来源基线变更（审批通过后才可选择）">
                <Select
                  v-model:value="baselineForm.createdFromChangeRequestId"
                  allow-clear
                  :options="baselineChangeOptions"
                  show-search
                />
              </Form.Item>
              <Row :gutter="12">
                <Col :span="12"
                  ><Form.Item label="BAC"
                    ><InputNumber
                      v-model:value="baselineForm.budgetAtCompletion"
                      :min="0"
                      style="width: 100%" /></Form.Item
                ></Col>
                <Col :span="12"
                  ><Form.Item label="生效日期"
                    ><Input
                      v-model:value="baselineForm.effectiveDate"
                      type="date" /></Form.Item
                ></Col>
              </Row>
              <Form.Item label="范围基线"
                ><Input.TextArea
                  v-model:value="baselineForm.scopeBaselineSummary"
                  :auto-size="{ minRows: 2, maxRows: 4 }"
              /></Form.Item>
              <Form.Item label="进度基线"
                ><Input.TextArea
                  v-model:value="baselineForm.scheduleBaselineSummary"
                  :auto-size="{ minRows: 2, maxRows: 4 }"
              /></Form.Item>
              <Form.Item label="成本基线（过渡保留，预算执行在项目经营维护）"
                ><Input.TextArea
                  v-model:value="baselineForm.costBaselineSummary"
                  :auto-size="{ minRows: 2, maxRows: 4 }"
              /></Form.Item>
              <Button
                :loading="savingBaseline"
                type="primary"
                @click="handleCreateBaseline"
                >保存基线</Button
              >
            </Form>
          </div>
        </Card>

        <Card
          :bordered="false"
          class="project-detail__inner-card rounded-2xl"
          title="项目绩效快照"
        >
          <template #extra>
            <Button
              size="small"
              type="primary"
              @click="
                showPerformanceSnapshotForm = !showPerformanceSnapshotForm
              "
            >
              {{ showPerformanceSnapshotForm ? '收起' : '生成快照' }}
            </Button>
          </template>
          <div
            class="project-detail__overview-list project-detail__overview-list--metrics"
          >
            <div
              v-for="metric in evmMetrics"
              :key="metric.label"
              class="project-detail__overview-item"
            >
              <span>{{ metric.label }}</span>
              <strong>{{ metric.value }}</strong>
            </div>
          </div>
          <div class="project-detail__muted project-detail__card-note">
            当前 PV / EV / AC 支持手工生成快照作为过渡；后续应优先从任务进度、预算、采购和工时自动汇总。
          </div>
          <div
            v-if="showPerformanceSnapshotForm"
            class="project-detail__inline-form-card"
          >
            <Form layout="vertical">
              <Row :gutter="12">
                <Col :span="12"
                  ><Form.Item label="统计日期"
                    ><Input
                      v-model:value="performanceSnapshotForm.snapshotDate"
                      type="date" /></Form.Item
                ></Col>
                <Col :span="12"
                  ><Form.Item label="统计粒度"
                    ><Select
                      v-model:value="performanceSnapshotForm.snapshotPeriod"
                      :options="[
                        { label: '周', value: 'Weekly' },
                        { label: '月', value: 'Monthly' },
                      ]" /></Form.Item
                ></Col>
              </Row>
              <Row :gutter="12">
                <Col :span="8"
                  ><Form.Item label="PV"
                    ><InputNumber
                      v-model:value="performanceSnapshotForm.plannedValue"
                      :min="0"
                      style="width: 100%" /></Form.Item
                ></Col>
                <Col :span="8"
                  ><Form.Item label="EV"
                    ><InputNumber
                      v-model:value="performanceSnapshotForm.earnedValue"
                      :min="0"
                      style="width: 100%" /></Form.Item
                ></Col>
                <Col :span="8"
                  ><Form.Item label="AC"
                    ><InputNumber
                      v-model:value="performanceSnapshotForm.actualCost"
                      :min="0"
                      style="width: 100%" /></Form.Item
                ></Col>
              </Row>
              <Button
                :loading="savingPerformanceSnapshot"
                type="primary"
                @click="handleCreatePerformanceSnapshot"
                >生成绩效快照</Button
              >
            </Form>
          </div>
          <div
            v-if="performanceTrendRows.length === 0"
            class="project-detail__muted"
          >
            暂无绩效快照。
          </div>
          <div v-else class="project-detail__stack-list">
            <article
              v-for="item in performanceTrendRows"
              :key="item.id"
              class="project-detail__stack-item"
            >
              <div class="project-detail__item-head">
                <Tag>{{ item.snapshotPeriod }}</Tag>
                <strong>{{ formatDate(item.snapshotDate) }}</strong>
              </div>
              <span
                >PV {{ formatMoney(item.plannedValue) }} / EV
                {{ formatMoney(item.earnedValue) }} / AC
                {{ formatMoney(item.actualCost) }}</span
              >
              <span
                >SPI {{ formatRatio(item.schedulePerformanceIndex) }} / CPI
                {{ formatRatio(item.costPerformanceIndex) }}</span
              >
            </article>
          </div>
        </Card>
      </Col>
    </Row>
  </section>
</template>
