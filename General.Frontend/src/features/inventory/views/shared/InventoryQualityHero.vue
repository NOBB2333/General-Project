<script setup lang="ts">
import { Check, ChevronRight, Search, X } from 'lucide-vue-next';

defineProps<{ title: string; description: string }>();
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.rejectMark"
      aria-hidden="true"
    >
      <X :size="42" />
    </div>
    <div
      :class="$style.document"
      aria-hidden="true"
    >
      <i
        v-for="index in 5"
        :key="index"
      />
    </div>
    <div
      :class="$style.inspection"
      aria-hidden="true"
    >
      <Search :size="56" />
    </div>
    <div
      :class="$style.approved"
      aria-hidden="true"
    >
      <Check :size="54" />
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
  background: linear-gradient(105deg, #0757a7 0%, #008f7a 55%, #4a9fe6 100%);
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

.rejectMark,
.approved {
  @apply absolute top-[38px] flex h-[86px] w-[86px] items-center justify-center rounded-full border-2 border-white/70 text-white/80;
}

.rejectMark {
  @apply left-[16%];
  background: rgba(184, 56, 120, 0.7);
}

.approved {
  @apply right-[17%];
  background: rgba(0, 181, 139, 0.75);
}

.document {
  @apply absolute -top-5 left-[36%] h-[150px] w-[180px] rotate-[-4deg] border border-white/60 bg-white/65 p-7;
}

.document i {
  @apply mt-3 block h-0.5 bg-[#1D4E89]/70;
}

.inspection {
  @apply absolute bottom-0 left-[53%] flex h-[84px] w-[120px] items-center justify-center border border-white/60 text-white/70;
  background: rgba(73, 183, 222, 0.45);
}
</style>
