<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi, OrganizationApi, ProjectApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Drawer,
  Form,
  Input,
  Popconfirm,
  Select,
  Space,
  Statistic,
  Tabs,
  Tag,
  TreeSelect,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  autoMatchIntegrationUsersApi,
  bindCurrentIntegrationUserApi,
  bindIntegrationUserApi,
  getIntegrationConnectorsApi,
  getIntegrationDepartmentBindingsApi,
  getIntegrationExternalAccountsApi,
  getIntegrationGroupBindingsApi,
  getIntegrationIdMappingsApi,
  getIntegrationUserBindingsApi,
  getOrganizationTreeApi,
  getProjectListApi,
  getUserListApi,
  importIntegrationExternalAccountsApi,
  resolveIntegrationUserBindingApi,
  saveIntegrationDepartmentBindingApi,
  saveIntegrationExternalAccountApi,
  saveIntegrationGroupBindingApi,
  saveIntegrationIdMappingApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { normalizePlatformTreeSelect } from '#/composables/platform/use-tree-normalization';

defineOptions({ name: 'IntegrationBindingsPage' });

const BINDING_MANAGE_CODE = 'Integration.Binding.Manage';
const BINDING_IMPORT_CODE = 'Integration.Binding.Import';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '待处理', value: 'Pending' },
  { label: '已绑定', value: 'Bound' },
  { label: '冲突', value: 'Conflict' },
  { label: '已停用', value: 'Disabled' },
];

const editStatusOptions = statusOptions.filter((item) => item.value !== 'All');

const matchTypeOptions = [
  { label: '人工绑定', value: 'Manual' },
  { label: '手机号', value: 'Mobile' },
  { label: '邮箱', value: 'Email' },
  { label: '手机号 + 邮箱', value: 'MobileAndEmail' },
  { label: '同步生成', value: 'Sync' },
  { label: '用户自助', value: 'SelfService' },
];

const objectTypeOptions = [
  { label: '项目', value: 'Project' },
  { label: '组织', value: 'OrganizationUnit' },
  { label: '用户', value: 'User' },
  { label: '经营对象', value: 'Business' },
  { label: '自定义', value: 'Custom' },
];

const groupTypeOptions = [
  { label: '群聊', value: 'Chat' },
  { label: '机器人', value: 'Robot' },
  { label: '频道', value: 'Channel' },
];

const accountColumns = [
  { field: 'displayName', slots: { default: 'externalAccount' }, title: '外部账号', minWidth: 220 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'mobile', title: '手机号', width: 150 },
  { field: 'email', minWidth: 220, showOverflow: 'tooltip', title: '邮箱' },
  { field: 'departmentExternalId', minWidth: 180, showOverflow: 'tooltip', title: '外部部门' },
  { field: 'lastSyncedTime', slots: { default: 'time' }, title: '同步时间', width: 170 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 260 },
] satisfies VxeGridProps<IntegrationApi.ExternalAccountItem>['columns'];

const userBindingColumns = [
  { field: 'externalDisplayName', slots: { default: 'externalUser' }, title: '外部账号', minWidth: 220 },
  { field: 'internalUserName', slots: { default: 'internalUser' }, title: '内部用户', minWidth: 190 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'matchType', slots: { default: 'matchType' }, title: '匹配方式', width: 130 },
  { field: 'conflictReason', minWidth: 260, showOverflow: 'tooltip', title: '冲突原因' },
  { field: 'boundTime', slots: { default: 'time' }, title: '绑定时间', width: 170 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 180 },
] satisfies VxeGridProps<IntegrationApi.UserBindingItem>['columns'];

