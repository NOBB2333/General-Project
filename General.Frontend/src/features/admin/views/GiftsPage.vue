<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  Archive,
  ChartNoAxesColumnIncreasing,
  CheckCircle2,
  Download,
  Eye,
  Gift,
  List,
  Plus,
  Search,
  Truck,
  UserRound,
  Users,
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
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { giftSectionRecords, giftsReferenceFixture } from '../model/gifts.data';
import type {
  GiftApplication,
  GiftApplicationDraft,
  GiftApplicationScope,
  GiftApplicationStatus,
  GiftCategory,
} from '../model/gifts.types';
import GiftApplicationsTable from './gifts/GiftApplicationsTable.vue';
import GiftSectionPanel from './gifts/GiftSectionPanel.vue';

const applications = ref<GiftApplication[]>(cloneReferenceRows(giftsReferenceFixture.rows));
const activeSection = ref('applications');
const activeScope = ref<GiftApplicationScope>('mine');
const activeStatus = ref<GiftApplicationStatus | '全部'>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<GiftApplication | null>(null);
const draft = reactive<GiftApplicationDraft>({
  title: '',
  project: '',
  category: '商务礼品',
  recipient: '',
  amount: '',
});

const sectionTabs = [
  { value: 'applications', label: '礼品申请', count: 7, icon: Gift },
  { value: 'catalog', label: '礼品清单', count: 12, icon: List },
  { value: 'inventory', label: '礼品库管理', count: 12, icon: Archive },
  { value: 'flows', label: '礼品库流水', count: 17, icon: ChartNoAxesColumnIncreasing },
];
const statusTabs: readonly (GiftApplicationStatus | '全部')[] = [
  '全部',
  '待审核',
  '审核中',
  '已通过',
  '已驳回',
  '已发放',
];
const categoryOptions: readonly UiSelectOption[] = (
  ['商务礼品', '节日礼品', '员工福利', '活动礼品'] satisfies GiftCategory[]
).map((value) => ({ value, label: value }));

