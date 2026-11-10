<script setup lang="ts">
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  ChevronLeft,
  ChevronRight,
  Image as ImageIcon,
  MoveHorizontal,
  Settings,
  Sparkles,
  X,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { productColumns } from './product.data';
import type { Product } from './product.types';

const props = defineProps<{
  rows: Product[];
  visibleColumns: Set<string>;
  selected: Set<string>;
  sortKey: string;
  sortDirection: 'asc' | 'desc';
}>();

const emit = defineEmits<{
  'toggle-all': [];
  'toggle-row': [id: string];
  sort: [key: string];
  status: [product: Product];
  remove: [product: Product];
  columns: [];
  barcode: [product: Product];
}>();

const allSelected = computed(() => props.rows.length > 0 && props.rows.every((row) => props.selected.has(row.id)));
const showScrollHelp = ref(false);

function money(value: number) {
  return value ? `¥${value.toLocaleString('zh-CN')}` : '-';
}

function columnValue(product: Product, key: string) {
  switch (key) {
    case 'purchasePrice':
      return money(product.purchasePrice);
    case 'recentPurchasePrice':
      return money(product.recentPurchasePrice);
    case 'recentSalePrice':
      return money(product.recentSalePrice);
    case 'presetSalePrice1':
      return money(product.presetSalePrice1);
    case 'taxIncluded':
      return product.taxIncluded ? '是' : '否';
    case 'defaultTaxRate':
      return `${product.defaultTaxRate}%`;
    case 'defaultSupplier':
      return product.defaultSupplier || '-';
    default:
      return String(product[key as keyof Product] || '-');
  }
}

function columnWidthClass(key: string) {
  const classes: Record<string, string> = {
    barcode: 'columnBarcode',
    model: 'columnModel',
    name: 'columnName',
    brand: 'columnBrand',
    unit: 'columnUnit',
    purchasePrice: 'columnPurchasePrice',
    recentPurchasePrice: 'columnRecentPurchasePrice',
    recentSalePrice: 'columnRecentSalePrice',
    presetSalePrice1: 'columnPresetSalePrice',
    purchaseCategory: 'columnPurchaseCategory',
    owner: 'columnOwner',
    createdAt: 'columnCreatedAt',
    status: 'columnStatus',
    taxIncluded: 'columnTaxIncluded',
    defaultTaxRate: 'columnDefaultTaxRate',
    defaultSupplier: 'columnDefaultSupplier',
  };
  return classes[key] ?? 'columnDefault';
}
</script>

