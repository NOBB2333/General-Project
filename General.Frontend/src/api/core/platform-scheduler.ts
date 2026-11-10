import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformSchedulerApi {
  export interface Job {
    cronExpression: string;
    description: string;
    handlerKey: string;
    isEnabled: boolean;
    isRunning: boolean;
    jobKey: string;
    lastRunResult: string;
    lastRunTime: null | string;
    lockExpirationTime: null | string;
    nextRunTime: null | string;
    runningInstanceId: null | string;
    title: string;
    triggerCount: number;
  }

  export interface Handler {
    handlerKey: string;
  }

  export interface Dashboard {
    enabledCount: number;
    failedLast24Hours: number;
    runningCount: number;
    slowLast24Hours: number;
    totalCount: number;
  }

  export interface ClusterNode {
    description: string;
    hostName: string;
    instanceId: string;
    lastHeartbeatTime: string;
    processId: string;
    startedAt: string;
    status: string;
  }

  export interface JobSaveInput {
    cronExpression: string;
    description: string;
    handlerKey: string;
    isEnabled: boolean;
    jobKey: string;
    title: string;
  }

  export interface ToggleInput {
    isEnabled: boolean;
  }

  export interface BatchInput {
    jobKeys: string[];
  }

  export interface BatchToggleInput extends BatchInput, ToggleInput {}

  export interface BatchClearRecordsInput extends BatchInput {
    keepLastN: number;
  }

  export interface OperationResult {
    jobKey: string;
    message: string;
    success: boolean;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface RecordItem {
    durationMilliseconds: null | number;
    endTime: null | string;
    errorMessage: null | string;
    instanceId: string;
    jobKey: string;
    jobTitle: string;
    result: null | string;
    startTime: string;
    status: string;
    triggerKey: string;
    triggerMode: string;
  }

  export interface RecordQuery {
    maxResultCount?: number;
    skipCount?: number;
    startedFrom?: string;
    startedTo?: string;
    status?: string;
    triggerMode?: string;
  }

  export interface Trigger {
    cronExpression: string;
    description: string;
    isEnabled: boolean;
    lastRunResult: string;
    lastRunTime: null | string;
    nextRunTime: null | string;
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
}

interface PlatformSchedulerClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const basePath = '/app/platform/scheduler';
const encodeKey = (key: string) => encodeURIComponent(key);

export function createPlatformSchedulerApi(client: PlatformSchedulerClient) {
  return {
    getJobs(signal?: AbortSignal): Promise<PlatformSchedulerApi.Job[]> {
      return client.request(`${basePath}/list`, { signal });
    },
    getHandlers(signal?: AbortSignal): Promise<PlatformSchedulerApi.Handler[]> {
      return client.request(`${basePath}/handlers`, { signal });
    },
    getDashboard(signal?: AbortSignal): Promise<PlatformSchedulerApi.Dashboard> {
      return client.request(`${basePath}/dashboard`, { signal });
    },
    // 当前节点心跳会在此 GET 中更新；页面只在用户点开节点面板时调用。
    getClusterNodes(signal?: AbortSignal): Promise<PlatformSchedulerApi.ClusterNode[]> {
      return client.request(`${basePath}/cluster-nodes`, { signal });
    },
    createJob(input: PlatformSchedulerApi.JobSaveInput, signal?: AbortSignal): Promise<PlatformSchedulerApi.Job> {
      return client.request(basePath, { body: input, method: 'POST', signal });
    },
    updateJob(
      jobKey: string,
      input: PlatformSchedulerApi.JobSaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformSchedulerApi.Job> {
      return client.request(`${basePath}/${encodeKey(jobKey)}`, { body: input, method: 'PUT', signal });
    },
    toggleJob(jobKey: string, isEnabled: boolean, signal?: AbortSignal): Promise<string> {
      const body: PlatformSchedulerApi.ToggleInput = { isEnabled };
      return client.request(`${basePath}/${encodeKey(jobKey)}/toggle`, { body, method: 'POST', signal });
    },
    toggleJobs(jobKeys: string[], isEnabled: boolean, signal?: AbortSignal): Promise<boolean> {
      const body: PlatformSchedulerApi.BatchToggleInput = { isEnabled, jobKeys };
      return client.request(`${basePath}/batch/toggle`, { body, method: 'POST', signal });
    },
    runJob(jobKey: string, signal?: AbortSignal): Promise<string> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/run`, { method: 'POST', signal });
    },
    runJobs(jobKeys: string[], signal?: AbortSignal): Promise<PlatformSchedulerApi.OperationResult[]> {
      const body: PlatformSchedulerApi.BatchInput = { jobKeys };
      return client.request(`${basePath}/batch/run`, { body, method: 'POST', signal });
    },
    cancelJob(jobKey: string, signal?: AbortSignal): Promise<string> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/cancel`, { method: 'POST', signal });
    },
    deleteJob(jobKey: string, signal?: AbortSignal): Promise<boolean> {
      return client.request(`${basePath}/${encodeKey(jobKey)}`, { method: 'DELETE', signal });
    },
    getRecords(
      jobKey: string,
      query: PlatformSchedulerApi.RecordQuery = {},
      signal?: AbortSignal,
    ): Promise<PlatformSchedulerApi.PagedResult<PlatformSchedulerApi.RecordItem>> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/records`, {
        query: {
          maxResultCount: query.maxResultCount,
          skipCount: query.skipCount,
          startedFrom: query.startedFrom,
          startedTo: query.startedTo,
          status: query.status,
          triggerMode: query.triggerMode,
        },
        signal,
      });
    },
    clearRecords(jobKey: string, keepLastN: number, signal?: AbortSignal): Promise<boolean> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/records`, {
        method: 'DELETE',
        query: { keepLastN },
        signal,
      });
    },
    clearRecordsBatch(jobKeys: string[], keepLastN: number, signal?: AbortSignal): Promise<boolean> {
      const body: PlatformSchedulerApi.BatchClearRecordsInput = { jobKeys, keepLastN };
      return client.request(`${basePath}/batch/records/clear`, { body, method: 'POST', signal });
    },
    getTriggers(jobKey: string, signal?: AbortSignal): Promise<PlatformSchedulerApi.Trigger[]> {
      // 若没有触发器，服务端会在读取时创建默认触发器。
      return client.request(`${basePath}/${encodeKey(jobKey)}/triggers`, { signal });
    },
    createTrigger(
      jobKey: string,
      input: PlatformSchedulerApi.TriggerSaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformSchedulerApi.Trigger> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/triggers`, { body: input, method: 'POST', signal });
    },
    updateTrigger(
      jobKey: string,
      triggerKey: string,
      input: PlatformSchedulerApi.TriggerSaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformSchedulerApi.Trigger> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/triggers/${encodeKey(triggerKey)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    toggleTrigger(jobKey: string, triggerKey: string, isEnabled: boolean, signal?: AbortSignal): Promise<string> {
      const body: PlatformSchedulerApi.ToggleInput = { isEnabled };
      return client.request(`${basePath}/${encodeKey(jobKey)}/triggers/${encodeKey(triggerKey)}/toggle`, {
        body,
        method: 'POST',
        signal,
      });
    },
    deleteTrigger(jobKey: string, triggerKey: string, signal?: AbortSignal): Promise<boolean> {
      return client.request(`${basePath}/${encodeKey(jobKey)}/triggers/${encodeKey(triggerKey)}`, {
        method: 'DELETE',
        signal,
      });
    },
  };
}
