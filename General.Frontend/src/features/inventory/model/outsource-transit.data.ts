import { defineReferenceFixture } from '@/components/page';
import type { OutsourceTransitRecord } from './outsource-transit.types';

const rows: readonly OutsourceTransitRecord[] = [
  {
    id: 'outsource-stock-001',
    workOrder: '—',
    workOrderName: '手工录入',
    purchaseOrder: 'OS-2026-0518-004',
    supplier: '东莞顺达注塑科技',
    supplierCode: 'W-WX-004',
    productCode: 'RM-ABS-K9806',
    productName: 'ABS粒料 K9806',
    specification: '25kg/袋',
    sent: 80,
    consumed: 0,
    returned: 0,
    balance: 80,
    ageDays: 9,
    amount: 960,
    unit: 'kg',
  },
  {
    id: 'outsource-stock-002',
    workOrder: 'MO-2026-0418',
    workOrderName: 'DCS主控外壳生产工单',
    purchaseOrder: 'OS-2026-0512-001',
    supplier: '苏州精机外协加工厂',
    supplierCode: 'W-WX-001',
    productCode: 'RM-AL-6061-25',
    productName: '6061铝合金板料',
    specification: '190×150×3mm',
    sent: 210,
    consumed: 120,
    returned: 10,
    balance: 80,
    ageDays: 15,
    amount: 1440,
    unit: '块',
  },
  {
    id: 'outsource-stock-003',
    workOrder: 'MO-2026-0418',
    workOrderName: 'DCS主控外壳生产工单',
    purchaseOrder: 'OS-2026-0512-001',
    supplier: '苏州精机外协加工厂',
    supplierCode: 'W-WX-001',
    productCode: 'RM-AL-6061-50',
    productName: '6061铝合金块料',
    specification: '260×190×65mm',
    sent: 210,
    consumed: 124,
    returned: 0,
    balance: 86,
    ageDays: 15,
    amount: 3870,
    unit: '块',
  },
];

export const outsourceTransitReferenceFixture = defineReferenceFixture<
  OutsourceTransitRecord,
  {
    sourceUrl: string;
    capturedAt: string;
    viewport: { width: number; height: number };
    rows: readonly OutsourceTransitRecord[];
  }
>({
  sourceUrl: 'https://demo.risemap.cn/inventory/subcontract-stock',
  capturedAt: '2026-09-17',
  viewport: { width: 1618, height: 899 },
  rows,
});
