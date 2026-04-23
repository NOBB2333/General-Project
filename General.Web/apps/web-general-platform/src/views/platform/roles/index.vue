<script lang="ts" setup>
import type { MenuApi, OrganizationApi, RoleApi, UserApi } from '#/api/core';

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
  Radio,
  Select,
  Skeleton,
  Space,
  Table,
  Tag,
  Tree,
  message,
} from 'ant-design-vue';

import {
  createRoleApi,
  deleteRoleApi,
  getMenuPermissionTreeApi,
  getOrganizationTreeApi,
  getRoleAuthorizationApi,
  getRoleListApi,
  getUserListApi,
  saveRoleAuthorizationApi,
  saveRoleMenusApi,
} from '#/api/core';

defineOptions({ name: 'PlatformRolePage' });

type DrawerType = 'api' | 'account' | 'data' | 'menu';

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

const ACCOUNT_SCOPE_OPTIONS = [
  { label: '数据范围 + 指定账号', value: 'data_and_users' },
  { label: '仅数据范围', value: 'data' },
  { label: '仅指定账号', value: 'only_users' },
  { label: '全部账号', value: 'all' },
];

const DATA_SCOPE_OPTIONS = [
  { label: '本组织及下级', value: 'current_org_and_descendants' },
  { label: '本组织', value: 'current_org' },
  { label: '仅本人相关', value: 'self' },
  { label: '自定义组织', value: 'custom' },
  { label: '全部数据', value: 'all' },
];

const columns = [
  { dataIndex: 'name', key: 'name', title: '角色名称', width: 180 },
  { dataIndex: 'description', key: 'description', title: '说明' },
  { dataIndex: 'userCount', key: 'userCount', title: '用户数', width: 90 },
  { dataIndex: 'menuCount', key: 'menuCount', title: '菜单数', width: 90 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 90 },
  { key: 'actions', title: '授权动作', width: 420 },
];

const roles = ref<RoleApi.RoleItem[]>([]);
const roleLoading = ref(true);
const createVisible = ref(false);
const saving = ref(false);
const drawerVisible = ref(false);
const drawerLoading = ref(false);
const activeRole = ref<null | RoleApi.RoleItem>(null);
const drawerType = ref<DrawerType>('menu');
const treeData = ref<UiTreeNode[]>([]);
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const users = ref<UserApi.UserListItem[]>([]);

const createForm = reactive<RoleApi.RoleSaveInput>({ name: '' });
const authorizationState = reactive<RoleApi.RoleAuthorization>({
  apiBlacklist: [],
  accountScopeMode: 'data_and_users',
  accountUserIds: [],
  customOrganizationUnitIds: [],
  dataScopeMode: 'current_org_and_descendants',
  menuIds: [],
});

const accountOptions = computed(() =>
  users.value.map((item) => ({
    label: `${item.displayName} (${item.username})`,
    value: item.id,
  })),
);

const metrics = computed(() => [
  { label: '平台角色', value: roles.value.length || 0 },
  {
    label: '角色总用户数',
    value: roles.value.reduce((total, item) => total + item.userCount, 0),
  },
  {
    label: '使用接口黑名单角色',
    value: roles.value.filter((item) => item.apiBlacklist.length > 0).length,
  },
  {
    label: '自定义数据范围角色',
    value: roles.value.filter((item) => item.dataScopeMode === 'custom').length,
  },
]);

function normalizeMenuTree(items: MenuApi.PermissionTreeItem[]): UiTreeNode[] {
  return items.map((item) => ({
    children: normalizeMenuTree(item.children || []),
    key: item.id,
    title: `[${item.appCode}] ${item.title}${item.permissionCode ? ` · ${item.permissionCode}` : ''}`,
  }));
}

function normalizeOrganizationTree(items: OrganizationApi.OrganizationTreeItem[]): UiTreeNode[] {
  return items.map((item) => ({
    children: normalizeOrganizationTree(item.children || []),
    key: item.id,
    title: item.displayName,
  }));
}

