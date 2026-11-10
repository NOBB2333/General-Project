import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import type { PageAction } from '@/components/page/page.contracts';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, type UiSelectOption } from '@/components/ui';
import { apiClient } from '@/api/request';
import { getOrganizationUnitTreeApi, type OrganizationApi } from '@/api/core/organization';
import { createIntegrationBindingsApi, type IntegrationBindingsApi } from '@/api/core/integration-bindings';
import { getUserListApi, type UserApi } from '@/api/core/user';
import { canAccess } from '@/directives/permission';
import {
  bindingStatusFilterValue,
  bindingStatusLabel,
  bindingStatusOptions,
  parseExternalAccountImportJson,
  toAccountRow,
  toDepartmentBindingRow,
  toGroupBindingRow,
  toIdMappingRow,
  toUserBindingRow,
  type BindingRow,
  type BindingTab,
} from './bindings.model';

type DialogKind =
  'account' | 'auto-match' | 'department' | 'group' | 'import' | 'mapping' | 'user-bind' | 'user-resolve';
type DraftKey =
  | 'avatarUrl'
  | 'connectorId'
  | 'departmentExternalId'
  | 'displayName'
  | 'email'
  | 'externalAccountId'
  | 'externalDepartmentId'
  | 'externalDepartmentName'
  | 'externalGroupId'
  | 'externalGroupName'
  | 'externalObjectId'
  | 'externalObjectType'
  | 'externalUnionId'
  | 'externalUserId'
  | 'groupType'
  | 'internalObjectId'
  | 'internalObjectName'
  | 'internalObjectType'
  | 'internalOrganizationUnitId'
  | 'internalUserId'
  | 'matchType'
  | 'mobile'
  | 'parentExternalDepartmentId'
  | 'remark'
  | 'robotCode'
  | 'source'
  | 'status';

type BindingDraft = Record<DraftKey, string>;

interface DraftField {
  hint?: string;
  key: DraftKey;
  kind?: 'select';
  label: string;
  maxLength?: number;
  options?: readonly UiSelectOption[];
  required?: boolean;
}

interface Props {
  initialTab: BindingTab;
}

const api = createIntegrationBindingsApi(apiClient);
const pageSize = 20;
const tabItems: ReadonlyArray<{ label: string; value: BindingTab }> = [
  { label: '外部账号', value: 'accounts' },
  { label: '用户绑定', value: 'users' },
  { label: '部门绑定', value: 'departments' },
  { label: '群与机器人', value: 'groups' },
  { label: '内外部 ID 映射', value: 'mappings' },
];
const statusFilterLabels = ['全部状态', ...bindingStatusOptions.map((option) => option.label)];
const allConnectorLabel = '全部连接器';
const statusSelectOptions: readonly UiSelectOption[] = bindingStatusOptions;
const groupTypeOptions: readonly UiSelectOption[] = [
  { label: '群聊', value: 'Chat' },
  { label: '机器人', value: 'Robot' },
  { label: '频道', value: 'Channel' },
];
const internalObjectTypeOptions: readonly UiSelectOption[] = [
  { label: '项目', value: 'Project' },
  { label: '组织', value: 'OrganizationUnit' },
  { label: '用户', value: 'User' },
  { label: '经营合同', value: 'BusinessContract' },
];
const externalObjectTypeOptions: readonly UiSelectOption[] = [
  { label: '外部用户', value: 'ExternalUser' },
  { label: '外部部门', value: 'ExternalDepartment' },
  { label: '外部群', value: 'ExternalGroup' },
];

function emptyDraft(): BindingDraft {
  return {
    avatarUrl: '',
    connectorId: '',
    departmentExternalId: '',
    displayName: '',
    email: '',
    externalAccountId: '',
    externalDepartmentId: '',
    externalDepartmentName: '',
    externalGroupId: '',
    externalGroupName: '',
    externalObjectId: '',
    externalObjectType: 'ExternalUser',
    externalUnionId: '',
    externalUserId: '',
    groupType: 'Chat',
    internalObjectId: '',
    internalObjectName: '',
    internalObjectType: 'Project',
    internalOrganizationUnitId: '',
    internalUserId: '',
    matchType: 'Manual',
    mobile: '',
    parentExternalDepartmentId: '',
    remark: '',
    robotCode: '',
    source: 'Manual',
    status: 'Pending',
  };
}

function errorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message ? error.message : fallback;
}

function connectorLabel(connector: IntegrationBindingsApi.ConnectorOption | undefined): string {
  return connector ? `${connector.displayName} · ${connector.connectorCode}` : '未知连接器';
}

