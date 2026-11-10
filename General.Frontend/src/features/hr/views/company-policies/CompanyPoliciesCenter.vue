<script setup lang="ts">
import { computed, ref } from 'vue';
import { CheckCircle2, ChevronRight, Clock3, Search, Star } from 'lucide-vue-next';
import { UiInput } from '@/components/ui';
import { policyChanges } from '../../model/company-policies.data';
import type { CompanyPolicyRecord } from '../../model/company-policies.types';
const props = defineProps<{ rows: readonly CompanyPolicyRecord[] }>();
const emit = defineEmits<{ open: [row: CompanyPolicyRecord] }>();
const query = ref('');
const scope = ref('全部');
const category = ref('全部分类');
const visible = computed(() => {
  const q = query.value.trim().toLowerCase();
  return props.rows.filter(
    (row) =>
      (scope.value === '全部' ||
        (scope.value === '待我确认' && row.status === '待确认') ||
        (scope.value === '最近发布' && row.status === '生效中') ||
        (scope.value === '草稿/审批' && row.status === '草稿')) &&
      (category.value === '全部分类' || row.category === category.value) &&
      (!q || `${row.title}${row.code}${row.category}`.toLowerCase().includes(q)),
  );
});
const scopes = ['全部', '待我确认', '最近发布', '草稿/审批'] as const;
const scopeCounts: Record<string, number> = { 全部: 6, 待我确认: 3, 最近发布: 5, '草稿/审批': 1 };
const categories = ['全部分类', '人事制度', '财务制度', '安全管理', '保密制度', '技术研发'] as const;
</script>
<template>
  <section :class="$style.layout">
    <div>
      <div :class="$style.filters">
        <div :class="$style.scope">
          <button
            v-for="item in scopes"
            :key="item"
            type="button"
            :class="{ [$style.active]: scope === item }"
            @click="scope = item"
          >
            {{ item }} <b>{{ scopeCounts[item] }}</b>
          </button>
        </div>
        <div :class="$style.search">
          <Search
            :size="13"
            :stroke-width="1.8"
          /><UiInput
            v-model="query"
            placeholder="搜索制度名称/编号/标签..."
            aria-label="搜索公司制度"
          />
        </div>
        <div :class="$style.categories">
          <button
            v-for="item in categories"
            :key="item"
            type="button"
            :class="{ [$style.activeCategory]: category === item }"
            @click="category = item"
          >
            {{ item }}
          </button>
        </div>
      </div>
      <div :class="$style.cards">
        <article
          v-for="row in visible"
          :key="row.id"
        >
          <span :class="$style.icon"
            ><Star
              v-if="row.id === 'cp1'"
              :size="12"
              :stroke-width="1.8" /><CheckCircle2
              v-else
              :size="15"
              :stroke-width="1.8"
          /></span>
          <div :class="$style.copy">
            <p>
              <strong>{{ row.title }}</strong
              ><b :data-status="row.status">{{ row.status }}</b
              ><small>{{ row.version }}</small
              ><i>{{ row.type }}</i>
            </p>
            <p>{{ row.summary }}</p>
            <dl>
              <div>
                <dt>编号 / 适用</dt>
                <dd>{{ row.code }}<small>全体员工</small></dd>
              </div>
              <div>
                <dt>条款</dt>
                <dd>
                  {{ row.clauses }} 条 · <em>{{ row.important }} 重要</em>
                </dd>
              </div>
              <div>
                <dt>我的进度</dt>
                <dd>
                  <span><i data-progress /></span>{{ row.myProgress }}%<small>{{ row.signed }}</small>
                </dd>
              </div>
              <div>
                <dt>全员覆盖</dt>
                <dd>
                  <span><i data-coverage /></span>{{ row.coverage }}%<small
                    >{{ 177 - row.overdue }}/187 · <em>{{ row.overdue }} 逾期</em></small
                  >
                </dd>
              </div>
            </dl>
          </div>
          <button
            type="button"
            :aria-label="`查看${row.title}`"
            @click="emit('open', row)"
          >
            <ChevronRight
              :size="16"
              :stroke-width="1.8"
            />
          </button>
        </article>
        <p
          v-if="visible.length === 0"
          :class="$style.empty"
        >
          没有匹配的公司制度
        </p>
      </div>
    </div>
    <aside>
      <header>
        <Clock3
          :size="14"
          :stroke-width="1.8"
        /><strong>最新变更动态</strong><button type="button">查看全部</button>
      </header>
      <article
        v-for="item in policyChanges"
        :key="item.id"
        :data-tone="item.tone"
      >
        <p>
          <i /><strong>{{ item.title }}</strong
          ><b>{{ item.version }}</b
          ><span v-if="item.tag">{{ item.tag }}</span>
        </p>
        <p>{{ item.note }}</p>
        <small>{{ item.date }} · {{ item.owner }}</small>
      </article>
    </aside>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.layout {
  @apply mt-4 grid grid-cols-[2fr_1fr] gap-3;
}
.filters {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.scope {
  @apply flex h-11 items-center border-b px-3;
  border-color: var(--ui-border-subtle);
}
.scope button {
  @apply h-9 border-0 bg-transparent px-3 text-[10px] font-semibold;
  color: var(--ui-text-muted);
}
.scope button.active {
  @apply rounded-t bg-[#deebff];
  color: var(--app-primary);
}
.scope b {
  @apply ml-1 rounded-full px-1.5 py-0.5 text-[8px];
  background: var(--ui-surface-hover);
}
.search {
  @apply relative float-right mr-3 mt-[-38px] w-[220px];
}
.search > svg {
  @apply absolute left-3 top-[10px] z-10;
  color: var(--ui-text-subtle);
}
.search input {
  @apply pl-8;
}
.categories {
  @apply flex h-11 items-center gap-2 px-4;
}
.categories button {
  @apply rounded-full border-0 px-3 py-1.5 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
.categories button.activeCategory {
  @apply text-white;
  background: var(--ui-text);
}
.cards {
  @apply mt-3 space-y-3;
}
.cards article {
  @apply grid min-h-[130px] grid-cols-[38px_1fr_24px] gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.icon {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.copy > p {
  @apply m-0 text-[10px];
  color: var(--ui-text-muted);
}
.copy > p:first-child {
  @apply flex items-center gap-2;
  color: var(--ui-text);
}
.copy > p strong {
  @apply text-[12px];
}
.copy > p b,
.copy > p i {
  @apply rounded px-2 py-0.5 text-[8px] not-italic;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.copy > p b[data-status='待确认'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.copy > p b[data-status='草稿'] {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.copy > p small {
  color: var(--ui-text-subtle);
}
.copy > p + p {
  @apply mt-2;
}
.copy dl {
  @apply m-0 mt-3 grid grid-cols-4 gap-4;
}
.copy dt {
  @apply text-[8px];
  color: var(--ui-text-subtle);
}
.copy dd {
  @apply m-0 mt-1 text-[10px] font-semibold;
}
.copy dd > small {
  @apply mt-1 block font-normal;
  color: var(--ui-text-subtle);
}
.copy em {
  @apply not-italic;
  color: var(--ui-danger);
}
.copy dd > span {
  @apply mr-2 inline-block h-1.5 w-[80px] rounded-full;
  vertical-align: middle;
  background: var(--ui-surface-hover);
}
.copy dd > span i {
  @apply block h-full w-full rounded-full;
  background: var(--ui-success);
}
.cards article > button {
  @apply mt-2 border-0 bg-transparent p-1;
  color: var(--ui-text-subtle);
}
.empty {
  @apply m-0 rounded border bg-white py-16 text-center text-[10px];
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.layout > aside {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.layout > aside header {
  @apply flex h-[42px] items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.layout > aside header strong {
  @apply text-[11px];
}
.layout > aside header button {
  @apply ml-auto border-0 bg-transparent text-[9px];
  color: var(--app-primary);
}
.layout > aside article {
  @apply border-b p-4;
  border-color: var(--ui-border-subtle);
}
.layout > aside article > p {
  @apply m-0 text-[9px] leading-5;
  color: var(--ui-text-muted);
}
.layout > aside article > p:first-child {
  @apply flex items-center gap-2;
  color: var(--ui-text);
}
.layout > aside article i {
  @apply h-1.5 w-1.5 rounded-full;
  background: var(--ui-success);
}
.layout > aside article[data-tone='danger'] i {
  background: var(--ui-danger);
}
.layout > aside article b {
  color: var(--app-primary);
}
.layout > aside article span {
  @apply rounded px-1.5 py-0.5 text-[8px];
  color: #6554c0;
  background: #eae6ff;
}
.layout > aside small {
  @apply mt-2 block text-[8px];
  color: var(--ui-text-subtle);
}
</style>
