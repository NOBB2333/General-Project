import { requestClient } from '#/api/request';

export namespace AuditLogApi {
  export interface AuditLogItem {
    actionSummary?: null | string;
    browserInfo?: null | string;
    clientIpAddress?: null | string;
    executionDuration: number;
    executionTime: string;
    exceptionMessage?: null | string;
    hasException: boolean;
    httpMethod?: null | string;
    httpStatusCode?: null | number;
    id: string;
    tenantName?: null | string;
    url?: null | string;
    userName?: null | string;
  }

  export interface AuditLogQuery {
    keyword?: string;
    maxResultCount?: number;
  }
}

export async function getAuditLogListApi(params?: AuditLogApi.AuditLogQuery) {
  return requestClient.get<AuditLogApi.AuditLogItem[]>('/app/audit-log/list', {
    params,
  });
}
