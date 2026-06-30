<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, InputNumber, Popconfirm, Select, Space, Switch, Table, Tabs, Tag, message } from 'ant-design-vue';

import {
  buildAiAgentRunPlanApi,
  createAiAgentApi,
  createAiCommandApi,
  createAiPersonaApi,
  createAiSkillApi,
  deleteAiAgentApi,
  deleteAiCommandApi,
  deleteAiPersonaApi,
  deleteAiSkillApi,
  getAiAgentsApi,
  getAiCommandsApi,
  getAiModelsApi,
  getAiPersonasApi,
  getAiPromptTemplatesApi,
  getAiSkillsApi,
  getAiToolsApi,
  updateAiAgentApi,
  updateAiCommandApi,
  updateAiPersonaApi,
  updateAiSkillApi,
} from '#/api/core';

defineOptions({ name: 'AiAgentDesignPage' });

const activeTab = ref('agents');
const loading = ref(false);
const activeDrawer = ref<'' | 'agent' | 'command' | 'persona' | 'skill'>('');
const saving = ref(false);
const editingId = ref('');
const runPlanVisible = ref(false);
const runPlan = ref<AiApi.AgentRunPlan>();

const skills = ref<AiApi.SkillItem[]>([]);
const personas = ref<AiApi.PersonaItem[]>([]);
const commands = ref<AiApi.CommandItem[]>([]);
const agents = ref<AiApi.AgentDefinitionItem[]>([]);
const tools = ref<AiApi.ToolDefinitionItem[]>([]);
const models = ref<AiApi.ModelItem[]>([]);
const templates = ref<AiApi.PromptTemplateItem[]>([]);

const query = reactive<AiApi.AgentDesignQuery>({
  includeDisabled: true,
  keyword: '',
});

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

const categoryOptions = [
  { label: '项目管理', value: '项目管理' },
  { label: '项目治理', value: '项目治理' },
  { label: '项目执行', value: '项目执行' },
  { label: '经营管理', value: '经营管理' },
  { label: '知识检索', value: '知识检索' },
];

const skillForm = reactive<AiApi.SkillSaveInput>({
  category: '项目管理',
  defaultMode: 'General',
  description: '',
  inputSchemaJson: '{}',
  instruction: '',
  isEnabled: true,
  name: '',
  outputSchemaJson: '{}',
  skillCode: '',
  sort: 0,
  toolCodes: [],
});

const personaForm = reactive<AiApi.PersonaSaveInput>({
  boundaryPrompt: '',
  configJson: '{}',
  description: '',
  isEnabled: true,
  name: '',
  personaCode: '',
  personalityPrompt: '',
  roleType: 'General',
  sort: 0,
  stylePrompt: '',
});

const commandForm = reactive<AiApi.CommandSaveInput>({
  agentCode: '',
  commandCode: '',
  configJson: '{}',
  description: '',
  exampleInput: '',
  inputPlaceholder: '',
  isEnabled: true,
  mode: 'General',
  name: '',
  promptTemplateCode: '',
  skillCode: '',
  slashName: '',
  sort: 0,
});

const agentForm = reactive<AiApi.AgentDefinitionSaveInput>({
  agentCode: '',
  defaultModelId: undefined,
  description: '',
  isEnabled: true,
  name: '',
  personaCode: '',
  roleType: 'General',
  runtimeConfigJson: '{"maxToolRounds":4,"requiresWritebackApproval":true}',
  skillCodes: [],
  sort: 0,
  systemPrompt: '',
  toolCodes: [],
});

