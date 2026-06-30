<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Drawer,
  Empty,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Select,
  Space,
  Switch,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createAiModelApi,
  createAiProviderApi,
  deleteAiModelApi,
  deleteAiProviderApi,
  getAiModelsApi,
  getAiProvidersApi,
  syncAiProviderModelsApi,
  testAiProviderApi,
  updateAiModelApi,
  updateAiProviderApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiProvidersPage' });

const providerColumns = [
  { align: 'left', field: 'name', title: '供应商名称', width: 180 },
  { field: 'providerType', title: '类型', width: 150 },
  { align: 'left', field: 'endpoint', title: 'Endpoint', minWidth: 260 },
  { field: 'modelCount', title: '模型数', width: 90 },
  { field: 'hasApiKey', slots: { default: 'hasApiKey' }, title: '密钥', width: 90 },
  { field: 'isEnabled', slots: { default: 'providerStatus' }, title: '状态', width: 120 },
  { fixed: 'right', slots: { default: 'providerActions' }, title: '操作', width: 340 },
] satisfies VxeGridProps<AiApi.ProviderItem>['columns'];

const modelColumns = [
  { align: 'left', field: 'displayName', title: '模型名称', width: 180 },
  { field: 'modelCode', title: '模型编码', width: 180 },
  { field: 'providerName', title: '供应商', width: 160 },
  { field: 'fallbackModelName', title: '降级模型', width: 180 },
  { field: 'capabilities', slots: { default: 'capabilities' }, title: '能力', minWidth: 220 },
  { field: 'contextWindow', title: '上下文', width: 110 },
  { field: 'maxOutputTokens', title: '输出上限', width: 110 },
  { field: 'isEnabled', slots: { default: 'modelStatus' }, title: '状态', width: 120 },
  { fixed: 'right', slots: { default: 'modelActions' }, title: '操作', width: 220 },
] satisfies VxeGridProps<AiApi.ModelItem>['columns'];

const capabilityOptions = [
  { label: '文本生成', value: 'textGeneration' },
  { label: '流式输出', value: 'streaming' },
  { label: '视觉', value: 'vision' },
  { label: '向量', value: 'embedding' },
  { label: '工具调用', value: 'toolCalling' },
];

const providerTypeOptions = [
  { label: 'OpenAI Responses', value: 'OpenAIResponses' },
  { label: 'Anthropic Messages', value: 'AnthropicMessages' },
];

const providers = ref<AiApi.ProviderItem[]>([]);
const models = ref<AiApi.ModelItem[]>([]);
const discoveredModels = ref<AiApi.DiscoveredModelItem[]>([]);
const providerLoading = ref(false);
const modelLoading = ref(false);
const providerDrawerVisible = ref(false);
const modelDrawerVisible = ref(false);
const discoveryDrawerVisible = ref(false);
const editingProviderId = ref<null | string>(null);
const editingModelId = ref<null | string>(null);
const discoveringProvider = ref<AiApi.ProviderItem>();
const savingProvider = ref(false);
const savingModel = ref(false);
const { actionLoadingKey, runAction } = useActionLoading();

const providerForm = reactive<AiApi.ProviderSaveInput>({
  apiKey: '',
  configJson: '{}',
  endpoint: '',
  isDefault: false,
  isEnabled: true,
  name: '',
  organizationId: '',
  providerType: 'OpenAIResponses',
  remark: '',
});

const modelForm = reactive<AiApi.ModelSaveInput>({
  capabilities: ['textGeneration', 'streaming', 'toolCalling'],
  contextWindow: 256_000,
  currency: 'USD',
  displayName: '',
  fallbackModelId: undefined,
  inputTokenPrice: 0,
  isDefault: false,
  isEnabled: true,
  maxOutputTokens: 4096,
  modelCode: '',
  modelType: 'standard',
  outputTokenPrice: 0,
  providerId: '',
  remark: '',
  sort: 0,
});

