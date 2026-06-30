<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, InputNumber, Popconfirm, Select, Space, Switch, Table, Tag, message } from 'ant-design-vue';

import {
  createAiAssistantApi,
  deleteAiAssistantApi,
  getAiAssistantsApi,
  getAiModelsApi,
  getAiToolsApi,
  updateAiAssistantApi,
} from '#/api/core';

defineOptions({ name: 'AiAssistantsPage' });

const modeOptions = [
  { label: '通用会话', value: 'General' },
  { label: '澄清模式', value: 'Clarify' },
  { label: '规划模式', value: 'Plan' },
  { label: '知识问答', value: 'Knowledge' },
  { label: '执行辅助', value: 'Execution' },
];

const roleOptions = [
  { label: 'PM', value: 'PM' },
  { label: 'PMO', value: 'PMO' },
  { label: '经营', value: 'Business' },
  { label: '通用', value: 'General' },
];

const assistants = ref<AiApi.AssistantItem[]>([]);
const models = ref<AiApi.ModelItem[]>([]);
const tools = ref<AiApi.ToolDefinitionItem[]>([]);
const loading = ref(false);
const drawerVisible = ref(false);
const saving = ref(false);
const editingId = ref('');

const form = reactive<AiApi.AssistantSaveInput>({
  assistantCode: '',
  configJson: '{}',
  defaultMode: 'General',
  defaultModelId: undefined,
  description: '',
  isEnabled: true,
  name: '',
  roleType: 'General',
  sort: 0,
  systemPrompt: '',
  toolCodes: [],
});

const modelOptions = computed(() =>
  models.value
    .filter((item) => item.isEnabled)
    .map((item) => ({ label: `${item.displayName} · ${item.providerName}`, value: item.id })),
);

const toolOptions = computed(() =>
  tools.value.map((item) => ({
    label: `${item.name} · ${item.toolCode}`,
    value: item.toolCode,
  })),
);

const columns = [
  { dataIndex: 'name', key: 'name', title: '助手名称', width: 220 },
  { dataIndex: 'roleType', key: 'roleType', title: '角色', width: 110 },
  { dataIndex: 'defaultMode', key: 'defaultMode', title: '默认模式', width: 120 },
  { dataIndex: 'toolCodes', key: 'toolCodes', title: '默认工具' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 120 },
  { dataIndex: 'sort', key: 'sort', title: '排序', width: 90 },
  { key: 'action', title: '操作', width: 170 },
];

function resetForm() {
  Object.assign(form, {
    assistantCode: '',
    configJson: '{}',
    defaultMode: 'General',
    defaultModelId: undefined,
    description: '',
    isEnabled: true,
    name: '',
    roleType: 'General',
    sort: 0,
    systemPrompt: '',
    toolCodes: [],
  });
}

