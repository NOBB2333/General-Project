<script setup lang="ts">
import { BarChart3, Building2, CalendarDays, Download, ListFilter, Star } from 'lucide-vue-next';
import { ref } from 'vue';
import { UiButton, UiCheckbox, UiSelect } from '@/components/ui';
import { salesBoardPersonalMetrics } from '../../model/sales-board.data';
import type { SalesBoardPersonalMetric } from '../../model/sales-board.types';

const emit = defineEmits<{
  inspect: [title: string, description: string, facts: string[]];
}>();

const salesperson = ref('张经理');
const period = ref('year');
const department = ref('all');
const customer = ref('all');
const product = ref('all');
const measure = ref('orders');
const includeVoided = ref(false);

const salespersonOptions = [
  { value: '张经理', label: '张经理 · 销售总监' },
  { value: '陈销售', label: '陈销售 · 高级销售' },
  { value: '王磊', label: '王磊 · 销售经理' },
  { value: '李明', label: '李明 · 销售代表' },
];
const periodOptions = [
  { value: 'month', label: '本月' },
  { value: 'quarter', label: '本季度' },
  { value: 'year', label: '本年' },
  { value: 'custom', label: '自定义' },
];
const departmentOptions = [
  { value: 'all', label: '全部部门' },
  { value: 'east-one', label: '华东销售一部' },
  { value: 'north', label: '华北销售部' },
  { value: 'south', label: '华南销售部' },
];
const customerOptions = [
  { value: 'all', label: '全部客户' },
  { value: 'a', label: 'A 类' },
  { value: 'b', label: 'B 类' },
  { value: 'c', label: 'C 类' },
];
const productOptions = [
  { value: 'all', label: '全部产品线' },
  { value: 'standard', label: '标准产品' },
  { value: 'custom', label: '大客户定制' },
  { value: 'service', label: '服务/方案' },
];
const measureOptions = [
  { value: 'orders', label: '按订单' },
  { value: 'income', label: '按收入' },
  { value: 'collections', label: '按回款' },
  { value: 'profit', label: '按毛利' },
];

function inspectMetric(metric: SalesBoardPersonalMetric) {
  emit('inspect', metric.label, `张经理 · ${metric.detail}`, [
    `当前数值：${metric.value}`,
    `同比变化：${metric.trend >= 0 ? '增长' : '下降'} ${Math.abs(metric.trend)}%`,
    `当前口径：${measureOptions.find((item) => item.value === measure.value)?.label ?? '按订单'}`,
  ]);
}

function showReport() {
  emit('inspect', '销售个人报表', '张经理 · 本年', [
    '订单金额：¥365万',
    '年度回款：¥231万',
    '销售毛利：¥108万',
    '综合健康度：86 分',
  ]);
}
</script>

