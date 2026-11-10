<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { Monitor } from 'lucide-vue-next';
import { createSystemMonitorApi, type SystemMonitorApi } from '@/api/core/system-monitor';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel } from '@/components/page/families/list/list-page.types';
import { UiDialog } from '@/components/ui';
import { routeNames } from '@/router/route.types';
import { canAccess } from '@/directives/permission';
import { formatMonitorNumber, formatMonitoringDate, toMonitorRows, type MonitorRow } from './monitoring.model';

const api = createSystemMonitorApi(apiClient);
const snapshot = ref<SystemMonitorApi.Snapshot | null>(null);
const loading = ref(false);
const error = ref('');
const readAt = ref('');
const selected = ref<MonitorRow | null>(null);
const open = ref(false);
let request: AbortController | undefined;
let version = 0;
const allowed = computed(() => canAccess('Platform.SystemMonitor.View'));
const rows = computed(() => (snapshot.value ? toMonitorRows(snapshot.value) : []));
const model = computed<ListPageModel<MonitorRow>>(() => ({
  actions: [{ id: 'refresh', label: loading.value ? '刷新中' : '刷新监控', kind: 'refresh' }],
  columns: [
    { key: 'name', title: '监控项目', width: 400 },
    { key: 'group', title: '分类', width: 400 },
    { key: 'value', title: '当前快照值', width: 400 },
  ],
  demoPath: '/platform/system-monitor',
  path: '/platform/system-monitor',
  key: '平台中心|系统监控',
  label: '系统监控',
  description: '当前后端运行节点的监控快照。点击项目查看完整信息和采样说明。',
  emptyHint: loading.value ? '正在采集监控快照…' : error.value ? '采集失败，请刷新重试。' : '暂无匹配的监控项目。',
  filters: [{ key: 'group', label: '监控分类', options: ['全部分类', ...new Set(rows.value.map((row) => row.group))] }],
  listScopes: [{ id: 'all', label: '运行节点快照' }],
  listTabs: [{ id: 'all', label: '全部监控项目' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'system-monitor',
  pageSize: 20,
  panels: [],
  primaryAction: '刷新监控',
  replication: { status: 'implemented', referenceUrl: '/platform/system-monitor' },
  routeName: routeNames.platformSystemMonitor,
  rows: rows.value,
  searchPlaceholder: '搜索监控项目 / 分类',
  stats: snapshot.value
    ? [
        {
          key: 'cpu',
          label: '系统处理器使用率',
          value: formatMonitorNumber(snapshot.value.systemCpuUsagePercent, '%'),
          tone: 'primary',
        },
        {
          key: 'memory',
          label: '系统内存使用率',
          value: formatMonitorNumber(snapshot.value.systemMemoryUsagePercent, '%'),
          tone: 'neutral',
        },
        {
          key: 'uptime',
          label: '进程运行分钟',
          value: formatMonitorNumber(snapshot.value.uptimeMinutes),
          tone: 'neutral',
        },
      ]
    : [],
  surface: 'list',
  tabs: [],
  tools: [],
}));

async function load() {
  request?.abort();
  const controller = new AbortController();
  request = controller;
  const currentVersion = ++version;
  loading.value = true;
  error.value = '';
  snapshot.value = null;
  readAt.value = '';
  selected.value = null;
  open.value = false;
  try {
    const result = await api.getSnapshot(controller.signal);
    if (currentVersion === version) {
      snapshot.value = result;
      readAt.value = formatMonitoringDate(new Date().toISOString());
    }
  } catch (reason) {
    if (currentVersion === version && !(reason instanceof Error && reason.name === 'AbortError'))
      error.value = reason instanceof Error ? reason.message : '监控采集失败，请刷新重试。';
  } finally {
    if (currentVersion === version) loading.value = false;
  }
}

onMounted(() => {
  if (allowed.value) void load();
});
onBeforeUnmount(() => request?.abort());
</script>

<template>
  <ListPageSurface
    v-if="allowed"
    :model="model"
    @action="(action) => action.id === 'refresh' && load()"
    @row="
      (row) => {
        selected = row;
        open = true;
      }
    "
  >
    <template #hero-icon><Monitor :size="20" /></template>
    <template #notice
      ><p v-if="readAt">读取时间：{{ readAt }}。点击刷新获取最新快照。</p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p></template
    >
  </ListPageSurface>
  <UiDialog
    v-model:open="open"
    :title="selected?.name || '监控项目详情'"
    description="当前快照的完整值与服务端采样口径。"
  >
    <dl v-if="selected">
      <dt>分类</dt>
      <dd>{{ selected.group }}</dd>
      <dt>当前值</dt>
      <dd>{{ selected.fullValue }}</dd>
      <dt>说明</dt>
      <dd>{{ selected.note || '来自当前后端运行节点快照。' }}</dd>
      <dt>读取时间</dt>
      <dd>{{ readAt }}</dd>
    </dl>
  </UiDialog>
</template>
