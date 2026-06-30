import { useAppConfig } from '@vben/hooks';
import { preferences } from '@vben/preferences';
import { useAccessStore } from '@vben/stores';

import { requestClient } from '#/api/request';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);

export namespace AiApi {
  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface ProviderItem {
    creationTime: string;
    endpoint: string;
    hasApiKey: boolean;
    id: string;
    isDefault: boolean;
    isEnabled: boolean;
    lastModificationTime?: string;
    modelCount: number;
    name: string;
    organizationId: string;
    providerType: string;
    remark: string;
  }

  export interface ProviderSaveInput {
    apiKey: string;
    configJson: string;
    endpoint: string;
    isDefault: boolean;
    isEnabled: boolean;
    name: string;
    organizationId: string;
    providerType: string;
    remark: string;
  }

  export interface ProviderTestResult {
    message: string;
    success: boolean;
  }

  export interface DiscoveredModelItem {
    displayName: string;
    modelCode: string;
    modelType: string;
  }

  export interface ProviderModelDiscoveryResult {
    createdCount: number;
    message: string;
    models: DiscoveredModelItem[];
    success: boolean;
    updatedCount: number;
  }

  export interface ModelItem {
    capabilities: string[];
    contextWindow: number;
    currency: string;
    displayName: string;
    fallbackModelId?: string;
    fallbackModelName: string;
    id: string;
    inputTokenPrice: number;
    isDefault: boolean;
    isEnabled: boolean;
    maxOutputTokens: number;
    modelCode: string;
    modelType: string;
    outputTokenPrice: number;
    providerId: string;
    providerName: string;
    remark: string;
    sort: number;
  }

  export interface ModelSaveInput {
    capabilities: string[];
    contextWindow: number;
    currency: string;
    displayName: string;
    fallbackModelId?: string;
    inputTokenPrice: number;
    isDefault: boolean;
    isEnabled: boolean;
    maxOutputTokens: number;
    modelCode: string;
    modelType: string;
    outputTokenPrice: number;
    providerId: string;
    remark: string;
    sort: number;
  }

  export interface ConversationItem {
    archivedTime?: string;
    completionTokens: number;
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    creationTime: string;
    id: string;
    lastMessageSummary: string;
    lastMessageTime?: string;
    messageCount: number;
    modelId?: string;
    modelName: string;
    mode: string;
    promptTokens: number;
    status: string;
    title: string;
    totalTokens: number;
  }

  export interface ConversationDetail extends ConversationItem {
    messages: MessageItem[];
  }

