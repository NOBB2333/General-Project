<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, nextTick, reactive, ref, watch } from 'vue';

import { Button, Col, Form, Input, InputNumber, Popconfirm, Row, Select, Space, Switch, Tag, Textarea } from 'ant-design-vue';
import { message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createProjectTaskFromWbsApi,
  createProjectWbsNodeApi,
  deleteProjectWbsNodeApi,
  updateProjectWbsNodeApi,
  type ProjectApi,
} from '#/api/core';

const props = defineProps<{
  sprintOptions: Array<{ label: string; value: string }>;
  embedded?: boolean;
  milestoneId?: string;
  milestoneName?: string;
  memberOptions: Array<{ label: string; value: string }>;
  milestoneOptions: Array<{ label: string; value: string }>;
  projectId?: string;
  tasks: ProjectApi.TaskItem[];
  wbsNodes: ProjectApi.WbsNode[];
}>();

const emit = defineEmits<{
  refreshed: [];
}>();

type WbsTreeNode = ProjectApi.WbsNode & {
  children?: WbsTreeNode[];
};

const emptyForm = (): ProjectApi.WbsNodeSaveInput => ({
  sprintId: undefined,
  deliveryDescription: '',
  estimateSummary: '',
  milestoneId: undefined,
  name: '',
  ownerUserId: undefined,
  parentId: undefined,
  scopeDescription: '',
  sortOrder: 0,
  status: '规划中',
  type: '工作包',
  wbsCode: '',
});

const form = reactive<ProjectApi.WbsNodeSaveInput>(emptyForm());
const taskForm = reactive<ProjectApi.TaskFromWbsSaveInput>({
  sprintId: undefined,
  developerOwnerUserId: undefined,
  developerOwnerUserIds: [],
  description: '',
  estimatedWorkHours: undefined,
  isOutOfContract: false,
  ownerUserId: '',
  plannedEndTime: undefined,
  plannedStartTime: undefined,
  priority: 'P1',
  productOwnerUserId: undefined,
  productOwnerUserIds: [],
  taskCode: '',
  testerOwnerUserId: undefined,
  testerOwnerUserIds: [],
  title: '',
});
const selectedNodeId = ref<string>('');
const saving = ref(false);
const generatingTaskId = ref('');
const deletingId = ref('');
const expandedRowKeys = ref<string[]>([]);
const hasAdjustedExpandedRows = ref(false);
const showTaskForm = ref(false);
const createMode = ref<'' | 'child' | 'root' | 'sibling'>('');
const previousSelectedNodeId = ref('');

const LARGE_WBS_NODE_THRESHOLD = 30;

const typeOptions = ['阶段', '模块', '工作包'].map((value) => ({
  label: value,
  value,
}));
const statusOptions = ['规划中', '已确认', '执行中', '已完成', '已取消'].map(
  (value) => ({
    label: value,
    value,
  }),
);
const priorityOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({ label: value, value }));

const wbsColumns = [
  { align: 'left' as const, field: 'name', minWidth: 260, slots: { default: 'name' }, title: 'WBS 节点', treeNode: true },
  { field: 'type', slots: { default: 'type' }, title: '类型', width: 90 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 100 },
  { field: 'taskCount', slots: { default: 'taskCount' }, title: '需求', width: 80 },
  { field: 'completionRate', slots: { default: 'completionRate' }, title: '完成率', width: 90 },
  { field: 'actualWorkHours', slots: { default: 'actualWorkHours' }, title: '实际工时', width: 110 },
  { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 180 },
] satisfies VxeGridProps<WbsTreeNode>['columns'];

const scopedWbsNodes = computed(() => {
  if (!props.milestoneId) {
    return props.wbsNodes;
  }

  return collectMilestoneScopedWbsNodes(props.wbsNodes, props.milestoneId);
});

