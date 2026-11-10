<script setup lang="ts">
import { Info, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { PaymentStage, PaymentTaskRow } from '../../model/payments.types';

defineProps<{
  rows: readonly PaymentTaskRow[];
  activeStage: string;
  query: string;
  title?: string;
}>();

const emit = defineEmits<{
  'update:activeStage': [value: string];
  'update:query': [value: string];
  register: [row: PaymentTaskRow];
}>();

const stages: readonly { value: '全部' | PaymentStage; label: string }[] = [
  { value: '全部', label: '全部(12)' },
  { value: '待付款', label: '待付款(9)' },
  { value: '审批中', label: '审批中(2)' },
  { value: '已驳回', label: '已驳回(1)' },
  { value: '已完成', label: '已完成' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.tableSection">
    <header :class="$style.toolbar">
      <UiTabs
        :model-value="activeStage"
        @update:model-value="emit('update:activeStage', $event)"
      >
        <UiTabsList
          label="付款任务状态"
          :class="$style.stageTabs"
        >
          <UiTabsTrigger
            v-for="stage in stages"
            :key="stage.value"
            :value="stage.value"
            :class="$style.stageTab"
          >
            {{ stage.label }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="搜索应付单号/供应商/PO..."
          aria-label="搜索付款任务"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
    </header>

    <div :class="$style.guidance">
      <Info
        :size="13"
        :stroke-width="1.8"
      />
      <span v-if="title"
        ><strong>{{ title }}</strong> ·
      </span>
      付款任务是已发起的付款请求。<strong>采购/外协/运输</strong> 由业务负责人显式发起；<strong>报销/工资/税费</strong>
      由上游单据审批通过后自动创建（审批状态为“已审批”）。合同预付场景允许申请金额 &gt; 应付金额。
    </div>

    <div :class="$style.tableScroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>付款任务编号</th>
            <th>来源</th>
            <th>关联单据号</th>
            <th>收款方</th>
            <th>申请付款金额</th>
            <th>应付款金额</th>
            <th>付款审批状态</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <div :class="$style.taskCode">
                <strong>{{ row.code }}</strong>
                <span v-if="row.urgent">加急</span>
              </div>
              <small>{{ row.submittedAt }} · {{ row.requester }}</small>
            </td>
            <td>
              <span :class="[$style.sourceBadge, row.automatic && $style.automaticSource]">{{ row.source }}</span>
            </td>
            <td>
              <strong :class="$style.referenceCode">{{ row.referenceCode }}</strong>
              <small :class="!row.payableAmount ? $style.warningText : undefined">应付: {{ row.payableCode }}</small>
            </td>
            <td>
              <strong>{{ row.payee }}</strong>
            </td>
            <td>
              <strong :class="$style.amountValue">{{ currency(row.requestedAmount) }}</strong>
              <small>期望 {{ row.expectedDate }}</small>
            </td>
            <td>
              <strong v-if="row.payableAmount">{{ currency(row.payableAmount) }}</strong>
              <strong
                v-else
                :class="$style.emptyPayable"
                >—</strong
              >
              <small
                v-if="row.overpaidAmount"
                :class="$style.warningText"
                >超付 {{ currency(row.overpaidAmount) }}</small
              >
              <small
                v-else-if="!row.payableAmount"
                :class="$style.warningText"
                >预付（应付未生成）</small
              >
            </td>
            <td>
              <span :class="[$style.approvalBadge, row.approvalStatus === '部分登记' && $style.partialBadge]">
                {{ row.approvalStatus }}
              </span>
              <small v-if="row.registeredAmount">已登记 {{ currency(row.registeredAmount) }}</small>
            </td>
            <td>
              <UiButton
                variant="primary"
                size="sm"
                :disabled="row.stage === '已完成'"
                @click="emit('register', row)"
                >{{ row.stage === '已完成' ? '已付款' : '登记付款' }}</UiButton
              >
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.emptyCell"
              colspan="8"
            >
              当前状态下没有付款任务
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableSection {
  @apply min-w-0 bg-white;
}

.toolbar {
  @apply flex min-h-10 items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.stageTabs {
  @apply gap-1;
}

.stageTab {
  @apply min-h-[34px] rounded-none border-b-2 border-transparent px-3 text-[11px];
}

.stageTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.searchField {
  @apply flex h-[26px] w-[290px] items-center gap-2 rounded border px-2;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  box-shadow: 0 0 0 1px var(--app-primary-light);
}

.searchField input {
  @apply h-full border-0 p-0 shadow-none;
}

.guidance {
  @apply flex items-center gap-1.5 border-b px-4 text-[10px] leading-5;
  min-height: 33px;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.guidance svg {
  @apply shrink-0;
  color: var(--ui-text-subtle);
}

.guidance strong {
  color: var(--app-primary);
}

.tableScroller {
  @apply overflow-x-auto;
}

.table {
  @apply w-full min-w-[1600px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-8 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[58px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table td:nth-child(1) {
  @apply min-w-[230px];
}

.table td:nth-child(2) {
  @apply min-w-[150px];
}

.table td:nth-child(3),
.table td:nth-child(4) {
  @apply min-w-[190px];
}

.table td:nth-child(5),
.table td:nth-child(6) {
  @apply min-w-[150px];
}

.table strong,
.table small {
  @apply block;
}

.table strong {
  @apply font-medium;
  color: var(--ui-text);
}

.table small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.taskCode {
  @apply flex items-center gap-2;
}

.taskCode strong,
.referenceCode {
  color: var(--app-primary) !important;
}

.taskCode span {
  @apply rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.sourceBadge {
  @apply inline-flex rounded px-2 py-1 font-medium;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.automaticSource {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.amountValue {
  color: var(--ui-danger) !important;
}

.warningText {
  color: var(--ui-warning) !important;
}

.emptyPayable {
  color: var(--ui-text-subtle) !important;
}

.approvalBadge {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.partialBadge {
  color: #008da6;
  background: #e6fcff;
}

.emptyCell {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .stageTab {
    transition: none;
  }
}
</style>
