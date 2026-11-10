<script setup lang="ts">
import { ArrowDown, ArrowUp, ArrowUpDown, ExternalLink, Eye, Minus } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { PurchaseRequestRow, PurchaseRequestSortKey } from '../../model/purchase-requests.types';

defineProps<{
  rows: readonly PurchaseRequestRow[];
  sortKey: PurchaseRequestSortKey | '';
  sortDirection: 'asc' | 'desc';
}>();
const emit = defineEmits<{
  sort: [key: PurchaseRequestSortKey];
  view: [row: PurchaseRequestRow];
  openOrder: [order: string];
}>();

const sortableColumns: readonly { key: PurchaseRequestSortKey; label: string }[] = [
  { key: 'id', label: '申请编号' },
  { key: 'title', label: '申请标题' },
  { key: 'priority', label: '优先级' },
  { key: 'requestor', label: '申请人' },
  { key: 'items', label: '商品数' },
  { key: 'amount', label: '金额' },
  { key: 'status', label: '状态' },
  { key: 'date', label: '日期' },
] as const;

function isSortable(key: string): key is PurchaseRequestSortKey {
  return sortableColumns.some((column) => column.key === key);
}

function columnLabel(key: string) {
  const sortable = sortableColumns.find((column) => column.key === key);
  if (sortable) return sortable.label;
  if (key === 'project') return '关联项目';
  if (key === 'orders') return '采购订单';
  if (key === 'action') return '操作';
  return key;
}
</script>

