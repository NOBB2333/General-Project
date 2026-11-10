import { apiClient } from '@/api/request';

export namespace BusinessApi {
  export interface ProjectListInput {
    keyword?: string;
  }

  export interface ProjectSummary {
    businessCloseStatus: string;
    contractAmount: null | number;
    costAmount: null | number;
    costPerformanceIndex: null | number;
    customerName: string;
    grossProfitAmount: null | number;
    grossProfitRate: null | number;
    id: string;
    invoicedAmount: null | number;
    managerName: string;
    name: string;
    procurementAmount: null | number;
    projectCode: string;
    receivedAmount: null | number;
    schedulePerformanceIndex: null | number;
    status: string;
    yearEndReceivableForecast: null | number;
  }

  export interface FocusItem {
    actionTab: string;
    description: string;
    projectId: string;
    projectName: string;
    type: string;
  }

  export interface WarningItem {
    actionTab: string;
    description: string;
    id: string;
    lastUpdatedTime: string;
    level: string;
    ownerName: string;
    ownerUserId: null | string;
    projectCode: string;
    projectId: string;
    projectName: string;
    status: string;
    type: string;
  }

  export interface Overview {
    canViewProfitAnalysis: boolean;
    canViewSensitiveAmounts: boolean;
    contractAmount: null | number;
    costAmount: null | number;
    focusProjects: FocusItem[];
    grossProfitAmount: null | number;
    grossProfitRate: null | number;
    lowProfitProjectCount: number;
    procurementAmount: null | number;
    projects: ProjectSummary[];
    receivableRiskProjectCount: number;
    receivedAmount: null | number;
    tailClosingProjectCount: number;
    warningItems: WarningItem[];
    yearEndReceivableForecast: null | number;
  }

  export interface BudgetExecutionItem {
    adjustedAmount: null | number;
    budgetCode: string;
    category: string;
    executedAmount: null | number;
    id: string;
    projectId: string;
    projectName: string;
    sortOrder: number;
    varianceAmount: null | number;
  }

  export interface AcceptanceItem {
    acceptanceCode: string;
    acceptanceDate: string;
    acceptanceResult: string;
    id: string;
    linkedContractCode: string;
    milestoneId: null | string;
    projectId: string;
    projectName: string;
    qualityInspectionCode: string;
    status: string;
    title: string;
  }

  export interface BusinessChainItem {
    chainCode: string;
    id: string;
    linkedContractCode: string;
    ownerName: string;
    ownerUserId: string;
    sourceChangeCode: string;
    stage: string;
    status: string;
    summary: string;
    title: string;
    type: string;
  }

  export interface ContractItem {
    amount: null | number;
    contractCode: string;
    counterpartyName: string;
    id: string;
    isRevenueContract: boolean;
    legalEntityId: null | string;
    legalEntityNameSnapshot: string;
    legalEntityTaxIdSnapshot: string;
    parentContractCode: string;
    projectId: string;
    projectName: string;
    signDate: string;
    sourceChangeCode: string;
    status: string;
    title: string;
    type: string;
  }

  export interface ProcurementItem {
    amount: null | number;
    id: string;
    linkedContractCode: string;
    procurementCode: string;
    projectId: string;
    projectName: string;
    signDate: string;
    sourceChangeCode: string;
    stage: string;
    status: string;
    supplierName: string;
    title: string;
  }

  export interface ReceivableItem {
    id: string;
    invoiceCode: string;
    linkedContractCode: string;
    plannedDate: string;
    projectId: string;
    projectName: string;
    receivedAmount: null | number;
    receivableCode: string;
    status: string;
    title: string;
    totalAmount: null | number;
  }

  export interface InvoiceItem {
    amount: null | number;
    id: string;
    invoiceCode: string;
    invoiceDate: string;
    legalEntityId: null | string;
    legalEntityNameSnapshot: string;
    legalEntityTaxIdSnapshot: string;
    linkedContractCode: string;
    projectId: string;
    projectName: string;
    receivableCode: string;
    status: string;
    taxAmount: null | number;
    title: string;
  }

  export interface ForecastHistoryItem {
    changeType: string;
    changedAt: string;
    changedBy: string;
    id: string;
    metric: string;
    newValue: string;
    oldValue: string;
    reason: string;
    relatedCode: string;
  }

