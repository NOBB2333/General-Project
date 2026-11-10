<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  BarChart3,
  CheckCircle2,
  ClipboardList,
  Plus,
  Search,
  Send,
  Sparkles,
  TrendingUp,
  TriangleAlert,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { surveysFixture } from '../model/surveys.data';
import type { SurveyDraft, SurveyRecord } from '../model/surveys.types';

type SurveyTab = '我的问卷' | '答卷数据' | '问卷模板' | '数据分析';
const rows = ref<SurveyRecord[]>(cloneReferenceRows(surveysFixture.rows));
const tab = ref<SurveyTab>('我的问卷');
const query = ref('');
const category = ref('全部类型');
const audience = ref('全部发布对象');
const status = ref('全部状态');
const formOpen = ref(false);
const draft = reactive<SurveyDraft>({ name: '', category: '员工满意度', audience: '全员' });
const categoryOptions: readonly UiSelectOption[] = [
  '全部类型',
  '客户满意度',
  '员工满意度',
  '售后服务评价',
  '供应商调查',
  '活动报名',
  '需求共创',
].map((value) => ({ value, label: value }));
const audienceOptions: readonly UiSelectOption[] = [
  '全部发布对象',
  '指定客户',
  '全员',
  '工单评价',
  '指定供应商',
  '公开链接',
  '重点客户',
].map((value) => ({ value, label: value }));
const statusOptions: readonly UiSelectOption[] = ['全部状态', '已发布', '草稿', '进行中', '已结束'].map((value) => ({
  value,
  label: value,
}));
const templates = [
  ['员工满意度', '匿名 · 部门分组 · 趋势对比'],
  ['客户满意度', '项目验收触发 · 自动关联CRM'],
  ['售后评价', '工单完成后自动派发链接'],
  ['供应商准入', '资质附件 · 生成档案草稿'],
  ['活动报名', '公开链接 + 二维码 · 进入线索池'],
  ['需求共创', '投票 + 上传截图 · 进入需求池'],
] as const;
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (item) =>
      (category.value === '全部类型' || item.category === category.value) &&
      (audience.value === '全部发布对象' || item.audience === audience.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle || `${item.code} ${item.name} ${item.owner}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openCreate(template?: string) {
  Object.assign(draft, {
    name: template ? `${template}问卷` : '',
    category: template || '员工满意度',
    audience: template === '活动报名' ? '公开链接' : '全员',
  });
  formOpen.value = true;
}
function saveSurvey() {
  if (!draft.name.trim()) return toast.warning('请填写问卷名称');
  rows.value.unshift({
    id: `survey-local-${rows.value.length + 1}`,
    code: `SRV-2026-${String(rows.value.length + 513).padStart(4, '0')}`,
    category: draft.category,
    name: draft.name,
    audience: draft.audience,
    responses: '0 / 0 · 0%',
    score: '—',
    owner: '张经理',
    date: '2026-09-17 → 2026-09-30',
    status: '草稿',
  });
  formOpen.value = false;
  toast.success('空白问卷已在本地创建');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/surveys"
    data-page-label="问卷调查"
    data-page-module="行政管理"
    data-surveys-page
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 问卷调查 · 调研中心</nav>
        <section>
          <span
            ><ClipboardList
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>问卷调查 / 调研中心</h1>
            <p>员工反馈 / 客户满意度 / 售后评价 / 供应商准入 / 活动报名 / 共创反馈 — 收集即闭环</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i>+58<small>NPS · 推荐值</small></i
        ><b /><em /><span />
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <Send
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>进行中问卷</span><strong>8</strong>
          <p>共 12 份问卷</p>
        </div>
      </article>
      <article>
        <CheckCircle2
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>本月回收</span><strong>612</strong>
          <p>环比 ↑ 18%</p>
        </div>
      </article>
      <article>
        <BarChart3
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>累计答卷</span><strong>746</strong>
          <p>全部已发布问卷</p>
        </div>
      </article>
      <article>
        <TriangleAlert
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>待处理反馈</span><strong>14</strong>
          <p>低分 / 待跟进答卷</p>
        </div>
      </article>
      <article>
        <TrendingUp
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>平均满意度</span><strong>4.00</strong>
          <p>1–5 分制 · 不含考试</p>
        </div>
      </article>
    </section>
    <section :class="$style.quick">
      <header>
        <div>
          <Sparkles
            :size="15"
            :stroke-width="1.8"
          /><strong>快捷创建</strong><span>· 选择常用场景，或使用 AI 一键生成问卷</span>
        </div>
        <div>
          <UiButton
            variant="secondary"
            size="sm"
            @click="toast.success('AI 问卷生成器已打开')"
            ><template #leading><Sparkles :size="14" /></template>AI 生成问卷</UiButton
          ><UiButton
            variant="primary"
            size="sm"
            @click="openCreate()"
            ><template #leading><Plus :size="14" /></template>新建空白问卷</UiButton
          >
        </div>
      </header>
      <div :class="$style.templateGrid">
        <button
          v-for="(item, index) in templates"
          :key="item[0]"
          :data-tone="index"
          @click="openCreate(item[0])"
        >
          <strong>{{ item[0] }}</strong
          ><span>{{ item[1] }}</span>
        </button>
      </div>
    </section>
    <section :class="$style.surface">
      <header :class="$style.toolbar">
        <nav>
          <button
            v-for="item in [
              ['我的问卷', '12'],
              ['答卷数据', '612'],
              ['问卷模板', '38'],
              ['数据分析', ''],
            ]"
            :key="item[0]"
            :data-active="tab === item[0]"
            @click="tab = item[0] as SurveyTab"
          >
            {{ item[0] }} <small v-if="item[1]">({{ item[1] }})</small>
          </button>
        </nav>
        <div :class="$style.filters">
          <UiSelect
            v-model="category"
            label="问卷类型"
            :options="categoryOptions"
            size="sm"
          /><UiSelect
            v-model="audience"
            label="发布对象"
            :options="audienceOptions"
            size="sm"
          /><UiSelect
            v-model="status"
            label="问卷状态"
            :options="statusOptions"
            size="sm"
          /><label :class="$style.search"
            ><Search :size="14" /><UiInput
              v-model="query"
              aria-label="搜索问卷"
              placeholder="问卷 / 编号 / 负责人 / 标签..."
          /></label>
        </div>
      </header>
      <div
        v-if="tab === '我的问卷'"
        :class="$style.tableWrap"
      >
        <table>
          <thead>
            <tr>
              <th>问卷编号 / 类型</th>
              <th>问卷名称</th>
              <th>发布对象</th>
              <th>回收 / 完成</th>
              <th>得分</th>
              <th>负责人</th>
              <th>时间</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in filtered"
              :key="row.id"
            >
              <td>
                <strong :class="$style.code">{{ row.code }}</strong
                ><small :data-category="row.category">{{ row.category }}</small>
              </td>
              <td>
                <b>{{ row.name }}</b
                ><small>▣ 12 题　◉ 项目交付节点</small>
              </td>
              <td>
                <b>{{ row.audience }}</b
                ><small>↪ 专属链接</small>
              </td>
              <td>
                <b>{{ row.responses }}</b
                ><i />
              </td>
              <td>
                <b>{{ row.score }}</b
                ><small :class="$style.lowScore">△ 2 条低分</small>
              </td>
              <td>
                <span :class="$style.avatar">{{ row.owner.slice(0, 1) }}</span
                ><b>{{ row.owner }}</b
                ><small>客户成功部</small>
              </td>
              <td>{{ row.date }}<small>最近提交 · 05-12 11:38</small></td>
              <td>
                <span
                  :class="$style.status"
                  :data-status="row.status"
                  >{{ row.status }}</span
                >
              </td>
              <td>
                <UiButton
                  size="xs"
                  variant="link"
                  @click="toast.success('已打开问卷详情')"
                  >查看</UiButton
                >
              </td>
            </tr>
          </tbody>
        </table>
        <p
          v-if="!filtered.length"
          :class="$style.empty"
        >
          当前条件下没有问卷
        </p>
      </div>
      <div
        v-else
        :class="$style.alternate"
      >
        <component
          :is="tab === '数据分析' ? BarChart3 : ClipboardList"
          :size="30"
        />
        <h2>{{ tab }}</h2>
        <p>
          {{
            tab === '答卷数据'
              ? '本月已回收 612 份答卷'
              : tab === '问卷模板'
                ? '38 个可复用问卷模板'
                : '平均满意度 4.00 分'
          }}
        </p>
      </div>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新建空白问卷"
    description="问卷只写入本地复刻站"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="问卷名称"
        required
        ><UiInput v-model="draft.name" /></UiFormField
      ><UiFormField label="问卷类型"
        ><UiSelect
          v-model="draft.category"
          label="问卷类型"
          :options="categoryOptions.slice(1)" /></UiFormField
      ><UiFormField label="发布对象"
        ><UiSelect
          v-model="draft.audience"
          label="发布对象"
          :options="audienceOptions.slice(1)"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveSurvey"
        >创建问卷</UiButton
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
  background: linear-gradient(
    90deg,
    #102a50 0%,
    #102a50 14%,
    #a4acb5 14%,
    #9ba3ab 36%,
    #273169 37%,
    #313a7e 59%,
    #0ea5bd 100%
  );
}
.hero > div:first-child {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.78);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.28);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.8);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[58%];
  background: linear-gradient(150deg, transparent, rgba(81, 68, 173, 0.72));
}
.heroArt i {
  @apply absolute right-[18%] top-4 flex h-24 w-24 flex-col items-center justify-center rounded-full border-[7px] text-[21px] not-italic;
  border-color: #36b37e;
  background: #263c86;
}
.heroArt small {
  @apply mt-1 block text-[8px] font-normal;
}
.heroArt b,
.heroArt em,
.heroArt span {
  @apply absolute bottom-0 w-7;
  background: #36b37e;
}
.heroArt b {
  @apply left-[36%] h-20;
}
.heroArt em {
  @apply left-[46%] h-14;
  background: #ffab00;
}
.heroArt span {
  @apply left-[56%] h-24;
  background: #0052cc;
}
.metrics {
  @apply mb-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex min-h-[96px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > svg {
  color: #0052cc;
}
.metrics article:nth-child(2) > svg {
  color: #00875a;
}
.metrics article:nth-child(3) > svg {
  color: #6554c0;
}
.metrics article:nth-child(4) > svg {
  color: #de350b;
}
.metrics article:nth-child(5) > svg {
  color: #ff8b00;
}
.metrics span,
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[20px];
}
.metrics article:nth-child(1) strong {
  color: #0052cc;
}
.metrics article:nth-child(2) strong {
  color: #00875a;
}
.metrics article:nth-child(3) strong {
  color: #6554c0;
}
.metrics article:nth-child(4) strong {
  color: #de350b;
}
.metrics article:nth-child(5) strong {
  color: #ff8b00;
}
.metrics p {
  @apply mt-1;
}
.quick {
  @apply mb-4 rounded border bg-white p-3;
  border-color: var(--ui-border);
}
.quick header {
  @apply mb-3 flex items-center justify-between;
}
.quick header > div {
  @apply flex items-center gap-2;
}
.quick header strong {
  @apply text-[12px];
}
.quick header span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.templateGrid {
  @apply grid grid-cols-6 gap-3;
}
.templateGrid button {
  @apply min-h-[59px] rounded px-3 text-left text-white;
  background: #0052cc;
}
.templateGrid button[data-tone='1'] {
  background: #00875a;
}
.templateGrid button[data-tone='2'] {
  background: #ff8b00;
}
.templateGrid button[data-tone='3'] {
  background: #bf5b00;
}
.templateGrid button[data-tone='4'] {
  background: #6554c0;
}
.templateGrid button[data-tone='5'] {
  background: #de350b;
}
.templateGrid strong {
  @apply block text-[11px];
}
.templateGrid span {
  @apply mt-2 block truncate text-[9px];
  color: rgba(255, 255, 255, 0.82);
}
.surface {
  @apply rounded border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex h-12 items-center border-b px-3;
  border-color: var(--ui-border);
}
.toolbar nav {
  @apply flex h-full;
}
.toolbar nav button {
  @apply h-full border-b-2 border-transparent px-3 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.toolbar nav button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.toolbar nav small {
  @apply text-[9px];
}
.filters {
  @apply ml-auto flex items-center gap-2;
}
.filters > div {
  @apply w-[112px];
}
.search {
  @apply flex h-8 w-[220px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.tableWrap {
  @apply overflow-x-auto;
}
.tableWrap table {
  @apply w-full min-w-[1380px] border-collapse text-left text-[10px];
}
.tableWrap th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.tableWrap td {
  @apply h-[72px] border-b px-4 whitespace-nowrap;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableWrap td b {
  @apply font-semibold;
  color: var(--ui-text);
}
.tableWrap td small {
  @apply mt-1 block text-[8px];
  color: var(--ui-text-subtle);
}
.code {
  color: var(--app-primary) !important;
}
.tableWrap td:first-child small {
  @apply mt-1 block w-max rounded px-2 py-1;
  color: #00875a;
  background: #e3fcef;
}
.tableWrap td:nth-child(4) i {
  @apply mt-2 block h-1 w-16 rounded;
  background: linear-gradient(to right, #36b37e 68%, #ebecf0 68%);
}
.lowScore {
  color: #de350b !important;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[9px] font-semibold text-white;
  background: #0052cc;
}
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='草稿'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='进行中'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已结束'] {
  color: #6554c0;
  background: #eae6ff;
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
.alternate {
  @apply flex min-h-[330px] flex-col items-center justify-center;
  color: var(--app-primary);
}
.alternate h2 {
  @apply mt-3 text-[16px];
  color: var(--ui-text);
}
.alternate p {
  @apply mt-2 text-[11px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
