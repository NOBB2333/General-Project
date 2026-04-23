<script lang="ts" setup>
import type { OnlineUserApi, OrganizationApi, RoleApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Select,
  Switch,
  Table,
  Tag,
  TreeSelect,
  message,
} from 'ant-design-vue';

import {
  createUserApi,
  deleteUserApi,
  getOnlineUserListApi,
  getOrganizationTreeApi,
  getRoleListApi,
  getUserListApi,
  updateUserApi,
} from '#/api/core';

defineOptions({ name: 'PlatformUserPage' });

const columns = [
  { dataIndex: 'displayName', key: 'displayName', title: '姓名' },
  { dataIndex: 'username', key: 'username', title: '账号' },
  { dataIndex: 'employeeNo', key: 'employeeNo', title: '工号', width: 120 },
  { dataIndex: 'phoneNumber', key: 'phoneNumber', title: '手机号', width: 140 },
  { dataIndex: 'email', key: 'email', title: '邮箱' },
  { dataIndex: 'externalSource', key: 'externalSource', title: '外部来源', width: 120 },
  { dataIndex: 'roles', key: 'roles', title: '角色' },
  { dataIndex: 'organizationUnitNames', key: 'organizationUnitNames', title: '部门' },
  { dataIndex: 'isOnline', key: 'isOnline', title: '在线', width: 90 },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 180 },
];

const onlineColumns = [
  { dataIndex: 'userName', key: 'userName', title: '账号' },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'device', key: 'device', title: '设备', width: 120 },
  { dataIndex: 'browser', key: 'browser', title: '浏览器' },
  { dataIndex: 'ipAddress', key: 'ipAddress', title: 'IP', width: 160 },
  { dataIndex: 'lastAccessedAt', key: 'lastAccessedAt', title: '最近活动', width: 200 },
];

interface TreeSelectNode {
  children?: TreeSelectNode[];
  title: string;
  value: string;
}

const keyword = ref('');
const loading = ref(false);
const modalVisible = ref(false);
const saving = ref(false);
const editingUserId = ref<null | string>(null);
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const organizationUnitId = ref<string>();
const roleItems = ref<RoleApi.RoleItem[]>([]);
const users = ref<UserApi.UserListItem[]>([]);
const onlineUsers = ref<OnlineUserApi.OnlineUserItem[]>([]);
const onlineVisible = ref(false);
const onlineLoading = ref(false);

const formState = reactive<any>({
  displayName: '',
  email: '',
  isActive: true,
  organizationUnitId: undefined,
  password: '',
  roleNames: [],
  username: '',
});

const metrics = computed(() => [
  { label: '用户总数', value: users.value.length },
  {
    label: '启用账号',
    value: users.value.filter((item) => item.isActive).length,
  },
  {
    label: '涉及部门',
    value: new Set(users.value.flatMap((item) => item.organizationUnitNames)).size,
  },
  {
    label: '在线会话',
    value: onlineUsers.value.length,
  },
]);

const roleOptions = computed(() =>
  roleItems.value.map((item) => ({ label: item.name, value: item.name })),
);

function normalizeTree(items: OrganizationApi.OrganizationTreeItem[]): TreeSelectNode[] {
  return items.map((item) => ({
    children: normalizeTree(item.children || []),
    title: item.displayName,
    value: item.id,
  }));
}

async function loadFilters() {
  const [tree, roles] = await Promise.all([getOrganizationTreeApi(), getRoleListApi()]);
  organizationTree.value = tree;
  roleItems.value = roles;
}

async function loadUsers() {
  loading.value = true;
  try {
    users.value = await getUserListApi({
      keyword: keyword.value || undefined,
      organizationUnitId: organizationUnitId.value || undefined,
    });
  } finally {
    loading.value = false;
  }
}

async function loadOnlineUsers() {
  onlineLoading.value = true;
  try {
    onlineUsers.value = await getOnlineUserListApi();
  } finally {
    onlineLoading.value = false;
  }
}

function openOnlineDrawer() {
  onlineVisible.value = true;
  void loadOnlineUsers();
}

