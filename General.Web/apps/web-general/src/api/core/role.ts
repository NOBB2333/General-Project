import { requestClient } from '#/api/request';

export namespace RoleApi {
  export interface RoleItem {
    description: string;
    homePath: string;
    id: string;
    menuCount: number;
    name: string;
    userCount: number;
  }

  export interface RoleSaveInput {
    name: string;
  }
}

export async function getRoleListApi() {
  return requestClient.get<RoleApi.RoleItem[]>('/app/role/list');
}

export async function createRoleApi(data: RoleApi.RoleSaveInput) {
  return requestClient.post<boolean>('/app/role', data);
}

export async function deleteRoleApi(id: string) {
  return requestClient.delete<boolean>(`/app/role/${id}`);
}
