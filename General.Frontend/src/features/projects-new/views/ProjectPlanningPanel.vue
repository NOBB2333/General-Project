<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  createProjectBaselineApi,
  createProjectMilestoneApi,
  createProjectSprintApi,
  createProjectWbsNodeApi,
  deleteProjectMilestoneApi,
  deleteProjectSprintApi,
  deleteProjectWbsNodeApi,
  getProjectDetailSectionApi,
  updateProjectMilestoneApi,
  updateProjectSprintApi,
  updateProjectWbsNodeApi,
  type ProjectApi,
} from '@/api/core/project';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

type PlanningKind = 'baseline' | 'milestone' | 'sprint' | 'wbs';
type DeleteTarget = { id: string; kind: Exclude<PlanningKind, 'baseline'>; name: string };

const props = defineProps<{ detail: ProjectApi.Detail; projectId: string }>();
const emit = defineEmits<{ changed: [] }>();

const dialogKind = ref<PlanningKind | null>(null);
const editingId = ref('');
const loadingBaselineChanges = ref(false);
const saving = ref(false);
const error = ref('');
const notice = ref('');
const approvedBaselineChanges = ref<ProjectApi.ChangeRequest[]>([]);
const deleteTarget = ref<DeleteTarget | null>(null);

const wbsForm = reactive({
  deliveryDescription: '',
  estimateSummary: '',
  milestoneId: '',
  name: '',
  ownerUserId: '',
  parentId: '',
  scopeDescription: '',
  sortOrder: '0',
  sprintId: '',
  status: '规划中',
  type: '工作包',
  wbsCode: '',
});

const sprintForm = reactive({
  endDate: '',
  name: '',
  ownerUserId: '',
  progress: '0',
  startDate: '',
  status: '未开始',
  summary: '',
});

const milestoneForm = reactive({
  actualCompletionDate: '',
  name: '',
  ownerUserId: '',
  plannedCompletionDate: '',
  sprintId: '',
  status: '未开始',
});

const baselineForm = reactive({
  budgetAtCompletion: '',
  costBaselineSummary: '',
  createdFromChangeRequestId: '',
  effectiveDate: localDateInput(new Date()),
  scheduleBaselineSummary: '',
  scopeBaselineSummary: '',
});

const canManagePlan = computed(() => hasAccessCode('Project.Task.Manage'));
const canManageGovernance = computed(() => hasAccessCode('Project.Governance.Manage'));
const canViewSensitiveAmounts = computed(() => hasAccessCode('Business.Budget.Sensitive'));
const canCreateBaseline = computed(() => canManageGovernance.value && canViewSensitiveAmounts.value);
const activeBaseline = computed(() => props.detail.activeBaseline);

const activeMemberOptions = computed<UiSelectOption[]>(() => {
  const seen = new Set<string>();
  return props.detail.members
    .filter((member) => member.isActive && !seen.has(member.userId) && Boolean(seen.add(member.userId)))
    .map((member) => ({ value: member.userId, label: member.userName }));
});

const sprintOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不关联 Sprint' },
  ...props.detail.sprints.map((sprint) => ({ value: sprint.id, label: `${sprint.sprintCode} · ${sprint.name}` })),
]);

const milestoneOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不关联里程碑' },
  ...props.detail.milestones.map((milestone) => ({ value: milestone.id, label: milestone.name })),
]);

const wbsTreeRows = computed(() => {
  const sorted = [...props.detail.wbsNodes].sort(
    (left, right) => left.sortOrder - right.sortOrder || left.wbsCode.localeCompare(right.wbsCode, 'zh-CN'),
  );
  const children = new Map<string, ProjectApi.WbsNode[]>();
  for (const node of sorted) {
    if (!node.parentId) continue;
    const siblings = children.get(node.parentId) ?? [];
    siblings.push(node);
    children.set(node.parentId, siblings);
  }

  const rows: Array<{ depth: number; node: ProjectApi.WbsNode }> = [];
  const visited = new Set<string>();
  const append = (node: ProjectApi.WbsNode, depth: number) => {
    if (visited.has(node.id)) return;
    visited.add(node.id);
    rows.push({ depth, node });
    for (const child of children.get(node.id) ?? []) append(child, depth + 1);
  };

  for (const node of sorted) {
    if (!node.parentId || !props.detail.wbsNodes.some((parent) => parent.id === node.parentId)) append(node, 0);
  }
  for (const node of sorted) append(node, 0);
  return rows;
});

