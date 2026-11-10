import type { ApiRequestOptions } from '@/api/client';

export namespace IntegrationConnectorsApi {
  export interface Connector {
    callbackPath: string;
    capabilities: string[];
    configJson: string;
    configSchemaJson: string;
    connectorCode: string;
    displayName: string;
    hasRobotWebhook: boolean;
    hasSecret: boolean;
    icon: string;
    id: string;
    isBuiltin: boolean;
    isConfigured: boolean;
    isEnabled: boolean;
    isTenantEnabled: boolean;
    lastTestMessage: string;
    lastTestStatus: string;
    lastTestTime: null | string;
    providerType: string;
    remark: string;
    sort: number;
    tenantConfigId: null | string;
  }

  export interface ConnectorQuery {
    includeDisabled?: boolean;
    keyword?: string;
  }

  export interface TenantConfigSaveInput {
    callbackPath: string;
    configJson: string;
    isEnabled: boolean;
    remark: string;
    robotWebhook?: string | null;
    secret: string;
  }

  export interface ToggleInput {
    isEnabled: boolean;
  }

  export interface SecretRotateInput {
    secret: string;
  }

  export interface ConnectionTestResult {
    message: string;
    success: boolean;
  }

  export interface ConnectorHealth {
    connectorCode: string;
    displayName: string;
    hasSecret: boolean;
    healthMessage: string;
    healthStatus: string;
    id: string;
    isConfigured: boolean;
    isEnabled: boolean;
    isTenantEnabled: boolean;
    lastTestMessage: string;
    lastTestStatus: string;
    lastTestTime: null | string;
    providerType: string;
  }

  export interface ConnectorHealthSummary {
    disabledCount: number;
    healthyCount: number;
    items: ConnectorHealth[];
    totalCount: number;
    unconfiguredCount: number;
    unhealthyCount: number;
    warningCount: number;
  }
}

export interface IntegrationConnectorsApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const encodeId = (id: string) => encodeURIComponent(id);

export function createIntegrationConnectorsApi(client: IntegrationConnectorsApiClient) {
  const rootPath = '/app/integration/connectors';

  return {
    getConnectors(query: IntegrationConnectorsApi.ConnectorQuery = {}, signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.Connector[]>(rootPath, {
        query: { includeDisabled: query.includeDisabled, keyword: query.keyword },
        signal,
      });
    },
    getHealth(signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.ConnectorHealthSummary>(`${rootPath}/health`, { signal });
    },
    saveTenantConfig(id: string, input: IntegrationConnectorsApi.TenantConfigSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.Connector>(`${rootPath}/${encodeId(id)}/tenant-config`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    toggle(id: string, input: IntegrationConnectorsApi.ToggleInput, signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.Connector>(`${rootPath}/${encodeId(id)}/toggle`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    rotateSecret(id: string, input: IntegrationConnectorsApi.SecretRotateInput, signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.Connector>(`${rootPath}/${encodeId(id)}/rotate-secret`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    testConnection(id: string, signal?: AbortSignal) {
      return client.request<IntegrationConnectorsApi.ConnectionTestResult>(`${rootPath}/${encodeId(id)}/test`, {
        method: 'POST',
        signal,
      });
    },
  };
}
