<script setup lang="ts">
import { UserRound } from 'lucide-vue-next';
import { UiTabsList, UiTabsTrigger } from '@/components/ui';
import { myPerformanceTabs } from '../../model/my-performance.data';
</script>

<template>
  <header :class="$style.header">
    <div :class="$style.headingRow">
      <UserRound
        :size="25"
        :stroke-width="1.8"
        aria-hidden="true"
      />
      <div :class="$style.headingCopy">
        <div :class="$style.titleLine">
          <h1>我的绩效</h1>
          <span>销售视角</span>
        </div>
        <p>张经理 · 实时查看你的业绩、奖金构成、冲刺进度与适用方案</p>
      </div>
    </div>

    <UiTabsList
      label="我的绩效视图"
      :class="$style.tabList"
      data-my-performance-tabs
    >
      <UiTabsTrigger
        v-for="tab in myPerformanceTabs"
        :key="tab.id"
        :value="tab.id"
        :class="$style.tabTrigger"
      >
        <component
          :is="tab.icon"
          :size="15"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <strong>{{ tab.label }}</strong>
        <span v-if="tab.badge">{{ tab.badge }}</span>
      </UiTabsTrigger>
    </UiTabsList>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.header {
  @apply shrink-0;
}

.headingRow {
  @apply flex items-start gap-3 px-1 pt-0.5;
  color: var(--ui-success);
}

.headingCopy {
  @apply min-w-0;
}

.titleLine {
  @apply flex items-center gap-2;
}

.titleLine h1 {
  @apply text-[20px] font-bold leading-7;
  color: var(--ui-text);
}

.titleLine span {
  @apply rounded px-2 py-0.5 text-[11px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.headingCopy p {
  @apply mt-0.5 text-[12px] leading-5;
  color: var(--ui-text-subtle);
}

.tabList {
  @apply mt-5 flex w-full items-stretch justify-start gap-0 border-b;
  border-color: var(--ui-border);
}

.tabTrigger {
  @apply min-h-[40px] rounded-none px-4 py-2 text-[12px];
  color: var(--ui-text-subtle);
  background: transparent;
}

.tabTrigger:hover {
  color: var(--app-primary);
  background: var(--ui-surface-subtle);
}

.tabTrigger strong {
  @apply font-semibold;
}

.tabTrigger span {
  @apply ml-0.5 rounded px-1.5 py-0.5 text-[10px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.tabTrigger[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}

@media (max-width: 1100px) {
  .tabTrigger {
    @apply px-2.5;
  }
}
</style>
