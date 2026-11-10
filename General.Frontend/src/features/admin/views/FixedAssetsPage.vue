<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  Box,
  ChartNoAxesColumn,
  CircleDollarSign,
  Download,
  Gauge,
  Monitor,
  Plus,
  Search,
  Sofa,
  TriangleAlert,
  Truck,
  Upload,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { fixedAssetsFixture } from '../model/fixed-assets.data';
import type { AssetCategory, AssetStatus, FixedAsset, FixedAssetDraft } from '../model/fixed-assets.types';
import FixedAssetsTable from './fixed-assets/FixedAssetsTable.vue';

const assets = ref<FixedAsset[]>(cloneReferenceRows(fixedAssetsFixture.rows));
const category = ref<AssetCategory | '全部'>('全部');
const status = ref<AssetStatus | '全部状态'>('全部状态');
const department = ref('全部部门');
const sort = ref('按编号');
const query = ref('');
const formOpen = ref(false);
const importOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<FixedAsset | null>(null);
const draft = reactive<FixedAssetDraft>({
  code: '',
  name: '',
  category: '电子设备',
  originalValue: '',
  user: '',
  location: '',
});

const categories: readonly { value: AssetCategory | '全部'; label: string; count: number }[] = [
  { value: '全部', label: '全部', count: 14 },
  { value: '电子设备', label: '电子设备', count: 8 },
  { value: '办公家具', label: '办公家具', count: 2 },
  { value: '生产设备', label: '生产设备', count: 1 },
  { value: '交通工具', label: '交通工具', count: 1 },
  { value: '仪器仪表', label: '仪器仪表', count: 2 },
];
const statusOptions: readonly UiSelectOption[] = ['全部状态', '在用', '闲置', '维修中', '已报废', '待处置'].map(
  (value) => ({ value, label: value }),
);
const departmentOptions: readonly UiSelectOption[] = [
  '全部部门',
  '工程部',
  '销售部',
  '技术部',
  '行政部',
  '总经办',
  '市场部',
  'IT部',
].map((value) => ({ value, label: value }));
const sortOptions: readonly UiSelectOption[] = ['按编号', '按原值', '按净值', '按累计折旧'].map((value) => ({
  value,
  label: value,
}));
const categoryOptions: readonly UiSelectOption[] = categories
  .slice(1)
  .map((item) => ({ value: item.value, label: item.label }));

