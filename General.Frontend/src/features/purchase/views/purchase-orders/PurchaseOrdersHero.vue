<script setup lang="ts">
import { Check, ChevronRight, FileText, ReceiptText, Truck, WalletCards } from 'lucide-vue-next';

const steps = [
  { id: 1, label: '采购申请', icon: FileText },
  { id: 2, label: '采购订单', icon: Truck },
  { id: 3, label: '到货入库', icon: Check },
  { id: 4, label: '采购发票', icon: ReceiptText },
  { id: 5, label: '付款', icon: WalletCards },
] as const;
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>采购管理</span><b aria-hidden="true">›</b><strong>采购订单</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"><Truck :size="23" /></span>
        <div>
          <h1>采购订单</h1>
          <p>正式采购订单下达、跟踪到货状态、应付与发票管理</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span
        v-for="index in 6"
        :key="index"
        :class="[$style.bar, $style[`bar${index}`]]"
      />
      <span :class="$style.truck"><Truck :size="67" /></span>
    </div>
  </header>

  <section
    :class="$style.workflow"
    aria-label="采购流程"
  >
    <template
      v-for="(step, index) in steps"
      :key="step.id"
    >
      <span :class="[$style.step, step.id === 2 && $style.stepActive]">
        <i>{{ step.id }}</i>
        {{ step.label }}
      </span>
      <ChevronRight
        v-if="index < steps.length - 1"
        :size="16"
        :stroke-width="1.8"
      />
    </template>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(105deg, #0747a6 0%, #0c5bd8 53%, #3b82f6 100%);
}

.copy {
  @apply relative z-[2];
}

.breadcrumb {
  @apply flex items-center gap-1.5 text-[11px] font-semibold;
  color: rgb(255 255 255 / 72%);
}

.breadcrumb strong {
  color: white;
}

.identity {
  @apply mt-3 flex items-center gap-3;
}

.icon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 13%);
}

.identity h1 {
  @apply m-0 text-[22px] font-semibold leading-7;
  letter-spacing: 0;
}

.identity p {
  @apply m-0 mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}

.scene {
  @apply absolute inset-y-0 right-0 w-[600px] overflow-hidden;
  background: rgb(255 255 255 / 5%);
  clip-path: ellipse(62% 140% at 74% 55%);
}

.bar {
  @apply absolute bottom-0 block w-9 rounded-t-sm;
  background: rgb(255 255 255 / 48%);
}

.bar1 {
  @apply left-[110px] h-7;
}
.bar2 {
  @apply left-[160px] h-12;
}
.bar3 {
  @apply left-[210px] h-[76px];
}
.bar4 {
  @apply left-[260px] h-[102px];
}
.bar5 {
  @apply left-[310px] h-[122px];
}
.bar6 {
  @apply left-[360px] h-[138px];
}

.truck {
  @apply absolute bottom-[-4px] right-16;
  color: rgb(255 255 255 / 78%);
}

.workflow {
  @apply mt-4 flex h-[74px] items-center justify-center gap-4 rounded border bg-white;
  color: #b3bac5;
  border-color: var(--ui-border);
}

.step {
  @apply inline-flex h-9 items-center gap-2 rounded-full px-4 text-[12px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.step i {
  @apply flex h-5 w-5 items-center justify-center rounded-full not-italic text-[10px];
  color: var(--ui-text-subtle);
  background: #dfe1e6;
}

.stepActive {
  color: white;
  background: var(--app-primary);
  box-shadow: 0 3px 8px rgb(0 82 204 / 20%);
}

.stepActive i {
  color: white;
  background: rgb(255 255 255 / 24%);
}
</style>
