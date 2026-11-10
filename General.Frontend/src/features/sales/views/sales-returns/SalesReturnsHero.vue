<script setup lang="ts">
import { Package, RotateCcw } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>销售业务</span><b aria-hidden="true">›</b><strong>销售退货</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <RotateCcw
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>销售退货</h1>
          <p>客户退货申请审核、退款处理、责任原因归集</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.orbit" />
      <RotateCcw
        :class="$style.returnArrow"
        :size="118"
        :stroke-width="1.4"
      />
      <Package
        :class="$style.package"
        :size="58"
        :stroke-width="1.35"
      />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-4 text-white;
  background: linear-gradient(108deg, #c35c00 0%, #e77700 45%, #ffab00 100%);
}

.copy {
  @apply relative z-[2];
}

.breadcrumb {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 76%);
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
  @apply m-0 text-[23px] font-semibold leading-7;
  letter-spacing: 0;
}

.identity p {
  @apply m-0 mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}

.scene {
  @apply absolute inset-y-0 right-14 w-[410px];
  color: rgb(255 255 255 / 88%);
}

.orbit {
  @apply absolute left-16 top-1/2 h-[155px] w-[270px] -translate-y-1/2 rounded-[50%] border border-dashed;
  border-color: rgb(255 255 255 / 30%);
}

.returnArrow {
  @apply absolute left-[100px] top-2;
}

.package {
  @apply absolute right-[104px] top-[37px];
  fill: rgb(255 255 255 / 86%);
  color: rgb(255 255 255 / 95%);
}
</style>
