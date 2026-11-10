<script setup lang="ts">
import { RefreshCw, UserRoundPlus } from 'lucide-vue-next';
import { UiButton, UiSelect, type UiSelectOption } from '@/components/ui';
import type { ProcessDispatchTableRow } from '../../model/process-dispatch.types';

defineProps<{
  rows: ProcessDispatchTableRow[];
}>();

const emit = defineEmits<{
  select: [row: ProcessDispatchTableRow];
  assign: [row: ProcessDispatchTableRow];
  'update-assignee': [row: ProcessDispatchTableRow, assignee: string];
  outsource: [row: ProcessDispatchTableRow];
}>();

function assigneeOptions(row: ProcessDispatchTableRow): UiSelectOption[] {
  return row.candidateAssignees.map((member) => ({ value: member, label: member }));
}

function statusClass(status: ProcessDispatchTableRow['status']) {
  if (status === '已完工') return 'statusComplete';
  if (status === '进行中') return 'statusActive';
  if (status === '委外中') return 'statusOutsourced';
  return 'statusPending';
}
</script>

<template>
  <section
    :class="$style.frame"
    data-module-dispatch-table
  >
    <table :class="$style.table">
      <thead>
        <tr>
          <th>工单号</th>
          <th>产品</th>
          <th>工序</th>
          <th>工作中心</th>
          <th>数量</th>
          <th>定额工时</th>
          <th>交期</th>
          <th>负责人</th>
          <th>状态</th>
          <th :class="$style.actionColumn">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-table-row-id="row.id"
        >
          <td>
            <button
              type="button"
              :class="$style.codeButton"
              :aria-label="`查看生产工单 ${row.code}`"
              @click="emit('select', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td>{{ row.product }}</td>
          <td>
            <span :class="$style.operationNo">{{ row.operationNo }}</span>
            <strong>{{ row.operationName }}</strong>
            <small
              v-if="row.predecessorPending"
              :class="$style.predecessor"
              >前道未完</small
            >
          </td>
          <td>{{ row.center }}</td>
          <td>{{ row.quantity }}</td>
          <td>{{ row.standardHours }}h</td>
          <td :class="row.dueLabel.includes('逾期') || row.dueLabel === '0天' ? $style.dueCritical : undefined">
            {{ row.dueLabel }}
          </td>
          <td>
            <UiSelect
              v-if="row.candidateAssignees.length"
              :model-value="row.assignee"
              :options="assigneeOptions(row)"
              :label="`选择 ${row.code} 工序${row.operationNo}负责人`"
              placeholder="选择负责人..."
              size="sm"
              :class="$style.assigneeSelect"
              @update:model-value="emit('update-assignee', row, $event)"
            />
            <span v-else>—</span>
          </td>
          <td>
            <span :class="[$style.status, $style[statusClass(row.status)]]">
              {{ row.status }}
            </span>
          </td>
          <td :class="$style.actionCell">
            <UiButton
              v-if="row.action !== '外协'"
              :variant="row.action === '派工' ? 'primary' : 'secondary'"
              size="xs"
              :aria-label="`${row.action} ${row.code} 工序 ${row.operationNo} ${row.operationName}`"
              @click="emit('assign', row)"
            >
              <template #leading>
                <RefreshCw
                  v-if="row.action === '改派'"
                  :size="12"
                  :stroke-width="1.8"
                />
                <UserRoundPlus
                  v-else
                  :size="12"
                  :stroke-width="1.8"
                />
              </template>
              {{ row.action }}
            </UiButton>
            <UiButton
              v-else
              variant="link"
              size="xs"
              :aria-label="`查看 ${row.code} 工序 ${row.operationNo} 委外进度`"
              @click="emit('outsource', row)"
            >
              外协
            </UiButton>
          </td>
        </tr>
      </tbody>
    </table>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.frame {
  @apply min-h-[520px] overflow-auto rounded border bg-white;
  border-color: var(--ui-border);
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.table {
  @apply w-full min-w-[1360px] border-collapse text-left text-[11px];
}

.table thead {
  @apply sticky top-0 z-10 h-10;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.table th,
.table td {
  @apply border-b px-3;
  border-color: var(--ui-border-subtle);
}

.table tbody tr {
  @apply h-[58px] transition-colors;
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.codeButton {
  @apply border-0 p-0 text-[11px] font-semibold;
  color: var(--app-primary);
  background: transparent;
}

.codeButton:hover,
.codeButton:focus-visible {
  text-decoration: underline;
}

.operationNo {
  @apply mr-1 text-[10px];
  color: var(--ui-text-subtle);
}

.table td strong {
  @apply font-semibold;
}

.predecessor {
  @apply ml-1 rounded px-1.5 py-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.dueCritical {
  @apply font-semibold;
  color: var(--ui-danger);
}

.assigneeSelect {
  @apply w-[160px];
}

.status {
  @apply whitespace-nowrap rounded px-2 py-1 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.statusComplete {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.statusActive {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.statusOutsourced {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.statusPending {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.actionColumn,
.actionCell {
  @apply sticky right-0 text-right;
  background: white;
}

.table thead .actionColumn {
  background: var(--ui-surface-subtle);
}

.table tbody tr:hover .actionCell {
  background: var(--ui-surface-hover);
}
</style>
