<script setup lang="ts">
import {
  CalendarDays,
  ClipboardCheck,
  FilePlus2,
  FileStack,
  GitBranch,
  LayoutGrid,
  Search,
  Send,
  ShieldAlert,
  Upload,
} from 'lucide-vue-next';
import { computed, ref, type Component } from 'vue';
import { useRouter } from 'vue-router';
import { UiButton, UiDialog, UiInput, toast } from '@/components/ui';
import { drawingOverviewReferenceFixture } from '../model/drawing-overview.data';
import type { DrawingOverviewAction } from '../model/drawing-overview.types';
import DrawingHero from './components/DrawingHero.vue';

const router = useRouter();
const query = ref('');
const editorOpen = ref(false);
const editorTitle = ref('新建图号');
const drawingNo = ref('');
const drawingName = ref('');

const metricIcons: Record<string, Component> = {
  total: FileStack,
  published: ClipboardCheck,
  pending: CalendarDays,
  review: ClipboardCheck,
  release: Send,
  changing: GitBranch,
  month: CalendarDays,
};
const actionIcons: Record<string, Component> = {
  'create-number': FilePlus2,
  'upload-version': Upload,
  'start-review': ClipboardCheck,
  'start-change': GitBranch,
  'pending-review': ClipboardCheck,
  'pending-release': Send,
  'version-risk': ShieldAlert,
};
const changes = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  if (!normalized) return drawingOverviewReferenceFixture.changes;
  return drawingOverviewReferenceFixture.changes.filter((row) =>
    [row.changeNo, row.name, row.reason, row.status].some((value) =>
      value.toLocaleLowerCase('zh-CN').includes(normalized),
    ),
  );
});

function runAction(action: DrawingOverviewAction) {
  if (action.target) {
    void router.push(action.target);
    return;
  }
  editorTitle.value = action.label;
  editorOpen.value = true;
}

