import { requestClient } from '#/api/request';

export namespace SchedulerApi {
  export interface JobItem {
    cronExpression: string;
    description: string;
    isEnabled: boolean;
    isRunning: boolean;
    handlerKey: string;
    jobKey: string;
    lastRunResult: string;
    lastRunTime?: null | string;
    lockExpirationTime?: null | string;
    nextRunTime?: null | string;
    runningInstanceId?: null | string;
    title: string;
    triggerCount: number;
  }

  export interface HandlerItem {
    handlerKey: string;
  }

  export interface JobSaveInput {
    cronExpression: string;
    description: string;
    handlerKey: string;
    isEnabled: boolean;
    jobKey: string;
    title: string;
  }

  export interface JobRecordItem {
    durationMilliseconds?: null | number;
    endTime?: null | string;
    errorMessage?: null | string;
    instanceId: string;
    jobKey: string;
    jobTitle: string;
    result?: null | string;
    startTime: string;
    status: string;
    triggerKey: string;
    triggerMode: string;
  }

  export interface JobRecordQuery {
    maxResultCount?: number;
    startedFrom?: string;
    startedTo?: string;
    status?: string;
    triggerMode?: string;
  }

  export interface TriggerItem {
    cronExpression: string;
    description: string;
    isEnabled: boolean;
    lastRunResult: string;
    lastRunTime?: null | string;
    nextRunTime?: null | string;
    title: string;
    triggerKey: string;
  }

  export interface TriggerSaveInput {
    cronExpression: string;
    description: string;
    isEnabled: boolean;
    title: string;
    triggerKey: string;
  }

  export interface ClusterNodeItem {
    description: string;
    hostName: string;
    instanceId: string;
    lastHeartbeatTime: string;
    processId: string;
    startedAt: string;
    status: string;
  }
}

export async function getSchedulerListApi() {
  return requestClient.get<SchedulerApi.JobItem[]>('/app/platform/scheduler/list');
}

export async function getSchedulerHandlersApi() {
  return requestClient.get<SchedulerApi.HandlerItem[]>('/app/platform/scheduler/handlers');
}

export async function getSchedulerClusterNodesApi() {
  return requestClient.get<SchedulerApi.ClusterNodeItem[]>('/app/platform/scheduler/cluster-nodes');
}

export async function createSchedulerJobApi(data: SchedulerApi.JobSaveInput) {
  return requestClient.post<SchedulerApi.JobItem>('/app/platform/scheduler', data);
}

export async function updateSchedulerJobApi(jobKey: string, data: SchedulerApi.JobSaveInput) {
  return requestClient.put<SchedulerApi.JobItem>(`/app/platform/scheduler/${jobKey}`, data);
}

export async function toggleSchedulerJobApi(jobKey: string, isEnabled: boolean) {
  return requestClient.post(`/app/platform/scheduler/${jobKey}/toggle`, {
    isEnabled,
  });
}

export async function runSchedulerJobApi(jobKey: string) {
  return requestClient.post<string>(`/app/platform/scheduler/${jobKey}/run`);
}

export async function cancelSchedulerJobApi(jobKey: string) {
  return requestClient.post<string>(`/app/platform/scheduler/${jobKey}/cancel`);
}

export async function deleteSchedulerJobApi(jobKey: string) {
  return requestClient.delete<boolean>(`/app/platform/scheduler/${jobKey}`);
}

export async function getSchedulerJobRecordsApi(jobKey: string, params?: SchedulerApi.JobRecordQuery) {
  return requestClient.get<SchedulerApi.JobRecordItem[]>(`/app/platform/scheduler/${jobKey}/records`, {
    params,
  });
}

export async function clearSchedulerJobRecordsApi(jobKey: string, keepLastN = 100) {
  return requestClient.delete<boolean>(`/app/platform/scheduler/${jobKey}/records`, {
    params: { keepLastN },
  });
}

export async function getSchedulerJobTriggersApi(jobKey: string) {
  return requestClient.get<SchedulerApi.TriggerItem[]>(`/app/platform/scheduler/${jobKey}/triggers`);
}

export async function createSchedulerJobTriggerApi(jobKey: string, data: SchedulerApi.TriggerSaveInput) {
  return requestClient.post<SchedulerApi.TriggerItem>(`/app/platform/scheduler/${jobKey}/triggers`, data);
}

export async function updateSchedulerJobTriggerApi(
  jobKey: string,
  triggerKey: string,
  data: SchedulerApi.TriggerSaveInput,
) {
  return requestClient.put<SchedulerApi.TriggerItem>(`/app/platform/scheduler/${jobKey}/triggers/${triggerKey}`, data);
}

export async function toggleSchedulerJobTriggerApi(jobKey: string, triggerKey: string, isEnabled: boolean) {
  return requestClient.post(`/app/platform/scheduler/${jobKey}/triggers/${triggerKey}/toggle`, {
    isEnabled,
  });
}

export async function deleteSchedulerJobTriggerApi(jobKey: string, triggerKey: string) {
  return requestClient.delete<boolean>(`/app/platform/scheduler/${jobKey}/triggers/${triggerKey}`);
}
