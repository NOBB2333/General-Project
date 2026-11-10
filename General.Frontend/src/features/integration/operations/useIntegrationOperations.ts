import { computed, onBeforeUnmount, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { PageAction } from '@/components/page/page.contracts';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import type { PageColumn, PageFilter } from '@/components/page/page.types';
import { toast, type UiSelectOption } from '@/components/ui';
import { apiClient } from '@/api/request';
import { ApiRequestError } from '@/api/client';
import { createIntegrationConnectorsApi, type IntegrationConnectorsApi } from '@/api/core/integration-connectors';
import { createIntegrationOperationsApi, type IntegrationOperationsApi } from '@/api/core/integration-operations';
import { canAccess } from '@/directives/permission';
import { routeNames } from '@/router/route.types';
import {
  createSyncJobDraft,
  deliveryStatusLabel,
  isDeliveryRetryable,
  isSyncRecordRetryable,
  isWebhookRetryable,
  syncJobStatusLabel,
  syncRecordStatusLabel,
  toDeliveryRow,
  toSyncJobDraft,
  toSyncJobRow,
  toSyncJobSaveInput,
  toSyncRecordRow,
  toWebhookEventRow,
  validateSyncJobDraft,
  webhookStatusLabel,
  type IntegrationOperationsSection,
  type SyncJobDraft,
  type SyncListMode,
} from './operations.model';

type DialogKind =
  | 'delivery-export'
  | 'delivery-retry'
  | 'delivery-skip'
  | 'detail'
  | 'sync-job'
  | 'sync-job-run'
  | 'sync-job-toggle'
  | 'sync-record-retry'
  | 'sync-record-skip'
  | 'webhook-retry'
  | 'webhook-skip'
  | null;

interface Props {
  initialSection?: IntegrationOperationsSection;
}

const api = createIntegrationOperationsApi(apiClient);
const connectorsApi = createIntegrationConnectorsApi(apiClient);
const pageSize = 20;

const deliveryStatusOptions = ['全部状态', '待发送', '发送中', '失败', '死信', '成功', '已跳过'];
const deliveryStatusValues: Record<string, string> = {
  全部状态: 'All',
  待发送: 'Pending',
  发送中: 'Sending',
  失败: 'Failed',
  死信: 'DeadLetter',
  成功: 'Succeeded',
  已跳过: 'Skipped',
};
const syncJobStatusOptions = ['全部状态', '启用', '停用'];
const syncJobStatusValues: Record<string, string> = { 全部状态: 'All', 启用: 'Active', 停用: 'Disabled' };
const syncRecordStatusOptions = ['全部状态', '待执行', '执行中', '成功', '失败', '已跳过'];
const syncRecordStatusValues: Record<string, string> = {
  全部状态: 'All',
  待执行: 'Pending',
  执行中: 'Running',
  成功: 'Succeeded',
  失败: 'Failed',
  已跳过: 'Skipped',
};
const webhookStatusOptions = ['全部状态', '已接收', '已解析', '已处理', '失败', '已跳过'];
const webhookStatusValues: Record<string, string> = {
  全部状态: 'All',
  已接收: 'Received',
  已解析: 'Parsed',
  已处理: 'Processed',
  失败: 'Failed',
  已跳过: 'Skipped',
};
const syncTypeOptions: readonly UiSelectOption[] = [
  { label: '全部类型', value: '全部类型' },
  { label: '组织', value: '组织' },
  { label: '用户', value: '用户' },
  { label: '群', value: '群' },
  { label: '会议', value: '会议' },
  { label: '自定义', value: '自定义' },
];
const syncTypeValues: Record<string, string> = {
  全部类型: '',
  组织: 'Department',
  用户: 'User',
  群: 'Group',
  会议: 'Meeting',
  自定义: 'Custom',
};
const syncTypeOptionsForForm: readonly UiSelectOption[] = [
  { label: '组织', value: 'Department' },
  { label: '用户', value: 'User' },
  { label: '群', value: 'Group' },
  { label: '会议', value: 'Meeting' },
  { label: '自定义', value: 'Custom' },
];

function selectSectionName(section: IntegrationOperationsSection): string {
  if (section === 'delivery') return '投递日志';
  if (section === 'sync') return '同步任务';
  return 'Webhook 事件';
}

function singleSelection<T extends { id: string }>(items: T[], rows: ListPageRow[]): T | null {
  if (rows.length !== 1) {
    toast.warning('每次请选择一条记录。');
    return null;
  }
  return items.find((item) => item.id === rows[0]?.id) ?? null;
}

function dateFilterToIso(value: string): string | undefined {
  if (!value) return undefined;
  const date = new Date(`${value}T00:00:00`);
  return Number.isFinite(date.getTime()) ? date.toISOString() : undefined;
}

function isAbortError(error: unknown): boolean {
  return error instanceof Error && error.name === 'AbortError';
}

function safeErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof ApiRequestError) {
    if (error.status === 401) return '登录状态已失效，请重新登录。';
    if (error.status === 403) return '当前账号无权执行此操作。';
    if (error.status === 400) return '后端拒绝了当前参数，请核对字段后重试。';
  }
  return fallback;
}

