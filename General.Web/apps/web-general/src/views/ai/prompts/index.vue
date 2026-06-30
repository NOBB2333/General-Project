<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Checkbox, Drawer, Form, Input, InputNumber, Popconfirm, Select, Space, Table, Tag, message } from 'ant-design-vue';

import {
  createAiConversationFromPromptTemplateApi,
  createAiPromptTemplateApi,
  dryRunAiPromptTemplateApi,
  dryRunAiPromptTemplatesApi,
  deleteAiPromptTemplateApi,
  getAiAssistantsApi,
  getAiModelsApi,
  getAiPromptTemplatesApi,
  getAiToolsApi,
  runAiScenarioAcceptanceApi,
  updateAiPromptTemplateApi,
} from '#/api/core';

defineOptions({ name: 'AiPromptsPage' });

const modeOptions = [
  { label: '通用会话', value: 'General' },
  { label: '澄清模式', value: 'Clarify' },
  { label: '规划模式', value: 'Plan' },
  { label: '知识问答', value: 'Knowledge' },
  { label: '执行辅助', value: 'Execution' },
];

const categoryOptions = [
  { label: '项目管理', value: '项目管理' },
  { label: '项目治理', value: '项目治理' },
  { label: '经营管理', value: '经营管理' },
  { label: '项目收尾', value: '项目收尾' },
];

const scenarioOptions = [
  { label: '周报', value: 'WeeklyReport' },
  { label: '会议纪要', value: 'MeetingMinutes' },
  { label: '风险分析', value: 'RiskAnalysis' },
  { label: '根因分析', value: 'IssueRootCause' },
  { label: '变更影响', value: 'ChangeImpact' },
  { label: '经营风险', value: 'BusinessRisk' },
  { label: '回款催收', value: 'ReceivableCollection' },
  { label: '项目复盘', value: 'Retrospective' },
  { label: '通用', value: 'General' },
];

const contextOptions = [
  { label: '无上下文', value: 'None' },
  { label: '项目上下文', value: 'Project' },
  { label: '经营上下文', value: 'Business' },
  { label: '风险上下文', value: 'Risk' },
  { label: '文档上下文', value: 'Document' },
];

const router = useRouter();
const templates = ref<AiApi.PromptTemplateItem[]>([]);
const assistants = ref<AiApi.AssistantItem[]>([]);
const models = ref<AiApi.ModelItem[]>([]);
const tools = ref<AiApi.ToolDefinitionItem[]>([]);
const loading = ref(false);
const drawerVisible = ref(false);
const saving = ref(false);
const editingId = ref('');
const launchDrawerVisible = ref(false);
const launching = ref(false);
const launchingTemplate = ref<AiApi.PromptTemplateItem>();
const dryRunDrawerVisible = ref(false);
const dryRunning = ref(false);
const dryRunTemplate = ref<AiApi.PromptTemplateItem>();
const dryRunResult = ref<AiApi.PromptTemplateDryRunResult>();
const batchDryRunning = ref(false);
const batchDryRunResult = ref<AiApi.PromptTemplateDryRunBatchResult>();
const acceptanceRunning = ref(false);
const acceptanceDrawerVisible = ref(false);
const acceptanceResult = ref<AiApi.ScenarioAcceptanceResult>();
const acceptanceTemplateOptions = [
  { label: '澄清模式', value: 'clarify-mode' },
  { label: '规划模式', value: 'plan-mode' },
  { label: '项目周报', value: 'project-weekly-report' },
  { label: '会议纪要', value: 'meeting-minutes-cleanup' },
  { label: '风险分析', value: 'risk-analysis' },
  { label: '经营解释', value: 'business-risk-explanation' },
];

const query = reactive<AiApi.PromptTemplateQuery>({
  assistantCode: undefined,
  category: undefined,
  includeDisabled: true,
  keyword: '',
  scenario: undefined,
});

const form = reactive<AiApi.PromptTemplateSaveInput>({
  assistantCode: '',
  category: '项目管理',
  description: '',
  inputSchemaJson: '{}',
  isEnabled: true,
  mode: 'General',
  name: '',
  outputSchemaJson: '{}',
  promptContent: '',
  scenario: 'General',
  sort: 0,
  templateCode: '',
  toolCodes: [],
  version: 1,
});

