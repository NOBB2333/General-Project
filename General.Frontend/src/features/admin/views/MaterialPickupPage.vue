<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { Archive, Box, ClipboardList, History, PackageCheck, Plus, Search } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { materialRequestsFixture, materialStocks } from '../model/material-pickup.data';
import type { MaterialRequest, MaterialRequestDraft, MaterialRequestStatus } from '../model/material-pickup.types';
import MaterialRequestCards from './material-pickup/MaterialRequestCards.vue';

type View = 'requests' | 'stock' | 'history';
const requests = ref<MaterialRequest[]>(cloneReferenceRows(materialRequestsFixture.rows));
const view = ref<View>('requests');
const query = ref('');
const status = ref<MaterialRequestStatus | '全部状态'>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<MaterialRequest | null>(null);
const draft = reactive<MaterialRequestDraft>({ reason: '', itemName: '', quantity: '' });
const statusOptions: readonly UiSelectOption[] = ['全部状态', '待审批', '已通过', '已驳回', '已领取'].map((value) => ({
  value,
  label: value,
}));
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return requests.value.filter(
    (item) =>
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle ||
        `${item.code} ${item.applicant} ${item.items.map((line) => line.name).join(' ')}`
          .toLocaleLowerCase('zh-CN')
          .includes(needle)),
  );
});
const historyRows = computed(() => requests.value.filter((item) => item.status === '已领取'));
function openCreate() {
  Object.assign(draft, { reason: '', itemName: '', quantity: '' });
  formOpen.value = true;
}
function saveRequest() {
  if (!draft.reason.trim() || !draft.itemName.trim() || !draft.quantity.trim())
    return toast.warning('请填写申请事由和物品信息');
  requests.value.unshift({
    id: `mr-local-${requests.value.length + 1}`,
    code: `MR-2026-${String(requests.value.length + 1).padStart(4, '0')}`,
    applicant: '张经理',
    department: '行政部',
    date: '2026-09-17',
    reason: draft.reason.trim(),
    items: [{ name: draft.itemName.trim(), quantity: draft.quantity.trim() }],
    status: '待审批',
  });
  formOpen.value = false;
  view.value = 'requests';
  toast.success('物料领取申请已提交到本地');
}
function openDetail(item: MaterialRequest) {
  selected.value = item;
  detailOpen.value = true;
}
function processRequest(item: MaterialRequest) {
  const next: MaterialRequestStatus = item.status === '待审批' ? '已通过' : '已领取';
  requests.value = requests.value.map((record) => (record.id === item.id ? { ...record, status: next } : record));
  toast.success(next === '已通过' ? '申请已在本地通过' : '物料已确认领取');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/admin/material-pickup"
    data-page-label="物料领取"
    data-page-module="行政管理"
    data-material-pickup-page
    :data-view="view"
    :data-result-count="filtered.length"
    :data-total-count="requests.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 物料领取</nav>
        <section>
          <span
            ><Box
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>物料领取</h1>
            <p>办公物料申请、审批、领取与库存管理</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i /><i /><i /><i /><b>NOTE</b><em />
      </div>
    </header>
    <section :class="$style.metrics">
      <article><span>待审批</span><strong>1</strong></article>
      <article><span>待领取</span><strong>1</strong></article>
      <article><span>物品种类</span><strong>10</strong></article>
      <article><span>库存预警</span><strong>1</strong></article>
      <article><span>库存总值</span><strong>¥7,438</strong></article>
    </section>
    <header :class="$style.navigation">
      <UiTabs v-model="view"
        ><UiTabsList
          label="物料领取业务区"
          :class="$style.tabs"
          ><UiTabsTrigger
            value="requests"
            :class="$style.tab"
            ><ClipboardList
              :size="14"
              :stroke-width="1.8"
            />领取申请 <em>{{ requests.filter((item) => item.status === '待审批').length }}</em></UiTabsTrigger
          ><UiTabsTrigger
            value="stock"
            :class="$style.tab"
            ><Archive
              :size="14"
              :stroke-width="1.8"
            />物品库 <em>{{ materialStocks.length }}</em></UiTabsTrigger
          ><UiTabsTrigger
            value="history"
            :class="$style.tab"
            ><History
              :size="14"
              :stroke-width="1.8"
            />领取记录 <em>{{ historyRows.length }}</em></UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      ><UiButton
        variant="primary"
        size="sm"
        @click="openCreate"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >提交申请</UiButton
      >
    </header>
    <section
      v-if="view === 'requests'"
      :class="$style.content"
    >
      <header :class="$style.toolbar">
        <label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索物料申请"
            placeholder="搜索申请单号、申请人、物品..." /></label
        ><UiSelect
          v-model="status"
          label="申请状态"
          :options="statusOptions"
          :class="$style.filter"
        />
      </header>
      <MaterialRequestCards
        :rows="filtered"
        @view="openDetail"
        @process="processRequest"
      />
    </section>
    <section
      v-else-if="view === 'stock'"
      :class="$style.stockGrid"
    >
      <article
        v-for="item in materialStocks"
        :key="item.id"
        :data-warning="item.stock <= item.warningAt"
      >
        <Box
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>库存 {{ item.stock }} {{ item.unit }} · 预警 {{ item.warningAt }} {{ item.unit }}</p>
        </div>
        <b>¥{{ item.price }}</b>
      </article>
    </section>
    <section
      v-else
      :class="$style.historyList"
    >
      <article
        v-for="item in historyRows"
        :key="item.id"
      >
        <PackageCheck
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.code }} · {{ item.applicant }}</strong>
          <p>{{ item.items.map((line) => `${line.name} ×${line.quantity}`).join('，') }}</p>
        </div>
        <span>{{ item.date }}</span>
      </article>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="提交领取申请"
    description="申请只写入本地复刻数据"
    size="md"
    ><div :class="$style.form">
      <UiFormField
        label="申请事由"
        required
        ><UiInput v-model="draft.reason" /></UiFormField
      ><UiFormField
        label="物品名称"
        required
        ><UiInput v-model="draft.itemName" /></UiFormField
      ><UiFormField
        label="领取数量"
        required
        ><UiInput v-model="draft.quantity"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveRequest"
        >提交申请</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.code || '申请详情'"
    :description="selected ? `${selected.applicant} · ${selected.department}` : ''"
    size="md"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <p>
        <span>申请日期</span><strong>{{ selected.date }}</strong>
      </p>
      <p>
        <span>申请事由</span><strong>{{ selected.reason }}</strong>
      </p>
      <p>
        <span>当前状态</span><strong>{{ selected.status }}</strong>
      </p>
      <ul>
        <li
          v-for="line in selected.items"
          :key="line.name"
        >
          {{ line.name }} × {{ line.quantity }}
        </li>
      </ul>
      <p v-if="selected.comment">
        <span>审批意见</span><strong>{{ selected.comment }}</strong>
      </p>
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
  @apply relative mx-6 mb-4 mt-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #145c43, #3e8057 55%, #0b8c58);
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
  border-color: rgba(255, 255, 255, 0.26);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[74%];
}
.heroArt i {
  @apply absolute bottom-0 h-9 w-12 border;
  border-color: #73512b;
  background: #c79b42;
}
.heroArt i:nth-child(1) {
  left: 5%;
  height: 74px;
}
.heroArt i:nth-child(2) {
  left: 14%;
  height: 105px;
}
.heroArt i:nth-child(3) {
  left: 23%;
  height: 52px;
}
.heroArt i:nth-child(4) {
  left: 42%;
  width: 110px;
  background: #b56b28;
}
.heroArt b {
  @apply absolute bottom-[-8px] right-[18%] flex h-16 w-20 items-center justify-center rounded-t text-[10px];
  color: #0d3f77;
  background: #e5edf7;
  box-shadow: 0 0 0 8px #0756aa;
}
.heroArt em {
  @apply absolute right-[6%] top-6 h-12 w-10 rotate-6;
  background: #6dd5e7;
}
.metrics {
  @apply mx-6 mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply min-h-[90px] rounded border p-4;
  border-color: var(--ui-border);
  background: #fffbea;
}
.metrics article:nth-child(2) {
  background: #e3fcef;
}
.metrics article:nth-child(3) {
  background: #deebff;
}
.metrics article:nth-child(4) {
  background: #ffebe6;
}
.metrics article:nth-child(5) {
  background: var(--ui-surface-hover);
}
.metrics span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[20px];
  color: #ff8b00;
}
.metrics article:nth-child(2) strong {
  color: #00875a;
}
.metrics article:nth-child(3) strong {
  color: #0052cc;
}
.metrics article:nth-child(4) strong {
  color: #de350b;
}
.metrics article:nth-child(5) strong {
  color: var(--ui-text);
}
.navigation {
  @apply flex h-14 items-center border-b border-t bg-white px-8;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-3;
}
.tab {
  @apply rounded-none border-b-2 border-transparent px-2;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}
