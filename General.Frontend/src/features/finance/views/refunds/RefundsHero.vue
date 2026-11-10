<script setup lang="ts">
import { RotateCcw } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 退款申请</p>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <RotateCcw
            :size="24"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>退款申请</h1>
          <p>客户退款、供应商退款的申请、审批与核销追踪</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.artwork"
      aria-hidden="true"
    >
      <span :class="$style.refundMark"><b>¥</b><small>REFUND</small></span>
      <span :class="$style.flowLine" />
      <span :class="$style.company">
        <i
          v-for="index in 6"
          :key="index"
        />
        <small>公司</small>
      </span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(108deg, #5e35b1 0%, #6554c0 42%, #9985ea 68%, #00a9c9 112%);
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

.artwork {
  @apply absolute inset-y-0 right-0 w-[60%];
}

.flowLine {
  @apply absolute left-0 right-[15%] top-[57px] border-t-2 border-dashed;
  border-color: rgb(255 255 255 / 25%);
  transform: rotate(8deg);
}

.refundMark {
  @apply absolute left-[31%] top-[-23px] flex h-[138px] w-[138px] flex-col items-center justify-center rounded-full border-[3px];
  border-color: rgb(255 255 255 / 28%);
  box-shadow:
    0 0 0 32px rgb(255 255 255 / 6%),
    inset 0 0 0 2px rgb(255 255 255 / 12%);
}

.refundMark b {
  @apply text-[29px] font-semibold;
  color: #ffe380;
}

.refundMark small {
  @apply mt-2 text-[10px] font-semibold;
  color: rgb(255 255 255 / 54%);
}

.company {
  @apply absolute right-[9%] top-3 grid h-[63px] w-[75px] grid-cols-3 gap-1 rounded-lg p-3;
  background: rgb(255 255 255 / 18%);
}

.company i {
  @apply block h-4;
  background: #ffe380;
}

.company small {
  @apply absolute left-0 right-0 top-[87px] text-center text-[10px] font-semibold;
  color: rgb(255 255 255 / 80%);
}
</style>
