<script setup lang="ts">
import {
  Bell,
  BookOpen,
  BrainCircuit,
  Clock3,
  FileQuestion,
  FolderKanban,
  Library,
  Sparkles,
  Trophy,
  Upload,
  UsersRound,
  ShieldCheck,
} from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import { trainingLeaders, trainingTasks } from '../../model/training.data';
const emit = defineEmits<{ mine: []; ai: [] }>();
const metrics = [
  { label: '课程数量', value: '8', note: '门已发布', icon: Library, tone: 'primary' },
  { label: '进行中考试', value: '3', note: '共 6 场', icon: Trophy, tone: 'warning' },
  { label: '全员覆盖率', value: '89%', note: '612 名员工在学', icon: UsersRound, tone: 'success' },
  { label: '证书有效', value: '9', note: '3 张即将到期', icon: ShieldCheck, tone: 'purple' },
  { label: '累计学时', value: '9,842h', note: '平均合格率 86%', icon: Clock3, tone: 'danger' },
] as const;
</script>
<template>
  <section :class="$style.pending">
    <Bell
      :size="18"
      :stroke-width="1.8"
    />
    <p>
      <strong>您有 4 项学习/考试待完成</strong
      ><span>MX-300 上市前考试 · 2026-05-30 | 信息安全年度合规（年度复训） · 2026-06-15 · 等 2 项</span>
    </p>
    <UiButton
      variant="warning"
      size="sm"
      @click="emit('mine')"
      >进入我的学习</UiButton
    >
  </section>
  <section :class="$style.metrics">
    <article
      v-for="item in metrics"
      :key="item.label"
      :data-tone="item.tone"
    >
      <component
        :is="item.icon"
        :size="18"
        :stroke-width="1.8"
      />
      <div>
        <span>{{ item.label }}</span
        ><strong>{{ item.value }}</strong
        ><small>{{ item.note }}</small>
      </div>
    </article>
  </section>
  <section :class="$style.tools">
    <div :class="$style.ai">
      <BrainCircuit
        :size="24"
        :stroke-width="1.8"
      />
      <div>
        <h2>AI 助手·让出题与课程制作快 10 倍</h2>
        <p>上传课件/规章/手册，AI 自动出题 · 课件智能问答 · 自动摘要 · 简答题辅助评分</p>
        <nav>
          <button
            type="button"
            @click="emit('ai')"
          >
            <Sparkles
              :size="13"
              :stroke-width="1.8"
            />上传课件自动出题</button
          ><button type="button">
            <Upload
              :size="13"
              :stroke-width="1.8"
            />一键导入课程</button
          ><button type="button">
            <Sparkles
              :size="13"
              :stroke-width="1.8"
            />智能学习路径
          </button>
        </nav>
      </div>
    </div>
    <div :class="$style.shortcuts">
      <article>
        <Library
          :size="17"
          :stroke-width="1.8"
        />
        <p><strong>课程库</strong><span>8 门</span></p>
      </article>
      <article>
        <FolderKanban
          :size="17"
          :stroke-width="1.8"
        />
        <p><strong>培训项目</strong><span>5 个项目</span></p>
      </article>
      <article>
        <FileQuestion
          :size="17"
          :stroke-width="1.8"
        />
        <p><strong>题库</strong><span>21 题</span></p>
      </article>
      <article>
        <BookOpen
          :size="17"
          :stroke-width="1.8"
        />
        <p><strong>试卷</strong><span>6 套</span></p>
      </article>
    </div>
  </section>
  <section :class="$style.bottom">
    <article>
      <header>
        <BookOpen
          :size="14"
          :stroke-width="1.8"
        /><strong>我的学习</strong
        ><button
          type="button"
          @click="emit('mine')"
        >
          查看全部
        </button>
      </header>
      <div :class="$style.profile">
        <b>张</b>
        <p><strong>张明远 · 销售部</strong><span>L4 进阶者　1280 积分</span><small>距离 专家 还差 1120 积分</small></p>
      </div>
      <p
        v-for="task in trainingTasks.slice(0, 2)"
        :key="task.id"
        :class="$style.task"
      >
        <Trophy
          :size="14"
          :stroke-width="1.8"
        /><span
          ><strong>{{ task.title }}</strong
          ><small>{{ task.date }}</small></span
        >
      </p>
    </article>
    <article>
      <header>
        <Trophy
          :size="14"
          :stroke-width="1.8"
        /><strong>学习榜·本月 Top</strong><button type="button">完整榜单</button>
      </header>
      <p
        v-for="(leader, index) in trainingLeaders"
        :key="leader[0]"
        :class="$style.leader"
      >
        <b>{{ index + 1 }}</b
        ><span
          ><strong>{{ leader[0] }} · {{ leader[1] }}</strong
          ><small>{{ leader[2] }}</small></span
        >
      </p>
    </article>
    <article>
      <header>
        <Bell
          :size="14"
          :stroke-width="1.8"
        /><strong>培训公告</strong><b>5</b>
      </header>
      <p
        v-for="notice in [
          'MX-300 上市前考试 5/30 截止，请尽快完成',
          '全员年度合规考试已开启，6/15 前完成',
          '新课《2026 客户服务礼仪》已上架',
          '您的「年度合规培训证书」剩余 9 天到期',
        ]"
        :key="notice"
        :class="$style.notice"
      >
        <span>{{ notice }}</span>
      </p>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
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
.metrics {
  @apply mt-4 grid grid-cols-5 gap-3;
}
.metrics article {
  @apply flex h-[84px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics svg {
  @apply box-content rounded p-2;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.metrics div {
  @apply flex flex-col;
}
.metrics span,
.metrics small {
  @apply text-[8px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply my-1 text-[18px];
}
.metrics article[data-tone='warning'] svg {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.metrics article[data-tone='success'] svg {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.metrics article[data-tone='purple'] svg {
  color: #6554c0;
  background: #eae6ff;
}
.metrics article[data-tone='danger'] svg {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.tools {
  @apply mt-4 grid grid-cols-[3fr_2fr] gap-3;
}
.ai {
  @apply flex h-[130px] items-center gap-4 rounded px-5 text-white;
  background: linear-gradient(105deg, #4b3fb3, #075dce 55%, #526da7);
}
.ai > svg {
  @apply box-content rounded border border-white/25 bg-white/10 p-3;
}
.ai h2 {
  @apply m-0 text-[14px];
}
.ai p {
  @apply m-0 mt-2 text-[10px] text-white/75;
}
.ai nav {
  @apply mt-3 flex gap-2;
}
.ai button {
  @apply flex items-center gap-1 rounded border-0 px-3 py-2 text-[9px] text-white;
  background: rgb(255 255 255 / 17%);
}
.shortcuts {
  @apply grid grid-cols-2 gap-3;
}
.shortcuts article {
  @apply flex items-center gap-3 rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.shortcuts svg {
  color: var(--app-primary);
}
.shortcuts p {
  @apply m-0 flex flex-col;
}
.shortcuts strong {
  @apply text-[10px];
}
.shortcuts span {
  @apply mt-1 text-[8px];
  color: var(--ui-text-subtle);
}
.bottom {
  @apply mt-4 grid grid-cols-3 gap-3;
}
.bottom > article {
  @apply min-h-[240px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.bottom header {
  @apply flex items-center gap-2;
}
.bottom header strong {
  @apply text-[11px];
}
.bottom header button {
  @apply ml-auto border-0 bg-transparent text-[9px];
  color: var(--app-primary);
}
.profile {
  @apply mt-4 flex items-center gap-3;
}
.profile > b {
  @apply flex h-10 w-10 items-center justify-center rounded-full text-[15px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.profile p,
.task span,
.leader span {
  @apply m-0 flex flex-col;
}
.profile strong,
.task strong,
.leader strong {
  @apply text-[10px];
}
.profile span,
.profile small,
.task small,
.leader small {
  @apply mt-1 text-[8px];
  color: var(--ui-text-subtle);
}
.task,
.leader,
.notice {
  @apply m-0 mt-3 flex items-center gap-3 border-t pt-3;
  border-color: var(--ui-border-subtle);
}
.task svg {
  color: var(--ui-warning);
}
.leader > b {
  @apply flex h-6 w-6 items-center justify-center rounded text-[9px] text-white;
  background: var(--ui-text-subtle);
}
.notice span {
  @apply text-[9px] leading-4;
  color: var(--ui-text-muted);
}
</style>
