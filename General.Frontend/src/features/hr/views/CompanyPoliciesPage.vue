<script setup lang="ts">
import { computed, ref } from 'vue';
import { AlertTriangle, BarChart3, FilePlus2, ScrollText, ShieldAlert, Sparkles, Upload } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { companyPoliciesFixture } from '../model/company-policies.data';
import type { CompanyPolicyRecord, CompanyPolicyView } from '../model/company-policies.types';
import PoliciesHero from './policies/PoliciesHero.vue';
import CompanyPoliciesCenter from './company-policies/CompanyPoliciesCenter.vue';
import CompanyPoliciesSummary from './company-policies/CompanyPoliciesSummary.vue';
import CompanyPoliciesView from './company-policies/CompanyPoliciesView.vue';

const rows = ref<CompanyPolicyRecord[]>(cloneReferenceRows(companyPoliciesFixture.rows));
const view = ref<CompanyPolicyView>('制度中心');
const detailOpen = ref(false);
const createOpen = ref(false);
const uploadOpen = ref(false);
const activeRow = ref<CompanyPolicyRecord | null>(null);
const uploadName = ref('');
const newTitle = ref('');
const pendingCount = computed(() => rows.value.filter((row) => row.status === '待确认').length);
function setView(value: string) {
  view.value = value as CompanyPolicyView;
}
function openRow(row: CompanyPolicyRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function handleFile(event: Event) {
  const input = event.target as HTMLInputElement;
  uploadName.value = input.files?.[0]?.name ?? '';
}
function saveUpload() {
  if (!uploadName.value) {
    toast.warning('请先选择本地 Word 或 PDF 文件');
    return;
  }
  uploadOpen.value = false;
  toast.success(`已在本地解析 ${uploadName.value}`);
}
function createPolicy() {
  if (!newTitle.value.trim()) {
    toast.warning('请填写制度名称');
    return;
  }
  rows.value = [
    {
      id: `cp-${Date.now()}`,
      code: `POL-DRAFT-${String(rows.value.length + 1).padStart(3, '0')}`,
      title: newTitle.value.trim(),
      version: 'V0.1',
      type: '制度',
      summary: '从空白文档创建的本地制度草稿。',
      category: '人事制度',
      status: '草稿',
      clauses: 0,
      important: 0,
      myProgress: 0,
      coverage: 0,
      signed: '草稿待审批',
      overdue: 0,
    },
    ...rows.value,
  ];
  createOpen.value = false;
  view.value = '制度中心';
  toast.success('制度草稿已保存到本地');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/company-policies"
    data-page-label="公司规章制度"
    data-page-module="人事管理"
    data-company-policies-page
    :data-view="view"
    :data-policy-count="rows.length"
  >
    <PoliciesHero
      breadcrumb="公司规章制度"
      subtitle="5 部制度 · 全员阅读率 89% · 电签率 88% · 4 起进行中违规通报"
    />
    <UiTabs
      :model-value="view"
      @update:model-value="setView"
      ><UiTabsList
        label="公司制度视图"
        :class="$style.tabs"
        ><UiTabsTrigger value="制度中心"
          ><ScrollText
            :size="14"
            :stroke-width="1.8"
          />制度中心 <b>3</b></UiTabsTrigger
        ><UiTabsTrigger value="AI 智能上传"
          ><Upload
            :size="14"
            :stroke-width="1.8"
          />AI 智能上传</UiTabsTrigger
        ><UiTabsTrigger value="合规看板"
          ><BarChart3
            :size="14"
            :stroke-width="1.8"
          />合规看板</UiTabsTrigger
        ><UiTabsTrigger value="违规通报"
          ><ShieldAlert
            :size="14"
            :stroke-width="1.8"
          />违规通报 <b>4</b></UiTabsTrigger
        ></UiTabsList
      ></UiTabs
    >
    <template v-if="view === '制度中心'">
      <section :class="$style.pending">
        <AlertTriangle
          :size="18"
          :stroke-width="1.8"
        />
        <p>
          <strong>您有 {{ pendingCount }} 部制度的新版本待签字确认</strong
          ><span>商业秘密与信息安全管理制度 · 员工考勤管理细则 · 差旅与费用报销办法</span>
        </p>
        <UiButton
          variant="warning"
          size="sm"
          @click="toast.success('已进入待确认制度队列')"
          >立即处理</UiButton
        >
      </section>
      <CompanyPoliciesSummary />
      <section :class="$style.aiRow">
        <div :class="$style.aiBanner">
          <Sparkles
            :size="24"
            :stroke-width="1.8"
          />
          <p>
            <strong>AI 智能上传：把现有 Word/PDF 一键变成可签字的制度</strong
            ><span>自动识别章节·提取重要条款·与现版本对比生成差异报告·建议是否要求全员重读</span>
          </p>
          <UiButton
            size="sm"
            @click="uploadOpen = true"
            ><template #leading
              ><Upload
                :size="14"
                :stroke-width="1.8" /></template
            >上传文件</UiButton
          >
        </div>
        <div :class="$style.quick">
          <button
            type="button"
            @click="createOpen = true"
          >
            <FilePlus2
              :size="16"
              :stroke-width="1.8"
            /><span><strong>新建制度</strong><small>从空白文档开始</small></span></button
          ><button
            type="button"
            @click="view = '合规看板'"
          >
            <BarChart3
              :size="16"
              :stroke-width="1.8"
            /><span><strong>合规看板</strong><small>阅读/电签/逾期</small></span></button
          ><button
            type="button"
            @click="view = '违规通报'"
          >
            <ShieldAlert
              :size="16"
              :stroke-width="1.8"
            /><span><strong>发布通报</strong><small>违规处理公示</small></span></button
          ><button
            type="button"
            @click="toast.success('匿名举报通道已在本地打开')"
          >
            <AlertTriangle
              :size="16"
              :stroke-width="1.8"
            /><span><strong>匿名举报</strong><small>保护举报人</small></span>
          </button>
        </div>
      </section>
      <CompanyPoliciesCenter
        :rows="rows"
        @open="openRow"
      />
    </template>
    <CompanyPoliciesView
      v-else
      :view="view"
      :rows="rows"
      @upload="uploadOpen = true"
      @create="createOpen = true"
      @open="openRow"
    />
  </main>

  <UiDialog
    v-model:open="uploadOpen"
    title="AI 智能上传"
    description="文件只在本地读取，不会提交到外部参考站"
    size="md"
    ><div :class="$style.upload">
      <label
        >选择 Word / PDF<input
          type="file"
          accept=".doc,.docx,.pdf"
          @change="handleFile"
      /></label>
      <p>{{ uploadName || '尚未选择文件' }}</p>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="uploadOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="saveUpload"
        >开始解析</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="createOpen"
    title="新建制度"
    description="从空白文档创建本地草稿"
    size="md"
    ><div :class="$style.create">
      <UiFormField
        label="制度名称"
        required
        ><UiInput
          v-model="newTitle"
          aria-label="公司制度名称"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="createOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="createPolicy"
        >创建草稿</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title || '制度详情'"
    :description="activeRow ? `${activeRow.code} · ${activeRow.version} · ${activeRow.status}` : undefined"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>制度分类</dt>
        <dd>{{ activeRow.category }}</dd>
      </div>
      <div>
        <dt>条款</dt>
        <dd>{{ activeRow.clauses }} 条 · {{ activeRow.important }} 条重要</dd>
      </div>
      <div>
        <dt>我的进度</dt>
        <dd>{{ activeRow.myProgress }}%</dd>
      </div>
      <div>
        <dt>全员覆盖</dt>
        <dd>{{ activeRow.coverage }}%</dd>
      </div>
      <div :class="$style.wide">
        <dt>摘要</dt>
        <dd>{{ activeRow.summary }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.tabs {
  @apply mt-3 h-11 w-full justify-start rounded border bg-white px-2;
  border-color: var(--ui-border);
}
.tabs [role='tab'] {
  @apply h-9;
}
.tabs b {
  @apply rounded-full px-1.5 py-0.5 text-[8px];
  color: white;
  background: var(--ui-warning);
}
.pending {
  @apply mt-4 flex h-[60px] items-center gap-3 rounded border px-4;
  border-color: #ffbd85;
  background: #fff8ed;
}
.pending > svg {
  @apply box-content rounded p-2;
  color: white;
  background: var(--ui-warning);
}
.pending p {
  @apply m-0 flex flex-1 flex-col text-[10px];
  color: var(--ui-text-muted);
}
.pending strong {
  @apply text-[11px];
  color: var(--ui-text);
}
.aiRow {
  @apply mt-4 grid grid-cols-[3fr_2fr] gap-3;
}
.aiBanner {
  @apply flex h-[120px] items-center gap-4 rounded px-5 text-white;
  background: linear-gradient(105deg, #4b3fb3, #075dce 55%, #02b8d2);
}
.aiBanner > svg {
  @apply box-content rounded border border-white/30 bg-white/10 p-3;
}
.aiBanner p {
  @apply m-0 flex flex-1 flex-col;
}
.aiBanner strong {
  @apply text-[14px];
}
.aiBanner span {
  @apply mt-2 text-[10px] text-white/75;
}
.quick {
  @apply grid grid-cols-2 gap-3;
}
.quick button {
  @apply flex items-center gap-3 rounded border bg-white px-3 text-left;
  border-color: var(--ui-border);
  color: var(--app-primary);
}
.quick button:hover {
  background: var(--ui-surface-hover);
}
.quick span {
  @apply flex flex-col;
}
.quick strong {
  @apply text-[10px];
  color: var(--ui-text);
}
.quick small {
  @apply mt-1 text-[8px];
  color: var(--ui-text-subtle);
}
.upload,
.create {
  @apply p-5;
}
.upload label {
  @apply flex h-20 cursor-pointer flex-col items-center justify-center rounded border border-dashed text-[10px];
  border-color: var(--app-primary);
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.upload input {
  @apply mt-2 text-[9px];
}
.upload p {
  @apply m-0 mt-3 text-center text-[10px];
  color: var(--ui-text-muted);
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details div {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply m-0 mt-2 text-[11px] font-medium;
}
.wide {
  @apply col-span-2;
}
</style>
