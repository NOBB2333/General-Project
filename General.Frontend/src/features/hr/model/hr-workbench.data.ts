import { defineReferenceFixture } from '@/components/page';
import type { EmployeeMovement, HrMetric, HrTrendPoint, RecruitmentPosition } from './hr-workbench.types';

export const hrMetrics: readonly HrMetric[] = [
  { id: 'active', label: '在职人数', value: '38', hint: '占总人数 90%', tone: 'blue' },
  { id: 'probation', label: '试用期', value: '3', hint: '未转正人员', tone: 'orange' },
  { id: 'joined', label: '本月入职', value: '1', hint: '新员工', tone: 'green' },
  { id: 'left', label: '本月离职', value: '1', hint: '离职率 2.4%', tone: 'red' },
  { id: 'recruiting', label: '招聘中岗位', value: '4', hint: '待招聘', tone: 'purple' },
  { id: 'salary', label: '月薪资总额', value: '¥78.3万', hint: '人均 ¥18,643', tone: 'cyan' },
];

export const employeeMovements: readonly EmployeeMovement[] = [
  { id: 'm1', name: '张丽娜', department: '工程部', position: '助理工程师', date: '2026-03-01', type: '入职' },
  { id: 'm2', name: '周文淑', department: '人事行政部', position: '人事专员', date: '2026-02-20', type: '入职' },
  { id: 'm3', name: '赵雪琴', department: '工程部', position: '助理工程师', date: '2026-01-15', type: '入职' },
  { id: 'm4', name: '郑伟亮', department: '销售部', position: '销售代表', date: '2026-02-28', type: '离职' },
];

export const recruitmentPositions: readonly RecruitmentPosition[] = [
  { id: 'r1', position: '高级DCS工程师', applications: 12, interviews: 4, offers: 0, status: '招聘中' },
  { id: 'r2', position: '销售经理', applications: 8, interviews: 3, offers: 1, status: '招聘中' },
  { id: 'r3', position: '助理工程师', applications: 25, interviews: 8, offers: 1, status: '招聘中' },
  { id: 'r4', position: '研发工程师', applications: 15, interviews: 5, offers: 0, status: '招聘中' },
];

export const hrTrend: readonly HrTrendPoint[] = [
  { month: '2025-10', total: 39, regular: 35, probation: 4, cost: 702 },
  { month: '2025-11', total: 40, regular: 36, probation: 4, cost: 725 },
  { month: '2025-12', total: 41, regular: 37, probation: 4, cost: 748 },
  { month: '2026-01', total: 41, regular: 37, probation: 4, cost: 756 },
  { month: '2026-02', total: 42, regular: 38, probation: 4, cost: 770 },
  { month: '2026-03', total: 42, regular: 38, probation: 4, cost: 783 },
];

export const hrWorkbenchFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/hr/workbench',
  capturedAt: '2026-08-16',
  rows: recruitmentPositions,
  defaults: { cutoff: '2026-03-15' },
});
