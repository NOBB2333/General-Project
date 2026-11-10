<script setup lang="ts">
import {
  AlertTriangle,
  ArrowRightLeft,
  Banknote,
  BellRing,
  Boxes,
  CircleCheck,
  Clock3,
  Download,
  FileBox,
  History,
  Plus,
  TrendingUp,
  Truck,
  Warehouse,
} from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { TransferLoanCategory, TransferLoanKind, TransferLoanOverdueItem } from '../../model/transfer-loan.types';

defineProps<{
  activeKind: 'all' | TransferLoanKind;
  categories: readonly TransferLoanCategory[];
  overdueItems: readonly TransferLoanOverdueItem[];
  totalAmount: number;
}>();

const emit = defineEmits<{
  'update:activeKind': [value: 'all' | TransferLoanKind];
  focusRow: [code: string];
  export: [];
  create: [];
}>();

function currency(value: number) {
  return `¥${(value / 10_000).toFixed(1)}万`;
}
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.heroCopy">
      <p :class="$style.breadcrumb">库存管理 <span>›</span> 调拨与借出</p>
      <div :class="$style.titleRow">
        <span :class="$style.heroIcon">
          <ArrowRightLeft
            :size="24"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>库存调拨与借出</h1>
          <p>跨仓库调拨、借出归还与外协领用单据管理</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.heroArtwork"
      aria-hidden="true"
    >
      <span :class="$style.warehouseA"><Warehouse :size="39" /></span>
      <span :class="$style.warehouseB"><Warehouse :size="39" /></span>
      <span :class="$style.truck"><Truck :size="37" /></span>
      <span :class="$style.routeLine" />
      <span :class="$style.routeLabel">调拨流转</span>
      <i :class="$style.crateOne" />
      <i :class="$style.crateTwo" />
      <i :class="$style.crateThree" />
    </div>
  </header>

  <div :class="$style.actions">
    <UiButton
      variant="secondary"
      size="sm"
      @click="emit('export')"
    >
      <template #leading><Download :size="14" /></template>
      导出
    </UiButton>
    <UiButton
      variant="primary"
      size="sm"
      @click="emit('create')"
    >
      <template #leading><Plus :size="14" /></template>
      新建单据
    </UiButton>
  </div>

  <section
    :class="$style.categories"
    aria-label="单据业务类型"
  >
    <button
      v-for="category in categories"
      :key="category.id"
      type="button"
      :class="[$style.categoryCard, activeKind === category.id && $style.categoryActive]"
      @click="emit('update:activeKind', category.id)"
    >
      <span :class="$style.categoryIcon">
        <Boxes
          v-if="category.id === 'all'"
          :size="18"
        />
        <ArrowRightLeft
          v-else-if="category.id === 'transfer'"
          :size="18"
        />
        <History
          v-else-if="category.id === 'loan'"
          :size="18"
        />
        <TrendingUp
          v-else
          :size="18"
        />
      </span>
      <span :class="$style.categoryCount">{{ category.count }}</span>
      <strong>{{ category.label }}</strong>
      <small>{{ category.description }}</small>
    </button>
  </section>

  <section :class="$style.overduePanel">
    <header :class="$style.overdueHeader">
      <span><BellRing :size="17" />逾期未归还提醒</span>
      <strong>{{ overdueItems.length }} 单逾期</strong>
    </header>
    <button
      v-for="item in overdueItems"
      :key="item.code"
      type="button"
      :class="$style.overdueRow"
      @click="emit('focusRow', item.code)"
    >
      <AlertTriangle :size="13" />
      <strong>{{ item.code }}</strong>
      <span>{{ item.company }}</span>
      <span>借出 {{ item.quantity }} 件</span>
      <em v-if="item.returned">{{ item.returned }}</em>
      <b>逾期 {{ item.overdueDays }} 天</b>
      <small>应还: {{ item.dueDate }}</small>
    </button>
    <p :class="$style.overdueFoot">其中 1 单为部分归还，仍有未归还商品</p>
  </section>

  <section
    :class="$style.summary"
    aria-label="调拨与借出汇总"
  >
    <span><Clock3 :size="15" /><small>待处理</small><strong>4</strong></span>
    <span><CircleCheck :size="15" /><small>已完成</small><strong>4</strong></span>
    <span
      ><FileBox :size="15" /><small>单据数</small><strong>{{ categories[0]?.count ?? 0 }}</strong></span
    >
    <span
      ><Banknote :size="15" /><small>总金额</small><strong>{{ currency(totalAmount) }}</strong></span
    >
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(105deg, #155b37 0%, #00875a 55%, #20a7c9 112%);
}

