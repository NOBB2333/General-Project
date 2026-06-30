<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { OnlineUserApi, OrganizationApi, RoleApi, UserApi } from '#/api/core';

import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Col,
  Descriptions,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Row,
  Select,
  Skeleton,
  Switch,
  Tag,
  Tree,
  TreeSelect,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createUserApi,
  deleteUserApi,
  forceLogoutOnlineUserApi,
  getOnlineUserListApi,
  getOrganizationTreeApi,
  getRoleOptionsApi,
  getUserListApi,
  resetUserPasswordApi,
  updateUserApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';
import {
  normalizePlatformTree,
  normalizePlatformTreeSelect,
  type PlatformTreeNode,
} from '#/composables/platform/use-tree-normalization';

defineOptions({ name: 'PlatformUserPage' });

const USER_MANAGE_CODE = 'Platform.User.Manage';

const { hasAccessByCodes } = useAccess();
const canManageUsers = computed(() => hasAccessByCodes([USER_MANAGE_CODE]));

const baseColumns = [
  { field: 'displayName', sortable: true, title: '姓名', width: 140 },
  { field: 'username', sortable: true, title: '账号', width: 160 },
  { field: 'employeeNo', sortable: true, title: '工号', width: 120 },
  { field: 'phoneNumber', sortable: true, title: '手机号', width: 140 },
  { field: 'email', sortable: true, title: '邮箱', width: 220 },
  { field: 'tenantName', slots: { default: 'tenantName' }, sortable: true, title: '租户', width: 120 },
  { field: 'externalSource', sortable: true, title: '外部来源', width: 120 },
  { field: 'roles', slots: { default: 'roles' }, title: '角色', width: 180 },
  { field: 'organizationUnitNames', slots: { default: 'organizationUnitNames' }, title: '部门', width: 200 },
  { field: 'isOnline', slots: { default: 'isOnline' }, sortable: true, title: '在线', width: 90 },
  { field: 'isActive', slots: { default: 'isActive' }, sortable: true, title: '状态', width: 100 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 280 },
] satisfies VxeGridProps<UserApi.UserListItem>['columns'];

const columns = computed<VxeGridProps<UserApi.UserListItem>['columns']>(() =>
  canManageUsers.value
    ? baseColumns
    : baseColumns.filter((item) => item.field !== 'actions'),
);

