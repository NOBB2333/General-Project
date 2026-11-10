<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { Ban, CheckCircle2, FileText, Megaphone, Plus, Search } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { noticesReferenceFixture } from '../model/notices.data';
import type {
  CompanyNotice,
  NoticeCategory,
  NoticeDraft,
  NoticePriority,
  NoticeScope,
  NoticeStatus,
} from '../model/notices.types';
import NoticesTable from './notices/NoticesTable.vue';

const notices = ref<CompanyNotice[]>(cloneReferenceRows(noticesReferenceFixture.rows));
const query = ref('');
const category = ref<NoticeCategory | '全部类别'>('全部类别');
const scope = ref<NoticeScope | '全部范围'>('全部范围');
const status = ref<NoticeStatus | '全部状态'>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const withdrawOpen = ref(false);
const selected = ref<CompanyNotice | null>(null);
const withdrawTarget = ref<CompanyNotice | null>(null);
const draft = reactive<NoticeDraft>({
  title: '',
  summary: '',
  category: '政策制度',
  priority: '普通',
  scope: '全公司',
});

const categoryOptions: readonly UiSelectOption[] = [
  '全部类别',
  '政策制度',
  '人事变动',
  '活动通知',
  '紧急通知',
  '节假日安排',
  '规章制度',
  '其他',
].map((value) => ({ value, label: value }));
const scopeOptions: readonly UiSelectOption[] = ['全部范围', '全公司', '指定部门', '指定人员'].map((value) => ({
  value,
  label: value,
}));
const statusOptions: readonly UiSelectOption[] = ['全部状态', '草稿', '已发布', '已撤回', '已过期'].map((value) => ({
  value,
  label: value,
}));
const formCategoryOptions = categoryOptions.slice(1);
const formScopeOptions = scopeOptions.slice(1);
const priorityOptions: readonly UiSelectOption[] = (['普通', '重要', '紧急'] satisfies NoticePriority[]).map(
  (value) => ({ value, label: value }),
);

const filteredNotices = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return notices.value.filter(
    (notice) =>
      (!needle ||
        `${notice.code} ${notice.title} ${notice.summary} ${notice.publisher} ${notice.department}`
          .toLocaleLowerCase('zh-CN')
          .includes(needle)) &&
      (category.value === '全部类别' || notice.category === category.value) &&
      (scope.value === '全部范围' || notice.scope === scope.value) &&
      (status.value === '全部状态' || notice.status === status.value),
  );
});
const publishedCount = computed(() => notices.value.filter((notice) => notice.status === '已发布').length);
const draftCount = computed(() => notices.value.filter((notice) => notice.status === '草稿').length);
const withdrawnCount = computed(() => notices.value.filter((notice) => notice.status === '已撤回').length);

function openCreate() {
  Object.assign(draft, { title: '', summary: '', category: '政策制度', priority: '普通', scope: '全公司' });
  formOpen.value = true;
}

function publishNotice() {
  if (!draft.title.trim() || !draft.summary.trim()) {
    toast.warning('请填写通知主题和正文摘要');
    return;
  }
  const next = notices.value.length + 1;
  notices.value.unshift({
    id: `notice-local-${next}`,
    code: `NOTC-LOCAL-${String(next).padStart(3, '0')}`,
    title: draft.title.trim(),
    summary: draft.summary.trim(),
    category: draft.category,
    priority: draft.priority,
    scope: draft.scope,
    scopeDetail: draft.scope === '全公司' ? '全体员工' : '本地指定范围',
    department: '人事行政部',
    publisher: '张经理',
    publisherInitial: '张',
    publishedAt: '2026-09-16 10:00',
    readCount: 0,
    totalReaders: draft.scope === '全公司' ? 96 : 12,
    status: '已发布',
    pinned: false,
    attachments: 0,
  });
  formOpen.value = false;
  toast.success('公司通知已在本地发布');
}

function openDetail(notice: CompanyNotice) {
  selected.value = notice;
  detailOpen.value = true;
}

function requestWithdraw(notice: CompanyNotice) {
  withdrawTarget.value = notice;
  withdrawOpen.value = true;
}

