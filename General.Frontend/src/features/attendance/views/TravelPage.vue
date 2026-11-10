<script setup lang="ts">
import { computed, ref, type Component } from 'vue';
import {
  CalendarDays,
  CheckCircle2,
  CircleDollarSign,
  Clock3,
  MapPin,
  Plane,
  Plus,
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
import { travelReferenceFixture } from '../model/travel.data';
import type { TravelDraft, TravelRecord, TravelStatus } from '../model/travel.types';
import TravelFormDialog from './travel/TravelFormDialog.vue';
import TravelHero from './travel/TravelHero.vue';
import TravelTable from './travel/TravelTable.vue';

type TravelTab = '我的出差' | '我的团队' | '审批管理';
interface StatCard {
  label: string;
  value: string;
  tone: string;
  icon: Component;
}

const rows = ref<TravelRecord[]>(cloneReferenceRows(travelReferenceFixture.rows));
const tab = ref<TravelTab>('我的出差');
const query = ref('');
const status = ref<'全部状态' | TravelStatus>('全部状态');
const startDate = ref('');
const endDate = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const withdrawOpen = ref(false);
const activeRow = ref<TravelRecord | null>(null);

const statusOptions: UiSelectOption[] = ['全部状态', '待审批', '已批准', '已驳回', '已撤回', '已完成', '已报销'].map(
  (value) => ({ value, label: value }),
);
const stats = computed<StatCard[]>(() => [
  { label: '本年出差', value: `${rows.value.length}次`, tone: 'blue', icon: Plane },
  {
    label: '出差天数',
    value: `${rows.value.reduce((sum, row) => sum + row.days, 0)}天`,
    tone: 'purple',
    icon: CalendarDays,
  },
  {
    label: '累计费用',
    value: '12,500元',
    tone: 'orange',
    icon: CircleDollarSign,
  },
  {
    label: '待审批',
    value: String(rows.value.filter((row) => row.status === '待审批').length),
    tone: 'red',
    icon: Clock3,
  },
  {
    label: '已通过',
    value: String(rows.value.filter((row) => ['已批准', '已完成', '已报销'].includes(row.status)).length),
    tone: 'green',
    icon: CheckCircle2,
  },
]);
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter(
    (row) =>
      (status.value === '全部状态' || row.status === status.value) &&
      (!startDate.value || row.departureDate >= startDate.value) &&
      (!endDate.value || row.returnDate <= endDate.value) &&
      (!keyword ||
        row.code.toLowerCase().includes(keyword) ||
        row.destination.includes(keyword) ||
        row.reason.toLowerCase().includes(keyword)),
  );
});