const onlineColumns = [
  { align: 'left', field: 'userName', title: '账号', width: 160 },
  { field: 'tenantName', slots: { default: 'tenantName' }, title: '租户', width: 120 },
  { field: 'device', title: '设备', width: 120 },
  { field: 'browser', title: '浏览器', width: 140 },
  { field: 'ipAddress', title: 'IP', width: 160 },
  { field: 'signedInAt', slots: { default: 'signedInAt' }, title: '登录时间', width: 200 },
  { field: 'lastAccessedAt', slots: { default: 'lastAccessedAt' }, title: '最近活动', width: 200 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<OnlineUserApi.OnlineUserItem>['columns'];

const mappingColumns = [
  { field: 'externalSource', title: '外部系统', width: 120 },
  { align: 'left', field: 'externalUserId', title: '外部账号标识', width: 220 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 100 },
  { field: 'boundAt', slots: { default: 'boundAt' }, title: '绑定时间', width: 180 },
  { field: 'lastSyncedAt', slots: { default: 'lastSyncedAt' }, title: '最近同步', width: 180 },
  { align: 'left', field: 'remark', slots: { default: 'remark' }, title: '备注', width: 220 },
] satisfies VxeGridProps<UserApi.ExternalAccountMapping>['columns'];

const keyword = ref('');
const userNameFilter = ref('');
const displayNameFilter = ref('');
const emailFilter = ref('');
const employeeNoFilter = ref('');
const phoneNumberFilter = ref('');
const loading = ref(false);
const createDrawerVisible = ref(false);
const resetPasswordSaving = ref(false);
const resetPasswordValue = ref('');
const resetPasswordVisible = ref(false);
const saving = ref(false);
const editingUserId = ref<null | string>(null);
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const organizationUnitId = ref<string>();
const organizationTreeKeyword = ref('');
const loadingTree = ref(true);
const statusFilter = ref<string>();
const roleName = ref<string>();
const roleItems = ref<RoleApi.RoleOption[]>([]);
const users = ref<UserApi.UserListItem[]>([]);
const totalCount = ref(0);
const pageIndex = ref(1);
const pageSize = ref(10);
const searchExpanded = ref(false);
const onlineUsers = ref<OnlineUserApi.OnlineUserItem[]>([]);
const onlinePagination = useLocalVxePagination(onlineUsers, { defaultPageSize: 10 });
const onlineVisible = ref(false);
const onlineLoading = ref(false);
const mappingVisible = ref(false);
const activeMappingUser = ref<null | UserApi.UserListItem>(null);
const externalAccounts = computed(() => activeMappingUser.value?.externalAccounts || []);
const mappingPagination = useLocalVxePagination(externalAccounts, { defaultPageSize: 10 });
const activeResetUser = ref<null | UserApi.UserListItem>(null);
const phoneNumberPattern = /^1[3-9]\d{9}$/;
const usernamePattern = /^[A-Za-z][A-Za-z0-9._-]{2,63}$/;
let keywordSearchTimer: ReturnType<typeof setTimeout> | undefined;
let suppressFilterWatch = false;
const { actionLoadingKey, runAction } = useActionLoading();

interface UserFormState {
  displayName: string;
  email: string;
  employeeNo: string;
  externalSource: string;
  externalUserId: string;
  isActive: boolean;
  organizationUnitId?: string;
  password: string;
  phoneNumber: string;
  roleNames: string[];
  username: string;
}

const formState = reactive<UserFormState>({
  displayName: '',
  email: '',
  employeeNo: '',
  externalSource: '',
  externalUserId: '',
  isActive: true,
  organizationUnitId: undefined,
  password: '',
  phoneNumber: '',
  roleNames: [],
  username: '',
});

const metrics = computed(() => [
  ...[
    { label: '用户总数', value: totalCount.value },
    {
      label: '当前页启用',
      value: users.value.filter((item) => item.isActive).length,
    },
    {
      label: '当前页部门',
      value: new Set(users.value.flatMap((item) => item.organizationUnitNames)).size,
    },
  ],
  ...(canManageUsers.value
    ? [
        {
          label: '在线会话',
          value: onlineUsers.value.length,
        },
      ]
    : []),
]);

const roleOptions = computed(() =>
  roleItems.value.map((item) => ({ label: item.name, value: item.name })),
);
const organizationNodes = computed(() =>
  normalizePlatformTree(filterOrganizationTree(organizationTree.value), buildTreeTitle),
);
const selectedOrganizationKeys = computed(() =>
  organizationUnitId.value ? [organizationUnitId.value] : [],
);
const userGridOptions = computed<VxeGridProps<UserApi.UserListItem>>(() => ({
  columns: columns.value,
  data: users.value,
  emptyText: '未找到用户数据',
  loading: loading.value,
  pagerConfig: {
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    total: totalCount.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));
const onlineGridOptions = computed<VxeGridProps<OnlineUserApi.OnlineUserItem>>(() => ({
  columns: onlineColumns,
  data: onlinePagination.pagedRows.value,
  emptyText: '暂无在线用户',
  loading: onlineLoading.value,
  pagerConfig: onlinePagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));
const mappingGridOptions = computed<VxeGridProps<UserApi.ExternalAccountMapping>>(() => ({
  columns: mappingColumns,
  data: mappingPagination.pagedRows.value,
  emptyText: '暂无映射记录',
  pagerConfig: mappingPagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const userGridEvents: NativeVxeGridEvents<UserApi.UserListItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadUsers();
  },
};
const onlineGridEvents: NativeVxeGridEvents<OnlineUserApi.OnlineUserItem> = {
  'page-change': onlinePagination.handlePageChange,
};
const mappingGridEvents: NativeVxeGridEvents<UserApi.ExternalAccountMapping> = {
  'page-change': mappingPagination.handlePageChange,
};

const [UserGrid] = useVbenVxeGrid<UserApi.UserListItem>({
  gridEvents: userGridEvents,
  gridOptions: {},
});
const [OnlineGrid] = useVbenVxeGrid<OnlineUserApi.OnlineUserItem>({ gridOptions: {} });
const [MappingGrid] = useVbenVxeGrid<UserApi.ExternalAccountMapping>({ gridOptions: {} });

function normalizeTree(items: OrganizationApi.OrganizationTreeItem[]): PlatformTreeNode[] {
  return normalizePlatformTreeSelect(items, (item) => item.displayName);
}

function buildTreeTitle(item: OrganizationApi.OrganizationTreeItem) {
  return item.disabled
    ? `${item.displayName}（仅可见）`
    : `${item.displayName}（${item.memberCount}）`;
}

function filterOrganizationTree(
  items: OrganizationApi.OrganizationTreeItem[],
): OrganizationApi.OrganizationTreeItem[] {
  const keyword = organizationTreeKeyword.value.trim().toLowerCase();
  if (!keyword) {
    return items;
  }

  const result: OrganizationApi.OrganizationTreeItem[] = [];
  for (const item of items) {
    const children = filterOrganizationTree(item.children || []);
    const matched =
      item.displayName.toLowerCase().includes(keyword) ||
      item.code.toLowerCase().includes(keyword);

    if (matched || children.length > 0) {
      result.push({
        ...item,
        children,
      });
    }
  }
  return result;
}

async function loadFilters() {
  loadingTree.value = true;
  const [treeResult, roleResult] = await Promise.allSettled([
    getOrganizationTreeApi(),
    getRoleOptionsApi(),
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
  loadingTree.value = false;
}

async function loadUsers() {
  loading.value = true;
  try {
    const result = await getUserListApi({
      displayName: displayNameFilter.value.trim() || undefined,
      email: emailFilter.value.trim() || undefined,
      employeeNo: employeeNoFilter.value.trim() || undefined,
      keyword: keyword.value || undefined,
      isActive:
        statusFilter.value === undefined
          ? undefined
          : statusFilter.value === 'true',
      maxResultCount: pageSize.value,
      organizationUnitId: organizationUnitId.value || undefined,
      phoneNumber: phoneNumberFilter.value.trim() || undefined,
      roleName: roleName.value || undefined,
      skipCount: (pageIndex.value - 1) * pageSize.value,
      userName: userNameFilter.value.trim() || undefined,
    });
    users.value = result.items;
    totalCount.value = result.totalCount;
  } catch {
    users.value = [];
    totalCount.value = 0;
    message.error('用户列表加载失败');
  } finally {
    loading.value = false;
  }
}

async function loadOnlineUsers() {
  if (!canManageUsers.value) {
    onlineUsers.value = [];
    return;
  }

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
  if (canManageUsers.value) {
    await Promise.allSettled([loadUsers(), loadOnlineUsers()]);
    return;
  }

  onlineUsers.value = [];
  await loadUsers();
}

async function handleForceLogout(record: OnlineUserApi.OnlineUserItem) {
  await runAction(`force-logout:${record.userId}`, async () => {
    await forceLogoutOnlineUserApi(record.userId);
    message.success(`已强制下线账号 ${record.userName}`);
    await refreshUserData();
  });
}

function openOnlineDrawer() {
  if (!canManageUsers.value) {
    return;
  }

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
  createDrawerVisible.value = true;
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
  createDrawerVisible.value = true;
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

    createDrawerVisible.value = false;
    await refreshUserData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await runAction(`delete:${id}`, async () => {
    await deleteUserApi(id);
    message.success('用户已删除');
    if (users.value.length === 1 && pageIndex.value > 1) {
      pageIndex.value -= 1;
    }
    await refreshUserData();
  });
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

  await runAction(`toggle:${record.id}`, async () => {
    await updateUserApi(record.id, payload);
    message.success(`账号已${checked ? '启用' : '停用'}`);
    await refreshUserData();
  });
}

function searchUsers() {
  if (keywordSearchTimer) {
    clearTimeout(keywordSearchTimer);
    keywordSearchTimer = undefined;
  }
  pageIndex.value = 1;
  void loadUsers();
}

function handleOrganizationSelect(keys: (number | string)[]) {
  organizationUnitId.value = keys.length > 0 ? `${keys[0]}` : undefined;
}

function clearOrganizationFilter() {
  if (organizationUnitId.value) {
    organizationUnitId.value = undefined;
    return;
  }

  searchUsers();
}

async function resetFilters() {
  suppressFilterWatch = true;
  keyword.value = '';
  userNameFilter.value = '';
  displayNameFilter.value = '';
  emailFilter.value = '';
  employeeNoFilter.value = '';
  phoneNumberFilter.value = '';
  organizationUnitId.value = undefined;
  roleName.value = undefined;
  statusFilter.value = undefined;
  suppressFilterWatch = false;
  pageIndex.value = 1;
  await loadUsers();
}

watch([
  organizationUnitId,
  roleName,
  statusFilter,
], () => {
  if (suppressFilterWatch) {
    return;
  }
  searchUsers();
});

watch([
  keyword,
  userNameFilter,
  displayNameFilter,
  emailFilter,
  employeeNoFilter,
  phoneNumberFilter,
], () => {
  if (suppressFilterWatch) {
    return;
  }
  if (keywordSearchTimer) {
    clearTimeout(keywordSearchTimer);
  }
  keywordSearchTimer = setTimeout(searchUsers, 350);
});

onMounted(async () => {
  await loadFilters();
  await refreshUserData();
});

onBeforeUnmount(() => {
  if (keywordSearchTimer) {
    clearTimeout(keywordSearchTimer);
  }
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

      <div class="platform-page__workspace">
        <Card :bordered="false" class="platform-page__org-card rounded-2xl" title="组织部门">
          <template #extra>
            <Button type="link" @click="clearOrganizationFilter">全部</Button>
          </template>
          <Skeleton :loading="loadingTree" active>
            <Input
              v-model:value="organizationTreeKeyword"
              allow-clear
              class="platform-page__org-search"
              placeholder="搜索部门"
            />
            <div v-if="organizationNodes.length === 0" class="platform-page__empty">
              <Empty :description="organizationTreeKeyword ? '未找到匹配部门' : '未找到组织树数据'" />
            </div>
            <Tree
              v-else
              block-node
              default-expand-all
              :selected-keys="selectedOrganizationKeys"
              :tree-data="organizationNodes"
              @select="handleOrganizationSelect"
            />
          </Skeleton>
        </Card>

        <div class="platform-page__main">
          <Card :bordered="false" class="platform-page__filter-card rounded-2xl" title="筛选条件">
            <template #extra>
              <Button type="link" @click="searchExpanded = !searchExpanded">
                {{ searchExpanded ? '收起' : '展开' }}
              </Button>
            </template>
            <Form layout="vertical">
              <Row :gutter="[12, 4]">
                <Col :lg="8" :md="12" :span="24">
                  <Form.Item label="综合关键字">
                    <Input
                      v-model:value="keyword"
                      allow-clear
                      placeholder="按账号、姓名、邮箱模糊筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col :lg="8" :md="12" :span="24">
                  <Form.Item label="账号">
                    <Input
                      v-model:value="userNameFilter"
                      allow-clear
                      placeholder="按账号筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col :lg="8" :md="12" :span="24">
                  <Form.Item label="姓名">
                    <Input
                      v-model:value="displayNameFilter"
                      allow-clear
                      placeholder="按姓名筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="工号">
                    <Input
                      v-model:value="employeeNoFilter"
                      allow-clear
                      placeholder="按工号筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="邮箱">
                    <Input
                      v-model:value="emailFilter"
                      allow-clear
                      placeholder="按邮箱筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="手机号">
                    <Input
                      v-model:value="phoneNumberFilter"
                      allow-clear
                      placeholder="按手机号筛选"
                      @pressEnter="searchUsers"
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="部门">
                    <TreeSelect
                      v-model:value="organizationUnitId"
                      allow-clear
                      placeholder="按部门筛选"
                      :tree-data="normalizeTree(organizationTree)"
                      tree-default-expand-all
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="状态">
                    <Select
                      v-model:value="statusFilter"
                      allow-clear
                      placeholder="按状态筛选"
                      :options="[
                        { label: '启用', value: 'true' },
                        { label: '停用', value: 'false' },
                      ]"
                    />
                  </Form.Item>
                </Col>
                <Col v-show="searchExpanded" :lg="8" :md="12" :span="24">
                  <Form.Item label="角色">
                    <Select
                      v-model:value="roleName"
                      allow-clear
                      placeholder="按角色筛选"
                      :options="roleOptions"
                    />
                  </Form.Item>
                </Col>
              </Row>
              <div class="platform-page__filter-actions">
                <Button @click="resetFilters">重置</Button>
              </div>
            </Form>
          </Card>

          <Card :bordered="false" class="platform-page__card rounded-2xl" title="用户列表">
            <template #extra>
              <div class="platform-page__table-actions">
                <Button v-if="canManageUsers" @click="openOnlineDrawer">在线会话</Button>
                <Button v-if="canManageUsers" type="primary" @click="openCreate">新增用户</Button>
              </div>
            </template>

            <UserGrid
              class="app-vxe-card-grid platform-page__user-grid-shell"
              grid-class="platform-page__user-grid"
              :grid-events="userGridEvents"
              :grid-options="userGridOptions"
            >
              <template #roles="{ row }">
                <div class="tag-list">
                  <Tag v-for="role in row.roles" :key="role" color="blue">
                    {{ role }}
                  </Tag>
                </div>
              </template>
              <template #organizationUnitNames="{ row }">
                <div class="tag-list">
                  <Tag
                    v-for="organizationUnitName in row.organizationUnitNames"
                    :key="organizationUnitName"
                  >
                    {{ organizationUnitName }}
                  </Tag>
                </div>
              </template>
              <template #tenantName="{ row }">
                <Tag v-if="row.tenantName" color="blue">
                  {{ row.tenantName }}
                </Tag>
                <span v-else>—</span>
              </template>
              <template #isActive="{ row }">
                <Switch
                  v-if="canManageUsers"
                  :checked="row.isActive"
                  checked-children="启用"
                  :loading="actionLoadingKey === `toggle:${row.id}`"
                  un-checked-children="停用"
                  @change="(checked) => handleToggleStatus(row, checked === true)"
                />
                <Tag v-else :color="row.isActive ? 'success' : 'default'">
                  {{ row.isActive ? '启用' : '停用' }}
                </Tag>
              </template>
              <template #isOnline="{ row }">
                <Tag :color="row.isOnline ? 'success' : 'default'">
                  {{ row.isOnline ? '在线' : '离线' }}
                </Tag>
              </template>
              <template #actions="{ row }">
                <div class="action-list">
                  <Button size="small" type="link" @click="openEdit(row)">
                    编辑
                  </Button>
                  <Button
                    size="small"
                    type="link"
                    @click="openMappingDrawer(row)"
                  >
                    映射记录
                  </Button>
                  <Button
                    size="small"
                    type="link"
                    @click="openResetPassword(row)"
                  >
                    重置密码
                  </Button>
                  <Popconfirm title="确认删除该用户？" @confirm="handleDelete(row.id)">
                    <Button
                      danger
                      :loading="actionLoadingKey === `delete:${row.id}`"
                      size="small"
                      type="link"
                    >
                      删除
                    </Button>
                  </Popconfirm>
                </div>
              </template>
            </UserGrid>
          </Card>
        </div>
      </div>
    </section>

    <Drawer
      v-model:open="onlineVisible"
      destroy-on-close
      placement="right"
      title="在线会话"
      width="720"
    >
      <OnlineGrid class="app-vxe-card-grid" :grid-events="onlineGridEvents" :grid-options="onlineGridOptions">
        <template #tenantName="{ row }">
          <Tag v-if="row.tenantName" color="blue">{{ row.tenantName }}</Tag>
          <span v-else>—</span>
        </template>
        <template #lastAccessedAt="{ row }">
          {{ row.lastAccessedAt ? new Date(row.lastAccessedAt).toLocaleString() : '-' }}
        </template>
        <template #signedInAt="{ row }">
          {{ row.signedInAt ? new Date(row.signedInAt).toLocaleString() : '-' }}
        </template>
        <template #actions="{ row }">
          <Popconfirm
            title="确认强制下线该用户？"
            @confirm="handleForceLogout(row)"
          >
            <Button
              :disabled="!row.canForceLogout"
              danger
              :loading="actionLoadingKey === `force-logout:${row.userId}`"
              size="small"
              type="link"
            >
              强制下线
            </Button>
          </Popconfirm>
        </template>
      </OnlineGrid>
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

      <MappingGrid class="app-vxe-card-grid" :grid-events="mappingGridEvents" :grid-options="mappingGridOptions">
        <template #status="{ row }">
          <Tag :color="row.status === 'active' ? 'success' : 'default'">
            {{ row.status === 'active' ? '有效' : row.status }}
          </Tag>
        </template>
        <template #boundAt="{ row }">
          {{ row.boundAt ? new Date(row.boundAt).toLocaleString() : '-' }}
        </template>
        <template #lastSyncedAt="{ row }">
          {{ row.lastSyncedAt ? new Date(row.lastSyncedAt).toLocaleString() : '-' }}
        </template>
        <template #remark="{ row }">
          {{ row.remark || '-' }}
        </template>
      </MappingGrid>
    </Drawer>

    <Drawer
      v-model:open="createDrawerVisible"
      :title="editingUserId ? '编辑用户' : '新增用户'"
      destroy-on-close
      width="720px"
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
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="createDrawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </div>
      </template>
    </Drawer>

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
@reference "@vben/tailwind-config/theme";
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
  @apply gap-4;
  color: var(--platform-text);
}

.platform-page__metrics {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.platform-page__metric,
.platform-page__card,
.platform-page__filter-card,
.platform-page__org-card {
  border: 1px solid var(--platform-border);
  @apply rounded-2xl;
  background: var(--platform-surface);
  box-shadow: var(--platform-shadow);
  color: inherit;
}

.platform-page__metric {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  @apply px-4 py-4;
}

.platform-page__metric span {
  color: var(--platform-text-secondary);
  font-size: 13px;
}

.platform-page__metric strong {
  color: var(--platform-text);
  font-size: 24px;
}

.platform-page__workspace {
  display: grid;
  @apply gap-4;
  grid-template-columns: 340px minmax(0, 1fr);
  align-items: stretch;
}

.platform-page__org-card {
  position: sticky;
  top: 16px;
  min-height: 100%;
}

.platform-page__main {
  display: flex;
  flex-direction: column;
  @apply gap-4;
  min-width: 0;
  min-height: 640px;
}

.platform-page__card {
  flex: 1;
}

.platform-page__empty {
  display: grid;
  min-height: 240px;
  place-items: center;
}

.platform-page__org-search {
  @apply mb-3;
}

.platform-page__filter-actions,
.platform-page__table-actions {
  display: flex;
  @apply gap-2.5;
  align-items: center;
}

.platform-page__filter-actions {
  justify-content: flex-end;
  @apply pt-1;
}

.platform-page__table-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.action-list {
  display: flex;
  flex-wrap: wrap;
  @apply gap-x-1 gap-y-0.5;
  justify-content: center;
}

@media (max-width: 1080px) {
  .platform-page__metrics,
  .form-grid {
    grid-template-columns: 1fr;
  }

  .platform-page__workspace {
    grid-template-columns: 1fr;
  }

  .platform-page__org-card {
    position: static;
  }

  .platform-page__filter-actions,
  .platform-page__table-actions {
    justify-content: flex-start;
  }
}
</style>
