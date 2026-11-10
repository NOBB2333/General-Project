<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import {
  UiButton,
  UiCheckbox,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  type UiSelectOption,
} from '@/components/ui';
import type {
  RevenueMethod,
  RevenueRecognitionRow,
  RevenueStatus,
  RevenueStatusFilter,
} from '../../model/revenue-recognition.types';

const props = defineProps<{
  rows: readonly RevenueRecognitionRow[];
  status: RevenueStatusFilter;
  statusCounts: Readonly<Record<RevenueStatus, number>>;
  query: string;
  startDate: string;
  endDate: string;
  customer: string;
  project: string;
  method: string;
  creator: string;
  selectedIds: readonly string[];
}>();

const emit = defineEmits<{
  'update:status': [value: RevenueStatusFilter];
  'update:query': [value: string];
  'update:startDate': [value: string];
  'update:endDate': [value: string];
  'update:customer': [value: string];
  'update:project': [value: string];
  'update:method': [value: string];
  'update:creator': [value: string];
  toggle: [id: string, checked: boolean];
  toggleAll: [checked: boolean];
  export: [];
  create: [];
  view: [row: RevenueRecognitionRow];
}>();

const statuses: readonly RevenueStatusFilter[] = ['全部', '草稿', '待审核', '已审核', '已驳回', '已作废'];
const customerOptions: readonly UiSelectOption[] = [
  { value: '', label: '客户' },
  { value: '国电南瑞', label: '国电南瑞' },
  { value: '华能电力科技', label: '华能电力科技' },
  { value: '中石化仪控中心', label: '中石化仪控中心' },
  { value: '上海智远工业', label: '上海智远工业' },
  { value: '北方水泥集团', label: '北方水泥集团' },
];
const projectOptions: readonly UiSelectOption[] = [
  { value: '', label: '项目' },
  { value: '南瑞二期扩容', label: '南瑞二期扩容' },
  { value: '华能DCS改造', label: '华能DCS改造' },
  { value: '中石化仪控', label: '中石化仪控' },
  { value: '智远PLC替换升级', label: '智远PLC替换升级' },
  { value: '水泥窑控制系统服务', label: '水泥窑控制系统服务' },
];
const methodOptions: readonly UiSelectOption[] = [
  { value: '', label: '确认方式' },
  ...(['按发货', '按里程碑', '按验收', '按周期', '手动', '混合', '服务'] as const).map((value) => ({
    value,
    label: value,
  })),
];
const creatorOptions: readonly UiSelectOption[] = [
  { value: '', label: '制单人' },
  { value: '王会计', label: '王会计' },
  { value: '张出纳', label: '张出纳' },
  { value: '系统自动', label: '系统自动' },
];

