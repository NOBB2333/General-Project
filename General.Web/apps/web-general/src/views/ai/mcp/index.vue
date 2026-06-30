<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Drawer,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tabs,
  Tag,
  message,
} from 'ant-design-vue';

import {
  createAiMcpServerApi,
  deleteAiMcpServerApi,
  discoverAiMcpToolsApi,
  getAiMcpInvocationsApi,
  getAiMcpServersApi,
  getAiMcpToolsApi,
  invokeAiMcpToolApi,
  testAiMcpServerApi,
  updateAiMcpServerApi,
  updateAiMcpToolPolicyApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiMcpPage' });

const transportOptions = [
  { label: 'Streamable HTTP', value: 'StreamableHttp' },
  { label: 'SSE', value: 'Sse' },
  { label: 'stdio', value: 'Stdio' },
];

const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '已完成', value: 'Completed' },
  { label: '失败', value: 'Failed' },
  { label: '超时', value: 'Timeout' },
  { label: '运行中', value: 'Running' },
];

const serverColumns = [
  { dataIndex: 'name', key: 'name', title: 'Server', width: 260 },
  { dataIndex: 'transportType', key: 'transportType', title: '传输', width: 140 },
  { dataIndex: 'endpoint', key: 'endpoint', title: '连接配置' },
  { dataIndex: 'toolCount', key: 'toolCount', title: '工具数', width: 90 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { dataIndex: 'lastDiscoveryTime', key: 'lastDiscoveryTime', title: '最近发现', width: 170 },
  { key: 'action', title: '操作', width: 330 },
];

const toolColumns = [
  { dataIndex: 'toolName', key: 'toolName', title: '工具', width: 260 },
  { dataIndex: 'serverName', key: 'serverName', title: 'Server', width: 180 },
  { dataIndex: 'permissionCode', key: 'permissionCode', title: '权限', width: 210 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { dataIndex: 'lastDiscoveredTime', key: 'lastDiscoveredTime', title: '发现时间', width: 170 },
  { key: 'action', title: '操作', width: 230 },
];

const invocationColumns = [
  { dataIndex: 'toolName', key: 'toolName', title: '工具', width: 240 },
  { dataIndex: 'serverName', key: 'serverName', title: 'Server', width: 180 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  { dataIndex: 'elapsedMilliseconds', key: 'elapsedMilliseconds', title: '耗时', width: 110 },
  { dataIndex: 'errorMessage', key: 'errorMessage', title: '错误信息' },
  { dataIndex: 'creationTime', key: 'creationTime', title: '调用时间', width: 170 },
  { key: 'action', title: '操作', width: 100 },
];

const servers = ref<AiApi.McpServerItem[]>([]);
const tools = ref<AiApi.McpToolItem[]>([]);
const invocations = ref<AiApi.McpInvocationItem[]>([]);
const activeTab = ref('servers');
const loadingServers = ref(false);
const loadingTools = ref(false);
const loadingInvocations = ref(false);
const serverDrawerVisible = ref(false);
const policyDrawerVisible = ref(false);
const invokeDrawerVisible = ref(false);
const invocationDrawerVisible = ref(false);
const savingServer = ref(false);
const savingPolicy = ref(false);
const invoking = ref(false);
const editingServerId = ref('');
const editingToolId = ref('');
const invokingTool = ref<AiApi.McpToolItem>();
const selectedInvocation = ref<AiApi.McpInvocationItem>();
const invokeResult = ref<AiApi.McpInvokeResult>();
const toolServerFilter = ref<string>();
const { actionLoadingKey, runAction } = useActionLoading();

const serverForm = reactive<AiApi.McpServerSaveInput>({
  argumentsJson: '[]',
  command: '',
  configJson: '{}',
  endpoint: '',
  environmentJson: '{}',
  headersJson: '{}',
  isEnabled: true,
  name: '',
  permissionCode: 'Ai.Mcp.Manage',
  protocolVersion: '2024-11-05',
  remark: '',
  serverCode: '',
  sort: 0,
  timeoutSeconds: 30,
  transportType: 'StreamableHttp',
});

const policyForm = reactive<AiApi.McpToolPolicyInput>({
  isEnabled: true,
  permissionCode: 'Ai.Mcp.Manage',
});

const invokeForm = reactive<AiApi.McpInvokeInput>({
  argumentsJson: '{}',
});

const invocationQuery = reactive<AiApi.McpInvocationQuery>({
  maxResultCount: 50,
  serverId: undefined,
  status: '',
  toolId: undefined,
  toolName: '',
});

const serverOptions = computed(() =>
  servers.value.map((item) => ({
    label: `${item.name} · ${item.serverCode}`,
    value: item.id,
  })),
);

const filteredToolOptions = computed(() =>
  tools.value
    .filter((item) => !invocationQuery.serverId || item.serverId === invocationQuery.serverId)
    .map((item) => ({
      label: `${item.toolName} · ${item.serverName}`,
      value: item.id,
    })),
);

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function statusColor(status: string) {
  if (status === 'Completed') {
    return 'green';
  }

  if (status === 'Running') {
    return 'processing';
  }

  if (status === 'Timeout') {
    return 'orange';
  }

  return status === 'Failed' ? 'red' : 'default';
}

function transportLabel(value: string) {
  return transportOptions.find((item) => item.value === value)?.label || value;
}

function validateJson(value: string, label: string, expected: 'array' | 'object') {
  let parsed: unknown;
  try {
    parsed = JSON.parse(value.trim() || (expected === 'array' ? '[]' : '{}'));
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }

  if (expected === 'array' && !Array.isArray(parsed)) {
    throw new Error(`${label} 必须是 JSON 数组`);
  }

  if (expected === 'object' && (Array.isArray(parsed) || parsed === null || typeof parsed !== 'object')) {
    throw new Error(`${label} 必须是 JSON 对象`);
  }
}

function normalizeServerPayload(): AiApi.McpServerSaveInput {
  return {
    ...serverForm,
    argumentsJson: serverForm.argumentsJson.trim() || '[]',
    command: serverForm.command.trim(),
    configJson: serverForm.configJson.trim() || '{}',
    endpoint: serverForm.endpoint.trim(),
    environmentJson: serverForm.environmentJson.trim() || '{}',
    headersJson: serverForm.headersJson.trim() || '{}',
    name: serverForm.name.trim(),
    permissionCode: serverForm.permissionCode.trim(),
    protocolVersion: serverForm.protocolVersion.trim() || '2024-11-05',
    remark: serverForm.remark.trim(),
    serverCode: serverForm.serverCode.trim(),
  };
}

function validateServerForm() {
  if (!serverForm.serverCode.trim() || !serverForm.name.trim()) {
    throw new Error('请填写 Server 编码和名称');
  }

  if (serverForm.transportType === 'Stdio' && !serverForm.command.trim()) {
    throw new Error('stdio MCP Server 必须配置启动命令');
  }

  if (serverForm.transportType !== 'Stdio' && !serverForm.endpoint.trim()) {
    throw new Error('HTTP/SSE MCP Server 必须配置 Endpoint');
  }

  validateJson(serverForm.argumentsJson, '启动参数', 'array');
  validateJson(serverForm.headersJson, '请求头', 'object');
  validateJson(serverForm.environmentJson, '环境变量', 'object');
  validateJson(serverForm.configJson, '扩展配置', 'object');
}

async function loadServers() {
  loadingServers.value = true;
  try {
    servers.value = await getAiMcpServersApi();
  } finally {
    loadingServers.value = false;
  }
}

async function loadTools() {
  loadingTools.value = true;
  try {
    tools.value = await getAiMcpToolsApi(toolServerFilter.value);
  } finally {
    loadingTools.value = false;
  }
}

function normalizeInvocationQuery() {
  return {
    maxResultCount: invocationQuery.maxResultCount,
    serverId: invocationQuery.serverId || undefined,
    status: invocationQuery.status || undefined,
    toolId: invocationQuery.toolId || undefined,
    toolName: invocationQuery.toolName?.trim() || undefined,
  };
}

async function loadInvocations() {
  loadingInvocations.value = true;
  try {
    invocations.value = await getAiMcpInvocationsApi(normalizeInvocationQuery());
  } finally {
    loadingInvocations.value = false;
  }
}

async function loadAll() {
  await Promise.all([loadServers(), loadTools(), loadInvocations()]);
}

function resetServerForm() {
  Object.assign(serverForm, {
    argumentsJson: '[]',
    command: '',
    configJson: '{}',
    endpoint: '',
    environmentJson: '{}',
    headersJson: '{}',
    isEnabled: true,
    name: '',
    permissionCode: 'Ai.Mcp.Manage',
    protocolVersion: '2024-11-05',
    remark: '',
    serverCode: '',
    sort: 0,
    timeoutSeconds: 30,
    transportType: 'StreamableHttp',
  });
}

function openCreateServer() {
  editingServerId.value = '';
  resetServerForm();
  serverDrawerVisible.value = true;
}

function openEditServer(record: AiApi.McpServerItem) {
  editingServerId.value = record.id;
  Object.assign(serverForm, {
    argumentsJson: record.argumentsJson || '[]',
    command: record.command,
    configJson: record.configJson || '{}',
    endpoint: record.endpoint,
    environmentJson: record.environmentJson || '{}',
    headersJson: record.headersJson || '{}',
    isEnabled: record.isEnabled,
    name: record.name,
    permissionCode: record.permissionCode || 'Ai.Mcp.Manage',
    protocolVersion: record.protocolVersion || '2024-11-05',
    remark: record.remark,
    serverCode: record.serverCode,
    sort: record.sort,
    timeoutSeconds: record.timeoutSeconds || 30,
    transportType: record.transportType || 'StreamableHttp',
  });
  serverDrawerVisible.value = true;
}

function openEditServerFromTable(record: Record<string, unknown>) {
  openEditServer(record as unknown as AiApi.McpServerItem);
}

async function handleSaveServer() {
  try {
    validateServerForm();
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  savingServer.value = true;
  try {
    const payload = normalizeServerPayload();
    if (editingServerId.value) {
      await updateAiMcpServerApi(editingServerId.value, payload);
      message.success('MCP Server 已保存');
    } else {
      await createAiMcpServerApi(payload);
      message.success('MCP Server 已创建');
    }

    serverDrawerVisible.value = false;
    await loadAll();
  } finally {
    savingServer.value = false;
  }
}

async function handleDeleteServer(record: AiApi.McpServerItem) {
  await deleteAiMcpServerApi(record.id);
  message.success('MCP Server 已删除');
  await loadAll();
}

async function handleDeleteServerFromTable(record: Record<string, unknown>) {
  await handleDeleteServer(record as unknown as AiApi.McpServerItem);
}

async function handleTestServer(record: AiApi.McpServerItem) {
  await runAction(`test-mcp-server:${record.id}`, async () => {
    const result = await testAiMcpServerApi(record.id);
    if (result.success) {
      message.success('MCP Server 连接测试通过');
    } else {
      message.warning(result.errorMessage || 'MCP Server 连接测试失败');
    }
    await loadServers();
  });
}

async function handleTestServerFromTable(record: Record<string, unknown>) {
  await handleTestServer(record as unknown as AiApi.McpServerItem);
}

async function handleDiscoverTools(record: AiApi.McpServerItem) {
  await runAction(`discover-mcp-tools:${record.id}`, async () => {
    const result = await discoverAiMcpToolsApi(record.id);
    if (result.errorMessage) {
      message.warning(result.errorMessage);
    } else {
      message.success(`已发现 ${result.toolCount} 个 MCP 工具`);
    }
    await loadAll();
  });
}

async function handleDiscoverToolsFromTable(record: Record<string, unknown>) {
  await handleDiscoverTools(record as unknown as AiApi.McpServerItem);
}

function openPolicy(record: AiApi.McpToolItem) {
  editingToolId.value = record.id;
  Object.assign(policyForm, {
    isEnabled: record.isEnabled,
    permissionCode: record.permissionCode || 'Ai.Mcp.Manage',
  });
  policyDrawerVisible.value = true;
}

function openPolicyFromTable(record: Record<string, unknown>) {
  openPolicy(record as unknown as AiApi.McpToolItem);
}

async function handleSavePolicy() {
  if (!editingToolId.value) {
    return;
  }

  savingPolicy.value = true;
  try {
    await updateAiMcpToolPolicyApi(editingToolId.value, {
      isEnabled: policyForm.isEnabled,
      permissionCode: policyForm.permissionCode.trim(),
    });
    message.success('工具策略已保存');
    policyDrawerVisible.value = false;
    await loadTools();
  } finally {
    savingPolicy.value = false;
  }
}

function openInvoke(record: AiApi.McpToolItem) {
  invokingTool.value = record;
  invokeResult.value = undefined;
  Object.assign(invokeForm, {
    argumentsJson: '{}',
  });
  invokeDrawerVisible.value = true;
}

function openInvokeFromTable(record: Record<string, unknown>) {
  openInvoke(record as unknown as AiApi.McpToolItem);
}

async function handleInvokeTool() {
  if (!invokingTool.value) {
    return;
  }

  try {
    validateJson(invokeForm.argumentsJson, '调用参数', 'object');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  invoking.value = true;
  try {
    invokeResult.value = await invokeAiMcpToolApi(invokingTool.value.id, {
      argumentsJson: invokeForm.argumentsJson.trim() || '{}',
    });
    if (invokeResult.value.status === 'Completed') {
      message.success('MCP 工具调用完成');
    } else {
      message.warning(invokeResult.value.errorMessage || 'MCP 工具调用失败');
    }
    await loadInvocations();
  } finally {
    invoking.value = false;
  }
}

function openInvocation(record: Record<string, unknown>) {
  selectedInvocation.value = record as unknown as AiApi.McpInvocationItem;
  invocationDrawerVisible.value = true;
}

function handleToolServerFilterChange() {
  loadTools();
}

function handleInvocationServerChange() {
  invocationQuery.toolId = undefined;
  loadInvocations();
}

onMounted(loadAll);
</script>

<template>
  <Page description="统一管理外部 MCP Server、发现工具、绑定权限并审计调用。" title="MCP 配置">
    <div class="ai-mcp-page">
      <Card :bordered="false" class="ai-mcp-page__summary rounded-2xl">
        <div class="ai-mcp-page__summary-grid">
          <div class="ai-mcp-page__metric">
            <span>MCP 服务</span>
            <strong>{{ servers.length }}</strong>
          </div>
          <div class="ai-mcp-page__metric">
            <span>已启用工具</span>
            <strong>{{ tools.filter((item) => item.isEnabled).length }}</strong>
          </div>
          <div class="ai-mcp-page__metric">
            <span>最近调用</span>
            <strong>{{ invocations.length }}</strong>
          </div>
          <div class="ai-mcp-page__metric">
            <span>异常服务</span>
            <strong>{{ servers.filter((item) => item.lastErrorMessage).length }}</strong>
          </div>
        </div>
      </Card>

      <Card :bordered="false" class="rounded-2xl">
        <Tabs v-model:activeKey="activeTab">
          <Tabs.TabPane key="servers" tab="Server 配置">
            <div class="ai-mcp-page__toolbar">
              <Space>
                <Button @click="loadServers">刷新</Button>
                <Button type="primary" @click="openCreateServer">新增 Server</Button>
              </Space>
            </div>
            <Table
              :columns="serverColumns"
              :data-source="servers"
              :loading="loadingServers"
              :pagination="{ pageSize: 20, showSizeChanger: true }"
              row-key="id"
              size="middle"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'name'">
                  <Space direction="vertical" :size="2">
                    <Space>
                      <strong>{{ record.name }}</strong>
                      <Tag v-if="record.lastErrorMessage" color="red">异常</Tag>
                    </Space>
                    <span class="ai-mcp-page__muted">{{ record.serverCode }}</span>
                    <span v-if="record.remark" class="ai-mcp-page__muted">{{ record.remark }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'transportType'">
                  <Tag>{{ transportLabel(record.transportType) }}</Tag>
                </template>
                <template v-else-if="column.key === 'endpoint'">
                  <Space direction="vertical" :size="2">
                    <span>{{ record.transportType === 'Stdio' ? record.command : record.endpoint }}</span>
                    <span v-if="record.lastErrorMessage" class="ai-mcp-page__error">{{ record.lastErrorMessage }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'isEnabled'">
                  <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
                </template>
                <template v-else-if="column.key === 'lastDiscoveryTime'">
                  {{ formatDate(record.lastDiscoveryTime) }}
                </template>
                <template v-else-if="column.key === 'action'">
                  <Space>
                    <Button
                      :loading="actionLoadingKey === `test-mcp-server:${record.id}`"
                      size="small"
                      @click="handleTestServerFromTable(record)"
                    >
                      测试
                    </Button>
                    <Button
                      :loading="actionLoadingKey === `discover-mcp-tools:${record.id}`"
                      size="small"
                      type="primary"
                      @click="handleDiscoverToolsFromTable(record)"
                    >
                      发现工具
                    </Button>
                    <Button size="small" @click="openEditServerFromTable(record)">编辑</Button>
                    <Popconfirm title="删除 Server 会同步删除已发现工具，确认继续？" @confirm="handleDeleteServerFromTable(record)">
                      <Button danger size="small">删除</Button>
                    </Popconfirm>
                  </Space>
                </template>
              </template>
            </Table>
          </Tabs.TabPane>

          <Tabs.TabPane key="tools" tab="工具策略">
            <div class="ai-mcp-page__toolbar">
              <Space>
                <Select
                  v-model:value="toolServerFilter"
                  allow-clear
                  class="ai-mcp-page__select"
                  placeholder="按 Server 筛选"
                  :options="serverOptions"
                  @change="handleToolServerFilterChange"
                />
                <Button @click="loadTools">刷新</Button>
              </Space>
            </div>
            <Table
              :columns="toolColumns"
              :data-source="tools"
              :loading="loadingTools"
              :pagination="{ pageSize: 20, showSizeChanger: true }"
              row-key="id"
              size="middle"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'toolName'">
                  <Space direction="vertical" :size="2">
                    <strong>{{ record.toolName }}</strong>
                    <span class="ai-mcp-page__muted">{{ record.description || '无描述' }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'permissionCode'">
                  <Tag>{{ record.permissionCode || '继承 Server 权限' }}</Tag>
                </template>
                <template v-else-if="column.key === 'isEnabled'">
                  <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
                </template>
                <template v-else-if="column.key === 'lastDiscoveredTime'">
                  {{ formatDate(record.lastDiscoveredTime) }}
                </template>
                <template v-else-if="column.key === 'action'">
                  <Space>
                    <Button size="small" @click="openPolicyFromTable(record)">策略</Button>
                    <Button :disabled="!record.isEnabled" size="small" type="primary" @click="openInvokeFromTable(record)">
                      调用
                    </Button>
                  </Space>
                </template>
              </template>
            </Table>
          </Tabs.TabPane>

          <Tabs.TabPane key="invocations" tab="调用日志">
            <div class="ai-mcp-page__filters">
              <Select
                v-model:value="invocationQuery.serverId"
                allow-clear
                class="ai-mcp-page__select"
                placeholder="Server"
                :options="serverOptions"
                @change="handleInvocationServerChange"
              />
              <Select
                v-model:value="invocationQuery.toolId"
                allow-clear
                class="ai-mcp-page__select"
                placeholder="工具"
                :options="filteredToolOptions"
                @change="loadInvocations"
              />
              <Select
                v-model:value="invocationQuery.status"
                class="ai-mcp-page__select"
                :options="statusOptions"
                @change="loadInvocations"
              />
              <Input
                v-model:value="invocationQuery.toolName"
                allow-clear
                class="ai-mcp-page__input"
                placeholder="工具名称"
                @press-enter="loadInvocations"
              />
              <Button @click="loadInvocations">查询</Button>
            </div>
            <Table
              :columns="invocationColumns"
              :data-source="invocations"
              :loading="loadingInvocations"
              :pagination="{ pageSize: 20, showSizeChanger: true }"
              row-key="invocationId"
              size="middle"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'status'">
                  <Tag :color="statusColor(record.status)">{{ record.status }}</Tag>
                </template>
                <template v-else-if="column.key === 'elapsedMilliseconds'">
                  {{ record.elapsedMilliseconds }} ms
                </template>
                <template v-else-if="column.key === 'errorMessage'">
                  <span :class="record.errorMessage ? 'ai-mcp-page__error' : 'ai-mcp-page__muted'">
                    {{ record.errorMessage || '-' }}
                  </span>
                </template>
                <template v-else-if="column.key === 'creationTime'">
                  {{ formatDate(record.creationTime) }}
                </template>
                <template v-else-if="column.key === 'action'">
                  <Button size="small" @click="openInvocation(record)">详情</Button>
                </template>
              </template>
            </Table>
          </Tabs.TabPane>
        </Tabs>
      </Card>
    </div>

    <Drawer
      v-model:open="serverDrawerVisible"
      :title="editingServerId ? '编辑 MCP Server' : '新增 MCP Server'"
      width="920px"
    >
      <Form layout="vertical">
        <div class="ai-mcp-page__form-grid">
          <Form.Item label="Server 编码" required>
            <Input v-model:value="serverForm.serverCode" :disabled="!!editingServerId" :maxlength="64" placeholder="如 feishu_docs" />
          </Form.Item>
          <Form.Item label="名称" required>
            <Input v-model:value="serverForm.name" :maxlength="128" />
          </Form.Item>
          <Form.Item label="传输类型" required>
            <Select v-model:value="serverForm.transportType" :options="transportOptions" />
          </Form.Item>
          <Form.Item label="超时时间（秒）">
            <InputNumber v-model:value="serverForm.timeoutSeconds" class="ai-mcp-page__number" :max="180" :min="1" />
          </Form.Item>
          <Form.Item label="Endpoint">
            <Input v-model:value="serverForm.endpoint" :maxlength="512" placeholder="Streamable HTTP / SSE 地址" />
          </Form.Item>
          <Form.Item label="启动命令">
            <Input v-model:value="serverForm.command" :maxlength="512" placeholder="stdio 传输使用" />
          </Form.Item>
          <Form.Item label="协议版本">
            <Input v-model:value="serverForm.protocolVersion" :maxlength="32" />
          </Form.Item>
          <Form.Item label="排序">
            <InputNumber v-model:value="serverForm.sort" class="ai-mcp-page__number" />
          </Form.Item>
          <Form.Item label="默认权限">
            <Input v-model:value="serverForm.permissionCode" :maxlength="128" placeholder="如 Ai.Mcp.Manage" />
          </Form.Item>
          <Form.Item label="启用">
            <Switch v-model:checked="serverForm.isEnabled" checked-children="启用" un-checked-children="停用" />
          </Form.Item>
        </div>
        <Form.Item label="启动参数 JSON 数组">
          <Input.TextArea v-model:value="serverForm.argumentsJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="请求头 JSON 对象">
          <Input.TextArea v-model:value="serverForm.headersJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="环境变量 JSON 对象">
          <Input.TextArea v-model:value="serverForm.environmentJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="扩展配置 JSON 对象">
          <Input.TextArea v-model:value="serverForm.configJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="serverForm.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="serverDrawerVisible = false">取消</Button>
          <Button :loading="savingServer" type="primary" @click="handleSaveServer">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="policyDrawerVisible"
      title="工具策略"
      width="560px"
    >
      <Form layout="vertical">
        <Form.Item label="启用状态">
          <Switch v-model:checked="policyForm.isEnabled" checked-children="启用" un-checked-children="停用" />
        </Form.Item>
        <Form.Item label="权限编码">
          <Input v-model:value="policyForm.permissionCode" :maxlength="128" placeholder="留空时继承 Server 权限" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="policyDrawerVisible = false">取消</Button>
          <Button :loading="savingPolicy" type="primary" @click="handleSavePolicy">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer
      v-model:open="invokeDrawerVisible"
      :title="invokingTool ? `调用工具：${invokingTool.toolName}` : '调用工具'"
      width="720px"
    >
      <Form layout="vertical">
        <Form.Item label="调用参数 JSON 对象">
          <Input.TextArea v-model:value="invokeForm.argumentsJson" :auto-size="{ minRows: 8, maxRows: 16 }" :maxlength="16_000" />
        </Form.Item>
        <Button :loading="invoking" type="primary" @click="handleInvokeTool">执行调用</Button>
      </Form>
      <div v-if="invokeResult" class="ai-mcp-page__result">
        <Space>
          <Tag :color="statusColor(invokeResult.status)">{{ invokeResult.status }}</Tag>
          <span>{{ invokeResult.elapsedMilliseconds }} ms</span>
        </Space>
        <p v-if="invokeResult.errorMessage" class="ai-mcp-page__error">{{ invokeResult.errorMessage }}</p>
        <pre>{{ invokeResult.outputJson }}</pre>
      </div>
    </Drawer>

    <Drawer v-model:open="invocationDrawerVisible" title="调用详情" width="760px">
      <template v-if="selectedInvocation">
        <div class="ai-mcp-page__detail">
          <div><span>Server</span><strong>{{ selectedInvocation.serverName }}</strong></div>
          <div><span>工具</span><strong>{{ selectedInvocation.toolName }}</strong></div>
          <div><span>状态</span><Tag :color="statusColor(selectedInvocation.status)">{{ selectedInvocation.status }}</Tag></div>
          <div><span>耗时</span><strong>{{ selectedInvocation.elapsedMilliseconds }} ms</strong></div>
          <div><span>调用时间</span><strong>{{ formatDate(selectedInvocation.creationTime) }}</strong></div>
          <div><span>超时阈值</span><strong>{{ selectedInvocation.timeoutSeconds }} 秒</strong></div>
        </div>
        <p v-if="selectedInvocation.errorMessage" class="ai-mcp-page__error">{{ selectedInvocation.errorMessage }}</p>
        <h4>输入</h4>
        <pre>{{ selectedInvocation.inputJson }}</pre>
        <h4>输出</h4>
        <pre>{{ selectedInvocation.outputJson }}</pre>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-mcp-page {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-mcp-page__summary {
  overflow: hidden;
}

.ai-mcp-page__summary-grid {
  display: grid;
  @apply gap-4;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.ai-mcp-page__metric {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  justify-content: center;
  min-width: 0;
  min-height: 72px;
}

.ai-mcp-page__metric span,
.ai-mcp-page__muted {
  color: var(--ant-color-text-tertiary);
}

.ai-mcp-page__metric span {
  overflow: hidden;
  line-height: 20px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-mcp-page__metric strong {
  color: var(--ant-color-text);
  font-size: 24px;
  line-height: 32px;
}

.ai-mcp-page__toolbar,
.ai-mcp-page__filters {
  display: flex;
  flex-wrap: wrap;
  @apply gap-3;
  justify-content: space-between;
  @apply mb-3;
}

.ai-mcp-page__filters {
  justify-content: flex-start;
}

.ai-mcp-page__select,
.ai-mcp-page__input {
  min-width: 220px;
}

.ai-mcp-page__number {
  width: 100%;
}

.ai-mcp-page__form-grid {
  display: grid;
  @apply gap-x-4;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.ai-mcp-page__error {
  color: var(--ant-color-error);
}

.ai-mcp-page__result {
  display: flex;
  flex-direction: column;
  @apply gap-3;
  @apply mt-4;
}

.ai-mcp-page__detail {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  @apply mb-4;
}

.ai-mcp-page__detail div {
  display: flex;
  flex-direction: column;
  @apply gap-1;
}

.ai-mcp-page__detail span {
  color: var(--ant-color-text-tertiary);
}

.ai-mcp-page pre {
  max-height: 360px;
  @apply m-0;
  overflow: auto;
  @apply p-3;
  white-space: pre-wrap;
  word-break: break-word;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

@media (max-width: 768px) {
  .ai-mcp-page__summary-grid,
  .ai-mcp-page__form-grid,
  .ai-mcp-page__detail {
    grid-template-columns: 1fr;
  }

  .ai-mcp-page__select,
  .ai-mcp-page__input {
    width: 100%;
  }
}
</style>
