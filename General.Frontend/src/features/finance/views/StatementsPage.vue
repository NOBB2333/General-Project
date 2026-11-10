<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog } from '@/components/ui';
import { statementCategories, statementsFixture } from '../model/statements.data';
import type { StatementParty, StatementRow } from '../model/statements.types';
import RecentStatements from './statements/RecentStatements.vue';
import StatementCategoryCard from './statements/StatementCategoryCard.vue';
import StatementsHero from './statements/StatementsHero.vue';

const rows = ref<StatementRow[]>(cloneReferenceRows(statementsFixture.rows));
const activeParty = ref<'全部' | StatementParty>('全部');
const detailOpen = ref(false);
const activeRow = ref<StatementRow | null>(null);
const visibleRows = computed(() =>
  activeParty.value === '全部' ? rows.value.slice(0, 5) : rows.value.filter((row) => row.party === activeParty.value),
);
const summaries = [
  { label: '待处理', value: 4, tone: 'warning' },
  { label: '已确认', value: 2, tone: 'success' },
  { label: '有差异', value: 2, tone: 'danger' },
] as const;
function openRow(row: StatementRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/finance/statements"
    data-page-label="对账单"
    data-page-module="财务管理"
    data-statements-page
    :data-active-party="activeParty"
    :data-result-count="visibleRows.length"
  >
    <StatementsHero />
    <section :class="$style.categories">
      <StatementCategoryCard
        v-for="category in statementCategories"
        :key="category.party"
        :category="category"
        @select="activeParty = category.party"
      />
    </section>
    <section :class="$style.summaries">
      <button
        v-for="item in summaries"
        :key="item.label"
        type="button"
        :data-tone="item.tone"
        @click="activeParty = '全部'"
      >
        <span>{{ item.label }}</span
        ><strong>{{ item.value }}</strong>
      </button>
    </section>
    <RecentStatements
      :rows="visibleRows"
      :active-party="activeParty"
      @open="openRow"
      @clear-filter="activeParty = '全部'"
    />
  </main>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '对账单详情'"
    :description="activeRow ? `${activeRow.company} · ${activeRow.status}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>往来单位</dt>
        <dd>{{ activeRow.company }}</dd>
      </div>
      <div>
        <dt>对账类型</dt>
        <dd>{{ activeRow.party }}对账</dd>
      </div>
      <div>
        <dt>对账期间</dt>
        <dd>{{ activeRow.period }}</dd>
      </div>
      <div>
        <dt>对账金额</dt>
        <dd>{{ money(activeRow.amount) }}</dd>
      </div>
      <div>
        <dt>负责人</dt>
        <dd>{{ activeRow.owner }}</dd>
      </div>
      <div>
        <dt>差异金额</dt>
        <dd>{{ money(activeRow.difference) }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1120px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.categories {
  @apply mt-4 grid grid-cols-2 gap-4;
}
.summaries {
  @apply mt-5 grid grid-cols-3 gap-4;
}
.summaries button {
  @apply flex h-[88px] items-center gap-4 rounded border bg-white px-6 text-left transition-colors;
  border-color: var(--ui-border);
}
.summaries button:hover {
  background: var(--ui-surface-hover);
}
.summaries span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.summaries strong {
  @apply block text-[21px];
  color: var(--ui-text);
}
.summaries button[data-tone='warning'] strong {
  color: #ff8b00;
}
.summaries button[data-tone='success'] strong {
  color: #00875a;
}
.summaries button[data-tone='danger'] strong {
  color: #de350b;
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details > div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[12px] font-semibold;
}
</style>