const parentOptions = computed<UiSelectOption[]>(() => {
  const excludedIds = new Set<string>();
  if (editingId.value) {
    const addDescendants = (parentId: string) => {
      for (const child of props.detail.wbsNodes.filter((node) => node.parentId === parentId)) {
        if (excludedIds.has(child.id)) continue;
        excludedIds.add(child.id);
        addDescendants(child.id);
      }
    };
    excludedIds.add(editingId.value);
    addDescendants(editingId.value);
  }

  return [
    { value: '', label: '顶层节点' },
    ...props.detail.wbsNodes
      .filter((node) => !excludedIds.has(node.id))
      .map((node) => ({ value: node.id, label: `${node.wbsCode} · ${node.name}` })),
  ];
});

const wbsTypeOptions = computed(() => withCurrentOptions(['阶段', '可交付成果', '工作包'], wbsForm.type));
const wbsStatusOptions = computed(() =>
  withCurrentOptions(['规划中', '未开始', '进行中', '已完成', '已关闭'], wbsForm.status),
);
const sprintStatusOptions = computed(() =>
  withCurrentOptions(['未开始', '规划中', '进行中', '已完成', '已关闭'], sprintForm.status),
);
const changeRequestOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '选择已批准的范围、进度或成本变更' },
  ...approvedBaselineChanges.value.map((change) => ({
    value: change.id,
    label: `${change.requestCode} · ${change.title}`,
  })),
]);

const activeMemberRequired = computed(() => activeMemberOptions.value.length > 0);
const currentDialogTitle = computed(() => {
  if (dialogKind.value === 'wbs') return editingId.value ? '编辑 WBS 节点' : '新增 WBS 节点';
  if (dialogKind.value === 'sprint') return editingId.value ? '编辑 Sprint' : '新增 Sprint';
  if (dialogKind.value === 'milestone') return editingId.value ? '编辑里程碑' : '新增里程碑';
  if (dialogKind.value === 'baseline') return activeBaseline.value ? '建立下一版基线' : '建立项目基线';
  return '';
});

function withCurrentOptions(values: string[], currentValue: string): UiSelectOption[] {
  const uniqueValues = [...new Set([...values, currentValue].filter(Boolean))];
  return uniqueValues.map((value) => ({ value, label: value }));
}

function ownerOptions(selectedUserId: string, selectedUserName: string): UiSelectOption[] {
  const options = [...activeMemberOptions.value];
  if (selectedUserId && !options.some((option) => option.value === selectedUserId)) {
    options.unshift({ value: selectedUserId, label: selectedUserName || '当前负责人' });
  }
  return [{ value: '', label: '请选择项目成员' }, ...options];
}

function localDateInput(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function toDateTime(value: string): null | string {
  return value ? `${value}T00:00:00` : null;
}

function displayDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}

function closeForm() {
  dialogKind.value = null;
  editingId.value = '';
  error.value = '';
}

function openWbsForm(node?: ProjectApi.WbsNode) {
  error.value = '';
  notice.value = '';
  editingId.value = node?.id ?? '';
  Object.assign(wbsForm, {
    deliveryDescription: node?.deliveryDescription ?? '',
    estimateSummary: node?.estimateSummary ?? '',
    milestoneId: node?.milestoneId ?? '',
    name: node?.name ?? '',
    ownerUserId: node?.ownerUserId ?? '',
    parentId: node?.parentId ?? '',
    scopeDescription: node?.scopeDescription ?? '',
    sortOrder: String(node?.sortOrder ?? props.detail.wbsNodes.length * 10),
    sprintId: node?.sprintId ?? '',
    status: node?.status ?? '规划中',
    type: node?.type ?? '工作包',
    wbsCode: node?.wbsCode ?? '',
  });
  dialogKind.value = 'wbs';
}

function openSprintForm(sprint?: ProjectApi.SprintItem) {
  error.value = '';
  notice.value = '';
  editingId.value = sprint?.id ?? '';
  Object.assign(sprintForm, {
    endDate: sprint?.endDate?.slice(0, 10) ?? '',
    name: sprint?.name ?? '',
    ownerUserId: sprint?.ownerUserId ?? '',
    progress: String(sprint?.progress ?? 0),
    startDate: sprint?.startDate?.slice(0, 10) ?? '',
    status: sprint?.status ?? '未开始',
    summary: sprint?.summary ?? '',
  });
  dialogKind.value = 'sprint';
}

function openMilestoneForm(milestone?: ProjectApi.MilestoneItem) {
  error.value = '';
  notice.value = '';
  editingId.value = milestone?.id ?? '';
  Object.assign(milestoneForm, {
    actualCompletionDate: milestone?.actualCompletionDate?.slice(0, 10) ?? '',
    name: milestone?.name ?? '',
    ownerUserId: milestone?.ownerUserId ?? '',
    plannedCompletionDate: milestone?.plannedCompletionDate.slice(0, 10) ?? '',
    sprintId: milestone?.sprintId ?? '',
    status: milestone?.status ?? '未开始',
  });
  dialogKind.value = 'milestone';
}

