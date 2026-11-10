<script setup lang="ts">
import { ExternalLink } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { BusinessInquiryTask } from '../../model/inquiries.types';

defineProps<{ tasks: readonly BusinessInquiryTask[] }>();
const emit = defineEmits<{ openModule: []; view: [task: BusinessInquiryTask] }>();
</script>

<template>
  <section>
    <header :class="$style.header">
      <div>
        <h2>业务询价任务</h2>
        <p>销售团队提交的询价需求，采购人员负责报价处理</p>
      </div>
      <UiButton @click="emit('openModule')"
        ><template #leading
          ><ExternalLink
            :size="14"
            :stroke-width="1.8" /></template
        >前往业务询价模块</UiButton
      >
    </header>
    <div :class="$style.stats">
      <article>
        <p>任务总数</p>
        <strong>{{ tasks.length }}</strong>
      </article>
      <article>
        <p>待分配</p>
        <strong :class="$style.warning">{{ tasks.filter((task) => task.status === '待分配').length }}</strong>
      </article>
      <article>
        <p>报价处理中</p>
        <strong>{{ tasks.filter((task) => task.status === '报价中').length }}</strong>
      </article>
      <article>
        <p>预计金额</p>
        <strong>{{ (tasks.reduce((sum, task) => sum + task.estimatedAmount, 0) / 10_000).toFixed(1) }}万</strong>
      </article>
    </div>
    <div :class="$style.list">
      <button
        v-for="task in tasks"
        :key="task.id"
        type="button"
        @click="emit('view', task)"
      >
        <span
          ><strong>{{ task.id }} · {{ task.title }}</strong
          ><small>{{ task.customerName }} · {{ task.materialCount }} 项物料 · 截止 {{ task.deadline }}</small></span
        ><span :class="$style.status">{{ task.status }}</span>
      </button>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.header {
  @apply mb-5 flex items-center justify-between;
}
.header h2 {
  @apply text-[18px] font-semibold;
  color: var(--ui-text);
}
.header p {
  @apply mt-0.5 text-[12px];
  color: var(--ui-text-subtle);
}
.stats {
  @apply mb-5 grid grid-cols-4 gap-3;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats p {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.stats strong {
  @apply mt-1 block text-[22px];
  color: var(--ui-text);
}
.stats .warning {
  color: var(--ui-warning);
}
.list {
  @apply overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.list button {
  @apply flex w-full items-center justify-between border-b px-5 py-4 text-left transition-colors;
  border-color: var(--ui-border-subtle);
}
.list button:hover {
  background: var(--ui-surface-subtle);
}
.list strong,
.list small {
  @apply block;
}
.list strong {
  @apply text-[13px] font-medium;
  color: var(--ui-text);
}
.list small {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.status {
  @apply rounded px-2.5 py-1 text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