  export interface ConversationCreateInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    mode: string;
    modelId?: string;
    title: string;
  }

  export interface ConversationUpdateInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    modelId?: string;
    title: string;
  }

  export interface ConversationQuery {
    keyword?: string;
    status?: string;
  }

  export interface ConversationSendInput {
    agentCode?: string;
    commandCode?: string;
    content: string;
    modelId?: string;
    skillCode?: string;
  }

  export interface ConversationSendResult {
    conversation: ConversationItem;
    messages: MessageItem[];
  }

  export interface ConversationStreamEvent {
    delta: string;
    errorMessage: string;
    event: 'completed' | 'delta' | 'error' | 'userMessage';
    result?: ConversationSendResult;
    userMessage?: MessageItem;
  }

  export interface ConversationExportResult {
    content: string;
    fileName: string;
  }

  export interface MessageItem {
    citations: string[];
    completionTokens: number;
    content: string;
    conversationId: string;
    creationTime: string;
    errorMessage: string;
    id: string;
    metadataJson: string;
    modelId?: string;
    modelName: string;
    promptTokens: number;
    providerId?: string;
    providerName: string;
    role: string;
    sequenceNo: number;
    status: string;
    totalTokens: number;
  }

  export interface PlanConfirmInput {
    confirmed: boolean;
    messageId: string;
  }

  export interface PlanDraftInput {
    messageId: string;
    title: string;
  }

  export interface PlanStepToggleInput {
    isDone: boolean;
    messageId: string;
    stepId: string;
  }

  export interface ToolDefinitionItem {
    category: string;
    creationTime: string;
    description: string;
    id: string;
    inputSchemaJson: string;
    isEnabled: boolean;
    name: string;
    outputSchemaJson: string;
    permissionCode: string;
    sort: number;
    timeoutSeconds: number;
    toolCode: string;
  }

  export interface SourceReferenceItem {
    path: string;
    sourceId: string;
    sourceType: string;
    title: string;
  }

  export interface ToolInvokeInput {
    conversationId?: string;
    inputJson: string;
    messageId?: string;
  }

  export interface ToolInvokeResult {
    elapsedMilliseconds: number;
    errorMessage: string;
    invocationId: string;
    outputJson: string;
    sourceReferences: SourceReferenceItem[];
    status: string;
    toolCode: string;
  }

  export interface ToolInvocationItem extends ToolInvokeResult {
    conversationId?: string;
    creationTime: string;
    inputJson: string;
    messageId?: string;
    timeoutSeconds: number;
  }

  export interface ToolInvocationQuery {
    conversationId?: string;
    maxResultCount?: number;
    messageId?: string;
    status?: string;
    toolCode?: string;
  }

  export interface AssistantItem {
    assistantCode: string;
    configJson: string;
    creationTime: string;
    defaultMode: string;
    defaultModelId?: string;
    defaultModelName: string;
    description: string;
    id: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    name: string;
    roleType: string;
    sort: number;
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface AssistantSaveInput {
    assistantCode: string;
    configJson: string;
    defaultMode: string;
    defaultModelId?: string;
    description: string;
    isEnabled: boolean;
    name: string;
    roleType: string;
    sort: number;
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface PromptTemplateItem {
    assistantCode: string;
    assistantName: string;
    category: string;
    creationTime: string;
    description: string;
    id: string;
    inputSchemaJson: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    mode: string;
    name: string;
    outputSchemaJson: string;
    promptContent: string;
    scenario: string;
    sort: number;
    templateCode: string;
    toolCodes: string[];
    version: number;
  }

  export interface PromptTemplateSaveInput {
    assistantCode: string;
    category: string;
    description: string;
    inputSchemaJson: string;
    isEnabled: boolean;
    mode: string;
    name: string;
    outputSchemaJson: string;
    promptContent: string;
    scenario: string;
    sort: number;
    templateCode: string;
    toolCodes: string[];
    version: number;
  }

  export interface PromptTemplateQuery {
    assistantCode?: string;
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    scenario?: string;
  }

  export interface PromptTemplateConversationCreateInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    inputJson: string;
    modelId?: string;
    title: string;
  }

  export interface PromptTemplateDryRunInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    inputJson: string;
  }

  export interface PromptTemplateDryRunBatchInput extends PromptTemplateDryRunInput {
    includeDisabled: boolean;
    onlyBuiltin: boolean;
    templateCodes: string[];
  }

  export interface PromptTemplateDryRunResult {
    assistantCode: string;
    assistantName: string;
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    displayContent: string;
    errors: string[];
    estimatedCompletionTokens: number;
    estimatedPromptTokens: number;
    inputJson: string;
    isModelCallSkipped: boolean;
    missingToolCodes: string[];
    mode: string;
    renderedPrompt: string;
    recommendedToolCodes: string[];
    scenario: string;
    structuredOutputJson: string;
    success: boolean;
    templateCode: string;
    templateId: string;
    templateName: string;
    warnings: string[];
  }

  export interface PromptTemplateDryRunBatchResult {
    failedCount: number;
    items: PromptTemplateDryRunResult[];
    missingRequiredTemplateCodes: string[];
    passedCount: number;
    success: boolean;
    totalCount: number;
  }

  export interface ScenarioAcceptanceInput extends PromptTemplateConversationCreateInput {
    templateCodes: string[];
  }

  export interface ScenarioAcceptanceItem {
    acceptanceCode: string;
    acceptanceName: string;
    assistantMessagePreview: string;
    conversationId?: string;
    errorMessage: string;
    mode: string;
    scenario: string;
    success: boolean;
    templateCode: string;
    templateId?: string;
    templateName: string;
    validationMessages: string[];
  }

  export interface ScenarioAcceptanceResult {
    failedCount: number;
    items: ScenarioAcceptanceItem[];
    missingRequiredTemplateCodes: string[];
    passedCount: number;
    success: boolean;
    totalCount: number;
  }

  export interface AgentDesignQuery {
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    roleType?: string;
  }

  export interface SkillItem {
    category: string;
    creationTime: string;
    defaultMode: string;
    description: string;
    id: string;
    inputSchemaJson: string;
    instruction: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    name: string;
    outputSchemaJson: string;
    skillCode: string;
    sort: number;
    toolCodes: string[];
  }

  export interface SkillSaveInput {
    category: string;
    defaultMode: string;
    description: string;
    inputSchemaJson: string;
    instruction: string;
    isEnabled: boolean;
    name: string;
    outputSchemaJson: string;
    skillCode: string;
    sort: number;
    toolCodes: string[];
  }

  export interface PersonaItem {
    boundaryPrompt: string;
    configJson: string;
    creationTime: string;
    description: string;
    id: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    name: string;
    personaCode: string;
    personalityPrompt: string;
    roleType: string;
    sort: number;
    stylePrompt: string;
  }

  export interface PersonaSaveInput {
    boundaryPrompt: string;
    configJson: string;
    description: string;
    isEnabled: boolean;
    name: string;
    personaCode: string;
    personalityPrompt: string;
    roleType: string;
    sort: number;
    stylePrompt: string;
  }

  export interface CommandItem {
    agentCode: string;
    commandCode: string;
    configJson: string;
    creationTime: string;
    description: string;
    exampleInput: string;
    id: string;
    inputPlaceholder: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    mode: string;
    name: string;
    promptTemplateCode: string;
    skillCode: string;
    slashName: string;
    sort: number;
  }

  export interface CommandSaveInput {
    agentCode: string;
    commandCode: string;
    configJson: string;
    description: string;
    exampleInput: string;
    inputPlaceholder: string;
    isEnabled: boolean;
    mode: string;
    name: string;
    promptTemplateCode: string;
    skillCode: string;
    slashName: string;
    sort: number;
  }

  export interface AgentDefinitionItem {
    agentCode: string;
    creationTime: string;
    defaultModelId?: string;
    defaultModelName: string;
    description: string;
    id: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    name: string;
    personaCode: string;
    roleType: string;
    runtimeConfigJson: string;
    skillCodes: string[];
    sort: number;
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface AgentDefinitionSaveInput {
    agentCode: string;
    defaultModelId?: string;
    description: string;
    isEnabled: boolean;
    name: string;
    personaCode: string;
    roleType: string;
    runtimeConfigJson: string;
    skillCodes: string[];
    sort: number;
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface AgentRunPlan {
    agentCode: string;
    agentName: string;
    modelName: string;
    personaCode: string;
    skillCodes: string[];
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface ArtifactItem {
    artifactCode: string;
    category: string;
    creationTime: string;
    description: string;
    examplePayloadJson: string;
    id: string;
    isBuiltin: boolean;
    isEnabled: boolean;
    jsonSchema: string;
    name: string;
    rendererType: string;
    sort: number;
    systemHint: string;
  }

  export interface ArtifactSaveInput {
    artifactCode: string;
    category: string;
    description: string;
    examplePayloadJson: string;
    isEnabled: boolean;
    jsonSchema: string;
    name: string;
    rendererType: string;
    sort: number;
    systemHint: string;
  }

  export interface ArtifactQuery {
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    rendererType?: string;
  }

  export interface WebSearchConfigItem {
    allowDomains: string[];
    configJson: string;
    creationTime: string;
    denyDomains: string[];
    endpoint: string;
    hasApiKey: boolean;
    id: string;
    isDefault: boolean;
    isEnabled: boolean;
    maxResults: number;
    name: string;
    providerType: string;
    remark: string;
    timeoutSeconds: number;
  }

  export interface WebSearchConfigSaveInput {
    allowDomains: string[];
    apiKey: string;
    configJson: string;
    denyDomains: string[];
    endpoint: string;
    isDefault: boolean;
    isEnabled: boolean;
    maxResults: number;
    name: string;
    providerType: string;
    remark: string;
    timeoutSeconds: number;
  }

  export interface WebSearchRecordItem {
    configId?: string;
    conversationId?: string;
    creationTime: string;
    elapsedMilliseconds: number;
    errorMessage: string;
    id: string;
    messageId?: string;
    providerName: string;
    query: string;
    requestJson: string;
    responseSummaryJson: string;
    resultCount: number;
    status: string;
  }

  export interface WebSearchRecordQuery {
    configId?: string;
    conversationId?: string;
    keyword?: string;
    maxResultCount?: number;
    status?: string;
  }

  export interface WebSearchInput {
    configId?: string;
    conversationId?: string;
    maxResults: number;
    messageId?: string;
    query: string;
  }

  export interface WebSearchResult {
    errorMessage: string;
    recordId: string;
    responseSummaryJson: string;
    resultCount: number;
    status: string;
  }

  export interface McpServerItem {
    argumentsJson: string;
    command: string;
    configJson: string;
    creationTime: string;
    endpoint: string;
    environmentJson: string;
    headersJson: string;
    id: string;
    isEnabled: boolean;
    lastDiscoveryTime?: string;
    lastErrorMessage: string;
    name: string;
    permissionCode: string;
    protocolVersion: string;
    remark: string;
    serverCode: string;
    sort: number;
    timeoutSeconds: number;
    toolCount: number;
    transportType: string;
  }

  export interface McpServerSaveInput {
    argumentsJson: string;
    command: string;
    configJson: string;
    endpoint: string;
    environmentJson: string;
    headersJson: string;
    isEnabled: boolean;
    name: string;
    permissionCode: string;
    protocolVersion: string;
    remark: string;
    serverCode: string;
    sort: number;
    timeoutSeconds: number;
    transportType: string;
  }

  export interface McpConnectionTestResult {
    errorMessage: string;
    rawResponseJson: string;
    success: boolean;
  }

  export interface McpToolItem {
    creationTime: string;
    description: string;
    id: string;
    inputSchemaJson: string;
    isEnabled: boolean;
    lastDiscoveredTime: string;
    permissionCode: string;
    serverId: string;
    serverName: string;
    toolName: string;
  }

  export interface McpToolPolicyInput {
    isEnabled: boolean;
    permissionCode: string;
  }

  export interface McpInvokeInput {
    argumentsJson: string;
  }

  export interface McpInvokeResult {
    elapsedMilliseconds: number;
    errorMessage: string;
    invocationId: string;
    outputJson: string;
    status: string;
    toolName: string;
  }

  export interface McpInvocationItem extends McpInvokeResult {
    creationTime: string;
    inputJson: string;
    serverId: string;
    serverName: string;
    timeoutSeconds: number;
    toolId?: string;
  }

  export interface McpInvocationQuery {
    maxResultCount?: number;
    serverId?: string;
    status?: string;
    toolId?: string;
    toolName?: string;
  }

  export interface McpDiscoveryResult {
    errorMessage: string;
    toolCount: number;
    tools: McpToolItem[];
  }

  export interface MemoryItem {
    content: string;
    contextId: string;
    contextType: string;
    creationTime: string;
    id: string;
    importance: number;
    isEnabled: boolean;
    knowledgeSourceId?: string;
    knowledgeSourceTitle: string;
    lastUsedTime?: string;
    memoryType: string;
    ownerUserId?: string;
    sourceReferencesJson: string;
    summary: string;
    tagsJson: string;
    title: string;
    useCount: number;
  }

  export interface MemorySaveInput {
    content: string;
    contextId: string;
    contextType: string;
    importance: number;
    isEnabled: boolean;
    knowledgeSourceId?: string;
    memoryType: string;
    ownerUserId?: string;
    sourceReferencesJson: string;
    summary: string;
    tagsJson: string;
    title: string;
  }

  export interface MemoryQuery {
    contextId?: string;
    contextType?: string;
    includeDisabled?: boolean;
    keyword?: string;
    knowledgeSourceId?: string;
    maxResultCount?: number;
    memoryType?: string;
    ownerUserId?: string;
  }

  export interface MemoryToggleInput {
    isEnabled: boolean;
  }

  export interface KnowledgeSourceItem {
    contentText: string;
    contextId: string;
    contextType: string;
    creationTime: string;
    id: string;
    lastIndexedTime: string;
    metadataJson: string;
    projectId?: string;
    sourceId: string;
    sourceType: string;
    status: string;
    summary: string;
    title: string;
  }

  export interface KnowledgeSourceQuery {
    contextId?: string;
    contextType?: string;
    keyword?: string;
    maxResultCount?: number;
    projectId?: string;
    sourceType?: string;
  }

  export interface MemoryImportResult {
    importedCount: number;
    projectId: string;
    skippedCount: number;
    sourceType: string;
  }

  export interface JobItem {
    assistantCode: string;
    contextScopeJson: string;
    creationTime: string;
    description: string;
    id: string;
    isEnabled: boolean;
    jobCode: string;
    jobType: string;
    lastRunStatus: string;
    lastRunSummary: string;
    lastRunTime?: string;
    modelId?: string;
    modelName: string;
    name: string;
    outputTargetJson: string;
    promptTemplateCode: string;
    schedulerJobKey: string;
  }

  export interface JobSaveInput {
    assistantCode: string;
    contextScopeJson: string;
    description: string;
    isEnabled: boolean;
    jobCode: string;
    jobType: string;
    modelId?: string;
    name: string;
    outputTargetJson: string;
    promptTemplateCode: string;
    schedulerJobKey: string;
  }

  export interface JobQuery {
    includeDisabled?: boolean;
    jobType?: string;
    keyword?: string;
    maxResultCount?: number;
    schedulerJobKey?: string;
  }

  export interface JobToggleInput {
    isEnabled: boolean;
  }

  export interface JobRunItem {
    aiJobId: string;
    draftItemCount: number;
    elapsedMilliseconds: number;
    errorMessage: string;
    finishedTime?: string;
    id: string;
    inputSnapshotJson: string;
    jobCode: string;
    jobName: string;
    jobType: string;
    outputDraftJson: string;
    runMode: string;
    startedTime: string;
    status: string;
  }

  export interface JobRunQuery {
    aiJobId?: string;
    jobCode?: string;
    jobType?: string;
    maxResultCount?: number;
    runMode?: string;
    startedFrom?: string;
    startedTo?: string;
    status?: string;
  }

  export interface JobRunResult {
    job: JobItem;
    run: JobRunItem;
    summary: string;
  }

  export interface UsageRecordItem {
    auditSummary: string;
    completionTokens: number;
    conversationId?: string;
    costAmount: number;
    creationTime: string;
    currency: string;
    elapsedMilliseconds: number;
    errorMessage: string;
    id: string;
    jobId?: string;
    modelCode: string;
    modelId?: string;
    modelName: string;
    projectId?: string;
    promptInjectionRiskScore: number;
    promptInjectionSignalsJson: string;
    promptTemplateCode: string;
    promptTokens: number;
    providerId?: string;
    providerName: string;
    requestPreview: string;
    responsePreview: string;
    scenario: string;
    source: string;
    status: string;
    toolCode: string;
    totalTokens: number;
    userId?: string;
    userName: string;
  }

  export interface UsageQuery {
    conversationId?: string;
    createdFrom?: string;
    createdTo?: string;
    keyword?: string;
    maxResultCount?: number;
    modelId?: string;
    projectId?: string;
    promptTemplateCode?: string;
    providerId?: string;
    scenario?: string;
    source?: string;
    status?: string;
    toolCode?: string;
    userId?: string;
  }

  export interface UsageBucket {
    costAmount: number;
    count: number;
    key: string;
    label: string;
    totalTokens: number;
  }

  export interface UsageDashboard {
    callCount: number;
    costAmount: number;
    currency: string;
    failedCount: number;
    highRiskPromptCount: number;
    modelBuckets: UsageBucket[];
    projectBuckets: UsageBucket[];
    scenarioBuckets: UsageBucket[];
    templateBuckets: UsageBucket[];
    toolCallCount: number;
    totalTokens: number;
    userBuckets: UsageBucket[];
    warnings: string[];
  }

  export interface WritebackRequestItem {
    actionCode: string;
    approvedTime?: string;
    approverUserId?: string;
    businessModule: string;
    conversationId?: string;
    creationTime: string;
    diffPreviewJson: string;
    errorMessage: string;
    id: string;
    messageId?: string;
    payloadJson: string;
    projectId?: string;
    rejectReason: string;
    sourceType: string;
    status: string;
    summary: string;
    targetEntityId: string;
    targetEntityType: string;
  }

  export interface WritebackRequestQuery {
    actionCode?: string;
    businessModule?: string;
    conversationId?: string;
    createdFrom?: string;
    createdTo?: string;
    keyword?: string;
    maxResultCount?: number;
    projectId?: string;
    skipCount?: number;
    status?: string;
    targetEntityType?: string;
  }

  export interface WritebackRequestCreateInput {
    actionCode: string;
    businessModule: string;
    conversationId?: string;
    diffPreviewJson: string;
    messageId?: string;
    payloadJson: string;
    projectId?: string;
    sourceType: string;
    summary: string;
    targetEntityId: string;
    targetEntityType: string;
  }

  export interface WritebackDecisionInput {
    reason: string;
  }
}

export async function getAiProvidersApi() {
  return requestClient.get<AiApi.ProviderItem[]>('/app/ai/providers');
}

export async function createAiProviderApi(data: AiApi.ProviderSaveInput) {
  return requestClient.post<AiApi.ProviderItem>('/app/ai/providers', data);
}

export async function updateAiProviderApi(id: string, data: AiApi.ProviderSaveInput) {
  return requestClient.put<AiApi.ProviderItem>(`/app/ai/providers/${id}`, data);
}

export async function testAiProviderApi(id: string) {
  return requestClient.post<AiApi.ProviderTestResult>(`/app/ai/providers/${id}/test`);
}

export async function discoverAiProviderModelsApi(id: string) {
  return requestClient.post<AiApi.ProviderModelDiscoveryResult>(`/app/ai/providers/${id}/models/discover`);
}

export async function syncAiProviderModelsApi(id: string) {
  return requestClient.post<AiApi.ProviderModelDiscoveryResult>(`/app/ai/providers/${id}/models/sync`);
}

export async function deleteAiProviderApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/providers/${id}`);
}

export async function getAiModelsApi(providerId?: string) {
  return requestClient.get<AiApi.ModelItem[]>('/app/ai/models', {
    params: providerId ? { providerId } : undefined,
  });
}

export async function createAiModelApi(data: AiApi.ModelSaveInput) {
  return requestClient.post<AiApi.ModelItem>('/app/ai/models', data);
}

export async function updateAiModelApi(id: string, data: AiApi.ModelSaveInput) {
  return requestClient.put<AiApi.ModelItem>(`/app/ai/models/${id}`, data);
}

export async function deleteAiModelApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/models/${id}`);
}

