<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  BarChart3,
  CalendarDays,
  CheckCircle2,
  ClipboardCheck,
  Clock3,
  ListChecks,
  MapPin,
  Navigation,
  Settings2,
  TriangleAlert,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, toast } from '@/components/ui';
import { salesCheckinReferenceFixture } from '../model/sales-checkin.data';
import type { SalesVisit } from '../model/sales-checkin.types';

const visits = ref<SalesVisit[]>(cloneReferenceRows(salesCheckinReferenceFixture.rows));
const activeTab = ref('today');
const activeVisit = computed(() => visits.value.find((item) => item.status === '进行中'));
const planned = computed(() => visits.value.filter((item) => item.status === '待拜访'));
const completed = computed(() => visits.value.filter((item) => item.status === '已完成'));
const tabs = [
  { value: 'today', label: '今日打卡', icon: MapPin },
  { value: 'plan', label: '拜访计划', icon: CalendarDays },
  { value: 'analytics', label: '拜访分析', icon: BarChart3 },
  { value: 'rules', label: '打卡规则', icon: Settings2 },
];
function checkIn(visit?: SalesVisit) {
  const target = visit ?? planned.value[0];
  if (!target) {
    toast.info('今日拜访均已签到');
    return;
  }
  visits.value = visits.value.map((item) =>
    item.id === target.id
      ? {
          ...item,
          status: '进行中',
          time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' }),
        }
      : item.status === '进行中'
        ? { ...item, status: '已完成', duration: '42min' }
        : item,
  );
  toast.success(`已在 ${target.customer} 签到`);
}
function checkOut() {
  if (!activeVisit.value) return;
  visits.value = visits.value.map((item) =>
    item.id === activeVisit.value?.id
      ? { ...item, status: '已完成', duration: '46min', summary: '拜访已完成，跟进事项已记录。' }
      : item,
  );
  toast.success('签退成功，拜访总结已保存');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/sales/sales-checkin"
    data-page-label="销售打卡"
    data-page-module="销售业务"
    data-sales-checkin-page
    :data-tab="activeTab"
    :data-active-visit="activeVisit?.id || ''"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>销售业务</span><b>›</b><strong>销售打卡</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><MapPin
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>销售打卡 · 外勤管理</h1>
            <p>拜访签到 / 路线轨迹 / 拜访总结 — 让每一次客户拜访都可量化</p>
          </div>
        </div>
      </div>
      <div :class="$style.mapArt">
        <MapPin
          :size="100"
          :stroke-width="1.6"
        /><svg viewBox="0 0 340 90"><path d="M5 72 C70 17 130 86 180 52 S263 5 335 44" /></svg>
      </div>
      <UiButton
        :class="$style.heroAction"
        variant="secondary"
        size="sm"
        @click="checkIn()"
        ><template #leading><Navigation :size="14" /></template>立即签到</UiButton
      >
    </header>
    <section :class="$style.metrics">
      <article>
        <CalendarDays :size="17" />
        <div>
          <span>今日计划</span><strong>{{ visits.length }}</strong>
        </div>
      </article>
      <article>
        <CheckCircle2 :size="17" />
        <div>
          <span>已签到</span><strong>{{ completed.length + (activeVisit ? 1 : 0) }}</strong>
        </div>
      </article>
      <article class="success">
        <ClipboardCheck :size="17" />
        <div>
          <span>已完成</span><strong>{{ completed.length }}</strong>
        </div>
      </article>
      <article class="danger">
        <TriangleAlert :size="17" />
        <div><span>异常打卡</span><strong>0</strong></div>
      </article>
      <article>
        <Clock3 :size="17" />
        <div><span>拜访时长</span><strong>1h37m</strong></div>
      </article>
    </section>
    <nav :class="$style.tabs">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        type="button"
        :class="[$style.tab, activeTab === tab.value && $style.tabActive]"
        @click="activeTab = tab.value"
      >
        <component
          :is="tab.icon"
          :size="15"
        />{{ tab.label }}</button
      ><span>当前身份： <strong>张伟</strong> <em>销售代表</em></span>
    </nav>
    <section
      v-if="activeTab === 'today'"
      :class="$style.board"
    >
      <article :class="$style.plan">
        <header>
          <ListChecks :size="17" />今日待拜访 · 2026-05-26 <em>{{ planned.length }} 家</em
          ><button
            type="button"
            @click="toast.info('临时拜访表单已准备')"
          >
            ＋ 临时拜访
          </button>
        </header>
        <div
          v-for="visit in planned"
          :key="visit.id"
          :class="$style.visitCard"
        >
          <div>
            <h3>
              {{ visit.customer }} <i>{{ visit.priority }}</i> <span>{{ visit.purpose }}</span>
            </h3>
            <p><Clock3 :size="13" />{{ visit.time }}　<MapPin :size="13" />{{ visit.address }}</p>
            <small>🎯 {{ visit.customer }} 项目跟进</small>
          </div>
          <UiButton
            variant="primary"
            size="sm"
            @click="checkIn(visit)"
            ><template #leading><Navigation :size="13" /></template>签到</UiButton
          >
        </div>
      </article>
      <article :class="$style.flow">
        <header><BarChart3 :size="17" />今日打卡流水</header>
        <div
          v-if="activeVisit"
          :class="[$style.flowItem, $style.active]"
        >
          <i />
          <div>
            <strong>{{ activeVisit.time }}</strong
            ><b>{{ activeVisit.customer }}</b>
            <p>
              <span>{{ activeVisit.purpose }}</span> 距客户 {{ activeVisit.distance }}
            </p>
            <UiButton
              variant="primary"
              size="xs"
              @click="checkOut"
              ><template #leading><Navigation :size="12" /></template>签退并提交总结</UiButton
            >
          </div>
          <em>进行中</em>
        </div>
        <div
          v-for="visit in completed"
          :key="visit.id"
          :class="[$style.flowItem, $style.done]"
        >
          <i />
          <div>
            <strong>{{ visit.time.split('-')[0] }}</strong
            ><b>{{ visit.customer }}</b>
            <p>
              <span>{{ visit.purpose }}</span> 距客户 {{ visit.distance }} · 停留 {{ visit.duration }}
            </p>
            <small>📝 {{ visit.summary }}</small>
          </div>
          <em>已完成</em>
        </div>
      </article>
    </section>
    <section
      v-else
      :class="$style.placeholder"
    >
      <component
        :is="tabs.find((tab) => tab.value === activeTab)?.icon"
        :size="34"
      />
      <h2>{{ tabs.find((tab) => tab.value === activeTab)?.label }}</h2>
      <p>
        {{
          activeTab === 'plan'
            ? '本周客户拜访计划与路线安排'
            : activeTab === 'analytics'
              ? '拜访次数、时长与客户转化分析'
              : '外勤定位、签到范围和异常规则'
        }}
      </p>
    </section>
  </main>
</template>
<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #403294 0%, #5243aa 52%, #00a3bf 100%);
}
.heroCopy {
  @apply relative z-[1];
}
.hero nav {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 80%);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 88%);
}
.mapArt {
  @apply pointer-events-none absolute bottom-0 right-20 flex items-center opacity-80;
}
.mapArt svg {
  @apply h-[90px] w-[340px];
}
.mapArt path {
  fill: none;
  stroke: #ffe380;
  stroke-width: 3;
}
.heroAction {
  @apply absolute right-6 top-14;
  color: #403294;
}
.metrics {
  @apply mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex min-h-[82px] items-start gap-3 rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
  color: #6b778c;
}
.metrics article span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.metrics article strong {
  @apply mt-1.5 block text-[24px];
  color: var(--ui-text);
}
.metrics article:nth-child(2) strong {
  color: #0052cc;
}
.metrics :global(.success) strong {
  color: #00875a;
}
.metrics :global(.danger) strong {
  color: #ff5630;
}
.tabs {
  @apply mb-4 flex h-11 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.tab {
  @apply inline-flex h-9 items-center gap-1.5 rounded px-3 text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.tabActive {
  @apply text-white;
  background: #5243aa;
}
.tabs > span {
  @apply ml-auto mr-3 text-[11px];
  color: var(--ui-text-subtle);
}
.tabs > span strong {
  color: var(--ui-text);
}
.tabs em {
  @apply ml-1 rounded px-2 py-1 not-italic;
  color: #5243aa;
  background: #eae6ff;
}
.board {
  @apply grid grid-cols-[1.45fr_1fr] gap-4;
}
.plan,
.flow {
  @apply min-h-[315px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.plan > header,
.flow > header {
  @apply mb-3 flex items-center gap-2 text-[14px] font-semibold;
}
.plan > header em {
  @apply rounded px-2 py-0.5 text-[10px] not-italic;
  color: #5243aa;
  background: #eae6ff;
}
.plan > header button {
  @apply ml-auto text-[11px];
  color: var(--app-primary);
}
.visitCard {
  @apply mb-3 flex min-h-[88px] items-center justify-between rounded border px-4 py-3;
  border-color: var(--ui-border);
}
.visitCard h3 {
  @apply text-[13px] font-semibold;
}
.visitCard h3 i {
  @apply ml-1 rounded px-1.5 py-0.5 not-italic text-[10px] text-white;
  background: #ff8b00;
}
.visitCard h3 span {
  @apply ml-1 rounded px-2 py-0.5 text-[10px] font-normal;
  color: #5243aa;
  background: #eae6ff;
}
.visitCard p {
  @apply mt-2 flex items-center text-[11px];
  color: var(--ui-text-subtle);
}
.visitCard small {
  @apply mt-1.5 block text-[10px];
  color: var(--ui-text-muted);
}
.flowItem {
  @apply relative flex gap-4 border-l-2 pb-4 pl-5;
  border-color: #0052cc;
}
.flowItem > i {
  @apply absolute left-[-7px] top-0 h-3 w-3 rounded-full;
  background: #0052cc;
}
.flowItem div {
  @apply min-w-0 flex-1;
}
.flowItem strong,
.flowItem b {
  @apply block text-[12px];
}
.flowItem b {
  @apply mt-1 text-[13px];
}
.flowItem p {
  @apply my-2 text-[11px];
  color: var(--ui-text-subtle);
}
.flowItem p span {
  @apply rounded px-2 py-1;
  color: #5243aa;
  background: #eae6ff;
}
.flowItem small {
  @apply block text-[10px] leading-5;
  color: var(--ui-text-muted);
}
.flowItem > em {
  @apply h-fit rounded px-2 py-1 text-[10px] not-italic;
  color: #0052cc;
  background: #deebff;
}
.flowItem.done {
  border-color: #36b37e;
}
.flowItem.done > i {
  background: #36b37e;
}
.flowItem.done > em {
  color: #00875a;
  background: #e3fcef;
}
.placeholder {
  @apply flex min-h-[315px] flex-col items-center justify-center rounded border bg-white;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.placeholder h2 {
  @apply mt-3 text-[17px] font-semibold;
  color: var(--ui-text);
}
.placeholder p {
  @apply mt-2 text-[12px];
}
</style>