function flattenOrganizationUnits(
  nodes: readonly OrganizationApi.TreeItem[],
  depth = 0,
  result: UiSelectOption[] = [],
): UiSelectOption[] {
  for (const node of nodes) {
    result.push({
      disabled: node.disabled,
      label: `${'　'.repeat(depth)}${node.displayName}`,
      value: node.id,
    });
    flattenOrganizationUnits(node.children, depth + 1, result);
  }
  return result;
}

export function useIntegrationBindings(props: Props) {
  const route = useRoute();
  const activeTab = ref<BindingTab>(props.initialTab);
  const connectors = ref<IntegrationBindingsApi.ConnectorOption[]>([]);
  const externalAccounts = ref<IntegrationBindingsApi.ExternalAccount[]>([]);
  const userBindings = ref<IntegrationBindingsApi.UserBinding[]>([]);
  const departmentBindings = ref<IntegrationBindingsApi.DepartmentBinding[]>([]);
  const groupBindings = ref<IntegrationBindingsApi.GroupBinding[]>([]);
  const idMappings = ref<IntegrationBindingsApi.IdMapping[]>([]);
  const organizations = ref<OrganizationApi.TreeItem[]>([]);
  const users = ref<UserApi.ListItem[]>([]);
  const loading = ref(false);
  const connectorsLoading = ref(false);
  const usersLoading = ref(false);
  const organizationsLoading = ref(false);
  const saving = ref(false);
  const currentPage = ref(1);
  const keyword = ref('');
  const selectedStatus = ref('All');
  const selectedConnectorId = ref('');
  const totalCount = ref(0);
  const loadError = ref('');
  const connectorError = ref('');
  const dialogError = ref('');
  const userError = ref('');
  const organizationError = ref('');
  const userSearch = ref('');
  const importJson = ref(
    '[\n  {\n    "externalUserId": "demo-user-001",\n    "displayName": "示例成员",\n    "email": "member@example.test",\n    "mobile": "13800000000"\n  }\n]',
  );
  const dialogKind = ref<DialogKind | null>(null);
  const dialogOpen = ref(false);
  const editingId = ref('');
  const pageSurfaceKey = ref(0);
  const draft = reactive<BindingDraft>(emptyDraft());
  const activeRequest = ref<AbortController | null>(null);
  const searchTimer = ref<number | undefined>();
  const userSearchTimer = ref<number | undefined>();
  let userRequestVersion = 0;
  let organizationsLoaded = false;
  let started = false;
  const canManage = computed(() => canAccess('Integration.Binding.Manage'));
  const canImport = computed(() => canAccess('Integration.Binding.Import'));

  const connectorOptions = computed<UiSelectOption[]>(() =>
    connectors.value.map((connector) => ({
      label: connectorLabel(connector),
      value: connector.id,
    })),
  );
  const connectorFilterOptions = computed(() => [
    allConnectorLabel,
    ...connectorOptions.value.map((option) => option.label),
  ]);
  const connectorFilterLabel = computed(
    () =>
      connectorOptions.value.find((option) => option.value === selectedConnectorId.value)?.label ?? allConnectorLabel,
  );
  const organizationOptions = computed<UiSelectOption[]>(() => [
    { label: '未绑定内部组织', value: '' },
    ...flattenOrganizationUnits(organizations.value),
  ]);
  const userOptions = computed<UiSelectOption[]>(() =>
    users.value.map((user) => ({
      disabled: !user.isActive,
      label: `${user.displayName || user.username}（${user.username}）${user.organizationUnitNames.length ? ` · ${user.organizationUnitNames.join('、')}` : ''}${user.isActive ? '' : ' · 已停用'}`,
      value: user.id,
    })),
  );

  const rows = computed<BindingRow[]>(() => {
    switch (activeTab.value) {
      case 'accounts':
        return externalAccounts.value.map((item) =>
          toAccountRow(item, connectorLabel(connectors.value.find((connector) => connector.id === item.connectorId))),
        );
      case 'users':
        return userBindings.value.map((item) =>
          toUserBindingRow(
            item,
            connectorLabel(connectors.value.find((connector) => connector.id === item.connectorId)),
          ),
        );
      case 'departments':
        return departmentBindings.value.map((item) =>
          toDepartmentBindingRow(
            item,
            connectorLabel(connectors.value.find((connector) => connector.id === item.connectorId)),
          ),
        );
      case 'groups':
        return groupBindings.value.map((item) =>
          toGroupBindingRow(
            item,
            connectorLabel(connectors.value.find((connector) => connector.id === item.connectorId)),
          ),
        );
      case 'mappings':
        return idMappings.value.map((item) =>
          toIdMappingRow(item, connectorLabel(connectors.value.find((connector) => connector.id === item.connectorId))),
        );
    }
  });

  const pageActions = computed<PageAction[]>(() => {
    const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
    if (activeTab.value === 'accounts') {
      if (canManage.value) {
        actions.push({ id: 'new', kind: 'create', label: '新增外部账号', tone: 'primary' });
        actions.push({ id: 'bind-user', kind: 'custom', label: '绑定内部用户', requiresSelection: true });
      }
      if (canImport.value) actions.push({ id: 'import', kind: 'import', label: '批量导入' });
    } else if (activeTab.value === 'users') {
      if (canManage.value) {
        actions.push({ id: 'auto-match', kind: 'custom', label: '自动匹配' });
        actions.push({ id: 'resolve-user', kind: 'custom', label: '处理所选', requiresSelection: true });
      }
    } else if (canManage.value) {
      const label =
        activeTab.value === 'departments'
          ? '新增部门绑定'
          : activeTab.value === 'groups'
            ? '新增群绑定'
            : '新增 ID 映射';
      actions.push({ id: 'new', kind: 'create', label, tone: 'primary' });
    }
    return actions;
  });

  const columns = computed(() => {
    switch (activeTab.value) {
      case 'accounts':
        return [
          { key: 'name', title: '外部账号', width: 190, sticky: true },
          { key: 'connector', title: '连接器', width: 190 },
          { key: 'externalId', title: '外部用户 ID', width: 210 },
          { key: 'related', title: '邮箱 / 手机 / 部门 ID', width: 300 },
          { key: 'status', title: '状态', width: 120, presentation: 'status' as const },
        ];
      case 'users':
        return [
          { key: 'name', title: '外部成员', width: 190, sticky: true },
          { key: 'connector', title: '连接器', width: 190 },
          { key: 'externalId', title: '外部用户 ID', width: 210 },
          { key: 'related', title: '内部用户 / 冲突原因', width: 320 },
          { key: 'status', title: '绑定状态', width: 130, presentation: 'status' as const },
        ];
      case 'departments':
        return [
          { key: 'name', title: '外部部门', width: 210, sticky: true },
          { key: 'connector', title: '连接器', width: 190 },
          { key: 'externalId', title: '外部部门 ID', width: 210 },
          { key: 'related', title: '内部组织', width: 280 },
          { key: 'status', title: '绑定状态', width: 130, presentation: 'status' as const },
        ];
      case 'groups':
        return [
          { key: 'name', title: '外部群 / 机器人', width: 210, sticky: true },
          { key: 'connector', title: '连接器', width: 190 },
          { key: 'externalId', title: '外部群 ID', width: 210 },
          { key: 'related', title: '内部对象', width: 280 },
          { key: 'status', title: '绑定状态', width: 130, presentation: 'status' as const },
        ];
      case 'mappings':
        return [
          { key: 'name', title: '内部对象', width: 260, sticky: true },
          { key: 'connector', title: '连接器', width: 190 },
          { key: 'externalId', title: '外部对象', width: 280 },
          { key: 'related', title: '来源', width: 130 },
          { key: 'status', title: '映射状态', width: 130, presentation: 'status' as const },
        ];
    }
  });

  const pageLabel = computed(() => (route.name === 'integration-groups' ? '群与机器人' : '账号绑定'));
  const pageModel = computed<ListPageModel<BindingRow>>(() => ({
    actions: pageActions.value,
    columns: columns.value,
    demoPath: route.path,
    description:
      activeTab.value === 'accounts'
        ? '维护外部账号资料，并与 General.Admin 用户、组织和业务对象建立真实映射。'
        : activeTab.value === 'users'
          ? '按外部账号和内部用户的真实绑定状态处理匹配与冲突。'
          : activeTab.value === 'departments'
            ? '将外部部门标识关联到 General.Admin 组织单元。'
            : activeTab.value === 'groups'
              ? '维护外部群、机器人与内部业务对象的标识绑定，不调用外部平台创建群。'
              : '查看和维护服务端用于内外部对象互认的 ID 映射。',
    emptyHint: loading.value
      ? '正在读取绑定记录…'
      : loadError.value
        ? '读取失败，请使用刷新操作重试。'
        : '当前筛选条件下没有绑定记录。',
    filters: [
      { key: 'connector', label: '连接器', options: connectorFilterOptions.value },
      { key: 'status', label: '状态', options: statusFilterLabels },
    ],
    key: `外部集成|${pageLabel.value}|${activeTab.value}`,
    label: pageLabel.value,
    listScopes: [{ id: 'all', label: '全部记录', count: totalCount.value }],
    listTabs: [{ id: 'all', label: '全部记录' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: '外部集成',
    moduleSlug: 'integration',
    pageSlug: 'integration-bindings',
    pageSize,
    panels: [],
    path: route.path,
    primaryAction: pageActions.value.find((action) => action.kind === 'create')?.label ?? '刷新',
    replication: { status: 'implemented', referenceUrl: route.path },
    routeName: String(route.name ?? 'integration-bindings'),
    rows: rows.value,
    searchPlaceholder:
      activeTab.value === 'accounts'
        ? '搜索账号名称、外部用户 ID、邮箱或手机号'
        : activeTab.value === 'users'
          ? '搜索外部成员、用户或冲突原因'
          : activeTab.value === 'departments'
            ? '搜索部门名称、外部部门 ID 或内部组织'
            : activeTab.value === 'groups'
              ? '搜索群名称、外部群 ID 或内部对象'
              : '搜索内外部对象类型或 ID',
    stats: [
      { key: 'total', label: '筛选结果', value: String(totalCount.value), tone: 'primary' },
      { key: 'page', label: '当前页记录', value: String(rows.value.length), tone: 'success' },
      { key: 'connectors', label: '连接器', value: String(connectors.value.length), tone: 'neutral' },
      {
        key: 'access',
        label: '当前权限',
        value: canManage.value ? '可维护' : '只读',
        tone: canManage.value ? 'warning' : 'neutral',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  }));

  const remoteFilterValues = computed(() => ({
    connector: connectorFilterLabel.value,
    status: statusFilterLabel.value,
  }));
  const remotePagination = computed(() => ({
    currentPage: currentPage.value,
    pageSize,
    searchQuery: keyword.value,
    totalCount: totalCount.value,
  }));
  const dialogTitle = computed(() => {
    switch (dialogKind.value) {
      case 'account':
        return `${editingId.value ? '编辑' : '新增'}外部账号`;
      case 'import':
        return '批量导入外部账号';
      case 'user-bind':
        return '绑定内部用户';
      case 'user-resolve':
        return '处理用户绑定冲突';
      case 'auto-match':
        return '执行账号自动匹配';
      case 'department':
        return `${editingId.value ? '编辑' : '新增'}部门绑定`;
      case 'group':
        return `${editingId.value ? '编辑' : '新增'}群绑定`;
      case 'mapping':
        return `${editingId.value ? '编辑' : '新增'} ID 映射`;
      default:
        return '';
    }
  });
  const dialogDescription = computed(() => {
    switch (dialogKind.value) {
      case 'account':
        return '同一连接器下重复的外部用户 ID 会更新现有账号资料。';
      case 'import':
        return '导入会按“连接器 + 外部用户 ID”新增或覆盖账号资料；仅接受下方列出的文本字段。';
      case 'user-bind':
        return '绑定后会同步更新外部账号状态，并写入用户与外部账号的 ID 映射。';
      case 'user-resolve':
        return '确认内部用户后，服务端会更新绑定、外部账号状态和 ID 映射。';
      case 'auto-match':
        return '会扫描该连接器下所有非停用账号，并重新计算已有匹配结果。';
      case 'department':
        return '同一连接器下重复的外部部门 ID 会更新现有绑定；选定组织后服务端会同步 ID 映射。';
      case 'group':
        return '同一连接器下重复的外部群 ID 会更新现有绑定；此操作只保存映射，不会在外部平台创建群。';
      case 'mapping':
        return '相同连接器、内外部对象类型与 ID 会更新同一条映射。';
      default:
        return '';
    }
  });

  const dialogFields = computed<DraftField[]>(() => {
    const connectorField: DraftField = {
      key: 'connectorId',
      label: '连接器',
      kind: 'select',
      options: connectorOptions.value,
      required: true,
    };
    const statusField: DraftField = {
      key: 'status',
      label: '状态',
      kind: 'select',
      options: statusSelectOptions,
      required: true,
    };
    switch (dialogKind.value) {
      case 'account':
        return [
          connectorField,
          { key: 'externalUserId', label: '外部用户 ID', maxLength: 256, required: true },
          { key: 'displayName', label: '显示名称', maxLength: 128 },
          { key: 'email', label: '邮箱', maxLength: 256 },
          { key: 'mobile', label: '手机号', maxLength: 64 },
          { key: 'externalUnionId', label: '外部 Union ID', maxLength: 256 },
          { key: 'avatarUrl', label: '头像地址', maxLength: 512 },
          { key: 'departmentExternalId', label: '外部部门 ID', maxLength: 256 },
          statusField,
        ];
      case 'user-bind':
      case 'user-resolve':
        return [
          {
            key: 'internalUserId',
            label: '内部用户',
            kind: 'select',
            options: userOptions.value,
            required: true,
          },
          { key: 'remark', label: '备注', maxLength: 512 },
        ];
      case 'department':
        return [
          connectorField,
          { key: 'externalDepartmentId', label: '外部部门 ID', maxLength: 256, required: true },
          { key: 'externalDepartmentName', label: '外部部门名称', maxLength: 128 },
          { key: 'parentExternalDepartmentId', label: '上级外部部门 ID', maxLength: 256 },
          {
            key: 'internalOrganizationUnitId',
            label: '内部组织',
            kind: 'select',
            options: organizationOptions.value,
            hint: organizationError.value || undefined,
          },
          statusField,
          { key: 'remark', label: '备注', maxLength: 512 },
        ];
      case 'group':
        return [
          connectorField,
          { key: 'externalGroupId', label: '外部群 ID', maxLength: 256, required: true },
          { key: 'externalGroupName', label: '外部群名称', maxLength: 128 },
          { key: 'groupType', label: '群类型', kind: 'select', options: groupTypeOptions, required: true },
          {
            key: 'internalObjectType',
            label: '内部对象类型',
            kind: 'select',
            options: internalObjectTypeOptions,
            required: true,
          },
          { key: 'internalObjectId', label: '内部对象 ID', maxLength: 128 },
          { key: 'internalObjectName', label: '内部对象名称', maxLength: 128 },
          { key: 'robotCode', label: '机器人编码', maxLength: 128 },
          statusField,
          { key: 'remark', label: '备注', maxLength: 512 },
        ];
      case 'mapping':
        return [
          connectorField,
          {
            key: 'internalObjectType',
            label: '内部对象类型',
            kind: 'select',
            options: internalObjectTypeOptions,
            required: true,
          },
          { key: 'internalObjectId', label: '内部对象 ID', maxLength: 128, required: true },
          {
            key: 'externalObjectType',
            label: '外部对象类型',
            kind: 'select',
            options: externalObjectTypeOptions,
            required: true,
          },
          { key: 'externalObjectId', label: '外部对象 ID', maxLength: 256, required: true },
          { key: 'source', label: '来源', maxLength: 32, required: true },
          statusField,
          { key: 'remark', label: '备注', maxLength: 512 },
        ];
      default:
        return [];
    }
  });

  async function loadConnectors(): Promise<void> {
    connectorsLoading.value = true;
    connectorError.value = '';
    try {
      connectors.value = await api.getConnectors();
    } catch (error) {
      connectorError.value = errorMessage(error, '连接器选项读取失败。');
    } finally {
      connectorsLoading.value = false;
    }
  }

  function makeQuery(): IntegrationBindingsApi.BindingQuery {
    return {
      connectorId: selectedConnectorId.value || undefined,
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize,
      skipCount: (currentPage.value - 1) * pageSize,
      status: selectedStatus.value,
    };
  }

  async function loadCurrentTab(): Promise<void> {
    activeRequest.value?.abort();
    const controller = new AbortController();
    activeRequest.value = controller;
    loading.value = true;
    loadError.value = '';
    const query = makeQuery();
    try {
      switch (activeTab.value) {
        case 'accounts': {
          const result = await api.getExternalAccounts(query, controller.signal);
          if (!controller.signal.aborted) {
            externalAccounts.value = result.items;
            totalCount.value = result.totalCount;
          }
          break;
        }
        case 'users': {
          const result = await api.getUserBindings(query, controller.signal);
          if (!controller.signal.aborted) {
            userBindings.value = result.items;
            totalCount.value = result.totalCount;
          }
          break;
        }
        case 'departments': {
          const result = await api.getDepartmentBindings(query, controller.signal);
          if (!controller.signal.aborted) {
            departmentBindings.value = result.items;
            totalCount.value = result.totalCount;
          }
          break;
        }
        case 'groups': {
          const result = await api.getGroupBindings(query, controller.signal);
          if (!controller.signal.aborted) {
            groupBindings.value = result.items;
            totalCount.value = result.totalCount;
          }
          break;
        }
        case 'mappings': {
          const result = await api.getIdMappings(query, controller.signal);
          if (!controller.signal.aborted) {
            idMappings.value = result.items;
            totalCount.value = result.totalCount;
          }
          break;
        }
      }
    } catch (error) {
      if (!controller.signal.aborted) loadError.value = errorMessage(error, '绑定记录读取失败，请重试。');
    } finally {
      if (activeRequest.value === controller) loading.value = false;
    }
  }

  function updateDraft(key: DraftKey, value: string): void {
    draft[key] = value;
  }

  function openDialog(kind: DialogKind, values?: Partial<BindingDraft>, id = ''): void {
    Object.assign(
      draft,
      emptyDraft(),
      {
        connectorId: selectedConnectorId.value || connectors.value[0]?.id || '',
      },
      values ?? {},
    );
    editingId.value = id;
    dialogError.value = '';
    dialogKind.value = kind;
    dialogOpen.value = true;
    if (kind === 'user-bind' || kind === 'user-resolve') {
      userSearch.value = values?.displayName || '';
      void loadUsers(userSearch.value);
    }
    if (kind === 'department') void loadOrganizations();
  }

  async function loadUsers(search: string): Promise<void> {
    const requestVersion = ++userRequestVersion;
    usersLoading.value = true;
    userError.value = '';
    try {
      const result = await getUserListApi({
        keyword: search.trim() || undefined,
        maxResultCount: 100,
        skipCount: 0,
      });
      if (requestVersion === userRequestVersion) users.value = result.items;
    } catch (error) {
      if (requestVersion === userRequestVersion) userError.value = errorMessage(error, '内部用户读取失败。');
    } finally {
      if (requestVersion === userRequestVersion) usersLoading.value = false;
    }
  }

  async function loadOrganizations(): Promise<void> {
    if (organizationsLoaded || organizationsLoading.value) return;
    organizationsLoading.value = true;
    organizationError.value = '';
    try {
      organizations.value = await getOrganizationUnitTreeApi();
      organizationsLoaded = true;
    } catch (error) {
      organizationError.value = errorMessage(error, '内部组织读取失败。');
    } finally {
      organizationsLoading.value = false;
    }
  }

  function openNewForTab(): void {
    if (activeTab.value === 'accounts') openDialog('account');
    else if (activeTab.value === 'departments') openDialog('department');
    else if (activeTab.value === 'groups') openDialog('group');
    else if (activeTab.value === 'mappings') openDialog('mapping');
  }

  function findSelectedRow(selectedRows: ListPageRow[]): BindingRow | null {
    if (selectedRows.length !== 1) return null;
    return rows.value.find((row) => row.id === selectedRows[0]?.id) ?? null;
  }

  function handleAction(action: PageAction, selectedRows: ListPageRow[]): void {
    if (action.id === 'reload') {
      pageSurfaceKey.value += 1;
      void loadCurrentTab();
      return;
    }
    if (action.id === 'new' && canManage.value) {
      openNewForTab();
      return;
    }
    if (action.id === 'import' && canImport.value) {
      importJson.value =
        '[\n  {\n    "externalUserId": "demo-user-001",\n    "displayName": "示例成员",\n    "email": "member@example.test",\n    "mobile": "13800000000"\n  }\n]';
      openDialog('import');
      return;
    }
    if (action.id === 'auto-match' && canManage.value) {
      openDialog('auto-match');
      return;
    }
    if (action.id === 'bind-user' && canManage.value) {
      const row = findSelectedRow(selectedRows);
      const account = row ? externalAccounts.value.find((item) => item.id === row.id) : undefined;
      if (!account) {
        toast.warning('请仅选择一条当前页外部账号记录。');
        return;
      }
      openDialog('user-bind', {
        displayName: account.displayName,
        externalAccountId: account.id,
      });
      return;
    }
    if (action.id === 'resolve-user' && canManage.value) {
      const row = findSelectedRow(selectedRows);
      const binding = row ? userBindings.value.find((item) => item.id === row.id) : undefined;
      if (!binding) {
        toast.warning('请仅选择一条当前页用户绑定记录。');
        return;
      }
      openDialog(
        'user-resolve',
        {
          displayName: binding.internalUserName || binding.externalDisplayName,
          externalAccountId: binding.externalAccountId,
          internalUserId: binding.internalUserId ?? '',
          remark: binding.remark,
        },
        binding.id,
      );
    }
  }

  function openRow(row: BindingRow): void {
    if (!canManage.value) {
      toast.info('当前账号只有查看权限，不能编辑绑定信息。');
      return;
    }
    switch (activeTab.value) {
      case 'accounts': {
        const item = externalAccounts.value.find((account) => account.id === row.id);
        if (item) {
          openDialog(
            'account',
            {
              avatarUrl: item.avatarUrl,
              connectorId: item.connectorId,
              departmentExternalId: item.departmentExternalId,
              displayName: item.displayName,
              email: item.email,
              externalUnionId: item.externalUnionId,
              externalUserId: item.externalUserId,
              mobile: item.mobile,
              status: item.status,
            },
            item.id,
          );
        }
        break;
      }
      case 'users': {
        const item = userBindings.value.find((binding) => binding.id === row.id);
        if (item) {
          openDialog(
            'user-resolve',
            {
              displayName: item.internalUserName || item.externalDisplayName,
              externalAccountId: item.externalAccountId,
              internalUserId: item.internalUserId ?? '',
              remark: item.remark,
            },
            item.id,
          );
        }
        break;
      }
      case 'departments': {
        const item = departmentBindings.value.find((binding) => binding.id === row.id);
        if (item) {
          openDialog(
            'department',
            {
              connectorId: item.connectorId,
              externalDepartmentId: item.externalDepartmentId,
              externalDepartmentName: item.externalDepartmentName,
              internalOrganizationUnitId: item.internalOrganizationUnitId ?? '',
              matchType: item.matchType || 'Manual',
              parentExternalDepartmentId: item.parentExternalDepartmentId,
              remark: item.remark,
              status: item.status,
            },
            item.id,
          );
        }
        break;
      }
      case 'groups': {
        const item = groupBindings.value.find((binding) => binding.id === row.id);
        if (item) {
          openDialog(
            'group',
            {
              connectorId: item.connectorId,
              externalGroupId: item.externalGroupId,
              externalGroupName: item.externalGroupName,
              groupType: item.groupType || 'Chat',
              internalObjectId: item.internalObjectId,
              internalObjectName: item.internalObjectName,
              internalObjectType: item.internalObjectType || 'Project',
              remark: item.remark,
              robotCode: item.robotCode,
              status: item.status,
            },
            item.id,
          );
        }
        break;
      }
      case 'mappings': {
        const item = idMappings.value.find((mapping) => mapping.id === row.id);
        if (item) {
          openDialog(
            'mapping',
            {
              connectorId: item.connectorId,
              externalObjectId: item.externalObjectId,
              externalObjectType: item.externalObjectType,
              internalObjectId: item.internalObjectId,
              internalObjectType: item.internalObjectType,
              remark: item.remark,
              source: item.source || 'Manual',
              status: item.status,
            },
            item.id,
          );
        }
        break;
      }
    }
  }

  function handleSearch(value: string): void {
    keyword.value = value;
    currentPage.value = 1;
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
    searchTimer.value = window.setTimeout(() => void loadCurrentTab(), 250);
  }

  function handleFilterChange(key: string, value: string): void {
    if (key === 'status') selectedStatus.value = bindingStatusFilterValue(value);
    if (key === 'connector') {
      selectedConnectorId.value = connectorOptions.value.find((option) => option.label === value)?.value ?? '';
    }
    currentPage.value = 1;
    pageSurfaceKey.value += 1;
    void loadCurrentTab();
  }

  function changePage(page: number): void {
    currentPage.value = page;
    pageSurfaceKey.value += 1;
    void loadCurrentTab();
  }

  function handleUserSearch(value: string): void {
    userSearch.value = value;
    if (userSearchTimer.value !== undefined) window.clearTimeout(userSearchTimer.value);
    userSearchTimer.value = window.setTimeout(() => void loadUsers(value), 250);
  }

  function validateDialog(): string | null {
    if ((dialogKind.value === 'auto-match' || dialogKind.value === 'import') && !draft.connectorId) {
      return '请选择连接器。';
    }
    if (dialogKind.value === 'user-bind' && !draft.externalAccountId) return '请先选择一个外部账号。';
    if (dialogKind.value === 'user-resolve' && !editingId.value) return '绑定记录已失效，请刷新列表后重试。';
    const required = dialogFields.value.find((field) => field.required && !draft[field.key].trim());
    if (required) return `请填写${required.label}。`;
    if ((dialogKind.value === 'department' || dialogKind.value === 'group') && draft.status === 'Bound') {
      if (dialogKind.value === 'department' && !draft.internalOrganizationUnitId)
        return '绑定状态为“已绑定”时必须选择内部组织。';
      if (dialogKind.value === 'group' && (!draft.internalObjectType.trim() || !draft.internalObjectId.trim())) {
        return '绑定状态为“已绑定”时必须填写内部对象类型和 ID。';
      }
    }
    return null;
  }

  async function submitDialog(): Promise<void> {
    if (!dialogKind.value) return;
    dialogError.value = '';
    const validationError = validateDialog();
    if (validationError) {
      dialogError.value = validationError;
      return;
    }
    saving.value = true;
    try {
      switch (dialogKind.value) {
        case 'account':
          await api.saveExternalAccount({
            avatarUrl: draft.avatarUrl.trim(),
            connectorId: draft.connectorId,
            departmentExternalId: draft.departmentExternalId.trim(),
            displayName: draft.displayName.trim(),
            email: draft.email.trim(),
            externalUnionId: draft.externalUnionId.trim(),
            externalUserId: draft.externalUserId.trim(),
            mobile: draft.mobile.trim(),
            status: draft.status,
          });
          break;
        case 'import': {
          const parsed = parseExternalAccountImportJson(importJson.value, draft.connectorId);
          if (parsed.error) {
            dialogError.value = parsed.error;
            return;
          }
          const result = await api.importExternalAccounts({
            accounts: parsed.accounts,
            connectorId: draft.connectorId,
          });
          dialogOpen.value = false;
          await loadCurrentTab();
          toast.success(`导入完成：新增 ${result.createdCount} 条，更新 ${result.updatedCount} 条。`);
          return;
        }
        case 'user-bind':
          await api.bindUser({
            externalAccountId: draft.externalAccountId,
            internalUserId: draft.internalUserId,
            matchType: 'Manual',
            remark: draft.remark.trim(),
          });
          break;
        case 'user-resolve':
          await api.resolveUserBinding(editingId.value, {
            internalUserId: draft.internalUserId,
            remark: draft.remark.trim(),
            status: 'Bound',
          });
          break;
        case 'auto-match': {
          const result = await api.autoMatchUsers(draft.connectorId);
          dialogOpen.value = false;
          await loadCurrentTab();
          toast.success(
            `自动匹配完成：扫描 ${result.scannedCount} 条，绑定 ${result.boundCount} 条，冲突 ${result.conflictCount} 条，待处理 ${result.pendingCount} 条。`,
          );
          return;
        }
        case 'department':
          await api.saveDepartmentBinding({
            connectorId: draft.connectorId,
            externalDepartmentId: draft.externalDepartmentId.trim(),
            externalDepartmentName: draft.externalDepartmentName.trim(),
            internalOrganizationUnitId: draft.internalOrganizationUnitId || null,
            matchType: draft.matchType || 'Manual',
            parentExternalDepartmentId: draft.parentExternalDepartmentId.trim(),
            remark: draft.remark.trim(),
            status: draft.status,
          });
          break;
        case 'group':
          await api.saveGroupBinding({
            connectorId: draft.connectorId,
            externalGroupId: draft.externalGroupId.trim(),
            externalGroupName: draft.externalGroupName.trim(),
            groupType: draft.groupType,
            internalObjectId: draft.internalObjectId.trim(),
            internalObjectName: draft.internalObjectName.trim(),
            internalObjectType: draft.internalObjectType,
            remark: draft.remark.trim(),
            robotCode: draft.robotCode.trim(),
            status: draft.status,
          });
          break;
        case 'mapping':
          await api.saveIdMapping({
            connectorId: draft.connectorId,
            externalObjectId: draft.externalObjectId.trim(),
            externalObjectType: draft.externalObjectType,
            internalObjectId: draft.internalObjectId.trim(),
            internalObjectType: draft.internalObjectType,
            remark: draft.remark.trim(),
            source: draft.source.trim(),
            status: draft.status,
          });
          break;
      }
      dialogOpen.value = false;
      await loadCurrentTab();
      toast.success('绑定信息已保存，并已重新读取列表。');
    } catch (error) {
      dialogError.value = errorMessage(error, '操作失败，请检查输入后重试。');
    } finally {
      saving.value = false;
    }
  }

  function selectTab(value: string): void {
    if (!tabItems.some((item) => item.value === value)) return;
    activeTab.value = value as BindingTab;
  }

  const statusFilterLabel = computed(() => {
    if (selectedStatus.value === 'All') return '全部状态';
    return bindingStatusLabel(selectedStatus.value).replace(/^待处理/, '存在');
  });

  watch(
    () => props.initialTab,
    (value) => {
      activeTab.value = value;
    },
  );
  watch(activeTab, () => {
    currentPage.value = 1;
    keyword.value = '';
    selectedStatus.value = 'All';
    selectedConnectorId.value = '';
    pageSurfaceKey.value += 1;
    if (started) void loadCurrentTab();
  });

  onMounted(async () => {
    await loadConnectors();
    started = true;
    await loadCurrentTab();
  });
  onBeforeUnmount(() => {
    activeRequest.value?.abort();
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
    if (userSearchTimer.value !== undefined) window.clearTimeout(userSearchTimer.value);
  });

  return {
    activeTab,
    canImport,
    canManage,
    changePage,
    connectorError,
    connectorOptions,
    connectorFilterLabel,
    connectorFilterOptions,
    connectors,
    connectorsLoading,
    dialogDescription,
    dialogError,
    dialogFields,
    dialogKind,
    dialogOpen,
    dialogTitle,
    draft,
    handleAction,
    handleFilterChange,
    handleSearch,
    handleUserSearch,
    importJson,
    keyword,
    loadError,
    loading,
    organizationError,
    organizationsLoading,
    pageModel,
    pageSurfaceKey,
    remoteFilterValues,
    remotePagination,
    saving,
    selectTab,
    submitDialog,
    tabItems,
    updateDraft,
    userError,
    userSearch,
    usersLoading,
    openRow,
  };
}