function openCreate() {
  editingUserId.value = null;
  formState.displayName = '';
  formState.email = '';
  formState.employeeNo = '';
  formState.externalSource = '';
  formState.externalUserId = '';
  formState.isActive = true;
  formState.organizationUnitId = undefined;
  formState.password = '1q2w3E*';
  formState.phoneNumber = '';
  formState.roleNames = [];
  formState.username = '';
  modalVisible.value = true;
}

function openEdit(record: UserApi.UserListItem) {
  editingUserId.value = record.id;
  formState.displayName = record.displayName;
  formState.email = record.email;
  formState.employeeNo = record.employeeNo || '';
  formState.externalSource = record.externalSource || '';
  formState.externalUserId = record.externalUserId || '';
  formState.isActive = record.isActive;
  formState.organizationUnitId =
    findOrganizationUnitIdByName(record.organizationUnitNames[0] || '') || undefined;
  formState.password = '';
  formState.phoneNumber = record.phoneNumber || '';
  formState.roleNames = [...record.roles];
  formState.username = record.username;
  modalVisible.value = true;
}

function findOrganizationUnitIdByName(displayName: string): null | string {
  const queue = [...organizationTree.value];
  while (queue.length > 0) {
    const current = queue.shift();
    if (!current) {
      continue;
    }
    if (current.displayName === displayName) {
      return current.id;
    }
    queue.push(...(current.children || []));
  }
  return null;
}

async function handleSubmit() {
  if (!formState.username.trim() || !formState.displayName.trim()) {
    message.warning('请填写用户名和显示名');
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...formState,
      organizationUnitId: formState.organizationUnitId || null,
      password: formState.password || undefined,
    };

    if (editingUserId.value) {
      await updateUserApi(editingUserId.value, payload);
      message.success('用户已更新');
    } else {
      await createUserApi(payload);
      message.success('用户已创建');
    }

    modalVisible.value = false;
    await Promise.all([loadUsers(), loadOnlineUsers()]);
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await deleteUserApi(id);
  message.success('用户已删除');
  await Promise.all([loadUsers(), loadOnlineUsers()]);
}

async function resetFilters() {
  keyword.value = '';
  organizationUnitId.value = undefined;
  await loadUsers();
}

onMounted(async () => {
  await Promise.all([loadFilters(), loadUsers(), loadOnlineUsers()]);
});
</script>

