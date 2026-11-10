<script setup lang="ts">
import { Plus, Trash2, X } from 'lucide-vue-next';
import { ref } from 'vue';
import type { ProductSku, SpecDimension } from './product.types';

const props = defineProps<{ dimensions: SpecDimension[]; skus: ProductSku[]; basePrice: number; baseCost: number }>();
const emit = defineEmits<{ update: [dimensions: SpecDimension[], skus: ProductSku[]] }>();
const inputs = ref<Record<string, string>>({});

function combinations(dimensions: SpecDimension[]) {
  const active = dimensions.filter((dimension) => dimension.name.trim() && dimension.values.length);
  if (!active.length) return [];
  return active.reduce<string[]>((result, dimension) => {
    const values = dimension.values.map((value) => `${dimension.name}:${value.value}`);
    return result.length ? result.flatMap((prefix) => values.map((value) => `${prefix} / ${value}`)) : values;
  }, []);
}

function sync(dimensions: SpecDimension[]) {
  const previous = new Map(props.skus.map((sku) => [sku.combination, sku]));
  const skus = combinations(dimensions).map(
    (combination, index): ProductSku =>
      previous.get(combination) ?? {
        id: `sku-${Date.now()}-${index}`,
        combination,
        salePrice: props.basePrice,
        costPrice: props.baseCost,
        barcode: '',
        enabled: true,
      },
  );
  emit('update', dimensions, skus);
}

function addDimension() {
  sync([...props.dimensions, { id: `dim-${Date.now()}`, name: '', values: [] }]);
}

function renameDimension(id: string, name: string) {
  sync(props.dimensions.map((dimension) => (dimension.id === id ? { ...dimension, name } : dimension)));
}

function removeDimension(id: string) {
  sync(props.dimensions.filter((dimension) => dimension.id !== id));
}

function addValue(dimensionId: string) {
  const value = (inputs.value[dimensionId] ?? '').trim();
  const dimension = props.dimensions.find((item) => item.id === dimensionId);
  if (!value || dimension?.values.some((item) => item.value === value)) return;
  inputs.value[dimensionId] = '';
  sync(
    props.dimensions.map((item) =>
      item.id === dimensionId ? { ...item, values: [...item.values, { id: `val-${Date.now()}`, value }] } : item,
    ),
  );
}

function removeValue(dimensionId: string, valueId: string) {
  sync(
    props.dimensions.map((item) =>
      item.id === dimensionId ? { ...item, values: item.values.filter((value) => value.id !== valueId) } : item,
    ),
  );
}

function updateSku(
  id: string,
  key: 'salePrice' | 'costPrice' | 'barcode' | 'enabled',
  value: string | number | boolean,
) {
  emit(
    'update',
    props.dimensions.map((item) => ({ ...item, values: item.values.map((value) => ({ ...value })) })),
    props.skus.map((sku) => (sku.id === id ? { ...sku, [key]: value } : sku)),
  );
}
</script>