function isSafeConnectorOptionSource(connector: IntegrationConnectorsApi.Connector): boolean {
  if ('secret' in connector || 'secretCipherText' in connector || 'robotWebhook' in connector) return false;
  try {
    const config = JSON.parse(connector.configJson || '{}') as Record<string, unknown>;
    if (Object.keys(config).some((key) => key.toLowerCase() === 'robotwebhook')) return false;
    const schema = JSON.parse(connector.configSchemaJson || '{}') as {
      fields?: Array<{ Key?: unknown; Secret?: unknown; key?: unknown; secret?: unknown }>;
    };
    const secretKeys = (schema.fields ?? []).flatMap((field) => {
      const key = typeof field.Key === 'string' ? field.Key : typeof field.key === 'string' ? field.key : '';
      return key && (field.Secret === true || field.secret === true) ? [key] : [];
    });
    return !secretKeys.some((key) =>
      Object.keys(config).some((candidate) => candidate.toLowerCase() === key.toLowerCase()),
    );
  } catch {
    return false;
  }
}

export function useIntegrationOperations(props: Props) {
  const route = useRoute();
  const router = useRouter();
  const activeSection = computed<IntegrationOperationsSection>(() => {
    if (route.name === routeNames.integrationDeliveryRecords) return 'delivery';
    if (route.name === routeNames.integrationSyncJobs) return 'sync';
    if (route.name === routeNames.integrationWebhookEvents) return 'webhook';
    return props.initialSection ?? 'delivery';
  });
  const syncMode = ref<SyncListMode>('jobs');
  const deliveryRecords = ref<IntegrationOperationsApi.DeliveryRecord[]>([]);
  const deliverySummary = ref<IntegrationOperationsApi.DeliverySummary | null>(null);
  const syncJobs = ref<IntegrationOperationsApi.SyncJob[]>([]);
  const syncRecords = ref<IntegrationOperationsApi.SyncRecord[]>([]);
  const webhookEvents = ref<IntegrationOperationsApi.WebhookEvent[]>([]);
  const connectorOptions = ref<UiSelectOption[]>([]);
  const connectorOptionError = ref('');
  const totalCount = ref(0);
  const currentPage = ref(1);
  const keyword = ref('');
  const deliveryStatusFilter = ref('全部状态');
  const deliveryConnectorFilter = ref('');
  const deliveryMessageTypeFilter = ref('');
  const deliveryTargetTypeFilter = ref('');
  const deliveryCreatedFrom = ref('');
  const deliveryCreatedTo = ref('');
  const syncJobStatusFilter = ref('全部状态');
  const syncRecordStatusFilter = ref('全部状态');
  const syncTypeFilter = ref('全部类型');
  const webhookStatusFilter = ref('全部状态');
  const webhookProviderFilter = ref('');
  const webhookEventTypeFilter = ref('');
  const loading = ref(false);
  const saving = ref(false);
  const exporting = ref(false);
  const loadError = ref('');
  const summaryError = ref('');
  const dialogError = ref('');
  const activeRequest = ref<AbortController | null>(null);
  const searchTimer = ref<number | undefined>();
  const dialogKind = ref<DialogKind>(null);
  const dialogOpen = ref(false);
  const skipReason = ref('');
  const syncJobDraft = reactive<SyncJobDraft>(createSyncJobDraft());
  const editingJob = ref<IntegrationOperationsApi.SyncJob | null>(null);
  const hiddenConfigJson = ref(false);
  const preservedConfigJson = ref('');
  const selectedDelivery = ref<IntegrationOperationsApi.DeliveryRecord | null>(null);
  const selectedSyncJob = ref<IntegrationOperationsApi.SyncJob | null>(null);
  const selectedSyncRecord = ref<IntegrationOperationsApi.SyncRecord | null>(null);
  const selectedWebhook = ref<IntegrationOperationsApi.WebhookEvent | null>(null);
  const detailDelivery = ref<IntegrationOperationsApi.DeliveryRecord | null>(null);
  const detailSyncJob = ref<IntegrationOperationsApi.SyncJob | null>(null);
  const detailSyncRecord = ref<IntegrationOperationsApi.SyncRecord | null>(null);
  const detailWebhook = ref<IntegrationOperationsApi.WebhookEvent | null>(null);

  const canManageDelivery = computed(() => canAccess('Integration.Delivery.Manage'));
  const canExportDelivery = computed(() => canAccess('Integration.Delivery.Export'));
  const canManageSync = computed(() => canAccess('Integration.Sync.Manage'));
  const canRunSync = computed(() => canAccess('Integration.Sync.Run'));
  const canManageWebhook = computed(() => canAccess('Integration.Webhook.Manage'));
  const canViewConnector = computed(() => canAccess('Integration.Connector.View'));
  const pageTitle = computed(() => {
    if (activeSection.value === 'delivery') return '投递日志';
    if (activeSection.value === 'webhook') return 'Webhook 事件';
    return syncMode.value === 'jobs' ? '同步任务' : '同步记录';
  });
  const pageRows = computed<ListPageRow[]>(() => {
    if (activeSection.value === 'delivery') return deliveryRecords.value.map(toDeliveryRow);
    if (activeSection.value === 'webhook') return webhookEvents.value.map(toWebhookEventRow);
    return syncMode.value === 'jobs' ? syncJobs.value.map(toSyncJobRow) : syncRecords.value.map(toSyncRecordRow);
  });
  const pageActions = computed<PageAction[]>(() => {
    const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
    if (activeSection.value === 'delivery') {
      if (canManageDelivery.value) {
        actions.push({ id: 'retry', kind: 'custom', label: '重试所选', requiresSelection: true });
        actions.push({ id: 'skip', kind: 'custom', label: '跳过所选', requiresSelection: true });
      }
      if (canExportDelivery.value) actions.push({ id: 'export', kind: 'export', label: '导出筛选结果' });
    } else if (activeSection.value === 'sync' && syncMode.value === 'jobs') {
      if (canManageSync.value) {
        actions.push({ id: 'create', kind: 'create', label: '新建任务', tone: 'primary' });
        actions.push({ id: 'edit', kind: 'custom', label: '编辑所选', requiresSelection: true });
        actions.push({ id: 'toggle', kind: 'custom', label: '启用 / 停用', requiresSelection: true });
      }
      if (canRunSync.value) actions.push({ id: 'run', kind: 'custom', label: '立即运行', requiresSelection: true });
    } else if (activeSection.value === 'sync') {
      if (canRunSync.value) actions.push({ id: 'retry', kind: 'custom', label: '重试所选', requiresSelection: true });
      if (canManageSync.value) actions.push({ id: 'skip', kind: 'custom', label: '跳过所选', requiresSelection: true });
    } else if (canManageWebhook.value) {
      actions.push({ id: 'retry', kind: 'custom', label: '重新解析所选', requiresSelection: true });
      actions.push({ id: 'skip', kind: 'custom', label: '跳过所选', requiresSelection: true });
    }
    return actions;
  });
  const pageModel = computed<ListPageModel>(() => {
    const isDelivery = activeSection.value === 'delivery';
    const isWebhook = activeSection.value === 'webhook';
    const isSyncJobs = activeSection.value === 'sync' && syncMode.value === 'jobs';
    const columns: PageColumn[] = isDelivery
      ? [
          { key: 'createdAt', title: '创建时间', width: 180, sticky: true },
          { key: 'connector', title: '连接器', width: 140 },
          { key: 'messageType', title: '消息类型', width: 130 },
          { key: 'targetType', title: '目标类型', width: 120 },
          { key: 'sourceType', title: '来源', width: 130 },
          { key: 'attempt', title: '尝试 / 上限', width: 120 },
          { key: 'status', title: '状态', width: 100, presentation: 'status' },
          { key: 'nextRetryAt', title: '下次重试', width: 180 },
          { key: 'duration', title: '耗时', width: 110 },
        ]
      : isWebhook
        ? [
            { key: 'receivedAt', title: '接收时间', width: 180, sticky: true },
            { key: 'provider', title: '平台', width: 130 },
            { key: 'connector', title: '连接器', width: 140 },
            { key: 'eventType', title: '事件类型', width: 210 },
            { key: 'signature', title: '签名状态', width: 110, presentation: 'status' },
            { key: 'status', title: '处理状态', width: 120, presentation: 'status' },
            { key: 'retryCount', title: '重试次数', width: 110 },
            { key: 'sourceIp', title: '来源地址', width: 150 },
          ]
        : isSyncJobs
          ? [
              { key: 'name', title: '任务名称', width: 210, sticky: true },
              { key: 'syncType', title: '同步类型', width: 120 },
              { key: 'direction', title: '同步方向', width: 110 },
              { key: 'connector', title: '连接器', width: 140 },
              { key: 'cron', title: 'Cron / 执行方式', width: 180 },
              { key: 'lastRunStatus', title: '最近状态', width: 110, presentation: 'status' },
              { key: 'lastRun', title: '最近运行', width: 180 },
              { key: 'enabled', title: '任务状态', width: 100, presentation: 'status' },
            ]
          : [
              { key: 'createdAt', title: '创建时间', width: 180, sticky: true },
              { key: 'connector', title: '连接器', width: 140 },
              { key: 'syncType', title: '同步类型', width: 120 },
              { key: 'mode', title: '模式', width: 100 },
              { key: 'counts', title: '处理计数', width: 340 },
              { key: 'status', title: '状态', width: 110, presentation: 'status' },
              { key: 'retryCount', title: '重试次数', width: 110 },
              { key: 'duration', title: '耗时', width: 110 },
            ];
    const filters: PageFilter[] = isDelivery
      ? [{ key: 'status', label: '投递状态', options: deliveryStatusOptions }]
      : isWebhook
        ? [{ key: 'status', label: '事件状态', options: webhookStatusOptions }]
        : isSyncJobs
          ? [
              { key: 'status', label: '任务状态', options: syncJobStatusOptions },
              { key: 'syncType', label: '同步类型', options: syncTypeOptions.map((option) => String(option.label)) },
            ]
          : [
              { key: 'status', label: '执行状态', options: syncRecordStatusOptions },
              { key: 'syncType', label: '同步类型', options: syncTypeOptions.map((option) => String(option.label)) },
            ];
    const statusCount = isDelivery ? (deliverySummary.value?.totalCount ?? totalCount.value) : totalCount.value;
    return {
      actions: pageActions.value,
      columns,
      demoPath: route.path,
      description: isDelivery
        ? '查看投递状态与重试记录。重试会再次调用连接器，可能重复投递；详情中的原始内容不在页面展示。'
        : isWebhook
          ? '查看外部回调的接收与解析状态。页面不显示原始请求头、请求体或Payload。'
          : isSyncJobs
            ? '管理连接器同步任务和执行计划。手动运行会调用连接器并可能写入业务数据。'
            : '查看同步执行计数与状态；原始请求、结果和冲突JSON不在页面展示。',
      emptyHint: loading.value
        ? `正在读取${pageTitle.value}…`
        : loadError.value
          ? `${pageTitle.value}读取失败，请刷新重试。`
          : `当前筛选条件下没有${pageTitle.value}。`,
      filters,
      key: `外部集成|${pageTitle.value}`,
      label: pageTitle.value,
      listScopes: [{ id: 'all', label: pageTitle.value, count: statusCount }],
      listTabs: [{ id: 'all', label: '全部记录' }],
      listViews: [{ id: 'list', label: '列表' }],
      module: '外部集成',
      moduleSlug: 'integration',
      pageSlug: isDelivery
        ? 'integration-delivery-records'
        : isWebhook
          ? 'integration-webhook-events'
          : isSyncJobs
            ? 'integration-sync-jobs'
            : 'integration-sync-records',
      pageSize,
      panels: [],
      path: route.path,
      primaryAction: pageActions.value.find((action) => action.kind === 'create')?.label ?? '刷新',
      replication: { status: 'implemented', referenceUrl: route.path },
      routeName: String(route.name ?? 'integration-operations'),
      rows: pageRows.value,
      searchPlaceholder: isDelivery
        ? '按连接器、消息类型或业务关键词筛选'
        : isWebhook
          ? '按事件编号或业务关键词筛选'
          : '按任务名称、编码、连接器或状态筛选',
      stats: isDelivery
        ? [
            {
              key: 'total',
              label: '筛选结果',
              value: String(deliverySummary.value?.totalCount ?? totalCount.value),
              tone: 'primary',
            },
            { key: 'failed', label: '失败', value: String(deliverySummary.value?.failedCount ?? 0), tone: 'warning' },
            { key: 'dead', label: '死信', value: String(deliverySummary.value?.deadLetterCount ?? 0), tone: 'danger' },
            {
              key: 'avg',
              label: '平均耗时',
              value: `${deliverySummary.value?.averageDurationMilliseconds ?? 0} ms`,
              tone: 'neutral',
            },
          ]
        : [
            { key: 'total', label: '筛选结果', value: String(totalCount.value), tone: 'primary' },
            { key: 'page', label: '当前页记录', value: String(pageRows.value.length), tone: 'success' },
            {
              key: 'access',
              label: '当前权限',
              value:
                activeSection.value === 'sync'
                  ? canManageSync.value || canRunSync.value
                    ? '可操作'
                    : '只读'
                  : canManageWebhook.value
                    ? '可处置'
                    : '只读',
              tone: 'neutral',
            },
          ],
      surface: 'list',
      tabs: [],
      tools: [],
    };
  });
  const remoteFilterValues = computed<Readonly<Record<string, string>>>(() => {
    const values: Record<string, string> = {};
    if (activeSection.value === 'delivery') {
      values.status = deliveryStatusFilter.value;
    } else if (activeSection.value === 'webhook') {
      values.status = webhookStatusFilter.value;
    } else {
      values.status = syncMode.value === 'jobs' ? syncJobStatusFilter.value : syncRecordStatusFilter.value;
      values.syncType = syncTypeFilter.value;
    }
    return values;
  });
  const remotePagination = computed(() => ({
    currentPage: currentPage.value,
    pageSize,
    searchQuery: keyword.value,
    totalCount: totalCount.value,
  }));
  const dialogTitle = computed(() => {
    switch (dialogKind.value) {
      case 'delivery-export':
        return '导出投递审计';
      case 'delivery-retry':
        return '确认重试投递';
      case 'delivery-skip':
        return '跳过投递记录';
      case 'sync-job':
        return editingJob.value ? '编辑同步任务' : '新建同步任务';
      case 'sync-job-run':
        return '确认运行同步任务';
      case 'sync-job-toggle':
        return '确认切换任务状态';
      case 'sync-record-retry':
        return '确认重试同步';
      case 'sync-record-skip':
        return '跳过同步记录';
      case 'webhook-retry':
        return '重新解析 Webhook 事件';
      case 'webhook-skip':
        return '跳过 Webhook 事件';
      case 'detail':
        return '记录详情';
      default:
        return '';
    }
  });
  const dialogDescription = computed(() => {
    switch (dialogKind.value) {
      case 'delivery-export':
        return '导出文件包含目标ID、业务键、错误原因和消息摘要；按当前筛选条件最多导出5000条，并要求投递导出权限。';
      case 'delivery-retry':
        return '此操作会再次调用当前连接器发送消息。后端未提供幂等保证，失败记录可能已被对端接收，请先核对外部状态以避免重复通知。';
      case 'delivery-skip':
        return '跳过会更新投递记录状态，不会再调用连接器发送器。';
      case 'sync-job':
        return '任务配置遵循后端字段；凭证在连接器配置中管理，不应写入同步映射JSON。';
      case 'sync-job-run':
        return selectedSyncJob.value?.isEnabled
          ? '运行会调用当前连接器并可能导入或更新业务数据。确认连接器、范围和数据源后再执行。'
          : '该任务已停用；强制运行会绕过停用状态并调用连接器，可能导入或更新业务数据。';
      case 'sync-job-toggle':
        return selectedSyncJob.value
          ? `${selectedSyncJob.value.isEnabled ? '停用' : '启用'}“${selectedSyncJob.value.name}”？任务状态变更会影响后续调度。`
          : '';
      case 'sync-record-retry':
        return '重试会再次调用任务连接器，并可能重复导入或更新数据；页面不显示记录中的原始请求和结果JSON。';
      case 'sync-record-skip':
        return '跳过会更新执行记录状态，不会启动新的同步任务。';
      case 'webhook-retry':
        return '该接口只重置事件状态并重新解析已存Payload，不会调用外部消息发送器。原始请求头和请求体不会展示。';
      case 'webhook-skip':
        return '跳过会更新事件处置状态；不会改变Webhook端点配置。';
      case 'detail':
        return '仅展示状态、时间和计数等必要元数据。接口返回的未脱敏JSON、请求头、请求体和原始错误不在页面展示。';
      default:
        return '';
    }
  });

  function deliveryQuery(): IntegrationOperationsApi.DeliveryQuery {
    return {
      connectorCode: deliveryConnectorFilter.value.trim() || undefined,
      createdFrom: dateFilterToIso(deliveryCreatedFrom.value),
      createdTo: dateFilterToIso(deliveryCreatedTo.value),
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize,
      messageType: deliveryMessageTypeFilter.value.trim() || undefined,
      skipCount: (currentPage.value - 1) * pageSize,
      status: deliveryStatusValues[deliveryStatusFilter.value] ?? 'All',
      targetType: deliveryTargetTypeFilter.value.trim() || undefined,
    };
  }

  function syncJobQuery(): IntegrationOperationsApi.SyncJobQuery {
    const syncType = syncTypeValues[syncTypeFilter.value];
    return {
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize,
      skipCount: (currentPage.value - 1) * pageSize,
      status: syncJobStatusValues[syncJobStatusFilter.value] ?? 'All',
      syncType: syncType || undefined,
    };
  }

  function syncRecordQuery(): IntegrationOperationsApi.SyncRecordQuery {
    const syncType = syncTypeValues[syncTypeFilter.value];
    return {
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize,
      skipCount: (currentPage.value - 1) * pageSize,
      status: syncRecordStatusValues[syncRecordStatusFilter.value] ?? 'All',
      syncType: syncType || undefined,
    };
  }

  function webhookEventQuery(): IntegrationOperationsApi.WebhookEventQuery {
    return {
      eventType: webhookEventTypeFilter.value.trim() || undefined,
      keyword: keyword.value.trim() || undefined,
      maxResultCount: pageSize,
      providerType: webhookProviderFilter.value.trim() || undefined,
      skipCount: (currentPage.value - 1) * pageSize,
      status: webhookStatusValues[webhookStatusFilter.value] ?? 'All',
    };
  }

  async function loadPage(): Promise<void> {
    activeRequest.value?.abort();
    const controller = new AbortController();
    activeRequest.value = controller;
    loading.value = true;
    loadError.value = '';
    summaryError.value = '';
    try {
      if (activeSection.value === 'delivery') {
        const result = await api.getDeliveryRecords(deliveryQuery(), controller.signal);
        if (controller.signal.aborted) return;
        deliveryRecords.value = result.items;
        totalCount.value = result.totalCount;
        try {
          deliverySummary.value = await api.getDeliverySummary(deliveryQuery(), controller.signal);
        } catch (error) {
          if (controller.signal.aborted || isAbortError(error)) return;
          deliverySummary.value = null;
          summaryError.value = '投递统计读取失败；列表记录仍可查看。';
        }
      } else if (activeSection.value === 'sync' && syncMode.value === 'jobs') {
        const result = await api.getSyncJobs(syncJobQuery(), controller.signal);
        if (controller.signal.aborted) return;
        syncJobs.value = result.items;
        totalCount.value = result.totalCount;
      } else if (activeSection.value === 'sync') {
        const result = await api.getSyncRecords(syncRecordQuery(), controller.signal);
        if (controller.signal.aborted) return;
        syncRecords.value = result.items;
        totalCount.value = result.totalCount;
      } else {
        const result = await api.getWebhookEvents(webhookEventQuery(), controller.signal);
        if (controller.signal.aborted) return;
        webhookEvents.value = result.items;
        totalCount.value = result.totalCount;
      }
    } catch (error) {
      if (controller.signal.aborted || isAbortError(error)) return;
      loadError.value = safeErrorMessage(error, `${pageTitle.value}读取失败，请稍后重试。`);
    } finally {
      if (activeRequest.value === controller) loading.value = false;
    }
  }

  function selectSection(section: string): void {
    const target = section as IntegrationOperationsSection;
    const routeName =
      target === 'delivery'
        ? routeNames.integrationDeliveryRecords
        : target === 'sync'
          ? routeNames.integrationSyncJobs
          : routeNames.integrationWebhookEvents;
    void router.push({ name: routeName });
  }

  function selectSyncMode(value: string): void {
    if (value !== 'jobs' && value !== 'records') return;
    syncMode.value = value;
  }

  function handleSearch(value: string): void {
    keyword.value = value;
    currentPage.value = 1;
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
    searchTimer.value = window.setTimeout(() => void loadPage(), 250);
  }

  function handleFilterChange(key: string, value: string): void {
    if (key === 'status' && activeSection.value === 'delivery') deliveryStatusFilter.value = value;
    if (key === 'status' && activeSection.value === 'webhook') webhookStatusFilter.value = value;
    if (key === 'status' && activeSection.value === 'sync' && syncMode.value === 'jobs') {
      syncJobStatusFilter.value = value;
    }
    if (key === 'status' && activeSection.value === 'sync' && syncMode.value === 'records') {
      syncRecordStatusFilter.value = value;
    }
    if (key === 'syncType' && activeSection.value === 'sync') syncTypeFilter.value = value;
    currentPage.value = 1;
    void loadPage();
  }

  function applyDeliveryFilters(): void {
    currentPage.value = 1;
    void loadPage();
  }

  function resetDeliveryFilters(): void {
    deliveryConnectorFilter.value = '';
    deliveryMessageTypeFilter.value = '';
    deliveryTargetTypeFilter.value = '';
    deliveryCreatedFrom.value = '';
    deliveryCreatedTo.value = '';
    currentPage.value = 1;
    void loadPage();
  }

  function applyWebhookFilters(): void {
    currentPage.value = 1;
    void loadPage();
  }

  function resetWebhookFilters(): void {
    webhookProviderFilter.value = '';
    webhookEventTypeFilter.value = '';
    currentPage.value = 1;
    void loadPage();
  }

  function changePage(page: number): void {
    currentPage.value = page;
    void loadPage();
  }

  async function loadConnectorOptions(): Promise<void> {
    connectorOptionError.value = '';
    connectorOptions.value = [];
    if (!canViewConnector.value) return;
    try {
      const connectors = await connectorsApi.getConnectors({ includeDisabled: true });
      if (!connectors.every(isSafeConnectorOptionSource)) {
        connectorOptionError.value = '连接器接口返回了不应展示的凭证配置，已停止提供下拉选项。';
        return;
      }
      connectorOptions.value = connectors.map((connector) => ({
        label: `${connector.displayName}（${connector.connectorCode}）`,
        value: connector.id,
      }));
    } catch {
      connectorOptionError.value = '连接器选项读取失败；可填写已有连接器 GUID。';
    }
  }

  function handleAction(action: PageAction, rows: ListPageRow[]): void {
    if (action.id === 'reload') {
      void loadPage();
      return;
    }

    if (activeSection.value === 'delivery') {
      if (action.id === 'export' && canExportDelivery.value) {
        dialogKind.value = 'delivery-export';
        dialogOpen.value = true;
        dialogError.value = '';
        return;
      }
      const record = singleSelection(deliveryRecords.value, rows);
      if (!record) return;
      selectedDelivery.value = record;
      if (action.id === 'retry' && canManageDelivery.value) {
        if (!isDeliveryRetryable(record)) {
          toast.warning('只有失败或死信记录可以重试。');
          return;
        }
        openDialog('delivery-retry');
      } else if (action.id === 'skip' && canManageDelivery.value) {
        openSkipDialog('delivery-skip');
      }
      return;
    }

    if (activeSection.value === 'sync' && syncMode.value === 'jobs') {
      if (action.id === 'create' && canManageSync.value) {
        openSyncJobDialog();
        return;
      }
      const job = singleSelection(syncJobs.value, rows);
      if (!job) return;
      selectedSyncJob.value = job;
      if (action.id === 'edit' && canManageSync.value) {
        openSyncJobDialog(job);
      } else if (action.id === 'toggle' && canManageSync.value) {
        openDialog('sync-job-toggle');
      } else if (action.id === 'run' && canRunSync.value) {
        openDialog('sync-job-run');
      }
      return;
    }

    if (activeSection.value === 'sync') {
      const record = singleSelection(syncRecords.value, rows);
      if (!record) return;
      selectedSyncRecord.value = record;
      if (action.id === 'retry' && canRunSync.value) {
        if (!isSyncRecordRetryable(record)) {
          toast.warning('只有失败的同步记录可以重试。');
          return;
        }
        openDialog('sync-record-retry');
      } else if (action.id === 'skip' && canManageSync.value) {
        openSkipDialog('sync-record-skip');
      }
      return;
    }

    const event = singleSelection(webhookEvents.value, rows);
    if (!event) return;
    selectedWebhook.value = event;
    if (action.id === 'retry' && canManageWebhook.value) {
      if (!isWebhookRetryable(event)) {
        toast.warning('只有失败事件可以重新解析。');
        return;
      }
      openDialog('webhook-retry');
    } else if (action.id === 'skip' && canManageWebhook.value) {
      openSkipDialog('webhook-skip');
    }
  }

  function openRow(row: ListPageRow): void {
    detailDelivery.value = null;
    detailSyncJob.value = null;
    detailSyncRecord.value = null;
    detailWebhook.value = null;
    if (activeSection.value === 'delivery') {
      detailDelivery.value = deliveryRecords.value.find((item) => item.id === row.id) ?? null;
    } else if (activeSection.value === 'webhook') {
      detailWebhook.value = webhookEvents.value.find((item) => item.id === row.id) ?? null;
    } else if (syncMode.value === 'jobs') {
      detailSyncJob.value = syncJobs.value.find((item) => item.id === row.id) ?? null;
    } else {
      detailSyncRecord.value = syncRecords.value.find((item) => item.id === row.id) ?? null;
    }
    openDialog('detail');
  }

  function openSyncJobDialog(job?: IntegrationOperationsApi.SyncJob): void {
    editingJob.value = job ?? null;
    Object.assign(syncJobDraft, toSyncJobDraft(job));
    preservedConfigJson.value = job?.configJson ?? '';
    // ConfigJson has no server-side redaction contract, so never render existing values in the form.
    hiddenConfigJson.value = Boolean(job);
    if (hiddenConfigJson.value) syncJobDraft.configJson = '{}';
    connectorOptionError.value = '';
    dialogError.value = '';
    dialogKind.value = 'sync-job';
    dialogOpen.value = true;
    void loadConnectorOptions();
  }

  function openDialog(kind: Exclude<DialogKind, null>): void {
    dialogKind.value = kind;
    dialogError.value = '';
    dialogOpen.value = true;
  }

  function openSkipDialog(kind: 'delivery-skip' | 'sync-record-skip' | 'webhook-skip'): void {
    skipReason.value = '';
    openDialog(kind);
  }

  function closeDialog(): void {
    if (saving.value || exporting.value) return;
    dialogOpen.value = false;
    dialogError.value = '';
    skipReason.value = '';
    dialogKind.value = null;
  }

  async function submitDialog(): Promise<void> {
    if (saving.value || exporting.value) return;
    dialogError.value = '';
    if (dialogKind.value === 'delivery-export') {
      await exportDeliveryRecords();
      return;
    }
    if (dialogKind.value === 'sync-job') {
      await saveSyncJob();
      return;
    }
    if (
      dialogKind.value === 'delivery-skip' ||
      dialogKind.value === 'sync-record-skip' ||
      dialogKind.value === 'webhook-skip'
    ) {
      if (!skipReason.value.trim()) {
        dialogError.value = '请填写处置原因。';
        return;
      }
    }

    saving.value = true;
    try {
      switch (dialogKind.value) {
        case 'delivery-retry':
          if (!selectedDelivery.value || !canManageDelivery.value) return;
          await api.retryDelivery(selectedDelivery.value.id);
          toast.success('投递重试已执行，列表状态已刷新。');
          break;
        case 'delivery-skip':
          if (!selectedDelivery.value || !canManageDelivery.value) return;
          await api.skipDelivery(selectedDelivery.value.id, { reason: skipReason.value.trim() });
          toast.success('投递记录已跳过。');
          break;
        case 'sync-job-toggle':
          if (!selectedSyncJob.value || !canManageSync.value) return;
          await api.toggleSyncJob(selectedSyncJob.value.id, !selectedSyncJob.value.isEnabled);
          toast.success('同步任务状态已更新。');
          break;
        case 'sync-job-run': {
          if (!selectedSyncJob.value || !canRunSync.value) return;
          const job = selectedSyncJob.value;
          await api.runSyncJob(job.id, { force: !job.isEnabled, mode: job.mode, requestJson: '{}' });
          syncMode.value = 'records';
          toast.success('同步请求已完成，执行记录已读取。');
          break;
        }
        case 'sync-record-retry':
          if (!selectedSyncRecord.value || !canRunSync.value) return;
          await api.retrySyncRecord(selectedSyncRecord.value.id);
          toast.success('同步重试已完成，执行记录已刷新。');
          break;
        case 'sync-record-skip':
          if (!selectedSyncRecord.value || !canManageSync.value) return;
          await api.skipSyncRecord(selectedSyncRecord.value.id, { reason: skipReason.value.trim() });
          toast.success('同步记录已跳过。');
          break;
        case 'webhook-retry':
          if (!selectedWebhook.value || !canManageWebhook.value) return;
          await api.retryWebhookEvent(selectedWebhook.value.id);
          toast.success('Webhook事件已重新解析。');
          break;
        case 'webhook-skip':
          if (!selectedWebhook.value || !canManageWebhook.value) return;
          await api.skipWebhookEvent(selectedWebhook.value.id, { reason: skipReason.value.trim() });
          toast.success('Webhook事件已跳过。');
          break;
        default:
          return;
      }
      closeDialogAfterSuccess();
      await loadPage();
    } catch (error) {
      dialogError.value = safeErrorMessage(error, '操作失败；服务端错误详情可能包含敏感数据，页面已隐藏。');
      toast.error(dialogError.value);
    } finally {
      saving.value = false;
    }
  }

  async function saveSyncJob(): Promise<void> {
    if (!canManageSync.value) return;
    const draftForSave = {
      ...syncJobDraft,
      configJson: hiddenConfigJson.value ? '{}' : syncJobDraft.configJson,
    };
    const validationError = validateSyncJobDraft(draftForSave);
    if (validationError) {
      dialogError.value = validationError;
      return;
    }

    const input = toSyncJobSaveInput(draftForSave);
    if (hiddenConfigJson.value && editingJob.value) input.configJson = preservedConfigJson.value;
    saving.value = true;
    try {
      const saved = await api.saveSyncJob(input);
      if (
        saved.jobCode !== input.jobCode ||
        saved.name !== input.name ||
        saved.connectorId !== input.connectorId ||
        saved.isEnabled !== input.isEnabled
      ) {
        throw new Error('保存结果与提交字段不一致。');
      }
      toast.success('同步任务已保存并读回确认。');
      closeDialogAfterSuccess();
      await loadPage();
    } catch (error) {
      dialogError.value = safeErrorMessage(error, '同步任务保存失败；服务端错误详情可能包含敏感数据，页面已隐藏。');
      toast.error(dialogError.value);
    } finally {
      saving.value = false;
    }
  }

  async function exportDeliveryRecords(): Promise<void> {
    if (!canExportDelivery.value) return;
    exporting.value = true;
    try {
      const blob = await api.exportDeliveryRecords({ ...deliveryQuery(), maxResultCount: 5000, skipCount: 0 });
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = '外部系统投递审计.csv';
      anchor.click();
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
      toast.success('投递审计文件已导出。');
      closeDialogAfterSuccess();
    } catch (error) {
      dialogError.value = safeErrorMessage(error, '导出失败；服务端错误详情已隐藏。');
      toast.error(dialogError.value);
    } finally {
      exporting.value = false;
    }
  }

  function closeDialogAfterSuccess(): void {
    dialogOpen.value = false;
    dialogKind.value = null;
    dialogError.value = '';
    skipReason.value = '';
  }

  function changeSyncMode(value: string): void {
    if (value === 'jobs' || value === 'records') selectSyncMode(value);
  }

  watch(
    [activeSection, syncMode],
    () => {
      currentPage.value = 1;
      keyword.value = '';
      void loadPage();
    },
    { immediate: true },
  );

  onBeforeUnmount(() => {
    activeRequest.value?.abort();
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
  });

  return {
    activeSection,
    applyDeliveryFilters,
    applyWebhookFilters,
    canExportDelivery,
    canManageDelivery,
    canManageSync,
    canManageWebhook,
    canRunSync,
    changePage,
    changeSyncMode,
    clear: resetDeliveryFilters,
    closeDialog,
    connectorOptionError,
    connectorOptions,
    deliveryCreatedFrom,
    deliveryCreatedTo,
    deliveryConnectorFilter,
    deliveryMessageTypeFilter,
    deliveryStatusFilter,
    deliveryStatusLabel,
    deliverySummary,
    deliveryTargetTypeFilter,
    detailDelivery,
    detailSyncJob,
    detailSyncRecord,
    detailWebhook,
    dialogDescription,
    dialogError,
    dialogKind,
    dialogOpen,
    dialogTitle,
    exporting,
    handleAction,
    handleFilterChange,
    handleSearch,
    hiddenConfigJson,
    isDeliveryRetryable,
    isSyncRecordRetryable,
    isWebhookRetryable,
    loadError,
    loading,
    openRow,
    pageModel,
    remoteFilterValues,
    remotePagination,
    resetDeliveryFilters,
    resetWebhookFilters,
    selectSection,
    selectSectionName,
    selectedDelivery,
    selectedSyncJob,
    selectedSyncRecord,
    selectedWebhook,
    skipReason,
    summaryError,
    syncJobDraft,
    syncJobStatusLabel,
    syncMode,
    syncRecordStatusLabel,
    syncTypeOptionsForForm,
    webhookEventTypeFilter,
    webhookProviderFilter,
    webhookStatusLabel,
    webhookStatusFilter,
    saving,
    submitDialog,
  };
}

export type IntegrationOperationsPageState = ReturnType<typeof useIntegrationOperations>;
