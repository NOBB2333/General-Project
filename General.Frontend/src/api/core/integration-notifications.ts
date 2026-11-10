import type { ApiRequestOptions } from '@/api/client';

export namespace IntegrationNotificationsApi {
  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface Template {
    bodyTemplate: string;
    cardJson: string;
    creationTime: string;
    id: string;
    messageType: string;
    name: string;
    previewVariablesJson: string;
    remark: string;
    status: string;
    templateCode: string;
    titleTemplate: string;
    variableSchemaJson: string;
  }

  export interface TemplateQuery {
    keyword?: string;
    maxResultCount: number;
    messageType?: string;
    skipCount: number;
    status?: string;
  }

  export interface TemplateSaveInput {
    bodyTemplate: string;
    cardJson: string;
    id?: string;
    messageType: string;
    name: string;
    previewVariablesJson: string;
    remark: string;
    status: string;
    templateCode: string;
    titleTemplate: string;
    variableSchemaJson: string;
  }

  export interface TemplateRenderInput {
    bodyTemplate: string;
    cardJson: string;
    templateId?: string;
    titleTemplate: string;
    variablesJson: string;
  }

  export interface TemplateRenderResult {
    missingVariables: string[];
    renderedBody: string;
    renderedCardJson: string;
    renderedTitle: string;
  }

  export interface Rule {
    channelType: string;
    conditionJson: string;
    connectorCode: string;
    connectorId: null | string;
    creationTime: string;
    eventCode: string;
    eventName: string;
    id: string;
    lastTriggeredTime: null | string;
    name: string;
    providerType: string;
    quietEndTime: string;
    quietStartTime: string;
    rateLimitMaxCount: number;
    rateLimitWindowMinutes: number;
    recipientRuleJson: string;
    remark: string;
    ruleCode: string;
    status: string;
    targetType: string;
    templateCode: string;
    templateId: string;
    templateName: string;
  }

  export interface RuleQuery {
    connectorId?: string;
    eventCode?: string;
    keyword?: string;
    maxResultCount: number;
    skipCount: number;
    status?: string;
  }

  export interface RuleSaveInput {
    channelType: string;
    conditionJson: string;
    connectorId: null | string;
    eventCode: string;
    eventName: string;
    id?: string;
    name: string;
    quietEndTime: string;
    quietStartTime: string;
    rateLimitMaxCount: number;
    rateLimitWindowMinutes: number;
    recipientRuleJson: string;
    remark: string;
    ruleCode: string;
    status: string;
    targetType: string;
    templateId: string;
  }

  export interface RuleEvaluationInput {
    eventCode: string;
    occurredAt?: string;
    variablesJson: string;
  }

  export interface RuleEvaluationItem {
    blockReason: string;
    canSend: boolean;
    channelType: string;
    connectorCode: string;
    connectorId: null | string;
    missingVariables: string[];
    renderedBody: string;
    renderedCardJson: string;
    renderedTitle: string;
    ruleId: string;
    ruleCode: string;
    ruleName: string;
    targetType: string;
    templateId: string;
    templateName: string;
  }

  export interface RuleEvaluationResult {
    blockedCount: number;
    eventCode: string;
    items: RuleEvaluationItem[];
    readyCount: number;
    totalRuleCount: number;
  }
}

export interface IntegrationNotificationsApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createIntegrationNotificationsApi(client: IntegrationNotificationsApiClient) {
  const rootPath = '/app/integration';

  return {
    getTemplates(query: IntegrationNotificationsApi.TemplateQuery, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.PagedResult<IntegrationNotificationsApi.Template>>(
        `${rootPath}/templates`,
        {
          query: {
            keyword: query.keyword,
            maxResultCount: query.maxResultCount,
            messageType: query.messageType,
            skipCount: query.skipCount,
            status: query.status,
          },
          signal,
        },
      );
    },
    saveTemplate(input: IntegrationNotificationsApi.TemplateSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.Template>(`${rootPath}/templates`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    renderTemplate(input: IntegrationNotificationsApi.TemplateRenderInput, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.TemplateRenderResult>(`${rootPath}/templates/render`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getRules(query: IntegrationNotificationsApi.RuleQuery, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.PagedResult<IntegrationNotificationsApi.Rule>>(
        `${rootPath}/notification-rules`,
        {
          query: {
            connectorId: query.connectorId,
            eventCode: query.eventCode,
            keyword: query.keyword,
            maxResultCount: query.maxResultCount,
            skipCount: query.skipCount,
            status: query.status,
          },
          signal,
        },
      );
    },
    saveRule(input: IntegrationNotificationsApi.RuleSaveInput, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.Rule>(`${rootPath}/notification-rules`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    toggleRule(id: string, status: string, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.Rule>(
        `${rootPath}/notification-rules/${encodeURIComponent(id)}/toggle`,
        { body: { status }, method: 'POST', signal },
      );
    },
    evaluateRules(input: IntegrationNotificationsApi.RuleEvaluationInput, signal?: AbortSignal) {
      return client.request<IntegrationNotificationsApi.RuleEvaluationResult>(
        `${rootPath}/notification-rules/evaluate`,
        {
          body: { ...input, commitTriggered: false },
          method: 'POST',
          signal,
        },
      );
    },
  };
}