async function openBaselineForm() {
  if (!canCreateBaseline.value) return;
  error.value = '';
  notice.value = '';
  editingId.value = '';
  approvedBaselineChanges.value = [];
  baselineForm.budgetAtCompletion = '';
  baselineForm.costBaselineSummary = '';
  baselineForm.createdFromChangeRequestId = '';
  baselineForm.effectiveDate = localDateInput(new Date());
  baselineForm.scheduleBaselineSummary = '';
  baselineForm.scopeBaselineSummary = '';
  loadingBaselineChanges.value = true;
  try {
    const control = await getProjectDetailSectionApi('control', props.projectId);
    approvedBaselineChanges.value = control.changeRequests.filter(
      (change) => change.status === '已批准' && ['范围', '进度', '成本'].includes(change.changeType),
    );
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '已批准变更读取失败，请重试。';
    loadingBaselineChanges.value = false;
    return;
  }
  loadingBaselineChanges.value = false;
  dialogKind.value = 'baseline';
}

async function runAction(action: () => Promise<unknown>, successMessage: string): Promise<boolean> {
  saving.value = true;
  error.value = '';
  notice.value = '';
  try {
    await action();
    notice.value = successMessage;
    emit('changed');
    return true;
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目计划保存失败，请重试。';
    return false;
  } finally {
    saving.value = false;
  }
}

async function saveWbs() {
  if (!wbsForm.wbsCode.trim() || !wbsForm.name.trim()) {
    error.value = '请填写 WBS 编码和名称。';
    return;
  }

  const input: ProjectApi.WbsNodeSaveInput = {
    deliveryDescription: wbsForm.deliveryDescription.trim(),
    estimateSummary: wbsForm.estimateSummary.trim(),
    milestoneId: wbsForm.milestoneId || null,
    name: wbsForm.name.trim(),
    ownerUserId: wbsForm.ownerUserId || null,
    parentId: wbsForm.parentId || null,
    scopeDescription: wbsForm.scopeDescription.trim(),
    sortOrder: Number(wbsForm.sortOrder) || 0,
    sprintId: wbsForm.sprintId || null,
    status: wbsForm.status,
    type: wbsForm.type,
    wbsCode: wbsForm.wbsCode.trim(),
  };
  const saved = await runAction(
    () =>
      editingId.value
        ? updateProjectWbsNodeApi(editingId.value, input)
        : createProjectWbsNodeApi(props.projectId, input),
    editingId.value ? 'WBS 节点已更新。' : 'WBS 节点已创建。',
  );
  if (saved) closeForm();
}

async function saveSprint() {
  if (!sprintForm.name.trim() || !sprintForm.ownerUserId) {
    error.value = '请填写 Sprint 名称并选择负责人。';
    return;
  }
  if (sprintForm.startDate && sprintForm.endDate && sprintForm.startDate > sprintForm.endDate) {
    error.value = 'Sprint 开始日期不能晚于结束日期。';
    return;
  }

  const input: ProjectApi.SprintSaveInput = {
    endDate: toDateTime(sprintForm.endDate),
    name: sprintForm.name.trim(),
    ownerUserId: sprintForm.ownerUserId,
    progress: Math.max(0, Math.min(100, Number(sprintForm.progress) || 0)),
    startDate: toDateTime(sprintForm.startDate),
    status: sprintForm.status,
    summary: sprintForm.summary.trim(),
  };
  const saved = await runAction(
    () =>
      editingId.value ? updateProjectSprintApi(editingId.value, input) : createProjectSprintApi(props.projectId, input),
    editingId.value ? 'Sprint 已更新。' : 'Sprint 已创建。',
  );
  if (saved) closeForm();
}

async function saveMilestone() {
  if (!milestoneForm.name.trim() || !milestoneForm.ownerUserId || !milestoneForm.plannedCompletionDate) {
    error.value = '请填写里程碑名称、负责人和计划完成日期。';
    return;
  }

  const input: ProjectApi.MilestoneSaveInput = {
    actualCompletionDate: toDateTime(milestoneForm.actualCompletionDate),
    name: milestoneForm.name.trim(),
    ownerUserId: milestoneForm.ownerUserId,
    plannedCompletionDate: `${milestoneForm.plannedCompletionDate}T00:00:00`,
    sprintId: milestoneForm.sprintId || null,
    status: milestoneForm.status,
  };
  const saved = await runAction(
    () =>
      editingId.value
        ? updateProjectMilestoneApi(editingId.value, input)
        : createProjectMilestoneApi(props.projectId, input),
    editingId.value ? '里程碑已更新。' : '里程碑已创建。',
  );
  if (saved) closeForm();
}

