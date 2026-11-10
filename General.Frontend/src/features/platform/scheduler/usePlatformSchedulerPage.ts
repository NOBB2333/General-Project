import { computed, ref } from 'vue';
import { createPlatformSchedulerApi, type PlatformSchedulerApi } from '@/api/core/platform-scheduler';
import { apiClient } from '@/api/request';
import { toast } from '@/components/ui';
import {
  classifySchedulerRunMessage,
  schedulerRunOutcomeLabel,
  toSchedulerJobRow,
  toSchedulerNodeRow,
  toSchedulerRecordRow,
  toSchedulerTriggerRow,
} from './scheduler.model';

type ConfirmationAction =
  | { isEnabled: boolean; jobKeys: string[]; kind: 'toggle-jobs' }
  | { jobKeys: string[]; kind: 'run-jobs' }
  | { jobKeys: string[]; kind: 'clear-records' }
  | { jobKeys: string[]; kind: 'delete-job' }
  | { jobKeys: string[]; kind: 'cancel-job' }
  | { isEnabled: boolean; jobKeys: string[]; kind: 'toggle-trigger'; triggerKey: string }
  | { jobKeys: string[]; kind: 'delete-trigger'; triggerKey: string };

type SchedulerPageMode = 'jobs' | 'nodes' | 'records' | 'triggers';

const api = createPlatformSchedulerApi(apiClient);
const recordPageSize = 50;

