<script lang="ts" setup>
import type { MenuApi, RoleApi } from '#/api/core';

import { computed, onMounted, reactive, ref, watch } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Empty,
  Form,
  Input,
  Modal,
  Skeleton,
  Tag,
  Tree,
  message,
} from 'ant-design-vue';

import {
  createRoleApi,
  deleteRoleApi,
  getMenuPermissionTreeApi,
  getRoleListApi,
  getRoleMenuIdsApi,
  saveRoleMenusApi,
} from '#/api/core';

interface UiTreeNode {
  children?: UiTreeNode[];
  key: string;
  title: string;
}

const checkedMenuKeys = ref<string[]>([]);
const loadingMenus = ref(false);
const loadingRoleMenus = ref(false);
const loadingRoles = ref(true);
const modalVisible = ref(false);
const saving = ref(false);
const roles = ref<RoleApi.RoleItem[]>([]);
const selectedRoleId = ref<string>('');
const treeData = ref<UiTreeNode[]>([]);
const formState = reactive<RoleApi.RoleSaveInput>({ name: '' });

const selectedRole = computed(
  () => roles.value.find((item) => item.id === selectedRoleId.value) ?? null,
);

const roleMetrics = computed(() => [
  { label: '平台角色', value: roles.value.length || 0 },
  { label: '已选角色用户', value: selectedRole.value?.userCount ?? 0 },
  { label: '已选角色菜单', value: selectedRole.value?.menuCount ?? 0 },
]);

function formatMenuTitle(item: MenuApi.PermissionTreeItem) {
  const typeLabel =
    item.type === 1 ? '目录' : item.type === 2 ? '页面' : '按钮';
  const suffix = item.permissionCode ? ` · ${item.permissionCode}` : '';
  return `[${typeLabel}] ${item.title}${suffix}`;
}

function normalizeTree(items: MenuApi.PermissionTreeItem[]): UiTreeNode[] {
  return items.map((item) => ({
    children: normalizeTree(item.children || []),
    key: item.id,
    title: formatMenuTitle(item),
  }));
}

async function loadRoles() {
  loadingRoles.value = true;
  try {
    roles.value = await getRoleListApi();
    const firstRole = roles.value[0];
    if (!selectedRoleId.value && firstRole) {
      selectedRoleId.value = firstRole.id;
    }
  } finally {
    loadingRoles.value = false;
  }
}

async function loadMenuTree() {
  loadingMenus.value = true;
  try {
    const menus = await getMenuPermissionTreeApi('platform,project,business');
    treeData.value = normalizeTree(menus);
  } finally {
    loadingMenus.value = false;
  }
}

async function loadRoleMenus(roleId: string) {
  if (!roleId) {
    checkedMenuKeys.value = [];
    return;
  }
  loadingRoleMenus.value = true;
  try {
    checkedMenuKeys.value = await getRoleMenuIdsApi(roleId);
  } finally {
    loadingRoleMenus.value = false;
  }
}

async function handleSaveMenus() {
  if (!selectedRoleId.value) {
    return;
  }

  saving.value = true;
  try {
    await saveRoleMenusApi(selectedRoleId.value, checkedMenuKeys.value);
    message.success('角色菜单授权已保存');
    await loadRoles();
    await loadRoleMenus(selectedRoleId.value);
  } finally {
    saving.value = false;
  }
}

async function handleCreateRole() {
  if (!formState.name.trim()) {
    message.warning('请输入角色名称');
    return;
  }

  saving.value = true;
  try {
    await createRoleApi({ name: formState.name.trim() });
    message.success('角色已创建');
    modalVisible.value = false;
    formState.name = '';
    await loadRoles();
  } finally {
    saving.value = false;
  }
}

async function handleDeleteRole(id: string) {
  await deleteRoleApi(id);
  message.success('角色已删除');
  selectedRoleId.value = '';
  await loadRoles();
}

watch(
  () => selectedRoleId.value,
  async (roleId) => {
    if (!roleId) {
      return;
    }
    await loadRoleMenus(roleId);
  },
);

onMounted(async () => {
  await Promise.all([loadRoles(), loadMenuTree()]);
});
</script>

