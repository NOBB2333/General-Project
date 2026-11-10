<script setup lang="ts">
import { ChevronRight, ClipboardCheck, LayoutGrid } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';

defineProps<{
  assigneeLabel: string;
}>();

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
        <strong>待我处理</strong>
      </nav>

      <div :class="$style.titleRow">
        <span :class="$style.heroIcon">
          <ClipboardCheck
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>待我处理</h1>
          <p>工程师工作台 · {{ assigneeLabel }} · 接单 → 预约 → 到场 → 处理 → 提交确认</p>
        </div>
      </div>
    </div>

    <UiButton
      :class="$style.ordersButton"
      variant="ghost"
      size="sm"
      @click="emit('navigate', '/service/service-tickets')"
    >
      服务工单
    </UiButton>

    <div
      :class="$style.artwork"
      aria-hidden="true"
    >
      <span :class="$style.artworkShell">
        <LayoutGrid
          :size="58"
          :stroke-width="2.4"
        />
      </span>
      <span :class="$style.artworkCurve" />
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[118.75px] overflow-hidden rounded-2xl;
  background: linear-gradient(112deg, #10275e 0%, #07489b 54%, #00acbc 100%);
}

.heroContent {
  @apply relative z-[1] px-6 py-5;
}

.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}

.breadcrumb strong {
  color: white;
}

.titleRow {
  @apply mt-4 flex items-center gap-3;
}

.heroIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.28);
}

.titleRow h1 {
  @apply m-0 text-[21px] font-bold leading-[29px];
  color: white;
}

.titleRow p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}

.ordersButton {
  @apply absolute right-6 top-1/2 z-[2] border;
  color: white;
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.3);
  transform: translateY(-50%);
}

.ordersButton:hover {
  color: white;
  background: rgba(255, 255, 255, 0.25);
}

.artwork {
  @apply pointer-events-none absolute inset-y-0 right-[238px] w-[260px];
  color: rgba(255, 255, 255, 0.61);
}

.artworkShell {
  @apply absolute -top-8 right-0 flex h-[130px] w-[130px] items-end justify-center rounded-2xl border-[3px] pb-4;
  border-color: currentColor;
}

.artworkShell::before {
  @apply absolute -top-2 left-5 h-5 w-3 rounded-sm;
  content: '';
  color: rgba(255, 255, 255, 0.72);
  background: currentColor;
  box-shadow: 78px 0 0 currentColor;
}

.artworkCurve {
  @apply absolute bottom-[-62px] right-[-12px] h-20 w-[286px] rounded-[50%] border-t-[3px];
  border-color: rgba(255, 255, 255, 0.32);
  transform: rotate(-13deg);
}

@media (max-width: 760px) {
  .artwork {
    @apply hidden;
  }

  .ordersButton {
    @apply right-4;
  }

  .titleRow p {
    @apply max-w-[360px] truncate;
  }
}
</style>
