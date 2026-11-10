<script setup lang="ts">
import { computed, nextTick, reactive, ref } from 'vue';
import {
  AlertTriangle,
  Building2,
  CalendarDays,
  CheckCircle2,
  Hourglass,
  MapPin,
  Plus,
  Search,
  Stamp,
  UserRound,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiAlertDialog,
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
import { sealApplicationsFixture, sealDepartmentStats, sealLedgerFixture, sealTypeStats } from '../model/seals.data';
import type { SealApplication, SealApplicationDraft, SealApplicationStatus, SealType } from '../model/seals.types';
import SealApplicationsTable from './seals/SealApplicationsTable.vue';

type SealView = 'applications' | 'tasks' | 'ledger' | 'stats';

const applications = ref<SealApplication[]>(cloneReferenceRows(sealApplicationsFixture.rows));
const ledger = cloneReferenceRows(sealLedgerFixture.rows);
const activeView = ref<SealView>('applications');
const status = ref<SealApplicationStatus | '全部状态'>('全部状态');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const confirmOpen = ref(false);
const selected = ref<SealApplication | null>(null);
const draft = reactive<SealApplicationDraft>({
  company: '瑞视自动化',
  sealType: '公章',
  fileName: '',
  purpose: '',
  copies: '1',
  urgency: '普通',
});

const statusOptions: readonly UiSelectOption[] = [
  '全部状态',
  '待审批',
  '审批中',
  '已通过',
  '已驳回',
  '已盖章',
  '已归还',
].map((value) => ({ value, label: value }));
const companyOptions: readonly UiSelectOption[] = ['瑞视自动化', '瑞视智能', '南京瑞视'].map((value) => ({
  value,
  label: value,
}));
const sealTypeOptions: readonly UiSelectOption[] = [
  '公章',
  '合同专用章',
  '财务专用章',
  '法人章',
  '发票专用章',
  '部门章',
].map((value) => ({ value, label: value }));
const urgencyOptions: readonly UiSelectOption[] = ['普通', '加急', '特急'].map((value) => ({ value, label: value }));
const filteredApplications = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return applications.value.filter(
    (item) =>
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle ||
        `${item.code} ${item.company} ${item.applicant} ${item.fileName} ${item.purpose}`
          .toLocaleLowerCase('zh-CN')
          .includes(needle)),
  );
});
const stampTasks = computed(() => applications.value.filter((item) => item.status === '已通过'));

