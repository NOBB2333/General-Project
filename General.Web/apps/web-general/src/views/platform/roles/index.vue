<script lang="ts" setup>
import type { MenuApi, OrganizationApi, PlatformEndpointApi, RoleApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

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
  Radio,
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
  getPlatformEndpointOptionsApi,
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

const DATA_SCOPE_OPTIONS = [
  { label: '本组织及下级', value: 'current_org_and_descendants' },
  { label: '本组织', value: 'current_org' },
  { label: '仅本人相关', value: 'self' },
  { label: '自定义组织', value: 'custom' },
  { label: '全部数据', value: 'all' },
];

const ACCOUNT_SCOPE_OPTIONS = [
  { label: '全部账号', value: 'all' },
  { label: '数据范围账号', value: 'data' },
  { label: '指定账号', value: 'only_users' },
  { label: '数据范围 + 指定账号', value: 'data_and_users' },
];

const columns = [
  { dataIndex: 'name', key: 'name', title: '角色名称', width: 180 },
  { dataIndex: 'description', key: 'description', title: '说明', ellipsis: true, width: 320 },
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
const endpointGroups = ref<PlatformEndpointApi.EndpointGroup[]>([]);
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const users = ref<UserApi.UserListItem[]>([]);
const accountKeyword = ref('');

const createForm = reactive<RoleApi.RoleSaveInput>({ name: '' });
const authorizationState = reactive<RoleApi.RoleAuthorization>({
  apiBlacklist: [],
  accountScopeMode: 'only_users',
  accountUserIds: [],
  customOrganizationUnitIds: [],
  dataScopeMode: 'current_org_and_descendants',
  menuIds: [],
});

const accountOptions = computed(() =>
  users.value
    .filter((item) => {
      const keyword = accountKeyword.value.trim().toLowerCase();
      if (!keyword) {
        return true;
      }
      return [item.displayName, item.username, item.email || '']
        .some((value) => value.toLowerCase().includes(keyword));
    })
    .map((item) => ({
      displayName: item.displayName,
      id: item.id,
      organizationUnitNames: item.organizationUnitNames,
      username: item.username,
    })),
);

const selectedAccountItems = computed(() => {
  const selectedIds = new Set(authorizationState.accountUserIds);
  return users.value.filter((item) => selectedIds.has(item.id));
});

const roleMemberItems = computed(() =>
  users.value.filter((item) => item.roles.includes(activeRole.value?.name ?? '')),
);

const accountScopeSummary = computed(() => {
  const scopeLabel = ACCOUNT_SCOPE_OPTIONS.find(
    (item) => item.value === authorizationState.accountScopeMode,
  )?.label ?? authorizationState.accountScopeMode;
  return `${scopeLabel}，已指定 ${authorizationState.accountUserIds.length} 个额外账号`;
});

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

function collectLeafKeys(nodes: UiTreeNode[]): Set<string> {
  const keys = new Set<string>();
  for (const node of nodes) {
    if (node.children && node.children.length > 0) {
      for (const key of collectLeafKeys(node.children)) {
        keys.add(key);
      }
    } else {
      keys.add(node.key);
    }
  }
  return keys;
}

function filterLeafMenuIds(menuIds: string[]) {
  const leafKeys = collectLeafKeys(treeData.value);
  return menuIds.filter((id) => leafKeys.has(id));
}

async function loadBaseData() {
  roleLoading.value = true;
  try {
    const [roleResult, menuResult, organizationResult, userResult, endpointResult] = await Promise.allSettled([
      getRoleListApi(),
      getMenuPermissionTreeApi('platform,project,business'),
      getOrganizationTreeApi(),
      getUserListApi(),
      getPlatformEndpointOptionsApi(),
    ]);
    roles.value = roleResult.status === 'fulfilled' ? roleResult.value : [];
    treeData.value = menuResult.status === 'fulfilled' ? normalizeMenuTree(menuResult.value) : [];
    organizationTree.value = organizationResult.status === 'fulfilled' ? organizationResult.value : [];
    users.value = userResult.status === 'fulfilled' ? userResult.value : [];
    endpointGroups.value = endpointResult.status === 'fulfilled' ? endpointResult.value : [];

    if (
      roleResult.status === 'rejected' ||
      menuResult.status === 'rejected' ||
      organizationResult.status === 'rejected' ||
      userResult.status === 'rejected' ||
      endpointResult.status === 'rejected'
    ) {
      message.warning('角色页面部分数据加载失败，已展示可用内容。');
    }
  } finally {
    roleLoading.value = false;
  }
}

async function loadRoleAuthorization(role: RoleApi.RoleItem) {
  try {
    const authorization = await getRoleAuthorizationApi(role.id);
    authorizationState.apiBlacklist = [...authorization.apiBlacklist];
    authorizationState.accountScopeMode = authorization.accountScopeMode;
    authorizationState.accountUserIds = [...authorization.accountUserIds];
    authorizationState.customOrganizationUnitIds = [...authorization.customOrganizationUnitIds];
    authorizationState.dataScopeMode = authorization.dataScopeMode;
    authorizationState.menuIds = filterLeafMenuIds(authorization.menuIds);
  } finally {
    drawerLoading.value = false;
  }
}

async function openDrawer(type: DrawerType, role: RoleApi.RoleItem) {
  activeRole.value = role;
  drawerType.value = type;
  accountKeyword.value = '';
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
      await saveRoleMenusApi(activeRole.value.id, filterLeafMenuIds(authorizationState.menuIds));
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
    account: '授权账号',
    data: '数据范围',
    menu: '菜单授权',
  };
  return `${activeRole.value.name} · ${labelMap[drawerType.value]}`;
}

function toggleAccount(userId: string, checked: boolean) {
  const selectedIds = new Set(authorizationState.accountUserIds);
  if (checked) {
    selectedIds.add(userId);
  } else {
    selectedIds.delete(userId);
  }
  authorizationState.accountUserIds = [...selectedIds];
}

onMounted(loadBaseData);
</script>

<template>
  <Page description="角色权限页按角色行内动作统一承接菜单授权、数据范围、账号授权和接口黑名单。" title="角色权限">
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
            :scroll="{ x: 1280 }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'description'">
                <div class="description-cell">{{ record.description }}</div>
              </template>
              <template v-else-if="column.key === 'status'">
                <Tag :color="record.status ? 'success' : 'default'">
                  {{ record.status ? '启用' : '停用' }}
                </Tag>
              </template>
              <template v-else-if="column.key === 'actions'">
                <Space wrap>
                  <Button size="small" @click="openDrawer('menu', record as RoleApi.RoleItem)">菜单</Button>
                  <Button size="small" @click="openDrawer('data', record as RoleApi.RoleItem)">数据</Button>
                  <Button size="small" @click="openDrawer('account', record as RoleApi.RoleItem)">账号</Button>
                  <Button size="small" @click="openDrawer('api', record as RoleApi.RoleItem)">接口</Button>
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
        width="680"
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
                <Form.Item v-if="authorizationState.dataScopeMode === 'custom'" label="自定义组织节点">
                  <Tree
                    v-model:checkedKeys="authorizationState.customOrganizationUnitIds"
                    block-node
                    checkable
                    default-expand-all
                    :height="420"
                    :tree-data="normalizeOrganizationTree(organizationTree)"
                  />
                </Form.Item>
              </Form>
            </template>

            <template v-else-if="drawerType === 'account'">
              <Form layout="vertical">
                <Form.Item label="账号范围模式">
                  <Radio.Group v-model:value="authorizationState.accountScopeMode">
                    <Space direction="vertical">
                      <Radio
                        v-for="item in ACCOUNT_SCOPE_OPTIONS"
                        :key="item.value"
                        :value="item.value"
                      >
                        {{ item.label }}
                      </Radio>
                    </Space>
                  </Radio.Group>
                </Form.Item>
              </Form>

              <!-- 当前已分配该角色的成员 -->
              <div class="role-member-section">
                <div class="role-member-section__header">
                  <strong>当前角色成员</strong>
                  <Tag color="blue">{{ roleMemberItems.length }}</Tag>
                </div>
                <div
                  v-if="roleMemberItems.length === 0"
                  class="role-member-section__empty"
                >
                  暂无成员分配此角色
                </div>
                <div v-else class="role-member-section__tags">
                  <Tag
                    v-for="u in roleMemberItems"
                    :key="u.id"
                  >
                    {{ u.displayName }} ({{ u.username }})
                  </Tag>
                </div>
              </div>

              <div class="account-scope">
                <div class="account-scope__panel">
                  <div class="account-scope__header">
                    <strong>可指定账号</strong>
                    <Input v-model:value="accountKeyword" allow-clear placeholder="按账号/姓名/邮箱筛选" />
                  </div>
                  <div class="account-scope__hint">
                    实际账号范围由上方模式决定；这里勾选的是“指定账号/额外放行账号”。
                  </div>
                  <div class="account-scope__list">
                    <label
                      v-for="item in accountOptions"
                      :key="item.id"
                      class="account-scope__item"
                    >
                      <Checkbox
                        :checked="authorizationState.accountUserIds.includes(item.id)"
                        @update:checked="(checked) => toggleAccount(item.id, checked)"
                      />
                      <div class="account-scope__meta">
                        <strong>{{ item.displayName }}</strong>
                        <span>{{ item.username }}</span>
                        <small>{{ item.organizationUnitNames.join(' / ') || '未分配部门' }}</small>
                      </div>
                    </label>
                  </div>
                </div>
                <div class="account-scope__panel">
                  <div class="account-scope__header">
                    <strong>已指定账号</strong>
                    <Tag color="blue">{{ authorizationState.accountUserIds.length }}</Tag>
                  </div>
                  <div class="account-scope__hint">{{ accountScopeSummary }}</div>
                  <div v-if="selectedAccountItems.length === 0" class="account-scope__empty">
                    暂未指定账号
                  </div>
                  <div v-else class="account-scope__selected">
                    <Tag
                      v-for="item in selectedAccountItems"
                      :key="item.id"
                      closable
                      @close.prevent="toggleAccount(item.id, false)"
                    >
                      {{ item.displayName }} ({{ item.username }})
                    </Tag>
                  </div>
                </div>
              </div>
            </template>

            <template v-else>
              <Form layout="vertical">
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
.platform-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.platform-page__metrics {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
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

.description-cell {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-height: 1.6;
}

.role-drawer {
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 100%;
}

.role-member-section {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 12px 16px;
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 8px;
  background: var(--ant-color-bg-layout);
}

.role-member-section__header {
  display: flex;
  gap: 8px;
  align-items: center;
}

.role-member-section__tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.role-member-section__empty {
  font-size: 12px;
  color: var(--ant-color-text-quaternary);
}

.account-scope {
  display: grid;
  gap: 16px;
  grid-template-columns: minmax(0, 1.3fr) minmax(280px, 1fr);
}

.account-scope__panel {
  display: flex;
  flex-direction: column;
  gap: 12px;
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 16px;
  padding: 16px;
}

.account-scope__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.account-scope__header :deep(.ant-input-affix-wrapper) {
  max-width: 280px;
}

.account-scope__hint {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  line-height: 1.6;
}

.account-scope__list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 440px;
  overflow: auto;
}

.account-scope__item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 12px;
  padding: 12px;
  cursor: pointer;
}

.account-scope__meta {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.account-scope__meta span,
.account-scope__meta small {
  color: var(--ant-color-text-secondary);
  word-break: break-all;
}

.account-scope__selected {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.account-scope__empty {
  display: grid;
  min-height: 120px;
  place-items: center;
  color: var(--ant-color-text-secondary);
}

.role-drawer__footer {
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

@media (max-width: 1080px) {
  .platform-page__metrics {
    grid-template-columns: 1fr;
  }

  .account-scope {
    grid-template-columns: 1fr;
  }
}
</style>
