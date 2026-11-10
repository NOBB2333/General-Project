<script setup lang="ts">
import type { CustomerMaterialRow } from '@/features/crm/model/customer-materials.types';
import CustomerMaterialTableRow from './CustomerMaterialTableRow.vue';

defineProps<{ rows: readonly CustomerMaterialRow[] }>();
const emit = defineEmits<{
  edit: [row: CustomerMaterialRow];
  remove: [row: CustomerMaterialRow];
}>();
</script>

<template>
  <div :class="$style.metaRow">共 {{ rows.length }} 条记录</div>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>客户名称</th>
          <th>我方物料编码</th>
          <th>我方物料名称</th>
          <th>客户物料编码</th>
          <th>客户物料名称</th>
          <th>备注</th>
          <th>最近维护</th>
          <th :class="$style.actionHeading">操作</th>
        </tr>
      </thead>
      <tbody>
        <CustomerMaterialTableRow
          v-for="row in rows"
          :key="row.id"
          :row="row"
          @edit="emit('edit', $event)"
          @remove="emit('remove', $event)"
        />
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有客户物料对照
    </p>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.metaRow {
  @apply flex min-h-10 items-center border-b px-4 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}

.scroller {
  @apply overflow-auto;
}

.table {
  @apply w-full min-w-[1370px] border-collapse text-left text-[13px];
}

.table th {
  @apply h-12 whitespace-nowrap border-b px-5 text-[11px] font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[72px] border-b px-5;
  color: var(--ui-text);
  border-color: var(--ui-border-subtle);
}

.table tbody tr {
  @apply transition-colors;
}

.table tbody tr:hover {
  background: var(--ui-primary-soft);
}

.table th:nth-child(1) {
  width: 190px;
}

.table th:nth-child(2) {
  width: 160px;
}

.table th:nth-child(3) {
  width: 220px;
}

.table th:nth-child(4) {
  width: 170px;
}

.table th:nth-child(5) {
  width: 190px;
}

.table th:nth-child(6) {
  width: 170px;
}

.table th:nth-child(7) {
  width: 170px;
}

.actionHeading {
  @apply text-center;
  width: 100px;
}

.empty {
  @apply flex min-h-40 items-center justify-center text-[13px];
  color: var(--ui-text-subtle);
}
</style>
