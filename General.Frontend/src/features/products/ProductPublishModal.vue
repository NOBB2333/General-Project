<script setup lang="ts">
import { Info, Network, Package } from 'lucide-vue-next';
import { computed, reactive, ref, watch } from 'vue';
import { UiButton, UiDialog } from '@/components/ui';
import type { Product } from './product.types';

interface PublishConfig {
  price: number | null;
  minOrder: number;
  discount: number;
}
const props = defineProps<{ open: boolean; products: Product[] }>();
const emit = defineEmits<{ close: []; publish: [] }>();
const configs = reactive<Record<string, PublishConfig>>({});
const error = ref('');
const dialogOpen = computed({
  get: () => props.open,
  set: (open: boolean) => {
    if (!open) emit('close');
  },
});

function initialize() {
  Object.keys(configs).forEach((key) => delete configs[key]);
  props.products.forEach((product) => {
    configs[product.id] = { price: null, minOrder: 1, discount: 0 };
  });
  error.value = '';
}

function publish() {
  if (
    props.products.some((product) => {
      const price = configs[product.id]?.price;
      return price === null || price === undefined || price <= 0;
    })
  ) {
    error.value = '请为所有商品设置有效的销售价格';
    return;
  }
  emit('publish');
}

watch(
  () => props.open,
  (open) => {
    if (open) initialize();
  },
);
</script>

<template>
  <UiDialog
    v-model:open="dialogOpen"
    title="发布到共享供应链"
    :description="`将选中的 ${products.length} 个商品发布到系统内 B2B 供应链平台`"
    size="xl"
    :content-class="$style.dialog"
  >
    <template #icon>
      <span :class="$style.label">
        <Network
          :size="20"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <div :class="$style.row">
      <Info
        :size="16"
        :class="$style.icon"
      />
      <div>
        <strong :class="$style.strongLabel">重要提示</strong>
        <p :class="$style.descriptionPrimary">
          发布后，系统内其他租户将能够看到您的商品信息并向您发起采购询价。请合理设置销售价格和起订量。
        </p>
      </div>
    </div>
    <div :class="$style.rowScrollable">
      <table :class="$style.table">
        <thead :class="$style.tableHead">
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">商品信息</th>
            <th :class="$style.headerCell">规格型号</th>
            <th :class="$style.headerCellRightAligned">当前库存</th>
            <th :class="$style.headerCellRightAligned">销售价格 *</th>
            <th :class="$style.headerCellRightAligned">起订量 *</th>
            <th :class="$style.headerCellRightAligned">优惠 (%)</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="product in products"
            :key="product.id"
            :class="$style.productRow"
          >
            <td :class="$style.headerCell">
              <strong :class="$style.strongLabelAlternative">{{ product.name }}</strong
              ><small :class="$style.caption">{{ product.id }}</small>
            </td>
            <td :class="$style.headerCell">
              <span :class="$style.labelAlternative">{{ product.model }}</span
              ><small :class="$style.caption">{{ product.spec }}</small>
            </td>
            <td
              :class="[
                $style.tableCell,
                (product.stock ?? 0) > 10
                  ? $style.stateSuccess
                  : (product.stock ?? 0) > 0
                    ? $style.stateWarning
                    : $style.stateDanger,
              ]"
            >
              {{ product.stock ?? 0 }} {{ product.unit }}
            </td>
            <td :class="$style.headerCell">
              <label :class="$style.field"
                ><span :class="$style.labelCompact">¥</span
                ><input
                  v-model.number="configs[product.id]!.price"
                  min="0"
                  step="0.01"
                  type="number"
                  placeholder="0.00"
                  :class="$style.input"
              /></label>
            </td>
            <td :class="$style.headerCell">
              <label :class="$style.fieldTrailing"
                ><input
                  v-model.number="configs[product.id]!.minOrder"
                  min="1"
                  type="number"
                  :class="$style.inputRightAligned"
                /><span :class="$style.labelCompact">{{ product.unit }}</span></label
              >
            </td>
            <td :class="$style.headerCell">
              <label :class="$style.fieldTrailingMlAutoW20Gap1"
                ><input
                  v-model.number="configs[product.id]!.discount"
                  min="0"
                  max="100"
                  type="number"
                  :class="$style.inputRightAlignedH32pxW16Px2"
                /><span :class="$style.labelCompact">%</span></label
              >
            </td>
          </tr>
        </tbody>
      </table>
      <p
        v-if="error"
        :class="$style.descriptionDanger"
      >
        {{ error }}
      </p>
    </div>
    <template #footer>
      <div :class="$style.footerMeta">
        <Package
          :size="14"
          :class="$style.iconMuted"
        />
        <span :class="$style.labelMuted">共 {{ products.length }} 个商品待发布</span>
      </div>
      <UiButton
        variant="secondary"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="publish"
      >
        <template #leading>
          <Network
            :size="16"
            :stroke-width="1.8"
          />
        </template>
        立即发布到共享供应链
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../styles/index.css";

