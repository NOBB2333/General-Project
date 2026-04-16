<script lang="ts" setup>
import type { MenuApi } from '#/api/core';

import { computed, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Descriptions,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Select,
  Space,
  Switch,
  Tree,
  message,
} from 'ant-design-vue';

import { createMenuApi, deleteMenuApi, getMenuPermissionTreeApi, updateMenuApi } from '#/api/core';

defineOptions({ name: 'PlatformMenuPage' });

const ALL_APP_CODES = 'platform,project,business';
const APP_OPTIONS = [
  { label: '平台', value: 'platform' },
  { label: '项目', value: 'project' },
  { label: '经营', value: 'business' },
];
const TYPE_OPTIONS = [
  { label: '目录', value: 1 },
  { label: '页面', value: 2 },
  { label: '按钮', value: 3 },
];

const loading = ref(false);
const saving = ref(false);
const items = ref<MenuApi.PermissionTreeItem[]>([]);
const selectedId = ref('');
const modalVisible = ref(false);
const editingId = ref<null | string>(null);
const formState = reactive<any>({
  appCode: 'platform',
  component: '/platform/workspace/index',
  icon: 'lucide:layout-dashboard',
  isEnabled: true,
  name: '',
  order: 10,
  parentId: undefined,
  path: '',
  permissionCode: '',
  title: '',
  type: 2,
});

const selectedMenu = computed(() => findMenuById(items.value, selectedId.value));
const treeData = computed(() => normalizeTree(items.value));

function normalizeTree(source: MenuApi.PermissionTreeItem[]): any[] {
  return source.map((item) => ({
    children: normalizeTree(item.children || []),
    key: item.id,
    title: `[${item.appCode}] ${item.title}`,
  }));
}

function findMenuById(source: MenuApi.PermissionTreeItem[], id: string): MenuApi.PermissionTreeItem | null {
  for (const item of source) {
    if (item.id === id) {
      return item;
    }
    const child = findMenuById(item.children || [], id);
    if (child) {
      return child;
    }
  }
  return null;
}

async function loadMenus() {
  loading.value = true;
  try {
    items.value = await getMenuPermissionTreeApi(ALL_APP_CODES);
    if (!selectedId.value && items.value[0]) {
      selectedId.value = items.value[0].id;
    }
  } finally {
    loading.value = false;
  }
}

function openCreate(parentId?: null | string) {
  editingId.value = null;
  formState.appCode = selectedMenu.value?.appCode || 'platform';
  formState.component = '';
  formState.icon = '';
  formState.isEnabled = true;
  formState.name = '';
  formState.order = 10;
  formState.parentId = parentId ?? undefined;
  formState.path = '';
  formState.permissionCode = '';
  formState.title = '';
  formState.type = 2;
  modalVisible.value = true;
}

function openEdit() {
  if (!selectedMenu.value) {
    return;
  }

  editingId.value = selectedMenu.value.id;
  formState.appCode = selectedMenu.value.appCode;
  formState.component = selectedMenu.value.component || '';
  formState.icon = selectedMenu.value.icon || '';
  formState.isEnabled = selectedMenu.value.isEnabled;
  formState.name = selectedMenu.value.name;
  formState.order = selectedMenu.value.order;
  formState.parentId = selectedMenu.value.parentId || undefined;
  formState.path = selectedMenu.value.path;
  formState.permissionCode = selectedMenu.value.permissionCode || '';
  formState.title = selectedMenu.value.title;
  formState.type = selectedMenu.value.type;
  modalVisible.value = true;
}

async function handleSubmit() {
  if (!formState.title.trim() || !formState.name.trim() || !formState.path.trim()) {
    message.warning('请先填写标题、名称和路径');
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...formState,
      component: formState.component || null,
      icon: formState.icon || null,
      permissionCode: formState.permissionCode || null,
    };

    if (editingId.value) {
      await updateMenuApi(editingId.value, payload);
      message.success('菜单已更新');
    } else {
      await createMenuApi(payload);
      message.success('菜单已创建');
    }

    modalVisible.value = false;
    await loadMenus();
  } finally {
    saving.value = false;
  }
}

async function handleDelete() {
  if (!selectedMenu.value) {
    return;
  }

  await deleteMenuApi(selectedMenu.value.id);
  message.success('菜单已删除');
  selectedId.value = '';
  await loadMenus();
}

loadMenus();
</script>

