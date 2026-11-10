<script setup lang="ts">
import { Package, RotateCcw, Trash2 } from 'lucide-vue-next';
import { computed, ref, watch } from 'vue';
import { UiAlertDialog, UiButton, UiDialog } from '@/components/ui';
import type { DeletedProduct } from './product.types';

const props = defineProps<{ open: boolean; products: DeletedProduct[] }>();
const emit = defineEmits<{ close: []; restore: [id: string]; permanentDelete: [id: string]; clear: [] }>();

type Confirmation = { kind: 'delete'; product: DeletedProduct } | { kind: 'clear' };
const confirmation = ref<Confirmation | null>(null);
const dialogOpen = computed({
  get: () => props.open,
  set: (open: boolean) => {
    if (!open) emit('close');
  },
});
const confirmationOpen = computed({
  get: () => confirmation.value !== null,
  set: (open: boolean) => {
    if (!open) confirmation.value = null;
  },
});
const confirmationTitle = computed(() => (confirmation.value?.kind === 'clear' ? '清空回收站' : '永久删除'));
const confirmationDescription = computed(() =>
  confirmation.value?.kind === 'clear'
    ? `确定要永久删除回收站中的所有 ${props.products.length} 个商品吗？此操作不可撤销。`
    : `确定要永久删除商品「${confirmation.value?.product.name}」吗？此操作不可撤销。`,
);
const confirmationLabel = computed(() => (confirmation.value?.kind === 'clear' ? '确认清空' : '永久删除'));

function confirm() {
  if (!confirmation.value) return;
  if (confirmation.value.kind === 'clear') emit('clear');
  else emit('permanentDelete', confirmation.value.product.id);
  confirmation.value = null;
}

watch(
  () => props.open,
  (open) => {
    if (!open) confirmation.value = null;
  },
);
</script>

<template>
  <UiDialog
    v-model:open="dialogOpen"
    title="回收站"
    description="已删除的商品可在此恢复或永久删除"
    size="xl"
    :content-class="$style.dialog"
  >
    <template #icon>
      <span :class="$style.label">
        <Trash2
          :size="18"
          :stroke-width="1.8"
          :class="$style.icon"
        />
      </span>
    </template>
    <template #actions>
      <UiButton
        v-if="products.length"
        variant="danger"
        size="sm"
        @click="confirmation = { kind: 'clear' }"
      >
        <template #icon>
          <Trash2
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        清空回收站
      </UiButton>
    </template>

    <div :class="$style.row">
      <div
        v-if="products.length === 0"
        :class="$style.rowCentered"
      >
        <span :class="$style.labelRound"
          ><Trash2
            :size="28"
            :class="$style.iconMuted" /></span
        ><strong :class="$style.strongLabel">回收站为空</strong
        ><span :class="$style.labelMuted">删除的商品将显示在这里</span>
      </div>
      <table
        v-else
        :class="$style.table"
      >
        <thead>
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">商品名称</th>
            <th :class="$style.headerCell">型号</th>
            <th :class="$style.headerCell">分类</th>
            <th :class="$style.headerCell">品牌</th>
            <th :class="$style.headerCell">删除时间</th>
            <th :class="$style.headerCellCentered">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="product in products"
            :key="product.id"
            :class="$style.productRow"
          >
            <td :class="$style.tableCell">
              <div :class="$style.rowGap2">
                <span
                  v-if="product.mainImage"
                  :class="$style.labelH7W7"
                  ><img
                    :src="product.mainImage"
                    alt=""
                    :class="$style.image" /></span
                ><span
                  v-else
                  :class="$style.labelCentered"
                  ><Package
                    :size="12"
                    :class="$style.iconMuted" /></span
                ><strong :class="$style.strongLabelAlternative">{{ product.name }}</strong>
              </div>
            </td>
            <td :class="$style.tableCellMuted">{{ product.model }}</td>
            <td :class="$style.tableCellMuted">{{ product.category }}</td>
            <td :class="$style.tableCellMuted">{{ product.brand }}</td>
            <td :class="$style.tableCellMuted">{{ product.deletedAt }}</td>
            <td :class="$style.tableCell">
              <div :class="$style.rowGap2Gap2">
                <button
                  @click="emit('restore', product.id)"
                  :class="$style.primaryButton"
                >
                  <RotateCcw :size="12" />恢复</button
                ><button
                  @click="confirmation = { kind: 'delete', product }"
                  :class="$style.dangerButtonDanger"
                >
                  <Trash2 :size="12" />永久删除
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <template #footer>
      <span :class="$style.footerCount">共 {{ products.length }} 个已删除商品</span>
      <UiButton
        variant="secondary"
        @click="dialogOpen = false"
      >
        关闭
      </UiButton>
    </template>
  </UiDialog>

  <UiAlertDialog
    v-model:open="confirmationOpen"
    :title="confirmationTitle"
    :description="confirmationDescription"
    :confirm-label="confirmationLabel"
    tone="danger"
    @confirm="confirm"
  />
</template>

<style module>
@reference "../../styles/index.css";

.dialog {
  width: 780px;
  max-height: 80vh;
}
.label {
  @apply flex h-9 w-9 items-center justify-center rounded-lg bg-[#FFEBE6];
}
.icon {
  @apply text-[#DE350B];
}
.row {
  @apply min-h-0 flex-1 overflow-y-auto;
}
.rowCentered {
  @apply flex flex-col items-center justify-center py-16;
}
.labelRound {
  @apply mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#F4F5F7];
}
.iconMuted {
  @apply text-[#C1C7D0];
}
.strongLabel {
  @apply text-[14px] font-medium text-[#6B778C];
}
.labelMuted {
  @apply mt-1 text-[12px] text-[#97A0AF];
}
.table {
  @apply w-full min-w-[700px] text-[12px];
}
.tableRow {
  @apply border-b border-[#DFE1E6] bg-[#F4F5F7] text-left text-[11px] font-semibold text-[#5E6C84];
}
.headerCell {
  @apply px-4 py-2.5;
}
.headerCellCentered {
  @apply px-4 py-2.5 text-center;
}
.productRow {
  @apply border-b border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.tableCell {
  @apply px-4 py-3;
}
.rowGap2 {
  @apply flex items-center gap-2;
}
.labelH7W7 {
  @apply h-7 w-7 shrink-0 overflow-hidden rounded border border-[#DFE1E6];
}
.image {
  @apply h-full w-full object-cover;
}
.labelCentered {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded border border-[#EBECF0] bg-[#F4F5F7];
}
.strongLabelAlternative {
  @apply text-[13px] font-normal;
}
.tableCellMuted {
  @apply px-4 py-3 text-[#6B778C];
}
.rowGap2Gap2 {
  @apply flex justify-center gap-2;
}
.primaryButton {
  @apply flex items-center gap-1 rounded px-2.5 py-1 text-app-primary hover:bg-app-primary-soft;
}
.dangerButtonDanger {
  @apply flex items-center gap-1 rounded px-2.5 py-1 text-[#DE350B] hover:bg-[#FFEBE6];
}
.footerCount {
  @apply mr-auto text-[12px] text-[#6B778C];
}
</style>
