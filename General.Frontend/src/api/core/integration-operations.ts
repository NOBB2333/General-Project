import type { ApiRequestOptions } from '@/api/client';

export namespace IntegrationOperationsApi {
  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface DeliveryRecord {
    attemptCount: number;
    businessKey: string;
    businessModule: string;
    connectorCode: string;
    connectorId: string;
    creationTime: string;
    deadLetterReason: string;
    durationMilliseconds: number;
    errorMessage: string;
    externalMessageId: string;
    id: string;
    lastAttemptTime: null | string;
    maxRetryCount: number;
    messageType: string;
    nextRetryTime: null | string;
    payloadPreview: string;
    providerType: string;
    requestJson: string;
    responseJson: string;
    sourceType: string;
    status: string;
    targetId: string;
    targetType: string;
  }

  export interface DeliveryQuery {
    connectorCode?: string;
    createdFrom?: string;
    createdTo?: string;
    keyword?: string;
    maxResultCount: number;
    messageType?: string;
    skipCount: number;
    status?: string;
    targetType?: string;
  }

  export interface DeliverySummary {
    averageDurationMilliseconds: number;
    connectorRates: Array<{
      connectorCode: string;
      displayName: string;
      failedCount: number;
      successRate: number;
      succeededCount: number;
      totalCount: number;
    }>;
    deadLetterCount: number;
    failedCount: number;
    pendingCount: number;
    skippedCount: number;
    succeededCount: number;
    totalCount: number;
  }

  export interface SyncJob {
    configJson: string;
    connectorCode: string;
    connectorId: string;
    cronExpression: string;
    direction: string;
    id: string;
    isEnabled: boolean;
    jobCode: string;
    lastRunError: string;
    lastRunStatus: string;
    lastRunSummary: string;
    lastRunTime: null | string;
    mode: string;
    name: string;
    providerType: string;
    remark: string;
    syncType: string;
  }

  export interface SyncJobQuery {
    connectorCode?: string;
    keyword?: string;
    maxResultCount: number;
    providerType?: string;
    skipCount: number;
    status?: string;
    syncType?: string;
  }

  export interface SyncJobSaveInput {
    configJson: string;
    connectorId: string;
    cronExpression: string;
    direction: string;
    id?: string;
    isEnabled: boolean;
    jobCode: string;
    mode: string;
    name: string;
    remark: string;
    syncType: string;
  }

  export interface SyncRecord {
    conflictCount: number;
    conflictJson: string;
    connectorCode: string;
    connectorId: string;
    creationTime: string;
    createdCount: number;
    durationMilliseconds: number;
    errorMessage: string;
    failedCount: number;
    finishedTime: null | string;
    id: string;
    mode: string;
    providerType: string;
    requestJson: string;
    resultJson: string;
    retryCount: number;
    skippedCount: number;
    startedTime: null | string;
    status: string;
    summary: string;
    syncJobId: string;
    syncType: string;
    totalCount: number;
    updatedCount: number;
  }

  export interface SyncRecordQuery {
    connectorCode?: string;
    keyword?: string;
    maxResultCount: number;
    mode?: string;
    providerType?: string;
    skipCount: number;
    status?: string;
    syncJobId?: string;
    syncType?: string;
  }

  export interface SyncRunInput {
    force: boolean;
    mode: string;
    requestJson: string;
  }

  export interface SkipInput {
    reason: string;
  }

  export interface WebhookEvent {
    actionCode: string;
    businessKey: string;
    connectorCode: string;
    connectorId: null | string;
    creationTime: string;
    dedupeKey: string;
    errorMessage: string;
    eventId: string;
    eventType: string;
    headersJson: string;
    id: string;
    normalizedPayloadJson: string;
    providerType: string;
    rawBody: string;
    rawPayloadJson: string;
    resolvedTime: null | string;
    retryCount: number;
    signatureStatus: string;
    sourceIp: string;
    status: string;
  }

