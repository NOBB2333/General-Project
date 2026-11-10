<script setup lang="ts">
import { ChevronLeft, ChevronRight, Download, Eye, Paperclip, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { SalesReturnRow, SalesReturnStatusFilter } from '../../model/sales-returns.types';

defineProps<{
  rows: readonly SalesReturnRow[];
  activeStatus: SalesReturnStatusFilter;
  query: string;
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  export: [];
  create: [];
  view: [row: SalesReturnRow];
}>();

const statuses: readonly SalesReturnStatusFilter[] = ['全部', '待审核', '已审核', '已退货', '已退款'];

function currency(value: number) {
  return `-¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.tablePanel">
    <header :class="$style.toolbar">
      <UiTabsList
        label="销售退货状态"
        :class="$style.statusTabs"
      >
        <UiTabsTrigger
          v-for="status in statuses"
          :key="status"
          :value="status"
          :class="$style.statusTab"
        >
          {{ status }}
        </UiTabsTrigger>
      </UiTabsList>

      <div :class="$style.tools">
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            :model-value="query"
            placeholder="搜索退货单号..."
            aria-label="搜索销售退货"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
        <UiButton
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
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
        >
          <template #leading>
            <Plus
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          新建退货
        </UiButton>
      </div>
    </header>

    <div :class="$style.tableScroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>退货单号</th>
            <th>销售单号</th>
            <th>客户</th>
            <th>退货原因</th>
            <th>数量</th>
            <th>金额</th>
            <th>状态</th>
            <th>附件</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <strong :class="$style.linkValue">{{ row.code }}</strong>
            </td>
            <td>
              <strong :class="$style.linkValue">{{ row.orderCode }}</strong>
            </td>
            <td>
              <strong :class="$style.customer">{{ row.customer }}</strong>
            </td>
            <td>
              <span :class="$style.reasonBadge">{{ row.reason }}</span>
            </td>
            <td>{{ row.quantity }}</td>
            <td>
              <strong :class="$style.amount">{{ currency(row.amount) }}</strong>
            </td>
            <td>
              <span
                :class="[
                  $style.statusBadge,
                  row.status === '待审核' && $style.statusPending,
                  row.status === '已审核' && $style.statusReviewed,
                  row.status === '已退货' && $style.statusReturned,
                  row.status === '已退款' && $style.statusRefunded,
                ]"
              >
                {{ row.status }}
              </span>
            </td>
            <td>
              <span :class="$style.attachment">
                <Paperclip
                  :size="13"
                  :stroke-width="1.8"
                />
                {{ row.attachments }}
              </span>
            </td>
            <td>
              <UiButton
                :class="$style.viewButton"
                variant="ghost"
                size="icon-sm"
                title="查看退货详情"
                aria-label="查看退货详情"
                @click="emit('view', row)"
              >
                <Eye
                  :size="14"
                  :stroke-width="1.8"
                />
              </UiButton>
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.emptyCell"
              colspan="9"
            >
              当前筛选下没有销售退货
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer :class="$style.footer">
      <span>共 {{ rows.length }} 条</span>
      <div :class="$style.pagination">
        <UiButton
          variant="ghost"
          size="icon-sm"
          disabled
          aria-label="上一页"
        >
          <ChevronLeft
            :size="14"
            :stroke-width="1.8"
          />
        </UiButton>
        <strong>1</strong>
        <UiButton
          variant="ghost"
          size="icon-sm"
          disabled
          aria-label="下一页"
        >
          <ChevronRight
            :size="14"
            :stroke-width="1.8"
          />
        </UiButton>
      </div>
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.tablePanel {
  @apply mt-5 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.toolbar {
  @apply flex min-h-[38px] items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.statusTabs {
  @apply gap-1;
}

.statusTab {
  @apply min-h-[36px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-muted);
}

.statusTab:hover,
.statusTab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.statusTab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}

.tools {
  @apply flex shrink-0 items-center gap-2;
}

.searchField {
  @apply flex h-8 w-[255px] items-center gap-2 rounded border px-2;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  box-shadow: 0 0 0 1px var(--app-primary-light);
}

.searchField input {
  @apply h-full border-0 p-0 shadow-none;
}

.tableScroller {
  @apply overflow-x-auto;
}

.table {
  @apply w-full min-w-[1180px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-[39px] border-b px-5 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[47px] border-b px-5 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table th:first-child,
.table td:first-child {
  @apply w-[165px];
}

.table th:nth-child(2),
.table td:nth-child(2) {
  @apply w-[165px];
}

.table th:nth-child(3),
.table td:nth-child(3) {
  @apply w-[190px];
}

.linkValue {
  @apply whitespace-nowrap font-semibold;
  color: #0065ff;
}

.customer {
  color: var(--ui-text);
}

.reasonBadge {
  @apply inline-flex rounded px-2 py-0.5 font-semibold;
  color: #ff5630;
  background: #ffebe6;
}

.amount {
  @apply whitespace-nowrap;
  color: #ff5630;
}

.statusBadge {
  @apply inline-flex rounded px-2 py-1 font-semibold;
}

.statusPending {
  color: #974f0c;
  background: #fff0b3;
}

.statusReviewed {
  color: #0052cc;
  background: #deebff;
}

.statusReturned {
  color: #006644;
  background: #e3fcef;
}

.statusRefunded {
  color: #0052cc;
  background: #deebff;
}

.attachment {
  @apply inline-flex items-center gap-1;
  color: var(--ui-text-subtle);
}

.viewButton {
  @apply opacity-0 transition-opacity;
}

.table tr:hover .viewButton,
.viewButton:focus-visible {
  @apply opacity-100;
}

.emptyCell {
  @apply h-28 text-center;
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex min-h-[55px] items-center justify-between px-5 text-[11px];
  color: var(--ui-text-subtle);
}

.pagination {
  @apply flex items-center gap-1;
}

.pagination strong {
  @apply inline-flex h-8 min-w-8 items-center justify-center rounded px-2 text-[11px] text-white;
  background: var(--app-primary);
}
</style>
