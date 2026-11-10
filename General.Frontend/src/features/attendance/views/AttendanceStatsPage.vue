<script setup lang="ts">
import { CalendarDays, ClipboardList, FileText, Plane } from 'lucide-vue-next';
import { ref } from 'vue';
import {
  UiButton,
  UiDialog,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  type UiSelectOption,
} from '@/components/ui';
import { cloneReferenceRows } from '@/components/page';
import { attendanceStatsReferenceFixture } from '../model/attendance-stats.data';
import AttendanceStatsCharts from './attendance-stats/AttendanceStatsCharts.vue';
import AttendanceStatsHero from './attendance-stats/AttendanceStatsHero.vue';
import AttendanceStatsMetrics from './attendance-stats/AttendanceStatsMetrics.vue';
import AttendanceStatsTable from './attendance-stats/AttendanceStatsTable.vue';

type StatsTab = '考勤分析' | '请假出差' | '在岗情况';
const tab = ref<StatsTab>('考勤分析');
const year = ref('2026年');
const detailsOpen = ref(false);
const yearOptions: UiSelectOption[] = ['2026年', '2025年'].map((value) => ({ value, label: value }));
const fixture = attendanceStatsReferenceFixture;
const rows = cloneReferenceRows(fixture.rows);
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="dashboard"
    data-page-route="/attendance/attendance-stats"
    data-attendance-stats-page
    :data-active-tab="tab"
  >
    <AttendanceStatsHero />
    <UiTabs v-model="tab">
      <div :class="$style.toolbar">
        <UiTabsList label="考勤统计视图">
          <UiTabsTrigger
            value="考勤分析"
            :class="$style.tab"
            ><ClipboardList :size="15" />考勤分析</UiTabsTrigger
          >
          <UiTabsTrigger
            value="请假出差"
            :class="$style.tab"
            ><Plane :size="15" />请假出差</UiTabsTrigger
          >
          <UiTabsTrigger
            value="在岗情况"
            :class="$style.tab"
            ><FileText :size="15" />在岗情况</UiTabsTrigger
          >
        </UiTabsList>
        <div :class="$style.actions">
          <UiButton
            size="md"
            @click="detailsOpen = true"
            ><CalendarDays :size="14" />查看考勤详情</UiButton
          ><UiSelect
            v-model="year"
            :options="yearOptions"
            label="选择年份"
            :class="$style.year"
          />
        </div>
      </div>
      <UiTabPanel value="考勤分析"
        ><AttendanceStatsMetrics /><AttendanceStatsCharts :fixture="fixture" /><AttendanceStatsTable :rows="rows"
      /></UiTabPanel>
      <UiTabPanel value="请假出差"
        ><section :class="$style.placeholder">
          <Plane :size="30" />
          <h2>请假出差趋势</h2>
          <p>按月份查看请假、出差和出勤变化。</p>
        </section></UiTabPanel
      >
      <UiTabPanel value="在岗情况"
        ><section :class="$style.placeholder">
          <ClipboardList :size="30" />
          <h2>在岗情况</h2>
          <p>查看部门在岗人数与实时出勤状态。</p>
        </section></UiTabPanel
      >
    </UiTabs>
    <UiDialog
      v-model:open="detailsOpen"
      title="考勤详情"
      description="2026 年 3 月考勤汇总"
      ><div :class="$style.dialogContent">
        <p>当前总出勤率 <strong>57.0%</strong></p>
        <p>总记录数 <strong>86</strong></p>
        <p>迟到次数 <strong>17</strong></p>
        <p>平均工时 <strong>7.9h</strong></p>
      </div>
      <template #footer
        ><UiButton
          variant="primary"
          size="sm"
          @click="detailsOpen = false"
          >关闭</UiButton
        ></template
      ></UiDialog
    >
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full w-full pb-8;
  color: var(--ui-text);
}
.toolbar {
  @apply flex h-[54px] items-center justify-between border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[54px] rounded-none px-4 text-[12px];
  background: transparent;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.actions {
  @apply flex items-center gap-2;
}
.year {
  @apply w-[108px];
}
.placeholder {
  @apply mt-4 flex min-h-[330px] flex-col items-center justify-center rounded border bg-white text-center;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.placeholder h2 {
  @apply mt-3 text-[16px] font-semibold;
  color: var(--ui-text);
}
.placeholder p {
  @apply mt-1 text-[12px];
}
.dialogContent {
  @apply grid grid-cols-2 gap-3 p-5 text-[12px];
}
.dialogContent p {
  @apply rounded border bg-[#FAFBFC] p-3;
  border-color: var(--ui-border-subtle);
}
.dialogContent strong {
  @apply ml-2 text-[18px];
  color: var(--app-primary);
}
</style>
