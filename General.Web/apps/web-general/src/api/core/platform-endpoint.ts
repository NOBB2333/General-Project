import { requestClient } from '#/api/request';

export namespace PlatformEndpointApi {
  export interface EndpointOption {
    key: string;
    label: string;
  }

  export interface EndpointGroup {
    groupName: string;
    items: EndpointOption[];
  }
}

export async function getPlatformEndpointOptionsApi() {
  return requestClient.get<PlatformEndpointApi.EndpointGroup[]>('/app/platform/endpoint/options');
}
