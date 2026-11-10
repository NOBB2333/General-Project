<script setup lang="ts">
import { Building2 } from 'lucide-vue-next';
import type { SupplierPricebookRow } from '../../model/supplier-pricebook.types';
defineProps<{ rows: readonly SupplierPricebookRow[] }>();
const emit = defineEmits<{ view: [row: SupplierPricebookRow] }>();
</script>
<template>
  <section :class="$style.grid">
    <button
      v-for="row in rows"
      :key="row.id"
      type="button"
      :class="$style.card"
      @click="emit('view', row)"
    >
      <div :class="$style.body">
        <header>
          <strong :class="$style.code">{{ row.id }}</strong
          ><span
            :class="$style.status"
            :data-status="row.status"
            >{{ row.status }}</span
          >
        </header>
        <h2>{{ row.name }}</h2>
        <p :class="$style.supplier">
          <Building2
            :size="13"
            :stroke-width="1.8"
          />{{ row.supplier }}
        </p>
        <div :class="$style.discount">
          <template v-if="row.discounts.length"
            ><span
              v-for="discount in row.discounts"
              :key="discount.label"
              :data-level="discount.label"
              >〰{{ discount.label }} {{ discount.rate }}%</span
            ></template
          ><small v-else>{{ row.pricingMethod }}</small>
        </div>
        <div :class="$style.meta">
          <span>{{ row.itemCount }} 项物料</span><span>{{ row.validFrom }} ~ {{ row.validTo }}</span>
        </div>
      </div>
      <footer>
        <span>创建：{{ row.createdBy }}</span
        ><span>更新：{{ row.updatedAt }}</span>
      </footer>
    </button>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的供应商价格本
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-3 gap-4;
}
.card {
  @apply overflow-hidden rounded-lg border bg-white text-left transition-colors;
  border-color: var(--ui-border);
}
.card:hover {
  border-color: #b3d4ff;
  background: #fafbfc;
}
.body {
  @apply p-4;
}
.body header {
  @apply flex items-center justify-between;
}
.code {
  @apply text-[13px] font-semibold;
  color: #0052cc;
}
.status {
  @apply rounded-full px-2.5 py-1 text-[10px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='草稿'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='已过期'] {
  color: #ff8b00;
  background: #fff7e6;
}
.status[data-status='已废弃'] {
  color: #de350b;
  background: #ffebe6;
}
.body h2 {
  @apply mt-3 text-[14px] font-semibold;
  color: #172b4d;
}
.supplier {
  @apply mt-2 flex items-center gap-1.5 text-[12px];
  color: #6b778c;
}
.discount {
  @apply mt-2 flex min-h-5 items-center gap-1.5;
}
.discount span {
  @apply rounded px-2 py-0.5 text-[10px] font-semibold;
  color: #ff8b00;
  background: #fff7e6;
}
.discount span[data-level='二级'] {
  color: #6554c0;
  background: #eae6ff;
}
.discount small {
  @apply text-[10px];
  color: #97a0af;
}
.meta {
  @apply mt-4 flex items-center justify-between text-[11px];
  color: #6b778c;
}
.card footer {
  @apply flex items-center justify-between border-t px-4 py-3 text-[10px];
  color: #6b778c;
  border-color: var(--ui-border);
  background: #fafbfc;
}
.empty {
  @apply col-span-3 rounded-lg border bg-white py-16 text-center text-[13px];
  color: #97a0af;
  border-color: var(--ui-border);
}
</style>
