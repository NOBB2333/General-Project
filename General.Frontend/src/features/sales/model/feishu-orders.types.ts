export interface FeishuConversation {
  id: string;
  name: string;
  preview: string;
  time: string;
  unread: number;
  pinned: boolean;
  tone: 'bot' | 'coral' | 'green' | 'purple' | 'orange';
  status?: string;
}

export type FeishuMessageKind = 'text' | 'attachment' | 'thinking' | 'preview' | 'result' | 'plan';
export type FeishuFlowKind = 'order' | 'payment' | 'shipment';

export interface AiPlanStepDisplay {
  expectedOutput: string;
  id: string;
  isDone: boolean;
  title: string;
}

export interface AiRunStepDisplay {
  code: string;
  status: string;
  summary: string;
  title: string;
}

export interface AiToolInvocationDisplay {
  elapsedMilliseconds: number;
  errorMessage: string;
  invocationId: string;
  status: string;
  toolCode: string;
}

export interface FeishuMessage {
  id: string;
  sender: 'bot' | 'user';
  kind: FeishuMessageKind;
  time: string;
  text?: string;
  fileName?: string;
  fileMeta?: string;
  flow?: FeishuFlowKind;
  title?: string;
  source?: string;
  fields?: readonly { label: string; value: string }[];
  total?: string;
  primaryLabel?: string;
  decided?: boolean;
  docNo?: string;
  summary?: string;
  badges?: readonly string[];
  aiPlan?: { confirmed: boolean; goal: string; steps: readonly AiPlanStepDisplay[] };
  aiRunSteps?: readonly AiRunStepDisplay[];
  aiToolInvocations?: readonly AiToolInvocationDisplay[];
  citations?: readonly string[];
  errorMessage?: string;
  modelName?: string;
  retryText?: string;
  statusLabel?: string;
  tokenSummary?: string;
  transient?: boolean;
  actionBusy?: boolean;
}

export interface FeishuTraceDocument {
  id: string;
  kind: FeishuFlowKind;
  docNo: string;
  title: string;
  summary: string;
  badges: readonly string[];
}
