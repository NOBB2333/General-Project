<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { OpenApiApplicationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Drawer, Form, Input, Popconfirm, Space, Switch, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createOpenApiApplicationApi,
  deleteOpenApiApplicationApi,
  getOpenApiApplicationListApi,
  resetOpenApiApplicationSecretApi,
  updateOpenApiApplicationApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformOpenApiPage' });

const columns = [
  { align: 'left', field: 'name', title: '应用名称', width: 180 },
  { field: 'appId', title: 'AppId', width: 260 },
  { field: 'scopes', slots: { default: 'scopes' }, title: '授权范围', width: 220 },
  { field: 'isEnabled', slots: { default: 'isEnabled' }, title: '状态', width: 90 },
  { align: 'left', field: 'remark', title: '备注' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 260 },
] satisfies VxeGridProps<OpenApiApplicationApi.ApplicationItem>['columns'];

const items = ref<OpenApiApplicationApi.ApplicationItem[]>([]);
const loading = ref(false);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const saving = ref(false);
const drawerVisible = ref(false);
const editingId = ref<null | string>(null);
const latestSecret = ref('');
const { actionLoadingKey, runAction } = useActionLoading();
const formState = reactive<OpenApiApplicationApi.ApplicationSaveInput>({
  isEnabled: true,
  name: '',
  remark: '',
  scopes: ['dict:read'],
});

const gridOptions = computed<VxeGridProps<OpenApiApplicationApi.ApplicationItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
}));

const gridEvents: NativeVxeGridEvents<OpenApiApplicationApi.ApplicationItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<OpenApiApplicationApi.ApplicationItem>({
  gridEvents,
  gridOptions: {},
});

async function loadItems() {
  loading.value = true;
  try {
    items.value = await getOpenApiApplicationListApi();
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  editingId.value = null;
  latestSecret.value = '';
  formState.isEnabled = true;
  formState.name = '';
  formState.remark = '';
  formState.scopes = ['dict:read'];
  drawerVisible.value = true;
}

function openEdit(item: OpenApiApplicationApi.ApplicationItem) {
  editingId.value = item.id;
  latestSecret.value = '';
  formState.isEnabled = item.isEnabled;
  formState.name = item.name;
  formState.remark = item.remark || '';
  formState.scopes = [...item.scopes];
  drawerVisible.value = true;
}

function parseScopes(value: string) {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean);
}

async function handleSubmit() {
  if (!formState.name.trim()) {
    message.warning('请输入应用名称');
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...formState,
      name: formState.name.trim(),
      remark: formState.remark.trim(),
      scopes: formState.scopes.length > 0 ? formState.scopes : ['*'],
    };

    if (editingId.value) {
      await updateOpenApiApplicationApi(editingId.value, payload);
      message.success('开放应用已保存');
    } else {
      const created = await createOpenApiApplicationApi(payload);
      latestSecret.value = created.appSecret;
      message.success('开放应用已创建');
    }

    if (editingId.value) {
      drawerVisible.value = false;
    }
    await loadItems();
  } finally {
    saving.value = false;
  }
}

async function handleResetSecret(id: string) {
  await runAction(`reset-secret:${id}`, async () => {
    const result = await resetOpenApiApplicationSecretApi(id);
    latestSecret.value = result.appSecret;
    message.success('密钥已重置，请立即记录新密钥');
  });
}

async function handleDelete(id: string) {
  await runAction(`delete:${id}`, async () => {
    await deleteOpenApiApplicationApi(id);
    message.success('开放应用已删除');
    await loadItems();
  });
}

onMounted(loadItems);
</script>

<template>
  <Page description="开放接口使用 AppId + HMAC 签名认证，密钥只在创建或重置时展示一次。" title="开放接口">
    <Card class="rounded-2xl" :bordered="false" title="开放应用">
      <template #extra>
        <Button type="primary" @click="openCreate">新增应用</Button>
      </template>

      <Alert
        v-if="latestSecret"
        class="open-api-secret"
        :message="`新密钥：${latestSecret}`"
        show-icon
        type="warning"
      />

      <Grid :grid-events="gridEvents" :grid-options="gridOptions">
        <template #scopes="{ row }">
          <Space wrap>
            <Tag v-for="scope in row.scopes" :key="scope" color="blue">{{ scope }}</Tag>
          </Space>
        </template>
        <template #isEnabled="{ row }">
          <Tag :color="row.isEnabled ? 'green' : 'default'">{{ row.isEnabled ? '启用' : '停用' }}</Tag>
        </template>
        <template #actions="{ row }">
          <Space>
            <Button size="small" type="link" @click="openEdit(row)">
              编辑
            </Button>
            <Popconfirm title="重置后旧密钥立即失效，确认继续？" @confirm="handleResetSecret(row.id)">
              <Button
                :loading="actionLoadingKey === `reset-secret:${row.id}`"
                size="small"
                type="link"
              >
                重置密钥
              </Button>
            </Popconfirm>
            <Popconfirm title="确认删除该开放应用？" @confirm="handleDelete(row.id)">
              <Button
                danger
                :loading="actionLoadingKey === `delete:${row.id}`"
                size="small"
                type="link"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </Grid>
    </Card>

    <Drawer
      v-model:open="drawerVisible"
      :title="editingId ? '编辑开放应用' : '新增开放应用'"
      destroy-on-close
      width="560"
    >
      <Form layout="vertical">
        <Form.Item label="应用名称" required>
          <Input v-model:value="formState.name" :maxlength="128" />
        </Form.Item>
        <Form.Item label="授权范围">
          <Input
            :value="formState.scopes.join(',')"
            placeholder="dict:read 或 *"
            @update:value="(value) => { formState.scopes = parseScopes(value); }"
          />
        </Form.Item>
        <Form.Item label="启用状态">
          <Switch v-model:checked="formState.isEnabled" checked-children="启用" un-checked-children="停用" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="formState.remark" :maxlength="256" :rows="3" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="drawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </div>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.open-api-secret {
  @apply mb-4;
}
</style>
