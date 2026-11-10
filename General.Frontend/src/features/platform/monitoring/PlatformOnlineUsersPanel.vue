<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { UsersRound } from 'lucide-vue-next';
import { createOnlineUserApi, type OnlineUserApi } from '@/api/core/online-user';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel } from '@/components/page/families/list/list-page.types';
import { toast } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { sessionState, clearSession } from '@/session/session-store';
import { hasCurrentTenantScope } from '@/session/token-storage';
import { routeNames } from '@/router/route.types';
import { isOwnOnlineAccount, toOnlineUserRow, type OnlineUserRow } from './monitoring.model';
import OnlineAccountDialog from './OnlineAccountDialog.vue';

const router = useRouter();
const api = createOnlineUserApi(apiClient);
const items = ref<OnlineUserApi.Item[]>([]);
const loading = ref(false);
const error = ref('');
const selected = ref<OnlineUserApi.Item | null>(null);
const open = ref(false);
const busy = ref(false);
const revokeError = ref('');
let request: AbortController | undefined;
let mutation: AbortController | undefined;
let version = 0;
const canManage = computed(() => canAccess('Platform.User.Manage'));
const ownAccount = computed(() =>
  Boolean(
    selected.value && isOwnOnlineAccount(selected.value, sessionState.user?.username ?? '', hasCurrentTenantScope()),
  ),
);
const rows = computed(() => items.value.map(toOnlineUserRow));
const model = computed<ListPageModel<OnlineUserRow>>(() => ({
  actions: [{ id: 'refresh', label: loading.value ? '刷新中' : '刷新在线账号', kind: 'refresh' }],
  columns: [
    { key: 'name', title: '账号', width: 240 },
    { key: 'tenant', title: '租户', width: 240 },
    { key: 'device', title: '设备', width: 240 },
    { key: 'browser', title: '浏览器', width: 240 },
    { key: 'lastAccess', title: '最近访问', width: 240 },
  ],
  demoPath: '/platform/users',
  path: '/platform/users',
  key: '平台中心|在线用户',
  label: '在线用户',
  description: '显示最近20分钟有访问记录的账号。点击账号查看详情并执行强制退出。',
  emptyHint: loading.value ? '正在加载在线账号…' : error.value ? '加载失败，请刷新重试。' : '暂无匹配的在线账号。',
  filters: [
    { key: 'device', label: '设备类型', options: ['全部设备', ...new Set(rows.value.map((row) => row.device))] },
  ],
  listScopes: [{ id: 'all', label: '可管理账号' }],
  listTabs: [{ id: 'all', label: '全部账号' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'online-users',
  pageSize: 20,
  panels: [],
  primaryAction: '刷新在线账号',
  replication: { status: 'implemented', referenceUrl: '/platform/users' },
  routeName: routeNames.platformUsers,
  rows: rows.value,
  searchPlaceholder: '搜索账号 / 租户 / 设备',
  stats: [
    { key: 'online', label: '活跃账号', value: String(items.value.length), tone: 'primary' },
    {
      key: 'manageable',
      label: '允许强制退出',
      value: String(items.value.filter((item) => item.canForceLogout).length),
      tone: 'neutral',
    },
  ],
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
  items.value = [];
  try {
    const result = await api.getList(controller.signal);
    if (currentVersion === version) items.value = result;
  } catch (reason) {
    if (currentVersion === version && !(reason instanceof Error && reason.name === 'AbortError'))
      error.value = reason instanceof Error ? reason.message : '加载失败，请刷新重试。';
  } finally {
    if (currentVersion === version) loading.value = false;
  }
}

function select(row: OnlineUserRow) {
  selected.value = items.value.find((item) => item.id === row.id) ?? null;
  revokeError.value = '';
  open.value = Boolean(selected.value);
}

async function revoke() {
  const target = selected.value;
  if (!target?.canForceLogout || !canManage.value || busy.value) return;
  const controller = new AbortController();
  mutation = controller;
  busy.value = true;
  revokeError.value = '';
  let confirmed = false;
  try {
    if (!(await api.forceLogout(target.userId, controller.signal))) throw new Error('服务未确认退出请求，请刷新核对。');
    confirmed = true;
    if (ownAccount.value) {
      clearSession();
      await router.replace({ name: routeNames.login });
      return;
    }
    const readback = await api.getList(controller.signal);
    items.value = readback;
    if (readback.some((item) => item.userId === target.userId))
      throw new Error('该账号仍有活动记录，可能已重新登录，请刷新核对。');
    open.value = false;
    toast.success('退出请求已完成，在线列表已读回确认。');
  } catch (reason) {
    if (reason instanceof Error && reason.name === 'AbortError') return;
    const message = reason instanceof Error ? reason.message : '强制退出失败，请重试。';
    revokeError.value = confirmed ? `已提交退出请求，未确认账号离线。${message}` : message;
  } finally {
    busy.value = false;
  }
}

onMounted(() => {
  if (canManage.value) void load();
});
onBeforeUnmount(() => {
  request?.abort();
  mutation?.abort();
});
</script>

<template>
  <ListPageSurface
    v-if="canManage"
    :model="model"
    @action="(action) => action.id === 'refresh' && load()"
    @row="select"
  >
    <template #hero-icon><UsersRound :size="20" /></template>
    <template #notice
      ><p>活跃状态按服务端最近20分钟访问记录判断；强制退出针对账号全部已有凭证。</p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p></template
    >
  </ListPageSurface>
  <OnlineAccountDialog
    v-model:open="open"
    :item="selected"
    :can-manage="canManage"
    :own-account="ownAccount"
    :busy="busy"
    :error="revokeError"
    @revoke="revoke"
  />
</template>
