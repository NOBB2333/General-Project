<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { Box, Clock3, Hourglass, Plus, Search, Truck, TriangleAlert } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { borrowableEquipment, equipmentBorrowsFixture } from '../model/equipment-borrow.data';
import type {
  BorrowItemType,
  BorrowStatus,
  EquipmentBorrow,
  EquipmentBorrowDraft,
} from '../model/equipment-borrow.types';
import EquipmentBorrowTable from './equipment-borrow/EquipmentBorrowTable.vue';

type MainView = 'records' | 'library';
type Scope = 'all' | 'mine' | 'pending' | 'using' | 'overdue';
const records = ref<EquipmentBorrow[]>(cloneReferenceRows(equipmentBorrowsFixture.rows));
const mainView = ref<MainView>('records');
const scope = ref<Scope>('all');
const query = ref('');
const type = ref<BorrowItemType | '全部类型'>('全部类型');
const status = ref<BorrowStatus | '全部状态'>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<EquipmentBorrow | null>(null);
const draft = reactive<EquipmentBorrowDraft>({
  item: '',
  assetCode: '',
  type: '仪器',
  quantity: '1 台',
  business: '',
  borrowedAt: '2026-09-17',
  dueAt: '2026-09-24',
});
const typeOptions: readonly UiSelectOption[] = ['全部类型', '仪器', '设备'].map((value) => ({ value, label: value }));
const createTypeOptions = typeOptions.slice(1);
const statusOptions: readonly UiSelectOption[] = [
  '全部状态',
  '待审批',
  '审批中',
  '使用中',
  '逾期未还',
  '已归还',
  '损坏报废',
].map((value) => ({ value, label: value }));
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return records.value.filter(
    (item) =>
      (scope.value === 'all' ||
        (scope.value === 'mine' && item.applicant === '张经理') ||
        (scope.value === 'pending' && ['待审批', '审批中'].includes(item.status)) ||
        (scope.value === 'using' && item.status === '使用中') ||
        (scope.value === 'overdue' && item.status === '逾期未还')) &&
      (type.value === '全部类型' || item.type === type.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle ||
        `${item.code} ${item.item} ${item.applicant} ${item.business}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
const scopeItems: readonly { value: Scope; label: string }[] = [
  { value: 'all', label: '全部记录' },
  { value: 'mine', label: '我的申请' },
  { value: 'pending', label: '待审批' },
  { value: 'using', label: '使用中' },
  { value: 'overdue', label: '逾期未还' },
];
function openCreate() {
  Object.assign(draft, {
    item: '',
    assetCode: '',
    type: '仪器',
    quantity: '1 台',
    business: '',
    borrowedAt: '2026-09-17',
    dueAt: '2026-09-24',
  });
  formOpen.value = true;
}
function saveBorrow() {
  if (!draft.item.trim() || !draft.assetCode.trim()) return toast.warning('请填写借用物品和资产编号');
  records.value.unshift({
    id: `eb-local-${records.value.length + 1}`,
    code: `EB-2026-${String(records.value.length + 89).padStart(4, '0')}`,
    applicant: '张经理',
    department: '行政部',
    ...draft,
    status: '待审批',
    approvers: '李 → 张',
  });
  formOpen.value = false;
  mainView.value = 'records';
  scope.value = 'all';
  toast.success('设备借用申请已提交到本地');
}
function openDetail(item: EquipmentBorrow) {
  selected.value = item;
  detailOpen.value = true;
}
function advanceRecord() {
  if (!selected.value) return;
  const next: BorrowStatus = selected.value.status === '待审批' ? '审批中' : '已归还';
  records.value = records.value.map((item) =>
    item.id === selected.value?.id ? { ...item, status: next, overdueHint: undefined } : item,
  );
  selected.value = { ...selected.value, status: next, overdueHint: undefined };
  toast.success(next === '审批中' ? '申请已进入本地审批' : '设备已确认归还');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/equipment-borrow"
    data-page-label="设备借用"
    data-page-module="行政管理"
    data-equipment-borrow-page
    :data-main-view="mainView"
    :data-scope="scope"
    :data-result-count="filtered.length"
    :data-total-count="records.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 设备借用</nav>
        <section>
          <span
            ><Truck
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>设备借用管理</h1>
            <p>设备借出申请、审批、归还跟踪与物品库存维护一体化管理</p>
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
    <header :class="$style.topActions">
      <div
        role="group"
        aria-label="设备借用主视图"
      >
        <button
          :data-active="mainView === 'records'"
          @click="mainView = 'records'"
        >
          借用记录</button
        ><button
          :data-active="mainView === 'library'"
          @click="mainView = 'library'"
        >
          物品库
        </button>
      </div>
      <UiButton
        variant="primary"
        size="sm"
        @click="openCreate"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >申请借用</UiButton
      >
    </header>
    <section :class="$style.metrics">
      <article>
        <span
          ><Box
            :size="18"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>借用总数</p>
          <strong>11</strong>
        </div>
      </article>
      <article>
        <span
          ><Clock3
            :size="18"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>使用中</p>
          <strong>1</strong>
        </div>
      </article>
      <article>
        <span
          ><TriangleAlert
            :size="18"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>逾期未还</p>
          <strong>1</strong>
        </div>
      </article>
      <article>
        <span
          ><Hourglass
            :size="18"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>待审批</p>
          <strong>2</strong>
        </div>
      </article>
    </section>
    <section
      v-if="mainView === 'records'"
      :class="$style.records"
    >
      <nav
        :class="$style.scopeTabs"
        aria-label="借用记录范围"
      >
        <button
          v-for="item in scopeItems"
          :key="item.value"
          :data-active="scope === item.value"
          @click="scope = item.value"
        >
          {{ item.label }}
        </button>
      </nav>
      <header :class="$style.toolbar">
        <label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索设备借用"
            placeholder="搜索编号、物品名称、申请人、关联业务…" /></label
        ><UiSelect
          v-model="type"
          label="物品类型"
          :options="typeOptions"
          :class="$style.filter"
        /><UiSelect
          v-model="status"
          label="借用状态"
          :options="statusOptions"
          :class="$style.filter"
        />
      </header>
      <EquipmentBorrowTable
        :rows="filtered"
        @view="openDetail"
      />
    </section>
    <section
      v-else
      :class="$style.library"
    >
      <article
        v-for="item in borrowableEquipment"
        :key="item.id"
        :data-available="item.available"
      >
        <Box
          :size="20"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.assetCode }} · {{ item.location }}</p>
        </div>
        <span>{{ item.available ? '可借用' : '借出中' }}</span
        ><UiButton
          size="sm"
          variant="secondary"
          :disabled="!item.available"
          @click="
            Object.assign(draft, { item: item.name, assetCode: item.assetCode, type: item.type });
            formOpen = true;
          "
          >申请借用</UiButton
        >
      </article>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="申请借用"
    description="设备借用申请只写入本地数据"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="借用物品"
        required
        ><UiInput v-model="draft.item" /></UiFormField
      ><UiFormField
        label="资产编号"
        required
        ><UiInput v-model="draft.assetCode" /></UiFormField
      ><UiFormField label="物品类型"
        ><UiSelect
          v-model="draft.type"
          label="借用物品类型"
          :options="createTypeOptions" /></UiFormField
      ><UiFormField label="数量"><UiInput v-model="draft.quantity" /></UiFormField
      ><UiFormField label="关联业务"><UiInput v-model="draft.business" /></UiFormField
      ><UiFormField label="借用日期"><UiInput v-model="draft.borrowedAt" /></UiFormField
      ><UiFormField label="归还日期"><UiInput v-model="draft.dueAt" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveBorrow"
        >提交申请</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.code || '借用详情'"
    :description="selected?.item"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>申请人</dt>
          <dd>{{ selected.applicant }} · {{ selected.department }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
        <div>
          <dt>借用日期</dt>
          <dd>{{ selected.borrowedAt }}</dd>
        </div>
        <div>
          <dt>归还日期</dt>
          <dd>{{ selected.dueAt }}</dd>
        </div>
        <div>
          <dt>关联业务</dt>
          <dd>{{ selected.business }}</dd>
        </div>
        <div>
          <dt>审批进度</dt>
          <dd>{{ selected.approvers }}</dd>
        </div>
      </dl>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ><UiButton
        v-if="selected && ['待审批', '使用中', '逾期未还'].includes(selected.status)"
        variant="primary"
        @click="advanceRecord"
        >{{ selected.status === '待审批' ? '提交审批' : '确认归还' }}</UiButton
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
  background: linear-gradient(110deg, #035467, #257f8f 55%, #008da4);
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
  @apply absolute inset-y-0 right-0 w-[72%];
}
.heroArt i {
  @apply absolute;
}
.heroArt i:first-child {
  @apply bottom-4 left-[28%] h-4 w-32 rounded-full border-[5px];
  border-color: #173c5e;
  background: #6e99b8;
}
.heroArt i:nth-child(2) {
  @apply bottom-14 left-[52%] h-3 w-16;
  background: #d95a43;
}
.heroArt i:nth-child(3) {
  @apply bottom-8 right-[28%] h-3 w-20 rotate-12;
  background: #885d39;
}
.heroArt b {
  @apply absolute right-[12%] top-[-44px] h-28 w-28 rounded-full border-[8px];
  border-color: rgba(10, 73, 95, 0.45);
}
.heroArt em {
  @apply absolute bottom-[-22px] right-[3%] h-12 w-24 -rotate-12 rounded;
  background: #ffd86b;
}
.topActions {
  @apply mb-3 flex h-12 items-center;
}
.topActions > div {
  @apply flex rounded bg-white p-1;
}
.topActions > div button {
  @apply h-8 rounded px-4 text-[11px];
  color: var(--ui-text-muted);
}
.topActions > div button[data-active='true'] {
  @apply border bg-white font-semibold shadow-sm;
  color: var(--app-primary);
  border-color: var(--ui-border);
}
.topActions > button {
  @apply ml-auto;
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[88px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > span {
  @apply flex h-10 w-10 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(2) > span {
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(3) > span {
  color: #de350b;
  background: #ffebe6;
}
.metrics article:nth-child(4) > span {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1 block text-[20px];
}
.records {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.scopeTabs {
  @apply flex h-12 items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.scopeTabs button {
  @apply h-12 border-b-2 border-transparent px-3 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.scopeTabs button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.toolbar {
  @apply flex h-16 items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.search {
  @apply flex w-[330px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.search input {
  @apply border-0 px-0;
  box-shadow: none;
}
.filter {
  @apply w-[110px];
}
.library {
  @apply grid grid-cols-2 gap-3;
}
.library article {
  @apply flex min-h-[82px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.library article > svg {
  color: var(--app-primary);
}
.library article > div {
  @apply min-w-0 flex-1;
}
.library strong {
  @apply text-[11px];
}
.library p {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.library article > span {
  @apply rounded px-2 py-1 text-[8px];
  color: #de350b;
  background: #ffebe6;
}
.library article[data-available='true'] > span {
  color: #00875a;
  background: #e3fcef;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.form > label:nth-child(5) {
  @apply col-span-2;
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
