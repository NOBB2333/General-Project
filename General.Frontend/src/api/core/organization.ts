import { apiClient } from '@/api/request';

export namespace OrganizationApi {
  export interface TreeItem {
    children: TreeItem[];
    code: string;
    disabled: boolean;
    displayName: string;
    directMemberCount: number;
    id: string;
    memberCount: number;
    parentId: null | string;
  }

  export interface SaveInput {
    displayName: string;
    parentId: null | string;
  }

  export interface MoveInput {
    parentId: null | string;
  }

  export interface TransferMembersInput {
    targetOrganizationUnitId: string;
    userIds: string[];
  }
}

export function getOrganizationUnitTreeApi(): Promise<OrganizationApi.TreeItem[]> {
  return apiClient.request('/app/organization-unit/tree');
}

export function createOrganizationUnitApi(input: OrganizationApi.SaveInput): Promise<boolean> {
  return apiClient.request('/app/organization-unit', { body: input, method: 'POST' });
}

export function updateOrganizationUnitApi(id: string, input: OrganizationApi.SaveInput): Promise<boolean> {
  return apiClient.request(`/app/organization-unit/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function moveOrganizationUnitApi(id: string, input: OrganizationApi.MoveInput): Promise<boolean> {
  return apiClient.request(`/app/organization-unit/${encodeURIComponent(id)}/move`, { body: input, method: 'PUT' });
}

export function deleteOrganizationUnitApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/organization-unit/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function transferOrganizationMembersApi(
  id: string,
  input: OrganizationApi.TransferMembersInput,
): Promise<boolean> {
  return apiClient.request(`/app/organization-unit/${encodeURIComponent(id)}/members/transfer`, {
    body: input,
    method: 'PUT',
  });
}
