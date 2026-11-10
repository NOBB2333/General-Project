<script setup lang="ts">
import { computed, ref } from 'vue';
import { CheckCircle2, Clock3, Download, Gift, Package, Plus, Search } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiInput, toast } from '@/components/ui';
import { goodwillOrdersReferenceFixture } from '../model/goodwill-orders.data';
import type { GoodwillStatus } from '../model/goodwill-orders.types';

const rows = ref(cloneReferenceRows(goodwillOrdersReferenceFixture.rows));
const query = ref('');
const activeType = ref('全部');
const activeStatus = ref('全部');
const types = ['全部', '备件赠送', '技术资料', '样品提供', '质量补偿', '客户关怀', '其他'];
const statuses: readonly (GoodwillStatus | '全部')[] = [
  '全部',
  '草稿',
  '待审批',
  '已审批',
  '发货中',
  '已完成',
  '已驳回',
];
const filteredRows = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (!needle ||
        `${row.code} ${row.customer} ${row.reason} ${row.owner}`.toLocaleLowerCase('zh-CN').includes(needle)) &&
      (activeType.value === '全部' || row.type === activeType.value) &&
      (activeStatus.value === '全部' || row.status === activeStatus.value),
  );
});
const count = (status: GoodwillStatus) => rows.value.filter((row) => row.status === status).length;
function exportCsv() {
  const csv = [
    '单号,客户,类型,状态,日期,创建人',
    ...filteredRows.value.map((row) =>
      [row.code, row.customer, row.type, row.status, row.date, row.owner].map((value) => `"${value}"`).join(','),
    ),
  ].join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv' }));
  link.download = 'goodwill-orders.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success('Goodwill 订单已导出');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/sales/goodwill-orders"
    data-page-label="Goodwill订单"
    data-page-module="销售业务"
    data-goodwill-orders-page
    :data-result-count="filteredRows.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>销售业务</span><b>›</b><strong>Goodwill 订单</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Gift
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>Goodwill 订单</h1>
            <p>客情维护、补偿赠送等无偿订单的审批与发货跟踪</p>
          </div>
        </div>
      </div>
      <div :class="$style.giftArt">
        <Gift
          :size="150"
          :stroke-width="1.2"
        /><span>★</span>
      </div>
    </header>
    <section
      :class="$style.metrics"
      aria-label="Goodwill订单概览"
    >
      <article>
        <Gift :size="18" />
        <div>
          <span>订单总数</span><strong>{{ rows.length + 2 }}</strong
          ><small>累计 54 件物品</small>
        </div>
      </article>
      <article :class="$style.warning">
        <Clock3 :size="18" />
        <div>
          <span>待审批</span><strong>{{ count('待审批') }}</strong
          ><small>需要处理</small>
        </div>
      </article>
      <article :class="$style.primary">
        <Package :size="18" />
        <div>
          <span>进行中</span><strong>{{ count('发货中') + count('已审批') }}</strong
          ><small>已审批/发货中</small>
        </div>
      </article>
      <article :class="$style.success">
        <CheckCircle2 :size="18" />
        <div>
          <span>已完成</span><strong>{{ count('已完成') }}</strong
          ><small>赠送已完结</small>
        </div>
      </article>
    </section>
    <section :class="$style.filters">
      <div :class="$style.filterLine">
        <span>类型：</span
        ><button
          v-for="type in types"
          :key="type"
          type="button"
          :class="[$style.chip, activeType === type && $style.chipActive]"
          @click="activeType = type"
        >
          {{ type }}
        </button>
      </div>
      <div :class="$style.listTools">
        <nav :class="$style.tabs">
          <button
            v-for="status in statuses"
            :key="status"
            type="button"
            :class="[$style.tab, activeStatus === status && $style.tabActive]"
            @click="activeStatus = status"
          >
            {{ status }}<em v-if="status !== '全部'">({{ count(status) }})</em>
          </button>
        </nav>
        <div :class="$style.actions">
          <label :class="$style.search"
            ><Search :size="15" /><UiInput
              v-model="query"
              aria-label="搜索 Goodwill 订单"
              placeholder="搜索单号、客户、原因、创建人…" /></label
          ><UiButton
            variant="secondary"
            size="sm"
            @click="exportCsv"
            ><template #leading><Download :size="14" /></template>导出</UiButton
          ><UiButton
            variant="primary"
            size="sm"
            @click="toast.info('新建 Goodwill 订单表单已准备')"
            ><template #leading><Plus :size="14" /></template>新建 Goodwill 订单</UiButton
          >
        </div>
      </div>
    </section>
    <section :class="$style.tablePanel">
      <table>
        <thead>
          <tr>
            <th>单号</th>
            <th>客户</th>
            <th>类型</th>
            <th>申请原因</th>
            <th>物品</th>
            <th>状态</th>
            <th>创建日期</th>
            <th>创建人</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in filteredRows"
            :key="row.id"
          >
            <td :class="$style.code">{{ row.code }}</td>
            <td>
              <strong>{{ row.customer }}</strong
              ><small>合同 HT-2026-018</small>
            </td>
            <td>
              <span :class="[$style.typeTag, $style[`type_${row.type}`]]">{{ row.type }}</span>
            </td>
            <td :class="$style.reason">{{ row.reason }}</td>
            <td>{{ row.items }}</td>
            <td>
              <span :class="[$style.status, $style[`status_${row.status}`]]">{{ row.status }}</span>
            </td>
            <td>{{ row.date }}</td>
            <td>
              <i :class="$style.avatar">{{ row.owner.slice(0, 1) }}</i
              >{{ row.owner }}
            </td>
            <td>
              <button
                type="button"
                :class="$style.more"
                title="更多操作"
              >
                •••
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <p
        v-if="!filteredRows.length"
        :class="$style.empty"
      >
        当前筛选下没有 Goodwill 订单
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
  background: linear-gradient(108deg, #bf2600 0%, #de350b 55%, #ff5630 100%);
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
.giftArt {
  @apply pointer-events-none absolute right-16 top-[-8px] flex items-center opacity-70;
}
.giftArt span {
  @apply absolute right-0 top-3 text-[54px] text-[#FFFAE6];
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[102px] items-start gap-3 rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
  color: #ff8b00;
}
.metrics article > svg {
  @apply mt-0.5;
}
.metrics article span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.metrics article strong {
  @apply mt-2 block text-[27px] leading-7;
  color: var(--ui-text);
}
.metrics article small {
  @apply mt-1.5 block text-[11px];
  color: #97a0af;
}
.metrics .warning strong {
  color: #ff8b00;
}
.metrics .primary {
  color: #0052cc;
}
.metrics .primary strong {
  color: #0052cc;
}
.metrics .success {
  color: #00875a;
}
.metrics .success strong {
  color: #00875a;
}
.filters {
  @apply mb-3 rounded border bg-white px-4 pt-3;
  border-color: var(--ui-border);
}
.filterLine {
  @apply flex min-h-9 items-center gap-1;
}
.filterLine > span {
  @apply mr-1 text-[12px] font-semibold;
  color: var(--ui-text-muted);
}
.chip {
  @apply rounded px-3 py-1.5 text-[12px];
  color: var(--ui-text-muted);
}
.chip:hover {
  background: var(--ui-surface-hover);
}
.chipActive {
  @apply text-white;
  background: var(--app-primary);
}
.listTools {
  @apply flex items-end justify-between border-t pt-2;
  border-color: var(--ui-border-subtle);
}
.tabs {
  @apply flex items-end gap-1;
}
.tab {
  @apply min-h-10 border-b-2 border-transparent px-4 text-[13px];
  color: var(--ui-text-muted);
}
.tab em {
  @apply ml-1 not-italic text-[11px];
  color: #97a0af;
}
.tabActive {
  @apply font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.actions {
  @apply flex items-center gap-2 pb-1;
}
.search {
  @apply flex h-8 w-[250px] items-center gap-1.5 rounded border px-2;
  color: #97a0af;
  border-color: var(--ui-border);
}
.search :global(input) {
  @apply min-w-0 flex-1 border-0 p-0 text-[11px] outline-none;
}
.tablePanel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
table {
  @apply w-full border-collapse text-[12px];
}
th {
  @apply h-10 bg-[#F4F5F7] px-4 text-left text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
td {
  @apply h-[70px] border-t px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
td strong {
  @apply block font-medium;
  color: var(--ui-text);
}
td small {
  @apply mt-1 block text-[10px];
  color: #97a0af;
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.reason {
  @apply max-w-[280px] truncate;
}
.typeTag,
.status {
  @apply inline-flex rounded px-2 py-1 text-[11px] font-medium;
}
.type_备件赠送 {
  color: #0052cc;
  background: #deebff;
}
.type_技术资料 {
  color: #00875a;
  background: #e3fcef;
}
.type_样品提供 {
  color: #6554c0;
  background: #eae6ff;
}
.type_质量补偿 {
  color: #de350b;
  background: #ffebe6;
}
.type_客户关怀 {
  color: #ff8b00;
  background: #fffae6;
}
.status_已完成 {
  color: #00875a;
  background: #e3fcef;
}
.status_已审批 {
  color: #0052cc;
  background: #deebff;
}
.status_待审批 {
  color: #ff8b00;
  background: #fffae6;
}
.status_发货中 {
  color: #6554c0;
  background: #eae6ff;
}
.status_草稿 {
  color: #6b778c;
  background: #f4f5f7;
}
.status_已驳回 {
  color: #de350b;
  background: #ffebe6;
}
.avatar {
  @apply mr-1 inline-flex h-6 w-6 items-center justify-center rounded-full not-italic text-[10px] font-semibold text-white;
  background: #0052cc;
}
.more {
  @apply text-[15px] text-[#6B778C];
}
.empty {
  @apply px-5 py-10 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
