import type { BusinessApi } from '@/api/core/business';
import { formatBusinessAmount, formatBusinessDate } from './business.models.ts';

export type BusinessRecordKind =
  'acceptance' | 'budget' | 'chain' | 'contract' | 'invoice' | 'procurement' | 'receivable';

export type BusinessRecordTarget =
  | { kind: 'acceptance'; item: BusinessApi.AcceptanceItem }
  | { kind: 'budget'; item: BusinessApi.BudgetExecutionItem }
  | { kind: 'chain'; item: BusinessApi.BusinessChainItem }
  | { kind: 'contract'; item: BusinessApi.ContractItem }
  | { kind: 'invoice'; item: BusinessApi.InvoiceItem }
  | { kind: 'procurement'; item: BusinessApi.ProcurementItem }
  | { kind: 'receivable'; item: BusinessApi.ReceivableItem };

export interface BusinessRecordForm {
  acceptanceCode: string;
  acceptanceDate: string;
  acceptanceResult: string;
  adjustedAmount: string;
  amount: string;
  budgetCode: string;
  category: string;
  chainCode: string;
  contractCode: string;
  counterpartyName: string;
  originalLegalEntityId: string;
  executedAmount: string;
  isRevenueContract: string;
  invoiceCode: string;
  invoiceDate: string;
  legalEntityId: string;
  linkedContractCode: string;
  milestoneId: string;
  ownerUserId: string;
  parentContractCode: string;
  procurementCode: string;
  plannedDate: string;
  qualityInspectionCode: string;
  receivedAmount: string;
  receivableCode: string;
  signDate: string;
  sortOrder: string;
  sourceChangeCode: string;
  stage: string;
  status: string;
  summary: string;
  supplierName: string;
  taxAmount: string;
  title: string;
  totalAmount: string;
  type: string;
}

export interface BusinessRecordRow {
  code: string;
  id: string;
  kind: BusinessRecordKind;
  primary: string;
  secondary: string;
  status: string;
  target: BusinessRecordTarget;
}

export type BusinessMutationInput =
  | { kind: 'acceptance'; input: BusinessApi.AcceptanceSaveInput }
  | { kind: 'budget'; input: BusinessApi.BudgetExecutionSaveInput }
  | { kind: 'chain'; input: BusinessApi.BusinessChainSaveInput }
  | { kind: 'contract'; input: BusinessApi.ContractSaveInput }
  | { kind: 'invoice'; input: BusinessApi.InvoiceSaveInput }
  | { kind: 'procurement'; input: BusinessApi.ProcurementSaveInput }
  | { kind: 'receivable'; input: BusinessApi.ReceivableSaveInput };

function emptyForm(): BusinessRecordForm {
  return {
    acceptanceCode: '',
    acceptanceDate: '',
    acceptanceResult: '',
    adjustedAmount: '',
    amount: '',
    budgetCode: '',
    category: '',
    chainCode: '',
    contractCode: '',
    counterpartyName: '',
    executedAmount: '',
    isRevenueContract: 'true',
    invoiceCode: '',
    invoiceDate: '',
    legalEntityId: '',
    linkedContractCode: '',
    milestoneId: '',
    ownerUserId: '',
    originalLegalEntityId: '',
    parentContractCode: '',
    procurementCode: '',
    plannedDate: '',
    qualityInspectionCode: '',
    receivedAmount: '',
    receivableCode: '',
    signDate: '',
    sortOrder: '0',
    sourceChangeCode: '',
    stage: '',
    status: '',
    summary: '',
    supplierName: '',
    taxAmount: '',
    title: '',
    totalAmount: '',
    type: '',
  };
}

function amountText(value: null | number): string {
  return value === null || !Number.isFinite(value) ? '' : String(value);
}

function dateText(value: null | string): string {
  return value ? value.slice(0, 10) : '';
}

