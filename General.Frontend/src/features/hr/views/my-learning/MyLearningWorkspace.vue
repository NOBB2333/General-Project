<script setup lang="ts">
import { Award, Play, Trophy } from 'lucide-vue-next';
import type { LearningCertificate, LearningCourse, LearningView } from '../../model/my-learning.types';
const props = defineProps<{
  view: LearningView;
  courses: readonly LearningCourse[];
  certificates: readonly LearningCertificate[];
}>();
const emit = defineEmits<{ open: [course: LearningCourse] }>();
const visibleCourses = () =>
  props.view === '必修任务'
    ? props.courses.filter((item) => item.required)
    : props.view === '我的课程'
      ? props.courses
      : props.courses.filter((item) => item.status === '在学');
</script>
<template>
  <section :class="$style.workspace">
    <template v-if="view === '学习总览' || view === '必修任务' || view === '我的课程'"
      ><header>
        <strong>{{ view === '学习总览' ? '正在学习' : view }}</strong
        ><b>{{ visibleCourses().length }}</b>
      </header>
      <div :class="$style.courses">
        <article
          v-for="(course, index) in visibleCourses()"
          :key="course.id"
        >
          <div
            :class="$style.cover"
            :data-tone="index % 2 ? 'orange' : 'blue'"
          >
            <span>{{ course.category }}</span
            ><i v-if="course.required">必修</i
            ><Play
              :size="30"
              :stroke-width="1.8"
            />
          </div>
          <div :class="$style.courseBody">
            <strong>{{ course.title }}</strong
            ><span>上次学习：{{ course.lastLearned }}</span
            ><progress
              :value="course.progress"
              max="100"
            >
              {{ course.progress }}%
            </progress>
            <p>
              <b>{{ course.progress }}%</b
              ><em>{{ course.status === '在学' ? `截止 ${course.deadline}` : '已完成' }}</em>
            </p>
            <button
              type="button"
              @click="emit('open', course)"
            >
              {{ course.status === '在学' ? '继续学习' : '查看课程' }}
            </button>
          </div>
        </article>
      </div></template
    ><template v-else-if="view === '我的考试'"
      ><header><strong>我的考试</strong><b>3</b></header>
      <div :class="$style.list">
        <article
          v-for="item in ['MX-300 上市前考试', '信息安全年度合规考试', '销售场景 Roleplay 考核']"
          :key="item"
        >
          <Trophy
            :size="17"
            :stroke-width="1.8"
          />
          <div>
            <strong>{{ item }}</strong
            ><span>截止 2026-06-15 · 允许 2 次作答</span>
          </div>
          <b>待完成</b>
        </article>
      </div></template
    ><template v-else
      ><header>
        <strong>我的证书</strong><b>{{ certificates.length }}</b>
      </header>
      <div :class="$style.list">
        <article
          v-for="item in certificates"
          :key="item.id"
        >
          <Award
            :size="17"
            :stroke-width="1.8"
          />
          <div>
            <strong>{{ item.title }}</strong
            ><span>{{ item.code }} · 有效期至 {{ item.expiresAt }}</span>
          </div>
          <b :data-status="item.status">{{ item.status }}</b>
        </article>
      </div></template
    >
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4;
}
.workspace > header {
  @apply flex h-8 items-center gap-2;
}
.workspace > header strong {
  @apply text-[12px];
}
.workspace > header b {
  @apply rounded px-2 py-0.5 text-[9px];
  background: var(--ui-surface-hover);
}
.courses {
  @apply grid grid-cols-2 gap-3;
}
.courses > article {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.cover {
  @apply relative flex h-[86px] items-center justify-center text-white;
  background: linear-gradient(120deg, #01b7d2, #075dcc);
}
.cover[data-tone='orange'] {
  background: linear-gradient(120deg, #ff9815, #c65c00);
}
.cover span,
.cover i {
  @apply absolute left-3 top-3 rounded px-2 py-1 text-[8px] font-semibold not-italic;
  background: rgb(255 255 255 / 18%);
}
.cover i {
  @apply left-auto right-3;
  color: white;
  background: var(--ui-danger);
}
.courseBody {
  @apply relative p-3;
}
.courseBody > strong {
  @apply block text-[11px];
}
.courseBody > span {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.courseBody progress {
  @apply mt-3 h-1.5 w-full;
  accent-color: var(--app-primary);
}
.courseBody p {
  @apply m-0 mt-1 flex justify-between text-[9px];
}
.courseBody p b {
  color: var(--app-primary);
}
.courseBody p em {
  @apply not-italic;
  color: var(--ui-danger);
}
.courseBody button {
  @apply mt-3 rounded border px-3 py-1.5 text-[9px];
  border-color: var(--ui-border);
  color: var(--app-primary);
  background: white;
}
.list {
  @apply space-y-3;
}
.list article {
  @apply flex h-[70px] items-center gap-3 rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.list article > svg {
  color: var(--app-primary);
}
.list article > div {
  @apply flex flex-1 flex-col;
}
.list strong {
  @apply text-[11px];
}
.list span {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.list article > b {
  @apply rounded px-2 py-1 text-[8px];
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.list article > b[data-status='有效'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
</style>
