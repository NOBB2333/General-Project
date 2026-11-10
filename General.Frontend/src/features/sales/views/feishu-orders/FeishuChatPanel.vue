<script setup lang="ts">
import { Bot, Ellipsis, Phone, Pin, Search, Sparkles, Video } from 'lucide-vue-next';
import { computed, nextTick, ref, watch } from 'vue';
import type { FeishuConversation, FeishuMessage } from '../../model/feishu-orders.types';
import FeishuComposer from './FeishuComposer.vue';
import FeishuMessageBubble from './FeishuMessageBubble.vue';

const props = withDefaults(
  defineProps<{
    busy?: boolean;
    conversation: FeishuConversation;
    mode?: 'ai' | 'feishu';
    modelName?: string;
    notice?: string;
    readonly?: boolean;
    messages: readonly FeishuMessage[];
    step: number;
  }>(),
  { busy: false, mode: 'feishu', modelName: '', notice: '', readonly: false },
);
const emit = defineEmits<{
  upload: [];
  confirm: [id: string];
  edit: [id: string];
  'confirm-plan': [id: string];
  'create-plan-draft': [id: string];
  'toggle-plan-step': [payload: { isDone: boolean; messageId: string; stepId: string }];
  retry: [text: string];
  cancel: [];
  payment: [];
  shipment: [];
  reset: [];
  send: [text: string];
}>();
const messageList = ref<HTMLElement | null>(null);
const isBotConversation = computed(() => props.mode === 'ai' || props.conversation.id === 'comap-bot');

watch(
  () => props.messages,
  async () => {
    await nextTick();
    if (messageList.value) messageList.value.scrollTop = messageList.value.scrollHeight;
  },
  { deep: true },
);
</script>

