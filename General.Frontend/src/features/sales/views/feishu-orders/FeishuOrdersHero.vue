<script setup lang="ts">
import { MessageSquare, RotateCcw } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

withDefaults(defineProps<{ mode?: 'ai' | 'feishu'; title?: string; subtitle?: string }>(), {
  mode: 'feishu',
  title: '',
  subtitle: '',
});
const emit = defineEmits<{ reset: [] }>();
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav aria-label="面包屑">
        <span>{{ mode === 'ai' ? 'AI 管理' : '销售业务' }}</span
        ><b aria-hidden="true">›</b><strong>{{ mode === 'ai' ? 'AI 对话' : '对话开单(飞书集成预览)' }}</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><MessageSquare
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>{{ mode === 'ai' ? title || 'AI 对话' : '对话开单' }}</h1>
          <p v-if="mode === 'ai'">
            {{ subtitle || '通过现有 AI 会话服务进行对话，计划、引用和运行状态以服务端记录为准。' }}
          </p>
          <p v-else>
            客户在飞书 IM 里通过自然语言或上传PO,完成 销售订单 → 回款登记 → 发货单
            全链路。本页为产品预览,所有数据均为脚本化演示。
          </p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.messageLine" /><span :class="$style.messageLineShort" /><span :class="$style.cardLine" />
    </div>
    <UiButton
      v-if="mode !== 'ai'"
      :class="$style.reset"
      variant="ghost"
      size="sm"
      @click="emit('reset')"
      ><template #leading
        ><RotateCcw
          :size="13"
          :stroke-width="1.8" /></template
      >重置演示</UiButton
    >
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(120deg, #1f3d8a 0%, #3370ff 55%, #5aa9ff 100%);
}
.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background-image: radial-gradient(circle, rgb(255 255 255 / 22%) 0 2px, transparent 2.5px);
  background-position: 70% 18px;
  background-size: 85px 80px;
  opacity: 0.65;
}
.copy {
  @apply relative z-[1] max-w-[72%];
}
.copy nav {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 75%);
}
.copy nav strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 22%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12px] leading-5;
  color: rgb(255 255 255 / 86%);
}
.scene {
  @apply pointer-events-none absolute right-[13%] top-4 h-[100px] w-[300px];
}
.messageLine,
.messageLineShort,
.cardLine {
  @apply absolute block rounded-xl;
}
.messageLine {
  @apply right-0 top-2 h-[42px] w-[185px];
  background: rgb(255 255 255 / 92%);
}
.messageLine::before,
.messageLine::after {
  @apply absolute left-4 h-1.5 rounded bg-[#3370ff];
  content: '';
}
.messageLine::before {
  @apply top-3 w-[130px];
}
.messageLine::after {
  @apply top-6 w-[95px] opacity-60;
}
.messageLineShort {
  @apply left-0 top-[64px] h-[36px] w-[170px];
  background: #3370ff;
}
.messageLineShort::after {
  @apply absolute left-4 top-[15px] h-1.5 w-[110px] rounded bg-white opacity-80;
  content: '';
}
.cardLine {
  @apply -bottom-8 right-2 h-[48px] w-[220px] border-t-[5px] border-[#3370ff] bg-white opacity-95;
}
.reset {
  @apply absolute right-6 top-[58px] z-[2] border text-white;
  border-color: rgb(255 255 255 / 24%);
  background: rgb(255 255 255 / 14%);
}
.reset:hover {
  color: white;
  background: rgb(255 255 255 / 24%);
}
</style>
