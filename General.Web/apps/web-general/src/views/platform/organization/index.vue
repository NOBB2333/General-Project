<script lang="ts" setup>
import type { OrganizationApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref, watch } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  Modal,
  Skeleton,
  Table,
  Tag,
  Tree,
  TreeSelect,
  message,
} from 'ant-design-vue';

import {
  createOrganizationUnitApi,
  deleteOrganizationUnitApi,
  getOrganizationTreeApi,
  getUserListApi,
  moveOrganizationUnitApi,
  transferOrganizationMembersApi,
  updateOrganizationUnitApi,
} from '#/api/core';

interface UiTreeNode {
  children?: UiTreeNode[];
  key: string;
  title: string;
}

const columns = [
  { dataIndex: 'displayName', key: 'displayName', title: '姓名' },
  { dataIndex: 'username', key: 'username', title: '账号' },
  { dataIndex: 'roles', key: 'roles', title: '角色' },
  { dataIndex: 'organizationUnitNames', key: 'organizationUnitNames', title: '部门' },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 120 },
];

const keyword = ref('');
const loadingMembers = ref(false);
const loadingTree = ref(true);
const modalVisible = ref(false);
const modalMode = ref<'create' | 'edit' | 'move'>('create');
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const saving = ref(false);
const selectedNodeKey = ref<string>('');
const transferLoading = ref(false);
const transferTargetOrganizationUnitId = ref<string>();
const transferUser = ref<null | UserApi.UserListItem>(null);
const transferVisible = ref(false);
const users = ref<UserApi.UserListItem[]>([]);
const formState = reactive({
  displayName: '',
  parentId: undefined as string | undefined,
});

const selectedNodeKeys = computed(() => (selectedNodeKey.value ? [selectedNodeKey.value] : []));
const selectedNode = computed(() => findNodeById(organizationTree.value, selectedNodeKey.value));

function buildTreeTitle(item: OrganizationApi.OrganizationTreeItem) {
  return item.disabled
    ? `${item.displayName}（仅可见）`
    : `${item.displayName}（${item.memberCount}）`;
}

function normalizeTree(items: OrganizationApi.OrganizationTreeItem[]): UiTreeNode[] {
  return items.map((item) => ({
    children: normalizeTree(item.children || []),
    key: item.id,
    title: buildTreeTitle(item),
  }));
}

function normalizeTreeSelect(
  items: OrganizationApi.OrganizationTreeItem[],
): Array<{ children?: any[]; title: string; value: string }> {
  return items.map((item) => ({
    children: normalizeTreeSelect(item.children || []),
    title: item.displayName,
    value: item.id,
  }));
}

function findNodeById(
  items: OrganizationApi.OrganizationTreeItem[],
  id: string,
): null | OrganizationApi.OrganizationTreeItem {
  for (const item of items) {
    if (item.id === id) {
      return item;
    }
    const child = findNodeById(item.children || [], id);
    if (child) {
      return child;
    }
  }
  return null;
}

async function loadTree() {
  loadingTree.value = true;
  try {
    organizationTree.value = await getOrganizationTreeApi();
    const firstNode = organizationTree.value[0];
    if (!selectedNodeKey.value && firstNode) {
      selectedNodeKey.value = firstNode.id;
    }
  } finally {
    loadingTree.value = false;
  }
}

async function loadMembers() {
  loadingMembers.value = true;
  try {
    users.value = await getUserListApi({
      keyword: keyword.value || undefined,
      organizationUnitId: selectedNodeKey.value || undefined,
    });
  } finally {
    loadingMembers.value = false;
  }
}

function openCreateRoot() {
  modalMode.value = 'create';
  formState.displayName = '';
  formState.parentId = undefined;
  modalVisible.value = true;
}

function openCreateChild() {
  modalMode.value = 'create';
  formState.displayName = '';
  formState.parentId = selectedNode.value?.id || undefined;
  modalVisible.value = true;
}

function openEdit() {
  if (!selectedNode.value) {
    return;
  }
  modalMode.value = 'edit';
  formState.displayName = selectedNode.value.displayName;
  formState.parentId = selectedNode.value.parentId || undefined;
  modalVisible.value = true;
}

function openMove() {
  if (!selectedNode.value) {
    return;
  }
  modalMode.value = 'move';
  formState.displayName = selectedNode.value.displayName;
  formState.parentId = selectedNode.value.parentId || undefined;
  modalVisible.value = true;
}

