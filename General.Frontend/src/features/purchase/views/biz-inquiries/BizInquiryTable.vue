<script setup lang="ts">
import { ArrowUpDown, Eye, UserRoundPlus } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { BizInquiryRow, BizInquirySortKey } from '../../model/biz-inquiries.types';

defineProps<{ rows: readonly BizInquiryRow[]; sortKey: BizInquirySortKey | '' }>();
const emit = defineEmits<{
  sort: [key: BizInquirySortKey];
  view: [row: BizInquiryRow];
  assign: [row: BizInquiryRow];
}>();
const headers = [
  ['id', '询价单号'],
  ['title', '标题/客户'],
  ['sales', '负责销售'],
  ['priority', '优先级'],
  ['buyer', '分配采购'],
  ['status', '处理状态'],
  ['result', '结果'],
  ['materials', '物料'],
  ['quotes', '报价区间'],
  ['estimatedAmount', '预估金额'],
  ['deadline', '截止日期'],
  ['action', '操作'],
] as const;
function sortable(key: string): key is BizInquirySortKey {
  return ['id', 'title', 'estimatedAmount', 'deadline'].includes(key);
}
</script>

<template>
  <section :class="$style.panel">
    <div :class="$style.scroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th
              v-for="header in headers"
              :key="header[0]"
              :class="sortable(header[0]) && $style.sortable"
              @click="sortable(header[0]) && emit('sort', header[0])"
            >
              <span
                >{{ header[1]
                }}<ArrowUpDown
                  v-if="sortable(header[0])"
                  :size="10"
                  :stroke-width="1.8"
                  :class="sortKey === header[0] && $style.sortActive"
              /></span>
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
              ><span :class="$style.subline">{{ row.customerName }}</span>
            </td>
            <td>
              <span :class="$style.person"
                ><i>{{ row.salesPerson.charAt(0) }}</i
                >{{ row.salesPerson }}</span
              >
            </td>
            <td>
              <span
                :class="$style.priority"
                :data-priority="row.priority"
                >{{ row.priority }}</span
              >
            </td>
            <td>
              <span
                v-if="row.assignedBuyer"
                :class="$style.person"
                ><i :class="$style.buyerAvatar">{{ row.assignedBuyer.charAt(0) }}</i
                >{{ row.assignedBuyer }}</span
              ><button
                v-else
                type="button"
                :class="$style.assign"
                @click.stop="emit('assign', row)"
              >
                <UserRoundPlus
                  :size="12"
                  :stroke-width="1.8"
                />分配
              </button>
            </td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>
              <span
                :class="$style.result"
                :data-result="row.result"
                >{{ row.result }}</span
              >
            </td>
            <td>{{ row.materialCount }}</td>
            <td>
              <template v-if="row.quoteMin !== null"
                ><strong :class="$style.quote">¥{{ row.quoteMin.toLocaleString('en-US') }}</strong
                ><span :class="$style.subline"
                  >¥{{ row.quoteMin.toLocaleString('en-US') }} ~ ¥{{ row.quoteMax?.toLocaleString('en-US') }}</span
                ><span :class="$style.subline">{{ row.quoteCount }}家报价</span></template
              ><span
                v-else
                :class="$style.muted"
                >暂无报价</span
              >
            </td>
            <td :class="$style.amount">¥{{ row.estimatedAmount.toLocaleString('en-US') }}</td>
            <td :class="$style.deadline">{{ row.deadline }}<span>已过期</span></td>
            <td>
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
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              colspan="12"
              :class="$style.empty"
            >
              暂无匹配的业务询价
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
  @apply w-full min-w-[1380px] border-collapse;
}
.table th {
  @apply border-b px-4 py-3 text-left text-[11px] font-semibold;
  color: #5e6c84;
  background: #fff;
  border-color: var(--ui-border);
}
.table th span {
  @apply inline-flex items-center gap-0.5;
}
.sortable {
  @apply cursor-pointer select-none;
}
.sortActive {
  color: #0052cc;
}
.table td {
  @apply whitespace-nowrap border-b px-4 py-3 text-[12px];
  color: #172b4d;
  border-color: #f4f5f7;
}
.table tbody tr {
  @apply cursor-pointer transition-colors;
}
.table tbody tr:hover td {
  background: #fafbfc;
}
.table th:nth-child(n + 3),
.table td:nth-child(n + 3) {
  @apply text-center;
}
.table th:nth-child(9),
.table td:nth-child(9),
.table th:nth-child(10),
.table td:nth-child(10) {
  @apply text-right;
}
.code {
  @apply text-[13px] font-medium hover:underline;
  color: #0052cc;
}
.title {
  @apply block min-w-[190px] font-semibold;
}
.subline {
  @apply mt-0.5 block text-[10px] font-normal;
  color: #6b778c;
}
.person {
  @apply inline-flex items-center gap-1.5;
}
.person i {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[9px] font-bold not-italic text-white;
  background: #0052cc;
}
.person .buyerAvatar {
  background: #00875a;
}
.priority,
.status,
.result {
  @apply rounded px-2 py-1 text-[10px] font-semibold;
}
.priority {
  color: #ff8b00;
  background: #fff0e0;
}
.priority[data-priority='紧急'] {
  color: #de350b;
  background: #ffebe6;
}
.priority[data-priority='低'] {
  color: #6b778c;
  background: #f4f5f7;
}
.status {
  color: #42526e;
  background: #ebecf0;
}
.status[data-status='报价中'] {
  color: #ff8b00;
  background: #fff7e6;
}
.status[data-status='已报价'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已完成'] {
  color: #6554c0;
  background: #eae6ff;
}
.result {
  color: #6b778c;
  background: #ebecf0;
}
.result[data-result='中标'] {
  color: #00875a;
  background: #e3fcef;
}
.result[data-result='未中'] {
  color: #de350b;
  background: #ffebe6;
}
.assign {
  @apply inline-flex items-center gap-1 rounded border border-dashed px-2 py-1 text-[11px];
  color: #6b778c;
  border-color: #c1c7d0;
}
.quote {
  @apply block text-[12px];
  color: #00875a;
}
.muted {
  @apply text-[11px];
  color: #97a0af;
}
.amount {
  @apply font-semibold;
}
.deadline {
  @apply font-semibold;
  color: #de350b !important;
}
.deadline span {
  @apply mt-0.5 block text-[10px] font-normal;
}
.empty {
  @apply py-12;
  text-align: center !important;
  color: #97a0af !important;
}
</style>
