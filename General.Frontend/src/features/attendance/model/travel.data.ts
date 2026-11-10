import { defineReferenceFixture } from '@/components/page';
import type { TravelRecord } from './travel.types';

const rows: readonly TravelRecord[] = [
  {
    id: 'travel-001',
    code: 'BT-2026-001',
    destination: '上海',
    transport: '高铁',
    reason: '参加客户需求对接会议',
    departureDate: '2026-03-20',
    returnDate: '2026-03-22',
    days: 3,
    budget: 3500,
    status: '待审批',
  },
  {
    id: 'travel-002',
    code: 'BT-2026-002',
    destination: '深圳',
    transport: '飞机',
    reason: '参加行业展会',
    departureDate: '2026-02-15',
    returnDate: '2026-02-17',
    days: 3,
    budget: 5200,
    status: '已报销',
  },
  {
    id: 'travel-005',
    code: 'BT-2026-005',
    destination: '成都',
    transport: '飞机',
    reason: '合作伙伴拜访',
    departureDate: '2026-01-10',
    returnDate: '2026-01-12',
    days: 3,
    budget: 4500,
    status: '已完成',
  },
];

export const travelReferenceFixture = defineReferenceFixture<
  TravelRecord,
  {
    sourceUrl: string;
    capturedAt: string;
    viewport: { width: number; height: number };
    rows: readonly TravelRecord[];
    defaults: { tab: string; status: string };
  }
>({
  sourceUrl: 'https://demo.risemap.cn/hr/business-trip',
  capturedAt: '2026-09-16',
  viewport: { width: 1618, height: 899 },
  rows,
  defaults: { tab: '我的出差', status: '全部状态' },
});
