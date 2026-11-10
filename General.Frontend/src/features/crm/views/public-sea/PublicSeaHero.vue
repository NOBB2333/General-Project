<script setup lang="ts">
import { Anchor } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>公海客户</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"
          ><Anchor
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>公海客户</h1>
          <p>未分配或已释放的客户资源池，销售人员可自行领取</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.center"><i /><b /></span>
      <span :class="$style.lineOne" />
      <span :class="$style.lineTwo" />
      <span :class="[$style.profile, $style.profileOne]"><i /><b /><b /></span>
      <span :class="[$style.profile, $style.profileTwo]"><i /><b /><b /></span>
      <span :class="[$style.profile, $style.profileThree]"><i /><b /><b /></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[119px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(105deg, #0e245a 0%, #123b82 48%, #5243aa 100%);
}
.copy {
  @apply relative z-10;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 72%);
}
.breadcrumb strong {
  color: white;
}
.titleRow {
  @apply flex items-center gap-3;
}
.titleIcon {
  @apply flex h-[38.5px] w-[38.5px] shrink-0 items-center justify-center rounded border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 12%);
}
.titleRow h1 {
  @apply text-[22px] font-semibold;
}
.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 76%);
}
.scene {
  @apply pointer-events-none absolute inset-0 overflow-hidden;
}
.center {
  @apply absolute left-[51%] top-1/2 h-[104px] w-[104px] -translate-y-1/2 rounded-full border-2;
  border-color: rgb(255 255 255 / 56%);
  background: rgb(255 255 255 / 9%);
}
.center::after {
  @apply absolute -inset-3 rounded-full border border-dashed;
  content: '';
  border-color: rgb(255 255 255 / 28%);
}
.center i,
.center b {
  @apply absolute top-1/2 block h-0.5 w-9 -translate-y-1/2;
  background: #ffab00;
}
.center i {
  left: 15px;
  transform: rotate(22deg);
}
.center b {
  right: 15px;
  transform: rotate(-22deg);
}
.lineOne,
.lineTwo {
  @apply absolute left-[20%] h-px w-[70%] origin-center;
  top: 50%;
  background: rgb(255 255 255 / 65%);
}
.lineOne {
  transform: rotate(4deg);
}
.lineTwo {
  transform: rotate(-5deg);
}
.profile {
  @apply absolute flex h-12 w-[136px] items-center gap-2 rounded border px-3;
  border-color: rgb(255 255 255 / 55%);
  background: rgb(12 28 73 / 55%);
}
.profile i {
  @apply block h-7 w-7 shrink-0 rounded-full border-2;
  border-color: rgb(255 255 255 / 75%);
  background: #4c9aff;
}
.profile b {
  @apply absolute left-12 h-1 rounded-full;
  width: 72px;
  background: rgb(255 255 255 / 60%);
}
.profile b:last-child {
  @apply mt-4;
  width: 52px;
}
.profileOne {
  left: 23%;
  top: -5px;
}
.profileTwo {
  left: 23%;
  bottom: -21px;
}
.profileThree {
  right: 7%;
  top: -5px;
}
</style>
