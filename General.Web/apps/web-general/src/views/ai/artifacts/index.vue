<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, InputNumber, Popconfirm, Select, Space, Switch, Table, Tag, message } from 'ant-design-vue';

import {
  createAiArtifactApi,
  deleteAiArtifactApi,
  getAiArtifactsApi,
  updateAiArtifactApi,
} from '#/api/core';

defineOptions({ name: 'AiArtifactsPage' });

const loading = ref(false);
const drawerVisible = ref(false);
const saving = ref(false);
const editingId = ref('');
const artifacts = ref<AiApi.ArtifactItem[]>([]);

const query = reactive<AiApi.ArtifactQuery>({
  includeDisabled: true,
  keyword: '',
});

const form = reactive<AiApi.ArtifactSaveInput>({
  artifactCode: '',
  category: 'Dashboard',
  description: '',
  examplePayloadJson: '{}',
  isEnabled: true,
  jsonSchema: '{}',
  name: '',
  rendererType: 'metric-card',
  sort: 0,
  systemHint: '',
});

const categoryOptions = [
  { label: '驾驶舱', value: 'Dashboard' },
  { label: '图表', value: 'Chart' },
  { label: '表格', value: 'Table' },
  { label: '行动', value: 'Action' },
];

const rendererOptions = [
  { label: '指标卡', value: 'metric-card' },
  { label: '行动队列', value: 'action-list' },
  { label: '风险矩阵', value: 'risk-matrix' },
  { label: '迷你表格', value: 'mini-table' },
];

const columns = [
  { dataIndex: 'name', key: 'name', title: '构件', width: 260 },
  { dataIndex: 'category', key: 'category', title: '分类', width: 110 },
  { dataIndex: 'rendererType', key: 'rendererType', title: '渲染器', width: 150 },
  { dataIndex: 'description', key: 'description', title: '说明' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 170 },
];

function normalizeQuery() {
  return {
    includeDisabled: query.includeDisabled,
    keyword: query.keyword?.trim() || undefined,
  };
}

async function loadData() {
  loading.value = true;
  try {
    artifacts.value = await getAiArtifactsApi(normalizeQuery());
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  Object.assign(form, {
    artifactCode: '',
    category: 'Dashboard',
    description: '',
    examplePayloadJson: '{}',
    isEnabled: true,
    jsonSchema: '{}',
    name: '',
    rendererType: 'metric-card',
    sort: 0,
    systemHint: '',
  });
}

function openCreate() {
  editingId.value = '';
  resetForm();
  drawerVisible.value = true;
}

function openEdit(record: Record<string, any>) {
  const item = record as AiApi.ArtifactItem;
  editingId.value = item.id;
  Object.assign(form, item);
  drawerVisible.value = true;
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

async function handleSubmit() {
  try {
    validateJson(form.jsonSchema, 'JSON Schema');
    validateJson(form.examplePayloadJson, '示例数据');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    if (editingId.value) {
      await updateAiArtifactApi(editingId.value, form);
    } else {
      await createAiArtifactApi(form);
    }

    drawerVisible.value = false;
    message.success('安全构件已保存');
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(record: Record<string, any>) {
  await deleteAiArtifactApi((record as AiApi.ArtifactItem).id);
  message.success('安全构件已删除');
  await loadData();
}

onMounted(loadData);
</script>

<template>
  <Page description="维护模型可输出的白名单 JSON 构件，用于图表、指标卡和行动队列，禁止任意 HTML 直接进入企业页面。" title="安全构件">
    <Card class="rounded-2xl" :bordered="false" title="构件列表">
      <template #extra>
        <Space>
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索编码、名称或说明" @press-enter="loadData" />
          <Switch v-model:checked="query.includeDisabled" checked-children="含停用" un-checked-children="只启用" />
          <Button @click="loadData">查询</Button>
          <Button type="primary" @click="openCreate">新增构件</Button>
        </Space>
      </template>

      <Table :columns="columns" :data-source="artifacts" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <Space direction="vertical" :size="2">
              <Space>
                <strong>{{ record.name }}</strong>
                <Tag v-if="record.isBuiltin" color="blue">内置</Tag>
              </Space>
              <span class="ai-artifacts-page__muted">{{ record.artifactCode }}</span>
            </Space>
          </template>
          <template v-else-if="column.key === 'rendererType'">
            <Tag>{{ record.rendererType }}</Tag>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
          </template>
          <template v-else-if="column.key === 'action'">
            <Space>
              <Button type="link" @click="openEdit(record)">编辑</Button>
              <Popconfirm title="确定删除该构件？" @confirm="handleDelete(record)">
                <Button :disabled="record.isBuiltin" danger type="link">删除</Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Drawer :open="drawerVisible" :title="editingId ? '编辑安全构件' : '新增安全构件'" width="720" @close="drawerVisible = false">
      <Form layout="vertical">
        <Form.Item label="构件编码"><Input v-model:value="form.artifactCode" :disabled="!!editingId" /></Form.Item>
        <Form.Item label="名称"><Input v-model:value="form.name" /></Form.Item>
        <Form.Item label="分类"><Select v-model:value="form.category" :options="categoryOptions" /></Form.Item>
        <Form.Item label="渲染器"><Select v-model:value="form.rendererType" :options="rendererOptions" /></Form.Item>
        <Form.Item label="JSON Schema"><Input.TextArea v-model:value="form.jsonSchema" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="16_000" /></Form.Item>
        <Form.Item label="示例数据"><Input.TextArea v-model:value="form.examplePayloadJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="16_000" /></Form.Item>
        <Form.Item label="模型输出提示"><Input.TextArea v-model:value="form.systemHint" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="4000" /></Form.Item>
        <Form.Item label="说明"><Input.TextArea v-model:value="form.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="排序"><InputNumber v-model:value="form.sort" class="ai-artifacts-page__number" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="form.isEnabled" /></Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="drawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </Space>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
.ai-artifacts-page__muted {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
}

.ai-artifacts-page__number {
  width: 160px;
}
</style>
