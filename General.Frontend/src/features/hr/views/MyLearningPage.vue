<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { Bell, BookOpen, CalendarDays, ShieldCheck, Trophy } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { myCertificates, myLearningFixture } from '../model/my-learning.data';
import type { LearningCourse, LearningView } from '../model/my-learning.types';
import MyLearningProfile from './my-learning/MyLearningProfile.vue';
import MyLearningWorkspace from './my-learning/MyLearningWorkspace.vue';
import TrainingHero from './training-shared/TrainingHero.vue';
import TrainingNavigation from './training-shared/TrainingNavigation.vue';
const router = useRouter();
const courses = ref<LearningCourse[]>(cloneReferenceRows(myLearningFixture.rows));
const view = ref<LearningView>('学习总览');
const detailOpen = ref(false);
const activeCourse = ref<LearningCourse | null>(null);
const todos = [
  { title: 'MX-300 上市前考试', date: '2026-05-30', urgent: true, icon: Trophy },
  { title: '信息安全年度合规（年度复训）', date: '2026-06-15', urgent: false, icon: BookOpen },
  { title: 'MX-300 销售作战集训营 · Day3 Roleplay', date: '今天 15:00', urgent: true, icon: CalendarDays },
  { title: '年度合规培训证书 9 天后到期，请重训', date: '2026-06-04', urgent: false, icon: ShieldCheck },
] as const;
function setView(value: string) {
  view.value = value as LearningView;
}
function openCourse(course: LearningCourse) {
  activeCourse.value = course;
  detailOpen.value = true;
}
function continueCourse() {
  if (!activeCourse.value) return;
  const row = courses.value.find((item) => item.id === activeCourse.value?.id);
  if (row && row.progress < 100) row.progress = Math.min(100, row.progress + 5);
  detailOpen.value = false;
  toast.success('学习进度已在本地更新');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/hr/my-learning"
    data-page-label="我的学习"
    data-page-module="人事管理"
    data-my-learning-page
    :data-view="view"
  >
    <TrainingHero subtitle="我的必修任务 / 在学课程 / 考试与证书 / 学习时长与积分"
      ><template #actions
        ><UiButton
          variant="inverted"
          size="sm"
          @click="router.push({ path: '/hr/training', query: { section: '课程库' } })"
          ><template #leading
            ><BookOpen
              :size="14"
              :stroke-width="1.8" /></template
          >浏览课程库</UiButton
        ></template
      ></TrainingHero
    ><TrainingNavigation active="我的学习" /><MyLearningProfile />
    <section :class="$style.todoHeader">
      <Bell
        :size="14"
        :stroke-width="1.8"
      /><strong>我的待办（4）</strong><button type="button">查看全部</button>
    </section>
    <section :class="$style.todos">
      <article
        v-for="item in todos"
        :key="item.title"
      >
        <component
          :is="item.icon"
          :size="16"
          :stroke-width="1.8"
        /><i v-if="item.urgent">急</i><strong>{{ item.title }}</strong
        ><span>{{ item.date }}</span>
      </article>
    </section>
    <UiTabs
      :model-value="view"
      @update:model-value="setView"
      ><UiTabsList
        label="个人学习视图"
        :class="$style.tabs"
        ><UiTabsTrigger value="学习总览">学习总览</UiTabsTrigger><UiTabsTrigger value="必修任务">必修任务</UiTabsTrigger
        ><UiTabsTrigger value="我的课程">我的课程 (4)</UiTabsTrigger
        ><UiTabsTrigger value="我的考试">我的考试</UiTabsTrigger
        ><UiTabsTrigger value="我的证书">我的证书 (3)</UiTabsTrigger></UiTabsList
      ></UiTabs
    ><MyLearningWorkspace
      :view="view"
      :courses="courses"
      :certificates="myCertificates"
      @open="openCourse"
    />
  </main>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeCourse?.title || '课程详情'"
    :description="activeCourse ? `${activeCourse.category} · 当前进度 ${activeCourse.progress}%` : undefined"
    size="md"
    ><div
      v-if="activeCourse"
      :class="$style.detail"
    >
      <p>{{ activeCourse.required ? '必修课程' : '选修课程' }} · 截止 {{ activeCourse.deadline }}</p>
      <progress
        :value="activeCourse.progress"
        max="100"
      >
        {{ activeCourse.progress }}%</progress
      ><strong>{{ activeCourse.progress }}%</strong>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="continueCourse"
        >继续学习</UiButton
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
.todoHeader {
  @apply mt-4 flex h-8 items-center gap-2;
}
.todoHeader > svg {
  color: var(--ui-warning);
}
.todoHeader strong {
  @apply text-[11px];
}
.todoHeader button {
  @apply ml-auto border-0 bg-transparent text-[9px];
  color: var(--app-primary);
}
.todos {
  @apply grid grid-cols-4 gap-3;
}
.todos article {
  @apply relative flex h-[92px] flex-col justify-center rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.todos svg {
  color: var(--app-primary);
}
.todos i {
  @apply absolute right-3 top-3 rounded px-1.5 py-0.5 text-[8px] not-italic;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.todos strong {
  @apply mt-3 text-[10px];
}
.todos span {
  @apply mt-2 text-[9px];
  color: var(--ui-text-subtle);
}
.tabs {
  @apply mt-4 h-11 w-full justify-start rounded border bg-white px-2;
  border-color: var(--ui-border);
}
.tabs [role='tab'] {
  @apply h-9;
}
.detail {
  @apply p-5;
}
.detail p {
  @apply m-0 text-[10px];
  color: var(--ui-text-muted);
}
.detail progress {
  @apply mt-4 h-2;
  width: calc(100% - 50px);
  accent-color: var(--app-primary);
}
.detail strong {
  @apply ml-3 text-[11px];
  color: var(--app-primary);
}
</style>
