<script setup lang="ts">
import { NotebookTabs } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 汇票管理</p>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <NotebookTabs
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>汇票管理</h1>
          <p>承兑汇票全生命周期：登记入库 → 背书/贴现/解付 → 归档</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.noteStack"
      aria-hidden="true"
    >
      <span>银行承兑汇票</span>
      <strong>¥ 580,000</strong>
      <small>承兑</small>
    </div>
    <div
      :class="$style.hourglass"
      aria-hidden="true"
    >
      <i />
      <b />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-6 py-4 text-white;
  background: linear-gradient(108deg, #172b75 0%, #1645a5 42%, #d63a16 76%, #ff991f 100%);
}

.copy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 78%);
}

.breadcrumb span {
  @apply px-1;
}

.identity {
  @apply mt-4 flex items-center gap-3;
}

.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 24%);
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

.noteStack {
  @apply absolute bottom-[-34px] left-[8%] flex h-[76px] w-[150px] rotate-[-2deg] flex-col rounded border px-4 py-3;
  color: #0c3d96;
  border-color: rgb(255 255 255 / 45%);
  background: rgb(255 255 255 / 68%);
  box-shadow:
    -14px 8px 0 rgb(255 255 255 / 22%),
    18px 7px 0 rgb(255 255 255 / 18%);
}

.noteStack span,
.noteStack small {
  @apply text-[8px] font-semibold;
}

.noteStack strong {
  @apply mt-1 text-[12px];
}

.noteStack small {
  @apply self-end;
}

.hourglass {
  @apply absolute inset-y-0 right-[18%] w-[106px] border-x-2;
  border-color: rgb(255 255 255 / 45%);
}

.hourglass::before,
.hourglass::after {
  @apply absolute left-[-5px] h-[5px] w-[114px] rounded;
  content: '';
  background: rgb(255 255 255 / 55%);
}

.hourglass::before {
  @apply top-0;
}

.hourglass::after {
  @apply bottom-0;
}

.hourglass i,
.hourglass b {
  @apply absolute left-1/2 h-0 w-0 -translate-x-1/2;
  border-left: 41px solid transparent;
  border-right: 41px solid transparent;
}

.hourglass i {
  @apply top-[10px];
  border-top: 52px solid #ffd866;
}

.hourglass b {
  @apply bottom-[10px];
  border-bottom: 34px solid #ffd866;
}
</style>
