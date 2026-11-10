<script setup lang="ts">
import { Landmark, ReceiptText, X } from 'lucide-vue-next';
import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows, ListPageSurface } from '@/components/page';
import type { PageAction } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import { collectionsReferenceFixture } from '@/features/sales/model/collections.data';
import { collectionsModel } from '@/features/sales/model/collections.model';
import type { SalesCollectionRow } from '@/features/sales/model/sales-ledger.types';

interface CollectionDraft {
  customer: string;
  payer: string;
  amount: string;
  date: string;
  method: string;
}

const rows = ref(cloneReferenceRows(collectionsReferenceFixture.rows));
const pageModel = computed(() => ({ ...collectionsModel, rows: rows.value }));
const detailOpen = ref(false);
const createOpen = ref(false);
const noticeVisible = ref(true);
const activeRow = ref<SalesCollectionRow | null>(null);
const draft = reactive<CollectionDraft>(emptyDraft());

function emptyDraft(): CollectionDraft {
  return { customer: '', payer: '', amount: '', date: '2026-08-16', method: '银行转账' };
}

function openDetails(row: SalesCollectionRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction) {
  if (action.kind === 'create') {
    Object.assign(draft, emptyDraft());
    createOpen.value = true;
    return;
  }
  if (action.kind === 'export') {
    exportRows(rows.value);
    return;
  }
  toast.info('导入入口仅保留本地演示，未读取外部文件');
}

function saveCollection() {
  if (!draft.customer.trim() || !draft.amount.trim()) {
    toast.warning('请填写客户和到账金额');
    return;
  }
  rows.value = [
    {
      id: `collection-local-${Date.now()}`,
      scopeGroup: 'mine',
      followed: true,
      code: `RC-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
      customer: draft.customer.trim(),
      owner: '张经理',
      date: draft.date,
      method: draft.method,
      payer: draft.payer.trim() || draft.customer.trim(),
      amount: draft.amount.trim(),
      allocated: `¥0.00 / ${draft.amount.trim()}`,
      status: '待分配',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('回款流水已保存到当前本地会话');
}

function exportRows(exportRows: SalesCollectionRow[]) {
  const keys = collectionsModel.columns.map((column) => column.key);
  const csv = [
    collectionsModel.columns.map((column) => column.title),
    ...exportRows.map((row) => keys.map((key) => String(row[key] ?? ''))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'sales-collections.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${exportRows.length} 笔回款流水`);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openDetails"
  >
    <template #hero-icon
      ><span :class="$style.heroIcon"
        ><Landmark
          :size="22"
          :stroke-width="1.8" /></span
    ></template>
    <template #hero-art
      ><div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <ReceiptText
          :size="82"
          :stroke-width="1.2"
        /></div
    ></template>
    <template
      v-if="noticeVisible"
      #notice
    >
      <div :class="$style.notice">
        <span
          ><Landmark
            :size="16"
            :stroke-width="1.8"
          />
          银行到账流水支持自动匹配订单，也可手动拆分分配。</span
        ><button
          type="button"
          :class="$style.noticeLink"
          @click="toast.info('选择一笔流水后可继续分配到多个销售订单')"
        >
          查看说明</button
        ><button
          type="button"
          :class="$style.noticeClose"
          aria-label="关闭回款说明"
          @click="noticeVisible = false"
        >
          <X
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '回款流水详情'"
    :description="activeRow ? `${activeRow.customer} · ${activeRow.status}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in collectionsModel.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ activeRow[column.key] || '—' }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    title="新建回款流水"
    description="记录只保存在当前本地复刻会话中。"
    size="lg"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="saveCollection"
    >
      <UiFormField
        label="客户"
        required
        wide
        ><UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
      /></UiFormField>
      <UiFormField label="付款方"
        ><UiInput
          v-model="draft.payer"
          placeholder="默认使用客户名称"
      /></UiFormField>
      <UiFormField
        label="到账金额"
        required
        ><UiInput
          v-model="draft.amount"
          placeholder="例如：¥100,000"
      /></UiFormField>
      <UiFormField label="到账日期"><UiInput v-model="draft.date" /></UiFormField>
      <UiFormField label="方式"><UiInput v-model="draft.method" /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveCollection"
        >保存流水</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.heroIcon {
  @apply inline-flex items-center justify-center rounded p-2;
  color: white;
  background: rgb(255 255 255 / 18%);
}
.heroArt {
  @apply flex items-center justify-center opacity-75;
  color: rgb(255 255 255 / 78%);
}
.notice {
  @apply flex items-center gap-2 rounded border px-3 py-2 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.notice span {
  @apply flex items-center gap-2;
}
.noticeLink {
  @apply ml-2 font-semibold;
  color: var(--app-primary);
}
.noticeClose {
  @apply ml-auto inline-flex items-center justify-center;
  color: var(--ui-text-subtle);
}
.detailGrid {
  @apply grid grid-cols-2 gap-3;
}
.detailGrid div {
  @apply rounded border px-3 py-2;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.detailGrid dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply mt-1 text-[13px] font-semibold;
  color: var(--ui-text);
}
.formGrid {
  @apply grid grid-cols-2 gap-4;
}
</style>
