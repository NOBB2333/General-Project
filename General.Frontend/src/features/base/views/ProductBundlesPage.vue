<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import { productBundlesReferenceFixture } from '../model/product-bundles.data';
import type { ProductBundleRow, ProductBundleStatusFilter } from '../model/product-bundles.types';
import ProductBundlesHero from './product-bundles/ProductBundlesHero.vue';
import ProductBundleStats from './product-bundles/ProductBundleStats.vue';
import ProductBundleTable from './product-bundles/ProductBundleTable.vue';
import ProductBundleToolbar from './product-bundles/ProductBundleToolbar.vue';

const rows = ref<ProductBundleRow[]>(cloneReferenceRows(productBundlesReferenceFixture.rows));
const query = ref('');
const status = ref<ProductBundleStatusFilter>('all');
const detailOpen = ref(false);
const createOpen = ref(false);
const activeRow = ref<ProductBundleRow | null>(null);
const draftName = ref('');
const draftCode = ref('');

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter((row) => {
    const matchesStatus =
      status.value === 'all' || (status.value === 'active' ? row.status === '启用' : row.status === '停用');
    const matchesKeyword = !keyword || `${row.code} ${row.name} ${row.description}`.toLowerCase().includes(keyword);
    return matchesStatus && matchesKeyword;
  });
});
const activeCount = computed(() => rows.value.filter((row) => row.status === '启用').length);
const referenceCount = computed(() => rows.value.reduce((total, row) => total + row.referenceCount, 0));

function openRow(row: ProductBundleRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function editRow(row: ProductBundleRow) {
  openRow(row);
  toast.info('编辑功能已在本地详情中打开');
}
function copyRow(row: ProductBundleRow) {
  rows.value = [
    {
      ...row,
      id: `${row.id}-copy-${Date.now()}`,
      code: `${row.code}-COPY`,
      name: `${row.name}（副本）`,
      status: '停用',
      referenceCount: 0,
    },
    ...rows.value,
  ];
  toast.success('商品组合已复制到本地列表');
}
function toggleRow(row: ProductBundleRow) {
  row.status = row.status === '启用' ? '停用' : '启用';
  toast.success(`已${row.status === '启用' ? '启用' : '停用'} ${row.code}`);
}
function removeRow(row: ProductBundleRow) {
  rows.value = rows.value.filter((item) => item.id !== row.id);
  toast.success(`${row.code} 已从本地列表删除`);
}
function openCreate() {
  draftName.value = '';
  draftCode.value = '';
  createOpen.value = true;
}
function saveBundle() {
  if (!draftName.value.trim()) {
    toast.warning('请输入组合名称');
    return;
  }
  rows.value = [
    {
      id: `bundle-local-${Date.now()}`,
      code: draftCode.value.trim() || `BDL-${String(rows.value.length + 1).padStart(3, '0')}`,
      name: draftName.value.trim(),
      description: '本地新建商品组合',
      productCount: 0,
      cost: '¥0',
      suggestedPrice: '¥0',
      margin: '毛利 0%',
      status: '停用',
      referenceCount: 0,
      creator: '张经理',
      createdAt: '2026-08-16',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('商品组合已保存到本地列表');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/base/product-bundles"
    data-product-bundles-page
    :data-status-filter="status"
  >
    <ProductBundlesHero />
    <ProductBundleStats
      :total="rows.length"
      :active="activeCount"
      :product-kinds="14"
      :references="referenceCount"
    />
    <section :class="$style.listArea">
      <ProductBundleToolbar
        v-model:query="query"
        v-model:status="status"
        :total="filteredRows.length"
        @create="openCreate"
      />
      <ProductBundleTable
        :rows="filteredRows"
        @open="openRow"
        @edit="editRow"
        @copy="copyRow"
        @toggle="toggleRow"
        @remove="removeRow"
      />
    </section>
  </main>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '商品组合详情'"
    :description="activeRow?.name"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>组合说明</dt>
        <dd>{{ activeRow.description }}</dd>
      </div>
      <div>
        <dt>商品数</dt>
        <dd>{{ activeRow.productCount }}</dd>
      </div>
      <div>
        <dt>成本合计</dt>
        <dd>{{ activeRow.cost }}</dd>
      </div>
      <div>
        <dt>建议售价</dt>
        <dd>{{ activeRow.suggestedPrice }} / {{ activeRow.margin }}</dd>
      </div>
      <div>
        <dt>状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>创建信息</dt>
        <dd>{{ activeRow.creator }} · {{ activeRow.createdAt }}</dd>
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
    title="新建商品组合"
    description="新增内容仅保存在本地复刻页面"
    size="md"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveBundle"
    >
      <UiFormField
        label="组合名称"
        required
        wide
        ><UiInput
          v-model="draftName"
          placeholder="请输入组合名称"
      /></UiFormField>
      <UiFormField
        label="组合编号"
        wide
        ><UiInput
          v-model="draftCode"
          placeholder="留空将自动生成"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveBundle"
        >保存组合</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-3 pb-8;
  color: var(--ui-text);
}
.listArea {
  @apply min-w-0;
}
.details {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}
.details div:first-child {
  @apply col-span-2;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium leading-5;
  color: var(--ui-text);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
