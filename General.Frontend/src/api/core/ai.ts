import type { ApiRequestOptions } from '@/api/client';
import { ApiRequestError } from '../client.ts';

export namespace AiApi {
  export interface Provider {
    creationTime: string;
    endpoint: string;
    hasApiKey: boolean;
    id: string;
    isDefault: boolean;
    isEnabled: boolean;
    lastModificationTime: null | string;
    modelCount: number;
    name: string;
    organizationId: string;
    providerType: string;
    remark: string;
  }

  export interface ProviderSaveInput {
    apiKey: string;
    configJson?: null | string;
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

  export interface DiscoveredModel {
    displayName: string;
    modelCode: string;
    modelType: string;
  }

  export interface ProviderModelDiscoveryResult {
    createdCount: number;
    models: DiscoveredModel[];
    message: string;
    success: boolean;
    updatedCount: number;
  }

  export interface Model {
    capabilities: string[];
    contextWindow: number;
    currency: string;
    displayName: string;
    fallbackModelId: null | string;
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
    fallbackModelId: null | string;
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

  export interface Assistant {
    assistantCode: string;
    configJson: string;
    creationTime: string;
    defaultMode: string;
    defaultModelId: null | string;
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
    defaultModelId: null | string;
    description: string;
    isEnabled: boolean;
    name: string;
    roleType: string;
    sort: number;
    systemPrompt: string;
    toolCodes: string[];
  }

  export interface PromptTemplate {
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

  export interface PromptTemplateQueryInput {
    assistantCode?: string;
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    scenario?: string;
  }

  export interface PromptTemplateUseOption {
    assistantName: string;
    category: string;
    description: string;
    id: string;
    inputSchemaJson: string;
    isBuiltin: boolean;
    mode: string;
    name: string;
    scenario: string;
    templateCode: string;
    version: number;
  }

  export interface PromptTemplateDryRunInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    inputJson: string;
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

  export interface PromptTemplateConversationCreateInput extends PromptTemplateDryRunInput {
    modelId: null | string;
    title: string;
  }

  export interface Skill {
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

  export interface SkillUseOption {
    category: string;
    defaultMode: string;
    description: string;
    inputSchemaJson: string;
    name: string;
    skillCode: string;
  }

  export interface Persona {
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

  export interface Command {
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
    sort: number;
    slashName: string;
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
    sort: number;
    slashName: string;
  }

  export interface Agent {
    agentCode: string;
    creationTime: string;
    defaultModelId: null | string;
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

  export interface AgentSaveInput {
    agentCode: string;
    defaultModelId: null | string;
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

  export interface AgentRunPlanInput {
    inputJson: string;
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

  export interface Artifact {
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

  export interface ArtifactQueryInput {
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    rendererType?: string;
  }

  export interface ToolDefinition {
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

  export interface ToolInvocation {
    conversationId: null | string;
    creationTime: string;
    elapsedMilliseconds: number;
    errorMessage: string;
    inputJson: string;
    invocationId: string;
    messageId: null | string;
    outputJson: string;
    sourceReferences: SourceReferenceItem[];
    status: string;
    timeoutSeconds: number;
    toolCode: string;
  }

  export interface ToolInvocationQueryInput {
    conversationId?: string;
    maxResultCount?: number;
    messageId?: string;
    status?: string;
    toolCode?: string;
  }

  export interface AgentDesignQueryInput {
    category?: string;
    includeDisabled?: boolean;
    keyword?: string;
    roleType?: string;
  }

  export interface Conversation {
    archivedTime: null | string;
    completionTokens: number;
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    creationTime: string;
    id: string;
    lastMessageSummary: string;
    lastMessageTime: null | string;
    messageCount: number;
    modelId: null | string;
    modelName: string;
    mode: string;
    promptTokens: number;
    status: string;
    title: string;
    totalTokens: number;
  }

  export interface ConversationDetail extends Conversation {
    messages: Message[];
  }

  export interface ConversationCreateInput {
    contextId: string;
    contextSnapshotJson: string;
    contextType: string;
    modelId: null | string;
    mode: string;
    title: string;
  }

