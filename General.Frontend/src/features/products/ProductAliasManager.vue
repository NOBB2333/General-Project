<script setup lang="ts">
import { Download, Filter, Pencil, Plus, Search, Trash2, Upload } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { toast, UiSelect } from '@/components/ui';
import { productAliases } from './product.data';
import type { Product, ProductAlias } from './product.types';
import ProductAliasImportModal from './ProductAliasImportModal.vue';
import ProductAliasModal from './ProductAliasModal.vue';

const props = defineProps<{ products: Product[] }>();

const rows = ref<ProductAlias[]>(structuredClone(productAliases));
const query = ref('');
const customerFilter = ref('');
const productFilter = ref('');
const selected = ref(new Set<string>());
const formOpen = ref(false);
const editing = ref<ProductAlias | null>(null);
const importOpen = ref(false);

const customers = computed(() => [
  ...new Set([
    ...rows.value.map((row) => row.customer),
    '华能电力科技',
    '中石化仪表部',
    '宝钢设备部',
    '上海宏控自动化',
    '国电南瑞',
    '南方电网设备',
  ]),
]);
const products = computed(() => [...new Set(rows.value.map((row) => row.product))]);
const customerOptions = computed(() => [
  { value: '', label: '全部客户' },
  ...customers.value.map((value) => ({ value, label: value })),
]);
const productOptions = computed(() => [
  { value: '', label: '全部商品' },
  ...products.value.map((value) => ({ value, label: value })),
]);
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter((row) => {
    if (customerFilter.value && row.customer !== customerFilter.value) return false;
    if (productFilter.value && row.product !== productFilter.value) return false;
    return (
      !keyword ||
      [row.customer, row.product, row.model, row.alias, row.aliasModel, row.notes].some((value) =>
        value.toLowerCase().includes(keyword),
      )
    );
  });
});
const allSelected = computed(
  () => filteredRows.value.length > 0 && filteredRows.value.every((row) => selected.value.has(row.id)),
);

function notify(message: string) {
  if (message.includes('已存在')) toast.warning(message);
  else toast.success(message);
}

function openCreate() {
  editing.value = null;
  formOpen.value = true;
}

function openEdit(alias: ProductAlias) {
  editing.value = alias;
  formOpen.value = true;
}

function saveAlias(alias: ProductAlias) {
  if (!editing.value && rows.value.some((row) => row.customer === alias.customer && row.product === alias.product)) {
    notify('该客户与商品的别名已存在');
    return;
  }
  if (editing.value) {
    rows.value = rows.value.map((row) => (row.id === editing.value?.id ? { ...alias, id: row.id } : row));
    notify('别名已更新');
  } else {
    rows.value = [...rows.value, alias];
    notify('别名已添加');
  }
  formOpen.value = false;
  editing.value = null;
}

function toggle(id: string) {
  const next = new Set(selected.value);
  next.has(id) ? next.delete(id) : next.add(id);
  selected.value = next;
}

function toggleAll() {
  selected.value = allSelected.value ? new Set() : new Set(filteredRows.value.map((row) => row.id));
}

function remove(id: string) {
  rows.value = rows.value.filter((row) => row.id !== id);
  const next = new Set(selected.value);
  next.delete(id);
  selected.value = next;
  notify('别名已删除');
}

function removeSelected() {
  const count = selected.value.size;
  rows.value = rows.value.filter((row) => !selected.value.has(row.id));
  selected.value = new Set();
  notify(`已删除 ${count} 条别名`);
}

