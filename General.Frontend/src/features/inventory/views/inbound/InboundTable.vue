<script setup lang="ts">
import { Paperclip } from 'lucide-vue-next';
import type { InboundRecord } from '../../model/inbound.types';

defineProps<{ rows: readonly InboundRecord[] }>();
const emit = defineEmits<{ view: [row: InboundRecord] }>();
const money = new Intl.NumberFormat('zh-CN', {
  style: 'currency',
  currency: 'CNY',
  maximumFractionDigits: 0,
});
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>入库单号</th>
          <th>入库来源</th>
          <th>到货登记单号</th>
          <th>供应商</th>
          <th>类型</th>
          <th>状态</th>
          <th>入库日期</th>
          <th>仓库</th>
          <th>数量</th>
          <th>金额</th>
          <th>经办人</th>
          <th>备注</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          tabindex="0"
          @click="emit('view', row)"
          @keydown.enter="emit('view', row)"
        >
          <td>
            <button
              :class="$style.codeButton"
              @click.stop="emit('view', row)"
            >
              {{ row.id }}
            </button>
            <span
              v-if="row.attachmentCount"
              :class="$style.attachment"
            >
              <Paperclip
                :size="11"
                :stroke-width="1.8"
              />
              {{ row.attachmentCount }}
            </span>
            <small v-if="row.batchCode !== '-'">{{ row.batchCode }}</small>
          </td>
          <td>
            <span
              v-if="row.source !== '-'"
              :class="$style.source"
              :data-source="row.source"
              >{{ row.source }}</span
            >
            <span v-else>-</span>
          </td>
          <td>
            <a
              v-if="row.arrivalCode !== '-'"
              href="#"
              @click.prevent.stop="emit('view', row)"
              >{{ row.arrivalCode }}</a
            >
            <span v-else>-</span>
          </td>
          <td>{{ row.supplier }}</td>
          <td>
            <span
              :class="$style.type"
              :data-type="row.type"
              >{{ row.type }}</span
            >
            <span
              v-if="row.zeroPurchase"
              :class="$style.zeroPurchase"
              >零星采购</span
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.date }}</td>
          <td>{{ row.warehouse }}</td>
          <td>
            <strong>{{ row.quantity }}</strong>
          </td>
          <td>
            <strong>{{ money.format(row.amount) }}</strong>
          </td>
          <td>{{ row.handler }}</td>
          <td :title="row.remark">{{ row.remark }}</td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的入库单
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
}

.scroller table {
  @apply w-full min-w-[1510px] border-collapse text-left text-[10px];
}

.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.scroller td {
  @apply h-[50px] max-w-[180px] cursor-pointer truncate whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.scroller tbody tr {
  @apply outline-none transition-colors;
}

.scroller tbody tr:hover,
.scroller tbody tr:focus-visible {
  background: var(--ui-surface-hover);
}

.scroller td:first-child {
  @apply min-w-[150px];
}

.scroller a,
.codeButton {
  color: var(--app-primary);
}

.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
}

.attachment {
  @apply ml-1 inline-flex items-center text-[9px];
  color: var(--ui-text-subtle);
}

.scroller small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}

.source,
.type,
.status,
.zeroPurchase {
  @apply mr-1 rounded px-2 py-1 text-[9px] font-semibold;
}

.source {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.source[data-source='免检入库'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.source[data-source='让步入库'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.type {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.type[data-type='采购入库'],
.zeroPurchase {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.type[data-type='退货入库'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.type[data-type='其他入库'] {
  color: #6554c0;
  background: #eae6ff;
}

.status[data-status='待审批'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.status[data-status='已入库'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.status[data-status='草稿'] {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.status[data-status='已驳回'],
.status[data-status='已作废'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
