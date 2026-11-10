<script setup lang="ts">
import { ChevronLeft, ChevronRight, Plus } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { toast, UiButton, UiDialog } from '@/components/ui';

type ViewMode = '日' | '周' | '月';
interface CalendarEvent {
  id: string;
  title: string;
  date: string;
  time: string;
  owner: string;
  type: string;
  color: 'blue' | 'green' | 'orange' | 'purple';
}

const viewMode = ref<ViewMode>('周');
const cursor = ref(new Date('2026-07-13T00:00:00'));
const createOpen = ref(false);
const draftTitle = ref('');
const draftDate = ref('2026-07-13');
const draftTime = ref('10:00');
const events = ref<CalendarEvent[]>([
  { id: 'e1', title: '销售周会', date: '2026-07-13', time: '10:00', owner: '张经理', type: '会议', color: 'blue' },
  { id: 'e2', title: '库存盘点启动', date: '2026-07-14', time: '14:00', owner: '刘仓管', type: '业务', color: 'green' },
  { id: 'e3', title: '供应商对账', date: '2026-07-15', time: '09:30', owner: '陈会计', type: '财务', color: 'orange' },
  {
    id: 'e4',
    title: '新员工入职培训',
    date: '2026-07-16',
    time: '13:30',
    owner: '人事',
    type: '人事',
    color: 'purple',
  },
  {
    id: 'e5',
    title: '项目评审：华能DCS',
    date: '2026-07-17',
    time: '15:00',
    owner: '王工',
    type: '项目',
    color: 'blue',
  },
]);
const weekStart = computed(() => {
  const d = new Date(cursor.value);
  const day = d.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
});

const weekDays = computed(() =>
  Array.from({ length: 7 }, (_, index) => {
    const d = new Date(weekStart.value);
    d.setDate(weekStart.value.getDate() + index);
    return d;
  }),
);

const rangeLabel = computed(() => {
  if (viewMode.value === '日') {
    return cursor.value.toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' });
  }
  if (viewMode.value === '周') {
    const start = weekDays.value[0]!;
    const end = weekDays.value[6]!;
    const weekNo = Math.ceil(((start.getTime() - new Date(start.getFullYear(), 0, 1).getTime()) / 86400000 + 1) / 7);
    return `${start.getFullYear()}年${start.getMonth() + 1}月${start.getDate()}日 - ${end.getMonth() + 1}月${end.getDate()}日（第 ${weekNo} 周）`;
  }
  return `${cursor.value.getFullYear()}年${cursor.value.getMonth() + 1}月`;
});

const monthCells = computed(() => {
  const year = cursor.value.getFullYear();
  const month = cursor.value.getMonth();
  const first = new Date(year, month, 1);
  const startOffset = (first.getDay() + 6) % 7;
  const start = new Date(year, month, 1 - startOffset);
  return Array.from({ length: 42 }, (_, index) => {
    const d = new Date(start);
    d.setDate(start.getDate() + index);
    return d;
  });
});

function fmt(d: Date) {
  return d.toISOString().slice(0, 10);
}

function eventsOn(date: string) {
  return events.value.filter((item) => item.date === date);
}

function notify(message: string, tone: 'success' | 'warning' = 'success') {
  toast[tone](message);
}

function shift(delta: number) {
  const next = new Date(cursor.value);
  if (viewMode.value === '日') next.setDate(next.getDate() + delta);
  else if (viewMode.value === '周') next.setDate(next.getDate() + delta * 7);
  else next.setMonth(next.getMonth() + delta);
  cursor.value = next;
}

function goToday() {
  cursor.value = new Date('2026-07-13T00:00:00');
  notify('已回到今天');
}

function createEvent() {
  if (!draftTitle.value.trim()) {
    notify('请输入日程标题', 'warning');
    return;
  }
  events.value = [
    {
      id: `e-${Date.now()}`,
      title: draftTitle.value.trim(),
      date: draftDate.value,
      time: draftTime.value,
      owner: '张经理',
      type: '日程',
      color: 'blue',
    },
    ...events.value,
  ];
  createOpen.value = false;
  draftTitle.value = '';
  notify('已创建日程');
}

const dayHours = Array.from({ length: 11 }, (_, i) => 8 + i);
</script>

