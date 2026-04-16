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
    varianceAmount?: null | number;
  }

  export interface BusinessChainItem {
    chainCode: string;
    id: string;
    linkedContractCode: string;
    ownerName: string;
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
    budgetExecutions: BudgetExecutionItem[];
    businessChains: BusinessChainItem[];
    canViewProfitAnalysis: boolean;
    canViewSensitiveAmounts: boolean;
    contracts: ContractItem[];
    forecastHistories: ForecastHistoryItem[];
    procurements: ProcurementItem[];
    receivables: ReceivableItem[];
    summary: ProjectSummary;
  }

  export interface Report {
    contractItems: ContractItem[];
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
  return requestClient.get<BusinessApi.ProjectSummary[]>('/app/business/projects', {
    params: { keyword },
  });
}

export async function getBusinessProjectDetailApi(projectId: string) {
  return requestClient.get<BusinessApi.ProjectDetail>(`/app/business/projects/${projectId}`);
}

export async function getBusinessReportApi(params?: BusinessApi.ReportQuery) {
  return requestClient.get<BusinessApi.Report>('/app/business/reports', {
    params,
  });
}
