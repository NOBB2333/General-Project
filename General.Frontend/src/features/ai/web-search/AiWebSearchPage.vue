<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Globe2 } from 'lucide-vue-next';
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
  UiSelect,
  UiSwitch,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
  type UiSelectOption,
} from '@/components/ui';

const api = createAiApi(apiClient);
const configs = ref<AiApi.WebSearchConfig[]>([]);
const records = ref<AiApi.WebSearchRecord[]>([]);
const activeTab = ref('configs');
const busy = ref(false);
const saving = ref(false);
const actionBusy = ref(false);
const loadError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const searchConfirmOpen = ref(false);
const recordDialogOpen = ref(false);
const editing = ref<AiApi.WebSearchConfig | null>(null);
const deleteTarget = ref<AiApi.WebSearchConfig | null>(null);
const recordTarget = ref<AiApi.WebSearchRecord | null>(null);
const formError = ref('');
const query = ref('');
const selectedConfigId = ref('');
const maxResults = ref(5);
const searchResult = ref<AiApi.WebSearchResult | null>(null);
const canManage = computed(() => canAccess('Ai.WebSearch.Manage'));

function blankConfig(): AiApi.WebSearchConfigSaveInput {
  return {
    apiKey: '',
    allowDomains: [],
    configJson: '{}',
    denyDomains: [],
    endpoint: '',
    isDefault: false,
    isEnabled: true,
    maxResults: 5,
    name: '',
    providerType: 'Tavily',
    remark: '',
    timeoutSeconds: 15,
  };
}

const draft = ref<AiApi.WebSearchConfigSaveInput>(blankConfig());
const configOptions = computed<UiSelectOption[]>(() => [
  { label: '使用系统默认配置', value: '' },
  ...configs.value.filter((item) => item.isEnabled).map((item) => ({ label: item.name, value: item.id })),
]);

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function formatTime(value: string): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

function formatDomains(values: string[]): string {
  return values.join('\n');
}

function parseDomains(value: string): string[] {
  return [
    ...new Set(
      value
        .split(/[\n,，]/)
        .map((item) => item.trim().toLowerCase())
        .filter(Boolean),
    ),
  ];
}

function isValidJson(value: string): boolean {
  try {
    JSON.parse(value);
    return true;
  } catch {
    return false;
  }
}

const configRows = computed<ListPageRow[]>(() =>
  configs.value.map((item) => ({
    endpoint: item.endpoint || '使用服务默认地址',
    hasApiKey: item.hasApiKey ? '已配置' : '未配置',
    id: item.id,
    isDefault: item.isDefault ? '默认' : '',
    maxResults: item.maxResults,
    name: item.name,
    providerType: item.providerType,
    status: item.isEnabled ? '启用' : '停用',
  })),
);

const recordRows = computed<ListPageRow[]>(() =>
  records.value.map((item) => ({
    creationTime: formatTime(item.creationTime),
    elapsedMilliseconds: item.elapsedMilliseconds,
    id: item.id,
    providerName: item.providerName,
    query: item.query,
    resultCount: item.resultCount,
    status: item.status,
  })),
);

