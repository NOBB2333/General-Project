import { defineReferenceFixture } from '@/components/page';
import type { FeishuConversation, FeishuMessage } from './feishu-orders.types';

const conversations: readonly FeishuConversation[] = [
  {
    id: 'comap-bot',
    name: 'Comap · 销售助手',
    preview: '(进行中)上传PO自动开单 demo',
    time: '刚刚',
    unread: 0,
    pinned: true,
    tone: 'bot',
  },
  {
    id: 'abc',
    name: '上海 ABC 贸易 · 陈采购',
    preview: '陈采购:本周还能再排一单吗',
    time: '10:42',
    unread: 2,
    pinned: false,
    tone: 'coral',
  },
  {
    id: 'li-eng',
    name: '李总 · XX 工程',
    preview: '你:报价单已发,请查收',
    time: '昨天',
    unread: 0,
    pinned: false,
    tone: 'green',
  },
  {
    id: 'ops',
    name: '销售部 · 全员通知',
    preview: '@all 周五对账例会改到下午4点',
    time: '周一',
    unread: 12,
    pinned: false,
    tone: 'purple',
  },
  {
    id: 'warehouse',
    name: '仓库 · 浦东中心仓',
    preview: '浦东仓:SD-20260520-003 已出库',
    time: '周一',
    unread: 0,
    pinned: false,
    tone: 'orange',
  },
];

export const initialFeishuMessages: readonly FeishuMessage[] = [
  {
    id: 'message-bot-welcome',
    sender: 'bot',
    kind: 'text',
    time: '20:48',
    text: '你好张经理👋 我是 Comap 销售助手。\n你可以直接在这里:\n· 上传客户的采购PO(PDF/图片/Excel),我帮你识别下单\n· 用一句话告诉我「给上海ABC贸易开个单,3 个法兰各 50 件」,我也能直接草拟\n· 已有订单时,发「SO20260526001 收到首款50万」登记回款,或「发货100件」生成发货单',
  },
];

export const feishuOrdersReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/sales/conversational',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: { activeConversation: 'comap-bot', activePanel: 'trace', step: 0 },
  rows: conversations,
});