function collectMilestoneScopedWbsNodes(
  nodes: ProjectApi.WbsNode[],
  milestoneId: string,
) {
  const nodeMap = new Map(nodes.map((node) => [node.id, node]));
  const childrenMap = new Map<string, ProjectApi.WbsNode[]>();
  const includedIds = new Set<string>();

  for (const node of nodes) {
    if (!node.parentId) {
      continue;
    }

    childrenMap.set(node.parentId, [
      ...(childrenMap.get(node.parentId) || []),
      node,
    ]);
  }

  const includeAncestors = (node: ProjectApi.WbsNode) => {
    let current = node.parentId ? nodeMap.get(node.parentId) : undefined;
    while (current && !includedIds.has(current.id)) {
      includedIds.add(current.id);
      current = current.parentId ? nodeMap.get(current.parentId) : undefined;
    }
  };

  const includeDescendants = (node: ProjectApi.WbsNode) => {
    for (const child of childrenMap.get(node.id) || []) {
      if (includedIds.has(child.id)) {
        continue;
      }

      includedIds.add(child.id);
      includeDescendants(child);
    }
  };

  for (const node of nodes) {
    if (node.milestoneId !== milestoneId) {
      continue;
    }

    includedIds.add(node.id);
    includeAncestors(node);
    includeDescendants(node);
  }

  return nodes.filter((node) => includedIds.has(node.id));
}

const treeData = computed(() => {
  const map = new Map<string, WbsTreeNode>();
  const roots: WbsTreeNode[] = [];

  for (const node of scopedWbsNodes.value) {
    map.set(node.id, { ...node, children: [] });
  }

  for (const node of map.values()) {
    if (node.parentId && map.has(node.parentId)) {
      map.get(node.parentId)?.children?.push(node);
    } else {
      roots.push(node);
    }
  }

  const sortNodes = (nodes: WbsTreeNode[]) => {
    nodes.sort(
      (left, right) =>
        left.sortOrder - right.sortOrder ||
        left.wbsCode.localeCompare(right.wbsCode, 'zh-CN'),
    );
    for (const node of nodes) {
      sortNodes(node.children || []);
      if (node.children?.length === 0) {
        delete node.children;
      }
    }
  };

  sortNodes(roots);
  return roots;
});

const wbsGridOptions = computed<VxeGridProps<WbsTreeNode>>(() => ({
  columns: wbsColumns,
  data: treeData.value,
  emptyText: '暂无 WBS 节点',
  pagerConfig: { enabled: false },
  rowClassName: ({ row }) => getRowClassName(row),
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
  treeConfig: {
    childrenField: 'children',
    rowField: 'id',
    transform: false,
  },
}));

const [WbsGrid, wbsGridApi] = useVbenVxeGrid<WbsTreeNode>({ gridOptions: {} });

const nodeOptions = computed(() =>
  scopedWbsNodes.value.map((node) => ({
    label: `${node.wbsCode} · ${node.name}`,
    value: node.id,
  })),
);

const selectedNode = computed(() =>
  scopedWbsNodes.value.find((node) => node.id === selectedNodeId.value),
);

const wbsFormTitle = computed(() => {
  if (selectedNodeId.value) {
    return '编辑 WBS 节点';
  }

  if (createMode.value === 'root') {
    return props.embedded ? '新增里程碑下 WBS' : '新增顶层 WBS';
  }

  if (createMode.value === 'sibling') {
    return '新增同级 WBS';
  }

  if (createMode.value === 'child') {
    return '新增子级 WBS';
  }

  return '新建 WBS 节点';
});

const wbsFormHint = computed(() => {
  if (selectedNode.value) {
    return `${selectedNode.value.wbsCode} · ${selectedNode.value.name}`;
  }

  if (form.parentId) {
    const parent = scopedWbsNodes.value.find((node) => node.id === form.parentId);
    return parent ? `父级：${parent.wbsCode} · ${parent.name}` : '父级：未选择';
  }

  return props.milestoneName ? `归属里程碑：${props.milestoneName}` : '顶层节点';
});

const selectedNodeTasks = computed(() => {
  const current = selectedNode.value;
  if (!current) {
    return [];
  }

  return props.tasks.filter(
    (task) =>
      task.wbsNodeId === current.id ||
      (!task.wbsNodeId && task.wbsCode === current.wbsCode),
  );
});

watch(
  () => scopedWbsNodes.value,
  (nodes) => {
    syncExpandedRowKeys();

    if (selectedNodeId.value && nodes.some((node) => node.id === selectedNodeId.value)) {
      syncFormFromSelected();
      return;
    }

    selectedNodeId.value = nodes[0]?.id || '';
    syncFormFromSelected();
  },
  { immediate: true },
);

