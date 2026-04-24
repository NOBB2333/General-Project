import { requestClient } from '#/api/request';

export namespace WorkspaceApi {
  export interface AttentionItem {
    detail: string;
    key: string;
    level: string;
    link?: null | string;
    title: string;
  }

  export interface SummaryItem {
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

export async function getPlatformWorkspaceSummaryApi() {
  return requestClient.get<WorkspaceApi.SummaryItem>('/app/platform/workspace/summary');
}
