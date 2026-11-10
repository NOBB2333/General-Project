import type { createApiClient } from '../client';

export namespace ProjectPlanTemplateApi {
  export interface Item {
    code: string;
    creationTime: string;
    description: string;
    id: string;
    isEnabled: boolean;
    lastModificationTime: null | string;
    name: string;
    nodeCount: number;
  }

  export interface Detail extends Item {
    nodes: Node[];
  }

  export interface Node {
    clientKey: string;
    deliveryDescription: string;
    estimateSummary: string;
    id: string;
    name: string;
    parentClientKey: null | string;
    scopeDescription: string;
    sortOrder: number;
    type: string;
    wbsCode: string;
  }

  export interface NodeSaveInput {
    clientKey: string;
    deliveryDescription?: string;
    estimateSummary?: string;
    id?: string;
    name: string;
    parentClientKey?: null | string;
    scopeDescription?: string;
    sortOrder: number;
    type: string;
    wbsCode: string;
  }

  export interface Page {
    items: Item[];
    totalCount: number;
  }

  export interface Query {
    filter?: string;
    maxResultCount: number;
    skipCount: number;
  }

  export interface SaveInput {
    description?: string;
    isEnabled: boolean;
    name: string;
    nodes: NodeSaveInput[];
  }
}

export function createProjectPlanTemplateApi(client: ReturnType<typeof createApiClient>) {
  const basePath = '/app/project/plan-templates';

  return {
    create(input: ProjectPlanTemplateApi.SaveInput, signal?: AbortSignal) {
      return client.request<string>(basePath, { body: input, method: 'POST', signal });
    },
    delete(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${basePath}/${encodeURIComponent(id)}`, { method: 'DELETE', signal });
    },
    get(id: string, signal?: AbortSignal) {
      return client.request<ProjectPlanTemplateApi.Detail>(`${basePath}/${encodeURIComponent(id)}`, { signal });
    },
    getList(input: ProjectPlanTemplateApi.Query, signal?: AbortSignal) {
      return client.request<ProjectPlanTemplateApi.Page>(basePath, { query: { ...input }, signal });
    },
    update(id: string, input: ProjectPlanTemplateApi.SaveInput, signal?: AbortSignal) {
      return client.request<boolean>(`${basePath}/${encodeURIComponent(id)}`, { body: input, method: 'PUT', signal });
    },
  };
}
