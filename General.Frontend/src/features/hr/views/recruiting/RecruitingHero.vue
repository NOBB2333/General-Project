<script setup lang="ts">
import { Crosshair, Sparkles, UserRoundPlus } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 招聘管理</p>
      <div :class="$style.title">
        <span :class="$style.icon"
          ><UserRoundPlus
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>招聘管理</h1>
          <p>岗位发布、候选人漏斗、面试评估、Offer 管理</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <span :class="$style.funnelTop" />
      <span :class="$style.funnelBottom" />
      <span :class="$style.target"
        ><Crosshair
          :size="78"
          :stroke-width="1.2" /></span
      ><Sparkles
        :size="18"
        :stroke-width="1.8"
        :class="$style.sparkleOne"
      /><Sparkles
        :size="13"
        :stroke-width="1.8"
        :class="$style.sparkleTwo"
      />
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[490px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #087a56, #079c9c 58%, #22ba77);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(5 111 77 / 75%), transparent);
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
.icon {
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
  background: linear-gradient(90deg, rgb(255 255 255 / 8%), transparent 64%);
}
.funnelTop,
.funnelBottom {
  @apply absolute left-[6%] w-[46%] border-x border-white/45;
}
.funnelTop {
  @apply top-0 h-[60%];
  clip-path: polygon(0 0, 100% 0, 72% 100%, 28% 100%);
  background: linear-gradient(180deg, rgb(255 255 255 / 8%), transparent);
}
.funnelTop::after,
.funnelTop::before {
  content: '';
  @apply absolute left-0 right-0 border-t border-dashed border-white/25;
}
.funnelTop::before {
  @apply top-3;
}
.funnelTop::after {
  @apply top-[58%];
}
.funnelBottom {
  @apply bottom-0 left-[19%] h-[40%] w-[20%];
  clip-path: polygon(0 0, 100% 0, 68% 100%, 32% 100%);
}
.target {
  @apply absolute right-[13%] top-1/2 -translate-y-1/2 text-white/50;
}
.sparkleOne,
.sparkleTwo {
  @apply absolute text-amber-300;
}
.sparkleOne {
  @apply right-[6%] top-5;
}
.sparkleTwo {
  @apply bottom-5 right-[26%];
}
</style>
