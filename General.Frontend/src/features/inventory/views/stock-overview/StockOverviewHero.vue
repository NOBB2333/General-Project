<script setup lang="ts">
import { Boxes, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.shelves"
      aria-hidden="true"
    >
      <i
        v-for="index in 18"
        :key="index"
      />
    </div>
    <div
      :class="$style.crate"
      aria-hidden="true"
    >
      <i /><i /><i />
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
        /><strong>库存总览</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><Boxes
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>库存总览</h1>
          <p>商品库存、在途、分类汇总与仓库视图</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(110deg, #102b68 0%, #0d4a93 52%, #10a6c0 100%);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}
.breadcrumb strong {
  color: white;
}
.heading {
  @apply mt-4 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.3);
}
.heading h1 {
  @apply m-0 text-[21px] font-bold leading-8 text-white;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}
.shelves {
  @apply absolute -right-3 -top-7 grid h-[170px] w-[390px] grid-cols-6 gap-2 border-l border-white/30 p-3 opacity-90;
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.24) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.24) 1px, transparent 1px);
  background-size: 65px 55px;
}
.shelves i {
  @apply m-1 rounded-sm;
  background: linear-gradient(#50d4df, #1599bf);
}
.shelves i:nth-child(4n + 2) {
  background: linear-gradient(#ffc35a, #ff8b00);
}
.shelves i:nth-child(5n) {
  background: linear-gradient(#83dedb, #36b3b8);
}
.crate {
  @apply absolute bottom-0 left-[45%] flex items-end gap-1;
}
.crate i {
  @apply block w-10 border border-[#7A521D] bg-[#C9AC68];
}
.crate i:nth-child(1) {
  height: 24px;
}
.crate i:nth-child(2) {
  height: 56px;
  clip-path: polygon(18% 0, 82% 0, 100% 100%, 0 100%);
}
.crate i:nth-child(3) {
  height: 40px;
  background: #39aacb;
}
</style>
