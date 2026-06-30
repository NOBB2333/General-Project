import { requestClient } from '#/api/request';

export namespace BusinessApi {
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

  export interface ReportExportQuery extends ReportQuery {
    ledgerKind: string;
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
    ownerUserId?: null | string;
    projectCode: string;
    projectId: string;
    projectName: string;
    status: string;
    type: string;
  }

  export interface ProjectSummary {
    businessCloseStatus: string;
    contractAmount?: null | number;
    costPerformanceIndex?: null | number;
    costAmount?: null | number;
    customerName: string;
    grossProfitAmount?: null | number;
    grossProfitRate?: null | number;
    id: string;
    invoicedAmount?: null | number;
    managerName: string;
    name: string;
    procurementAmount?: null | number;
    projectCode: string;
    receivedAmount?: null | number;
    schedulePerformanceIndex?: null | number;
    status: string;
    yearEndReceivableForecast?: null | number;
  }

  export interface Overview {
    canViewProfitAnalysis: boolean;
    canViewSensitiveAmounts: boolean;
    contractAmount?: null | number;
    costAmount?: null | number;
    focusProjects: FocusItem[];
    grossProfitAmount?: null | number;
    grossProfitRate?: null | number;
    lowProfitProjectCount: number;
    procurementAmount?: null | number;
    projects: ProjectSummary[];
    receivableRiskProjectCount: number;
    receivedAmount?: null | number;
    tailClosingProjectCount: number;
    warningItems: WarningItem[];
    yearEndReceivableForecast?: null | number;
  }

  export interface BudgetExecutionItem {
    adjustedAmount?: null | number;
    budgetCode: string;
    category: string;
    executedAmount?: null | number;
    id: string;
    projectId: string;
    projectName: string;
    sortOrder: number;
    varianceAmount?: null | number;
  }

  export interface BudgetExecutionSaveInput {
    adjustedAmount: number;
    budgetCode: string;
    category: string;
    executedAmount: number;
    sortOrder: number;
    varianceAmount?: number;
  }

  export interface AcceptanceItem {
    acceptanceCode: string;
    acceptanceDate: string;
    acceptanceResult: string;
    id: string;
    linkedContractCode: string;
    milestoneId?: null | string;
    projectId: string;
    projectName: string;
    qualityInspectionCode: string;
    status: string;
    title: string;
  }

  export interface AcceptanceSaveInput {
    acceptanceCode: string;
    acceptanceDate?: string;
    acceptanceResult: string;
    linkedContractCode: string;
    milestoneId?: string;
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

  export interface ContractItem {
    amount?: null | number;
    contractCode: string;
    counterpartyName: string;
    id: string;
    isRevenueContract: boolean;
    parentContractCode: string;
    projectId: string;
    projectName: string;
    signDate: string;
    sourceChangeCode: string;
    status: string;
    title: string;
    type: string;
  }

  export interface ContractSaveInput {
    amount: number;
    contractCode: string;
    counterpartyName: string;
    isRevenueContract: boolean;
    parentContractCode: string;
    signDate?: string;
    sourceChangeCode: string;
    status: string;
    title: string;
    type: string;
  }

  export interface ProcurementItem {
    amount?: null | number;
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

  export interface ProcurementSaveInput {
    amount: number;
    linkedContractCode: string;
    procurementCode: string;
    signDate?: string;
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
    receivableCode: string;
    receivedAmount?: null | number;
    status: string;
    title: string;
    totalAmount?: null | number;
  }

  export interface ReceivableSaveInput {
    invoiceCode: string;
    linkedContractCode: string;
    plannedDate?: string;
    receivableCode: string;
    receivedAmount: number;
    status: string;
    title: string;
    totalAmount: number;
  }

  export interface InvoiceItem {
    amount?: null | number;
    id: string;
    invoiceCode: string;
    invoiceDate: string;
    linkedContractCode: string;
    projectId: string;
    projectName: string;
    receivableCode: string;
    status: string;
    taxAmount?: null | number;
    title: string;
  }

