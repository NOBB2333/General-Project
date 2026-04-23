<script lang="ts" setup>
import type { TenantApi } from '#/api/core';

import { computed, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Checkbox,
  Collapse,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Skeleton,
  Space,
  Table,
  Tag,
  Tree,
  message,
} from 'ant-design-vue';

import {
  createTenantApi,
  deleteTenantApi,
  getMenuPermissionTreeApi,
  getTenantAuthorizationApi,
  getTenantListApi,
  getTenantUsersApi,
  saveTenantAuthorizationApi,
  setTenantStatusApi,
  getUserListApi,
  type MenuApi as MenuApiNamespace,
} from '#/api/core';

defineOptions({ name: 'PlatformTenantPage' });

type DrawerType = 'api' | 'menu' | 'users';

interface UiTreeNode {
  children?: UiTreeNode[];
  key: string;
  title: string;
}

const API_GROUPS = [
  {
    controller: '平台治理',
    items: [
      'GET:/api/app/audit-log/dashboard',
      'GET:/api/app/audit-log/list',
      'GET:/api/app/menu/all',
      'GET:/api/app/organization-unit/tree',
      'GET:/api/app/system-monitor',
      'GET:/api/app/update-log/list',
    ],
  },
  {
    controller: '用户与认证',
    items: [
      'GET:/api/app/auth/codes',
      'GET:/api/app/user/info',
      'GET:/api/app/user/list',
      'PUT:/api/app/user/password',
    ],
  },
  {
    controller: '项目与经营',
    items: [
      'GET:/api/app/business/overview',
      'GET:/api/app/business/projects',
      'GET:/api/app/business/projects/{projectId}',
      'GET:/api/app/business/reports',
      'GET:/api/app/project/detail/{projectId}',
      'GET:/api/app/project/list',
      'GET:/api/app/project/my-related',
      'GET:/api/app/project/raid/list',
      'GET:/api/app/project/task/list',
      'GET:/api/app/project/workspace',
    ],
  },
  {
    controller: '文件与任务',
    items: [
      'POST:/api/app/platform/scheduler/{jobKey}/run',
      'POST:/api/app/platform/scheduler/{jobKey}/toggle',
      'GET:/api/app/file/list',
      'POST:/api/app/file/upload',
      'DELETE:/api/app/file/{fileKey}',
    ],
  },
];

const columns = [
  { dataIndex: 'name', key: 'name', title: '租户名称', width: 180 },
  { dataIndex: 'adminUserName', key: 'adminUserName', title: '管理员账号', width: 140 },
  { dataIndex: 'adminEmail', key: 'adminEmail', title: '管理员邮箱', width: 220 },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 90 },
  { dataIndex: 'defaultConnectionString', key: 'defaultConnectionString', title: '默认连接串' },
  { dataIndex: 'remark', key: 'remark', title: '备注', width: 220 },
  { dataIndex: 'creationTime', key: 'creationTime', title: '创建时间', width: 180 },
  { key: 'actions', title: '操作', width: 420 },
];

const loading = ref(false);
const saving = ref(false);
const items = ref<TenantApi.TenantItem[]>([]);
const modalVisible = ref(false);
const drawerVisible = ref(false);
const drawerLoading = ref(false);
const drawerType = ref<DrawerType>('menu');
const activeTenant = ref<null | TenantApi.TenantItem>(null);
const treeData = ref<UiTreeNode[]>([]);
const userOptions = ref<Array<{ label: string; value: string }>>([]);
const tenantUsers = ref<TenantApi.TenantUserItem[]>([]);

const tenantUserColumns = [
  { dataIndex: 'username', key: 'username', title: '账号', width: 160 },
  { dataIndex: 'displayName', key: 'displayName', title: '姓名', width: 140 },
  { dataIndex: 'email', key: 'email', title: '邮箱' },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 90 },
];

const formState = reactive({
  adminUserId: undefined as string | undefined,
  defaultConnectionString: '',
  name: '',
  remark: '',
});