function exportCsv() {
  const header = '客户名称,商品名称,商品型号,客户商品名称,客户商品型号,备注';
  const content =
    '\uFEFF' +
    [
      header,
      ...filteredRows.value.map((row) =>
        [row.customer, row.product, row.model, row.alias, row.aliasModel, row.notes]
          .map((value) => `"${value.replace(/"/g, '""')}"`)
          .join(','),
      ),
    ].join('\n');
  const url = URL.createObjectURL(new Blob([content], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = `商品别名_导出_${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);
  notify('导出成功');
}

function importAliases(aliases: ProductAlias[]) {
  rows.value = [...rows.value, ...aliases];
  notify(`成功导入 ${aliases.length} 条别名`);
}
</script>

<template>
  <section :class="$style.section">
    <div :class="$style.row">
      <label :class="$style.field"
        ><Search
          :size="14"
          :class="$style.icon" /><input
          v-model="query"
          placeholder="搜索客户名、商品名、别名..."
          :class="$style.searchInput" /></label
      ><Filter
        :size="13"
        :class="$style.icon"
      /><UiSelect
        v-model="customerFilter"
        :options="customerOptions"
        label="筛选客户"
        :class="$style.select"
      /><UiSelect
        v-model="productFilter"
        :options="productOptions"
        label="筛选商品"
        :class="$style.selectH38pxPx25Py7px"
      /><button
        v-if="selected.size"
        @click="removeSelected"
        :class="$style.dangerButton"
      >
        <Trash2
          :size="13"
          :class="$style.iconMr1"
        />删除选中 ({{ selected.size }})
      </button>
      <div :class="$style.rowTrailing">
        <span :class="$style.label">共 {{ filteredRows.length }} 条</span
        ><button
          @click="exportCsv"
          :class="$style.button"
        >
          <Download
            :size="14"
            :class="$style.iconMuted"
          />导出</button
        ><button
          @click="importOpen = true"
          :class="$style.buttonMuted"
        >
          <Upload
            :size="14"
            :class="$style.iconMuted"
          />批量导入</button
        ><button
          @click="openCreate"
          :class="$style.primaryButton"
        >
          <Plus
            :size="14"
            :class="$style.iconMr1"
          />新增别名
        </button>
      </div>
    </div>
    <div :class="$style.card">
      <table :class="$style.table">
        <thead>
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">
              <input
                type="checkbox"
                :checked="allSelected"
                @change="toggleAll"
                :class="$style.checkbox"
              />
            </th>
            <th :class="$style.headerCellCompact">客户名称</th>
            <th :class="$style.headerCellCompact">商品名称</th>
            <th :class="$style.headerCellCompact">商品型号</th>
            <th :class="$style.headerCellCompact">客户商品名称</th>
            <th :class="$style.headerCellCompact">客户商品型号</th>
            <th :class="$style.headerCellCompact">备注</th>
            <th :class="$style.headerCellCentered">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredRows.length === 0">
            <td
              colspan="8"
              :class="$style.tableCell"
            >
              <p :class="$style.description">暂无商品别名数据</p>
              <p :class="$style.descriptionMt1">点击“新增别名”或“批量导入”添加商品别名</p>
            </td>
          </tr>
          <tr
            v-for="item in filteredRows"
            :key="item.id"
            :class="$style.itemRow"
          >
            <td :class="$style.tableCellW40pxPx3Py3">
              <input
                type="checkbox"
                :checked="selected.has(item.id)"
                @change="toggle(item.id)"
                :class="$style.checkbox"
              />
            </td>
            <td :class="$style.tableCellPx4Py3">
              <span :class="$style.badge">{{ item.customerAbbr }}</span
              ><span>{{ item.customer }}</span>
            </td>
            <td :class="$style.tableCellPx4Py3">{{ item.product }}</td>
            <td :class="$style.tableCellPx4Py3Px4Py3">
              <span :class="$style.labelMuted">{{ item.model }}</span>
            </td>
            <td :class="$style.tableCellPx4Py3Px4Py3Px4Py3">{{ item.alias }}</td>
            <td :class="$style.tableCellPx4Py3Px4Py3">
              <span :class="$style.labelPurple">{{ item.aliasModel }}</span>
            </td>
            <td :class="$style.tableCellMuted">{{ item.notes || '-' }}</td>
            <td :class="$style.tableCellPx4Py3Px4Py3">
              <div :class="$style.rowGap1">
                <button
                  title="编辑"
                  @click="openEdit(item)"
                  :class="$style.editButton"
                >
                  <Pencil :size="13" /></button
                ><button
                  title="删除"
                  @click="remove(item.id)"
                  :class="$style.deleteButton"
                >
                  <Trash2 :size="13" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <ProductAliasModal
      :open="formOpen"
      :alias="editing"
      :products="props.products"
      :customers="customers"
      @close="
        formOpen = false;
        editing = null;
      "
      @save="saveAlias"
    />
    <ProductAliasImportModal
      :open="importOpen"
      @close="importOpen = false"
      @import="importAliases"
    />
  </section>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.section {
  @apply mt-4;
}
.row {
  @apply mb-4 flex h-[38px] items-center gap-3;
}
.field {
  @apply flex w-[260px] items-center gap-2 rounded border border-[#DFE1E6] bg-white px-3 py-[7px];
}
.icon {
  @apply text-[#6B778C];
}
.searchInput {
  @apply min-w-0 flex-1 bg-transparent text-[13px] outline-none;
}
.select {
  @apply h-[38px] max-w-[180px] rounded border border-[#DFE1E6] bg-white px-2.5 py-[7px] text-[13px] text-[#42526E];
}
.selectH38pxPx25Py7px {
  @apply h-[38px] max-w-[270px] rounded border border-[#DFE1E6] bg-white px-2.5 py-[7px] text-[13px] text-[#42526E];
}
.dangerButton {
  @apply rounded border border-[#FFBDAD] px-3 py-[7px] text-[13px] text-[#DE350B] hover:bg-[#FFEBE6];
}
.iconMr1 {
  @apply mr-1 inline;
}
.rowTrailing {
  @apply ml-auto flex items-center gap-2;
}
.label {
  @apply mr-2 text-[12px] text-[#6B778C];
}
.button {
  @apply w-[68.3px] rounded border border-[#DFE1E6] px-3 py-[7px] text-[13px] text-[#42526E] hover:bg-[#F4F5F7];
}
.iconMuted {
  @apply mr-1 inline text-[#6B778C];
}
.buttonMuted {
  @apply w-[94.3px] rounded border border-[#DFE1E6] px-3 py-[7px] text-[13px] text-[#42526E] hover:bg-[#F4F5F7];
}
.primaryButton {
  @apply w-[92.3px] rounded bg-app-primary px-3 py-[7px] text-[13px] font-medium text-white hover:bg-app-primary-hover;
}
.card {
  @apply overflow-x-auto rounded-lg border border-[#DFE1E6] bg-white;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full min-w-[1250px] border-collapse;
}
.tableRow {
  @apply h-[39px] border-b border-[#DFE1E6] bg-[#FAFBFC] text-left font-semibold text-[#5E6C84];
}
.headerCell {
  @apply w-[40px] px-3 py-2.5 text-center;
}
.checkbox {
  @apply h-3.5 w-3.5 accent-app-primary;
}
.headerCellCompact {
  @apply px-4 py-2.5 text-[11px];
}
.headerCellCentered {
  @apply w-[80px] px-4 py-2.5 text-center text-[11px];
}
.tableCell {
  @apply py-20 text-center text-[#97A0AF];
}
.description {
  @apply text-[14px] font-medium;
}
.descriptionMt1 {
  @apply mt-1 text-[12px];
}
.itemRow {
  @apply h-[45.5px] border-b border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.tableCellW40pxPx3Py3 {
  @apply w-[40px] px-3 py-3;
}
.tableCellPx4Py3 {
  @apply px-4 py-3 text-[13px];
}
.badge {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full bg-app-primary text-[10px] font-semibold text-white;
}
.tableCellPx4Py3Px4Py3 {
  @apply px-4 py-3;
}
.labelMuted {
  @apply rounded bg-[#F4F5F7] px-2 py-0.5 text-[12px] text-[#42526E];
}
.tableCellPx4Py3Px4Py3Px4Py3 {
  @apply px-4 py-3 text-[13px] font-medium;
}
.labelPurple {
  @apply rounded bg-[#EAE6FF] px-2 py-0.5 text-[12px] text-[#403294];
}
.tableCellMuted {
  @apply px-4 py-3 text-[12px] text-[#6B778C];
}
.rowGap1 {
  @apply flex justify-center gap-1;
}
.editButton {
  @apply rounded p-1.5 text-[#6B778C] hover:bg-app-primary-soft hover:text-app-primary;
}
.deleteButton {
  @apply rounded p-1.5 text-[#6B778C] hover:bg-[#FFEBE6] hover:text-[#DE350B];
}
</style>
