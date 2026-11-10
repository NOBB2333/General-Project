<script setup lang="ts">
import { HandCoins } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 借款贷款管理</p>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><HandCoins
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>借款贷款管理</h1>
          <p>员工借款（公司→员工）与 银行贷款（银行→公司）统一管理</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.bank"
      aria-hidden="true"
    >
      <b>¥</b><i /><i /><i /><i /><i />
    </div>
    <div
      :class="$style.cashFlow"
      aria-hidden="true"
    >
      <span>¥</span><span>¥</span><span>¥</span><span>¥</span><span>¥</span>
    </div>
    <div
      :class="$style.contract"
      aria-hidden="true"
    >
      <i /><i /><i /><strong>¥ 1,200,000.00</strong>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(105deg, #09245b 0%, #0d4c9a 50%, #a88308 115%);
}

.copy {
  @apply relative z-10;
}
.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 72%);
}
.breadcrumb span {
  @apply px-1;
}
.identity {
  @apply mt-3 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 12%);
}
.identity h1,
.identity p {
  @apply m-0;
}
.identity h1 {
  @apply text-[22px] font-semibold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 82%);
}
.bank {
  @apply absolute bottom-0 left-[17%] h-[75px] w-[185px];
  background: rgb(255 255 255 / 18%);
  clip-path: polygon(50% 0, 100% 35%, 100% 47%, 95% 47%, 95% 100%, 5% 100%, 5% 47%, 0 47%, 0 35%);
}
.bank b {
  @apply absolute left-1/2 top-3 -translate-x-1/2 text-[15px];
  color: #c8aa45;
}
.bank i {
  @apply absolute bottom-0 h-[56px] w-[17px];
  background: rgb(255 255 255 / 30%);
}
.bank i:nth-of-type(1) {
  left: 18px;
}
.bank i:nth-of-type(2) {
  left: 55px;
}
.bank i:nth-of-type(3) {
  left: 92px;
}
.bank i:nth-of-type(4) {
  left: 129px;
}
.bank i:nth-of-type(5) {
  left: 166px;
}
.cashFlow {
  @apply absolute left-[32%] top-[47px] flex w-[42%] justify-between border-t-2 border-dashed pt-4;
  border-color: rgb(255 255 255 / 34%);
}
.cashFlow::after {
  @apply absolute -right-2 -top-2 h-0 w-0;
  content: '';
  border-bottom: 6px solid transparent;
  border-left: 10px solid rgb(255 255 255 / 70%);
  border-top: 6px solid transparent;
}
.cashFlow span {
  @apply flex h-[21px] w-[21px] items-center justify-center rounded-full text-[10px] font-semibold;
  color: #6d5810;
  background: #cbb15b;
}
.contract {
  @apply absolute -bottom-5 right-[14%] h-[114px] w-[168px] rounded-t bg-white/90 p-4;
  color: #173a75;
}
.contract i {
  @apply mb-2 block h-1 rounded;
  background: #8a93a6;
}
.contract i:first-child {
  @apply w-20;
  background: #273b6f;
}
.contract strong {
  @apply mt-4 block rounded px-2 py-2 text-[13px];
  background: #d8e5f7;
}
</style>