async function saveBaseline() {
  const budget = Number(baselineForm.budgetAtCompletion);
  if (!Number.isFinite(budget) || budget < 0) {
    error.value = '请填写不小于 0 的完工预算。';
    return;
  }
  if (
    !baselineForm.effectiveDate ||
    !baselineForm.scopeBaselineSummary.trim() ||
    !baselineForm.scheduleBaselineSummary.trim()
  ) {
    error.value = '请填写生效日期、范围基线和进度基线摘要。';
    return;
  }
  if (activeBaseline.value && !baselineForm.createdFromChangeRequestId) {
    error.value = '后续基线必须关联本项目已批准的范围、进度或成本变更。';
    return;
  }

  const input: ProjectApi.BaselineSaveInput = {
    budgetAtCompletion: budget,
    costBaselineSummary: baselineForm.costBaselineSummary.trim(),
    createdFromChangeRequestId: baselineForm.createdFromChangeRequestId || null,
    effectiveDate: `${baselineForm.effectiveDate}T00:00:00`,
    isActive: true,
    scheduleBaselineSummary: baselineForm.scheduleBaselineSummary.trim(),
    scopeBaselineSummary: baselineForm.scopeBaselineSummary.trim(),
  };
  const saved = await runAction(() => createProjectBaselineApi(props.projectId, input), '活动基线已建立。');
  if (saved) closeForm();
}

function requestDelete(kind: DeleteTarget['kind'], id: string, name: string) {
  deleteTarget.value = { id, kind, name };
}

async function confirmDelete() {
  const target = deleteTarget.value;
  if (!target) return;
  const deleteRequest =
    target.kind === 'wbs'
      ? deleteProjectWbsNodeApi(target.id)
      : target.kind === 'sprint'
        ? deleteProjectSprintApi(target.id)
        : deleteProjectMilestoneApi(target.id);
  const removed = await runAction(() => deleteRequest, `${target.name} 已删除。`);
  if (removed) deleteTarget.value = null;
}
</script>