<template>
  <section
    :class="$style.section"
    data-board-section="personal"
    :data-personal-salesperson="salesperson"
    :data-personal-period="period"
    :data-personal-measure="measure"
  >
    <div :class="$style.filters">
      <ListFilter
        :size="16"
        :stroke-width="1.8"
      />
      <strong>筛选：</strong>
      <UiSelect
        v-model="salesperson"
        :options="salespersonOptions"
        label="销售人员"
        :class="$style.salespersonSelect"
      />
      <UiSelect
        v-model="period"
        :options="periodOptions"
        label="统计周期"
      />
      <UiSelect
        v-model="department"
        :options="departmentOptions"
        label="部门"
      />
      <UiSelect
        v-model="customer"
        :options="customerOptions"
        label="客户分层"
      />
      <UiSelect
        v-model="product"
        :options="productOptions"
        label="产品线"
      />
      <UiSelect
        v-model="measure"
        :options="measureOptions"
        label="数据口径"
      />
      <UiCheckbox
        v-model="includeVoided"
        label="含已作废"
      />
      <UiButton
        variant="secondary"
        size="sm"
        @click="showReport"
      >
        <Download :size="14" />
        导出报表
      </UiButton>
      <UiButton
        variant="secondary"
        size="sm"
        @click="showReport"
      >
        查看明细
      </UiButton>
    </div>

    <article :class="$style.profileHero">
      <div :class="$style.identity">
        <span :class="$style.avatar">张</span>
        <div>
          <div :class="$style.nameLine">
            <h2>张经理</h2>
            <span>销售总监</span>
          </div>
          <p><Building2 :size="13" />华东销售一部 · 销售总监</p>
          <p><CalendarDays :size="13" />入职 2024-01-15 · 司龄 27 个月</p>
          <p>上海 · 江苏 · 浙江</p>
          <p>电力自动化 · 冶金 · 石化</p>
          <div :class="$style.badges"><span>本月销冠</span><span>Q1 突破奖</span><span>客户口碑王</span></div>
        </div>
      </div>
      <dl :class="$style.profileStats">
        <div>
          <dt>负责客户</dt>
          <dd>38<small>家</small></dd>
        </div>
        <div>
          <dt>在跑商机</dt>
          <dd>21<small>个</small></dd>
        </div>
        <div>
          <dt>本月新增</dt>
          <dd>9<small>家</small></dd>
        </div>
        <div>
          <dt>团队排名</dt>
          <dd>2<small>位</small></dd>
        </div>
        <div>
          <dt>勤奋指数</dt>
          <dd>92<small>分</small></dd>
        </div>
        <div>
          <dt>客户口碑</dt>
          <dd>4.7<small>/5</small></dd>
        </div>
      </dl>
      <div :class="$style.healthCard">
        <div :class="$style.healthTop">
          <span :class="$style.score">86<small>/100 优秀</small></span>
          <div>
            <h3><Star :size="15" />销售健康度评分</h3>
            <p>业绩完成度 <strong>92</strong></p>
            <p>回款质量 <strong>78</strong></p>
            <p>商机推进 <strong>88</strong></p>
          </div>
        </div>
        <div :class="$style.goalProgress">
          <span>销售目标 <strong>73%</strong><i data-goal="sales" /></span>
          <span>回款目标 <strong>54%</strong><i data-goal="collections" /></span>
          <span>毛利目标 <strong>72%</strong><i data-goal="profit" /></span>
        </div>
      </div>
    </article>

    <header :class="$style.sectionHeading">
      <BarChart3
        :size="16"
        :stroke-width="1.8"
      />
      <h2>核心 KPI 总览</h2>
      <span>· 数据口径：订单金额 · 含 12 月走势</span>
    </header>
    <div :class="$style.metricGrid">
      <button
        v-for="metric in salesBoardPersonalMetrics"
        :key="metric.id"
        type="button"
        :data-metric-tone="metric.tone"
        @click="inspectMetric(metric)"
      >
        <span :class="$style.metricTopline">
          <i
            ><component
              :is="metric.icon"
              :size="16"
              :stroke-width="1.8"
          /></i>
          <em :data-trend-down="metric.trend < 0">{{ metric.trend >= 0 ? '↑' : '↓' }} {{ Math.abs(metric.trend) }}%</em>
        </span>
        <small>{{ metric.label }}</small>
        <strong>{{ metric.value }}</strong>
        <p>{{ metric.detail }}</p>
        <span :class="$style.sparkline" />
      </button>
    </div>

    <div :class="$style.supportGrid">
      <article>
        <h3>年度目标完成分析</h3>
        <p>当前时间进度 32% · 实时仪表盘 + 全年预测</p>
        <dl>
          <div>
            <dt>销售订单目标</dt>
            <dd>73% <span>领先目标</span></dd>
          </div>
          <div>
            <dt>回款目标</dt>
            <dd>54% <span>领先目标</span></dd>
          </div>
          <div>
            <dt>毛利目标</dt>
            <dd>72% <span>领先目标</span></dd>
          </div>
        </dl>
      </article>
      <article>
        <h3>销售漏斗分析</h3>
        <p>线索 → 商机 → 订单 → 回款 全链路转化</p>
        <ol>
          <li><strong>56</strong><span>新增线索</span></li>
          <li><strong>32</strong><span>转为商机</span></li>
          <li><strong>14</strong><span>赢单订单</span></li>
          <li><strong>9</strong><span>已回款</span></li>
        </ol>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.section {
  @apply min-w-0 pb-8 pt-4;
}

.filters {
  @apply flex min-h-[55px] items-center gap-2 rounded border px-3;
  border-color: var(--ui-border);
  background: var(--ui-surface);
  color: var(--ui-text-subtle);
}

.filters > strong {
  @apply whitespace-nowrap text-[11px];
}

.filters > :global(button) {
  @apply shrink-0;
}

.salespersonSelect {
  min-width: 170px;
}

.profileHero {
  @apply mt-4 grid min-h-[190px] grid-cols-[1.25fr_1.55fr_1.35fr] gap-4 overflow-hidden rounded p-5 text-white;
  background: #0747a6;
}

.identity {
  @apply flex items-start gap-3;
}

.avatar {
  @apply flex h-16 w-16 shrink-0 items-center justify-center rounded-full border-2 text-[27px] font-semibold;
  border-color: rgb(255 255 255 / 55%);
  background: rgb(255 255 255 / 12%);
}

.nameLine {
  @apply flex items-center gap-2;
}

.nameLine h2 {
  @apply text-[18px] font-semibold;
}

.nameLine span,
.badges span {
  @apply rounded px-1.5 py-0.5 text-[9px];
  background: rgb(255 255 255 / 16%);
}

