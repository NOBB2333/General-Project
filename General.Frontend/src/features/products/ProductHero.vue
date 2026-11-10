<script setup lang="ts">
import { Boxes } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.breadcrumbs">基础资料 <span>›</span> <strong>商品管理</strong></div>
    <div :class="$style.heading">
      <span :class="$style.iconFrame">
        <Boxes
          :size="25"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h1 :class="$style.title">商品管理</h1>
        <p :class="$style.description">维护 SKU 主数据、品牌型号与多规格变体，沉淀商品资产</p>
      </div>
    </div>

    <div
      :class="$style.illustration"
      aria-hidden="true"
    >
      <div :class="$style.illustrationGlow" />
      <div :class="$style.illustrationGrid">
        <template
          v-for="n in 12"
          :key="n"
        >
          <div :class="$style.illustrationCard">
            <template v-if="n > 4">
              <i :class="$style.cardLine" />
              <i :class="$style.cardLineShort" />
              <i :class="$style.cardDot" />
            </template>
          </div>
        </template>
      </div>
      <div :class="$style.illustrationBase" />
    </div>
  </section>
</template>

<style module>
@reference "../../styles/index.css";
.hero {
  @apply relative overflow-hidden rounded-2xl bg-gradient-to-r from-app-primary-hover via-app-primary to-[#2568E9] px-6 py-5 text-white;
}
.breadcrumbs {
  @apply relative z-10 mb-3 flex items-center gap-1 text-[12px] font-medium text-white/75;
}
.breadcrumbs span {
  @apply text-white/50;
}
.breadcrumbs strong {
  @apply font-semibold text-white;
}
.heading {
  @apply relative z-10 flex items-center gap-3;
}
.iconFrame {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-white/20 bg-white/15 text-white backdrop-blur-sm;
}
.title {
  @apply text-[22px] font-bold tracking-[-.02em] text-white;
}
.description {
  @apply mt-1 text-[12.5px] text-white/85;
}
.illustration {
  @apply absolute inset-y-0 right-0 w-[42%] overflow-hidden;
}
.illustrationGlow {
  @apply absolute -right-8 -top-28 h-[310px] w-[310px] rounded-full bg-white/[.04];
}
.illustrationGrid {
  @apply absolute bottom-[-2px] left-10 grid grid-cols-4 gap-2 opacity-40;
}
.illustrationCard {
  @apply relative h-[42px] w-[46px] rounded bg-white/90;
}
.cardLine {
  @apply absolute left-2 top-3 h-[2px] w-5 rounded bg-app-primary;
}
.cardLineShort {
  @apply absolute left-2 top-5 h-[2px] w-3 rounded bg-app-primary;
}
.cardDot {
  @apply absolute right-2 top-3 h-2 w-2 rounded-full bg-[#FFE380];
}
.illustrationBase {
  @apply absolute bottom-[-5px] right-32 h-5 w-2 rounded-t bg-white/70;
}
</style>
