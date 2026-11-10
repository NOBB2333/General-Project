<script setup lang="ts">
import { ref } from 'vue';
import { UiCheckbox } from '@/components/ui';
import type { TenderRow } from '@/features/crm/model/tenders.types';

defineProps<{ rows: readonly TenderRow[] }>();
const emit = defineEmits<{ select: [row: TenderRow] }>();
const selectedIds = ref<string[]>([]);

function formatWan(value: number) {
  return `¥${(value / 10000).toFixed(2)}万`;
}

function toggleSelection(id: string, checked: boolean | 'indeterminate') {
  selectedIds.value = checked
    ? [...new Set([...selectedIds.value, id])]
    : selectedIds.value.filter((value) => value !== id);
}

function toggleAll(rows: readonly TenderRow[], checked: boolean | 'indeterminate') {
  selectedIds.value = checked ? rows.map((row) => row.id) : [];
}
</script>

<template>
  <div
    :class="$style.scroll"
    data-tender-table
  >
    <table :class="$style.table">
      <thead>
        <tr>
          <th>
            <UiCheckbox
              :model-value="rows.length > 0 && selectedIds.length === rows.length"
              label="选择全部投标项目"
              @update:model-value="toggleAll(rows, $event)"
            />
          </th>
          <th>项目名称 / 编号</th>
          <th>招标单位 / 客户</th>
          <th>负责人</th>
          <th>当前阶段</th>
          <th>投标截止</th>
          <th>开标时间</th>
          <th>保证金</th>
          <th>结果</th>
          <th>风险提醒</th>
          <th>更新时间</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          data-tender-row
          @dblclick="emit('select', row)"
        >
          <td>
            <UiCheckbox
              :model-value="selectedIds.includes(row.id)"
              :label="`选择${row.name}`"
              @update:model-value="toggleSelection(row.id, $event)"
            />
          </td>
          <td>
            <button
              type="button"
              :class="$style.projectName"
              @click="emit('select', row)"
            >
              {{ row.name }}
            </button>
            <span :class="$style.code">{{ row.code }}</span
            ><em :data-priority="row.priority">{{ row.priority }}</em>
          </td>
          <td>
            <strong>{{ row.customer }}</strong
            ><span>{{ row.customerShort }}</span>
          </td>
          <td>
            <span :class="$style.ownerAvatar">{{ row.owner.slice(0, 1) }}</span
            >{{ row.owner }}
          </td>
          <td>
            <span
              :class="$style.stage"
              :data-stage="row.stage"
              >{{ row.stage }}</span
            >
          </td>
          <td>{{ row.deadline }}</td>
          <td>{{ row.openingDate }}</td>
          <td>
            <span
              :class="$style.deposit"
              :data-status="row.depositStatus"
              >{{ row.depositStatus }}</span
            ><small>{{ formatWan(row.depositAmount) }}</small>
          </td>
          <td>
            <span
              :class="$style.result"
              :data-result="row.result"
              >{{ row.result }}</span
            >
          </td>
          <td>
            <span v-if="!row.risks.length">-</span>
            <span
              v-for="risk in row.risks"
              v-else
              :key="risk"
              :class="$style.risk"
              >{{ risk }}</span
            >
          </td>
          <td>{{ row.updatedAt }}</td>
        </tr>
        <tr v-if="!rows.length">
          <td
            colspan="11"
            :class="$style.empty"
          >
            当前筛选下没有投标项目
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroll {
  @apply overflow-x-auto rounded border bg-white;
  border-color: var(--ui-border);
}
.table {
  @apply w-full min-w-[1320px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-10 whitespace-nowrap px-3 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-bottom: 1px solid var(--ui-border);
}
.table td {
  @apply h-[88px] px-3 align-middle;
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: #f7faff;
}
.table th:first-child,
.table td:first-child {
  @apply w-10;
}
.table th:nth-child(2) {
  width: 225px;
}
.table th:nth-child(3) {
  width: 210px;
}
.table th:nth-child(4) {
  width: 92px;
}
.table th:nth-child(5),
.table th:nth-child(6),
.table th:nth-child(7) {
  width: 105px;
}
.table th:nth-child(8) {
  width: 115px;
}
.table th:nth-child(9) {
  width: 90px;
}
.table th:nth-child(10) {
  width: 125px;
}
.table th:nth-child(11) {
  width: 125px;
}
.projectName {
  @apply block max-w-[205px] truncate text-left text-[12px] font-semibold;
  color: var(--ui-text);
}
.projectName:hover {
  color: var(--app-primary);
}
.code {
  @apply mt-1 inline-block;
  color: #97a0af;
}
.table td > strong,
.table td > span:not(.stage):not(.deposit):not(.result):not(.risk):not(.ownerAvatar):not(.code) {
  @apply block;
}
.table td > strong {
  @apply max-w-[190px] truncate font-medium;
  color: var(--ui-text);
}
.table td > small {
  @apply mt-1 block;
}
.table em {
  @apply ml-2 rounded px-1.5 py-0.5 text-[9px] not-italic;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.table em[data-priority='中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.table em[data-priority='低'] {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.ownerAvatar {
  @apply mr-1.5 inline-flex h-5 w-5 items-center justify-center rounded-full text-[9px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.stage,
.deposit,
.result,
.risk {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-medium;
}
.stage {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.stage[data-stage='已开标'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.stage[data-stage='评标中'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.deposit {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.deposit[data-status='已缴纳'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.deposit[data-status='待退还'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.result {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.result[data-result='中标'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.risk {
  @apply mr-1;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
</style>
