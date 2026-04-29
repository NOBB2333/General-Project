<script lang="ts" setup>
import type { OnlineUserApi, OrganizationApi, RoleApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Descriptions,
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
  forceLogoutOnlineUserApi,
  getOnlineUserListApi,
  getOrganizationTreeApi,
  getRoleListApi,
  getUserListApi,
  resetUserPasswordApi,
  updateUserApi,
} from '#/api/core';

defineOptions({ name: 'PlatformUserPage' });

const columns = [
  { dataIndex: 'displayName', key: 'displayName', title: '姓名', width: 140 },
  { dataIndex: 'username', key: 'username', title: '账号', width: 160 },
  { dataIndex: 'employeeNo', key: 'employeeNo', title: '工号', width: 120 },
  { dataIndex: 'phoneNumber', key: 'phoneNumber', title: '手机号', width: 140 },
  { dataIndex: 'email', key: 'email', title: '邮箱', width: 220 },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'externalSource', key: 'externalSource', title: '外部来源', width: 120 },
  { dataIndex: 'roles', key: 'roles', title: '角色', width: 180 },
  { dataIndex: 'organizationUnitNames', key: 'organizationUnitNames', title: '部门', width: 200 },
  { dataIndex: 'isOnline', key: 'isOnline', title: '在线', width: 90 },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 210 },
];

const onlineColumns = [
  { dataIndex: 'userName', key: 'userName', title: '账号' },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'device', key: 'device', title: '设备', width: 120 },
  { dataIndex: 'browser', key: 'browser', title: '浏览器' },
  { dataIndex: 'ipAddress', key: 'ipAddress', title: 'IP', width: 160 },
  { dataIndex: 'signedInAt', key: 'signedInAt', title: '登录时间', width: 200 },
  { dataIndex: 'lastAccessedAt', key: 'lastAccessedAt', title: '最近活动', width: 200 },
  { key: 'actions', title: '操作', width: 120 },
];

interface TreeSelectNode {
  children?: TreeSelectNode[];
  title: string;
  value: string;
}

const keyword = ref('');
const loading = ref(false);
const modalVisible = ref(false);
const resetPasswordSaving = ref(false);
const resetPasswordValue = ref('');
const resetPasswordVisible = ref(false);
const saving = ref(false);
const editingUserId = ref<null | string>(null);
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const organizationUnitId = ref<string>();
const statusFilter = ref<string>();
const roleName = ref<string>();
const roleItems = ref<RoleApi.RoleItem[]>([]);
const users = ref<UserApi.UserListItem[]>([]);
const onlineUsers = ref<OnlineUserApi.OnlineUserItem[]>([]);
const onlineVisible = ref(false);
const onlineLoading = ref(false);
const mappingVisible = ref(false);
const activeMappingUser = ref<null | UserApi.UserListItem>(null);
const activeResetUser = ref<null | UserApi.UserListItem>(null);
const phoneNumberPattern = /^1[3-9]\d{9}$/;
const usernamePattern = /^[A-Za-z][A-Za-z0-9._-]{2,63}$/;

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
  const [treeResult, roleResult] = await Promise.allSettled([
    getOrganizationTreeApi(),
    getRoleListApi(),
  ]);
  if (treeResult.status === 'fulfilled') {
    organizationTree.value = treeResult.value;
  }
  if (roleResult.status === 'fulfilled') {
    roleItems.value = roleResult.value;
  }
  if (treeResult.status === 'rejected' || roleResult.status === 'rejected') {
    message.warning('筛选条件部分加载失败，稍后可重试。');
  }
}

async function loadUsers() {
  loading.value = true;
  try {
    users.value = await getUserListApi({
      keyword: keyword.value || undefined,
      isActive:
        statusFilter.value === undefined
          ? undefined
          : statusFilter.value === 'true',
      organizationUnitId: organizationUnitId.value || undefined,
      roleName: roleName.value || undefined,
    });
  } catch {
    users.value = [];
    message.error('用户列表加载失败');
  } finally {
    loading.value = false;
  }
}

async function loadOnlineUsers() {
  onlineLoading.value = true;
  try {
    onlineUsers.value = await getOnlineUserListApi();
  } catch {
    onlineUsers.value = [];
    message.error('在线会话加载失败');
  } finally {
    onlineLoading.value = false;
  }
}

async function refreshUserData() {
  await Promise.allSettled([loadUsers(), loadOnlineUsers()]);
}

