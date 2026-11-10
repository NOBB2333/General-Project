<script setup lang="ts">
import { ArrowUpFromLine, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.routeLine"
      aria-hidden="true"
    >
      →
    </div>
    <div
      :class="$style.conveyor"
      aria-hidden="true"
    >
      <i
        v-for="index in 18"
        :key="index"
      />
    </div>
    <div
      :class="$style.boxes"
      aria-hidden="true"
    >
      <i />
      <i />
      <i />
    </div>
    <div
      :class="$style.truck"
      aria-hidden="true"
    >
      <i />
      <b />
    </div>
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>库存管理</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>出库管理</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon">
          <ArrowUpFromLine
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>出库管理</h1>
          <p>管理所有出库单和待出库发货单</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(105deg, #162f78 0%, #453687 55%, #18a6c8 100%);
}

.content {
  @apply relative px-6 py-5;
}

.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
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
  border-color: rgba(255, 255, 255, 0.3);
}

.heading h1 {
  @apply m-0 text-[21px] font-bold leading-8;
}

.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}

.routeLine {
  @apply absolute left-[16%] right-[15%] top-0 h-5 border-t-2 border-dashed text-right text-[32px] leading-3;
  color: #70c4e7;
  border-color: #70c4e7;
}

.conveyor {
  @apply absolute bottom-0 left-[4%] right-[2%] flex h-7 items-end border-t-2 border-white/40;
}

.conveyor i {
  @apply block h-7 flex-1 border-r border-white/30;
}

.boxes {
  @apply absolute bottom-7 left-[31%] right-[45%] flex items-end justify-between;
}

.boxes i {
  @apply block h-11 w-14 rounded-t border border-white/60 bg-[#B54386];
}

.boxes i:nth-child(2) {
  @apply h-10 w-12 bg-[#E6A73B];
}

.boxes i:nth-child(3) {
  @apply h-8 w-12 bg-[#B54386];
}

.truck {
  @apply absolute bottom-7 right-[2%] h-[68px] w-[210px] border border-white/70 bg-[#102B68];
}

.truck i {
  @apply absolute -right-[70px] bottom-0 h-12 w-[70px] bg-[#5DB5CF];
  clip-path: polygon(0 0, 67% 0, 100% 45%, 100% 100%, 0 100%);
}

.truck b {
  @apply absolute -bottom-10 -right-[45px] h-8 w-8 rounded-full border-[7px] border-[#D8EFF5] bg-[#335A83];
}
</style>