<template>
  <Page description="菜单管理用于维护平台、项目、经营三个 APP 的导航和按钮权限。" title="菜单管理">
    <div class="platform-menu">
      <Card :bordered="false" class="platform-menu__tree" title="菜单树">
        <template #extra>
          <Space>
            <Button type="primary" @click="openCreate()">新增根菜单</Button>
            <Button :disabled="!selectedMenu" @click="openCreate(selectedMenu?.id || undefined)">
              新增子菜单
            </Button>
          </Space>
        </template>

        <Tree
          v-if="treeData.length > 0"
          block-node
          default-expand-all
          :loading="loading"
          :selected-keys="selectedId ? [selectedId] : []"
          :tree-data="treeData"
          @select="(keys) => { selectedId = `${keys[0] ?? ''}`; }"
        />
        <Empty v-else description="暂无菜单" />
      </Card>

      <Card :bordered="false" title="菜单详情">
        <template #extra>
          <Space v-if="selectedMenu">
            <Button @click="openEdit">编辑</Button>
            <Button danger @click="handleDelete">删除</Button>
          </Space>
        </template>

        <Descriptions v-if="selectedMenu" :column="1" bordered size="small">
          <Descriptions.Item label="应用编码">{{ selectedMenu.appCode }}</Descriptions.Item>
          <Descriptions.Item label="菜单标题">{{ selectedMenu.title }}</Descriptions.Item>
          <Descriptions.Item label="名称">{{ selectedMenu.name }}</Descriptions.Item>
          <Descriptions.Item label="路径">{{ selectedMenu.path }}</Descriptions.Item>
          <Descriptions.Item label="组件">{{ selectedMenu.component || '-' }}</Descriptions.Item>
          <Descriptions.Item label="图标">{{ selectedMenu.icon || '-' }}</Descriptions.Item>
          <Descriptions.Item label="按钮码">{{ selectedMenu.permissionCode || '-' }}</Descriptions.Item>
          <Descriptions.Item label="类型">
            {{ selectedMenu.type === 1 ? '目录' : selectedMenu.type === 2 ? '页面' : '按钮' }}
          </Descriptions.Item>
          <Descriptions.Item label="排序">{{ selectedMenu.order }}</Descriptions.Item>
          <Descriptions.Item label="状态">
            {{ selectedMenu.isEnabled ? '启用' : '停用' }}
          </Descriptions.Item>
        </Descriptions>
        <Empty v-else description="请选择左侧菜单节点" />
      </Card>
    </div>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="editingId ? '编辑菜单' : '新增菜单'"
      width="720px"
      @ok="handleSubmit"
    >
      <Form layout="vertical">
        <div class="platform-menu__form-grid">
          <Form.Item label="应用编码" required>
            <Select v-model:value="formState.appCode" :options="APP_OPTIONS" />
          </Form.Item>
          <Form.Item label="菜单类型" required>
            <Select v-model:value="formState.type" :options="TYPE_OPTIONS" />
          </Form.Item>
          <Form.Item label="菜单标题" required>
            <Input v-model:value="formState.title" :maxlength="128" />
          </Form.Item>
          <Form.Item label="菜单名称" required>
            <Input v-model:value="formState.name" :maxlength="64" />
          </Form.Item>
          <Form.Item class="platform-menu__full" label="路径" required>
            <Input v-model:value="formState.path" :maxlength="256" />
          </Form.Item>
          <Form.Item v-if="formState.type !== 3" class="platform-menu__full" label="组件路径">
            <Input
              v-model:value="formState.component"
              :maxlength="256"
              placeholder="/platform/users/index"
            />
          </Form.Item>
          <Form.Item label="图标">
            <Input v-model:value="formState.icon" :maxlength="128" placeholder="lucide:users" />
          </Form.Item>
          <Form.Item label="按钮码">
            <Input
              v-model:value="formState.permissionCode"
              :maxlength="128"
              placeholder="Platform.User.Manage"
            />
          </Form.Item>
          <Form.Item label="排序">
            <InputNumber v-model:value="formState.order" :min="0" style="width: 100%" />
          </Form.Item>
          <Form.Item label="启用状态">
            <Switch v-model:checked="formState.isEnabled" />
          </Form.Item>
        </div>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-menu {
  display: grid;
  gap: 16px;
  grid-template-columns: 360px minmax(0, 1fr);
}

.platform-menu__form-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.platform-menu__full {
  grid-column: 1 / -1;
}

@media (max-width: 1080px) {
  .platform-menu,
  .platform-menu__form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
