import { defineReferenceFixture } from '@/components/page';
import type { RefundMetric, RefundRow } from './refunds.types';

export const refundMetrics = [
  { id: 'approval', label: '待审批', value: '3 笔', tone: 'warning' },
  { id: 'processing', label: '处理中', value: '3 笔', tone: 'primary' },
  { id: 'pendingAmount', label: '待退金额', value: '¥34.0万', tone: 'violet' },
  { id: 'writtenOff', label: '已核销合计', value: '¥5.1万', tone: 'success' },
] as const satisfies readonly RefundMetric[];

const customerRows: readonly RefundRow[] = [
  {
    id: 'customer-refund-0012',
    kind: 'customer',
    code: 'REF-2026-0012',
    contract: 'HT-2026-001',
    party: '深圳市科创园区管理有限公司',
    description: '智慧园区监控系统采购合同',
    currency: 'CNY',
    amount: 44_850,
    method: '银行转账',
    status: '待审批',
    approvalStatus: '待审批',
    paymentStatus: '待付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-03-05',
  },
  {
    id: 'customer-refund-0011',
    kind: 'customer',
    code: 'REF-2026-0011',
    contract: 'HT-2026-002',
    party: '杭州恒通科技有限公司',
    description: '办公设备年度采购框架协议',
    currency: 'CNY',
    amount: 13_000,
    method: '原路退回',
    status: '审批中',
    approvalStatus: '审批中',
    paymentStatus: '待付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-03-03',
  },
  {
    id: 'customer-refund-0010',
    kind: 'customer',
    code: 'REF-2026-0010',
    contract: 'HT-2026-003',
    party: '上海宝钢设备部',
    description: '工厂自动化控制系统合同',
    currency: 'USD',
    amount: 5_165,
    method: '电汇',
    status: '已批准',
    approvalStatus: '已通过',
    paymentStatus: '待付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-02-28',
  },
  {
    id: 'customer-refund-0009',
    kind: 'customer',
    code: 'REF-2026-0009',
    contract: 'HT-2026-005',
    party: '华能电力科技有限公司',
    description: 'DCS系统升级改造合同',
    currency: 'CNY',
    amount: 85_500,
    method: '银行转账',
    status: '已付款',
    approvalStatus: '已通过',
    paymentStatus: '已付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-02-20',
  },
  {
    id: 'customer-refund-0008',
    kind: 'customer',
    code: 'REF-2026-0008',
    contract: 'HT-2026-004',
    party: '南京智安科技有限公司',
    description: '网络安全设备采购合同',
    currency: 'CNY',
    amount: 22_000,
    method: '承兑汇票',
    status: '已核销',
    approvalStatus: '已通过',
    paymentStatus: '已付款',
    writeoffStatus: '已核销',
    requestedAt: '2026-02-15',
  },
  {
    id: 'customer-refund-0007',
    kind: 'customer',
    code: 'REF-2026-0007',
    contract: 'HT-2026-006',
    party: '中石化仪控中心',
    description: '实验室检测设备合同',
    currency: 'EUR',
    amount: 3_680,
    method: '电汇',
    status: '已核销',
    approvalStatus: '已通过',
    paymentStatus: '已付款',
    writeoffStatus: '已核销',
    requestedAt: '2026-02-10',
  },
  {
    id: 'customer-refund-0006',
    kind: 'customer',
    code: 'REF-2026-0006',
    contract: 'HT-2025-028',
    party: '广州物流科技',
    description: '仓储系统集成合同',
    currency: 'CNY',
    amount: 15_000,
    method: '银行转账',
    status: '已驳回',
    approvalStatus: '已驳回',
    paymentStatus: '-',
    writeoffStatus: '-',
    requestedAt: '2026-02-05',
  },
  {
    id: 'customer-refund-0005',
    kind: 'customer',
    code: 'REF-2026-0005',
    contract: 'HT-2025-026',
    party: '苏州汇川技术',
    description: '变频器采购补充合同',
    currency: 'CNY',
    amount: 8_600,
    method: '原路退回',
    status: '草稿',
    approvalStatus: '-',
    paymentStatus: '-',
    writeoffStatus: '-',
    requestedAt: '2026-02-01',
  },
  {
    id: 'customer-refund-0004',
    kind: 'customer',
    code: 'REF-2026-0004',
    contract: 'HT-2025-024',
    party: '无锡远东电缆',
    description: '工业电缆采购合同',
    currency: 'CNY',
    amount: 36_800,
    method: '银行转账',
    status: '审批中',
    approvalStatus: '审批中',
    paymentStatus: '待付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-01-28',
  },
  {
    id: 'customer-refund-0003',
    kind: 'customer',
    code: 'REF-2026-0003',
    contract: 'HT-2025-021',
    party: '宁波海天机电',
    description: '注塑机控制系统合同',
    currency: 'CNY',
    amount: 62_000,
    method: '银行转账',
    status: '已付款',
    approvalStatus: '已通过',
    paymentStatus: '已付款',
    writeoffStatus: '待核销',
    requestedAt: '2026-01-22',
  },
];

const supplierNames = [
  '上海锐明自动化有限公司',
  '苏州工业设备厂',
  '杭州精密仪器有限公司',
  '南京电气成套有限公司',
  '宁波液压件制造厂',
  '无锡传感器科技',
  '常州工控元件公司',
  '嘉兴包装材料有限公司',
] as const;

const supplierRows: readonly RefundRow[] = supplierNames.map((party, index) => {
  const statuses: readonly RefundRow['status'][] = [
    '待审批',
    '审批中',
    '已批准',
    '已付款',
    '已核销',
    '草稿',
    '已核销',
    '已驳回',
  ];
  const status = statuses[index];
  const approved = ['已批准', '已付款', '已核销'].includes(status);
  const approvalStatus: RefundRow['approvalStatus'] = (() => {
    switch (status) {
      case '已批准':
      case '已付款':
      case '已核销':
        return '已通过';
      case '已驳回':
        return '已驳回';
      case '草稿':
        return '-';
      default:
        return status;
    }
  })();
  return {
    id: `supplier-refund-${index + 1}`,
    kind: 'supplier',
    code: `SREF-2026-${String(8 - index).padStart(4, '0')}`,
    contract: `PO-2026-${String(index + 21).padStart(3, '0')}`,
    party,
    description: '采购退货与供应商退款',
    currency: 'CNY',
    amount: [28_600, 16_800, 42_000, 35_500, 9_800, 12_600, 21_900, 7_500][index],
    method: index % 3 === 0 ? '原路退回' : '银行转账',
    status,
    approvalStatus,
    paymentStatus: status === '已付款' || status === '已核销' ? '已付款' : approved ? '待付款' : '-',
    writeoffStatus: status === '已核销' ? '已核销' : approved ? '待核销' : '-',
    requestedAt: `2026-03-${String(14 - index).padStart(2, '0')}`,
  } satisfies RefundRow;
});

export const refundsReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/finance/refunds',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: {
    activeKind: 'customer',
    activeStatus: '全部',
  },
  rows: [...customerRows, ...supplierRows],
});
