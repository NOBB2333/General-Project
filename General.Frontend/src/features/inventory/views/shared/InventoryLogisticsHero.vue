<script setup lang="ts">
import { ChevronRight } from 'lucide-vue-next';

defineProps<{
  title: string;
  description: string;
}>();
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.truck"
      aria-hidden="true"
    >
      <i />
      <i />
      <b />
      <b />
    </div>
    <div
      :class="$style.arrow"
      aria-hidden="true"
    >
      →
    </div>
    <div
      :class="$style.boxes"
      aria-hidden="true"
    >
      <i
        v-for="index in 8"
        :key="index"
      />
    </div>
    <div
      :class="$style.shelf"
      aria-hidden="true"
    >
      <i
        v-for="index in 12"
        :key="index"
      />
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
        <strong>{{ title }}</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"><slot name="icon" /></span>
        <div>
          <h1>{{ title }}</h1>
          <p>{{ description }}</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(105deg, #102b68 0%, #104b8f 62%, #00875a 100%);
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

.truck {
  @apply absolute bottom-0 left-[23%] h-[86px] w-[170px] border border-white/50;
}

.truck i:first-child {
  @apply absolute bottom-0 left-0 h-full w-[100px] bg-[#0E2A59]/80;
}

.truck i:nth-child(2) {
  @apply absolute bottom-0 right-0 h-[68px] w-[70px] bg-[#55ABC3];
  clip-path: polygon(0 0, 72% 0, 100% 46%, 100% 100%, 0 100%);
}

.truck b {
  @apply absolute -bottom-3 h-6 w-6 rounded-full border-4 border-[#0E2A59] bg-[#7E93A9];
}

.truck b:nth-of-type(1) {
  left: 36px;
}

.truck b:nth-of-type(2) {
  right: 20px;
}

.arrow {
  @apply absolute left-[48%] top-[44px] text-[33px] text-white/80;
}

.boxes {
  @apply absolute bottom-0 left-[55%] flex items-end gap-4;
}

.boxes i {
  @apply block h-10 w-12 rounded-t bg-[#E0A23B];
}

.boxes i:nth-child(2n) {
  height: 27px;
}

.boxes i:nth-child(3n) {
  height: 45px;
}

.shelf {
  @apply absolute bottom-0 right-[6%] grid h-[108px] w-[250px] grid-cols-4 gap-2 border border-white/30 p-3;
  background: #151f63;
}

.shelf i {
  @apply rounded-sm;
  background: linear-gradient(#62bad1, #3d96b5);
}

.shelf i:nth-child(3n) {
  background: linear-gradient(#e5b757, #c47a18);
}
</style>
