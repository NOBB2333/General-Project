<script setup lang="ts">
import { ShieldCheck } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 授信额度管理</p>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <ShieldCheck
            :size="24"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>授信额度管理</h1>
          <p>统一管理客户授信额度、监控使用情况与超期风险</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.gauge"
      aria-hidden="true"
    >
      <i
        v-for="index in 7"
        :key="index"
      />
      <strong>信用评分</strong>
      <span />
    </div>
    <div
      :class="$style.vip"
      aria-hidden="true"
    >
      <b>★</b>
      <small>VIP 客户</small>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(108deg, #0c3f99 0%, #075cc2 38%, #5144b5 72%, #00a9c9 112%);
}
.copy {
  @apply relative z-10;
}
.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 76%);
}
.breadcrumb span {
  @apply px-1;
}
.identity {
  @apply mt-3 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 14%);
}
.identity h1,
.identity p {
  @apply m-0;
}
.identity h1 {
  @apply text-[23px] font-semibold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}
.gauge {
  @apply absolute left-[43%] top-[-64px] h-[190px] w-[285px] rounded-t-full border-[12px];
  border-color: #78bd83 #f6c453 #ff7452 #78bd83;
  border-bottom-color: transparent;
}
.gauge i {
  @apply absolute top-[72px] h-7 w-px;
  left: calc(12% + var(--gauge-offset, 0%));
  background: rgb(255 255 255 / 52%);
  transform-origin: 132px 0;
}
.gauge i:nth-child(1) {
  transform: rotate(-55deg);
}
.gauge i:nth-child(2) {
  transform: rotate(-36deg);
}
.gauge i:nth-child(3) {
  transform: rotate(-18deg);
}
.gauge i:nth-child(4) {
  transform: rotate(0deg);
}
.gauge i:nth-child(5) {
  transform: rotate(18deg);
}
.gauge i:nth-child(6) {
  transform: rotate(36deg);
}
.gauge i:nth-child(7) {
  transform: rotate(55deg);
}
.gauge strong {
  @apply absolute bottom-5 left-0 right-0 text-center text-[13px];
  color: rgb(255 255 255 / 65%);
}
.gauge span {
  @apply absolute bottom-[-7px] left-1/2 h-[82px] w-1 -translate-x-1/2;
  background: rgb(255 255 255 / 85%);
  transform-origin: bottom;
  transform: rotate(0deg);
}
.vip {
  @apply absolute right-[11%] top-0 flex flex-col items-center;
}
.vip b {
  @apply text-[58px] leading-none;
  color: #ffe380;
}
.vip small {
  @apply mt-5 text-[10px] font-semibold;
  color: rgb(255 255 255 / 78%);
}
</style>
