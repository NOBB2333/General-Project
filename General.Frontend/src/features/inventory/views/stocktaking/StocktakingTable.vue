<script setup lang="ts">
import { Paperclip } from 'lucide-vue-next';
import { UiCheckbox } from '@/components/ui';
import type { StocktakingRecord } from '../../model/stocktaking.types';

const props = defineProps<{ rows: readonly StocktakingRecord[]; selectedIds: readonly string[] }>();
const emit = defineEmits<{
  view: [row: StocktakingRecord];
  toggle: [id: string, selected: boolean];
  'toggle-all': [selected: boolean];
}>();
function selected(id: string) {
  return props.selectedIds.includes(id);
}
function progressLevel(row: StocktakingRecord) {
  return row.checked === 0 ? 'zero' : row.checked === row.total ? 'complete' : 'partial';
}
function money(value: number) {
  return value === 0 ? '-' : `${value < 0 ? '-' : '+'}¥${Math.abs(value).toLocaleString('zh-CN')}`;
}
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <colgroup>
        <col :class="$style.checkboxColumn" />
        <col :class="$style.codeColumn" />
        <col :class="$style.nameColumn" />
        <col :class="$style.warehouseColumn" />
        <col :class="$style.typeColumn" />
        <col :class="$style.dateColumn" />
        <col :class="$style.dateColumn" />
        <col :class="$style.progressColumn" />
        <col :class="$style.numberColumn" />
        <col :class="$style.amountColumn" />
        <col :class="$style.statusColumn" />
        <col :class="$style.lockColumn" />
        <col :class="$style.operatorColumn" />
      </colgroup>
      <thead>
        <tr>
          <th>
            <UiCheckbox
              :model-value="rows.length > 0 && rows.every((row) => selected(row.id))"
              label="选择全部盘点单"
              @update:model-value="emit('toggle-all', $event === true)"
            />
          </th>
          <th>盘点单号 ↕</th>
          <th>盘点名称</th>
          <th>仓库</th>
          <th>类型</th>
          <th>计划日期 ↓</th>
          <th>实际日期</th>
          <th>进度 ↕</th>
          <th>差异</th>
          <th>盘差 ↕</th>
          <th>状态</th>
          <th>锁账</th>
          <th>操作人</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <UiCheckbox
              :model-value="selected(row.id)"
              :label="`选择 ${row.code}`"
              @update:model-value="emit('toggle', row.id, $event === true)"
            />
          </td>
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.code }}</button
            ><small v-if="row.attachmentCount"><Paperclip :size="10" />{{ row.attachmentCount }}</small>
          </td>
          <td>
            <strong>{{ row.name }}</strong
            ><small>{{ row.description }}</small>
          </td>
          <td>{{ row.warehouse }}</td>
          <td>
            <span
              :class="$style.type"
              :data-type="row.type"
              >{{ row.type }}</span
            >
          </td>
          <td>{{ row.plannedDate }}</td>
          <td>{{ row.actualDate }}</td>
          <td>
            <div :class="$style.progress">
              <i :data-level="progressLevel(row)" /><strong>{{ row.checked }}/{{ row.total }}</strong>
            </div>
          </td>
          <td>
            <strong :class="$style.difference">{{ row.differences || '-' }}</strong>
          </td>
          <td>
            <strong
              :class="$style.amount"
              :data-negative="row.differenceAmount < 0"
              >{{ money(row.differenceAmount) }}</strong
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <span
              :class="$style.account"
              :data-locked="row.accountLocked"
              >{{ row.accountLocked ? '已锁账' : row.lockDeadline === '-' ? '-' : '已解锁' }}</span
            ><small v-if="row.lockDeadline !== '-'">截止 {{ row.lockDeadline }}</small>
          </td>
          <td>{{ row.operator }}</td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的盘点记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1320px] table-fixed border-collapse text-left text-[10px];
}
.checkboxColumn {
  width: 36px;
}
.codeColumn {
  width: 135px;
}
.nameColumn {
  width: 190px;
}
.warehouseColumn {
  width: 115px;
}
.typeColumn {
  width: 70px;
}
.dateColumn {
  width: 94px;
}
.progressColumn {
  width: 118px;
}
.numberColumn {
  width: 58px;
}
.amountColumn {
  width: 74px;
}
.statusColumn {
  width: 78px;
}
.lockColumn {
  width: 138px;
}
.operatorColumn {
  width: 70px;
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-2 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[54px] overflow-hidden whitespace-nowrap border-b px-2;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td:nth-child(3) strong,
.scroller td:nth-child(3) small {
  @apply truncate;
}
.scroller small {
  @apply mt-1 flex items-center gap-1 text-[9px];
  color: var(--ui-text-subtle);
}
.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
  color: var(--app-primary);
}
.type,
.status,
.account {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.type {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.type[data-type='抽盘'] {
  color: #6554c0;
  background: #eae6ff;
}
.type[data-type='清盘'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.progress {
  @apply flex items-center gap-2;
}
.progress i {
  @apply h-1.5 w-14 overflow-hidden rounded bg-[#EBECF0];
}
.progress i::after {
  @apply block h-full rounded content-[''];
  width: 0;
  background: var(--app-primary);
}
.progress i[data-level='partial']::after {
  width: 42%;
}
.progress i[data-level='complete']::after {
  width: 100%;
  background: var(--ui-success);
}
.difference,
.amount[data-negative='true'] {
  color: var(--ui-danger);
}
.status {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.status[data-status='盘点中'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='待审核'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已完成'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.account {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.account[data-locked='true'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