watch(
  () => [props.projectId, props.milestoneId],
  () => {
    hasAdjustedExpandedRows.value = false;
    syncExpandedRowKeys();
  },
);

function resetForm(next?: Partial<ProjectApi.WbsNodeSaveInput>) {
  Object.assign(form, emptyForm(), {
    milestoneId: props.milestoneId || undefined,
  }, next);
}

function resetTaskForm(next?: Partial<ProjectApi.TaskFromWbsSaveInput>) {
  Object.assign(taskForm, {
    sprintId: undefined,
    developerOwnerUserId: undefined,
    developerOwnerUserIds: [],
    description: '',
    estimatedWorkHours: undefined,
    isOutOfContract: false,
    ownerUserId: '',
    plannedEndTime: undefined,
    plannedStartTime: undefined,
    priority: 'P1',
    productOwnerUserId: undefined,
    productOwnerUserIds: [],
    taskCode: '',
    testerOwnerUserId: undefined,
    testerOwnerUserIds: [],
    title: '',
  }, next);
}

function syncFormFromSelected() {
  const current = selectedNode.value;
  if (!current) {
    resetForm();
    return;
  }

  resetForm({
    sprintId: current.sprintId || undefined,
    deliveryDescription: current.deliveryDescription,
    estimateSummary: current.estimateSummary,
    milestoneId: current.milestoneId || undefined,
    name: current.name,
    ownerUserId: current.ownerUserId || undefined,
    parentId: current.parentId || undefined,
    scopeDescription: current.scopeDescription,
    sortOrder: current.sortOrder,
    status: current.status || '规划中',
    type: current.type || '工作包',
    wbsCode: current.wbsCode,
  });
}

function handleSelectNode(node: ProjectApi.WbsNode) {
  createMode.value = '';
  selectedNodeId.value = node.id;
  showTaskForm.value = false;
  syncFormFromSelected();
}

function getRowClassName(record: ProjectApi.WbsNode) {
  return record.id === selectedNodeId.value ? 'project-detail__table-row--selected' : '';
}

function resolveTaskCount(node: ProjectApi.WbsNode) {
  return props.tasks.filter(
    (task) =>
      task.wbsNodeId === node.id ||
      (!task.wbsNodeId && task.wbsCode === node.wbsCode),
  ).length || node.taskCount;
}

function resolveNodeTasks(node: ProjectApi.WbsNode) {
  return props.tasks.filter(
    (task) =>
      task.wbsNodeId === node.id ||
      (!task.wbsNodeId && task.wbsCode === node.wbsCode),
  );
}

function resolveCompletionRate(node: ProjectApi.WbsNode) {
  const tasks = resolveNodeTasks(node);
  if (tasks.length === 0) {
    return node.completionRate || 0;
  }

  const closedCount = tasks.filter((task) =>
    ['已完成', '已关闭'].includes(task.status),
  ).length;
  return Math.round((closedCount / tasks.length) * 100);
}

function resolveEstimatedWorkHours(node: ProjectApi.WbsNode) {
  const tasks = resolveNodeTasks(node);
  const value =
    tasks.length === 0
      ? node.estimatedWorkHours || 0
      : tasks.reduce((sum, task) => sum + (task.estimatedWorkHours || 0), 0);
  return formatHours(value);
}

function resolveActualWorkHours(node: ProjectApi.WbsNode) {
  const tasks = resolveNodeTasks(node);
  const value =
    tasks.length === 0
      ? node.actualWorkHours || 0
      : tasks.reduce((sum, task) => sum + (task.actualWorkHours || 0), 0);
  return formatHours(value);
}

function formatHours(value: number) {
  return `${Number(value || 0).toFixed(1)} 小时`;
}

function readFirstSelectedUserId(value: unknown) {
  return Array.isArray(value) && typeof value[0] === 'string' ? value[0] : undefined;
}

function canGenerateFromNode(node: ProjectApi.WbsNode) {
  return node.type === '工作包';
}