.tab em {
  @apply rounded-full px-2 py-0.5 text-[8px] not-italic text-white;
  background: #6b778c;
}
.tab[data-state='active'] em {
  background: var(--app-primary);
}
.navigation > button {
  @apply ml-auto;
}
.content {
  @apply mx-6 pt-5;
}
.toolbar {
  @apply mb-4 flex h-10 items-center gap-3;
}
.search {
  @apply flex w-[400px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: white;
}
.search input {
  @apply border-0 px-0;
  box-shadow: none;
}
.filter {
  @apply w-[120px];
}
.stockGrid {
  @apply mx-6 grid grid-cols-2 gap-3 pt-5;
}
.stockGrid article,
.historyList article {
  @apply flex min-h-[72px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.stockGrid article > svg {
  color: #0052cc;
}
.stockGrid article[data-warning='true'] {
  border-color: #ffbdad;
  background: #fff7f5;
}
.stockGrid article[data-warning='true'] > svg {
  color: #de350b;
}
.stockGrid article div,
.historyList article div {
  @apply min-w-0 flex-1;
}
.stockGrid strong,
.historyList strong {
  @apply text-[11px];
}
.stockGrid p,
.historyList p {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.stockGrid b {
  @apply text-[11px];
  color: #00875a;
}
.historyList {
  @apply mx-6 space-y-3 pt-5;
}
.historyList article > svg {
  color: #00875a;
}
.historyList article > span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.form {
  @apply space-y-4 p-5;
}
.detail {
  @apply p-5;
}
.detail > p {
  @apply flex border-b py-3;
  border-color: var(--ui-border-subtle);
}
.detail span {
  @apply w-24 text-[10px];
  color: var(--ui-text-subtle);
}
.detail strong {
  @apply text-[11px] font-medium;
}
.detail ul {
  @apply my-3 rounded p-3 text-[10px];
  background: var(--ui-surface-subtle);
}
.detail li + li {
  @apply mt-2;
}
</style>