<template>
  <section :class="$style.panel">
    <div :class="$style.scroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th
              v-for="key in [
                'id',
                'title',
                'priority',
                'project',
                'requestor',
                'items',
                'amount',
                'status',
                'orders',
                'date',
                'action',
              ]"
              :key="key"
              :class="isSortable(key) && $style.sortable"
              @click="isSortable(key) && emit('sort', key)"
            >
              <span>
                {{ columnLabel(key) }}
                <ArrowUp
                  v-if="sortKey === key && sortDirection === 'asc'"
                  :size="11"
                  :stroke-width="1.8"
                />
                <ArrowDown
                  v-else-if="sortKey === key && sortDirection === 'desc'"
                  :size="11"
                  :stroke-width="1.8"
                />
                <ArrowUpDown
                  v-else-if="isSortable(key)"
                  :class="$style.sortHint"
                  :size="11"
                  :stroke-width="1.8"
                />
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
            :data-status="row.status"
          >
            <td>
              <button
                type="button"
                :class="$style.code"
                @click="emit('view', row)"
              >
                {{ row.id }}
              </button>
            </td>
            <td :class="$style.title">{{ row.title }}</td>
            <td>
              <span
                :class="$style.priority"
                :data-priority="row.priority"
              >
                <ArrowUp
                  v-if="row.priority === '高'"
                  :size="14"
                  :stroke-width="1.8"
                />
                <Minus
                  v-else-if="row.priority === '中'"
                  :size="14"
                  :stroke-width="1.8"
                />
                <ArrowDown
                  v-else
                  :size="14"
                  :stroke-width="1.8"
                />
                {{ row.priority }}
              </span>
            </td>
            <td :class="row.project === '-' ? $style.muted : $style.project">
              {{ row.project === '-' ? '库存采购' : row.project }}
            </td>
            <td>{{ row.requestor }}</td>
            <td>{{ row.items }}</td>
            <td :class="$style.amount">¥{{ row.amount.toLocaleString('en-US') }}</td>
            <td>
              <span :class="$style.status">{{ row.status }}</span>
            </td>
            <td>
              <div
                v-if="row.poList.length"
                :class="$style.orders"
              >
                <button
                  v-for="order in row.poList"
                  :key="order"
                  type="button"
                  @click="emit('openOrder', order)"
                >
                  {{ order.replace('PO-2026-', '') }}
                  <ExternalLink
                    :size="9"
                    :stroke-width="1.8"
                  />
                </button>
              </div>
              <span
                v-else
                :class="$style.empty"
                >-</span
              >
            </td>
            <td :class="$style.date">{{ row.date }}</td>
            <td>
              <UiButton
                variant="ghost"
                size="icon-sm"
                :aria-label="`查看${row.id}`"
                title="查看详情"
                @click="emit('view', row)"
              >
                <Eye
                  :size="14"
                  :stroke-width="1.8"
                />
              </UiButton>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <footer :class="$style.footer">
      <span>共 {{ rows.length }} 条记录</span>
      <div :class="$style.pagination">
        <button
          type="button"
          aria-label="上一页"
          disabled
        >
          ‹
        </button>
        <strong>1</strong>
        <button
          type="button"
          aria-label="下一页"
          disabled
        >
          ›
        </button>
      </div>
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}
.table {
  @apply w-full min-w-[1200px] border-collapse;
}
.table th {
  @apply border-b px-5 py-3 text-left text-[11px] font-semibold;
  color: #5e6c84;
  background: var(--ui-surface-hover);
  border-color: var(--ui-border);
}
.table th span {
  @apply inline-flex items-center;
}
.sortable {
  @apply cursor-pointer select-none;
}
.sortHint {
  @apply ml-0.5 opacity-0 transition-opacity;
  color: #c1c7d0;
}
.sortable:hover .sortHint {
  @apply opacity-100;
}
.table td {
  @apply whitespace-nowrap border-b px-5 py-3 text-[13px];
  color: var(--ui-text);
  border-color: var(--ui-surface-hover);
}
.table tbody tr:hover td {
  background: var(--ui-surface-subtle);
}
.table th:nth-child(3),
.table th:nth-child(6),
.table th:nth-child(8),
.table th:nth-child(9),
.table th:nth-child(11),
.table td:nth-child(3),
.table td:nth-child(6),
.table td:nth-child(8),
.table td:nth-child(9),
.table td:nth-child(11) {
  @apply text-center;
}
.table th:nth-child(7),
.table td:nth-child(7) {
  @apply text-right;
}
.code {
  @apply text-[13px] font-medium hover:underline;
  color: var(--app-primary);
}
.title {
  @apply min-w-[180px];
}
.priority {
  @apply inline-flex items-center gap-1 text-[11px];
  color: var(--ui-text-subtle);
}
.priority[data-priority='高'] svg {
  color: #ff5630;
}
.priority[data-priority='中'] svg {
  color: #ffab00;
}
.priority[data-priority='低'] svg {
  color: #0065ff;
}
.project {
  color: #0065ff !important;
}
.muted,
.date {
  color: var(--ui-text-subtle) !important;
}
.amount {
  @apply font-medium;
}
.status {
  @apply inline-block rounded px-2.5 py-1 text-[11px] font-semibold;
}
tr[data-status='草稿'] .status {
  color: #6b778c;
  background: #ebecf0;
}
tr[data-status='审批中'] .status {
  color: #ffab00;
  background: #fffae6;
}
tr[data-status='询价中'] .status {
  color: #0065ff;
  background: #e6fcff;
}
tr[data-status='比价中'] .status {
  color: #6554c0;
  background: #eae6ff;
}
tr[data-status='采购中'] .status {
  color: #0052cc;
  background: #deebff;
}
tr[data-status='已完成'] .status {
  color: #00875a;
  background: #e3fcef;
}
tr[data-status='已驳回'] .status {
  color: #ff5630;
  background: #ffebe6;
}
.orders {
  @apply flex flex-wrap items-center justify-center gap-1;
}
.orders button {
  @apply inline-flex items-center gap-0.5 rounded px-1.5 py-0.5 text-[10px] font-medium transition-colors;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.orders button:hover {
  background: #b3d4ff;
}
.empty {
  color: #c1c7d0;
}
.footer {
  @apply flex items-center justify-between border-t px-5 py-4 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.pagination {
  @apply flex items-center gap-1;
}
.pagination button,
.pagination strong {
  @apply flex h-8 w-8 items-center justify-center rounded;
}
.pagination button {
  color: var(--ui-text-subtle);
}
.pagination strong {
  @apply text-white;
  background: var(--app-primary);
}
@media (prefers-reduced-motion: reduce) {
  .sortHint {
    transition: none;
  }
}
</style>