function openDetails(row: TravelRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function requestWithdraw(row: TravelRecord) {
  activeRow.value = row;
  withdrawOpen.value = true;
}
function confirmWithdraw() {
  if (!activeRow.value) return;
  rows.value = rows.value.map((row) => (row.id === activeRow.value?.id ? { ...row, status: '已撤回' } : row));
  withdrawOpen.value = false;
  toast.success('出差申请已撤回');
}
function reimburse(row: TravelRecord) {
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, status: '已报销' } : item));
  toast.success(`${row.code} 已提交报销`);
}
function saveDraft(draft: TravelDraft) {
  const start = new Date(`${draft.departureDate}T00:00:00`);
  const end = new Date(`${draft.returnDate}T00:00:00`);
  rows.value = [
    {
      id: `travel-local-${Date.now()}`,
      code: `BT-2026-${String(rows.value.length + 6).padStart(3, '0')}`,
      destination: draft.destination,
      transport: draft.transport,
      reason: draft.reason,
      departureDate: draft.departureDate,
      returnDate: draft.returnDate,
      days: Math.floor((end.getTime() - start.getTime()) / 86_400_000) + 1,
      budget: Number(draft.budget),
      status: '待审批',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  tab.value = '我的出差';
  toast.success('出差申请已提交');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/attendance/travel"
    data-travel-page
    :data-active-tab="tab"
  >
    <TravelHero />
    <UiTabs v-model="tab">
      <div :class="$style.tabBar">
        <UiTabsList label="出差管理视图">
          <UiTabsTrigger
            value="我的出差"
            :class="$style.tab"
            ><MapPin
              :size="14"
              :stroke-width="1.8"
            />我的出差</UiTabsTrigger
          >
          <UiTabsTrigger
            value="我的团队"
            :class="$style.tab"
            ><Users
              :size="14"
              :stroke-width="1.8"
            />我的团队</UiTabsTrigger
          >
          <UiTabsTrigger
            value="审批管理"
            :class="$style.tab"
            ><UserRoundCheck
              :size="14"
              :stroke-width="1.8"
            />审批管理<span :class="$style.badge">3</span></UiTabsTrigger
          >
        </UiTabsList>
      </div>
      <UiTabPanel value="我的出差">
        <section :class="$style.stats">
          <article
            v-for="item in stats"
            :key="item.label"
            :data-tone="item.tone"
          >
            <span
              ><component
                :is="item.icon"
                :size="14"
                :stroke-width="1.8"
              />{{ item.label }}</span
            ><strong>{{ item.value }}</strong>
          </article>
        </section>
        <div :class="$style.filters">
          <UiInput
            v-model="query"
            :class="$style.search"
            aria-label="搜索出差记录"
            placeholder="搜索单号/目的地/事由..."
          />
          <UiSelect
            v-model="status"
            :class="$style.select"
            :options="statusOptions"
            label="全部状态"
            size="md"
          />
          <UiInput
            v-model="startDate"
            :class="$style.date"
            type="date"
            aria-label="开始日期"
          /><span>至</span
          ><UiInput
            v-model="endDate"
            :class="$style.date"
            type="date"
            aria-label="结束日期"
          />
          <UiButton
            variant="primary"
            size="md"
            @click="formOpen = true"
            ><template #leading
              ><Plus
                :size="14"
                :stroke-width="1.8" /></template
            >新建出差申请</UiButton
          >
        </div>
        <TravelTable
          :rows="filteredRows"
          @view="openDetails"
          @withdraw="requestWithdraw"
          @reimburse="reimburse"
        />
      </UiTabPanel>
      <UiTabPanel value="我的团队"
        ><section :class="$style.secondary">
          <h2>团队出差日程</h2>
          <p>集中查看团队成员的目的地、时间与预算安排。</p>
          <TravelTable
            :rows="rows"
            @view="openDetails"
            @withdraw="requestWithdraw"
            @reimburse="reimburse"
          /></section
      ></UiTabPanel>
      <UiTabPanel value="审批管理"
        ><section :class="$style.secondary">
          <h2>待我审批</h2>
          <p>当前有 3 条团队出差申请等待处理。</p>
          <div :class="$style.approval">
            <strong>王小红 · 杭州客户拜访</strong><span>2026-03-28 至 2026-03-29 · 预算 2,800 元</span
            ><UiButton
              size="xs"
              variant="primary"
              @click="toast.success('出差申请已批准')"
              >批准</UiButton
            >
          </div>
        </section></UiTabPanel
      >
    </UiTabs>
  </main>

  <TravelFormDialog
    v-model:open="formOpen"
    @save="saveDraft"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '出差详情'"
    :description="activeRow ? `${activeRow.destination} · ${activeRow.status}` : undefined"
    size="sm"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>行程</dt>
        <dd>{{ activeRow.departureDate }} 至 {{ activeRow.returnDate }}</dd>
      </div>
      <div>
        <dt>交通方式</dt>
        <dd>{{ activeRow.transport }}</dd>
      </div>
      <div>
        <dt>预算</dt>
        <dd>{{ activeRow.budget.toLocaleString('zh-CN') }} 元</dd>
      </div>
      <div>
        <dt>出差事由</dt>
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
    title="撤回出差申请"
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
.tabBar {
  @apply mb-3 flex h-[56px] items-center border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[56px] rounded-none px-4 text-[12px];
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
.stats {
  @apply grid grid-cols-5 gap-3;
}
.stats article {
  @apply h-[72px] rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
  --stat-color: #0052cc;
}
.stats article[data-tone='purple'] {
  --stat-color: #6554c0;
}
.stats article[data-tone='orange'] {
  --stat-color: #ff8b00;
}
.stats article[data-tone='red'] {
  --stat-color: #ff5630;
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
.filters {
  @apply mt-4 flex items-center gap-3;
}
.filters > span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.search {
  @apply w-[285px];
}
.select {
  @apply w-[90px];
}
.date {
  @apply w-[132px];
}
.filters > :last-child {
  @apply ml-auto;
}
.secondary {
  @apply mt-4 rounded-lg border bg-white p-4;
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
