<script setup lang="ts">
import { Eye, Lock } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { CreditLimitRow, CreditView, PendingCreditRequest } from '../../model/credit-limits.types';

defineProps<{
  view: CreditView;
  rows: readonly CreditLimitRow[];
  pendingRows: readonly PendingCreditRequest[];
}>();
const emit = defineEmits<{
  viewRow: [row: CreditLimitRow];
  viewRequest: [row: PendingCreditRequest];
}>();

function money(value: number) {
  return value ? `¥${value.toLocaleString('zh-CN')}` : '-';
}
function percent(row: CreditLimitRow) {
  return `${Math.round((row.used / row.limit) * 100)}%`;
}
function usageBucket(row: CreditLimitRow) {
  const value = Math.round((row.used / row.limit) * 100);
  return `usage${Math.min(100, Math.max(0, Math.round(value / 5) * 5))}`;
}
</script>

<template>
  <div :class="$style.tableScroller">
    <table
      v-if="view === 'customers'"
      :class="$style.table"
    >
      <thead>
        <tr>
          <th>客户名称</th>
          <th>类型</th>
          <th>授信额度</th>
          <th>已用额度</th>
          <th>可用额度</th>
          <th>使用率</th>
          <th>超期金额</th>
          <th>超期笔数</th>
          <th>状态</th>
          <th>冻结状态</th>
          <th>负责人</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong :class="$style.customer"
              ><Lock
                v-if="row.freezeStatus === '已冻结'"
                :size="13"
              />{{ row.customer }}</strong
            >
          </td>
          <td>{{ row.customerType }}</td>
          <td>
            <strong>{{ money(row.limit) }}</strong>
          </td>
          <td>{{ money(row.used) }}</td>
          <td>
            <strong :class="$style.available">{{ money(row.limit - row.used) }}</strong>
          </td>
          <td>
            <span :class="$style.usage"
              ><i :class="$style[usageBucket(row)]" /><small>{{ percent(row) }}</small></span
            >
          </td>
          <td>
            <strong :class="$style.overdue">{{ money(row.overdueAmount) }}</strong>
          </td>
          <td>
            <span :class="$style.overdueCount">{{ row.overdueCount ? `${row.overdueCount} 笔` : '-' }}</span>
          </td>
          <td>
            <span
              :class="$style.badge"
              :data-risk="row.riskStatus"
              >{{ row.riskStatus }}</span
            >
          </td>
          <td>
            <span
              :class="$style.badge"
              :data-freeze="row.freezeStatus"
              ><Lock
                v-if="row.freezeStatus === '已冻结'"
                :size="11"
              />{{ row.freezeStatus }}</span
            >
          </td>
          <td>{{ row.owner }}</td>
          <td>
            <UiButton
              variant="link"
              size="xs"
              @click="emit('viewRow', row)"
              ><Eye
                :size="13"
                :stroke-width="1.8"
              />查看</UiButton
            >
          </td>
        </tr>
        <tr v-if="!rows.length">
          <td
            :class="$style.empty"
            colspan="12"
          >
            当前筛选下没有授信客户
          </td>
        </tr>
      </tbody>
    </table>
    <table
      v-else
      :class="$style.table"
    >
      <thead>
        <tr>
          <th>申请编号</th>
          <th>客户名称</th>
          <th>类型</th>
          <th>当前额度</th>
          <th>申请额度</th>
          <th>申请人</th>
          <th>申请日期</th>
          <th>申请原因</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in pendingRows"
          :key="row.id"
        >
          <td>
            <strong :class="$style.code">{{ row.id }}</strong>
          </td>
          <td>
            <strong :class="$style.customer">{{ row.customer }}</strong>
          </td>
          <td>{{ row.customerType }}</td>
          <td>{{ money(row.currentLimit) }}</td>
          <td>
            <strong :class="$style.available">{{ money(row.requestedLimit) }}</strong>
          </td>
          <td>{{ row.owner }}</td>
          <td>{{ row.requestedAt }}</td>
          <td>{{ row.reason }}</td>
          <td>
            <span
              :class="$style.badge"
              data-risk="预警"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <UiButton
              variant="link"
              size="xs"
              @click="emit('viewRequest', row)"
              >审批</UiButton
            >
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.tableScroller {
  @apply overflow-x-auto;
}
.table {
  @apply w-full border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-10 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.table td {
  @apply h-[59px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.customer {
  @apply flex items-center gap-1 font-semibold;
  color: var(--ui-text);
}
.customer svg {
  color: var(--ui-danger);
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.available {
  color: var(--ui-success);
}
.overdue,
.overdueCount {
  color: var(--ui-danger);
}
.usage {
  @apply flex items-center gap-2;
}
.usage i {
  @apply relative block h-1.5 w-14 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}
.usage i::after {
  @apply absolute inset-y-0 left-0 rounded-full;
  content: '';
  background: var(--ui-success);
}
.usage0::after {
  width: 0%;
}
.usage5::after {
  width: 5%;
}
.usage10::after {
  width: 10%;
}
.usage15::after {
  width: 15%;
}
.usage20::after {
  width: 20%;
}
.usage25::after {
  width: 25%;
}
.usage30::after {
  width: 30%;
}
.usage35::after {
  width: 35%;
}
.usage40::after {
  width: 40%;
}
.usage45::after {
  width: 45%;
}
.usage50::after {
  width: 50%;
}
.usage55::after {
  width: 55%;
}
.usage60::after {
  width: 60%;
}
.usage65::after {
  width: 65%;
}
.usage70::after {
  width: 70%;
}
.usage75::after {
  width: 75%;
}
.usage80::after {
  width: 80%;
}
.usage85::after {
  width: 85%;
}
.usage90::after {
  width: 90%;
}
.usage95::after {
  width: 95%;
}
.usage100::after {
  width: 100%;
}
.usage small {
  color: var(--ui-text-subtle);
}
.badge {
  @apply inline-flex items-center gap-1 whitespace-nowrap rounded-full px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.badge[data-risk='正常'] {
  color: var(--ui-success);
  background: #e3fcef;
}
.badge[data-risk='预警'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.badge[data-risk='超限'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.badge[data-freeze='已冻结'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.empty {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
