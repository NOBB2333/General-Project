<script setup lang="ts">
import { computed, ref, type Component } from 'vue';
import {
  CalendarDays,
  CheckCircle2,
  CircleDollarSign,
  Clock3,
  Coffee,
  List,
  Plus,
  Timer,
  UserRoundCheck,
  Users,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiAlertDialog,
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
import { overtimeReferenceFixture } from '../model/overtime.data';
import type { OvertimeDraft, OvertimeRecord, OvertimeStatus } from '../model/overtime.types';
import OvertimeFormDialog from './overtime/OvertimeFormDialog.vue';
import OvertimeHero from './overtime/OvertimeHero.vue';
import OvertimeTable from './overtime/OvertimeTable.vue';

type OvertimeTab = '我的加班' | '我的团队' | '审批管理';
interface StatCard {
  label: string;
  value: string;
  tone: string;
  icon: Component;
}
const rows = ref<OvertimeRecord[]>(cloneReferenceRows(overtimeReferenceFixture.rows));
const tab = ref<OvertimeTab>('我的加班');
const view = ref<'list' | 'calendar'>('list');
const query = ref('');
const status = ref<'全部状态' | OvertimeStatus>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const withdrawOpen = ref(false);
const activeRow = ref<OvertimeRecord | null>(null);
const statusOptions: UiSelectOption[] = ['全部状态', '待审批', '已批准', '已驳回', '已撤回'].map((value) => ({
  value,
  label: value,
}));
const stats: StatCard[] = [
  { label: '累计加班', value: '18h', tone: 'blue', icon: Timer },
  { label: '待审批', value: '1', tone: 'orange', icon: Clock3 },
  { label: '已批准', value: '4', tone: 'green', icon: CheckCircle2 },
  { label: '调休天数', value: '1.9天', tone: 'green', icon: Coffee },
  { label: '加班费时长', value: '3h', tone: 'orange', icon: CircleDollarSign },
];
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter(
    (row) =>
      (status.value === '全部状态' || row.status === status.value) &&
      (!keyword || row.code.toLowerCase().includes(keyword) || row.reason.toLowerCase().includes(keyword)),
  );
});
function openDetails(row: OvertimeRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function requestWithdraw(row: OvertimeRecord) {
  activeRow.value = row;
  withdrawOpen.value = true;
}
function confirmWithdraw() {
  if (!activeRow.value) return;
  rows.value = rows.value.map((row) => (row.id === activeRow.value?.id ? { ...row, status: '已撤回' } : row));
  withdrawOpen.value = false;
  toast.success('加班申请已撤回');
}
function saveDraft(draft: OvertimeDraft) {
  const [startHour, startMinute] = draft.startTime.split(':').map(Number);
  const [endHour, endMinute] = draft.endTime.split(':').map(Number);
  const hours = Math.max(0.5, (endHour * 60 + endMinute - (startHour * 60 + startMinute)) / 60);
  rows.value = [
    {
      id: `overtime-local-${Date.now()}`,
      code: `OT-2026-${String(rows.value.length + 10).padStart(3, '0')}`,
      date: draft.date,
      period: `${draft.startTime} - ${draft.endTime}`,
      hours,
      reason: draft.reason,
      projectCount: Number(draft.projectCount) || 1,
      compensation: draft.compensation,
      compensationLabel: draft.compensation,
      status: '待审批',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  tab.value = '我的加班';
  toast.success('加班申请已提交');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/attendance/overtime"
    data-overtime-page
    :data-active-tab="tab"
  >
    <OvertimeHero />
    <section :class="$style.stats">
      <article
        v-for="item in stats"
        :key="item.label"
        :data-tone="item.tone"
      >
        <span
          ><component
            :is="item.icon"
            :size="15"
            :stroke-width="1.8"
          />{{ item.label }}</span
        ><strong>{{ item.value }}</strong>
      </article>
    </section>
    <UiTabs v-model="tab">
      <div :class="$style.tabBar">
        <UiTabsList label="加班管理视图"
          ><UiTabsTrigger
            value="我的加班"
            :class="$style.tab"
            ><Timer
              :size="14"
              :stroke-width="1.8"
            />我的加班</UiTabsTrigger
          ><UiTabsTrigger
            value="我的团队"
            :class="$style.tab"
            ><Users
              :size="14"
              :stroke-width="1.8"
            />我的团队</UiTabsTrigger
          ><UiTabsTrigger
            value="审批管理"
            :class="$style.tab"
            ><UserRoundCheck
              :size="14"
              :stroke-width="1.8"
            />审批管理<span :class="$style.badge">4</span></UiTabsTrigger
          ></UiTabsList
        >
        <div :class="$style.actions">
          <UiButton
            variant="ghost"
            size="icon-sm"
            title="列表视图"
            aria-label="列表视图"
            :data-active="view === 'list'"
            @click="view = 'list'"
            ><List
              :size="15"
              :stroke-width="1.8" /></UiButton
          ><UiButton
            variant="ghost"
            size="icon-sm"
            title="日历视图"
            aria-label="日历视图"
            :data-active="view === 'calendar'"
            @click="view = 'calendar'"
            ><CalendarDays
              :size="15"
              :stroke-width="1.8" /></UiButton
          ><UiButton
            variant="primary"
            size="md"
            @click="formOpen = true"
            ><template #leading
              ><Plus
                :size="14"
                :stroke-width="1.8" /></template
            >新建加班</UiButton
          >
        </div>
      </div>
      <UiTabPanel value="我的加班">
        <div :class="$style.filters">
          <UiInput
            v-model="query"
            :class="$style.search"
            aria-label="搜索加班记录"
            placeholder="搜索单号/事由..."
          /><UiSelect
            v-model="status"
            :class="$style.select"
            :options="statusOptions"
            label="全部状态"
            size="md"
          />
        </div>
        <OvertimeTable
          v-if="view === 'list'"
          :rows="filteredRows"
          @view="openDetails"
          @withdraw="requestWithdraw"
        />
        <section
          v-else
          :class="$style.calendarPanel"
        >
          <article
            v-for="row in filteredRows"
            :key="row.id"
          >
            <time>{{ row.date }}</time
            ><strong>{{ row.reason }}</strong
            ><span>{{ row.period }} · {{ row.hours }}h</span>
          </article>
        </section>
      </UiTabPanel>
      <UiTabPanel value="我的团队"
        ><section :class="$style.secondary">
          <h2>团队加班记录</h2>
          <p>查看团队成员的加班安排与补偿方式。</p>
          <OvertimeTable
            :rows="rows"
            @view="openDetails"
            @withdraw="requestWithdraw"
          /></section
      ></UiTabPanel>
      <UiTabPanel value="审批管理"
        ><section :class="$style.secondary">
          <h2>待我审批</h2>
          <p>当前有 4 条团队加班申请等待处理。</p>
          <div :class="$style.approval">
            <strong>王小红 · 版本发布支持</strong><span>2026-03-28 · 18:00 - 22:00</span
            ><UiButton
              size="xs"
              variant="primary"
              @click="toast.success('加班申请已批准')"
              >批准</UiButton
            >
          </div>
        </section></UiTabPanel
      >
    </UiTabs>
  </main>
  <OvertimeFormDialog
    v-model:open="formOpen"
    @save="saveDraft"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '加班详情'"
    :description="activeRow ? `${activeRow.date} · ${activeRow.status}` : undefined"
    size="sm"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>加班时段</dt>
        <dd>{{ activeRow.period }}</dd>
      </div>
      <div>
        <dt>时长</dt>
        <dd>{{ activeRow.hours }}h</dd>
      </div>
      <div>
        <dt>补偿方式</dt>
        <dd>{{ activeRow.compensationLabel }}</dd>
      </div>
      <div>
        <dt>加班事由</dt>
        <dd>{{ activeRow.reason }}</dd>
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
  <UiAlertDialog
    v-model:open="withdrawOpen"
    title="撤回加班申请"
    :description="`确认撤回 ${activeRow?.code ?? ''}？`"
    confirm-label="确认撤回"
    tone="warning"
    @confirm="confirmWithdraw"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full w-full pb-8;
  color: var(--ui-text);
}
.stats {
  @apply mt-4 grid grid-cols-5 gap-3;
}
.stats article {
  @apply h-[72px] rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
  --stat-color: #0052cc;
}
.stats article[data-tone='orange'] {
  --stat-color: #ff8b00;
}
.stats article[data-tone='green'] {
  --stat-color: #00875a;
}
.stats span {
  @apply flex items-center gap-1.5 text-[11px];
  color: var(--ui-text-subtle);
}
.stats span svg,
.stats strong {
  color: var(--stat-color);
}
.stats strong {
  @apply mt-0.5 block text-[21px] font-bold;
}
.tabBar {
  @apply mt-3 flex h-[48px] items-center justify-between;
}
.tab {
  @apply h-9 px-3 text-[12px];
}
.badge {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px] font-semibold text-white;
  background: #ff5630;
}
.actions {
  @apply flex items-center gap-1;
}
.actions button[data-active='true'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.filters {
  @apply flex items-center gap-3;
}
.search {
  @apply w-[285px];
}
.select {
  @apply w-[90px];
}
.calendarPanel {
  @apply mt-3 grid grid-cols-3 gap-3;
}
.calendarPanel article {
  @apply flex flex-col rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.calendarPanel time {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.calendarPanel strong {
  @apply mt-2 text-[13px];
}
.calendarPanel span {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.secondary {
  @apply mt-3 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.secondary h2 {
  @apply text-[15px] font-semibold;
}
.secondary > p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.approval {
  @apply mt-4 flex items-center gap-4 rounded border p-4;
  border-color: var(--ui-border);
}
.approval span {
  @apply flex-1 text-[11px];
  color: var(--ui-text-subtle);
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