async function loadData() {
  loading.value = true;
  try {
    const [assistantResult, modelResult, toolResult] = await Promise.all([
      getAiAssistantsApi(true),
      getAiModelsApi(),
      getAiToolsApi(),
    ]);
    assistants.value = assistantResult;
    models.value = modelResult;
    tools.value = toolResult;
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  editingId.value = '';
  resetForm();
  drawerVisible.value = true;
}

function openEdit(record: AiApi.AssistantItem) {
  editingId.value = record.id;
  Object.assign(form, {
    assistantCode: record.assistantCode,
    configJson: record.configJson || '{}',
    defaultMode: record.defaultMode || 'General',
    defaultModelId: record.defaultModelId,
    description: record.description,
    isEnabled: record.isEnabled,
    name: record.name,
    roleType: record.roleType || 'General',
    sort: record.sort,
    systemPrompt: record.systemPrompt,
    toolCodes: [...record.toolCodes],
  });
  drawerVisible.value = true;
}

function openEditFromTable(record: Record<string, any>) {
  openEdit(record as AiApi.AssistantItem);
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

async function handleSubmit() {
  if (!form.assistantCode.trim() || !form.name.trim() || !form.systemPrompt.trim()) {
    message.warning('请填写助手编码、名称和系统提示词');
    return;
  }

  try {
    validateJson(form.configJson, '扩展配置');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...form,
      assistantCode: form.assistantCode.trim(),
      configJson: form.configJson.trim() || '{}',
      description: form.description.trim(),
      name: form.name.trim(),
      systemPrompt: form.systemPrompt.trim(),
    };
    if (editingId.value) {
      await updateAiAssistantApi(editingId.value, payload);
      message.success('助手已保存');
    } else {
      await createAiAssistantApi(payload);
      message.success('助手已创建');
    }

    drawerVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(record: AiApi.AssistantItem) {
  await deleteAiAssistantApi(record.id);
  message.success('助手已删除');
  await loadData();
}

async function handleDeleteFromTable(record: Record<string, any>) {
  await handleDelete(record as AiApi.AssistantItem);
}

onMounted(loadData);
</script>

<template>
  <Page description="维护 PM、PMO、经营等 AI 助手角色，统一配置系统提示词、默认模式和工具范围。" title="助手角色">
    <Card class="rounded-2xl" :bordered="false" title="助手列表">
      <template #extra>
        <Button type="primary" @click="openCreate">新增助手</Button>
      </template>

      <Table
        :columns="columns"
        :data-source="assistants"
        :loading="loading"
        :pagination="{ pageSize: 20, showSizeChanger: true }"
        row-key="id"
        size="middle"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <Space direction="vertical" :size="2">
              <Space>
                <strong>{{ record.name }}</strong>
                <Tag v-if="record.isBuiltin" color="blue">内置</Tag>
              </Space>
              <span class="ai-assistants-page__muted">{{ record.assistantCode }}</span>
              <span class="ai-assistants-page__muted">{{ record.description || '-' }}</span>
            </Space>
          </template>
          <template v-else-if="column.key === 'roleType'">
            <Tag>{{ record.roleType || '-' }}</Tag>
          </template>
          <template v-else-if="column.key === 'defaultMode'">
            {{ modeOptions.find((item) => item.value === record.defaultMode)?.label || record.defaultMode }}
          </template>
          <template v-else-if="column.key === 'toolCodes'">
            <Space wrap>
              <Tag v-for="toolCode in record.toolCodes" :key="toolCode">{{ toolCode }}</Tag>
              <span v-if="record.toolCodes.length === 0" class="ai-assistants-page__muted">未配置</span>
            </Space>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
          </template>
          <template v-else-if="column.key === 'action'">
            <Space>
              <Button type="link" @click="openEditFromTable(record)">编辑</Button>
              <Popconfirm
                v-if="!record.isBuiltin"
                title="确认删除该助手？"
                @confirm="handleDeleteFromTable(record)"
              >
                <Button danger type="link">删除</Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Drawer
      v-model:open="drawerVisible"
      :title="editingId ? '编辑助手' : '新增助手'"
      width="920px"
    >
      <Form :label-col="{ span: 4 }" :model="form" :wrapper-col="{ span: 19 }" layout="horizontal">
        <Form.Item label="助手编码" required>
          <Input v-model:value="form.assistantCode" :disabled="!!editingId" :maxlength="64" placeholder="例如 pm-assistant" />
        </Form.Item>
        <Form.Item label="助手名称" required>
          <Input v-model:value="form.name" :maxlength="128" placeholder="例如 PM 项目助手" />
        </Form.Item>
        <Form.Item label="角色类型">
          <Select v-model:value="form.roleType" :options="roleOptions" />
        </Form.Item>
        <Form.Item label="默认模式">
          <Select v-model:value="form.defaultMode" :options="modeOptions" />
        </Form.Item>
        <Form.Item label="默认模型">
          <Select v-model:value="form.defaultModelId" allow-clear :options="modelOptions" placeholder="不指定则使用会话默认模型" />
        </Form.Item>
        <Form.Item label="默认工具">
          <Select v-model:value="form.toolCodes" mode="multiple" :options="toolOptions" placeholder="选择助手可调用的只读工具" />
        </Form.Item>
        <Form.Item label="说明">
          <Input.TextArea v-model:value="form.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
        <Form.Item label="系统提示词" required>
          <Input.TextArea v-model:value="form.systemPrompt" :auto-size="{ minRows: 8, maxRows: 16 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="扩展配置">
          <Input.TextArea v-model:value="form.configJson" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="状态">
          <Space>
            <Switch v-model:checked="form.isEnabled" />
            <InputNumber v-model:value="form.sort" :min="0" addon-before="排序" />
          </Space>
        </Form.Item>
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
.ai-assistants-page__muted {
  color: var(--ant-color-text-tertiary);
}
</style>
