import assert from 'node:assert/strict';
import test from 'node:test';

import {
  buildBusinessMutationInput,
  buildBusinessRecordRows,
  createBusinessRecordForm,
} from '../src/features/business/model/business-records.model.ts';

function createDetail(overrides = {}) {
  return {
    acceptances: [],
    budgetExecutions: [],
    businessChains: [],
    canViewProfitAnalysis: false,
    canViewSensitiveAmounts: true,
    contracts: [],
    forecastHistories: [],
    invoices: [],
    procurements: [],
    receivables: [],
    summary: {},
    ...overrides,
  };
}

test('budget mutation preserves entered decimal values and leaves variance to the server', () => {
  const form = {
    ...createBusinessRecordForm('budget', 'current-user'),
    adjustedAmount: '1250.375',
    budgetCode: 'BUD-001',
    category: '实施成本',
    executedAmount: '1250.625',
    sortOrder: '2',
  };

  assert.deepEqual(buildBusinessMutationInput('budget', form, 'current-user'), {
    kind: 'budget',
    input: {
      adjustedAmount: 1250.375,
      budgetCode: 'BUD-001',
      category: '实施成本',
      executedAmount: 1250.625,
      sortOrder: 2,
      varianceAmount: null,
    },
  });
});

test('budget and contract forms reject negative amounts before sending a request', () => {
  const budgetForm = {
    ...createBusinessRecordForm('budget', 'current-user'),
    adjustedAmount: '-0.01',
    budgetCode: 'BUD-001',
    category: '实施成本',
    executedAmount: '0',
  };
  assert.throws(() => buildBusinessMutationInput('budget', budgetForm, 'current-user'), /大于或等于零/u);

  const contractForm = {
    ...createBusinessRecordForm('contract', 'current-user'),
    amount: '-1',
    contractCode: 'CON-001',
    counterpartyName: '客户甲',
    status: '执行中',
    title: '项目合同',
    type: '销售合同',
  };
  assert.throws(() => buildBusinessMutationInput('contract', contractForm, 'current-user'), /大于或等于零/u);
});

test('invoice and receivable forms map the real DTO fields and preserve entered decimal amounts', () => {
  const invoiceForm = {
    ...createBusinessRecordForm('invoice', 'current-user'),
    amount: '125.375',
    invoiceCode: 'INV-001',
    invoiceDate: '2026-10-10',
    linkedContractCode: 'CON-001',
    receivableCode: 'REC-001',
    status: '已开票',
    taxAmount: '10.625',
    title: '项目发票',
  };
  assert.deepEqual(buildBusinessMutationInput('invoice', invoiceForm, 'current-user'), {
    kind: 'invoice',
    input: {
      amount: 125.375,
      clearLegalEntity: false,
      invoiceCode: 'INV-001',
      invoiceDate: '2026-10-10',
      legalEntityId: null,
      linkedContractCode: 'CON-001',
      receivableCode: 'REC-001',
      status: '已开票',
      taxAmount: 10.625,
      title: '项目发票',
    },
  });

  const receivableForm = {
    ...createBusinessRecordForm('receivable', 'current-user'),
    invoiceCode: 'INV-001',
    linkedContractCode: 'CON-001',
    plannedDate: '2026-11-01',
    receivableCode: 'REC-001',
    receivedAmount: '100.125',
    status: '部分回款',
    title: '项目应收',
    totalAmount: '300.875',
  };
  assert.deepEqual(buildBusinessMutationInput('receivable', receivableForm, 'current-user'), {
    kind: 'receivable',
    input: {
      invoiceCode: 'INV-001',
      linkedContractCode: 'CON-001',
      plannedDate: '2026-11-01',
      receivableCode: 'REC-001',
      receivedAmount: 100.125,
      status: '部分回款',
      title: '项目应收',
      totalAmount: 300.875,
    },
  });
});

