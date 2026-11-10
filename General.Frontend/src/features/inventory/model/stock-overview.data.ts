import { defineReferenceFixture } from '@/components/page';
import type { InventoryMovement, StockOverviewFixture } from './stock-overview.types';

export const stockOverviewReferenceFixture = defineReferenceFixture<InventoryMovement, StockOverviewFixture>({
  sourceUrl: 'https://demo.risemap.cn/inventory/overview',
  capturedAt: '2026-09-16',
  viewport: { width: 1618, height: 899 },
  warehouses: [
    { name: '上海主仓', amount: 184, label: '¥120万' },
    { name: '项目仓(华能)', amount: 28, label: '¥22万' },
    { name: '项目仓(中石化)', amount: 17, label: '¥8.4万' },
    { name: '备品备件仓', amount: 34, label: '¥4万' },
  ],
  categories: [
    { name: '触摸屏', quantity: 46, tone: 'blue' },
    { name: 'PLC', quantity: 24, tone: 'cyan' },
    { name: '配件', quantity: 77, tone: 'green' },
    { name: '电源', quantity: 38, tone: 'orange' },
    { name: '其他', quantity: 92, tone: 'purple' },
  ],
  rows: [
    {
      id: 'TR-2026-0019',
      kind: '调拨出库',
      description: '普通调拨 → 华能DCS项目仓',
      quantity: '-15件',
      amount: '¥15.6万',
    },
    { id: 'TR-2026-0020', kind: '借出', description: '借出 → 华能电力科技', quantity: '-6件', amount: '¥4.3万' },
    {
      id: 'TR-2026-0018',
      kind: '变价调拨',
      description: '变价调拨 → 宝钢产线项目仓',
      quantity: '-8件',
      amount: '¥8.9万',
    },
    { id: 'IN-2026-0044', kind: '调入/归还', description: '采购入库 · 上海主仓', quantity: '+34件', amount: '¥22.4万' },
  ],
});
