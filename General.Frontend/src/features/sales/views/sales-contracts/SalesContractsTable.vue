<script setup lang="ts">
import { AlertCircle, ArrowLeftRight, FileText } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiButton } from '@/components/ui';
import type { SalesContractRow } from '@/features/sales/model/sales-contracts.types';

const props = defineProps<{
  rows: SalesContractRow[];
}>();

const emit = defineEmits<{
  detail: [row: SalesContractRow];
  invoice: [row: SalesContractRow];
}>();

const tableScroller = ref<HTMLElement | null>(null);
const amountTotal = computed(() => props.rows.reduce((total, row) => total + row.amountValue, 0));

function progressClass(value: string) {
  const percentage = Number.parseInt(value, 10);
  if (!Number.isFinite(percentage) || percentage <= 0) return 'progress0';
  if (percentage <= 40) return 'progress40';
  if (percentage <= 48) return 'progress48';
  if (percentage <= 50) return 'progress50';
  if (percentage <= 60) return 'progress60';
  if (percentage <= 75) return 'progress75';
  return 'progress100';
}

function statusClass(status: SalesContractRow['status']) {
  if (status === '待审批') return 'statusPending';
  if (status === '已完成') return 'statusDone';
  if (status === '已终止') return 'statusTerminated';
  if (status === '草稿') return 'statusDraft';
  return 'statusActive';
}

function formatAmount(value: number) {
  return `¥${new Intl.NumberFormat('zh-CN').format(value)}`;
}

function toggleHorizontalPosition() {
  const scroller = tableScroller.value;
  if (!scroller) return;
  scroller.scrollTo({ left: scroller.scrollLeft > 0 ? 0 : scroller.scrollWidth, behavior: 'smooth' });
}
</script>

<template>
  <div :class="$style.tableArea">
    <UiButton
      :class="$style.scrollHint"
      variant="secondary"
      size="xs"
      @click="toggleHorizontalPosition"
    >
      <template #leading>
        <ArrowLeftRight
          :size="13"
          :stroke-width="1.8"
        />
      </template>
      左右滑动
    </UiButton>

    <div
      ref="tableScroller"
      :class="$style.tableScroller"
    >
      <table :class="$style.table">
        <colgroup>
          <col :class="$style.codeColumn" />
          <col :class="$style.nameColumn" />
          <col :class="$style.customerColumn" />
          <col :class="$style.sourceColumn" />
          <col :class="$style.amountColumn" />
          <col :class="$style.progressColumn" />
          <col :class="$style.progressColumn" />
          <col :class="$style.progressColumn" />
          <col :class="$style.statusColumn" />
          <col :class="$style.orderColumn" />
          <col :class="$style.trafficColumn" />
          <col :class="$style.dateColumn" />
          <col :class="$style.ownerColumn" />
          <col :class="$style.actionColumn" />
        </colgroup>
        <thead>
          <tr>
            <th>合同编号</th>
            <th>合同名称</th>
            <th>客户</th>
            <th>来源</th>
            <th>合同金额</th>
            <th>开票</th>
            <th>发货</th>
            <th>收款</th>
            <th>状态</th>
            <th>订单状态</th>
            <th>红绿灯</th>
            <th>签订日期</th>
            <th>经办人</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
            :class="row.status === '执行中' && $style.attentionRow"
          >
            <td>
              <button
                type="button"
                :class="$style.codeLink"
                @click="emit('detail', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>
              <strong :class="$style.contractName">{{ row.name }}</strong>
              <span :class="$style.contractSubtitle">{{ row.subtitle }}</span>
            </td>
            <td :class="$style.customerCell">{{ row.customer }}</td>
            <td>
              <span
                v-if="row.source !== '直接下单'"
                :class="$style.sourceTag"
              >
                <FileText
                  :size="11"
                  :stroke-width="1.8"
                />
                {{ row.source }}
              </span>
              <span
                v-else
                :class="$style.directSource"
                >{{ row.source }}</span
              >
            </td>
            <td :class="$style.amountCell">{{ row.amount }}</td>
            <td>
              <span :class="$style.progressCell">
                <span :class="$style.progressTrack">
                  <i :class="[$style.progressFill, $style.invoiceFill, $style[progressClass(row.invoicePct)]]" />
                </span>
                <strong>{{ row.invoicePct }}</strong>
              </span>
            </td>
            <td>
              <span :class="$style.progressCell">
                <span :class="$style.progressTrack">
                  <i :class="[$style.progressFill, $style.shipFill, $style[progressClass(row.shipPct)]]" />
                </span>
                <strong>{{ row.shipPct }}</strong>
              </span>
            </td>
            <td>
              <span :class="$style.progressCell">
                <span :class="$style.progressTrack">
                  <i :class="[$style.progressFill, $style.collectFill, $style[progressClass(row.collectPct)]]" />
                </span>
                <strong>{{ row.collectPct }}</strong>
              </span>
            </td>
            <td>
              <span :class="[$style.statusTag, $style[statusClass(row.status)]]">{{ row.status }}</span>
            </td>
            <td>
              <span
                v-if="row.orderStatus === '未下单'"
                :class="$style.orderWarning"
              >
                <AlertCircle
                  :size="12"
                  :stroke-width="1.8"
                />
                {{ row.orderStatus }}
              </span>
              <span
                v-else
                :class="$style.orderEmpty"
                >{{ row.orderStatus }}</span
              >
            </td>
            <td>
              <span
                :class="[$style.trafficLight, $style[`traffic_${row.traffic}`]]"
                :aria-label="`红绿灯状态：${row.traffic}`"
              />
            </td>
            <td>{{ row.signDate }}</td>
            <td>{{ row.owner }}</td>
            <td>
              <UiButton
                v-if="row.status === '执行中'"
                variant="ghost"
                size="xs"
                @click="emit('invoice', row)"
              >
                申请开票
              </UiButton>
            </td>
          </tr>
        </tbody>
      </table>

      <div
        v-if="!rows.length"
        :class="$style.emptyState"
      >
        当前筛选下没有销售合同
      </div>
    </div>

    <footer :class="$style.footer">
      <span>共 {{ rows.length }} 条 · 金额合计 {{ formatAmount(amountTotal) }}</span>
      <button
        type="button"
        :class="$style.pageButton"
        aria-current="page"
      >
        1
      </button>
    </footer>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableArea {
  @apply relative min-h-0;
}

