<script setup lang="ts">
import { Eye, Link2, Pencil, ShoppingCart, Trash2 } from 'lucide-vue-next';
import type {
  SpecialPriceApprovalStatus,
  SpecialPriceOrderStatus,
  SpecialPriceRecord,
  SpecialPriceUrgency,
} from '../../model/special-prices.types';

defineProps<{ rows: readonly SpecialPriceRecord[] }>();
const emit = defineEmits<{
  view: [row: SpecialPriceRecord];
  edit: [row: SpecialPriceRecord];
  delete: [row: SpecialPriceRecord];
  order: [row: SpecialPriceRecord];
}>();

function approvalTone(value: SpecialPriceApprovalStatus) {
  return { 草稿: 'neutral', 审批中: 'info', 已通过: 'success', 已驳回: 'danger', 已撤回: 'neutral' }[value];
}

function urgencyTone(value: SpecialPriceUrgency) {
  return { 普通: 'neutral', 加急: 'warning', 特急: 'danger' }[value];
}

function orderTone(value: SpecialPriceOrderStatus) {
  return { 未下单: 'neutral', 已下单: 'success', 部分下单: 'warning', 已作废: 'danger' }[value];
}

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function discount(row: SpecialPriceRecord) {
  if (!row.totalNormalAmount) return '0%';
  return `${((row.totalLostAmount / row.totalNormalAmount) * 100).toFixed(1)}%`;
}
</script>

<template>
  <section
    :class="$style.surface"
    aria-label="特价申请列表"
  >
    <div :class="$style.tableWrap">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>特价编号</th>
            <th>客户名称</th>
            <th>来源报价单</th>
            <th :class="$style.center">物料数</th>
            <th :class="$style.amount">正常总价</th>
            <th :class="$style.amount">申请总价</th>
            <th :class="$style.amount">让利金额</th>
            <th :class="$style.amount">让利率</th>
            <th :class="$style.center">紧急程度</th>
            <th :class="$style.center">审批状态</th>
            <th :class="$style.center">下单状态</th>
            <th>关联单号</th>
            <th>申请人</th>
            <th>申请时间</th>
            <th :class="$style.center">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <button
                type="button"
                :class="$style.code"
                @click="emit('view', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td :class="$style.customer">{{ row.customer }}</td>
            <td :class="$style.muted">{{ row.sourceQuotationCode || '—' }}</td>
            <td :class="$style.center">{{ row.itemCount }}</td>
            <td :class="$style.amount">{{ currency(row.totalNormalAmount) }}</td>
            <td :class="$style.amount">{{ currency(row.totalRequestedAmount) }}</td>
            <td :class="[$style.amount, $style.lost]">{{ currency(row.totalLostAmount) }}</td>
            <td :class="[$style.amount, Number.parseFloat(discount(row)) > 10 && $style.highDiscount]">
              {{ discount(row) }}
            </td>
            <td :class="$style.center">
              <span :class="[$style.badge, $style[urgencyTone(row.urgency)]]">{{ row.urgency }}</span>
            </td>
            <td :class="$style.center">
              <span :class="[$style.badge, $style[approvalTone(row.status)]]">{{ row.status }}</span>
            </td>
            <td :class="$style.center">
              <span :class="[$style.badge, $style[orderTone(row.orderStatus)]]">{{ row.orderStatus }}</span>
            </td>
            <td>
              <button
                v-if="row.linkedOrderCode"
                type="button"
                :class="$style.link"
                @click="emit('view', row)"
              >
                <Link2
                  :size="12"
                  :stroke-width="1.8"
                />{{ row.linkedOrderCode }}
              </button>
              <span
                v-else
                :class="$style.muted"
                >—</span
              >
            </td>
            <td>{{ row.createdBy }}</td>
            <td :class="$style.muted">{{ row.createdAt }}</td>
            <td>
              <div :class="$style.actions">
                <button
                  type="button"
                  title="查看详情"
                  @click="emit('view', row)"
                >
                  <Eye
                    :size="13"
                    :stroke-width="1.8"
                  />查看
                </button>
                <button
                  v-if="row.status === '已通过' && row.orderStatus === '未下单'"
                  type="button"
                  :class="$style.order"
                  title="按此特价下单"
                  @click="emit('order', row)"
                >
                  <ShoppingCart
                    :size="13"
                    :stroke-width="1.8"
                  />下单
                </button>
                <template v-if="row.status === '草稿'">
                  <button
                    type="button"
                    title="编辑"
                    @click="emit('edit', row)"
                  >
                    <Pencil
                      :size="13"
                      :stroke-width="1.8"
                    />编辑
                  </button>
                  <button
                    type="button"
                    :class="$style.remove"
                    title="删除"
                    @click="emit('delete', row)"
                  >
                    <Trash2
                      :size="13"
                      :stroke-width="1.8"
                    />删除
                  </button>
                </template>
              </div>
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              colspan="15"
              :class="$style.empty"
            >
              暂无数据
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.surface {
  @apply overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border-subtle);
}
.tableWrap {
  @apply overflow-x-auto;
}
.table {
  @apply w-full min-w-[1370px] border-collapse text-left text-[11.5px];
  color: var(--ui-text);
}
.table thead {
  background: #fafbfc;
}
.table th {
  @apply whitespace-nowrap px-3 py-3 text-[10.5px] font-semibold;
  color: #5e6c84;
}
.table td {
  @apply h-[54px] whitespace-nowrap border-t px-3 py-2;
  border-color: var(--ui-border-subtle);
}
.table tbody tr {
  @apply transition-colors;
}
.table tbody tr:hover {
  background: var(--ui-surface-subtle);
}
.center {
  @apply text-center;
}
.amount {
  @apply text-right tabular-nums;
}
.customer {
  @apply font-medium;
}
.muted {
  color: var(--ui-text-subtle);
}
.code,
.link {
  @apply inline-flex items-center gap-1 font-semibold hover:underline;
  color: #0065ff;
}
.lost,
.highDiscount {
  @apply font-semibold;
  color: #ff1f1f;
}
.badge {
  @apply inline-flex rounded px-2 py-0.5 text-[10px] font-semibold;
}
.neutral {
  color: #5e6c84;
  background: #f1f2f4;
}
.info {
  color: #0065ff;
  background: #deebff;
}
.success {
  color: #00875a;
  background: #e3fcef;
}
.warning {
  color: #d97008;
  background: #fff3e0;
}
.danger {
  color: #de350b;
  background: #ffebe6;
}
.actions {
  @apply flex items-center justify-center gap-1;
}
.actions button {
  @apply inline-flex items-center gap-1 rounded px-1.5 py-1 text-[10px] transition-colors;
  color: var(--ui-text-subtle);
}
.actions button:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.actions .order {
  color: #0065ff;
}
.actions .remove:hover {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.empty {
  @apply py-16 text-center;
  color: #97a0af;
}
</style>
