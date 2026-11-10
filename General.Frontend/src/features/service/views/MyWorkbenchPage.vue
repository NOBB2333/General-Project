<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { cloneReferenceRows } from '@/components/page';
import { UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { myWorkbenchMetrics, myWorkbenchReferenceFixture } from '../model/my-workbench.data';
import type {
  WorkbenchQuickLink,
  WorkbenchScheduleSection,
  WorkbenchSummaryCard,
  WorkbenchTabId,
} from '../model/my-workbench.types';
import WorkbenchHero from './my-workbench/WorkbenchHero.vue';
import WorkbenchMetrics from './my-workbench/WorkbenchMetrics.vue';
import WorkbenchTabPanel from './my-workbench/WorkbenchTabPanel.vue';
import WorkbenchTodayPanel from './my-workbench/WorkbenchTodayPanel.vue';

interface WorkbenchTab {
  id: WorkbenchTabId;
  label: string;
}

const router = useRouter();
const activeTab = ref<WorkbenchTabId>('today');
const rows = ref(cloneReferenceRows(myWorkbenchReferenceFixture.rows));

const tabs: readonly WorkbenchTab[] = [
  { id: 'today', label: '今日' },
  { id: 'orders', label: '我的工单' },
  { id: 'performance', label: '业绩' },
  { id: 'parts', label: '我的备件' },
  { id: 'expenses', label: '我的费用单' },
];

const scheduleSections = computed(() =>
  rows.value.filter((row): row is WorkbenchScheduleSection => row.kind === 'schedule'),
);
const summaryCards = computed(() => rows.value.filter((row): row is WorkbenchSummaryCard => row.kind === 'summary'));
const quickLinks = computed(() => rows.value.filter((row): row is WorkbenchQuickLink => row.kind === 'quick-link'));

function selectTab(tab: WorkbenchTabId) {
  activeTab.value = tab;
}

function navigate(route: string) {
  void router.push(route);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="board"
    data-page-route="/service/my-workbench"
    data-my-workbench-page
    :data-active-tab="activeTab"
  >
    <WorkbenchHero @navigate="navigate" />
    <WorkbenchMetrics :metrics="myWorkbenchMetrics" />

    <UiTabs
      v-model="activeTab"
      :class="$style.tabsRoot"
    >
      <UiTabsList
        :class="$style.tabList"
        label="我的工作台视图"
      >
        <UiTabsTrigger
          v-for="tab in tabs"
          :key="tab.id"
          :class="$style.tabTrigger"
          :value="tab.id"
        >
          {{ tab.label }}
        </UiTabsTrigger>
      </UiTabsList>

      <section :class="$style.tabBody">
        <UiTabPanel value="today">
          <WorkbenchTodayPanel
            :schedule-sections="scheduleSections"
            :summary-cards="summaryCards"
            :quick-links="quickLinks"
            @select-tab="selectTab"
            @navigate="navigate"
          />
        </UiTabPanel>
        <UiTabPanel
          v-for="tab in tabs.filter((item) => item.id !== 'today')"
          :key="tab.id"
          :value="tab.id"
        >
          <WorkbenchTabPanel :tab="tab.id as Exclude<WorkbenchTabId, 'today'>" />
        </UiTabPanel>
      </section>
    </UiTabs>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-w-0 max-w-[1500px] pb-10;
  color: var(--ui-text);
}

.tabsRoot {
  @apply block;
}

.tabList {
  @apply flex w-full items-center rounded-t-lg border border-b-0 bg-white px-2;
  border-color: var(--ui-border);
}

.tabTrigger {
  @apply min-h-[43px] rounded-none px-4 py-3 text-[13px] font-normal;
  color: #5e6c84;
}

.tabTrigger:hover {
  @apply bg-transparent;
  color: var(--ui-text-muted);
}

.tabTrigger[data-state='active'] {
  @apply bg-transparent font-semibold;
  color: var(--app-primary);
}

.tabTrigger[data-state='active']::after {
  @apply absolute inset-x-0 bottom-0 h-0.5;
  content: '';
  background: var(--app-primary);
}

.tabBody {
  @apply min-h-[343px] rounded-b-lg border bg-white p-5;
  border-color: var(--ui-border);
}
</style>
