<script setup lang="ts">
import type { Component } from 'vue';
import { ChevronRight } from 'lucide-vue-next';

defineProps<{
  title: string;
  description: string;
  icon: Component;
  variant: 'overview' | 'customer' | 'archive' | 'review' | 'change' | 'distribution' | 'association';
}>();
</script>

<template>
  <header
    :class="$style.hero"
    :data-variant="variant"
  >
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>图纸管理</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>{{ title }}</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.icon">
          <component
            :is="icon"
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>{{ title }}</h1>
          <p>{{ description }}</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.art"
      aria-hidden="true"
    >
      <i
        v-for="index in 7"
        :key="index"
      />
      <b /><b /><b />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[120px] overflow-hidden rounded-lg text-white;
  background: linear-gradient(112deg, #074394 0%, #0955b8 62%, #2b65dc 100%);
}
.hero[data-variant='customer'] {
  background: linear-gradient(112deg, #c82500 0%, #e1320b 62%, #ff6b55 100%);
}
.hero[data-variant='archive'] {
  background: linear-gradient(112deg, #1d337e 0%, #0d4ca7 62%, #2b65dc 100%);
}
.hero[data-variant='review'] {
  background: linear-gradient(112deg, #c66800 0%, #e77b00 62%, #ff981d 100%);
}
.hero[data-variant='change'] {
  background: linear-gradient(112deg, #49309e 0%, #5140ad 62%, #806bd1 100%);
}
.hero[data-variant='distribution'] {
  background: linear-gradient(112deg, #155b78 0%, #03a6c5 62%, #62d8e7 100%);
}
.hero[data-variant='association'] {
  background: linear-gradient(112deg, #263378 0%, #4c3daa 62%, #7d69d1 100%);
}
.copy {
  @apply relative z-10 px-6 py-5;
}
.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgb(255 255 255 / 76%);
}
.breadcrumb strong {
  color: white;
}
.titleRow {
  @apply mt-4 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 14%);
}
.titleRow h1 {
  @apply m-0 text-[22px] font-bold leading-7;
}
.titleRow p {
  @apply m-0 mt-0.5 text-[12px];
  color: rgb(255 255 255 / 84%);
}
.art {
  @apply absolute inset-y-0 right-0 w-[39%] overflow-hidden;
  background: rgb(255 255 255 / 7%);
}
.art::before {
  content: '';
  @apply absolute -right-6 -top-20 h-[250px] w-[250px] rounded-full;
  background: rgb(255 255 255 / 6%);
}
.art i {
  @apply absolute bottom-0 block w-4 rounded-t-sm;
  background: rgb(255 255 255 / 38%);
}
.art i:nth-child(1) {
  left: 80px;
  height: 20px;
}
.art i:nth-child(2) {
  left: 120px;
  height: 50px;
}
.art i:nth-child(3) {
  left: 160px;
  height: 75px;
}
.art i:nth-child(4) {
  left: 200px;
  height: 104px;
}
.art i:nth-child(5) {
  left: 240px;
  height: 64px;
}
.art i:nth-child(6) {
  left: 280px;
  height: 92px;
}
.art i:nth-child(7) {
  left: 320px;
  height: 116px;
}
.art b {
  @apply absolute block border-2 border-white/80;
}
.art b:nth-of-type(1) {
  @apply left-[52%] top-8 h-14 w-12 rounded bg-white/10;
}
.art b:nth-of-type(2) {
  @apply left-[66%] top-12 h-12 w-10 rounded bg-white/10;
}
.art b:nth-of-type(3) {
  @apply left-[80%] top-6 h-16 w-12 rounded bg-white/10;
}
.hero[data-variant='overview'] .art::after {
  content: '';
  @apply absolute bottom-5 left-16 h-16 w-[360px] border-b-2 border-white;
  transform: skewY(-15deg);
}
.hero:not([data-variant='overview']) .art i {
  @apply hidden;
}
.hero[data-variant='association'] .art b,
.hero[data-variant='distribution'] .art b {
  @apply rounded-full;
}
.hero[data-variant='archive'] .art b {
  @apply top-9 h-20 w-12 rounded-t bg-white/70;
  border-color: rgb(255 255 255 / 70%);
}
.hero[data-variant='customer'] .art b {
  @apply top-8 h-24 w-28 rounded-t-lg bg-white/90;
  border-color: rgb(255 255 255 / 70%);
}
@media (max-width: 900px) {
  .art {
    @apply hidden;
  }
  .titleRow p {
    @apply max-w-[70vw] truncate;
  }
}
</style>
