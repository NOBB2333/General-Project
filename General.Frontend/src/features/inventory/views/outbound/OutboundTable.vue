<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { OutboundRecord } from '../../model/outbound.types';

defineProps<{ rows: readonly OutboundRecord[] }>();
const emit = defineEmits<{ view: [row: OutboundRecord] }>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>出库单号</th>
          <th>发货单号</th>
          <th>客户</th>
          <th>物料明细</th>
          <th>仓库</th>
          <th>出库日期</th>
          <th>状态</th>
          <th>物流信息</th>
          <th>收入确认</th>
          <th>操作员</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.id }}
            </button>
          </td>
          <td>
            <a
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.shipmentCode }}</a
            >
          </td>
          <td>
            <strong>{{ row.customer }}</strong>
            <small>{{ row.contractCode }}</small>
          </td>
          <td>{{ row.material }}</td>
          <td>{{ row.warehouse }}</td>
          <td>{{ row.date }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <template v-if="row.carrier !== '-'">
              <span>{{ row.carrier }}</span>
              <a
                href="#"
                @click.prevent="emit('view', row)"
                >{{ row.trackingCode }}</a
              >
            </template>
            <span v-else>-</span>
          </td>
          <td>
            <span
              :class="$style.revenue"
              :data-status="row.revenueStatus"
              >{{ row.revenueStatus }}</span
            >
          </td>
          <td>{{ row.operator }}</td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="查看出库详情"
              title="查看出库详情"
              @click="emit('view', row)"
            >
              <Eye
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
      暂无匹配的出库单
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
}

.scroller table {
  @apply w-full min-w-[1420px] border-collapse text-left text-[10px];
}

.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.scroller td {
  @apply h-[55px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.codeButton,
.scroller a {
  color: var(--app-primary);
}

.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
}

.scroller td:nth-child(3) strong,
.scroller td:nth-child(8) span {
  @apply block;
}

.scroller small,
.scroller td:nth-child(8) a {
  @apply mt-1 block text-[9px];
}

.scroller small {
  color: var(--ui-text-subtle);
}

.status,
.revenue {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}

.status[data-status='已出库'],
.revenue[data-status='已确认'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.status[data-status='待出库'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.revenue[data-status='未确认'],
.revenue[data-status='-'] {
  color: var(--ui-text-subtle);
  background: transparent;
}

.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
