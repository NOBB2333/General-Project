<script setup lang="ts">
import { Plus, Search, X } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '../../reference';
import type { PageAction } from '../../page.contracts';
import type { PageModel } from '../../page.types';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import ListPageSurface from './ListPageSurface.vue';
import { normalizeListPageModel } from './normalizeListPageModel';
import type { ListPageRow } from './list-page.types';

const props = defineProps<{ model: PageModel }>();
const rows = ref<ListPageRow[]>(cloneReferenceRows(props.model.rows));
const pageModel = computed(() => normalizeListPageModel(props.model, rows.value));
const activeRow = ref<ListPageRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const draftName = ref('');
const draftCode = ref('');

function openRow(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function cellValue(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function exportRows(selectedRows: ListPageRow[]) {
  const columns = props.model.columns;
  const csv = [
    columns.map((column) => column.title),
    ...selectedRows.map((row) => columns.map((column) => cellValue(row, column.key))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = `${props.model.pageSlug}.csv`;
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${selectedRows.length} 条${props.model.label}记录`);
}

function runAction(action: PageAction, selectedRows: ListPageRow[]) {
  if (action.kind === 'export') {
    exportRows(selectedRows.length ? selectedRows : rows.value);
    return;
  }
  if (action.kind === 'create') {
    draftName.value = '';
    draftCode.value = '';
    createOpen.value = true;
    return;
  }
  toast.info(`${action.label}仅在本地复刻页执行`);
}

function saveDraft() {
  const name = draftName.value.trim();
  if (!name) {
    toast.warning(`请填写${props.model.label}名称`);
    return;
  }
  const code =
    draftCode.value.trim() ||
    `${props.model.pageSlug
      .toUpperCase()
      .replace(/[^A-Z]/g, '')
      .slice(0, 5)}-${String(rows.value.length + 1).padStart(3, '0')}`;
  rows.value = [
    { id: `${props.model.pageSlug}-local-${Date.now()}`, code, name, status: '草稿', updatedAt: '2026-08-16 10:00' },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success(`${props.model.label}已保存为本地草稿`);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openRow"
  >
    <template #hero-icon
      ><span :class="$style.heroIcon"
        ><Search
          :size="20"
          :stroke-width="1.8" /></span
    ></template>
    <template #notice
      ><div :class="$style.notice">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><span>{{ props.model.description }}</span>
      </div></template
    >
  </ListPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? cellValue(activeRow, 'code') : `${props.model.label}详情`"
    description="参考记录只读，本地创建记录可在当前会话修改"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in props.model.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ cellValue(activeRow, column.key) }}</dd>
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
    :title="`新建${props.model.label}`"
    description="新建记录仅保存在本地演示会话"
  >
    <div :class="$style.formGrid">
      <UiFormField label="编号"
        ><UiInput
          v-model="draftCode"
          placeholder="可选，留空自动生成"
      /></UiFormField>
      <UiFormField label="名称"
        ><UiInput
          v-model="draftName"
          :placeholder="`请输入${props.model.label}名称`"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="createOpen = false"
        ><X :size="14" />取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveDraft"
        ><Plus :size="14" />保存</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg bg-[#DEEBFF] text-[#0052CC];
}
.notice {
  @apply flex items-center gap-2 rounded border border-[#B3D4FF] bg-[#DEEBFF]/40 px-3 py-2 text-[12px] text-[#42526E];
}
.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 text-[12px];
}
.detailGrid dt {
  @apply text-[#6B778C];
}
.detailGrid dd {
  @apply mt-1 font-medium text-[#172B4D];
}
.formGrid {
  @apply grid gap-4;
}
</style>
