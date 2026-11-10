<script setup lang="ts">
import { Box, Download, FileText, PauseCircle, Repeat2, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { CostCenterRow, CostCollectionTab, CostQueue } from '../../model/cost-centers.types';

defineProps<{
  rows: readonly CostCenterRow[];
  activeTab: CostCollectionTab;
  activeQueue: CostQueue;
  query: string;
  source: string;
  status: string;
}>();

const emit = defineEmits<{
  'update:activeTab': [value: CostCollectionTab];
  'update:activeQueue': [value: CostQueue];
  'update:query': [value: string];
  'update:source': [value: string];
  'update:status': [value: string];
  collect: [row: CostCenterRow];
  suspend: [row: CostCenterRow];
  export: [];
}>();

const tabs = [
  { value: 'pending-costs', label: '待归集成本', count: 9, icon: Box },
  { value: 'sales-transfer', label: '销售成本结转', count: 2, icon: Repeat2 },
  { value: 'confirmations', label: '成本确认记录', count: 11, icon: FileText },
] as const;

const sourceOptions: UiSelectOption[] = [
  { value: '', label: '全部来源' },
  { value: '采购入库', label: '采购入库' },
  { value: '生产领料', label: '生产领料' },
  { value: '外协结算', label: '外协结算' },
  { value: '费用报销', label: '费用报销' },
  { value: '工时归集', label: '工时归集' },
  { value: '制造费用分摊', label: '制造费用分摊' },
  { value: '安装调试', label: '安装调试' },
  { value: '运输费用', label: '运输费用' },
  { value: '差旅报销', label: '差旅报销' },
  { value: '付款核销', label: '付款核销' },
];
const statusOptions: UiSelectOption[] = [
  { value: '', label: '全部状态' },
  { value: '待归集', label: '待归集' },
  { value: '部分归集', label: '部分归集' },
];

function currency(value: number) {
  if (value === 0) return '-';
  return `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}
</script>

<template>
  <section :class="$style.panel">
    <UiTabs
      :model-value="activeTab"
      @update:model-value="emit('update:activeTab', $event as CostCollectionTab)"
    >
      <UiTabsList
        label="成本归集数据视图"
        :class="$style.dataTabs"
      >
        <UiTabsTrigger
          v-for="tab in tabs"
          :key="tab.value"
          :value="tab.value"
          :class="$style.dataTab"
        >
          <component
            :is="tab.icon"
            :size="14"
            :stroke-width="1.8"
          />
          {{ tab.label }}
          <span>{{ tab.count }}</span>
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div :class="$style.queueBar">
      <div
        :class="$style.queueTabs"
        role="group"
        aria-label="归集队列"
      >
        <button
          type="button"
          :class="[$style.queueButton, activeQueue === 'pending' && $style.queueActive]"
          :aria-pressed="activeQueue === 'pending'"
          @click="emit('update:activeQueue', 'pending')"
        >
          <Box
            :size="14"
            :stroke-width="1.8"
          />
          待归集 <span>9</span>
        </button>
        <button
          type="button"
          :class="[$style.queueButton, activeQueue === 'suspended' && $style.queueActive]"
          :aria-pressed="activeQueue === 'suspended'"
          @click="emit('update:activeQueue', 'suspended')"
        >
          <PauseCircle
            :size="14"
            :stroke-width="1.8"
          />
          暂挂 <span>1</span>
        </button>
      </div>
    </div>

    <div :class="$style.filters">
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="搜索编号、名称、客户..."
          aria-label="搜索成本记录"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <UiSelect
        :model-value="source"
        :options="sourceOptions"
        label="成本来源"
        size="sm"
        @update:model-value="emit('update:source', $event)"
      />
      <UiSelect
        :model-value="status"
        :options="statusOptions"
        label="成本状态"
        size="sm"
        @update:model-value="emit('update:status', $event)"
      />
      <UiButton
        :class="$style.exportButton"
        variant="secondary"
        size="sm"
        @click="emit('export')"
      >
        <template #leading>
          <Download
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        导出
      </UiButton>
    </div>

    <div :class="$style.tableScroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>成本编号</th>
            <th>来源类型</th>
            <th>成本名称</th>
            <th>成本类型</th>
            <th>客户</th>
            <th>合同/项目</th>
            <th>发生日期</th>
            <th>总金额</th>
            <th>已归集</th>
            <th>未归集</th>
            <th>状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <strong :class="$style.code">{{ row.code }}</strong>
            </td>
            <td>
              <span
                :class="$style.sourceBadge"
                :data-source="row.sourceType"
                >{{ row.sourceType }}</span
              >
            </td>
            <td>
              <strong :class="$style.name">{{ row.name }}</strong>
            </td>
            <td>{{ row.costType }}</td>
            <td>{{ row.customer }}</td>
            <td>
              <span :class="row.contract !== '-' ? $style.contract : undefined">{{ row.contract }}</span>
            </td>
            <td>{{ row.date }}</td>
            <td>
              <strong :class="$style.amount">{{ currency(row.totalAmount) }}</strong>
            </td>
            <td>
              <span :class="$style.collected">{{ currency(row.collectedAmount) }}</span>
            </td>
            <td>
              <strong :class="$style.openAmount">{{ currency(row.totalAmount - row.collectedAmount) }}</strong>
            </td>
            <td>
              <span :class="[$style.statusBadge, row.status === '部分归集' && $style.partialStatus]">
                {{ row.status }}
              </span>
            </td>
            <td>
              <div :class="$style.actions">
                <UiButton
                  variant="primary"
                  size="xs"
                  @click="emit('collect', row)"
                  >去归集</UiButton
                >
                <UiButton
                  v-if="row.status !== '部分归集'"
                  variant="secondary"
                  size="xs"
                  @click="emit('suspend', row)"
                  >暂挂</UiButton
                >
              </div>
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.emptyCell"
              colspan="12"
            >
              当前筛选下没有成本记录
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.dataTabs {
  @apply flex min-h-[42px] border-b px-4;
  border-color: var(--ui-border);
}

.dataTab {
  @apply min-h-10 rounded-none border-b-2 border-transparent px-4 text-[11px];
  color: var(--ui-text-subtle);
}

.dataTab:hover,
.dataTab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.dataTab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}

.dataTab span,
.queueButton span {
  @apply inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-border-subtle);
}

.queueBar {
  @apply flex min-h-[48px] items-center px-8;
}

.queueTabs {
  @apply flex gap-2;
}

.queueButton {
  @apply inline-flex min-h-8 items-center gap-1.5 rounded border px-3 text-[11px] font-semibold transition-colors;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

.queueActive {
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.queueActive span {
  @apply text-white;
  background: var(--app-primary);
}

.filters {
  @apply flex min-h-[55px] items-center gap-3 px-8;
}

.searchField {
  @apply flex h-8 w-[270px] items-center gap-2 rounded border px-2;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
}

.searchField input {
  @apply h-full border-0 p-0 shadow-none;
}

.exportButton {
  @apply ml-auto;
}

.tableScroller {
  @apply mx-4 overflow-x-auto;
}

.table {
  @apply w-full table-fixed border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}

.table th:nth-child(1) {
  width: 10%;
}

.table th:nth-child(2) {
  width: 8%;
}

.table th:nth-child(3) {
  width: 14%;
}

.table th:nth-child(4),
.table th:nth-child(5) {
  width: 8%;
}

.table th:nth-child(6) {
  width: 9%;
}

.table th:nth-child(7) {
  width: 10%;
}

.table th:nth-child(8),
.table th:nth-child(9) {
  width: 8%;
}

.table th:nth-child(10) {
  width: 7%;
}

.table th:nth-child(11),
.table th:nth-child(12) {
  width: 5%;
}

.table th {
  @apply h-[34px] border-y px-3 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[43px] truncate border-b px-3 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.code,
.contract {
  color: #0065ff;
}

.name,
.amount {
  color: var(--ui-text);
}

.sourceBadge,
.statusBadge {
  @apply inline-flex rounded px-2 py-0.5 font-semibold;
  color: #de350b;
  background: #ffebe6;
}

.sourceBadge[data-source='差旅报销'] {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.sourceBadge[data-source='安装调试'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.sourceBadge[data-source='制造费用分摊'] {
  color: #974f0c;
  background: #fff0b3;
}

.sourceBadge[data-source='工时归集'] {
  color: var(--ui-success);
  background: #e3fcef;
}

.statusBadge {
  color: #974f0c;
  background: #fff0b3;
}

.partialStatus {
  color: #0052cc;
  background: #deebff;
}

.collected {
  color: var(--ui-success);
}

.openAmount {
  color: var(--ui-warning);
}

.actions {
  @apply flex items-center gap-1.5;
}

.emptyCell {
  @apply h-28 text-center;
  color: var(--ui-text-subtle);
}
</style>
