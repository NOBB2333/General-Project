import type { ListPageModel, ListPageRow } from '@/components/page';
import type { usePlatformSchedulerPage } from './usePlatformSchedulerPage';

type SchedulerPagePageState = ReturnType<typeof usePlatformSchedulerPage>;

export function createSchedulerListModel(
  page: SchedulerPagePageState,
  canManage: boolean,
  canExecute: boolean,
): ListPageModel<ListPageRow> {
  const mode = page.mode.value;
  const jobsMode = mode === 'jobs';
  const triggersMode = mode === 'triggers';
  const recordsMode = mode === 'records';
  const actions = jobsMode
    ? [
        { id: 'reload-jobs', label: page.loading.value ? '读取中' : '重新读取', kind: 'refresh' as const },
        ...(canManage
          ? [
              { id: 'create-job', label: '新建任务', kind: 'custom' as const, tone: 'primary' as const },
              {
                id: 'toggle-jobs',
                label: '启用 / 停用所选',
                kind: 'custom' as const,
                requiresSelection: true,
              },
              {
                id: 'clear-selected-records',
                label: '清理所选记录',
                kind: 'custom' as const,
                tone: 'danger' as const,
                requiresSelection: true,
              },
            ]
          : []),
        ...(canExecute
          ? [
              {
                id: 'run-jobs',
                label: '立即执行所选',
                kind: 'custom' as const,
                tone: 'primary' as const,
                requiresSelection: true,
              },
            ]
          : []),
      ]
    : triggersMode
      ? [
          {
            id: 'reload-details',
            label: page.detailsLoading.value ? '读取中' : '重新读取',
            kind: 'refresh' as const,
          },
          ...(canManage
            ? [
                { id: 'add-trigger', label: '新增触发器', kind: 'custom' as const, tone: 'primary' as const },
                {
                  id: 'edit-trigger',
                  label: '编辑所选触发器',
                  kind: 'custom' as const,
                  requiresSelection: true,
                },
                {
                  id: 'toggle-trigger',
                  label: '启用 / 停用所选',
                  kind: 'custom' as const,
                  requiresSelection: true,
                },
                {
                  id: 'delete-trigger',
                  label: '删除所选触发器',
                  kind: 'custom' as const,
                  tone: 'danger' as const,
                  requiresSelection: true,
                },
              ]
            : []),
        ]
      : recordsMode
        ? [
            {
              id: 'reload-records',
              label: page.recordsLoading.value ? '读取中' : '重新读取',
              kind: 'refresh' as const,
            },
            ...(canManage
              ? [
                  {
                    id: 'clear-history',
                    label: '清理当前任务记录',
                    kind: 'custom' as const,
                    tone: 'danger' as const,
                  },
                ]
              : []),
          ]
        : [
            {
              id: 'reload-nodes',
              label: page.nodesLoading.value ? '读取中' : '重新读取节点',
              kind: 'refresh' as const,
            },
          ];

  const columns = jobsMode
    ? [
        { key: 'title', title: '任务名称', width: 190 },
        { key: 'id', title: '任务键', width: 190 },
        { key: 'handlerKey', title: '处理器', width: 220 },
        { key: 'cronExpression', title: 'Cron 表达式', width: 180 },
        { key: 'enabledState', title: '主开关', width: 80 },
        { key: 'runningState', title: '运行状态', width: 90 },
        { key: 'lastRunTime', title: '上次运行', width: 180 },
        { key: 'nextRunTime', title: '下次运行', width: 180 },
        { key: 'triggerCount', title: '触发器数', width: 90 },
      ]
    : triggersMode
      ? [
          { key: 'title', title: '触发器名称', width: 200 },
          { key: 'id', title: '触发器键', width: 200 },
          { key: 'cronExpression', title: 'Cron 表达式', width: 220 },
          { key: 'enabledState', title: '触发器开关', width: 100 },
          { key: 'lastRunTime', title: '上次运行', width: 190 },
          { key: 'nextRunTime', title: '下次运行', width: 190 },
        ]
      : recordsMode
        ? [
            { key: 'startedAt', title: '开始时间', width: 190 },
            { key: 'status', title: '状态', width: 100 },
            { key: 'triggerMode', title: '触发方式', width: 100 },
            { key: 'trigger', title: '触发器', width: 160 },
            { key: 'duration', title: '耗时', width: 120 },
            { key: 'result', title: '结果 / 错误', width: 420 },
          ]
        : [
            { key: 'hostName', title: '主机名', width: 190 },
            { key: 'id', title: '实例 ID', width: 280 },
            { key: 'status', title: '节点状态', width: 100 },
            { key: 'processId', title: '进程号', width: 120 },
            { key: 'startedAt', title: '启动时间', width: 190 },
            { key: 'heartbeat', title: '最后心跳', width: 190 },
            { key: 'description', title: '说明', width: 220 },
          ];

  const rows: ListPageRow[] = jobsMode
    ? page.rows.value
    : triggersMode
      ? page.triggerRows.value
      : recordsMode
        ? page.recordRows.value
        : page.nodeRows.value;
  const label = jobsMode ? '定时任务' : triggersMode ? '任务触发器' : recordsMode ? '任务执行记录' : '调度集群节点';
  const tabs =
    jobsMode || triggersMode
      ? [
          { id: 'all', label: jobsMode ? '全部任务' : '全部触发器' },
          { id: 'enabled', label: '已启用', filterKey: 'enabledState', filterValue: '启用' },
          { id: 'disabled', label: '已停用', filterKey: 'enabledState', filterValue: '停用' },
        ]
      : [{ id: 'all', label: label }];
  const count = jobsMode
    ? page.rows.value.length
    : triggersMode
      ? page.triggerRows.value.length
      : recordsMode
        ? page.recordTotalCount.value
        : page.nodeRows.value.length;
  const loading = jobsMode
    ? page.loading.value
    : triggersMode
      ? page.detailsLoading.value
      : recordsMode
        ? page.recordsLoading.value
        : page.nodesLoading.value;
  const error = jobsMode
    ? page.overviewError.value
    : triggersMode
      ? page.detailsError.value
      : recordsMode
        ? page.recordsError.value
        : page.nodesError.value;

  return {
    actions,
    columns,
    demoPath: '/platform/scheduler',
    description: jobsMode
      ? '查看平台定时任务、处理器和执行情况；管理操作会直接影响后台调度。'
      : triggersMode
        ? `管理“${page.selectedJob.value?.title ?? ''}”的独立调度触发器。`
        : recordsMode
          ? `按时间、状态和触发方式查看“${page.selectedJob.value?.title ?? ''}”的运行历史。`
          : '查看调度服务实例和最近心跳。',
    emptyHint: loading ? `正在读取${label}…` : error ? `${label}读取失败，请重新读取。` : `暂无${label}。`,
    filters: [],
    key: `平台中心|${label}`,
    label,
    listScopes: [{ id: 'all', label, count }],
    listTabs: tabs,
    listViews: [{ id: 'list', label: '列表' }],
    module: '平台中心',
    moduleSlug: 'platform',
    pageSlug: 'platform-scheduler',
    pageSize: recordsMode ? 50 : 20,
    panels: [],
    path: '/platform/scheduler',
    primaryAction: jobsMode
      ? '新建任务'
      : triggersMode
        ? '新增触发器'
        : recordsMode
          ? '清理当前任务记录'
          : '重新读取节点',
    replication: { status: 'implemented', referenceUrl: '/platform/scheduler' },
    routeName: 'platform-scheduler',
    rows,
    searchPlaceholder: jobsMode
      ? '搜索任务名称 / 任务键 / 处理器 / Cron'
      : triggersMode
        ? '搜索触发器名称 / 键 / Cron'
        : recordsMode
          ? '搜索结果 / 状态 / 触发器'
          : '搜索主机名 / 实例 ID / 状态',
    stats: jobsMode
      ? [
          { key: 'all', label: '任务总数', value: String(page.dashboard.value.totalCount), tone: 'primary' },
          {
            key: 'enabled',
            label: '自动调度已启用',
            value: String(page.dashboard.value.enabledCount),
            tone: 'success',
          },
          { key: 'running', label: '当前运行中', value: String(page.dashboard.value.runningCount), tone: 'warning' },
          {
            key: 'failed',
            label: '近 24 小时失败',
            value: String(page.dashboard.value.failedLast24Hours),
            tone: 'danger',
          },
          {
            key: 'slow',
            label: '近 24 小时耗时任务',
            value: String(page.dashboard.value.slowLast24Hours),
            tone: 'neutral',
          },
        ]
      : triggersMode
        ? [
            { key: 'all', label: '触发器总数', value: String(page.triggers.value.length), tone: 'primary' },
            {
              key: 'enabled',
              label: '已启用',
              value: String(page.triggers.value.filter((trigger) => trigger.isEnabled).length),
              tone: 'success',
            },
            {
              key: 'task',
              label: '任务主开关',
              value: page.selectedJob.value?.isEnabled ? '启用' : '停用',
              tone: 'neutral',
            },
          ]
        : recordsMode
          ? [
              { key: 'total', label: '符合条件记录', value: String(page.recordTotalCount.value), tone: 'primary' },
              {
                key: 'page',
                label: '当前页',
                value: `${page.recordPage.value} / ${page.recordPageCount.value}`,
                tone: 'neutral',
              },
              { key: 'size', label: '每页上限', value: '50 条', tone: 'neutral' },
            ]
          : [
              { key: 'all', label: '集群节点', value: String(page.nodes.value.length), tone: 'primary' },
              {
                key: 'online',
                label: '在线',
                value: String(page.nodes.value.filter((node) => node.status.toLowerCase() === 'online').length),
                tone: 'success',
              },
              {
                key: 'offline',
                label: '离线',
                value: String(page.nodes.value.filter((node) => node.status.toLowerCase() === 'offline').length),
                tone: 'warning',
              },
            ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}