<template>
  <Page
    description="角色权限页已支持新增角色、删除角色和菜单授权；用户角色分配统一在用户管理页维护。"
    title="角色权限"
  >
    <section class="platform-page">
      <div class="platform-page__metrics">
        <article
          v-for="metric in roleMetrics"
          :key="metric.label"
          class="platform-page__metric"
        >
          <span>{{ metric.label }}</span>
          <strong>{{ metric.value }}</strong>
        </article>
      </div>

      <div class="platform-page__grid">
        <Card class="platform-page__card" title="角色列表">
          <template #extra>
            <Button type="primary" @click="modalVisible = true">新增角色</Button>
          </template>

          <Skeleton :loading="loadingRoles" active>
            <div v-if="roles.length === 0" class="platform-page__empty">
              <Empty description="未找到角色数据" />
            </div>
            <div v-else class="role-list">
              <button
                v-for="role in roles"
                :key="role.id"
                :class="[
                  'role-list__item',
                  { 'role-list__item--active': role.id === selectedRoleId },
                ]"
                type="button"
                @click="selectedRoleId = role.id"
              >
                <div class="role-list__header">
                  <strong>{{ role.name }}</strong>
                  <Tag color="blue">{{ role.userCount }} 人</Tag>
                </div>
                <p>{{ role.description }}</p>
                <div class="role-list__meta">
                  <span>菜单 {{ role.menuCount }}</span>
                  <Button
                    danger
                    size="small"
                    type="link"
                    @click.stop="handleDeleteRole(role.id)"
                  >
                    删除
                  </Button>
                </div>
              </button>
            </div>
          </Skeleton>
        </Card>

        <Card class="platform-page__card" title="菜单授权">
          <template #extra>
            <Button
              :disabled="!selectedRoleId"
              :loading="saving"
              type="primary"
              @click="handleSaveMenus"
            >
              保存授权
            </Button>
          </template>

          <Skeleton :loading="loadingMenus || loadingRoleMenus" active>
            <Alert
              v-if="selectedRole"
              :message="`${selectedRole.name}：${selectedRole.description}`"
              class="mb-4"
              show-icon
              type="info"
            />

            <div v-if="treeData.length === 0" class="platform-page__empty">
              <Empty description="未找到菜单授权树" />
            </div>
            <Tree
              v-else
              v-model:checkedKeys="checkedMenuKeys"
              block-node
              checkable
              default-expand-all
              :height="620"
              :tree-data="treeData"
            />
          </Skeleton>
        </Card>
      </div>
    </section>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      title="新增角色"
      @ok="handleCreateRole"
    >
      <Form layout="vertical">
        <Form.Item label="角色名称" required>
          <Input v-model:value="formState.name" :maxlength="64" placeholder="例如：qa" />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-page {
  --platform-surface: var(--ant-color-bg-container, hsl(var(--card)));
  --platform-surface-alt: var(--ant-color-bg-elevated, var(--platform-surface));
  --platform-surface-soft: var(--ant-color-fill-quaternary, hsl(var(--accent)));
  --platform-surface-active: var(
    --ant-color-primary-bg,
    var(--platform-surface-soft)
  );
  --platform-border: var(--ant-color-border-secondary, hsl(var(--border)));
  --platform-border-strong: var(--ant-color-primary-border, var(--ant-color-primary));
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
  grid-template-columns: repeat(3, minmax(0, 1fr));
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

.platform-page__grid {
  display: grid;
  gap: 16px;
  grid-template-columns: 360px minmax(0, 1fr);
}

.platform-page__card :deep(.ant-card-head) {
  border-bottom-color: var(--platform-border);
}

.platform-page__card :deep(.ant-card-head),
.platform-page__card :deep(.ant-card-body) {
  background: transparent;
  color: inherit;
}

.platform-page__empty {
  display: grid;
  min-height: 320px;
  place-items: center;
}

.role-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.role-list__item {
  width: 100%;
  padding: 16px;
  border: 1px solid var(--platform-border);
  border-radius: 18px;
  background: linear-gradient(
    180deg,
    var(--platform-surface-alt) 0%,
    var(--platform-surface-soft) 100%
  );
  color: var(--platform-text);
  text-align: left;
}

.role-list__item--active,
.role-list__item:hover {
  border-color: var(--platform-border-strong);
  background: var(--platform-surface-active);
  box-shadow: var(--platform-shadow);
}

.role-list__header,
.role-list__meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.role-list__item p {
  margin: 10px 0 12px;
  color: var(--platform-text-secondary);
  line-height: 1.7;
}

.role-list__meta {
  color: var(--platform-text-secondary);
  font-size: 12px;
}

@media (max-width: 1080px) {
  .platform-page__metrics,
  .platform-page__grid {
    grid-template-columns: 1fr;
  }
}
</style>
