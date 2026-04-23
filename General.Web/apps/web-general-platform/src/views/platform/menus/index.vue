<script lang="ts" setup>
import type { MenuApi } from '#/api/core';

import { computed, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  InputNumber,
  Select,
  Space,
  Switch,
  Tree,
  message,
} from 'ant-design-vue';

import { createMenuApi, deleteMenuApi, getMenuPermissionTreeApi, setMenuEnabledApi, updateMenuApi } from '#/api/core';

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
const creating = ref(false);

const formState = reactive<any>({
  appCode: 'platform',
  component: '',
  icon: '',
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
    title: `[${item.appCode}] ${item.title}${item.isEnabled ? '' : ' · 已停用'}`,
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

function patchFormBySelection(menu?: MenuApi.PermissionTreeItem | null) {
  if (!menu) {
    formState.appCode = 'platform';
    formState.component = '';
    formState.icon = '';
    formState.isEnabled = true;
    formState.name = '';
    formState.order = 10;
    formState.parentId = undefined;
    formState.path = '';
    formState.permissionCode = '';
    formState.title = '';
    formState.type = 2;
    return;
  }

  formState.appCode = menu.appCode;
  formState.component = menu.component || '';
  formState.icon = menu.icon || '';
  formState.isEnabled = menu.isEnabled;
  formState.name = menu.name;
  formState.order = menu.order;
  formState.parentId = menu.parentId || undefined;
  formState.path = menu.path;
  formState.permissionCode = menu.permissionCode || '';
  formState.title = menu.title;
  formState.type = menu.type;
}

async function loadMenus() {
  loading.value = true;
  try {
    items.value = await getMenuPermissionTreeApi(ALL_APP_CODES);
    if (!selectedId.value && items.value[0]) {
      selectedId.value = items.value[0].id;
    }
    patchFormBySelection(findMenuById(items.value, selectedId.value));
  } finally {
    loading.value = false;
  }
}

function openCreate(parentId?: null | string) {
  creating.value = true;
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
}

function selectMenu(menuId: string) {
  selectedId.value = menuId;
  creating.value = false;
  patchFormBySelection(findMenuById(items.value, menuId));
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

    if (creating.value || !selectedMenu.value) {
      await createMenuApi(payload);
      message.success('菜单已创建');
      creating.value = false;
    } else {
      await updateMenuApi(selectedMenu.value.id, payload);
      message.success('菜单已更新');
    }

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
  creating.value = false;
  await loadMenus();
}

async function handleToggleEnabled(checked: unknown) {
  if (!selectedMenu.value) {
    return;
  }

  const nextEnabled = checked === true;
  await setMenuEnabledApi(selectedMenu.value.id, nextEnabled);
  message.success(`菜单已${nextEnabled ? '启用' : '停用'}`);
  await loadMenus();
}

loadMenus();
</script>

<template>
  <Page description="菜单管理重构为左侧树、右侧详情编辑，启停直接在详情头部完成。" title="菜单管理">
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
          @select="(keys) => { if (keys[0]) selectMenu(`${keys[0]}`); }"
        />
        <Empty v-else description="暂无菜单" />
      </Card>

      <Card :bordered="false" title="详情编辑">
        <template #extra>
          <Space>
            <Switch
              v-if="selectedMenu && !creating"
              :checked="selectedMenu.isEnabled"
              checked-children="启用"
              un-checked-children="停用"
              @change="(checked) => void handleToggleEnabled(checked)"
            />
            <Button v-if="selectedMenu && !creating" danger @click="handleDelete">删除</Button>
            <Button :loading="saving" type="primary" @click="handleSubmit">
              {{ creating ? '创建菜单' : '保存修改' }}
            </Button>
          </Space>
        </template>

        <div v-if="!selectedMenu && !creating" class="platform-menu__empty">
          <Empty description="请选择左侧菜单节点，或直接新增菜单" />
        </div>

        <Form v-else layout="vertical">
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
          </div>
        </Form>
      </Card>
    </div>
  </Page>
</template>

<style scoped>
.platform-menu {
  display: grid;
  gap: 16px;
  grid-template-columns: 360px minmax(0, 1fr);
}

.platform-menu__empty {
  display: grid;
  min-height: 360px;
  place-items: center;
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