export async function getAiConversationsApi(params: AiApi.ConversationQuery = {}) {
  return requestClient.get<AiApi.ConversationItem[]>('/app/ai/conversations', { params });
}

export async function getAiConversationApi(id: string) {
  return requestClient.get<AiApi.ConversationDetail>(`/app/ai/conversations/${id}`);
}

export async function createAiConversationApi(data: AiApi.ConversationCreateInput) {
  return requestClient.post<AiApi.ConversationDetail>('/app/ai/conversations', data);
}

export async function updateAiConversationApi(id: string, data: AiApi.ConversationUpdateInput) {
  return requestClient.put<AiApi.ConversationItem>(`/app/ai/conversations/${id}`, data);
}

export async function sendAiConversationMessageApi(id: string, data: AiApi.ConversationSendInput) {
  return requestClient.post<AiApi.ConversationSendResult>(`/app/ai/conversations/${id}/messages`, data);
}

export async function streamAiConversationMessageApi(
  id: string,
  data: AiApi.ConversationSendInput,
  onEvent: (event: AiApi.ConversationStreamEvent) => void,
) {
  const accessStore = useAccessStore();
  const response = await fetch(`${apiURL}/app/ai/conversations/${id}/messages/stream`, {
    body: JSON.stringify(data),
    headers: {
      'Accept-Language': preferences.app.locale,
      Authorization: accessStore.accessToken ? `Bearer ${accessStore.accessToken}` : '',
      'Content-Type': 'application/json',
    },
    method: 'POST',
  });
  if (!response.ok || !response.body) {
    const text = await response.text();
    throw new Error(text || `AI 流式响应失败：${response.status}`);
  }

  const decoder = new TextDecoder();
  const reader = response.body.getReader();
  let buffer = '';
  while (true) {
    const { done, value } = await reader.read();
    if (value) {
      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop() || '';
      for (const line of lines) {
        const text = line.trim();
        if (!text) {
          continue;
        }
        onEvent(JSON.parse(text) as AiApi.ConversationStreamEvent);
      }
    }

    if (done) {
      break;
    }
  }

  const remaining = buffer.trim();
  if (remaining) {
    onEvent(JSON.parse(remaining) as AiApi.ConversationStreamEvent);
  }
}

