<script setup lang="ts">
import { ReceiptText } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p>财务管理 <span>›</span> 费用中心</p>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><ReceiptText
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>费用中心</h1>
          <small>管理员工报销申请与企业费用支出</small>
        </div>
      </div>
    </div>
    <div
      :class="$style.receipt"
      aria-hidden="true"
    >
      <i /><i /><i /><i />
    </div>
    <div
      :class="$style.donut"
      aria-hidden="true"
    >
      <strong>本月报销<br /><b>¥124.5K</b></strong>
    </div>
    <div
      :class="$style.legend"
      aria-hidden="true"
    >
      <span>差旅费 <b>¥48.2K</b></span
      ><span>招待费 <b>¥35.6K</b></span
      ><span>办公费 <b>¥22.1K</b></span
      ><span>交通费 <b>¥18.6K</b></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(104deg, #a80f58 0%, #df2c70 46%, #ff7752 77%, #ffbd48 110%);
}
.copy {
  @apply relative z-10;
}
.copy > p {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 76%);
}
.copy > p span {
  @apply px-1;
}
.identity {
  @apply mt-3 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 13%);
}
.identity h1 {
  @apply m-0 text-[22px] font-semibold;
}
.identity small {
  @apply mt-1 block text-[12px];
  color: rgb(255 255 255 / 84%);
}
.receipt {
  @apply absolute bottom-0 left-[13%] h-[110px] w-[190px] rounded-t bg-white/70 px-8 pt-4;
  transform: rotate(-1deg);
}
.receipt i {
  @apply mb-3 block h-1 rounded;
  background: #9c6b84;
}
.receipt i:first-child {
  @apply w-20;
  background: #74425f;
}
.donut {
  @apply absolute -top-[24px] left-[45%] flex h-[145px] w-[145px] items-center justify-center rounded-full border-[22px];
  border-color: rgb(255 255 255 / 16%) rgb(255 199 73 / 58%) rgb(255 255 255 / 14%) rgb(255 255 255 / 20%);
}
.donut strong {
  @apply text-center text-[10px];
}
.donut b {
  @apply text-[17px];
  color: #ffe380;
}
.legend {
  @apply absolute right-[7%] top-2 flex w-[240px] flex-col gap-2;
}
.legend span {
  @apply flex justify-between rounded px-3 py-1.5 text-[10px] font-semibold;
  background: rgb(255 255 255 / 14%);
}
.legend b {
  color: #ffe380;
}
</style>
