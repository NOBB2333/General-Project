<script setup lang="ts">
import { Workflow, Plus, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows, ListPageSurface, normalizeListPageModel } from '@/components/page';
import type { ListPageRow, PageAction, PageModel } from '@/components/page';
import { UiButton, UiDialog, UiInput, toast } from '@/components/ui';

const props = defineProps<{ model: PageModel }>();
const rows = ref<ListPageRow[]>(cloneReferenceRows(props.model.rows));
const pageModel = computed(() => normalizeListPageModel(props.model, rows.value));
const activeRow = ref<ListPageRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const draftName = ref('');

function cellValue(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function openRow(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction, selectedRows: ListPageRow[]) {
  if (action.kind === 'export') {
    const rowsToExport = selectedRows.length ? selectedRows : rows.value;
    const header = props.model.columns.map((column) => column.title);
    const body = rowsToExport.map((row) => props.model.columns.map((column) => cellValue(row, column.key)));
    const csv = [header, ...body]
      .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
    link.download = `${props.model.pageSlug}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
    toast.success(`已导出 ${rowsToExport.length} 条${props.model.label}记录`);
    return;
  }
  if (action.kind === 'create') {
    draftName.value = '';
    createOpen.value = true;
    return;
  }
  toast.info(`${action.label}已在本地复刻会话执行`);
}

function saveDraft() {
  const name = draftName.value.trim();
  if (!name) {
    toast.warning(`请填写${props.model.label}名称`);
    return;
  }
  rows.value = [
    {
      id: `${props.model.pageSlug}-local-${Date.now()}`,
      code: `LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
      name,
      title: name,
      status: '草稿',
      owner: '当前用户',
      updatedAt: '2026-09-17 10:00',
    },
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
    <template #hero-icon>
      <span :class="$style.heroIcon">
        <Workflow
          :size="20"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #notice>
      <div :class="$style.notice">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <span>{{ props.model.description }}</span>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? cellValue(activeRow, 'code') : `${props.model.label}详情`"
    description="参考记录只读；本地创建记录仅保留在当前会话"
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
    <template #footer>
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    :title="`新建${props.model.label}`"
    description="仅在本地复刻会话中创建一条草稿"
  >
    <label :class="$style.formField">
      <span>名称</span>
      <UiInput
        v-model="draftName"
        :placeholder="`输入${props.model.label}名称`"
      />
    </label>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="saveDraft"
      >
        <Plus
          :size="14"
          :stroke-width="1.8"
        />保存草稿
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

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

.formField {
  @apply grid gap-1.5 text-[12px] font-medium text-[#42526E];
}
</style>
