import type { ApiRequestOptions } from '@/api/client';

export namespace AuditLogApi {
  export interface Item {
    id: string;
    actionSummary: string | null;
    browserInfo: string | null;
    category: string | null;
    clientIpAddress: string | null;
    executionDuration: number;
    executionTime: string;
    exceptionMessage: string | null;
    hasException: boolean;
    httpMethod: string | null;
    httpStatusCode: number | null;
    hostOperatorUserId: string | null;
    hostOperatorUserName: string | null;
    impersonatedUserId: string | null;
    impersonatedUserName: string | null;
    isHostTenantOperation: boolean;
    menuTitle: string | null;
    operationSessionId: string | null;
    operationTenantId: string | null;
    tenantName: string | null;
    url: string | null;
    userName: string | null;
  }

  export interface Query {
    category?: string;
    endTime?: string;
    keyword?: string;
    maxResultCount: number;
    skipCount: number;
    startTime?: string;
  }

  export interface PagedResult {
    items: Item[];
    totalCount: number;
  }

  export interface StatItem {
    count: number;
    key: string;
    label: string;
  }

  export interface Dashboard {
    accessLogs: Item[];
    auditLogs: Item[];
    exceptionLogs: Item[];
    operationLogs: Item[];
    topApis: StatItem[];
    topMenus: StatItem[];
    topPages: StatItem[];
    topUsers: StatItem[];
  }

  export interface PageVisitInput {
    menuPath: string;
    menuTitle: string;
  }
}

interface AuditLogClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createAuditLogApi(client: AuditLogClient) {
  return {
    getPaged(query: AuditLogApi.Query, signal?: AbortSignal): Promise<AuditLogApi.PagedResult> {
      return client.request('/app/audit-log/paged', { query: { ...query }, signal });
    },
    getDashboard(query: AuditLogApi.Query, signal?: AbortSignal): Promise<AuditLogApi.Dashboard> {
      return client.request('/app/audit-log/dashboard', { query: { ...query }, signal });
    },
    reportPageVisit(input: AuditLogApi.PageVisitInput): Promise<boolean> {
      return client.request('/app/audit-log/page-visit', { method: 'POST', body: input });
    },
  };
}
