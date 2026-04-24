import { requestClient } from '#/api/request';

export namespace SchedulerApi {
  export interface JobItem {
    cronExpression: string;
    description: string;
    isEnabled: boolean;
    isRunning: boolean;
    jobKey: string;
    lastRunResult: string;
    lastRunTime?: null | string;
    nextRunTime?: null | string;
    title: string;
  }
}

export async function getSchedulerListApi() {
  return requestClient.get<SchedulerApi.JobItem[]>('/app/platform/scheduler/list');
}

export async function toggleSchedulerJobApi(jobKey: string, isEnabled: boolean) {
  return requestClient.post(`/app/platform/scheduler/${jobKey}/toggle`, {
    isEnabled,
  });
}

export async function runSchedulerJobApi(jobKey: string) {
  return requestClient.post<string>(`/app/platform/scheduler/${jobKey}/run`);
}
