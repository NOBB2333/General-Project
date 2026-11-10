<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabPanel } from '@/components/ui';
import {
  salesBoardMetrics,
  salesBoardPeriods,
  salesBoardRankings,
  salesBoardReferenceFixture,
  salesBoardScopes,
  salesBoardTabs,
  salesBoardTrend,
} from '../model/sales-board.data';
import type {
  SalesBoardMetric,
  SalesBoardPeriod,
  SalesBoardScope,
  SalesBoardSection,
} from '../model/sales-board.types';
import SalesBoardHeader from './sales-board/SalesBoardHeader.vue';
import SalesBoardMetrics from './sales-board/SalesBoardMetrics.vue';
import SalesBoardOverview from './sales-board/SalesBoardOverview.vue';
import SalesBoardSectionPanel from './sales-board/SalesBoardSectionPanel.vue';

const referenceDefaults = salesBoardReferenceFixture.defaults!;
const localRows = ref(cloneReferenceRows(salesBoardReferenceFixture.rows));
const activeSection = ref<SalesBoardSection>(referenceDefaults.section as SalesBoardSection);
const activeScope = ref<SalesBoardScope>(referenceDefaults.scope as SalesBoardScope);
const activePeriod = ref<SalesBoardPeriod>(referenceDefaults.period as SalesBoardPeriod);
const detailOpen = ref(false);
const detailTitle = ref('');
const detailDescription = ref('');
const detailFacts = ref<string[]>([]);

const currentScopeLabel = computed(
  () => salesBoardScopes.find((scope) => scope.id === activeScope.value)?.label ?? '全公司',
);
const currentPeriodLabel = computed(
  () => salesBoardPeriods.find((period) => period.id === activePeriod.value)?.label ?? '本月',
);
const stateAnnouncement = computed(() => `${currentScopeLabel.value} · ${currentPeriodLabel.value}`);

function changeSection(value: SalesBoardSection) {
  activeSection.value = value;
}

function changeScope(value: SalesBoardScope) {
  activeScope.value = value;
}

function changePeriod(value: SalesBoardPeriod) {
  activePeriod.value = value;
}

function openDetail(title: string, description: string, facts: string[]) {
  detailTitle.value = title;
  detailDescription.value = description;
  detailFacts.value = facts;
  detailOpen.value = true;
}

function openMetric(metric: SalesBoardMetric) {
  openDetail(metric.label, metric.detail, [
    `当前数值：${metric.value}${metric.unit ?? ''}`,
    `较上期：${metric.trend >= 0 ? '增长' : '下降'} ${Math.abs(metric.trend)}%`,
    `数据范围：${stateAnnouncement.value}`,
  ]);
}

function openTrend() {
  const latest = salesBoardTrend[salesBoardTrend.length - 1]!;
  openDetail('业绩 / 合同 / 回款趋势', '过去 6 个月的销售核心金额走势。', [
    `最新确认业绩：${latest.performance}万`,
    `最新合同总额：${latest.contracts}万`,
    `最新实际回款：${latest.collections}万`,
  ]);
}

function openTarget() {
  openDetail('2026 年度目标', '年度销售目标完成情况。', [
    '年度目标：¥24,000,000.00',
    '已完成：¥1,742,161.60',
    '完成率：7.3%',
    '目标缺口：¥22,257,838.40',
  ]);
}

function openRanking(id: string) {
  const person = salesBoardRankings.find((item) => item.id === id);
  if (!person) return;
  openDetail(`${person.name} · 本期业绩`, person.team, [
    `当前排名：第 ${person.rank} 名`,
    `确认业绩：${person.amount}`,
    `目标达成：${person.rate}`,
  ]);
}

function openSectionDetail(label: string) {
  const section = salesBoardTabs.find((tab) => tab.id === activeSection.value)!;
  openDetail(`${section.label} · ${label}`, section.badge, [
    `当前视角：${currentScopeLabel.value}`,
    `当前周期：${currentPeriodLabel.value}`,
    `本地参考行：${localRows.value.filter((row) => row.section === activeSection.value).length} 条`,
  ]);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/sales-perf/sales-board"
    data-page-label="销售看板"
    data-page-module="销售绩效"
    data-module-sales-board
    :data-active-section="activeSection"
    :data-active-scope="activeScope"
    :data-active-period="activePeriod"
    :data-local-row-count="localRows.length"
  >
    <SalesBoardHeader
      :section="activeSection"
      :scope="activeScope"
      :period="activePeriod"
      :updated-at="String(referenceDefaults.updatedAt)"
      @update:section="changeSection"
      @update:scope="changeScope"
      @update:period="changePeriod"
    >
      <UiTabPanel value="overview">
        <SalesBoardMetrics
          :metrics="salesBoardMetrics"
          @select="openMetric"
        />
        <SalesBoardOverview
          @open-trend="openTrend"
          @open-target="openTarget"
          @open-ranking="openRanking"
        />
      </UiTabPanel>

      <UiTabPanel
        v-for="tab in salesBoardTabs.slice(1)"
        :key="tab.id"
        :value="tab.id"
      >
        <SalesBoardSectionPanel
          :section="tab.id"
          :scope="activeScope"
          :period="activePeriod"
          @inspect="openSectionDetail"
        />
      </UiTabPanel>
    </SalesBoardHeader>

    <p
      :class="$style.stateAnnouncement"
      aria-live="polite"
    >
      当前数据范围：{{ stateAnnouncement }}
    </p>

    <UiDialog
      v-model:open="detailOpen"
      :title="detailTitle"
      :description="detailDescription"
      size="sm"
    >
      <dl
        :class="$style.detailFacts"
        data-board-detail-facts
      >
        <div
          v-for="fact in detailFacts"
          :key="fact"
        >
          <dt>{{ fact.split('：')[0] }}</dt>
          <dd>{{ fact.includes('：') ? fact.slice(fact.indexOf('：') + 1) : fact }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
        >
          知道了
        </UiButton>
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1050px] pb-8;
  color: var(--ui-text);
}

.stateAnnouncement {
  @apply sr-only;
}

.detailFacts {
  @apply grid gap-2 p-5;
}

.detailFacts div {
  @apply grid grid-cols-[120px_1fr] gap-3 rounded border px-3 py-2.5;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detailFacts dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.detailFacts dd {
  @apply text-right text-[12px] font-semibold;
  color: var(--ui-text);
}
</style>
