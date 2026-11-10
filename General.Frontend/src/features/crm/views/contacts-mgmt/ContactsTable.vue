<script setup lang="ts">
import type { ContactRow } from '@/features/crm/model/contacts-mgmt.types';
import ContactTableRow from './ContactTableRow.vue';

defineProps<{ rows: readonly ContactRow[] }>();
</script>

<template>
  <div :class="$style.scroll">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>联系人</th>
          <th>职位 / 部门</th>
          <th>所属公司</th>
          <th>任职状态</th>
          <th>决策权重</th>
          <th>联系方式</th>
          <th>维护人</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <ContactTableRow
          v-for="row in rows"
          :key="row.id"
          :row="row"
        />
        <tr v-if="!rows.length">
          <td
            colspan="8"
            :class="$style.empty"
          >
            当前条件下没有联系人
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroll {
  @apply overflow-x-auto;
}
.table {
  @apply w-full min-w-[1120px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-12 whitespace-nowrap px-4 font-semibold;
  background: var(--ui-surface-hover);
  color: var(--ui-text-subtle);
  border-bottom: 1px solid var(--ui-border);
}
.table th:nth-child(1) {
  width: 180px;
}
.table th:nth-child(2) {
  width: 180px;
}
.table th:nth-child(3) {
  width: 260px;
}
.table th:nth-child(4) {
  width: 110px;
}
.table th:nth-child(5) {
  width: 130px;
}
.table th:nth-child(6) {
  width: 250px;
}
.table th:nth-child(7) {
  width: 120px;
}
.table th:nth-child(8) {
  width: 90px;
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
</style>
