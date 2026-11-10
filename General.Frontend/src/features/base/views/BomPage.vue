<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiAlertDialog, UiButton, UiCheckbox, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { bomReferenceFixture } from '../model/bom.data';
import type { BomCategoryFilter, BomRow, BomStatusFilter, BomType, BomTypeFilter } from '../model/bom.types';
import BomCategoryTree from './bom/BomCategoryTree.vue';
import BomHero from './bom/BomHero.vue';
import BomStats from './bom/BomStats.vue';
import BomTable from './bom/BomTable.vue';
import BomToolbar from './bom/BomToolbar.vue';

const rows = ref<BomRow[]>(cloneReferenceRows(bomReferenceFixture.rows));
const status = ref<BomStatusFilter>('all');
const type = ref<BomTypeFilter>('all');
const category = ref<BomCategoryFilter>('all');
const query = ref('');
const activeRow = ref<BomRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const compareOpen = ref(false);
const deleteOpen = ref(false);
const pendingDelete = ref<BomRow | null>(null);
const compareIds = ref<Set<string>>(new Set());
const compareReady = ref(false);
const draftName = ref('');
const draftProduct = ref('');
const draftType = ref<BomType>('标准');

const typeOptions: UiSelectOption[] = [
  { value: '标准', label: '标准 BOM' },
  { value: '项目', label: '项目 BOM' },
];
const statusValue: Record<BomStatusFilter, string | null> = {
  all: null,
  draft: '草稿',
  review: '待评审',
  active: '已生效',
  invalid: '已失效',
  archived: '已归档',
};

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter((row) => {
    const matchesStatus = !statusValue[status.value] || row.status === statusValue[status.value];
    const matchesType = type.value === 'all' || (type.value === 'standard' ? row.type === '标准' : row.type === '项目');
    const matchesCategory =
      category.value === 'all' ||
      (category.value === 'standard' && row.type === '标准') ||
      (category.value === 'project' && row.type === '项目') ||
      (category.value === 'control' && ['dcs', 'sis'].includes(row.category)) ||
      (category.value === 'drive' && row.category === 'vfd') ||
      row.category === category.value;
    const matchesQuery = !keyword || `${row.code} ${row.name} ${row.productCode}`.toLowerCase().includes(keyword);
    return matchesStatus && matchesType && matchesCategory && matchesQuery;
  });
});