function confirmWithdraw() {
  if (!withdrawTarget.value) return;
  notices.value = notices.value.map((notice) =>
    notice.id === withdrawTarget.value?.id ? { ...notice, status: '已撤回' } : notice,
  );
  withdrawOpen.value = false;
  toast.success(`${withdrawTarget.value.code} 已在本地撤回`);
  withdrawTarget.value = null;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/notices"
    data-page-label="公司通知"
    data-page-module="行政管理"
    data-notices-page
    :data-result-count="filteredNotices.length"
    :data-total-count="notices.length"
    :data-published-count="publishedCount"
    :data-withdrawn-count="withdrawnCount"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>公司通知</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><Megaphone
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>公司通知</h1>
            <p>发布、查询、撤回公司级通知，支持指定范围与附件</p>
          </div>
        </div>
      </div>
      <Megaphone
        :class="$style.heroArt"
        :size="132"
        :stroke-width="1.3"
      />
    </header>

    <section :class="$style.metrics">
      <article>
        <Megaphone
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <span>通知总数</span><strong>{{ notices.length }}</strong
          ><small>全部历史记录</small>
        </div>
      </article>
      <article :class="$style.metricPublished">
        <CheckCircle2
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <span>已发布</span><strong>{{ publishedCount }}</strong
          ><small>正常生效中</small>
        </div>
      </article>
      <article :class="$style.metricDraft">
        <FileText
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <span>草稿</span><strong>{{ draftCount }}</strong
          ><small>未发布</small>
        </div>
      </article>
      <article :class="$style.metricWithdrawn">
        <Ban
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <span>已撤回</span><strong>{{ withdrawnCount }}</strong
          ><small>需关注</small>
        </div>
      </article>
    </section>

    <section :class="$style.toolbar">
      <UiSelect
        v-model="category"
        label="通知类别"
        :options="categoryOptions"
        size="sm"
      />
      <UiSelect
        v-model="scope"
        label="发布范围"
        :options="scopeOptions"
        size="sm"
      />
      <UiSelect
        v-model="status"
        label="通知状态"
        :options="statusOptions"
        size="sm"
      />
      <label :class="$style.search">
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <UiInput
          v-model="query"
          aria-label="搜索公司通知"
          placeholder="搜索主题/发布人/部门..."
        />
      </label>
      <UiButton
        variant="primary"
        @click="openCreate"
      >
        <template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8"
        /></template>
        发布通知
      </UiButton>
    </section>

    <NoticesTable
      :rows="filteredNotices"
      @view="openDetail"
      @withdraw="requestWithdraw"
    />
  </main>

  <UiDialog
    v-model:open="formOpen"
    title="发布公司通知"
    description="创建一条仅保存在本地复刻环境的公司通知"
    size="lg"
  >
    <div :class="$style.form">
      <UiFormField
        label="通知主题"
        required
        :class="$style.formWide"
      >
        <UiInput
          v-model="draft.title"
          aria-label="通知主题"
        />
      </UiFormField>
      <UiFormField label="类别">
        <UiSelect
          v-model="draft.category"
          label="通知类别"
          :options="formCategoryOptions"
        />
      </UiFormField>
      <UiFormField label="优先级">
        <UiSelect
          v-model="draft.priority"
          label="通知优先级"
          :options="priorityOptions"
        />
      </UiFormField>
      <UiFormField label="发布范围">
        <UiSelect
          v-model="draft.scope"
          label="通知发布范围"
          :options="formScopeOptions"
        />
      </UiFormField>
      <UiFormField
        label="正文摘要"
        required
        :class="$style.formWide"
      >
        <UiTextarea
          v-model="draft.summary"
          aria-label="通知正文摘要"
        />
      </UiFormField>
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="publishNotice"
        >发布通知</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.title || '通知详情'"
    :description="selected?.code"
    size="lg"
  >
    <div
      v-if="selected"
      :class="$style.detail"
    >
      <p>{{ selected.summary }}</p>
      <dl>
        <div>
          <dt>类别 / 优先级</dt>
          <dd>{{ selected.category }} / {{ selected.priority }}</dd>
        </div>
        <div>
          <dt>发布范围</dt>
          <dd>{{ selected.scope }} · {{ selected.scopeDetail }}</dd>
        </div>
        <div>
          <dt>发布部门 / 发布人</dt>
          <dd>{{ selected.department }} / {{ selected.publisher }}</dd>
        </div>
        <div>
          <dt>发布时间</dt>
          <dd>{{ selected.publishedAt }}</dd>
        </div>
        <div>
          <dt>阅读情况</dt>
          <dd>{{ selected.readCount }} / {{ selected.totalReaders }}</dd>
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
        >知道了</UiButton
      ></template
    >
  </UiDialog>

  <UiAlertDialog
    v-model:open="withdrawOpen"
    title="撤回公司通知"
    :description="withdrawTarget ? `确定撤回 ${withdrawTarget.code}《${withdrawTarget.title}》吗？` : ''"
    confirm-label="确认撤回"
    tone="warning"
    @confirm="confirmWithdraw"
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
  background: linear-gradient(105deg, #123776 0%, #0052cc 48%, #ff8b00 100%);
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
  @apply absolute right-[120px] top-[-6px] opacity-70;
  color: white;
  transform: rotate(-10deg);
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

.metricPublished {
  color: var(--ui-success) !important;
}

.metricDraft {
  color: var(--ui-warning) !important;
}

.metricWithdrawn {
  color: var(--ui-danger) !important;
}

.toolbar {
  @apply mb-3 flex items-center gap-2;
}

.search {
  @apply ml-auto flex h-8 w-[320px] items-center gap-2 rounded border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}

.search :global(input) {
  @apply min-w-0 flex-1 border-0 p-0 outline-none;
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

.detail > p {
  @apply rounded border p-4 text-[12px] leading-6;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detail dl {
  @apply mt-4 grid grid-cols-2 gap-x-6 gap-y-4;
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
  color: var(--ui-text);
}
</style>
