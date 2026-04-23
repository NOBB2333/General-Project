import { requestClient } from '#/api/request';

export namespace AuditLogApi {
  export interface AuditLogItem {
    actionSummary?: null | string;
    browserInfo?: null | string;
    category?: null | string;
    clientIpAddress?: null | string;
    executionDuration: number;
    executionTime: string;
    exceptionMessage?: null | string;
    hasException: boolean;
    httpMethod?: null | string;
    httpStatusCode?: null | number;
    id: string;
    menuTitle?: null | string;
    tenantName?: null | string;
    url?: null | string;
    userName?: null | string;
  }

  export interface AuditLogQuery {
    category?: string;
    endTime?: string;
    keyword?: string;
    maxResultCount?: number;
    startTime?: string;
  }

  export interface LogStatItem {
    count: number;
    key: string;
    label: string;
  }

  export interface LogDashboard {
    accessLogs: AuditLogItem[];
    auditLogs: AuditLogItem[];
    exceptionLogs: AuditLogItem[];
    operationLogs: AuditLogItem[];
    topApis: LogStatItem[];
    topMenus: LogStatItem[];
    topPages: LogStatItem[];
    topUsers: LogStatItem[];
  }

  export interface PageVisitInput {
    menuPath?: string;
    menuTitle?: string;
  }
}

export async function getAuditLogListApi(params?: AuditLogApi.AuditLogQuery) {
  return requestClient.get<AuditLogApi.AuditLogItem[]>('/app/audit-log/list', {
    params,
  });
}

export async function getAuditLogDashboardApi(params?: AuditLogApi.AuditLogQuery) {
  return requestClient.get<AuditLogApi.LogDashboard>('/app/audit-log/dashboard', {
    params,
  });
}

export async function reportPageVisitApi(input: AuditLogApi.PageVisitInput) {
  return requestClient.post<boolean>('/app/audit-log/page-visit', input);
}
