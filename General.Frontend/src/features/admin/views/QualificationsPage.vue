<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  Bell,
  BookOpen,
  CheckCircle2,
  FileBadge2,
  FileText,
  FolderOpen,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  TriangleAlert,
  XCircle,
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
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { policyApplications, qualificationMaterials, qualificationsFixture } from '../model/qualifications.data';
import type {
  Qualification,
  QualificationCategory,
  QualificationDraft,
  QualificationStatus,
} from '../model/qualifications.types';
import QualificationsTable from './qualifications/QualificationsTable.vue';

type Section = 'ledger' | 'alerts' | 'applications' | 'materials';
const records = ref<Qualification[]>(cloneReferenceRows(qualificationsFixture.rows));
const section = ref<Section>('ledger');
const category = ref<QualificationCategory | '全部类别'>('全部类别');
const status = ref<QualificationStatus | '全部状态'>('全部状态');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const deleteOpen = ref(false);
const editingId = ref<string | null>(null);
const selected = ref<Qualification | null>(null);
const deleteCandidate = ref<Qualification | null>(null);
const draft = reactive<QualificationDraft>({
  name: '',
  archiveCode: '',
  category: '体系认证',
  certificateNo: '',
  issuer: '',
  validTo: '',
  owner: '',
});
const categoryOptions: readonly UiSelectOption[] = [
  '全部类别',
  '体系认证',
  '行业资质',
  '安全资质',
  '知识产权证书',
  '产品认证',
  '企业荣誉资质',
  '政府认定类资质',
].map((value) => ({ value, label: value }));
const statusOptions: readonly UiSelectOption[] = [
  '全部状态',
  '有效',
  '即将到期',
  '年审中',
  '复审中',
  '已过期',
  '已注销',
].map((value) => ({ value, label: value }));
const createCategoryOptions = categoryOptions.slice(1);
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return records.value.filter(
    (item) =>
      (category.value === '全部类别' || item.category === category.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle || `${item.name} ${item.certificateNo}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openCreate() {
  editingId.value = null;
  Object.assign(draft, {
    name: '',
    archiveCode: `CERT-${String(records.value.length + 1).padStart(3, '0')}`,
    category: '体系认证',
    certificateNo: '',
    issuer: '',
    validTo: '',
    owner: '',
  });
  formOpen.value = true;
}
function openEdit(item: Qualification) {
  editingId.value = item.id;
  Object.assign(draft, {
    name: item.name,
    archiveCode: item.archiveCode,
    category: item.category,
    certificateNo: item.certificateNo,
    issuer: item.issuer,
    validTo: item.validTo,
    owner: item.owner,
  });
  formOpen.value = true;
}
function saveRecord() {
  if (!draft.name.trim() || !draft.certificateNo.trim()) return toast.warning('请填写资质名称和证书编号');
  if (editingId.value) {
    records.value = records.value.map((item) => (item.id === editingId.value ? { ...item, ...draft } : item));
  } else {
    records.value.unshift({
      id: `cert-local-${records.value.length + 1}`,
      ...draft,
      validFrom: '2026-09-17',
      status: '有效',
    });
  }
  formOpen.value = false;
  toast.success(editingId.value ? '资质已在本地更新' : '资质已添加到本地台账');
}
function openDetail(item: Qualification) {
  selected.value = item;
  detailOpen.value = true;
}
function requestDelete(item: Qualification) {
  deleteCandidate.value = item;
  deleteOpen.value = true;
}
function confirmDelete() {
  if (deleteCandidate.value) records.value = records.value.filter((item) => item.id !== deleteCandidate.value?.id);
  deleteOpen.value = false;
  toast.success('资质记录已从本地台账删除');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/qualifications"
    data-page-label="资质与申报"
    data-page-module="行政管理"
    data-qualifications-page
    :data-section="section"
    :data-result-count="filtered.length"
    :data-total-count="records.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 资质与申报</nav>
        <section>
          <span
            ><ShieldCheck
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>资质与申报管理</h1>
            <p>企业证书资质台账、到期提醒、政策申报跟踪与材料统一管理</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i>资 质 证 书</i><i>荣 誉 证 书</i><b>1</b><em>★</em>
      </div>
    </header>
    <header :class="$style.navigation">
      <UiTabs v-model="section"
        ><UiTabsList
          label="资质业务区"
          :class="$style.tabs"
          ><UiTabsTrigger
            value="ledger"
            :class="$style.tab"
            ><BookOpen
              :size="14"
              :stroke-width="1.8"
            />资质台账</UiTabsTrigger
          ><UiTabsTrigger
            value="alerts"
            :class="$style.tab"
            ><Bell
              :size="14"
              :stroke-width="1.8"
            />到期提醒</UiTabsTrigger
          ><UiTabsTrigger
            value="applications"
            :class="$style.tab"
            ><FileText
              :size="14"
              :stroke-width="1.8"
            />政策申报</UiTabsTrigger
          ><UiTabsTrigger
            value="materials"
            :class="$style.tab"
            ><FolderOpen
              :size="14"
              :stroke-width="1.8"
            />材料库</UiTabsTrigger
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
        >新增资质</UiButton
      >
    </header>
    <template v-if="section === 'ledger'">
      <section :class="$style.metrics">
        <article>
          <span
            ><CheckCircle2
              :size="17"
              :stroke-width="1.8"
          /></span>
          <div>
            <p>有效资质</p>
            <strong>10</strong>
          </div>
        </article>
        <article>
          <span
            ><TriangleAlert
              :size="17"
              :stroke-width="1.8"
          /></span>
          <div>
            <p>即将到期</p>
            <strong>1</strong>
          </div>
        </article>
        <article>
          <span
            ><RefreshCw
              :size="17"
              :stroke-width="1.8"
          /></span>
          <div>
            <p>审核中</p>
            <strong>0</strong>
          </div>
        </article>
        <article>
          <span
            ><XCircle
              :size="17"
              :stroke-width="1.8"
          /></span>
          <div>
            <p>已过期</p>
            <strong>1</strong>
          </div>
        </article>
      </section>
      <section :class="$style.toolbar">
        <label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索资质"
            placeholder="搜索资质名称、证书编号..." /></label
        ><UiSelect
          v-model="category"
          label="资质类别"
          :options="categoryOptions"
          :class="$style.filter"
        /><UiSelect
          v-model="status"
          label="资质状态"
          :options="statusOptions"
          :class="$style.filter"
        /><span>共 {{ filtered.length }} 条记录</span>
      </section>
      <QualificationsTable
        :class="$style.tableSurface"
        :rows="filtered"
        @view="openDetail"
        @edit="openEdit"
        @remove="requestDelete"
      />
    </template>
    <section
      v-else-if="section === 'alerts'"
      :class="$style.alternate"
    >
      <header>
        <h2>到期提醒</h2>
        <p>按剩余有效期跟踪换证与复审任务</p>
      </header>
      <article
        v-for="item in records.filter((record) => record.expiryHint)"
        :key="item.id"
      >
        <TriangleAlert
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.validFrom }} ~ {{ item.validTo }} · {{ item.expiryHint }}</p>
        </div>
        <UiButton
          size="sm"
          variant="secondary"
          @click="openDetail(item)"
          >查看</UiButton
        >
      </article>
    </section>
    <section
      v-else-if="section === 'applications'"
      :class="$style.alternate"
    >
      <header>
        <h2>政策申报</h2>
        <p>跟踪政策项目的截止日期和材料进度</p>
      </header>
      <article
        v-for="item in policyApplications"
        :key="item.id"
      >
        <FileBadge2
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.department }} · 截止 {{ item.deadline }}</p>
        </div>
        <span>{{ item.progress }}</span>
      </article>
    </section>
    <section
      v-else
      :class="$style.alternate"
    >
      <header>
        <h2>材料库</h2>
        <p>统一维护资质与政策申报复用材料</p>
      </header>
      <article
        v-for="item in qualificationMaterials"
        :key="item.id"
      >
        <FolderOpen
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.count }} 份材料 · 更新于 {{ item.updatedAt }}</p>
        </div>
        <UiButton
          size="sm"
          variant="secondary"
          >打开</UiButton
        >
      </article>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    :title="editingId ? '编辑资质' : '新增资质'"
    description="维护企业资质证书台账"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="资质名称"
        required
        ><UiInput v-model="draft.name" /></UiFormField
      ><UiFormField label="台账编号"><UiInput v-model="draft.archiveCode" /></UiFormField
      ><UiFormField label="资质类别"
        ><UiSelect
          v-model="draft.category"
          label="新增资质类别"
          :options="createCategoryOptions" /></UiFormField
      ><UiFormField
        label="证书编号"
        required
        ><UiInput v-model="draft.certificateNo" /></UiFormField
      ><UiFormField label="发证机构"><UiInput v-model="draft.issuer" /></UiFormField
      ><UiFormField label="有效期至"><UiInput v-model="draft.validTo" /></UiFormField
      ><UiFormField label="责任人"><UiInput v-model="draft.owner" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveRecord"
        >保存资质</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.name || '资质详情'"
    :description="selected?.certificateNo"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>类别</dt>
          <dd>{{ selected.category }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
        <div>
          <dt>发证机构</dt>
          <dd>{{ selected.issuer }}</dd>
        </div>
        <div>
          <dt>责任人</dt>
          <dd>{{ selected.owner }}</dd>
        </div>
        <div>
          <dt>有效期</dt>
          <dd>{{ selected.validFrom }} ~ {{ selected.validTo }}</dd>
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
  <UiAlertDialog
    v-model:open="deleteOpen"
    title="删除资质记录"
    :description="`确认删除“${deleteCandidate?.name || ''}”吗？此操作只影响本地复刻数据。`"
    confirm-label="删除"
    @confirm="confirmDelete"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mx-6 mb-4 mt-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #842c13, #cb5a07 55%, #ff8b00);
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
  @apply absolute inset-y-0 right-0 w-[70%];
}
.heroArt i {
  @apply absolute bottom-[-16px] flex h-28 w-40 items-start justify-center rounded-t border-2 pt-5 text-[11px] not-italic;
  color: #9d3618;
  border-color: #8d3c13;
  background: #e7c29e;
}
.heroArt i:first-child {
  left: 8%;
}
.heroArt i:nth-child(2) {
  left: 38%;
  height: 92px;
  background: #e7d3b9;
}
.heroArt b {
  @apply absolute bottom-0 left-[58%] flex h-20 w-24 items-center justify-center rounded-t-full text-[12px];
  color: #8d3c13;
  background: #e6a741;
}
.heroArt em {
  @apply absolute right-[12%] top-[-28px] flex h-32 w-32 items-center justify-center rounded-full border-[5px] text-[36px] not-italic;
  color: #842c13;
  border-color: #8d3c13;
  background: #ffd060;
}
.navigation {
  @apply mb-4 flex h-12 items-center border-b bg-white px-8;
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
.navigation > button {
  @apply ml-auto;
}
.metrics {
  @apply mx-6 mb-4 grid grid-cols-4 gap-4;
}
.metrics article {
  @apply flex min-h-[82px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > span {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(2) > span {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(3) > span {
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(4) > span {
  color: #de350b;
  background: #ffebe6;
}
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1 block text-[20px];
}
.toolbar {
  @apply mx-6 mb-4 flex h-[85px] items-center gap-3 rounded border bg-white px-4;
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
  @apply w-[145px];
}
.toolbar > span {
  @apply ml-auto text-[10px];
  color: var(--ui-text-subtle);
}
.alternate {
  @apply mx-6 overflow-hidden rounded border bg-white;
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
.alternate article {
  @apply flex min-h-[68px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border-subtle);
}
.alternate article > svg {
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
  @apply rounded px-2 py-1 text-[9px];
  color: #ff8b00;
  background: #fff0b3;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.tableSurface {
  @apply mx-6;
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
.detail dl div:last-child {
  @apply col-span-2;
}
.detail dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[11px] font-medium;
}
</style>
