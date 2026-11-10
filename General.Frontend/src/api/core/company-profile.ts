import type { ApiRequestOptions } from '@/api/client';

export namespace CompanyProfileApi {
  export interface Item {
    address: string;
    city: string;
    creditCode: string;
    email: string;
    fullName: string;
    hasLogo: boolean;
    industry: string;
    introduction: string;
    legalPerson: string;
    phone: string;
    platformName: string;
    province: string;
    scale: string;
    shortName: string;
    slogan: string;
    website: string;
  }

  export type SaveInput = Omit<Item, 'hasLogo'>;
}

interface CompanyProfileClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createCompanyProfileApi(client: CompanyProfileClient) {
  const rootPath = '/app/platform/company-profile';

  return {
    get(signal?: AbortSignal): Promise<CompanyProfileApi.Item> {
      return client.request(rootPath, { signal });
    },
    getLogo(signal?: AbortSignal): Promise<Blob | undefined> {
      return client.request(`${rootPath}/logo`, { responseType: 'blob', signal });
    },
    update(input: CompanyProfileApi.SaveInput, signal?: AbortSignal): Promise<boolean> {
      return client.request(rootPath, { body: input, method: 'PUT', signal });
    },
    uploadLogo(file: File, signal?: AbortSignal): Promise<boolean> {
      const body = new FormData();
      body.append('file', file);
      return client.request(`${rootPath}/logo`, { body, method: 'POST', signal });
    },
  };
}
