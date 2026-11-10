<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Wrench } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';

type ToolsView = 'catalog' | 'invocations';

const api = createAiApi(apiClient);
const activeView = ref<ToolsView>('catalog');
const tools = ref<AiApi.ToolDefinition[]>([]);
const invocations = ref<AiApi.ToolInvocation[]>([]);
const loadingTools = ref(false);
const loadingInvocations = ref(false);
const toolError = ref('');
const invocationError = ref('');
const canUseConversation = computed(() => canAccess('Ai.Conversation.Use'));

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function makeListModel(
  key: string,
  label: string,
  description: string,
  path: string,
  rows: ListPageRow[],
  busy: boolean,
  error: string,
  columns: ListPageModel['columns'],
): ListPageModel {
  return {
    actions: [{ id: 'reload', kind: 'refresh', label: busy ? '读取中' : '刷新' }],
    columns,
    demoPath: '/ai/agent-design',
    description,
    emptyHint: busy ? `正在读取${label}…` : error ? `${label}读取失败，请刷新重试。` : `暂无${label}。`,
    filters: [],
    key,
    label,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: key.includes('工具目录') ? 'ai-tools' : 'ai-tool-invocations',
    pageSize: 20,
    panels: [],
    path,
    primaryAction: '',
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-agent-design',
    rows,
    searchPlaceholder: `搜索${label}名称、编码或状态`,
    stats: [{ key: 'total', label: `全部${label}`, value: String(rows.length), tone: 'primary' }],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const toolRows = computed<ListPageRow[]>(() =>
  tools.value.map((item) => ({
    category: item.category,
    id: item.id,
    name: item.name,
    permission: item.permissionCode || 'Ai.Conversation.Use',
    status: item.isEnabled ? '启用' : '停用',
    timeout: `${item.timeoutSeconds} 秒`,
    toolCode: item.toolCode,
  })),
);

const invocationRows = computed<ListPageRow[]>(() =>
  invocations.value.map((item) => ({
    conversationId: item.conversationId || '—',
    elapsed: `${item.elapsedMilliseconds} ms`,
    id: item.invocationId,
    status: item.status,
    time: Number.isNaN(Date.parse(item.creationTime))
      ? item.creationTime
      : new Date(item.creationTime).toLocaleString(),
    toolCode: item.toolCode,
  })),
);

const toolsPageModel = computed(() =>
  makeListModel(
    'AI管理|工具目录',
    '工具',
    '查看Agent和对话可用的工具、权限要求与超时配置。',
    '/ai/agent-design',
    toolRows.value,
    loadingTools.value,
    toolError.value,
    [
      { key: 'name', title: '工具名称', width: 220, sticky: true },
      { key: 'toolCode', title: '工具编码', width: 240 },
      { key: 'category', title: '分类', width: 140 },
      { key: 'permission', title: '所需权限', width: 220 },
      { key: 'timeout', title: '超时', width: 100 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

const invocationsPageModel = computed(() =>
  makeListModel(
    'AI管理|工具调用记录',
    '调用记录',
    '最近100条已保存的工具调用状态。',
    '/ai/agent-design',
    invocationRows.value,
    loadingInvocations.value,
    invocationError.value,
    [
      { key: 'time', title: '调用时间', width: 190, sticky: true },
      { key: 'toolCode', title: '工具编码', width: 240 },
      { key: 'status', title: '结果状态', width: 120, presentation: 'status' },
      { key: 'elapsed', title: '耗时', width: 120 },
      { key: 'conversationId', title: '会话 ID', width: 300 },
    ],
  ),
);

async function loadTools(): Promise<void> {
  loadingTools.value = true;
  toolError.value = '';
  try {
    tools.value = await api.getTools();
  } catch (error) {
    toolError.value = errorText(error);
  } finally {
    loadingTools.value = false;
  }
}

async function loadInvocations(): Promise<void> {
  loadingInvocations.value = true;
  invocationError.value = '';
  try {
    invocations.value = await api.getToolInvocations({ maxResultCount: 100 });
  } catch (error) {
    invocationError.value = errorText(error);
  } finally {
    loadingInvocations.value = false;
  }
}

function handleToolAction(action: PageAction): void {
  if (action.id === 'reload') void loadTools();
}

function handleInvocationAction(action: PageAction): void {
  if (action.id === 'reload') void loadInvocations();
}

onMounted(() => {
  if (!canUseConversation.value) return;
  void loadTools();
  void loadInvocations();
});
</script>

<template>
  <p
    v-if="!canUseConversation"
    role="alert"
  >
    当前账号没有AI会话使用权限。
  </p>
  <UiTabs
    v-else
    v-model="activeView"
  >
    <UiTabsList label="工具管理">
      <UiTabsTrigger value="catalog">工具目录</UiTabsTrigger>
      <UiTabsTrigger value="invocations">调用记录</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="catalog">
      <ListPageSurface
        :model="toolsPageModel"
        @action="handleToolAction"
      >
        <template #hero-icon
          ><Wrench
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>工具目录由后端注册并维护。本页不直接调用工具；会话执行时仍由服务端逐项检查权限。</p>
          <p
            v-if="toolError"
            role="alert"
          >
            {{ toolError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="invocations">
      <ListPageSurface
        :model="invocationsPageModel"
        @action="handleInvocationAction"
      >
        <template #hero-icon
          ><Wrench
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>显示最近100条调用的时间、工具、状态和耗时，不展示输入或输出正文。当前后端没有单独的审批记录接口。</p>
          <p
            v-if="invocationError"
            role="alert"
          >
            {{ invocationError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
  </UiTabs>
</template>