<template>
  <div :class="$style.div">
    <button
      aria-label="表格可左右滚动，点击查看操作技巧"
      @click="showScrollHelp = !showScrollHelp"
      :class="$style.viewButton"
    >
      <MoveHorizontal :size="12" />左右滑动
    </button>
    <div
      v-if="showScrollHelp"
      :class="$style.card"
    >
      <button
        @click="showScrollHelp = false"
        :class="$style.button"
      >
        <X :size="12" /></button
      ><strong :class="$style.strongLabel">表格支持横向滚动</strong>
      <p :class="$style.description">按住 Shift + 鼠标滚轮，或在触控板上左右滑动，可查看全部商品字段和操作。</p>
    </div>
    <div :class="$style.divScrollable">
      <table :class="$style.table">
        <thead>
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">
              <button
                title="列设置"
                @click="emit('columns')"
                :class="$style.settingsButton"
              >
                <Settings :size="14" />
              </button>
            </th>
            <th :class="$style.headerCellW44pxPx1Py3">
              <label :class="$style.field">
                <input
                  type="checkbox"
                  :checked="allSelected"
                  @change="emit('toggle-all')"
                  :class="$style.checkbox"
                />
              </label>
            </th>
            <template
              v-for="column in productColumns"
              :key="column.key"
            >
              <th
                v-if="visibleColumns.has(column.key)"
                :class="[
                  $style.headerCellCompact,
                  [
                    $style[columnWidthClass(column.key)],
                    column.align === 'right' ? $style.stateRightAligned : $style.stateDefault,
                  ],
                ]"
              >
                <button
                  v-if="
                    [
                      'barcode',
                      'model',
                      'brand',
                      'purchasePrice',
                      'recentPurchasePrice',
                      'recentSalePrice',
                      'presetSalePrice1',
                      'createdAt',
                      'status',
                    ].includes(column.key)
                  "
                  :class="[$style.buttonAlternative, column.align === 'right' ? $style.stateDefaultAlternative : '']"
                  @click="emit('sort', column.key === 'barcode' ? 'id' : column.key === 'model' ? 'name' : column.key)"
                >
                  {{ column.label }}
                  <ArrowUp
                    v-if="
                      sortKey === (column.key === 'barcode' ? 'id' : column.key === 'model' ? 'name' : column.key) &&
                      sortDirection === 'asc'
                    "
                    :size="12"
                    :class="$style.icon"
                  />
                  <ArrowDown
                    v-else-if="
                      sortKey === (column.key === 'barcode' ? 'id' : column.key === 'model' ? 'name' : column.key)
                    "
                    :size="12"
                    :class="$style.icon"
                  />
                  <ArrowUpDown
                    v-else
                    :size="12"
                    :class="$style.iconMuted"
                  />
                </button>
                <span
                  v-else
                  :class="$style.label"
                  >{{ column.label }}</span
                >
              </th>
            </template>
            <th :class="$style.headerCellCompactMuted">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="product in rows"
            :key="product.id"
            :class="$style.productRow"
          >
            <td :class="$style.tableCell" />
            <td :class="$style.tableCellCentered">
              <input
                type="checkbox"
                :checked="selected.has(product.id)"
                @change="emit('toggle-row', product.id)"
                :class="$style.checkbox"
              />
            </td>
            <template
              v-for="column in productColumns"
              :key="column.key"
            >
              <td
                v-if="visibleColumns.has(column.key)"
                :class="[$style.tableCellPx3, column.align === 'right' ? $style.stateRightAlignedRightAligned : '']"
              >
                <button
                  v-if="column.key === 'barcode'"
                  @click="emit('barcode', product)"
                  :class="$style.primaryButton"
                >
                  {{ product.barcode || '-' }}
                </button>
                <div
                  v-else-if="column.key === 'model'"
                  :class="$style.row"
                >
                  <span
                    v-if="product.mainImage"
                    :class="$style.labelH8W8"
                    ><img
                      :src="product.mainImage"
                      alt=""
                      :class="$style.image"
                  /></span>
                  <span
                    v-else
                    :class="$style.labelCentered"
                    ><ImageIcon :size="14"
                  /></span>
                  <span :class="$style.labelAlternative">
                    <span :class="$style.labelStrong">
                      {{ product.model }}
                      <span
                        v-if="product.mainImage"
                        :class="$style.badge"
                        ><Sparkles :size="9" />商机</span
                      >
                    </span>
                    <span :class="$style.labelCompact">{{ product.spec }}</span>
                  </span>
                </div>
                <div
                  v-else-if="column.key === 'owner'"
                  :class="$style.row"
                >
                  <span :class="$style.badgePrimary">{{ product.owner[0] }}</span>
                  <span>{{ product.owner }}</span>
                </div>
                <span
                  v-else-if="column.key === 'status'"
                  :class="[$style.badgeRound, product.status === '启用' ? $style.stateSuccess : $style.stateMuted]"
                  >{{ product.status }}</span
                >
                <span
                  v-else
                  :class="column.key === 'recentPurchasePrice' ? $style.stateMutedMuted : ''"
                  >{{ columnValue(product, column.key) }}</span
                >
              </td>
            </template>
            <td :class="$style.tableCellCompact">
              <button
                @click="emit('status', product)"
                :class="$style.buttonWarning"
              >
                {{ product.status === '启用' ? '停用' : '启用' }}
              </button>
              <span :class="$style.labelMuted">|</span>
              <button
                @click="emit('remove', product)"
                :class="$style.dangerButton"
              >
                删除
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div :class="$style.rowWithTopDivider">
      <span :class="$style.labelMutedAlternative">共 {{ rows.length }} 条记录</span>
      <div :class="$style.rowGap1">
        <button
          disabled
          :class="$style.iconButton"
        >
          <ChevronLeft :size="14" />
        </button>
        <span
          aria-current="page"
          :class="$style.labelPrimary"
          >1</span
        >
        <button
          disabled
          :class="$style.iconButton"
        >
          <ChevronRight :size="14" />
        </button>
      </div>
    </div>
  </div>
</template>

<style module>
@reference "../../styles/index.css";
.columnBarcode {
  min-width: 140px;
}
.columnModel {
  min-width: 224.5px;
}
.columnName {
  min-width: 160px;
}
.columnBrand {
  min-width: 120px;
}
.columnUnit {
  min-width: 80px;
}
.columnPurchasePrice {
  min-width: 110px;
}
.columnRecentPurchasePrice {
  min-width: 130px;
}
.columnRecentSalePrice {
  min-width: 120px;
}
.columnPresetSalePrice {
  min-width: 130px;
}
.columnPurchaseCategory {
  min-width: 120px;
}
.columnOwner {
  min-width: 100px;
}
.columnCreatedAt {
  min-width: 127px;
}
.columnStatus {
  min-width: 100px;
}
.columnTaxIncluded {
  min-width: 90px;
}
.columnDefaultTaxRate {
  min-width: 90px;
}
.columnDefaultSupplier {
  min-width: 160px;
}
.columnDefault {
  min-width: 100px;
}

