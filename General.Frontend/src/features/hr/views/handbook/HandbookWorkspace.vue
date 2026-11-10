<script setup lang="ts">
import { BookOpen, Building2, CheckCircle2, Search, Star } from 'lucide-vue-next';
import { UiInput } from '@/components/ui';
import type { HandbookChapter } from '../../model/handbook.types';

defineProps<{ query: string; chapters: readonly HandbookChapter[]; activeChapter: HandbookChapter }>();
const emit = defineEmits<{ 'update:query': [value: string]; select: [id: string] }>();
</script>

<template>
  <section :class="$style.workspace">
    <aside :class="$style.navigation">
      <div :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><UiInput
          :model-value="query"
          placeholder="搜索条款..."
          aria-label="搜索条款"
          @update:model-value="emit('update:query', $event)"
        />
      </div>
      <nav aria-label="员工手册章节">
        <section
          v-for="chapter in chapters"
          :key="chapter.id"
        >
          <button
            type="button"
            :class="[$style.chapterButton, { [$style.active]: chapter.id === activeChapter.id }]"
            @click="emit('select', chapter.id)"
          >
            <Building2
              :size="14"
              :stroke-width="1.8"
            /><strong>{{ chapter.title }}</strong
            ><span>{{ chapter.clauses.length }}/{{ chapter.clauses.length }}</span>
          </button>
          <button
            v-for="clause in chapter.clauses"
            :key="clause.id"
            type="button"
            :class="$style.clauseButton"
            @click="emit('select', chapter.id)"
          >
            <Star
              v-if="clause.important"
              :size="10"
              :stroke-width="1.8"
            /><span>{{ clause.title }}</span
            ><CheckCircle2
              :size="10"
              :stroke-width="1.8"
            />
          </button>
        </section>
        <p v-if="chapters.length === 0">没有匹配的手册条款</p>
      </nav>
    </aside>
    <article :class="$style.content">
      <header>
        <span
          ><BookOpen
            :size="20"
            :stroke-width="1.8"
        /></span>
        <div>
          <small>第 {{ activeChapter.number }} 章</small>
          <h2>{{ activeChapter.title }}</h2>
        </div>
        <p>
          {{ activeChapter.clauses.length }} 条款 · 更新 {{ activeChapter.updatedAt
          }}<strong>已读 {{ activeChapter.clauses.length }}/{{ activeChapter.clauses.length }}</strong>
        </p>
      </header>
      <section
        v-for="clause in activeChapter.clauses"
        :key="clause.id"
        :class="$style.clause"
      >
        <div>
          <span>{{ clause.number }}</span>
          <h3>{{ clause.title }}</h3>
          <small
            ><CheckCircle2
              :size="10"
              :stroke-width="1.8"
            />已读</small
          >
        </div>
        <p>{{ clause.content }}</p>
      </section>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 grid grid-cols-[320px_1fr] gap-4;
}
.navigation,
.content {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.search {
  @apply relative border-b p-3;
  border-color: var(--ui-border-subtle);
}
.search > svg {
  @apply absolute left-6 top-[24px] z-10;
  color: var(--ui-text-subtle);
}
.search input {
  @apply pl-9;
}
.navigation nav {
  @apply max-h-[520px] overflow-y-auto p-3;
}
.navigation nav section + section {
  @apply mt-2;
}
.chapterButton,
.clauseButton {
  @apply flex w-full items-center border-0 bg-transparent text-left;
  color: var(--ui-text-muted);
}
.chapterButton {
  @apply h-8 gap-2 rounded px-2 text-[10px];
}
.chapterButton:hover,
.chapterButton.active {
  background: var(--ui-surface-hover);
  color: var(--app-primary);
}
.chapterButton strong {
  @apply flex-1;
  color: var(--ui-text);
}
.chapterButton span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.clauseButton {
  @apply h-7 gap-2 pl-7 pr-2 text-[9px];
}
.clauseButton > span {
  @apply flex-1;
}
.clauseButton > svg:first-child {
  color: var(--ui-warning);
  fill: currentColor;
}
.clauseButton > svg:last-child {
  color: var(--ui-success);
}
.navigation nav > p {
  @apply m-0 px-3 py-8 text-center text-[10px];
  color: var(--ui-text-subtle);
}
.content > header {
  @apply flex h-[76px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border);
  background: linear-gradient(90deg, var(--app-primary-soft), #fff);
}
.content > header > span {
  @apply flex h-10 w-10 items-center justify-center rounded;
  color: var(--app-primary);
  background: #deebff;
}
.content header small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.content header h2 {
  @apply m-0 mt-1 text-[18px];
}
.content header > p {
  @apply ml-auto text-right text-[9px];
  color: var(--ui-text-muted);
}
.content header > p strong {
  @apply mt-1 block;
  color: var(--app-primary);
}
.clause {
  @apply min-h-[124px] border-b px-6 py-5;
  border-color: var(--ui-border-subtle);
}
.clause:last-child {
  @apply border-b-0;
}
.clause > div {
  @apply flex items-center gap-3;
}
.clause > div > span {
  @apply text-[12px] font-semibold;
  color: var(--ui-text-subtle);
}
.clause h3 {
  @apply m-0 text-[14px];
}
.clause small {
  @apply flex items-center gap-1 rounded px-2 py-0.5 text-[8px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.clause > p {
  @apply m-0 mt-4 pl-6 text-[11px] leading-7;
  color: var(--ui-text-muted);
}
</style>