function handleCreateRoot() {
  previousSelectedNodeId.value = selectedNodeId.value;
  createMode.value = 'root';
  selectedNodeId.value = '';
  showTaskForm.value = false;
  resetForm({
    type: props.embedded ? '模块' : '阶段',
    wbsCode: resolveNextSiblingCode(undefined),
  });
}

function handleCreateSibling() {
  const current = selectedNode.value;
  previousSelectedNodeId.value = selectedNodeId.value;
  createMode.value = 'sibling';
  selectedNodeId.value = '';
  showTaskForm.value = false;
  resetForm({
    parentId: current?.parentId || undefined,
    type: current?.type || '工作包',
    wbsCode: resolveNextSiblingCode(current?.parentId || undefined),
  });
}

function handleCreateChild() {
  const current = selectedNode.value;
  previousSelectedNodeId.value = selectedNodeId.value;
  createMode.value = 'child';
  selectedNodeId.value = '';
  showTaskForm.value = false;
  resetForm({
    parentId: current?.id,
    type: current?.type === '阶段' ? '模块' : '工作包',
    wbsCode: resolveNextChildCode(current),
  });
}

function handleCancelWbsForm() {
  if (selectedNodeId.value) {
    syncFormFromSelected();
    return;
  }

  createMode.value = '';
  selectedNodeId.value =
    previousSelectedNodeId.value &&
    scopedWbsNodes.value.some((node) => node.id === previousSelectedNodeId.value)
      ? previousSelectedNodeId.value
      : scopedWbsNodes.value[0]?.id || '';
  syncFormFromSelected();
}

function handleOpenTaskForm(node?: ProjectApi.WbsNode) {
  const current = node || selectedNode.value;
  if (!current) {
    message.warning('请先选择一个 WBS 工作包');
    return;
  }

  if (!canGenerateFromNode(current)) {
    message.warning('只有工作包节点才能生成需求');
    return;
  }

  selectedNodeId.value = current.id;
  resetTaskForm({
    sprintId: current.sprintId || undefined,
    estimatedWorkHours: current.estimatedWorkHours || undefined,
    ownerUserId: current.ownerUserId || props.memberOptions[0]?.value || '',
    productOwnerUserId: current.ownerUserId || undefined,
    productOwnerUserIds: current.ownerUserId ? [current.ownerUserId] : [],
    priority: 'P1',
    description: current.scopeDescription || '',
    title: current.name,
  });
  showTaskForm.value = true;
}

async function handleSave() {
  if (!props.projectId) {
    message.warning('请先选择项目');
    return;
  }

  if (!form.wbsCode.trim() || !form.name.trim()) {
    message.error('请填写 WBS 编码和节点名称');
    return;
  }

  saving.value = true;
  try {
    const payload = normalizeForm();
    if (selectedNodeId.value) {
      await updateProjectWbsNodeApi(selectedNodeId.value, payload);
      message.success('WBS 节点已更新');
    } else {
      const nodeId = await createProjectWbsNodeApi(props.projectId, payload);
      selectedNodeId.value = nodeId;
      createMode.value = '';
      message.success('WBS 节点已创建');
    }

    if (payload.parentId) {
      includeExpandedRowKeys([payload.parentId]);
    }

    emit('refreshed');
  } catch (error) {
    const description = error instanceof Error ? error.message : 'WBS 节点保存失败';
    message.error(description || 'WBS 节点保存失败');
  } finally {
    saving.value = false;
  }
}

async function handleDelete(node: ProjectApi.WbsNode) {
  deletingId.value = node.id;
  try {
    await deleteProjectWbsNodeApi(node.id);
    message.success('WBS 节点已删除');
    selectedNodeId.value = '';
    resetForm();
    emit('refreshed');
  } catch (error) {
    const description = error instanceof Error ? error.message : 'WBS 节点删除失败';
    message.error(description || 'WBS 节点删除失败');
  } finally {
    deletingId.value = '';
  }
}

