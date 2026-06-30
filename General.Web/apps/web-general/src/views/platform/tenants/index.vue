<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { PlatformEndpointApi, TenantApi } from '#/api/core';

import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Checkbox,
  Collapse,
  Drawer,
  Form,
  Input,
  Popconfirm,
  Select,
  Skeleton,
  Space,
  Tag,
  Tree,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createTenantApi,
  deleteTenantApi,
  getMenuPermissionTreeApi,
  getPlatformEndpointOptionsApi,
  getTenantAuthorizationApi,
  getTenantPagedApi,
  getTenantUsersApi,
  saveTenantAuthorizationApi,
  setTenantStatusApi,
  updateTenantApi,
  type MenuApi as MenuApiNamespace,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import {
  collectPlatformLeafKeys,
  normalizePlatformTree,
  type PlatformTreeNode,
} from '#/composables/platform/use-tree-normalization';
import { useAuthStore } from '#/store';

defineOptions({ name: 'PlatformTenantPage' });

type DrawerType = 'api' | 'edit' | 'menu' | 'users';

const TENANT_MANAGE_CODE = 'Platform.Tenant.Manage';

type TenantAuthorizationFormState = Omit<TenantApi.TenantAuthorization, 'adminUserId' | 'remark'> & {
  adminUserId?: string;
  remark?: string;
};

const { hasAccessByCodes } = useAccess();
const canManageTenants = computed(() => hasAccessByCodes([TENANT_MANAGE_CODE]));

const baseColumns = [
  { align: 'left', field: 'name', title: '租户名称', width: 180 },
  { field: 'adminUserName', slots: { default: 'adminUserName' }, title: '管理员账号', width: 140 },
  { align: 'left', field: 'adminEmail', slots: { default: 'adminEmail' }, title: '管理员邮箱', width: 220 },
  { field: 'isActive', slots: { default: 'isActive' }, title: '状态', width: 90 },
  { align: 'left', field: 'defaultConnectionStringDisplay', slots: { default: 'defaultConnectionStringDisplay' }, title: '连接配置', width: 220 },
  { align: 'left', field: 'remark', slots: { default: 'remark' }, title: '备注', width: 220 },
  { field: 'creationTime', slots: { default: 'creationTime' }, title: '创建时间', width: 180 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 560 },
] satisfies VxeGridProps<TenantApi.TenantItem>['columns'];

const columns = computed<VxeGridProps<TenantApi.TenantItem>['columns']>(() =>
  canManageTenants.value
    ? baseColumns
    : baseColumns.map((item) => item.field === 'actions' ? { ...item, width: 120 } : item),
);

const loading = ref(false);
const pageIndex = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const saving = ref(false);
const items = ref<TenantApi.TenantItem[]>([]);
const createDrawerVisible = ref(false);
const drawerVisible = ref(false);
const drawerLoading = ref(false);
const drawerType = ref<DrawerType>('menu');
const activeTenant = ref<null | TenantApi.TenantItem>(null);
const treeData = ref<PlatformTreeNode[]>([]);
const endpointGroups = ref<PlatformEndpointApi.EndpointGroup[]>([]);
const userOptions = ref<Array<{ label: string; value: string }>>([]);
const tenantUsers = ref<TenantApi.TenantUserItem[]>([]);
const { actionLoadingKey, runAction } = useActionLoading();
const authStore = useAuthStore();
const router = useRouter();

const tenantUserColumns = [
  { field: 'username', title: '账号', width: 160 },
  { field: 'displayName', title: '姓名', width: 140 },
  { align: 'left', field: 'email', title: '邮箱', minWidth: 220 },
  { field: 'isActive', slots: { default: 'isActive' }, title: '状态', width: 90 },
] satisfies VxeGridProps<TenantApi.TenantUserItem>['columns'];

const formState = reactive({
  adminEmail: '',
  adminPassword: '',
  adminUserId: undefined as string | undefined,
  adminUserName: '',
  defaultConnectionString: '',
  name: '',
  remark: '',
});

const authorizationState = reactive<TenantAuthorizationFormState>({
  adminUserId: undefined,
  apiBlacklist: [],
  isActive: true,
  menuIds: [],
  remark: undefined,
});

