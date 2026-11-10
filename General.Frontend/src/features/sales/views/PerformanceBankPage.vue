<script setup lang="ts">
import { computed, ref } from 'vue';
import { ArrowDownLeft, BarChart3, Building2, CalendarDays, Landmark, Medal, UserRound } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { performanceBankReferenceFixture } from '../model/performance-bank.data';
import type { PerformancePeriod, PerformanceTab } from '../model/performance-bank.types';

const rows = cloneReferenceRows(performanceBankReferenceFixture.rows);
const scope = ref('all');
const activeTab = ref<PerformanceTab>('mine');
const period = ref<PerformancePeriod>('本年');
const tabs: readonly { value: PerformanceTab; label: string; icon: typeof Landmark }[] = [
  { value: 'mine', label: '我的业绩银行', icon: Landmark },
  { value: 'flow', label: '业绩流水', icon: Building2 },
  { value: 'rebook', label: 'Rebook 记录', icon: ArrowDownLeft },
  { value: 'confirm', label: '销售业绩确认', icon: Medal },
  { value: 'analysis', label: '业绩分析', icon: BarChart3 },
];
const periods: readonly PerformancePeriod[] = ['全部', '本年', 'Q1', 'Q2', 'Q3', 'Q4', '本月'];
const filteredRows = computed(() =>
  period.value === '全部' ? rows : rows.filter((row) => row.date.startsWith('2026')),
);
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="dashboard"
    data-page-route="/sales/performance-bank"
    data-page-label="业绩银行"
    data-page-module="销售业务"
    data-performance-bank-page
    :data-tab="activeTab"
    :data-period="period"
    :data-result-count="filteredRows.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>销售业务</span><b>›</b><strong>业绩银行</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Landmark
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>业绩银行</h1>
            <p>销售业绩流水、Rebook分配与确认审批，沉淀团队成果</p>
          </div>
        </div>
      </div>
      <div :class="$style.medalArt">
        <Medal
          :size="124"
          :stroke-width="1.2"
        /><span>★</span>
      </div>
    </header>
    <section :class="$style.scopeBar">
      <button
        v-for="item in [
          ['all', '所有'],
          ['mine', '我负责的'],
          ['team', '下属负责的'],
          ['followed', '我关注的'],
        ]"
        :key="item[0]"
        type="button"
        :class="[$style.scopeButton, scope === item[0] && $style.scopeActive]"
        @click="scope = item[0]"
      >
        <UserRound :size="15" />{{ item[1] }}
      </button>
    </section>
    <section :class="$style.metrics">
      <article><span>累计确认业绩</span><strong>¥1,742,161.60</strong></article>
      <article :class="$style.green"><span>累计毛利</span><strong>¥614,161.60</strong></article>
      <article><span>业绩笔数</span><strong>8</strong></article>
      <article :class="$style.orange"><span>待审批</span><strong>1</strong></article>
    </section>
    <section :class="$style.workspace">
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
            :size="16"
          />{{ tab.label }}
        </button>
      </nav>
      <div :class="$style.controls">
        <button
          type="button"
          :class="$style.person"
        >
          <UserRound :size="15" />张经理 · 华东销售一部
        </button>
        <div :class="$style.periods">
          <CalendarDays :size="15" />
          <button
            v-for="item in periods"
            :key="item"
            type="button"
            :class="period === item && $style.periodActive"
            @click="period = item"
          >
            {{ item }}
          </button>
        </div>
      </div>
      <div :class="$style.owner">
        <span :class="$style.avatar">张</span>
        <div><strong>张经理的业绩银行</strong><small>华东销售一部 · 销售总监</small></div>
      </div>
    </section>
    <section :class="$style.detailMetrics">
      <article><span>业绩净额</span><strong>¥949,206.40</strong><small>共 3 条流水</small></article>
      <article><span>原始确认</span><strong>¥1,082,212.00</strong><small>2 条</small></article>
      <article :class="$style.green"><span>业绩分入</span><strong>+¥0.00</strong><small>0 条</small></article>
      <article :class="$style.red"><span>业绩分出</span><strong>-¥133,005.60</strong><small>1 条</small></article>
    </section>
    <section :class="$style.chartPanel">
      <header><BarChart3 :size="18" />月度业绩趋势 (2026)</header>
      <svg
        viewBox="0 0 1000 220"
        role="img"
        aria-label="月度业绩趋势"
      >
        <g :class="$style.grid">
          <line
            v-for="y in [25, 70, 115, 160, 205]"
            :key="y"
            x1="55"
            :y1="y"
            x2="970"
            :y2="y"
          />
        </g>
        <polyline
          points="55,187 130,160 205,198 280,120 355,178 430,95 505,152 580,78 655,134 730,84 805,121 880,65 950,101"
        />
        <path
          d="M55 205 L55 187 L130 160 L205 198 L280 120 L355 178 L430 95 L505 152 L580 78 L655 134 L730 84 L805 121 L880 65 L950 101 L950 205 Z"
        />
      </svg>
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
  background: linear-gradient(108deg, #403294 0%, #5243aa 58%, #6554c0 100%);
}
.hero::before {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: radial-gradient(circle at 18% 8%, rgb(255 255 255 / 15%) 0 2px, transparent 3px);
  background-size: 120px 120px;
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
.medalArt {
  @apply pointer-events-none absolute right-24 top-0 flex items-center opacity-60;
}
.medalArt span {
  @apply absolute right-2 top-5 text-[47px] text-[#FFFAE6];
}
.scopeBar {
  @apply mb-4 flex min-h-10 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.scopeButton {
  @apply inline-flex min-h-8 items-center gap-1.5 rounded px-3 text-[12px];
  color: var(--ui-text-muted);
}
.scopeButton:hover {
  background: var(--ui-surface-hover);
}
.scopeActive {
  @apply text-white;
  background: var(--app-primary);
}
.metrics,
.detailMetrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article,
.detailMetrics article {
  @apply min-h-[102px] rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.metrics span,
.detailMetrics span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.metrics strong,
.detailMetrics strong {
  @apply mt-2 block text-[27px] leading-7;
  color: #0052cc;
}
.metrics .green strong,
.detailMetrics .green strong {
  color: #00875a;
}
.metrics .orange strong {
  color: #ff8b00;
}
.workspace {
  @apply mb-4 rounded border bg-white px-5;
  border-color: var(--ui-border);
}
.tabs {
  @apply flex items-center gap-1 border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply inline-flex min-h-12 items-center gap-1.5 border-b-2 border-transparent px-4 text-[13px];
  color: var(--ui-text-muted);
}
.tabActive {
  @apply font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.controls {
  @apply flex items-center gap-3 py-3;
}
.person,
.periods {
  @apply inline-flex h-9 items-center gap-1.5 rounded border px-3 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.periods {
  @apply gap-2;
}
.periods button {
  @apply rounded px-2 py-1;
}
.periodActive {
  @apply text-white;
  background: var(--app-primary);
}
.owner {
  @apply flex items-center gap-3 rounded px-3 py-3;
  background: #e9e6ff;
}
.avatar {
  @apply flex h-10 w-10 items-center justify-center rounded-full text-[17px] font-semibold text-white;
  background: #0052cc;
}
.owner strong {
  @apply block text-[14px];
}
.owner small {
  @apply mt-1 block text-[11px];
  color: var(--ui-text-subtle);
}
.detailMetrics {
  @apply mb-4;
}
.detailMetrics article small {
  @apply mt-1.5 block text-[11px];
  color: #97a0af;
}
.detailMetrics .red strong {
  color: #de350b;
}
.chartPanel {
  @apply min-h-[300px] rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.chartPanel header {
  @apply flex items-center gap-2 text-[15px] font-semibold;
  color: #0052cc;
}
.chartPanel svg {
  @apply mt-3 h-[230px] w-full;
}
.grid line {
  stroke: #ebecf0;
  stroke-dasharray: 3 4;
}
.chartPanel polyline {
  fill: none;
  stroke: #ff8b00;
  stroke-width: 3;
  stroke-linecap: round;
  stroke-linejoin: round;
}
.chartPanel path {
  fill: rgb(255 139 0 / 12%);
}
</style>
