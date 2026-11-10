<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabPanel, UiTabs } from '@/components/ui';
import { myPerformanceReferenceFixture } from '../model/my-performance.data';
import type {
  MyPerformanceBankEntry,
  MyPerformanceDetail,
  MyPerformanceLedgerEntry,
  MyPerformanceMetric,
  MyPerformancePlan,
  MyPerformanceReward,
  MyPerformanceSummary,
  MyPerformanceTab,
} from '../model/my-performance.types';
import MyPerformanceHeader from './my-performance/MyPerformanceHeader.vue';
import MyPerformanceOverview from './my-performance/MyPerformanceOverview.vue';
import MyPerformanceSecondaryPanels from './my-performance/MyPerformanceSecondaryPanels.vue';

const localRows = ref(cloneReferenceRows(myPerformanceReferenceFixture.rows));
const activeTab = ref<MyPerformanceTab>(myPerformanceReferenceFixture.defaults.activeTab as MyPerformanceTab);
const detailOpen = ref(false);
const detail = ref<MyPerformanceDetail>({ title: '', description: '', facts: [] });

const metrics = computed(() => localRows.value.filter((row): row is MyPerformanceMetric => row.kind === 'metric'));
const rewards = computed(() => localRows.value.filter((row): row is MyPerformanceReward => row.kind === 'reward'));
const summaries = computed(() => localRows.value.filter((row): row is MyPerformanceSummary => row.kind === 'summary'));
const bankEntries = computed(() => localRows.value.filter((row): row is MyPerformanceBankEntry => row.kind === 'bank'));
const ledgerEntries = computed(() =>
  localRows.value.filter((row): row is MyPerformanceLedgerEntry => row.kind === 'ledger'),
);
const plans = computed(() => localRows.value.filter((row): row is MyPerformancePlan => row.kind === 'plan'));

function updateTab(value: string) {
  activeTab.value = value as MyPerformanceTab;
}

function inspect(nextDetail: MyPerformanceDetail) {
  detail.value = nextDetail;
  detailOpen.value = true;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="dashboard"
    data-page-route="/sales-perf/my-performance"
    data-page-label="我的绩效"
    data-page-module="销售绩效"
    data-module-my-performance
    :data-active-tab="activeTab"
    :data-local-row-count="localRows.length"
  >
    <UiTabs
      :model-value="activeTab"
      @update:model-value="updateTab"
    >
      <MyPerformanceHeader />

      <UiTabPanel value="overview">
        <MyPerformanceOverview
          :metrics="metrics"
          :rewards="rewards"
          :summaries="summaries"
          @inspect="inspect"
        />
      </UiTabPanel>

      <MyPerformanceSecondaryPanels
        :bank-entries="bankEntries"
        :ledger-entries="ledgerEntries"
        :plans="plans"
      />
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      :title="detail.title"
      :description="detail.description"
      size="sm"
    >
      <dl
        :class="$style.detailFacts"
        data-performance-detail
      >
        <div
          v-for="fact in detail.facts"
          :key="fact"
        >
          <dt>{{ fact.includes('：') ? fact.slice(0, fact.indexOf('：')) : '说明' }}</dt>
          <dd>{{ fact.includes('：') ? fact.slice(fact.indexOf('：') + 1) : fact }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
          >知道了</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1050px] px-[26px] pb-8 pt-[22px];
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
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