export function createBusinessRecordForm(
  kind: BusinessRecordKind,
  currentUserId: string,
  target?: BusinessRecordTarget,
): BusinessRecordForm {
  const form = emptyForm();
  if (!target) {
    if (kind === 'chain') form.ownerUserId = currentUserId;
    return form;
  }

  if (target.kind !== kind) return form;

  switch (target.kind) {
    case 'acceptance':
      return {
        ...form,
        acceptanceCode: target.item.acceptanceCode,
        acceptanceDate: dateText(target.item.acceptanceDate),
        acceptanceResult: target.item.acceptanceResult,
        linkedContractCode: target.item.linkedContractCode,
        milestoneId: target.item.milestoneId ?? '',
        qualityInspectionCode: target.item.qualityInspectionCode,
        status: target.item.status,
        title: target.item.title,
      };
    case 'budget':
      return {
        ...form,
        adjustedAmount: amountText(target.item.adjustedAmount),
        budgetCode: target.item.budgetCode,
        category: target.item.category,
        executedAmount: amountText(target.item.executedAmount),
        sortOrder: String(target.item.sortOrder),
      };
    case 'chain':
      return {
        ...form,
        chainCode: target.item.chainCode,
        linkedContractCode: target.item.linkedContractCode,
        ownerUserId: target.item.ownerUserId,
        sourceChangeCode: target.item.sourceChangeCode,
        stage: target.item.stage,
        status: target.item.status,
        summary: target.item.summary,
        title: target.item.title,
        type: target.item.type,
      };
    case 'contract':
      return {
        ...form,
        amount: amountText(target.item.amount),
        contractCode: target.item.contractCode,
        counterpartyName: target.item.counterpartyName,
        isRevenueContract: String(target.item.isRevenueContract),
        legalEntityId: target.item.legalEntityId ?? '',
        originalLegalEntityId: target.item.legalEntityId ?? '',
        parentContractCode: target.item.parentContractCode,
        signDate: dateText(target.item.signDate),
        sourceChangeCode: target.item.sourceChangeCode,
        status: target.item.status,
        title: target.item.title,
        type: target.item.type,
      };
    case 'invoice':
      return {
        ...form,
        amount: amountText(target.item.amount),
        invoiceCode: target.item.invoiceCode,
        invoiceDate: dateText(target.item.invoiceDate),
        legalEntityId: target.item.legalEntityId ?? '',
        originalLegalEntityId: target.item.legalEntityId ?? '',
        linkedContractCode: target.item.linkedContractCode,
        receivableCode: target.item.receivableCode,
        status: target.item.status,
        taxAmount: amountText(target.item.taxAmount),
        title: target.item.title,
      };
    case 'procurement':
      return {
        ...form,
        amount: amountText(target.item.amount),
        linkedContractCode: target.item.linkedContractCode,
        procurementCode: target.item.procurementCode,
        signDate: dateText(target.item.signDate),
        sourceChangeCode: target.item.sourceChangeCode,
        stage: target.item.stage,
        status: target.item.status,
        supplierName: target.item.supplierName,
        title: target.item.title,
      };
    case 'receivable':
      return {
        ...form,
        invoiceCode: target.item.invoiceCode,
        linkedContractCode: target.item.linkedContractCode,
        plannedDate: dateText(target.item.plannedDate),
        receivableCode: target.item.receivableCode,
        receivedAmount: amountText(target.item.receivedAmount),
        status: target.item.status,
        title: target.item.title,
        totalAmount: amountText(target.item.totalAmount),
      };
  }
}