function makeModel(rows: ListPageRow[], label: string, actions: PageAction[], columns: PageColumn[]): ListPageModel {
  return {
    actions,
    columns,
    demoPath: '/ai/web-search',
    description: label === '配置' ? '管理联网搜索供应商、密钥和域名范围。' : '查看真实搜索请求的状态、耗时和审计结果。',
    emptyHint: busy.value ? '正在读取数据…' : loadError.value || `暂无搜索${label}。`,
    filters: [],
    key: `AI管理|联网搜索|${label}`,
    label: `搜索${label}`,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-web-search',
    pageSize: 20,
    panels: [],
    path: '/ai/web-search',
    primaryAction: actions.find((action) => action.kind === 'create')?.label ?? '刷新',
    replication: { status: 'implemented', referenceUrl: '/ai/web-search' },
    routeName: 'ai-web-search',
    rows,
    searchPlaceholder: `搜索${label}、状态或内容`,
    stats: [
      { key: 'total', label: `全部${label}`, value: String(rows.length), tone: 'primary' },
      {
        key: 'enabled',
        label: '成功 / 启用',
        value: String(rows.filter((row) => ['Completed', '启用'].includes(String(row.status))).length),
        tone: 'success',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const configsModel = computed(() =>
  makeModel(
    configRows.value,
    '配置',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value
        ? [
            { id: 'create', kind: 'create' as const, label: '新增配置' },
            { id: 'edit', kind: 'custom' as const, label: '编辑配置', requiresSelection: true },
            { id: 'delete', kind: 'delete' as const, label: '删除配置', requiresSelection: true },
          ]
        : []),
    ],
    [
      { key: 'name', title: '配置名称', width: 220 },
      { key: 'providerType', title: '供应商', width: 150 },
      { key: 'endpoint', title: 'Endpoint', width: 280 },
      { key: 'hasApiKey', title: '密钥', width: 100 },
      { key: 'maxResults', title: '最大结果数', width: 110 },
      { key: 'isDefault', title: '默认配置', width: 100 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

const recordsModel = computed(() =>
  makeModel(
    recordRows.value,
    '记录',
    [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }],
    [
      { key: 'creationTime', title: '搜索时间', width: 185 },
      { key: 'query', title: '查询词', width: 300 },
      { key: 'providerName', title: '供应商', width: 160 },
      { key: 'resultCount', title: '结果数', width: 90 },
      { key: 'elapsedMilliseconds', title: '耗时（毫秒）', width: 130 },
      { key: 'status', title: '状态', width: 110, presentation: 'status' },
    ],
  ),
);

async function load(): Promise<void> {
  busy.value = true;
  loadError.value = '';
  try {
    const [configResult, recordResult] = await Promise.all([
      api.getWebSearchConfigs(true),
      api.getWebSearchRecords({ maxResultCount: 200 }),
    ]);
    configs.value = configResult;
    records.value = recordResult;
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function openForm(item: AiApi.WebSearchConfig | null): void {
  editing.value = item;
  draft.value = item
    ? {
        apiKey: '',
        allowDomains: [...item.allowDomains],
        configJson: item.configJson,
        denyDomains: [...item.denyDomains],
        endpoint: item.endpoint,
        isDefault: item.isDefault,
        isEnabled: item.isEnabled,
        maxResults: item.maxResults,
        name: item.name,
        providerType: item.providerType,
        remark: item.remark,
        timeoutSeconds: item.timeoutSeconds,
      }
    : blankConfig();
  formError.value = '';
  dialogOpen.value = true;
}

async function saveConfig(): Promise<void> {
  if (!canManage.value) return;
  if (!isValidJson(draft.value.configJson)) {
    formError.value = 'Config 必须是合法 JSON。';
    return;
  }
  if (draft.value.endpoint.trim()) {
    try {
      const endpoint = new URL(draft.value.endpoint);
      if (!['http:', 'https:'].includes(endpoint.protocol)) throw new Error('scheme');
    } catch {
      formError.value = 'Endpoint 必须是合法的 HTTP 或 HTTPS 地址。';
      return;
    }
  }
  saving.value = true;
  formError.value = '';
  try {
    if (editing.value) await api.updateWebSearchConfig(editing.value.id, draft.value);
    else await api.createWebSearchConfig(draft.value);
    dialogOpen.value = false;
    await load();
    toast.success(editing.value ? '联网搜索配置已更新。' : '联网搜索配置已创建。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

function handleConfigAction(action: PageAction, rows: ListPageRow[]): void {
  if (action.id === 'reload') void load();
  if (action.id === 'create' && canManage.value) openForm(null);
  const item = configs.value.find((config) => config.id === rows[0]?.id);
  if (!item || !canManage.value) return;
  if (action.id === 'edit') openForm(item);
  if (action.id === 'delete') {
    deleteTarget.value = item;
    deleteDialogOpen.value = true;
  }
}

async function confirmDelete(): Promise<void> {
  if (!deleteTarget.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    await api.deleteWebSearchConfig(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('搜索配置已删除；历史搜索记录保留。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

function requestSearch(): void {
  if (!query.value.trim() || !canManage.value) return;
  searchConfirmOpen.value = true;
}

async function confirmSearch(): Promise<void> {
  if (!query.value.trim() || !canManage.value) return;
  actionBusy.value = true;
  searchResult.value = null;
  try {
    const result = await api.runWebSearch({
      configId: selectedConfigId.value || null,
      conversationId: null,
      maxResults: maxResults.value,
      messageId: null,
      query: query.value.trim(),
    });
    searchResult.value = result;
    searchConfirmOpen.value = false;
    if (result.status === 'Completed') toast.success(`联网搜索完成：返回 ${result.resultCount} 条结果。`);
    else toast.error(result.errorMessage || '联网搜索失败，详情已写入搜索记录。');
    await load();
    activeTab.value = 'search';
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

function openRecord(row: ListPageRow): void {
  const record = records.value.find((item) => item.id === row.id);
  if (!record) return;
  recordTarget.value = record;
  recordDialogOpen.value = true;
}

onMounted(() => void load());
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="联网搜索管理">
      <UiTabsTrigger value="configs">服务配置</UiTabsTrigger>
      <UiTabsTrigger value="search">搜索测试</UiTabsTrigger>
      <UiTabsTrigger value="records">搜索记录</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="configs">
      <ListPageSurface
        :model="configsModel"
        @action="handleConfigAction"
      >
        <template #hero-icon
          ><Globe2
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>API Key 在后端加密保存；编辑时留空会保留现有密钥。域名白名单用于限制服务 Endpoint 主机。</p>
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
            当前账号没有联网搜索管理权限。
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="search">
      <section>
        <UiFormField label="搜索服务"
          ><UiSelect
            v-model="selectedConfigId"
            label="搜索服务"
            :options="configOptions"
        /></UiFormField>
        <UiFormField label="查询内容"
          ><UiTextarea
            v-model="query"
            rows="3"
            maxlength="512"
            placeholder="输入要搜索的问题或关键词"
        /></UiFormField>
        <UiFormField label="结果条数"
          ><UiInput
            :model-value="String(maxResults)"
            type="number"
            min="1"
            max="50"
            @update:model-value="maxResults = Number($event)"
        /></UiFormField>
        <UiButton
          variant="primary"
          :disabled="actionBusy || !query.trim() || !canManage"
          @click="requestSearch"
          >搜索</UiButton
        >
        <p>搜索会向所选服务发送查询词并在后端保存请求记录。第三方响应按纯文本展示，不会按 HTML 执行。</p>
        <p
          v-if="searchResult"
          role="status"
        >
          状态：{{ searchResult.status }}；结果数：{{ searchResult.resultCount }}；记录 ID：{{ searchResult.recordId }}
        </p>
        <p
          v-if="searchResult?.errorMessage"
          role="alert"
        >
          {{ searchResult.errorMessage }}
        </p>
        <UiFormField
          v-if="searchResult"
          label="第三方原始响应（纯文本）"
          wide
        >
          <UiTextarea
            :model-value="searchResult.responseSummaryJson"
            rows="14"
            readonly
          />
        </UiFormField>
      </section>
    </UiTabPanel>
    <UiTabPanel value="records">
      <ListPageSurface
        :model="recordsModel"
        @action="(action) => action.id === 'reload' && load()"
        @row="openRecord"
      >
        <template #notice>
          <p>搜索记录可能含查询词与第三方返回内容。仅显示脱敏以外的状态摘要，详情按纯文本显示。</p>
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
    :title="editing ? `编辑搜索配置：${editing.name}` : '新增搜索配置'"
    description="服务 Endpoint 和域名策略会在后端执行校验。密钥不会在页面回显。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="saveConfig">
      <UiFormField label="名称"
        ><UiInput
          v-model="draft.name"
          maxlength="128"
          required
      /></UiFormField>
      <UiFormField label="供应商类型"
        ><UiInput
          v-model="draft.providerType"
          maxlength="64"
          placeholder="BingWebSearch / SerpApi / Tavily / Custom"
          required
      /></UiFormField>
      <UiFormField label="Endpoint"
        ><UiInput
          v-model="draft.endpoint"
          maxlength="512"
          placeholder="内置供应商可使用默认地址"
      /></UiFormField>
      <UiFormField
        label="API Key"
        :hint="editing?.hasApiKey ? '当前已配置密钥；留空保留现有密钥，填写新值可替换。' : '密钥由后端加密保存。'"
      >
        <UiInput
          v-model="draft.apiKey"
          type="password"
          maxlength="8000"
          autocomplete="new-password"
        />
      </UiFormField>
      <UiFormField
        label="允许域名"
        hint="每行一个域名；为空时不启用白名单。"
        ><UiTextarea
          :model-value="formatDomains(draft.allowDomains)"
          rows="3"
          @update:model-value="draft.allowDomains = parseDomains($event)"
      /></UiFormField>
      <UiFormField label="拒绝域名"
        ><UiTextarea
          :model-value="formatDomains(draft.denyDomains)"
          rows="3"
          @update:model-value="draft.denyDomains = parseDomains($event)"
      /></UiFormField>
      <UiFormField label="最大结果数"
        ><UiInput
          :model-value="String(draft.maxResults)"
          type="number"
          min="1"
          max="50"
          @update:model-value="draft.maxResults = Number($event)"
      /></UiFormField>
      <UiFormField label="超时秒数"
        ><UiInput
          :model-value="String(draft.timeoutSeconds)"
          type="number"
          min="1"
          max="300"
          @update:model-value="draft.timeoutSeconds = Number($event)"
      /></UiFormField>
      <UiFormField label="默认配置"
        ><UiSwitch
          v-model="draft.isDefault"
          label="作为默认搜索服务"
      /></UiFormField>
      <UiFormField label="配置状态"
        ><UiSwitch
          v-model="draft.isEnabled"
          label="启用搜索服务"
      /></UiFormField>
      <UiFormField
        label="供应商 Config JSON"
        wide
        ><UiTextarea
          v-model="draft.configJson"
          rows="4"
          maxlength="8000"
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
        @click="saveConfig"
        >{{ saving ? '保存中…' : '保存配置' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="recordDialogOpen"
    :title="`搜索记录：${recordTarget?.query ?? ''}`"
    description="请求与响应 JSON 以纯文本显示，不会按 HTML 执行。"
    size="lg"
    @update:open="recordDialogOpen = $event"
  >
    <UiFormField label="状态"
      ><UiInput
        :model-value="recordTarget?.status ?? ''"
        readonly
    /></UiFormField>
    <UiFormField
      label="请求摘要"
      wide
      ><UiTextarea
        :model-value="recordTarget?.requestJson ?? ''"
        rows="4"
        readonly
    /></UiFormField>
    <UiFormField
      label="响应摘要"
      wide
      ><UiTextarea
        :model-value="recordTarget?.responseSummaryJson ?? ''"
        rows="12"
        readonly
    /></UiFormField>
    <p
      v-if="recordTarget?.errorMessage"
      role="alert"
    >
      {{ recordTarget.errorMessage }}
    </p>
  </UiDialog>

  <UiAlertDialog
    :open="searchConfirmOpen"
    title="确认发起联网搜索"
    :description="`查询词“${query.trim()}”将发送至所选搜索服务，并写入搜索记录。`"
    confirm-label="确认搜索"
    tone="warning"
    @update:open="searchConfirmOpen = $event"
    @confirm="confirmSearch"
  />
  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除搜索配置"
    :description="deleteTarget ? `删除“${deleteTarget.name}”后不可恢复；历史搜索记录保留。` : ''"
    confirm-label="删除配置"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
