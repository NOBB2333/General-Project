import type { CompensationItem, PayslipRow } from './compensation.types';

export const salaryComposition: readonly CompensationItem[] = [
  { id: 'base', label: '基本工资', value: '¥18,000.00' },
  { id: 'allowance', label: '岗位津贴', value: '¥3,000.00' },
  { id: 'performance', label: '绩效奖金', value: '¥4,500.00' },
  { id: 'overtime', label: '加班费', value: '¥1,200.00' },
  { id: 'subsidy', label: '各类补贴', value: '¥1,000.00' },
];
export const deductions: readonly CompensationItem[] = [
  { id: 'insurance', label: '社会保险(个人)', value: '¥2,834.00', note: '养老+医疗+失业' },
  { id: 'fund', label: '住房公积金(个人)', value: '¥2,160.00', note: '缴存比例12%' },
  { id: 'tax', label: '个人所得税', value: '¥2,872.00', note: '累计预扣法' },
  { id: 'other', label: '其他扣款', value: '¥0.00', note: '无' },
];
export const payslips: readonly PayslipRow[] = [
  { id: 'p1', month: '2026-02', gross: '¥27,700.00', deductions: '¥7,866.00', net: '¥19,834.00', status: '已发放' },
  { id: 'p2', month: '2026-01', gross: '¥28,100.00', deductions: '¥7,921.00', net: '¥20,179.00', status: '已发放' },
  { id: 'p3', month: '2025-12', gross: '¥29,500.00', deductions: '¥8,306.00', net: '¥21,194.00', status: '已发放' },
];
export const benefits: readonly CompensationItem[] = [
  { id: 'b1', label: '年度补充医疗', value: '¥18,000.00', note: '公司承担' },
  { id: 'b2', label: '年度培训额度', value: '¥20,000.00', note: '剩余 ¥12,400' },
  { id: 'b3', label: '节日与生日福利', value: '¥4,600.00', note: '年度累计' },
  { id: 'b4', label: '交通与通讯补贴', value: '¥14,400.00', note: '按月发放' },
];