function createDrawing() {
  if (!drawingNo.value.trim() || !drawingName.value.trim()) return;
  editorOpen.value = false;
  toast.success(`${drawingNo.value} 已保存到当前本地会话`);
  drawingNo.value = '';
  drawingName.value = '';
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-implementation="drawing-overview"
    data-page-route="/drawings/drawing-overview"
  >
    <DrawingHero
      title="图纸总览"
      description="图纸数量趋势、评审/发布/变更概览，掌握图纸资产全景"
      :icon="LayoutGrid"
      variant="overview"
    />

    <section
      :class="$style.metrics"
      aria-label="图纸指标"
    >
      <article
        v-for="metric in drawingOverviewReferenceFixture.metrics"
        :key="metric.id"
        :data-tone="metric.tone"
      >
        <span :class="$style.metricIcon">
          <component
            :is="metricIcons[metric.id]"
            :size="17"
            :stroke-width="1.8"
          />
        </span>
        <small>{{ metric.label }}</small>
        <strong>{{ metric.value }}</strong>
      </article>
    </section>

    <section
      :class="$style.quickActions"
      aria-label="图纸快捷操作"
    >
      <button
        v-for="action in drawingOverviewReferenceFixture.actions"
        :key="action.id"
        type="button"
        @click="runAction(action)"
      >
        <span
          ><component
            :is="actionIcons[action.id]"
            :size="17"
            :stroke-width="1.8"
        /></span>
        {{ action.label }}
      </button>
    </section>

    <label :class="$style.searchField">
      <Search
        :size="15"
        :stroke-width="1.8"
      />
      <UiInput
        v-model="query"
        placeholder="搜索图号 / 图纸名称 / 物料..."
        aria-label="搜索图号、图纸名称或物料"
      />
    </label>

    <section :class="$style.queueGrid">
      <article :class="$style.queueCard">
        <header>
          <h2>
            <CalendarDays
              :size="16"
              :stroke-width="1.8"
            />待审核图纸 <span>3</span>
          </h2>
          <button
            type="button"
            @click="router.push('/drawings/drawing-review')"
          >
            查看全部 →
          </button>
        </header>
        <button
          v-for="row in drawingOverviewReferenceFixture.rows"
          :key="row.id"
          type="button"
          :class="$style.queueRow"
          @click="router.push('/drawings/drawing-review')"
        >
          <i />
          <span
            ><b>{{ row.drawingNo }}</b
            ><strong>{{ row.name }}</strong
            ><small>版本 {{ row.version }}　提交人: {{ row.submitter }}　{{ row.submittedAt }}</small></span
          >
          <em data-tone="warning">{{ row.status }}</em>
        </button>
      </article>

      <article :class="$style.queueCard">
        <header>
          <h2>
            <GitBranch
              :size="16"
              :stroke-width="1.8"
            />最近变更图纸 <span>5</span>
          </h2>
          <button
            type="button"
            @click="router.push('/drawings/drawing-change')"
          >
            查看全部 →
          </button>
        </header>
        <button
          v-for="row in changes"
          :key="row.id"
          type="button"
          :class="$style.queueRow"
          @click="router.push('/drawings/drawing-change')"
        >
          <i />
          <span
            ><b>{{ row.changeNo }}</b
            ><strong>{{ row.name }}</strong
            ><small>{{ row.versions }}　{{ row.reason }}　{{ row.changedAt }}</small></span
          >
          <em
            :data-tone="
              row.status.includes('退回')
                ? 'danger'
                : row.status.includes('执行')
                  ? 'purple'
                  : row.status.includes('通过')
                    ? 'blue'
                    : 'neutral'
            "
            >{{ row.status }}</em
          >
        </button>
        <p
          v-if="!changes.length"
          :class="$style.empty"
        >
          没有匹配的变更记录
        </p>
      </article>
    </section>

    <UiDialog
      v-model:open="editorOpen"
      :title="editorTitle"
      description="数据只保存到当前本地会话"
      size="sm"
    >
      <form
        :class="$style.editor"
        @submit.prevent="createDrawing"
      >
        <label
          ><span>图号</span
          ><UiInput
            v-model="drawingNo"
            placeholder="例如 DWG-2026-001"
            autofocus
        /></label>
        <label
          ><span>图纸名称</span
          ><UiInput
            v-model="drawingName"
            placeholder="请输入图纸名称"
        /></label>
      </form>
      <template #footer>
        <UiButton
          size="sm"
          @click="editorOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          :disabled="!drawingNo.trim() || !drawingName.trim()"
          @click="createDrawing"
          >保存</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.metrics {
  @apply mt-4 grid grid-cols-7 gap-3;
}
.metrics article {
  @apply relative min-h-[126px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metricIcon {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: #1d4f91;
  background: #deebff;
}
.metrics small {
  @apply mt-3 block text-[11px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1 block text-[22px] leading-7;
}
.metrics article[data-tone='green'] .metricIcon {
  color: #00875a;
  background: #e3fcef;
}
.metrics article[data-tone='green'] strong {
  color: #00875a;
}
.metrics article[data-tone='orange'] .metricIcon {
  color: #ff8b00;
  background: #fffae6;
}
.metrics article[data-tone='orange'] strong {
  color: #ff8b00;
}
.metrics article[data-tone='purple'] .metricIcon {
  color: #6554c0;
  background: #eae6ff;
}
.metrics article[data-tone='purple'] strong {
  color: #6554c0;
}
.metrics article[data-tone='blue'] .metricIcon {
  color: #0052cc;
  background: #deebff;
}
.metrics article[data-tone='blue'] strong {
  color: #0052cc;
}
.quickActions {
  @apply mt-4 grid grid-cols-7 gap-3;
}
.quickActions button {
  @apply flex h-[52px] items-center gap-2 rounded border bg-white px-4 text-left text-[12px] font-semibold transition-colors;
  border-color: var(--ui-border);
}
.quickActions button:hover {
  border-color: #4c9aff;
  background: #f7faff;
}
.quickActions button span {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.searchField {
  @apply ml-auto mt-4 flex h-9 w-[286px] items-center gap-2 rounded border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.searchField :global(input) {
  @apply h-7 border-0 p-0 shadow-none;
}
.queueGrid {
  @apply mt-3 grid grid-cols-2 gap-4;
}
.queueCard {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.queueCard > header {
  @apply flex h-[58px] items-center justify-between border-b px-5;
  border-color: var(--ui-border);
}
.queueCard h2 {
  @apply flex items-center gap-2 text-[14px] font-semibold;
}
.queueCard h2 svg {
  color: #6554c0;
}
.queueCard h2 span {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[10px];
  color: #6554c0;
  background: #eae6ff;
}
.queueCard header button {
  @apply text-[11px] font-semibold;
  color: #0052cc;
}
.queueRow {
  @apply grid min-h-[65px] w-full grid-cols-[6px_1fr_auto] items-center gap-3 border-b px-5 text-left transition-colors;
  border-color: var(--ui-border-subtle);
}
.queueRow:hover {
  background: #f7faff;
}
.queueRow > i {
  @apply h-8 w-1 rounded-full;
  background: #ffab00;
}
.queueCard:nth-child(2) .queueRow > i {
  background: #7a869a;
}
.queueRow > span {
  @apply min-w-0;
}
.queueRow b {
  @apply mr-2 text-[11px];
  color: #0052cc;
}
.queueRow strong {
  @apply text-[11px];
}
.queueRow small {
  @apply mt-1 block truncate text-[10px];
  color: #7a869a;
}
.queueRow em {
  @apply rounded px-2 py-1 text-[9px] not-italic;
  background: #f4f5f7;
  color: #6b778c;
}
.queueRow em[data-tone='warning'] {
  color: #ff8b00;
  background: #fffae6;
}
.queueRow em[data-tone='danger'] {
  color: #de350b;
  background: #ffebe6;
}
.queueRow em[data-tone='purple'] {
  color: #6554c0;
  background: #eae6ff;
}
.queueRow em[data-tone='blue'] {
  color: #0052cc;
  background: #deebff;
}
.empty {
  @apply p-8 text-center text-[11px];
  color: var(--ui-text-subtle);
}
.editor {
  @apply grid gap-4 p-5;
}
.editor label span {
  @apply mb-1.5 block text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
@media (max-width: 1100px) {
  .metrics,
  .quickActions {
    @apply grid-cols-4;
  }
  .queueGrid {
    @apply grid-cols-1;
  }
}
</style>
