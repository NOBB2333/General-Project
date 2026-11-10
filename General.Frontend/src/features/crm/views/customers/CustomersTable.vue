<script setup lang="ts">
import { Columns3 } from 'lucide-vue-next';
import { ref } from 'vue';
import { UiButton, UiCheckbox, UiPopover } from '@/components/ui';
import { customerColumns } from '@/features/crm/model/customers.model';
import type { CustomerColumn, CustomerRow } from '@/features/crm/model/customers.types';
import CustomerTableRow from './CustomerTableRow.vue';

defineProps<{
  rows: CustomerRow[];
  visibleColumns: ReadonlySet<CustomerColumn['key']>;
}>();

const emit = defineEmits<{
  details: [row: CustomerRow];
  edit: [row: CustomerRow];
  followUp: [row: CustomerRow];
  recycle: [row: CustomerRow];
  traffic: [row: CustomerRow];
  'toggle-column': [key: CustomerColumn['key']];
}>();

const columnPopoverOpen = ref(false);
</script>

<template>
  <div :class="$style.metaRow">
    <span>共 {{ rows.length }} 条记录</span>
    <UiPopover
      v-model:open="columnPopoverOpen"
      size="sm"
      align="end"
    >
      <template #trigger>
        <UiButton
          size="xs"
          aria-label="设置客户表格显示列"
        >
          <template #leading><Columns3 :size="13" /></template>
          列设置
          <span :class="$style.columnCount">{{ visibleColumns.size }}</span>
        </UiButton>
      </template>
      <div :class="$style.columnPanel">
        <header>
          <strong>显示列</strong>
          <span>{{ visibleColumns.size }}/{{ customerColumns.length }}</span>
        </header>
        <label
          v-for="column in customerColumns"
          :key="column.key"
          :class="$style.columnOption"
        >
          <UiCheckbox
            :model-value="visibleColumns.has(column.key)"
            :disabled="'required' in column && column.required"
            :label="`显示${column.label}列`"
            @update:model-value="emit('toggle-column', column.key)"
          />
          <span>{{ column.label }}</span>
        </label>
      </div>
    </UiPopover>
  </div>

  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th v-if="visibleColumns.has('name')">客户名称</th>
          <th
            v-if="visibleColumns.has('trafficLight')"
            :class="$style.centerCell"
          >
            红绿灯
          </th>
          <th v-if="visibleColumns.has('type')">类型</th>
          <th v-if="visibleColumns.has('level')">级别</th>
          <th v-if="visibleColumns.has('owner')">负责人</th>
          <th v-if="visibleColumns.has('contact')">联系方式</th>
          <th
            v-if="visibleColumns.has('creditLimit')"
            :class="$style.numberCell"
          >
            信用额度
          </th>
          <th
            v-if="visibleColumns.has('paymentDays')"
            :class="$style.centerCell"
          >
            账期
          </th>
          <th
            v-if="visibleColumns.has('totalSales')"
            :class="$style.numberCell"
          >
            累计销售
          </th>
          <th
            v-if="visibleColumns.has('receivable')"
            :class="$style.numberCell"
          >
            应收余额
          </th>
          <th :class="$style.centerCell">操作</th>
        </tr>
      </thead>
      <tbody>
        <CustomerTableRow
          v-for="row in rows"
          :key="row.id"
          :row="row"
          :visible-columns="visibleColumns"
          @details="emit('details', $event)"
          @edit="emit('edit', $event)"
          @follow-up="emit('followUp', $event)"
          @recycle="emit('recycle', $event)"
          @traffic="emit('traffic', $event)"
        />
      </tbody>
    </table>

    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有客户，请调整筛选条件
    </div>
  </div>

  <footer :class="$style.footer">
    <span>共 {{ rows.length }} 条记录</span>
    <span>1 / 1</span>
  </footer>
</template>

<style module>
@reference "../../../../styles/index.css";

.metaRow {
  @apply flex min-h-11 items-center justify-between border-b px-4 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}

.columnCount {
  @apply rounded-full px-1.5 py-0.5 text-[9px] font-semibold text-white;
  background: var(--app-primary);
}

.columnPanel {
  @apply py-1;
}

.columnPanel header {
  @apply flex items-center justify-between border-b px-4 py-3 text-[12px];
  border-color: var(--ui-border-subtle);
}

.columnPanel header span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.columnOption {
  @apply flex cursor-pointer items-center gap-3 px-4 py-2 text-[12px] transition-colors;
}

.columnOption:hover {
  background: var(--ui-surface-hover);
}

.scroller {
  @apply min-h-0 overflow-auto;
}

.table {
  @apply w-full min-w-[1430px] border-collapse text-left text-[13px];
}

.table th {
  @apply h-12 whitespace-nowrap border-b px-5 text-[11px] font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[59px] whitespace-nowrap border-b px-5;
  color: var(--ui-text);
  border-color: var(--ui-border-subtle);
}

.table tbody tr {
  @apply cursor-pointer transition-colors;
}

.table tbody tr:hover {
  background: var(--ui-primary-soft);
}

.centerCell {
  @apply text-center;
}

.numberCell {
  @apply text-right;
}

.empty {
  @apply flex min-h-40 items-center justify-center text-[13px];
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex min-h-12 items-center justify-between border-t px-5 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
</style>