const toolOptions = computed(() => tools.value.map((item) => ({ label: `${item.name} · ${item.toolCode}`, value: item.toolCode })));
const skillOptions = computed(() => skills.value.map((item) => ({ label: `${item.name} · ${item.skillCode}`, value: item.skillCode })));
const personaOptions = computed(() => personas.value.map((item) => ({ label: `${item.name} · ${item.personaCode}`, value: item.personaCode })));
const agentOptions = computed(() => agents.value.map((item) => ({ label: `${item.name} · ${item.agentCode}`, value: item.agentCode })));
const templateOptions = computed(() => templates.value.map((item) => ({ label: `${item.name} · ${item.templateCode}`, value: item.templateCode })));
const modelOptions = computed(() => models.value.filter((item) => item.isEnabled).map((item) => ({ label: `${item.displayName} · ${item.providerName}`, value: item.id })));

const agentColumns = [
  { dataIndex: 'name', key: 'name', title: 'Agent', width: 240 },
  { dataIndex: 'roleType', key: 'roleType', title: '角色', width: 100 },
  { dataIndex: 'personaCode', key: 'personaCode', title: '画像', width: 160 },
  { dataIndex: 'skillCodes', key: 'skillCodes', title: '技能' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 230 },
];

const skillColumns = [
  { dataIndex: 'name', key: 'name', title: '技能', width: 250 },
  { dataIndex: 'category', key: 'category', title: '分类', width: 120 },
  { dataIndex: 'defaultMode', key: 'defaultMode', title: '模式', width: 120 },
  { dataIndex: 'toolCodes', key: 'toolCodes', title: '工具' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 170 },
];

const personaColumns = [
  { dataIndex: 'name', key: 'name', title: '角色画像', width: 260 },
  { dataIndex: 'roleType', key: 'roleType', title: '角色', width: 110 },
  { dataIndex: 'description', key: 'description', title: '说明' },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 170 },
];

const commandColumns = [
  { dataIndex: 'slashName', key: 'slashName', title: '命令', width: 170 },
  { dataIndex: 'name', key: 'name', title: '名称', width: 220 },
  { dataIndex: 'skillCode', key: 'skillCode', title: '技能', width: 180 },
  { dataIndex: 'agentCode', key: 'agentCode', title: 'Agent', width: 180 },
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
    const [skillResult, personaResult, commandResult, agentResult, toolResult, modelResult, templateResult] = await Promise.all([
      getAiSkillsApi(normalizeQuery()),
      getAiPersonasApi(normalizeQuery()),
      getAiCommandsApi(normalizeQuery()),
      getAiAgentsApi(normalizeQuery()),
      getAiToolsApi(),
      getAiModelsApi(),
      getAiPromptTemplatesApi({ includeDisabled: true }),
    ]);
    skills.value = skillResult;
    personas.value = personaResult;
    commands.value = commandResult;
    agents.value = agentResult;
    tools.value = toolResult;
    models.value = modelResult;
    templates.value = templateResult;
  } finally {
    loading.value = false;
  }
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

function resetSkillForm() {
  Object.assign(skillForm, {
    category: '项目管理',
    defaultMode: 'General',
    description: '',
    inputSchemaJson: '{}',
    instruction: '',
    isEnabled: true,
    name: '',
    outputSchemaJson: '{}',
    skillCode: '',
    sort: 0,
    toolCodes: [],
  });
}

function resetPersonaForm() {
  Object.assign(personaForm, {
    boundaryPrompt: '',
    configJson: '{}',
    description: '',
    isEnabled: true,
    name: '',
    personaCode: '',
    personalityPrompt: '',
    roleType: 'General',
    sort: 0,
    stylePrompt: '',
  });
}

function resetCommandForm() {
  Object.assign(commandForm, {
    agentCode: '',
    commandCode: '',
    configJson: '{}',
    description: '',
    exampleInput: '',
    inputPlaceholder: '',
    isEnabled: true,
    mode: 'General',
    name: '',
    promptTemplateCode: '',
    skillCode: '',
    slashName: '',
    sort: 0,
  });
}

function resetAgentForm() {
  Object.assign(agentForm, {
    agentCode: '',
    defaultModelId: undefined,
    description: '',
    isEnabled: true,
    name: '',
    personaCode: '',
    roleType: 'General',
    runtimeConfigJson: '{"maxToolRounds":4,"requiresWritebackApproval":true}',
    skillCodes: [],
    sort: 0,
    systemPrompt: '',
    toolCodes: [],
  });
}

