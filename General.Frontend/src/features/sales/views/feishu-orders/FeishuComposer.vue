<script setup lang="ts">
import { AtSign, Image, Paperclip, Send, Smile, Sparkles, Square } from 'lucide-vue-next';
import { ref } from 'vue';
import { UiTextarea } from '@/components/ui';

const props = withDefaults(defineProps<{ busy?: boolean; mode?: 'ai' | 'feishu'; readonly?: boolean }>(), {
  busy: false,
  mode: 'feishu',
  readonly: false,
});
const emit = defineEmits<{ cancel: []; upload: []; send: [text: string] }>();
const draft = ref('');

function sendMessage() {
  if (props.busy || props.readonly) return;
  const value = draft.value.trim();
  if (!value) return;
  emit('send', value);
  draft.value = '';
}
</script>

<template>
  <footer :class="$style.composer">
    <div :class="$style.tools">
      <button
        v-if="props.mode !== 'ai'"
        type="button"
        title="表情"
      >
        <Smile
          :size="15"
          :stroke-width="1.8"
        />
      </button>
      <button
        v-if="props.mode !== 'ai'"
        type="button"
        title="@"
      >
        <AtSign
          :size="15"
          :stroke-width="1.8"
        />
      </button>
      <button
        v-if="props.mode !== 'ai'"
        type="button"
        title="上传文件"
        @click="emit('upload')"
      >
        <Paperclip
          :size="15"
          :stroke-width="1.8"
        />
      </button>
      <button
        v-if="props.mode !== 'ai'"
        type="button"
        title="图片"
      >
        <Image
          :size="15"
          :stroke-width="1.8"
        />
      </button>
      <span
        ><Sparkles
          :size="10"
          :stroke-width="1.8"
        />{{ props.mode === 'ai' ? 'AI 对话' : 'AI 解析 PO/语义,落单到 Comap' }}</span
      >
    </div>
    <UiTextarea
      v-model="draft"
      rows="2"
      :disabled="props.busy || props.readonly"
      :placeholder="
        props.mode === 'ai'
          ? '输入消息…Enter 发送，Shift + Enter 换行'
          : '输入消息发给助手,例如「给上海ABC贸易开个单 100 件 FL-304-DN50」,或上传PO文件…'
      "
      @keydown.enter.exact.prevent="sendMessage"
    />
    <div :class="$style.footer">
      <span>{{ props.busy ? '正在等待 AI 响应' : 'Enter 发送 · Shift + Enter 换行' }}</span>
      <button
        v-if="props.busy"
        type="button"
        @click="emit('cancel')"
      >
        <Square
          :size="12"
          :stroke-width="1.8"
        />停止生成
      </button>
      <button
        v-else
        type="button"
        :disabled="props.readonly || !draft.trim()"
        @click="sendMessage"
      >
        <Send
          :size="12"
          :stroke-width="1.8"
        />发送
      </button>
    </div>
  </footer>
</template>

<style module>
@reference "../../../../styles/index.css";

.composer {
  @apply border-t bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.tools {
  @apply flex items-center gap-0.5 rounded-t-lg border border-b-0 px-2 pt-1.5;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.tools button {
  @apply rounded p-1.5 transition-colors;
}
.tools button:hover {
  background: var(--ui-surface-hover);
}
.tools span {
  @apply ml-auto inline-flex items-center gap-1 text-[9.5px];
  color: #97a0af;
}
.composer textarea {
  @apply min-h-[54px] rounded-none border-y-0 py-2 text-[12px];
  box-shadow: none;
}
.footer {
  @apply flex items-center justify-between rounded-b-lg border border-t-0 px-2 pb-2;
  color: #97a0af;
  border-color: var(--ui-border);
}
.footer > span {
  @apply text-[9.5px];
}
.footer button {
  @apply inline-flex items-center gap-1 rounded px-3 py-1 text-[10.5px] font-semibold text-white disabled:opacity-40;
  background: #3370ff;
}
</style>