export async function confirmAiConversationPlanApi(id: string, data: AiApi.PlanConfirmInput) {
  return requestClient.post<AiApi.MessageItem>(`/app/ai/conversations/${id}/plan/confirm`, data);
}

export async function toggleAiConversationPlanStepApi(id: string, data: AiApi.PlanStepToggleInput) {
  return requestClient.post<AiApi.MessageItem>(`/app/ai/conversations/${id}/plan/steps/toggle`, data);
}

export async function createAiConversationPlanDraftApi(id: string, data: AiApi.PlanDraftInput) {
  return requestClient.post<AiApi.ConversationDetail>(`/app/ai/conversations/${id}/plan/draft`, data);
}

export async function archiveAiConversationApi(id: string) {
  return requestClient.post<AiApi.ConversationItem>(`/app/ai/conversations/${id}/archive`);
}

export async function restoreAiConversationApi(id: string) {
  return requestClient.post<AiApi.ConversationItem>(`/app/ai/conversations/${id}/restore`);
}

export async function exportAiConversationApi(id: string) {
  return requestClient.get<AiApi.ConversationExportResult>(`/app/ai/conversations/${id}/export`);
}

export async function deleteAiConversationApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/conversations/${id}`);
}

export async function getAiToolsApi() {
  return requestClient.get<AiApi.ToolDefinitionItem[]>('/app/ai/tools');
}

export async function invokeAiToolApi(toolCode: string, data: AiApi.ToolInvokeInput) {
  return requestClient.post<AiApi.ToolInvokeResult>(`/app/ai/tools/${encodeURIComponent(toolCode)}/invoke`, data);
}

export async function getAiToolInvocationsApi(params: AiApi.ToolInvocationQuery = {}) {
  return requestClient.get<AiApi.ToolInvocationItem[]>('/app/ai/tool-invocations', { params });
}

export async function getAiAssistantsApi(includeDisabled = true) {
  return requestClient.get<AiApi.AssistantItem[]>('/app/ai/assistants', {
    params: { includeDisabled },
  });
}

export async function createAiAssistantApi(data: AiApi.AssistantSaveInput) {
  return requestClient.post<AiApi.AssistantItem>('/app/ai/assistants', data);
}

export async function updateAiAssistantApi(id: string, data: AiApi.AssistantSaveInput) {
  return requestClient.put<AiApi.AssistantItem>(`/app/ai/assistants/${id}`, data);
}

export async function deleteAiAssistantApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/assistants/${id}`);
}