<template>
  <div :class="$style.list">
    <div :class="$style.listAlternative">
      <div
        v-for="(dimension, index) in dimensions"
        :key="dimension.id"
        :class="$style.dimensionRow"
      >
        <input
          :value="dimension.name"
          :placeholder="`规格项 ${index + 1}`"
          @input="renameDimension(dimension.id, ($event.target as HTMLInputElement).value)"
          :class="$style.input"
        />
        <div :class="$style.row">
          <span
            v-for="value in dimension.values"
            :key="value.id"
            :class="$style.badge"
            >{{ value.value }}<button @click="removeValue(dimension.id, value.id)"><X :size="11" /></button></span
          ><input
            v-model="inputs[dimension.id]"
            placeholder="输入后回车"
            @keydown.enter.prevent="addValue(dimension.id)"
            @blur="addValue(dimension.id)"
            :class="$style.inputPrimary"
          />
        </div>
        <button
          title="删除规格项"
          @click="removeDimension(dimension.id)"
          :class="$style.deleteButton"
        >
          <Trash2 :size="14" />
        </button>
      </div>
      <button
        @click="addDimension"
        :class="$style.primaryButton"
      >
        <Plus :size="14" />添加规格项
      </button>
    </div>
    <div
      v-if="skus.length"
      :class="$style.card"
    >
      <header :class="$style.header">
        <strong :class="$style.strongLabel">SKU 组合</strong><span :class="$style.badgePurple">{{ skus.length }}</span
        ><small :class="$style.caption">根据规格值自动生成</small>
      </header>
      <div :class="$style.div">
        <table :class="$style.table">
          <thead>
            <tr :class="$style.tableRow">
              <th :class="$style.headerCell">规格组合</th>
              <th :class="$style.headerCellW28Px3Py2">成本价</th>
              <th :class="$style.headerCellW28Px3Py2">销售价</th>
              <th :class="$style.headerCellW40Px3Py2">条码</th>
              <th :class="$style.headerCellCentered">启用</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="sku in skus"
              :key="sku.id"
              :class="$style.skuRow"
            >
              <td :class="$style.tableCell">{{ sku.combination }}</td>
              <td :class="$style.headerCell">
                <input
                  :value="sku.costPrice"
                  type="number"
                  @input="updateSku(sku.id, 'costPrice', Number(($event.target as HTMLInputElement).value))"
                  :class="$style.inputWide"
                />
              </td>
              <td :class="$style.headerCell">
                <input
                  :value="sku.salePrice"
                  type="number"
                  @input="updateSku(sku.id, 'salePrice', Number(($event.target as HTMLInputElement).value))"
                  :class="$style.inputWide"
                />
              </td>
              <td :class="$style.headerCell">
                <input
                  :value="sku.barcode"
                  placeholder="可单独设置"
                  @input="updateSku(sku.id, 'barcode', ($event.target as HTMLInputElement).value)"
                  :class="$style.inputWide"
                />
              </td>
              <td :class="$style.tableCellCentered">
                <input
                  :checked="sku.enabled"
                  type="checkbox"
                  @change="updateSku(sku.id, 'enabled', ($event.target as HTMLInputElement).checked)"
                  :class="$style.checkbox"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.list {
  @apply space-y-4 border-l-2 border-[#6554C0]/20 pl-3;
}
.listAlternative {
  @apply space-y-3;
}
.dimensionRow {
  @apply flex items-start gap-3 rounded-lg border border-[#EBECF0] bg-[#FAFBFC] p-3;
}
.input {
  @apply h-8 w-[100px] shrink-0 rounded border border-[#DFE1E6] bg-white px-2.5 text-[12px] outline-none focus:border-[#4C9AFF];
}
.row {
  @apply flex flex-1 flex-wrap items-center gap-1.5;
}
.badge {
  @apply inline-flex items-center gap-1 rounded bg-app-primary-soft px-2 py-1 text-[11px] text-app-primary;
}
.inputPrimary {
  @apply h-7 w-[110px] rounded border border-dashed border-[#C1C7D0] bg-white px-2 text-[11px] outline-none focus:border-[#4C9AFF];
}
.deleteButton {
  @apply rounded p-1 text-[#6B778C] hover:bg-[#FFEBE6] hover:text-[#DE350B];
}
.primaryButton {
  @apply flex items-center gap-1.5 rounded px-3 py-1.5 text-[12px] text-app-primary hover:bg-app-primary-soft;
}
.card {
  @apply overflow-hidden rounded-lg border border-[#DFE1E6];
}
.header {
  @apply flex items-center border-b border-[#DFE1E6] bg-[#FAFBFC] px-3 py-2.5;
}
.strongLabel {
  @apply text-[12px];
}
.badgePurple {
  @apply ml-2 rounded bg-[#EAE6FF] px-1.5 py-0.5 text-[10px] font-semibold text-[#6554C0];
}
.caption {
  @apply ml-auto text-[10px] text-[#97A0AF];
}
.div {
  @apply max-h-[260px] overflow-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full min-w-[680px] text-[11px];
}
.tableRow {
  @apply border-b border-[#DFE1E6] bg-[#F4F5F7] text-left text-[#5E6C84];
}
.headerCell {
  @apply px-3 py-2;
}
.headerCellW28Px3Py2 {
  @apply w-28 px-3 py-2;
}
.headerCellW40Px3Py2 {
  @apply w-40 px-3 py-2;
}
.headerCellCentered {
  @apply w-16 px-3 py-2 text-center;
}
.skuRow {
  @apply border-b border-[#F4F5F7];
}
.tableCell {
  @apply px-3 py-2 font-medium;
}
.inputWide {
  @apply h-8 w-full rounded border border-[#DFE1E6] px-2;
}
.tableCellCentered {
  @apply px-3 py-2 text-center;
}
.checkbox {
  @apply accent-app-primary;
}
</style>
