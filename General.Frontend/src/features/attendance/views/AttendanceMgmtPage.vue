<script setup lang="ts">
import { computed, ref, type Component } from 'vue';
import {
  BriefcaseBusiness,
  CalendarDays,
  CheckCircle2,
  Clock3,
  Download,
  FilePenLine,
  Plane,
  RefreshCw,
  RotateCcw,
  Settings,
  Timer,
  UserRound,
  Users,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
  type UiSelectOption,
} from '@/components/ui';
import { attendanceMgmtReferenceFixture } from '../model/attendance-mgmt.data';
import type { AttendanceCorrectionDraft, AttendanceRecord, AttendanceStatus } from '../model/attendance-mgmt.types';
import AttendanceAuxiliaryPanels from './attendance-mgmt/AttendanceAuxiliaryPanels.vue';
import AttendanceCorrectionDialog from './attendance-mgmt/AttendanceCorrectionDialog.vue';
import AttendanceHero from './attendance-mgmt/AttendanceHero.vue';
import AttendanceTable from './attendance-mgmt/AttendanceTable.vue';

type AttendanceTab = '每日考勤' | '个人考勤' | '补签申请' | '考勤规则';
interface StatCard {
  label: string;
  value: string;
  tone: string;
  icon: Component;
}
const rows = ref<AttendanceRecord[]>(cloneReferenceRows(attendanceMgmtReferenceFixture.rows));
const tab = ref<AttendanceTab>('每日考勤');
const query = ref('');
const startDate = ref('2026-03-01');
const endDate = ref('2026-03-10');
const department = ref('全部部门');
const person = ref('全部人员');
const status = ref<'全部状态' | AttendanceStatus>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<AttendanceRecord | null>(null);
const correctionCount = ref(2);
const stats: StatCard[] = [
  { label: '总记录', value: '90', tone: 'neutral', icon: CalendarDays },
  { label: '正常', value: '49', tone: 'green', icon: CheckCircle2 },
  { label: '迟到', value: '21', tone: 'orange', icon: Clock3 },
  { label: '早退', value: '9', tone: 'red', icon: Timer },
  { label: '旷工', value: '4', tone: 'red', icon: UserRound },
  { label: '请假', value: '2', tone: 'purple', icon: BriefcaseBusiness },
  { label: '出差', value: '2', tone: 'blue', icon: Plane },
  { label: '加班', value: '6', tone: 'cyan', icon: Timer },
];
const departmentOptions: UiSelectOption[] = [
  '全部部门',
  '产品部',
  '技术部',
  '市场部',
  '销售部',
  '财务部',
  '人事部',
].map((value) => ({ value, label: value }));
const personOptions: UiSelectOption[] = ['全部人员', ...rows.value.map((row) => row.name)].map((value) => ({
  value,
  label: value,
}));
const statusOptions: UiSelectOption[] = [
  '全部状态',
  '正常',
  '迟到',
  '早退',
  '迟到+早退',
  '旷工',
  '请假',
  '出差',
  '加班',
].map((value) => ({ value, label: value }));
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter(
    (row) =>
      (!keyword || row.name.toLowerCase().includes(keyword) || row.id.includes(keyword)) &&
      (!startDate.value || row.date >= startDate.value) &&
      (!endDate.value || row.date <= endDate.value) &&
      (department.value === '全部部门' || row.department === department.value) &&
      (person.value === '全部人员' || row.name === person.value) &&
      (status.value === '全部状态' || row.status === status.value),
  );
});
const resultTotal = computed(() =>
  query.value || department.value !== '全部部门' || person.value !== '全部人员' || status.value !== '全部状态'
    ? filteredRows.value.length
    : 90,
);
function resetFilters() {
  query.value = '';
  startDate.value = '2026-03-01';
  endDate.value = '2026-03-10';
  department.value = '全部部门';
  person.value = '全部人员';
  status.value = '全部状态';
}
function openDetails(row: AttendanceRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function openCorrection(row?: AttendanceRecord) {
  activeRow.value = row ?? null;
  formOpen.value = true;
}
function saveCorrection(draft: AttendanceCorrectionDraft) {
  rows.value = rows.value.map((row) =>
    row.name === draft.person && row.date === draft.date ? { ...row, remark: '补签审批中' } : row,
  );
  correctionCount.value += 1;
  formOpen.value = false;
  toast.success('补签申请已提交');
}
function exportRows() {
  const head = [
    '日期',
    '星期',
    '姓名',
    '部门',
    '规定上班',
    '规定下班',
    '实际签到',
    '实际签退',
    '工时',
    '状态',
    '迟到(分)',
    '早退(分)',
    '备注',
  ];
  const body = filteredRows.value.map((row) => [
    row.date,
    row.weekday,
    row.name,
    row.department,
    row.scheduledIn,
    row.scheduledOut,
    row.actualIn,
    row.actualOut,
    row.hours,
    row.status,
    row.lateMinutes ?? '-',
    row.earlyMinutes ?? '-',
    row.remark,
  ]);
  const csv = [head, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const anchor = document.createElement('a');
  anchor.href = href;
  anchor.download = '每日考勤.csv';
  anchor.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/attendance/attendance-mgmt"
    data-attendance-mgmt-page
    :data-active-tab="tab"
  >
    <AttendanceHero />
    <UiTabs v-model="tab"
      ><div :class="$style.tabBar">
        <UiTabsList label="考勤管理视图"
          ><UiTabsTrigger
            value="每日考勤"
            :class="$style.tab"
            ><CalendarDays
              :size="14"
              :stroke-width="1.8"
            />每日考勤</UiTabsTrigger
          ><UiTabsTrigger
            value="个人考勤"
            :class="$style.tab"
            ><Users
              :size="14"
              :stroke-width="1.8"
            />个人考勤</UiTabsTrigger
          ><UiTabsTrigger
            value="补签申请"
            :class="$style.tab"
            ><FilePenLine
              :size="14"
              :stroke-width="1.8"
            />补签申请<span :class="$style.badge">{{ correctionCount }}</span></UiTabsTrigger
          ><UiTabsTrigger
            value="考勤规则"
            :class="$style.tab"
            ><Settings
              :size="14"
              :stroke-width="1.8"
            />考勤规则</UiTabsTrigger
          ></UiTabsList
        >
        <div :class="$style.actions">
          <UiButton
            variant="primary"
            size="md"
            @click="openCorrection()"
            ><FilePenLine
              :size="14"
              :stroke-width="1.8"
            />新建补签</UiButton
          ><UiButton
            size="md"
            @click="exportRows"
            ><Download
              :size="14"
              :stroke-width="1.8"
            />导出</UiButton
          ><UiButton
            variant="ghost"
            size="icon"
            title="刷新"
            aria-label="刷新"
            @click="toast.success('考勤记录已刷新')"
            ><RefreshCw
              :size="15"
              :stroke-width="1.8"
          /></UiButton>
        </div>
      </div>
      <UiTabPanel value="每日考勤">
        <section :class="$style.stats">
          <article
            v-for="item in stats"
            :key="item.label"
            :data-tone="item.tone"
          >
            <strong>{{ item.value }}</strong
            ><span>{{ item.label }}</span>
          </article>
        </section>
        <div :class="$style.insights">
          <span
            ><CheckCircle2
              :size="13"
              :stroke-width="1.8"
            />出勤率 54.4%</span
          ><span
            ><Clock3
              :size="13"
              :stroke-width="1.8"
            />平均工时 7.4h</span
          ><span
            ><Timer
              :size="13"
              :stroke-width="1.8"
            />累计迟到 543分钟</span
          >
        </div>
        <div :class="$style.filters">
          <UiInput
            v-model="query"
            :class="$style.search"
            aria-label="搜索考勤记录"
            placeholder="搜索姓名/工号..."
          /><UiInput
            v-model="startDate"
            :class="$style.date"
            type="date"
            aria-label="开始日期"
          /><span>~</span
          ><UiInput
            v-model="endDate"
            :class="$style.date"
            type="date"
            aria-label="结束日期"
          /><UiSelect
            v-model="department"
            :class="$style.select"
            :options="departmentOptions"
            label="全部部门"
            size="md"
          /><UiSelect
            v-model="person"
            :class="$style.personSelect"
            :options="personOptions"
            label="全部人员"
            size="md"
          /><UiSelect
            v-model="status"
            :class="$style.select"
            :options="statusOptions"
            label="全部状态"
            size="md"
          /><UiButton
            variant="ghost"
            size="sm"
            @click="resetFilters"
            ><RotateCcw
              :size="13"
              :stroke-width="1.8"
            />重置</UiButton
          >
        </div>
        <AttendanceTable
          :rows="filteredRows"
          :total="resultTotal"
          @view="openDetails"
          @correct="openCorrection"
        />
      </UiTabPanel>
      <UiTabPanel value="个人考勤"><AttendanceAuxiliaryPanels tab="个人考勤" /></UiTabPanel
      ><UiTabPanel value="补签申请"><AttendanceAuxiliaryPanels tab="补签申请" /></UiTabPanel
      ><UiTabPanel value="考勤规则"><AttendanceAuxiliaryPanels tab="考勤规则" /></UiTabPanel>
    </UiTabs>
  </main>
  <AttendanceCorrectionDialog
    v-model:open="formOpen"
    :person="activeRow?.name"
    :date="activeRow?.date"
    @save="saveCorrection"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? `${activeRow.name} · ${activeRow.date}` : '考勤详情'"
    :description="activeRow?.status"
    size="sm"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>规定时间</dt>
        <dd>{{ activeRow.scheduledIn }} - {{ activeRow.scheduledOut }}</dd>
      </div>
      <div>
        <dt>实际时间</dt>
        <dd>{{ activeRow.actualIn }} - {{ activeRow.actualOut }}</dd>
      </div>
      <div>
        <dt>工时</dt>
        <dd>{{ activeRow.hours }}</dd>
      </div>
      <div>
        <dt>备注</dt>
        <dd>{{ activeRow.remark }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full w-full pb-8;
  color: var(--ui-text);
}
.tabBar {
  @apply flex h-[68px] items-center justify-between border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[68px] rounded-none px-4 text-[12px];
  background: transparent;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.badge {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px] font-semibold text-white;
  background: #ff5630;
}
.actions {
  @apply flex items-center gap-2;
}
.stats {
  @apply mt-4 grid grid-cols-8 gap-2;
}
.stats article {
  @apply flex h-[64px] flex-col items-center justify-center rounded border;
  border-color: var(--ui-border);
  background: #f4f5f7;
  --stat-color: #42526e;
}
.stats article[data-tone='green'] {
  --stat-color: #00875a;
  background: #e3fcef;
}
.stats article[data-tone='orange'] {
  --stat-color: #ff8b00;
  background: #fff0b3;
}
.stats article[data-tone='red'] {
  --stat-color: #de350b;
  background: #ffebe6;
}
.stats article[data-tone='purple'] {
  --stat-color: #6554c0;
  background: #eae6ff;
}
.stats article[data-tone='blue'] {
  --stat-color: #0052cc;
  background: #deebff;
}
.stats article[data-tone='cyan'] {
  --stat-color: #00a3bf;
  background: #e6fcff;
}
.stats strong {
  @apply text-[20px];
  color: var(--stat-color);
}
.stats span {
  @apply mt-1 text-[10px];
  color: var(--stat-color);
}
.insights {
  @apply flex h-11 items-center gap-5 text-[11px];
}
.insights span {
  @apply flex items-center gap-1;
  color: var(--ui-text-subtle);
}
.insights span:first-child {
  color: #00875a;
}
.insights span:last-child {
  color: #ff8b00;
}
.filters {
  @apply flex items-center gap-2;
}
.filters > span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.search {
  @apply w-[240px];
}
.date {
  @apply w-[145px];
}
.select {
  @apply w-[110px];
}
.personSelect {
  @apply w-[155px];
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[12px] font-medium;
}
</style>
