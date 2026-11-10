<script setup lang="ts">
import { ref } from 'vue';
import { BarChart3, Filter, LineChart, Sparkles, TriangleAlert } from 'lucide-vue-next';
import { UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { AnalyticsPeriod, AnalyticsPerspective, AnalyticsView } from '../model/service-analytics.types';

const period = ref<AnalyticsPeriod>('近30天');
const view = ref<AnalyticsView>('staff');
const region = ref('全部地区');
const team = ref('全部团队');
const charge = ref('全部计费');
const activeTypes = ref<string[]>([]);
const urgency = ref('');
const perspective = ref<AnalyticsPerspective>('售后主管视角');
const periods: readonly AnalyticsPeriod[] = ['今日', '近7天', '近30天', '近90天', '本季', '全部'];
const types = ['售后报修', '备件更换', '安装调试', '客户投诉', '巡检保养', '有偿服务', '远程支持'];
const urgencies = ['紧急', '高', '中', '低'];
const perspectives: readonly AnalyticsPerspective[] = ['售后主管视角', '工程师视角', '财务视角'];
const tabs: readonly { value: AnalyticsView; label: string }[] = [
  { value: 'staff', label: '按服务人员分析' },
  { value: 'customer', label: '按客户分析' },
  { value: 'type', label: '按工单类型分析' },
  { value: 'month', label: '按时间(月)分析' },
  { value: 'region', label: '按区域分析' },
];
const regionOptions: readonly UiSelectOption[] = [
  { value: '全部地区', label: '全部地区' },
  { value: '华东', label: '华东' },
  { value: '华南', label: '华南' },
  { value: '华北', label: '华北' },
];
const teamOptions: readonly UiSelectOption[] = [
  { value: '全部团队', label: '全部团队' },
  { value: '技术一组', label: '技术一组' },
  { value: '技术二组', label: '技术二组' },
];
const chargeOptions: readonly UiSelectOption[] = [
  { value: '全部计费', label: '全部计费' },
  { value: '免费', label: '免费' },
  { value: '有偿', label: '有偿' },
];
function toggleType(type: string) {
  activeTypes.value = activeTypes.value.includes(type)
    ? activeTypes.value.filter((item) => item !== type)
    : [...activeTypes.value, type];
}
function reset() {
  period.value = '近30天';
  region.value = '全部地区';
  team.value = '全部团队';
  charge.value = '全部计费';
  activeTypes.value = [];
  urgency.value = '';
  perspective.value = '售后主管视角';
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="dashboard"
    data-page-route="/service/service-analytics"
    data-page-label="服务分析"
    data-page-module="服务管理"
    data-service-analytics-page
    :data-period="period"
    :data-view="view"
    :data-types="activeTypes.join(',')"
    :data-urgency="urgency"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>服务管理</span><b>›</b><strong>服务分析</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"><LineChart :size="23" /></span>
          <div>
            <h1>服务分析</h1>
            <p>按人 / 客户 / 类型 / 时间 / 区域多维透视，含期间对比、AI 经营洞察与质量预警</p>
          </div>
        </div>
      </div>
      <BarChart3
        :class="$style.heroArt"
        :size="126"
      /><span :class="$style.heroBadge">服务工单</span>
    </header>
    <section :class="$style.filters">
      <div :class="$style.filterMain">
        <Filter :size="16" /><span>时间</span
        ><button
          v-for="item in periods"
          :key="item"
          type="button"
          :class="[$style.pill, period === item && $style.pillActive]"
          @click="period = item"
        >
          {{ item }}</button
        ><i /><UiSelect
          v-model="region"
          label="地区"
          :options="regionOptions"
          size="sm"
        /><UiSelect
          v-model="team"
          label="团队"
          :options="teamOptions"
          size="sm"
        /><UiSelect
          v-model="charge"
          label="计费方式"
          :options="chargeOptions"
          size="sm"
        /><span>类型</span
        ><button
          v-for="item in types"
          :key="item"
          type="button"
          :class="[$style.typeButton, activeTypes.includes(item) && $style.typeActive]"
          @click="toggleType(item)"
        >
          {{ item }}
        </button>
      </div>
      <div :class="$style.filterSecondary">
        <span>紧急</span
        ><button
          v-for="item in urgencies"
          :key="item"
          type="button"
          :class="[$style.urgencyButton, urgency === item && $style.urgencyActive]"
          @click="urgency = urgency === item ? '' : item"
        >
          {{ item }}
        </button>
        <div>
          <button
            v-for="item in perspectives"
            :key="item"
            type="button"
            :class="[$style.perspective, perspective === item && $style.perspectiveActive]"
            @click="perspective = item"
          >
            {{ item }}</button
          ><button
            type="button"
            :class="$style.reset"
            @click="reset"
          >
            × 重置
          </button>
        </div>
      </div>
    </section>
    <section :class="$style.metrics">
      <article><span>工单总量</span><strong>0</strong><small>完成 0 · 在办 0 · 超时 0</small></article>
      <article class="success"><span>SLA 达标率</span><strong>100%</strong><small>超时 0 单</small></article>
      <article><span>一次解决率</span><strong>0%</strong><small>单次处理即解决占比</small></article>
      <article><span>平均响应 / 处理</span><strong>0分 / 0h</strong><small>受理与处理总时长</small></article>
      <article class="purple"><span>客户满意度</span><strong>—</strong><small>客户/回访 评分（5 分制）</small></article>
      <article class="success"><span>服务收入</span><strong>¥0</strong><small>备件成本 ¥0</small></article>
      <article class="success"><span>服务毛利</span><strong>¥0</strong><small>无收入</small></article>
      <article class="success"><span>问题解决率</span><strong>0%</strong><small>客户/回访 确认已解决</small></article>
    </section>
    <section :class="$style.analysis">
      <header>
        <nav>
          <button
            v-for="tab in tabs"
            :key="tab.value"
            type="button"
            :class="view === tab.value && $style.tabActive"
            @click="view = tab.value"
          >
            {{ tab.label }}
          </button>
        </nav>
        <span>共 0 组</span>
      </header>
      <div :class="$style.empty">当前筛选下无数据</div>
    </section>
    <section :class="$style.insights">
      <article>
        <h2><Sparkles :size="17" />AI 服务经营分析</h2>
        <p>本月服务工单 0 单，较上月（0 单）增长 0%。</p>
      </article>
      <article>
        <h2><TriangleAlert :size="17" />AI 质量预警</h2>
        <p>当前没有需要预警的服务质量问题。</p>
      </article>
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
  background: linear-gradient(108deg, #102a5c 0%, #0747a6 56%, #00b8d9 100%);
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
.heroArt {
  @apply absolute right-[280px] top-[-10px] opacity-35;
}
.heroBadge {
  @apply absolute right-6 top-[59px] rounded border px-3 py-2 text-[12px];
  border-color: rgb(255 255 255 / 35%);
  background: rgb(255 255 255 / 12%);
}
.filters {
  @apply mb-4 rounded border bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.filterMain,
.filterSecondary {
  @apply flex items-center gap-1.5;
}
.filterMain > span,
.filterSecondary > span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.filterMain > i {
  @apply mx-1 h-7 border-l;
  border-color: var(--ui-border);
}
.pill,
.typeButton,
.urgencyButton {
  @apply rounded border bg-white px-2.5 py-1.5 text-[11px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.pill {
  @apply rounded-full;
}
.pillActive {
  @apply border-transparent text-white;
  background: var(--app-primary);
}
.typeActive {
  color: #0052cc;
  border-color: #4c9aff;
  background: #deebff;
}
.filterSecondary {
  @apply mt-3;
}
.filterSecondary > div {
  @apply ml-auto flex gap-2;
}
.urgencyActive {
  color: #de350b;
  border-color: #ff8f73;
  background: #ffebe6;
}
.perspective {
  @apply rounded border px-3 py-1.5 text-[11px];
  color: #5243aa;
  border-color: #6554c0;
}
.perspectiveActive {
  @apply text-white;
  background: #6554c0;
}
.reset {
  @apply px-2 py-1.5 text-[11px];
  color: var(--ui-text-subtle);
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply min-h-[102px] rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.metrics span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[25px];
  color: #0052cc;
}
.metrics small {
  @apply mt-1.5 block text-[11px];
  color: #97a0af;
}
.metrics :global(.success) strong {
  color: #00875a;
}
.metrics :global(.purple) strong {
  color: #6554c0;
}
.analysis {
  @apply mb-4 min-h-[185px] rounded border bg-white;
  border-color: var(--ui-border);
}
.analysis header {
  @apply flex h-11 items-end justify-between border-b px-2;
  border-color: var(--ui-border);
}
.analysis nav {
  @apply flex h-full items-end;
}
.analysis nav button {
  @apply h-full border-b-2 border-transparent px-4 text-[12px];
  color: var(--ui-text-muted);
}
.analysis nav .tabActive {
  @apply font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.analysis header span {
  @apply pb-3 pr-3 text-[11px];
  color: #97a0af;
}
.empty {
  @apply flex h-[138px] items-center justify-center text-[12px];
  color: #97a0af;
}
.insights {
  @apply grid grid-cols-2 gap-3;
}
.insights article {
  @apply min-h-[82px] rounded border p-4;
  border-color: #e9e6ff;
  background: #faf9ff;
}
.insights h2 {
  @apply flex items-center gap-2 text-[13px] font-semibold;
  color: #5243aa;
}
.insights p {
  @apply mt-3 text-[12px];
  color: var(--ui-text-muted);
}
.insights article:last-child {
  @apply border-transparent bg-transparent;
}
.insights article:last-child h2 {
  color: #de350b;
}
</style>