async function handleForceLogout(record: OnlineUserApi.OnlineUserItem) {
  await forceLogoutOnlineUserApi(record.userId);
  message.success(`已强制下线账号 ${record.userName}`);
  await refreshUserData();
}

function openOnlineDrawer() {
  onlineVisible.value = true;
  void loadOnlineUsers();
}

function openMappingDrawer(record: UserApi.UserListItem) {
  activeMappingUser.value = record;
  mappingVisible.value = true;
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
  formState.password = '';
  formState.phoneNumber = '';
  formState.roleNames = [];
  formState.username = '';
  modalVisible.value = true;
}

function openResetPassword(record: UserApi.UserListItem) {
  activeResetUser.value = record;
  resetPasswordValue.value = '';
  resetPasswordVisible.value = true;
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
  if (!usernamePattern.test(formState.username.trim())) {
    message.warning('账号必须以英文字母开头，只能包含英文、数字、点、下划线或中划线，长度 3-64 位');
    return;
  }
  if (formState.email?.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formState.email.trim())) {
    message.warning('邮箱格式不正确');
    return;
  }
  if (formState.phoneNumber?.trim() && !phoneNumberPattern.test(formState.phoneNumber.trim())) {
    message.warning('手机号格式不正确，请输入 11 位中国大陆手机号');
    return;
  }
  if (!editingUserId.value && !formState.password.trim()) {
    message.warning('创建用户时请输入初始密码');
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
    await refreshUserData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await deleteUserApi(id);
  message.success('用户已删除');
  await refreshUserData();
}

async function handleResetPassword() {
  if (!activeResetUser.value) {
    return;
  }
  if (!resetPasswordValue.value.trim()) {
    message.warning('请输入新密码');
    return;
  }

  resetPasswordSaving.value = true;
  try {
    await resetUserPasswordApi(activeResetUser.value.id, {
      newPassword: resetPasswordValue.value.trim(),
    });
    message.success(`已重置账号 ${activeResetUser.value.username} 的密码`);
    resetPasswordVisible.value = false;
  } finally {
    resetPasswordSaving.value = false;
  }
}

async function handleToggleStatus(record: UserApi.UserListItem, checked: boolean) {
  const payload = {
    displayName: record.displayName,
    email: record.email,
    employeeNo: record.employeeNo || undefined,
    externalSource: record.externalSource || undefined,
    externalUserId: record.externalUserId || undefined,
    isActive: checked,
    organizationUnitId:
      findOrganizationUnitIdByName(record.organizationUnitNames[0] || '') || null,
    phoneNumber: record.phoneNumber || undefined,
    roleNames: [...record.roles],
    username: record.username,
  };

  await updateUserApi(record.id, payload);
  message.success(`账号已${checked ? '启用' : '停用'}`);
  await refreshUserData();
}

async function resetFilters() {
  keyword.value = '';
  organizationUnitId.value = undefined;
  roleName.value = undefined;
  statusFilter.value = undefined;
  await loadUsers();
}