const providerGridOptions = computed<VxeGridProps<AiApi.ProviderItem>>(() => ({
  columns: providerColumns,
  data: providers.value,
  loading: providerLoading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
}));

const modelGridOptions = computed<VxeGridProps<AiApi.ModelItem>>(() => ({
  columns: modelColumns,
  data: models.value,
  loading: modelLoading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
}));

const providerOptions = computed(() => providers.value.map((item) => ({ label: item.name, value: item.id })));
const fallbackModelOptions = computed(() =>
  models.value
    .filter((item) => item.id !== editingModelId.value && item.isEnabled)
    .map((item) => ({
      label: `${item.displayName} · ${item.providerName}`,
      value: item.id,
    })),
);

const [ProviderGrid] = useVbenVxeGrid<AiApi.ProviderItem>({
  gridOptions: {},
});

const [ModelGrid] = useVbenVxeGrid<AiApi.ModelItem>({
  gridOptions: {},
});

async function loadProviders() {
  providerLoading.value = true;
  try {
    providers.value = await getAiProvidersApi();
  } finally {
    providerLoading.value = false;
  }
}

async function loadModels() {
  modelLoading.value = true;
  try {
    models.value = await getAiModelsApi();
  } finally {
    modelLoading.value = false;
  }
}

async function loadAll() {
  await Promise.all([loadProviders(), loadModels()]);
}

function resetProviderForm() {
  Object.assign(providerForm, {
    apiKey: '',
    configJson: '{}',
    endpoint: '',
    isDefault: providers.value.length === 0,
    isEnabled: true,
    name: '',
    organizationId: '',
    providerType: 'OpenAIResponses',
    remark: '',
  });
}

function openCreateProvider() {
  editingProviderId.value = null;
  resetProviderForm();
  providerDrawerVisible.value = true;
}

function openEditProvider(item: AiApi.ProviderItem) {
  editingProviderId.value = item.id;
  Object.assign(providerForm, {
    apiKey: '',
    configJson: '{}',
    endpoint: item.endpoint,
    isDefault: item.isDefault,
    isEnabled: item.isEnabled,
    name: item.name,
    organizationId: item.organizationId,
    providerType: item.providerType,
    remark: item.remark,
  });
  providerDrawerVisible.value = true;
}

async function handleProviderSubmit() {
  if (!providerForm.name.trim()) {
    message.warning('请输入供应商名称');
    return;
  }

  if (!providerForm.endpoint.trim()) {
    message.warning('请输入 Endpoint');
    return;
  }

  if (!editingProviderId.value && !providerForm.apiKey.trim()) {
    message.warning('新增供应商时必须填写密钥');
    return;
  }

  savingProvider.value = true;
  try {
    const payload = {
      ...providerForm,
      apiKey: providerForm.apiKey.trim(),
      endpoint: providerForm.endpoint.trim(),
      name: providerForm.name.trim(),
      organizationId: providerForm.organizationId.trim(),
      remark: providerForm.remark.trim(),
    };

    if (editingProviderId.value) {
      await updateAiProviderApi(editingProviderId.value, payload);
      message.success('供应商已保存');
    } else {
      await createAiProviderApi(payload);
      message.success('供应商已创建');
    }

    providerDrawerVisible.value = false;
    await loadAll();
  } finally {
    savingProvider.value = false;
  }
}

async function handleTestProvider(id: string) {
  await runAction(`test-provider:${id}`, async () => {
    const result = await testAiProviderApi(id);
    if (result.success) {
      message.success(result.message);
    } else {
      message.warning(result.message);
    }
  });
}

async function handleDiscoverModels(row: AiApi.ProviderItem) {
  await runAction(`discover-provider-models:${row.id}`, async () => {
    const result = await syncAiProviderModelsApi(row.id);
    discoveringProvider.value = row;
    discoveredModels.value = result.models || [];
    discoveryDrawerVisible.value = true;
    if (result.success) {
      message.success(result.message);
      await loadAll();
    } else {
      message.warning(result.message);
    }
  });
}

