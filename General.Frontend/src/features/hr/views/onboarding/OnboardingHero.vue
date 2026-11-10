<script setup lang="ts">
import { DoorOpen, Sparkles } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 入职离职</p>
      <div :class="$style.title">
        <span
          ><DoorOpen
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>入职离职管理</h1>
          <p>员工入职流程、离职手续、试用期评估与人事变动跟踪</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <span :class="$style.door" />
      <span :class="$style.floor" />
      <Sparkles
        :size="13"
        :stroke-width="1.8"
        :class="$style.sparkleOne"
      /><Sparkles
        :size="10"
        :stroke-width="1.8"
        :class="$style.sparkleTwo"
      />
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[520px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(180deg, #123783, #07599e 62%, #05a9c9);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(14 51 122 / 72%), transparent);
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
.door {
  @apply absolute bottom-0 left-[35%] h-[108px] w-[98px] border-x-2 border-t-2 border-white/30;
  background: linear-gradient(90deg, #baa15d, #e5ca81 58%, #af9653);
  box-shadow: 0 0 52px rgb(255 231 151 / 55%);
}
.door::after {
  content: '';
  @apply absolute right-2 top-1/2 h-2 w-2 rounded-full bg-white/70;
}
.floor {
  @apply absolute bottom-0 left-0 right-0 h-11;
  background: linear-gradient(155deg, transparent 49.5%, rgb(255 255 255 / 18%) 50%, transparent 50.5%);
}
.sparkleOne,
.sparkleTwo {
  @apply absolute text-amber-100;
}
.sparkleOne {
  @apply left-[18%] top-[62%];
}
.sparkleTwo {
  @apply right-[25%] top-[65%];
}
</style>
