<script setup lang="ts">
import { computed, ref } from 'vue';
import { BarChart3, Download, Edit3, History, Printer } from 'lucide-vue-next';
import { UiButton, UiDialog, UiInput, toast } from '@/components/ui';
import { handbookChapters } from '../model/handbook.data';
import type { HandbookChapter } from '../model/handbook.types';
import HandbookHero from './handbook/HandbookHero.vue';
import HandbookProgress from './handbook/HandbookProgress.vue';
import HandbookWorkspace from './handbook/HandbookWorkspace.vue';

const query = ref('');
const activeChapterId = ref(handbookChapters[0]?.id ?? '');
const dialog = ref<'versions' | 'readers' | 'edit' | null>(null);

const visibleChapters = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  if (!keyword) return handbookChapters;
  return handbookChapters
    .map((chapter) => ({
      ...chapter,
      clauses: chapter.clauses.filter((clause) =>
        `${clause.number}${clause.title}${clause.content}`.toLowerCase().includes(keyword),
      ),
    }))
    .filter((chapter) => chapter.title.toLowerCase().includes(keyword) || chapter.clauses.length > 0);
});

const activeChapter = computed<HandbookChapter>(() => {
  return (
    visibleChapters.value.find((chapter) => chapter.id === activeChapterId.value) ??
    visibleChapters.value[0] ??
    handbookChapters[0]!
  );
});

function exportPdf() {
  toast.success('员工手册 PDF 已生成到本地下载队列');
}

function printHandbook() {
  window.print();
}

function saveDraft() {
  dialog.value = null;
  toast.success('手册修订草稿已保存在本地');
}

function setDialogOpen(open: boolean) {
  if (!open) dialog.value = null;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/handbook"
    data-page-label="员工手册"
    data-page-module="人事管理"
    data-handbook-page
    :data-active-chapter="activeChapter.id"
    :data-visible-chapters="visibleChapters.length"
  >
    <HandbookHero />
    <section :class="$style.toolbar">
      <p><i />版本 <strong>V3.2</strong><span>· 发布于 2026-02-01</span><b />修订人 孙丽萍</p>
      <div>
        <UiButton
          size="sm"
          @click="dialog = 'versions'"
          ><template #leading
            ><History
              :size="14"
              :stroke-width="1.8" /></template
          >版本记录</UiButton
        >
        <UiButton
          size="sm"
          @click="dialog = 'readers'"
          ><template #leading
            ><BarChart3
              :size="14"
              :stroke-width="1.8" /></template
          >阅读看板</UiButton
        >
        <UiButton
          size="sm"
          @click="printHandbook"
          ><template #leading
            ><Printer
              :size="14"
              :stroke-width="1.8" /></template
          >打印</UiButton
        >
        <UiButton
          size="sm"
          @click="exportPdf"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出PDF</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="dialog = 'edit'"
          ><template #leading
            ><Edit3
              :size="14"
              :stroke-width="1.8" /></template
          >编辑手册</UiButton
        >
      </div>
    </section>
    <HandbookProgress />
    <HandbookWorkspace
      v-model:query="query"
      :chapters="visibleChapters"
      :active-chapter="activeChapter"
      @select="activeChapterId = $event"
    />
  </main>

  <UiDialog
    :open="dialog !== null"
    title="员工手册管理"
    description="版本、阅读与本地修订信息"
    size="md"
    @update:open="setDialogOpen"
  >
    <div
      v-if="dialog === 'versions'"
      :class="$style.dialogList"
    >
      <p><strong>V3.2</strong><span>当前版本 · 2026-02-01 · 孙丽萍</span></p>
      <p><strong>V3.1</strong><span>2025-08-15 · 更新考勤与假期制度</span></p>
      <p><strong>V3.0</strong><span>2025-01-01 · 完成员工手册年度修订</span></p>
    </div>
    <div
      v-else-if="dialog === 'readers'"
      :class="$style.readerBoard"
    >
      <strong>74%</strong><span>全员已读率</span
      ><progress
        value="74"
        max="100"
      >
        74%
      </progress>
      <p>已读 8 人 · 未完成 3 人 · 重要条款签字率 100%</p>
    </div>
    <div
      v-else
      :class="$style.editForm"
    >
      <label
        >修订版本<UiInput
          model-value="V3.3"
          aria-label="修订版本"
      /></label>
      <label
        >修订说明<UiInput
          model-value="年度制度与安全条款更新"
          aria-label="修订说明"
      /></label>
      <p>编辑内容将保存为本地草稿，不会影响参考站数据。</p>
    </div>
    <template #footer>
      <UiButton
        size="sm"
        @click="dialog = null"
        >取消</UiButton
      >
      <UiButton
        v-if="dialog === 'edit'"
        variant="primary"
        size="sm"
        @click="saveDraft"
        >保存草稿</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.toolbar {
  @apply flex h-[62px] items-center justify-between;
}
.toolbar > p {
  @apply m-0 flex items-center gap-2 text-[10px];
  color: var(--ui-text-muted);
}
.toolbar > p i {
  @apply h-2 w-2 rounded-full;
  background: var(--ui-success);
}
.toolbar > p strong {
  @apply text-[11px];
  color: var(--ui-text);
}
.toolbar > p b {
  @apply mx-1 h-4 w-px;
  background: var(--ui-border);
}
.toolbar > div {
  @apply flex gap-2;
}
.dialogList {
  @apply space-y-2 p-5;
}
.dialogList p {
  @apply m-0 flex items-center justify-between rounded border px-4 py-3 text-[11px];
  border-color: var(--ui-border-subtle);
}
.dialogList span {
  color: var(--ui-text-muted);
}
.readerBoard {
  @apply flex flex-col gap-2 p-6;
}
.readerBoard > strong {
  @apply text-[28px];
  color: var(--app-primary);
}
.readerBoard > span,
.readerBoard > p {
  @apply m-0 text-[10px];
  color: var(--ui-text-muted);
}
.readerBoard progress {
  @apply h-2 w-full;
  accent-color: var(--app-primary);
}
.editForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.editForm label {
  @apply text-[10px] font-semibold;
  color: var(--ui-text-muted);
}
.editForm label > * {
  @apply mt-2;
}
.editForm p {
  @apply col-span-2 m-0 rounded px-3 py-2 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
</style>