async function handleSubmit() {
  if (!selectedNode.value && modalMode.value !== 'create') {
    return;
  }
  if (modalMode.value !== 'move' && !formState.displayName.trim()) {
    message.warning('请输入部门名称');
    return;
  }

  saving.value = true;
  try {
    if (modalMode.value === 'create') {
      await createOrganizationUnitApi({
        displayName: formState.displayName.trim(),
        parentId: formState.parentId || null,
      });
      message.success('组织节点已创建');
    } else if (modalMode.value === 'edit') {
      await updateOrganizationUnitApi(selectedNode.value!.id, {
        displayName: formState.displayName.trim(),
        parentId: formState.parentId || null,
      });
      message.success('组织节点已更新');
    } else {
      await moveOrganizationUnitApi(selectedNode.value!.id, formState.parentId || null);
      message.success('组织节点已移动');
    }

    modalVisible.value = false;
    await loadTree();
    await loadMembers();
  } finally {
    saving.value = false;
  }
}

async function handleDelete() {
  if (!selectedNode.value) {
    return;
  }

  await deleteOrganizationUnitApi(selectedNode.value.id);
  message.success('组织节点已删除');
  selectedNodeKey.value = '';
  await loadTree();
  await loadMembers();
}

function openTransfer(record: UserApi.UserListItem) {
  transferUser.value = record;
  transferTargetOrganizationUnitId.value = undefined;
  transferVisible.value = true;
}

async function handleTransfer() {
  if (!selectedNode.value || !transferUser.value) {
    return;
  }
  if (!transferTargetOrganizationUnitId.value) {
    message.warning('请选择目标部门');
    return;
  }

  transferLoading.value = true;
  try {
    await transferOrganizationMembersApi(selectedNode.value.id, {
      targetOrganizationUnitId: transferTargetOrganizationUnitId.value,
      userIds: [transferUser.value.id],
    });
    message.success(`已将 ${transferUser.value.displayName} 转移到新部门`);
    transferVisible.value = false;
    await loadTree();
    await loadMembers();
  } finally {
    transferLoading.value = false;
  }
}

watch(
  () => selectedNodeKey.value,
  async () => {
    await loadMembers();
  },
);

onMounted(async () => {
  await loadTree();
  await loadMembers();
});
</script>

<template>
  <Page
    description="组织架构页已支持新增、重命名、移动和删除组织节点，并联动查看当前节点成员。"
    title="组织架构"
  >
    <section class="platform-page">
      <div class="platform-page__grid">
        <Card class="platform-page__card" title="组织树">
          <template #extra>
            <Button type="primary" @click="openCreateRoot">新增节点</Button>
          </template>

          <Skeleton :loading="loadingTree" active>
            <div v-if="organizationTree.length === 0" class="platform-page__empty">
              <Empty description="未找到组织树数据" />
            </div>
            <Tree
              v-else
              block-node
              default-expand-all
              :selectedKeys="selectedNodeKeys"
              :tree-data="normalizeTree(organizationTree)"
              @select="(keys) => { selectedNodeKey = `${keys[0] ?? ''}`; }"
            />
          </Skeleton>
        </Card>

        <div class="platform-page__stack">
          <Card class="platform-page__card" title="节点概览">
            <template #extra>
              <div class="toolbar">
                <Button type="primary" @click="openCreateChild">新增节点</Button>
                <Button :disabled="!selectedNode" @click="openEdit">编辑</Button>
                <Button :disabled="!selectedNode" @click="openMove">移动</Button>
                <Button danger :disabled="!selectedNode" @click="handleDelete">删除</Button>
              </div>
            </template>

            <div v-if="selectedNode" class="summary-panel">
              <div class="summary-panel__main">
                <span>当前节点</span>
                <strong>{{ selectedNode.displayName }}</strong>
                <p>{{ selectedNode.disabled ? '仅可见祖先节点' : '当前角色可操作范围' }}</p>
              </div>
              <div class="summary-panel__facts">
                <article>
                  <span>层级编码</span>
                  <strong>{{ selectedNode.code }}</strong>
                </article>
                <article>
                  <span>直属成员</span>
                  <strong>{{ selectedNode.directMemberCount }}</strong>
                </article>
                <article>
                  <span>成员总数</span>
                  <strong>{{ selectedNode.memberCount }}</strong>
                </article>
              </div>
            </div>
            <div v-else class="platform-page__empty platform-page__empty--small">
              <Empty description="请选择组织节点" />
            </div>
          </Card>

          <Card class="platform-page__card" title="部门成员">
            <template #extra>
              <div class="toolbar">
                <Input
                  v-model:value="keyword"
                  allow-clear
                  placeholder="按账号/姓名/邮箱筛选"
                  @pressEnter="loadMembers"
                />
                <Button type="primary" @click="loadMembers">查询</Button>
              </div>
            </template>

            <Table
              :columns="columns"
              :data-source="users"
              :loading="loadingMembers"
              :pagination="{ pageSize: 8 }"
              row-key="id"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'roles'">
                  <div class="tag-list">
                    <Tag v-for="role in record.roles" :key="role" color="blue">
                      {{ role }}
                    </Tag>
                  </div>
                </template>
                <template v-else-if="column.key === 'organizationUnitNames'">
                  <div class="tag-list">
                    <Tag
                      v-for="organizationUnitName in record.organizationUnitNames"
                      :key="organizationUnitName"
                    >
                      {{ organizationUnitName }}
                    </Tag>
                  </div>
                </template>
                <template v-else-if="column.key === 'isActive'">
                  <Tag :color="record.isActive ? 'green' : 'default'">
                    {{ record.isActive ? '启用' : '停用' }}
                  </Tag>
                </template>
                <template v-else-if="column.key === 'actions'">
                  <Button size="small" type="link" @click="openTransfer(record as UserApi.UserListItem)">
                    转移成员
                  </Button>
                </template>
              </template>
            </Table>
          </Card>
        </div>
      </div>
    </section>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="
        modalMode === 'create'
          ? '新增组织节点'
          : modalMode === 'edit'
            ? '编辑组织节点'
            : '移动组织节点'
      "
      @ok="handleSubmit"
    >
      <Form layout="vertical">
        <Form.Item v-if="modalMode !== 'move'" label="部门名称" required>
          <Input v-model:value="formState.displayName" :maxlength="64" />
        </Form.Item>
        <Form.Item :label="modalMode === 'create' ? '父级节点' : '移动到父级节点'">
          <TreeSelect
            v-model:value="formState.parentId"
            allow-clear
            placeholder="为空表示根节点"
            :tree-data="normalizeTreeSelect(organizationTree)"
            tree-default-expand-all
          />
        </Form.Item>
      </Form>
    </Modal>

    <Modal
      v-model:open="transferVisible"
      :confirm-loading="transferLoading"
      title="转移部门成员"
      @ok="handleTransfer"
    >
      <Form layout="vertical">
        <Form.Item label="当前成员">
          <Input :value="transferUser?.displayName || '-'" disabled />
        </Form.Item>
        <Form.Item label="目标部门" required>
          <TreeSelect
            v-model:value="transferTargetOrganizationUnitId"
            placeholder="选择转移后的部门"
            :tree-data="normalizeTreeSelect(organizationTree)"
            tree-default-expand-all
          />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-page {
  --platform-surface: var(--ant-color-bg-container, hsl(var(--card)));
  --platform-surface-alt: var(--ant-color-bg-elevated, var(--platform-surface));
  --platform-surface-soft: var(--ant-color-fill-quaternary, hsl(var(--accent)));
  --platform-border: var(--ant-color-border-secondary, hsl(var(--border)));
  --platform-text: var(--ant-color-text, hsl(var(--foreground)));
  --platform-text-secondary: var(
    --ant-color-text-secondary,
    hsl(var(--muted-foreground))
  );
  --platform-shadow: var(--ant-box-shadow-secondary, 0 18px 50px rgb(15 23 42 / 6%));

  display: flex;
  flex-direction: column;
  color: var(--platform-text);
}

