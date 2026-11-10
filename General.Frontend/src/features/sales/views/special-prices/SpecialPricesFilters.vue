<script setup lang="ts">
import { Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput } from '@/components/ui';
import type {
  SpecialPriceApprovalFilter,
  SpecialPriceOrderFilter,
  SpecialPriceUrgencyFilter,
} from '../../model/special-prices.types';

defineProps<{
  query: string;
  approvalStatus: SpecialPriceApprovalFilter;
  urgency: SpecialPriceUrgencyFilter;
  orderStatus: SpecialPriceOrderFilter;
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:approvalStatus': [value: SpecialPriceApprovalFilter];
  'update:urgency': [value: SpecialPriceUrgencyFilter];
  'update:orderStatus': [value: SpecialPriceOrderFilter];
  create: [];
}>();

const approvalOptions: readonly SpecialPriceApprovalFilter[] = ['全部', '草稿', '审批中', '已通过', '已驳回', '已撤回'];
const urgencyOptions: readonly SpecialPriceUrgencyFilter[] = ['全部', '普通', '加急', '特急'];
const orderOptions: readonly SpecialPriceOrderFilter[] = ['全部', '未下单', '已下单', '部分下单', '已作废'];
</script>

<template>
  <section
    :class="$style.filters"
    aria-label="特价筛选"
  >
    <div :class="$style.primaryRow">
      <label :class="$style.search">
        <Search
          :size="16"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          aria-label="搜索特价"
          placeholder="搜索特价编号、客户、订单号、申请人…"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <div :class="$style.group">
        <span>审批状态</span>
        <button
          v-for="item in approvalOptions"
          :key="item"
          type="button"
          :class="[$style.option, approvalStatus === item && $style.active]"
          @click="emit('update:approvalStatus', item)"
        >
          {{ item }}
        </button>
      </div>
      <UiButton
        variant="primary"
        @click="emit('create')"
        ><template #leading
          ><Plus
            :size="15"
            :stroke-width="1.8" /></template
        >新建特价</UiButton
      >
    </div>
    <div :class="$style.secondaryRow">
      <div :class="$style.group">
        <span>紧急程度</span>
        <button
          v-for="item in urgencyOptions"
          :key="item"
          type="button"
          :class="[$style.option, urgency === item && $style.active]"
          @click="emit('update:urgency', item)"
        >
          {{ item }}
        </button>
      </div>
      <div :class="[$style.group, $style.orderGroup]">
        <span>下单状态</span>
        <button
          v-for="item in orderOptions"
          :key="item"
          type="button"
          :class="[$style.option, orderStatus === item && $style.active]"
          @click="emit('update:orderStatus', item)"
        >
          {{ item }}
        </button>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.filters {
  @apply mb-4 rounded-lg border bg-white p-4 shadow-sm;
  border-color: var(--ui-border-subtle);
}
.primaryRow,
.secondaryRow,
.group {
  @apply flex items-center;
}
.primaryRow {
  @apply gap-3;
}
.primaryRow > :last-child {
  @apply ml-auto;
}
.secondaryRow {
  @apply mt-3 gap-4 border-t pt-3;
  border-color: var(--ui-border-subtle);
}
.search {
  @apply relative block w-[290px] shrink-0;
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: #97a0af;
}
.search input {
  @apply pl-9;
}
.group {
  @apply gap-1;
}
.group > span {
  @apply mr-1 whitespace-nowrap text-[11px];
  color: #97a0af;
}
.option {
  @apply min-h-7 whitespace-nowrap rounded px-2.5 text-[11px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.option:hover {
  background: var(--ui-surface-hover);
}
.active {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.orderGroup {
  @apply border-l pl-4;
  border-color: var(--ui-border);
}
</style>
