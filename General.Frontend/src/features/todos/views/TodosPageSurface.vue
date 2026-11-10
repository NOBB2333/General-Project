<script setup lang="ts">
import { CheckSquare, Plus } from 'lucide-vue-next';
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
const draft = ref('');

function cellValue(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function openRow(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction) {
  if (action.kind === 'create') {
    draft.value = '';
    createOpen.value = true;
    return;
  }
  toast.info(`${action.label}已在本地复刻会话执行`);
}

function saveTodo() {
  const name = draft.value.trim();
  if (!name) {
    toast.warning('请填写待办名称');
    return;
  }
  rows.value = [
    {
      id: `todos-local-${Date.now()}`,
      code: `TODO-${String(rows.value.length + 1).padStart(4, '0')}`,
      name,
      title: name,
      status: '待处理',
      owner: '当前用户',
      updatedAt: '2026-09-17 10:00',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('待办已保存到当前本地会话');
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openRow"
  >
    <template #hero-icon>
      <span :class="$style.heroIcon"
        ><CheckSquare
          :size="20"
          :stroke-width="1.8"
      /></span>
    </template>
  </ListPageSurface>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? cellValue(activeRow, 'name') : '待办详情'"
    description="参考任务只读详情"
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
    title="新建待办"
    description="仅在当前本地会话创建草稿"
  >
    <label :class="$style.formField">
      <span>待办名称</span>
      <UiInput
        v-model="draft"
        placeholder="输入任务名称"
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
        @click="saveTodo"
        ><Plus
          :size="14"
          :stroke-width="1.8"
        />保存</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg bg-[#E3FCEF] text-[#00875A];
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
