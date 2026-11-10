<script setup lang="ts">
import { ChevronRight, ClipboardList } from 'lucide-vue-next';

const flowRows = [
  { label: '入库', value: '+128' },
  { label: '出库', value: '-64' },
  { label: '入库', value: '+96' },
  { label: '出库', value: '-32' },
] as const;
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.flowBoard"
      aria-hidden="true"
    >
      <div
        v-for="row in flowRows"
        :key="`${row.label}-${row.value}`"
        :data-direction="row.label"
      >
        <i /><span>{{ row.label }}</span
        ><strong>{{ row.value }}</strong
        ><i />
      </div>
    </div>
    <div
      :class="$style.routeMarks"
      aria-hidden="true"
    >
      <i
        v-for="index in 3"
        :key="index"
      /><b>出&nbsp;←</b>
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
        /><strong>库存流水</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><ClipboardList
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>库存流水</h1>
          <p>查看所有出入库流水记录与数据报表</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(104deg, #10225e 0%, #0f2868 35%, #076b84 70%, #00875a 100%);
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
  border-color: rgba(255, 255, 255, 0.28);
}
.heading h1 {
  @apply m-0 text-[21px] font-bold leading-8;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.86);
}
.flowBoard {
  @apply absolute -top-2 left-[35%] w-[30%] border-x border-white/35 bg-white/70 px-1;
}
.flowBoard > div {
  @apply my-1 grid h-[24px] grid-cols-[1fr_42px_54px_1fr] items-center gap-3 px-3 text-[10px];
  background: rgba(0, 135, 90, 0.13);
}
.flowBoard > div[data-direction='出库'] {
  background: rgba(222, 53, 11, 0.12);
}
.flowBoard i {
  @apply h-3 rounded-sm;
  background: rgba(23, 43, 77, 0.12);
}
.flowBoard span {
  @apply rounded border px-1 py-0.5 text-center font-semibold;
  color: #00875a;
  border-color: #00875a;
}
.flowBoard div[data-direction='出库'] span,
.flowBoard div[data-direction='出库'] strong {
  color: #de350b;
  border-color: #de350b;
}
.flowBoard strong {
  color: #00875a;
}
.routeMarks {
  @apply absolute right-[3%] top-[34px] flex items-center gap-16;
}
.routeMarks i {
  @apply h-5 w-5 rounded-sm bg-[#E57D5A];
}
.routeMarks b {
  @apply flex h-12 w-14 items-center justify-center rounded-lg border-2 border-white/70 text-[12px] text-white;
  background: #dc7958;
}
</style>