async function handleGenerateTask(node: ProjectApi.WbsNode) {
  if (!props.projectId) {
    message.warning('请先选择项目');
    return;
  }

  if (!canGenerateFromNode(node)) {
    message.warning('只有工作包节点才能生成需求');
    return;
  }

  const ownerUserId = taskForm.ownerUserId || node.ownerUserId || props.memberOptions[0]?.value;
  if (!ownerUserId) {
    message.error('请先选择需求负责人，或为项目添加成员');
    return;
  }

  if (!taskForm.title.trim()) {
    message.error('请填写需求标题');
    return;
  }

  generatingTaskId.value = node.id;
  try {
    await createProjectTaskFromWbsApi(props.projectId, node.id, {
      sprintId: taskForm.sprintId || undefined,
      description: taskForm.description?.trim() || node.scopeDescription || '',
      developerOwnerUserId: taskForm.developerOwnerUserIds?.[0] || taskForm.developerOwnerUserId || undefined,
      developerOwnerUserIds: taskForm.developerOwnerUserIds || [],
      estimatedWorkHours: taskForm.estimatedWorkHours,
      isOutOfContract: taskForm.isOutOfContract,
      ownerUserId,
      plannedEndTime: taskForm.plannedEndTime || undefined,
      plannedStartTime: taskForm.plannedStartTime || undefined,
      productOwnerUserId: taskForm.productOwnerUserIds?.[0] || taskForm.productOwnerUserId || undefined,
      productOwnerUserIds: taskForm.productOwnerUserIds || [],
      priority: taskForm.priority || 'P1',
      taskCode: taskForm.taskCode?.trim() || '',
      testerOwnerUserId: taskForm.testerOwnerUserIds?.[0] || taskForm.testerOwnerUserId || undefined,
      testerOwnerUserIds: taskForm.testerOwnerUserIds || [],
      title: taskForm.title.trim(),
    });
    message.success('已从 WBS 生成需求');
    showTaskForm.value = false;
    resetTaskForm();
    emit('refreshed');
  } catch (error) {
    const description = error instanceof Error ? error.message : '从 WBS 生成需求失败';
    message.error(description || '从 WBS 生成需求失败');
  } finally {
    generatingTaskId.value = '';
  }
}

function normalizeForm(): ProjectApi.WbsNodeSaveInput {
  return {
    sprintId: form.sprintId || undefined,
    deliveryDescription: form.deliveryDescription?.trim() || '',
    estimateSummary: form.estimateSummary?.trim() || '',
    milestoneId: props.milestoneId || form.milestoneId || undefined,
    name: form.name.trim(),
    ownerUserId: form.ownerUserId || undefined,
    parentId: form.parentId || undefined,
    scopeDescription: form.scopeDescription?.trim() || '',
    sortOrder: form.sortOrder || 0,
    status: form.status || '规划中',
    type: form.type || '工作包',
    wbsCode: form.wbsCode.trim(),
  };
}

function resolveNextSiblingCode(parentId?: string) {
  const sourceNodes = parentId ? scopedWbsNodes.value : props.wbsNodes;
  const siblings = sourceNodes.filter((node) => (node.parentId || undefined) === parentId);
  if (siblings.length === 0) {
    return parentId ? `${selectedNode.value?.wbsCode || '1'}.1` : '1';
  }

  const last = siblings
    .map((node) => node.wbsCode)
    .sort((left, right) => left.localeCompare(right, 'zh-CN', { numeric: true }))
    .at(-1);
  return incrementCode(last || '1');
}

function resolveNextChildCode(node?: ProjectApi.WbsNode) {
  if (!node) {
    return '1';
  }

  const children = scopedWbsNodes.value.filter((item) => item.parentId === node.id);
  if (children.length === 0) {
    return `${node.wbsCode}.1`;
  }

  const last = children
    .map((item) => item.wbsCode)
    .sort((left, right) => left.localeCompare(right, 'zh-CN', { numeric: true }))
    .at(-1);
  return incrementCode(last || `${node.wbsCode}.1`);
}

function incrementCode(code: string) {
  const parts = code.split('.');
  const last = Number(parts.at(-1));
  if (Number.isNaN(last)) {
    return `${code}.1`;
  }

  parts[parts.length - 1] = `${last + 1}`;
  return parts.join('.');
}

function handleExpandAll() {
  hasAdjustedExpandedRows.value = true;
  expandedRowKeys.value = collectExpandableNodeIds(treeData.value);
  applyTreeExpandedRows();
}

