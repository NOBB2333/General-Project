<script setup lang="ts">
import { Bot, CheckCircle2, Plug, Sparkles } from 'lucide-vue-next';
import type { FeishuTraceDocument } from '../../model/feishu-orders.types';

defineProps<{ activePanel: 'trace' | 'context'; documents: readonly FeishuTraceDocument[]; step: number }>();
const emit = defineEmits<{ 'update:activePanel': [value: 'trace' | 'context'] }>();
</script>

<template>
  <aside
    :class="$style.panel"
    aria-label="会话业务面板"
  >
    <nav>
      <button
        type="button"
        :class="activePanel === 'trace' && $style.active"
        @click="emit('update:activePanel', 'trace')"
      >
        本次会话产生的单据</button
      ><button
        type="button"
        :class="activePanel === 'context' && $style.active"
        @click="emit('update:activePanel', 'context')"
      >
        接入说明 / 飞书配置
      </button>
    </nav>
    <div
      v-if="activePanel === 'trace'"
      :class="$style.body"
    >
      <div
        v-if="!documents.length"
        :class="$style.empty"
      >
        <span
          ><Sparkles
            :size="22"
            :stroke-width="1.8" /></span
        ><strong>会话尚未产生单据</strong>
        <p>在左侧聊天里点击「📎 上传客户PO(示例)」开始演示。每确认一张飞书卡片,这里都会沉淀一条单据痕迹。</p>
      </div>
      <ol
        v-else
        :class="$style.timeline"
      >
        <li
          v-for="(document, index) in documents"
          :key="document.id"
        >
          <span :class="$style.dot">{{ index + 1 }}</span>
          <article>
            <header>
              <CheckCircle2
                :size="14"
                :stroke-width="1.8"
              /><strong>{{ document.title }}</strong>
            </header>
            <a
              href="#"
              @click.prevent
              >{{ document.docNo }}</a
            >
            <p>{{ document.summary }}</p>
            <div>
              <span
                v-for="badge in document.badges"
                :key="badge"
                >{{ badge }}</span
              >
            </div>
          </article>
        </li>
      </ol>
    </div>
    <div
      v-else
      :class="$style.context"
    >
      <span :class="$style.contextIcon"
        ><Plug
          :size="21"
          :stroke-width="1.8"
      /></span>
      <h3>飞书机器人接入</h3>
      <p>通过飞书开放平台机器人 SDK 收发消息，并将识别结果写入 Comap 业务服务。</p>
      <dl>
        <div>
          <dt>应用状态</dt>
          <dd>演示环境</dd>
        </div>
        <div>
          <dt>机器人</dt>
          <dd>
            <Bot
              :size="12"
              :stroke-width="1.8"
            />Comap 销售助手
          </dd>
        </div>
        <div>
          <dt>当前流程</dt>
          <dd>步骤 {{ step }} / 6</dd>
        </div>
      </dl>
    </div>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply flex w-[340px] shrink-0 flex-col overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.panel > nav {
  @apply flex border-b;
  border-color: var(--ui-border);
}
.panel > nav button {
  @apply flex-1 border-b-2 border-transparent px-3 py-2.5 text-[11px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.panel > nav .active {
  @apply font-semibold;
  color: var(--ui-text);
  border-color: #3370ff;
}
.body {
  @apply min-h-0 flex-1 overflow-y-auto p-3;
}
.empty {
  @apply mx-auto max-w-[250px] pt-12 text-center;
}
.empty > span {
  @apply mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full;
  color: #97a0af;
  background: var(--ui-surface-hover);
}
.empty strong {
  @apply block text-[12px];
  color: var(--ui-text-muted);
}
.empty p {
  @apply mt-2 text-[10.5px] leading-[1.7];
  color: var(--ui-text-subtle);
}
.timeline {
  @apply relative pl-5;
}
.timeline::before {
  @apply absolute bottom-2 left-[7px] top-2 w-0.5;
  content: '';
  background: var(--ui-border);
}
.timeline li {
  @apply relative pb-4;
}
.dot {
  @apply absolute -left-5 top-0 z-[1] flex h-4 w-4 items-center justify-center rounded-full text-[8px] font-bold text-white;
  background: #3370ff;
}
.timeline article {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}
.timeline header {
  @apply flex items-center gap-1.5;
  color: var(--ui-success);
}
.timeline header strong {
  @apply text-[11px];
  color: var(--ui-text);
}
.timeline a {
  @apply mt-1 block text-[10px] font-semibold;
  color: #3370ff;
}
.timeline p {
  @apply mt-1 text-[9.5px] leading-4;
  color: var(--ui-text-subtle);
}
.timeline article > div {
  @apply mt-2 flex flex-wrap gap-1;
}
.timeline article > div span {
  @apply rounded px-1.5 py-0.5 text-[8px];
  color: #00875a;
  background: #e3fcef;
}
.context {
  @apply px-5 py-8 text-center;
}
.contextIcon {
  @apply mx-auto flex h-12 w-12 items-center justify-center rounded-full;
  color: #3370ff;
  background: #e6f0ff;
}
.context h3 {
  @apply mt-3 text-[13px] font-semibold;
  color: var(--ui-text);
}
.context > p {
  @apply mt-2 text-[10.5px] leading-5;
  color: var(--ui-text-subtle);
}
.context dl {
  @apply mt-5 space-y-2 text-left;
}
.context dl div {
  @apply flex items-center justify-between rounded border px-3 py-2;
  border-color: var(--ui-border-subtle);
}
.context dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.context dd {
  @apply flex items-center gap-1 text-[10px] font-medium;
  color: var(--ui-text);
}
</style>
