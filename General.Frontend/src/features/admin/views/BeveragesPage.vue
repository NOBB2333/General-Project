<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  AlertTriangle,
  Boxes,
  ChartNoAxesColumnIncreasing,
  CheckCircle2,
  ClipboardList,
  Download,
  Plus,
  Search,
  TrendingDown,
  Wine,
} from 'lucide-vue-next';
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
  UiTextarea,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { beverageRequestsFixture, beverageStocksFixture } from '../model/beverages.data';
import type {
  BeverageCategory,
  BeverageRequest,
  BeverageRequestDraft,
  BeverageRequestStatus,
} from '../model/beverages.types';
import BeverageAnalytics from './beverages/BeverageAnalytics.vue';
import BeverageLedgerTable from './beverages/BeverageLedgerTable.vue';
import BeverageRequestList from './beverages/BeverageRequestList.vue';

type BeverageView = 'ledger' | 'requests';

const stocks = cloneReferenceRows(beverageStocksFixture.rows);
const requests = ref<BeverageRequest[]>(cloneReferenceRows(beverageRequestsFixture.rows));
const activeView = ref<BeverageView>('ledger');
const category = ref<BeverageCategory | '全部'>('全部');
const requestStatus = ref<BeverageRequestStatus | '全部'>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const selectedRequest = ref<BeverageRequest | null>(null);
const draft = reactive<BeverageRequestDraft>({
  category: '白酒',
  itemName: '',
  quantity: '',
  purpose: '',
  project: '',
});

const categoryOptions: readonly UiSelectOption[] = ['全部', '白酒', '红酒', '啤酒', '饮料', '茶叶', '其他'].map(
  (value) => ({ value, label: value }),
);
const requestCategoryOptions: readonly UiSelectOption[] = categoryOptions.slice(1);
const requestStatuses: readonly (BeverageRequestStatus | '全部')[] = ['全部', '待审核', '已通过', '已驳回', '已领取'];
const currency = new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY', maximumFractionDigits: 0 });

