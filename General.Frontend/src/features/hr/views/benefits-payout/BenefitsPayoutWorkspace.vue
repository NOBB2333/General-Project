<script setup lang="ts">
import { Box, Eye, Send, Ticket } from 'lucide-vue-next';
import { UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { BenefitDistribution, BenefitInventoryItem, BenefitView } from '../../model/benefits-payout.types';
defineProps<{ view: BenefitView; rows: readonly BenefitDistribution[]; inventory: readonly BenefitInventoryItem[] }>();
const emit = defineEmits<{ 'update:view': [value: BenefitView]; open: [row: BenefitDistribution] }>();
const setView = (value: string) => emit('update:view', value as BenefitView);
const money = (value: number) => `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2 })}`;
</script>
<template>
  <section :class="$style.workspace">
    <header :class="$style.tabs">
      <UiTabs
        :model-value="view"
        @update:model-value="setView"
        ><UiTabsList label="福利业务视图"
          ><UiTabsTrigger value="发放记录"
            ><Send
              :size="14"
              :stroke-width="1.8"
            />发放记录</UiTabsTrigger
          ><UiTabsTrigger value="兑换码库"
            ><Ticket
              :size="14"
              :stroke-width="1.8"
            />兑换码库 <small>7</small></UiTabsTrigger
          ><UiTabsTrigger value="实物库"
            ><Box
              :size="14"
              :stroke-width="1.8"
            />实物库 <small>7</small></UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
    </header>
    <div
      v-if="view === '发放记录'"
      :class="$style.records"
    >
      <article
        v-for="row in rows"
        :key="row.id"
      >
        <span
          :class="$style.kind"
          :data-kind="row.kind"
          ><Ticket
            v-if="row.kind === '兑换码'"
            :size="18"
            :stroke-width="1.8" /><Box
            v-else
            :size="18"
            :stroke-width="1.8"
        /></span>
        <div :class="$style.copy">
          <p>
            <strong>{{ row.title }}</strong
            ><span>✓ {{ row.status }}</span
            ><small>{{ row.kind }}</small>
          </p>
          <p>{{ row.date }} · {{ row.owner }} · 来源: {{ row.source }}</p>
        </div>
        <dl>
          <div>
            <dt>发放对象</dt>
            <dd>{{ row.audience }}</dd>
          </div>
          <div>
            <dt>人数 × 每人</dt>
            <dd>{{ row.people }}人 × {{ row.quantityPerPerson }}</dd>
          </div>
          <div>
            <dt>发放总值</dt>
            <dd>{{ money(row.totalValue) }}</dd>
          </div>
        </dl>
        <button
          type="button"
          :aria-label="`查看${row.title}`"
          @click="emit('open', row)"
        >
          <Eye
            :size="14"
            :stroke-width="1.8"
          />
        </button>
        <p :class="$style.note">{{ row.note }}</p>
      </article>
    </div>
    <div
      v-else
      :class="$style.inventory"
    >
      <article
        v-for="item in inventory"
        :key="item.id"
      >
        <span :data-kind="view"
          ><Ticket
            v-if="view === '兑换码库'"
            :size="19"
            :stroke-width="1.8" /><Box
            v-else
            :size="19"
            :stroke-width="1.8"
        /></span>
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.source }}</p>
        </div>
        <dl>
          <dt>剩余数量</dt>
          <dd>{{ item.remaining }}</dd>
        </dl>
        <dl>
          <dt>单位价值</dt>
          <dd>{{ item.unitValue }}</dd>
        </dl>
        <small>{{ item.status }}</small>
      </article>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4;
}
.tabs {
  @apply flex h-12 items-end border-b;
  border-color: var(--ui-border);
}
.tabs [role='tab'] {
  @apply h-11 rounded-none px-4;
}
.tabs [role='tab'][data-state='active'] {
  @apply border-b-2 bg-transparent;
  border-color: var(--app-primary);
}
.tabs small {
  @apply ml-1 rounded px-1.5 py-0.5 text-[8px];
  background: var(--ui-surface-hover);
}
.records {
  @apply mt-4 space-y-3;
}
.records article {
  @apply relative grid min-h-[116px] grid-cols-[36px_1fr_auto_24px] items-start gap-3 rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.kind {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: #6554c0;
  background: #eae6ff;
}
.kind[data-kind='实物'] {
  color: #00a3bf;
  background: #e6fcff;
}
.copy p {
  @apply m-0 flex items-center gap-2 text-[10px];
  color: var(--ui-text-subtle);
}
.copy p:first-child {
  @apply text-[12px];
  color: var(--ui-text);
}
.copy p + p {
  @apply mt-1;
}
.copy span {
  @apply rounded px-2 py-0.5 text-[9px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.copy small {
  @apply rounded px-2 py-0.5 text-[9px];
  background: var(--ui-surface-hover);
}
.records dl {
  @apply m-0 flex gap-7;
}
.records dl div {
  @apply min-w-[82px] text-right;
}
.records dt {
  @apply text-[8px];
  color: var(--ui-text-subtle);
}
.records dd {
  @apply m-0 mt-1 text-[11px] font-semibold;
}
.records button {
  @apply mt-2;
  color: var(--ui-text-subtle);
}
.records button:hover {
  color: var(--app-primary);
}
.note {
  @apply col-span-4 m-0 rounded px-3 py-2 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
.inventory {
  @apply mt-4 grid grid-cols-2 gap-3;
}
.inventory article {
  @apply flex min-h-[88px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.inventory > article > span {
  @apply flex h-10 w-10 items-center justify-center rounded;
  color: #6554c0;
  background: #eae6ff;
}
.inventory > article > span[data-kind='实物库'] {
  color: #00a3bf;
  background: #e6fcff;
}
.inventory article > div {
  @apply min-w-0 flex-1;
}
.inventory strong {
  @apply text-[12px];
}
.inventory p {
  @apply m-0 mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.inventory dl {
  @apply m-0 min-w-[72px];
}
.inventory dt {
  @apply text-[8px];
  color: var(--ui-text-subtle);
}
.inventory dd {
  @apply m-0 mt-1 text-[12px] font-semibold;
}
.inventory article > small {
  @apply rounded px-2 py-1 text-[9px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
</style>
