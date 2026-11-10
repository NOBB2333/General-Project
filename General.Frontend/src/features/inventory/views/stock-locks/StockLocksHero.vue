<script setup lang="ts">
import { ChevronRight, KeyRound, LockKeyhole } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.smallLocks"
      aria-hidden="true"
    >
      <i
        v-for="index in 4"
        :key="index"
        ><b
      /></i>
    </div>
    <div
      :class="$style.centralLock"
      aria-hidden="true"
    >
      <span
        ><LockKeyhole
          :size="60"
          :stroke-width="1.5"
      /></span>
    </div>
    <div
      :class="$style.key"
      aria-hidden="true"
    >
      <KeyRound
        :size="104"
        :stroke-width="2.4"
      />
    </div>
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>库存管理</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>库存锁定</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><LockKeyhole
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>库存锁定管理</h1>
          <p>管理订单/项目锁定库存，共 12 条活跃锁定</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(103deg, #10265f 0%, #184698 52%, #4c37a8 100%);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.72);
}
.breadcrumb strong,
.heading h1 {
  color: white;
}
.heading {
  @apply mt-4 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.3);
}
.heading h1 {
  @apply m-0 text-[21px] font-bold leading-8;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.86);
}
.smallLocks {
  @apply absolute inset-y-0 left-[15%] w-[24%];
}
.smallLocks i {
  @apply absolute h-10 w-14 rounded bg-white/10;
}
.smallLocks i:nth-child(1) {
  @apply left-8 top-0;
}
.smallLocks i:nth-child(2) {
  @apply left-0 top-10;
}
.smallLocks i:nth-child(3) {
  @apply left-[150px] top-1;
}
.smallLocks i:nth-child(4) {
  @apply left-[190px] top-[90px];
}
.smallLocks b {
  @apply absolute left-1/2 top-1/2 h-2 w-2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-[#C5A84E];
}
.smallLocks b::before {
  @apply absolute right-1/2 top-1/2 h-px w-8 bg-[#C5A84E] content-[''];
}
.centralLock {
  @apply absolute -top-9 left-[45%] flex h-[190px] w-[190px] items-center justify-center rounded-full border-2 border-white/25;
  box-shadow:
    inset 0 0 0 10px rgba(255, 255, 255, 0.08),
    inset 0 0 0 22px rgba(255, 255, 255, 0.06);
}
.centralLock span {
  @apply flex h-[92px] w-[88px] items-center justify-center rounded-xl border-2 border-[#FFD15C] text-[#203A7F];
  background: linear-gradient(#f6c05a, #d98b2b);
  box-shadow: inset 8px 0 rgba(255, 255, 255, 0.18);
}
.key {
  @apply absolute right-[9%] top-[12px] text-[#FFD94E];
  transform: rotate(45deg);
}
</style>