function openCreate(type: 'agent' | 'command' | 'persona' | 'skill') {
  editingId.value = '';
  if (type === 'skill') resetSkillForm();
  if (type === 'persona') resetPersonaForm();
  if (type === 'command') resetCommandForm();
  if (type === 'agent') resetAgentForm();
  activeDrawer.value = type;
}

function openEdit(type: 'agent' | 'command' | 'persona' | 'skill', record: Record<string, any>) {
  editingId.value = record.id;
  if (type === 'skill') Object.assign(skillForm, { ...record, toolCodes: [...record.toolCodes] });
  if (type === 'persona') Object.assign(personaForm, record);
  if (type === 'command') Object.assign(commandForm, record);
  if (type === 'agent') Object.assign(agentForm, { ...record, skillCodes: [...record.skillCodes], toolCodes: [...record.toolCodes] });
  activeDrawer.value = type;
}

async function handleSubmit() {
  try {
    if (activeDrawer.value === 'skill') {
      validateJson(skillForm.inputSchemaJson, '输入 Schema');
      validateJson(skillForm.outputSchemaJson, '输出 Schema');
    }
    if (activeDrawer.value === 'persona') validateJson(personaForm.configJson, '扩展配置');
    if (activeDrawer.value === 'command') validateJson(commandForm.configJson, '命令配置');
    if (activeDrawer.value === 'agent') validateJson(agentForm.runtimeConfigJson, '运行时配置');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    if (activeDrawer.value === 'skill') {
      editingId.value ? await updateAiSkillApi(editingId.value, skillForm) : await createAiSkillApi(skillForm);
    }
    if (activeDrawer.value === 'persona') {
      editingId.value ? await updateAiPersonaApi(editingId.value, personaForm) : await createAiPersonaApi(personaForm);
    }
    if (activeDrawer.value === 'command') {
      editingId.value ? await updateAiCommandApi(editingId.value, commandForm) : await createAiCommandApi(commandForm);
    }
    if (activeDrawer.value === 'agent') {
      editingId.value ? await updateAiAgentApi(editingId.value, agentForm) : await createAiAgentApi(agentForm);
    }

    activeDrawer.value = '';
    message.success('配置已保存');
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(type: 'agent' | 'command' | 'persona' | 'skill', id: string) {
  if (type === 'skill') await deleteAiSkillApi(id);
  if (type === 'persona') await deleteAiPersonaApi(id);
  if (type === 'command') await deleteAiCommandApi(id);
  if (type === 'agent') await deleteAiAgentApi(id);
  message.success('配置已删除');
  await loadData();
}

async function openRunPlan(record: Record<string, any>) {
  runPlan.value = await buildAiAgentRunPlanApi((record as AiApi.AgentDefinitionItem).id);
  runPlanVisible.value = true;
}

onMounted(loadData);
</script>

<template>
  <Page description="统一维护企业内 Agent、技能、角色画像和快捷命令，作为 AI 会话与自动化任务的可控运行基座。" title="Agent 编排">
    <Card :bordered="false" class="ai-agent-design rounded-2xl">
      <div class="ai-agent-design__toolbar">
        <Space>
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索编码、名称或说明" @press-enter="loadData" />
          <Switch v-model:checked="query.includeDisabled" checked-children="含停用" un-checked-children="只启用" />
          <Button @click="loadData">查询</Button>
        </Space>
      </div>

      <Tabs v-model:activeKey="activeTab">
        <Tabs.TabPane key="agents" tab="Agent">
          <div class="ai-agent-design__tab-actions">
            <Button type="primary" @click="openCreate('agent')">新增 Agent</Button>
          </div>
          <Table :columns="agentColumns" :data-source="agents" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <Space direction="vertical" :size="2">
                  <Space><strong>{{ record.name }}</strong><Tag v-if="record.isBuiltin" color="blue">内置</Tag></Space>
                  <span class="ai-agent-design__muted">{{ record.agentCode }}</span>
                  <span class="ai-agent-design__muted">{{ record.description || '-' }}</span>
                </Space>
              </template>
              <template v-else-if="column.key === 'skillCodes'">
                <Space wrap><Tag v-for="code in record.skillCodes" :key="code">{{ code }}</Tag></Space>
              </template>
              <template v-else-if="column.key === 'isEnabled'">
                <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" @click="openRunPlan(record)">运行计划</Button>
                  <Button type="link" @click="openEdit('agent', record)">编辑</Button>
                  <Popconfirm title="确定删除该 Agent？" @confirm="handleDelete('agent', record.id)">
                    <Button :disabled="record.isBuiltin" danger type="link">删除</Button>
                  </Popconfirm>
                </Space>
              </template>
            </template>
          </Table>
        </Tabs.TabPane>

        <Tabs.TabPane key="skills" tab="技能">
          <div class="ai-agent-design__tab-actions">
            <Button type="primary" @click="openCreate('skill')">新增技能</Button>
          </div>
          <Table :columns="skillColumns" :data-source="skills" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <Space direction="vertical" :size="2">
                  <Space><strong>{{ record.name }}</strong><Tag v-if="record.isBuiltin" color="blue">内置</Tag></Space>
                  <span class="ai-agent-design__muted">{{ record.skillCode }}</span>
                  <span class="ai-agent-design__muted">{{ record.description || '-' }}</span>
                </Space>
              </template>
              <template v-else-if="column.key === 'toolCodes'">
                <Space wrap><Tag v-for="code in record.toolCodes" :key="code">{{ code }}</Tag></Space>
              </template>
              <template v-else-if="column.key === 'isEnabled'">
                <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" @click="openEdit('skill', record)">编辑</Button>
                  <Popconfirm title="确定删除该技能？" @confirm="handleDelete('skill', record.id)">
                    <Button :disabled="record.isBuiltin" danger type="link">删除</Button>
                  </Popconfirm>
                </Space>
              </template>
            </template>
          </Table>
        </Tabs.TabPane>

        <Tabs.TabPane key="personas" tab="角色画像">
          <div class="ai-agent-design__tab-actions">
            <Button type="primary" @click="openCreate('persona')">新增角色画像</Button>
          </div>
          <Table :columns="personaColumns" :data-source="personas" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <Space direction="vertical" :size="2">
                  <Space><strong>{{ record.name }}</strong><Tag v-if="record.isBuiltin" color="blue">内置</Tag></Space>
                  <span class="ai-agent-design__muted">{{ record.personaCode }}</span>
                </Space>
              </template>
              <template v-else-if="column.key === 'isEnabled'">
                <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" @click="openEdit('persona', record)">编辑</Button>
                  <Popconfirm title="确定删除该角色画像？" @confirm="handleDelete('persona', record.id)">
                    <Button :disabled="record.isBuiltin" danger type="link">删除</Button>
                  </Popconfirm>
                </Space>
              </template>
            </template>
          </Table>
        </Tabs.TabPane>

        <Tabs.TabPane key="commands" tab="快捷命令">
          <div class="ai-agent-design__tab-actions">
            <Button type="primary" @click="openCreate('command')">新增命令</Button>
          </div>
          <Table :columns="commandColumns" :data-source="commands" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'slashName'">
                <Tag color="blue">{{ record.slashName }}</Tag>
              </template>
              <template v-else-if="column.key === 'isEnabled'">
                <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" @click="openEdit('command', record)">编辑</Button>
                  <Popconfirm title="确定删除该命令？" @confirm="handleDelete('command', record.id)">
                    <Button :disabled="record.isBuiltin" danger type="link">删除</Button>
                  </Popconfirm>
                </Space>
              </template>
            </template>
          </Table>
        </Tabs.TabPane>
      </Tabs>
    </Card>

    <Drawer :open="activeDrawer === 'agent'" :title="editingId ? '编辑 Agent' : '新增 Agent'" width="720" @close="activeDrawer = ''">
      <Form layout="vertical">
        <Form.Item label="Agent 编码"><Input v-model:value="agentForm.agentCode" :disabled="!!editingId" /></Form.Item>
        <Form.Item label="名称"><Input v-model:value="agentForm.name" /></Form.Item>
        <Form.Item label="角色"><Select v-model:value="agentForm.roleType" :options="roleOptions" /></Form.Item>
        <Form.Item label="角色画像"><Select v-model:value="agentForm.personaCode" allow-clear :options="personaOptions" /></Form.Item>
        <Form.Item label="默认模型"><Select v-model:value="agentForm.defaultModelId" allow-clear :options="modelOptions" /></Form.Item>
        <Form.Item label="技能"><Select v-model:value="agentForm.skillCodes" mode="multiple" :options="skillOptions" /></Form.Item>
        <Form.Item label="工具"><Select v-model:value="agentForm.toolCodes" mode="multiple" :options="toolOptions" /></Form.Item>
        <Form.Item label="系统提示词"><Input.TextArea v-model:value="agentForm.systemPrompt" :auto-size="{ minRows: 6, maxRows: 12 }" :maxlength="16_000" /></Form.Item>
        <Form.Item label="运行时配置"><Input.TextArea v-model:value="agentForm.runtimeConfigJson" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="说明"><Input.TextArea v-model:value="agentForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="排序"><InputNumber v-model:value="agentForm.sort" class="ai-agent-design__number" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="agentForm.isEnabled" /></Form.Item>
      </Form>
      <template #footer><Space><Button @click="activeDrawer = ''">取消</Button><Button :loading="saving" type="primary" @click="handleSubmit">保存</Button></Space></template>
    </Drawer>

    <Drawer :open="activeDrawer === 'skill'" :title="editingId ? '编辑技能' : '新增技能'" width="720" @close="activeDrawer = ''">
      <Form layout="vertical">
        <Form.Item label="技能编码"><Input v-model:value="skillForm.skillCode" :disabled="!!editingId" /></Form.Item>
        <Form.Item label="名称"><Input v-model:value="skillForm.name" /></Form.Item>
        <Form.Item label="分类"><Select v-model:value="skillForm.category" :options="categoryOptions" /></Form.Item>
        <Form.Item label="默认模式"><Select v-model:value="skillForm.defaultMode" :options="modeOptions" /></Form.Item>
        <Form.Item label="工具"><Select v-model:value="skillForm.toolCodes" mode="multiple" :options="toolOptions" /></Form.Item>
        <Form.Item label="技能说明"><Input.TextArea v-model:value="skillForm.instruction" :auto-size="{ minRows: 6, maxRows: 12 }" :maxlength="16_000" /></Form.Item>
        <Form.Item label="输入 Schema"><Input.TextArea v-model:value="skillForm.inputSchemaJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="输出 Schema"><Input.TextArea v-model:value="skillForm.outputSchemaJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="说明"><Input.TextArea v-model:value="skillForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="排序"><InputNumber v-model:value="skillForm.sort" class="ai-agent-design__number" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="skillForm.isEnabled" /></Form.Item>
      </Form>
      <template #footer><Space><Button @click="activeDrawer = ''">取消</Button><Button :loading="saving" type="primary" @click="handleSubmit">保存</Button></Space></template>
    </Drawer>

    <Drawer :open="activeDrawer === 'persona'" :title="editingId ? '编辑角色画像' : '新增角色画像'" width="720" @close="activeDrawer = ''">
      <Form layout="vertical">
        <Form.Item label="画像编码"><Input v-model:value="personaForm.personaCode" :disabled="!!editingId" /></Form.Item>
        <Form.Item label="名称"><Input v-model:value="personaForm.name" /></Form.Item>
        <Form.Item label="角色"><Select v-model:value="personaForm.roleType" :options="roleOptions" /></Form.Item>
        <Form.Item label="人格提示词"><Input.TextArea v-model:value="personaForm.personalityPrompt" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="边界提示词"><Input.TextArea v-model:value="personaForm.boundaryPrompt" :auto-size="{ minRows: 4, maxRows: 8 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="表达风格"><Input.TextArea v-model:value="personaForm.stylePrompt" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="4000" /></Form.Item>
        <Form.Item label="扩展配置"><Input.TextArea v-model:value="personaForm.configJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="说明"><Input.TextArea v-model:value="personaForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="排序"><InputNumber v-model:value="personaForm.sort" class="ai-agent-design__number" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="personaForm.isEnabled" /></Form.Item>
      </Form>
      <template #footer><Space><Button @click="activeDrawer = ''">取消</Button><Button :loading="saving" type="primary" @click="handleSubmit">保存</Button></Space></template>
    </Drawer>

    <Drawer :open="activeDrawer === 'command'" :title="editingId ? '编辑快捷命令' : '新增快捷命令'" width="720" @close="activeDrawer = ''">
      <Form layout="vertical">
        <Form.Item label="命令编码"><Input v-model:value="commandForm.commandCode" :disabled="!!editingId" /></Form.Item>
        <Form.Item label="斜杠命令"><Input v-model:value="commandForm.slashName" placeholder="/周报" /></Form.Item>
        <Form.Item label="名称"><Input v-model:value="commandForm.name" /></Form.Item>
        <Form.Item label="模式"><Select v-model:value="commandForm.mode" :options="modeOptions" /></Form.Item>
        <Form.Item label="技能"><Select v-model:value="commandForm.skillCode" allow-clear :options="skillOptions" /></Form.Item>
        <Form.Item label="场景模板"><Select v-model:value="commandForm.promptTemplateCode" allow-clear :options="templateOptions" /></Form.Item>
        <Form.Item label="Agent"><Select v-model:value="commandForm.agentCode" allow-clear :options="agentOptions" /></Form.Item>
        <Form.Item label="输入提示"><Input v-model:value="commandForm.inputPlaceholder" /></Form.Item>
        <Form.Item label="示例输入"><Input.TextArea v-model:value="commandForm.exampleInput" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="命令配置"><Input.TextArea v-model:value="commandForm.configJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="说明"><Input.TextArea v-model:value="commandForm.description" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="排序"><InputNumber v-model:value="commandForm.sort" class="ai-agent-design__number" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="commandForm.isEnabled" /></Form.Item>
      </Form>
      <template #footer><Space><Button @click="activeDrawer = ''">取消</Button><Button :loading="saving" type="primary" @click="handleSubmit">保存</Button></Space></template>
    </Drawer>

    <Drawer :open="runPlanVisible" title="Agent 运行计划" width="640" @close="runPlanVisible = false">
      <Space v-if="runPlan" direction="vertical" class="ai-agent-design__run-plan">
        <Tag color="blue">{{ runPlan.agentCode }}</Tag>
        <div><strong>{{ runPlan.agentName }}</strong></div>
        <div>模型：{{ runPlan.modelName || '未指定' }}</div>
        <div>画像：{{ runPlan.personaCode || '未指定' }}</div>
        <div>技能：{{ runPlan.skillCodes.join('、') || '未配置' }}</div>
        <div>工具：{{ runPlan.toolCodes.join('、') || '未配置' }}</div>
        <Input.TextArea :value="runPlan.systemPrompt" :auto-size="{ minRows: 8, maxRows: 14 }" readonly />
      </Space>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-agent-design__toolbar {
  @apply mb-4;
}

.ai-agent-design__tab-actions {
  display: flex;
  justify-content: flex-end;
  @apply mb-3;
}

.ai-agent-design__muted {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
}

.ai-agent-design__number {
  width: 160px;
}

.ai-agent-design__run-plan {
  width: 100%;
}
</style>