const launchForm = reactive<AiApi.PromptTemplateConversationCreateInput>({
  contextId: '',
  contextSnapshotJson: '{}',
  contextType: 'Project',
  inputJson: '{}',
  modelId: undefined,
  title: '',
});

const dryRunForm = reactive<AiApi.PromptTemplateDryRunInput>({
  contextId: '',
  contextSnapshotJson: '{}',
  contextType: 'Project',
  inputJson: '{}',
});

const acceptanceForm = reactive<AiApi.ScenarioAcceptanceInput>({
  contextId: '',
  contextSnapshotJson: '{}',
  contextType: 'Project',
  inputJson: '{}',
  modelId: undefined,
  templateCodes: acceptanceTemplateOptions.map((item) => item.value),
  title: '五期 AI 场景验收',
});

const assistantOptions = computed(() =>
  assistants.value
    .filter((item) => item.isEnabled)
    .map((item) => ({ label: `${item.name} · ${item.assistantCode}`, value: item.assistantCode })),
);

const toolOptions = computed(() =>
  tools.value.map((item) => ({
    label: `${item.name} · ${item.toolCode}`,
    value: item.toolCode,
  })),
);

const modelOptions = computed(() =>
  models.value
    .filter((item) => item.isEnabled)
    .map((item) => ({ label: `${item.displayName} · ${item.providerName}`, value: item.id })),
);

const columns = [
  { dataIndex: 'name', key: 'name', title: '模板名称', width: 260 },
  { dataIndex: 'category', key: 'category', title: '分类', width: 120 },
  { dataIndex: 'scenario', key: 'scenario', title: '场景', width: 150 },
  { dataIndex: 'assistantName', key: 'assistantName', title: '助手', width: 180 },
  { dataIndex: 'toolCodes', key: 'toolCodes', title: '关联工具' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 110 },
  { key: 'action', title: '操作', width: 170 },
];

function resetForm() {
  Object.assign(form, {
    assistantCode: '',
    category: '项目管理',
    description: '',
    inputSchemaJson: '{}',
    isEnabled: true,
    mode: 'General',
    name: '',
    outputSchemaJson: '{}',
    promptContent: '',
    scenario: 'General',
    sort: 0,
    templateCode: '',
    toolCodes: [],
    version: 1,
  });
}

function normalizeQuery() {
  return {
    assistantCode: query.assistantCode || undefined,
    category: query.category || undefined,
    includeDisabled: query.includeDisabled,
    keyword: query.keyword?.trim() || undefined,
    scenario: query.scenario || undefined,
  };
}