.dialog {
  width: 900px;
}
.label {
  @apply flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br text-white;
}
.row {
  @apply mx-6 mt-4 flex h-[64px] shrink-0 items-start gap-2 rounded-lg border border-app-primary/20 bg-app-primary-soft px-4 py-3;
}
.icon {
  @apply mt-0.5 shrink-0 text-app-primary;
}
.strongLabel {
  @apply text-[13px] leading-[19.5px] text-app-primary;
}
.descriptionPrimary {
  @apply mt-1 text-[12px] text-app-primary;
}
.rowScrollable {
  @apply min-h-0 flex-1 overflow-y-auto px-6 py-4;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full min-w-[700px] text-[12px];
}
.tableHead {
  @apply sticky top-0 z-10 bg-white;
}
.tableRow {
  @apply border-b-2 border-[#DFE1E6] text-left text-[11px] font-semibold text-[#5E6C84];
}
.headerCell {
  @apply px-3 py-3;
}
.headerCellRightAligned {
  @apply px-3 py-3 text-right;
}
.productRow {
  @apply border-b border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.strongLabelAlternative {
  @apply block text-[13px];
}
.caption {
  @apply mt-0.5 block text-[11px] text-[#6B778C];
}
.labelAlternative {
  @apply block text-[#42526E];
}
.tableCell {
  @apply px-3 py-3 text-right font-medium;
}
.stateSuccess {
  @apply text-[#00875A];
}
.stateWarning {
  @apply text-[#FF8B00];
}
.stateDanger {
  @apply text-[#FF5630];
}
.field {
  @apply relative left-[1.5px] ml-auto flex w-32 items-center gap-1;
}
.labelCompact {
  @apply text-[11px] text-[#6B778C];
}
.input {
  @apply h-[32px] w-28 rounded border border-[#DFE1E6] px-2 py-1.5 text-right text-[13px] outline-none focus:border-app-primary;
}
.fieldTrailing {
  @apply relative right-[1px] ml-auto flex w-24 items-center gap-1;
}
.inputRightAligned {
  @apply h-[32px] w-20 rounded border border-[#DFE1E6] px-2 py-1.5 text-right text-[13px];
}
.fieldTrailingMlAutoW20Gap1 {
  @apply ml-auto flex w-20 items-center gap-1;
}
.inputRightAlignedH32pxW16Px2 {
  @apply h-[32px] w-16 rounded border border-[#DFE1E6] px-2 py-1.5 text-right text-[13px];
}
.descriptionDanger {
  @apply mt-3 rounded border border-[#FFBDAD] bg-[#FFEBE6] px-3 py-2 text-[12px] text-[#DE350B];
}
.footerMeta {
  @apply mr-auto flex items-center;
}
.iconMuted {
  @apply mr-2 text-[#6B778C];
}
.labelMuted {
  @apply text-[12px] text-[#6B778C];
}
</style>