export async function getAiPromptTemplatesApi(params: AiApi.PromptTemplateQuery = {}) {
  return requestClient.get<AiApi.PromptTemplateItem[]>('/app/ai/prompt-templates', { params });
}

export async function createAiPromptTemplateApi(data: AiApi.PromptTemplateSaveInput) {
  return requestClient.post<AiApi.PromptTemplateItem>('/app/ai/prompt-templates', data);
}

export async function updateAiPromptTemplateApi(id: string, data: AiApi.PromptTemplateSaveInput) {
  return requestClient.put<AiApi.PromptTemplateItem>(`/app/ai/prompt-templates/${id}`, data);
}

export async function deleteAiPromptTemplateApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/prompt-templates/${id}`);
}

export async function createAiConversationFromPromptTemplateApi(id: string, data: AiApi.PromptTemplateConversationCreateInput) {
  return requestClient.post<AiApi.ConversationDetail>(`/app/ai/prompt-templates/${id}/conversations`, data);
}

export async function dryRunAiPromptTemplatesApi(data: AiApi.PromptTemplateDryRunBatchInput) {
  return requestClient.post<AiApi.PromptTemplateDryRunBatchResult>('/app/ai/prompt-templates/dry-run', data);
}

export async function dryRunAiPromptTemplateApi(id: string, data: AiApi.PromptTemplateDryRunInput) {
  return requestClient.post<AiApi.PromptTemplateDryRunResult>(`/app/ai/prompt-templates/${id}/dry-run`, data);
}

export async function runAiScenarioAcceptanceApi(data: AiApi.ScenarioAcceptanceInput) {
  return requestClient.post<AiApi.ScenarioAcceptanceResult>('/app/ai/prompt-templates/acceptance-run', data);
}

export async function getAiSkillsApi(params: AiApi.AgentDesignQuery = {}) {
  return requestClient.get<AiApi.SkillItem[]>('/app/ai/skills', { params });
}

export async function createAiSkillApi(data: AiApi.SkillSaveInput) {
  return requestClient.post<AiApi.SkillItem>('/app/ai/skills', data);
}

export async function updateAiSkillApi(id: string, data: AiApi.SkillSaveInput) {
  return requestClient.put<AiApi.SkillItem>(`/app/ai/skills/${id}`, data);
}

export async function deleteAiSkillApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/skills/${id}`);
}