  export interface ConversationUpdateInput extends Omit<ConversationCreateInput, 'mode'> {}

  export interface ConversationQueryInput {
    keyword?: string;
    status?: string;
  }

  export interface ConversationSendInput {
    agentCode?: string;
    commandCode?: string;
    content: string;
    modelId?: null | string;
    skillCode?: string;
  }

  export interface ConversationExport {
    content: string;
    fileName: string;
  }

  export interface Message {
    citations: string[];
    completionTokens: number;
    content: string;
    conversationId: string;
    creationTime: string;
    errorMessage: string;
    id: string;
    metadataJson: string;
    modelId: null | string;
    modelName: string;
    promptTokens: number;
    providerId: null | string;
    providerName: string;
    role: string;
    sequenceNo: number;
    status: string;
    totalTokens: number;
  }

  export interface ConversationSendResult {
    conversation: Conversation;
    messages: Message[];
  }

  export interface ConversationStreamEvent {
    delta: string;
    errorMessage: string;
    event: 'completed' | 'delta' | 'error' | 'userMessage' | string;
    result: ConversationSendResult | null;
    userMessage: Message | null;
  }

  export interface PlanConfirmInput {
    confirmed: boolean;
    messageId: string;
  }

  export interface PlanStepToggleInput {
    isDone: boolean;
    messageId: string;
    stepId: string;
  }

  export interface PlanDraftInput {
    messageId: string;
    title?: string;
  }

  export interface McpServer {
    argumentsJson: string;
    hasArguments: boolean;
    command: string;
    configJson: string;
    hasConfig: boolean;
    creationTime: string;
    endpoint: string;
    environmentJson: string;
    hasEnvironment: boolean;
    headersJson: string;
    hasHeaders: boolean;
    id: string;
    isEnabled: boolean;
    lastDiscoveryTime: null | string;
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

  export interface McpTool {
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

  export interface McpInvocation extends McpInvokeResult {
    creationTime: string;
    inputJson: string;
    serverId: string;
    serverName: string;
    timeoutSeconds: number;
    toolId: null | string;
  }

  export interface McpDiscoveryResult {
    errorMessage: string;
    toolCount: number;
    tools: McpTool[];
  }

  export interface McpConnectionTestResult {
    errorMessage: string;
    rawResponseJson: string;
    success: boolean;
  }

  export interface McpInvocationQueryInput {
    maxResultCount?: number;
    serverId?: string;
    status?: string;
    toolId?: string;
    toolName?: string;
  }

  export interface Memory {
    content: string;
    contextId: string;
    contextType: string;
    creationTime: string;
    id: string;
    importance: number;
    isEnabled: boolean;
    knowledgeSourceId: null | string;
    knowledgeSourceTitle: string;
    lastUsedTime: null | string;
    memoryType: string;
    ownerUserId: null | string;
    sourceReferencesJson: string;
    summary: string;
    tagsJson: string;
    title: string;
    useCount: number;
  }

  export interface MemorySaveInput {
    content: string;
    contextId: null | string;
    contextType: null | string;
    importance: number;
    isEnabled: boolean;
    knowledgeSourceId: null | string;
    memoryType: string;
    ownerUserId: null | string;
    sourceReferencesJson: string;
    summary: string;
    tagsJson: string;
    title: string;
  }

  export interface MemoryQueryInput {
    contextId?: string;
    contextType?: string;
    includeDisabled?: boolean;
    knowledgeSourceId?: string;
    keyword?: string;
    maxResultCount?: number;
    memoryType?: string;
    ownerUserId?: string;
  }

  export interface KnowledgeSource {
    contextId: string;
    contextType: string;
    contentText: string;
    creationTime: string;
    id: string;
    lastIndexedTime: string;
    metadataJson: string;
    projectId: null | string;
    sourceId: string;
    sourceType: string;
    status: string;
    summary: string;
    title: string;
  }

  export interface KnowledgeSourceQueryInput {
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

