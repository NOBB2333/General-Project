<script setup lang="ts">
import { ChartNoAxesCombined, Layers3, Settings2, TimerReset } from 'lucide-vue-next';
import { UiTabsList, UiTabsTrigger } from '@/components/ui';

const tabs = [
  { value: 'collection', label: '成本归集', icon: Layers3 },
  { value: 'results', label: '归集结果看板', icon: ChartNoAxesCombined },
  { value: 'rules', label: '成本规则', icon: Settings2 },
] as const;
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 成本中心</p>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <TimerReset
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>成本中心</h1>
          <p>管理销售成本结转、成本归集与规则配置</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.artwork"
      aria-hidden="true"
    >
      <span :class="$style.donut"><b>总成本</b><strong>¥9.8M</strong></span>
      <span :class="$style.costBar"><i />¥400K</span>
      <span :class="$style.costBar"><i />¥320K</span>
      <span :class="$style.costBar"><i />¥240K</span>
    </div>
  </header>

  <UiTabsList
    label="成本中心业务视图"
    :class="$style.mainTabs"
  >
    <UiTabsTrigger
      v-for="tab in tabs"
      :key="tab.value"
      :value="tab.value"
      :class="$style.mainTab"
    >
      <component
        :is="tab.icon"
        :size="15"
        :stroke-width="1.8"
      />
      {{ tab.label }}
    </UiTabsTrigger>
  </UiTabsList>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-6 py-4 text-white;
  background: linear-gradient(106deg, #d62d06 0%, #ef3d10 47%, #ff8b00 100%);
}

.copy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 78%);
}

.breadcrumb span {
  @apply px-1;
}

.identity {
  @apply mt-4 flex items-center gap-3;
}

.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 13%);
}

.identity h1 {
  @apply m-0 text-[23px] font-semibold;
  letter-spacing: 0;
}

.identity p {
  @apply m-0 mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}

.artwork {
  @apply absolute inset-y-0 right-[8%] w-[48%];
}

.donut {
  @apply absolute top-1/2 flex h-[210px] w-[210px] -translate-y-1/2 flex-col items-center justify-center rounded-full text-center;
  left: -4%;
  color: #ffda70;
  background:
    radial-gradient(circle at center, #2f2946 0 19%, transparent 20%),
    conic-gradient(#ff8b00 0 30%, #ffbd4a 30% 52%, #ffda70 52% 76%, #de350b 76% 100%);
}

.donut b {
  @apply text-[9px];
}

.donut strong {
  @apply text-[16px];
}

.costBar {
  @apply absolute flex h-[22px] w-[200px] items-center gap-3 rounded px-3 text-[9px] font-semibold;
  right: 5%;
  color: rgb(255 255 255 / 72%);
  background: rgb(255 255 255 / 14%);
}

.costBar::before {
  @apply h-[3px] w-[66px] rounded-full;
  content: '';
  background: rgb(255 255 255 / 72%);
}

.costBar:nth-of-type(2) {
  @apply top-[27px];
}

.costBar:nth-of-type(3) {
  @apply top-[60px];
}

.costBar:nth-of-type(4) {
  @apply top-[93px];
}

.costBar i {
  @apply block h-2 w-2 rounded-full;
  background: #ffab00;
}

.mainTabs {
  @apply mt-4 flex min-h-[43px] rounded border bg-white px-4;
  border-color: var(--ui-border);
}

.mainTab {
  @apply min-h-[41px] rounded-none border-b-2 border-transparent px-4 text-[12px];
  color: var(--ui-text-subtle);
}

.mainTab:hover,
.mainTab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.mainTab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}
</style>