export async function getAiPersonasApi(params: AiApi.AgentDesignQuery = {}) {
  return requestClient.get<AiApi.PersonaItem[]>('/app/ai/personas', { params });
}

export async function createAiPersonaApi(data: AiApi.PersonaSaveInput) {
  return requestClient.post<AiApi.PersonaItem>('/app/ai/personas', data);
}

export async function updateAiPersonaApi(id: string, data: AiApi.PersonaSaveInput) {
  return requestClient.put<AiApi.PersonaItem>(`/app/ai/personas/${id}`, data);
}

export async function deleteAiPersonaApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/personas/${id}`);
}

export async function getAiCommandsApi(params: AiApi.AgentDesignQuery = {}) {
  return requestClient.get<AiApi.CommandItem[]>('/app/ai/commands', { params });
}

export async function createAiCommandApi(data: AiApi.CommandSaveInput) {
  return requestClient.post<AiApi.CommandItem>('/app/ai/commands', data);
}

export async function updateAiCommandApi(id: string, data: AiApi.CommandSaveInput) {
  return requestClient.put<AiApi.CommandItem>(`/app/ai/commands/${id}`, data);
}

export async function deleteAiCommandApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/commands/${id}`);
}

export async function getAiAgentsApi(params: AiApi.AgentDesignQuery = {}) {
  return requestClient.get<AiApi.AgentDefinitionItem[]>('/app/ai/agents', { params });
}