async function loadData() {
  loading.value = true;
  try {
    const [templateResult, assistantResult, toolResult, modelResult] = await Promise.all([
      getAiPromptTemplatesApi(normalizeQuery()),
      getAiAssistantsApi(true),
      getAiToolsApi(),
      getAiModelsApi(),
    ]);
    templates.value = templateResult;
    assistants.value = assistantResult;
    tools.value = toolResult;
    models.value = modelResult;
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  editingId.value = '';
  resetForm();
  drawerVisible.value = true;
}

function openEdit(record: AiApi.PromptTemplateItem) {
  editingId.value = record.id;
  Object.assign(form, {
    assistantCode: record.assistantCode,
    category: record.category || '项目管理',
    description: record.description,
    inputSchemaJson: record.inputSchemaJson || '{}',
    isEnabled: record.isEnabled,
    mode: record.mode || 'General',
    name: record.name,
    outputSchemaJson: record.outputSchemaJson || '{}',
    promptContent: record.promptContent,
    scenario: record.scenario || 'General',
    sort: record.sort,
    templateCode: record.templateCode,
    toolCodes: [...record.toolCodes],
    version: record.version,
  });
  drawerVisible.value = true;
}

function openEditFromTable(record: Record<string, any>) {
  openEdit(record as AiApi.PromptTemplateItem);
}

function openLaunch(record: AiApi.PromptTemplateItem) {
  launchingTemplate.value = record;
  Object.assign(launchForm, {
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: record.category === '经营管理' ? 'Business' : 'Project',
    inputJson: '{}',
    modelId: undefined,
    title: `${record.name} · ${new Date().toLocaleString()}`,
  });
  launchDrawerVisible.value = true;
}

function openLaunchFromTable(record: Record<string, any>) {
  openLaunch(record as AiApi.PromptTemplateItem);
}

function openDryRun(record: AiApi.PromptTemplateItem) {
  dryRunTemplate.value = record;
  dryRunResult.value = undefined;
  Object.assign(dryRunForm, {
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: record.category === '经营管理' ? 'Business' : 'Project',
    inputJson: '{}',
  });
  dryRunDrawerVisible.value = true;
}

function openDryRunFromTable(record: Record<string, any>) {
  openDryRun(record as AiApi.PromptTemplateItem);
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

function normalizeJson(value: string, label: string) {
  validateJson(value, label);
  return value.trim() || '{}';
}

async function handleSubmit() {
  if (!form.templateCode.trim() || !form.name.trim() || !form.promptContent.trim()) {
    message.warning('请填写模板编码、名称和提示词内容');
    return;
  }

  try {
    validateJson(form.inputSchemaJson, '输入 Schema');
    validateJson(form.outputSchemaJson, '输出 Schema');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...form,
      assistantCode: form.assistantCode.trim(),
      category: form.category.trim(),
      description: form.description.trim(),
      inputSchemaJson: form.inputSchemaJson.trim() || '{}',
      name: form.name.trim(),
      outputSchemaJson: form.outputSchemaJson.trim() || '{}',
      promptContent: form.promptContent.trim(),
      scenario: form.scenario.trim(),
      templateCode: form.templateCode.trim(),
    };
    if (editingId.value) {
      await updateAiPromptTemplateApi(editingId.value, payload);
      message.success('模板已保存');
    } else {
      await createAiPromptTemplateApi(payload);
      message.success('模板已创建');
    }

    drawerVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(record: AiApi.PromptTemplateItem) {
  await deleteAiPromptTemplateApi(record.id);
  message.success('模板已删除');
  await loadData();
}

async function handleDeleteFromTable(record: Record<string, any>) {
  await handleDelete(record as AiApi.PromptTemplateItem);
}

async function handleLaunchConversation() {
  if (!launchingTemplate.value) {
    return;
  }

  try {
    validateJson(launchForm.contextSnapshotJson, '上下文快照');
    validateJson(launchForm.inputJson, '模板输入');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  launching.value = true;
  try {
    await createAiConversationFromPromptTemplateApi(launchingTemplate.value.id, {
      ...launchForm,
      contextId: launchForm.contextId.trim(),
      contextSnapshotJson: launchForm.contextSnapshotJson.trim() || '{}',
      inputJson: launchForm.inputJson.trim() || '{}',
      title: launchForm.title.trim(),
    });
    message.success('会话已创建');
    launchDrawerVisible.value = false;
    await router.push('/ai/conversations');
  } finally {
    launching.value = false;
  }
}

async function handleDryRun() {
  if (!dryRunTemplate.value) {
    return;
  }

  try {
    validateJson(dryRunForm.contextSnapshotJson, '上下文快照');
    validateJson(dryRunForm.inputJson, '模板输入');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  dryRunning.value = true;
  try {
    dryRunResult.value = await dryRunAiPromptTemplateApi(dryRunTemplate.value.id, {
      ...dryRunForm,
      contextId: dryRunForm.contextId.trim(),
      contextSnapshotJson: dryRunForm.contextSnapshotJson.trim() || '{}',
      inputJson: dryRunForm.inputJson.trim() || '{}',
    });
    message.success(dryRunResult.value.success ? '自检完成' : '自检完成，但存在问题');
  } finally {
    dryRunning.value = false;
  }
}

async function handleBatchDryRun() {
  batchDryRunning.value = true;
  try {
    batchDryRunResult.value = await dryRunAiPromptTemplatesApi({
      contextId: '',
      contextSnapshotJson: '{}',
      contextType: 'Project',
      includeDisabled: true,
      inputJson: '{}',
      onlyBuiltin: true,
      templateCodes: [],
    });
    if (batchDryRunResult.value.success) {
      message.success(`内置模板自检通过：${batchDryRunResult.value.passedCount} / ${batchDryRunResult.value.totalCount}`);
    } else {
      message.warning(`内置模板自检未通过：失败 ${batchDryRunResult.value.failedCount} 项`);
    }
  } finally {
    batchDryRunning.value = false;
  }
}

async function handleScenarioAcceptance() {
  let contextSnapshotJson = '{}';
  let inputJson = '{}';
  try {
    contextSnapshotJson = normalizeJson(acceptanceForm.contextSnapshotJson, '上下文快照');
    inputJson = normalizeJson(acceptanceForm.inputJson, '验收输入');
  } catch (error) {
    message.warning((error as Error).message || '验收输入必须是合法 JSON');
    return;
  }

  acceptanceRunning.value = true;
  try {
    acceptanceResult.value = await runAiScenarioAcceptanceApi({
      ...acceptanceForm,
      contextId: acceptanceForm.contextId.trim(),
      contextSnapshotJson,
      inputJson,
      templateCodes: acceptanceForm.templateCodes,
      title: acceptanceForm.title.trim() || '五期 AI 场景验收',
    });
    acceptanceDrawerVisible.value = false;
    if (acceptanceResult.value.success) {
      message.success(`真实场景验收通过：${acceptanceResult.value.passedCount} / ${acceptanceResult.value.totalCount}`);
    } else {
      message.warning(`真实场景验收未通过：失败 ${acceptanceResult.value.failedCount} 项`);
    }
  } finally {
    acceptanceRunning.value = false;
  }
}

onMounted(loadData);
</script>

<template>
  <Page description="维护项目周报、会议纪要、风险分析、经营解释等 AI 管理场景模板。" title="场景模板">
    <Card :bordered="false" class="rounded-2xl" title="模板筛选">
      <div class="ai-prompts-page__filters">
        <Input.Search v-model:value="query.keyword" allow-clear placeholder="搜索编码、名称或说明" @search="loadData" />
        <Select v-model:value="query.category" allow-clear :options="categoryOptions" placeholder="分类" @change="loadData" />
        <Select v-model:value="query.scenario" allow-clear :options="scenarioOptions" placeholder="场景" @change="loadData" />
        <Select v-model:value="query.assistantCode" allow-clear :options="assistantOptions" placeholder="助手" @change="loadData" />
        <Checkbox v-model:checked="query.includeDisabled" @change="loadData">包含停用</Checkbox>
        <Space>
          <Button :loading="batchDryRunning" @click="handleBatchDryRun">全量自检</Button>
          <Button :loading="acceptanceRunning" @click="acceptanceDrawerVisible = true">真实验收</Button>
          <Button type="primary" @click="openCreate">新增模板</Button>
        </Space>
      </div>
      <div v-if="batchDryRunResult" class="ai-prompts-page__batch-result">
        <Tag :color="batchDryRunResult.success ? 'green' : 'orange'">
          {{ batchDryRunResult.success ? '内置模板自检通过' : '内置模板自检未通过' }}
        </Tag>
        <span>通过 {{ batchDryRunResult.passedCount }} 项，失败 {{ batchDryRunResult.failedCount }} 项，总计 {{ batchDryRunResult.totalCount }} 项</span>
        <span v-if="batchDryRunResult.missingRequiredTemplateCodes.length > 0">
          缺失：{{ batchDryRunResult.missingRequiredTemplateCodes.join('，') }}
        </span>
      </div>
      <div v-if="acceptanceResult" class="ai-prompts-page__acceptance">
        <div class="ai-prompts-page__batch-result">
          <Tag :color="acceptanceResult.success ? 'green' : 'orange'">
            {{ acceptanceResult.success ? '真实场景验收通过' : '真实场景验收未通过' }}
          </Tag>
          <span>通过 {{ acceptanceResult.passedCount }} 项，失败 {{ acceptanceResult.failedCount }} 项，总计 {{ acceptanceResult.totalCount }} 项</span>
          <span v-if="acceptanceResult.missingRequiredTemplateCodes.length > 0">
            缺失：{{ acceptanceResult.missingRequiredTemplateCodes.join('，') }}
          </span>
        </div>
        <div class="ai-prompts-page__acceptance-list">
          <div v-for="item in acceptanceResult.items" :key="item.acceptanceCode" class="ai-prompts-page__acceptance-item">
            <div class="ai-prompts-page__acceptance-title">
              <Space>
                <Tag :color="item.success ? 'green' : 'red'">{{ item.success ? '通过' : '失败' }}</Tag>
                <strong>{{ item.acceptanceName }}</strong>
                <span class="ai-prompts-page__muted">{{ item.templateCode }} · {{ item.mode }}</span>
              </Space>
              <Button v-if="item.conversationId" type="link" @click="router.push(`/ai/conversations?id=${item.conversationId}`)">查看会话</Button>
            </div>
            <div v-if="item.errorMessage" class="ai-prompts-page__acceptance-error">{{ item.errorMessage }}</div>
            <div v-if="item.validationMessages.length > 0" class="ai-prompts-page__acceptance-checks">
              <Tag v-for="validationMessage in item.validationMessages" :key="validationMessage" color="orange">
                {{ validationMessage }}
              </Tag>
            </div>
            <div v-else class="ai-prompts-page__muted">{{ item.assistantMessagePreview || '模型已返回，但没有可展示预览。' }}</div>
          </div>
        </div>
      </div>
    </Card>

    <Card :bordered="false" class="rounded-2xl" title="模板库">
      <Table
        :columns="columns"
        :data-source="templates"
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
                <Tag>v{{ record.version }}</Tag>
              </Space>
              <span class="ai-prompts-page__muted">{{ record.templateCode }}</span>
              <span class="ai-prompts-page__muted">{{ record.description || '-' }}</span>
            </Space>
          </template>
          <template v-else-if="column.key === 'category'">
            <Tag>{{ record.category }}</Tag>
          </template>
          <template v-else-if="column.key === 'scenario'">
            {{ scenarioOptions.find((item) => item.value === record.scenario)?.label || record.scenario }}
          </template>
          <template v-else-if="column.key === 'assistantName'">
            {{ record.assistantName || record.assistantCode || '-' }}
          </template>
          <template v-else-if="column.key === 'toolCodes'">
            <Space wrap>
              <Tag v-for="toolCode in record.toolCodes" :key="toolCode">{{ toolCode }}</Tag>
              <span v-if="record.toolCodes.length === 0" class="ai-prompts-page__muted">未配置</span>
            </Space>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
          </template>
          <template v-else-if="column.key === 'action'">
            <Space>
              <Button type="link" @click="openLaunchFromTable(record)">发起会话</Button>
              <Button type="link" @click="openDryRunFromTable(record)">自检预览</Button>
              <Button type="link" @click="openEditFromTable(record)">编辑</Button>
              <Popconfirm
                v-if="!record.isBuiltin"
                title="确认删除该模板？"
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
      :title="editingId ? '编辑模板' : '新增模板'"
      width="980px"
    >
      <Form :label-col="{ span: 4 }" :model="form" :wrapper-col="{ span: 19 }" layout="horizontal">
        <Form.Item label="模板编码" required>
          <Input v-model:value="form.templateCode" :disabled="!!editingId" :maxlength="128" placeholder="例如 project-weekly-report" />
        </Form.Item>
        <Form.Item label="模板名称" required>
          <Input v-model:value="form.name" :maxlength="128" />
        </Form.Item>
        <Form.Item label="分类 / 场景">
          <div class="ai-prompts-page__compact">
            <Select v-model:value="form.category" :options="categoryOptions" />
            <Select v-model:value="form.scenario" :options="scenarioOptions" />
          </div>
        </Form.Item>
        <Form.Item label="助手 / 模式">
          <div class="ai-prompts-page__compact">
            <Select v-model:value="form.assistantCode" allow-clear :options="assistantOptions" placeholder="选择助手" />
            <Select v-model:value="form.mode" :options="modeOptions" />
          </div>
        </Form.Item>
        <Form.Item label="关联工具">
          <Select v-model:value="form.toolCodes" mode="multiple" :options="toolOptions" placeholder="选择模板推荐调用的只读工具" />
        </Form.Item>
        <Form.Item label="说明">
          <Input.TextArea v-model:value="form.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
        <Form.Item label="提示词内容" required>
          <Input.TextArea v-model:value="form.promptContent" :auto-size="{ minRows: 8, maxRows: 16 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="输入 Schema">
          <Input.TextArea v-model:value="form.inputSchemaJson" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="输出 Schema">
          <Input.TextArea v-model:value="form.outputSchemaJson" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="状态">
          <Space>
            <Checkbox v-model:checked="form.isEnabled">启用</Checkbox>
            <InputNumber v-model:value="form.version" :min="1" addon-before="版本" />
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

    <Drawer
      v-model:open="launchDrawerVisible"
      title="按模板发起会话"
      width="820px"
    >
      <Form :label-col="{ span: 5 }" :model="launchForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="模板">
          <Input :value="launchingTemplate ? `${launchingTemplate.name} · ${launchingTemplate.templateCode}` : '-'" disabled />
        </Form.Item>
        <Form.Item label="会话标题">
          <Input v-model:value="launchForm.title" :maxlength="128" />
        </Form.Item>
        <Form.Item label="上下文类型">
          <Select v-model:value="launchForm.contextType" :options="contextOptions" />
        </Form.Item>
        <Form.Item label="上下文编号">
          <Input v-model:value="launchForm.contextId" :maxlength="128" placeholder="项目 ID，经营模板也使用项目 ID 作为上下文编号" />
        </Form.Item>
        <Form.Item label="模型">
          <Select v-model:value="launchForm.modelId" allow-clear :options="modelOptions" placeholder="不指定则使用助手默认模型或会话默认模型" />
        </Form.Item>
        <Form.Item label="上下文快照">
          <Input.TextArea v-model:value="launchForm.contextSnapshotJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="模板输入">
          <Input.TextArea v-model:value="launchForm.inputJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="8000" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="launchDrawerVisible = false">取消</Button>
          <Button :loading="launching" type="primary" @click="handleLaunchConversation">发起会话</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="dryRunDrawerVisible"
      title="模板自检预览"
      width="980px"
    >
      <Form :label-col="{ span: 5 }" :model="dryRunForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="模板">
          <Input :value="dryRunTemplate ? `${dryRunTemplate.name} · ${dryRunTemplate.templateCode}` : '-'" disabled />
        </Form.Item>
        <Form.Item label="上下文类型">
          <Select v-model:value="dryRunForm.contextType" :options="contextOptions" />
        </Form.Item>
        <Form.Item label="上下文编号">
          <Input v-model:value="dryRunForm.contextId" :maxlength="128" />
        </Form.Item>
        <Form.Item label="上下文快照">
          <Input.TextArea v-model:value="dryRunForm.contextSnapshotJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="模板输入">
          <Input.TextArea v-model:value="dryRunForm.inputJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="8000" />
        </Form.Item>
      </Form>

      <div v-if="dryRunResult" class="ai-prompts-page__dry-run">
        <Card :bordered="false" class="rounded-2xl" size="small" title="自检结果">
          <Space direction="vertical" :size="8" style="width: 100%;">
            <div><strong>状态：</strong>{{ dryRunResult.success ? '通过' : '未通过' }}</div>
            <div><strong>助手：</strong>{{ dryRunResult.assistantName || dryRunResult.assistantCode || '-' }}</div>
            <div><strong>估算 Token：</strong>{{ dryRunResult.estimatedPromptTokens }} / {{ dryRunResult.estimatedCompletionTokens }}</div>
            <div><strong>推荐工具：</strong>{{ dryRunResult.recommendedToolCodes.join('，') || '无' }}</div>
            <div><strong>缺失工具：</strong>{{ dryRunResult.missingToolCodes.join('，') || '无' }}</div>
            <div><strong>说明：</strong>{{ dryRunResult.warnings.join('；') || '无' }}</div>
            <div v-if="dryRunResult.errors.length > 0"><strong>错误：</strong>{{ dryRunResult.errors.join('；') }}</div>
          </Space>
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small" title="渲染后的提示词">
          <Input.TextArea :value="dryRunResult.renderedPrompt" :auto-size="{ minRows: 8, maxRows: 14 }" readonly />
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small" title="结构化输出">
          <Input.TextArea :value="dryRunResult.structuredOutputJson" :auto-size="{ minRows: 8, maxRows: 14 }" readonly />
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small" title="展示内容">
          <Input.TextArea :value="dryRunResult.displayContent" :auto-size="{ minRows: 6, maxRows: 12 }" readonly />
        </Card>
      </div>
      <template #footer>
        <Space>
          <Button @click="dryRunDrawerVisible = false">关闭</Button>
          <Button :loading="dryRunning" type="primary" @click="handleDryRun">执行自检</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="acceptanceDrawerVisible"
      title="真实场景验收"
      width="860px"
    >
      <Form :label-col="{ span: 5 }" :model="acceptanceForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="验收标题">
          <Input v-model:value="acceptanceForm.title" :maxlength="128" />
        </Form.Item>
        <Form.Item label="验收项">
          <Select
            v-model:value="acceptanceForm.templateCodes"
            mode="multiple"
            :options="acceptanceTemplateOptions"
            placeholder="选择要执行的验收项"
          />
        </Form.Item>
        <Form.Item label="上下文类型">
          <Select v-model:value="acceptanceForm.contextType" :options="contextOptions" />
        </Form.Item>
        <Form.Item label="上下文编号">
          <Input v-model:value="acceptanceForm.contextId" :maxlength="128" placeholder="建议填写真实项目 ID，用于触发项目/经营工具调用" />
        </Form.Item>
        <Form.Item label="模型">
          <Select v-model:value="acceptanceForm.modelId" allow-clear :options="modelOptions" placeholder="不指定则使用默认模型" />
        </Form.Item>
        <Form.Item label="上下文快照">
          <Input.TextArea v-model:value="acceptanceForm.contextSnapshotJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="验收输入">
          <Input.TextArea v-model:value="acceptanceForm.inputJson" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="8000" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="acceptanceDrawerVisible = false">取消</Button>
          <Button :loading="acceptanceRunning" type="primary" @click="handleScenarioAcceptance">开始验收</Button>
        </Space>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-prompts-page__filters {
  display: grid;
  grid-template-columns: minmax(180px, 1.2fr) 160px 160px minmax(180px, 1fr) 110px max-content;
  @apply gap-2.5;
  align-items: center;
}

.ai-prompts-page__batch-result {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
  align-items: center;
  @apply mt-3;
  color: var(--ant-color-text-secondary);
}

.ai-prompts-page__muted {
  color: var(--ant-color-text-tertiary);
}

.ai-prompts-page__compact {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  @apply gap-2;
}

.ai-prompts-page__dry-run {
  display: grid;
  @apply gap-3;
  @apply mt-4;
}

.ai-prompts-page__acceptance {
  display: grid;
  @apply gap-2.5;
  @apply mt-3;
}

.ai-prompts-page__acceptance-list {
  display: grid;
  @apply gap-2;
}

.ai-prompts-page__acceptance-item {
  display: grid;
  @apply gap-1.5;
  @apply px-3 py-2.5;
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
  background: var(--ant-color-bg-container);
}

.ai-prompts-page__acceptance-title {
  display: flex;
  @apply gap-2;
  align-items: center;
  justify-content: space-between;
}

.ai-prompts-page__acceptance-error {
  color: var(--ant-color-error);
}

.ai-prompts-page__acceptance-checks {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
}

@media (max-width: 1200px) {
  .ai-prompts-page__filters {
    grid-template-columns: 1fr;
  }

  .ai-prompts-page__acceptance-title {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