const authorizationState = reactive<TenantApi.TenantAuthorization>({
  adminUserId: undefined,
  apiBlacklist: [],
  isActive: true,
  menuIds: [],
  remark: undefined,
});

const metrics = computed(() => [
  { label: '租户数量', value: items.value.length },
  { label: '启用租户', value: items.value.filter((item) => item.isActive).length },
  {
    label: '使用接口黑名单租户',
    value: items.value.filter((item) => item.apiBlacklist.length > 0).length,
  },
]);

function normalizeMenuTree(items: MenuApiNamespace.PermissionTreeItem[]): UiTreeNode[] {
  return items.map((item) => ({
    children: normalizeMenuTree(item.children || []),
    key: item.id,
    title: `[${item.appCode}] ${item.title}`,
  }));
}

async function loadTenants() {
  loading.value = true;
  try {
    const [tenants, menus] = await Promise.all([
      getTenantListApi(),
      getMenuPermissionTreeApi('platform,project,business'),
    ]);
    items.value = tenants;
    treeData.value = normalizeMenuTree(menus);
    const users = await getUserListApi();
    userOptions.value = users.map((item) => ({
      label: `${item.username} · ${item.displayName}`,
      value: item.id,
    }));
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  formState.adminUserId = undefined;
  formState.name = '';
  formState.defaultConnectionString = '';
  formState.remark = '';
  modalVisible.value = true;
}

async function handleSubmit() {
  if (!formState.name.trim()) {
    message.warning('请输入租户名称');
    return;
  }

  saving.value = true;
  try {
    await createTenantApi({
      adminUserId: formState.adminUserId || null,
      defaultConnectionString: formState.defaultConnectionString || null,
      name: formState.name.trim(),
      remark: formState.remark || null,
    });
    message.success('租户已创建');
    modalVisible.value = false;
    await loadTenants();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await deleteTenantApi(id);
  message.success('租户已删除');
  await loadTenants();
}

async function handleToggleStatus(record: TenantApi.TenantItem) {
  await setTenantStatusApi(record.id, !record.isActive);
  message.success(`租户已${record.isActive ? '停用' : '启用'}`);
  await loadTenants();
}

async function openDrawer(type: DrawerType, tenant: TenantApi.TenantItem) {
  activeTenant.value = tenant;
  drawerType.value = type;
  drawerVisible.value = true;
  drawerLoading.value = true;
  try {
    if (type === 'users') {
      tenantUsers.value = await getTenantUsersApi(tenant.id);
    } else {
      const authorization = await getTenantAuthorizationApi(tenant.id);
      authorizationState.adminUserId = authorization.adminUserId || undefined;
      authorizationState.apiBlacklist = [...authorization.apiBlacklist];
      authorizationState.isActive = authorization.isActive;
      authorizationState.menuIds = [...authorization.menuIds];
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
    await saveTenantAuthorizationApi(activeTenant.value.id, {
      adminUserId: authorizationState.adminUserId || null,
      apiBlacklist: authorizationState.apiBlacklist,
      isActive: authorizationState.isActive,
      menuIds: authorizationState.menuIds,
      remark: authorizationState.remark || null,
    });
    message.success('租户授权已保存');
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

      <Card class="platform-page__card" title="租户台账">
        <template #extra>
          <Button type="primary" @click="openCreate">新增租户</Button>
        </template>

        <Skeleton :loading="loading" active>
          <div v-if="items.length === 0" class="platform-page__empty">
            <Empty description="暂无租户" />
          </div>
          <Table
            v-else
            :columns="columns"
            :data-source="items"
            :pagination="{ pageSize: 10 }"
            row-key="id"
            :scroll="{ x: 1280 }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'isActive'">
                <Tag :color="record.isActive ? 'success' : 'default'">
                  {{ record.isActive ? '启用' : '停用' }}
                </Tag>
              </template>
              <template v-else-if="column.key === 'creationTime'">
                {{ new Date(record.creationTime).toLocaleString() }}
              </template>
              <template v-else-if="column.key === 'adminUserName'">
                {{ record.adminUserName || '-' }}
              </template>
              <template v-else-if="column.key === 'adminEmail'">
                {{ record.adminEmail || '-' }}
              </template>
              <template v-else-if="column.key === 'remark'">
                {{ record.remark || '-' }}
              </template>
              <template v-else-if="column.key === 'actions'">
                <Space wrap>
                  <Popconfirm
                    :title="`确认${record.isActive ? '停用' : '启用'}租户【${record.name}】？`"
                    @confirm="handleToggleStatus(record as TenantApi.TenantItem)"
                  >
                    <Button size="small">
                      {{ record.isActive ? '停用' : '启用' }}
                    </Button>
                  </Popconfirm>
                  <Button size="small" @click="openDrawer('users', record as TenantApi.TenantItem)">查看账号</Button>
                  <Button size="small" @click="openDrawer('menu', record as TenantApi.TenantItem)">菜单授权</Button>
                  <Button size="small" @click="openDrawer('api', record as TenantApi.TenantItem)">接口黑名单</Button>
                  <Button danger size="small" type="link" @click="handleDelete((record as TenantApi.TenantItem).id)">
                    删除
                  </Button>
                </Space>
              </template>
            </template>
          </Table>
        </Skeleton>
      </Card>

      <Modal
        v-model:open="modalVisible"
        :confirm-loading="saving"
        title="新增租户"
        @ok="handleSubmit"
      >
        <Form layout="vertical">
          <Form.Item label="租户名称" required>
            <Input v-model:value="formState.name" :maxlength="64" placeholder="例如：demo-tenant" />
          </Form.Item>
          <Form.Item label="默认连接串">
            <Input
              v-model:value="formState.defaultConnectionString"
              :maxlength="512"
              placeholder="可选，不填则沿用默认数据库"
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
      </Modal>

      <Drawer
        v-model:open="drawerVisible"
        destroy-on-close
        placement="right"
        width="680"
        :title="activeTenant ? `${activeTenant.name} · ${drawerType === 'menu' ? '菜单授权' : drawerType === 'api' ? '接口黑名单' : '账号列表'}` : ''"
      >
        <Skeleton :loading="drawerLoading" active>
          <div v-if="activeTenant" class="tenant-drawer">
            <template v-if="drawerType === 'users'">
              <Table
                :columns="tenantUserColumns"
                :data-source="tenantUsers"
                :pagination="false"
                row-key="id"
                size="small"
              >
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'isActive'">
                    <Tag :color="(record as TenantApi.TenantUserItem).isActive ? 'success' : 'default'">
                      {{ (record as TenantApi.TenantUserItem).isActive ? '启用' : '停用' }}
                    </Tag>
                  </template>
                </template>
              </Table>
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
                <Form.Item label="接口黑名单">
                  <Collapse :bordered="false" ghost>
                    <Collapse.Panel
                      v-for="group in API_GROUPS"
                      :key="group.controller"
                      :header="group.controller"
                    >
                      <Checkbox.Group
                        v-model:value="authorizationState.apiBlacklist"
                        class="api-group"
                        :options="group.items.map((item) => ({ label: item, value: item }))"
                      />
                    </Collapse.Panel>
                  </Collapse>
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
.tenant-drawer {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

.tenant-drawer__footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: auto;
  padding-top: 16px;
}

.api-group {
  display: grid;
  gap: 10px;
}

.api-group :deep(.ant-checkbox-group-item) {
  display: flex;
  margin-inline-end: 0;
  white-space: normal;
}

.platform-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-page__metrics {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.platform-page__metric,
.platform-page__card {
  border-radius: 20px;
}

.platform-page__metric {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px 18px;
  border: 1px solid var(--ant-color-border-secondary);
  background: var(--ant-color-bg-container);
}

.platform-page__metric span {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.platform-page__metric strong {
  font-size: 30px;
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