function handleCollapseToModuleLevel() {
  hasAdjustedExpandedRows.value = true;
  expandedRowKeys.value = collectModuleLevelExpandedRowKeys(treeData.value);
  applyTreeExpandedRows();
}

function syncExpandedRowKeys() {
  if (hasAdjustedExpandedRows.value) {
    const validIds = new Set(scopedWbsNodes.value.map((node) => node.id));
    expandedRowKeys.value = expandedRowKeys.value.filter((key) => validIds.has(key));
    applyTreeExpandedRows();
    return;
  }

  expandedRowKeys.value =
    scopedWbsNodes.value.length > LARGE_WBS_NODE_THRESHOLD
      ? collectModuleLevelExpandedRowKeys(treeData.value)
      : collectExpandableNodeIds(treeData.value);
  applyTreeExpandedRows();
}

function includeExpandedRowKeys(keys: string[]) {
  hasAdjustedExpandedRows.value = true;
  expandedRowKeys.value = Array.from(new Set([...expandedRowKeys.value, ...keys]));
  applyTreeExpandedRows();
}

function collectExpandableNodeIds(nodes: WbsTreeNode[]) {
  const keys: string[] = [];
  walkExpandableNodes(nodes, (node) => {
    keys.push(node.id);
  });
  return keys;
}

function collectModuleLevelExpandedRowKeys(nodes: WbsTreeNode[]) {
  const keys: string[] = [];
  walkExpandableNodes(nodes, (node, depth) => {
    if (depth === 0 || node.type === '阶段') {
      keys.push(node.id);
    }
  });
  return keys;
}

function walkExpandableNodes(
  nodes: WbsTreeNode[],
  visitor: (node: WbsTreeNode, depth: number) => void,
  depth = 0,
) {
  for (const node of nodes) {
    if (node.children?.length) {
      visitor(node, depth);
      walkExpandableNodes(node.children, visitor, depth + 1);
    }
  }
}

function flattenTreeRows(nodes: WbsTreeNode[]) {
  const rows: WbsTreeNode[] = [];
  const walk = (items: WbsTreeNode[]) => {
    for (const item of items) {
      rows.push(item);
      if (item.children?.length) {
        walk(item.children);
      }
    }
  };

  walk(nodes);
  return rows;
}

function applyTreeExpandedRows() {
  void nextTick(() => {
    const expandedIds = new Set(expandedRowKeys.value);
    const rows = flattenTreeRows(treeData.value).filter((node) => expandedIds.has(node.id));
    wbsGridApi.grid?.clearTreeExpand();
    if (rows.length > 0) {
      wbsGridApi.grid?.setTreeExpand(rows, true);
    }
  });
}
</script>

