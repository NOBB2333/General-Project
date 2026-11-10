<script setup lang="ts">
import { computed } from 'vue';
import type { ProjectApi } from '@/api/core/project';
import { UiButton } from '@/components/ui';

const props = withDefaults(
  defineProps<{
    description: string;
    emptyHint?: string;
    items: ProjectApi.WorklogItem[];
    limit?: number;
    refreshable?: boolean;
    title: string;
    totalHours?: number;
  }>(),
  { emptyHint: '当前权限范围内没有工时记录。', limit: 0, refreshable: false },
);

const emit = defineEmits<{ refresh: [] }>();
const visibleItems = computed(() => (props.limit ? props.items.slice(0, props.limit) : props.items));

function formatDate(value: string): string {
  return value ? value.slice(0, 10) : '—';
}
</script>

<template>
  <section :class="$style.section">
    <header :class="$style.sectionHeader">
      <div>
        <h3>{{ title }}</h3>
        <p>{{ description }}</p>
      </div>
      <UiButton
        v-if="refreshable"
        variant="ghost"
        size="sm"
        @click="emit('refresh')"
      >
        刷新记录
      </UiButton>
      <strong v-if="totalHours !== undefined">{{ totalHours }} 小时</strong>
    </header>
    <div
      v-if="visibleItems.length"
      :class="$style.worklogList"
    >
      <article
        v-for="item in visibleItems"
        :key="item.id"
        :class="$style.worklog"
      >
        <div>
          <strong>{{ item.userName }}</strong>
          <span>{{ formatDate(item.workDate) }}</span>
          <span>{{ item.hours }} 小时</span>
        </div>
        <small>{{ item.taskTitle || '未关联任务' }}</small>
        <p>{{ item.summary }}</p>
      </article>
    </div>
    <p
      v-else
      :class="$style.empty"
    >
      {{ emptyHint }}
    </p>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";
.section {
  @apply min-w-0 rounded border border-[#DFE1E6] bg-white p-4;
}
.sectionHeader {
  @apply flex items-start justify-between gap-3;
}
.sectionHeader h3 {
  @apply text-[13px] font-semibold text-[#172B4D];
}
.sectionHeader p {
  @apply mt-1 text-[10px] text-[#6B778C];
}
.sectionHeader > strong {
  @apply shrink-0 text-[12px] font-semibold text-[#172B4D];
}
.worklogList {
  @apply mt-3 space-y-2;
}
.worklog {
  @apply border-t border-[#F4F5F7] py-2 text-[11px];
}
.worklog > div {
  @apply flex flex-wrap gap-x-3 gap-y-1;
}
.worklog strong {
  @apply font-medium text-[#172B4D];
}
.worklog span,
.worklog small {
  @apply text-[#6B778C];
}
.worklog small {
  @apply mt-1 block text-[10px];
}
.worklog p {
  @apply mt-1 text-[#42526E];
}
.empty {
  @apply mt-3 rounded border border-dashed border-[#DFE1E6] px-3 py-5 text-center text-[11px] text-[#6B778C];
}
</style>