const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return assets.value.filter(
    (item) =>
      (category.value === '全部' || item.category === category.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (department.value === '全部部门' || item.department === department.value) &&
      (!needle || `${item.code} ${item.name} ${item.brand}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});

function openCreate() {
  Object.assign(draft, {
    code: `FA-2026-${String(assets.value.length + 1).padStart(3, '0')}`,
    name: '',
    category: '电子设备',
    originalValue: '',
    user: '',
    location: '',
  });
  formOpen.value = true;
}
function saveAsset() {
  const value = Number(draft.originalValue);
  if (!draft.name.trim() || !Number.isFinite(value) || value <= 0) return toast.warning('请填写资产名称和有效原值');
  assets.value.unshift({
    id: `asset-local-${assets.value.length + 1}`,
    code: draft.code,
    name: draft.name.trim(),
    brand: '本地登记',
    specification: '待补充',
    category: draft.category,
    originalValue: value,
    netValue: value,
    monthlyDepreciation: 0,
    user: draft.user.trim() || '未分配',
    status: '在用',
    location: draft.location.trim() || '待分配',
    department: '行政部',
  });
  category.value = '全部';
  formOpen.value = false;
  toast.success('固定资产已登记到本地台账');
}
function openDetail(asset: FixedAsset) {
  selected.value = asset;
  detailOpen.value = true;
}
function exportAssets() {
  const lines = [
    ['资产编号', '资产名称', '分类', '原值', '净值', '使用人', '状态'],
    ...filtered.value.map((item) => [
      item.code,
      item.name,
      item.category,
      item.originalValue,
      item.netValue,
      item.user,
      item.status,
    ]),
  ];
  const blob = new Blob([lines.map((line) => line.join(',')).join('\n')], { type: 'text/csv;charset=utf-8' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = 'fixed-assets.csv';
  link.click();
  URL.revokeObjectURL(link.href);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/fixed-assets"
    data-page-label="固定资产"
    data-page-module="行政管理"
    data-fixed-assets-page
    :data-result-count="filtered.length"
    :data-total-count="assets.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 固定资产</nav>
        <section>
          <span
            ><Monitor
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>固定资产登记与折旧</h1>
            <p>资产台账、折旧核算、使用人变更、报废流程的统一管控</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i /><i /><i /><b /><em />
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <div>
          <span>资产总数</span><strong>14<small>件</small></strong>
        </div>
        <Box
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div><span>资产原值</span><strong>¥424,600.00</strong></div>
        <CircleDollarSign
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div><span>资产净值</span><strong>¥291,672.76</strong></div>
        <ChartNoAxesColumn
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div><span>月折旧额</span><strong>¥4,823.35</strong></div>
        <Gauge
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div><span>综合折旧率</span><strong>31.3%</strong></div>
        <TriangleAlert
          :size="18"
          :stroke-width="1.8"
        />
      </article>
    </section>
    <nav
      :class="$style.categoryTabs"
      aria-label="资产分类"
    >
      <button
        v-for="item in categories"
        :key="item.value"
        :data-active="category === item.value"
        @click="category = item.value"
      >
        <Monitor
          v-if="item.value === '电子设备'"
          :size="13"
          :stroke-width="1.8"
        /><Sofa
          v-else-if="item.value === '办公家具'"
          :size="13"
          :stroke-width="1.8"
        /><Truck
          v-else-if="item.value === '交通工具'"
          :size="13"
          :stroke-width="1.8"
        /><Box
          v-else
          :size="13"
          :stroke-width="1.8"
        />{{ item.label }} <em>{{ item.count }}</em>
      </button>
    </nav>
    <section :class="$style.toolbar">
      <label :class="$style.search"
        ><Search
          :size="15"
          :stroke-width="1.8" /><UiInput
          v-model="query"
          aria-label="搜索固定资产"
          placeholder="搜索资产编号、名称、品牌..."
      /></label>
      <UiSelect
        v-model="status"
        label="资产状态"
        :options="statusOptions"
        :class="$style.filter"
      /><UiSelect
        v-model="department"
        label="资产部门"
        :options="departmentOptions"
        :class="$style.filter"
      />
      <UiSelect
        v-model="sort"
        label="资产排序"
        :options="sortOptions"
        :class="$style.sort"
      /><UiButton
        variant="secondary"
        size="sm"
        @click="importOpen = true"
        ><template #leading
          ><Upload
            :size="14"
            :stroke-width="1.8" /></template
        >批量导入</UiButton
      ><UiButton
        variant="secondary"
        size="sm"
        @click="exportAssets"
        ><template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="openCreate"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新增资产</UiButton
      >
    </section>
    <FixedAssetsTable
      :rows="filtered"
      @view="openDetail"
    />
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新增资产"
    description="登记资产基础信息和初始价值"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="资产编号"
        required
        ><UiInput v-model="draft.code" /></UiFormField
      ><UiFormField
        label="资产名称"
        required
        ><UiInput v-model="draft.name" /></UiFormField
      ><UiFormField label="资产分类"
        ><UiSelect
          v-model="draft.category"
          label="新增资产分类"
          :options="categoryOptions" /></UiFormField
      ><UiFormField
        label="资产原值"
        required
        ><UiInput v-model="draft.originalValue" /></UiFormField
      ><UiFormField label="使用人"><UiInput v-model="draft.user" /></UiFormField
      ><UiFormField label="存放地点"><UiInput v-model="draft.location" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveAsset"
        >保存资产</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="importOpen"
    title="批量导入资产"
    description="仅在本地模拟导入流程"
    size="md"
    ><div :class="$style.importBox">
      <Upload
        :size="28"
        :stroke-width="1.8"
      /><strong>选择资产台账文件</strong>
      <p>支持 XLSX、CSV，当前复刻环境不会向外部站点上传。</p>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="importOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="
          importOpen = false;
          toast.success('本地导入校验已完成');
        "
        >开始校验</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.name || '资产详情'"
    :description="selected?.code"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>分类</dt>
          <dd>{{ selected.category }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
        <div>
          <dt>原值</dt>
          <dd>¥{{ selected.originalValue.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</dd>
        </div>
        <div>
          <dt>净值</dt>
          <dd>¥{{ selected.netValue.toLocaleString('zh-CN', { minimumFractionDigits: 2 }) }}</dd>
        </div>
        <div>
          <dt>使用人</dt>
          <dd>{{ selected.user }}</dd>
        </div>
        <div>
          <dt>存放地点</dt>
          <dd>{{ selected.location }}</dd>
        </div>
      </dl>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-3 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #034c65, #006d83 55%, #00a3b8);
}
.hero > div:first-child {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.25);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.76);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[62%];
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.24) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.24) 1px, transparent 1px);
  background-size: 52px 52px;
}
.heroArt i {
  @apply absolute bottom-[-10px] h-24 w-32 rounded-t border-[6px];
  border-color: #063c59;
  background: #158bb3;
}
.heroArt i:nth-child(1) {
  left: 20%;
}
.heroArt i:nth-child(2) {
  left: 55%;
  bottom: -28px;
  width: 190px;
}
.heroArt i:nth-child(3) {
  right: 4%;
  bottom: -42px;
  width: 90px;
}
.heroArt b,
.heroArt em {
  @apply absolute bottom-6 h-2 w-14;
  background: #68c8dd;
}
.heroArt b {
  left: 59%;
}
.heroArt em {
  right: 8%;
  transform: rotate(-40deg);
}
.metrics {
  @apply mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex min-h-[100px] items-start rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article div {
  @apply min-w-0 flex-1;
}
.metrics span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-4 block text-[18px] font-semibold;
}
.metrics small {
  @apply ml-1 text-[9px];
  color: var(--ui-text-subtle);
}
.metrics svg {
  @apply rounded p-2;
  box-sizing: content-box;
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(2) svg {
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(3) svg {
  color: #6554c0;
  background: #eae6ff;
}
.metrics article:nth-child(4) svg {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(5) svg {
  color: #de350b;
  background: #ffebe6;
}
.categoryTabs {
  @apply mb-4 flex items-center gap-2;
}
.categoryTabs button {
  @apply flex h-8 items-center gap-1 rounded-full border px-3 text-[10px] font-medium;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: white;
}
.categoryTabs button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: var(--app-primary-soft);
}
.categoryTabs em {
  @apply not-italic;
  color: var(--ui-text-subtle);
}
.toolbar {
  @apply mb-3 flex h-11 items-center gap-2;
}
.search {
  @apply flex w-[280px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.filter {
  @apply w-[105px];
}
.sort {
  @apply ml-auto w-[105px];
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.importBox {
  @apply m-5 flex min-h-[180px] flex-col items-center justify-center rounded border border-dashed text-center;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.importBox strong {
  @apply mt-3 text-[12px];
  color: var(--ui-text);
}
.importBox p {
  @apply mt-1 text-[10px];
}
.detail {
  @apply p-5;
}
.detail dl {
  @apply grid grid-cols-2 gap-3;
}
.detail dl div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[11px] font-medium;
}
</style>