  export interface InvoiceSaveInput {
    amount: number;
    invoiceCode: string;
    invoiceDate?: string;
    linkedContractCode: string;
    receivableCode: string;
    status: string;
    taxAmount: number;
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
}

export async function getBusinessOverviewApi() {
  return requestClient.get<BusinessApi.Overview>('/app/business/overview');
}

export async function getBusinessProjectListApi(keyword?: string) {
  return requestClient.get<BusinessApi.ProjectSummary[]>(
    '/app/business/projects',
    {
      params: { keyword },
    },
  );
}

export async function getBusinessProjectDetailApi(projectId: string) {
  return requestClient.get<BusinessApi.ProjectDetail>(
    `/app/business/projects/${projectId}`,
  );
}

export async function getBusinessReportApi(params?: BusinessApi.ReportQuery) {
  return requestClient.get<BusinessApi.Report>('/app/business/reports', {
    params,
  });
}

export async function exportBusinessReportApi(
  params: BusinessApi.ReportExportQuery,
) {
  return requestClient.get<Blob>('/app/business/reports/export', {
    params,
    responseReturn: 'body',
    responseType: 'blob',
  });
}

export async function createBusinessBudgetApi(
  projectId: string,
  data: BusinessApi.BudgetExecutionSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/budgets`,
    data,
  );
}

export async function updateBusinessBudgetApi(
  projectId: string,
  budgetExecutionId: string,
  data: BusinessApi.BudgetExecutionSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/budgets/${budgetExecutionId}`,
    data,
  );
}

export async function deleteBusinessBudgetApi(
  projectId: string,
  budgetExecutionId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/budgets/${budgetExecutionId}`,
  );
}

export async function createBusinessAcceptanceApi(
  projectId: string,
  data: BusinessApi.AcceptanceSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/acceptances`,
    data,
  );
}

export async function updateBusinessAcceptanceApi(
  projectId: string,
  acceptanceId: string,
  data: BusinessApi.AcceptanceSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/acceptances/${acceptanceId}`,
    data,
  );
}

export async function deleteBusinessAcceptanceApi(
  projectId: string,
  acceptanceId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/acceptances/${acceptanceId}`,
  );
}

export async function createBusinessChainApi(
  projectId: string,
  data: BusinessApi.BusinessChainSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/chains`,
    data,
  );
}

export async function updateBusinessChainApi(
  projectId: string,
  chainId: string,
  data: BusinessApi.BusinessChainSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/chains/${chainId}`,
    data,
  );
}

export async function deleteBusinessChainApi(
  projectId: string,
  chainId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/chains/${chainId}`,
  );
}

export async function createBusinessContractApi(
  projectId: string,
  data: BusinessApi.ContractSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/contracts`,
    data,
  );
}

export async function updateBusinessContractApi(
  projectId: string,
  contractId: string,
  data: BusinessApi.ContractSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/contracts/${contractId}`,
    data,
  );
}

export async function deleteBusinessContractApi(
  projectId: string,
  contractId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/contracts/${contractId}`,
  );
}

export async function createBusinessProcurementApi(
  projectId: string,
  data: BusinessApi.ProcurementSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/procurements`,
    data,
  );
}

export async function updateBusinessProcurementApi(
  projectId: string,
  procurementId: string,
  data: BusinessApi.ProcurementSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/procurements/${procurementId}`,
    data,
  );
}

export async function deleteBusinessProcurementApi(
  projectId: string,
  procurementId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/procurements/${procurementId}`,
  );
}

export async function createBusinessInvoiceApi(
  projectId: string,
  data: BusinessApi.InvoiceSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/invoices`,
    data,
  );
}

export async function updateBusinessInvoiceApi(
  projectId: string,
  invoiceId: string,
  data: BusinessApi.InvoiceSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/invoices/${invoiceId}`,
    data,
  );
}

export async function deleteBusinessInvoiceApi(
  projectId: string,
  invoiceId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/invoices/${invoiceId}`,
  );
}

export async function createBusinessReceivableApi(
  projectId: string,
  data: BusinessApi.ReceivableSaveInput,
) {
  return requestClient.post<string>(
    `/app/business/projects/${projectId}/receivables`,
    data,
  );
}

export async function updateBusinessReceivableApi(
  projectId: string,
  receivableId: string,
  data: BusinessApi.ReceivableSaveInput,
) {
  return requestClient.put<boolean>(
    `/app/business/projects/${projectId}/receivables/${receivableId}`,
    data,
  );
}

export async function deleteBusinessReceivableApi(
  projectId: string,
  receivableId: string,
) {
  return requestClient.delete<boolean>(
    `/app/business/projects/${projectId}/receivables/${receivableId}`,
  );
}
