<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { BookOpen, GitBranch, Plus, Search, Sparkles } from 'lucide-vue-next';
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
import { serviceKbReferenceFixture } from '../model/service-kb.data';
import type { KnowledgeArticle, KnowledgeDraft } from '../model/service-kb.types';

const articles = ref<KnowledgeArticle[]>(cloneReferenceRows(serviceKbReferenceFixture.rows));
const activeId = ref('kb006');
const detailTab = ref('overview');
const query = ref('');
const category = ref('全部');
const productLine = ref('全部产品线');
const status = ref('全部状态');
const aiQuestion = ref('');
const aiAnswer = ref('');
const formOpen = ref(false);
const draft = reactive<KnowledgeDraft>({
  title: '',
  category: '通讯问题',
  productLine: '通用',
  phenomenon: '',
  solution: '',
});
const categories = ['全部', '通讯问题', '电气问题', 'HMI问题', '巡检保养', '收费说明', '客户沟通话术'];
const productOptions: readonly UiSelectOption[] = [
  { value: '全部产品线', label: '全部产品线' },
  { value: '通用', label: '通用' },
  { value: 'PLC / 控制器', label: 'PLC / 控制器' },
  { value: 'DCS / 仪表', label: 'DCS / 仪表' },
  { value: 'HMI / 触摸屏', label: 'HMI / 触摸屏' },
  { value: '变频器', label: '变频器' },
];
const statusOptions: readonly UiSelectOption[] = [
  { value: '全部状态', label: '全部状态' },
  { value: '已发布', label: '已发布' },
  { value: '草稿', label: '草稿' },
];
const categoryOptions: readonly UiSelectOption[] = categories
  .filter((item) => item !== '全部')
  .map((value) => ({ value, label: value }));
const detailTabs = [
  { value: 'overview', label: '概览' },
  { value: 'steps', label: '排查步骤' },
  { value: 'parts', label: '备件清单' },
  { value: 'tickets', label: '关联工单' },
  { value: 'history', label: '编辑历史 (1)' },
] as const;
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return articles.value.filter(
    (row) =>
      (!needle ||
        `${row.code} ${row.title} ${row.subtitle} ${row.tags.join(' ')}`.toLocaleLowerCase('zh-CN').includes(needle)) &&
      (category.value === '全部' || row.category === category.value) &&
      (productLine.value === '全部产品线' || row.productLine === productLine.value) &&
      (status.value === '全部状态' || row.status === status.value),
  );
});
const activeArticle = computed(
  () => filtered.value.find((row) => row.id === activeId.value) ?? filtered.value[0] ?? null,
);

watch(filtered, (rows) => {
  if (!rows.some((row) => row.id === activeId.value)) activeId.value = rows[0]?.id ?? '';
});

