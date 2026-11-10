import type { PlatformSchedulerApi } from '@/api/core/platform-scheduler';
import type { ListPageRow } from '@/components/page';

export interface SchedulerJobRow extends ListPageRow {
  cronExpression: string;
  enabledState: string;
  handlerKey: string;
  lastRunTime: string;
  nextRunTime: string;
  runningState: string;
  title: string;
  triggerCount: number;
}

export type SchedulerRunOutcome = 'cancelled' | 'failure' | 'skipped' | 'success' | 'unknown';

export function formatSchedulerTime(value: null | string | undefined): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat('zh-CN', { dateStyle: 'medium', timeStyle: 'medium' }).format(date);
}

export function toSchedulerJobRow(job: PlatformSchedulerApi.Job): SchedulerJobRow {
  return {
    cronExpression: job.cronExpression,
    enabledState: job.isEnabled ? '启用' : '停用',
    handlerKey: job.handlerKey,
    id: job.jobKey,
    lastRunTime: formatSchedulerTime(job.lastRunTime),
    nextRunTime: formatSchedulerTime(job.nextRunTime),
    runningState: job.isRunning ? '执行中' : '空闲',
    title: job.title,
    triggerCount: job.triggerCount,
  };
}

export function classifySchedulerRunMessage(message: string): SchedulerRunOutcome {
  if (message.includes('执行失败')) return 'failure';
  if (message.includes('执行已取消')) return 'cancelled';
  if (message.includes('已跳过')) return 'skipped';
  if (message.includes('执行成功')) return 'success';
  return 'unknown';
}

export function schedulerRunOutcomeLabel(outcome: SchedulerRunOutcome): string {
  switch (outcome) {
    case 'success':
      return '执行成功';
    case 'failure':
      return '执行失败';
    case 'cancelled':
      return '已取消';
    case 'skipped':
      return '已跳过';
    default:
      return '结果待核对';
  }
}

export function toSchedulerRecordRow(record: PlatformSchedulerApi.RecordItem): ListPageRow {
  return {
    duration: record.durationMilliseconds === null ? '—' : `${record.durationMilliseconds} 毫秒`,
    id: record.instanceId,
    result: record.errorMessage || record.result || '—',
    startedAt: formatSchedulerTime(record.startTime),
    status: schedulerRecordStatusLabel(record.status),
    trigger: record.triggerKey,
    triggerMode: record.triggerMode === 'manual' ? '手动' : record.triggerMode === 'auto' ? '自动' : record.triggerMode,
  };
}

export function schedulerRecordStatusLabel(status: string): string {
  switch (status.toLocaleLowerCase()) {
    case 'running':
      return '执行中';
    case 'success':
      return '成功';
    case 'failed':
      return '失败';
    case 'skipped':
      return '跳过';
    case 'cancelled':
      return '已取消';
    default:
      return status || '未知';
  }
}

export function toSchedulerTriggerRow(trigger: PlatformSchedulerApi.Trigger): ListPageRow {
  return {
    cronExpression: trigger.cronExpression,
    enabledState: trigger.isEnabled ? '启用' : '停用',
    id: trigger.triggerKey,
    lastRunTime: formatSchedulerTime(trigger.lastRunTime),
    nextRunTime: formatSchedulerTime(trigger.nextRunTime),
    title: trigger.title,
  };
}

export function toSchedulerNodeRow(node: PlatformSchedulerApi.ClusterNode): ListPageRow {
  return {
    description: node.description,
    heartbeat: formatSchedulerTime(node.lastHeartbeatTime),
    hostName: node.hostName,
    id: node.instanceId,
    processId: node.processId,
    startedAt: formatSchedulerTime(node.startedAt),
    status:
      node.status.toLocaleLowerCase() === 'online'
        ? '在线'
        : node.status.toLocaleLowerCase() === 'offline'
          ? '离线'
          : node.status,
  };
}
