import { defineReferenceFixture } from '@/components/page';
import type { DrawingLinksFixture } from './drawing-links.types';

export const drawingLinksReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/drawing/association',
  capturedAt: '2026-09-17',
  viewport: { width: 1618, height: 899 },
  rows: [
    { id: 'query-001', drawingNo: 'DWG-2025-001', name: '主轴承座', queriedAt: '2025-04-08 09:15', source: '手工查询' },
    {
      id: 'query-002',
      drawingNo: 'DWG-2025-003',
      name: '传动齿轮组',
      queriedAt: '2025-04-07 16:30',
      source: '图号档案',
    },
    {
      id: 'query-003',
      drawingNo: 'DWG-2025-005',
      name: '机架焊接总成',
      queriedAt: '2025-04-07 11:00',
      source: '版本详情',
    },
    { id: 'query-004', drawingNo: 'DWG-2025-007', name: '密封端盖', queriedAt: '2025-04-06 14:20', source: '手工查询' },
    {
      id: 'query-005',
      drawingNo: 'DWG-2025-002',
      name: '电机安装板',
      queriedAt: '2025-04-05 10:45',
      source: '图号档案',
    },
  ],
  associations: [
    { id: 'link-001', category: 'BOM', code: 'BOM-2025-012', description: '主轴承座总成 BOM', status: '生效中' },
    { id: 'link-002', category: '物料', code: 'MAT-00128', description: '主轴承座毛坯', status: '在用' },
    { id: 'link-003', category: '采购', code: 'PO-2025-038', description: '精密机械厂采购单', status: '执行中' },
    { id: 'link-004', category: '库存', code: 'LOT-2025-0310', description: '上海主仓批次', status: '可用' },
    { id: 'link-005', category: '变更', code: 'ECN-2025-005', description: '客户要求 V2 → V3', status: '执行中' },
  ],
} satisfies DrawingLinksFixture);