const metrics = computed(() => [
  { label: '租户数量', value: totalCount.value },
  { label: '启用租户', value: items.value.filter((item) => item.isActive).length },
  {
    label: '使用接口黑名单租户',
    value: items.value.filter((item) => item.apiBlacklist.length > 0).length,
  },
]);
const tenantGridOptions = computed<VxeGridProps<TenantApi.TenantItem>>(() => ({
  columns: columns.value,
  data: items.value,
  emptyText: '暂无租户',
  loading: loading.value,
  pagerConfig: {
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: totalCount.value,
  },
  rowConfig: { height: 48, keyField: 'id' },
  scrollX: { enabled: true },
}));
const tenantUserGridOptions = computed<VxeGridProps<TenantApi.TenantUserItem>>(() => ({
  columns: tenantUserColumns,
  data: tenantUsers.value,
  emptyText: '暂无租户账号',
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const gridEvents: NativeVxeGridEvents<TenantApi.TenantItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadTenants();
  },
};

const [TenantGrid] = useVbenVxeGrid<TenantApi.TenantItem>({ gridEvents, gridOptions: {} });
const [TenantUserGrid] = useVbenVxeGrid<TenantApi.TenantUserItem>({ gridOptions: {} });

function normalizeMenuTree(items: MenuApiNamespace.PermissionTreeItem[]): PlatformTreeNode[] {
  return normalizePlatformTree(items, (item) => `[${item.appCode}] ${item.title}`);
}

function filterLeafMenuIds(menuIds: string[]) {
  const leafKeys = collectPlatformLeafKeys(treeData.value);
  return menuIds.filter((id) => leafKeys.has(id));
}

async function loadTenants() {
  loading.value = true;
  try {
    if (!canManageTenants.value) {
      const result = await getTenantPagedApi({
        maxResultCount: pageSize.value,
        skipCount: (pageIndex.value - 1) * pageSize.value,
      });
      items.value = result.items;
      totalCount.value = result.totalCount;
      treeData.value = [];
      endpointGroups.value = [];
      return;
    }

    const [tenantResult, menuResult, endpointResult] = await Promise.allSettled([
      getTenantPagedApi({
        maxResultCount: pageSize.value,
        skipCount: (pageIndex.value - 1) * pageSize.value,
      }),
      getMenuPermissionTreeApi('platform,project,business,ai'),
      getPlatformEndpointOptionsApi(),
    ]);
    items.value = tenantResult.status === 'fulfilled' ? tenantResult.value.items : [];
    totalCount.value = tenantResult.status === 'fulfilled' ? tenantResult.value.totalCount : 0;
    treeData.value = menuResult.status === 'fulfilled' ? normalizeMenuTree(menuResult.value) : [];
    endpointGroups.value = endpointResult.status === 'fulfilled' ? endpointResult.value : [];

    if (
      tenantResult.status === 'rejected' ||
      menuResult.status === 'rejected' ||
      endpointResult.status === 'rejected'
    ) {
      message.warning('租户页面部分数据加载失败，已展示可用内容。');
    }
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  formState.adminEmail = '';
  formState.adminPassword = '';
  formState.adminUserId = undefined;
  formState.adminUserName = '';
  formState.name = '';
  formState.defaultConnectionString = '';
  formState.remark = '';
  createDrawerVisible.value = true;
}

function fillEditForm(tenant: TenantApi.TenantItem) {
  formState.adminEmail = '';
  formState.adminPassword = '';
  formState.adminUserId = tenant.adminUserId || undefined;
  formState.adminUserName = '';
  formState.name = tenant.name;
  formState.defaultConnectionString = '';
  formState.remark = tenant.remark || '';
}

async function handleSubmit() {
  if (!formState.name.trim()) {
    message.warning('请输入租户名称');
    return;
  }
  if (!formState.adminUserName.trim()) {
    message.warning('请输入租户管理员账号');
    return;
  }
  if (!formState.adminPassword.trim()) {
    message.warning('请输入租户管理员初始密码');
    return;
  }

  saving.value = true;
  try {
    await createTenantApi({
      adminEmail: formState.adminEmail.trim() || null,
      adminPassword: formState.adminPassword.trim(),
      adminUserId: null,
      adminUserName: formState.adminUserName.trim(),
      defaultConnectionString: formState.defaultConnectionString || null,
      name: formState.name.trim(),
      remark: formState.remark || null,
    });
    message.success('租户已创建');
    createDrawerVisible.value = false;
    await loadTenants();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await runAction(`delete:${id}`, async () => {
    await deleteTenantApi(id);
    message.success('租户已删除');
    await loadTenants();
  });
}

async function handleToggleStatus(record: TenantApi.TenantItem) {
  await runAction(`toggle:${record.id}`, async () => {
    await setTenantStatusApi(record.id, !record.isActive);
    message.success(`租户已${record.isActive ? '停用' : '启用'}`);
    await loadTenants();
  });
}

async function handleEnterTenant(record: TenantApi.TenantItem) {
  await runAction(`enter:${record.id}`, async () => {
    await authStore.enterTenantOperation(record.id);
    message.success(`已开始运维租户【${record.name}】`);
    await router.replace('/platform/organization');
  });
}

async function openDrawer(type: DrawerType, tenant: TenantApi.TenantItem) {
  if (type !== 'users' && !canManageTenants.value) {
    return;
  }

  activeTenant.value = tenant;
  drawerType.value = type;
  drawerVisible.value = true;
  drawerLoading.value = true;
  try {
    if (type === 'edit') {
      fillEditForm(tenant);
      tenantUsers.value = await getTenantUsersApi(tenant.id);
      userOptions.value = tenantUsers.value.map((item) => ({
        label: `${item.username} · ${item.displayName}`,
        value: item.id,
      }));
    } else if (type === 'users') {
      tenantUsers.value = await getTenantUsersApi(tenant.id);
    } else {
      tenantUsers.value = await getTenantUsersApi(tenant.id);
      userOptions.value = tenantUsers.value.map((item) => ({
        label: `${item.username} · ${item.displayName}`,
        value: item.id,
      }));
      const authorization = await getTenantAuthorizationApi(tenant.id);
      authorizationState.adminUserId = authorization.adminUserId || undefined;
      authorizationState.apiBlacklist = [...authorization.apiBlacklist];
      authorizationState.isActive = authorization.isActive;
      authorizationState.menuIds = filterLeafMenuIds(authorization.menuIds);
      authorizationState.remark = authorization.remark || undefined;
    }
  } finally {
    drawerLoading.value = false;
  }
}

async function saveDrawer() {
  if (!activeTenant.value) {
    return;
  }

  saving.value = true;
  try {
    if (drawerType.value === 'edit') {
      if (!formState.name.trim()) {
        message.warning('请输入租户名称');
        return;
      }
      await updateTenantApi(activeTenant.value.id, {
        adminEmail: null,
        adminPassword: null,
        adminUserId: formState.adminUserId || null,
        adminUserName: null,
        defaultConnectionString: formState.defaultConnectionString.trim() || null,
        name: formState.name.trim(),
        remark: formState.remark || null,
      });
      message.success('租户信息已保存');
    } else {
      await saveTenantAuthorizationApi(activeTenant.value.id, {
        adminUserId: authorizationState.adminUserId || null,
        apiBlacklist: authorizationState.apiBlacklist,
        isActive: authorizationState.isActive,
        menuIds: filterLeafMenuIds(authorizationState.menuIds),
        remark: authorizationState.remark || null,
      });
      message.success('租户授权已保存');
    }
    drawerVisible.value = false;
    await loadTenants();
  } finally {
    saving.value = false;
  }
}

loadTenants();
</script>

<template>
  <Page description="租户管理升级为租户台账，并支持菜单授权与整站接口黑名单配置。" title="租户管理">
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

      <Card :bordered="false" class="platform-page__card rounded-2xl" title="租户台账">
        <template #extra>
          <Button v-if="canManageTenants" type="primary" @click="openCreate">新增租户</Button>
        </template>

        <Skeleton :loading="loading" active>
          <TenantGrid class="app-vxe-card-grid" :grid-events="gridEvents" :grid-options="tenantGridOptions">
            <template #isActive="{ row }">
              <Tag :color="row.isActive ? 'success' : 'default'">
                {{ row.isActive ? '启用' : '停用' }}
              </Tag>
            </template>
            <template #creationTime="{ row }">
              {{ new Date(row.creationTime).toLocaleString() }}
            </template>
            <template #adminUserName="{ row }">
              {{ row.adminUserName || '-' }}
            </template>
            <template #adminEmail="{ row }">
              {{ row.adminEmail || '-' }}
            </template>
            <template #remark="{ row }">
              {{ row.remark || '-' }}
            </template>
            <template #defaultConnectionStringDisplay="{ row }">
              <Space size="small">
                <Tag :color="row.hasDefaultConnectionString ? 'success' : 'default'">
                  {{ row.hasDefaultConnectionString ? '已配置' : '未配置' }}
                </Tag>
                <span>{{ row.defaultConnectionStringDisplay || '-' }}</span>
              </Space>
            </template>
            <template #actions="{ row }">
              <Space class="whitespace-nowrap" size="small">
                <Popconfirm
                  v-if="canManageTenants"
                  :title="`确认${row.isActive ? '停用' : '启用'}租户【${row.name}】？`"
                  @confirm="handleToggleStatus(row)"
                >
                  <Button
                    :loading="actionLoadingKey === `toggle:${row.id}`"
                    size="small"
                  >
                    {{ row.isActive ? '停用' : '启用' }}
                  </Button>
                </Popconfirm>
                <Button
                  v-if="canManageTenants"
                  size="small"
                  @click="openDrawer('edit', row)"
                >
                  编辑信息
                </Button>
                <Button
                  v-if="canManageTenants"
                  :loading="actionLoadingKey === `enter:${row.id}`"
                  size="small"
                  type="primary"
                  @click="handleEnterTenant(row)"
                >
                  运维
                </Button>
                <Button size="small" @click="openDrawer('users', row)">查看账号</Button>
                <Button
                  v-if="canManageTenants"
                  size="small"
                  @click="openDrawer('menu', row)"
                >
                  菜单授权
                </Button>
                <Button
                  v-if="canManageTenants"
                  size="small"
                  @click="openDrawer('api', row)"
                >
                  接口黑名单
                </Button>
                <Button
                  v-if="canManageTenants"
                  danger
                  :loading="actionLoadingKey === `delete:${row.id}`"
                  size="small"
                  type="link"
                  @click="handleDelete(row.id)"
                >
                  删除
                </Button>
              </Space>
            </template>
          </TenantGrid>
        </Skeleton>
      </Card>

      <Drawer
        v-model:open="createDrawerVisible"
        destroy-on-close
        title="新增租户"
        width="640"
      >
        <Form layout="vertical">
          <Form.Item label="租户名称" required>
            <Input v-model:value="formState.name" :maxlength="64" placeholder="例如：demo-tenant" />
          </Form.Item>
          <Form.Item label="默认连接串">
            <Input
              v-model:value="formState.defaultConnectionString"
              :maxlength="512"
              placeholder="仅创建时填写，列表页不会再展示原始连接串"
            />
          </Form.Item>
          <Form.Item label="管理员账号">
            <Input
              v-model:value="formState.adminUserName"
              :maxlength="64"
              placeholder="英文账号，例如 tenant.admin"
            />
          </Form.Item>
          <Form.Item label="管理员邮箱">
            <Input
              v-model:value="formState.adminEmail"
              :maxlength="256"
              placeholder="可选，留空自动生成"
            />
          </Form.Item>
          <Form.Item label="初始密码" required>
            <Input.Password
              v-model:value="formState.adminPassword"
              :maxlength="128"
              placeholder="请输入租户管理员初始密码"
            />
          </Form.Item>
          <Form.Item label="备注">
            <Input.TextArea v-model:value="formState.remark" :maxlength="256" :rows="3" />
          </Form.Item>
        </Form>
        <template #footer>
          <div class="flex justify-end gap-2">
            <Button @click="createDrawerVisible = false">取消</Button>
            <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
          </div>
        </template>
      </Drawer>

      <Drawer
        v-model:open="drawerVisible"
        destroy-on-close
        placement="right"
        width="680"
        :title="activeTenant ? `${activeTenant.name} · ${drawerType === 'edit' ? '编辑信息' : drawerType === 'menu' ? '菜单授权' : drawerType === 'api' ? '接口黑名单' : '账号列表'}` : ''"
      >
        <Skeleton :loading="drawerLoading" active>
          <div v-if="activeTenant" class="tenant-drawer">
            <template v-if="drawerType === 'edit'">
              <Form layout="vertical">
                <Form.Item label="租户名称" required>
                  <Input v-model:value="formState.name" :maxlength="64" placeholder="例如：demo-tenant" />
                </Form.Item>
                <Form.Item label="默认连接串">
                  <Input
                    v-model:value="formState.defaultConnectionString"
                    :maxlength="512"
                    placeholder="留空表示保持原配置不变"
                  />
                </Form.Item>
                <Form.Item label="管理员账号">
                  <Select
                    v-model:value="formState.adminUserId"
                    allow-clear
                    placeholder="选择该租户管理员"
                    :options="userOptions"
                  />
                </Form.Item>
                <Form.Item label="备注">
                  <Input.TextArea v-model:value="formState.remark" :maxlength="256" :rows="3" />
                </Form.Item>
              </Form>
            </template>

            <template v-else-if="drawerType === 'users'">
              <TenantUserGrid class="app-vxe-card-grid" :grid-options="tenantUserGridOptions">
                <template #isActive="{ row }">
                  <Tag :color="row.isActive ? 'success' : 'default'">
                    {{ row.isActive ? '启用' : '停用' }}
                  </Tag>
                </template>
              </TenantUserGrid>
              <div class="tenant-drawer__footer">
                <Button @click="drawerVisible = false">关闭</Button>
              </div>
            </template>

            <template v-else-if="drawerType === 'menu'">
              <Tree
                v-model:checkedKeys="authorizationState.menuIds"
                block-node
                checkable
                default-expand-all
                :height="620"
                :tree-data="treeData"
              />
            </template>

            <template v-else>
              <Form layout="vertical">
                <Form.Item label="管理员账号">
                  <Select
                    v-model:value="authorizationState.adminUserId"
                    allow-clear
                    placeholder="选择该租户管理员"
                    :options="userOptions"
                  />
                </Form.Item>
                <Form.Item label="备注">
                  <Input.TextArea
                    :value="authorizationState.remark || ''"
                    :maxlength="256"
                    :rows="3"
                    @update:value="(value) => { authorizationState.remark = value || undefined; }"
                  />
                </Form.Item>
                <Form.Item label="接口黑名单（选中 = 禁止访问）">
                  <Collapse :bordered="false" ghost>
                    <Collapse.Panel
                      v-for="group in endpointGroups"
                      :key="group.groupName"
                      :header="group.groupName"
                    >
                      <Checkbox.Group
                        v-model:value="authorizationState.apiBlacklist"
                        class="api-group"
                        :options="group.items.map((item) => ({ label: item.label, value: item.key }))"
                      />
                    </Collapse.Panel>
                  </Collapse>
                  <Empty v-if="endpointGroups.length === 0" description="暂无可配置接口黑名单" />
                </Form.Item>
              </Form>
            </template>

            <template v-if="drawerType !== 'users'">
              <div class="tenant-drawer__footer">
                <Button @click="drawerVisible = false">取消</Button>
                <Button :loading="saving" type="primary" @click="saveDrawer">保存</Button>
              </div>
            </template>
          </div>
        </Skeleton>
      </Drawer>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.tenant-drawer {
  display: flex;
  flex-direction: column;
  @apply gap-4;
  min-height: 100%;
}

.tenant-drawer__footer {
  display: flex;
  justify-content: flex-end;
  @apply gap-3;
  @apply mt-auto;
  @apply pt-4;
}

.api-group {
  display: grid;
  @apply gap-2.5;
}

.api-group :deep(.ant-checkbox-group-item) {
  display: flex;
  @apply me-0;
  white-space: normal;
}

.platform-page {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.platform-page__metrics {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.platform-page__metric,
.platform-page__card {
  @apply rounded-2xl;
}

.platform-page__metric {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  @apply px-4 py-4;
  border: 1px solid var(--ant-color-border-secondary);
  background: var(--ant-color-bg-container);
}

.platform-page__metric span {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.platform-page__metric strong {
  font-size: 24px;
}

.platform-page__empty {
  display: grid;
  min-height: 240px;
  place-items: center;
}

@media (max-width: 1080px) {
  .platform-page__metrics {
    grid-template-columns: 1fr;
  }
}
</style>
