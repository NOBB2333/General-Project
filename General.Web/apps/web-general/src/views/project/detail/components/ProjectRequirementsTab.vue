<script lang="ts" setup>
import { computed } from 'vue';

import {
  Button,
  Card,
  Checkbox,
  Col,
  Drawer,
  Form,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Tag,
  Tabs,
  Upload,
} from 'ant-design-vue';

import type { ProjectApi } from '#/api/core';

import { useProjectDetailContext } from '../context';
import ProjectTasksTab from './ProjectTasksTab.vue';

const {
  detail,
  editingRequirementId,
  filteredRequirements,
  formatHours,
  createRequirementFileUploadRequest,
  getPriorityColor,
  handleCancelRequirementEdit,
  handleCancelTaskFromRequirement,
  handleCreateTaskFromRequirement,
  handleCreateRequirementDraft,
  handleEditRequirement,
  handleOpenTaskFromRequirement,
  handleSaveRequirement,
  memberSelectOptions,
  milestoneSelectOptions,
  openProjectFilePreview,
  projectFiles,
  requirementForm,
  requirementMetrics,
  requirementPriorityOptions,
  requirementReplacementOptions,
  requirementSourceOptions,
  requirementStatusFilter,
  requirementStatusOptions,
  requirementTypeOptions,
  requirementWbsFilter,
  savingRequirement,
  savingTaskFromRequirement,
  splittingRequirementId,
  taskFromRequirementForm,
  wbsNodeOptions,
} = useProjectDetailContext();

const innerTab = defineModel<'requirements' | 'tasks'>('innerTab', {
  default: 'requirements',
});

function resolveRequirementTasks(requirementId: string) {
  return (detail.value?.tasks || []).filter(
    (item: ProjectApi.TaskItem) => item.requirementId === requirementId,
  );
}

function resolveRequirementFiles(requirementId: string) {
  return projectFiles.value.filter(
    (item) =>
      item.businessType === 'ProjectRequirement' &&
      item.businessId === requirementId,
  );
}

function resolveRequirementWorkHours(requirementId: string) {
  const tasks = resolveRequirementTasks(requirementId);
  return {
    actual: tasks.reduce((sum, item) => sum + (item.actualWorkHours || 0), 0),
    estimated: tasks.reduce(
      (sum, item) => sum + (item.estimatedWorkHours || 0),
      0,
    ),
  };
}

function buildRequirementDiff(
  current: ProjectApi.ProjectRequirement,
  predecessor?: null | ProjectApi.ProjectRequirement,
) {
  if (!predecessor) {
    return ['当前需求没有可对比的旧版本。'];
  }

  const diffs: string[] = [];
  if (predecessor.title !== current.title) {
    diffs.push(`标题：${predecessor.title || '-'} → ${current.title || '-'}`);
  }
  if ((predecessor.wbsNodeName || '') !== (current.wbsNodeName || '')) {
    diffs.push(`WBS：${predecessor.wbsNodeName || '-'} → ${current.wbsNodeName || '-'}`);
  }
  if ((predecessor.milestoneName || '') !== (current.milestoneName || '')) {
    diffs.push(`里程碑：${predecessor.milestoneName || '-'} → ${current.milestoneName || '-'}`);
  }
  if (predecessor.status !== current.status) {
    diffs.push(`状态：${predecessor.status || '-'} → ${current.status || '-'}`);
  }
  if (predecessor.priority !== current.priority) {
    diffs.push(`优先级：${predecessor.priority || '-'} → ${current.priority || '-'}`);
  }
  if (predecessor.acceptanceCriteria !== current.acceptanceCriteria) {
    diffs.push('验收标准已更新');
  }
  if (predecessor.description !== current.description) {
    diffs.push('需求描述已更新');
  }
  if (predecessor.changeReason !== current.changeReason) {
    diffs.push('变更原因已更新');
  }

  return diffs.length > 0 ? diffs : ['当前版本与旧版本未发现字段差异。'];
}

const requirementById = computed(() => {
  const map = new Map<string, ProjectApi.ProjectRequirement>();
  for (const item of detail.value?.requirements || []) {
    map.set(item.id, item);
  }
  return map;
});

const requirementVersionGroups = computed(() =>
  Array.from(
    (detail.value?.requirements || []).reduce((groups, item) => {
      const key = item.requirementCode || item.id;
      const list = groups.get(key) || [];
      list.push(item);
      groups.set(key, list);
      return groups;
    }, new Map<string, ProjectApi.ProjectRequirement[]>()),
    ([requirementCode, items]) => ({
      currentRequirement: items.find((item) => item.isCurrent) || items[0] || null,
      requirementCode,
      versions: [...items].sort(
        (left, right) =>
          right.versionNo - left.versionNo ||
          left.title.localeCompare(right.title, 'zh-CN'),
      ),
    }),
  ).sort((left, right) =>
    left.requirementCode.localeCompare(right.requirementCode, 'zh-CN', {
      numeric: true,
    }),
  ),
);