<template>
  <div
    :class="$style.page"
    data-module-page
    data-page-label="公司日历"
    data-page-module="公司日历"
    data-page-layout="calendar"
  >
    <header :class="$style.toolbar">
      <div :class="$style.navGroup">
        <UiButton
          size="sm"
          variant="secondary"
          @click="goToday"
          >今天</UiButton
        >
        <button
          type="button"
          :class="$style.iconBtn"
          aria-label="上一页"
          @click="shift(-1)"
        >
          <ChevronLeft
            :size="16"
            :stroke-width="1.8"
          />
        </button>
        <button
          type="button"
          :class="$style.iconBtn"
          aria-label="下一页"
          @click="shift(1)"
        >
          <ChevronRight
            :size="16"
            :stroke-width="1.8"
          />
        </button>
        <h1 :class="$style.range">{{ rangeLabel }}</h1>
      </div>
      <div :class="$style.rightGroup">
        <div :class="$style.viewSwitch">
          <button
            v-for="mode in ['日', '周', '月']"
            :key="mode"
            type="button"
            :class="[$style.viewBtn, viewMode === mode && $style.viewActive]"
            @click="viewMode = mode as ViewMode"
          >
            {{ mode }}
          </button>
        </div>
        <UiButton
          size="sm"
          variant="primary"
          @click="createOpen = true"
        >
          <template #leading>
            <Plus
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          创建日程
        </UiButton>
      </div>
    </header>

    <div
      v-if="viewMode === '月'"
      :class="$style.monthGrid"
      data-calendar-view="month"
    >
      <div
        v-for="name in ['一', '二', '三', '四', '五', '六', '日']"
        :key="name"
        :class="$style.weekday"
      >
        {{ name }}
      </div>
      <button
        v-for="day in monthCells"
        :key="fmt(day)"
        type="button"
        :class="[
          $style.monthCell,
          day.getMonth() !== cursor.getMonth() && $style.mutedDay,
          fmt(day) === '2026-07-13' && $style.today,
        ]"
        @click="
          cursor = day;
          viewMode = '日';
        "
      >
        <span>{{ day.getDate() }}</span>
        <small
          v-for="event in eventsOn(fmt(day)).slice(0, 2)"
          :key="event.id"
          :class="[$style.eventChip, $style[event.color]]"
          >{{ event.title }}</small
        >
      </button>
    </div>

    <div
      v-else-if="viewMode === '周'"
      :class="$style.weekGrid"
      data-calendar-view="week"
    >
      <div :class="$style.weekHead" />
      <div
        v-for="day in weekDays"
        :key="fmt(day)"
        :class="[$style.weekHead, fmt(day) === '2026-07-13' && $style.today]"
      >
        <strong>{{ ['日', '一', '二', '三', '四', '五', '六'][day.getDay()] }}</strong>
        <span>{{ day.getMonth() + 1 }}/{{ day.getDate() }}</span>
      </div>
      <template
        v-for="hour in dayHours"
        :key="hour"
      >
        <div :class="$style.hourLabel">{{ String(hour).padStart(2, '0') }}:00</div>
        <div
          v-for="day in weekDays"
          :key="`${fmt(day)}-${hour}`"
          :class="$style.hourCell"
        >
          <button
            v-for="event in eventsOn(fmt(day)).filter((item) => Number(item.time.slice(0, 2)) === hour)"
            :key="event.id"
            type="button"
            :class="[$style.eventBlock, $style[event.color]]"
            @click="notify(`已打开：${event.title}`)"
          >
            <strong>{{ event.time }} {{ event.title }}</strong>
            <small>{{ event.owner }}</small>
          </button>
        </div>
      </template>
    </div>

    <div
      v-else
      :class="$style.dayView"
      data-calendar-view="day"
    >
      <div
        v-for="hour in dayHours"
        :key="hour"
        :class="$style.dayRow"
      >
        <div :class="$style.hourLabel">{{ String(hour).padStart(2, '0') }}:00</div>
        <div :class="$style.daySlot">
          <button
            v-for="event in eventsOn(fmt(cursor)).filter((item) => Number(item.time.slice(0, 2)) === hour)"
            :key="event.id"
            type="button"
            :class="[$style.eventBlock, $style[event.color]]"
            @click="notify(`已打开：${event.title}`)"
          >
            <strong>{{ event.time }} {{ event.title }}</strong>
            <small>{{ event.type }} · {{ event.owner }}</small>
          </button>
        </div>
      </div>
    </div>

    <UiDialog
      v-model:open="createOpen"
      title="创建日程"
      description="在公司日历中新增本地演示日程"
      size="md"
    >
      <div :class="$style.form">
        <label
          ><span>标题</span
          ><input
            v-model="draftTitle"
            placeholder="请输入日程标题"
        /></label>
        <label
          ><span>日期</span
          ><input
            v-model="draftDate"
            type="date"
        /></label>
        <label
          ><span>时间</span
          ><input
            v-model="draftTime"
            type="time"
        /></label>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="createOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="createEvent"
          >保存</UiButton
        >
      </template>
    </UiDialog>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply flex h-full min-h-0 flex-col gap-3 overflow-auto p-4;
  background: #fafbfc;
}
.toolbar {
  @apply flex flex-wrap items-center justify-between gap-3 rounded-lg border bg-white px-3 py-2;
  border-color: var(--ui-border);
}
.navGroup,
.rightGroup {
  @apply flex items-center gap-2;
}
.iconBtn {
  @apply inline-flex h-8 w-8 items-center justify-center rounded border bg-white;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}