export function buildBusinessRecordRows(
  detail: BusinessApi.ProjectDetail,
  kind: BusinessRecordKind,
): BusinessRecordRow[] {
  switch (kind) {
    case 'acceptance':
      return detail.acceptances.map((item) => ({
        code: item.acceptanceCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${formatBusinessDate(item.acceptanceDate)} · ${item.acceptanceResult} · 合同 ${item.linkedContractCode || '未关联'} · 里程碑 ${item.milestoneId || '未关联'} · 质量检查编号 ${item.qualityInspectionCode || '未填写'}`,
        status: item.status,
        target: { kind, item },
      }));
    case 'budget':
      return detail.budgetExecutions.map((item) => ({
        code: item.budgetCode,
        id: item.id,
        kind,
        primary: item.category,
        secondary: `调整后 ${formatBusinessAmount(item.adjustedAmount)} · 已执行 ${formatBusinessAmount(item.executedAmount)} · 差异 ${formatBusinessAmount(item.varianceAmount)}`,
        status: '',
        target: { kind, item },
      }));
    case 'chain':
      return detail.businessChains.map((item) => ({
        code: item.chainCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${item.type} · ${item.stage} · 负责人 ${item.ownerName} · 合同 ${item.linkedContractCode || '未关联'} · 变更 ${item.sourceChangeCode || '未关联'}${item.summary ? ` · ${item.summary}` : ''}`,
        status: item.status,
        target: { kind, item },
      }));
    case 'contract':
      return detail.contracts.map((item) => ({
        code: item.contractCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${item.type} · ${item.counterpartyName} · 主体 ${item.legalEntityNameSnapshot || '未指定'} · ${formatBusinessAmount(item.amount)} · 签约 ${formatBusinessDate(item.signDate)} · ${item.isRevenueContract ? '收入合同' : '成本合同'}`,
        status: item.status,
        target: { kind, item },
      }));
    case 'invoice':
      return detail.invoices.map((item) => ({
        code: item.invoiceCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${formatBusinessDate(item.invoiceDate)} · 主体 ${item.legalEntityNameSnapshot || '未指定'} · 金额 ${formatBusinessAmount(item.amount)} · 税额 ${formatBusinessAmount(item.taxAmount)} · 合同 ${item.linkedContractCode || '未关联'} · 应收 ${item.receivableCode || '未关联'}`,
        status: item.status,
        target: { kind, item },
      }));
    case 'procurement':
      return detail.procurements.map((item) => ({
        code: item.procurementCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${item.supplierName} · ${formatBusinessAmount(item.amount)} · ${item.stage} · 签约 ${formatBusinessDate(item.signDate)} · 合同 ${item.linkedContractCode || '未关联'}`,
        status: item.status,
        target: { kind, item },
      }));
    case 'receivable':
      return detail.receivables.map((item) => ({
        code: item.receivableCode,
        id: item.id,
        kind,
        primary: item.title,
        secondary: `${formatBusinessDate(item.plannedDate)} · 应收 ${formatBusinessAmount(item.totalAmount)} · 已收 ${formatBusinessAmount(item.receivedAmount)} · 合同 ${item.linkedContractCode || '未关联'} · 发票 ${item.invoiceCode || '未关联'}`,
        status: item.status,
        target: { kind, item },
      }));
  }
}

function parseAmount(raw: string, label: string): number {
  const value = Number(raw);
  if (!raw.trim() || !Number.isFinite(value) || value < 0) {
    throw new Error(`${label}必须是大于或等于零的有效金额。`);
  }
  return value;
}

function required(raw: string, label: string): string {
  const value = raw.trim();
  if (!value) throw new Error(`请填写${label}。`);
  return value;
}

function optional(raw: string): string {
  return raw.trim();
}

function optionalDate(raw: string): null | string {
  return raw.trim() || null;
}

export function buildBusinessMutationInput(
  kind: BusinessRecordKind,
  form: BusinessRecordForm,
  currentUserId: string,
): BusinessMutationInput {
  switch (kind) {
    case 'acceptance':
      return {
        kind,
        input: {
          acceptanceCode: required(form.acceptanceCode, '验收编号'),
          acceptanceDate: optionalDate(form.acceptanceDate),
          acceptanceResult: required(form.acceptanceResult, '验收结果'),
          linkedContractCode: optional(form.linkedContractCode),
          milestoneId: optional(form.milestoneId) || null,
          qualityInspectionCode: optional(form.qualityInspectionCode),
          status: required(form.status, '验收状态'),
          title: required(form.title, '验收事项'),
        },
      };
    case 'budget': {
      const sortOrder = Number(form.sortOrder);
      if (!Number.isInteger(sortOrder)) throw new Error('排序值必须是整数。');
      return {
        kind,
        input: {
          adjustedAmount: parseAmount(form.adjustedAmount, '调整后预算'),
          budgetCode: required(form.budgetCode, '预算编号'),
          category: required(form.category, '预算类别'),
          executedAmount: parseAmount(form.executedAmount, '已执行预算'),
          sortOrder,
          varianceAmount: null,
        },
      };
    }
    case 'chain': {
      const ownerUserId = optional(form.ownerUserId) || currentUserId;
      if (!ownerUserId) throw new Error('无法获取当前用户，不能保存经营链负责人。');
      return {
        kind,
        input: {
          chainCode: required(form.chainCode, '经营链编号'),
          linkedContractCode: optional(form.linkedContractCode),
          ownerUserId,
          sourceChangeCode: optional(form.sourceChangeCode),
          stage: required(form.stage, '阶段'),
          status: required(form.status, '状态'),
          summary: optional(form.summary),
          title: required(form.title, '事项名称'),
          type: required(form.type, '事项类型'),
        },
      };
    }
    case 'contract':
      return {
        kind,
        input: {
          amount: parseAmount(form.amount, '合同'),
          contractCode: required(form.contractCode, '合同编号'),
          counterpartyName: required(form.counterpartyName, '合同方'),
          clearLegalEntity: Boolean(form.originalLegalEntityId && !form.legalEntityId),
          isRevenueContract: form.isRevenueContract === 'true',
          legalEntityId: optional(form.legalEntityId) || null,
          parentContractCode: optional(form.parentContractCode),
          signDate: optionalDate(form.signDate),
          sourceChangeCode: optional(form.sourceChangeCode),
          status: required(form.status, '合同状态'),
          title: required(form.title, '合同名称'),
          type: required(form.type, '合同类型'),
        },
      };
    case 'invoice':
      return {
        kind,
        input: {
          amount: parseAmount(form.amount, '发票'),
          invoiceCode: required(form.invoiceCode, '发票编号'),
          invoiceDate: optionalDate(form.invoiceDate),
          clearLegalEntity: Boolean(form.originalLegalEntityId && !form.legalEntityId),
          legalEntityId: optional(form.legalEntityId) || null,
          linkedContractCode: optional(form.linkedContractCode),
          receivableCode: optional(form.receivableCode),
          status: required(form.status, '发票状态'),
          taxAmount: parseAmount(form.taxAmount, '税额'),
          title: required(form.title, '发票事项'),
        },
      };
    case 'procurement':
      return {
        kind,
        input: {
          amount: parseAmount(form.amount, '采购'),
          linkedContractCode: optional(form.linkedContractCode),
          procurementCode: required(form.procurementCode, '采购编号'),
          signDate: optionalDate(form.signDate),
          sourceChangeCode: optional(form.sourceChangeCode),
          stage: required(form.stage, '采购阶段'),
          status: required(form.status, '采购状态'),
          supplierName: required(form.supplierName, '供应商'),
          title: required(form.title, '采购事项'),
        },
      };
    case 'receivable':
      return {
        kind,
        input: {
          invoiceCode: optional(form.invoiceCode),
          linkedContractCode: optional(form.linkedContractCode),
          plannedDate: optionalDate(form.plannedDate),
          receivableCode: required(form.receivableCode, '回款编号'),
          receivedAmount: parseAmount(form.receivedAmount, '已回款'),
          status: required(form.status, '回款状态'),
          title: required(form.title, '应收事项'),
          totalAmount: parseAmount(form.totalAmount, '应收'),
        },
      };
  }
}
