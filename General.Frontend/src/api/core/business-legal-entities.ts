import { apiClient } from '@/api/request';
import type { ApiRequestOptions } from '@/api/client';

export namespace BusinessLegalEntityApi {
  export interface Item {
    address: string;
    bankAccount: null | string;
    bankAccountMask: string;
    bankName: string;
    canViewSensitiveFields: boolean;
    id: string;
    isDefault: boolean;
    name: string;
    shortName: string;
    taxId: string;
  }

  export interface SaveInput {
    address: string;
    bankAccount?: null | string;
    bankName: string;
    name: string;
    shortName: string;
    taxId: string;
  }
}

interface BusinessLegalEntitiesClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createBusinessLegalEntitiesApi(client: BusinessLegalEntitiesClient) {
  const rootPath = '/app/business/legal-entities';

  return {
    create(input: BusinessLegalEntityApi.SaveInput): Promise<string> {
      return client.request(rootPath, { method: 'POST', body: input });
    },
    delete(id: string): Promise<boolean> {
      return client.request(`${rootPath}/${encodeURIComponent(id)}`, { method: 'DELETE' });
    },
    getList(signal?: AbortSignal): Promise<BusinessLegalEntityApi.Item[]> {
      return client.request(rootPath, { signal });
    },
    setDefault(id: string): Promise<boolean> {
      return client.request(`${rootPath}/${encodeURIComponent(id)}/default`, { method: 'POST' });
    },
    update(id: string, input: BusinessLegalEntityApi.SaveInput): Promise<boolean> {
      return client.request(`${rootPath}/${encodeURIComponent(id)}`, { method: 'PUT', body: input });
    },
  };
}

const businessLegalEntitiesApi = createBusinessLegalEntitiesApi(apiClient);

export function getBusinessLegalEntitiesApi(signal?: AbortSignal): Promise<BusinessLegalEntityApi.Item[]> {
  return businessLegalEntitiesApi.getList(signal);
}

export function createBusinessLegalEntityApi(input: BusinessLegalEntityApi.SaveInput): Promise<string> {
  return businessLegalEntitiesApi.create(input);
}

export function updateBusinessLegalEntityApi(id: string, input: BusinessLegalEntityApi.SaveInput): Promise<boolean> {
  return businessLegalEntitiesApi.update(id, input);
}

export function setDefaultBusinessLegalEntityApi(id: string): Promise<boolean> {
  return businessLegalEntitiesApi.setDefault(id);
}

export function deleteBusinessLegalEntityApi(id: string): Promise<boolean> {
  return businessLegalEntitiesApi.delete(id);
}
