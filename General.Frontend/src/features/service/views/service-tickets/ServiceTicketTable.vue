<script setup lang="ts">
import { ArrowLeftRight, ScanLine } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiCheckbox } from '@/components/ui';
import type { ServiceTicketRow } from '../../model/service-tickets.types';

type CheckedState = boolean | 'indeterminate';

const props = defineProps<{
  rows: readonly ServiceTicketRow[];
  selectedIds: ReadonlySet<string>;
}>();

const emit = defineEmits<{
  toggle: [id: string, checked: boolean];
  toggleAll: [checked: boolean];
  open: [row: ServiceTicketRow];
}>();

const allRowsSelected = computed(
  () => props.rows.length > 0 && props.rows.every((row) => props.selectedIds.has(row.id)),
);

function booleanChecked(value: CheckedState) {
  return value === true;
}

function typeTone(type: string) {
  if (type === '售后报修') return 'typeRepair';
  if (type === '远程支持') return 'typeRemote';
  if (type === '巡检保养') return 'typeInspect';
  return 'typeDefault';
}

function urgencyTone(urgency: ServiceTicketRow['urgency']) {
  if (urgency === '紧急') return 'urgencyCritical';
  if (urgency === '高') return 'urgencyHigh';
  if (urgency === '中') return 'urgencyMedium';
  return 'urgencyLow';
}

function statusTone(status: ServiceTicketRow['status']) {
  if (status === '处理中') return 'statusProcessing';
  if (status === '待客户确认') return 'statusConfirm';
  if (status === '已完成') return 'statusDone';
  return 'statusWaiting';
}
</script>

<template>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <colgroup>
        <col :class="$style.selectColumn" />
        <col :class="$style.codeColumn" />
        <col :class="$style.titleColumn" />
        <col :class="$style.typeColumn" />
        <col :class="$style.customerColumn" />
        <col :class="$style.productColumn" />
        <col :class="$style.warrantyColumn" />
        <col :class="$style.urgencyColumn" />
        <col :class="$style.statusColumn" />
        <col :class="$style.ownerColumn" />
        <col :class="$style.timeColumn" />
        <col :class="$style.responseColumn" />
        <col :class="$style.actionColumn" />
      </colgroup>
      <thead>
        <tr>
          <th :class="$style.selectCell">
            <UiCheckbox
              :model-value="allRowsSelected"
              label="选择当前结果全部工单"
              @update:model-value="emit('toggleAll', booleanChecked($event))"
            />
          </th>
          <th>工单编号</th>
          <th>工单标题</th>
          <th>类型</th>
          <th>客户</th>
          <th>产品 / SN</th>
          <th>质保</th>
          <th>紧急</th>
          <th>状态</th>
          <th>负责人</th>
          <th>创建时间</th>
          <th>响应</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :class="selectedIds.has(row.id) && $style.selectedRow"
        >
          <td :class="$style.selectCell">
            <UiCheckbox
              :model-value="selectedIds.has(row.id)"
              :label="`选择工单 ${row.code}`"
              @update:model-value="emit('toggle', row.id, booleanChecked($event))"
            />
          </td>
          <td>
            <button
              type="button"
              :class="$style.codeLink"
              @click="emit('open', row)"
            >
              <ScanLine
                v-if="row.aiRisk"
                :size="13"
                :stroke-width="1.8"
              />
              {{ row.code }}
            </button>
          </td>
          <td>
            <div :class="$style.titleCell">
              <span
                v-if="row.aiRisk"
                :class="$style.aiRisk"
                >⚡ AI高风险</span
              >
              <strong>{{ row.title }}</strong>
            </div>
          </td>
          <td>
            <span :class="[$style.typeTag, $style[typeTone(row.type)]]">{{ row.type }}</span>
          </td>
          <td>
            <div :class="$style.stackedCell">
              <strong>{{ row.customer }}</strong>
              <small>⌕ {{ row.contact }}</small>
            </div>
          </td>
          <td>
            <div :class="$style.stackedCell">
              <strong>{{ row.product }}</strong>
              <small :class="$style.serial">{{ row.serialNumber }}</small>
            </div>
          </td>
          <td>
            <span :class="$style.warrantyTag">{{ row.warranty }}</span>
          </td>
          <td>
            <span :class="[$style.urgencyTag, $style[urgencyTone(row.urgency)]]">{{ row.urgency }}</span>
          </td>
          <td>
            <div :class="$style.statusCell">
              <span :class="[$style.statusTag, $style[statusTone(row.status)]]">{{ row.status }}</span>
              <span
                v-if="row.overdue"
                :class="$style.overdueTag"
                >超时</span
              >
            </div>
          </td>
          <td>{{ row.owner }}</td>
          <td :class="$style.mutedCell">{{ row.createdAt }}</td>
          <td :class="$style.mutedCell">{{ row.response }}</td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              :aria-label="`查看 ${row.code}`"
              title="查看工单"
              @click="emit('open', row)"
            >
              <ArrowLeftRight
                :size="14"
                :stroke-width="1.8"
              />
            </UiButton>
          </td>
        </tr>
      </tbody>
    </table>

    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有服务工单
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply mt-3 min-w-0 overflow-x-auto border;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.table {
  @apply w-full table-fixed border-collapse text-left text-[11px];
  min-width: 1390px;
  color: var(--ui-text);
}

