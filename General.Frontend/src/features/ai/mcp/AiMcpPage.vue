<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { PlugZap } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { PageColumn } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import {
  toast,
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSwitch,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
} from '@/components/ui';

const api = createAiApi(apiClient);
const servers = ref<AiApi.McpServer[]>([]);
const tools = ref<AiApi.McpTool[]>([]);
const invocations = ref<AiApi.McpInvocation[]>([]);
const activeTab = ref('servers');
const busy = ref(false);
const saving = ref(false);
const loadError = ref('');
const dialogOpen = ref(false);
const policyDialogOpen = ref(false);
const invocationDialogOpen = ref(false);
const confirmOpen = ref(false);
const deleteOpen = ref(false);
const editingServer = ref<AiApi.McpServer | null>(null);
const policyTool = ref<AiApi.McpTool | null>(null);
const invocationTool = ref<AiApi.McpTool | null>(null);
const deleteServer = ref<AiApi.McpServer | null>(null);
const externalAction = ref<'discover' | 'test' | null>(null);
const selectedServer = ref<AiApi.McpServer | null>(null);
const actionBusy = ref(false);
const formError = ref('');
const argsError = ref('');
const invocationArgs = ref('{}');
const canManage = computed(() => canAccess('Ai.Mcp.Manage'));

function blankServer(): AiApi.McpServerSaveInput {
  return {
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
  };
}

const draft = ref<AiApi.McpServerSaveInput>(blankServer());
const policyDraft = ref<AiApi.McpToolPolicyInput>({ isEnabled: true, permissionCode: '' });

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function parseJson(value: string, expected: 'array' | 'object'): boolean {
  try {
    const parsed: unknown = JSON.parse(value);
    return expected === 'array'
      ? Array.isArray(parsed)
      : typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed);
  } catch {
    return false;
  }
}

const serverRows = computed<ListPageRow[]>(() =>
  servers.value.map((item) => ({
    endpoint: item.transportType === 'Stdio' ? item.command : item.endpoint,
    id: item.id,
    name: item.name,
    serverCode: item.serverCode,
    status: item.isEnabled ? '启用' : '停用',
    toolCount: item.toolCount,
    transportType: item.transportType,
  })),
);

const toolRows = computed<ListPageRow[]>(() =>
  tools.value.map((item) => ({
    description: item.description,
    id: item.id,
    permissionCode: item.permissionCode || '继承 Server 权限',
    serverName: item.serverName,
    status: item.isEnabled ? '启用' : '停用',
    toolName: item.toolName,
  })),
);

const invocationRows = computed<ListPageRow[]>(() =>
  invocations.value.map((item) => ({
    creationTime: formatTime(item.creationTime),
    elapsedMilliseconds: item.elapsedMilliseconds,
    id: item.invocationId,
    serverName: item.serverName,
    status: item.status,
    toolName: item.toolName,
  })),
);