test('contract and invoice forms preserve selected legal entity ids and explicitly clear existing associations', () => {
  const legalEntityId = 'entity-001';
  const contract = {
    ...createBusinessRecordForm('contract', 'current-user', {
      kind: 'contract',
      item: {
        amount: 100,
        contractCode: 'CON-001',
        counterpartyName: '客户甲',
        id: 'contract-001',
        isRevenueContract: true,
        legalEntityId,
        legalEntityNameSnapshot: '甲方科技有限公司',
        legalEntityTaxIdSnapshot: '91310000EXAMPLE001',
        parentContractCode: '',
        projectId: 'project-001',
        projectName: '项目甲',
        signDate: '2026-10-10',
        sourceChangeCode: '',
        status: '执行中',
        title: '实施合同',
        type: '销售合同',
      },
    }),
    amount: '100',
    status: '执行中',
  };

  assert.equal(contract.legalEntityId, legalEntityId);
  assert.equal(buildBusinessMutationInput('contract', contract, 'current-user').input.legalEntityId, legalEntityId);
  contract.legalEntityId = '';
  assert.equal(buildBusinessMutationInput('contract', contract, 'current-user').input.clearLegalEntity, true);
});

test('invoice and receivable forms reject negative amounts', () => {
  const invoiceForm = {
    ...createBusinessRecordForm('invoice', 'current-user'),
    amount: '100',
    invoiceCode: 'INV-001',
    status: '已开票',
    taxAmount: '-0.01',
    title: '项目发票',
  };
  assert.throws(() => buildBusinessMutationInput('invoice', invoiceForm, 'current-user'), /大于或等于零/u);

  const receivableForm = {
    ...createBusinessRecordForm('receivable', 'current-user'),
    receivedAmount: '-0.01',
    receivableCode: 'REC-001',
    status: '部分回款',
    title: '项目应收',
    totalAmount: '300',
  };
  assert.throws(() => buildBusinessMutationInput('receivable', receivableForm, 'current-user'), /大于或等于零/u);
});

test('new business chain assigns current user and edit preserves existing owner', () => {
  const created = createBusinessRecordForm('chain', 'current-user-id');
  assert.equal(created.ownerUserId, 'current-user-id');

  const existing = createBusinessRecordForm('chain', 'current-user-id', {
    kind: 'chain',
    item: {
      chainCode: 'CHN-001',
      id: 'chain-id',
      linkedContractCode: 'CON-001',
      ownerName: '原负责人',
      ownerUserId: 'existing-owner-id',
      sourceChangeCode: 'CHG-001',
      stage: '执行',
      status: '进行中',
      summary: '',
      title: '经营链事项',
      type: '交付',
    },
  });
  assert.equal(existing.ownerUserId, 'existing-owner-id');
  assert.equal(buildBusinessMutationInput('chain', existing, 'current-user-id').input.ownerUserId, 'existing-owner-id');
});

test('business record rows preserve amount masking and expose only in-scope record kinds', () => {
  const rows = buildBusinessRecordRows(
    createDetail({
      budgetExecutions: [
        {
          adjustedAmount: null,
          budgetCode: 'BUD-001',
          category: '实施成本',
          executedAmount: null,
          id: 'budget-id',
          projectId: 'project-id',
          projectName: '项目甲',
          sortOrder: 1,
          varianceAmount: null,
        },
      ],
    }),
    'budget',
  );

  assert.equal(rows.length, 1);
  assert.equal(rows[0].code, 'BUD-001');
  assert.match(rows[0].secondary, /调整后 — · 已执行 — · 差异 —/u);
  assert.equal(rows[0].target.kind, 'budget');
});

test('invoice and receivable record rows preserve service-side amount masking', () => {
  const detail = createDetail({
    invoices: [
      {
        amount: null,
        id: 'invoice-id',
        invoiceCode: 'INV-001',
        invoiceDate: '2026-10-10',
        linkedContractCode: 'CON-001',
        projectId: 'project-id',
        projectName: '项目甲',
        receivableCode: 'REC-001',
        status: '已开票',
        taxAmount: null,
        title: '项目发票',
      },
    ],
    receivables: [
      {
        id: 'receivable-id',
        invoiceCode: 'INV-001',
        linkedContractCode: 'CON-001',
        plannedDate: '2026-11-01',
        projectId: 'project-id',
        projectName: '项目甲',
        receivedAmount: null,
        receivableCode: 'REC-001',
        status: '部分回款',
        title: '项目应收',
        totalAmount: null,
      },
    ],
  });

  const invoiceRows = buildBusinessRecordRows(detail, 'invoice');
  const receivableRows = buildBusinessRecordRows(detail, 'receivable');
  assert.match(invoiceRows[0].secondary, /金额 — · 税额 —/u);
  assert.match(receivableRows[0].secondary, /应收 — · 已收 —/u);
});