export function usePlatformSchedulerPage() {
  const jobs = ref<PlatformSchedulerApi.Job[]>([]);
  const handlers = ref<PlatformSchedulerApi.Handler[]>([]);
  const dashboard = ref<PlatformSchedulerApi.Dashboard>({
    enabledCount: 0,
    failedLast24Hours: 0,
    runningCount: 0,
    slowLast24Hours: 0,
    totalCount: 0,
  });
  const nodes = ref<PlatformSchedulerApi.ClusterNode[]>([]);
  const loading = ref(false);
  const nodesLoading = ref(false);
  const overviewError = ref('');
  const nodesError = ref('');
  const selectedJob = ref<null | PlatformSchedulerApi.Job>(null);
  const mode = ref<SchedulerPageMode>('jobs');
  const detailTab = ref<'records' | 'triggers'>('triggers');
  const detailsLoading = ref(false);
  const detailsError = ref('');
  const triggers = ref<PlatformSchedulerApi.Trigger[]>([]);
  const records = ref<PlatformSchedulerApi.RecordItem[]>([]);
  const recordTotalCount = ref(0);
  const recordPage = ref(1);
  const recordStatus = ref('');
  const recordMode = ref('');
  const startedFrom = ref('');
  const startedTo = ref('');
  const recordsLoading = ref(false);
  const recordsError = ref('');
  const jobDialogOpen = ref(false);
  const editingJob = ref<null | PlatformSchedulerApi.Job>(null);
  const savingJob = ref(false);
  const triggerDialogOpen = ref(false);
  const editingTrigger = ref<null | PlatformSchedulerApi.Trigger>(null);
  const savingTrigger = ref(false);
  const confirmationOpen = ref(false);
  const confirming = ref(false);
  const confirmation = ref<ConfirmationAction | null>(null);
  const keepLastN = ref(100);

  const rows = computed(() => jobs.value.map(toSchedulerJobRow));
  const nodeRows = computed(() => nodes.value.map(toSchedulerNodeRow));
  const triggerRows = computed(() => triggers.value.map(toSchedulerTriggerRow));
  const recordRows = computed(() => records.value.map(toSchedulerRecordRow));
  const recordPageCount = computed(() => Math.max(1, Math.ceil(recordTotalCount.value / recordPageSize)));

  async function loadOverview(): Promise<boolean> {
    loading.value = true;
    overviewError.value = '';
    try {
      const [nextJobs, nextHandlers, nextDashboard] = await Promise.all([
        api.getJobs(),
        api.getHandlers(),
        api.getDashboard(),
      ]);
      jobs.value = nextJobs;
      handlers.value = nextHandlers;
      dashboard.value = nextDashboard;
      if (selectedJob.value) {
        selectedJob.value = nextJobs.find((job) => job.jobKey === selectedJob.value?.jobKey) ?? null;
      }
      return true;
    } catch (error) {
      overviewError.value = error instanceof Error ? error.message : '定时任务读取失败，请重试。';
      return false;
    } finally {
      loading.value = false;
    }
  }

  function recordQuery() {
    return {
      maxResultCount: recordPageSize,
      skipCount: (recordPage.value - 1) * recordPageSize,
      ...(recordStatus.value ? { status: recordStatus.value } : {}),
      ...(recordMode.value ? { triggerMode: recordMode.value } : {}),
      ...(startedFrom.value ? { startedFrom: startedFrom.value } : {}),
      ...(startedTo.value ? { startedTo: startedTo.value } : {}),
    } satisfies PlatformSchedulerApi.RecordQuery;
  }

  async function loadRecords(page = recordPage.value): Promise<boolean> {
    const job = selectedJob.value;
    if (!job) return false;
    recordsLoading.value = true;
    recordsError.value = '';
    recordPage.value = Math.max(1, page);
    try {
      const result = await api.getRecords(job.jobKey, recordQuery());
      records.value = result.items;
      recordTotalCount.value = result.totalCount;
      return true;
    } catch (error) {
      recordsError.value = error instanceof Error ? error.message : '执行记录读取失败，请重试。';
      return false;
    } finally {
      recordsLoading.value = false;
    }
  }

  async function loadJobDetails(jobKey: string): Promise<boolean> {
    const job = jobs.value.find((item) => item.jobKey === jobKey);
    if (!job) {
      toast.error('任务已不存在，请重新读取任务列表。');
      return false;
    }
    selectedJob.value = job;
    detailTab.value = 'triggers';
    mode.value = 'triggers';
    detailsLoading.value = true;
    detailsError.value = '';
    recordPage.value = 1;
    recordStatus.value = '';
    recordMode.value = '';
    startedFrom.value = '';
    startedTo.value = '';
    try {
      // 读取触发器时，后端会在列表为空时补建默认触发器。
      const [nextTriggers, recordResult] = await Promise.all([
        api.getTriggers(jobKey),
        api.getRecords(jobKey, { maxResultCount: recordPageSize, skipCount: 0 }),
      ]);
      triggers.value = nextTriggers;
      records.value = recordResult.items;
      recordTotalCount.value = recordResult.totalCount;
      return true;
    } catch (error) {
      detailsError.value = error instanceof Error ? error.message : '任务详情读取失败，请重试。';
      return false;
    } finally {
      detailsLoading.value = false;
    }
  }

  async function reloadDetails(): Promise<boolean> {
    const job = selectedJob.value;
    if (!job) return false;
    const current = jobs.value.find((item) => item.jobKey === job.jobKey);
    if (!current) {
      selectedJob.value = null;
      mode.value = 'jobs';
      return false;
    }
    selectedJob.value = current;
    detailsLoading.value = true;
    detailsError.value = '';
    try {
      const [nextTriggers, recordResult] = await Promise.all([
        api.getTriggers(current.jobKey),
        api.getRecords(current.jobKey, recordQuery()),
      ]);
      triggers.value = nextTriggers;
      records.value = recordResult.items;
      recordTotalCount.value = recordResult.totalCount;
      return true;
    } catch (error) {
      detailsError.value = error instanceof Error ? error.message : '任务详情读回失败，请重新读取。';
      return false;
    } finally {
      detailsLoading.value = false;
    }
  }

  async function refreshAfterWrite(): Promise<boolean> {
    const overviewReadBack = await loadOverview();
    if (!overviewReadBack) return false;
    if (mode.value === 'nodes') {
      try {
        nodes.value = await api.getClusterNodes();
        return true;
      } catch (error) {
        nodesError.value = error instanceof Error ? error.message : '集群节点读回失败，请重新读取。';
        return false;
      }
    }
    if (!selectedJob.value) return true;
    return reloadDetails();
  }

  async function loadClusterNodes() {
    mode.value = 'nodes';
    nodesLoading.value = true;
    nodesError.value = '';
    try {
      nodes.value = await api.getClusterNodes();
    } catch (error) {
      nodesError.value = error instanceof Error ? error.message : '集群节点读取失败，请重试。';
      toast.error(nodesError.value);
    } finally {
      nodesLoading.value = false;
    }
  }

  function backToJobs() {
    mode.value = 'jobs';
    selectedJob.value = null;
    detailsError.value = '';
    nodesError.value = '';
  }

  function setDetailTab(tab: 'records' | 'triggers') {
    detailTab.value = tab;
    mode.value = tab;
    if (tab === 'records') void loadRecords(1);
    else void reloadDetails();
  }

  function openCreateJob() {
    editingJob.value = null;
    jobDialogOpen.value = true;
  }

  function openEditJob(job: PlatformSchedulerApi.Job | null = selectedJob.value) {
    if (!job || job.isRunning) return;
    editingJob.value = job;
    jobDialogOpen.value = true;
  }

  async function saveJob(input: PlatformSchedulerApi.JobSaveInput) {
    if (savingJob.value) return;
    savingJob.value = true;
    try {
      const saved = editingJob.value ? await api.updateJob(editingJob.value.jobKey, input) : await api.createJob(input);
      const readBack = await loadOverview();
      const actual = jobs.value.find((job) => job.jobKey === saved.jobKey);
      if (
        !readBack ||
        !actual ||
        actual.handlerKey !== input.handlerKey ||
        actual.cronExpression !== input.cronExpression
      ) {
        toast.error('任务保存请求已返回，但列表读回未确认处理器和 Cron；请重新读取核对。');
        return;
      }
      jobDialogOpen.value = false;
      editingJob.value = null;
      if (selectedJob.value?.jobKey === actual.jobKey) selectedJob.value = actual;
      toast.success('任务已保存，并已从任务列表确认读回。');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '任务保存失败，请重试。');
    } finally {
      savingJob.value = false;
    }
  }

  function openCreateTrigger() {
    editingTrigger.value = null;
    triggerDialogOpen.value = true;
  }

  function openEditTrigger(triggerKey: string) {
    editingTrigger.value = triggers.value.find((trigger) => trigger.triggerKey === triggerKey) ?? null;
    if (editingTrigger.value) triggerDialogOpen.value = true;
  }

  async function saveTrigger(input: PlatformSchedulerApi.TriggerSaveInput) {
    const job = selectedJob.value;
    if (!job || savingTrigger.value) return;
    savingTrigger.value = true;
    try {
      const saved = editingTrigger.value
        ? await api.updateTrigger(job.jobKey, editingTrigger.value.triggerKey, input)
        : await api.createTrigger(job.jobKey, input);
      const nextTriggers = await api.getTriggers(job.jobKey);
      triggers.value = nextTriggers;
      const actual = nextTriggers.find((trigger) => trigger.triggerKey === saved.triggerKey);
      if (!actual || actual.cronExpression !== input.cronExpression || actual.isEnabled !== input.isEnabled) {
        toast.error('触发器保存已返回，但读回未确认配置，请重新读取核对。');
        return;
      }
      triggerDialogOpen.value = false;
      editingTrigger.value = null;
      toast.success('触发器已保存，并已从服务端确认读回。');
      const overviewReadBack = await loadOverview();
      if (overviewReadBack) {
        selectedJob.value = jobs.value.find((item) => item.jobKey === job.jobKey) ?? job;
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '触发器保存失败，请重试。');
    } finally {
      savingTrigger.value = false;
    }
  }

  function openConfirmation(action: ConfirmationAction) {
    confirmation.value = action;
    keepLastN.value = 100;
    confirmationOpen.value = true;
  }

  function requestRun(jobKeys: string[]) {
    if (jobKeys.length > 0) openConfirmation({ jobKeys, kind: 'run-jobs' });
  }

  function requestToggleJobs(jobKeys: string[]) {
    if (jobKeys.length === 0) return;
    const selected = jobs.value.filter((job) => jobKeys.includes(job.jobKey));
    const isEnabled = selected.some((job) => !job.isEnabled);
    openConfirmation({ isEnabled, jobKeys, kind: 'toggle-jobs' });
  }

  function requestClearRecords(jobKeys: string[]) {
    if (jobKeys.length > 0) openConfirmation({ jobKeys, kind: 'clear-records' });
  }

  function requestDeleteJob(jobKey: string) {
    openConfirmation({ jobKeys: [jobKey], kind: 'delete-job' });
  }

  function requestCancelJob(jobKey: string) {
    openConfirmation({ jobKeys: [jobKey], kind: 'cancel-job' });
  }

  function requestToggleTrigger(trigger: PlatformSchedulerApi.Trigger) {
    const job = selectedJob.value;
    if (!job) return;
    openConfirmation({
      isEnabled: !trigger.isEnabled,
      jobKeys: [job.jobKey],
      kind: 'toggle-trigger',
      triggerKey: trigger.triggerKey,
    });
  }

  function requestDeleteTrigger(trigger: PlatformSchedulerApi.Trigger) {
    const job = selectedJob.value;
    if (!job) return;
    openConfirmation({ jobKeys: [job.jobKey], kind: 'delete-trigger', triggerKey: trigger.triggerKey });
  }

  const confirmationTitle = computed(() => {
    switch (confirmation.value?.kind) {
      case 'run-jobs':
        return '确认立即执行任务';
      case 'toggle-jobs':
        return confirmation.value.isEnabled ? '确认启用自动调度' : '确认停用自动调度';
      case 'clear-records':
        return '确认清理执行记录';
      case 'delete-job':
        return '确认删除定时任务';
      case 'cancel-job':
        return '确认发送取消信号';
      case 'toggle-trigger':
        return confirmation.value.isEnabled ? '确认启用触发器' : '确认停用触发器';
      case 'delete-trigger':
        return '确认删除触发器';
      default:
        return '确认操作';
    }
  });

  const confirmationDescription = computed(() => {
    const action = confirmation.value;
    if (!action) return '';
    const titles = action.jobKeys.map((key) => jobs.value.find((job) => job.jobKey === key)?.title ?? key);
    switch (action.kind) {
      case 'run-jobs':
        return `将立即调用 ${titles.join('、')} 的真实处理器。即使任务主开关处于停用状态，手动执行仍会运行；处理器可能写入业务数据、发送通知或调用外部服务。`;
      case 'toggle-jobs':
        return `${titles.join('、')} 将${action.isEnabled ? '启用' : '停用'}。任务主开关与触发器开关分别保存；启用后后台服务会按已启用触发器自动扫描执行。`;
      case 'clear-records':
        return `每个所选任务仅保留最新 N 条执行记录，其余记录将被物理删除。输入 0 会删除全部记录。`;
      case 'delete-job':
        return `删除“${titles[0]}”会同时物理删除其全部触发器和执行历史，且无法从本页面恢复。`;
      case 'cancel-job':
        return `向“${titles[0]}”发送取消信号。处理器可能不会响应；提交后仍需重新读取状态确认是否结束。`;
      case 'toggle-trigger':
        return `将触发器“${action.triggerKey}”${action.isEnabled ? '启用' : '停用'}。这不会同步改变任务主开关。`;
      case 'delete-trigger':
        return `只删除触发器“${action.triggerKey}”。如果它是该任务唯一的触发器，后端下次读取时会自动补建默认触发器。`;
    }
  });

  function confirmationButtonLabel() {
    switch (confirmation.value?.kind) {
      case 'run-jobs':
        return '确认执行';
      case 'toggle-jobs':
        return confirmation.value.isEnabled ? '确认启用' : '确认停用';
      case 'clear-records':
        return '确认清理';
      case 'delete-job':
      case 'delete-trigger':
        return '确认删除';
      case 'cancel-job':
        return '发送取消信号';
      case 'toggle-trigger':
        return confirmation.value.isEnabled ? '确认启用' : '确认停用';
      default:
        return '确认';
    }
  }

  async function confirmAction() {
    const action = confirmation.value;
    if (!action || confirming.value) return;
    if (
      action.kind === 'clear-records' &&
      (!Number.isInteger(keepLastN.value) || keepLastN.value < 0 || keepLastN.value > 10_000)
    ) {
      toast.error('保留条数必须是 0 至 10000 的整数。');
      return;
    }
    confirming.value = true;
    let writeCompleted = false;
    try {
      if (action.kind === 'run-jobs') {
        if (action.jobKeys.length === 1) {
          const message = await api.runJob(action.jobKeys[0]!);
          writeCompleted = true;
          const outcome = classifySchedulerRunMessage(message);
          if (outcome === 'success') toast.success(message);
          else if (outcome === 'failure') toast.error(message);
          else toast.warning(`${schedulerRunOutcomeLabel(outcome)}：${message}`);
        } else {
          const results = await api.runJobs(action.jobKeys);
          writeCompleted = true;
          const failures = results.filter((result) => !result.success);
          if (failures.length === 0) toast.success(`已完成 ${results.length} 个任务的手动执行请求。`);
          else
            toast.error(
              `有 ${failures.length} 个任务未成功：${failures.map((item) => `${item.jobKey}：${item.message}`).join('；')}`,
            );
        }
      } else if (action.kind === 'toggle-jobs') {
        const result =
          action.jobKeys.length === 1
            ? await api.toggleJob(action.jobKeys[0]!, action.isEnabled)
            : await api.toggleJobs(action.jobKeys, action.isEnabled);
        if (result === false) throw new Error('服务未确认任务启停操作。');
        writeCompleted = true;
        toast.success(`已提交${action.isEnabled ? '启用' : '停用'}任务操作。`);
      } else if (action.kind === 'clear-records') {
        const result =
          action.jobKeys.length === 1
            ? await api.clearRecords(action.jobKeys[0]!, keepLastN.value)
            : await api.clearRecordsBatch(action.jobKeys, keepLastN.value);
        if (!result) throw new Error('服务未确认执行记录清理。');
        writeCompleted = true;
        toast.success(`执行记录清理已完成，按每项保留最新 ${keepLastN.value} 条处理。`);
      } else if (action.kind === 'delete-job') {
        if (!(await api.deleteJob(action.jobKeys[0]!))) throw new Error('服务未确认任务删除。');
        writeCompleted = true;
        mode.value = 'jobs';
        selectedJob.value = null;
        toast.success('任务、触发器和执行记录已删除。');
      } else if (action.kind === 'cancel-job') {
        const message = await api.cancelJob(action.jobKeys[0]!);
        writeCompleted = true;
        toast.info(message);
      } else if (action.kind === 'toggle-trigger') {
        await api.toggleTrigger(action.jobKeys[0]!, action.triggerKey, action.isEnabled);
        writeCompleted = true;
        toast.success(`触发器已${action.isEnabled ? '启用' : '停用'}。`);
      } else {
        if (!(await api.deleteTrigger(action.jobKeys[0]!, action.triggerKey)))
          throw new Error('服务未确认触发器删除。');
        writeCompleted = true;
        toast.success('触发器已删除。');
      }

      confirmationOpen.value = false;
      confirmation.value = null;
      if (writeCompleted) {
        const readBack = await refreshAfterWrite();
        if (!readBack) toast.warning('操作已提交，但服务端读回失败；请使用“重新读取”核对当前状态。');
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '操作失败，请重新读取状态后重试。');
    } finally {
      confirming.value = false;
    }
  }

  function closeConfirmation(open: boolean) {
    if (confirming.value) return;
    confirmationOpen.value = open;
    if (!open) confirmation.value = null;
  }

  function closeJobDialog() {
    if (savingJob.value) return;
    jobDialogOpen.value = false;
    editingJob.value = null;
  }

  function closeTriggerDialog() {
    if (savingTrigger.value) return;
    triggerDialogOpen.value = false;
    editingTrigger.value = null;
  }

  return {
    confirmation,
    confirmationButtonLabel,
    confirmationDescription,
    confirmationOpen,
    confirmationTitle,
    confirming,
    closeConfirmation,
    closeJobDialog,
    closeTriggerDialog,
    confirmAction,
    dashboard,
    detailTab,
    detailsError,
    detailsLoading,
    editingJob,
    editingTrigger,
    handlers,
    jobDialogOpen,
    jobs,
    keepLastN,
    loadClusterNodes,
    mode,
    loadJobDetails,
    loadOverview,
    loadRecords,
    loading,
    nodes,
    nodesError,
    nodesLoading,
    nodeRows,
    backToJobs,
    openCreateJob,
    openCreateTrigger,
    openEditJob,
    openEditTrigger,
    overviewError,
    recordMode,
    recordPage,
    recordPageCount,
    recordRows,
    recordStatus,
    recordTotalCount,
    recordsError,
    recordsLoading,
    reloadDetails,
    requestCancelJob,
    requestClearRecords,
    requestDeleteJob,
    requestDeleteTrigger,
    requestRun,
    requestToggleJobs,
    requestToggleTrigger,
    rows,
    saveJob,
    saveTrigger,
    savingJob,
    savingTrigger,
    selectedJob,
    setDetailTab,
    startedFrom,
    startedTo,
    triggerDialogOpen,
    triggerRows,
    triggers,
  };
}
