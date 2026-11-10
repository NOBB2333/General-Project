import { defineReferenceFixture } from '@/components/page';
import type { SalesVisit } from './sales-checkin.types';
const rows: readonly SalesVisit[] = [
  {
    id: 'visit-01',
    customer: '通用电气南京',
    time: '14:00-16:00',
    address: '南京市江宁区将军大道 8 号',
    purpose: '方案讲解',
    priority: 'B',
    status: '待拜访',
    distance: '48m',
  },
  {
    id: 'visit-02',
    customer: '宝钢工程技术',
    time: '16:30-17:30',
    address: '上海市宝山区富锦路 1881 号',
    purpose: '关系维护',
    priority: 'B',
    status: '待拜访',
  },
  {
    id: 'visit-03',
    customer: '华能电力科技有限公司',
    time: '09:18-10:55',
    address: '南京市鼓楼区中山北路 2 号',
    purpose: '需求沟通',
    priority: 'A',
    status: '已完成',
    distance: '35m',
    duration: '97min',
    summary: '对接二期 DCS 改造范围，确认本周五前提交初版方案。',
  },
];
export const salesCheckinReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/sales/check-in',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: { tab: 'today' },
  rows,
});