<template>
  <div :class="['project-detail__wbs-panel', { 'project-detail__wbs-panel--embedded': embedded }]">
    <div class="project-detail__section-head">
      <span>
        {{ milestoneName ? `${milestoneName} 下的 WBS 工作包` : '按阶段、模块、工作包维护范围结构；需求依赖在需求详情里维护。' }}
      </span>
      <Space>
        <Button size="small" @click="handleCollapseToModuleLevel">折叠到模块层</Button>
        <Button size="small" @click="handleExpandAll">展开全部</Button>
        <Button size="small" @click="handleCreateRoot">新增根节点</Button>
        <Button :disabled="!selectedNode" size="small" @click="handleCreateSibling">新增同级</Button>
        <Button :disabled="!selectedNode" size="small" type="primary" @click="handleCreateChild">新增子级</Button>
      </Space>
    </div>

    <div class="project-detail__wbs-layout">
      <div class="project-detail__wbs-tree">
        <WbsGrid
          class="app-vxe-card-grid"
          grid-class="project-detail__wbs-grid-table"
          :grid-options="wbsGridOptions"
        >
          <template #name="{ row }">
            <button class="project-detail__link-button" type="button" @click="handleSelectNode(row)">
              <strong>{{ row.wbsCode }}</strong>
              <span>{{ row.name }}</span>
            </button>
          </template>
          <template #type="{ row }">
            <Tag>{{ row.type }}</Tag>
          </template>
          <template #status="{ row }">
            <Tag color="processing">{{ row.status }}</Tag>
          </template>
          <template #taskCount="{ row }">
            {{ resolveTaskCount(row) }}
          </template>
          <template #completionRate="{ row }">
            {{ resolveCompletionRate(row) }}%
          </template>
          <template #actualWorkHours="{ row }">
            {{ resolveActualWorkHours(row) }}
          </template>
          <template #actions="{ row }">
            <Space size="small">
              <Button
                size="small"
                type="link"
                @click.stop="handleSelectNode(row)"
              >
                编辑
              </Button>
              <Button
                :disabled="!canGenerateFromNode(row)"
                :loading="generatingTaskId === row.id"
                size="small"
                type="link"
                @click.stop="handleOpenTaskForm(row)"
              >
                生成需求
              </Button>
              <Popconfirm title="确认删除这个 WBS 节点？" @confirm="handleDelete(row)">
                <Button
                  danger
                  :loading="deletingId === row.id"
                  size="small"
                  type="link"
                >
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </WbsGrid>
      </div>

      <div class="project-detail__wbs-detail">
        <Form layout="vertical">
          <div class="project-detail__section-head project-detail__wbs-form-head">
            <div class="project-detail__wbs-form-title">
              <strong>{{ wbsFormTitle }}</strong>
              <span>{{ wbsFormHint }}</span>
            </div>
            <Space>
              <Button @click="handleCancelWbsForm">取消</Button>
              <Button :loading="saving" type="primary" @click="handleSave">
                {{ selectedNodeId ? '保存节点' : '创建节点' }}
              </Button>
            </Space>
          </div>
          <div class="project-detail__wbs-form-section">
            <span>基础信息</span>
            <Row :gutter="[12, 0]">
              <Col :span="8">
                <Form.Item label="WBS 编码">
                  <Input v-model:value="form.wbsCode" placeholder="例如 1.2.1" />
                </Form.Item>
              </Col>
              <Col :span="16">
                <Form.Item label="节点名称">
                  <Input v-model:value="form.name" placeholder="填写阶段、模块或工作包名称" />
                </Form.Item>
              </Col>
              <Col :span="8">
                <Form.Item label="节点类型">
                  <Select v-model:value="form.type" :options="typeOptions" />
                </Form.Item>
              </Col>
              <Col :span="8">
                <Form.Item label="节点状态">
                  <Select v-model:value="form.status" :options="statusOptions" />
                </Form.Item>
              </Col>
              <Col :span="8">
                <Form.Item label="排序">
                  <InputNumber v-model:value="form.sortOrder" :min="0" style="width: 100%" />
                </Form.Item>
              </Col>
            </Row>
          </div>

          <div class="project-detail__wbs-form-section">
            <span>归属关系</span>
            <Row :gutter="[12, 0]">
              <Col :span="12">
                <Form.Item label="父级 WBS">
                  <Select
                    v-model:value="form.parentId"
                    allow-clear
                    :options="nodeOptions.filter((item) => item.value !== selectedNodeId)"
                    placeholder="不选则为顶层节点"
                    show-search
                  />
                </Form.Item>
              </Col>
              <Col :span="12">
                <Form.Item label="负责人">
                  <Select v-model:value="form.ownerUserId" allow-clear :options="memberOptions" show-search />
                </Form.Item>
              </Col>
              <Col :span="12">
                <Form.Item label="里程碑">
                  <Select
                    v-model:value="form.milestoneId"
                    allow-clear
                    :disabled="Boolean(milestoneId)"
                    :options="milestoneOptions"
                    show-search
                  />
                </Form.Item>
              </Col>
              <Col :span="12">
                <Form.Item label="所属排期">
                  <Select v-model:value="form.sprintId" allow-clear :options="sprintOptions" show-search />
                </Form.Item>
              </Col>
            </Row>
          </div>

          <div class="project-detail__wbs-form-section">
            <span>范围与交付</span>
            <Row :gutter="[12, 0]">
              <Col :span="24">
                <Form.Item label="范围说明">
                  <Textarea v-model:value="form.scopeDescription" :auto-size="{ minRows: 2, maxRows: 4 }" />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Form.Item label="交付物说明">
                  <Textarea v-model:value="form.deliveryDescription" :auto-size="{ minRows: 2, maxRows: 4 }" />
                </Form.Item>
              </Col>
              <Col :span="24">
                <Form.Item label="估算摘要">
                  <Input v-model:value="form.estimateSummary" />
                </Form.Item>
              </Col>
            </Row>
          </div>
        </Form>

        <Form v-if="showTaskForm && selectedNode" class="project-detail__inline-form-card" layout="vertical">
          <Row :gutter="[12, 0]">
            <Col :span="12">
              <Form.Item label="需求标题">
                <Input v-model:value="taskForm.title" :maxlength="128" />
              </Form.Item>
            </Col>
            <Col :span="12">
              <Form.Item label="负责人">
                <Select v-model:value="taskForm.ownerUserId" :options="memberOptions" show-search />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="产品">
                <Select
                  v-model:value="taskForm.productOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="memberOptions"
                  show-search
                  @change="(value) => { taskForm.productOwnerUserId = readFirstSelectedUserId(value); }"
                />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="开发">
                <Select
                  v-model:value="taskForm.developerOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="memberOptions"
                  show-search
                  @change="(value) => { taskForm.developerOwnerUserId = readFirstSelectedUserId(value); }"
                />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="测试">
                <Select
                  v-model:value="taskForm.testerOwnerUserIds"
                  allow-clear
                  mode="multiple"
                  :options="memberOptions"
                  show-search
                  @change="(value) => { taskForm.testerOwnerUserId = readFirstSelectedUserId(value); }"
                />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="所属排期">
                <Select v-model:value="taskForm.sprintId" allow-clear :options="sprintOptions" show-search />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="计划开始">
                <Input v-model:value="taskForm.plannedStartTime" type="date" />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="计划结束">
                <Input v-model:value="taskForm.plannedEndTime" type="date" />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="预计工时">
                <InputNumber v-model:value="taskForm.estimatedWorkHours" :min="0" style="width: 100%" />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="优先级">
                <Select v-model:value="taskForm.priority" :options="priorityOptions" />
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="需求编号">
                <Tag color="processing">{{ taskForm.taskCode || '系统自动生成' }}</Tag>
              </Form.Item>
            </Col>
            <Col :span="8">
              <Form.Item label="合同外需求">
                <Switch v-model:checked="taskForm.isOutOfContract" checked-children="是" un-checked-children="否" />
              </Form.Item>
            </Col>
            <Col :span="24">
              <Form.Item label="需求说明">
                <Textarea
                  v-model:value="taskForm.description"
                  :auto-size="{ minRows: 2, maxRows: 5 }"
                  :maxlength="2000"
                  placeholder="说明需求要解决什么、怎么验收、关键约束是什么"
                />
              </Form.Item>
            </Col>
            <Col :span="24">
              <Space>
                <Button :loading="generatingTaskId === selectedNode.id" type="primary" @click="handleGenerateTask(selectedNode)">
                  创建需求
                </Button>
                <Button @click="showTaskForm = false">取消</Button>
              </Space>
            </Col>
          </Row>
        </Form>

        <div v-if="selectedNode" class="project-detail__overview-list project-detail__overview-list--metrics">
          <div class="project-detail__overview-item">
            <span>完成率</span>
            <strong>{{ resolveCompletionRate(selectedNode) }}%</strong>
          </div>
          <div class="project-detail__overview-item">
            <span>预计工时</span>
            <strong>{{ resolveEstimatedWorkHours(selectedNode) }}</strong>
          </div>
          <div class="project-detail__overview-item">
            <span>实际工时</span>
            <strong>{{ resolveActualWorkHours(selectedNode) }}</strong>
          </div>
        </div>

        <div class="project-detail__wbs-linked">
          <strong>关联需求</strong>
          <div v-if="selectedNodeTasks.length === 0" class="project-detail__muted">当前节点暂无关联需求。</div>
          <div v-else class="project-detail__tag-list">
            <Tag v-for="task in selectedNodeTasks" :key="task.id">{{ task.taskCode }} · {{ task.title }}</Tag>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