async function loadBaseData() {
  roleLoading.value = true;
  try {
    const [roleResult, menuResult, organizationResult, userResult] = await Promise.all([
      getRoleListApi(),
      getMenuPermissionTreeApi('platform,project,business'),
      getOrganizationTreeApi(),
      getUserListApi(),
    ]);
    roles.value = roleResult;
    treeData.value = normalizeMenuTree(menuResult);
    organizationTree.value = organizationResult;
    users.value = userResult;
  } finally {
    roleLoading.value = false;
  }
}

async function loadRoleAuthorization(role: RoleApi.RoleItem) {
  drawerLoading.value = true;
  try {
    const authorization = await getRoleAuthorizationApi(role.id);
    authorizationState.apiBlacklist = [...authorization.apiBlacklist];
    authorizationState.accountScopeMode = authorization.accountScopeMode;
    authorizationState.accountUserIds = [...authorization.accountUserIds];
    authorizationState.customOrganizationUnitIds = [...authorization.customOrganizationUnitIds];
    authorizationState.dataScopeMode = authorization.dataScopeMode;
    authorizationState.menuIds = [...authorization.menuIds];
  } finally {
    drawerLoading.value = false;
  }
}

async function openDrawer(type: DrawerType, role: RoleApi.RoleItem) {
  activeRole.value = role;
  drawerType.value = type;
  drawerVisible.value = true;
  await loadRoleAuthorization(role);
}

async function saveCurrentDrawer() {
  if (!activeRole.value) {
    return;
  }

  saving.value = true;
  try {
    if (drawerType.value === 'menu') {
      await saveRoleMenusApi(activeRole.value.id, authorizationState.menuIds);
      message.success('菜单授权已保存');
    } else {
      await saveRoleAuthorizationApi(activeRole.value.id, {
        apiBlacklist: authorizationState.apiBlacklist,
        accountScopeMode: authorizationState.accountScopeMode,
        accountUserIds: authorizationState.accountUserIds,
        customOrganizationUnitIds: authorizationState.customOrganizationUnitIds,
        dataScopeMode: authorizationState.dataScopeMode,
      });
      message.success('授权配置已保存');
    }

    drawerVisible.value = false;
    await loadBaseData();
  } finally {
    saving.value = false;
  }
}

async function handleCreateRole() {
  if (!createForm.name.trim()) {
    message.warning('请输入角色名称');
    return;
  }

  saving.value = true;
  try {
    await createRoleApi({ name: createForm.name.trim() });
    message.success('角色已创建');
    createVisible.value = false;
    createForm.name = '';
    await loadBaseData();
  } finally {
    saving.value = false;
  }
}

async function handleDeleteRole(id: string) {
  await deleteRoleApi(id);
  message.success('角色已删除');
  await loadBaseData();
}

function resolveDrawerTitle() {
  if (!activeRole.value) {
    return '';
  }
  const labelMap: Record<DrawerType, string> = {
    api: '接口黑名单',
    account: '账号范围',
    data: '数据范围',
    menu: '菜单授权',
  };
  return `${activeRole.value.name} · ${labelMap[drawerType.value]}`;
}

onMounted(loadBaseData);
</script>

