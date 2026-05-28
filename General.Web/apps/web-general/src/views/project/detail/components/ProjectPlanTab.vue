<script lang="ts" setup>
import { Button, Card, Col, Form, Input, InputNumber, Progress, Row, Select, Table, Tag } from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';

const {
  cycleColumns,
  dependencyTypeOptions,
  detail,
  formatDate,
  handleCreateTaskDependency,
  handleUpdateTaskWbs,
  loading,
  milestoneColumns,
  reportColumns,
  reportItems,
  savingTaskDependency,
  savingTaskWbsId,
  taskDependencyForm,
  taskParentOptions,
  taskSelectOptions,
  taskWbsRows,
} = useProjectDetailContext();
</script>

<template>
<section class="project-detail__section">
  <Row :gutter="[16, 16]">
    <Col :lg="14" :span="24">
      <Card :bordered="false" class="project-detail__inner-card" title="阶段 / 周期 / 发布计划">
        <Table
          :columns="cycleColumns"
          :data-source="detail?.cycles || []"
          :loading="loading"
          :pagination="false"
          row-key="id"
          size="small"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'progress'">
              <Progress :percent="(record as ProjectApi.CycleItem).progress" size="small" />
            </template>
            <template v-else-if="column.key === 'endDate'">
              {{ formatDate((record as ProjectApi.CycleItem).endDate) }}
            </template>
          </template>
        </Table>
      </Card>
    </Col>
    <Col :lg="10" :span="24">
      <Card :bordered="false" class="project-detail__inner-card" title="里程碑">
        <Table
          :columns="milestoneColumns"
          :data-source="detail?.milestones || []"
          :loading="loading"
          :pagination="false"
          row-key="id"
          size="small"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'plannedCompletionDate'">
              {{ formatDate((record as ProjectApi.WorkspaceMilestoneItem).plannedCompletionDate) }}
            </template>
          </template>
        </Table>
      </Card>
    </Col>
  </Row>

  <Card :bordered="false" class="project-detail__inner-card" title="日报 / 周报 / 月报">
    <div v-if="reportItems.length === 0" class="project-detail__muted">
      当前未归档日报 / 周报 / 月报，后续可直接关联项目文档与周期。
    </div>
    <Table
      v-else
      :columns="reportColumns"
      :data-source="reportItems"
      :pagination="false"
      row-key="id"
      size="small"
    />
  </Card>

  <Card :bordered="false" class="project-detail__inner-card" title="WBS 任务拆解 / 依赖关系">
    <div class="project-detail__overview-list project-detail__overview-list--metrics">
      <div class="project-detail__overview-item">
        <span>WBS 编码任务</span>
        <strong>{{ (detail?.tasks || []).filter((item: ProjectApi.TaskItem) => item.wbsCode).length }}</strong>
      </div>
      <div class="project-detail__overview-item">
        <span>父子任务</span>
        <strong>{{ (detail?.tasks || []).filter((item: ProjectApi.TaskItem) => item.parentTaskId).length }}</strong>
      </div>
      <div class="project-detail__overview-item">
        <span>依赖关系</span>
        <strong>{{ detail?.taskDependencies.length || 0 }}</strong>
      </div>
    </div>
    <Form class="project-detail__inline-form" layout="vertical">
      <Row :gutter="[12, 0]">
        <Col :span="12">
          <Form.Item label="前置任务">
            <Select v-model:value="taskDependencyForm.predecessorTaskId" :options="taskSelectOptions" show-search />
          </Form.Item>
        </Col>
        <Col :span="12">
          <Form.Item label="后续任务">
            <Select v-model:value="taskDependencyForm.successorTaskId" :options="taskSelectOptions" show-search />
          </Form.Item>
        </Col>
        <Col :span="12">
          <Form.Item label="依赖类型">
            <Select v-model:value="taskDependencyForm.dependencyType" :options="dependencyTypeOptions" />
          </Form.Item>
        </Col>
        <Col :span="12">
          <Form.Item label="操作">
            <Button :loading="savingTaskDependency" type="primary" @click="handleCreateTaskDependency">新增依赖</Button>
          </Form.Item>
        </Col>
      </Row>
    </Form>
    <div class="project-detail__stack-list">
      <article v-for="{ form, item } in taskWbsRows.slice(0, 8)" :key="item.id" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag>{{ item.status }}</Tag>
          <strong>{{ item.title }}</strong>
        </div>
        <div class="project-detail__wbs-grid">
          <Form.Item label="WBS 编码">
            <Input :value="form.wbsCode" :maxlength="64" @update:value="(val) => { form.wbsCode = val; }" />
          </Form.Item>
          <Form.Item label="父任务">
            <Select
              :value="form.parentTaskId"
              allow-clear
              :options="taskParentOptions.filter((option: { value: string }) => option.value !== item.id)"
              show-search
              @update:value="(val) => { form.parentTaskId = typeof val === 'string' ? val : undefined; }"
            />
          </Form.Item>
          <Form.Item label="排序">
            <InputNumber :value="form.sortOrder" :min="0" @update:value="(val) => { form.sortOrder = typeof val === 'number' ? val : 0; }" />
          </Form.Item>
          <Form.Item label="基线开始">
            <Input :value="form.baselineStartTime" type="date" @update:value="(val) => { form.baselineStartTime = val; }" />
          </Form.Item>
          <Form.Item label="基线结束">
            <Input :value="form.baselineEndTime" type="date" @update:value="(val) => { form.baselineEndTime = val; }" />
          </Form.Item>
        </div>
        <Button :loading="savingTaskWbsId === item.id" size="small" type="primary" @click="handleUpdateTaskWbs(item.id)">保存 WBS</Button>
      </article>
    </div>
    <div v-if="(detail?.taskDependencies || []).length === 0" class="project-detail__muted">暂无任务依赖。</div>
    <div v-else class="project-detail__stack-list">
      <article v-for="item in detail?.taskDependencies || []" :key="item.id" class="project-detail__stack-item">
        <strong>{{ item.predecessorTaskTitle }} -> {{ item.successorTaskTitle }}</strong>
        <span>{{ item.dependencyType }}</span>
      </article>
    </div>
    <div v-if="(detail?.taskScheduleImpacts || []).length > 0" class="project-detail__stack-list">
      <article v-for="item in detail?.taskScheduleImpacts || []" :key="`${item.predecessorTaskId}-${item.successorTaskId}`" class="project-detail__stack-item">
        <div class="project-detail__item-head">
          <Tag color="error">影响识别</Tag>
          <strong>{{ item.successorTaskTitle }}</strong>
        </div>
        <span>{{ item.predecessorTaskTitle }} -> {{ item.successorTaskTitle }}</span>
        <span>{{ item.impactReason }}</span>
      </article>
    </div>
  </Card>
</section>
</template>