<template>
  <section
    :class="$style.chat"
    :aria-label="props.mode === 'ai' ? 'AI对话' : '飞书对话'"
  >
    <header :class="$style.chatHeader">
      <span :class="$style.botAvatar"
        ><Bot
          :size="16"
          :stroke-width="1.8"
      /></span>
      <div>
        <strong>{{ conversation.name }}</strong>
        <p>
          {{
            props.mode === 'ai'
              ? props.modelName || 'AI 会话 · 服务端记录'
              : isBotConversation
                ? '飞书机器人 · 接入 Comap ERP · 在线'
                : '飞书会话预览'
          }}
        </p>
      </div>
      <span
        v-if="isBotConversation && props.mode !== 'ai'"
        :class="$style.aiBadge"
        ><Sparkles
          :size="9"
          :stroke-width="1.8"
        />AI</span
      >
      <nav aria-label="会话工具">
        <slot
          v-if="props.mode === 'ai'"
          name="header-actions"
        />
        <template v-else>
          <button
            type="button"
            title="搜索"
          >
            <Search
              :size="15"
              :stroke-width="1.8"
            /></button
          ><button
            type="button"
            title="通话"
          >
            <Phone
              :size="15"
              :stroke-width="1.8"
            /></button
          ><button
            type="button"
            title="视频"
          >
            <Video
              :size="15"
              :stroke-width="1.8"
            /></button
          ><button
            type="button"
            title="置顶"
          >
            <Pin
              :size="15"
              :stroke-width="1.8"
            /></button
          ><button
            type="button"
            title="更多"
          >
            <Ellipsis
              :size="15"
              :stroke-width="1.8"
            />
          </button>
        </template>
      </nav>
    </header>
    <div
      v-if="props.notice"
      :class="$style.notice"
    >
      <span>i</span>{{ props.notice }}
    </div>
    <div
      v-else-if="isBotConversation && props.mode !== 'ai'"
      :class="$style.notice"
    >
      <span>i</span>这是 Comap 内置的“飞书 IM”预览界面。实际部署时此区域是飞书原生消息流,系统通过飞书机器人 SDK
      收发消息,数据落到 Comap 后端。
    </div>
    <div
      ref="messageList"
      :class="$style.messages"
    >
      <div
        v-if="props.mode === 'ai' && !messages.length"
        :class="$style.otherConversation"
      >
        <strong>{{ conversation.name }}</strong>
        <p>从已有会话中选择一条，或新建会话后发送第一条消息。</p>
      </div>
      <template v-if="isBotConversation && !(props.mode === 'ai' && !messages.length)">
        <div :class="$style.day">
          <span>{{ props.mode === 'ai' ? '会话消息' : '今天' }}</span>
        </div>
        <FeishuMessageBubble
          v-for="message in messages"
          :key="message.id"
          :message="message"
          :mode="props.mode"
          @confirm="emit('confirm', $event)"
          @edit="emit('edit', $event)"
          @confirm-plan="emit('confirm-plan', $event)"
          @create-plan-draft="emit('create-plan-draft', $event)"
          @toggle-plan-step="emit('toggle-plan-step', $event)"
          @retry="emit('retry', $event)"
        />
      </template>
      <div
        v-else-if="props.mode !== 'ai'"
        :class="$style.otherConversation"
      >
        <strong>{{ conversation.name }}</strong>
        <p>演示侧栏仅展示会话入口，完整消息流由飞书原生客户端承载。</p>
      </div>
    </div>
    <div
      v-if="isBotConversation && props.mode !== 'ai'"
      :class="$style.quickActions"
    >
      <button
        v-if="step === 0"
        type="button"
        :class="$style.primaryAction"
        @click="emit('upload')"
      >
        📎 上传客户PO(示例)
      </button>
      <button
        v-else-if="step === 1 || step === 3 || step === 5"
        type="button"
        disabled
      >
        👇 在上方卡片确认
      </button>
      <template v-else-if="step === 2"
        ><button
          type="button"
          :class="$style.primaryAction"
          @click="emit('payment')"
        >
          💵 登记首款回款</button
        ><button
          type="button"
          @click="emit('shipment')"
        >
          🚚 登记发货
        </button></template
      >
      <button
        v-else-if="step === 4"
        type="button"
        :class="$style.primaryAction"
        @click="emit('shipment')"
      >
        🚚 登记发货
      </button>
      <button
        v-else-if="step === 6"
        type="button"
        :class="$style.primaryAction"
        @click="emit('reset')"
      >
        🔄 重置演示重新开始
      </button>
    </div>
    <FeishuComposer
      v-if="isBotConversation"
      :mode="props.mode"
      :busy="props.busy"
      :readonly="props.readonly"
      @upload="emit('upload')"
      @send="emit('send', $event)"
      @cancel="emit('cancel')"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.chat {
  @apply flex min-w-0 flex-1 flex-col overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.chatHeader {
  @apply flex h-[52px] shrink-0 items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.botAvatar {
  @apply flex h-8 w-8 items-center justify-center rounded-full text-white;
  background: #3370ff;
}
.chatHeader > div {
  @apply min-w-0;
}
.chatHeader strong {
  @apply block truncate text-[13px] font-semibold;
  color: var(--ui-text);
}
.chatHeader p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.aiBadge {
  @apply inline-flex items-center gap-0.5 rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: #3370ff;
  background: #e6f0ff;
}
.chatHeader nav {
  @apply ml-auto flex items-center gap-1;
  color: var(--ui-text-subtle);
}
.chatHeader nav button {
  @apply rounded p-1.5 transition-colors;
}
.chatHeader nav button:hover {
  background: var(--ui-surface-hover);
}
.notice {
  @apply flex items-center gap-2 border-b px-4 py-2 text-[10.5px] leading-4;
  color: #7a5800;
  border-color: #ffe9a8;
  background: #fffae6;
}
.notice span {
  @apply flex h-3 w-3 items-center justify-center rounded-full border text-[8px] font-bold;
  border-color: #b88700;
}
.messages {
  @apply min-h-0 flex-1 overflow-y-auto bg-[#f4f5f7] px-5 py-3;
}
.day {
  @apply my-2 flex justify-center;
}
.day span {
  @apply rounded-full px-2.5 py-0.5 text-[9px] font-medium;
  color: var(--ui-text-subtle);
  background: rgb(223 225 230 / 85%);
}
.otherConversation {
  @apply m-auto max-w-[360px] pt-24 text-center;
}
.otherConversation strong {
  @apply text-[14px];
  color: var(--ui-text);
}
.otherConversation p {
  @apply mt-2 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.quickActions {
  @apply flex flex-wrap gap-2 border-t bg-white px-4 pb-1 pt-2;
  border-color: var(--ui-border);
}
.quickActions button {
  @apply rounded-full px-3 py-1.5 text-[10.5px] font-medium transition-colors;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.quickActions button:disabled {
  @apply cursor-default;
  color: #97a0af;
}
.quickActions .primaryAction {
  @apply text-white;
  background: #3370ff;
}
</style>
