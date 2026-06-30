import { requestClient } from '#/api/request';

export namespace IntegrationApi {
  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface ConnectorItem {
    capabilities: string[];
    configJson: string;
    configSchemaJson: string;
    connectorCode: string;
    displayName: string;
    hasSecret: boolean;
    icon: string;
    id: string;
    isBuiltin: boolean;
    isConfigured: boolean;
    isEnabled: boolean;
    isTenantEnabled: boolean;
    lastTestMessage: string;
    lastTestStatus: string;
    lastTestTime?: string;
    providerType: string;
    remark: string;
    sort: number;
    tenantConfigId?: string;
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

  export interface ConnectorHealthItem {
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
    lastTestTime?: string;
    providerType: string;
  }

  export interface ConnectorHealthSummary {
    disabledCount: number;
    healthyCount: number;
    items: ConnectorHealthItem[];
    totalCount: number;
    unconfiguredCount: number;
    unhealthyCount: number;
    warningCount: number;
  }

  export interface MessageSendTestInput {
    body: string;
    messageType: string;
    targetId: string;
    targetType: string;
  }

  export interface MessageSendResult {
    deliveryRecordId?: string;
    externalMessageId: string;
    message: string;
    status: string;
    success: boolean;
  }

  export interface DeliveryConnectorRate {
    connectorCode: string;
    displayName: string;
    failedCount: number;
    successRate: number;
    succeededCount: number;
    totalCount: number;
  }

  export interface DeliveryRecordItem {
    attemptCount: number;
    businessKey: string;
    businessModule: string;
    connectorCode: string;
    connectorId: string;
    creationTime: string;
    deadLetterReason: string;
    durationMilliseconds: number;
    errorMessage: string;
    externalMessageId: string;
    id: string;
    lastAttemptTime?: string;
    maxRetryCount: number;
    messageType: string;
    nextRetryTime?: string;
    payloadPreview: string;
    providerType: string;
    requestJson: string;
    responseJson: string;
    sourceType: string;
    status: string;
    targetId: string;
    targetType: string;
  }

  export interface DeliveryRecordQuery {
    connectorCode?: string;
    createdFrom?: string;
    createdTo?: string;
    keyword?: string;
    maxResultCount?: number;
    messageType?: string;
    skipCount?: number;
    status?: string;
    targetType?: string;
  }

  export interface DeliverySkipInput {
    reason: string;
  }

  export interface DeliverySummary {
    averageDurationMilliseconds: number;
    connectorRates: DeliveryConnectorRate[];
    deadLetterCount: number;
    failedCount: number;
    pendingCount: number;
    skippedCount: number;
    succeededCount: number;
    totalCount: number;
  }

  export interface SyncJobItem {
    configJson: string;
    connectorCode: string;
    connectorId: string;
    cronExpression: string;
    direction: string;
    id: string;
    isEnabled: boolean;
    jobCode: string;
    lastRunError: string;
    lastRunStatus: string;
    lastRunSummary: string;
    lastRunTime?: string;
    mode: string;
    name: string;
    providerType: string;
    remark: string;
    syncType: string;
  }

  export interface SyncJobQuery {
    connectorCode?: string;
    keyword?: string;
    maxResultCount?: number;
    providerType?: string;
    skipCount?: number;
    status?: string;
    syncType?: string;
  }

  export interface SyncJobSaveInput {
    configJson: string;
    connectorId: string;
    cronExpression: string;
    direction: string;
    id?: string;
    isEnabled: boolean;
    jobCode: string;
    mode: string;
    name: string;
    remark: string;
    syncType: string;
  }

  export interface SyncJobToggleInput {
    isEnabled: boolean;
  }

  export interface SyncRunInput {
    force: boolean;
    mode: string;
    requestJson: string;
  }

  export interface SyncRecordItem {
    conflictCount: number;
    conflictJson: string;
    connectorCode: string;
    connectorId: string;
    createdCount: number;
    creationTime: string;
    durationMilliseconds: number;
    errorMessage: string;
    failedCount: number;
    finishedTime?: string;
    id: string;
    mode: string;
    providerType: string;
    requestJson: string;
    resultJson: string;
    retryCount: number;
    skippedCount: number;
    startedTime?: string;
    status: string;
    summary: string;
    syncJobId: string;
    syncType: string;
    totalCount: number;
    updatedCount: number;
  }

  export interface SyncRecordQuery {
    connectorCode?: string;
    keyword?: string;
    maxResultCount?: number;
    mode?: string;
    providerType?: string;
    skipCount?: number;
    status?: string;
    syncJobId?: string;
    syncType?: string;
  }

  export interface SyncRecordSkipInput {
    reason: string;
  }

  export interface WebhookEventItem {
    actionCode: string;
    businessKey: string;
    connectorCode: string;
    connectorId?: string;
    creationTime: string;
    dedupeKey: string;
    errorMessage: string;
    eventId: string;
    eventType: string;
    headersJson: string;
    id: string;
    normalizedPayloadJson: string;
    providerType: string;
    rawBody: string;
    rawPayloadJson: string;
    resolvedTime?: string;
    retryCount: number;
    signatureStatus: string;
    sourceIp: string;
    status: string;
  }

  export interface WebhookEventQuery {
    eventType?: string;
    keyword?: string;
    maxResultCount?: number;
    providerType?: string;
    skipCount?: number;
    status?: string;
  }

  export interface WebhookReceiveInput {
    actionCode: string;
    businessKey: string;
    connectorCode: string;
    dedupeKey: string;
    eventId: string;
    eventType: string;
    headersJson: string;
    providerType: string;
    rawBody: string;
    rawPayloadJson: string;
    signatureStatus: string;
    sourceIp: string;
  }

  export interface WebhookSkipInput {
    reason: string;
  }

  export interface ExternalAccountItem {
    avatarUrl: string;
    connectorCode: string;
    connectorId: string;
    departmentExternalId: string;
    displayName: string;
    email: string;
    externalUnionId: string;
    externalUserId: string;
    id: string;
    lastSyncedTime?: string;
    mobile: string;
    providerType: string;
    rawJson: string;
    status: string;
  }

  export interface ExternalAccountQuery {
    connectorCode?: string;
    connectorId?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface ExternalAccountSaveInput {
    avatarUrl: string;
    connectorId: string;
    departmentExternalId: string;
    displayName: string;
    email: string;
    externalUnionId: string;
    externalUserId: string;
    mobile: string;
    rawJson: string;
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

  export interface UserBindingItem {
    boundTime?: string;
    connectorCode: string;
    connectorId: string;
    conflictReason: string;
    externalAccountId: string;
    externalDisplayName: string;
    externalUserId: string;
    id: string;
    internalUserId?: string;
    internalUserName: string;
    matchType: string;
    providerType: string;
    remark: string;
    status: string;
  }

  export interface UserBindingQuery {
    connectorCode?: string;
    connectorId?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface UserBindingSaveInput {
    externalAccountId: string;
    internalUserId: string;
    matchType: string;
    remark: string;
  }

  export interface SelfBindingInput {
    externalAccountId: string;
    remark: string;
  }

  export interface BindingResolveInput {
    internalUserId?: string;
    remark: string;
    status: string;
  }

  export interface AutoMatchInput {
    connectorId: string;
  }

  export interface AutoMatchResult {
    boundCount: number;
    conflictCount: number;
    pendingCount: number;
    scannedCount: number;
  }

  export interface DepartmentBindingItem {
    connectorCode: string;
    connectorId: string;
    conflictReason: string;
    externalDepartmentId: string;
    externalDepartmentName: string;
    id: string;
    internalOrganizationUnitId?: string;
    internalOrganizationUnitName: string;
    matchType: string;
    parentExternalDepartmentId: string;
    providerType: string;
    remark: string;
    status: string;
  }

  export interface DepartmentBindingQuery {
    connectorId?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface DepartmentBindingSaveInput {
    connectorId: string;
    externalDepartmentId: string;
    externalDepartmentName: string;
    internalOrganizationUnitId?: string;
    matchType: string;
    parentExternalDepartmentId: string;
    remark: string;
    status: string;
  }

  export interface GroupBindingItem {
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

  export interface GroupBindingQuery {
    connectorId?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface GroupBindingSaveInput {
    connectorId: string;
    externalGroupId: string;
    externalGroupName: string;
    groupType: string;
    internalObjectId: string;
    internalObjectName: string;
    internalObjectType: string;
    remark: string;
    robotCode: string;
    status: string;
  }

  export interface IdMappingItem {
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

  export interface IdMappingQuery {
    connectorId?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface IdMappingSaveInput {
    connectorId: string;
    externalObjectId: string;
    externalObjectType: string;
    internalObjectId: string;
    internalObjectType: string;
    remark: string;
    source: string;
    status: string;
  }

  export interface MessageTemplateItem {
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

  export interface MessageTemplateQuery {
    keyword?: string;
    maxResultCount?: number;
    messageType?: string;
    skipCount?: number;
    status?: string;
  }

  export interface MessageTemplateSaveInput {
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

  export interface NotificationRuleItem {
    channelType: string;
    conditionJson: string;
    connectorCode: string;
    connectorId?: string;
    creationTime: string;
    eventCode: string;
    eventName: string;
    id: string;
    lastTriggeredTime?: string;
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

  export interface NotificationRuleQuery {
    connectorId?: string;
    eventCode?: string;
    keyword?: string;
    maxResultCount?: number;
    skipCount?: number;
    status?: string;
  }

  export interface NotificationRuleSaveInput {
    channelType: string;
    conditionJson: string;
    connectorId?: string;
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

  export interface NotificationRuleToggleInput {
    status: string;
  }

  export interface NotificationEventInput {
    commitTriggered: boolean;
    eventCode: string;
    occurredAt?: string;
    variablesJson: string;
  }

  export interface NotificationRuleEvaluationItem {
    blockReason: string;
    canSend: boolean;
    channelType: string;
    connectorCode: string;
    connectorId?: string;
    missingVariables: string[];
    renderedBody: string;
    renderedCardJson: string;
    renderedTitle: string;
    ruleCode: string;
    ruleId: string;
    ruleName: string;
    targetType: string;
    templateId: string;
    templateName: string;
  }

  export interface NotificationEvaluationResult {
    blockedCount: number;
    eventCode: string;
    items: NotificationRuleEvaluationItem[];
    readyCount: number;
    totalRuleCount: number;
  }

  export interface NotificationDispatchItem {
    blockReason: string;
    deliveryRecordId?: string;
    message: string;
    ruleCode: string;
    ruleName: string;
    status: string;
    targetId: string;
    targetType: string;
  }

  export interface NotificationDispatchResult {
    blockedCount: number;
    failedCount: number;
    items: NotificationDispatchItem[];
    noTargetCount: number;
    sentCount: number;
    skippedCount: number;
  }
}

export async function getIntegrationConnectorsApi(params: IntegrationApi.ConnectorQuery = {}) {
  return requestClient.get<IntegrationApi.ConnectorItem[]>('/app/integration/connectors', { params });
}

export async function getIntegrationConnectorHealthApi() {
  return requestClient.get<IntegrationApi.ConnectorHealthSummary>('/app/integration/connectors/health');
}

export async function saveIntegrationTenantConfigApi(id: string, data: IntegrationApi.TenantConfigSaveInput) {
  return requestClient.put<IntegrationApi.ConnectorItem>(`/app/integration/connectors/${id}/tenant-config`, data);
}

export async function toggleIntegrationConnectorApi(id: string, data: IntegrationApi.ToggleInput) {
  return requestClient.post<IntegrationApi.ConnectorItem>(`/app/integration/connectors/${id}/toggle`, data);
}

export async function rotateIntegrationConnectorSecretApi(id: string, data: IntegrationApi.SecretRotateInput) {
  return requestClient.post<IntegrationApi.ConnectorItem>(`/app/integration/connectors/${id}/rotate-secret`, data);
}

export async function testIntegrationConnectorApi(id: string) {
  return requestClient.post<IntegrationApi.ConnectionTestResult>(`/app/integration/connectors/${id}/test`);
}

export async function sendIntegrationTestMessageApi(id: string, data: IntegrationApi.MessageSendTestInput) {
  return requestClient.post<IntegrationApi.MessageSendResult>(`/app/integration/connectors/${id}/send-test`, data);
}

export async function getIntegrationMessageTemplatesApi(params: IntegrationApi.MessageTemplateQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.MessageTemplateItem>>('/app/integration/templates', {
    params,
  });
}

export async function saveIntegrationMessageTemplateApi(data: IntegrationApi.MessageTemplateSaveInput) {
  return requestClient.post<IntegrationApi.MessageTemplateItem>('/app/integration/templates', data);
}

export async function renderIntegrationMessageTemplateApi(data: IntegrationApi.TemplateRenderInput) {
  return requestClient.post<IntegrationApi.TemplateRenderResult>('/app/integration/templates/render', data);
}

export async function getIntegrationNotificationRulesApi(params: IntegrationApi.NotificationRuleQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.NotificationRuleItem>>(
    '/app/integration/notification-rules',
    { params },
  );
}

export async function saveIntegrationNotificationRuleApi(data: IntegrationApi.NotificationRuleSaveInput) {
  return requestClient.post<IntegrationApi.NotificationRuleItem>('/app/integration/notification-rules', data);
}

export async function toggleIntegrationNotificationRuleApi(id: string, data: IntegrationApi.NotificationRuleToggleInput) {
  return requestClient.post<IntegrationApi.NotificationRuleItem>(`/app/integration/notification-rules/${id}/toggle`, data);
}

export async function evaluateIntegrationNotificationRulesApi(data: IntegrationApi.NotificationEventInput) {
  return requestClient.post<IntegrationApi.NotificationEvaluationResult>('/app/integration/notification-rules/evaluate', data);
}

export async function dispatchIntegrationNotificationRulesApi(data: IntegrationApi.NotificationEventInput) {
  return requestClient.post<IntegrationApi.NotificationDispatchResult>('/app/integration/notification-rules/dispatch', data);
}

export async function getIntegrationDeliveryRecordsApi(params: IntegrationApi.DeliveryRecordQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.DeliveryRecordItem>>('/app/integration/delivery-records', {
    params,
  });
}

export async function getIntegrationDeliverySummaryApi(params: IntegrationApi.DeliveryRecordQuery = {}) {
  return requestClient.get<IntegrationApi.DeliverySummary>('/app/integration/delivery-records/summary', { params });
}

export async function exportIntegrationDeliveryRecordsApi(params: IntegrationApi.DeliveryRecordQuery = {}) {
  return requestClient.get<Blob>('/app/integration/delivery-records/export', {
    params,
    responseReturn: 'body',
    responseType: 'blob',
  });
}

export async function getIntegrationSyncJobsApi(params: IntegrationApi.SyncJobQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.SyncJobItem>>('/app/integration/sync-jobs', {
    params,
  });
}

export async function saveIntegrationSyncJobApi(data: IntegrationApi.SyncJobSaveInput) {
  return requestClient.post<IntegrationApi.SyncJobItem>('/app/integration/sync-jobs', data);
}

export async function toggleIntegrationSyncJobApi(id: string, data: IntegrationApi.SyncJobToggleInput) {
  return requestClient.post<IntegrationApi.SyncJobItem>(`/app/integration/sync-jobs/${id}/toggle`, data);
}

export async function runIntegrationSyncJobApi(id: string, data: IntegrationApi.SyncRunInput) {
  return requestClient.post<IntegrationApi.SyncRecordItem>(`/app/integration/sync-jobs/${id}/run`, data);
}

export async function getIntegrationSyncRecordsApi(params: IntegrationApi.SyncRecordQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.SyncRecordItem>>('/app/integration/sync-records', {
    params,
  });
}

export async function retryIntegrationSyncRecordApi(id: string) {
  return requestClient.post<IntegrationApi.SyncRecordItem>(`/app/integration/sync-records/${id}/retry`);
}

export async function skipIntegrationSyncRecordApi(id: string, data: IntegrationApi.SyncRecordSkipInput) {
  return requestClient.post<IntegrationApi.SyncRecordItem>(`/app/integration/sync-records/${id}/skip`, data);
}

export async function retryIntegrationDeliveryApi(id: string) {
  return requestClient.post<IntegrationApi.DeliveryRecordItem>(`/app/integration/delivery-records/${id}/retry`);
}

export async function skipIntegrationDeliveryApi(id: string, data: IntegrationApi.DeliverySkipInput) {
  return requestClient.post<IntegrationApi.DeliveryRecordItem>(`/app/integration/delivery-records/${id}/skip`, data);
}

export async function getIntegrationExternalAccountsApi(params: IntegrationApi.ExternalAccountQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.ExternalAccountItem>>(
    '/app/integration/bindings/external-accounts',
    { params },
  );
}

export async function saveIntegrationExternalAccountApi(data: IntegrationApi.ExternalAccountSaveInput) {
  return requestClient.post<IntegrationApi.ExternalAccountItem>('/app/integration/bindings/external-accounts', data);
}

export async function importIntegrationExternalAccountsApi(data: IntegrationApi.ExternalAccountBatchInput) {
  return requestClient.post<IntegrationApi.BatchResult>('/app/integration/bindings/external-accounts/import', data);
}

export async function getIntegrationUserBindingsApi(params: IntegrationApi.UserBindingQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.UserBindingItem>>('/app/integration/bindings/users', {
    params,
  });
}

export async function autoMatchIntegrationUsersApi(data: IntegrationApi.AutoMatchInput) {
  return requestClient.post<IntegrationApi.AutoMatchResult>('/app/integration/bindings/users/auto-match', data);
}

export async function bindIntegrationUserApi(data: IntegrationApi.UserBindingSaveInput) {
  return requestClient.post<IntegrationApi.UserBindingItem>('/app/integration/bindings/users', data);
}

export async function bindCurrentIntegrationUserApi(data: IntegrationApi.SelfBindingInput) {
  return requestClient.post<IntegrationApi.UserBindingItem>('/app/integration/bindings/users/self', data);
}

export async function resolveIntegrationUserBindingApi(id: string, data: IntegrationApi.BindingResolveInput) {
  return requestClient.post<IntegrationApi.UserBindingItem>(`/app/integration/bindings/users/${id}/resolve`, data);
}

export async function getIntegrationDepartmentBindingsApi(params: IntegrationApi.DepartmentBindingQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.DepartmentBindingItem>>(
    '/app/integration/bindings/departments',
    { params },
  );
}

export async function saveIntegrationDepartmentBindingApi(data: IntegrationApi.DepartmentBindingSaveInput) {
  return requestClient.post<IntegrationApi.DepartmentBindingItem>('/app/integration/bindings/departments', data);
}

export async function getIntegrationGroupBindingsApi(params: IntegrationApi.GroupBindingQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.GroupBindingItem>>('/app/integration/bindings/groups', {
    params,
  });
}

export async function saveIntegrationGroupBindingApi(data: IntegrationApi.GroupBindingSaveInput) {
  return requestClient.post<IntegrationApi.GroupBindingItem>('/app/integration/bindings/groups', data);
}

export async function getIntegrationIdMappingsApi(params: IntegrationApi.IdMappingQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.IdMappingItem>>(
    '/app/integration/bindings/id-mappings',
    { params },
  );
}

export async function saveIntegrationIdMappingApi(data: IntegrationApi.IdMappingSaveInput) {
  return requestClient.post<IntegrationApi.IdMappingItem>('/app/integration/bindings/id-mappings', data);
}

export async function getIntegrationWebhookEventsApi(params: IntegrationApi.WebhookEventQuery = {}) {
  return requestClient.get<IntegrationApi.PagedResult<IntegrationApi.WebhookEventItem>>(
    '/app/integration/webhook-events',
    { params },
  );
}

export async function receiveIntegrationWebhookEventApi(data: IntegrationApi.WebhookReceiveInput) {
  return requestClient.post<IntegrationApi.WebhookEventItem>('/app/integration/webhook-events/receive', data);
}

export async function retryIntegrationWebhookEventApi(id: string) {
  return requestClient.post<IntegrationApi.WebhookEventItem>(`/app/integration/webhook-events/${id}/retry`);
}

export async function skipIntegrationWebhookEventApi(id: string, data: IntegrationApi.WebhookSkipInput) {
  return requestClient.post<IntegrationApi.WebhookEventItem>(`/app/integration/webhook-events/${id}/skip`, data);
}