<template>
  <Page
    description="用户管理页已接入新增、编辑、删除、角色分配，并在右侧抽屉查看在线会话。"
    title="用户管理"
  >
    <section class="platform-page">
      <div class="platform-page__metrics">
        <article
          v-for="metric in metrics"
          :key="metric.label"
          class="platform-page__metric"
        >
          <span>{{ metric.label }}</span>
          <strong>{{ metric.value }}</strong>
        </article>
      </div>

      <Card class="platform-page__card" title="用户列表">
        <template #extra>
          <div class="toolbar">
            <Input
              v-model:value="keyword"
              allow-clear
              placeholder="按账号/姓名/邮箱筛选"
              @pressEnter="loadUsers"
            />
            <TreeSelect
              v-model:value="organizationUnitId"
              allow-clear
              class="toolbar__select"
              placeholder="按部门筛选"
              :tree-data="normalizeTree(organizationTree)"
              tree-default-expand-all
            />
            <Button type="primary" @click="loadUsers">查询</Button>
            <Button @click="resetFilters">重置</Button>
            <Button @click="openOnlineDrawer">在线会话</Button>
            <Button type="primary" @click="openCreate">新增用户</Button>
          </div>
        </template>

        <div v-if="!loading && users.length === 0" class="platform-page__empty">
          <Empty description="未找到用户数据" />
        </div>
        <Table
          v-else
          :columns="columns"
          :data-source="users"
          :loading="loading"
          :pagination="{ pageSize: 10 }"
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
            <template v-else-if="column.key === 'isOnline'">
              <Tag :color="record.isOnline ? 'success' : 'default'">
                {{ record.isOnline ? '在线' : '离线' }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'actions'">
              <div class="action-list">
                <Button size="small" type="link" @click="openEdit(record as UserApi.UserListItem)">
                  编辑
                </Button>
                <Popconfirm title="确认删除该用户？" @confirm="handleDelete(record.id)">
                  <Button danger size="small" type="link">删除</Button>
                </Popconfirm>
              </div>
            </template>
          </template>
        </Table>
      </Card>
    </section>

    <Drawer
      v-model:open="onlineVisible"
      destroy-on-close
      placement="right"
      title="在线会话"
      width="720"
    >
      <div v-if="!onlineLoading && onlineUsers.length === 0" class="platform-page__empty">
        <Empty description="暂无在线用户" />
      </div>
      <Table
        v-else
        :columns="onlineColumns"
        :data-source="onlineUsers"
        :loading="onlineLoading"
        :pagination="{ pageSize: 8 }"
        row-key="id"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'tenantName'">
            <Tag color="blue">{{ record.tenantName || '默认租户' }}</Tag>
          </template>
          <template v-else-if="column.key === 'lastAccessedAt'">
            {{ record.lastAccessedAt ? new Date(record.lastAccessedAt).toLocaleString() : '-' }}
          </template>
        </template>
      </Table>
    </Drawer>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="editingUserId ? '编辑用户' : '新增用户'"
      width="720px"
      @ok="handleSubmit"
    >
      <Form layout="vertical">
        <div class="form-grid">
          <Form.Item label="用户名" required>
            <Input v-model:value="formState.username" :maxlength="64" />
          </Form.Item>
          <Form.Item label="显示名" required>
            <Input v-model:value="formState.displayName" :maxlength="64" />
          </Form.Item>
          <Form.Item class="form-grid__full" label="邮箱">
            <Input v-model:value="formState.email" :maxlength="256" />
          </Form.Item>
          <Form.Item label="工号">
            <Input v-model:value="formState.employeeNo" :maxlength="64" />
          </Form.Item>
          <Form.Item label="手机号">
            <Input v-model:value="formState.phoneNumber" :maxlength="32" />
          </Form.Item>
          <Form.Item label="外部来源">
            <Input v-model:value="formState.externalSource" :maxlength="64" placeholder="钉钉 / 飞书 / 企微" />
          </Form.Item>
          <Form.Item label="外部账号标识">
            <Input v-model:value="formState.externalUserId" :maxlength="128" />
          </Form.Item>
          <Form.Item label="密码">
            <Input.Password
              v-model:value="formState.password"
              :maxlength="128"
              :placeholder="editingUserId ? '留空则不修改密码' : '默认密码 1q2w3E*'"
            />
          </Form.Item>
          <Form.Item label="状态">
            <Switch v-model:checked="formState.isActive" />
          </Form.Item>
          <Form.Item class="form-grid__full" label="所属部门">
            <TreeSelect
              v-model:value="formState.organizationUnitId"
              allow-clear
              placeholder="选择部门"
              :tree-data="normalizeTree(organizationTree)"
              tree-default-expand-all
            />
          </Form.Item>
          <Form.Item class="form-grid__full" label="角色">
            <Select
              v-model:value="formState.roleNames"
              mode="multiple"
              placeholder="选择一个或多个角色"
              :options="roleOptions"
            />
          </Form.Item>
        </div>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-page {
  --platform-surface: var(--ant-color-bg-container, hsl(var(--card)));
  --platform-border: var(--ant-color-border-secondary, hsl(var(--border)));
  --platform-text: var(--ant-color-text, hsl(var(--foreground)));
  --platform-text-secondary: var(
    --ant-color-text-secondary,
    hsl(var(--muted-foreground))
  );
  --platform-shadow: var(--ant-box-shadow-secondary, 0 18px 50px rgb(15 23 42 / 6%));

  display: flex;
  flex-direction: column;
  gap: 16px;
  color: var(--platform-text);
}

.platform-page__metrics {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.platform-page__metric,
.platform-page__card {
  border: 1px solid var(--platform-border);
  border-radius: 20px;
  background: var(--platform-surface);
  box-shadow: var(--platform-shadow);
  color: inherit;
}

.platform-page__metric {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px 18px;
}

.platform-page__metric span {
  color: var(--platform-text-secondary);
  font-size: 13px;
}

.platform-page__metric strong {
  color: var(--platform-text);
  font-size: 30px;
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
  min-height: 240px;
  place-items: center;
}

.toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.toolbar :deep(.ant-input),
.toolbar__select {
  min-width: 220px;
}

.tag-list,
.action-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.form-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.form-grid__full {
  grid-column: 1 / -1;
}

@media (max-width: 1080px) {
  .platform-page__metrics,
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
