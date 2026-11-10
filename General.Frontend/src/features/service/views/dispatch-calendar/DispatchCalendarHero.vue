<script setup lang="ts">
import { CalendarClock, ChevronRight, UsersRound } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

const emit = defineEmits<{
  goDispatch: [];
}>();
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.calendarArtwork"
      aria-hidden="true"
    >
      <span :class="$style.artworkBinding" />
      <span
        v-for="index in 4"
        :key="index"
        :class="$style.artworkCell"
      />
    </div>

    <div :class="$style.heroContent">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>服务管理</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <span>派工日历</span>
      </nav>

      <div :class="$style.headingRow">
        <span
          :class="$style.heroIcon"
          aria-hidden="true"
        >
          <CalendarClock
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div :class="$style.headingCopy">
          <h1>派工日历</h1>
          <p>工程师 × 日期视图 · 一眼看清排程冲突、待预约和负载分布</p>
        </div>
      </div>
    </div>

    <UiButton
      :class="$style.dispatchButton"
      size="sm"
      variant="secondary"
      @click="emit('goDispatch')"
    >
      <template #leading>
        <UsersRound
          :size="15"
          :stroke-width="1.8"
        />
      </template>
      去派工台
    </UiButton>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 h-[118.75px] shrink-0 overflow-hidden rounded-2xl;
  background: linear-gradient(112deg, #10285f 0%, #064ca2 56%, #00aebf 100%);
}

.heroContent {
  @apply relative z-[1] px-6 py-5;
}

.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}

.headingRow {
  @apply mt-4 flex items-center gap-3;
}

.heroIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.28);
  box-shadow: 0 4px 14px rgba(9, 30, 66, 0.18);
}

.headingCopy {
  @apply min-w-0;
}

.headingCopy h1 {
  @apply m-0 text-[21px] font-bold leading-[30px];
  color: white;
}

.headingCopy p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}

.dispatchButton {
  @apply absolute right-6 top-[59px] z-[2];
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.38);
}

.dispatchButton:hover {
  background: rgba(255, 255, 255, 0.22);
}

.calendarArtwork {
  @apply absolute -top-8 grid h-[154px] w-[174px] grid-cols-2 gap-5 rounded-2xl border-[3px] p-7 opacity-60;
  right: 160px;
  border-color: rgba(255, 255, 255, 0.48);
  transform: rotate(0deg);
}

.calendarArtwork::after {
  @apply absolute -bottom-10 -left-24 h-20 w-56 rounded-[50%] border-t-2;
  content: '';
  border-color: rgba(255, 255, 255, 0.28);
  transform: rotate(-14deg);
}

.artworkBinding {
  @apply absolute -top-3 left-[30px] h-6 w-[3px] rounded-full;
  color: rgba(255, 255, 255, 0.72);
  background: currentColor;
  box-shadow: 104px 0 0 currentColor;
}

.artworkCell {
  @apply h-7 w-7 rounded bg-white/70;
}

@media (max-width: 760px) {
  .headingCopy p {
    @apply max-w-[360px] truncate;
  }

  .calendarArtwork {
    @apply hidden;
  }
}
</style>