  export interface MemoryToggleInput {
    isEnabled: boolean;
  }

  export interface Job {
    assistantCode: string;
    contextScopeJson: string;
    creationTime: string;
    description: string;
    id: string;
    isEnabled: boolean;
    jobCode: string;
    jobType: string;
    lastRunTime: null | string;
    lastRunStatus: string;
    lastRunSummary: string;
    modelId: null | string;
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
    modelId: null | string;
    name: string;
    outputTargetJson: string;
    promptTemplateCode: string;
    schedulerJobKey: string;
  }

  export interface JobQueryInput {
    includeDisabled?: boolean;
    jobType?: string;
    keyword?: string;
    maxResultCount?: number;
    schedulerJobKey?: string;
  }

  export interface JobRun {
    aiJobId: string;
    draftItemCount: number;
    elapsedMilliseconds: number;
    errorMessage: string;
    finishedTime: null | string;
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

  export interface JobRunQueryInput {
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
    job: Job;
    run: JobRun;
    summary: string;
  }

  export interface UsageRecord {
    auditSummary: string;
    completionTokens: number;
    conversationId: null | string;
    costAmount: number;
    creationTime: string;
    currency: string;
    elapsedMilliseconds: number;
    errorMessage: string;
    id: string;
    jobId: null | string;
    modelCode: string;
    modelId: null | string;
    modelName: string;
    projectId: null | string;
    promptInjectionRiskScore: number;
    promptInjectionSignalsJson: string;
    promptTemplateCode: string;
    promptTokens: number;
    providerId: null | string;
    providerName: string;
    requestPreview: string;
    responsePreview: string;
    scenario: string;
    source: string;
    status: string;
    toolCode: string;
    totalTokens: number;
    userId: null | string;
    userName: string;
  }

  export interface UsageQueryInput {
    conversationId?: string;
    createdFrom?: string;
    createdTo?: string;
    keyword?: string;
    maxResultCount?: number;
    modelId?: string;
    projectId?: string;
    providerId?: string;
    promptTemplateCode?: string;
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

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface WritebackRequest {
    actionCode: string;
    approvedTime: null | string;
    approverUserId: null | string;
    businessModule: string;
    conversationId: null | string;
    creationTime: string;
    diffPreviewJson: string;
    errorMessage: string;
    id: string;
    messageId: null | string;
    payloadJson: string;
    projectId: null | string;
    rejectReason: string;
    sourceType: string;
    status: string;
    summary: string;
    targetEntityId: string;
    targetEntityType: string;
  }

  export interface WritebackQueryInput {
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

  export interface WritebackCreateInput {
    actionCode: string;
    businessModule: string;
    conversationId: null | string;
    diffPreviewJson: string;
    messageId: null | string;
    payloadJson: string;
    projectId: null | string;
    sourceType: string;
    summary: string;
    targetEntityId: string;
    targetEntityType: string;
  }

  export interface WritebackDecisionInput {
    reason: string;
  }

  export interface WebSearchConfig {
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
    apiKey: string;
    allowDomains: string[];
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

  export interface WebSearchRecord {
    configId: null | string;
    conversationId: null | string;
    creationTime: string;
    elapsedMilliseconds: number;
    errorMessage: string;
    id: string;
    messageId: null | string;
    providerName: string;
    query: string;
    requestJson: string;
    responseSummaryJson: string;
    resultCount: number;
    status: string;
  }

  export interface WebSearchRecordQueryInput {
    configId?: string;
    conversationId?: string;
    keyword?: string;
    maxResultCount?: number;
    status?: string;
  }

  export interface WebSearchInput {
    configId: null | string;
    conversationId: null | string;
    maxResults: number;
    messageId: null | string;
    query: string;
  }