<template>
  <Page
    description="角色权限页按角色行内动作统一承接菜单授权、数据范围、账号范围和接口黑名单。"
    title="角色权限"
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

      <Card class="platform-page__card" title="角色列表">
        <template #extra>
          <Button type="primary" @click="createVisible = true">新增角色</Button>
        </template>

        <Skeleton :loading="roleLoading" active>
          <div v-if="roles.length === 0" class="platform-page__empty">
            <Empty description="未找到角色数据" />
          </div>
          <Table
            v-else
            :columns="columns"
            :data-source="roles"
            :pagination="{ pageSize: 10 }"
            row-key="id"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'status'">
                <Tag :color="record.status ? 'success' : 'default'">
                  {{ record.status ? '启用' : '停用' }}
                </Tag>
              </template>
              <template v-else-if="column.key === 'actions'">
                <Space wrap>
                  <Button size="small" type="default" @click="openDrawer('menu', record as RoleApi.RoleItem)">菜单</Button>
                  <Button size="small" type="default" @click="openDrawer('data', record as RoleApi.RoleItem)">数据</Button>
                  <Button size="small" type="default" @click="openDrawer('account', record as RoleApi.RoleItem)">账号</Button>
                  <Button size="small" type="default" @click="openDrawer('api', record as RoleApi.RoleItem)">接口</Button>
                  <Button danger size="small" type="link" @click="handleDeleteRole((record as RoleApi.RoleItem).id)">
                    删除
                  </Button>
                </Space>
              </template>
            </template>
          </Table>
        </Skeleton>
      </Card>

      <Modal
        v-model:open="createVisible"
        :confirm-loading="saving"
        title="新增角色"
        @ok="handleCreateRole"
      >
        <Form layout="vertical">
          <Form.Item label="角色名称" required>
            <Input v-model:value="createForm.name" :maxlength="64" />
          </Form.Item>
        </Form>
      </Modal>

      <Drawer
        v-model:open="drawerVisible"
        :confirm-loading="saving"
        destroy-on-close
        placement="right"
        width="560"
        :title="resolveDrawerTitle()"
        @close="activeRole = null"
      >
        <Skeleton :loading="drawerLoading" active>
          <div v-if="activeRole" class="role-drawer">
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

            <template v-else-if="drawerType === 'data'">
              <Form layout="vertical">
                <Form.Item label="数据范围模式">
                  <Radio.Group v-model:value="authorizationState.dataScopeMode">
                    <Space direction="vertical">
                      <Radio
                        v-for="item in DATA_SCOPE_OPTIONS"
                        :key="item.value"
                        :value="item.value"
                      >
                        {{ item.label }}
                      </Radio>
                    </Space>
                  </Radio.Group>
                </Form.Item>
                <Form.Item
                  v-if="authorizationState.dataScopeMode === 'custom'"
                  label="自定义组织节点"
                >
                  <Tree
                    v-model:checkedKeys="authorizationState.customOrganizationUnitIds"
                    block-node
                    checkable
                    default-expand-all
                    :tree-data="normalizeOrganizationTree(organizationTree)"
                  />
                </Form.Item>
              </Form>
            </template>

            <template v-else-if="drawerType === 'account'">
              <Form layout="vertical">
                <Form.Item label="当前角色成员">
                  <div v-if="activeRole" class="role-member-list">
                    <template v-for="user in users.filter(u => u.roles.includes(activeRole!.name))" :key="user.id">
                      <Tag color="blue">{{ user.displayName }}（{{ user.username }}）</Tag>
                    </template>
                    <span
                      v-if="!users.filter(u => u.roles.includes(activeRole!.name)).length"
                      class="role-member-list__empty"
                    >
                      暂无用户分配此角色
                    </span>
                  </div>
                </Form.Item>
                <Form.Item label="账号范围模式">
                  <Select
                    v-model:value="authorizationState.accountScopeMode"
                    :options="ACCOUNT_SCOPE_OPTIONS"
                  />
                </Form.Item>
                <Form.Item
                  v-if="authorizationState.accountScopeMode !== 'data' && authorizationState.accountScopeMode !== 'all'"
                  label="指定账号"
                >
                  <Select
                    v-model:value="authorizationState.accountUserIds"
                    mode="multiple"
                    :options="accountOptions"
                    placeholder="选择允许访问的账号"
                  />
                </Form.Item>
              </Form>
            </template>

            <template v-else>
              <Form layout="vertical">
                <Form.Item label="接口黑名单">
                  <Select
                    v-model:value="authorizationState.apiBlacklist"
                    mode="multiple"
                    :options="API_OPTIONS.map((item) => ({ label: item, value: item }))"
                    placeholder="选择禁止访问的接口权限码"
                  />
                </Form.Item>
              </Form>
            </template>

            <div class="role-drawer__footer">
              <Button @click="drawerVisible = false">取消</Button>
              <Button :loading="saving" type="primary" @click="saveCurrentDrawer">保存</Button>
            </div>
          </div>
        </Skeleton>
      </Drawer>
    </section>
  </Page>
</template>

<style scoped>
.role-drawer {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

.role-drawer__footer {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: auto;
  padding-top: 16px;
}

.role-member-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.role-member-list__empty {
  color: var(--ant-color-text-quaternary);
  font-size: 13px;
}
</style>
