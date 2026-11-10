<script setup lang="ts">
import { Eye, Search } from 'lucide-vue-next';
import { UiButton, UiInput } from '@/components/ui';
import type { InvoiceRecord, InvoiceSection, InvoiceTask, UninvoicedItem } from '../../model/invoices.types';

defineProps<{
  section: Exclude<InvoiceSection, 'overview'>;
  rows: readonly InvoiceRecord[];
  uninvoicedRows: readonly UninvoicedItem[];
  tasks: readonly InvoiceTask[];
  query: string;
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  view: [row: InvoiceRecord];
  process: [task: InvoiceTask];
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.panel">
    <header :class="$style.panelHeader">
      <div>
        <h2 v-if="section === 'outbound'">销项发票</h2>
        <h2 v-else-if="section === 'inbound'">进项发票</h2>
        <h2 v-else-if="section === 'uninvoiced'">未开票汇总</h2>
        <h2 v-else>开票任务</h2>
        <p v-if="section === 'outbound'">登记并跟踪已向客户开具的发票</p>
        <p v-else-if="section === 'inbound'">管理供应商发票及税务认证状态</p>
        <p v-else-if="section === 'uninvoiced'">汇总已发生业务但尚未完成开票或收票的单据</p>
        <p v-else>处理已审批通过的销项开票申请</p>
      </div>
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="搜索发票/单位/关联单据..."
          aria-label="搜索发票记录"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
    </header>

    <div :class="$style.tableScroller">
      <table
        v-if="section === 'outbound' || section === 'inbound'"
        :class="$style.table"
      >
        <thead>
          <tr>
            <th>发票号码</th>
            <th>方向</th>
            <th>日期</th>
            <th>对方单位</th>
            <th>类型</th>
            <th>含税金额</th>
            <th>税额</th>
            <th>关联单据</th>
            <th>备注</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <strong :class="$style.invoiceNumber">{{ row.number }}</strong>
            </td>
            <td>
              <span :class="[$style.directionBadge, row.direction === '进项' && $style.directionInbound]">
                {{ row.direction }}
              </span>
            </td>
            <td>{{ row.date }}</td>
            <td>
              <strong :class="$style.partyName">{{ row.party }}</strong>
            </td>
            <td>{{ row.type }}</td>
            <td>
              <strong :class="$style.amount">{{ currency(row.amount) }}</strong>
            </td>
            <td>{{ currency(row.tax) }}</td>
            <td>
              <span :class="$style.referenceCode">{{ row.referenceCode }}</span>
            </td>
            <td>{{ row.remark }}</td>
            <td>
              <UiButton
                variant="ghost"
                size="icon-sm"
                title="查看发票"
                aria-label="查看发票"
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
              colspan="10"
            >
              当前筛选下没有发票记录
            </td>
          </tr>
        </tbody>
      </table>

      <table
        v-else-if="section === 'uninvoiced'"
        :class="[$style.table, $style.compactTable]"
      >
        <thead>
          <tr>
            <th>方向</th>
            <th>关联单据</th>
            <th>往来单位</th>
            <th>待开票金额</th>
            <th>预计日期</th>
            <th>状态</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in uninvoicedRows"
            :key="row.id"
          >
            <td>
              <span :class="[$style.directionBadge, row.direction === '进项' && $style.directionInbound]">
                {{ row.direction }}
              </span>
            </td>
            <td>
              <strong :class="$style.referenceCode">{{ row.referenceCode }}</strong>
            </td>
            <td>{{ row.party }}</td>
            <td>
              <strong :class="$style.amount">{{ currency(row.amount) }}</strong>
            </td>
            <td>{{ row.dueDate }}</td>
            <td>
              <span :class="$style.pendingBadge">{{ row.status }}</span>
            </td>
          </tr>
          <tr v-if="!uninvoicedRows.length">
            <td
              :class="$style.emptyCell"
              colspan="6"
            >
              当前筛选下没有未开票单据
            </td>
          </tr>
        </tbody>
      </table>

      <table
        v-else
        :class="[$style.table, $style.compactTable]"
      >
        <thead>
          <tr>
            <th>申请编号</th>
            <th>申请时间</th>
            <th>客户</th>
            <th>发票类型</th>
            <th>开票金额</th>
            <th>申请人</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="task in tasks"
            :key="task.id"
          >
            <td>
              <strong :class="$style.referenceCode">{{ task.code }}</strong>
            </td>
            <td>{{ task.requestedAt }}</td>
            <td>{{ task.party }}</td>
            <td>{{ task.type }}</td>
            <td>
              <strong :class="$style.amount">{{ currency(task.amount) }}</strong>
            </td>
            <td>{{ task.applicant }}</td>
            <td>
              <UiButton
                :variant="task.status === '待处理' ? 'danger' : 'secondary'"
                size="sm"
                :disabled="task.status === '已开票'"
                @click="emit('process', task)"
              >
                {{ task.status === '待处理' ? '立即开票' : '已开票' }}
              </UiButton>
            </td>
          </tr>
          <tr v-if="!tasks.length">
            <td
              :class="$style.emptyCell"
              colspan="7"
            >
              当前筛选下没有开票任务
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
  @apply mt-5 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.panelHeader {
  @apply flex min-h-[64px] items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.panelHeader h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader p {
  @apply m-0 mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.searchField {
  @apply flex h-8 w-[310px] items-center gap-2 rounded border px-2;
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

.tableScroller {
  @apply overflow-x-auto;
}

.table {
  @apply w-full min-w-[1450px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.compactTable {
  @apply min-w-[980px];
}

.table th {
  @apply h-9 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[52px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.invoiceNumber,
.partyName,
.amount {
  color: var(--ui-text);
}

.invoiceNumber {
  @apply whitespace-nowrap font-semibold;
}

.directionBadge,
.pendingBadge {
  @apply inline-flex w-fit rounded px-2 py-0.5 text-[10px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.directionInbound {
  color: #de350b;
  background: #ffebe6;
}

.pendingBadge {
  color: #974f0c;
  background: #fff0b3;
}

.referenceCode {
  @apply font-semibold;
  color: var(--app-primary);
}

.emptyCell {
  @apply h-28 text-center;
  color: var(--ui-text-subtle);
}
</style>