export async function createAiAgentApi(data: AiApi.AgentDefinitionSaveInput) {
  return requestClient.post<AiApi.AgentDefinitionItem>('/app/ai/agents', data);
}

export async function updateAiAgentApi(id: string, data: AiApi.AgentDefinitionSaveInput) {
  return requestClient.put<AiApi.AgentDefinitionItem>(`/app/ai/agents/${id}`, data);
}

export async function buildAiAgentRunPlanApi(id: string, inputJson = '{}') {
  return requestClient.post<AiApi.AgentRunPlan>(`/app/ai/agents/${id}/run-plan`, { inputJson });
}

export async function deleteAiAgentApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/agents/${id}`);
}

export async function getAiArtifactsApi(params: AiApi.ArtifactQuery = {}) {
  return requestClient.get<AiApi.ArtifactItem[]>('/app/ai/artifacts', { params });
}

export async function createAiArtifactApi(data: AiApi.ArtifactSaveInput) {
  return requestClient.post<AiApi.ArtifactItem>('/app/ai/artifacts', data);
}

export async function updateAiArtifactApi(id: string, data: AiApi.ArtifactSaveInput) {
  return requestClient.put<AiApi.ArtifactItem>(`/app/ai/artifacts/${id}`, data);
}

export async function deleteAiArtifactApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/artifacts/${id}`);
}

export async function getAiWebSearchConfigsApi(includeDisabled = true) {
  return requestClient.get<AiApi.WebSearchConfigItem[]>('/app/ai/web-search/configs', {
    params: { includeDisabled },
  });
}

export async function createAiWebSearchConfigApi(data: AiApi.WebSearchConfigSaveInput) {
  return requestClient.post<AiApi.WebSearchConfigItem>('/app/ai/web-search/configs', data);
}

export async function updateAiWebSearchConfigApi(id: string, data: AiApi.WebSearchConfigSaveInput) {
  return requestClient.put<AiApi.WebSearchConfigItem>(`/app/ai/web-search/configs/${id}`, data);
}

export async function deleteAiWebSearchConfigApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/web-search/configs/${id}`);
}

export async function getAiWebSearchRecordsApi(params: AiApi.WebSearchRecordQuery = {}) {
  return requestClient.get<AiApi.WebSearchRecordItem[]>('/app/ai/web-search/records', { params });
}

export async function runAiWebSearchApi(data: AiApi.WebSearchInput) {
  return requestClient.post<AiApi.WebSearchResult>('/app/ai/web-search/search', data);
}

export async function getAiMcpServersApi() {
  return requestClient.get<AiApi.McpServerItem[]>('/app/ai/mcp/servers');
}

export async function createAiMcpServerApi(data: AiApi.McpServerSaveInput) {
  return requestClient.post<AiApi.McpServerItem>('/app/ai/mcp/servers', data);
}

export async function updateAiMcpServerApi(id: string, data: AiApi.McpServerSaveInput) {
  return requestClient.put<AiApi.McpServerItem>(`/app/ai/mcp/servers/${id}`, data);
}

export async function deleteAiMcpServerApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/mcp/servers/${id}`);
}

export async function testAiMcpServerApi(id: string) {
  return requestClient.post<AiApi.McpConnectionTestResult>(`/app/ai/mcp/servers/${id}/test`);
}

export async function discoverAiMcpToolsApi(id: string) {
  return requestClient.post<AiApi.McpDiscoveryResult>(`/app/ai/mcp/servers/${id}/discover-tools`);
}

