<script setup lang="ts">
import { Calculator, FileText, JapaneseYen } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>销售报价</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"><FileText :size="22" /></span>
        <div>
          <h1>销售报价</h1>
          <p>报价单创建、审批与转订单全流程管理</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.currency"><JapaneseYen :size="32" /></span>
      <span :class="$style.document"><i /><i /><i /><i /></span>
      <span :class="$style.calculator">
        <Calculator :size="34" />
        <b>126,800</b>
        <em>+8.5%</em>
      </span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(135deg, #0747a6 0%, #5243aa 50%, #00875a 100%);
}
.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: linear-gradient(90deg, rgb(5 35 91 / 28%) 0%, transparent 58%);
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
  border-color: rgb(255 255 255 / 32%);
  background: rgb(255 255 255 / 12%);
}
.titleRow h1 {
  @apply text-[22px] font-semibold;
}
.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 78%);
}
.scene {
  @apply pointer-events-none absolute inset-y-0 right-0 w-[94%] overflow-hidden;
}
.currency {
  @apply absolute left-[6%] top-1/2 flex h-[72px] w-[72px] -translate-y-1/2 items-center justify-center rounded-full;
  color: #65566a;
  background: #c7bea0;
  box-shadow: 0 0 0 2px rgb(255 255 255 / 25%);
}
.document {
  @apply absolute left-[22%] top-[-26px] h-[170px] w-[270px] rotate-[-2deg] border px-6 py-9;
  background: #c5cee1;
  border-color: rgb(255 255 255 / 28%);
}
.document i {
  @apply mb-3 block h-1 rounded-full;
  width: 76%;
  background: #38548b;
}
.document i:nth-child(2) {
  width: 56%;
}
.document i:nth-child(3) {
  width: 82%;
}
.document i:nth-child(4) {
  width: 64%;
}
.calculator {
  @apply absolute right-[35%] top-[-20px] flex h-[182px] w-[150px] flex-col items-center border px-5 pt-3;
  color: #bfefff;
  background: #10366b;
  border-color: rgb(255 255 255 / 25%);
}
.calculator b {
  @apply mt-1 text-[18px];
}
.calculator em {
  @apply absolute right-[-78px] top-11 text-[12px] not-italic;
  color: rgb(255 255 255 / 52%);
}
@media (max-width: 900px) {
  .scene {
    @apply opacity-35;
  }
}
</style>
