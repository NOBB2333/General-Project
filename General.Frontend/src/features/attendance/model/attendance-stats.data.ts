import { defineReferenceFixture } from '@/components/page';
import type { AttendanceStatsFixture, AttendanceStatsRow } from './attendance-stats.types';

export const attendanceStatsReferenceFixture = defineReferenceFixture<AttendanceStatsRow, AttendanceStatsFixture>({
  sourceUrl: 'https://demo.risemap.cn/hr/stats-dashboard',
  capturedAt: '2026-09-16',
  viewport: { width: 1618, height: 899 },
  trend: [
    { date: '03-02', normal: 7, late: 2, early: 1, absent: 0, leave: 1 },
    { date: '03-03', normal: 6, late: 2, early: 2, absent: 1, leave: 1 },
    { date: '03-04', normal: 7, late: 1, early: 1, absent: 1, leave: 0 },
    { date: '03-05', normal: 8, late: 2, early: 1, absent: 1, leave: 0 },
    { date: '03-06', normal: 6, late: 3, early: 1, absent: 0, leave: 0 },
    { date: '03-09', normal: 7, late: 2, early: 1, absent: 1, leave: 0 },
    { date: '03-10', normal: 8, late: 1, early: 1, absent: 0, leave: 0 },
  ],
  status: [
    { label: '正常', value: '49', tone: 'normal' },
    { label: '迟到', value: '13', tone: 'late' },
    { label: '早退', value: '8', tone: 'early' },
    { label: '迟到+早退', value: '4', tone: 'absent' },
    { label: '旷工', value: '5', tone: 'absent' },
    { label: '请假', value: '4', tone: 'leave' },
    { label: '出差', value: '3', tone: 'travel' },
  ],
  departments: [
    { rank: 1, name: '人事部', rate: '75%' },
    { rank: 2, name: '技术部', rate: '72%' },
    { rank: 3, name: '财务部', rate: '69.2%' },
    { rank: 4, name: '产品部', rate: '66.7%' },
    { rank: 5, name: '销售部', rate: '64.7%' },
    { rank: 6, name: '市场部', rate: '62.5%' },
  ],
  lateDistribution: [
    { label: '5-10分钟', value: 6 },
    { label: '10-20分钟', value: 8 },
    { label: '20-30分钟', value: 5 },
    { label: '30-60分钟', value: 3 },
    { label: '30分钟以上', value: 2 },
  ],
  rows: [
    { department: '产品部', normal: '12', late: '3', early: '2', absent: '1', rate: '66.7%', hours: '7.8h' },
    { department: '技术部', normal: '18', late: '4', early: '2', absent: '1', rate: '72%', hours: '8.2h' },
    { department: '市场部', normal: '10', late: '3', early: '2', absent: '1', rate: '62.5%', hours: '7.5h' },
    { department: '销售部', normal: '11', late: '2', early: '3', absent: '1', rate: '64.7%', hours: '7.6h' },
    { department: '财务部', normal: '9', late: '2', early: '1', absent: '1', rate: '69.2%', hours: '7.9h' },
    { department: '人事部', normal: '8', late: '1', early: '1', absent: '0', rate: '75%', hours: '8.0h' },
  ],
});
