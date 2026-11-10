<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Plus, Sparkles } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import { trainingFixture } from '../model/training.data';
import type { TrainingCourse } from '../model/training.types';
import TrainingDashboard from './training/TrainingDashboard.vue';
import TrainingModuleView from './training/TrainingModuleView.vue';
import TrainingHero from './training-shared/TrainingHero.vue';
import TrainingNavigation from './training-shared/TrainingNavigation.vue';
const route = useRoute();
const router = useRouter();
const rows = ref<TrainingCourse[]>(cloneReferenceRows(trainingFixture.rows));
const createOpen = ref(false);
const aiOpen = ref(false);
const title = ref('');
const activeSection = computed(() => (typeof route.query.section === 'string' ? route.query.section : '培训中心'));
watch(
  () => route.path,
  () => {
    if (route.path === '/hr/training' && !route.query.section) return;
  },
);
function saveCourse() {
  if (!title.value.trim()) {
    toast.warning('请填写课程名称');
    return;
  }
  rows.value = [
    {
      id: `tc-${Date.now()}`,
      title: title.value.trim(),
      category: '内部培训',
      required: false,
      hours: 1,
      learners: 0,
      completion: 0,
      status: '草稿',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('课程草稿已保存到本地');
}
function openMyLearning() {
  router.push('/hr/my-learning');
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/hr/training"
    data-page-label="培训与考试"
    data-page-module="人事管理"
    data-training-page
    :data-section="activeSection"
    :data-course-count="rows.length"
  >
    <TrainingHero subtitle="8 门课程 · 3 个进行中考试 · 全员覆盖率 89% · 累计学时 9,842h"
      ><template #actions
        ><UiButton
          variant="inverted"
          size="sm"
          @click="createOpen = true"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建课程</UiButton
        ><UiButton
          variant="inverted"
          size="sm"
          @click="aiOpen = true"
          ><template #leading
            ><Sparkles
              :size="14"
              :stroke-width="1.8" /></template
          >AI 智能出题</UiButton
        ></template
      ></TrainingHero
    ><TrainingNavigation :active="activeSection" /><TrainingDashboard
      v-if="activeSection === '培训中心'"
      @mine="openMyLearning"
      @ai="aiOpen = true"
    /><TrainingModuleView
      v-else
      :section="activeSection"
      :courses="rows"
    />
  </main>
  <UiDialog
    v-model:open="createOpen"
    title="新建课程"
    description="课程仅保存到当前本地会话"
    size="md"
    ><div :class="$style.form">
      <UiFormField
        label="课程名称"
        required
        ><UiInput
          v-model="title"
          aria-label="课程名称"
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
        @click="saveCourse"
        >保存课程</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="aiOpen"
    title="AI 智能出题"
    description="上传课件后在本地生成题目草稿"
    size="md"
    ><div :class="$style.ai">
      <Sparkles
        :size="24"
        :stroke-width="1.8"
      />
      <p>支持课程课件、制度和员工手册，自动生成单选、多选、判断和简答题草稿。</p>
      <UiInput
        model-value="MX-300 产品知识"
        aria-label="出题主题"
      />
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="aiOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="
          aiOpen = false;
          toast.success('AI 题目草稿已在本地生成');
        "
        >生成题目</UiButton
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
.form,
.ai {
  @apply p-5;
}
.ai {
  @apply flex flex-col items-center gap-3 text-center;
}
.ai > svg {
  color: #6554c0;
}
.ai p {
  @apply m-0 text-[10px] leading-5;
  color: var(--ui-text-muted);
}
</style>
