<script setup lang="ts">
import { Gavel } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>招投标</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"><Gavel :size="22" /></span>
        <div>
          <h1>招投标管理</h1>
          <p>招投标项目全生命周期管理：投标、保证金、结果记录</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.document"><i /><i /><i /><i /><b /></span>
      <span :class="$style.scale">
        <i :class="$style.scaleTop" />
        <i :class="$style.scalePost" />
        <i :class="$style.scaleLeft" />
        <i :class="$style.scaleRight" />
      </span>
      <span :class="$style.gavel"><i /><b /></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(104deg, #201547 0%, #28245d 30%, #3f3357 61%, #72451f 100%);
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
  @apply flex h-[38px] w-[38px] shrink-0 items-center justify-center rounded border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 11%);
}
.titleRow h1 {
  @apply text-[22px] font-semibold;
}
.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 76%);
}
.scene {
  @apply pointer-events-none absolute inset-y-0 right-0 w-[88%] overflow-hidden;
}
.document {
  @apply absolute left-[8%] top-[-24px] h-[174px] w-[180px] rotate-[-2deg] border px-5 py-8;
  background: #cbc8c7;
  border-color: rgb(255 255 255 / 25%);
}
.document i {
  @apply mb-3 block h-0.5;
  width: 84%;
  background: #485070;
}
.document i:nth-child(2) {
  width: 62%;
}
.document i:nth-child(4) {
  width: 70%;
}
.document b {
  @apply absolute bottom-[-4px] left-1/2 h-12 w-12 -translate-x-1/2 rounded-full border-4;
  border-color: #a94f46;
}
.scale {
  @apply absolute left-[56%] top-7 h-24 w-32;
  color: #d9d5b3;
}
.scaleTop,
.scalePost,
.scaleLeft,
.scaleRight {
  @apply absolute block;
  background: currentColor;
}
.scaleTop {
  @apply left-0 top-1 h-0.5 w-full;
}
.scalePost {
  @apply left-1/2 top-0 h-[82px] w-0.5 -translate-x-1/2;
}
.scaleLeft,
.scaleRight {
  @apply top-1 h-10 w-0.5;
}
.scaleLeft {
  left: 16px;
}
.scaleRight {
  right: 16px;
}
.scaleLeft::after,
.scaleRight::after {
  @apply absolute -left-4 bottom-0 h-3 w-8 rounded-b-full;
  content: '';
  background: #d1c776;
}
.gavel {
  @apply absolute right-[11%] top-[72px] h-3 w-28 rotate-[9deg] rounded;
  background: #955407;
}
.gavel i {
  @apply absolute -left-3 -top-2 h-7 w-12 rounded;
  background: #ad6108;
}
.gavel b {
  @apply absolute -right-9 top-8 h-2 w-20 rounded;
  background: #7c470c;
}
</style>