async function handleDeleteProvider(id: string) {
  await runAction(`delete-provider:${id}`, async () => {
    await deleteAiProviderApi(id);
    message.success('供应商已删除');
    await loadAll();
  });
}

function resetModelForm(providerId?: string) {
  Object.assign(modelForm, {
    capabilities: ['textGeneration', 'streaming', 'toolCalling'],
    contextWindow: 256_000,
    currency: 'USD',
    displayName: '',
    fallbackModelId: undefined,
    inputTokenPrice: 0,
    isDefault: false,
    isEnabled: true,
    maxOutputTokens: 4096,
    modelCode: '',
    modelType: 'standard',
    outputTokenPrice: 0,
    providerId: providerId || providers.value[0]?.id || '',
    remark: '',
    sort: models.value.length * 10,
  });
}

function openCreateModel(providerId?: string) {
  if (providers.value.length === 0) {
    message.warning('请先创建模型供应商');
    return;
  }

  editingModelId.value = null;
  resetModelForm(providerId);
  modelDrawerVisible.value = true;
}

function getCapabilityLabel(value: string) {
  return capabilityOptions.find((item) => item.value === value)?.label || value;
}

function openEditModel(item: AiApi.ModelItem) {
  editingModelId.value = item.id;
  Object.assign(modelForm, {
    capabilities: [...item.capabilities],
    contextWindow: item.contextWindow,
    currency: item.currency,
    displayName: item.displayName,
    fallbackModelId: item.fallbackModelId,
    inputTokenPrice: item.inputTokenPrice,
    isDefault: item.isDefault,
    isEnabled: item.isEnabled,
    maxOutputTokens: item.maxOutputTokens,
    modelCode: item.modelCode,
    modelType: item.modelType,
    outputTokenPrice: item.outputTokenPrice,
    providerId: item.providerId,
    remark: item.remark,
    sort: item.sort,
  });
  modelDrawerVisible.value = true;
}

async function handleModelSubmit() {
  if (!modelForm.providerId) {
    message.warning('请选择供应商');
    return;
  }

  if (!modelForm.modelCode.trim()) {
    message.warning('请输入模型');
    return;
  }

  savingModel.value = true;
  try {
    const payload = {
      ...modelForm,
      currency: modelForm.currency.trim() || 'USD',
      displayName: modelForm.displayName.trim() || modelForm.modelCode.trim(),
      modelCode: modelForm.modelCode.trim(),
      modelType: modelForm.modelType.trim() || 'standard',
      remark: modelForm.remark.trim(),
    };

    if (editingModelId.value) {
      await updateAiModelApi(editingModelId.value, payload);
      message.success('模型已保存');
    } else {
      await createAiModelApi(payload);
      message.success('模型已创建');
    }

    modelDrawerVisible.value = false;
    await loadAll();
  } finally {
    savingModel.value = false;
  }
}

async function handleDeleteModel(id: string) {
  await runAction(`delete-model:${id}`, async () => {
    await deleteAiModelApi(id);
    message.success('模型已删除');
    await loadAll();
  });
}

onMounted(loadAll);
</script>

