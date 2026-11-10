<script setup lang="ts">
import { Eye, Factory, SquarePen, TriangleAlert, Warehouse } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { WarehouseRow } from '../../model/warehouses.types';

defineProps<{ rows: readonly WarehouseRow[] }>();
const emit = defineEmits<{ view: [row: WarehouseRow]; edit: [row: WarehouseRow] }>();

function formatMoney(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}
</script>

<template>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>仓库编号</th>
          <th>仓库名称</th>
          <th>类型</th>
          <th>负责人</th>
          <th>SKU</th>
          <th>库存数量</th>
          <th>库存金额</th>
          <th>采购总额</th>
          <th>预估销售额</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-type="row.type"
        >
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('view', row)"
            >
              {{ row.id }}
            </button>
          </td>
          <td>
            <div :class="$style.identity">
              <span :class="$style.icon">
                <Factory
                  v-if="row.isVirtual"
                  :size="13"
                  :stroke-width="1.8"
                />
                <Warehouse
                  v-else
                  :size="13"
                  :stroke-width="1.8"
                />
              </span>
              <div>
                <strong>{{ row.name }}</strong>
                <span>{{ row.address }}</span>
              </div>
            </div>
          </td>
          <td>
            <span :class="$style.typeBadge">{{ row.type }}</span>
            <span
              v-if="row.isVirtual"
              :class="$style.virtualBadge"
              title="虚拟仓库"
            >
              <Factory
                :size="9"
                :stroke-width="1.8"
              />
            </span>
          </td>
          <td>{{ row.manager }}</td>
          <td :class="$style.number">
            {{ row.totalSKU }}
            <TriangleAlert
              v-if="row.noPriceSKU > 0"
              :class="$style.warning"
              :size="11"
              :stroke-width="1.8"
            />
          </td>
          <td :class="$style.number">{{ row.totalQty }}</td>
          <td :class="$style.number">{{ row.isVirtual ? '—' : formatMoney(row.totalValue) }}</td>
          <td :class="[$style.number, $style.purchase]">{{ row.isVirtual ? '—' : formatMoney(row.totalPurchase) }}</td>
          <td :class="[$style.number, $style.sales]">{{ row.isVirtual ? '—' : formatMoney(row.estimatedSales) }}</td>
          <td>
            <span :class="$style.status">{{ row.status }}</span>
          </td>
          <td>
            <div :class="$style.actions">
              <UiButton
                variant="ghost"
                size="icon-sm"
                :aria-label="`查看${row.name}`"
                title="查看仓库"
                @click="emit('view', row)"
              >
                <Eye
                  :size="13"
                  :stroke-width="1.8"
                />
              </UiButton>
              <UiButton
                variant="ghost"
                size="icon-sm"
                :aria-label="`编辑${row.name}`"
                title="编辑仓库"
                @click="emit('edit', row)"
              >
                <SquarePen
                  :size="13"
                  :stroke-width="1.8"
                />
              </UiButton>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}
.table {
  @apply w-full min-w-[1180px] border-collapse;
}
.table th {
  @apply whitespace-nowrap border-b px-4 py-3 text-left text-[11px] font-semibold;
  color: #5e6c84;
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
}
.table td {
  @apply whitespace-nowrap border-b px-4 py-3 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-surface-hover);
}
.table tr:hover td {
  background: var(--ui-surface-subtle);
}
.table th:nth-child(n + 5):nth-child(-n + 9),
.table td:nth-child(n + 5):nth-child(-n + 9) {
  @apply text-right;
}
.table th:nth-child(3),
.table th:nth-child(10),
.table th:nth-child(11),
.table td:nth-child(3),
.table td:nth-child(10),
.table td:nth-child(11) {
  @apply text-center;
}
.code {
  @apply text-[12px] font-semibold;
  color: var(--app-primary);
}
.identity {
  @apply flex items-center gap-2;
}
.icon {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded text-white;
  background: #00875a;
}
tr[data-type='主仓'] .icon {
  background: #0052cc;
}
tr[data-type='项目仓'] .icon {
  background: #0065ff;
}
tr[data-type='委外仓'] .icon {
  background: #5243aa;
}
.identity strong,
.identity span {
  @apply block;
}
.identity strong {
  @apply text-[13px] font-medium;
  color: var(--ui-text);
}
.identity span {
  @apply mt-0.5 text-[11px];
  color: #97a0af;
}
.typeBadge {
  @apply inline-block rounded px-2 py-0.5 text-[11px] font-medium;
  color: #00875a;
  background: #e3fcef;
}
tr[data-type='主仓'] .typeBadge {
  color: #0052cc;
  background: #deebff;
}
tr[data-type='项目仓'] .typeBadge {
  color: #0065ff;
  background: #e6fcff;
}
tr[data-type='委外仓'] .typeBadge,
.virtualBadge {
  color: #5243aa;
  background: #eae6ff;
}
.virtualBadge {
  @apply ml-1 inline-flex rounded px-1 py-0.5;
}
.number {
  @apply font-semibold;
  color: var(--ui-text) !important;
}
.warning {
  @apply ml-1 inline;
  color: #ff8b00;
}
.purchase {
  color: #0052cc !important;
}
.sales {
  color: #00875a !important;
}
.status {
  @apply inline-block rounded px-2 py-0.5 text-[11px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.actions {
  @apply inline-flex items-center gap-1;
}
</style>
