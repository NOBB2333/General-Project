<script setup lang="ts">
import { Download, FileText, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { outboundReferenceFixture } from '../model/outbound.data';
import type { OutboundRecord, OutboundStatus } from '../model/outbound.types';
import OutboundHero from './outbound/OutboundHero.vue';
import OutboundTable from './outbound/OutboundTable.vue';

type OutboundView = 'list' | 'pending' | 'details';
type StatusFilter = '全部' | OutboundStatus;

const views: readonly { value: OutboundView; label: string; count: number }[] = [
  { value: 'list', label: '出库单列表', count: 8 },
  { value: 'pending', label: '待出库发货单', count: 4 },
  { value: 'details', label: '出库明细', count: 9 },
];
const statuses: readonly StatusFilter[] = ['全部', '已出库', '待出库'];
const rows = ref<OutboundRecord[]>(cloneReferenceRows(outboundReferenceFixture.rows));
const activeView = ref<OutboundView>('list');
const activeStatus = ref<StatusFilter>('全部');
const query = ref('');
const detailOpen = ref(false);
const activeRow = ref<OutboundRecord | null>(null);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesView = activeView.value !== 'pending' || row.status === '待出库';
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !keyword ||
      [row.id, row.shipmentCode, row.customer, row.contractCode, row.material].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(keyword),
      );
    return matchesView && matchesStatus && matchesQuery;
  });
});

function openDetails(row: OutboundRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function exportRows() {
  const header = [
    '出库单号',
    '发货单号',
    '客户',
    '合同号',
    '物料明细',
    '仓库',
    '出库日期',
    '状态',
    '物流公司',
    '物流单号',
    '收入确认',
    '操作员',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.shipmentCode,
    row.customer,
    row.contractCode,
    row.material,
    row.warehouse,
    row.date,
    row.status,
    row.carrier,
    row.trackingCode,
    row.revenueStatus,
    row.operator,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '出库管理.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/outbound"
    data-page-label="出库管理"
    data-page-module="库存管理"
    data-outbound-page
    :data-filtered-row-count="filteredRows.length"
  >
    <OutboundHero />

    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="exportRows"
      >
        <Download
          :size="14"
          :stroke-width="1.8"
        />
        导出
      </UiButton>
    </div>

    <section :class="$style.listPanel">
      <div :class="$style.viewToolbar">
        <UiTabs v-model="activeView">
          <UiTabsList label="出库业务视图">
            <UiTabsTrigger
              v-for="item in views"
              :key="item.value"
              :value="item.value"
            >
              <FileText
                v-if="item.value === 'details'"
                :size="13"
                :stroke-width="1.8"
              />
              {{ item.label }}
              <span :class="$style.count">{{ item.count }}</span>
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
      </div>

      <div :class="$style.filterToolbar">
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="query"
            aria-label="搜索出库单"
            placeholder="搜索出库单号、客户、发货单号..."
          />
        </label>
        <UiTabs v-model="activeStatus">
          <UiTabsList label="出库状态筛选">
            <UiTabsTrigger
              v-for="status in statuses"
              :key="status"
              :value="status"
              >{{ status }}</UiTabsTrigger
            >
          </UiTabsList>
        </UiTabs>
      </div>

      <OutboundTable
        :rows="filteredRows"
        @view="openDetails"
      />
    </section>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '出库详情'"
      :description="activeRow ? `${activeRow.customer} · ${activeRow.status}` : undefined"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>发货单号</dt>
          <dd>{{ activeRow.shipmentCode }}</dd>
        </div>
        <div>
          <dt>合同号</dt>
          <dd>{{ activeRow.contractCode }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>物料明细</dt>
          <dd>{{ activeRow.material }}</dd>
        </div>
        <div>
          <dt>仓库 / 日期</dt>
          <dd>{{ activeRow.warehouse }} · {{ activeRow.date }}</dd>
        </div>
        <div>
          <dt>操作员</dt>
          <dd>{{ activeRow.operator }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>物流信息</dt>
          <dd>{{ activeRow.carrier }} · {{ activeRow.trackingCode }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          size="sm"
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.actions {
  @apply flex h-[62px] items-center justify-end;
}

.listPanel {
  @apply overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}

.viewToolbar,
.filterToolbar {
  @apply flex items-center border-b px-5;
  border-color: var(--ui-border);
}

.viewToolbar {
  @apply h-[50px];
}

.filterToolbar {
  @apply h-[64px] gap-3;
}

.count {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.searchField {
  @apply relative block w-[320px];
}

.searchField > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}

.searchField input {
  @apply pl-9;
}

.detailGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.detailGrid div {
  @apply rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply m-0 mt-1 text-[12px] font-medium;
}

.detailWide {
  @apply col-span-2;
}
</style>
