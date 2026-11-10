<script setup lang="ts">
import { computed, ref, type Component } from 'vue';
import { ClipboardList, FileSpreadsheet, Info, Package, Upload, Warehouse } from 'lucide-vue-next';
import { ListPageSurface, cloneReferenceRows } from '@/components/page';
import type { ListPageModel, ListPageRow, PageAction, PageModel } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';

type FeatureListModel = Omit<ListPageModel, 'surface'> & Pick<PageModel, 'surface'>;

const props = defineProps<{
  model: FeatureListModel;
  fixtureRows: readonly ListPageRow[];
  entityLabel: string;
  icon?: Component;
  toolHint?: string;
}>();

const rows = ref<ListPageRow[]>(cloneReferenceRows(props.fixtureRows));
const pageModel = computed<ListPageModel>(() => ({ ...props.model, surface: 'list', rows: rows.value }));
const activeRow = ref<ListPageRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const importOpen = ref(false);
const pendingAction = ref<PageAction | null>(null);
const actionOpen = ref(false);
const draftCode = ref('');
const draftName = ref('');
const draftOwner = ref('张经理');

const entityIcon = computed(() => props.icon ?? (props.entityLabel.includes('仓库') ? Warehouse : Package));
const nameKey = computed(
  () => props.model.columns.find((column) => column.key === 'name' || column.key === 'title')?.key ?? 'name',
);
const codeKey = computed(() => props.model.columns.find((column) => column.key === 'code')?.key ?? 'code');

function openDetails(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction, selectedRows: ListPageRow[]) {
  pendingAction.value = action;
  if (action.kind === 'create') {
    draftCode.value = '';
    draftName.value = '';
    draftOwner.value = '张经理';
    createOpen.value = true;
    return;
  }
  if (action.kind === 'import') {
    importOpen.value = true;
    return;
  }
  if (action.kind === 'export') {
    exportRows(selectedRows.length ? selectedRows : rows.value);
    return;
  }
  actionOpen.value = true;
}

function saveRecord() {
  if (!draftName.value.trim()) {
    toast.warning(`请填写${props.entityLabel}名称`);
    return;
  }
  const prefix =
    props.model.pageSlug
      .toUpperCase()
      .replace(/[^A-Z]/g, '')
      .slice(0, 4) || 'BASE';
  const next: ListPageRow = {
    id: `${props.model.pageSlug}-local-${Date.now()}`,
    [codeKey.value]: draftCode.value.trim() || `${prefix}-${String(rows.value.length + 1).padStart(4, '0')}`,
    [nameKey.value]: draftName.value.trim(),
    owner: draftOwner.value,
    status: '草稿',
    updatedAt: '2026-08-16 10:00',
  };
  rows.value = [next, ...rows.value];
  createOpen.value = false;
  toast.success(`${props.entityLabel}已保存为本地草稿`);
}

function importDemoRows() {
  const imported: ListPageRow = {
    id: `${props.model.pageSlug}-import-${Date.now()}`,
    [codeKey.value]: `${props.model.pageSlug.toUpperCase().slice(0, 4)}-IMPORT`,
    [nameKey.value]: `导入的${props.entityLabel}`,
    owner: '张经理',
    status: '草稿',
    updatedAt: '2026-08-16 10:00',
  };
  rows.value = [imported, ...rows.value];
  importOpen.value = false;
  toast.success(`已导入 1 条${props.entityLabel}演示数据`);
}

function confirmAction() {
  if (!pendingAction.value) return;
  toast.success(`${pendingAction.value.label}已在本地完成`);
  actionOpen.value = false;
  pendingAction.value = null;
}

function exportRows(exportRows: ListPageRow[]) {
  const columns = props.model.columns.filter((column) => column.key !== 'action');
  const content = [
    columns.map((column) => column.title),
    ...exportRows.map((row) => columns.map((column) => String(row[column.key] ?? ''))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${content}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = `${props.model.pageSlug}.csv`;
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${exportRows.length} 条${props.entityLabel}`);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openDetails"
  >
    <template #hero-icon>
      <span :class="$style.heroIcon">
        <component
          :is="entityIcon"
          :size="22"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #hero-art>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <ClipboardList
          :size="86"
          :stroke-width="1.2"
        />
        <FileSpreadsheet
          :size="54"
          :stroke-width="1.2"
        />
      </div>
    </template>
    <template #notice>
      <div :class="$style.notice">
        <Info
          :size="15"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <span>{{ toolHint ?? `本页数据来自参考站只读快照，新增和导入仅保存在当前本地会话。` }}</span>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="String(activeRow?.[codeKey] ?? `${props.entityLabel}详情`)"
    :description="activeRow ? String(activeRow[nameKey] ?? '') : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in model.columns.filter((item) => item.key !== 'action')"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ activeRow[column.key] || '—' }}</dd>
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
    :title="`新建${entityLabel}`"
    description="创建仅保存在当前本地会话中的草稿"
    size="md"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="saveRecord"
    >
      <UiFormField
        label="名称"
        required
        wide
      >
        <UiInput
          v-model="draftName"
          :placeholder="`请输入${entityLabel}名称`"
        />
      </UiFormField>
      <UiFormField label="编号">
        <UiInput
          v-model="draftCode"
          placeholder="可选，留空自动生成"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiInput v-model="draftOwner" />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="saveRecord"
        >保存草稿</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="importOpen"
    :title="`导入${entityLabel}`"
    description="本地演示导入，不会上传到参考站"
    size="sm"
  >
    <div :class="$style.importBody">
      <Upload
        :size="20"
        :stroke-width="1.8"
        aria-hidden="true"
      />
      <p>确认导入一条演示记录？实际项目可在这里接入 CSV 模板。</p>
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="importOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="importDemoRows"
        >确认导入</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="actionOpen"
    :title="pendingAction?.label ?? `${entityLabel}操作`"
    description="该操作仅作用于本地复刻页面"
    size="sm"
  >
    <p :class="$style.actionMessage">确认执行“{{ pendingAction?.label }}”？完成后会通过统一消息反馈结果。</p>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="actionOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="confirmAction"
        >确认执行</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.heroIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  border-color: color-mix(in srgb, white 35%, transparent);
  background: color-mix(in srgb, white 14%, transparent);
}

.heroArt {
  @apply absolute bottom-0 right-36 flex items-end gap-12;
  color: color-mix(in srgb, white 55%, transparent);
}

.notice {
  @apply flex items-center gap-2 rounded border px-3 py-2 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}

.detailGrid dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.importBody {
  @apply flex items-start gap-3 p-5 text-[13px];
  color: var(--ui-text-subtle);
}

.importBody svg {
  @apply shrink-0;
  color: var(--app-primary);
}

.importBody p {
  @apply m-0 leading-5;
}

.actionMessage {
  @apply m-0 p-5 text-[13px] leading-6;
  color: var(--ui-text-subtle);
}
</style>
