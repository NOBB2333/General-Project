<script setup lang="ts">
import { CalendarDays, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.pattern"
      aria-hidden="true"
    />
    <div
      :class="$style.accentOne"
      aria-hidden="true"
    />
    <div
      :class="$style.accentTwo"
      aria-hidden="true"
    />
    <div
      :class="$style.accentThree"
      aria-hidden="true"
    />

    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>考勤假期</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <span>团队日历</span>
      </nav>

      <div :class="$style.headingRow">
        <span
          :class="$style.icon"
          aria-hidden="true"
        >
          <CalendarDays
            :size="21"
            :stroke-width="1.8"
          />
        </span>
        <div :class="$style.headingCopy">
          <h1>团队日历</h1>
          <p>查看团队请假/出差排期，识别人力冲突</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 h-[118.75px] shrink-0 overflow-hidden rounded-2xl;
  background: linear-gradient(135deg, #0b708f 0%, #0052cc 52%, #5243aa 100%);
}

.pattern {
  @apply absolute inset-0 opacity-60;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='76' height='49' viewBox='0 0 76 49'%3E%3Crect x='2' y='2' width='72' height='45' rx='5' fill='none' stroke='white' stroke-opacity='.13' stroke-width='1'/%3E%3C/svg%3E");
  background-size: 76px 49px;
}

.pattern::after {
  @apply absolute rounded-lg border-2;
  content: '';
  left: 75.4%;
  top: 16px;
  width: 76px;
  height: 49px;
  border-color: rgba(255, 255, 255, 0.72);
}

.accentOne,
.accentTwo,
.accentThree {
  @apply absolute h-3 rounded-full opacity-60;
  filter: saturate(0.85);
}

.accentOne {
  left: 8px;
  top: 59px;
  width: 236px;
  background: #ff7452;
}

.accentTwo {
  left: 265px;
  top: 59px;
  width: 152px;
  background: #36b37e;
}

.accentThree {
  left: 351px;
  top: 87px;
  width: 270px;
  background: #ff8b00;
}

.content {
  @apply relative px-6 py-5;
}

.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}

.headingRow {
  @apply mt-4 flex items-center gap-3;
}

.icon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.3);
  box-shadow: 0 4px 14px rgba(9, 30, 66, 0.16);
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

@media (prefers-reduced-motion: reduce) {
  .pattern {
    transform: none;
  }
}
</style>