.identity p {
  @apply mt-1 flex items-center gap-1 text-[9px];
  color: rgb(255 255 255 / 78%);
}

.badges {
  @apply mt-3 flex flex-wrap gap-1.5;
}

.profileStats {
  @apply grid grid-cols-3 gap-2 self-center;
}

.profileStats div {
  @apply rounded p-3;
  background: rgb(255 255 255 / 12%);
}

.profileStats dt {
  @apply text-[9px];
  color: rgb(255 255 255 / 72%);
}

.profileStats dd {
  @apply mt-1 text-[17px] font-semibold;
}

.profileStats small {
  @apply ml-0.5 text-[9px] font-normal;
}

.healthCard {
  @apply rounded p-4;
  background: #f7f5ff;
  color: var(--ui-text);
}

.healthTop {
  @apply flex gap-4;
}

.score {
  @apply flex h-16 w-16 shrink-0 flex-col items-center justify-center rounded-full border-[7px] text-[21px] font-bold;
  border-color: var(--app-primary);
  color: var(--app-primary);
}

.score small {
  @apply text-[7px] font-medium;
  color: var(--ui-text-subtle);
}

.healthTop h3 {
  @apply mb-1 flex items-center gap-1 text-[11px] font-semibold;
}

.healthTop p {
  @apply flex min-w-[125px] justify-between text-[8px] leading-4;
  color: var(--ui-text-subtle);
}

.healthTop strong {
  color: #00875a;
}

.goalProgress {
  @apply mt-3 grid grid-cols-3 gap-2 border-t pt-2;
  border-color: var(--ui-border-subtle);
}

.goalProgress span {
  @apply grid grid-cols-[1fr_auto] gap-1 text-[8px];
}

.goalProgress i {
  @apply col-span-2 h-1 rounded-full;
  width: 73%;
  background: var(--app-primary);
}

.goalProgress i[data-goal='collections'] {
  width: 54%;
  background: #00875a;
}

.goalProgress i[data-goal='profit'] {
  width: 72%;
  background: #ff8b00;
}

.sectionHeading {
  @apply mt-4 flex items-center gap-2;
  color: var(--app-primary);
}

.sectionHeading h2 {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}

.sectionHeading span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.metricGrid {
  @apply mt-2 grid grid-cols-4 gap-3;
}

.metricGrid button {
  @apply relative min-h-[135px] overflow-hidden rounded border p-3 text-left outline-none transition-colors;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.metricGrid button:hover {
  border-color: #b3d4ff;
}

.metricGrid button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.metricTopline {
  @apply flex items-center justify-between;
}

.metricTopline i {
  @apply flex h-7 w-7 items-center justify-center rounded not-italic;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.metricTopline em {
  @apply rounded px-1.5 py-0.5 text-[8px] font-semibold not-italic;
  color: #00875a;
  background: #e3fcef;
}

.metricTopline em[data-trend-down='true'] {
  color: #de350b;
  background: #ffebe6;
}

.metricGrid small {
  @apply mt-3 block text-[9px];
  color: var(--ui-text-subtle);
}

.metricGrid strong {
  @apply mt-1 block text-[17px] font-semibold;
}

.metricGrid p {
  @apply mt-0.5 text-[8px];
  color: var(--ui-text-subtle);
}

.sparkline {
  @apply absolute inset-x-0 bottom-0 h-7;
  background: repeating-linear-gradient(105deg, var(--app-primary-soft) 0 14px, transparent 14px 24px);
}

.supportGrid {
  @apply mt-4 grid grid-cols-2 gap-4;
}

.supportGrid article {
  @apply rounded border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.supportGrid h3 {
  @apply text-[12px] font-semibold;
}

.supportGrid > article > p {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}

.supportGrid dl {
  @apply mt-4 grid grid-cols-3 gap-2;
}

.supportGrid dl div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}

.supportGrid dt,
.supportGrid li span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}

.supportGrid dd {
  @apply mt-1 text-[15px] font-semibold;
  color: var(--app-primary);
}

.supportGrid dd span {
  @apply ml-1 text-[8px] font-normal;
  color: #00875a;
}

.supportGrid ol {
  @apply mt-4 flex items-center justify-between;
}

.supportGrid li {
  @apply flex min-w-[70px] flex-col items-center rounded border p-3;
  border-color: var(--ui-border-subtle);
}

.supportGrid li strong {
  @apply text-[16px];
  color: var(--app-primary);
}

@media (max-width: 1240px) {
  .filters {
    @apply flex-wrap py-2;
  }

  .profileHero {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .healthCard {
    @apply col-span-2;
  }
}

@media (prefers-reduced-motion: reduce) {
  .metricGrid button {
    transition: none;
  }
}
</style>
