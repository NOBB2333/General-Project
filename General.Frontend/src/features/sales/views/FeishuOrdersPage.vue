<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { feishuOrdersReferenceFixture, initialFeishuMessages } from '../model/feishu-orders.data';
import type {
  FeishuConversation,
  FeishuFlowKind,
  FeishuMessage,
  FeishuTraceDocument,
} from '../model/feishu-orders.types';
import FeishuChatPanel from './feishu-orders/FeishuChatPanel.vue';
import FeishuConversationList from './feishu-orders/FeishuConversationList.vue';
import FeishuOrdersHero from './feishu-orders/FeishuOrdersHero.vue';
import FeishuTracePanel from './feishu-orders/FeishuTracePanel.vue';

const props = withDefaults(
  defineProps<{
    conversationCount?: number;
    messageCount?: number;
    mode?: 'ai' | 'feishu';
    pageLabel?: string;
    pageModule?: string;
    pageRoute?: string;
  }>(),
  { mode: 'feishu', pageLabel: '', pageModule: '', pageRoute: '' },
);
const conversations = cloneReferenceRows(feishuOrdersReferenceFixture.rows);
const activeConversationId = ref('comap-bot');
const activePanel = ref<'trace' | 'context'>('trace');
const messages = ref<FeishuMessage[]>(cloneInitialMessages());
const step = ref(0);
let flowVersion = 0;

const activeConversation = computed<FeishuConversation>(
  () => conversations.find((conversation) => conversation.id === activeConversationId.value) ?? conversations[0]!,
);
const documents = computed<FeishuTraceDocument[]>(() =>
  messages.value
    .filter((message) => message.kind === 'result' && message.flow && message.docNo && message.title && message.summary)
    .map((message) => ({
      id: message.id,
      kind: message.flow as FeishuFlowKind,
      docNo: message.docNo as string,
      title: message.title as string,
      summary: message.summary as string,
      badges: message.badges ?? [],
    })),
);

function timestamp() {
  return new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit', hour12: false });
}

function cloneInitialMessages(): FeishuMessage[] {
  return structuredClone(initialFeishuMessages) as FeishuMessage[];
}

function resetDemo() {
  flowVersion += 1;
  messages.value = cloneInitialMessages();
  step.value = 0;
  activeConversationId.value = 'comap-bot';
  activePanel.value = 'trace';
  toast.info('演示已重置');
}

function previewMessage(flow: FeishuFlowKind): FeishuMessage {
  if (flow === 'order') {
    return {
      id: 'preview-order',
      sender: 'bot',
      kind: 'preview',
      flow,
      time: timestamp(),
      title: '已从PO识别到 销售订单 草稿',
      source: '来源:上海ABC贸易-采购订单-20260520.pdf(2页)',
      fields: [
        { label: '客户', value: '上海 ABC 贸易有限公司' },
        { label: '客户PO号', value: 'ABC-PO-2026-0517' },
        { label: '收货地址', value: '上海市浦东新区张江路 88 号' },
        { label: '付款条件', value: '首款30% / 发货60% / 验收10%' },
        { label: '交期', value: '2026-06-20 之前' },
        { label: '物料', value: '3 行 / 360 件' },
      ],
      total: '¥18,020.00',
      primaryLabel: '确认创建销售订单',
    };
  }
  if (flow === 'payment') {
    return {
      id: 'preview-payment',
      sender: 'bot',
      kind: 'preview',
      flow,
      time: timestamp(),
      title: '已识别为 销售回款 登记',
      source: '对应订单:SO-20260526-001 · 上海 ABC 贸易',
      fields: [
        { label: '关联订单', value: 'SO-20260526-001' },
        { label: '回款金额', value: '¥5,406.00' },
        { label: '付款方式', value: '微信支付' },
        { label: '回款阶段', value: '首款(计划第1期)' },
      ],
      total: '¥5,406.00',
      primaryLabel: '登记回款',
    };
  }
  return {
    id: 'preview-shipment',
    sender: 'bot',
    kind: 'preview',
    flow,
    time: timestamp(),
    title: '已生成 销售发货单 草稿',
    source: '对应订单:SO-20260526-001 · 上海 ABC 贸易',
    fields: [
      { label: '关联订单', value: 'SO-20260526-001' },
      { label: '计划发货日期', value: '2026-05-28' },
      { label: '收货联系人', value: '陈采购 · 13900001234' },
      { label: '承运方式', value: '陆运(顺丰快运)' },
    ],
    total: '¥14,280.00',
    primaryLabel: '创建发货单',
  };
}

function resultMessage(flow: FeishuFlowKind): FeishuMessage {
  if (flow === 'order')
    return {
      id: 'result-order',
      sender: 'bot',
      kind: 'result',
      flow,
      time: timestamp(),
      docNo: 'SO-20260526-001',
      title: '销售订单已创建',
      summary: '上海 ABC 贸易有限公司 · 3 行物料 · ¥18,020.00 · 状态:已审批',
      badges: ['已审批', '首款待收', '未发货'],
    };
  if (flow === 'payment')
    return {
      id: 'result-payment',
      sender: 'bot',
      kind: 'result',
      flow,
      time: timestamp(),
      docNo: 'PC-20260526-001',
      title: '回款已登记 · 等待财务确认',
      summary: '¥5,406.00 · 微信支付 · 已核销首款计划(第 1/3 期)',
      badges: ['已登记', '首款已收', '待财务确认'],
    };
  return {
    id: 'result-shipment',
    sender: 'bot',
    kind: 'result',
    flow,
    time: timestamp(),
    docNo: 'SD-20260526-001',
    title: '发货单已创建 · 通知仓库备货',
    summary: '2 行物料(140 件) · ¥14,280.00 · 计划发货 2026-05-28',
    badges: ['待出库', '已通知仓库', '顺丰快运'],
  };
}