function openCreate() {
  Object.assign(draft, {
    company: '瑞视自动化',
    sealType: '公章',
    fileName: '',
    purpose: '',
    copies: '1',
    urgency: '普通',
  });
  formOpen.value = true;
}
async function createApplication() {
  const copies = Number(draft.copies);
  if (!draft.fileName.trim() || !draft.purpose.trim() || !Number.isInteger(copies) || copies < 1) {
    toast.warning('请填写文件名称、用途和有效份数');
    return;
  }
  const next = applications.value.length + 1;
  applications.value.unshift({
    id: `seal-local-${next}`,
    code: `SA-LOCAL-${String(next).padStart(3, '0')}`,
    urgency: draft.urgency === '普通' ? undefined : draft.urgency,
    company: draft.company,
    applicant: '张经理',
    department: '销售部',
    sealType: draft.sealType,
    fileName: draft.fileName.trim(),
    fileType: '申请文件',
    copies,
    purpose: draft.purpose.trim(),
    status: '待审批',
    appliedAt: '2026-09-16',
  });
  status.value = '全部状态';
  query.value = '';
  formOpen.value = false;
  await nextTick();
  activeView.value = 'applications';
  toast.success('用章申请已在本地创建');
}
function openDetail(application: SealApplication) {
  selected.value = application;
  detailOpen.value = true;
}
function requestStamp(application: SealApplication) {
  selected.value = application;
  confirmOpen.value = true;
}
function confirmStamp() {
  if (selected.value) selected.value.status = '已盖章';
  confirmOpen.value = false;
  toast.success('盖章状态已在本地确认');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/seals"
    data-page-label="用章管理"
    data-page-module="行政管理"
    data-seals-page
    :data-active-view="activeView"
    :data-result-count="filteredApplications.length"
    :data-total-count="applications.length"
    :data-task-count="stampTasks.length"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>用章管理</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Stamp
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>用章管理</h1>
            <p>印章登记、申请审批、外带跟踪与风险监控，确保用印合规</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.stampArt"
        aria-hidden="true"
      >
        <i /><b />
      </div>
    </header>

    <section :class="$style.metrics">
      <article>
        <Stamp
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>在用印章</span><strong>7</strong><small>共8枚（含作废）</small></div>
      </article>
      <article :class="$style.metricPending">
        <Hourglass
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>待审批</span><strong>2</strong><small>用章申请</small></div>
      </article>
      <article :class="$style.metricMonth">
        <CalendarDays
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>本月用章</span><strong>8</strong><small>2026年2月</small></div>
      </article>
      <article :class="$style.metricOutside">
        <AlertTriangle
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>外带记录</span><strong>2</strong><small>需跟踪归还</small></div>
      </article>
    </section>

    <section :class="$style.workspace">
      <header :class="$style.toolbar">
        <UiTabs v-model="activeView"
          ><UiTabsList
            label="用章管理业务区"
            :class="$style.tabs"
          >
            <UiTabsTrigger
              value="applications"
              :class="$style.tab"
              >用章申请 <em>({{ applications.length }})</em></UiTabsTrigger
            >
            <UiTabsTrigger
              value="tasks"
              :class="$style.tab"
              >待盖章任务 <em>({{ stampTasks.length }})</em></UiTabsTrigger
            >
            <UiTabsTrigger
              value="ledger"
              :class="$style.tab"
              >印章台账 <em>({{ ledger.length }})</em></UiTabsTrigger
            >
            <UiTabsTrigger
              value="stats"
              :class="$style.tab"
              >用章统计</UiTabsTrigger
            >
          </UiTabsList></UiTabs
        >
        <template v-if="activeView === 'applications'"
          ><UiSelect
            v-model="status"
            label="用章申请状态"
            :options="statusOptions"
            :class="$style.statusSelect" /><label :class="$style.search"
            ><Search
              :size="15"
              :stroke-width="1.8" /><UiInput
              v-model="query"
              aria-label="搜索用章申请"
              placeholder="搜索申请..." /></label
        ></template>
        <UiButton
          variant="primary"
          size="sm"
          @click="openCreate"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建用章申请</UiButton
        >
      </header>

      <SealApplicationsTable
        v-if="activeView === 'applications'"
        :rows="filteredApplications"
        @view="openDetail"
      />
      <div
        v-else-if="activeView === 'tasks'"
        :class="$style.taskList"
      >
        <article
          v-for="task in stampTasks"
          :key="task.id"
        >
          <span :class="$style.taskIcon"
            ><Stamp
              :size="20"
              :stroke-width="1.8"
          /></span>
          <div>
            <strong>{{ task.fileName }}</strong>
            <p>{{ task.sealType }} · {{ task.fileType }} · {{ task.copies }}份</p>
            <small>申请人：{{ task.applicant }}（{{ task.department }}）</small>
          </div>
          <span :class="$style.approved"
            ><CheckCircle2
              :size="13"
              :stroke-width="1.8"
            />审批通过<br />{{ task.appliedAt }}</span
          ><UiButton
            variant="primary"
            size="sm"
            @click="requestStamp(task)"
            >确认盖章</UiButton
          >
        </article>
        <p
          v-if="!stampTasks.length"
          :class="$style.empty"
        >
          当前没有待盖章任务
        </p>
      </div>
      <div
        v-else-if="activeView === 'ledger'"
        :class="$style.ledgerGrid"
      >
        <article
          v-for="item in ledger"
          :key="item.id"
        >
          <span
            :class="$style.ledgerStatus"
            :data-status="item.status"
            >{{ item.status }}</span
          >
          <div :class="$style.ledgerHeading">
            <Stamp
              :size="20"
              :stroke-width="1.8"
            />
            <div>
              <strong>{{ item.name }}</strong
              ><small>{{ item.type }}</small>
            </div>
          </div>
          <p>
            <Building2
              :size="13"
              :stroke-width="1.8"
            />{{ item.company }}
          </p>
          <p>
            <UserRound
              :size="13"
              :stroke-width="1.8"
            />管理人：{{ item.custodian }} · {{ item.department }}
          </p>
          <p>
            <MapPin
              :size="13"
              :stroke-width="1.8"
            />{{ item.location }}
          </p>
        </article>
      </div>
      <div
        v-else
        :class="$style.statsGrid"
      >
        <article>
          <h2>按印章类型统计</h2>
          <p
            v-for="entry in sealTypeStats"
            :key="entry[0]"
          >
            <span>{{ entry[0] }}</span
            ><i><b /></i><strong>{{ entry[1] }}</strong>
          </p>
        </article>
        <article>
          <h2>按部门统计</h2>
          <p
            v-for="entry in sealDepartmentStats"
            :key="entry[0]"
          >
            <span>{{ entry[0] }}</span
            ><i><b /></i><strong>{{ entry[1] }}</strong>
          </p>
        </article>
        <article>
          <h2>最近用章记录</h2>
          <div
            v-for="item in applications.filter((row) => row.status === '已盖章').slice(0, 5)"
            :key="item.id"
            :class="$style.recent"
          >
            <span :class="$style.recentIcon"
              ><Stamp
                :size="14"
                :stroke-width="1.8"
            /></span>
            <div>
              <strong>{{ item.fileName }}</strong
              ><small>{{ item.applicant }} · {{ item.sealType }} · {{ item.appliedAt }}</small>
            </div>
            <em>已盖章</em>
          </div>
        </article>
      </div>
    </section>
  </main>

  <UiDialog
    v-model:open="formOpen"
    title="新建用章申请"
    description="提交后进入本地审批队列"
    size="lg"
    ><div :class="$style.form">
      <UiFormField label="公司抬头"
        ><UiSelect
          v-model="draft.company"
          label="公司抬头"
          :options="companyOptions" /></UiFormField
      ><UiFormField label="印章类型"
        ><UiSelect
          v-model="draft.sealType"
          label="印章类型"
          :options="sealTypeOptions" /></UiFormField
      ><UiFormField
        label="文件名称"
        required
        :class="$style.formWide"
        ><UiInput
          v-model="draft.fileName"
          aria-label="用章文件名称" /></UiFormField
      ><UiFormField
        label="盖章份数"
        required
        ><UiInput
          v-model="draft.copies"
          aria-label="用章份数"
          inputmode="numeric" /></UiFormField
      ><UiFormField label="紧急程度"
        ><UiSelect
          v-model="draft.urgency"
          label="紧急程度"
          :options="urgencyOptions" /></UiFormField
      ><UiFormField
        label="用章用途"
        required
        :class="$style.formWide"
        ><UiTextarea
          v-model="draft.purpose"
          aria-label="用章用途"
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
        @click="createApplication"
        >提交申请</UiButton
      ></template
    ></UiDialog
  >

  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.fileName || '用章申请详情'"
    :description="selected?.code"
    size="lg"
    ><dl
      v-if="selected"
      :class="$style.detail"
    >
      <div>
        <dt>申请人</dt>
        <dd>{{ selected.applicant }} · {{ selected.department }}</dd>
      </div>
      <div>
        <dt>公司抬头</dt>
        <dd>{{ selected.company }}</dd>
      </div>
      <div>
        <dt>印章类型</dt>
        <dd>{{ selected.sealType }}</dd>
      </div>
      <div>
        <dt>文件份数</dt>
        <dd>{{ selected.fileType }} · {{ selected.copies }}份</dd>
      </div>
      <div>
        <dt>用途</dt>
        <dd>{{ selected.purpose }}</dd>
      </div>
      <div>
        <dt>状态</dt>
        <dd>{{ selected.status }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
  <UiAlertDialog
    v-model:open="confirmOpen"
    title="确认已完成盖章？"
    :description="selected ? `${selected.fileName}将从待盖章任务移出。` : ''"
    confirm-label="确认盖章"
    tone="warning"
    @confirm="confirmStamp"
  />
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
  background: linear-gradient(110deg, #620e23, #ae1d42 55%, #db2e6d);
}
.heroCopy {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-4 flex items-center gap-2 text-[10px];
  color: rgba(255, 255, 255, 0.78);
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
.stampArt {
  @apply absolute bottom-0 right-[12%] h-[112px] w-32;
}
.stampArt i {
  @apply absolute bottom-2 left-10 h-[72px] w-12 rounded-t-lg;
  background: #6e351f;
  border-bottom: 10px solid #3f2419;
}
.stampArt i::before {
  @apply absolute -top-2 left-1 h-4 w-10 rounded-t;
  content: '';
  background: #805426;
}
.stampArt b {
  @apply absolute bottom-[-28px] left-2 h-24 w-24 rounded-full border-[5px];
  border-color: #ff5630;
  box-shadow: inset 0 0 0 8px rgba(255, 86, 48, 0.45);
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[94px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
  color: #0065ff;
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
  color: var(--app-primary);
}
.metrics small {
  @apply text-[9px];
}
.metricPending {
  color: #ff8b00 !important;
}
.metricPending strong {
  color: #ff8b00 !important;
}
.metricMonth {
  color: #6554c0 !important;
}
.metricMonth strong {
  color: #6554c0 !important;
}
.metricOutside {
  color: #ff5630 !important;
}
.metricOutside strong {
  color: #ff5630 !important;
}
.workspace {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-14 items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.tabs {
  @apply mr-auto gap-4;
}
.tab {
  @apply rounded-none border-b-2 border-transparent px-0;
}
.tab[data-state='active'] {
  border-color: var(--app-primary);
  background: transparent;
}
.tab em {
  @apply text-[9px] not-italic;
  color: var(--ui-text-subtle);
}
.statusSelect {
  @apply w-[120px];
}
.search {
  @apply flex w-[210px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.taskList {
  @apply p-4;
}
.taskList article {
  @apply flex items-center gap-4 rounded border p-4;
  border-color: var(--ui-border);
}
.taskIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg;
  color: #ff8b00;
  background: #fff0b3;
}
.taskList article > div {
  @apply min-w-0 flex-1;
}
.taskList strong {
  @apply text-[12px] font-semibold;
}
.taskList p,
.taskList small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.approved {
  @apply text-right text-[9px] leading-5;
  color: #00875a;
}
.approved svg {
  @apply mr-1 inline;
}
.empty {
  @apply py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.ledgerGrid {
  @apply grid grid-cols-4 gap-3 p-4;
}
.ledgerGrid article {
  @apply relative rounded border p-4;
  border-color: var(--ui-border);
}
.ledgerStatus {
  @apply absolute right-3 top-3 rounded px-2 py-1 text-[8px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.ledgerStatus[data-status='借出'] {
  color: #ff8b00;
  background: #fff0b3;
}
.ledgerStatus[data-status='作废'] {
  color: #de350b;
  background: #ffebe6;
}
.ledgerHeading {
  @apply mb-4 flex items-center gap-3;
  color: var(--app-primary);
}
.ledgerHeading strong,
.ledgerHeading small {
  @apply block;
}
.ledgerHeading strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text);
}
.ledgerHeading small {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.ledgerGrid p {
  @apply mt-2 flex items-center gap-2 text-[9px];
  color: var(--ui-text-muted);
}
.statsGrid {
  @apply grid grid-cols-3 gap-4 p-4;
}
.statsGrid > article {
  @apply rounded border p-4;
  border-color: var(--ui-border);
}
.statsGrid h2 {
  @apply mb-4 text-[12px] font-semibold;
}
.statsGrid article > p {
  @apply mt-3 grid grid-cols-[90px_1fr_20px] items-center gap-2 text-[9px];
}
.statsGrid article > p i {
  @apply h-1.5 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}
.statsGrid article > p i b {
  @apply block h-full w-2/3 rounded-full;
  background: var(--app-primary);
}
.statsGrid article > p strong {
  @apply text-right;
}
.recent {
  @apply flex items-center gap-2 border-b py-2;
  border-color: var(--ui-border-subtle);
}
.recentIcon {
  @apply flex h-7 w-7 items-center justify-center rounded;
  color: #de350b;
  background: #ffebe6;
}
.recent > div {
  @apply min-w-0 flex-1;
}
.recent strong,
.recent small {
  @apply block truncate;
}
.recent strong {
  @apply text-[9px] font-semibold;
}
.recent small {
  @apply mt-1 text-[8px];
  color: var(--ui-text-subtle);
}
.recent em {
  @apply text-[8px] not-italic;
  color: #00875a;
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
</style>