.platform-page__grid {
  display: grid;
  gap: 16px;
  grid-template-columns: 340px minmax(0, 1fr);
}

.platform-page__stack {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-page__card {
  border: 1px solid var(--platform-border);
  border-radius: 20px;
  background: var(--platform-surface);
  box-shadow: var(--platform-shadow);
  color: inherit;
}

.platform-page__card :deep(.ant-card-head),
.platform-page__card :deep(.ant-card-body) {
  background: transparent;
  color: inherit;
}

.platform-page__card :deep(.ant-card-head) {
  border-bottom-color: var(--platform-border);
}

.platform-page__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

.platform-page__empty--small {
  min-height: 120px;
}

.summary-panel {
  display: grid;
  gap: 14px;
  grid-template-columns: minmax(220px, 0.9fr) minmax(0, 1.6fr);
}

.summary-panel__main,
.summary-panel__facts article {
  border: 1px solid var(--platform-border);
  border-radius: 16px;
  background: linear-gradient(
    180deg,
    var(--platform-surface-alt) 0%,
    var(--platform-surface-soft) 100%
  );
}

.summary-panel__main {
  padding: 16px;
}

.summary-panel__main span,
.summary-panel__facts span {
  display: block;
  margin-bottom: 8px;
  color: var(--platform-text-secondary);
  font-size: 12px;
}

.summary-panel__main strong {
  color: var(--platform-text);
  font-size: 22px;
  line-height: 1.6;
}

.summary-panel__main p {
  margin: 8px 0 0;
  color: var(--platform-text-secondary);
}

.summary-panel__facts {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.summary-panel__facts article {
  padding: 14px;
}

.summary-panel__facts strong {
  display: block;
  overflow: hidden;
  color: var(--platform-text);
  font-size: 18px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.toolbar,
.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

@media (max-width: 1080px) {
  .platform-page__grid,
  .summary-panel,
  .summary-panel__facts {
    grid-template-columns: 1fr;
  }
}
</style>
