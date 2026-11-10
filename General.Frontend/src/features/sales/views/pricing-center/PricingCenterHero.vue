<script setup lang="ts">
import { Target } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>销售业务</span><b aria-hidden="true">›</b><strong>价格策略中心</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Target
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>价格策略中心</h1>
          <p>统一管理商品定价、折扣策略、框架协议及特价审批</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.targetArt"
      aria-hidden="true"
    >
      <i /><i /><i /><b /> <span /><span /><span /><span />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(106deg, #403294 0%, #4a3da2 58%, #20abc0 100%);
}
.copy {
  @apply relative z-[2];
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 74%);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 12%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-0.5 text-[12.5px];
  color: rgb(255 255 255 / 82%);
}
.targetArt {
  @apply absolute -right-2 top-1/2 h-[230px] w-[450px] -translate-y-1/2;
  opacity: 0.46;
}
.targetArt i {
  @apply absolute left-1/2 top-1/2 rounded-full border-2;
  border-color: rgb(255 255 255 / 65%);
  transform: translate(-50%, -50%);
}
.targetArt i:nth-child(1) {
  @apply h-[210px] w-[210px];
}
.targetArt i:nth-child(2) {
  @apply h-[146px] w-[146px];
}
.targetArt i:nth-child(3) {
  @apply h-[82px] w-[82px];
}
.targetArt b {
  @apply absolute left-1/2 top-1/2 h-5 w-5 rounded-full bg-white;
  transform: translate(-50%, -50%);
}
.targetArt span {
  @apply absolute left-1/2 top-1/2 h-px w-[230px];
  background: repeating-linear-gradient(90deg, rgb(255 255 255 / 65%) 0 9px, transparent 9px 15px);
  transform-origin: left;
}
.targetArt span:nth-of-type(1) {
  transform: rotate(28deg);
}
.targetArt span:nth-of-type(2) {
  transform: rotate(152deg);
}
.targetArt span:nth-of-type(3) {
  transform: rotate(208deg);
}
.targetArt span:nth-of-type(4) {
  transform: rotate(332deg);
}
</style>
