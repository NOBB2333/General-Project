<script setup lang="ts">
import {
  Banknote,
  Bell,
  CalendarDays,
  ChevronLeft,
  ChevronRight,
  Ellipsis,
  Eye,
  FileStack,
  List,
  Search,
  SlidersHorizontal,
  Users,
  X,
} from 'lucide-vue-next';
import { computed } from 'vue';
import {
  UiButton,
  UiCheckbox,
  UiCollapsible,
  UiDropdownMenu,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
} from '@/components/ui';
import type { UiDropdownMenuItem, UiSelectOption } from '@/components/ui';
import type {
  FinanceAccountRow,
  FinanceAccountRowAction,
  FinanceAccountSide,
  FinanceAccountView,
} from '../../model/accounts.types';

interface AgeTab {
  value: string;
  label: string;
  count?: number;
  risk?: boolean;
  danger?: boolean;
}

interface GroupedRow {
  label: string;
  count: number;
  amount: number;
  settled: number;
  open: number;
}

const props = defineProps<{
  side: FinanceAccountSide;
  view: FinanceAccountView;
  ageTab: string;
  ageTabs: readonly AgeTab[];
  query: string;
  source: string;
  sourceOptions: readonly UiSelectOption[];
  billRelation: string;
  billOptions: readonly UiSelectOption[];
  startDate: string;
  endDate: string;
  advancedOpen: boolean;
  filteredRows: readonly FinanceAccountRow[];
  pagedRows: readonly FinanceAccountRow[];
  groupedRows: readonly GroupedRow[];
  selected: ReadonlySet<string>;
  page: number;
  pageCount: number;
}>();

const emit = defineEmits<{
  'update:view': [value: FinanceAccountView];
  'update:ageTab': [value: string];
  'update:query': [value: string];
  'update:source': [value: string];
  'update:billRelation': [value: string];
  'update:startDate': [value: string];
  'update:endDate': [value: string];
  'update:advancedOpen': [value: boolean];
  'update:page': [value: number];
  toggleSelection: [id: string];
  togglePageSelection: [];
  rowAction: [kind: FinanceAccountRowAction, row: FinanceAccountRow];
  export: [];
}>();

const allSelected = computed(
  () => props.pagedRows.length > 0 && props.pagedRows.every((row) => props.selected.has(row.id)),
);
const someSelected = computed(() => props.pagedRows.some((row) => props.selected.has(row.id)) && !allSelected.value);

const menuItems: readonly UiDropdownMenuItem[] = [
  { id: 'copy', label: '复制记录' },
  { id: 'reverse', label: '冲回记录', separatorBefore: true, tone: 'danger' },
];

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN', { maximumFractionDigits: 0 })}`;
}

function statusTone(row: FinanceAccountRow) {
  if (row.status === '已完成' || row.status === '已结清') return 'statusSuccess';
  if (row.status.startsWith('逾期90') || row.status === '逾期61-90天') return 'statusDanger';
  if (row.status.startsWith('逾期')) return 'statusWarning';
  return 'statusSuccess';
}
</script>