<template>
  <section
    :class="$style.planGrid"
    aria-label="项目计划管理"
  >
    <p
      v-if="error && !dialogKind"
      role="alert"
      :class="$style.feedbackError"
    >
      {{ error }}
    </p>
    <p
      v-if="notice"
      role="status"
      :class="$style.feedbackSuccess"
    >
      {{ notice }}
    </p>

    <section
      :class="[$style.section, $style.fullWidth]"
      aria-label="WBS 工作分解结构"
    >
      <header :class="$style.sectionHeader">
        <div>
          <h3 :class="$style.heading">WBS 工作分解结构</h3>
          <p :class="$style.description">按父节点展示层级；同级顺序通过排序值保存。</p>
        </div>
        <UiButton
          v-if="canManagePlan"
          variant="primary"
          :disabled="saving"
          @click="openWbsForm()"
        >
          新增节点
        </UiButton>
      </header>
      <div
        v-if="wbsTreeRows.length"
        :class="$style.itemList"
      >
        <article
          v-for="row in wbsTreeRows"
          :key="row.node.id"
          :class="$style.item"
        >
          <div :class="$style.itemMain">
            <div :class="$style.itemTitleRow">
              <span
                v-if="row.depth"
                :class="$style.treeMarker"
                aria-hidden="true"
              >
                {{ `${'　'.repeat(row.depth)}└` }}
              </span>
              <span :class="$style.code">{{ row.node.wbsCode }}</span>
              <h4 :class="$style.itemTitle">{{ row.node.name }}</h4>
              <span :class="$style.status">{{ row.node.status }}</span>
            </div>
            <p :class="$style.meta">
              {{ row.node.type }} · 负责人 {{ row.node.ownerName || '未分配' }} · {{ row.node.taskCount }} 个任务 ·
              {{ row.node.requirementCount }} 个需求
            </p>
            <p :class="$style.meta">
              Sprint：{{ row.node.sprintName || '—' }} · 里程碑：{{ row.node.milestoneName || '—' }} · 完成率
              {{ row.node.completionRate }}%
            </p>
            <p
              v-if="row.node.scopeDescription || row.node.deliveryDescription || row.node.estimateSummary"
              :class="$style.itemDescription"
            >
              <span v-if="row.node.scopeDescription">范围：{{ row.node.scopeDescription }}</span>
              <span v-if="row.node.deliveryDescription"> · 交付：{{ row.node.deliveryDescription }}</span>
              <span v-if="row.node.estimateSummary"> · 估算：{{ row.node.estimateSummary }}</span>
            </p>
          </div>
          <div
            v-if="canManagePlan"
            :class="$style.actions"
          >
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="openWbsForm(row.node)"
            >
              编辑
            </UiButton>
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="requestDelete('wbs', row.node.id, row.node.name)"
            >
              删除
            </UiButton>
          </div>
        </article>
      </div>
      <p
        v-else
        :class="$style.empty"
      >
        还没有 WBS 节点。
      </p>
    </section>

    <section
      :class="$style.section"
      aria-label="Sprint 迭代计划"
    >
      <header :class="$style.sectionHeader">
        <div>
          <h3 :class="$style.heading">Sprint 迭代</h3>
          <p :class="$style.description">日期、负责人、状态和进度来自项目计划接口。</p>
        </div>
        <UiButton
          v-if="canManagePlan"
          variant="primary"
          :disabled="saving"
          @click="openSprintForm()"
        >
          新增 Sprint
        </UiButton>
      </header>
      <div
        v-if="detail.sprints.length"
        :class="$style.itemList"
      >
        <article
          v-for="sprint in detail.sprints"
          :key="sprint.id"
          :class="$style.item"
        >
          <div :class="$style.itemMain">
            <div :class="$style.itemTitleRow">
              <span :class="$style.code">{{ sprint.sprintCode }}</span>
              <h4 :class="$style.itemTitle">{{ sprint.name }}</h4>
              <span :class="$style.status">{{ sprint.status }}</span>
            </div>
            <p :class="$style.meta">
              {{ displayDate(sprint.startDate) }} 至 {{ displayDate(sprint.endDate) }} · 负责人
              {{ sprint.ownerName || '未分配' }}
            </p>
            <div :class="$style.progressRow">
              <progress
                :class="$style.progressTrack"
                :value="Math.min(100, Math.max(0, sprint.progress))"
                max="100"
                :aria-label="`${sprint.name} 完成率`"
              />
              <span :class="$style.progressValue">{{ sprint.progress }}%</span>
            </div>
            <p :class="$style.meta">
              {{ sprint.taskCount }} 个任务 · {{ sprint.milestoneCount }} 个里程碑 · {{ sprint.riskCount }} 个风险
            </p>
            <p
              v-if="sprint.summary"
              :class="$style.itemDescription"
            >
              {{ sprint.summary }}
            </p>
          </div>
          <div
            v-if="canManagePlan"
            :class="$style.actions"
          >
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="openSprintForm(sprint)"
            >
              编辑
            </UiButton>
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="requestDelete('sprint', sprint.id, sprint.name)"
            >
              删除
            </UiButton>
          </div>
        </article>
      </div>
      <p
        v-else
        :class="$style.empty"
      >
        还没有 Sprint。
      </p>
    </section>

    <section
      :class="$style.section"
      aria-label="项目里程碑"
    >
      <header :class="$style.sectionHeader">
        <div>
          <h3 :class="$style.heading">里程碑</h3>
          <p :class="$style.description">里程碑完成状态仍由验收与质量规则校验。</p>
        </div>
        <UiButton
          v-if="canManagePlan"
          variant="primary"
          :disabled="saving"
          @click="openMilestoneForm()"
        >
          新增里程碑
        </UiButton>
      </header>
      <div
        v-if="detail.milestones.length"
        :class="$style.itemList"
      >
        <article
          v-for="milestone in detail.milestones"
          :key="milestone.id"
          :class="$style.item"
        >
          <div :class="$style.itemMain">
            <div :class="$style.itemTitleRow">
              <h4 :class="$style.itemTitle">{{ milestone.name }}</h4>
              <span :class="$style.status">{{ milestone.status }}</span>
            </div>
            <p :class="$style.meta">
              计划完成 {{ displayDate(milestone.plannedCompletionDate) }} · 实际完成
              {{ displayDate(milestone.actualCompletionDate) }} · 负责人 {{ milestone.ownerName || '未分配' }}
            </p>
            <p :class="$style.meta">Sprint：{{ milestone.sprintName || '—' }}</p>
          </div>
          <div
            v-if="canManagePlan"
            :class="$style.actions"
          >
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="openMilestoneForm(milestone)"
            >
              编辑
            </UiButton>
            <UiButton
              variant="ghost"
              size="sm"
              :disabled="saving"
              @click="requestDelete('milestone', milestone.id, milestone.name)"
            >
              删除
            </UiButton>
          </div>
        </article>
      </div>
      <p
        v-else
        :class="$style.empty"
      >
        还没有里程碑。
      </p>
    </section>

    <section
      :class="[$style.section, $style.fullWidth]"
      aria-label="项目基线"
    >
      <header :class="$style.sectionHeader">
        <div>
          <h3 :class="$style.heading">项目基线</h3>
          <p :class="$style.description">活动基线由项目治理接口维护。</p>
        </div>
        <UiButton
          v-if="canCreateBaseline"
          variant="primary"
          :disabled="saving || loadingBaselineChanges"
          @click="openBaselineForm"
        >
          {{ activeBaseline ? '建立下一版基线' : '建立基线' }}
        </UiButton>
      </header>
      <template v-if="activeBaseline">
        <div :class="$style.baselineGrid">
          <div :class="$style.baselineMetric">
            <span :class="$style.metricLabel">基线版本</span>
            <strong :class="$style.metricValue">V{{ activeBaseline.versionNo }}</strong>
          </div>
          <div :class="$style.baselineMetric">
            <span :class="$style.metricLabel">生效日期</span>
            <strong :class="$style.metricValue">{{ displayDate(activeBaseline.effectiveDate) }}</strong>
          </div>
          <div :class="$style.baselineSummary">
            <span :class="$style.metricLabel">范围基线</span>
            <p>{{ activeBaseline.scopeBaselineSummary || '—' }}</p>
          </div>
          <div :class="$style.baselineSummary">
            <span :class="$style.metricLabel">进度基线</span>
            <p>{{ activeBaseline.scheduleBaselineSummary || '—' }}</p>
          </div>
        </div>
        <template v-if="canViewSensitiveAmounts">
          <div :class="$style.baselineGrid">
            <div :class="$style.baselineMetric">
              <span :class="$style.metricLabel">完工预算</span>
              <strong :class="$style.metricValue">{{ activeBaseline.budgetAtCompletion ?? '—' }}</strong>
            </div>
            <div :class="$style.baselineSummary">
              <span :class="$style.metricLabel">成本基线</span>
              <p>{{ activeBaseline.costBaselineSummary || '—' }}</p>
            </div>
          </div>
        </template>
      </template>
      <p
        v-else
        :class="$style.empty"
      >
        当前没有活动基线。是否存在已停用的历史版本需要查询基线历史记录。
      </p>
      <p
        v-if="!canCreateBaseline && (canManageGovernance || !canViewSensitiveAmounts)"
        role="note"
        :class="$style.accessNote"
      >
        建立成本基线需要项目治理权限和敏感预算权限。
      </p>
    </section>

    <UiDialog
      :open="dialogKind !== null"
      :title="currentDialogTitle"
      description="保存由项目后端校验权限、引用和状态规则。"
      size="lg"
      layer="top"
      @update:open="(open) => !open && closeForm()"
    >
      <div :class="$style.formGrid">
        <template v-if="dialogKind === 'wbs'">
          <UiFormField
            label="WBS 编码"
            required
          >
            <UiInput
              v-model="wbsForm.wbsCode"
              required
            />
          </UiFormField>
          <UiFormField
            label="节点名称"
            required
          >
            <UiInput
              v-model="wbsForm.name"
              required
            />
          </UiFormField>
          <UiFormField label="节点类型">
            <UiSelect
              v-model="wbsForm.type"
              label="节点类型"
              :options="wbsTypeOptions"
            />
          </UiFormField>
          <UiFormField label="状态">
            <UiSelect
              v-model="wbsForm.status"
              label="WBS 状态"
              :options="wbsStatusOptions"
            />
          </UiFormField>
          <UiFormField label="父节点">
            <UiSelect
              v-model="wbsForm.parentId"
              label="父节点"
              :options="parentOptions"
            />
          </UiFormField>
          <UiFormField label="同级排序值">
            <UiInput
              v-model="wbsForm.sortOrder"
              type="number"
            />
          </UiFormField>
          <UiFormField label="负责人">
            <UiSelect
              v-model="wbsForm.ownerUserId"
              label="WBS 负责人"
              :options="ownerOptions(wbsForm.ownerUserId, '')"
              :disabled="!activeMemberRequired"
            />
          </UiFormField>
          <UiFormField label="关联 Sprint">
            <UiSelect
              v-model="wbsForm.sprintId"
              label="关联 Sprint"
              :options="sprintOptions"
            />
          </UiFormField>
          <UiFormField
            label="关联里程碑"
            wide
          >
            <UiSelect
              v-model="wbsForm.milestoneId"
              label="关联里程碑"
              :options="milestoneOptions"
            />
          </UiFormField>
          <UiFormField
            label="范围说明"
            wide
          >
            <UiTextarea
              v-model="wbsForm.scopeDescription"
              rows="3"
            />
          </UiFormField>
          <UiFormField
            label="交付说明"
            wide
          >
            <UiTextarea
              v-model="wbsForm.deliveryDescription"
              rows="3"
            />
          </UiFormField>
          <UiFormField
            label="估算摘要"
            wide
          >
            <UiTextarea
              v-model="wbsForm.estimateSummary"
              rows="2"
            />
          </UiFormField>
        </template>

        <template v-else-if="dialogKind === 'sprint'">
          <UiFormField
            label="Sprint 名称"
            required
          >
            <UiInput
              v-model="sprintForm.name"
              required
            />
          </UiFormField>
          <UiFormField
            label="负责人"
            required
          >
            <UiSelect
              v-model="sprintForm.ownerUserId"
              label="Sprint 负责人"
              :options="
                ownerOptions(
                  sprintForm.ownerUserId,
                  detail.sprints.find((item) => item.id === editingId)?.ownerName ?? '',
                )
              "
              :disabled="!activeMemberRequired"
            />
          </UiFormField>
          <UiFormField label="开始日期">
            <UiInput
              v-model="sprintForm.startDate"
              type="date"
            />
          </UiFormField>
          <UiFormField label="结束日期">
            <UiInput
              v-model="sprintForm.endDate"
              type="date"
            />
          </UiFormField>
          <UiFormField label="状态">
            <UiSelect
              v-model="sprintForm.status"
              label="Sprint 状态"
              :options="sprintStatusOptions"
            />
          </UiFormField>
          <UiFormField label="进度（0–100）">
            <UiInput
              v-model="sprintForm.progress"
              type="number"
              min="0"
              max="100"
            />
          </UiFormField>
          <UiFormField
            label="Sprint 摘要"
            wide
          >
            <UiTextarea
              v-model="sprintForm.summary"
              rows="3"
            />
          </UiFormField>
        </template>

        <template v-else-if="dialogKind === 'milestone'">
          <UiFormField
            label="里程碑名称"
            required
          >
            <UiInput
              v-model="milestoneForm.name"
              required
            />
          </UiFormField>
          <UiFormField
            label="负责人"
            required
          >
            <UiSelect
              v-model="milestoneForm.ownerUserId"
              label="里程碑负责人"
              :options="
                ownerOptions(
                  milestoneForm.ownerUserId,
                  detail.milestones.find((item) => item.id === editingId)?.ownerName ?? '',
                )
              "
              :disabled="!activeMemberRequired"
            />
          </UiFormField>
          <UiFormField
            label="计划完成日期"
            required
          >
            <UiInput
              v-model="milestoneForm.plannedCompletionDate"
              type="date"
              required
            />
          </UiFormField>
          <UiFormField label="实际完成日期">
            <UiInput
              v-model="milestoneForm.actualCompletionDate"
              type="date"
            />
          </UiFormField>
          <UiFormField
            label="关联 Sprint"
            wide
          >
            <UiSelect
              v-model="milestoneForm.sprintId"
              label="关联 Sprint"
              :options="sprintOptions"
            />
          </UiFormField>
          <UiFormField
            label="当前状态"
            wide
          >
            <p>{{ milestoneForm.status }}（完成状态通过里程碑状态更新和验收校验管理）</p>
          </UiFormField>
        </template>

        <template v-else-if="dialogKind === 'baseline'">
          <UiFormField
            label="基线生效日期"
            required
          >
            <UiInput
              v-model="baselineForm.effectiveDate"
              type="date"
              required
            />
          </UiFormField>
          <UiFormField
            label="完工预算（元）"
            required
          >
            <UiInput
              v-model="baselineForm.budgetAtCompletion"
              type="number"
              min="0"
              required
            />
          </UiFormField>
          <UiFormField
            label="范围基线摘要"
            required
            wide
          >
            <UiTextarea
              v-model="baselineForm.scopeBaselineSummary"
              rows="3"
              required
            />
          </UiFormField>
          <UiFormField
            label="进度基线摘要"
            required
            wide
          >
            <UiTextarea
              v-model="baselineForm.scheduleBaselineSummary"
              rows="3"
              required
            />
          </UiFormField>
          <UiFormField
            label="成本基线摘要"
            wide
          >
            <UiTextarea
              v-model="baselineForm.costBaselineSummary"
              rows="3"
            />
          </UiFormField>
          <UiFormField
            label="已批准变更"
            wide
          >
            <UiSelect
              v-model="baselineForm.createdFromChangeRequestId"
              label="关联已批准的基线变更"
              :options="changeRequestOptions"
              :required="Boolean(activeBaseline)"
              :disabled="loadingBaselineChanges || (Boolean(activeBaseline) && approvedBaselineChanges.length === 0)"
            />
            <p
              v-if="activeBaseline && approvedBaselineChanges.length === 0"
              role="note"
            >
              暂无已批准的范围、进度或成本变更，不能创建下一版活动基线。
            </p>
            <p
              v-else-if="!activeBaseline"
              role="note"
            >
              如果项目已有非活动历史基线，后端仍会要求关联已批准的基线变更。
            </p>
          </UiFormField>
        </template>

        <p
          v-if="!activeMemberRequired && (dialogKind === 'sprint' || dialogKind === 'milestone')"
          role="note"
          :class="$style.formNote"
        >
          当前详情没有可选的在岗项目成员。请先维护项目成员，再分配负责人。
        </p>
        <p
          v-if="error"
          role="alert"
          :class="$style.formNotice"
        >
          {{ error }}
        </p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="saving"
          @click="closeForm"
        >
          取消
        </UiButton>
        <UiButton
          variant="primary"
          :disabled="
            saving ||
            (dialogKind === 'baseline' && activeBaseline !== null && !baselineForm.createdFromChangeRequestId) ||
            ((dialogKind === 'sprint' || dialogKind === 'milestone') && !activeMemberRequired)
          "
          :loading="saving"
          @click="
            dialogKind === 'wbs'
              ? saveWbs()
              : dialogKind === 'sprint'
                ? saveSprint()
                : dialogKind === 'milestone'
                  ? saveMilestone()
                  : saveBaseline()
          "
        >
          保存
        </UiButton>
      </template>
    </UiDialog>

    <UiAlertDialog
      :open="deleteTarget !== null"
      title="确认删除计划项"
      :description="`确认删除“${deleteTarget?.name ?? ''}”？如果该项已被任务或其他记录引用，服务端会阻止删除并说明原因。`"
      confirm-label="确认删除"
      @update:open="(open) => !open && (deleteTarget = null)"
      @confirm="confirmDelete"
    />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.planGrid {
  @apply grid grid-cols-1 gap-4 border-t pt-4 xl:grid-cols-2;
}

