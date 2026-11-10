<script setup lang="ts">
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  ExternalLink,
  Eye,
  FileText,
  Link2,
  Plus,
  ShoppingCart,
} from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { InquiryRow, InquirySortKey } from '../../model/inquiries.types';

defineProps<{ rows: readonly InquiryRow[]; sortKey: InquirySortKey | ''; sortDirection: 'asc' | 'desc' }>();
const emit = defineEmits<{ sort: [key: InquirySortKey]; view: [row: InquiryRow]; copyLink: [row: InquiryRow] }>();
const columns: readonly { key: string; label: string; sortable?: boolean }[] = [
  { key: 'id', label: '询价单号', sortable: true },
  { key: 'title', label: '标题', sortable: true },
  { key: 'source', label: '来源' },
  { key: 'buyer', label: '负责采购' },
  { key: 'supplierCount', label: '供应商', sortable: true },
  { key: 'materials', label: '物料数' },
  { key: 'totalBudget', label: '预算金额', sortable: true },
  { key: 'status', label: '状态', sortable: true },
  { key: 'deadline', label: '截止日期', sortable: true },
  { key: 'action', label: '操作' },
];
function isSortable(key: string): key is InquirySortKey {
  return columns.some((item) => item.key === key && item.sortable);
}
</script>

<template>
  <section :class="$style.panel">
    <div :class="$style.scroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th
              v-for="column in columns"
              :key="column.key"
              :class="column.sortable && $style.sortable"
              @click="isSortable(column.key) && emit('sort', column.key)"
            >
              <span
                >{{ column.label }}
                <ArrowUp
                  v-if="sortKey === column.key && sortDirection === 'asc'"
                  :size="11"
                  :stroke-width="1.8"
                />
                <ArrowDown
                  v-else-if="sortKey === column.key && sortDirection === 'desc'"
                  :size="11"
                  :stroke-width="1.8"
                />
                <ArrowUpDown
                  v-else-if="column.sortable"
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
            @click="emit('view', row)"
          >
            <td>
              <button
                type="button"
                :class="$style.code"
                @click.stop="emit('view', row)"
              >
                {{ row.id }}
              </button>
            </td>
            <td>
              <strong :class="$style.title">{{ row.title }}</strong
              ><span
                v-if="row.projectName !== '-'"
                :class="$style.subline"
                >{{ row.projectName }}</span
              >
            </td>
            <td>
              <span :class="$style.source"
                ><ShoppingCart
                  v-if="row.sourceType === '采购申请'"
                  :size="10"
                  :stroke-width="1.8"
                /><Plus
                  v-else-if="row.sourceType === '手动创建'"
                  :size="10"
                  :stroke-width="1.8"
                /><FileText
                  v-else
                  :size="10"
                  :stroke-width="1.8"
                />{{ row.sourceType }}</span
              ><span
                v-if="row.sourceId !== '-'"
                :class="$style.sourceId"
                >{{ row.sourceId }}</span
              >
            </td>
            <td>
              <span
                v-if="row.assignedBuyer"
                :class="$style.buyer"
                ><i>{{ row.assignedBuyer.charAt(0) }}</i
                >{{ row.assignedBuyer }}</span
              ><span
                v-else
                :class="$style.muted"
                >未分配</span
              >
            </td>
            <td>
              <strong :class="$style.quoteCount">{{ row.quotedCount }}/{{ row.supplierCount }}</strong
              ><span :class="$style.subline">{{ row.quotedCount === row.supplierCount ? '全部已报' : '报价中' }}</span>
            </td>
            <td>{{ row.materialCount }}</td>
            <td :class="$style.amount">¥{{ row.totalBudget.toLocaleString('en-US') }}</td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td :class="$style.deadline">{{ row.deadline }}<span>已过期</span></td>
            <td>
              <div :class="$style.actions">
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  title="查看详情"
                  :aria-label="`查看${row.id}`"
                  @click.stop="emit('view', row)"
                  ><Eye
                    :size="14"
                    :stroke-width="1.8"
                /></UiButton>
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  title="复制供应商报价链接"
                  :aria-label="`复制${row.id}报价链接`"
                  @click.stop="emit('copyLink', row)"
                  ><Link2
                    :size="13"
                    :stroke-width="1.8"
                /></UiButton>
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  title="新窗口打开"
                  :aria-label="`打开${row.id}`"
                  @click.stop="emit('view', row)"
                  ><ExternalLink
                    :size="13"
                    :stroke-width="1.8"
                /></UiButton>
              </div>
            </td>
          </tr>
          <tr v-if="rows.length === 0">
            <td
              colspan="10"
              :class="$style.empty"
            >
              暂无匹配的询价单
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
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}
.table {
  @apply w-full min-w-[1100px] border-collapse;
}
.table th {
  @apply border-b px-5 py-3 text-left text-[11px] font-semibold;
  color: #5e6c84;
  background: #f4f5f7;
  border-color: var(--ui-border);
}
.table th span {
  @apply inline-flex items-center;
}
.sortable {
  @apply cursor-pointer select-none;
}
.sortHint {
  @apply ml-0.5 opacity-0;
  color: #c1c7d0;
}
.sortable:hover .sortHint {
  @apply opacity-100;
}
.table td {
  @apply whitespace-nowrap border-b px-5 py-3 text-[13px];
  color: #172b4d;
  border-color: #f4f5f7;
}
.table tbody tr {
  @apply cursor-pointer transition-colors;
}
.table tbody tr:hover td {
  background: #fafbfc;
}
.table th:nth-child(3),
.table th:nth-child(4),
.table th:nth-child(5),
.table th:nth-child(6),
.table th:nth-child(8),
.table th:nth-child(10),
.table td:nth-child(3),
.table td:nth-child(4),
.table td:nth-child(5),
.table td:nth-child(6),
.table td:nth-child(8),
.table td:nth-child(10) {
  @apply text-center;
}
.table th:nth-child(7),
.table td:nth-child(7) {
  @apply text-right;
}
.code {
  @apply font-medium hover:underline;
  color: #0052cc;
}
.title {
  @apply block min-w-[180px] font-normal;
}
.subline {
  @apply mt-0.5 block text-[10px] font-normal;
  color: #6b778c;
}
.source {
  @apply inline-flex items-center gap-1 rounded px-2 py-0.5 text-[10px] font-medium;
  color: #42526e;
  background: #f4f5f7;
}
.sourceId {
  @apply mt-0.5 block text-[10px];
  color: #0052cc;
}
.buyer {
  @apply inline-flex items-center gap-1.5 text-[12px];
}
.buyer i {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[9px] font-bold not-italic text-white;
  background: #0052cc;
}
.muted {
  @apply text-[11px];
  color: #97a0af;
}
.quoteCount {
  @apply block font-normal;
}
.amount {
  @apply font-medium;
}
.status {
  @apply rounded px-2.5 py-1 text-[11px] font-semibold;
  color: #42526e;
  background: #ebecf0;
}
.status[data-status='已完成比价'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已转采购单'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-status='待发布'] {
  color: #42526e;
  background: #ebecf0;
}
.status[data-status='报价收集中'] {
  color: #0052cc;
  background: #deebff;
}
.deadline {
  color: #6b778c !important;
}
.deadline span {
  @apply ml-1 text-[10px];
  color: #ff5630;
}
.actions {
  @apply flex items-center justify-center gap-1;
}
.actions button:first-child {
  color: #0052cc;
}
.empty {
  @apply py-12;
  color: #97a0af !important;
  text-align: center !important;
}
</style>
