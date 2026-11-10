<script setup lang="ts">
import { CalendarDays, FolderKanban, PackageCheck, UserRound } from 'lucide-vue-next';
import type { BeverageRequest } from '../../model/beverages.types';

defineProps<{ rows: readonly BeverageRequest[] }>();
const emit = defineEmits<{ view: [request: BeverageRequest] }>();

const currency = new Intl.NumberFormat('zh-CN', {
  style: 'currency',
  currency: 'CNY',
  maximumFractionDigits: 0,
});
</script>

<template>
  <div :class="$style.list">
    <button
      v-for="request in rows"
      :key="request.id"
      type="button"
      :class="$style.card"
      @click="emit('view', request)"
    >
      <span :class="$style.code">{{ request.code }}</span>
      <div :class="$style.categories">
        <span
          v-for="category in request.categories"
          :key="category"
          :data-category="category"
          >{{ category }}</span
        >
      </div>
      <div :class="$style.summary">
        <strong>{{ request.summary }}</strong>
        <span>{{ request.itemSummary }}</span>
      </div>
      <strong :class="$style.amount">{{ currency.format(request.amount) }}</strong>
      <span
        :class="$style.status"
        :data-status="request.status"
        >{{ request.status }}</span
      >
      <div :class="$style.meta">
        <UserRound
          :size="13"
          :stroke-width="1.8"
        />{{ request.applicant }} · {{ request.department }}
      </div>
      <div :class="$style.meta">
        <CalendarDays
          :size="13"
          :stroke-width="1.8"
        />{{ request.appliedAt }}
      </div>
      <div :class="$style.purpose">
        <PackageCheck
          :size="13"
          :stroke-width="1.8"
        />用途：{{ request.purpose }}
      </div>
      <div :class="$style.project">
        <FolderKanban
          :size="13"
          :stroke-width="1.8"
        />{{ request.project || '未关联项目' }}
      </div>
    </button>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有申请记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.list {
  @apply grid grid-cols-2 gap-3 p-4;
}
.card {
  @apply grid cursor-pointer grid-cols-[90px_150px_1fr_100px_80px] items-center gap-x-3 gap-y-2 rounded border p-4 text-left outline-none transition-colors;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.card:hover {
  border-color: var(--app-primary);
  background: var(--ui-surface-hover);
}
.card:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}
.code {
  @apply text-[10px] font-semibold;
  color: var(--app-primary);
}
.categories {
  @apply flex flex-wrap gap-1;
}
.categories span {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #ff8b00;
  background: #fff0b3;
}
.categories span[data-category='红酒'] {
  color: #de350b;
  background: #ffebe6;
}
.categories span[data-category='啤酒'],
.categories span[data-category='饮料'] {
  color: #0052cc;
  background: #deebff;
}
.categories span[data-category='茶叶'] {
  color: #6554c0;
  background: #eae6ff;
}
.summary {
  @apply min-w-0;
}
.summary strong,
.summary span {
  @apply block truncate;
}
.summary strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text);
}
.summary span {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.amount {
  @apply text-right text-[12px];
  color: var(--ui-text);
}
.status {
  @apply justify-self-end rounded px-2 py-1 text-[9px] font-semibold;
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已通过'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已领取'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.meta,
.purpose,
.project {
  @apply flex items-center gap-1.5 text-[9px];
  color: var(--ui-text-subtle);
}
.meta:nth-of-type(1) {
  @apply col-start-1 col-end-3;
}
.purpose {
  @apply col-start-1 col-end-4;
}
.project {
  @apply col-start-4 col-end-6 justify-end;
}
.empty {
  @apply col-span-2 py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
@media (max-width: 1150px) {
  .list {
    @apply grid-cols-1;
  }
  .empty {
    @apply col-span-1;
  }
}
</style>