function appendPreview(flow: FeishuFlowKind, thinking: string) {
  const version = flowVersion;
  messages.value.push({
    id: `thinking-${flow}-${Date.now()}`,
    sender: 'bot',
    kind: 'thinking',
    time: timestamp(),
    text: thinking,
  });
  window.setTimeout(() => {
    if (version !== flowVersion) return;
    messages.value = [...messages.value.filter((message) => message.kind !== 'thinking'), previewMessage(flow)];
  }, 260);
}

function uploadPo() {
  if (step.value !== 0) return;
  messages.value.push({
    id: 'upload-po',
    sender: 'user',
    kind: 'attachment',
    time: timestamp(),
    fileName: '上海ABC贸易-采购订单-20260520.pdf',
    fileMeta: 'PDF · 1.2 MB · 2页',
  });
  step.value = 1;
  appendPreview('order', '正在解析 PO 文件…抽取客户、SKU、数量、价格、交期');
}

function beginPayment() {
  if (step.value !== 2) return;
  messages.value.push({
    id: 'request-payment',
    sender: 'user',
    kind: 'text',
    time: timestamp(),
    text: 'SO-20260526-001 客户已付首款 5406 元,微信支付,请登记回款',
  });
  step.value = 3;
  appendPreview('payment', '识别为回款登记…匹配订单 + 解析金额/方式/流水号');
}

function beginShipment() {
  if (step.value !== 2 && step.value !== 4) return;
  messages.value.push({
    id: 'request-shipment',
    sender: 'user',
    kind: 'text',
    time: timestamp(),
    text: 'SO-20260526-001 这周先发 法兰DN50 100件、DN80 40件',
  });
  step.value = 5;
  appendPreview('shipment', '解析发货指令…对比订单未发数量,生成发货单草稿');
}

function confirmPreview(id: string) {
  const preview = messages.value.find((message) => message.id === id);
  if (!preview?.flow || preview.decided) return;
  preview.decided = true;
  messages.value.push(resultMessage(preview.flow));
  step.value = preview.flow === 'order' ? 2 : preview.flow === 'payment' ? 4 : 6;
  activePanel.value = 'trace';
  toast.success(
    preview.flow === 'order'
      ? '已创建 SO-20260526-001'
      : preview.flow === 'payment'
        ? '已登记回款 PC-20260526-001'
        : '已创建发货单 SD-20260526-001',
  );
}

function editPreview() {
  toast.info('演示环境：实际系统会打开字段编辑器，允许逐字段修改后再确认');
}

function sendMessage(text: string) {
  const version = flowVersion;
  messages.value.push({ id: `user-${Date.now()}`, sender: 'user', kind: 'text', time: timestamp(), text });
  window.setTimeout(() => {
    if (version !== flowVersion) return;
    messages.value.push({
      id: `bot-${Date.now()}`,
      sender: 'bot',
      kind: 'text',
      time: timestamp(),
      text: '演示环境仅执行预设链路。请使用快捷按钮体验上传PO、登记回款和登记发货。',
    });
  }, 220);
}

function createConversation() {
  toast.info('演示环境不会创建真实飞书会话');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="custom"
    :data-page-route="props.pageRoute || '/sales/feishu-orders'"
    :data-page-label="props.pageLabel || (props.mode === 'ai' ? 'AI 对话' : '对话开单(飞书)')"
    :data-page-module="props.pageModule || (props.mode === 'ai' ? 'AI 管理' : '销售业务')"
    :data-feishu-orders-page="props.mode === 'feishu' || undefined"
    :data-ai-conversations-page="props.mode === 'ai' || undefined"
    :data-conversation-count="props.mode === 'ai' ? props.conversationCount : undefined"
    :data-message-count="props.mode === 'ai' ? props.messageCount : messages.length"
    :data-flow-step="props.mode === 'feishu' ? step : undefined"
    :data-document-count="props.mode === 'feishu' ? documents.length : undefined"
  >
    <slot name="hero">
      <FeishuOrdersHero @reset="resetDemo" />
    </slot>
    <section :class="$style.workspace">
      <slot name="workspace">
        <FeishuConversationList
          :conversations="conversations"
          :active-id="activeConversationId"
          @select="activeConversationId = $event"
          @create="createConversation"
        />
        <FeishuChatPanel
          :conversation="activeConversation"
          :messages="messages"
          :step="step"
          @upload="uploadPo"
          @confirm="confirmPreview"
          @edit="editPreview"
          @payment="beginPayment"
          @shipment="beginShipment"
          @reset="resetDemo"
          @send="sendMessage"
        />
        <FeishuTracePanel
          :active-panel="activePanel"
          :documents="documents"
          :step="step"
          @update:active-panel="activePanel = $event"
        />
      </slot>
    </section>
    <slot name="dialogs" />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-6;
  color: var(--ui-text);
}
.workspace {
  @apply flex gap-3;
  height: calc(100vh - 300px);
  min-height: 520px;
}
</style>
