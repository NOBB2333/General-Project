<script setup lang="ts">
import { computed, ref, type Component } from 'vue';
import { AlertTriangle, Boxes, CircleDollarSign, Download, PackageSearch, Truck, WalletCards } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabPanel, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { stockOverviewReferenceFixture } from '../model/stock-overview.data';
import type { InventoryMovement, InventoryOverviewTab } from '../model/stock-overview.types';
import StockOverviewActivity from './stock-overview/StockOverviewActivity.vue';
import StockOverviewCharts from './stock-overview/StockOverviewCharts.vue';
import StockOverviewHero from './stock-overview/StockOverviewHero.vue';
import StockOverviewSummary from './stock-overview/StockOverviewSummary.vue';

interface SummaryItem {
  label: string;
  value: string;
  note: string;
  tone: string;
  icon: Component;
  interactive?: boolean;
}
const tab = ref<InventoryOverviewTab>('总览');
const detailOpen = ref(false);
const detailTitle = ref('在途库存');
const selectedMovement = ref<InventoryMovement | null>(null);
const rows = cloneReferenceRows(stockOverviewReferenceFixture.rows);
const tabs: InventoryOverviewTab[] = ['总览', '商品库存', '在途库存', '分类汇总', '仓库视图'];
const summary: SummaryItem[] = [
  { label: '库存总采购成本', value: '¥154.4万', note: '20 种商品', tone: 'neutral', icon: WalletCards },
  { label: '预估销售总价', value: '¥205.3万', note: '毛利率 24.8%', tone: 'green', icon: CircleDollarSign },
  { label: 'SKU总数', value: '10', note: '账面总量 277', tone: 'neutral', icon: Boxes },
  { label: '在途库存', value: '49 件', note: '点击查看详情 →', tone: 'blue', icon: Truck, interactive: true },
  { label: '库存预警', value: '3 项', note: '低库存/缺货', tone: 'red', icon: AlertTriangle, interactive: true },
  {
    label: '售价为0的物料',
    value: '3 项',
    note: '需设置预估售价',
    tone: 'orange',
    icon: PackageSearch,
    interactive: true,
  },
];
const activeRows = computed(() =>
  tab.value === '在途库存' ? rows.filter((row) => row.quantity.startsWith('+')) : rows,
);
function openDetail(label: string) {
  detailTitle.value = label;
  selectedMovement.value = null;
  detailOpen.value = true;
}
function openMovement(row: InventoryMovement) {
  selectedMovement.value = row;
  detailTitle.value = row.id;
  detailOpen.value = true;
}
function exportInventory() {
  const csv = [
    '单号,类型,描述,数量,金额',
    ...rows.map((row) => [row.id, row.kind, row.description, row.quantity, row.amount].join(',')),
  ].join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = '库存总览.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success('库存总览已导出');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="dashboard"
    data-page-route="/inventory/overview"
    data-stock-overview-page
    :data-active-tab="tab"
  >
    <StockOverviewHero />
    <div :class="$style.exportRow">
      <UiButton
        size="md"
        @click="exportInventory"
        ><Download
          :size="14"
          :stroke-width="1.8"
        />导出</UiButton
      >
    </div>
    <UiTabs v-model="tab">
      <div :class="$style.tabFrame">
        <UiTabsList label="库存总览视图"
          ><UiTabsTrigger
            v-for="item in tabs"
            :key="item"
            :value="item"
            :class="$style.tab"
            >{{ item }}<span v-if="item === '在途库存'">6</span></UiTabsTrigger
          ></UiTabsList
        >
      </div>
      <UiTabPanel value="总览">
        <div :class="$style.scope">
          <strong>我的管辖范围</strong><span>PLC</span><span>配件</span><span>西门子</span><span>三菱</span
          ><span>上海主仓</span><small>完全管理权限 · 管理员可查看全部</small>
        </div>
        <StockOverviewSummary
          :items="summary"
          @detail="openDetail"
        />
        <StockOverviewCharts
          :warehouses="stockOverviewReferenceFixture.warehouses"
          :categories="stockOverviewReferenceFixture.categories"
        />
        <StockOverviewActivity
          :rows="rows"
          @select="openMovement"
          @manage="tab = '在途库存'"
        />
      </UiTabPanel>
      <UiTabPanel
        v-for="item in tabs.slice(1)"
        :key="item"
        :value="item"
        ><section :class="$style.listPanel">
          <header>
            <h2>{{ item }}</h2>
            <span>共 {{ activeRows.length }} 条</span>
          </header>
          <button
            v-for="row in activeRows"
            :key="row.id"
            type="button"
            @click="openMovement(row)"
          >
            <b>{{ row.id }}</b
            ><span>{{ row.description }}</span
            ><strong>{{ row.quantity }}</strong
            ><em>{{ row.amount }}</em>
          </button>
        </section></UiTabPanel
      >
    </UiTabs>
    <UiDialog
      v-model:open="detailOpen"
      :title="detailTitle"
      description="库存数据仅在本地页面状态中展示"
      size="sm"
      ><div :class="$style.detail">
        <template v-if="selectedMovement"
          ><p>
            业务类型<strong>{{ selectedMovement.kind }}</strong>
          </p>
          <p>
            库存变化<strong>{{ selectedMovement.quantity }}</strong>
          </p>
          <p>
            业务金额<strong>{{ selectedMovement.amount }}</strong>
          </p>
          <p>
            业务说明<strong>{{ selectedMovement.description }}</strong>
          </p></template
        ><template v-else
          ><p>在途库存<strong>49 件</strong></p>
          <p>库存预警<strong>3 项</strong></p>
          <p>售价待维护<strong>3 项</strong></p>
          <p>账面总量<strong>277</strong></p></template
        >
      </div>
      <template #footer
        ><UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
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
.exportRow {
  @apply flex h-[68px] items-center justify-end;
}
.tabFrame {
  @apply rounded-t border bg-white px-4;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[52px] rounded-none px-5;
  background: transparent;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.tab span {
  @apply ml-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-[#DEEBFF] px-1 text-[9px] text-[#0052CC];
}
.scope {
  @apply mt-4 flex h-11 items-center gap-2 rounded border px-4 text-[10px];
  color: #0052cc;
  border-color: #b3d4ff;
  background: #deebff;
}
.scope strong {
  @apply mr-1;
}
.scope span {
  @apply rounded px-2 py-1 font-semibold;
  background: rgba(255, 255, 255, 0.48);
}
.scope small {
  @apply ml-auto;
  color: #97a0af;
}
.listPanel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.listPanel header {
  @apply flex items-center justify-between border-b px-5 py-4;
  border-color: var(--ui-border-subtle);
}
.listPanel h2 {
  @apply text-[15px] font-semibold;
}
.listPanel header span {
  @apply text-[10px];
  color: #6b778c;
}
.listPanel > button {
  @apply grid w-full grid-cols-[130px_1fr_100px_100px] gap-3 border-b px-5 py-3 text-left text-[11px] transition-colors;
  border-color: var(--ui-border-subtle);
}
.listPanel > button:hover {
  background: #f4f5f7;
}
.listPanel > button b {
  color: #0052cc;
}
.listPanel > button strong {
  @apply text-right;
  color: #de350b;
}
.listPanel > button em {
  @apply text-right not-italic;
  color: #42526e;
}
.detail {
  @apply grid grid-cols-2 gap-3 p-5;
}
.detail p {
  @apply flex flex-col rounded border bg-[#FAFBFC] p-3 text-[10px];
  color: #6b778c;
  border-color: var(--ui-border-subtle);
}
.detail strong {
  @apply mt-1 text-[13px];
  color: #172b4d;
}
</style>