  export interface WebhookEventQuery {
    eventType?: string;
    keyword?: string;
    maxResultCount: number;
    providerType?: string;
    skipCount: number;
    status?: string;
  }
}

export interface IntegrationOperationsApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const rootPath = '/app/integration';
const encodeId = (id: string) => encodeURIComponent(id);

export function createIntegrationOperationsApi(client: IntegrationOperationsApiClient) {
  return {
    exportDeliveryRecords(query: IntegrationOperationsApi.DeliveryQuery, signal?: AbortSignal) {
      return client.request<Blob>(`${rootPath}/delivery-records/export`, {
        query: {
          connectorCode: query.connectorCode,
          createdFrom: query.createdFrom,
          createdTo: query.createdTo,
          keyword: query.keyword,
          maxResultCount: query.maxResultCount,
          messageType: query.messageType,
          skipCount: query.skipCount,
          status: query.status,
          targetType: query.targetType,
        },
        responseType: 'blob',
        signal,
      });
    },
    getDeliveryRecords(query: IntegrationOperationsApi.DeliveryQuery, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.PagedResult<IntegrationOperationsApi.DeliveryRecord>>(
        `${rootPath}/delivery-records`,
        { query: { ...query }, signal },
      );
    },
    getDeliverySummary(query: IntegrationOperationsApi.DeliveryQuery, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.DeliverySummary>(`${rootPath}/delivery-records/summary`, {
        query: { ...query },
        signal,
      });
    },
    getSyncJobs(query: IntegrationOperationsApi.SyncJobQuery, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.PagedResult<IntegrationOperationsApi.SyncJob>>(
        `${rootPath}/sync-jobs`,
        { query: { ...query }, signal },
      );
    },
    getSyncRecords(query: IntegrationOperationsApi.SyncRecordQuery, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.PagedResult<IntegrationOperationsApi.SyncRecord>>(
        `${rootPath}/sync-records`,
        { query: { ...query }, signal },
      );
    },
    getWebhookEvents(query: IntegrationOperationsApi.WebhookEventQuery, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.PagedResult<IntegrationOperationsApi.WebhookEvent>>(
        `${rootPath}/webhook-events`,
        { query: { ...query }, signal },
      );
    },
    retryDelivery(id: string, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.DeliveryRecord>(
        `${rootPath}/delivery-records/${encodeId(id)}/retry`,
        { method: 'POST', signal },
      );
    },
    retrySyncRecord(id: string, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.SyncRecord>(`${rootPath}/sync-records/${encodeId(id)}/retry`, {
        method: 'POST',
        signal,
      });
    },
    retryWebhookEvent(id: string, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.WebhookEvent>(`${rootPath}/webhook-events/${encodeId(id)}/retry`, {
        method: 'POST',
        signal,
      });
    },
    saveSyncJob(input: IntegrationOperationsApi.SyncJobSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.SyncJob>(`${rootPath}/sync-jobs`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    skipDelivery(id: string, input: IntegrationOperationsApi.SkipInput, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.DeliveryRecord>(
        `${rootPath}/delivery-records/${encodeId(id)}/skip`,
        { body: input, method: 'POST', signal },
      );
    },
    skipSyncRecord(id: string, input: IntegrationOperationsApi.SkipInput, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.SyncRecord>(`${rootPath}/sync-records/${encodeId(id)}/skip`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    skipWebhookEvent(id: string, input: IntegrationOperationsApi.SkipInput, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.WebhookEvent>(`${rootPath}/webhook-events/${encodeId(id)}/skip`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    toggleSyncJob(id: string, isEnabled: boolean, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.SyncJob>(`${rootPath}/sync-jobs/${encodeId(id)}/toggle`, {
        body: { isEnabled },
        method: 'POST',
        signal,
      });
    },
    runSyncJob(id: string, input: IntegrationOperationsApi.SyncRunInput, signal?: AbortSignal) {
      return client.request<IntegrationOperationsApi.SyncRecord>(`${rootPath}/sync-jobs/${encodeId(id)}/run`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
  };
}
