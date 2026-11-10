<script setup lang="ts">
import { Archive, ChevronDown, Download, Filter, RefreshCw, RotateCcw, Search } from 'lucide-vue-next';
import { reactive, ref } from 'vue';
import { UiButton, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { DrawingPublishFilters } from '@/features/drawings/model/drawing-publish.types';

const emit = defineEmits<{
  search: [filters: DrawingPublishFilters];
  reset: [];
  history: [];
  export: [];
  refresh: [];
}>();

const open = ref(true);
const expanded = ref(false);
const draft = reactive<DrawingPublishFilters>(emptyFilters());

const statusOptions: UiSelectOption[] = [
  '全部',
  '待发布',
  '待检查',
  '检查通过',
  '待确认发布',
  '已发布',
  '发布失败',
  '已取消',
].map((label) => ({ value: label === '全部' ? '' : label, label }));
const reviewOptions: UiSelectOption[] = ['全部', '审核通过', '审核中', '审核退回'].map((label) => ({
  value: label === '全部' ? '' : label,
  label,
}));
const yesNoOptions: UiSelectOption[] = ['全部', '是', '否'].map((label) => ({
  value: label === '全部' ? '' : label,
  label,
}));

function emptyFilters(): DrawingPublishFilters {
  return {
    drawingNo: '',
    name: '',
    pendingVersion: '',
    publishStatus: '',
    reviewStatus: '',
    currentVersion: '',
    publishedFrom: '',
    publishedTo: '',
    effectiveFrom: '',
    effectiveTo: '',
    majorChange: '',
    hasImpact: '',
  };
}

function submit() {
  emit('search', { ...draft });
}

function reset() {
  Object.assign(draft, emptyFilters());
  emit('reset');
}
</script>

<template>
  <section :class="$style.panel">
    <header :class="$style.header">
      <button
        type="button"
        :class="$style.toggle"
        :aria-expanded="open"
        @click="open = !open"
      >
        <Filter
          :size="15"
          :stroke-width="1.8"
        />
        筛选条件
        <ChevronDown
          :size="14"
          :stroke-width="1.8"
          :class="[$style.chevron, open && $style.chevronOpen]"
        />
      </button>
      <div :class="$style.tools">
        <UiButton
          size="sm"
          @click="emit('history')"
          ><template #leading
            ><Archive
              :size="14"
              :stroke-width="1.8" /></template
          >查看已发布记录</UiButton
        >
        <UiButton
          size="sm"
          @click="emit('export')"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >批量导出</UiButton
        >
        <UiButton
          size="sm"
          aria-label="刷新发布列表"
          title="刷新"
          @click="emit('refresh')"
          ><template #leading
            ><RefreshCw
              :size="14"
              :stroke-width="1.8" /></template
          >刷新</UiButton
        >
      </div>
    </header>

    <form
      v-if="open"
      :class="$style.body"
      @submit.prevent="submit"
    >
      <div :class="$style.primaryGrid">
        <UiFormField label="图号"
          ><UiInput
            v-model="draft.drawingNo"
            placeholder="请输入图号"
        /></UiFormField>
        <UiFormField label="图纸名称"
          ><UiInput
            v-model="draft.name"
            placeholder="请输入图纸名称"
        /></UiFormField>
        <UiFormField label="待发布版本"
          ><UiInput
            v-model="draft.pendingVersion"
            placeholder="请输入版本号"
        /></UiFormField>
        <UiFormField label="发布状态"
          ><UiSelect
            v-model="draft.publishStatus"
            :options="statusOptions"
            label="发布状态"
        /></UiFormField>
      </div>

      <button
        type="button"
        :class="$style.moreButton"
        :aria-expanded="expanded"
        @click="expanded = !expanded"
      >
        <ChevronDown
          :size="13"
          :stroke-width="1.8"
          :class="[$style.chevron, expanded && $style.chevronOpen]"
        />
        {{ expanded ? '收起更多筛选' : '展开更多筛选' }}
      </button>

      <div
        v-if="expanded"
        :class="$style.advancedGrid"
      >
        <UiFormField label="审核状态"
          ><UiSelect
            v-model="draft.reviewStatus"
            :options="reviewOptions"
            label="审核状态"
        /></UiFormField>
        <UiFormField label="当前生效版本"
          ><UiInput
            v-model="draft.currentVersion"
            placeholder="请输入当前版本号"
        /></UiFormField>
        <UiFormField label="发布时间（起）"
          ><UiInput
            v-model="draft.publishedFrom"
            type="date"
        /></UiFormField>
        <UiFormField label="发布时间（止）"
          ><UiInput
            v-model="draft.publishedTo"
            type="date"
        /></UiFormField>
        <UiFormField label="生效日期（起）"
          ><UiInput
            v-model="draft.effectiveFrom"
            type="date"
        /></UiFormField>
        <UiFormField label="生效日期（止）"
          ><UiInput
            v-model="draft.effectiveTo"
            type="date"
        /></UiFormField>
        <UiFormField label="是否重大变更"
          ><UiSelect
            v-model="draft.majorChange"
            :options="yesNoOptions"
            label="是否重大变更"
        /></UiFormField>
        <UiFormField label="是否存在影响对象"
          ><UiSelect
            v-model="draft.hasImpact"
            :options="yesNoOptions"
            label="是否存在影响对象"
        /></UiFormField>
      </div>

      <div :class="$style.formActions">
        <UiButton
          type="submit"
          variant="primary"
          ><template #leading
            ><Search
              :size="14"
              :stroke-width="1.8" /></template
          >查询</UiButton
        >
        <UiButton
          type="button"
          @click="reset"
          ><template #leading
            ><RotateCcw
              :size="14"
              :stroke-width="1.8" /></template
          >重置</UiButton
        >
      </div>
    </form>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply overflow-hidden rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.header {
  @apply flex min-h-12 items-center justify-between gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.toggle {
  @apply flex items-center gap-2 text-[13px] font-semibold;
  color: var(--ui-text-muted);
}
.tools {
  @apply flex items-center gap-2;
}
.body {
  @apply px-4 py-3;
}
.primaryGrid,
.advancedGrid {
  @apply grid grid-cols-4 gap-3;
}
.advancedGrid {
  @apply mt-3 border-t pt-3;
  border-color: var(--ui-border-subtle);
}
.moreButton {
  @apply mt-3 flex items-center gap-1 text-[12px] font-medium;
  color: var(--app-primary);
}
.chevron {
  @apply transition-transform;
}
.chevronOpen {
  transform: rotate(180deg);
}
.formActions {
  @apply mt-4 flex items-center gap-2;
}

@media (max-width: 900px) {
  .header {
    @apply items-start py-2;
  }
  .tools {
    @apply flex-wrap justify-end;
  }
  .primaryGrid,
  .advancedGrid {
    @apply grid-cols-2;
  }
}

@media (prefers-reduced-motion: reduce) {
  .chevron {
    transition: none;
  }
}
</style>
