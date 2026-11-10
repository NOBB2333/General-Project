import type { ApiRequestOptions } from '@/api/client';

export namespace IntegrationBindingsApi {
  export interface ConnectorOption {
    connectorCode: string;
    displayName: string;
    id: string;
    providerType: string;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface BindingQuery {
    connectorId?: string;
    keyword?: string;
    maxResultCount: number;
    skipCount: number;
    status?: string;
  }

  export interface ExternalAccount {
    avatarUrl: string;
    connectorCode: string;
    connectorId: string;
    departmentExternalId: string;
    displayName: string;
    email: string;
    externalUnionId: string;
    externalUserId: string;
    id: string;
    lastSyncedTime: null | string;
    mobile: string;
    providerType: string;
    rawJson: string;
    status: string;
  }

  export interface ExternalAccountSaveInput {
    avatarUrl?: string;
    connectorId: string;
    departmentExternalId?: string;
    displayName?: string;
    email?: string;
    externalUnionId?: string;
    externalUserId: string;
    mobile?: string;
    rawJson?: string;
    status: string;
  }

  export interface ExternalAccountBatchInput {
    accounts: ExternalAccountSaveInput[];
    connectorId: string;
  }

  export interface BatchResult {
    createdCount: number;
    updatedCount: number;
  }

  export interface UserBinding {
    boundTime: null | string;
    connectorCode: string;
    connectorId: string;
    conflictReason: string;
    externalAccountId: string;
    externalDisplayName: string;
    externalUserId: string;
    id: string;
    internalUserId: null | string;
    internalUserName: string;
    matchType: string;
    providerType: string;
    remark: string;
    status: string;
  }

  export interface UserBindingSaveInput {
    externalAccountId: string;
    internalUserId: string;
    matchType: string;
    remark?: string;
  }

  export interface UserBindingResolveInput {
    internalUserId?: null | string;
    remark?: string;
    status: string;
  }

  export interface AutoMatchResult {
    boundCount: number;
    conflictCount: number;
    pendingCount: number;
    scannedCount: number;
  }

  export interface DepartmentBinding {
    connectorCode: string;
    connectorId: string;
    conflictReason: string;
    externalDepartmentId: string;
    externalDepartmentName: string;
    id: string;
    internalOrganizationUnitId: null | string;
    internalOrganizationUnitName: string;
    matchType: string;
    parentExternalDepartmentId: string;
    providerType: string;
    remark: string;
    status: string;
  }

  export interface DepartmentBindingSaveInput {
    connectorId: string;
    externalDepartmentId: string;
    externalDepartmentName?: string;
    internalOrganizationUnitId?: null | string;
    matchType: string;
    parentExternalDepartmentId?: string;
    remark?: string;
    status: string;
  }

  export interface GroupBinding {
    connectorCode: string;
    connectorId: string;
    externalGroupId: string;
    externalGroupName: string;
    groupType: string;
    id: string;
    internalObjectId: string;
    internalObjectName: string;
    internalObjectType: string;
    providerType: string;
    remark: string;
    robotCode: string;
    status: string;
  }

  export interface GroupBindingSaveInput {
    connectorId: string;
    externalGroupId: string;
    externalGroupName?: string;
    groupType: string;
    internalObjectId?: string;
    internalObjectName?: string;
    internalObjectType?: string;
    remark?: string;
    robotCode?: string;
    status: string;
  }

  export interface IdMapping {
    connectorCode: string;
    connectorId: string;
    externalObjectId: string;
    externalObjectType: string;
    id: string;
    internalObjectId: string;
    internalObjectType: string;
    providerType: string;
    remark: string;
    source: string;
    status: string;
  }

  export interface IdMappingSaveInput {
    connectorId: string;
    externalObjectId: string;
    externalObjectType: string;
    internalObjectId: string;
    internalObjectType: string;
    remark?: string;
    source: string;
    status: string;
  }
}

export interface IntegrationBindingsApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const encodeId = (id: string) => encodeURIComponent(id);

export function createIntegrationBindingsApi(client: IntegrationBindingsApiClient) {
  const rootPath = '/app/integration/bindings';

  return {
    getConnectors(signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.ConnectorOption[]>(`${rootPath}/connectors`, { signal });
    },
    getExternalAccounts(query: IntegrationBindingsApi.BindingQuery, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.PagedResult<IntegrationBindingsApi.ExternalAccount>>(
        `${rootPath}/external-accounts`,
        { query: { ...query }, signal },
      );
    },
    saveExternalAccount(input: IntegrationBindingsApi.ExternalAccountSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.ExternalAccount>(`${rootPath}/external-accounts`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    importExternalAccounts(input: IntegrationBindingsApi.ExternalAccountBatchInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.BatchResult>(`${rootPath}/external-accounts/import`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getUserBindings(query: IntegrationBindingsApi.BindingQuery, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.PagedResult<IntegrationBindingsApi.UserBinding>>(
        `${rootPath}/users`,
        { query: { ...query }, signal },
      );
    },
    autoMatchUsers(connectorId: string, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.AutoMatchResult>(`${rootPath}/users/auto-match`, {
        body: { connectorId },
        method: 'POST',
        signal,
      });
    },
    bindUser(input: IntegrationBindingsApi.UserBindingSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.UserBinding>(`${rootPath}/users`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    resolveUserBinding(id: string, input: IntegrationBindingsApi.UserBindingResolveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.UserBinding>(`${rootPath}/users/${encodeId(id)}/resolve`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getDepartmentBindings(query: IntegrationBindingsApi.BindingQuery, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.PagedResult<IntegrationBindingsApi.DepartmentBinding>>(
        `${rootPath}/departments`,
        { query: { ...query }, signal },
      );
    },
    saveDepartmentBinding(input: IntegrationBindingsApi.DepartmentBindingSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.DepartmentBinding>(`${rootPath}/departments`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getGroupBindings(query: IntegrationBindingsApi.BindingQuery, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.PagedResult<IntegrationBindingsApi.GroupBinding>>(
        `${rootPath}/groups`,
        { query: { ...query }, signal },
      );
    },
    saveGroupBinding(input: IntegrationBindingsApi.GroupBindingSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.GroupBinding>(`${rootPath}/groups`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getIdMappings(query: IntegrationBindingsApi.BindingQuery, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.PagedResult<IntegrationBindingsApi.IdMapping>>(
        `${rootPath}/id-mappings`,
        { query: { ...query }, signal },
      );
    },
    saveIdMapping(input: IntegrationBindingsApi.IdMappingSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationBindingsApi.IdMapping>(`${rootPath}/id-mappings`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
  };
}