function openRow(row: BomRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function copyRow(row: BomRow) {
  rows.value = [
    {
      ...row,
      id: `${row.id}-copy-${Date.now()}`,
      code: `${row.code}-COPY`,
      name: `${row.name}（副本）`,
      status: '草稿',
    },
    ...rows.value,
  ];
  toast.success('BOM 已复制到本地草稿');
}
function transitionRow(row: BomRow) {
  row.status = row.status === '草稿' ? '待评审' : row.status === '待评审' ? '已生效' : '草稿';
  toast.success(`${row.code} 已更新为${row.status}`);
}
function requestRemove(row: BomRow) {
  pendingDelete.value = row;
  deleteOpen.value = true;
}
function confirmRemove() {
  if (!pendingDelete.value) return;
  rows.value = rows.value.filter((row) => row.id !== pendingDelete.value?.id);
  toast.success('BOM 草稿已从本地删除');
  deleteOpen.value = false;
  pendingDelete.value = null;
}
function openCreate() {
  draftName.value = '';
  draftProduct.value = '';
  draftType.value = '标准';
  createOpen.value = true;
}
function setDraftType(value: string) {
  if (value === '标准' || value === '项目') draftType.value = value;
}
function saveBom() {
  if (!draftName.value.trim() || !draftProduct.value.trim()) {
    toast.warning('请填写 BOM 名称和产品编码');
    return;
  }
  rows.value = [
    {
      id: `bom-local-${Date.now()}`,
      code: `BOM-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
      name: draftName.value.trim(),
      productCode: draftProduct.value.trim(),
      type: draftType.value,
      version: 'V1.0',
      status: '草稿',
      project: draftType.value === '项目' ? '待关联项目' : '-',
      materialCount: 0,
      cost: '¥0',
      updatedAt: '2026-08-16',
      creator: '张经理',
      category: draftType.value === '项目' ? 'huaneng' : 'dcs',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('BOM 已保存到本地草稿');
}
function toggleCompare(id: string) {
  const next = new Set(compareIds.value);
  next.has(id) ? next.delete(id) : next.add(id);
  compareIds.value = next;
  compareReady.value = false;
}
function runCompare() {
  if (compareIds.value.size < 2) {
    toast.warning('请选择至少两个 BOM');
    return;
  }
  compareReady.value = true;
}
function exportRows() {
  const lines = [
    ['BOM编号', 'BOM名称', '产品', '版本', '状态'],
    ...rows.value.map((row) => [row.code, row.name, row.productCode, row.version, row.status]),
  ];
  const url = URL.createObjectURL(
    new Blob([`\uFEFF${lines.map((line) => line.join(',')).join('\n')}`], { type: 'text/csv;charset=utf-8' }),
  );
  const link = document.createElement('a');
  link.href = url;
  link.download = 'bom.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success('已导出 BOM 列表');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/base/bom"
    data-bom-page
    :data-status-filter="status"
    :data-type-filter="type"
    :data-category-filter="category"
  >
    <BomHero
      @compare="compareOpen = true"
      @export="exportRows"
      @create="openCreate"
    />
    <BomStats
      :total="rows.length"
      :draft="rows.filter((row) => row.status === '草稿').length"
      :review="rows.filter((row) => row.status === '待评审').length"
      :active="rows.filter((row) => row.status === '已生效').length"
    />
    <section :class="$style.workspace">
      <BomCategoryTree
        :active="category"
        @select="category = $event"
      />
      <div :class="$style.listPane">
        <BomToolbar
          v-model:status="status"
          v-model:type="type"
          v-model:query="query"
          :total="filteredRows.length"
        />
        <BomTable
          :rows="filteredRows"
          @open="openRow"
          @copy="copyRow"
          @transition="transitionRow"
          @remove="requestRemove"
        />
      </div>
    </section>
  </main>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? 'BOM详情'"
    :description="activeRow?.name"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>产品 / 设备</dt>
        <dd>{{ activeRow.productCode }}</dd>
      </div>
      <div>
        <dt>类型与版本</dt>
        <dd>{{ activeRow.type }} · {{ activeRow.version }}</dd>
      </div>
      <div>
        <dt>当前状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>物料与成本</dt>
        <dd>{{ activeRow.materialCount }} 项 · {{ activeRow.cost }}</dd>
      </div>
      <div>
        <dt>适用项目</dt>
        <dd>{{ activeRow.project }}</dd>
      </div>
      <div>
        <dt>更新信息</dt>
        <dd>{{ activeRow.updatedAt }} · {{ activeRow.creator }}</dd>
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
    title="新建BOM"
    description="新增内容仅保存在本地复刻页面"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveBom"
    >
      <UiFormField
        label="BOM名称"
        required
        wide
        ><UiInput
          v-model="draftName"
          placeholder="请输入 BOM 名称"
      /></UiFormField>
      <UiFormField
        label="产品编码"
        required
        ><UiInput
          v-model="draftProduct"
          placeholder="例如 FP-DCS-200"
      /></UiFormField>
      <UiFormField label="BOM类型"
        ><UiSelect
          :model-value="draftType"
          :options="typeOptions"
          label="BOM类型"
          @update:model-value="setDraftType"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveBom"
        >保存草稿</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="compareOpen"
    title="BOM对比"
    description="选择至少两个 BOM 比较版本、物料数量和成本"
    size="lg"
  >
    <div :class="$style.compareList">
      <label
        v-for="row in rows"
        :key="row.id"
        ><UiCheckbox
          :model-value="compareIds.has(row.id)"
          :label="`选择 ${row.code}`"
          @update:model-value="toggleCompare(row.id)"
        /><span
          ><strong>{{ row.code }}</strong
          >{{ row.name }}</span
        ><b>{{ row.version }} · {{ row.materialCount }} 项 · {{ row.cost }}</b></label
      >
      <p
        v-if="compareReady"
        :class="$style.compareResult"
      >
        已生成 {{ compareIds.size }} 个 BOM 的本地对比结果；差异项按版本、物料数和成本展示。
      </p>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="compareOpen = false"
        >关闭</UiButton
      ><UiButton
        variant="primary"
        @click="runCompare"
        >开始对比</UiButton
      ></template
    >
  </UiDialog>

  <UiAlertDialog
    v-model:open="deleteOpen"
    title="删除 BOM 草稿"
    :description="`确认删除 ${pendingDelete?.code ?? ''}？该操作只影响本地页面。`"
    confirm-label="删除"
    @confirm="confirmRemove"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-3 pb-8;
  color: var(--ui-text);
}
.workspace {
  @apply flex min-h-[410px] overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.listPane {
  @apply min-w-0 flex-1;
}
.details {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.compareList {
  @apply space-y-2 p-5;
}
.compareList label {
  @apply grid min-h-12 grid-cols-[24px_1fr_auto] items-center gap-2 rounded border px-3;
  border-color: var(--ui-border);
}
.compareList label > span {
  @apply flex flex-col text-[11px];
  color: var(--ui-text-subtle);
}
.compareList label strong {
  @apply text-[12px];
  color: var(--app-primary);
}
.compareList label > b {
  @apply text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.compareResult {
  @apply rounded border px-3 py-2 text-[12px];
  color: #006644;
  border-color: #abf5d1;
  background: #e3fcef;
}
</style>