const filteredStocks = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return stocks.filter(
    (item) =>
      (category.value === '全部' || item.category === category.value) &&
      (!needle || `${item.code} ${item.name} ${item.specification}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
const filteredRequests = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return requests.value.filter(
    (item) =>
      (requestStatus.value === '全部' || item.status === requestStatus.value) &&
      (category.value === '全部' || item.categories.includes(category.value)) &&
      (!needle ||
        `${item.code} ${item.summary} ${item.applicant} ${item.purpose}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
const currentCount = computed(() =>
  activeView.value === 'ledger' ? filteredStocks.value.length : filteredRequests.value.length,
);

function openRequest() {
  Object.assign(draft, { category: '白酒', itemName: '', quantity: '', purpose: '', project: '' });
  formOpen.value = true;
}

function submitRequest() {
  if (!draft.itemName.trim() || !draft.quantity.trim() || !draft.purpose.trim()) {
    toast.warning('请填写酒水名称、数量和申请用途');
    return;
  }
  const next = requests.value.length + 1;
  requests.value.unshift({
    id: `brq-local-${next}`,
    code: `BRQ-LOCAL-${String(next).padStart(3, '0')}`,
    categories: [draft.category],
    summary: draft.itemName.trim(),
    amount: 0,
    status: '待审核',
    applicant: '张经理',
    department: '销售部',
    appliedAt: '2026-09-16',
    purpose: draft.purpose.trim(),
    project: draft.project.trim() || undefined,
    itemSummary: `1种 · ${draft.quantity.trim()}`,
  });
  activeView.value = 'requests';
  requestStatus.value = '全部';
  category.value = '全部';
  query.value = '';
  formOpen.value = false;
  toast.success('酒水申请已在本地提交');
}

function openDetail(request: BeverageRequest) {
  selectedRequest.value = request;
  detailOpen.value = true;
}

function exportCurrent() {
  const rows =
    activeView.value === 'ledger'
      ? filteredStocks.value.map((item) => [
          item.code,
          item.name,
          item.category,
          item.specification,
          item.stock,
          item.unitPrice,
          item.stockValue,
          item.location,
          item.status,
        ])
      : filteredRequests.value.map((item) => [
          item.code,
          item.summary,
          item.categories.join('/'),
          item.amount,
          item.status,
          item.applicant,
          item.appliedAt,
          item.purpose,
        ]);
  const headers =
    activeView.value === 'ledger'
      ? ['编号', '名称', '类别', '规格', '库存', '单价', '库存价值', '存放位置', '状态']
      : ['编号', '申请内容', '类别', '金额', '状态', '申请人', '日期', '用途'];
  const csv = [headers, ...rows]
    .map((line) => line.map((value) => `"${String(value).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = activeView.value === 'ledger' ? 'beverage-ledger.csv' : 'beverage-requests.csv';
  anchor.click();
  URL.revokeObjectURL(url);
  toast.success('酒水数据已导出');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/beverages"
    data-page-label="酒水管理"
    data-page-module="行政管理"
    data-beverages-page
    :data-active-view="activeView"
    :data-category="category"
    :data-request-status="requestStatus"
    :data-result-count="currentCount"
    :data-request-count="requests.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>酒水管理</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Wine
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>酒水管理</h1>
            <p>酒水库存盘点、申请审批、领用记录与采购计划全流程管控</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.bottleShelf"
        aria-hidden="true"
      >
        <i
          v-for="item in 9"
          :key="item"
        />
      </div>
    </header>

    <section :class="$style.metrics">
      <article>
        <Wine
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>库存总值</span><strong>¥6.55万</strong><small>12 种 · 139 件</small></div>
      </article>
      <article :class="$style.metricUsage">
        <TrendingDown
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>本月领用</span><strong>¥0.58万</strong><small>3 笔</small></div>
      </article>
      <article :class="$style.metricPending">
        <AlertTriangle
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>待审核</span><strong>2</strong><small>需及时处理</small></div>
      </article>
      <article :class="$style.metricSafe">
        <CheckCircle2
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>库存预警</span><strong>0</strong><small>库存充足</small></div>
      </article>
      <article :class="$style.metricCategory">
        <ChartNoAxesColumnIncreasing
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>品类数</span><strong>5</strong><small>12 种商品</small></div>
      </article>
    </section>

    <BeverageAnalytics />

    <section :class="$style.workspace">
      <header :class="$style.workspaceHeader">
        <UiTabs v-model="activeView">
          <UiTabsList
            label="酒水业务视图"
            :class="$style.viewTabs"
          >
            <UiTabsTrigger
              value="ledger"
              :class="$style.viewTab"
              ><Boxes
                :size="14"
                :stroke-width="1.8"
              />库存台账 <em>{{ stocks.length }}</em></UiTabsTrigger
            >
            <UiTabsTrigger
              value="requests"
              :class="$style.viewTab"
              ><ClipboardList
                :size="14"
                :stroke-width="1.8"
              />申请记录 <em>{{ requests.length }}</em></UiTabsTrigger
            >
          </UiTabsList>
        </UiTabs>
        <UiButton
          variant="secondary"
          size="sm"
          @click="exportCurrent"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="openRequest"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >申请酒水</UiButton
        >
      </header>

      <div :class="$style.filters">
        <UiSelect
          v-model="category"
          label="酒水类别"
          :options="categoryOptions"
          :class="$style.categorySelect"
        />
        <UiTabs
          v-if="activeView === 'requests'"
          v-model="requestStatus"
        >
          <UiTabsList
            label="申请状态"
            :class="$style.statusTabs"
          >
            <UiTabsTrigger
              v-for="status in requestStatuses"
              :key="status"
              :value="status"
              :class="$style.statusTab"
            >
              {{ status
              }}<small v-if="status !== '全部'">({{ requests.filter((item) => item.status === status).length }})</small>
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
        <label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索酒水"
            placeholder="搜索酒水名称..."
        /></label>
      </div>

      <BeverageLedgerTable
        v-if="activeView === 'ledger'"
        :rows="filteredStocks"
      />
      <BeverageRequestList
        v-else
        :rows="filteredRequests"
        @view="openDetail"
      />
      <footer :class="$style.workspaceFooter">
        <template v-if="activeView === 'ledger'"
          >共 {{ filteredStocks.length }} 种 · 库存价值
          {{ currency.format(filteredStocks.reduce((sum, item) => sum + item.stockValue, 0)) }}</template
        >
        <template v-else
          >共 {{ filteredRequests.length }} 条 ·
          {{ currency.format(filteredRequests.reduce((sum, item) => sum + item.amount, 0)) }}</template
        >
      </footer>
    </section>
  </main>

  <UiDialog
    v-model:open="formOpen"
    title="申请酒水"
    description="申请提交后进入本地行政审批队列"
    size="lg"
  >
    <div :class="$style.form">
      <UiFormField
        label="酒水类别"
        required
        ><UiSelect
          v-model="draft.category"
          label="申请酒水类别"
          :options="requestCategoryOptions"
      /></UiFormField>
      <UiFormField
        label="酒水名称"
        required
        ><UiInput
          v-model="draft.itemName"
          aria-label="申请酒水名称"
      /></UiFormField>
      <UiFormField
        label="申请数量"
        required
        ><UiInput
          v-model="draft.quantity"
          aria-label="申请酒水数量"
          placeholder="例如：2瓶"
      /></UiFormField>
      <UiFormField label="关联项目"
        ><UiInput
          v-model="draft.project"
          aria-label="酒水关联项目"
      /></UiFormField>
      <UiFormField
        label="申请用途"
        required
        :class="$style.formWide"
        ><UiTextarea
          v-model="draft.purpose"
          aria-label="酒水申请用途"
          :rows="3"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="submitRequest"
        >提交申请</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="detailOpen"
    :title="selectedRequest?.summary || '酒水申请详情'"
    :description="selectedRequest?.code"
    size="lg"
  >
    <dl
      v-if="selectedRequest"
      :class="$style.detail"
    >
      <div>
        <dt>申请人</dt>
        <dd>{{ selectedRequest.applicant }} · {{ selectedRequest.department }}</dd>
      </div>
      <div>
        <dt>申请日期</dt>
        <dd>{{ selectedRequest.appliedAt }}</dd>
      </div>
      <div>
        <dt>申请内容</dt>
        <dd>{{ selectedRequest.summary }}（{{ selectedRequest.itemSummary }}）</dd>
      </div>
      <div>
        <dt>状态</dt>
        <dd>{{ selectedRequest.status }}</dd>
      </div>
      <div>
        <dt>用途</dt>
        <dd>{{ selectedRequest.purpose }}</dd>
      </div>
      <div>
        <dt>关联项目</dt>
        <dd>{{ selectedRequest.project || '未关联项目' }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-3 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(105deg, #4f071f 0%, #77263b 52%, #4a0b25 100%);
}
.hero::after {
  @apply absolute inset-x-0 bottom-8 h-px;
  content: '';
  background: rgba(255, 255, 255, 0.15);
}
.heroCopy {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-4 flex items-center gap-2 text-[10px];
  color: rgba(255, 255, 255, 0.75);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.25);
  background: rgba(255, 255, 255, 0.12);
}
.identity h1 {
  @apply text-[20px] font-semibold;
}
.identity p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.bottleShelf {
  @apply absolute inset-y-0 right-0 w-[58%];
  background: linear-gradient(180deg, rgba(255, 139, 0, 0.28), transparent 70%);
}
.bottleShelf i {
  @apply absolute bottom-9 h-11 w-5 rounded-sm;
  background: #263829;
  box-shadow: inset 0 7px #d5a34f;
}
.bottleShelf i::before {
  @apply absolute -top-4 left-1.5 h-5 w-2 rounded-t-sm;
  content: '';
  background: inherit;
}
.bottleShelf i:nth-child(1) {
  left: 8%;
}
.bottleShelf i:nth-child(2) {
  left: 18%;
  bottom: 12px;
  height: 66px;
  width: 52px;
  border-radius: 15px 15px 2px 2px;
  background: #71371d;
}
.bottleShelf i:nth-child(3) {
  left: 29%;
}
.bottleShelf i:nth-child(4) {
  left: 40%;
  background: #4a261f;
}
.bottleShelf i:nth-child(5) {
  left: 51%;
}
.bottleShelf i:nth-child(6) {
  left: 62%;
  bottom: 14px;
  height: 55px;
  width: 70px;
  border-radius: 28px 28px 2px 2px;
  background: #5b351b;
}
.bottleShelf i:nth-child(7) {
  left: 74%;
}
.bottleShelf i:nth-child(8) {
  left: 84%;
  background: #4b211d;
}
.bottleShelf i:nth-child(9) {
  left: 94%;
}
.metrics {
  @apply mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex min-h-[94px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
  color: #ff5630;
}
.metrics span,
.metrics small {
  @apply block;
  color: var(--ui-text-subtle);
}
.metrics span {
  @apply text-[10px];
}
.metrics strong {
  @apply my-1 block text-[20px] font-semibold;
  color: var(--ui-text);
}
.metrics small {
  @apply text-[9px];
}
.metricUsage {
  color: #00875a !important;
}
.metricUsage strong,
.metricSafe strong {
  color: #00875a !important;
}
.metricPending {
  color: #ff8b00 !important;
}
.metricPending strong {
  color: #ff8b00 !important;
}
.metricSafe {
  color: #00875a !important;
}
.metricCategory {
  color: #6554c0 !important;
}
.metricCategory strong {
  color: #6554c0 !important;
}
.workspace {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.workspaceHeader {
  @apply flex h-14 items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.viewTabs {
  @apply mr-auto gap-4;
}
.viewTab {
  @apply rounded-none border-b-2 border-transparent px-1;
}
.viewTab[data-state='active'] {
  border-color: var(--app-primary);
  background: transparent;
}
.viewTab em {
  @apply rounded-full px-2 py-0.5 text-[9px] not-italic;
  background: var(--ui-surface-hover);
}
.filters {
  @apply flex min-h-14 items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.categorySelect {
  @apply w-[110px];
}
.statusTabs {
  @apply gap-0;
}
.statusTab {
  @apply min-h-7 px-2 text-[10px];
}
.statusTab small {
  @apply ml-0.5;
}
.search {
  @apply ml-auto flex w-[230px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.workspaceFooter {
  @apply border-t px-4 py-3 text-[10px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formWide {
  @apply col-span-2;
}
.detail {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detail div {
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
@media (prefers-reduced-motion: reduce) {
  .workspace button {
    transition: none;
  }
}
</style>