  export interface ProjectDetail {
    acceptances: AcceptanceItem[];
    budgetExecutions: BudgetExecutionItem[];
    businessChains: BusinessChainItem[];
    canViewProfitAnalysis: boolean;
    canViewSensitiveAmounts: boolean;
    contracts: ContractItem[];
    forecastHistories: ForecastHistoryItem[];
    invoices: InvoiceItem[];
    procurements: ProcurementItem[];
    receivables: ReceivableItem[];
    summary: ProjectSummary;
  }

  export interface ReportQuery {
    businessCloseStatus?: string;
    contractType?: string;
    counterpartyName?: string;
    keyword?: string;
    plannedDateFrom?: string;
    plannedDateTo?: string;
    projectCode?: string;
    receivableStatus?: string;
    signDateFrom?: string;
    signDateTo?: string;
    supplierName?: string;
    warningLevel?: string;
  }

  export type ReportLedgerKind =
    'acceptance' | 'budget' | 'contract' | 'invoice' | 'procurement' | 'profit' | 'project' | 'receivable' | 'warning';

  export interface Report {
    acceptanceItems: AcceptanceItem[];
    budgetItems: BudgetExecutionItem[];
    contractItems: ContractItem[];
    invoiceItems: InvoiceItem[];
    profitItems: ProjectSummary[];
    procurementItems: ProcurementItem[];
    projectItems: ProjectSummary[];
    receivableItems: ReceivableItem[];
    warningItems: WarningItem[];
  }

  export interface BudgetExecutionSaveInput {
    adjustedAmount: number;
    budgetCode: string;
    category: string;
    executedAmount: number;
    sortOrder: number;
    varianceAmount: null | number;
  }

  export interface AcceptanceSaveInput {
    acceptanceCode: string;
    acceptanceDate: null | string;
    acceptanceResult: string;
    linkedContractCode: string;
    milestoneId: null | string;
    qualityInspectionCode: string;
    status: string;
    title: string;
  }

  export interface BusinessChainSaveInput {
    chainCode: string;
    linkedContractCode: string;
    ownerUserId: string;
    sourceChangeCode: string;
    stage: string;
    status: string;
    summary: string;
    title: string;
    type: string;
  }

  export interface ContractSaveInput {
    amount: number;
    clearLegalEntity: boolean;
    contractCode: string;
    counterpartyName: string;
    isRevenueContract: boolean;
    legalEntityId: null | string;
    parentContractCode: string;
    signDate: null | string;
    sourceChangeCode: string;
    status: string;
    title: string;
    type: string;
  }

  export interface ProcurementSaveInput {
    amount: number;
    linkedContractCode: string;
    procurementCode: string;
    signDate: null | string;
    sourceChangeCode: string;
    stage: string;
    status: string;
    supplierName: string;
    title: string;
  }

  export interface InvoiceSaveInput {
    amount: number;
    clearLegalEntity: boolean;
    invoiceCode: string;
    invoiceDate: null | string;
    legalEntityId: null | string;
    linkedContractCode: string;
    receivableCode: string;
    status: string;
    taxAmount: number;
    title: string;
  }

  export interface ReceivableSaveInput {
    invoiceCode: string;
    linkedContractCode: string;
    plannedDate: null | string;
    receivableCode: string;
    receivedAmount: number;
    status: string;
    title: string;
    totalAmount: number;
  }
}

export function getBusinessOverviewApi(signal?: AbortSignal): Promise<BusinessApi.Overview> {
  return apiClient.request('/app/business/overview', { signal });
}

export function getBusinessProjectsApi(
  input: BusinessApi.ProjectListInput = {},
  signal?: AbortSignal,
): Promise<BusinessApi.ProjectSummary[]> {
  return apiClient.request('/app/business/projects', { query: { ...input }, signal });
}

export function getBusinessProjectDetailApi(
  projectId: string,
  signal?: AbortSignal,
): Promise<BusinessApi.ProjectDetail> {
  return apiClient.request(`/app/business/projects/${encodeURIComponent(projectId)}`, { signal });
}

export function getBusinessReportApi(
  input: BusinessApi.ReportQuery = {},
  signal?: AbortSignal,
): Promise<BusinessApi.Report> {
  return apiClient.request('/app/business/reports', { query: { ...input }, signal });
}

