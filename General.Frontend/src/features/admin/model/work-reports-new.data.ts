import { defineReferenceFixture } from '@/components/page';
import type { WorkReportNewSubmission, WorkReportNewTemplate } from './work-reports-new.types';

export const workReportNewTemplates: readonly WorkReportNewTemplate[] = [
  { id: 'research-weekly', name: '研发周报', frequency: '周报', state: 'active', tone: 'purple' },
  { id: 'sales-monthly', name: '销售月报', frequency: '月报', state: 'active', tone: 'teal' },
  { id: 'sales-progress', name: '销售进度汇报', frequency: '进度', state: 'active', tone: 'green' },
  { id: 'sales-weekly', name: '销售周报', frequency: '周报', state: 'retired', tone: 'muted' },
];

export const workReportsNewFixture = defineReferenceFixture<WorkReportNewSubmission>({
  sourceUrl: 'https://demo.risemap.cn/reportx',
  capturedAt: '2026-09-17',
  rows: [
    {
      id: 'reportx-zhang-ming',
      person: '张明',
      submitted: true,
      keyResults:
        '1）库存核销重构上线：压测 500 并发下单超卖归零（原每千单约 3 单超卖）；2）支付回调幂等改造灰度 20%，重复回调导致的重复发货问题 0 复现；3）核销链路单测覆盖率从 68% 提升到 85%。',
      details:
        '3 项：完成订单中心库存核销重构，并发下单不再超卖；支付回调幂等改造，灰度 20%；补齐核销链路单测，覆盖率 68%→85%。',
      nextPlan:
        '1）支付幂等灰度放量到 100% 并观察一周；2）启动退款核销自动化设计评审；3）把库存核销的告警接入值班大盘。',
      risks: '支付幂等放量存在回调风暴风险，已准备限流与降级预案；退款核销涉及财务口径，需财务同学一起对齐。',
      score: 87.5,
    },
    { id: 'reportx-li-si', person: '李四', submitted: false, keyResults: '', details: '', nextPlan: '', risks: '' },
    { id: 'reportx-wang-wu', person: '王五', submitted: false, keyResults: '', details: '', nextPlan: '', risks: '' },
    { id: 'reportx-zhao-min', person: '赵敏', submitted: false, keyResults: '', details: '', nextPlan: '', risks: '' },
  ],
});
