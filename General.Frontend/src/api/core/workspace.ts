import { apiClient } from '@/api/request';

export namespace WorkspaceApi {
  export interface AttentionItem {
    detail: string;
    key: string;
    level: string;
    link: null | string;
    title: string;
  }

  export interface Summary {
    attentionItems: AttentionItem[];
    exceptionLogCount: number;
    fileCount: number;
    onlineUserCount: number;
    organizationCount: number;
    roleCount: number;
    tenantCount: number;
    updateLogCount: number;
    userCount: number;
  }
}

export function getWorkspaceSummaryApi(): Promise<WorkspaceApi.Summary> {
  return apiClient.request('/app/platform/workspace/summary');
}