function askAi() {
  const question = aiQuestion.value.trim();
  if (!question) {
    toast.warning('请先描述故障');
    return;
  }
  const match =
    articles.value.find((row) =>
      question.includes('PLC') ? row.code === 'KB-002' : question.includes('过热') ? row.code === 'KB-003' : false,
    ) ?? articles.value[0];
  aiAnswer.value = `推荐 ${match.code}《${match.title}》：${match.steps.slice(0, 2).join('；')}。`;
  activeId.value = match.id;
}
function createArticle() {
  if (!draft.title.trim() || !draft.phenomenon.trim() || !draft.solution.trim()) {
    toast.warning('请填写标题、问题现象和解决步骤');
    return;
  }
  const id = `kb-local-${articles.value.length + 1}`;
  articles.value.unshift({
    id,
    code: `KB-LOCAL-${String(articles.value.length + 1).padStart(3, '0')}`,
    title: draft.title.trim(),
    subtitle: draft.phenomenon.trim(),
    category: draft.category,
    productLine: draft.productLine,
    status: '草稿',
    tags: ['本地草稿'],
    views: 0,
    references: 0,
    updatedAt: '2026-09-16',
    phenomenon: draft.phenomenon.trim(),
    cause: '待补充',
    steps: draft.solution.split('\n').filter(Boolean),
    parts: [],
    tickets: [],
  });
  activeId.value = id;
  formOpen.value = false;
  Object.assign(draft, {
    title: '',
    category: '通讯问题',
    productLine: '通用',
    phenomenon: '',
    solution: '',
  });
  toast.success('知识条目已保存为本地草稿');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/service/service-kb"
    data-page-label="服务知识库"
    data-page-module="服务管理"
    data-service-kb-page
    :data-result-count="filtered.length"
    :data-active-article="activeId"
    :data-detail-tab="detailTab"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>服务管理</span><b>›</b><strong>服务知识库</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><BookOpen
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>服务知识库</h1>
            <p>把工单里「遇到过 → 解决了」沉淀下来 · 客服 / 工程师查得快、用得上、能复用</p>
          </div>
        </div>
      </div>
      <BookOpen
        :class="$style.heroArt"
        :size="126"
        :stroke-width="1.8"
      />
      <div :class="$style.heroActions">
        <UiButton
          variant="ghost"
          size="sm"
          @click="toast.info('已进入本地工单沉淀流程')"
          ><template #leading
            ><GitBranch
              :size="14"
              :stroke-width="1.8" /></template
          >从工单沉淀</UiButton
        ><UiButton
          variant="secondary"
          size="sm"
          @click="formOpen = true"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建条目</UiButton
        >
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <span>知识库条目</span><strong>{{ articles.length }}</strong
        ><small
          >已发布 {{ articles.filter((row) => row.status === '已发布').length }} · 草稿
          {{ articles.filter((row) => row.status === '草稿').length }}</small
        >
      </article>
      <article :class="$style.metricSuccess">
        <span>本月新增</span><strong>0</strong><small>近 30 日沉淀新条目数</small>
      </article>
      <article :class="$style.metricReference">
        <span>本月被引用</span><strong>0</strong><small>应用到工单 / 生成回复</small>
      </article>
      <article>
        <span>草稿待发布</span><strong>{{ articles.filter((row) => row.status === '草稿').length }}</strong
        ><small>无待审</small>
      </article>
    </section>
    <section :class="$style.ai">
      <strong
        ><Sparkles
          :size="16"
          :stroke-width="1.8"
        />AI 智能问答 · 用一句话描述故障，AI 按关键词命中度排序推荐</strong
      >
      <div>
        <UiInput
          v-model="aiQuestion"
          aria-label="AI故障提问"
          placeholder="例：S7-1200 PLC 上电 RUN 灯不亮怎么排查？"
          @keyup.enter="askAi"
        /><UiButton
          variant="primary"
          @click="askAi"
          ><template #leading
            ><Sparkles
              :size="14"
              :stroke-width="1.8" /></template
          >AI 解答</UiButton
        >
      </div>
      <p v-if="aiAnswer">{{ aiAnswer }}</p>
    </section>
    <section :class="$style.filters">
      <div>
        <span>分类</span
        ><button
          v-for="item in categories"
          :key="item"
          type="button"
          :class="[$style.categoryButton, category === item && $style.categoryActive]"
          @click="category = item"
        >
          {{ item }}
        </button>
      </div>
      <div>
        <UiSelect
          v-model="productLine"
          label="产品线"
          :options="productOptions"
          size="sm"
        /><UiSelect
          v-model="status"
          label="状态"
          :options="statusOptions"
          size="sm"
        /><label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索知识条目"
            placeholder="搜索标题 / 现象 / 步骤 / 标签…" /></label
        ><span>共 {{ filtered.length }} 条</span>
      </div>
    </section>
    <section :class="$style.workspace">
      <aside>
        <button
          v-for="article in filtered"
          :key="article.id"
          type="button"
          :class="[$style.articleCard, activeId === article.id && $style.articleActive]"
          @click="activeId = article.id"
        >
          <div>
            <strong>{{ article.code }}</strong
            ><span>{{ article.category }}</span
            ><em>{{ article.productLine }}</em
            ><i>{{ article.status }}</i>
          </div>
          <h3>{{ article.title }}</h3>
          <p>{{ article.subtitle }}</p>
          <small
            >◉ {{ article.views }}　↻ 引用 {{ article.references }} <time>{{ article.updatedAt }}</time></small
          >
        </button>
      </aside>
      <article
        v-if="activeArticle"
        :class="$style.detail"
      >
        <header>
          <div>
            <strong>{{ activeArticle.code }}</strong
            ><span>{{ activeArticle.category }}</span
            ><em>{{ activeArticle.productLine }}</em
            ><i>{{ activeArticle.status }}</i>
          </div>
          <h2>{{ activeArticle.title }}</h2>
          <p>
            适用：{{ activeArticle.productLine }}　更新 {{ activeArticle.updatedAt }} · 客服-小林　◉ 浏览
            {{ activeArticle.views }}　↻ 引用 {{ activeArticle.references }}
          </p>
          <div>
            <span
              v-for="tag in activeArticle.tags"
              :key="tag"
              >#{{ tag }}</span
            >
          </div>
        </header>
        <UiTabs v-model="detailTab">
          <UiTabsList
            label="知识条目详情"
            :class="$style.detailTabs"
          >
            <UiTabsTrigger
              v-for="tab in detailTabs"
              :key="tab.value"
              :class="$style.detailTab"
              :value="tab.value"
            >
              {{ tab.label }}
            </UiTabsTrigger>
          </UiTabsList>
          <div :class="$style.detailBody">
            <template v-if="detailTab === 'overview'"
              ><h3>问题现象</h3>
              <p>{{ activeArticle.phenomenon }}</p>
              <h3>可能原因</h3>
              <p>{{ activeArticle.cause }}</p></template
            >
            <ol v-else-if="detailTab === 'steps'">
              <li
                v-for="step in activeArticle.steps"
                :key="step"
              >
                {{ step }}
              </li>
            </ol>
            <ul v-else-if="detailTab === 'parts'">
              <li
                v-for="part in activeArticle.parts"
                :key="part"
              >
                {{ part }}
              </li>
              <li v-if="!activeArticle.parts.length">无需备件</li>
            </ul>
            <ul v-else-if="detailTab === 'tickets'">
              <li
                v-for="ticket in activeArticle.tickets"
                :key="ticket"
              >
                {{ ticket }}
              </li>
              <li v-if="!activeArticle.tickets.length">暂无关联工单</li>
            </ul>
            <p v-else>2026-05-10　客服-小林发布当前版本</p>
          </div>
        </UiTabs>
      </article>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新建知识条目"
    description="在本地创建知识库草稿"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="标题"
        required
        ><UiInput
          v-model="draft.title"
          aria-label="条目标题" /></UiFormField
      ><UiFormField label="分类"
        ><UiSelect
          v-model="draft.category"
          label="分类"
          :options="categoryOptions" /></UiFormField
      ><UiFormField label="产品线"
        ><UiSelect
          v-model="draft.productLine"
          label="产品线"
          :options="productOptions.slice(1)" /></UiFormField
      ><UiFormField
        label="问题现象"
        required
        ><UiTextarea
          v-model="draft.phenomenon"
          aria-label="问题现象" /></UiFormField
      ><UiFormField
        label="解决步骤"
        required
        ><UiTextarea
          v-model="draft.solution"
          aria-label="解决步骤"
          placeholder="每行一个步骤"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="createArticle"
        >保存草稿</UiButton
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
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #102a5c 0%, #0747a6 56%, #00b8d9 100%);
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
  color: rgb(255 255 255 / 88%);
}
.heroArt {
  @apply absolute right-[270px] top-[-8px] opacity-30;
}
.heroActions {
  @apply absolute right-6 top-[59px] flex gap-2;
}
.heroActions > :first-child {
  color: white;
  background: rgb(255 255 255 / 13%);
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply min-h-[96px] rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.metrics span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1.5 block text-[24px];
  color: #0052cc;
}
.metrics small {
  @apply mt-1 block text-[11px];
  color: #97a0af;
}
.metricSuccess strong {
  color: #00875a;
}
.metricReference strong {
  color: #6554c0;
}
.ai {
  @apply mb-4 rounded border p-4;
  color: #5243aa;
  border-color: #e9e6ff;
  background: #faf9ff;
}
.ai > strong {
  @apply flex items-center gap-1.5 text-[12px];
}
.ai > div {
  @apply mt-2 flex gap-2;
}
.ai > div > :first-child {
  @apply flex-1;
}
.ai p {
  @apply mt-2 text-[11px];
  color: var(--ui-text-muted);
}
.filters {
  @apply mb-3 rounded border bg-white p-3;
  border-color: var(--ui-border);
}
.filters > div {
  @apply flex items-center gap-2;
}
.filters > div + div {
  @apply mt-2;
}
.filters > div > span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.categoryButton {
  @apply rounded-full border px-3 py-1.5 text-[11px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.categoryActive {
  @apply border-transparent text-white;
  background: var(--app-primary);
}
.search {
  @apply ml-auto flex h-8 w-[325px] items-center gap-2 rounded border px-3;
  color: #97a0af;
  border-color: var(--ui-border);
}
.search :global(input) {
  @apply min-w-0 flex-1 border-0 p-0 outline-none;
}
.workspace {
  @apply grid min-h-[390px] grid-cols-[330px_1fr] gap-3;
}
.workspace aside {
  @apply space-y-2;
}
.articleCard {
  @apply block w-full rounded border bg-white p-3 text-left;
  border-color: var(--ui-border);
}
.articleActive {
  border-color: #0065ff;
  background: #f4f8ff;
}
.articleCard > div,
.detail header > div {
  @apply flex items-center gap-1.5 text-[9px];
}
.articleCard > div strong,
.detail header > div strong {
  color: #0052cc;
}
.articleCard > div span,
.articleCard > div em,
.articleCard > div i,
.detail header span,
.detail header em,
.detail header i {
  @apply rounded px-1.5 py-0.5 not-italic;
  color: #0052cc;
  background: #deebff;
}
.articleCard > div i,
.detail header i {
  color: #00875a;
  background: #e3fcef;
}
.articleCard h3 {
  @apply mt-2 text-[12px] font-semibold;
}
.articleCard p {
  @apply mt-1 truncate text-[10px];
  color: var(--ui-text-subtle);
}
.articleCard small {
  @apply mt-2 block text-[9px];
  color: #6b778c;
}
.articleCard time {
  @apply float-right;
}
.detail {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.detail header {
  @apply p-5;
}
.detail header h2 {
  @apply mt-3 text-[18px] font-semibold;
}
.detail header p {
  @apply mt-1.5 text-[10px];
  color: var(--ui-text-subtle);
}
.detail header > div:last-child {
  @apply mt-2;
}
.detailTabs {
  @apply flex border-y px-2;
  border-color: var(--ui-border);
}
.detailTab {
  @apply h-11 border-b-2 border-transparent px-4 text-[11px];
  color: var(--ui-text-muted);
  background: transparent;
}
.detailTab[data-state='active'] {
  @apply font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}
.detailBody {
  @apply p-5 text-[12px];
}
.detailBody h3 {
  @apply mt-3 font-semibold first:mt-0;
}
.detailBody p,
.detailBody li {
  @apply mt-2 leading-5;
  color: var(--ui-text-muted);
}
.detailBody ol,
.detailBody ul {
  @apply list-inside list-decimal;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.form > :nth-last-child(-n + 2) {
  @apply col-span-2;
}
</style>