onMounted(async () => {
  await loadFilters();
  await refreshUserData();
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
            <Select
              v-model:value="roleName"
              allow-clear
              class="toolbar__select"
              placeholder="按角色筛选"
              :options="roleOptions"
            />
            <Select
              v-model:value="statusFilter"
              allow-clear
              class="toolbar__select"
              placeholder="按状态筛选"
              :options="[
                { label: '启用', value: 'true' },
                { label: '停用', value: 'false' },
              ]"
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
          :scroll="{ x: 1480 }"
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
            <template v-else-if="column.key === 'tenantName'">
              <Tag v-if="(record as UserApi.UserListItem).tenantName" color="blue">
                {{ (record as UserApi.UserListItem).tenantName }}
              </Tag>
              <span v-else>—</span>
            </template>
            <template v-else-if="column.key === 'isActive'">
              <Switch
                :checked="record.isActive"
                checked-children="启用"
                un-checked-children="停用"
                @change="(checked) => handleToggleStatus(record as UserApi.UserListItem, checked === true)"
              />
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
                <Button
                  size="small"
                  type="link"
                  @click="openMappingDrawer(record as UserApi.UserListItem)"
                >
                  映射记录
                </Button>
                <Button
                  size="small"
                  type="link"
                  @click="openResetPassword(record as UserApi.UserListItem)"
                >
                  重置密码
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
            <Tag v-if="record.tenantName" color="blue">{{ record.tenantName }}</Tag>
            <span v-else>—</span>
          </template>
          <template v-else-if="column.key === 'lastAccessedAt'">
            {{ record.lastAccessedAt ? new Date(record.lastAccessedAt).toLocaleString() : '-' }}
          </template>
          <template v-else-if="column.key === 'signedInAt'">
            {{ record.signedInAt ? new Date(record.signedInAt).toLocaleString() : '-' }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <Popconfirm
              title="确认强制下线该用户？"
              @confirm="handleForceLogout(record as OnlineUserApi.OnlineUserItem)"
            >
              <Button
                :disabled="!(record as OnlineUserApi.OnlineUserItem).canForceLogout"
                danger
                size="small"
                type="link"
              >
                强制下线
              </Button>
            </Popconfirm>
          </template>
        </template>
      </Table>
    </Drawer>

    <Drawer
      v-model:open="mappingVisible"
      destroy-on-close
      placement="right"
      title="账号映射记录"
      width="760"
    >
      <Descriptions v-if="activeMappingUser" :column="2" bordered class="mb-4" size="small">
        <Descriptions.Item label="姓名">{{ activeMappingUser.displayName }}</Descriptions.Item>
        <Descriptions.Item label="账号">{{ activeMappingUser.username }}</Descriptions.Item>
        <Descriptions.Item label="当前外部来源">
          {{ activeMappingUser.externalSource || '-' }}
        </Descriptions.Item>
        <Descriptions.Item label="当前外部标识">
          {{ activeMappingUser.externalUserId || '-' }}
        </Descriptions.Item>
      </Descriptions>

      <div
        v-if="!activeMappingUser || activeMappingUser.externalAccounts.length === 0"
        class="platform-page__empty"
      >
        <Empty description="暂无映射记录" />
      </div>
      <Table
        v-else
        :columns="[
          { dataIndex: 'externalSource', key: 'externalSource', title: '外部系统', width: 120 },
          { dataIndex: 'externalUserId', key: 'externalUserId', title: '外部账号标识', width: 220 },
          { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
          { dataIndex: 'boundAt', key: 'boundAt', title: '绑定时间', width: 180 },
          { dataIndex: 'lastSyncedAt', key: 'lastSyncedAt', title: '最近同步', width: 180 },
          { dataIndex: 'remark', key: 'remark', title: '备注', width: 220 },
        ]"
        :data-source="activeMappingUser?.externalAccounts || []"
        :pagination="{ pageSize: 8 }"
        row-key="id"
        :scroll="{ x: 980 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'status'">
            <Tag :color="record.status === 'active' ? 'success' : 'default'">
              {{ record.status === 'active' ? '有效' : record.status }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'boundAt'">
            {{ record.boundAt ? new Date(record.boundAt).toLocaleString() : '-' }}
          </template>
          <template v-else-if="column.key === 'lastSyncedAt'">
            {{ record.lastSyncedAt ? new Date(record.lastSyncedAt).toLocaleString() : '-' }}
          </template>
          <template v-else-if="column.key === 'remark'">
            {{ record.remark || '-' }}
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
            <Input
              v-model:value="formState.username"
              :maxlength="64"
              placeholder="英文账号，例如 zhangsan 或 zhang.san"
            />
          </Form.Item>
          <Form.Item label="显示名" required>
            <Input v-model:value="formState.displayName" :maxlength="64" />
          </Form.Item>
          <Form.Item class="form-grid__full" label="邮箱">
            <Input v-model:value="formState.email" :maxlength="256" />
          </Form.Item>
          <Form.Item label="工号">
            <Input
              v-model:value="formState.employeeNo"
              :maxlength="64"
              placeholder="留空自动生成，也可手动录入已有工号"
            />
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
          <Form.Item label="密码" :required="!editingUserId">
            <Input.Password
              v-model:value="formState.password"
              :maxlength="128"
              :placeholder="editingUserId ? '留空则不修改密码' : '请输入初始密码'"
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

    <Modal
      v-model:open="resetPasswordVisible"
      :confirm-loading="resetPasswordSaving"
      title="重置用户密码"
      @ok="handleResetPassword"
    >
      <Form layout="vertical">
        <Form.Item label="目标账号">
          <Input :value="activeResetUser?.username || '-'" disabled />
        </Form.Item>
        <Form.Item label="新密码" required>
          <Input.Password
            v-model:value="resetPasswordValue"
            :maxlength="128"
            placeholder="请输入新的初始化密码"
          />
        </Form.Item>
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
