import { requestClient } from '#/api/request';

export namespace OrganizationApi {
  export interface OrganizationMemberTransferInput {
    targetOrganizationUnitId: string;
    userIds: string[];
  }

  export interface OrganizationSaveInput {
    displayName: string;
    parentId?: null | string;
  }

  export interface OrganizationTreeItem {
    children: OrganizationTreeItem[];
    code: string;
    directMemberCount: number;
    disabled: boolean;
    displayName: string;
    id: string;
    memberCount: number;
    parentId: null | string;
  }
}

export async function getOrganizationTreeApi() {
  return requestClient.get<OrganizationApi.OrganizationTreeItem[]>(
    '/app/organization-unit/tree',
  );
}

export async function createOrganizationUnitApi(
  data: OrganizationApi.OrganizationSaveInput,
) {
  return requestClient.post<boolean>('/app/organization-unit', data);
}

export async function updateOrganizationUnitApi(
  id: string,
  data: OrganizationApi.OrganizationSaveInput,
) {
  return requestClient.put<boolean>(`/app/organization-unit/${id}`, data);
}

export async function moveOrganizationUnitApi(id: string, parentId?: null | string) {
  return requestClient.put<boolean>(`/app/organization-unit/${id}/move`, {
    parentId,
  });
}

export async function transferOrganizationMembersApi(
  id: string,
  data: OrganizationApi.OrganizationMemberTransferInput,
) {
  return requestClient.put<boolean>(`/app/organization-unit/${id}/members/transfer`, data);
}

export async function deleteOrganizationUnitApi(id: string) {
  return requestClient.delete<boolean>(`/app/organization-unit/${id}`);
}
