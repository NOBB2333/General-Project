<script lang="ts" setup>
import type { TenantApi } from '#/api/core';

import { computed, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Select,
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
  type MenuApi as MenuApiNamespace,
} from '#/api/core';

defineOptions({ name: 'PlatformTenantPage' });

type DrawerType = 'api' | 'menu' | 'users';

interface UiTreeNode {
  children?: UiTreeNode[];
  key: string;
  title: string;
}

const API_OPTIONS = [
  'Platform.Organization.Manage',
  'Platform.User.Manage',
  'Platform.Role.Manage',
  'Platform.Tenant.Manage',
  'Platform.Menu.Manage',
  'Platform.File.Manage',
];

const columns = [
  { dataIndex: 'name', key: 'name', title: '租户名称', width: 180 },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 90 },
  { dataIndex: 'defaultConnectionString', ellipsis: true, key: 'defaultConnectionString', title: '默认连接串', width: 280 },
  { dataIndex: 'creationTime', key: 'creationTime', title: '创建时间', width: 180 },
  { key: 'actions', title: '操作', width: 380 },
];

const loading = ref(false);
const saving = ref(false);
const items = ref<TenantApi.TenantItem[]>([]);
const modalVisible = ref(false);
const drawerVisible = ref(false);
const drawerLoading = ref(false);
const drawerType = ref<DrawerType>('menu');
const activeTenant = ref<null | TenantApi.TenantItem>(null);
const tenantUsers = ref<TenantApi.TenantUserItem[]>([]);

const tenantUserColumns = [
  { dataIndex: 'username', key: 'username', title: '账号', width: 160 },
  { dataIndex: 'displayName', key: 'displayName', title: '姓名', width: 140 },
  { dataIndex: 'email', key: 'email', title: '邮箱' },
  { dataIndex: 'isActive', key: 'isActive', title: '状态', width: 80 },
];
const treeData = ref<UiTreeNode[]>([]);

const formState = reactive({
  defaultConnectionString: '',
  name: '',
});

const authorizationState = reactive<TenantApi.TenantAuthorization>({
  apiBlacklist: [],
  isActive: true,
  menuIds: [],
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
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  formState.name = '';
  formState.defaultConnectionString = '';
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
      defaultConnectionString: formState.defaultConnectionString || null,
      name: formState.name.trim(),
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
      authorizationState.apiBlacklist = [...authorization.apiBlacklist];
      authorizationState.isActive = authorization.isActive;
      authorizationState.menuIds = [...authorization.menuIds];
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
      apiBlacklist: authorizationState.apiBlacklist,
      isActive: authorizationState.isActive,
      menuIds: authorizationState.menuIds,
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
  <Page description="租户管理升级为租户台账，并支持菜单授权与接口黑名单配置。" title="租户管理">
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
              <template v-else-if="column.key === 'actions'">
                <Space wrap>
                  <Button size="small" @click="openDrawer('menu', record as TenantApi.TenantItem)">菜单授权</Button>
                  <Button size="small" @click="openDrawer('api', record as TenantApi.TenantItem)">接口黑名单</Button>
                  <Button size="small" @click="openDrawer('users', record as TenantApi.TenantItem)">查看账号</Button>
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
        </Form>
      </Modal>

      <Drawer
        v-model:open="drawerVisible"
        destroy-on-close
        placement="right"
        width="560"
        :title="activeTenant ? `${activeTenant.name} · ${drawerType === 'menu' ? '菜单授权' : drawerType === 'users' ? '账号列表' : '接口黑名单'}` : ''"
      >
        <Skeleton :loading="drawerLoading" active>
          <div v-if="activeTenant" class="tenant-drawer">
            <template v-if="drawerType === 'users'">
              <Table
                :columns="tenantUserColumns"
                :data-source="tenantUsers"
                :pagination="{ pageSize: 10 }"
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

            <template v-else>
              <Form layout="vertical">
                <Form.Item label="租户状态">
                  <Select
                    :value="authorizationState.isActive ? 'active' : 'inactive'"
                    :options="[
                      { label: '启用', value: 'active' },
                      { label: '停用', value: 'inactive' },
                    ]"
                    @update:value="(value) => { authorizationState.isActive = value === 'active'; }"
                  />
                </Form.Item>
              </Form>

              <template v-if="drawerType === 'menu'">
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
                  <Form.Item label="接口黑名单">
                    <Select
                      v-model:value="authorizationState.apiBlacklist"
                      mode="multiple"
                      :options="API_OPTIONS.map((item) => ({ label: item, value: item }))"
                    />
                  </Form.Item>
                </Form>
              </template>

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
  gap: 12px;
  justify-content: flex-end;
  margin-top: auto;
  padding-top: 16px;
}
</style>
