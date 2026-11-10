import { defineReferenceFixture } from '@/components/page';
import type { MaterialRequest, MaterialStock } from './material-pickup.types';

export const materialRequestsFixture = defineReferenceFixture<MaterialRequest>({
  sourceUrl: 'https://demo.risemap.cn/ops/materials',
  capturedAt: '2026-09-17',
  rows: [
    {
      id: 'mr-01',
      code: 'MR-2026-0001',
      applicant: '张伟',
      department: '技术部',
      date: '2026-03-07',
      reason: '日常办公消耗',
      items: [
        { name: 'A4打印纸', quantity: '5包' },
        { name: '中性笔(黑)', quantity: '2盒' },
      ],
      status: '已领取',
    },
    {
      id: 'mr-02',
      code: 'MR-2026-0002',
      applicant: '王芳',
      department: '市场部',
      date: '2026-03-08',
      reason: '新员工入职配备',
      items: [
        { name: '无线鼠标', quantity: '1个' },
        { name: 'USB-C 转接器', quantity: '1个' },
      ],
      status: '已通过',
    },
    {
      id: 'mr-03',
      code: 'MR-2026-0003',
      applicant: '陈磊',
      department: '工程部',
      date: '2026-03-09',
      reason: '现场施工需要防护用品',
      items: [
        { name: '防护手套', quantity: '3盒' },
        { name: '安全帽', quantity: '2顶' },
      ],
      status: '待审批',
    },
    {
      id: 'mr-04',
      code: 'MR-2026-0004',
      applicant: '刘洋',
      department: '行政部',
      date: '2026-03-09',
      reason: '补充清洁用品',
      items: [{ name: '洗手液', quantity: '10瓶' }],
      status: '已驳回',
      comment: '数量过多，请调整为5瓶重新申请',
    },
  ],
});
export const materialStocks: readonly MaterialStock[] = [
  { id: 'stock-01', name: 'A4打印纸', unit: '包', stock: 36, warningAt: 10, price: 28 },
  { id: 'stock-02', name: '中性笔(黑)', unit: '盒', stock: 18, warningAt: 5, price: 42 },
  { id: 'stock-03', name: '无线鼠标', unit: '个', stock: 12, warningAt: 4, price: 89 },
  { id: 'stock-04', name: 'USB-C 转接器', unit: '个', stock: 8, warningAt: 3, price: 129 },
  { id: 'stock-05', name: '防护手套', unit: '盒', stock: 2, warningAt: 3, price: 65 },
  { id: 'stock-06', name: '安全帽', unit: '顶', stock: 16, warningAt: 5, price: 55 },
  { id: 'stock-07', name: '洗手液', unit: '瓶', stock: 22, warningAt: 6, price: 18 },
  { id: 'stock-08', name: '文件夹', unit: '个', stock: 45, warningAt: 10, price: 12 },
  { id: 'stock-09', name: '标签纸', unit: '卷', stock: 20, warningAt: 5, price: 35 },
  { id: 'stock-10', name: '电池', unit: '盒', stock: 24, warningAt: 6, price: 32 },
];