.range {
  @apply m-0 text-[16px] font-semibold;
  color: var(--ui-text);
}
.viewSwitch {
  @apply inline-flex rounded border p-0.5;
  border-color: var(--ui-border);
}
.viewBtn {
  @apply rounded px-3 py-1 text-[12px];
  color: var(--ui-text-muted);
  background: transparent;
}
.viewActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.monthGrid {
  @apply grid flex-1 grid-cols-7 gap-px overflow-auto rounded-lg border bg-[#dfe1e6];
  border-color: var(--ui-border);
  min-height: 520px;
}
.weekday {
  @apply bg-[#f4f5f7] px-2 py-2 text-center text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.monthCell {
  @apply flex min-h-[88px] flex-col items-start gap-1 bg-white p-2 text-left;
  color: var(--ui-text);
}
.mutedDay {
  color: #a5adba;
  background: #fafbfc;
}
.today {
  box-shadow: inset 0 0 0 2px var(--app-primary);
}
.eventChip {
  @apply max-w-full truncate rounded px-1.5 py-0.5 text-[11px];
}
.weekGrid {
  @apply grid flex-1 overflow-auto rounded-lg border bg-white;
  border-color: var(--ui-border);
  grid-template-columns: 64px repeat(7, minmax(120px, 1fr));
  min-height: 520px;
}
.weekHead {
  @apply sticky top-0 z-[1] border-b bg-[#f4f5f7] px-2 py-2 text-center text-[12px];
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}
.weekHead strong {
  @apply block text-[13px];
  color: var(--ui-text);
}
.hourLabel {
  @apply border-b border-r px-2 py-3 text-right text-[11px];
  border-color: var(--ui-border-subtle);
  color: var(--ui-text-subtle);
}
.hourCell,
.daySlot {
  @apply min-h-[56px] border-b border-r p-1;
  border-color: var(--ui-border-subtle);
}
.eventBlock {
  @apply mb-1 flex w-full flex-col items-start rounded px-2 py-1 text-left;
}
.eventBlock strong {
  @apply text-[12px];
}
.eventBlock small {
  @apply text-[11px] opacity-80;
}
.blue {
  background: #deebff;
  color: #0747a6;
}
.green {
  background: #e3fcef;
  color: #006644;
}
.orange {
  background: #fffae6;
  color: #974f0c;
}
.purple {
  background: #eae6ff;
  color: #403294;
}
.dayView {
  @apply overflow-auto rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.dayRow {
  @apply grid grid-cols-[64px_1fr];
}
.form {
  @apply grid gap-3;
}
.form label {
  @apply flex flex-col gap-1 text-[12px];
  color: var(--ui-text-muted);
}
.form input {
  @apply h-9 rounded border px-3 text-[13px];
  border-color: var(--ui-border);
  color: var(--ui-text);
}
</style>
