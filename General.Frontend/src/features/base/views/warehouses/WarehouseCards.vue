<script setup lang="ts">
import {
  Eye,
  Factory,
  MapPin,
  PackageSearch,
  ShoppingCart,
  SquarePen,
  TrendingUp,
  TriangleAlert,
  Warehouse,
} from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { WarehouseRow } from '../../model/warehouses.types';

defineProps<{ rows: readonly WarehouseRow[] }>();
const emit = defineEmits<{ view: [row: WarehouseRow]; edit: [row: WarehouseRow] }>();

function formatWan(value: number) {
  return `¥${(value / 10_000).toFixed(0)}万`;
}
</script>

<template>
  <div :class="$style.grid">
    <article
      v-for="row in rows"
      :key="row.id"
      :class="$style.card"
      :data-type="row.type"
    >
      <header :class="$style.cardHeader">
        <div :class="$style.identity">
          <span :class="$style.warehouseIcon">
            <Factory
              v-if="row.isVirtual"
              :size="20"
              :stroke-width="1.8"
            />
            <Warehouse
              v-else
              :size="20"
              :stroke-width="1.8"
            />
          </span>
          <div :class="$style.heading">
            <div :class="$style.nameLine">
              <button
                type="button"
                :class="$style.nameButton"
                @click="emit('view', row)"
              >
                {{ row.name }}
              </button>
              <span
                v-if="row.isVirtual"
                :class="$style.virtualBadge"
              >
                <PackageSearch
                  :size="9"
                  :stroke-width="1.8"
                />
                虚拟
              </span>
            </div>
            <span :class="$style.typeBadge">{{ row.type }}</span>
          </div>
        </div>
        <div :class="$style.actions">
          <UiButton
            variant="ghost"
            size="icon-sm"
            :aria-label="`查看${row.name}`"
            title="查看仓库"
            @click="emit('view', row)"
          >
            <Eye
              :size="14"
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
              :size="14"
              :stroke-width="1.8"
            />
          </UiButton>
        </div>
      </header>

      <p :class="$style.address">
        <MapPin
          :size="12"
          :stroke-width="1.8"
        />
        {{ row.address }}
      </p>

      <div
        v-if="row.isVirtual"
        :class="$style.virtualMetrics"
      >
        <div>
          <span>在途物料种类</span>
          <strong>{{ row.totalSKU }}</strong>
        </div>
        <div>
          <span>
            <Factory
              :size="10"
              :stroke-width="1.8"
            />
            在途数量
          </span>
          <strong :class="$style.virtualValue">{{ row.totalQty.toLocaleString('en-US') }}</strong>
        </div>
        <div>
          <span>关联供应商</span>
          <strong :class="$style.manager">{{ row.manager }}</strong>
        </div>
      </div>

      <div
        v-else
        :class="$style.metrics"
      >
        <div>
          <span>SKU数量</span>
          <strong>{{ row.totalSKU }}</strong>
        </div>
        <div>
          <span>库存数量</span>
          <strong>{{ row.totalQty }}</strong>
        </div>
        <div>
          <span>库存金额</span>
          <strong>{{ formatWan(row.totalValue) }}</strong>
        </div>
        <div>
          <span>
            <ShoppingCart
              :size="10"
              :stroke-width="1.8"
            />
            采购总额
          </span>
          <strong :class="$style.purchase">{{ formatWan(row.totalPurchase) }}</strong>
        </div>
        <div>
          <span>
            <TrendingUp
              :size="10"
              :stroke-width="1.8"
            />
            预估销售额
          </span>
          <strong :class="$style.sales">{{ formatWan(row.estimatedSales) }}</strong>
        </div>
      </div>

      <p
        v-if="row.noPriceSKU > 0"
        :class="$style.warning"
      >
        <TriangleAlert
          :size="11"
          :stroke-width="1.8"
        />
        {{ row.noPriceSKU }}个SKU无销售价格，对应采购总额 ¥{{ row.noPricePurchaseTotal.toLocaleString('en-US') }}
      </p>
    </article>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.card {
  @apply min-w-0 rounded-lg border bg-white p-5 transition-shadow;
  border-color: var(--ui-border);
}
.card:hover {
  @apply shadow-md;
}
.cardHeader {
  @apply mb-3 flex items-start justify-between;
}
.identity {
  @apply flex min-w-0 items-center gap-3;
}
.warehouseIcon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-lg text-white;
  background: #00875a;
}
.card[data-type='主仓'] .warehouseIcon {
  background: #0052cc;
}
.card[data-type='项目仓'] .warehouseIcon {
  background: #0065ff;
}
.card[data-type='委外仓'] .warehouseIcon {
  background: #5243aa;
}
.heading {
  @apply min-w-0;
}
.nameLine {
  @apply flex min-w-0 items-center gap-1.5;
}
.nameButton {
  @apply truncate text-left text-[14px] font-semibold transition-colors;
  color: var(--ui-text);
}
.nameButton:hover,
.nameButton:focus-visible {
  color: var(--app-primary);
}
.typeBadge {
  @apply mt-1 inline-block rounded px-2.5 py-0.5 text-[11px] font-medium;
  color: #00875a;
  background: #e3fcef;
}
.card[data-type='主仓'] .typeBadge {
  color: #0052cc;
  background: #deebff;
}
.card[data-type='项目仓'] .typeBadge {
  color: #0065ff;
  background: #e6fcff;
}
.card[data-type='委外仓'] .typeBadge {
  color: #5243aa;
  background: #eae6ff;
}
.virtualBadge {
  @apply inline-flex shrink-0 items-center gap-0.5 rounded px-1.5 py-0.5 text-[10px] font-semibold;
  color: #5243aa;
  background: #eae6ff;
}
.actions {
  @apply flex shrink-0 items-center gap-1 opacity-0 transition-opacity;
}
.card:hover .actions,
.card:focus-within .actions {
  @apply opacity-100;
}
.address {
  @apply mb-4 flex items-center gap-1.5 truncate text-[12px];
  color: var(--ui-text-subtle);
}
.metrics,
.virtualMetrics {
  @apply grid gap-3 border-t pt-4;
  border-color: var(--ui-border);
}
.metrics {
  @apply grid-cols-5;
}
.virtualMetrics {
  @apply grid-cols-3;
}
.metrics > div,
.virtualMetrics > div {
  @apply min-w-0;
}
.metrics span,
.virtualMetrics span {
  @apply flex items-center gap-1 whitespace-nowrap text-[11px];
  color: #97a0af;
}
.metrics strong,
.virtualMetrics strong {
  @apply mt-0.5 block truncate text-[18px] font-bold;
  color: var(--ui-text);
}
.purchase {
  color: #0052cc !important;
}
.sales {
  color: #00875a !important;
}
.virtualValue {
  color: #5243aa !important;
}
.manager {
  @apply text-[13px];
  color: var(--ui-text-muted) !important;
}
.warning {
  @apply mt-3 flex items-center gap-1 border-t pt-3 text-[11px];
  color: #ff8b00;
  border-color: var(--ui-surface-hover);
}
@media (max-width: 920px) {
  .grid {
    @apply grid-cols-1;
  }
}
</style>