const departmentColumns = [
  { field: 'externalDepartmentName', slots: { default: 'externalDepartment' }, title: '外部部门', minWidth: 220 },
  { field: 'internalOrganizationUnitName', minWidth: 190, title: '内部组织' },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'matchType', slots: { default: 'matchType' }, title: '匹配方式', width: 130 },
  { field: 'parentExternalDepartmentId', minWidth: 180, showOverflow: 'tooltip', title: '上级外部部门' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<IntegrationApi.DepartmentBindingItem>['columns'];

const groupColumns = [
  { field: 'externalGroupName', slots: { default: 'externalGroup' }, title: '外部群', minWidth: 220 },
  { field: 'internalObjectName', slots: { default: 'internalObject' }, title: '内部对象', minWidth: 220 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'groupType', slots: { default: 'groupType' }, title: '群类型', width: 110 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'robotCode', minWidth: 160, showOverflow: 'tooltip', title: '机器人' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<IntegrationApi.GroupBindingItem>['columns'];

const mappingColumns = [
  { field: 'internalObjectType', slots: { default: 'internalObject' }, title: '内部对象', minWidth: 230 },
  { field: 'externalObjectType', slots: { default: 'externalObject' }, title: '外部对象', minWidth: 230 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 150 },
  { field: 'source', title: '来源', width: 110 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'remark', minWidth: 220, showOverflow: 'tooltip', title: '备注' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<IntegrationApi.IdMappingItem>['columns'];

const { hasAccessByCodes } = useAccess();
const { actionLoadingKey, runAction } = useActionLoading();
const canManage = computed(() => hasAccessByCodes([BINDING_MANAGE_CODE]));
const canImport = computed(() => hasAccessByCodes([BINDING_IMPORT_CODE]));

const activeTab = ref('accounts');
const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const users = ref<UserApi.UserListItem[]>([]);
const organizations = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const projects = ref<ProjectApi.ProjectListItem[]>([]);

const accounts = ref<IntegrationApi.ExternalAccountItem[]>([]);
const userBindings = ref<IntegrationApi.UserBindingItem[]>([]);
const departmentBindings = ref<IntegrationApi.DepartmentBindingItem[]>([]);
const groupBindings = ref<IntegrationApi.GroupBindingItem[]>([]);
const idMappings = ref<IntegrationApi.IdMappingItem[]>([]);

const accountsTotal = ref(0);
const userBindingsTotal = ref(0);
const departmentBindingsTotal = ref(0);
const groupBindingsTotal = ref(0);
const idMappingsTotal = ref(0);

const accountPage = ref(1);
const accountPageSize = ref(20);
const userBindingPage = ref(1);
const userBindingPageSize = ref(20);
const departmentPage = ref(1);
const departmentPageSize = ref(20);
const groupPage = ref(1);
const groupPageSize = ref(20);
const mappingPage = ref(1);
const mappingPageSize = ref(20);

const loadingAccounts = ref(false);
const loadingUserBindings = ref(false);
const loadingDepartments = ref(false);
const loadingGroups = ref(false);
const loadingMappings = ref(false);
const saving = ref(false);

const accountQuery = reactive({ connectorId: '', keyword: '', status: 'All' });
const userBindingQuery = reactive({ connectorId: '', keyword: '', status: 'All' });
const departmentQuery = reactive({ connectorId: '', keyword: '', status: 'All' });
const groupQuery = reactive({ connectorId: '', keyword: '', status: 'All' });
const mappingQuery = reactive({ connectorId: '', keyword: '', status: 'All' });

const accountDrawerOpen = ref(false);
const accountEditing = ref<IntegrationApi.ExternalAccountItem>();
const accountForm = reactive<IntegrationApi.ExternalAccountSaveInput>(createAccountForm());

const importModalOpen = ref(false);
const importConnectorId = ref('');
const importJson = ref('');

const userBindingDrawerOpen = ref(false);
const userBindingEditing = ref<IntegrationApi.UserBindingItem>();
const userBindingForm = reactive({
  externalAccountId: '',
  internalUserId: '',
  matchType: 'Manual',
  remark: '',
  status: 'Bound',
});

const departmentDrawerOpen = ref(false);
const departmentEditing = ref<IntegrationApi.DepartmentBindingItem>();
const departmentForm = reactive<IntegrationApi.DepartmentBindingSaveInput>(createDepartmentForm());

const groupDrawerOpen = ref(false);
const groupEditing = ref<IntegrationApi.GroupBindingItem>();
const groupForm = reactive<IntegrationApi.GroupBindingSaveInput>(createGroupForm());

const mappingDrawerOpen = ref(false);
const mappingEditing = ref<IntegrationApi.IdMappingItem>();
const mappingForm = reactive<IntegrationApi.IdMappingSaveInput>(createMappingForm());

const connectorOptions = computed(() => [
  { label: '全部连接器', value: '' },
  ...connectors.value.map((item) => ({ label: `${item.displayName} · ${item.connectorCode}`, value: item.id })),
]);

const enabledConnectorOptions = computed(() =>
  connectors.value.map((item) => ({ label: `${item.displayName} · ${item.connectorCode}`, value: item.id })),
);

const userOptions = computed(() =>
  users.value.map((item) => ({ label: `${item.displayName || item.username} · ${item.email || item.phoneNumber || item.username}`, value: item.id })),
);

const organizationTreeData = computed(() => normalizePlatformTreeSelect(organizations.value, (item) => item.displayName));

const projectOptions = computed(() =>
  projects.value.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id })),
);

const externalAccountOptions = computed(() =>
  accounts.value.map((item) => ({
    label: `${item.displayName || item.externalUserId} · ${item.externalUserId}`,
    value: item.id,
  })),
);

const metrics = computed(() => [
  { label: '外部账号', value: accountsTotal.value },
  { label: '用户绑定', value: userBindingsTotal.value },
  { label: '部门绑定', value: departmentBindingsTotal.value },
  { label: '群绑定', value: groupBindingsTotal.value },
]);

const accountGridOptions = computed<VxeGridProps<IntegrationApi.ExternalAccountItem>>(() => ({
  columns: accountColumns,
  data: accounts.value,
  emptyText: '暂无外部账号',
  loading: loadingAccounts.value,
  pagerConfig: {
    currentPage: accountPage.value,
    pageSize: accountPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: accountsTotal.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const userBindingGridOptions = computed<VxeGridProps<IntegrationApi.UserBindingItem>>(() => ({
  columns: userBindingColumns,
  data: userBindings.value,
  emptyText: '暂无用户绑定',
  loading: loadingUserBindings.value,
  pagerConfig: {
    currentPage: userBindingPage.value,
    pageSize: userBindingPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: userBindingsTotal.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const departmentGridOptions = computed<VxeGridProps<IntegrationApi.DepartmentBindingItem>>(() => ({
  columns: departmentColumns,
  data: departmentBindings.value,
  emptyText: '暂无部门绑定',
  loading: loadingDepartments.value,
  pagerConfig: {
    currentPage: departmentPage.value,
    pageSize: departmentPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: departmentBindingsTotal.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const groupGridOptions = computed<VxeGridProps<IntegrationApi.GroupBindingItem>>(() => ({
  columns: groupColumns,
  data: groupBindings.value,
  emptyText: '暂无群绑定',
  loading: loadingGroups.value,
  pagerConfig: {
    currentPage: groupPage.value,
    pageSize: groupPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: groupBindingsTotal.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const mappingGridOptions = computed<VxeGridProps<IntegrationApi.IdMappingItem>>(() => ({
  columns: mappingColumns,
  data: idMappings.value,
  emptyText: '暂无 ID 映射',
  loading: loadingMappings.value,
  pagerConfig: {
    currentPage: mappingPage.value,
    pageSize: mappingPageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: idMappingsTotal.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const accountGridEvents: NativeVxeGridEvents<IntegrationApi.ExternalAccountItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    accountPage.value = pageSize === accountPageSize.value ? currentPage || 1 : 1;
    accountPageSize.value = pageSize || accountPageSize.value;
    void loadAccounts();
  },
};

const userBindingGridEvents: NativeVxeGridEvents<IntegrationApi.UserBindingItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    userBindingPage.value = pageSize === userBindingPageSize.value ? currentPage || 1 : 1;
    userBindingPageSize.value = pageSize || userBindingPageSize.value;
    void loadUserBindings();
  },
};

const departmentGridEvents: NativeVxeGridEvents<IntegrationApi.DepartmentBindingItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    departmentPage.value = pageSize === departmentPageSize.value ? currentPage || 1 : 1;
    departmentPageSize.value = pageSize || departmentPageSize.value;
    void loadDepartmentBindings();
  },
};

const groupGridEvents: NativeVxeGridEvents<IntegrationApi.GroupBindingItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    groupPage.value = pageSize === groupPageSize.value ? currentPage || 1 : 1;
    groupPageSize.value = pageSize || groupPageSize.value;
    void loadGroupBindings();
  },
};

const mappingGridEvents: NativeVxeGridEvents<IntegrationApi.IdMappingItem> = {
  'page-change': ({ currentPage, pageSize }) => {
    mappingPage.value = pageSize === mappingPageSize.value ? currentPage || 1 : 1;
    mappingPageSize.value = pageSize || mappingPageSize.value;
    void loadIdMappings();
  },
};

const [AccountGrid] = useVbenVxeGrid<IntegrationApi.ExternalAccountItem>({ gridEvents: accountGridEvents, gridOptions: {} });
const [UserBindingGrid] = useVbenVxeGrid<IntegrationApi.UserBindingItem>({ gridEvents: userBindingGridEvents, gridOptions: {} });
const [DepartmentGrid] = useVbenVxeGrid<IntegrationApi.DepartmentBindingItem>({ gridEvents: departmentGridEvents, gridOptions: {} });
const [GroupGrid] = useVbenVxeGrid<IntegrationApi.GroupBindingItem>({ gridEvents: groupGridEvents, gridOptions: {} });
const [MappingGrid] = useVbenVxeGrid<IntegrationApi.IdMappingItem>({ gridEvents: mappingGridEvents, gridOptions: {} });

function createAccountForm(): IntegrationApi.ExternalAccountSaveInput {
  return {
    avatarUrl: '',
    connectorId: '',
    departmentExternalId: '',
    displayName: '',
    email: '',
    externalUnionId: '',
    externalUserId: '',
    mobile: '',
    rawJson: '{}',
    status: 'Pending',
  };
}

function createDepartmentForm(): IntegrationApi.DepartmentBindingSaveInput {
  return {
    connectorId: '',
    externalDepartmentId: '',
    externalDepartmentName: '',
    internalOrganizationUnitId: undefined,
    matchType: 'Manual',
    parentExternalDepartmentId: '',
    remark: '',
    status: 'Bound',
  };
}

function createGroupForm(): IntegrationApi.GroupBindingSaveInput {
  return {
    connectorId: '',
    externalGroupId: '',
    externalGroupName: '',
    groupType: 'Chat',
    internalObjectId: '',
    internalObjectName: '',
    internalObjectType: 'Project',
    remark: '',
    robotCode: '',
    status: 'Bound',
  };
}

function createMappingForm(): IntegrationApi.IdMappingSaveInput {
  return {
    connectorId: '',
    externalObjectId: '',
    externalObjectType: 'ExternalUser',
    internalObjectId: '',
    internalObjectType: 'User',
    remark: '',
    source: 'Manual',
    status: 'Bound',
  };
}

function normalizeQuery<T extends { connectorId: string; keyword: string; status: string }>(
  query: T,
  page: number,
  pageSize: number,
) {
  return {
    connectorId: query.connectorId || undefined,
    keyword: query.keyword.trim() || undefined,
    maxResultCount: pageSize,
    skipCount: (page - 1) * pageSize,
    status: query.status || 'All',
  };
}

function formatTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function connectorLabel(idOrCode: string) {
  return connectors.value.find((item) => item.id === idOrCode || item.connectorCode === idOrCode)?.displayName || idOrCode || '-';
}

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Bound') {
    return 'green';
  }
  if (value === 'Conflict') {
    return 'red';
  }
  if (value === 'Pending') {
    return 'blue';
  }
  if (value === 'Disabled') {
    return 'default';
  }
  return 'default';
}

function matchTypeLabel(value: string) {
  return matchTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

function getStringValue(item: Record<string, unknown>, keys: string[]) {
  for (const key of keys) {
    const value = item[key];
    if (typeof value === 'string' && value.trim()) {
      return value.trim();
    }
  }
  return '';
}

function assertJson(value: string, label: string) {
  try {
    JSON.parse(value || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

function assertConnector(value: string) {
  if (!value) {
    throw new Error('请选择连接器');
  }
}

async function loadMetadata() {
  const [connectorResult, userResult, organizationResult, projectResult] = await Promise.allSettled([
    getIntegrationConnectorsApi({ includeDisabled: true }),
    getUserListApi({ maxResultCount: 500 }),
    getOrganizationTreeApi(),
    getProjectListApi({}),
  ]);

  connectors.value = connectorResult.status === 'fulfilled' ? connectorResult.value : [];
  users.value = userResult.status === 'fulfilled' ? userResult.value.items : [];
  organizations.value = organizationResult.status === 'fulfilled' ? organizationResult.value : [];
  projects.value = projectResult.status === 'fulfilled' ? projectResult.value : [];

  if (connectorResult.status === 'rejected') {
    message.warning('连接器列表加载失败，绑定操作需要先选择连接器。');
  }
}

async function loadAccounts(resetPage = false) {
  if (resetPage) {
    accountPage.value = 1;
  }
  loadingAccounts.value = true;
  try {
    const result = await getIntegrationExternalAccountsApi(
      normalizeQuery(accountQuery, accountPage.value, accountPageSize.value),
    );
    accounts.value = result.items;
    accountsTotal.value = result.totalCount;
  } finally {
    loadingAccounts.value = false;
  }
}

async function loadUserBindings(resetPage = false) {
  if (resetPage) {
    userBindingPage.value = 1;
  }
  loadingUserBindings.value = true;
  try {
    const result = await getIntegrationUserBindingsApi(
      normalizeQuery(userBindingQuery, userBindingPage.value, userBindingPageSize.value),
    );
    userBindings.value = result.items;
    userBindingsTotal.value = result.totalCount;
  } finally {
    loadingUserBindings.value = false;
  }
}

async function loadDepartmentBindings(resetPage = false) {
  if (resetPage) {
    departmentPage.value = 1;
  }
  loadingDepartments.value = true;
  try {
    const result = await getIntegrationDepartmentBindingsApi(
      normalizeQuery(departmentQuery, departmentPage.value, departmentPageSize.value),
    );
    departmentBindings.value = result.items;
    departmentBindingsTotal.value = result.totalCount;
  } finally {
    loadingDepartments.value = false;
  }
}

async function loadGroupBindings(resetPage = false) {
  if (resetPage) {
    groupPage.value = 1;
  }
  loadingGroups.value = true;
  try {
    const result = await getIntegrationGroupBindingsApi(
      normalizeQuery(groupQuery, groupPage.value, groupPageSize.value),
    );
    groupBindings.value = result.items;
    groupBindingsTotal.value = result.totalCount;
  } finally {
    loadingGroups.value = false;
  }
}

async function loadIdMappings(resetPage = false) {
  if (resetPage) {
    mappingPage.value = 1;
  }
  loadingMappings.value = true;
  try {
    const result = await getIntegrationIdMappingsApi(
      normalizeQuery(mappingQuery, mappingPage.value, mappingPageSize.value),
    );
    idMappings.value = result.items;
    idMappingsTotal.value = result.totalCount;
  } finally {
    loadingMappings.value = false;
  }
}

async function refreshActive(resetPage = true) {
  if (activeTab.value === 'accounts') {
    await loadAccounts(resetPage);
  } else if (activeTab.value === 'users') {
    await loadUserBindings(resetPage);
  } else if (activeTab.value === 'departments') {
    await loadDepartmentBindings(resetPage);
  } else if (activeTab.value === 'groups') {
    await loadGroupBindings(resetPage);
  } else {
    await loadIdMappings(resetPage);
  }
}

function openAccountDrawer(item?: IntegrationApi.ExternalAccountItem) {
  accountEditing.value = item;
  Object.assign(accountForm, createAccountForm(), item ? {
    avatarUrl: item.avatarUrl,
    connectorId: item.connectorId,
    departmentExternalId: item.departmentExternalId,
    displayName: item.displayName,
    email: item.email,
    externalUnionId: item.externalUnionId,
    externalUserId: item.externalUserId,
    mobile: item.mobile,
    rawJson: item.rawJson || '{}',
    status: item.status || 'Pending',
  } : {});
  accountDrawerOpen.value = true;
}

async function submitAccount() {
  try {
    assertConnector(accountForm.connectorId);
    if (!accountForm.externalUserId.trim()) {
      throw new Error('请填写外部用户 ID');
    }
    assertJson(accountForm.rawJson, '原始数据');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationExternalAccountApi(accountForm);
    message.success('外部账号已保存');
    accountDrawerOpen.value = false;
    await loadAccounts();
  } finally {
    saving.value = false;
  }
}

function openImportModal() {
  importConnectorId.value = accountQuery.connectorId || enabledConnectorOptions.value[0]?.value || '';
  importJson.value = '[\n  {\n    "externalUserId": "ou_xxx",\n    "displayName": "张三",\n    "mobile": "13800000000",\n    "email": "zhangsan@example.com"\n  }\n]';
  importModalOpen.value = true;
}

function normalizeImportAccounts(value: unknown): IntegrationApi.ExternalAccountSaveInput[] {
  if (!Array.isArray(value)) {
    throw new Error('导入内容必须是 JSON 数组');
  }

  return value.map((item) => {
    if (!item || typeof item !== 'object' || Array.isArray(item)) {
      throw new Error('导入数组中的每一项都必须是对象');
    }
    const source = item as Record<string, unknown>;
    const externalUserId = getStringValue(source, ['externalUserId', 'external_user_id', 'openId', 'open_id', 'userId', 'user_id']);
    if (!externalUserId) {
      throw new Error('每个外部账号必须包含 externalUserId');
    }

    return {
      avatarUrl: getStringValue(source, ['avatarUrl', 'avatar_url']),
      connectorId: importConnectorId.value,
      departmentExternalId: getStringValue(source, ['departmentExternalId', 'department_id', 'departmentId']),
      displayName: getStringValue(source, ['displayName', 'name', 'nickname', 'userName']),
      email: getStringValue(source, ['email']),
      externalUnionId: getStringValue(source, ['externalUnionId', 'union_id', 'unionId']),
      externalUserId,
      mobile: getStringValue(source, ['mobile', 'phone', 'phoneNumber']),
      rawJson: JSON.stringify(source),
      status: getStringValue(source, ['status']) || 'Pending',
    };
  });
}

async function submitImport() {
  try {
    assertConnector(importConnectorId.value);
    const accounts = normalizeImportAccounts(JSON.parse(importJson.value || '[]'));
    if (accounts.length === 0) {
      throw new Error('导入账号不能为空');
    }

    saving.value = true;
    const result = await importIntegrationExternalAccountsApi({
      accounts,
      connectorId: importConnectorId.value,
    });
    message.success(`导入完成：新增 ${result.createdCount}，更新 ${result.updatedCount}`);
    importModalOpen.value = false;
    await loadAccounts(true);
  } catch (error) {
    message.warning((error as Error).message);
  } finally {
    saving.value = false;
  }
}

function openUserBindingDrawer(item?: IntegrationApi.UserBindingItem, account?: IntegrationApi.ExternalAccountItem) {
  userBindingEditing.value = item;
  Object.assign(userBindingForm, {
    externalAccountId: item?.externalAccountId || account?.id || '',
    internalUserId: item?.internalUserId || '',
    matchType: item?.matchType || 'Manual',
    remark: item?.remark || '',
    status: item?.status === 'Conflict' || item?.status === 'Pending' ? 'Bound' : item?.status || 'Bound',
  });
  userBindingDrawerOpen.value = true;
}

async function submitUserBinding() {
  if (!userBindingForm.externalAccountId) {
    message.warning('请选择外部账号');
    return;
  }
  if (userBindingForm.status === 'Bound' && !userBindingForm.internalUserId) {
    message.warning('确认绑定必须选择内部用户');
    return;
  }

  saving.value = true;
  try {
    if (userBindingEditing.value) {
      await resolveIntegrationUserBindingApi(userBindingEditing.value.id, {
        internalUserId: userBindingForm.internalUserId || undefined,
        remark: userBindingForm.remark,
        status: userBindingForm.status,
      });
    } else {
      await bindIntegrationUserApi({
        externalAccountId: userBindingForm.externalAccountId,
        internalUserId: userBindingForm.internalUserId,
        matchType: userBindingForm.matchType,
        remark: userBindingForm.remark,
      });
    }
    message.success('用户绑定已保存');
    userBindingDrawerOpen.value = false;
    await Promise.all([loadAccounts(), loadUserBindings()]);
  } finally {
    saving.value = false;
  }
}

async function selfBindAccount(item: IntegrationApi.ExternalAccountItem) {
  await runAction(`self-bind:${item.id}`, async () => {
    await bindCurrentIntegrationUserApi({
      externalAccountId: item.id,
      remark: '用户自助绑定',
    });
    message.success('自助绑定成功');
    await Promise.all([loadAccounts(), loadUserBindings(true)]);
  });
}

async function autoMatchUsers() {
  if (!userBindingQuery.connectorId && !accountQuery.connectorId) {
    message.warning('请选择要自动匹配的连接器');
    return;
  }

  const connectorId = userBindingQuery.connectorId || accountQuery.connectorId;
  await runAction(`auto-match:${connectorId}`, async () => {
    const result = await autoMatchIntegrationUsersApi({ connectorId });
    message.success(`扫描 ${result.scannedCount} 个账号，绑定 ${result.boundCount}，冲突 ${result.conflictCount}，待处理 ${result.pendingCount}`);
    await Promise.all([loadAccounts(true), loadUserBindings(true)]);
  });
}

function openDepartmentDrawer(item?: IntegrationApi.DepartmentBindingItem) {
  departmentEditing.value = item;
  Object.assign(departmentForm, createDepartmentForm(), item ? {
    connectorId: item.connectorId,
    externalDepartmentId: item.externalDepartmentId,
    externalDepartmentName: item.externalDepartmentName,
    internalOrganizationUnitId: item.internalOrganizationUnitId || undefined,
    matchType: item.matchType || 'Manual',
    parentExternalDepartmentId: item.parentExternalDepartmentId,
    remark: item.remark,
    status: item.status || 'Bound',
  } : { connectorId: departmentQuery.connectorId || enabledConnectorOptions.value[0]?.value || '' });
  departmentDrawerOpen.value = true;
}

async function submitDepartment() {
  try {
    assertConnector(departmentForm.connectorId);
    if (!departmentForm.externalDepartmentId.trim()) {
      throw new Error('请填写外部部门 ID');
    }
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationDepartmentBindingApi({
      ...departmentForm,
      internalOrganizationUnitId: departmentForm.internalOrganizationUnitId || undefined,
    });
    message.success('部门绑定已保存');
    departmentDrawerOpen.value = false;
    await loadDepartmentBindings();
  } finally {
    saving.value = false;
  }
}

function applyProjectToGroup(projectId: string) {
  const project = projects.value.find((item) => item.id === projectId);
  if (!project) {
    return;
  }
  groupForm.internalObjectId = project.id;
  groupForm.internalObjectName = `${project.projectCode} · ${project.name}`;
}

function openGroupDrawer(item?: IntegrationApi.GroupBindingItem) {
  groupEditing.value = item;
  Object.assign(groupForm, createGroupForm(), item ? {
    connectorId: item.connectorId,
    externalGroupId: item.externalGroupId,
    externalGroupName: item.externalGroupName,
    groupType: item.groupType || 'Chat',
    internalObjectId: item.internalObjectId,
    internalObjectName: item.internalObjectName,
    internalObjectType: item.internalObjectType || 'Project',
    remark: item.remark,
    robotCode: item.robotCode,
    status: item.status || 'Bound',
  } : { connectorId: groupQuery.connectorId || enabledConnectorOptions.value[0]?.value || '' });
  groupDrawerOpen.value = true;
}

async function submitGroup() {
  try {
    assertConnector(groupForm.connectorId);
    if (!groupForm.externalGroupId.trim()) {
      throw new Error('请填写外部群 ID');
    }
    if (!groupForm.internalObjectType.trim() || !groupForm.internalObjectId.trim()) {
      throw new Error('请填写内部对象类型和 ID');
    }
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationGroupBindingApi(groupForm);
    message.success('群绑定已保存');
    groupDrawerOpen.value = false;
    await loadGroupBindings();
  } finally {
    saving.value = false;
  }
}

function openMappingDrawer(item?: IntegrationApi.IdMappingItem) {
  mappingEditing.value = item;
  Object.assign(mappingForm, createMappingForm(), item ? {
    connectorId: item.connectorId,
    externalObjectId: item.externalObjectId,
    externalObjectType: item.externalObjectType,
    internalObjectId: item.internalObjectId,
    internalObjectType: item.internalObjectType,
    remark: item.remark,
    source: item.source || 'Manual',
    status: item.status || 'Bound',
  } : { connectorId: mappingQuery.connectorId || enabledConnectorOptions.value[0]?.value || '' });
  mappingDrawerOpen.value = true;
}

async function submitMapping() {
  try {
    assertConnector(mappingForm.connectorId);
    if (!mappingForm.internalObjectType.trim() || !mappingForm.internalObjectId.trim()) {
      throw new Error('请填写内部对象类型和 ID');
    }
    if (!mappingForm.externalObjectType.trim() || !mappingForm.externalObjectId.trim()) {
      throw new Error('请填写外部对象类型和 ID');
    }
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationIdMappingApi(mappingForm);
    message.success('ID 映射已保存');
    mappingDrawerOpen.value = false;
    await loadIdMappings();
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await loadMetadata();
  await Promise.all([
    loadAccounts(),
    loadUserBindings(),
    loadDepartmentBindings(),
    loadGroupBindings(),
    loadIdMappings(),
  ]);
});
</script>

<template>
  <Page description="维护外部账号、内部用户、组织、项目群和内外部 ID 映射，是飞书、钉钉、企业微信同步与通知的统一绑定入口。" title="账号绑定">
    <div class="integration-bindings flex flex-col gap-3">
      <Alert
        show-icon
        type="info"
        message="外部账号由连接器同步、批量导入或人工录入产生；用户、部门、群和 ID 映射统一在这里维护，业务模块只保存内部对象 ID，不直接依赖外部平台。"
      />

      <div class="integration-bindings__metrics grid grid-cols-4 gap-3">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="integration-bindings__main-card rounded-2xl">
        <Tabs v-model:activeKey="activeTab" class="pt-3" @change="refreshActive(false)">
          <Tabs.TabPane key="accounts" tab="外部账号">
            <div class="integration-bindings__toolbar mb-3 flex items-center justify-between gap-3">
              <Input.Search v-model:value="accountQuery.keyword" allow-clear placeholder="搜索姓名、外部 ID、手机号、邮箱" @search="loadAccounts(true)" />
              <Select v-model:value="accountQuery.connectorId" :options="connectorOptions" @change="loadAccounts(true)" />
              <Select v-model:value="accountQuery.status" :options="statusOptions" @change="loadAccounts(true)" />
              <Space>
                <Button @click="loadAccounts(true)">刷新</Button>
                <Button v-if="canImport" @click="openImportModal">批量导入</Button>
                <Button v-if="canManage" type="primary" @click="openAccountDrawer()">新增账号</Button>
              </Space>
            </div>

            <AccountGrid :grid-events="accountGridEvents" :grid-options="accountGridOptions" class="app-vxe-card-grid">
              <template #externalAccount="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.displayName || row.externalUserId }}</strong>
                  <span>{{ row.externalUserId }}</span>
                </div>
              </template>
              <template #connector="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ connectorLabel(row.connectorId) }}</strong>
                  <span>{{ row.providerType }}</span>
                </div>
              </template>
              <template #status="{ row }">
                <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
              </template>
              <template #time="{ row }">
                {{ formatTime(row.lastSyncedTime) }}
              </template>
              <template #actions="{ row }">
                <Space>
                  <Button v-if="canManage" size="small" type="link" @click="openAccountDrawer(row)">编辑</Button>
                  <Button v-if="canManage" size="small" type="link" @click="openUserBindingDrawer(undefined, row)">绑定用户</Button>
                  <Popconfirm title="使用当前登录用户绑定该外部账号？" @confirm="selfBindAccount(row)">
                    <Button :loading="actionLoadingKey === `self-bind:${row.id}`" size="small" type="link">自助绑定</Button>
                  </Popconfirm>
                </Space>
              </template>
            </AccountGrid>
          </Tabs.TabPane>

          <Tabs.TabPane key="users" tab="用户绑定">
            <div class="integration-bindings__toolbar mb-3 flex items-center justify-between gap-3">
              <Input.Search v-model:value="userBindingQuery.keyword" allow-clear placeholder="搜索外部账号、内部用户、冲突原因" @search="loadUserBindings(true)" />
              <Select v-model:value="userBindingQuery.connectorId" :options="connectorOptions" @change="loadUserBindings(true)" />
              <Select v-model:value="userBindingQuery.status" :options="statusOptions" @change="loadUserBindings(true)" />
              <Space>
                <Button @click="loadUserBindings(true)">刷新</Button>
                <Button
                  v-if="canManage"
                  :loading="actionLoadingKey.startsWith('auto-match:')"
                  @click="autoMatchUsers"
                >
                  自动匹配
                </Button>
                <Button v-if="canManage" type="primary" @click="openUserBindingDrawer()">人工绑定</Button>
              </Space>
            </div>

            <UserBindingGrid :grid-events="userBindingGridEvents" :grid-options="userBindingGridOptions" class="app-vxe-card-grid">
              <template #externalUser="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.externalDisplayName || row.externalUserId }}</strong>
                  <span>{{ row.externalUserId }}</span>
                </div>
              </template>
              <template #internalUser="{ row }">
                {{ row.internalUserName || '-' }}
              </template>
              <template #status="{ row }">
                <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
              </template>
              <template #matchType="{ row }">
                {{ matchTypeLabel(row.matchType) }}
              </template>
              <template #time="{ row }">
                {{ formatTime(row.boundTime) }}
              </template>
              <template #actions="{ row }">
                <Space>
                  <Button v-if="canManage" size="small" type="link" @click="openUserBindingDrawer(row)">处理</Button>
                </Space>
              </template>
            </UserBindingGrid>
          </Tabs.TabPane>

          <Tabs.TabPane key="departments" tab="部门绑定">
            <div class="integration-bindings__toolbar mb-3 flex items-center justify-between gap-3">
              <Input.Search v-model:value="departmentQuery.keyword" allow-clear placeholder="搜索外部部门、内部组织" @search="loadDepartmentBindings(true)" />
              <Select v-model:value="departmentQuery.connectorId" :options="connectorOptions" @change="loadDepartmentBindings(true)" />
              <Select v-model:value="departmentQuery.status" :options="statusOptions" @change="loadDepartmentBindings(true)" />
              <Space>
                <Button @click="loadDepartmentBindings(true)">刷新</Button>
                <Button v-if="canManage" type="primary" @click="openDepartmentDrawer()">新增部门绑定</Button>
              </Space>
            </div>

            <DepartmentGrid :grid-events="departmentGridEvents" :grid-options="departmentGridOptions" class="app-vxe-card-grid">
              <template #externalDepartment="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.externalDepartmentName || row.externalDepartmentId }}</strong>
                  <span>{{ row.externalDepartmentId }}</span>
                </div>
              </template>
              <template #connector="{ row }">
                {{ connectorLabel(row.connectorId) }}
              </template>
              <template #status="{ row }">
                <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
              </template>
              <template #matchType="{ row }">
                {{ matchTypeLabel(row.matchType) }}
              </template>
              <template #actions="{ row }">
                <Button v-if="canManage" size="small" type="link" @click="openDepartmentDrawer(row)">编辑</Button>
              </template>
            </DepartmentGrid>
          </Tabs.TabPane>

          <Tabs.TabPane key="groups" tab="群绑定">
            <div class="integration-bindings__toolbar mb-3 flex items-center justify-between gap-3">
              <Input.Search v-model:value="groupQuery.keyword" allow-clear placeholder="搜索外部群、内部对象、机器人" @search="loadGroupBindings(true)" />
              <Select v-model:value="groupQuery.connectorId" :options="connectorOptions" @change="loadGroupBindings(true)" />
              <Select v-model:value="groupQuery.status" :options="statusOptions" @change="loadGroupBindings(true)" />
              <Space>
                <Button @click="loadGroupBindings(true)">刷新</Button>
                <Button v-if="canManage" type="primary" @click="openGroupDrawer()">新增群绑定</Button>
              </Space>
            </div>

            <GroupGrid :grid-events="groupGridEvents" :grid-options="groupGridOptions" class="app-vxe-card-grid">
              <template #externalGroup="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.externalGroupName || row.externalGroupId }}</strong>
                  <span>{{ row.externalGroupId }}</span>
                </div>
              </template>
              <template #internalObject="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.internalObjectName || row.internalObjectId }}</strong>
                  <span>{{ row.internalObjectType }} · {{ row.internalObjectId }}</span>
                </div>
              </template>
              <template #connector="{ row }">
                {{ connectorLabel(row.connectorId) }}
              </template>
              <template #groupType="{ row }">
                {{ groupTypeOptions.find((item) => item.value === row.groupType)?.label || row.groupType }}
              </template>
              <template #status="{ row }">
                <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
              </template>
              <template #actions="{ row }">
                <Button v-if="canManage" size="small" type="link" @click="openGroupDrawer(row)">编辑</Button>
              </template>
            </GroupGrid>
          </Tabs.TabPane>

          <Tabs.TabPane key="mappings" tab="ID 映射">
            <div class="integration-bindings__toolbar mb-3 flex items-center justify-between gap-3">
              <Input.Search v-model:value="mappingQuery.keyword" allow-clear placeholder="搜索内部对象、外部对象" @search="loadIdMappings(true)" />
              <Select v-model:value="mappingQuery.connectorId" :options="connectorOptions" @change="loadIdMappings(true)" />
              <Select v-model:value="mappingQuery.status" :options="statusOptions" @change="loadIdMappings(true)" />
              <Space>
                <Button @click="loadIdMappings(true)">刷新</Button>
                <Button v-if="canManage" type="primary" @click="openMappingDrawer()">新增映射</Button>
              </Space>
            </div>

            <MappingGrid :grid-events="mappingGridEvents" :grid-options="mappingGridOptions" class="app-vxe-card-grid">
              <template #internalObject="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.internalObjectType }}</strong>
                  <span>{{ row.internalObjectId }}</span>
                </div>
              </template>
              <template #externalObject="{ row }">
                <div class="integration-bindings__stack-cell">
                  <strong>{{ row.externalObjectType }}</strong>
                  <span>{{ row.externalObjectId }}</span>
                </div>
              </template>
              <template #connector="{ row }">
                {{ connectorLabel(row.connectorId) }}
              </template>
              <template #status="{ row }">
                <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
              </template>
              <template #actions="{ row }">
                <Button v-if="canManage" size="small" type="link" @click="openMappingDrawer(row)">编辑</Button>
              </template>
            </MappingGrid>
          </Tabs.TabPane>
        </Tabs>
      </Card>
    </div>

    <Drawer v-model:open="accountDrawerOpen" :destroy-on-close="true" :title="accountEditing ? '编辑外部账号' : '新增外部账号'" width="720">
      <template #extra>
        <Space>
          <Button @click="accountDrawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitAccount">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="accountForm.connectorId" :disabled="!!accountEditing" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="外部用户 ID" required>
          <Input v-model:value="accountForm.externalUserId" :disabled="!!accountEditing" :maxlength="256" />
        </Form.Item>
        <Form.Item label="显示名称">
          <Input v-model:value="accountForm.displayName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="Union ID">
          <Input v-model:value="accountForm.externalUnionId" :maxlength="256" />
        </Form.Item>
        <Form.Item label="手机号">
          <Input v-model:value="accountForm.mobile" :maxlength="64" />
        </Form.Item>
        <Form.Item label="邮箱">
          <Input v-model:value="accountForm.email" :maxlength="256" />
        </Form.Item>
        <Form.Item label="外部部门 ID">
          <Input v-model:value="accountForm.departmentExternalId" :maxlength="256" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="accountForm.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="原始数据 JSON">
          <Input.TextArea v-model:value="accountForm.rawJson" :auto-size="{ minRows: 5, maxRows: 10 }" />
        </Form.Item>
      </Form>
    </Drawer>

    <Drawer v-model:open="importModalOpen" destroy-on-close title="批量导入外部账号" width="760">
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="importConnectorId" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="账号 JSON 数组" required>
          <Input.TextArea v-model:value="importJson" :auto-size="{ minRows: 12, maxRows: 18 }" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="importModalOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitImport">导入</Button>
        </div>
      </template>
    </Drawer>

    <Drawer v-model:open="userBindingDrawerOpen" :destroy-on-close="true" :title="userBindingEditing ? '处理用户绑定' : '人工绑定用户'" width="640">
      <template #extra>
        <Space>
          <Button @click="userBindingDrawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitUserBinding">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="外部账号" required>
          <Select
            v-model:value="userBindingForm.externalAccountId"
            :disabled="!!userBindingEditing"
            :options="externalAccountOptions"
            show-search
          />
        </Form.Item>
        <Form.Item label="内部用户" required>
          <Select v-model:value="userBindingForm.internalUserId" :options="userOptions" allow-clear show-search />
        </Form.Item>
        <Form.Item label="状态" v-if="userBindingEditing">
          <Select v-model:value="userBindingForm.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="匹配方式" v-else>
          <Select v-model:value="userBindingForm.matchType" :options="matchTypeOptions" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="userBindingForm.remark" :auto-size="{ minRows: 3, maxRows: 6 }" />
        </Form.Item>
      </Form>
    </Drawer>

    <Drawer v-model:open="departmentDrawerOpen" :destroy-on-close="true" :title="departmentEditing ? '编辑部门绑定' : '新增部门绑定'" width="680">
      <template #extra>
        <Space>
          <Button @click="departmentDrawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitDepartment">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="departmentForm.connectorId" :disabled="!!departmentEditing" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="外部部门 ID" required>
          <Input v-model:value="departmentForm.externalDepartmentId" :disabled="!!departmentEditing" :maxlength="256" />
        </Form.Item>
        <Form.Item label="外部部门名称">
          <Input v-model:value="departmentForm.externalDepartmentName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="内部组织">
          <TreeSelect
            v-model:value="departmentForm.internalOrganizationUnitId"
            allow-clear
            show-search
            tree-default-expand-all
            :tree-data="organizationTreeData"
          />
        </Form.Item>
        <Form.Item label="上级外部部门 ID">
          <Input v-model:value="departmentForm.parentExternalDepartmentId" :maxlength="256" />
        </Form.Item>
        <Form.Item label="匹配方式">
          <Select v-model:value="departmentForm.matchType" :options="matchTypeOptions" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="departmentForm.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="departmentForm.remark" :auto-size="{ minRows: 3, maxRows: 6 }" />
        </Form.Item>
      </Form>
    </Drawer>

    <Drawer v-model:open="groupDrawerOpen" :destroy-on-close="true" :title="groupEditing ? '编辑群绑定' : '新增群绑定'" width="720">
      <template #extra>
        <Space>
          <Button @click="groupDrawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitGroup">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="groupForm.connectorId" :disabled="!!groupEditing" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="外部群 ID" required>
          <Input v-model:value="groupForm.externalGroupId" :disabled="!!groupEditing" :maxlength="256" />
        </Form.Item>
        <Form.Item label="外部群名称">
          <Input v-model:value="groupForm.externalGroupName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="群类型">
          <Select v-model:value="groupForm.groupType" :options="groupTypeOptions" />
        </Form.Item>
        <Form.Item label="内部对象类型">
          <Select v-model:value="groupForm.internalObjectType" :options="objectTypeOptions" />
        </Form.Item>
        <Form.Item v-if="groupForm.internalObjectType === 'Project'" label="选择项目">
          <Select :options="projectOptions" allow-clear show-search @change="(value) => applyProjectToGroup(value as string)" />
        </Form.Item>
        <Form.Item label="内部对象 ID" required>
          <Input v-model:value="groupForm.internalObjectId" :maxlength="128" />
        </Form.Item>
        <Form.Item label="内部对象名称">
          <Input v-model:value="groupForm.internalObjectName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="机器人编码">
          <Input v-model:value="groupForm.robotCode" :maxlength="128" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="groupForm.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="groupForm.remark" :auto-size="{ minRows: 3, maxRows: 6 }" />
        </Form.Item>
      </Form>
    </Drawer>

    <Drawer v-model:open="mappingDrawerOpen" :destroy-on-close="true" :title="mappingEditing ? '编辑 ID 映射' : '新增 ID 映射'" width="680">
      <template #extra>
        <Space>
          <Button @click="mappingDrawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submitMapping">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="mappingForm.connectorId" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="内部对象类型" required>
          <Select v-model:value="mappingForm.internalObjectType" :options="objectTypeOptions" />
        </Form.Item>
        <Form.Item label="内部对象 ID" required>
          <Input v-model:value="mappingForm.internalObjectId" :maxlength="128" />
        </Form.Item>
        <Form.Item label="外部对象类型" required>
          <Input v-model:value="mappingForm.externalObjectType" :maxlength="64" />
        </Form.Item>
        <Form.Item label="外部对象 ID" required>
          <Input v-model:value="mappingForm.externalObjectId" :maxlength="256" />
        </Form.Item>
        <Form.Item label="来源">
          <Input v-model:value="mappingForm.source" :maxlength="32" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="mappingForm.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="mappingForm.remark" :auto-size="{ minRows: 3, maxRows: 6 }" />
        </Form.Item>
      </Form>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-bindings {
  display: flex;
  flex-direction: column;
   @apply gap-3;
}

.integration-bindings__metrics {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
   @apply gap-3;
}

.integration-bindings__toolbar {
  display: grid;
  grid-template-columns: minmax(240px, 1fr) 220px 180px auto;
   @apply gap-3;
  align-items: center;
   @apply mb-3;
}

.integration-bindings__stack-cell {
  display: flex;
  min-width: 0;
  flex-direction: column;
   @apply gap-0.5;
  line-height: 1.35;
}

.integration-bindings__stack-cell strong,
.integration-bindings__stack-cell span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-bindings__stack-cell span {
  color: rgb(100 116 139);
  font-size: 12px;
}

@media (max-width: 1100px) {
  .integration-bindings__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .integration-bindings__toolbar {
    grid-template-columns: 1fr;
  }
}
</style>
