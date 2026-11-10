import { defineReferenceFixture } from '@/components/page';
import type { LearningCertificate, LearningCourse } from './my-learning.types';
const rows: readonly LearningCourse[] = [
  {
    id: 'lc1',
    title: '产品知识·新品 MX-300 全解',
    category: '产品知识',
    lastLearned: '今天 11:20',
    progress: 60,
    deadline: '2026-05-30',
    required: true,
    status: '在学',
  },
  {
    id: 'lc2',
    title: 'Salesforce 般的「销售商机推进 7 步法」',
    category: '销售技能',
    lastLearned: '昨天 18:10',
    progress: 35,
    deadline: '2026-06-20',
    required: false,
    status: '在学',
  },
  {
    id: 'lc3',
    title: '新员工入职通关',
    category: '入职培训',
    lastLearned: '2026-04-18',
    progress: 100,
    deadline: '2026-04-30',
    required: true,
    status: '已完成',
  },
  {
    id: 'lc4',
    title: '信息安全年度合规',
    category: '合规培训',
    lastLearned: '2026-03-12',
    progress: 100,
    deadline: '2026-06-15',
    required: true,
    status: '已完成',
  },
];
export const myLearningFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/hr/training/my',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  rows,
});
export const myCertificates: readonly LearningCertificate[] = [
  { id: 'mc1', title: 'Comap 新员工通关证书', code: 'CERT-2026-001', expiresAt: '永久', status: '有效' },
  { id: 'mc2', title: 'MX-300 销售上岗资格', code: 'CERT-2026-002', expiresAt: '2027-03-18', status: '有效' },
  { id: 'mc3', title: '年度合规培训证书', code: 'CERT-2025-009', expiresAt: '2026-06-04', status: '即将到期' },
];
