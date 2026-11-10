<script setup lang="ts">
import { BarChart3, BookOpen, FileQuestion, FolderKanban, Settings2, Trophy } from 'lucide-vue-next';
import type { TrainingCourse } from '../../model/training.types';
defineProps<{ section: string; courses: readonly TrainingCourse[] }>();
const iconBySection = {
  课程库: BookOpen,
  培训项目: FolderKanban,
  题库管理: FileQuestion,
  试卷管理: FileQuestion,
  考试中心: Trophy,
  数据分析: BarChart3,
  培训配置: Settings2,
} as const;
</script>
<template>
  <section :class="$style.surface">
    <header>
      <component
        :is="iconBySection[section as keyof typeof iconBySection] || BookOpen"
        :size="22"
        :stroke-width="1.8"
      />
      <div>
        <h2>{{ section }}</h2>
        <p>培训与考试中心 · {{ section }}业务工作区</p>
      </div>
    </header>
    <div
      v-if="section === '课程库'"
      :class="$style.courses"
    >
      <article
        v-for="course in courses"
        :key="course.id"
      >
        <BookOpen
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ course.title }}</strong>
          <p>{{ course.category }} · {{ course.hours }} 学时 · {{ course.learners }} 人学习</p>
        </div>
        <span>{{ course.status }}</span>
      </article>
    </div>
    <div
      v-else-if="section === '数据分析'"
      :class="$style.metrics"
    >
      <article><span>累计学习人次</span><strong>1,246</strong></article>
      <article><span>课程完成率</span><strong>86%</strong></article>
      <article><span>考试合格率</span><strong>86%</strong></article>
      <article><span>人均学时</span><strong>16.1h</strong></article>
    </div>
    <div
      v-else
      :class="$style.placeholder"
    >
      <component
        :is="iconBySection[section as keyof typeof iconBySection] || BookOpen"
        :size="32"
        :stroke-width="1.8"
      /><strong>{{ section }}</strong>
      <p>当前模块已接入培训中心本地工作流。</p>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.surface {
  @apply mt-4 overflow-hidden rounded border bg-white;
  min-height: 480px;
  border-color: var(--ui-border);
}
.surface > header {
  @apply flex h-[80px] items-center gap-4 border-b px-6;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.surface > header > svg {
  @apply box-content rounded p-3;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.surface h2 {
  @apply m-0 text-[16px];
}
.surface header p {
  @apply m-0 mt-1 text-[10px];
  color: var(--ui-text-muted);
}
.courses {
  @apply grid grid-cols-2 gap-3 p-6;
}
.courses article {
  @apply flex h-[76px] items-center gap-3 rounded border p-4;
  border-color: var(--ui-border-subtle);
}
.courses svg {
  color: var(--app-primary);
}
.courses div {
  @apply flex-1;
}
.courses strong {
  @apply text-[11px];
}
.courses p {
  @apply m-0 mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.courses span {
  @apply rounded px-2 py-1 text-[8px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.metrics {
  @apply grid grid-cols-4 gap-4 p-6;
}
.metrics article {
  @apply flex h-28 flex-col justify-center rounded border p-4;
  border-color: var(--ui-border-subtle);
}
.metrics span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-3 text-[22px];
  color: var(--app-primary);
}
.placeholder {
  @apply flex h-[380px] flex-col items-center justify-center;
  color: var(--ui-text-subtle);
}
.placeholder strong {
  @apply mt-4 text-[14px];
  color: var(--ui-text);
}
.placeholder p {
  @apply text-[10px];
}
</style>