const requirementReplacementLinks = computed(() =>
  (detail.value?.requirements || [])
    .filter((item) => Boolean(item.replacesRequirementId))
    .map((item) => ({
      current: item,
      predecessor:
        requirementById.value.get(item.replacesRequirementId || '') || null,
    }))
    .sort((left, right) => {
      const leftCode = left.predecessor?.requirementCode || left.current.requirementCode;
      const rightCode = right.predecessor?.requirementCode || right.current.requirementCode;
      return (
        leftCode.localeCompare(rightCode, 'zh-CN', { numeric: true }) ||
        right.current.versionNo - left.current.versionNo
      );
    }),
);
</script>

<template>
  <section class="project-detail__section">
    <div class="project-detail__scope-note">
      <strong>需求与任务</strong>
      <span>需求维护业务能力、版本和验收口径；任务维护负责人、计划、工时和执行状态。两者通过需求、WBS 和里程碑追踪，不把需求正文塞进任务表单。</span>
      <span>状态边界：任务完成只回填任务进度和工时，不自动关闭需求；需求验收通过验收记录追踪，里程碑关闭仍在「目标与计划」按验收标准、质量检查和交付文件校验。</span>
    </div>

    <div class="project-detail__overview-list project-detail__overview-list--metrics">
      <div v-for="metric in requirementMetrics" :key="metric.label" class="project-detail__overview-item">
        <span>{{ metric.label }}</span>
        <strong>{{ metric.value }}</strong>
      </div>
    </div>

    <Tabs v-model:activeKey="innerTab" class="project-detail__inner-tabs" size="small">
      <Tabs.TabPane key="requirements" tab="需求" />
      <Tabs.TabPane key="tasks" tab="任务" />
    </Tabs>

    <Row v-if="innerTab === 'requirements'" :gutter="[16, 16]">
      <Col :lg="10" :span="24">
        <Card :bordered="false" class="project-detail__inner-card rounded-2xl" :title="editingRequirementId ? '编辑需求' : '新增需求'">
          <Form class="project-detail__form" layout="vertical">
            <Row :gutter="12">
              <Col :lg="8" :span="24">
                <Form.Item label="需求编号">
                  <Input v-model:value="requirementForm.requirementCode" :maxlength="64" placeholder="REQ-001" />
                </Form.Item>
              </Col>
              <Col :lg="16" :span="24">
                <Form.Item label="需求标题">
                  <Input v-model:value="requirementForm.title" :maxlength="256" />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="所属 WBS">
                  <Select v-model:value="requirementForm.wbsNodeId" allow-clear :options="wbsNodeOptions" show-search />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="关联里程碑">
                  <Select v-model:value="requirementForm.milestoneId" allow-clear :options="milestoneSelectOptions" show-search />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="状态">
                  <Select v-model:value="requirementForm.status" :options="requirementStatusOptions" />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="优先级">
                  <Select v-model:value="requirementForm.priority" :options="requirementPriorityOptions" />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="类型">
                  <Select v-model:value="requirementForm.type" :options="requirementTypeOptions" />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="来源">
                  <Select v-model:value="requirementForm.source" :options="requirementSourceOptions" />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="版本">
                  <InputNumber v-model:value="requirementForm.versionNo" :min="1" style="width: 100%" />
                </Form.Item>
              </Col>
              <Col :lg="8" :span="12">
                <Form.Item label="规模估算">
                  <InputNumber v-model:value="requirementForm.sizeEstimate" :min="0" style="width: 100%" />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="业务负责人">
                  <Select v-model:value="requirementForm.businessOwnerUserId" allow-clear :options="memberSelectOptions" show-search />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="产品负责人">
                  <Select v-model:value="requirementForm.productOwnerUserId" allow-clear :options="memberSelectOptions" show-search />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Form.Item label="替代原需求">
                  <Select v-model:value="requirementForm.replacesRequirementId" allow-clear :options="requirementReplacementOptions" show-search />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Form.Item label="需求描述">
                  <Input.TextArea v-model:value="requirementForm.description" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="4096" />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Form.Item label="验收标准">
                  <Input.TextArea v-model:value="requirementForm.acceptanceCriteria" :auto-size="{ minRows: 2, maxRows: 5 }" :maxlength="2048" />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="变更原因">
                  <Input.TextArea v-model:value="requirementForm.changeReason" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="1024" />
                </Form.Item>
              </Col>
              <Col :lg="12" :span="24">
                <Form.Item label="影响说明">
                  <Input.TextArea v-model:value="requirementForm.impactSummary" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="2048" />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Checkbox v-model:checked="requirementForm.isCurrent">当前有效版本</Checkbox>
              </Col>
            </Row>
            <Space>
              <Button :loading="savingRequirement" type="primary" @click="handleSaveRequirement">
                {{ editingRequirementId ? '保存需求' : '创建需求' }}
              </Button>
              <Button @click="handleCancelRequirementEdit">清空</Button>
            </Space>
          </Form>
        </Card>
      </Col>

      <Col :lg="14" :span="24">
        <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="需求列表">
          <div class="project-detail__toolbar">
            <Space wrap>
              <span class="project-detail__toolbar-label">状态</span>
              <Select v-model:value="requirementStatusFilter" allow-clear :options="requirementStatusOptions" placeholder="全部状态" style="width: 140px" />
            </Space>
            <Space wrap>
              <span class="project-detail__toolbar-label">WBS</span>
              <Select v-model:value="requirementWbsFilter" allow-clear :options="wbsNodeOptions" placeholder="全部 WBS" show-search style="width: 220px" />
              <Button size="small" @click="handleCreateRequirementDraft(requirementWbsFilter || undefined)">按当前 WBS 新建</Button>
            </Space>
          </div>

          <div v-if="filteredRequirements.length === 0" class="project-detail__muted">当前筛选下暂无需求。</div>
          <div v-else class="project-detail__stack-list">
            <article v-for="item in filteredRequirements" :key="item.id" class="project-detail__stack-item">
              <div class="project-detail__item-head">
                <Tag :color="getPriorityColor(item.priority)">{{ item.priority }}</Tag>
                <Tag>{{ item.status }}</Tag>
                <Tag v-if="item.isCurrent" color="green">当前版本</Tag>
                <Tag>V{{ item.versionNo }}</Tag>
              </div>
              <div class="project-detail__requirement-row">
                <div>
                  <strong>{{ item.requirementCode }} · {{ item.title }}</strong>
                  <p>{{ item.description || '暂无需求描述' }}</p>
                </div>
                <Space>
                  <Button size="small" type="link" @click="handleOpenTaskFromRequirement(item)">拆任务</Button>
                  <Button size="small" type="link" @click="handleEditRequirement(item)">编辑</Button>
                </Space>
              </div>
              <div class="project-detail__knowledge-meta">
                <span>{{ item.type }} / {{ item.source }}</span>
                <span>{{ item.wbsNodeName || '未关联 WBS' }}</span>
                <span>{{ item.milestoneName || '未关联里程碑' }}</span>
                <span>{{ item.productOwnerName || '未指定产品负责人' }}</span>
                <span>任务 {{ resolveRequirementTasks(item.id).length }}</span>
                <span>
                  预计 {{ formatHours(resolveRequirementWorkHours(item.id).estimated) }} /
                  实际 {{ formatHours(resolveRequirementWorkHours(item.id).actual) }}
                </span>
              </div>
              <div v-if="resolveRequirementTasks(item.id).length > 0" class="project-detail__tag-list">
                <Tag v-for="task in resolveRequirementTasks(item.id)" :key="task.id">
                  {{ task.taskCode }} · {{ task.title }}
                </Tag>
              </div>
              <div class="project-detail__requirement-files">
                <Upload :custom-request="createRequirementFileUploadRequest(item)" :show-upload-list="false">
                  <Button size="small">上传附件</Button>
                </Upload>
                <div v-if="resolveRequirementFiles(item.id).length === 0" class="project-detail__muted">
                  暂无需求附件。
                </div>
                <div v-else class="project-detail__tag-list">
                  <Tag v-for="file in resolveRequirementFiles(item.id)" :key="file.id" @click="openProjectFilePreview(file)">
                    {{ file.fileName }}
                  </Tag>
                </div>
              </div>
              <div v-if="item.acceptanceCriteria" class="project-detail__muted">验收标准：{{ item.acceptanceCriteria }}</div>
              <div v-if="item.replacesRequirementId" class="project-detail__muted">
                替代：{{ item.replacesRequirementTitle || item.replacesRequirementId }}
              </div>
            </article>
          </div>
        </Card>

        <Card :bordered="false" class="project-detail__inner-card rounded-2xl" title="版本历史 / 替代链">
          <div class="project-detail__muted project-detail__card-note">
            同编号版本按需求编号聚合，替代链按“新需求替代旧需求”展示，方便追踪需求变更前后关系。
          </div>

          <div v-if="requirementVersionGroups.length === 0" class="project-detail__muted">
            当前没有需求版本历史。
          </div>
          <div v-else class="project-detail__stack-list">
            <article v-for="group in requirementVersionGroups" :key="group.requirementCode" class="project-detail__stack-item">
              <div class="project-detail__item-head">
                <Tag color="blue">{{ group.requirementCode }}</Tag>
                <Tag>{{ group.versions.length }} 个版本</Tag>
                <Tag v-if="group.currentRequirement" color="green">
                  当前 V{{ group.currentRequirement.versionNo }}
                </Tag>
              </div>
              <div class="project-detail__knowledge-meta">
                <span v-for="version in group.versions" :key="version.id">
                  V{{ version.versionNo }} / {{ version.status }}
                  <template v-if="version.isCurrent"> / 当前</template>
                </span>
              </div>
              <div class="project-detail__muted">
                最新标题：{{ group.versions[0]?.title || '-' }}，
                最新 WBS：{{ group.versions[0]?.wbsNodeName || '未关联 WBS' }}
              </div>
            </article>
          </div>

          <div class="project-detail__muted project-detail__card-note">替代链</div>
          <div v-if="requirementReplacementLinks.length === 0" class="project-detail__muted">
            当前没有替代关系。
          </div>
          <div v-else class="project-detail__stack-list">
            <article v-for="link in requirementReplacementLinks" :key="link.current.id" class="project-detail__stack-item">
              <div class="project-detail__item-head">
                <Tag>
                  {{ link.predecessor?.requirementCode || '旧需求' }}
                  V{{ link.predecessor?.versionNo || '-' }}
                </Tag>
                <Tag color="orange">替代</Tag>
                <Tag color="green">
                  {{ link.current.requirementCode }} V{{ link.current.versionNo }}
                </Tag>
              </div>
              <strong>{{ link.current.title }}</strong>
              <div class="project-detail__knowledge-meta">
                <span>{{ link.predecessor?.status || '未知状态' }}</span>
                <span>{{ link.current.status }}</span>
                <span>{{ link.current.wbsNodeName || '未关联 WBS' }}</span>
                <span>任务 {{ resolveRequirementTasks(link.current.id).length }}</span>
                <span>
                  预计 {{ formatHours(resolveRequirementWorkHours(link.current.id).estimated) }} /
                  实际 {{ formatHours(resolveRequirementWorkHours(link.current.id).actual) }}
                </span>
              </div>
              <div class="project-detail__muted">
                变更差异：
                <Tag v-for="diff in buildRequirementDiff(link.current, link.predecessor)" :key="diff">
                  {{ diff }}
                </Tag>
              </div>
              <div class="project-detail__muted">
                替代原因：{{ link.current.changeReason || '未填写' }}
              </div>
            </article>
          </div>
        </Card>
      </Col>
    </Row>

    <ProjectTasksTab v-else />

    <Drawer
      :open="Boolean(splittingRequirementId)"
      title="从需求创建任务"
      destroy-on-close
      width="720"
      @close="handleCancelTaskFromRequirement"
    >
      <Form class="project-detail__form" layout="vertical">
        <Row :gutter="12">
          <Col :lg="10" :span="24">
            <Form.Item label="任务编号">
              <Input v-model:value="taskFromRequirementForm.taskCode" :maxlength="64" placeholder="自动生成" />
            </Form.Item>
          </Col>
          <Col :lg="14" :span="24">
            <Form.Item label="任务标题">
              <Input v-model:value="taskFromRequirementForm.title" :maxlength="256" />
            </Form.Item>
          </Col>
          <Col :lg="12" :span="24">
            <Form.Item label="负责人">
              <Select v-model:value="taskFromRequirementForm.ownerUserId" :options="memberSelectOptions" show-search />
            </Form.Item>
          </Col>
          <Col :lg="12" :span="24">
            <Form.Item label="优先级">
              <Select v-model:value="taskFromRequirementForm.priority" :options="requirementPriorityOptions" />
            </Form.Item>
          </Col>
          <Col :lg="8" :span="24">
            <Form.Item label="预计工时">
              <InputNumber v-model:value="taskFromRequirementForm.estimatedWorkHours" :min="0" style="width: 100%" />
            </Form.Item>
          </Col>
          <Col :lg="8" :span="24">
            <Form.Item label="计划开始">
              <Input v-model:value="taskFromRequirementForm.plannedStartTime" type="date" />
            </Form.Item>
          </Col>
          <Col :lg="8" :span="24">
            <Form.Item label="计划完成">
              <Input v-model:value="taskFromRequirementForm.plannedEndTime" type="date" />
            </Form.Item>
          </Col>
        </Row>
        <div class="project-detail__muted">
          任务会自动关联当前需求；如果需求已关联 WBS，任务会继承该 WBS。
        </div>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="handleCancelTaskFromRequirement">取消</Button>
          <Button :loading="savingTaskFromRequirement" type="primary" @click="handleCreateTaskFromRequirement">创建任务</Button>
        </div>
      </template>
    </Drawer>
  </section>
</template>