export function exportBusinessReportApi(
  input: BusinessApi.ReportQuery & { ledgerKind: BusinessApi.ReportLedgerKind },
  signal?: AbortSignal,
): Promise<Blob> {
  return apiClient.request('/app/business/reports/export', {
    query: { ...input },
    responseType: 'blob',
    signal,
  });
}

type BusinessMutationCollection =
  'acceptances' | 'budgets' | 'chains' | 'contracts' | 'invoices' | 'procurements' | 'receivables';

function createBusinessRecordApi<TInput>(
  projectId: string,
  collection: BusinessMutationCollection,
  input: TInput,
): Promise<string> {
  return apiClient.request(`/app/business/projects/${encodeURIComponent(projectId)}/${collection}`, {
    method: 'POST',
    body: input,
  });
}

function updateBusinessRecordApi<TInput>(
  projectId: string,
  collection: BusinessMutationCollection,
  recordId: string,
  input: TInput,
): Promise<boolean> {
  return apiClient.request(
    `/app/business/projects/${encodeURIComponent(projectId)}/${collection}/${encodeURIComponent(recordId)}`,
    { method: 'PUT', body: input },
  );
}

function deleteBusinessRecordApi(
  projectId: string,
  collection: BusinessMutationCollection,
  recordId: string,
): Promise<boolean> {
  return apiClient.request(
    `/app/business/projects/${encodeURIComponent(projectId)}/${collection}/${encodeURIComponent(recordId)}`,
    { method: 'DELETE' },
  );
}

export function createBusinessBudgetExecutionApi(
  projectId: string,
  input: BusinessApi.BudgetExecutionSaveInput,
): Promise<string> {
  return createBusinessRecordApi(projectId, 'budgets', input);
}

export function updateBusinessBudgetExecutionApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.BudgetExecutionSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'budgets', recordId, input);
}

export function deleteBusinessBudgetExecutionApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'budgets', recordId);
}

export function createBusinessAcceptanceApi(
  projectId: string,
  input: BusinessApi.AcceptanceSaveInput,
): Promise<string> {
  return createBusinessRecordApi(projectId, 'acceptances', input);
}

export function updateBusinessAcceptanceApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.AcceptanceSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'acceptances', recordId, input);
}

export function deleteBusinessAcceptanceApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'acceptances', recordId);
}

export function createBusinessChainApi(projectId: string, input: BusinessApi.BusinessChainSaveInput): Promise<string> {
  return createBusinessRecordApi(projectId, 'chains', input);
}

export function updateBusinessChainApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.BusinessChainSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'chains', recordId, input);
}

export function deleteBusinessChainApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'chains', recordId);
}

export function createBusinessContractApi(projectId: string, input: BusinessApi.ContractSaveInput): Promise<string> {
  return createBusinessRecordApi(projectId, 'contracts', input);
}

export function updateBusinessContractApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.ContractSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'contracts', recordId, input);
}

export function deleteBusinessContractApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'contracts', recordId);
}

export function createBusinessProcurementApi(
  projectId: string,
  input: BusinessApi.ProcurementSaveInput,
): Promise<string> {
  return createBusinessRecordApi(projectId, 'procurements', input);
}

export function updateBusinessProcurementApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.ProcurementSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'procurements', recordId, input);
}

export function deleteBusinessProcurementApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'procurements', recordId);
}

export function createBusinessInvoiceApi(projectId: string, input: BusinessApi.InvoiceSaveInput): Promise<string> {
  return createBusinessRecordApi(projectId, 'invoices', input);
}

export function updateBusinessInvoiceApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.InvoiceSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'invoices', recordId, input);
}

export function deleteBusinessInvoiceApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'invoices', recordId);
}

export function createBusinessReceivableApi(
  projectId: string,
  input: BusinessApi.ReceivableSaveInput,
): Promise<string> {
  return createBusinessRecordApi(projectId, 'receivables', input);
}

export function updateBusinessReceivableApi(
  projectId: string,
  recordId: string,
  input: BusinessApi.ReceivableSaveInput,
): Promise<boolean> {
  return updateBusinessRecordApi(projectId, 'receivables', recordId, input);
}

export function deleteBusinessReceivableApi(projectId: string, recordId: string): Promise<boolean> {
  return deleteBusinessRecordApi(projectId, 'receivables', recordId);
}