<template>
  <Page
    description="统一管理 OpenAI Responses 与 Anthropic Messages 两类标准协议；密钥仅加密保存，不在页面回显。"
    title="模型供应商"
  >
    <div class="ai-provider-page">
      <Card :bordered="false" class="rounded-2xl" title="供应商配置">
        <template #extra>
          <Button type="primary" @click="openCreateProvider">新增供应商</Button>
        </template>

        <Alert
          class="ai-provider-page__alert"
          message="供应商协议只保留 OpenAI Responses 与 Anthropic Messages。DeepSeek Anthropic 地址可配置为 https://api.deepseek.com/anthropic。"
          show-icon
          type="info"
        />

        <ProviderGrid :grid-options="providerGridOptions" class="app-vxe-card-grid">
          <template #hasApiKey="{ row }">
            <Tag :color="row.hasApiKey ? 'green' : 'orange'">{{ row.hasApiKey ? '已配置' : '未配置' }}</Tag>
          </template>

          <template #providerStatus="{ row }">
            <Space :size="4">
              <Tag v-if="row.isDefault" color="blue">默认</Tag>
              <Tag :color="row.isEnabled ? 'green' : 'default'">{{ row.isEnabled ? '启用' : '停用' }}</Tag>
            </Space>
          </template>

          <template #providerActions="{ row }">
            <Space>
              <Button
                :loading="actionLoadingKey === `test-provider:${row.id}`"
                size="small"
                type="link"
                @click="handleTestProvider(row.id)"
              >
                测试
              </Button>
              <Button
                :loading="actionLoadingKey === `discover-provider-models:${row.id}`"
                size="small"
                type="link"
                @click="handleDiscoverModels(row)"
              >
                获取模型
              </Button>
              <Button size="small" type="link" @click="openCreateModel(row.id)">新增模型</Button>
              <Button size="small" type="link" @click="openEditProvider(row)">编辑</Button>
              <Popconfirm title="删除供应商会同步删除其模型配置，确认删除？" @confirm="handleDeleteProvider(row.id)">
                <Button :loading="actionLoadingKey === `delete-provider:${row.id}`" danger size="small" type="link">
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </ProviderGrid>
      </Card>

      <Card :bordered="false" class="rounded-2xl" title="模型配置">
        <template #extra>
          <Button @click="openCreateModel()">新增模型</Button>
        </template>

        <ModelGrid :grid-options="modelGridOptions" class="app-vxe-card-grid">
          <template #capabilities="{ row }">
            <Space :size="4" wrap>
              <Tag v-for="item in row.capabilities" :key="item" color="blue">{{ getCapabilityLabel(item) }}</Tag>
            </Space>
          </template>

          <template #modelStatus="{ row }">
            <Space :size="4">
              <Tag v-if="row.isDefault" color="blue">默认</Tag>
              <Tag :color="row.isEnabled ? 'green' : 'default'">{{ row.isEnabled ? '启用' : '停用' }}</Tag>
            </Space>
          </template>

          <template #modelActions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openEditModel(row)">编辑</Button>
              <Popconfirm title="确认删除该模型配置？" @confirm="handleDeleteModel(row.id)">
                <Button :loading="actionLoadingKey === `delete-model:${row.id}`" danger size="small" type="link">
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </ModelGrid>
      </Card>
    </div>

    <Drawer
      v-model:open="providerDrawerVisible"
      :title="editingProviderId ? '编辑供应商' : '新增供应商'"
      width="720px"
    >
      <Form :label-col="{ span: 5 }" :model="providerForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="供应商名称" required>
          <Input v-model:value="providerForm.name" :maxlength="128" placeholder="例如 OpenAI 官方、内网模型网关" />
        </Form.Item>
        <Form.Item label="供应商类型" required>
          <Select v-model:value="providerForm.providerType" :options="providerTypeOptions" />
        </Form.Item>
        <Form.Item label="Endpoint" required>
          <Input v-model:value="providerForm.endpoint" :maxlength="512" placeholder="https://api.openai.com/v1" />
        </Form.Item>
        <Form.Item label="密钥" :required="!editingProviderId">
          <Input.Password
            v-model:value="providerForm.apiKey"
            :maxlength="2048"
            :placeholder="editingProviderId ? '不填写则沿用原密钥' : '请输入供应商 API Key'"
          />
        </Form.Item>
        <Form.Item label="组织标识">
          <Input v-model:value="providerForm.organizationId" :maxlength="128" placeholder="可选" />
        </Form.Item>
        <Form.Item label="默认供应商">
          <Switch v-model:checked="providerForm.isDefault" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="providerForm.isEnabled" />
        </Form.Item>
        <Form.Item label="扩展配置">
          <Input.TextArea v-model:value="providerForm.configJson" :auto-size="{ minRows: 3, maxRows: 6 }" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="providerForm.remark" :auto-size="{ minRows: 2, maxRows: 4 }" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="providerDrawerVisible = false">取消</Button>
          <Button :loading="savingProvider" type="primary" @click="handleProviderSubmit">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="modelDrawerVisible"
      :title="editingModelId ? '编辑模型' : '新增模型'"
      width="760px"
    >
      <Form :label-col="{ span: 5 }" :model="modelForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="供应商" required>
          <Select
            v-model:value="modelForm.providerId"
            :disabled="!!editingModelId"
            :options="providerOptions"
          />
        </Form.Item>
        <Form.Item label="模型" required>
          <Input v-model:value="modelForm.modelCode" :maxlength="128" placeholder="例如 deepseek-v4-pro" />
        </Form.Item>
        <Form.Item label="显示名称">
          <Input v-model:value="modelForm.displayName" :maxlength="128" placeholder="不填则使用模型名" />
        </Form.Item>
        <Form.Item label="降级模型">
          <Select
            v-model:value="modelForm.fallbackModelId"
            allow-clear
            :options="fallbackModelOptions"
            placeholder="主模型失败时自动切换"
          />
        </Form.Item>
        <Form.Item label="能力">
          <Select v-model:value="modelForm.capabilities" :options="capabilityOptions" mode="multiple" />
        </Form.Item>
        <Form.Item label="上下文长度">
          <InputNumber v-model:value="modelForm.contextWindow" :min="0" class="ai-provider-page__number" />
        </Form.Item>
        <Form.Item label="输出上限">
          <InputNumber v-model:value="modelForm.maxOutputTokens" :min="0" class="ai-provider-page__number" />
        </Form.Item>
        <Form.Item label="输入价格">
          <InputNumber v-model:value="modelForm.inputTokenPrice" :min="0" class="ai-provider-page__number" />
        </Form.Item>
        <Form.Item label="输出价格">
          <InputNumber v-model:value="modelForm.outputTokenPrice" :min="0" class="ai-provider-page__number" />
        </Form.Item>
        <Form.Item label="币种">
          <Input v-model:value="modelForm.currency" :maxlength="16" />
        </Form.Item>
        <Form.Item label="排序">
          <InputNumber v-model:value="modelForm.sort" class="ai-provider-page__number" />
        </Form.Item>
        <Form.Item label="默认模型">
          <Switch v-model:checked="modelForm.isDefault" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="modelForm.isEnabled" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="modelForm.remark" :auto-size="{ minRows: 2, maxRows: 4 }" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="modelDrawerVisible = false">取消</Button>
          <Button :loading="savingModel" type="primary" @click="handleModelSubmit">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="discoveryDrawerVisible"
      :title="discoveringProvider ? `可用模型：${discoveringProvider.name}` : '可用模型'"
      width="680px"
    >
      <div class="ai-provider-page__discovery">
        <Empty v-if="discoveredModels.length === 0" description="未发现可用模型" />
        <article v-for="item in discoveredModels" v-else :key="item.modelCode" class="ai-provider-page__discovery-item">
          <span>
            <strong>{{ item.displayName || item.modelCode }}</strong>
            <em>{{ item.modelCode }}</em>
          </span>
          <Tag>{{ item.modelType || 'standard' }}</Tag>
          <Tag color="green">已同步</Tag>
        </article>
      </div>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-provider-page {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-provider-page__alert {
  @apply mb-3;
}

.ai-provider-page__number {
  width: 100%;
}

.ai-provider-page__discovery {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
}

.ai-provider-page__discovery-item {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  @apply gap-3;
  align-items: center;
  @apply p-3;
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-provider-page__discovery-item span {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  min-width: 0;
}

.ai-provider-page__discovery-item strong,
.ai-provider-page__discovery-item em {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-provider-page__discovery-item em {
  color: var(--ant-color-text-tertiary);
  font-style: normal;
}
</style>