.fullWidth {
  @apply xl:col-span-2;
}

.section {
  @apply min-w-0 space-y-3 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}

.sectionHeader {
  @apply flex flex-wrap items-start justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border-subtle);
}

.heading {
  @apply text-sm font-semibold;
  color: var(--ui-text);
}

.description {
  @apply mt-1 text-xs leading-5;
  color: var(--ui-text-subtle);
}

.itemList {
  @apply divide-y;
  border-color: var(--ui-border-subtle);
}

.item {
  @apply flex min-w-0 flex-col gap-3 py-3 sm:flex-row sm:items-start sm:justify-between;
  border-color: var(--ui-border-subtle);
}

.itemMain {
  @apply min-w-0 flex-1 space-y-1.5;
}

.itemTitleRow {
  @apply flex flex-wrap items-center gap-x-2 gap-y-1;
}

.itemTitle {
  @apply text-sm font-medium;
  color: var(--ui-text);
}

.code {
  @apply rounded px-1.5 py-0.5 font-mono text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.treeMarker {
  @apply font-mono text-xs;
  color: var(--ui-text-subtle);
}

.status {
  @apply rounded-full px-2 py-0.5 text-[10px] font-medium;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.meta {
  @apply text-xs leading-5;
  color: var(--ui-text-subtle);
}

.itemDescription {
  @apply text-xs leading-5;
  color: var(--ui-text-muted);
}

.actions {
  @apply flex shrink-0 items-center gap-1;
}

.empty {
  @apply rounded-md border border-dashed px-4 py-6 text-center text-sm;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.progressRow {
  @apply flex max-w-sm items-center gap-2;
}

.progressTrack {
  @apply h-1.5 w-full max-w-sm overflow-hidden rounded-full;
  border: 0;
  appearance: none;
  accent-color: var(--app-primary);
  background: var(--ui-border-subtle);
}

.progressTrack::-webkit-progress-bar {
  background: var(--ui-border-subtle);
  border-radius: 999px;
}

.progressTrack::-webkit-progress-value,
.progressTrack::-moz-progress-bar {
  border-radius: 999px;
  background: var(--app-primary);
}

.progressValue {
  @apply w-9 text-right text-[11px] font-medium tabular-nums;
  color: var(--ui-text-muted);
}

.baselineGrid {
  @apply grid grid-cols-1 gap-3 sm:grid-cols-2;
}

.baselineMetric,
.baselineSummary {
  @apply min-w-0 rounded-md border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.metricLabel {
  @apply block text-[11px] font-medium;
  color: var(--ui-text-subtle);
}

.metricValue {
  @apply mt-1 block text-sm font-semibold;
  color: var(--ui-text);
}

.baselineSummary p {
  @apply mt-1 whitespace-pre-wrap text-xs leading-5;
  color: var(--ui-text);
}

.accessNote,
.formNote {
  @apply rounded-md px-3 py-2 text-xs leading-5;
  color: var(--ui-text-muted);
  background: var(--ui-warning-soft);
}

.feedbackError,
.feedbackSuccess {
  @apply rounded-md px-3 py-2 text-xs leading-5 xl:col-span-2;
}

.feedbackError,
.formNotice {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.feedbackSuccess {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.formGrid {
  @apply grid grid-cols-2 gap-3 p-4;
}

.formNotice {
  @apply col-span-2 rounded-md px-3 py-2 text-xs leading-5;
}
</style>
