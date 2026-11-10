<script setup lang="ts">
import { ChevronRight, UsersRound } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.largeGear"
      aria-hidden="true"
    />
    <div
      :class="$style.smallGear"
      aria-hidden="true"
    />

    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>生产制造</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <span>工序派工</span>
      </nav>

      <div :class="$style.headingRow">
        <span
          :class="$style.icon"
          aria-hidden="true"
        >
          <UsersRound
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div :class="$style.headingCopy">
          <h1>工序派工</h1>
          <p>将已下达工单的工序分派到工作中心成员/班组，按交期优先排程</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 h-[118.75px] shrink-0 overflow-hidden rounded-2xl;
  background: linear-gradient(120deg, #102954 0%, #0b4da9 54%, #6554c0 100%);
}

.largeGear,
.smallGear {
  @apply absolute rounded-full border opacity-15;
  border-color: white;
}

.largeGear {
  right: 278px;
  top: -45px;
  width: 145px;
  height: 145px;
  border-width: 2px;
  box-shadow:
    0 -18px 0 -15px white,
    0 18px 0 -15px white,
    18px 0 0 -15px white,
    -18px 0 0 -15px white;
}

.smallGear {
  right: 178px;
  bottom: -46px;
  width: 92px;
  height: 92px;
  border-width: 2px;
  box-shadow:
    0 -15px 0 -12px white,
    15px 0 0 -12px white,
    -15px 0 0 -12px white;
}

.largeGear::after,
.smallGear::after {
  @apply absolute rounded-full border;
  content: '';
  inset: 35%;
  border-color: rgba(255, 255, 255, 0.7);
}

.content {
  @apply relative px-6 py-5;
}

.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.78);
}

.headingRow {
  @apply mt-4 flex items-center gap-3;
}

.icon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.13);
  border-color: rgba(255, 255, 255, 0.3);
}

.headingCopy {
  @apply min-w-0;
}

.headingCopy h1 {
  @apply m-0 text-[21px] font-bold leading-[33px];
  color: white;
}

.headingCopy p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}
</style>