function statusLabel(status: RevenueStatusFilter) {
  return status === '全部' ? status : `${status} (${props.statusCounts[status]})`;
}
function money(value: number | null) {
  return value === null ? '-' : `¥${value.toLocaleString('zh-CN')}`;
}
function percent(row: RevenueRecognitionRow) {
  return Math.round((row.accumulated / row.contractAmount) * 100);
}
function progressClass(row: RevenueRecognitionRow) {
  return `progress${Math.min(100, Math.max(0, Math.round(percent(row) / 10) * 10))}`;
}
function selectedState() {
  if (!props.rows.length || !props.selectedIds.length) return false;
  return props.rows.every((row) => props.selectedIds.includes(row.id)) ? true : 'indeterminate';
}
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.toolbar">
      <UiTabs
        :model-value="status"
        @update:model-value="emit('update:status', $event as RevenueStatusFilter)"
      >
        <UiTabsList
          label="收入确认状态"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            v-for="item in statuses"
            :key="item"
            :value="item"
            :class="$style.tab"
            >{{ statusLabel(item) }}</UiTabsTrigger
          >
        </UiTabsList>
      </UiTabs>
      <div :class="$style.actions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('export')"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建确认单</UiButton
        >
        <label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true" /><UiInput
            :model-value="query"
            placeholder="搜索确认单号/合同/客户..."
            aria-label="搜索收入确认"
            @update:model-value="emit('update:query', $event)"
        /></label>
      </div>
    </header>

    <div :class="$style.filters">
      <span>确认日期:</span>
      <UiInput
        :model-value="startDate"
        type="date"
        aria-label="确认开始日期"
        @update:model-value="emit('update:startDate', $event)"
      />
      <span>至</span>
      <UiInput
        :model-value="endDate"
        type="date"
        aria-label="确认结束日期"
        @update:model-value="emit('update:endDate', $event)"
      />
      <UiSelect
        :model-value="customer"
        :options="customerOptions"
        label="客户筛选"
        @update:model-value="emit('update:customer', $event)"
      />
      <UiSelect
        :model-value="project"
        :options="projectOptions"
        label="项目筛选"
        @update:model-value="emit('update:project', $event)"
      />
      <UiSelect
        :model-value="method"
        :options="methodOptions"
        label="确认方式筛选"
        @update:model-value="emit('update:method', $event)"
      />
      <UiSelect
        :model-value="creator"
        :options="creatorOptions"
        label="制单人筛选"
        @update:model-value="emit('update:creator', $event)"
      />
    </div>

    <div :class="$style.tableScroller">
      <span :class="$style.scrollHint">↔ 左右滑动</span>
      <table :class="$style.table">
        <thead>
          <tr>
            <th>
              <UiCheckbox
                :model-value="selectedState()"
                label="选择全部收入确认"
                @update:model-value="emit('toggleAll', Boolean($event))"
              />
            </th>
            <th>确认单号</th>
            <th>来源合同</th>
            <th>客户名称</th>
            <th>确认方式</th>
            <th>本次确认金额</th>
            <th>累计 / 合同</th>
            <th>剩余待确认</th>
            <th>确认日期</th>
            <th>财务期间</th>
            <th>开票状态</th>
            <th>状态</th>
            <th>创建人 / 时间</th>
            <th>审核人 / 时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <UiCheckbox
                :model-value="selectedIds.includes(row.id)"
                :label="`选择 ${row.code}`"
                @update:model-value="emit('toggle', row.id, Boolean($event))"
              />
            </td>
            <td>
              <strong :class="$style.code">{{ row.code }}</strong>
            </td>
            <td>
              <strong :class="$style.code">{{ row.contract }}</strong>
            </td>
            <td>
              <strong>{{ row.customer }}</strong>
            </td>
            <td>
              <span
                :class="$style.method"
                :data-method="row.method"
                >{{ row.method }}</span
              >
            </td>
            <td>
              <strong :class="$style.amount">{{ money(row.amount) }}</strong>
            </td>
            <td>
              <span :class="$style.cumulative"
                ><strong>{{ money(row.accumulated) }}</strong
                ><small>/ {{ money(row.contractAmount) }}</small
                ><i><b :class="$style[progressClass(row)]" /></i><small>{{ percent(row) }}%</small></span
              >
            </td>
            <td>
              <strong :class="$style.remaining">{{ money(row.remaining) }}</strong>
            </td>
            <td>{{ row.confirmedAt }}</td>
            <td>
              <span :class="$style.period">{{ row.period }}</span>
            </td>
            <td>
              <span
                :class="$style.invoice"
                :data-invoice="row.invoiceStatus"
                >{{ row.invoiceStatus }}</span
              >
            </td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>
              <span :class="$style.people"
                ><strong>{{ row.creator }}</strong
                ><small>{{ row.createdAt }}</small></span
              >
            </td>
            <td>
              <span :class="$style.people"
                ><strong>{{ row.reviewer }}</strong
                ><small>{{ row.reviewedAt }}</small></span
              >
            </td>
            <td>
              <UiButton
                variant="link"
                size="xs"
                @click="emit('view', row)"
                >查看</UiButton
              >
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.empty"
              colspan="15"
            >
              当前筛选下没有收入确认单
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[54px] items-center gap-4 border-b px-4;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-1;
}
.tab {
  @apply min-h-[52px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}
.tab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}
.actions {
  @apply ml-auto flex items-center gap-2;
}
.searchField {
  @apply flex h-8 w-[245px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: transparent;
  background: var(--ui-surface-hover);
}
.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
.filters {
  @apply flex min-h-[58px] items-center gap-2 border-b px-4 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.filters input {
  @apply w-[130px];
}
.filters > div {
  @apply w-[150px];
}
.tableScroller {
  @apply relative overflow-x-auto;
}
.scrollHint {
  @apply absolute right-2 top-2 z-10 rounded-full border px-2 py-1 text-[9px] font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}
.table {
  @apply min-w-[1900px] border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.table td {
  @apply h-[72px] border-b px-3 align-middle;
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.code {
  @apply whitespace-nowrap font-semibold;
  color: var(--app-primary);
}
.method,
.period,
.invoice,
.status {
  @apply inline-flex whitespace-nowrap rounded px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.method[data-method='按发货'],
.method[data-method='混合'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.method[data-method='按验收'] {
  color: var(--ui-success);
  background: #e3fcef;
}
.method[data-method='按里程碑'] {
  color: #6554c0;
  background: #eae6ff;
}
.amount {
  color: var(--ui-success);
}
.remaining {
  color: var(--ui-warning);
}
.cumulative {
  @apply flex min-w-[130px] flex-col;
}
.cumulative small {
  color: var(--ui-text-subtle);
}
.cumulative i {
  @apply my-1 block h-1 w-24 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}
.cumulative b {
  @apply block h-full rounded-full;
  background: var(--ui-warning);
}
.progress0 {
  width: 0%;
}
.progress10 {
  width: 10%;
}
.progress20 {
  width: 20%;
}
.progress30 {
  width: 30%;
}
.progress40 {
  width: 40%;
}
.progress50 {
  width: 50%;
}
.progress60 {
  width: 60%;
}
.progress70 {
  width: 70%;
}
.progress80 {
  width: 80%;
}
.progress90 {
  width: 90%;
}
.progress100 {
  width: 100%;
}
.invoice[data-invoice='已开票'],
.invoice[data-invoice='已认证'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.invoice[data-invoice='部分开票'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已审核'] {
  color: var(--ui-success);
  background: #e3fcef;
}
.status[data-status='待审核'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='草稿'] {
  color: var(--ui-text-muted);
  background: var(--ui-border-subtle);
}
.status[data-status='已驳回'],
.status[data-status='已作废'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.people {
  @apply flex min-w-[125px] flex-col;
}
.people small {
  @apply mt-1;
  color: var(--ui-text-subtle);
}
.empty {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