function pageModel(
  rows: ListPageRow[],
  label: string,
  path: string,
  actions: PageAction[],
  columns: PageColumn[],
): ListPageModel {
  return {
    actions,
    columns,
    demoPath: path,
    description: '维护 MCP Server、工具权限策略和真实调用审计记录。连接测试、工具发现与调用会访问对应外部服务。',
    emptyHint: busy.value ? '正在读取数据…' : loadError.value || `暂无${label}。`,
    filters: [],
    key: `AI管理|MCP|${label}`,
    label,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-mcp',
    pageSize: 20,
    panels: [],
    path,
    primaryAction: actions.find((action) => action.kind === 'create')?.label ?? '刷新',
    replication: { status: 'implemented', referenceUrl: '/ai/mcp' },
    routeName: 'ai-mcp',
    rows,
    searchPlaceholder: `搜索${label}`,
    stats: [
      { key: 'total', label: `全部${label}`, value: String(rows.length), tone: 'primary' },
      {
        key: 'enabled',
        label: '启用',
        value: String(rows.filter((row) => row.status === '启用').length),
        tone: 'success',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const serversModel = computed(() =>
  pageModel(
    serverRows.value,
    'Server',
    '/ai/mcp',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value
        ? [
            { id: 'create', kind: 'create' as const, label: '新增 Server' },
            { id: 'edit', kind: 'custom' as const, label: '编辑 Server', requiresSelection: true },
            { id: 'test', kind: 'custom' as const, label: '测试连接', requiresSelection: true },
            { id: 'discover', kind: 'custom' as const, label: '发现工具', requiresSelection: true },
            { id: 'delete', kind: 'delete' as const, label: '删除 Server', requiresSelection: true },
          ]
        : []),
    ],
    [
      { key: 'name', title: 'Server 名称', width: 210 },
      { key: 'serverCode', title: '编码', width: 170 },
      { key: 'transportType', title: '传输类型', width: 130 },
      { key: 'endpoint', title: '连接地址 / 命令', width: 260 },
      { key: 'toolCount', title: '工具数', width: 90 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

const toolsModel = computed(() =>
  pageModel(
    toolRows.value,
    '工具',
    '/ai/mcp',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value
        ? [
            { id: 'policy', kind: 'custom' as const, label: '配置策略', requiresSelection: true },
            { id: 'invoke', kind: 'custom' as const, label: '调用工具', requiresSelection: true },
          ]
        : []),
    ],
    [
      { key: 'toolName', title: '工具名称', width: 220 },
      { key: 'serverName', title: 'MCP Server', width: 180 },
      { key: 'description', title: '说明', width: 300 },
      { key: 'permissionCode', title: '权限码', width: 190 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

const invocationsModel = computed(() =>
  pageModel(
    invocationRows.value,
    '调用记录',
    '/ai/mcp',
    [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }],
    [
      { key: 'creationTime', title: '调用时间', width: 180 },
      { key: 'serverName', title: 'MCP Server', width: 180 },
      { key: 'toolName', title: '工具', width: 180 },
      { key: 'status', title: '结果', width: 120, presentation: 'status' },
      { key: 'elapsedMilliseconds', title: '耗时（毫秒）', width: 130 },
    ],
  ),
);

function formatTime(value: string): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

async function load(): Promise<void> {
  busy.value = true;
  loadError.value = '';
  try {
    const [serverResult, toolResult, invocationResult] = await Promise.all([
      api.getMcpServers(),
      api.getMcpTools(),
      api.getMcpInvocations({ maxResultCount: 100 }),
    ]);
    servers.value = serverResult;
    tools.value = toolResult;
    invocations.value = invocationResult;
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function selected<T extends { id: string }>(rows: ListPageRow[], collection: T[]): T | null {
  const id = rows[0]?.id;
  return id ? (collection.find((item) => item.id === id) ?? null) : null;
}

function openServerForm(item: AiApi.McpServer | null): void {
  editingServer.value = item;
  draft.value = item
    ? {
        argumentsJson: '',
        command: item.command,
        configJson: '',
        endpoint: item.endpoint,
        environmentJson: '',
        headersJson: '',
        isEnabled: item.isEnabled,
        name: item.name,
        permissionCode: item.permissionCode,
        protocolVersion: item.protocolVersion,
        remark: item.remark,
        serverCode: item.serverCode,
        sort: item.sort,
        timeoutSeconds: item.timeoutSeconds,
        transportType: item.transportType,
      }
    : blankServer();
  formError.value = '';
  dialogOpen.value = true;
}

async function saveServer(): Promise<void> {
  if (!canManage.value) return;
  if (
    (Boolean(draft.value.argumentsJson.trim()) && !parseJson(draft.value.argumentsJson, 'array')) ||
    (Boolean(draft.value.headersJson.trim()) && !parseJson(draft.value.headersJson, 'object')) ||
    (Boolean(draft.value.environmentJson.trim()) && !parseJson(draft.value.environmentJson, 'object')) ||
    (Boolean(draft.value.configJson.trim()) && !parseJson(draft.value.configJson, 'object'))
  ) {
    formError.value = 'Arguments 必须为 JSON 数组，Headers、Environment、Config 必须为 JSON 对象。';
    return;
  }
  if (draft.value.transportType === 'Stdio' ? !draft.value.command.trim() : !draft.value.endpoint.trim()) {
    formError.value =
      draft.value.transportType === 'Stdio' ? 'stdio Server 必须填写启动命令。' : 'HTTP/SSE Server 必须填写 Endpoint。';
    return;
  }
  saving.value = true;
  formError.value = '';
  try {
    if (editingServer.value) await api.updateMcpServer(editingServer.value.id, draft.value);
    else await api.createMcpServer(draft.value);
    dialogOpen.value = false;
    await load();
    toast.success(editingServer.value ? 'MCP Server 已更新。' : 'MCP Server 已创建。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

function handleServerAction(action: PageAction, rows: ListPageRow[]): void {
  if (action.id === 'reload') void load();
  if (action.id === 'create' && canManage.value) openServerForm(null);
  const item = selected(rows, servers.value);
  if (action.id === 'edit' && item && canManage.value) openServerForm(item);
  if (action.id === 'delete' && item && canManage.value) {
    deleteServer.value = item;
    deleteOpen.value = true;
  }
  if ((action.id === 'test' || action.id === 'discover') && item && canManage.value) {
    selectedServer.value = item;
    externalAction.value = action.id;
    confirmOpen.value = true;
  }
}

function handleToolAction(action: PageAction, rows: ListPageRow[]): void {
  if (action.id === 'reload') void load();
  const item = selected(rows, tools.value);
  if (!item || !canManage.value) return;
  if (action.id === 'policy') {
    policyTool.value = item;
    policyDraft.value = { isEnabled: item.isEnabled, permissionCode: item.permissionCode };
    policyDialogOpen.value = true;
  }
  if (action.id === 'invoke') {
    invocationTool.value = item;
    invocationArgs.value = '{}';
    argsError.value = '';
    invocationDialogOpen.value = true;
  }
}

async function savePolicy(): Promise<void> {
  if (!policyTool.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    await api.updateMcpToolPolicy(policyTool.value.id, policyDraft.value);
    policyDialogOpen.value = false;
    await load();
    toast.success('MCP 工具策略已保存。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

async function runExternalAction(): Promise<void> {
  if (!selectedServer.value || !externalAction.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    if (externalAction.value === 'test') {
      const result = await api.testMcpServer(selectedServer.value.id);
      if (result.success) toast.success('MCP Server 连接测试成功。');
      else toast.error(result.errorMessage || 'MCP Server 连接测试失败。');
    } else {
      const result = await api.discoverMcpTools(selectedServer.value.id);
      toast.success(`工具发现完成：读取 ${result.toolCount} 项。`);
      if (result.errorMessage) toast.error(result.errorMessage);
    }
    await load();
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
    confirmOpen.value = false;
    externalAction.value = null;
    selectedServer.value = null;
  }
}

async function runInvocation(): Promise<void> {
  if (!invocationTool.value || !canManage.value) return;
  if (!parseJson(invocationArgs.value, 'object')) {
    argsError.value = '参数必须是合法的 JSON 对象。';
    return;
  }
  argsError.value = '';
  actionBusy.value = true;
  try {
    const result = await api.invokeMcpTool(invocationTool.value.id, { argumentsJson: invocationArgs.value });
    if (result.status === 'Completed') toast.success(`工具调用已完成，记录编号 ${result.invocationId}。`);
    else toast.error(result.errorMessage || `工具调用状态：${result.status}`);
    invocationDialogOpen.value = false;
    await load();
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

async function confirmDelete(): Promise<void> {
  if (!deleteServer.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    await api.deleteMcpServer(deleteServer.value.id);
    deleteOpen.value = false;
    deleteServer.value = null;
    await load();
    toast.success('MCP Server 及其工具配置已删除。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

onMounted(() => void load());
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="MCP 管理">
      <UiTabsTrigger value="servers">Server</UiTabsTrigger>
      <UiTabsTrigger value="tools">工具策略</UiTabsTrigger>
      <UiTabsTrigger value="invocations">调用记录</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="servers">
      <ListPageSurface
        :model="serversModel"
        @action="handleServerAction"
      >
        <template #hero-icon
          ><PlugZap
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>连接测试和工具发现会连接配置的 Server。调用工具会真实执行远端操作，并记录调用审计。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
          <p
            v-if="!canManage"
            role="alert"
          >
            当前账号没有 MCP 管理权限。
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="tools">
      <ListPageSurface
        :model="toolsModel"
        @action="handleToolAction"
      >
        <template #notice>
          <p>工具输入 Schema 由 MCP Server 提供。只启用经过确认的工具，并按实际数据访问范围设置权限码。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="invocations">
      <ListPageSurface
        :model="invocationsModel"
        @action="(action) => action.id === 'reload' && load()"
      >
        <template #notice>
          <p>此处只显示调用状态、来源与耗时；输入和输出可能包含业务数据，不在列表中展开。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
  </UiTabs>

  <UiDialog
    :open="dialogOpen"
    :title="editingServer ? `编辑 MCP Server：${editingServer.name}` : '新增 MCP Server'"
    description="Server 配置按管理权限访问，请勿填写与业务无关的敏感信息。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="saveServer">
      <UiFormField label="名称"
        ><UiInput
          v-model="draft.name"
          maxlength="128"
          required
      /></UiFormField>
      <UiFormField label="Server 编码"
        ><UiInput
          v-model="draft.serverCode"
          maxlength="64"
          required
          :readonly="Boolean(editingServer)"
      /></UiFormField>
      <UiFormField label="传输类型"
        ><UiInput
          v-model="draft.transportType"
          placeholder="StreamableHttp / Sse / Stdio"
          required
      /></UiFormField>
      <UiFormField label="Endpoint"
        ><UiInput
          v-model="draft.endpoint"
          maxlength="512"
          placeholder="HTTP/SSE Server 地址"
      /></UiFormField>
      <UiFormField label="启动命令"
        ><UiInput
          v-model="draft.command"
          maxlength="512"
          placeholder="stdio Server 命令"
      /></UiFormField>
      <UiFormField
        label="Arguments JSON"
        :hint="
          editingServer?.hasArguments ? '已有启动参数已隐藏；留空保留，填写 [] 可清空。' : '启动参数按 JSON 数组填写。'
        "
        ><UiTextarea
          v-model="draft.argumentsJson"
          rows="3"
          maxlength="16000"
      /></UiFormField>
      <UiFormField
        label="Headers JSON"
        :hint="
          editingServer?.hasHeaders
            ? '已有请求头已隐藏；留空保留，填写 {} 可清空。'
            : 'HTTP 请求头；只发送到受信任的 MCP Server。'
        "
        ><UiTextarea
          v-model="draft.headersJson"
          rows="3"
          maxlength="16000"
      /></UiFormField>
      <UiFormField
        label="Environment JSON"
        :hint="
          editingServer?.hasEnvironment ? '已有环境变量已隐藏；留空保留，填写 {} 可清空。' : 'stdio 进程环境变量。'
        "
        ><UiTextarea
          v-model="draft.environmentJson"
          rows="3"
          maxlength="16000"
      /></UiFormField>
      <UiFormField
        label="Config JSON"
        :hint="
          editingServer?.hasConfig ? '已有扩展配置已隐藏；留空保留，填写 {} 可清空。' : '扩展配置按 JSON 对象填写。'
        "
        ><UiTextarea
          v-model="draft.configJson"
          rows="3"
          maxlength="16000"
      /></UiFormField>
      <UiFormField label="协议版本"
        ><UiInput
          v-model="draft.protocolVersion"
          maxlength="32"
      /></UiFormField>
      <UiFormField label="默认权限码"
        ><UiInput
          v-model="draft.permissionCode"
          maxlength="128"
      /></UiFormField>
      <UiFormField label="超时秒数"
        ><UiInput
          :model-value="String(draft.timeoutSeconds)"
          type="number"
          min="1"
          max="300"
          @update:model-value="draft.timeoutSeconds = Number($event)"
      /></UiFormField>
      <UiFormField label="排序"
        ><UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
      /></UiFormField>
      <UiFormField label="Server 状态"
        ><UiSwitch
          v-model="draft.isEnabled"
          label="启用 Server"
      /></UiFormField>
      <UiFormField
        label="备注"
        wide
        ><UiTextarea
          v-model="draft.remark"
          rows="2"
          maxlength="512"
      /></UiFormField>
      <p
        v-if="formError"
        role="alert"
      >
        {{ formError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="saveServer"
        >{{ saving ? '保存中…' : '保存 Server' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="policyDialogOpen"
    :title="policyTool ? `工具策略：${policyTool.toolName}` : '工具策略'"
    description="权限码用于后端调用授权；留空时继承 Server 权限。"
    @update:open="policyDialogOpen = $event"
  >
    <UiFormField label="工具状态"
      ><UiSwitch
        v-model="policyDraft.isEnabled"
        label="启用此工具"
    /></UiFormField>
    <UiFormField label="权限码"
      ><UiInput
        v-model="policyDraft.permissionCode"
        maxlength="128"
        placeholder="留空则继承 Server 权限"
    /></UiFormField>
    <template #footer>
      <UiButton
        :disabled="actionBusy"
        @click="policyDialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="actionBusy"
        @click="savePolicy"
        >保存策略</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="invocationDialogOpen"
    :title="invocationTool ? `调用 MCP 工具：${invocationTool.toolName}` : '调用 MCP 工具'"
    description="确认参数后将立即访问 MCP Server 并执行操作。调用结果会记录在审计日志中。"
    size="lg"
    @update:open="invocationDialogOpen = $event"
  >
    <UiFormField label="调用参数 JSON"
      ><UiTextarea
        v-model="invocationArgs"
        rows="8"
        maxlength="16000"
    /></UiFormField>
    <p
      v-if="argsError"
      role="alert"
    >
      {{ argsError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="actionBusy"
        @click="invocationDialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="actionBusy || !invocationTool?.isEnabled"
        @click="runInvocation"
        >{{ actionBusy ? '执行中…' : '确认并执行' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="confirmOpen"
    title="确认连接 MCP Server"
    :description="
      selectedServer
        ? `${externalAction === 'test' ? '连接测试' : '工具发现'}将访问 ${selectedServer.name}（${selectedServer.endpoint || selectedServer.command}）。`
        : ''
    "
    confirm-label="继续连接"
    @update:open="confirmOpen = $event"
    @confirm="runExternalAction"
    @cancel="
      selectedServer = null;
      externalAction = null;
    "
  />
  <UiAlertDialog
    :open="deleteOpen"
    title="确认删除 MCP Server"
    :description="deleteServer ? `删除 ${deleteServer.name} 会同时删除该 Server 下的工具策略；历史调用记录保留。` : ''"
    confirm-label="删除 Server"
    @update:open="deleteOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteServer = null"
  />
</template>