  export interface WebSearchResult {
    errorMessage: string;
    recordId: string;
    responseSummaryJson: string;
    resultCount: number;
    status: string;
  }
}

export interface AiApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const encodeId = (id: string) => encodeURIComponent(id);

export function createAiApi(client: AiApiClient) {
  const rootPath = '/app/ai';

  async function consumeConversationStream(
    conversationId: string,
    input: AiApi.ConversationSendInput,
    options: { onEvent: (event: AiApi.ConversationStreamEvent) => void; signal?: AbortSignal; timeoutMs?: number },
  ): Promise<AiApi.ConversationSendResult> {
    const response = await client.request<Response>(
      `${rootPath}/conversations/${encodeId(conversationId)}/messages/stream`,
      {
        body: input,
        headers: { Accept: 'application/x-ndjson' },
        method: 'POST',
        responseType: 'stream',
        signal: options.signal,
        timeoutMs: options.timeoutMs ?? 120_000,
      },
    );
    const reader = response.body?.getReader();
    if (!reader) throw new ApiRequestError('AI 对话没有返回可读取的数据流。');

    const decoder = new TextDecoder();
    let buffer = '';
    let result: AiApi.ConversationSendResult | undefined;

    function dispatch(line: string) {
      const normalized = line.trim();
      if (!normalized) return;
      let event: AiApi.ConversationStreamEvent;
      try {
        event = JSON.parse(normalized) as AiApi.ConversationStreamEvent;
      } catch (error) {
        throw new ApiRequestError('AI 对话返回了无法识别的流式事件。', { cause: error });
      }
      options.onEvent(event);
      if (event.event === 'completed' && event.result) result = event.result;
    }

    try {
      while (true) {
        const chunk = await reader.read();
        buffer += decoder.decode(chunk.value, { stream: !chunk.done });
        const lines = buffer.split('\n');
        buffer = lines.pop() ?? '';
        lines.forEach(dispatch);
        if (chunk.done) break;
      }
      dispatch(buffer);
    } catch (error) {
      if (error instanceof Error && error.name === 'AbortError') throw error;
      if (error instanceof Error && error.name === 'TimeoutError') {
        throw new ApiRequestError('AI 对话请求超时，请刷新会话确认状态后再重试。', { cause: error });
      }
      if (error instanceof ApiRequestError) throw error;
      throw new ApiRequestError('AI 对话连接中断，请刷新会话确认本轮是否已保存。', { cause: error });
    } finally {
      reader.releaseLock();
    }

    if (!result) throw new ApiRequestError('AI 流式连接已结束，未收到完成事件。');
    return result;
  }

  return {
    getProviders(signal?: AbortSignal) {
      return client.request<AiApi.Provider[]>(`${rootPath}/providers`, { signal });
    },
    createProvider(input: AiApi.ProviderSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Provider>(`${rootPath}/providers`, { body: input, method: 'POST', signal });
    },
    updateProvider(id: string, input: AiApi.ProviderSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Provider>(`${rootPath}/providers/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteProvider(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/providers/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    testProvider(id: string, signal?: AbortSignal) {
      return client.request<AiApi.ProviderTestResult>(`${rootPath}/providers/${encodeId(id)}/test`, {
        method: 'POST',
        signal,
      });
    },
    discoverProviderModels(id: string, signal?: AbortSignal) {
      return client.request<AiApi.ProviderModelDiscoveryResult>(
        `${rootPath}/providers/${encodeId(id)}/models/discover`,
        { method: 'POST', signal },
      );
    },
    syncProviderModels(id: string, signal?: AbortSignal) {
      return client.request<AiApi.ProviderModelDiscoveryResult>(`${rootPath}/providers/${encodeId(id)}/models/sync`, {
        method: 'POST',
        signal,
      });
    },
    getManageModels(providerId?: string, signal?: AbortSignal) {
      return client.request<AiApi.Model[]>(`${rootPath}/models/manage`, {
        query: { providerId },
        signal,
      });
    },
    createModel(input: AiApi.ModelSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Model>(`${rootPath}/models`, { body: input, method: 'POST', signal });
    },
    updateModel(id: string, input: AiApi.ModelSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Model>(`${rootPath}/models/${encodeId(id)}`, { body: input, method: 'PUT', signal });
    },
    deleteModel(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/models/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getModels(providerId?: string, signal?: AbortSignal) {
      return client.request<AiApi.Model[]>(`${rootPath}/models`, { query: { providerId }, signal });
    },
    getAssistants(includeDisabled = true, signal?: AbortSignal) {
      return client.request<AiApi.Assistant[]>(`${rootPath}/assistants`, { query: { includeDisabled }, signal });
    },
    createAssistant(input: AiApi.AssistantSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Assistant>(`${rootPath}/assistants`, { body: input, method: 'POST', signal });
    },
    updateAssistant(id: string, input: AiApi.AssistantSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Assistant>(`${rootPath}/assistants/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteAssistant(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/assistants/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getPromptTemplates(input: AiApi.PromptTemplateQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.PromptTemplate[]>(`${rootPath}/prompt-templates`, {
        query: {
          assistantCode: input.assistantCode,
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          scenario: input.scenario,
        },
        signal,
      });
    },
    getUsablePromptTemplates(signal?: AbortSignal) {
      return client.request<AiApi.PromptTemplateUseOption[]>(`${rootPath}/prompt-templates/available`, { signal });
    },
    createPromptTemplate(input: AiApi.PromptTemplateSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.PromptTemplate>(`${rootPath}/prompt-templates`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    updatePromptTemplate(id: string, input: AiApi.PromptTemplateSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.PromptTemplate>(`${rootPath}/prompt-templates/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deletePromptTemplate(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/prompt-templates/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    dryRunPromptTemplate(id: string, input: AiApi.PromptTemplateDryRunInput, signal?: AbortSignal) {
      return client.request<AiApi.PromptTemplateDryRunResult>(`${rootPath}/prompt-templates/${encodeId(id)}/dry-run`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    createConversationFromTemplate(
      id: string,
      input: AiApi.PromptTemplateConversationCreateInput,
      signal?: AbortSignal,
    ) {
      return client.request<AiApi.ConversationDetail>(`${rootPath}/prompt-templates/${encodeId(id)}/conversations`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getSkills(input: AiApi.AgentDesignQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Skill[]>(`${rootPath}/skills`, {
        query: {
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          roleType: input.roleType,
        },
        signal,
      });
    },
    getUsableSkills(signal?: AbortSignal) {
      return client.request<AiApi.SkillUseOption[]>(`${rootPath}/skills/available`, { signal });
    },
    createSkill(input: AiApi.SkillSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Skill>(`${rootPath}/skills`, { body: input, method: 'POST', signal });
    },
    updateSkill(id: string, input: AiApi.SkillSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Skill>(`${rootPath}/skills/${encodeId(id)}`, { body: input, method: 'PUT', signal });
    },
    deleteSkill(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/skills/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getPersonas(input: AiApi.AgentDesignQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Persona[]>(`${rootPath}/personas`, {
        query: {
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          roleType: input.roleType,
        },
        signal,
      });
    },
    createPersona(input: AiApi.PersonaSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Persona>(`${rootPath}/personas`, { body: input, method: 'POST', signal });
    },
    updatePersona(id: string, input: AiApi.PersonaSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Persona>(`${rootPath}/personas/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deletePersona(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/personas/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getCommands(input: AiApi.AgentDesignQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Command[]>(`${rootPath}/commands`, {
        query: {
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          roleType: input.roleType,
        },
        signal,
      });
    },
    createCommand(input: AiApi.CommandSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Command>(`${rootPath}/commands`, { body: input, method: 'POST', signal });
    },
    updateCommand(id: string, input: AiApi.CommandSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Command>(`${rootPath}/commands/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteCommand(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/commands/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getAgents(input: AiApi.AgentDesignQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Agent[]>(`${rootPath}/agents`, {
        query: {
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          roleType: input.roleType,
        },
        signal,
      });
    },
    createAgent(input: AiApi.AgentSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Agent>(`${rootPath}/agents`, { body: input, method: 'POST', signal });
    },
    updateAgent(id: string, input: AiApi.AgentSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Agent>(`${rootPath}/agents/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteAgent(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/agents/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    buildAgentRunPlan(id: string, input: AiApi.AgentRunPlanInput, signal?: AbortSignal) {
      return client.request<AiApi.AgentRunPlan>(`${rootPath}/agents/${encodeId(id)}/run-plan`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getArtifacts(input: AiApi.ArtifactQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Artifact[]>(`${rootPath}/artifacts`, {
        query: {
          category: input.category,
          includeDisabled: input.includeDisabled,
          keyword: input.keyword,
          rendererType: input.rendererType,
        },
        signal,
      });
    },
    createArtifact(input: AiApi.ArtifactSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Artifact>(`${rootPath}/artifacts`, { body: input, method: 'POST', signal });
    },
    updateArtifact(id: string, input: AiApi.ArtifactSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Artifact>(`${rootPath}/artifacts/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteArtifact(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/artifacts/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getTools(signal?: AbortSignal) {
      return client.request<AiApi.ToolDefinition[]>(`${rootPath}/tools`, { signal });
    },
    getToolInvocations(input: AiApi.ToolInvocationQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.ToolInvocation[]>(`${rootPath}/tool-invocations`, {
        query: {
          conversationId: input.conversationId,
          maxResultCount: input.maxResultCount,
          messageId: input.messageId,
          status: input.status,
          toolCode: input.toolCode,
        },
        signal,
      });
    },
    getConversations(input: AiApi.ConversationQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Conversation[]>(`${rootPath}/conversations`, {
        query: { keyword: input.keyword, status: input.status },
        signal,
      });
    },
    getConversation(id: string, signal?: AbortSignal) {
      return client.request<AiApi.ConversationDetail>(`${rootPath}/conversations/${encodeId(id)}`, { signal });
    },
    createConversation(input: AiApi.ConversationCreateInput, signal?: AbortSignal) {
      return client.request<AiApi.ConversationDetail>(`${rootPath}/conversations`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    updateConversation(id: string, input: AiApi.ConversationUpdateInput, signal?: AbortSignal) {
      return client.request<AiApi.Conversation>(`${rootPath}/conversations/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    sendMessage(id: string, input: AiApi.ConversationSendInput, signal?: AbortSignal) {
      return client.request<AiApi.ConversationSendResult>(`${rootPath}/conversations/${encodeId(id)}/messages`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    streamMessage: consumeConversationStream,
    confirmPlan(id: string, input: AiApi.PlanConfirmInput, signal?: AbortSignal) {
      return client.request<AiApi.Message>(`${rootPath}/conversations/${encodeId(id)}/plan/confirm`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    togglePlanStep(id: string, input: AiApi.PlanStepToggleInput, signal?: AbortSignal) {
      return client.request<AiApi.Message>(`${rootPath}/conversations/${encodeId(id)}/plan/steps/toggle`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    createPlanDraft(id: string, input: AiApi.PlanDraftInput, signal?: AbortSignal) {
      return client.request<AiApi.ConversationDetail>(`${rootPath}/conversations/${encodeId(id)}/plan/draft`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    archiveConversation(id: string, signal?: AbortSignal) {
      return client.request<AiApi.Conversation>(`${rootPath}/conversations/${encodeId(id)}/archive`, {
        method: 'POST',
        signal,
      });
    },
    restoreConversation(id: string, signal?: AbortSignal) {
      return client.request<AiApi.Conversation>(`${rootPath}/conversations/${encodeId(id)}/restore`, {
        method: 'POST',
        signal,
      });
    },
    exportConversation(id: string, signal?: AbortSignal) {
      return client.request<AiApi.ConversationExport>(`${rootPath}/conversations/${encodeId(id)}/export`, { signal });
    },
    deleteConversation(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/conversations/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getMcpServers(signal?: AbortSignal) {
      return client.request<AiApi.McpServer[]>(`${rootPath}/mcp/servers`, { signal });
    },
    createMcpServer(input: AiApi.McpServerSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.McpServer>(`${rootPath}/mcp/servers`, { body: input, method: 'POST', signal });
    },
    updateMcpServer(id: string, input: AiApi.McpServerSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.McpServer>(`${rootPath}/mcp/servers/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteMcpServer(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/mcp/servers/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    testMcpServer(id: string, signal?: AbortSignal) {
      return client.request<AiApi.McpConnectionTestResult>(`${rootPath}/mcp/servers/${encodeId(id)}/test`, {
        method: 'POST',
        signal,
      });
    },
    discoverMcpTools(id: string, signal?: AbortSignal) {
      return client.request<AiApi.McpDiscoveryResult>(`${rootPath}/mcp/servers/${encodeId(id)}/discover-tools`, {
        method: 'POST',
        signal,
      });
    },
    getMcpTools(serverId?: string, signal?: AbortSignal) {
      return client.request<AiApi.McpTool[]>(`${rootPath}/mcp/tools`, { query: { serverId }, signal });
    },
    updateMcpToolPolicy(id: string, input: AiApi.McpToolPolicyInput, signal?: AbortSignal) {
      return client.request<AiApi.McpTool>(`${rootPath}/mcp/tools/${encodeId(id)}/policy`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    invokeMcpTool(id: string, input: AiApi.McpInvokeInput, signal?: AbortSignal) {
      return client.request<AiApi.McpInvokeResult>(`${rootPath}/mcp/tools/${encodeId(id)}/invoke`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    getMcpInvocations(input: AiApi.McpInvocationQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.McpInvocation[]>(`${rootPath}/mcp/invocations`, {
        query: {
          maxResultCount: input.maxResultCount,
          serverId: input.serverId,
          status: input.status,
          toolId: input.toolId,
          toolName: input.toolName,
        },
        signal,
      });
    },
    getMemories(input: AiApi.MemoryQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Memory[]>(`${rootPath}/memories`, {
        query: {
          contextId: input.contextId,
          contextType: input.contextType,
          includeDisabled: input.includeDisabled,
          knowledgeSourceId: input.knowledgeSourceId,
          keyword: input.keyword,
          maxResultCount: input.maxResultCount,
          memoryType: input.memoryType,
          ownerUserId: input.ownerUserId,
        },
        signal,
      });
    },
    createMemory(input: AiApi.MemorySaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Memory>(`${rootPath}/memories`, { body: input, method: 'POST', signal });
    },
    updateMemory(id: string, input: AiApi.MemorySaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Memory>(`${rootPath}/memories/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    toggleMemory(id: string, input: AiApi.MemoryToggleInput, signal?: AbortSignal) {
      return client.request<AiApi.Memory>(`${rootPath}/memories/${encodeId(id)}/toggle`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    deleteMemory(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/memories/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getKnowledgeSources(input: AiApi.KnowledgeSourceQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.KnowledgeSource[]>(`${rootPath}/knowledge-sources`, {
        query: {
          contextId: input.contextId,
          contextType: input.contextType,
          keyword: input.keyword,
          maxResultCount: input.maxResultCount,
          projectId: input.projectId,
          sourceType: input.sourceType,
        },
        signal,
      });
    },
    importProjectDocuments(projectId: string, signal?: AbortSignal) {
      return client.request<AiApi.MemoryImportResult>(
        `${rootPath}/knowledge-sources/project/${encodeId(projectId)}/documents/import`,
        { method: 'POST', signal },
      );
    },
    importProjectMeetings(projectId: string, signal?: AbortSignal) {
      return client.request<AiApi.MemoryImportResult>(
        `${rootPath}/knowledge-sources/project/${encodeId(projectId)}/meetings/import`,
        { method: 'POST', signal },
      );
    },
    importProjectRisks(projectId: string, signal?: AbortSignal) {
      return client.request<AiApi.MemoryImportResult>(
        `${rootPath}/knowledge-sources/project/${encodeId(projectId)}/risks/import`,
        { method: 'POST', signal },
      );
    },
    getJobs(input: AiApi.JobQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.Job[]>(`${rootPath}/jobs`, { query: { ...input }, signal });
    },
    createJob(input: AiApi.JobSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Job>(`${rootPath}/jobs`, { body: input, method: 'POST', signal });
    },
    updateJob(id: string, input: AiApi.JobSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.Job>(`${rootPath}/jobs/${encodeId(id)}`, { body: input, method: 'PUT', signal });
    },
    toggleJob(id: string, input: { isEnabled: boolean }, signal?: AbortSignal) {
      return client.request<AiApi.Job>(`${rootPath}/jobs/${encodeId(id)}/toggle`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    deleteJob(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/jobs/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    runJob(id: string, signal?: AbortSignal) {
      return client.request<AiApi.JobRunResult>(`${rootPath}/jobs/${encodeId(id)}/run`, {
        method: 'POST',
        signal,
        timeoutMs: 120_000,
      });
    },
    getJobRuns(input: AiApi.JobRunQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.JobRun[]>(`${rootPath}/job-runs`, { query: { ...input }, signal });
    },
    getUsageDashboard(input: AiApi.UsageQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.UsageDashboard>(`${rootPath}/usage/dashboard`, { query: { ...input }, signal });
    },
    getUsageRecords(input: AiApi.UsageQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.UsageRecord[]>(`${rootPath}/usage/records`, { query: { ...input }, signal });
    },
    getWritebacks(input: AiApi.WritebackQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.PagedResult<AiApi.WritebackRequest>>(`${rootPath}/writebacks`, {
        query: { ...input },
        signal,
      });
    },
    createWriteback(input: AiApi.WritebackCreateInput, signal?: AbortSignal) {
      return client.request<AiApi.WritebackRequest>(`${rootPath}/writebacks`, { body: input, method: 'POST', signal });
    },
    approveWriteback(id: string, signal?: AbortSignal) {
      return client.request<AiApi.WritebackRequest>(`${rootPath}/writebacks/${encodeId(id)}/approve`, {
        method: 'POST',
        signal,
      });
    },
    rejectWriteback(id: string, input: AiApi.WritebackDecisionInput, signal?: AbortSignal) {
      return client.request<AiApi.WritebackRequest>(`${rootPath}/writebacks/${encodeId(id)}/reject`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    cancelWriteback(id: string, signal?: AbortSignal) {
      return client.request<AiApi.WritebackRequest>(`${rootPath}/writebacks/${encodeId(id)}/cancel`, {
        method: 'POST',
        signal,
      });
    },
    getWebSearchConfigs(includeDisabled = true, signal?: AbortSignal) {
      return client.request<AiApi.WebSearchConfig[]>(`${rootPath}/web-search/configs`, {
        query: { includeDisabled },
        signal,
      });
    },
    createWebSearchConfig(input: AiApi.WebSearchConfigSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.WebSearchConfig>(`${rootPath}/web-search/configs`, {
        body: input,
        method: 'POST',
        signal,
      });
    },
    updateWebSearchConfig(id: string, input: AiApi.WebSearchConfigSaveInput, signal?: AbortSignal) {
      return client.request<AiApi.WebSearchConfig>(`${rootPath}/web-search/configs/${encodeId(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
    deleteWebSearchConfig(id: string, signal?: AbortSignal) {
      return client.request<boolean>(`${rootPath}/web-search/configs/${encodeId(id)}`, { method: 'DELETE', signal });
    },
    getWebSearchRecords(input: AiApi.WebSearchRecordQueryInput = {}, signal?: AbortSignal) {
      return client.request<AiApi.WebSearchRecord[]>(`${rootPath}/web-search/records`, {
        query: {
          configId: input.configId,
          conversationId: input.conversationId,
          keyword: input.keyword,
          maxResultCount: input.maxResultCount,
          status: input.status,
        },
        signal,
      });
    },
    runWebSearch(input: AiApi.WebSearchInput, signal?: AbortSignal) {
      return client.request<AiApi.WebSearchResult>(`${rootPath}/web-search/search`, {
        body: input,
        method: 'POST',
        signal,
        timeoutMs: 120_000,
      });
    },
  };
}
