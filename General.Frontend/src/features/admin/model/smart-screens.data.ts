import { defineReferenceFixture } from '@/components/page';
import type { ScreenActivity } from './smart-screens.types';

export const screenActivitiesFixture = defineReferenceFixture<ScreenActivity>({
  sourceUrl: 'https://demo.risemap.cn/ops/screens',
  capturedAt: '2026-09-17',
  rows: [
    {
      id: 'screen-01',
      time: '14:02',
      tone: '生成',
      tag: '客户欢迎',
      text: 'AI生成「华能电力欢迎屏」3版本，已自动选用「正式+科技感」',
      status: '等待审核',
    },
    {
      id: 'screen-02',
      time: '14:00',
      tone: '发布',
      tag: '销售战报',
      text: '成交喜报已发布到「销售部战报屏」「老板办公室屏」',
      status: '成功',
    },
    {
      id: 'screen-03',
      time: '13:58',
      tone: '建议',
      tag: '',
      text: '前台屏已连续播放28天，建议更新企业文化或客户案例',
      status: '成功',
    },
    {
      id: 'screen-04',
      time: '13:50',
      tone: '播插',
      tag: '车间任务',
      text: 'P0紧急通知触发：检测到南京车间网络异常，自动切换离线缓存',
      status: '成功',
    },
    {
      id: 'screen-05',
      time: '13:42',
      tone: '审核',
      tag: '优秀员工',
      text: '「5月优秀员工榜单」AI风险检查通过 · 无敏感词',
      status: '成功',
    },
    {
      id: 'screen-06',
      time: '13:30',
      tone: '生成',
      tag: '销售战报',
      text: 'AI根据销售签单事件自动生成「成交喜报」短视频脚本',
      status: '成功',
    },
    {
      id: 'screen-07',
      time: '13:15',
      tone: '建议',
      tag: '销售战报',
      text: '本周成交订单5笔，建议生成销售周战报上墙',
      status: '成功',
    },
    {
      id: 'screen-08',
      time: '12:00',
      tone: '发布',
      tag: '节日祝福',
      text: '节日祝福「母亲节温馨问候」已自动发布到全部办公区屏',
      status: '成功',
    },
  ],
});