.heroCopy {
  @apply relative z-10;
}
.breadcrumb {
  @apply m-0 text-[11px] font-semibold;
  color: rgb(255 255 255 / 72%);
}
.breadcrumb span {
  @apply px-1;
}
.titleRow {
  @apply mt-3 flex items-center gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 13%);
}
.titleRow h1 {
  @apply m-0 text-[22px] font-semibold;
  letter-spacing: 0;
}
.titleRow p {
  @apply m-0 mt-1 text-[12px];
  color: rgb(255 255 255 / 82%);
}
.heroArtwork {
  @apply absolute inset-y-0 right-0 w-[72%] overflow-hidden;
}
.warehouseA,
.warehouseB,
.truck {
  @apply absolute z-10 flex items-center justify-center;
  color: rgb(255 255 255 / 72%);
}
.warehouseA {
  @apply bottom-[-5px] left-[17%];
}
.truck {
  @apply bottom-[-2px] left-[43%];
}
.warehouseB {
  @apply bottom-[-5px] right-[12%];
}
.routeLine {
  @apply absolute left-[23%] right-[17%] top-[74px] border-t-2 border-dashed;
  border-color: rgb(255 255 255 / 55%);
  transform: rotate(-1deg);
}
.routeLabel {
  @apply absolute left-[52%] top-6 text-[11px] font-semibold;
  color: rgb(255 255 255 / 70%);
}
.crateOne,
.crateTwo,
.crateThree {
  @apply absolute h-6 w-6 rounded-sm;
  background: #e5a83b;
  box-shadow: inset 0 0 0 1px rgb(255 255 255 / 12%);
}
.crateOne {
  @apply left-[31%] top-7;
}
.crateTwo {
  @apply left-[59%] top-8;
}
.crateThree {
  @apply right-[24%] top-[68px];
}

.actions {
  @apply flex h-[66px] items-center justify-end gap-2;
}
.categories {
  @apply grid grid-cols-4 gap-3;
}
.categoryCard {
  @apply relative flex min-h-[110px] flex-col items-start rounded border bg-white p-4 text-left transition-colors;
  border-color: var(--ui-border);
  color: var(--ui-text);
}
.categoryCard:hover {
  border-color: var(--app-primary-light);
  background: var(--ui-surface-hover);
}
.categoryActive {
  border-color: var(--ui-text) !important;
  box-shadow: inset 0 3px 0 var(--ui-text);
}
.categoryIcon {
  @apply mb-3 flex h-8 w-8 items-center justify-center rounded;
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
.categoryCount {
  @apply absolute right-4 top-4 rounded-full px-2 py-0.5 text-[10px] font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
.categoryCard strong {
  @apply text-[13px];
}
.categoryCard small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.overduePanel {
  @apply mt-4 min-h-[182px] rounded border p-3;
  color: var(--ui-danger);
  border-color: #ffbdad;
  background: var(--ui-danger-soft);
}
.overdueHeader {
  @apply mb-2 flex items-center justify-between text-[11px];
}
.overdueHeader span {
  @apply flex items-center gap-2 font-semibold;
}
.overdueHeader strong {
  @apply rounded-full px-2 py-0.5 text-[9px] text-white;
  background: var(--ui-danger);
}
.overdueRow {
  @apply mb-1 flex min-h-8 w-full items-center gap-3 rounded bg-white px-3 text-left text-[10px];
  color: var(--ui-text-muted);
}
.overdueRow > svg,
.overdueRow b {
  @apply shrink-0;
  color: var(--ui-danger);
}
.overdueRow strong {
  color: var(--app-primary);
}
.overdueRow em {
  @apply rounded px-1 py-0.5 not-italic;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.overdueRow b {
  @apply ml-auto font-semibold;
}
.overdueRow small {
  color: var(--ui-text-subtle);
}
.overdueFoot {
  @apply mb-0 mt-2 text-[10px];
  color: var(--ui-warning);
}

.summary {
  @apply mt-4 flex min-h-12 items-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.summary > span {
  @apply flex items-center gap-2 border-r px-4 text-[11px];
  border-color: var(--ui-border);
}
.summary > span:first-child {
  @apply pl-0;
}
.summary > span:last-child {
  @apply border-r-0;
}
.summary svg {
  color: var(--app-primary);
}
.summary small {
  color: var(--ui-text-muted);
}
.summary strong {
  @apply text-[14px];
  color: var(--ui-text);
}
</style>
