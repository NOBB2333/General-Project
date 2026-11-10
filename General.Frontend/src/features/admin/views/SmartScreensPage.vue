<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  BrainCircuit,
  CalendarDays,
  Grid2X2,
  Monitor,
  Play,
  Radio,
  ShieldCheck,
  Sparkles,
  TriangleAlert,
  Wifi,
  Zap,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, toast } from '@/components/ui';
import { screenActivitiesFixture } from '../model/smart-screens.data';
import type { ScreenActivity } from '../model/smart-screens.types';

type ScreenTab = '总览' | '快捷发布' | '终端管理' | '发布任务' | '审核中心' | '播放记录';
const activities = ref<ScreenActivity[]>(cloneReferenceRows(screenActivitiesFixture.rows));
const tab = ref<ScreenTab>('总览');
const tabs: readonly { value: ScreenTab; label: string; icon: typeof Radio }[] = [
  { value: '总览', label: '总览', icon: Radio },
  { value: '快捷发布', label: '快捷发布', icon: Sparkles },
  { value: '终端管理', label: '终端管理 1', icon: Monitor },
  { value: '发布任务', label: '发布任务', icon: CalendarDays },
  { value: '审核中心', label: '审核中心 5', icon: ShieldCheck },
  { value: '播放记录', label: '播放记录', icon: Play },
];
const pendingCount = computed(() => activities.value.filter((item) => item.status === '等待审核').length);
function approve(item: ScreenActivity) {
  activities.value = activities.value.map((entry) => (entry.id === item.id ? { ...entry, status: '成功' } : entry));
  toast.success('智慧屏内容已在本地通过审核');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/admin/smart-screens"
    data-page-label="智慧屏发布中心"
    data-page-module="行政管理"
    data-smart-screens-page
  >
    <header :class="$style.hero">
      <div :class="$style.heroContent">
        <nav>行政管理 <b>›</b> 智慧屏发布中心 · {{ tab }}</nav>
        <section>
          <span
            ><Monitor
              :size="25"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>智慧屏发布中心</h1>
            <p>AI 内容工厂 + 多终端发布 + 业务事件自动上屏 — 让企业每一块屏幕成为经营展示窗口</p>
          </div>
        </section>
      </div>
      <div :class="$style.heroActions">
        <UiButton
          variant="warning"
          size="sm"
          @click="toast.success('AI 内容工厂已打开')"
          ><template #leading><Sparkles :size="14" /></template>AI 一键生成</UiButton
        ><UiButton
          variant="secondary"
          size="sm"
          @click="tab = '终端管理'"
          ><template #leading><Monitor :size="14" /></template>终端管理</UiButton
        >
      </div>
    </header>
    <header :class="$style.navigation">
      <nav>
        <button
          v-for="item in tabs"
          :key="item.value"
          :data-active="tab === item.value"
          @click="tab = item.value"
        >
          <component
            :is="item.icon"
            :size="14"
            :stroke-width="1.8"
          />{{ item.label }}
        </button>
      </nav>
      <div>
        <UiButton
          variant="secondary"
          size="xs"
          @click="toast.success('AI 工厂已打开')"
          ><template #leading><Sparkles :size="13" /></template>AI 工厂</UiButton
        ><UiButton
          variant="secondary"
          size="xs"
          @click="toast.success('模板中心已打开')"
          ><template #leading><Grid2X2 :size="13" /></template>模板</UiButton
        ><UiButton
          variant="secondary"
          size="xs"
          @click="toast.success('素材库已打开')"
          ><template #leading><Monitor :size="13" /></template>素材</UiButton
        >
      </div>
    </header>
    <template v-if="tab === '总览'">
      <section :class="$style.metrics">
        <article>
          <Wifi :size="18" />
          <div>
            <span>在线终端</span><strong>9</strong>
            <p>共 12 块屏幕</p>
          </div>
        </article>
        <article>
          <TriangleAlert :size="18" />
          <div>
            <span>异常终端</span><strong>1</strong>
            <p>离线 1 · 维护 1</p>
          </div>
        </article>
        <article>
          <Play :size="18" />
          <div>
            <span>今日播放</span><strong>1,284</strong>
            <p>环比 ↑ 22%</p>
          </div>
        </article>
        <article>
          <BrainCircuit :size="18" />
          <div>
            <span>AI 生成</span><strong>38</strong>
            <p>今日生成 · 自动 27</p>
          </div>
        </article>
        <article>
          <ShieldCheck :size="18" />
          <div>
            <span>待审核</span><strong>5</strong>
            <p>敏感内容 2 / 普通 5</p>
          </div>
        </article>
        <article>
          <Zap :size="18" />
          <div>
            <span>事件触发</span><strong>14</strong>
            <p>今日自动上屏</p>
          </div>
        </article>
      </section>
      <section :class="$style.dashboard">
        <article :class="$style.activity">
          <header>
            <div><BrainCircuit :size="16" /><strong>AI 内容活动</strong><span>· 今日 38 条</span></div>
            <button @click="toast.success('已进入 AI 工厂')">进入 AI 工厂 ›</button>
          </header>
          <ul>
            <li
              v-for="item in activities"
              :key="item.id"
            >
              <time>{{ item.time }}</time
              ><span
                :class="$style.activityTone"
                :data-tone="item.tone"
                >{{ item.tone }}</span
              ><small v-if="item.tag">{{ item.tag }}</small>
              <p>{{ item.text }}</p>
              <button
                :data-status="item.status"
                @click="item.status === '等待审核' ? approve(item) : undefined"
              >
                {{ item.status }}
              </button>
            </li>
          </ul>
        </article>
        <aside :class="$style.launchers">
          <button
            data-tone="ai"
            @click="toast.success('AI 内容工厂已打开')"
          >
            <BrainCircuit :size="20" /><strong>AI 内容工厂</strong><span>选择场景 → 10秒生成屏幕内容</span></button
          ><button
            data-tone="template"
            @click="toast.success('模板中心已打开')"
          >
            <Grid2X2 :size="20" /><strong>模板中心</strong><span>16 个官方模板 · 动态数据</span></button
          ><button
            data-tone="market"
            @click="toast.success('场景市场已打开')"
          >
            <Zap :size="20" /><strong>场景应用市场</strong><span>16 个场景 · 业务事件自动触发</span></button
          ><button
            data-tone="material"
            @click="toast.success('素材库已打开')"
          >
            <Monitor :size="20" /><strong>素材库</strong><span>图片 · 视频 · PPT · AI生成</span>
          </button>
        </aside>
      </section>
    </template>
    <section
      v-else
      :class="$style.alternate"
    >
      <component
        :is="tabs.find((item) => item.value === tab)?.icon || Monitor"
        :size="32"
      />
      <h2>{{ tab }}</h2>
      <p v-if="tab === '终端管理'">9 台终端在线，1 台终端异常</p>
      <p v-else-if="tab === '审核中心'">{{ pendingCount }} 条内容等待审核</p>
      <p v-else>智慧屏{{ tab }}工作区已连接本地状态</p>
      <UiButton
        variant="primary"
        size="sm"
        @click="toast.success(`${tab}操作已写入本地`)"
        >新建{{ tab }}</UiButton
      >
    </section>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-4 min-h-[200px] overflow-hidden rounded-lg px-6 py-6 text-white;
  background-image: url('../../../assets/admin/smart-screens-hero.jpg');
  background-position: center 48%;
  background-size: cover;
}
.hero::before {
  content: '';
  @apply absolute inset-0;
  background: linear-gradient(
    110deg,
    rgba(11, 31, 61, 0.85) 0%,
    rgba(11, 31, 61, 0.62) 38%,
    rgba(11, 31, 61, 0.28) 70%,
    rgba(11, 31, 61, 0.1) 100%
  );
}
.heroContent {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.8);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.82);
}
.heroActions {
  @apply absolute right-6 top-[58px] z-[2] flex gap-2;
}
.navigation {
  @apply mb-4 flex h-[39px] items-center justify-between rounded border bg-white px-3;
  border-color: var(--ui-border);
}
.navigation nav {
  @apply flex h-full;
}
.navigation nav button {
  @apply flex h-full items-center gap-1.5 border-b-2 border-transparent px-3 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.navigation nav button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.navigation > div {
  @apply flex gap-2;
}
.metrics {
  @apply mb-4 grid grid-cols-6 gap-3;
}
.metrics article {
  @apply min-h-[96px] rounded border bg-white p-3.5;
  border-color: var(--ui-border);
}
.metrics article > svg {
  @apply mr-2 inline-block align-middle;
  width: 16px;
  height: 16px;
  color: #00875a;
}
.metrics article > div {
  display: contents;
}
.metrics article:nth-child(2) > svg {
  color: #de350b;
}
.metrics article:nth-child(3) > svg {
  color: #0052cc;
}
.metrics article:nth-child(4) > svg {
  color: #6554c0;
}
.metrics article:nth-child(5) > svg {
  color: #ff8b00;
}
.metrics article:nth-child(6) > svg {
  color: #ff8b00;
}
.metrics span,
.metrics p {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.metrics span {
  @apply align-middle;
}
.metrics strong {
  @apply block text-[22px];
}
.metrics article:nth-child(1) strong {
  color: #00875a;
}
.metrics article:nth-child(2) strong {
  color: #de350b;
}
.metrics article:nth-child(3) strong {
  color: #0052cc;
}
.metrics article:nth-child(4) strong {
  color: #6554c0;
}
.metrics article:nth-child(5) strong {
  color: #ff8b00;
}
.metrics article:nth-child(6) strong {
  color: #ff8b00;
}
.metrics p {
  @apply mt-0.5;
}
.dashboard {
  @apply grid grid-cols-[minmax(0,2.04fr)_minmax(280px,1fr)] gap-4;
}
.activity {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.activity > header {
  @apply flex h-[42px] items-center justify-between border-b px-4;
  border-color: var(--ui-border);
}
.activity header > div {
  @apply flex items-center gap-2;
}
.activity header svg {
  color: #6554c0;
}
.activity header strong {
  @apply text-[12px];
}
.activity header span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.activity header button {
  @apply text-[10px] font-semibold;
  color: var(--app-primary);
}
.activity li {
  @apply flex h-9 items-center gap-2 border-b px-4 text-[10px];
  border-color: var(--ui-border-subtle);
}
.activity time {
  @apply w-10 font-semibold;
  color: var(--ui-text-subtle);
}
.activityTone,
.activity li small {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #6554c0;
  background: #eae6ff;
}
.activityTone[data-tone='发布'] {
  color: #00875a;
  background: #e3fcef;
}
.activityTone[data-tone='建议'] {
  color: #0052cc;
  background: #deebff;
}
.activityTone[data-tone='播插'] {
  color: #de350b;
  background: #ffebe6;
}
.activityTone[data-tone='审核'] {
  color: #ff8b00;
  background: #fff0b3;
}
.activity li small {
  color: #00875a;
  background: #e3fcef;
}
.activity li p {
  @apply min-w-0 flex-1 truncate;
}
.activity li > button {
  @apply rounded px-2 py-1 text-[8px];
  color: #00875a;
  background: #e3fcef;
}
.activity li > button[data-status='等待审核'] {
  color: #ff8b00;
  background: #fff0b3;
}
.launchers {
  @apply grid gap-3;
}
.launchers button {
  @apply flex min-h-[88px] flex-col justify-center rounded px-5 text-left text-white;
  background: linear-gradient(120deg, #5143aa, #0052cc);
}
.launchers button[data-tone='template'] {
  background: linear-gradient(120deg, #0052cc, #00a3bf);
}
.launchers button[data-tone='market'] {
  background: linear-gradient(120deg, #ff8b00, #de350b);
}
.launchers button[data-tone='material'] {
  background: linear-gradient(120deg, #00875a, #006644);
}
.launchers svg {
  @apply mb-2;
}
.launchers strong {
  @apply text-[13px];
}
.launchers span {
  @apply mt-1 text-[9px];
  color: rgba(255, 255, 255, 0.85);
}
.alternate {
  @apply flex min-h-[490px] flex-col items-center justify-center rounded border bg-white;
  border-color: var(--ui-border);
  color: var(--app-primary);
}
.alternate h2 {
  @apply mt-3 text-[17px];
  color: var(--ui-text);
}
.alternate p {
  @apply mb-5 mt-2 text-[11px];
  color: var(--ui-text-subtle);
}
</style>
