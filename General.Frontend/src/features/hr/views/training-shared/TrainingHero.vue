<script setup lang="ts">
import { GraduationCap } from 'lucide-vue-next';
defineProps<{ subtitle: string }>();
defineSlots<{ actions?: () => unknown }>();
</script>
<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 培训与考试</p>
      <div :class="$style.title">
        <span
          ><GraduationCap
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>培训与考试中心</h1>
          <p>{{ subtitle }}</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <i /><i /><i /><b />
    </div>
    <div
      v-if="$slots.actions"
      :class="$style.actions"
    >
      <slot name="actions" />
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative grid h-[120px] grid-cols-[520px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #074ba4, #0764d8 52%, #55439e);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(4 68 153 / 75%), transparent);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/75;
}
.copy > p span {
  @apply px-1.5;
}
.title {
  @apply mt-3 flex items-center gap-3;
}
.title > span {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/30 bg-white/15;
}
.title h1 {
  @apply m-0 text-[22px] font-semibold;
}
.title p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.visual {
  @apply relative overflow-hidden;
}
.visual::before {
  @apply absolute right-[18%] top-0 h-[92px] w-[210px] rounded-b-lg border border-white/15;
  content: '';
}
.visual::after {
  @apply absolute right-[20%] top-3 h-px w-[160px];
  content: '';
  background: rgb(255 255 255 / 20%);
  box-shadow:
    0 16px rgb(255 255 255 / 20%),
    0 32px rgb(255 255 255 / 20%);
}
.visual i {
  @apply absolute h-1.5 w-1.5 rounded-full bg-white/10;
}
.visual i:first-child {
  @apply left-[8%] top-6;
}
.visual i:nth-child(2) {
  @apply left-[24%] top-14;
}
.visual i:nth-child(3) {
  @apply left-[46%] top-8;
}
.visual b {
  @apply absolute bottom-7 right-[20%] h-6 w-6 rounded-full border border-white/20;
}
.actions {
  @apply absolute right-6 top-[58px] z-20 flex gap-2;
}
</style>
