<script lang="ts" setup>
import type { OpenApiApplicationApi } from '#/api/core';

import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Form, Input, Modal, Popconfirm, Space, Switch, Table, Tag, message } from 'ant-design-vue';

import {
  createOpenApiApplicationApi,
  deleteOpenApiApplicationApi,
  getOpenApiApplicationListApi,
  resetOpenApiApplicationSecretApi,
  updateOpenApiApplicationApi,
} from '#/api/core';

defineOptions({ name: 'PlatformOpenApiPage' });

const columns = [
  { dataIndex: 'name', key: 'name', title: '应用名称', width: 180 },
  { dataIndex: 'appId', key: 'appId', title: 'AppId', width: 260 },
  { dataIndex: 'scopes', key: 'scopes', title: '授权范围', width: 220 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 90 },
  { dataIndex: 'remark', key: 'remark', title: '备注' },
  { key: 'actions', title: '操作', width: 260 },
];

const items = ref<OpenApiApplicationApi.ApplicationItem[]>([]);
const loading = ref(false);
const saving = ref(false);
const modalVisible = ref(false);
const editingId = ref<null | string>(null);
const latestSecret = ref('');
const formState = reactive<OpenApiApplicationApi.ApplicationSaveInput>({
  isEnabled: true,
  name: '',
  remark: '',
  scopes: ['dict:read'],
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
  modalVisible.value = true;
}

function openEdit(item: OpenApiApplicationApi.ApplicationItem) {
  editingId.value = item.id;
  latestSecret.value = '';
  formState.isEnabled = item.isEnabled;
  formState.name = item.name;
  formState.remark = item.remark || '';
  formState.scopes = [...item.scopes];
  modalVisible.value = true;
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
      modalVisible.value = false;
    }
    await loadItems();
  } finally {
    saving.value = false;
  }
}

async function handleResetSecret(id: string) {
  const result = await resetOpenApiApplicationSecretApi(id);
  latestSecret.value = result.appSecret;
  message.success('密钥已重置，请立即记录新密钥');
}

async function handleDelete(id: string) {
  await deleteOpenApiApplicationApi(id);
  message.success('开放应用已删除');
  await loadItems();
}

onMounted(loadItems);
</script>

<template>
  <Page description="开放接口使用 AppId + HMAC 签名认证，密钥只在创建或重置时展示一次。" title="开放接口">
    <Card :bordered="false" title="开放应用">
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

      <Table :columns="columns" :data-source="items" :loading="loading" :pagination="{ pageSize: 10 }" row-key="id">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'scopes'">
            <Space wrap>
              <Tag v-for="scope in record.scopes" :key="scope" color="blue">{{ scope }}</Tag>
            </Space>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Button size="small" type="link" @click="openEdit(record as OpenApiApplicationApi.ApplicationItem)">
                编辑
              </Button>
              <Popconfirm title="重置后旧密钥立即失效，确认继续？" @confirm="handleResetSecret(record.id)">
                <Button size="small" type="link">重置密钥</Button>
              </Popconfirm>
              <Popconfirm title="确认删除该开放应用？" @confirm="handleDelete(record.id)">
                <Button danger size="small" type="link">删除</Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="editingId ? '编辑开放应用' : '新增开放应用'"
      @ok="handleSubmit"
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
    </Modal>
  </Page>
</template>

<style scoped>
.open-api-secret {
  margin-bottom: 16px;
}
</style>
