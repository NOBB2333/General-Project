<script setup lang="ts">
import { Bot, CheckCircle2, FileText, LoaderCircle, Pencil } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiCheckbox } from '@/components/ui';
import type { FeishuMessage } from '../../model/feishu-orders.types';

const props = withDefaults(defineProps<{ message: FeishuMessage; mode?: 'ai' | 'feishu' }>(), { mode: 'feishu' });
const message = computed(() => props.message);
const mode = computed(() => props.mode);
const emit = defineEmits<{
  confirm: [id: string];
  edit: [id: string];
  'confirm-plan': [id: string];
  'create-plan-draft': [id: string];
  'toggle-plan-step': [payload: { isDone: boolean; messageId: string; stepId: string }];
  retry: [text: string];
}>();
</script>

<template>
  <article :class="[$style.message, message.sender === 'user' && $style.userMessage]">
    <span :class="[$style.avatar, message.sender === 'user' && $style.userAvatar]">
      <b v-if="message.sender === 'user'">张</b
      ><Bot
        v-else
        :size="15"
        :stroke-width="1.8"
      />
    </span>
    <div :class="$style.content">
      <p :class="$style.sender">
        {{
          mode === 'ai'
            ? message.sender === 'bot'
              ? message.modelName || 'AI 助手'
              : '你'
            : message.sender === 'bot'
              ? 'Comap · 销售助手'
              : '张经理'
        }}
        <time>{{ message.time }}{{ message.statusLabel ? ` · ${message.statusLabel}` : '' }}</time>
      </p>
      <section
        v-if="mode === 'ai' && (message.errorMessage || message.retryText || message.transient)"
        :class="$style.previewCard"
      >
        <header>
          <div>
            <strong>{{ message.transient ? '本轮未保存' : 'AI 请求失败' }}</strong>
            <p>{{ message.transient ? '状态已重新读取，可安全重试。' : '本轮结果已由服务端记录。' }}</p>
          </div>
          <span>{{ message.statusLabel || '失败' }}</span>
        </header>
        <div :class="$style.textBubble">{{ message.errorMessage || message.text || '请求未完成' }}</div>
        <footer v-if="message.retryText">
          <button
            type="button"
            :class="$style.confirm"
            :disabled="message.actionBusy"
            @click="emit('retry', message.retryText!)"
          >
            {{ message.transient ? '重新发送' : '重试' }}
          </button>
        </footer>
      </section>
      <section
        v-else-if="mode === 'ai' && message.kind === 'plan' && message.aiPlan"
        :class="$style.previewCard"
      >
        <header>
          <div>
            <strong>{{ message.aiPlan.goal }}</strong>
            <p>{{ message.aiPlan.steps.length }} 个步骤 · {{ message.aiPlan.confirmed ? '已确认' : '等待确认' }}</p>
          </div>
          <span>执行计划</span>
        </header>
        <dl>
          <template
            v-for="(step, index) in message.aiPlan.steps"
            :key="step.id"
          >
            <div>
              <dt>步骤 {{ index + 1 }}</dt>
              <dd>
                <UiCheckbox
                  :model-value="step.isDone"
                  :disabled="message.actionBusy"
                  :label="`标记${step.title}为${step.isDone ? '未完成' : '已完成'}`"
                  @update:model-value="
                    emit('toggle-plan-step', { isDone: $event === true, messageId: message.id, stepId: step.id })
                  "
                />
                {{ step.title }}
              </dd>
            </div>
            <div v-if="step.expectedOutput">
              <dt>预期产出</dt>
              <dd>{{ step.expectedOutput }}</dd>
            </div>
          </template>
        </dl>
        <footer v-if="!message.aiPlan.confirmed">
          <button
            type="button"
            :disabled="message.actionBusy"
            @click="emit('create-plan-draft', message.id)"
          >
            生成任务草稿
          </button>
          <button
            type="button"
            :class="$style.confirm"
            :disabled="message.actionBusy"
            @click="emit('confirm-plan', message.id)"
          >
            {{ message.actionBusy ? '处理中…' : '确认计划' }}
          </button>
        </footer>
        <p
          v-else
          :class="$style.confirmed"
        >
          <CheckCircle2
            :size="13"
            :stroke-width="1.8"
          />计划已确认
        </p>
      </section>
      <div
        v-else-if="message.kind === 'text'"
        :class="$style.textBubble"
      >
        {{ message.text }}
      </div>
      <div
        v-else-if="message.kind === 'attachment'"
        :class="$style.attachment"
      >
        <FileText
          :size="22"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ message.fileName }}</strong
          ><span>{{ message.fileMeta }}</span>
        </div>
      </div>
      <div
        v-else-if="message.kind === 'thinking'"
        :class="$style.thinking"
      >
        <LoaderCircle
          :size="15"
          :stroke-width="1.8"
        />{{ message.text }}
      </div>
      <section
        v-else-if="message.kind === 'preview'"
        :class="$style.previewCard"
      >
        <header>
          <div>
            <strong>{{ message.title }}</strong>
            <p>{{ message.source }}</p>
          </div>
          <span>AI 识别</span>
        </header>
        <dl>
          <div
            v-for="field in message.fields"
            :key="field.label"
          >
            <dt>{{ field.label }}</dt>
            <dd>{{ field.value }}</dd>
          </div>
        </dl>
        <p
          v-if="message.total"
          :class="$style.total"
        >
          {{ message.flow === 'order' ? '订单总额(不含税)' : '金额' }}<strong>{{ message.total }}</strong>
        </p>
        <footer v-if="!message.decided">
          <button
            type="button"
            @click="emit('edit', message.id)"
          >
            <Pencil
              :size="12"
              :stroke-width="1.8"
            />我来改一下</button
          ><button
            type="button"
            :class="$style.confirm"
            @click="emit('confirm', message.id)"
          >
            <CheckCircle2
              :size="12"
              :stroke-width="1.8"
            />{{ message.primaryLabel }}
          </button>
        </footer>
        <p
          v-else
          :class="$style.confirmed"
        >
          <CheckCircle2
            :size="13"
            :stroke-width="1.8"
          />已确认并写入本地演示数据
        </p>
      </section>
      <section
        v-else-if="message.kind === 'result'"
        :class="$style.resultCard"
      >
        <header>
          <CheckCircle2
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <strong>{{ message.title }}</strong>
            <p>{{ message.docNo }}</p>
          </div>
        </header>
        <p>{{ message.summary }}</p>
        <div>
          <span
            v-for="badge in message.badges"
            :key="badge"
            >{{ badge }}</span
          >
        </div>
      </section>
      <section
        v-if="mode === 'ai' && message.aiRunSteps?.length"
        :class="$style.resultCard"
      >
        <header>
          <CheckCircle2
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <strong>本轮运行步骤</strong>
            <p>{{ message.statusLabel }}</p>
          </div>
        </header>
        <p
          v-for="step in message.aiRunSteps"
          :key="step.code"
        >
          {{ step.title }} · {{ step.status }}：{{ step.summary }}
        </p>
      </section>
      <section
        v-if="mode === 'ai' && message.aiToolInvocations?.length"
        :class="$style.resultCard"
      >
        <header>
          <Bot
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <strong>工具调用</strong>
            <p>{{ message.aiToolInvocations.length }} 次服务端记录</p>
          </div>
        </header>
        <p
          v-for="tool in message.aiToolInvocations"
          :key="tool.invocationId"
        >
          {{ tool.toolCode }} · {{ tool.status }} · {{ tool.elapsedMilliseconds }} ms{{
            tool.errorMessage ? ` · ${tool.errorMessage}` : ''
          }}
        </p>
      </section>
      <section
        v-if="mode === 'ai' && message.citations?.length"
        :class="$style.resultCard"
      >
        <header>
          <FileText
            :size="18"
            :stroke-width="1.8"
          />
          <div>
            <strong>引用来源</strong>
            <p>{{ message.citations.length }} 条</p>
          </div>
        </header>
        <p
          v-for="citation in message.citations"
          :key="citation"
        >
          {{ citation }}
        </p>
      </section>
      <p
        v-if="mode === 'ai' && message.tokenSummary"
        :class="$style.sender"
      >
        {{ message.tokenSummary }}
      </p>
    </div>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.message {
  @apply mb-4 flex gap-2.5;
}
.userMessage {
  @apply flex-row-reverse;
}
.avatar {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded text-white;
  background: #3370ff;
}
.userAvatar {
  @apply rounded-full;
  background: #0052cc;
}
.userAvatar b {
  @apply text-[11px];
}
.content {
  @apply max-w-[82%];
}
.sender {
  @apply mb-1 text-[11px] font-medium;
  color: var(--ui-text-subtle);
}
.userMessage .sender {
  @apply text-right;
}
.sender time {
  @apply ml-1 font-normal;
  color: #97a0af;
}
.textBubble {
  @apply whitespace-pre-line rounded-lg border bg-white px-3 py-2.5 text-[12px] leading-[1.55];
  color: var(--ui-text);
  border-color: var(--ui-border);
}
.userMessage .textBubble {
  @apply border-0 text-white;
  background: #3370ff;
}
.attachment {
  @apply flex items-center gap-3 rounded-lg border bg-white p-3;
  color: #3370ff;
  border-color: var(--ui-border);
}
.attachment div {
  @apply min-w-0;
}
.attachment strong {
  @apply block truncate text-[12px];
  color: var(--ui-text);
}
.attachment span {
  @apply mt-0.5 block text-[10px];
  color: var(--ui-text-subtle);
}
.thinking {
  @apply flex items-center gap-2 rounded-lg border bg-white px-3 py-2 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.thinking svg {
  @apply animate-spin;
}
.previewCard,
.resultCard {
  @apply overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
  box-shadow: 0 2px 6px rgb(9 30 66 / 8%);
}
.previewCard > header {
  @apply flex items-start justify-between border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.previewCard > header strong {
  @apply text-[13px];
  color: var(--ui-text);
}
.previewCard > header p {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}
.previewCard > header span {
  @apply rounded px-2 py-0.5 text-[9px] font-semibold;
  color: #3370ff;
  background: #e6f0ff;
}
.previewCard dl {
  @apply grid grid-cols-2 gap-x-5 gap-y-2 px-4 py-3;
}
.previewCard dl div {
  @apply min-w-0;
}
.previewCard dt {
  @apply text-[9px];
  color: #97a0af;
}
.previewCard dd {
  @apply truncate text-[11px] font-medium;
  color: var(--ui-text);
}
.total {
  @apply flex items-center justify-between border-t px-4 py-2 text-[10px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.total strong {
  @apply text-[14px];
  color: var(--ui-text);
}
.previewCard footer {
  @apply flex justify-end gap-2 border-t px-4 py-2.5;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.previewCard footer button {
  @apply inline-flex items-center gap-1 rounded border px-3 py-1.5 text-[10px] transition-colors;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: white;
}
.previewCard footer .confirm {
  @apply border-transparent text-white;
  background: #3370ff;
}
.confirmed {
  @apply flex items-center gap-1 border-t px-4 py-2 text-[10px];
  color: var(--ui-success);
  border-color: var(--ui-border-subtle);
  background: var(--ui-success-soft);
}
.resultCard {
  @apply border-l-4;
  border-left-color: #36b37e;
}
.resultCard > header {
  @apply flex items-center gap-2 px-4 pt-3;
  color: #36b37e;
}
.resultCard > header strong {
  @apply block text-[12px];
  color: var(--ui-text);
}
.resultCard > header p {
  @apply text-[10px] font-semibold;
  color: #3370ff;
}
.resultCard > p {
  @apply px-4 py-2 text-[10px] leading-4;
  color: var(--ui-text-subtle);
}
.resultCard > div {
  @apply flex gap-1.5 px-4 pb-3;
}
.resultCard > div span {
  @apply rounded px-2 py-0.5 text-[9px];
  color: #00875a;
  background: #e3fcef;
}
</style>