<template>
  <section :class="$style.ledger">
    <UiTabs
      :model-value="ageTab"
      @update:model-value="emit('update:ageTab', String($event))"
    >
      <UiTabsList
        label="账龄筛选"
        :class="$style.ageTabs"
      >
        <UiTabsTrigger
          v-for="tab in ageTabs"
          :key="tab.value"
          :value="tab.value"
          :class="[$style.ageTab, tab.risk && $style.riskTab, tab.danger && $style.dangerTab]"
        >
          <span
            v-if="tab.risk || tab.danger"
            :class="[$style.riskDot, tab.danger && $style.dangerDot]"
          />
          {{ tab.label }}
          <span :class="$style.tabCount">{{ tab.count === undefined ? '' : `(${tab.count})` }}</span>
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div :class="$style.toolbar">
      <UiTabs
        :model-value="view"
        @update:model-value="emit('update:view', String($event) as FinanceAccountView)"
      >
        <UiTabsList
          label="账款视图"
          :class="$style.viewTabs"
        >
          <UiTabsTrigger
            value="list"
            :class="$style.viewTab"
          >
            <List
              :size="13"
              :stroke-width="1.8"
            />
            列表
          </UiTabsTrigger>
          <UiTabsTrigger
            value="party"
            :class="$style.viewTab"
          >
            <Users
              :size="13"
              :stroke-width="1.8"
            />
            {{ side === 'receivable' ? '按客户' : '按对象' }}
          </UiTabsTrigger>
          <UiTabsTrigger
            value="contract"
            :class="$style.viewTab"
          >
            <FileStack
              :size="13"
              :stroke-width="1.8"
            />
            按合同
          </UiTabsTrigger>
          <UiTabsTrigger
            v-if="side === 'payable'"
            value="project"
            :class="$style.viewTab"
            >按项目</UiTabsTrigger
          >
        </UiTabsList>
      </UiTabs>

      <div :class="$style.filters">
        <UiSelect
          :model-value="source"
          :options="sourceOptions"
          label="来源"
          size="sm"
          :class="$style.filterSelect"
          @update:model-value="emit('update:source', $event)"
        />
        <UiSelect
          :model-value="billRelation"
          :options="billOptions"
          :label="side === 'receivable' ? '票据关系' : '核销状态'"
          size="sm"
          :class="$style.filterSelect"
          @update:model-value="emit('update:billRelation', $event)"
        />
        <UiCollapsible
          :open="advancedOpen"
          @update:open="emit('update:advancedOpen', $event)"
        >
          <template #trigger>
            <UiButton
              variant="subtle"
              size="sm"
              :class="$style.advancedButton"
            >
              <template #leading>
                <SlidersHorizontal
                  :size="13"
                  :stroke-width="1.8"
                />
              </template>
              高级筛选
            </UiButton>
          </template>
          <div
            v-if="advancedOpen"
            :class="$style.advancedPanel"
          >
            <label :class="$style.dateField">
              <CalendarDays
                :size="13"
                :stroke-width="1.8"
              />
              <UiInput
                :model-value="startDate"
                type="date"
                aria-label="到期开始日期"
                @update:model-value="emit('update:startDate', $event)"
              />
            </label>
            <span :class="$style.dateSeparator">~</span>
            <label :class="$style.dateField">
              <UiInput
                :model-value="endDate"
                type="date"
                aria-label="到期结束日期"
                @update:model-value="emit('update:endDate', $event)"
              />
              <CalendarDays
                :size="13"
                :stroke-width="1.8"
              />
            </label>
          </div>
        </UiCollapsible>
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            :model-value="query"
            :placeholder="side === 'receivable' ? '搜索应收号/销售单/客户/合同...' : '搜索应付号/采购单/供应商/合同...'"
            aria-label="搜索账款"
            @update:model-value="emit('update:query', $event)"
          />
          <button
            v-if="query"
            type="button"
            :class="$style.clearSearch"
            aria-label="清除搜索"
            @click="emit('update:query', '')"
          >
            <X
              :size="12"
              :stroke-width="1.8"
            />
          </button>
        </label>
        <UiButton
          variant="secondary"
          size="sm"
          :class="$style.exportButton"
          @click="emit('export')"
        >
          导出
        </UiButton>
      </div>
    </div>

    <div
      v-if="view === 'list'"
      :class="$style.tableWrap"
    >
      <table :class="$style.table">
        <thead>
          <tr>
            <th :class="$style.checkCell">
              <UiCheckbox
                :model-value="someSelected ? 'indeterminate' : allSelected"
                label="全选当前页"
                @update:model-value="emit('togglePageSelection')"
              />
            </th>
            <th>单号</th>
            <th>来源</th>
            <th>{{ side === 'receivable' ? '客户/业务员' : '收款方/联系人' }}</th>
            <th>合同/项目</th>
            <th>{{ side === 'receivable' ? '应收金额' : '应付金额' }}</th>
            <th>{{ side === 'receivable' ? '已收' : '已付' }}</th>
            <th>{{ side === 'receivable' ? '未收' : '未付' }}</th>
            <th>到期日</th>
            <th>状态</th>
            <th>{{ side === 'receivable' ? '发票' : '付款审核' }}</th>
            <th>{{ side === 'receivable' ? '票据' : '核销' }}</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in pagedRows"
            :key="row.id"
            :class="row.status.startsWith('逾期90') || row.status === '逾期61-90天' ? $style.highRiskRow : undefined"
          >
            <td :class="$style.checkCell">
              <UiCheckbox
                :model-value="selected.has(row.id)"
                :label="`选择 ${row.code}`"
                @update:model-value="emit('toggleSelection', row.id)"
              />
            </td>
            <td>
              <button
                type="button"
                :class="$style.codeButton"
                @click="emit('rowAction', 'view', row)"
              >
                {{ row.code }}
              </button>
              <small :class="$style.secondaryText">{{ row.sourceCode }}</small>
            </td>
            <td>
              <span :class="$style.sourceBadge">{{ row.source }}</span>
            </td>
            <td>
              <strong :class="$style.primaryCell">{{ row.party }}</strong>
              <small :class="$style.secondaryText">{{ row.contactRole }}: {{ row.contact }}</small>
            </td>
            <td>
              <strong :class="$style.codeLink">{{ row.contract }}</strong>
              <small
                v-if="row.project"
                :class="$style.secondaryText"
                >{{ row.project }}</small
              >
            </td>
            <td :class="$style.moneyCell">{{ money(row.amount) }}</td>
            <td :class="$style.receivedCell">{{ money(row.settled) }}</td>
            <td :class="[$style.moneyCell, row.amount - row.settled < 0 && $style.negativeCell]">
              {{ money(row.amount - row.settled) }}
            </td>
            <td>
              <strong :class="$style.dateCell">{{ row.dueDate }}</strong>
              <small
                v-if="row.terms"
                :class="$style.secondaryText"
                >{{ row.terms }}</small
              >
            </td>
            <td>
              <span :class="[$style.status, $style[statusTone(row)]]">{{ row.status }}</span>
            </td>
            <td>{{ row.invoice }}</td>
            <td>{{ row.bill }}</td>
            <td :class="$style.actionCell">
              <button
                type="button"
                title="查看"
                :class="$style.iconButton"
                @click="emit('rowAction', 'view', row)"
              >
                <Eye
                  :size="14"
                  :stroke-width="1.8"
                />
              </button>
              <button
                v-if="row.status !== '已完成' && row.status !== '已结清'"
                type="button"
                :title="side === 'receivable' ? '登记收款' : '登记付款'"
                :class="$style.iconButton"
                @click="emit('rowAction', 'settle', row)"
              >
                <Banknote
                  :size="14"
                  :stroke-width="1.8"
                />
              </button>
              <button
                v-if="row.status.startsWith('逾期')"
                type="button"
                :title="side === 'receivable' ? '发起催收' : '发起催付'"
                :class="$style.iconButton"
                @click="emit('rowAction', 'remind', row)"
              >
                <Bell
                  :size="14"
                  :stroke-width="1.8"
                />
              </button>
              <UiDropdownMenu
                :items="menuItems"
                label="更多"
                @select="emit('rowAction', $event as FinanceAccountRowAction, row)"
              >
                <template #trigger>
                  <button
                    type="button"
                    title="更多"
                    :class="$style.iconButton"
                  >
                    <Ellipsis
                      :size="14"
                      :stroke-width="1.8"
                    />
                  </button>
                </template>
              </UiDropdownMenu>
            </td>
          </tr>
          <tr v-if="!pagedRows.length">
            <td
              colspan="13"
              :class="$style.emptyCell"
            >
              当前筛选下没有账款记录
            </td>
          </tr>
        </tbody>
      </table>
      <div :class="$style.tableFooter">
        <span
          >共 {{ filteredRows.length }} 条 · {{ side === 'receivable' ? '未收' : '未付' }}合计:
          {{ money(filteredRows.reduce((total, row) => total + row.amount - row.settled, 0)) }}</span
        >
        <div :class="$style.pagination">
          <button
            type="button"
            :disabled="page <= 1"
            aria-label="上一页"
            @click="emit('update:page', page - 1)"
          >
            <ChevronLeft :size="14" />
          </button>
          <button
            v-for="index in pageCount"
            :key="index"
            type="button"
            :class="page === index ? $style.pageActive : undefined"
            @click="emit('update:page', index)"
          >
            {{ index }}
          </button>
          <button
            type="button"
            :disabled="page >= pageCount"
            aria-label="下一页"
            @click="emit('update:page', page + 1)"
          >
            <ChevronRight :size="14" />
          </button>
        </div>
      </div>
    </div>

    <div
      v-else
      :class="$style.groupGrid"
    >
      <article
        v-for="group in groupedRows"
        :key="group.label"
        :class="$style.groupCard"
      >
        <header>
          <strong>{{ group.label }}</strong>
          <span>{{ group.count }} 笔</span>
        </header>
        <dl>
          <div>
            <dt>{{ side === 'receivable' ? '应收金额' : '应付金额' }}</dt>
            <dd>{{ money(group.amount) }}</dd>
          </div>
          <div>
            <dt>{{ side === 'receivable' ? '已收' : '已付' }}</dt>
            <dd>{{ money(group.settled) }}</dd>
          </div>
          <div>
            <dt>{{ side === 'receivable' ? '未收' : '未付' }}</dt>
            <dd>{{ money(group.open) }}</dd>
          </div>
        </dl>
      </article>
      <p
        v-if="!groupedRows.length"
        :class="$style.emptyGroup"
      >
        当前筛选下没有汇总数据
      </p>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.ledger {
  @apply min-h-0 flex-1 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.ageTabs {
  @apply w-full flex-wrap gap-0 border-b px-1;
  border-color: var(--ui-border-subtle);
}

.ageTab {
  @apply min-h-10 rounded-none px-4 py-2.5 text-[13px];
}

.ageTab[data-state='active'] {
  @apply font-semibold;
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}

.riskTab[data-state='active'] {
  box-shadow: inset 0 -2px 0 #ff5630;
}

.dangerTab[data-state='active'] {
  color: var(--ui-danger);
  box-shadow: inset 0 -2px 0 var(--ui-danger);
}

.tabCount {
  @apply ml-0.5 text-[10px];
  color: #97a0af;
}

.ageTab[data-state='active'] .tabCount {
  color: var(--app-primary);
}

.riskDot {
  @apply inline-block h-1.5 w-1.5 rounded-full;
  background: #ff5630;
}

.dangerDot {
  background: var(--ui-danger);
}

.toolbar {
  @apply flex flex-wrap items-center justify-between gap-2 border-b px-3 py-2;
  border-color: var(--ui-border-subtle);
}

.viewTabs {
  @apply gap-0 rounded border;
  border-color: var(--ui-border);
}

.viewTab {
  @apply min-h-7 rounded-none px-2.5 py-1.5 text-[11px];
}

.viewTab[data-state='active'] {
  color: white;
  background: var(--app-primary);
}

.filters {
  @apply flex min-w-0 flex-1 flex-wrap items-center justify-end gap-2;
}

.filterSelect {
  @apply min-w-[132px];
}

.advancedButton {
  @apply min-h-8;
}

.advancedPanel {
  @apply absolute z-10 mt-2 flex items-center gap-1 rounded border bg-white p-2 shadow-lg;
  border-color: var(--ui-border);
}

.dateField {
  @apply flex h-8 items-center gap-1 rounded border bg-white px-1.5;
  border-color: var(--ui-border);
}

.dateField :global(input) {
  @apply h-7 w-[126px] border-0 px-1 text-[11px];
  box-shadow: none;
}

.dateSeparator {
  @apply px-0.5 text-[12px];
  color: var(--ui-text-subtle);
}

.searchField {
  @apply flex h-8 min-w-[220px] max-w-[270px] flex-1 items-center gap-1.5 rounded border bg-white px-2;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}

.searchField :global(input) {
  @apply h-7 border-0 px-0 text-[11px];
  box-shadow: none;
}

.clearSearch {
  @apply flex h-5 w-5 items-center justify-center rounded;
  color: var(--ui-text-subtle);
}

.clearSearch:hover {
  background: var(--ui-surface-hover);
}

.exportButton {
  @apply min-h-8;
}

.tableWrap {
  @apply min-h-0 flex-1 overflow-auto;
}

.table {
  @apply w-full min-w-[1510px] border-collapse text-left text-[12px];
}

.table th {
  @apply sticky top-0 z-[1] border-b px-3 py-2.5 font-medium;
  background: #f4f5f7;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}

.table td {
  @apply border-b px-3 py-2.5 align-middle;
  border-color: var(--ui-border-subtle);
  color: var(--ui-text);
}

.table tbody tr:hover {
  background: #fafbfc;
}

.highRiskRow {
  background: #fff8f6;
}

.checkCell {
  @apply w-10 px-2;
}

.codeButton,
.codeLink {
  @apply block text-left text-[13px] font-semibold;
  color: var(--app-primary);
}

.codeButton:hover,
.codeLink:hover {
  text-decoration: underline;
}

.sourceBadge {
  @apply inline-flex rounded px-2 py-1 text-[11px] font-semibold;
  color: #006644;
  background: #e3fcef;
}

.primaryCell {
  @apply block max-w-[190px] truncate text-[12px] font-medium;
}

.secondaryText {
  @apply mt-1 block max-w-[210px] truncate text-[11px];
  color: #97a0af;
}

.moneyCell {
  @apply whitespace-nowrap text-[13px] font-semibold;
}

.receivedCell {
  @apply whitespace-nowrap text-[13px] font-semibold;
  color: var(--ui-success);
}

.negativeCell {
  color: var(--ui-danger);
}

.dateCell {
  @apply block whitespace-nowrap text-[12px] font-medium;
}

.status {
  @apply inline-flex whitespace-nowrap rounded px-2 py-1 text-[11px] font-semibold;
}

.statusSuccess {
  color: #006644;
  background: #e3fcef;
}

.statusWarning {
  color: #974f0c;
  background: #fffae6;
}

.statusDanger {
  color: var(--ui-danger);
  background: #ffebe6;
}

.actionCell {
  @apply whitespace-nowrap;
}

.iconButton {
  @apply inline-flex h-7 w-7 items-center justify-center rounded transition-colors;
  color: var(--ui-text-subtle);
}

.iconButton:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.emptyCell {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}

.tableFooter {
  @apply flex items-center justify-between border-t px-3 py-2 text-[11px];
  border-color: var(--ui-border-subtle);
  color: var(--ui-text-subtle);
}

.pagination {
  @apply flex items-center gap-1;
}

.pagination button {
  @apply flex h-7 min-w-7 items-center justify-center rounded border px-1.5 text-[11px] transition-colors disabled:cursor-not-allowed disabled:opacity-40;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}

.pagination button:hover:not(:disabled) {
  background: var(--ui-surface-hover);
}

.pagination .pageActive {
  color: white;
  background: var(--app-primary);
  border-color: var(--app-primary);
}

.groupGrid {
  @apply grid auto-rows-max grid-cols-3 gap-3 overflow-auto p-3;
}

.groupCard {
  @apply rounded border bg-white p-3;
  border-color: var(--ui-border);
}

.groupCard header {
  @apply flex items-center justify-between border-b pb-2;
  border-color: var(--ui-border-subtle);
}

.groupCard header strong {
  @apply max-w-[75%] truncate text-[13px];
}

.groupCard header span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.groupCard dl {
  @apply mt-2 grid grid-cols-3 gap-2;
}

.groupCard dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.groupCard dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.emptyGroup {
  @apply col-span-full py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}

@media (max-width: 900px) {
  .toolbar {
    @apply items-stretch;
  }

  .filters {
    @apply justify-start;
  }

  .searchField {
    @apply max-w-none;
  }

  .groupGrid {
    @apply grid-cols-2;
  }
}

@media (max-width: 600px) {
  .ageTabs {
    @apply overflow-x-auto;
  }

  .ageTab {
    @apply whitespace-nowrap px-3 text-[11px];
  }

  .filters {
    @apply grid w-full grid-cols-2;
  }

  .filterSelect,
  .searchField,
  .exportButton {
    @apply w-full max-w-none;
  }

  .advancedPanel {
    @apply left-0 right-0 flex-wrap;
  }

  .groupGrid {
    @apply grid-cols-1;
  }
}
</style>