export async function getAiMcpToolsApi(serverId?: string) {
  return requestClient.get<AiApi.McpToolItem[]>('/app/ai/mcp/tools', {
    params: serverId ? { serverId } : undefined,
  });
}

export async function updateAiMcpToolPolicyApi(id: string, data: AiApi.McpToolPolicyInput) {
  return requestClient.put<AiApi.McpToolItem>(`/app/ai/mcp/tools/${id}/policy`, data);
}

export async function invokeAiMcpToolApi(id: string, data: AiApi.McpInvokeInput) {
  return requestClient.post<AiApi.McpInvokeResult>(`/app/ai/mcp/tools/${id}/invoke`, data);
}

export async function getAiMcpInvocationsApi(params: AiApi.McpInvocationQuery = {}) {
  return requestClient.get<AiApi.McpInvocationItem[]>('/app/ai/mcp/invocations', { params });
}

export async function getAiMemoriesApi(params: AiApi.MemoryQuery = {}) {
  return requestClient.get<AiApi.MemoryItem[]>('/app/ai/memories', { params });
}

export async function createAiMemoryApi(data: AiApi.MemorySaveInput) {
  return requestClient.post<AiApi.MemoryItem>('/app/ai/memories', data);
}

export async function updateAiMemoryApi(id: string, data: AiApi.MemorySaveInput) {
  return requestClient.put<AiApi.MemoryItem>(`/app/ai/memories/${id}`, data);
}

export async function toggleAiMemoryApi(id: string, data: AiApi.MemoryToggleInput) {
  return requestClient.post<AiApi.MemoryItem>(`/app/ai/memories/${id}/toggle`, data);
}

export async function deleteAiMemoryApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/memories/${id}`);
}

export async function getAiKnowledgeSourcesApi(params: AiApi.KnowledgeSourceQuery = {}) {
  return requestClient.get<AiApi.KnowledgeSourceItem[]>('/app/ai/knowledge-sources', { params });
}

export async function importAiProjectDocumentMemoriesApi(projectId: string) {
  return requestClient.post<AiApi.MemoryImportResult>(`/app/ai/knowledge-sources/project/${projectId}/documents/import`);
}

export async function importAiProjectMeetingMemoriesApi(projectId: string) {
  return requestClient.post<AiApi.MemoryImportResult>(`/app/ai/knowledge-sources/project/${projectId}/meetings/import`);
}

export async function importAiProjectRiskMemoriesApi(projectId: string) {
  return requestClient.post<AiApi.MemoryImportResult>(`/app/ai/knowledge-sources/project/${projectId}/risks/import`);
}

export async function getAiJobsApi(params: AiApi.JobQuery = {}) {
  return requestClient.get<AiApi.JobItem[]>('/app/ai/jobs', { params });
}

export async function createAiJobApi(data: AiApi.JobSaveInput) {
  return requestClient.post<AiApi.JobItem>('/app/ai/jobs', data);
}

export async function updateAiJobApi(id: string, data: AiApi.JobSaveInput) {
  return requestClient.put<AiApi.JobItem>(`/app/ai/jobs/${id}`, data);
}

export async function toggleAiJobApi(id: string, data: AiApi.JobToggleInput) {
  return requestClient.post<AiApi.JobItem>(`/app/ai/jobs/${id}/toggle`, data);
}

export async function deleteAiJobApi(id: string) {
  return requestClient.delete<boolean>(`/app/ai/jobs/${id}`);
}

export async function runAiJobApi(id: string) {
  return requestClient.post<AiApi.JobRunResult>(`/app/ai/jobs/${id}/run`);
}

export async function getAiJobRunsApi(params: AiApi.JobRunQuery = {}) {
  return requestClient.get<AiApi.JobRunItem[]>('/app/ai/job-runs', { params });
}

export async function getAiUsageDashboardApi(params: AiApi.UsageQuery = {}) {
  return requestClient.get<AiApi.UsageDashboard>('/app/ai/usage/dashboard', { params });
}

export async function getAiUsageRecordsApi(params: AiApi.UsageQuery = {}) {
  return requestClient.get<AiApi.UsageRecordItem[]>('/app/ai/usage/records', { params });
}

export async function getAiWritebacksApi(params: AiApi.WritebackRequestQuery = {}) {
  return requestClient.get<AiApi.PagedResult<AiApi.WritebackRequestItem>>('/app/ai/writebacks', { params });
}

export async function createAiWritebackApi(data: AiApi.WritebackRequestCreateInput) {
  return requestClient.post<AiApi.WritebackRequestItem>('/app/ai/writebacks', data);
}

export async function approveAiWritebackApi(id: string) {
  return requestClient.post<AiApi.WritebackRequestItem>(`/app/ai/writebacks/${id}/approve`);
}

export async function rejectAiWritebackApi(id: string, data: AiApi.WritebackDecisionInput) {
  return requestClient.post<AiApi.WritebackRequestItem>(`/app/ai/writebacks/${id}/reject`, data);
}

export async function cancelAiWritebackApi(id: string) {
  return requestClient.post<AiApi.WritebackRequestItem>(`/app/ai/writebacks/${id}/cancel`);
}