const scopedApplications = computed(() => applications.value.filter((item) => item.scope === activeScope.value));
const filteredApplications = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return scopedApplications.value.filter(
    (item) =>
      (activeStatus.value === '全部' || item.status === activeStatus.value) &&
      (!needle ||
        `${item.code} ${item.title} ${item.project} ${item.recipient}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
const scopedAmount = computed(() => scopedApplications.value.reduce((total, item) => total + item.amount, 0));
const pendingCount = computed(
  () => scopedApplications.value.filter((item) => item.status === '待审核' || item.status === '审核中').length,
);
const approvedAmount = computed(() =>
  scopedApplications.value
    .filter((item) => item.status === '已通过' || item.status === '已发放')
    .reduce((total, item) => total + item.amount, 0),
);
const deliveredCount = computed(() => scopedApplications.value.filter((item) => item.status === '已发放').length);
const currency = new Intl.NumberFormat('zh-CN', { maximumFractionDigits: 2 });

function openCreate() {
  Object.assign(draft, { title: '', project: '', category: '商务礼品', recipient: '', amount: '' });
  formOpen.value = true;
}

function createApplication() {
  const amount = Number(draft.amount);
  if (!draft.title.trim() || !draft.recipient.trim() || !Number.isFinite(amount) || amount <= 0) {
    toast.warning('请填写申请标题、赠送对象和有效金额');
    return;
  }
  const next = applications.value.length + 1;
  applications.value.unshift({
    id: `gift-local-${next}`,
    code: `GFT-LOCAL-${String(next).padStart(3, '0')}`,
    title: draft.title.trim(),
    project: draft.project.trim() || '临时礼品申请',
    category: draft.category,
    recipientType: '客户',
    recipient: draft.recipient.trim(),
    amount,
    appliedAt: '2026-09-16',
    status: '待审核',
    approvalStep: 1,
    scope: 'mine',
  });
  activeSection.value = 'applications';
  activeScope.value = 'mine';
  activeStatus.value = '全部';
  formOpen.value = false;
  toast.success('礼品申请已在本地提交');
}

function openDetail(application: GiftApplication) {
  selected.value = application;
  detailOpen.value = true;
}

function exportApplications() {
  const lines = [
    ['编号', '申请标题', '类别', '赠送对象', '金额', '申请日期', '状态'],
    ...filteredApplications.value.map((item) => [
      item.code,
      item.title,
      item.category,
      item.recipient,
      String(item.amount),
      item.appliedAt,
      item.status,
    ]),
  ];
  const blob = new Blob(
    [lines.map((line) => line.map((value) => `"${value.replaceAll('"', '""')}"`).join(',')).join('\n')],
    {
      type: 'text/csv;charset=utf-8',
    },
  );
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = 'gift-applications.csv';
  anchor.click();
  URL.revokeObjectURL(url);
  toast.success('礼品申请已导出');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/gifts"
    data-page-label="礼品管理"
    data-page-module="行政管理"
    data-gifts-page
    :data-active-section="activeSection"
    :data-active-scope="activeScope"
    :data-result-count="filteredApplications.length"
    :data-total-count="applications.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>礼品管理</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Gift
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>礼品管理</h1>
            <p>员工礼品申请、审批、库存与发放，覆盖商务赠礼与节日福利</p>
          </div>
        </div>
      </div>
      <Gift
        :class="$style.heroArt"
        :size="118"
        :stroke-width="1.5"
      />
    </header>

    <UiTabs v-model="activeSection">
      <div :class="$style.sectionBar">
        <UiTabsList
          label="礼品管理业务区"
          :class="$style.sectionTabs"
        >
          <UiTabsTrigger
            v-for="tab in sectionTabs"
            :key="tab.value"
            :class="$style.sectionTab"
            :value="tab.value"
          >
            <component
              :is="tab.icon"
              :size="14"
              :stroke-width="1.8"
            />{{ tab.label }} <em>{{ tab.count }}</em>
          </UiTabsTrigger>
        </UiTabsList>
        <UiButton
          variant="secondary"
          size="sm"
          @click="exportApplications"
        >
          <template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          @click="openCreate"
        >
          <template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建申请
        </UiButton>
      </div>

      <template v-if="activeSection === 'applications'">
        <UiTabs v-model="activeScope">
          <UiTabsList
            label="礼品申请范围"
            :class="$style.scopeTabs"
          >
            <UiTabsTrigger
              value="mine"
              :class="$style.scopeTab"
            >
              <UserRound
                :size="14"
                :stroke-width="1.8"
              />我的申请 <em>{{ applications.filter((item) => item.scope === 'mine').length }}</em>
            </UiTabsTrigger>
            <UiTabsTrigger
              value="team"
              :class="$style.scopeTab"
            >
              <Users
                :size="14"
                :stroke-width="1.8"
              />团队申请 <em>{{ applications.filter((item) => item.scope === 'team').length }}</em>
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <section :class="$style.metrics">
          <article>
            <Gift
              :size="17"
              :stroke-width="1.8"
            />
            <div>
              <span>申请总额</span><strong>¥{{ currency.format(scopedAmount / 10000) }}万</strong
              ><small>共 {{ scopedApplications.length }} 笔</small>
            </div>
          </article>
          <article :class="$style.metricPending">
            <ChartNoAxesColumnIncreasing
              :size="17"
              :stroke-width="1.8"
            />
            <div>
              <span>待处理审批</span><strong>{{ pendingCount }}</strong
              ><small>需及时审核</small>
            </div>
          </article>
          <article :class="$style.metricApproved">
            <CheckCircle2
              :size="17"
              :stroke-width="1.8"
            />
            <div>
              <span>已批准金额</span><strong>¥{{ currency.format(approvedAmount / 10000) }}万</strong
              ><small>含已发放</small>
            </div>
          </article>
          <article>
            <Truck
              :size="17"
              :stroke-width="1.8"
            />
            <div>
              <span>已发放</span><strong>{{ deliveredCount }}</strong
              ><small>已完成发放</small>
            </div>
          </article>
        </section>

        <section :class="$style.tablePanel">
          <header>
            <UiTabs v-model="activeStatus">
              <UiTabsList
                label="礼品申请状态"
                :class="$style.statusTabs"
              >
                <UiTabsTrigger
                  v-for="item in statusTabs"
                  :key="item"
                  :class="$style.statusTab"
                  :value="item"
                >
                  {{ item
                  }}<small v-if="item !== '全部'"
                    >({{ scopedApplications.filter((row) => row.status === item).length }})</small
                  >
                </UiTabsTrigger>
              </UiTabsList>
            </UiTabs>
            <label :class="$style.search"
              ><Search
                :size="15"
                :stroke-width="1.8" /><UiInput
                v-model="query"
                aria-label="搜索礼品申请"
                placeholder="搜索编号/标题/申请人..."
            /></label>
          </header>
          <GiftApplicationsTable
            :rows="filteredApplications"
            @view="openDetail"
          />
          <footer>
            共 {{ filteredApplications.length }} 条 · 合计 ¥{{
              currency.format(filteredApplications.reduce((sum, item) => sum + item.amount, 0))
            }}
          </footer>
        </section>
      </template>

      <GiftSectionPanel
        v-else-if="activeSection === 'catalog'"
        title="礼品清单"
        :rows="giftSectionRecords.catalog"
      />
      <GiftSectionPanel
        v-else-if="activeSection === 'inventory'"
        title="礼品库管理"
        :rows="giftSectionRecords.inventory"
      />
      <GiftSectionPanel
        v-else
        title="礼品库流水"
        :rows="giftSectionRecords.flows"
      />
    </UiTabs>
  </main>

  <UiDialog
    v-model:open="formOpen"
    title="新建礼品申请"
    description="提交后进入本地审批队列"
    size="lg"
  >
    <div :class="$style.form">
      <UiFormField
        label="申请标题"
        required
        :class="$style.formWide"
        ><UiInput
          v-model="draft.title"
          aria-label="礼品申请标题"
      /></UiFormField>
      <UiFormField label="关联项目"
        ><UiInput
          v-model="draft.project"
          aria-label="礼品关联项目"
      /></UiFormField>
      <UiFormField label="礼品类别"
        ><UiSelect
          v-model="draft.category"
          label="礼品类别"
          :options="categoryOptions"
      /></UiFormField>
      <UiFormField
        label="赠送对象"
        required
        ><UiInput
          v-model="draft.recipient"
          aria-label="礼品赠送对象"
      /></UiFormField>
      <UiFormField
        label="申请金额"
        required
        ><UiInput
          v-model="draft.amount"
          aria-label="礼品申请金额"
          inputmode="decimal"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="createApplication"
        >提交申请</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.title || '礼品申请详情'"
    :description="selected?.code"
    size="lg"
  >
    <div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>关联项目</dt>
          <dd>{{ selected.project }}</dd>
        </div>
        <div>
          <dt>礼品类别</dt>
          <dd>{{ selected.category }}</dd>
        </div>
        <div>
          <dt>赠送对象</dt>
          <dd>{{ selected.recipient }}</dd>
        </div>
        <div>
          <dt>申请金额</dt>
          <dd>¥{{ currency.format(selected.amount) }}</dd>
        </div>
        <div>
          <dt>申请日期</dt>
          <dd>{{ selected.appliedAt }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
      </dl>
    </div>
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
  background: linear-gradient(105deg, #9e1b55 0%, #c72c66 48%, #ff8b00 100%);
}
.heroCopy {
  @apply relative z-[1];
}
.hero nav {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 80%);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}
.heroArt {
  @apply absolute right-[170px] top-[20px] opacity-60;
  color: white;
}
.sectionBar {
  @apply mb-3 flex items-center border-b;
  border-color: var(--ui-border);
}
.sectionTabs {
  @apply flex-1 gap-0;
}
.sectionTab {
  @apply h-12 rounded-none border-b-2 border-transparent px-4;
  background: transparent;
}
.sectionTab[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}
.sectionTab em,
.scopeTab em {
  @apply rounded-full px-1.5 py-0.5 text-[9px] not-italic;
  background: var(--ui-surface-hover);
}
.sectionBar > button {
  @apply ml-2;
}
.scopeTabs {
  @apply mb-3 gap-1;
}
.scopeTab {
  @apply min-w-[130px] border;
  border-color: var(--ui-border);
}
.scopeTab[data-state='active'] {
  @apply text-white;
  background: var(--app-primary);
}
.scopeTab[data-state='active'] em {
  background: rgb(255 255 255 / 20%);
}
.metrics {
  @apply mb-3 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[96px] items-start gap-2 rounded border bg-white px-4 py-4;
  color: var(--app-primary);
  border-color: var(--ui-border);
}
.metrics span {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1 block text-[22px];
}
.metrics small {
  @apply mt-1 block text-[10px] font-normal;
  color: var(--ui-text-subtle);
}
.metricPending {
  color: var(--ui-warning) !important;
}
.metricApproved {
  color: var(--ui-success) !important;
}
.tablePanel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.tablePanel > header {
  @apply flex h-12 items-center border-b px-2;
  border-color: var(--ui-border);
}
.statusTabs {
  @apply gap-0;
}
.statusTab {
  @apply rounded-none border-b-2 border-transparent;
  background: transparent;
}
.statusTab[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}
.statusTab small {
  @apply ml-1 text-[9px];
  color: var(--ui-text-subtle);
}
.search {
  @apply ml-auto flex h-8 w-[280px] items-center gap-2 rounded bg-[#f4f5f7] px-3;
  color: var(--ui-text-subtle);
}
.search :global(input) {
  @apply min-w-0 flex-1 border-0 bg-transparent p-0 outline-none;
}
.tablePanel > footer {
  @apply px-4 py-3 text-[11px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formWide {
  @apply col-span-2;
}
.detail {
  @apply p-5;
}
.detail dl {
  @apply grid grid-cols-2 gap-x-6 gap-y-4;
}
.detail dl div {
  @apply border-b pb-2;
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[12px] font-medium;
}
</style>