/* Semantic template classes */
.div {
  @apply relative border-x border-t border-[#DFE1E6] bg-white;
}
.viewButton {
  @apply absolute right-2 top-2 z-20 flex items-center gap-1 rounded-full border border-[#B3D4FF] bg-app-primary-soft/95 px-2.5 py-1 text-[10.5px] font-medium text-app-primary shadow-sm transition-colors hover:bg-[#D2E5FF] active:scale-95;
}
.card {
  @apply absolute right-2 top-10 z-30 w-[240px] rounded-lg border border-[#DFE1E6] bg-white p-3 text-[11px] text-[#42526E] shadow-xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.button {
  @apply absolute right-2 top-2 text-[#6B778C];
}
.strongLabel {
  @apply block text-[12px] text-[#172B4D];
}
.description {
  @apply mt-1 leading-5;
}
.divScrollable {
  @apply overflow-x-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full min-w-[1600px] border-collapse;
}
.tableRow {
  @apply border-y border-[#DFE1E6] bg-[#F4F5F7];
}
.headerCell {
  @apply w-[40px] min-w-[40px] px-2 py-3;
}
.settingsButton {
  @apply rounded p-1 text-[#6B778C] transition-colors hover:bg-[#EBECF0];
}
.headerCellW44pxPx1Py3 {
  @apply w-[44px] min-w-[44px] px-1 py-3;
}
.field {
  @apply flex cursor-pointer items-center justify-center p-1;
}
.checkbox {
  @apply h-4 w-4 cursor-pointer accent-app-primary;
}
.headerCellCompact {
  @apply px-3 py-3 text-[11px] font-semibold tracking-[.04em] text-[#5E6C84];
}
.stateRightAligned {
  @apply text-right;
}
.stateDefault {
  @apply text-left;
}
.buttonAlternative {
  @apply inline-flex items-center whitespace-nowrap transition-colors hover:text-[#172B4D];
}
.stateDefaultAlternative {
  @apply justify-end;
}
.icon {
  @apply ml-1 text-app-primary;
}
.iconMuted {
  @apply ml-1 text-[#C1C7D0] opacity-0 transition-opacity group-hover/th:opacity-100;
}
.label {
  @apply whitespace-nowrap;
}
.headerCellCompactMuted {
  @apply min-w-[120px] bg-[#F4F5F7] px-3 py-3 text-left text-[11px] font-semibold tracking-[.04em] text-[#5E6C84];
}
.productRow {
  @apply h-[58px] border-b border-[#F4F5F7] transition-colors hover:bg-[#FAFBFC];
}
.tableCell {
  @apply px-2;
}
.tableCellCentered {
  @apply px-1 text-center;
}
.tableCellPx3 {
  @apply px-3 text-[12px] text-[#172B4D];
}
.stateRightAlignedRightAligned {
  @apply text-right tabular-nums;
}
.primaryButton {
  @apply font-medium text-app-primary;
}
.row {
  @apply flex items-center gap-2;
}
.labelH8W8 {
  @apply h-8 w-8 shrink-0 overflow-hidden rounded border border-[#DFE1E6];
}
.image {
  @apply h-full w-full object-cover;
}
.labelCentered {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded border border-[#EBECF0] bg-[#F4F5F7] text-[#C1C7D0];
}
.labelAlternative {
  @apply min-w-0;
}
.labelStrong {
  @apply flex items-center gap-1.5 whitespace-nowrap text-[13px] font-semibold text-[#172B4D];
}
.badge {
  @apply inline-flex items-center gap-0.5 rounded border border-[#FFAB00] bg-[#FFFAE6] px-1 py-0.5 text-[9px] font-medium text-[#FF8B00];
}
.labelCompact {
  @apply block whitespace-nowrap text-[11px] text-[#6B778C];
}
.badgePrimary {
  @apply flex h-6 w-6 items-center justify-center rounded-full bg-app-primary-soft text-[10px] font-semibold text-app-primary;
}
.badgeRound {
  @apply inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium;
}
.stateSuccess {
  @apply bg-[#E3FCEF] text-[#00875A];
}
.stateMuted {
  @apply bg-[#EBECF0] text-[#6B778C];
}
.stateMutedMuted {
  @apply text-[#6B778C];
}
.tableCellCompact {
  @apply px-3 text-[11px] font-medium;
}
.buttonWarning {
  @apply text-[#6B778C] transition-colors hover:text-[#FF8B00];
}
.labelMuted {
  @apply mx-2 text-[#C1C7D0];
}
.dangerButton {
  @apply text-[#6B778C] transition-colors hover:text-[#DE350B];
}
.rowWithTopDivider {
  @apply flex h-[52px] items-center justify-between border-t border-[#DFE1E6] px-4;
}
.labelMutedAlternative {
  @apply text-[12px] text-[#6B778C];
}
.rowGap1 {
  @apply flex items-center gap-1;
}
.iconButton {
  @apply rounded p-1.5 text-[#C1C7D0];
}
.labelPrimary {
  @apply flex h-8 w-8 items-center justify-center rounded bg-app-primary text-[12px] font-medium text-white;
}
</style>
