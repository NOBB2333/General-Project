<script setup lang="ts">
import { Box, Truck } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>销售业务</span><b aria-hidden="true">›</b><strong>销售发货单</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <Truck
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>销售发货单</h1>
          <p>发货出库登记、物流跟踪、签收回执与开票联动</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <Box
        :size="66"
        :stroke-width="1.25"
      />
      <i :class="$style.route" />
      <span :class="$style.loadingDock"><b /><b /><b /></span>
      <Truck
        :class="$style.sceneTruck"
        :size="72"
        :stroke-width="1.25"
      />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative flex min-h-[120px] items-center overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(108deg, #1b4f72 0%, #2060b5 54%, #4c9aff 100%);
}
.copy {
  @apply relative z-[2];
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 74%);
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
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-0.5 text-[12.5px];
  color: rgb(255 255 255 / 82%);
}
.scene {
  @apply absolute inset-y-0 right-12 w-[490px];
  color: rgb(255 255 255 / 82%);
}
.scene > svg:first-child {
  @apply absolute left-20 top-0;
}
.route {
  @apply absolute left-4 top-[73px] h-0.5 w-52;
  background: repeating-linear-gradient(90deg, rgb(255 255 255 / 58%) 0 18px, transparent 18px 28px);
}
.loadingDock {
  @apply absolute bottom-0 right-20 flex h-[59px] w-[160px] items-end justify-center rounded-t-lg;
  background: rgb(255 255 255 / 80%);
}
.loadingDock b {
  @apply h-12 w-px;
  margin: 0 18px;
  background: rgb(32 96 181 / 32%);
}
.sceneTruck {
  @apply absolute bottom-[-6px] right-0;
}
@media (max-width: 900px) {
  .scene {
    @apply -right-24 opacity-50;
  }
}
</style>
