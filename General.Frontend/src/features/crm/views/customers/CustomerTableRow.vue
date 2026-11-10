<script setup lang="ts">
import { Eye, MessageSquarePlus, Pencil, Phone, Star, Trash2 } from 'lucide-vue-next';
import { UiTooltip } from '@/components/ui';
import type { CustomerColumn, CustomerRow, CustomerTrafficColor } from '@/features/crm/model/customers.types';

defineProps<{
  row: CustomerRow;
  visibleColumns: ReadonlySet<CustomerColumn['key']>;
}>();

const emit = defineEmits<{
  details: [row: CustomerRow];
  edit: [row: CustomerRow];
  followUp: [row: CustomerRow];
  recycle: [row: CustomerRow];
  traffic: [row: CustomerRow];
}>();

function money(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}

function shortLevel(level: CustomerRow['level']) {
  return level.split('（')[0];
}

function trafficLabel(color: CustomerTrafficColor) {
  return color === 'green' ? '绿灯' : color === 'yellow' ? '黄灯' : '红灯';
}
</script>

<template>
  <tr>
    <td v-if="visibleColumns.has('name')">
      <button
        type="button"
        :class="$style.customerLink"
        @click.stop="emit('details', row)"
      >
        <Star
          v-if="row.followed"
          :class="$style.followed"
          :size="12"
          fill="currentColor"
        />
        {{ row.name }}
      </button>
    </td>
    <td
      v-if="visibleColumns.has('trafficLight')"
      :class="$style.centerCell"
      @click.stop
    >
      <UiTooltip :content="`${trafficLabel(row.trafficColor)}：${row.trafficReason}`">
        <button
          type="button"
          :class="[$style.trafficButton, $style[`traffic_${row.trafficColor}`]]"
          :aria-label="`${row.name}${trafficLabel(row.trafficColor)}，点击切换`"
          @click="emit('traffic', row)"
        />
      </UiTooltip>
    </td>
    <td v-if="visibleColumns.has('type')">
      <span
        :class="[
          $style.typeTag,
          row.type === '终端客户' ? $style.typeEndUser : row.type === '集成商' ? $style.typeIntegrator : $style.typeEpc,
        ]"
        >{{ row.type }}</span
      >
    </td>
    <td v-if="visibleColumns.has('level')">
      <span
        :class="[
          $style.levelTag,
          row.level.startsWith('S') ? $style.levelS : row.level.startsWith('A') ? $style.levelA : $style.levelB,
        ]"
        >{{ shortLevel(row.level) }}</span
      >
    </td>
    <td v-if="visibleColumns.has('owner')">
      <span :class="$style.owner">
        <i>{{ row.owner[0] }}</i>
        {{ row.owner }}
      </span>
    </td>
    <td v-if="visibleColumns.has('contact')">
      <span :class="$style.phone">
        <Phone :size="12" />
        {{ row.phone }}
      </span>
    </td>
    <td
      v-if="visibleColumns.has('creditLimit')"
      :class="$style.numberCell"
    >
      {{ money(row.creditLimit) }}
    </td>
    <td
      v-if="visibleColumns.has('paymentDays')"
      :class="$style.centerCell"
    >
      {{ row.paymentDays }}天
    </td>
    <td
      v-if="visibleColumns.has('totalSales')"
      :class="$style.numberCell"
    >
      <strong>{{ money(row.totalSales) }}</strong>
    </td>
    <td
      v-if="visibleColumns.has('receivable')"
      :class="[$style.numberCell, row.receivable > 200000 && $style.overdue]"
    >
      <strong>{{ row.receivable ? money(row.receivable) : '-' }}</strong>
    </td>
    <td
      :class="$style.actionCell"
      @click.stop
    >
      <button
        type="button"
        :class="$style.followUpButton"
        @click="emit('followUp', row)"
      >
        <MessageSquarePlus :size="13" />
        跟进
        <span v-if="row.followUpCount">{{ row.followUpCount }}</span>
      </button>
      <span :class="$style.rowActions">
        <UiTooltip content="查看客户">
          <button
            type="button"
            aria-label="查看客户"
            @click="emit('details', row)"
          >
            <Eye :size="14" />
          </button>
        </UiTooltip>
        <UiTooltip content="编辑客户">
          <button
            type="button"
            aria-label="编辑客户"
            @click="emit('edit', row)"
          >
            <Pencil :size="14" />
          </button>
        </UiTooltip>
        <UiTooltip content="移入回收站">
          <button
            type="button"
            :class="$style.dangerAction"
            aria-label="移入回收站"
            @click="emit('recycle', row)"
          >
            <Trash2 :size="14" />
          </button>
        </UiTooltip>
      </span>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.centerCell {
  @apply text-center;
}

.numberCell {
  @apply text-right;
}

.customerLink {
  @apply inline-flex items-center gap-1.5 font-semibold;
  color: var(--app-primary);
}

.customerLink:hover,
.customerLink:focus-visible {
  text-decoration: underline;
}

.followed {
  color: #ffab00;
}

.trafficButton {
  @apply mx-auto block h-3 w-3 rounded-full transition-transform active:scale-[.97];
}

.traffic_green {
  background: var(--ui-success);
}

.traffic_yellow {
  background: var(--ui-warning);
}

.traffic_red {
  background: var(--ui-danger);
}

.typeTag,
.levelTag {
  @apply inline-flex rounded px-2 py-1 text-[11px] font-medium;
}

.typeEndUser {
  color: #00875a;
  background: #e3fcef;
}

.typeIntegrator {
  color: #0052cc;
  background: #deebff;
}

.typeEpc {
  color: #ff5630;
  background: #fffae6;
}

.levelS {
  color: #ff5630;
  background: #ffebe6;
}

.levelA {
  color: #ff8b00;
  background: #fff7e6;
}

.levelB {
  color: #ff8b00;
  background: #fffae6;
}

.owner,
.phone {
  @apply inline-flex items-center gap-2;
}

.owner i {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[9px] font-semibold not-italic text-white;
  background: var(--app-primary);
}

.phone svg {
  color: var(--ui-text-subtle);
}

.overdue {
  color: #ff5630;
}

.actionCell {
  @apply text-center;
}

.followUpButton {
  @apply inline-flex items-center gap-1 rounded px-2 py-1 text-[12px] font-medium transition-colors;
  color: var(--app-primary);
}

.followUpButton:hover {
  background: var(--app-primary-soft);
}

.followUpButton span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.rowActions {
  @apply ml-1 inline-flex items-center gap-0.5 opacity-0 transition-opacity;
}

tr:hover .rowActions,
.rowActions:focus-within {
  @apply opacity-100;
}

.rowActions button {
  @apply rounded p-1.5 transition-colors;
  color: var(--ui-text-subtle);
}

.rowActions button:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.rowActions .dangerAction:hover {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
</style>
