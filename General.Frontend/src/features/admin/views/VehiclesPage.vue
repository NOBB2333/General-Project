<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  CalendarDays,
  CarFront,
  CircleDollarSign,
  FileText,
  Gauge,
  Plus,
  Search,
  TriangleAlert,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { vehicleRequestsFixture, vehiclesFixture } from '../model/vehicles.data';
import type { VehicleDraft, VehicleRecord, VehicleRequest, VehicleStatus } from '../model/vehicles.types';
import VehiclesTable from './vehicles/VehiclesTable.vue';

type View = 'company' | 'private' | 'calendar' | 'fleet' | 'stats';
const requests = ref<VehicleRequest[]>(cloneReferenceRows(vehicleRequestsFixture.rows));
const view = ref<View>('company');
const status = ref<VehicleStatus | '全部状态'>('全部状态');
const query = ref('');
const company = ref('全部公司');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<VehicleRequest | null>(null);
const fleet = ref<VehicleRecord[]>(cloneReferenceRows(vehiclesFixture));
const draft = reactive<VehicleDraft>({ purpose: '', route: '', time: '', mileage: '', vehicleType: '新能源' });
const statusOptions: readonly UiSelectOption[] = [
  '全部状态',
  '待审批',
  '审批中',
  '已派车',
  '已用车',
  '已通过',
  '已驳回',
].map((value) => ({ value, label: value }));
const companyOptions: readonly UiSelectOption[] = ['全部公司', '瑞视自动化', '瑞视智能', '南京瑞视'].map((value) => ({
  value,
  label: value,
}));
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return requests.value.filter(
    (item) =>
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle ||
        `${item.code} ${item.applicant} ${item.purpose} ${item.route}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openCreate() {
  Object.assign(draft, {
    purpose: '',
    route: '',
    time: '2026-09-17 09:00 至 2026-09-17 18:00',
    mileage: '0 km · 1 人',
    vehicleType: '新能源',
  });
  formOpen.value = true;
}
function saveRequest() {
  if (!draft.purpose.trim() || !draft.route.trim()) return toast.warning('请填写用车用途和路线');
  requests.value.unshift({
    id: `cv-local-${requests.value.length + 1}`,
    code: `CV-2026-${String(requests.value.length + 219).padStart(4, '0')}`,
    applicant: '张经理',
    department: '行政部',
    ...draft,
    dispatch: '未派车',
    status: '待审批',
    approver: '李 → 孙',
  });
  formOpen.value = false;
  toast.success('公司用车申请已提交到本地');
}
function openDetail(item: VehicleRequest) {
  selected.value = item;
  detailOpen.value = true;
}
function approve() {
  if (!selected.value) return;
  requests.value = requests.value.map((item) =>
    item.id === selected.value?.id ? { ...item, status: '已通过' } : item,
  );
  selected.value = { ...selected.value, status: '已通过' };
  toast.success('用车申请已在本地通过');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/vehicles"
    data-page-label="车辆管理"
    data-page-module="行政管理"
    data-vehicles-page
    :data-view="view"
    :data-result-count="filtered.length"
    :data-total-count="requests.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 车辆管理</nav>
        <section>
          <span
            ><CarFront
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>车辆管理</h1>
            <p>公司用车申请、私车公用补贴与车辆台账一体化管理</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i>FLEET</i><b /><em />
      </div>
      <div :class="$style.heroActions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="view = 'private'"
          >私车公用申请</UiButton
        ><UiButton
          variant="secondary"
          size="sm"
          @click="openCreate"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >公司用车申请</UiButton
        >
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <CarFront
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>可用车辆</span><strong>5</strong>
          <p>共 8 辆 · 使用中 1</p>
        </div>
      </article>
      <article>
        <Gauge
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>用车待审批</span><strong>4</strong>
          <p>公司 2 · 私车 2</p>
        </div>
      </article>
      <article>
        <CircleDollarSign
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>本月私车补贴</span><strong>¥524</strong>
          <p>已结算合计</p>
        </div>
      </article>
      <article>
        <TriangleAlert
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>证件/保养提醒</span><strong>2</strong>
          <p>年检 0 · 保险 0 · 保养 2</p>
        </div>
      </article>
    </section>
    <header :class="$style.navigation">
      <nav>
        <button
          v-for="item in [
            ['company', '公司用车 7'],
            ['private', '私车公用 7'],
            ['calendar', '用车日历'],
            ['fleet', '车辆台账 8'],
            ['stats', '用车统计'],
          ]"
          :key="item[0]"
          :data-active="view === item[0]"
          @click="view = item[0] as View"
        >
          {{ item[1] }}
        </button>
      </nav>
      <div
        v-if="view === 'company'"
        :class="$style.filters"
      >
        <UiSelect
          v-model="company"
          label="用车公司"
          :options="companyOptions"
        /><UiSelect
          v-model="status"
          label="用车状态"
          :options="statusOptions"
        /><label :class="$style.search"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索用车申请"
            placeholder="搜索申请..."
        /></label>
      </div>
    </header>
    <section
      v-if="view === 'company'"
      :class="$style.tableSurface"
    >
      <VehiclesTable
        :rows="filtered"
        @view="openDetail"
      />
    </section>
    <section
      v-else-if="view === 'private'"
      :class="$style.alternate"
    >
      <header>
        <h2>私车公用申请</h2>
        <p>本月私车公用里程与补贴结算</p>
      </header>
      <article>
        <CircleDollarSign
          :size="20"
          :stroke-width="1.8"
        />
        <div>
          <strong>本月已结算补贴</strong>
          <p>12 次申请 · 524 元</p>
        </div>
        <UiButton
          variant="primary"
          size="sm"
          @click="toast.success('私车公用申请入口已准备')"
          >发起申请</UiButton
        >
      </article>
    </section>
    <section
      v-else-if="view === 'calendar'"
      :class="$style.alternate"
    >
      <header>
        <h2>用车日历</h2>
        <p>按日期查看公司车辆占用情况</p>
      </header>
      <div :class="$style.calendar">
        <article
          v-for="day in ['05-04', '05-05', '05-06', '05-09', '05-10', '05-12']"
          :key="day"
        >
          <strong>{{ day }}</strong
          ><span>{{ requests.filter((item) => item.time.includes(day)).length || 0 }} 项申请</span>
        </article>
      </div>
    </section>
    <section
      v-else-if="view === 'fleet'"
      :class="$style.alternate"
    >
      <header>
        <h2>车辆台账</h2>
        <p>8 辆公司车辆，当前 5 辆可用</p>
      </header>
      <article
        v-for="item in fleet"
        :key="item.id"
      >
        <CarFront
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.plate }} · {{ item.type }}</strong>
          <p>司机：{{ item.driver }} · 下次年检 {{ item.nextInspection }}</p>
        </div>
        <span :data-status="item.status">{{ item.status }}</span>
      </article>
    </section>
    <section
      v-else
      :class="$style.stats"
    >
      <FileText
        :size="26"
        :stroke-width="1.8"
      />
      <h2>用车统计</h2>
      <strong>本月 18 次</strong>
      <p>公司用车 12 次 · 私车公用 6 次 · 总里程 1,280 km</p>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="公司用车申请"
    description="申请只写入本地用车数据"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="用车用途"
        required
        ><UiInput v-model="draft.purpose" /></UiFormField
      ><UiFormField
        label="用途/路线"
        required
        ><UiInput v-model="draft.route" /></UiFormField
      ><UiFormField label="用车时间"><UiInput v-model="draft.time" /></UiFormField
      ><UiFormField label="里程与人数"><UiInput v-model="draft.mileage" /></UiFormField
      ><UiFormField label="车型"><UiInput v-model="draft.vehicleType" /></UiFormField>
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
    :title="selected?.code || '用车详情'"
    :description="selected?.purpose"
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
          <dt>路线</dt>
          <dd>{{ selected.route }}</dd>
        </div>
        <div>
          <dt>用车时间</dt>
          <dd>{{ selected.time }}</dd>
        </div>
        <div>
          <dt>车型 / 里程</dt>
          <dd>{{ selected.vehicleType }} · {{ selected.mileage }}</dd>
        </div>
        <div>
          <dt>派车</dt>
          <dd>{{ selected.dispatch }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
      </dl>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ><UiButton
        v-if="selected && selected.status === '待审批'"
        variant="primary"
        @click="approve"
        >通过申请</UiButton
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
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #152948, #0e3e83 58%, #096e94);
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
  @apply absolute inset-y-0 right-0 w-[54%];
  background: linear-gradient(165deg, transparent 30%, rgba(42, 84, 153, 0.8));
}
.heroArt i {
  @apply absolute right-[28%] top-2 rounded border px-5 py-2 text-[10px] not-italic;
  border-color: #7cc2af;
  background: #1c6f5e;
}
.heroArt b {
  @apply absolute bottom-[-20px] left-[36%] h-24 w-1;
  background: #8ca4ca;
}
.heroArt em {
  @apply absolute right-[12%] top-[-20px] h-20 w-32 rounded-full;
  background: rgba(170, 206, 238, 0.28);
}
.heroActions {
  @apply absolute bottom-7 right-6 z-[2] flex gap-2;
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[96px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > svg {
  @apply rounded p-2;
  box-sizing: content-box;
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(2) > svg {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(3) > svg {
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(4) > svg {
  color: #de350b;
  background: #ffebe6;
}
.metrics span,
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[20px];
}
.metrics p {
  @apply mt-1;
}
.navigation {
  @apply flex h-14 items-center border-b;
  border-color: var(--ui-border);
}
.navigation nav {
  @apply flex h-full;
}
.navigation nav button {
  @apply h-full border-b-2 border-transparent px-3 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.navigation nav button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.filters {
  @apply ml-auto flex items-center gap-2;
}
.filters > div {
  @apply w-[110px];
}
.search {
  @apply flex w-[220px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.tableSurface {
  @apply rounded border bg-white;
  border-color: var(--ui-border);
}
.alternate {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.alternate > header {
  @apply border-b p-5;
  border-color: var(--ui-border);
}
.alternate h2 {
  @apply text-[16px] font-semibold;
}
.alternate header p {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.alternate > article {
  @apply flex min-h-[76px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border-subtle);
}
.alternate > article > svg {
  color: var(--app-primary);
}
.alternate article > div {
  @apply min-w-0 flex-1;
}
.alternate article strong {
  @apply text-[11px];
}
.alternate article p {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.alternate article > span {
  @apply rounded px-2 py-1 text-[8px];
  color: #00875a;
  background: #e3fcef;
}
.calendar {
  @apply grid grid-cols-3 gap-3 p-5;
}
.calendar article {
  @apply flex min-h-[100px] flex-col justify-between rounded border p-4;
  border-color: var(--ui-border);
}
.calendar span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.stats {
  @apply rounded border bg-white py-16 text-center;
  border-color: var(--ui-border);
  color: #0052cc;
}
.stats h2 {
  @apply mt-3 text-[16px];
  color: var(--ui-text);
}
.stats strong {
  @apply mt-4 block text-[28px];
}
.stats p {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
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