.scrollHint {
  @apply absolute right-2 top-2 z-20;
  color: var(--app-primary);
  border-color: var(--app-primary-border);
  background: var(--app-primary-soft);
}

.tableScroller {
  @apply min-h-0 overflow-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.table {
  @apply w-full min-w-[1600px] table-fixed border-collapse text-left text-[12px];
}

.codeColumn {
  width: 120px;
}
.nameColumn {
  width: 200px;
}
.customerColumn {
  width: 220px;
}
.sourceColumn {
  width: 120px;
}
.amountColumn {
  width: 110px;
}
.progressColumn {
  width: 100px;
}
.statusColumn {
  width: 90px;
}
.orderColumn {
  width: 95px;
}
.trafficColumn {
  width: 60px;
}
.dateColumn {
  width: 100px;
}
.ownerColumn {
  width: 80px;
}
.actionColumn {
  width: 100px;
}

.table th {
  @apply sticky top-0 z-10 border-b px-3 py-3 text-[11px] font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table th:last-child {
  @apply pr-24;
}

.table td {
  @apply relative border-b px-3 py-3 align-middle;
  height: 68px;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--app-primary-soft);
}

.attentionRow td:first-child::before {
  @apply absolute inset-y-0 left-0 w-[3px];
  content: '';
  background: #ff5630;
}

.codeLink {
  @apply font-semibold transition-colors;
  color: var(--app-primary);
}

.codeLink:hover {
  color: var(--app-primary-hover);
}

.contractName {
  @apply block truncate text-[13px] font-semibold;
  color: var(--ui-text);
}

.contractSubtitle {
  @apply mt-1 block truncate text-[10px];
  color: var(--ui-text-subtle);
}

.customerCell {
  @apply font-medium;
  color: var(--ui-text);
}

.sourceTag,
.directSource {
  @apply inline-flex items-center gap-1 rounded px-2 py-1 text-[11px] font-semibold;
}

.sourceTag {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.directSource {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.amountCell {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.progressCell {
  @apply flex min-w-[88px] items-center gap-2;
}

.progressCell strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-muted);
}

.progressTrack {
  @apply block h-1.5 w-16 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}

.progressFill {
  @apply block h-full rounded-full;
}
.invoiceFill {
  background: var(--app-primary);
}
.shipFill {
  background: var(--ui-warning);
}
.collectFill {
  background: var(--ui-success);
}
.progress0 {
  width: 0;
}
.progress40 {
  width: 40%;
}
.progress48 {
  width: 48%;
}
.progress50 {
  width: 50%;
}
.progress60 {
  width: 60%;
}
.progress75 {
  width: 75%;
}
.progress100 {
  width: 100%;
}

.statusTag {
  @apply inline-flex rounded px-2 py-1 text-[11px] font-semibold;
}
.statusActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.statusPending {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.statusDone {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.statusTerminated {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.statusDraft {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.orderWarning {
  @apply inline-flex items-center gap-1 font-semibold;
  color: #ff5630;
}

.orderEmpty {
  color: var(--ui-text-subtle);
}

.trafficLight {
  @apply mx-auto block h-3 w-3 rounded-full;
}
.traffic_warning {
  background: var(--ui-warning);
}
.traffic_success {
  background: var(--ui-success);
}
.traffic_neutral {
  background: #c1c7d0;
}

.emptyState {
  @apply flex min-h-40 items-center justify-center px-4 text-[13px];
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex min-h-11 items-center gap-4 border-t px-4 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}

.pageButton {
  @apply ml-auto flex h-7 min-w-7 items-center justify-center rounded border px-2 font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
