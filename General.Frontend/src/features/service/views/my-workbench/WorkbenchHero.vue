<script setup lang="ts">
import { BriefcaseBusiness, CalendarClock, ChevronRight } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

const emit = defineEmits<{
  navigate: [route: string];
}>();
</script>

<template>
  <header :class="$style.hero">
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
        <strong>我的工作台</strong>
      </nav>

      <div :class="$style.titleBar">
        <div :class="$style.identity">
          <span :class="$style.heroIcon">
            <BriefcaseBusiness
              :size="22"
              :stroke-width="1.8"
            />
          </span>
          <div>
            <h1>我的工作台</h1>
            <p>工程师工作台</p>
          </div>
        </div>
        <div :class="$style.actions">
          <UiButton
            :class="$style.queueAction"
            size="sm"
            @click="emit('navigate', '/service/my-queue')"
          >
            去「待我处理」
          </UiButton>
          <UiButton
            :class="$style.calendarAction"
            size="sm"
            @click="emit('navigate', '/service/dispatch-calendar')"
          >
            <template #leading>
              <CalendarClock
                :size="13"
                :stroke-width="1.8"
              />
            </template>
            派工日历
          </UiButton>
        </div>
      </div>
    </div>

    <div
      :class="$style.artwork"
      aria-hidden="true"
    >
      <span :class="$style.artworkFrame">
        <i />
        <i />
        <i />
        <i />
        <i />
        <i />
        <i />
      </span>
      <span :class="$style.artworkCurve" />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(135deg, #1b2a6b 0%, #0747a6 45%, #00a3bf 100%);
}

.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: linear-gradient(90deg, rgb(11 31 61 / 62%) 0%, rgb(11 31 61 / 18%) 55%, transparent 100%);
}

.heroContent {
  @apply relative z-[2] px-6 py-5;
}

.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium text-white/75;
}

.breadcrumb strong {
  @apply font-semibold text-white;
}

.titleBar {
  @apply flex items-center justify-between gap-4;
}

.identity {
  @apply flex min-w-0 items-center gap-3;
}

.heroIcon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-white/20 bg-white/15 text-white;
}

.identity h1 {
  @apply text-[22px] font-bold text-white;
}

.identity p {
  @apply mt-1 text-[12.5px] text-white/85;
}

.actions {
  @apply flex shrink-0 items-center gap-2;
}

.queueAction {
  @apply border border-white/30 bg-white/15 text-white;
  backdrop-filter: blur(4px);
}

.queueAction:hover {
  @apply bg-white/25;
}

.calendarAction {
  @apply border-white bg-white font-semibold;
  color: #0747a6;
}

.calendarAction:hover {
  background: #f4f8ff;
}

.artwork {
  @apply pointer-events-none absolute inset-y-0 right-[14%] z-[1] w-[210px];
}

.artworkFrame {
  @apply absolute right-0 top-[-28px] grid h-[150px] w-[150px] grid-cols-2 gap-8 rounded-[14px] border-[3px] border-white/50 p-5;
}

.artworkFrame i {
  @apply block h-[34px] w-[34px] rounded bg-white/85;
}

.artworkFrame i:nth-child(n + 4) {
  @apply h-[14px] w-[14px] rounded-none;
}

.artworkFrame i:nth-child(4) {
  @apply ml-[72px] mt-[-2px];
}

.artworkFrame i:nth-child(5) {
  @apply ml-[48px] mt-[-2px];
}

.artworkFrame i:nth-child(6) {
  @apply ml-[72px] mt-[-48px];
}

.artworkFrame i:nth-child(7) {
  @apply ml-[48px] mt-[-48px];
}

.artworkCurve {
  @apply absolute right-[-52px] top-[82px] h-[100px] w-[270px] rounded-[50%] border-t-[3px];
  border-color: rgb(127 227 240 / 55%);
  transform: rotate(-12deg);
}

@media (max-width: 760px) {
  .actions {
    @apply hidden;
  }

  .artwork {
    @apply right-[-60px] opacity-50;
  }
}
</style>