.table thead {
  background: #f4f5f7;
}

.table th {
  @apply px-2.5 font-semibold;
  color: var(--ui-text-muted);
  height: 43px;
}

.table td {
  @apply border-t px-2.5 py-2.5 align-middle;
  height: 56px;
  border-color: var(--ui-border-subtle);
}

.table tbody tr {
  @apply transition-colors;
  background: white;
}

.table tbody tr:hover,
.selectedRow {
  background: #f7faff;
}

.selectColumn {
  width: 42px;
}

.codeColumn {
  width: 150px;
}

.titleColumn {
  width: 275px;
}

.typeColumn {
  width: 98px;
}

.customerColumn {
  width: 155px;
}

.productColumn {
  width: 220px;
}

.warrantyColumn {
  width: 84px;
}

.urgencyColumn {
  width: 70px;
}

.statusColumn {
  width: 125px;
}

.ownerColumn {
  width: 78px;
}

.timeColumn {
  width: 112px;
}

.responseColumn,
.actionColumn {
  width: 62px;
}

.selectCell {
  @apply pl-4 pr-1;
}

.codeLink {
  @apply inline-flex items-center gap-1 whitespace-nowrap font-semibold outline-none;
  color: var(--app-primary);
}

.codeLink:hover {
  text-decoration: underline;
}

.codeLink:focus-visible {
  border-radius: var(--app-compact-control-radius);
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.titleCell {
  @apply flex min-w-0 items-center gap-1.5;
}

.titleCell strong {
  @apply min-w-0 truncate font-semibold;
}

.aiRisk,
.typeTag,
.warrantyTag,
.urgencyTag,
.statusTag,
.overdueTag {
  @apply inline-flex shrink-0 items-center whitespace-nowrap rounded px-1.5 py-1 text-[9px] font-semibold leading-none;
}

.aiRisk {
  color: #6554c0;
  background: #eae6ff;
}

.typeRepair {
  color: #de350b;
  background: #ffebe6;
}

.typeRemote {
  color: #00a3bf;
  background: #e6fcff;
}

.typeInspect {
  color: #6554c0;
  background: #eae6ff;
}

.typeDefault {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.stackedCell {
  @apply min-w-0;
}

.stackedCell strong,
.stackedCell small {
  @apply block truncate;
}

.stackedCell strong {
  @apply font-medium;
}

.stackedCell small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.serial {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.warrantyTag {
  color: #00875a;
  background: #e3fcef;
}

.urgencyCritical {
  color: #de350b;
  background: #ffebe6;
}

.urgencyHigh {
  color: #ff5630;
  background: #ffebe6;
}

.urgencyMedium {
  color: #ff8b00;
  background: #fff0b3;
}

.urgencyLow {
  color: var(--ui-text-subtle);
  background: #ebecf0;
}

.statusCell {
  @apply flex items-center gap-1;
}

.statusWaiting {
  color: #0052cc;
  background: #deebff;
}

.statusProcessing {
  color: #6554c0;
  background: #eae6ff;
}

.statusConfirm {
  color: #ff8b00;
  background: #fff0b3;
}

.statusDone {
  color: #00875a;
  background: #e3fcef;
}

.overdueTag {
  color: #de350b;
  background: #ffebe6;
}

.mutedCell {
  color: var(--ui-text-subtle);
}

.empty {
  @apply flex h-44 items-center justify-center text-[12px];
  color: var(--ui-text-subtle);
  background: white;
}

@media (prefers-reduced-motion: reduce) {
  .table tbody tr {
    transition: none;
  }
}
</style>
