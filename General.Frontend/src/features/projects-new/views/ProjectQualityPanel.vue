<script setup lang="ts">
import { computed, ref } from 'vue';
import type { ProjectApi } from '@/api/core/project';
import { UiButton } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';
import ProjectQualityChecklistDialog from './ProjectQualityChecklistDialog.vue';
import ProjectQualityInspectionDialog from './ProjectQualityInspectionDialog.vue';

const props = defineProps<{
  detail: ProjectApi.Detail;
  projectId: string;
}>();

const emit = defineEmits<{
  changed: [];
  openIssues: [];
}>();

const checklistDialogOpen = ref(false);
const inspectionDialogOpen = ref(false);
const canManageQuality = computed(() => hasAccessCode('Project.Quality.Manage'));
const checklists = computed(() => props.detail.qualityChecklists);
const inspections = computed(() => props.detail.qualityInspections);

function formatDateTime(value: string): string {
  return value ? value.slice(0, 16).replace('T', ' ') : '—';
}

function inspectionLabel(value: string): string {
  return value || '未提供结果';
}

function changed() {
  emit('changed');
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目质量检查"
  >
    <header :class="$style.header">
      <div>
        <h3 :class="$style.heading">质量检查单与检查记录</h3>
        <p :class="$style.note">检查结果和整改要求按后端记录原样展示；整改事项可关联到项目问题并通过问题状态跟踪。</p>
      </div>
      <div
        v-if="canManageQuality"
        :class="$style.actions"
      >
        <UiButton
          variant="secondary"
          size="sm"
          @click="checklistDialogOpen = true"
        >
          新建检查单
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          :disabled="checklists.length === 0"
          @click="inspectionDialogOpen = true"
        >
          登记检查
        </UiButton>
      </div>
    </header>

    <h4 :class="$style.subheading">检查单（{{ checklists.length }}）</h4>
    <div
      v-if="checklists.length"
      :class="$style.list"
    >
      <article
        v-for="checklist in checklists"
        :key="checklist.id"
        :class="$style.item"
      >
        <div :class="$style.itemMain">
          <div :class="$style.titleRow">
            <h5 :class="$style.title">{{ checklist.title }}</h5>
            <span :class="$style.status">{{ checklist.category }}</span>
          </div>
          <p :class="$style.meta">{{ checklist.description }}</p>
          <p :class="$style.meta">检查记录数：{{ checklist.inspectionCount }}</p>
        </div>
      </article>
    </div>
    <p
      v-else
      :class="$style.empty"
    >
      当前项目没有可见的质量检查单。
    </p>

    <div :class="$style.historyHeader">
      <h4 :class="$style.subheading">检查记录（{{ inspections.length }}）</h4>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('openIssues')"
      >
        去问题与风险分区跟踪整改
      </UiButton>
    </div>
    <div
      v-if="inspections.length"
      :class="$style.list"
    >
      <article
        v-for="inspection in inspections"
        :key="inspection.id"
        :class="$style.item"
      >
        <div :class="$style.itemMain">
          <div :class="$style.titleRow">
            <h5 :class="$style.title">{{ inspection.qualityChecklistTitle }}</h5>
            <span :class="$style.status">{{ inspectionLabel(inspection.inspectionResult) }}</span>
          </div>
          <p :class="$style.meta">
            {{ formatDateTime(inspection.inspectionTime) }} · 检查人 {{ inspection.inspectorName || '—' }}
          </p>
          <p
            v-if="inspection.findingSummary"
            :class="$style.meta"
          >
            发现：{{ inspection.findingSummary }}
          </p>
          <p
            v-if="inspection.rectificationRequirement"
            :class="$style.meta"
          >
            整改要求：{{ inspection.rectificationRequirement }}
          </p>
        </div>
      </article>
    </div>
    <p
      v-else
      :class="$style.empty"
    >
      当前项目没有质量检查记录。
    </p>

    <ProjectQualityChecklistDialog
      v-model:open="checklistDialogOpen"
      :project-id="projectId"
      @saved="changed"
    />
    <ProjectQualityInspectionDialog
      v-model:open="inspectionDialogOpen"
      :detail="detail"
      :project-id="projectId"
      @saved="changed"
    />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply mt-4 space-y-3;
}

.header,
.historyHeader {
  @apply flex flex-wrap items-start justify-between gap-3;
}

.heading,
.subheading,
.title {
  @apply font-medium;
}

.heading {
  @apply text-sm;
}

.subheading {
  @apply text-xs;
}

.note,
.meta,
.empty {
  @apply text-[11px];
  color: var(--ui-muted-foreground);
}

.actions {
  @apply flex flex-wrap gap-2;
}

.list {
  @apply grid gap-2;
}

.item {
  @apply flex flex-wrap items-start justify-between gap-3 rounded border p-3;
  border-color: var(--ui-border);
}

.itemMain {
  @apply min-w-0 flex-1;
}

.titleRow {
  @apply flex flex-wrap items-center gap-2;
}

.title {
  @apply text-xs;
}

.status {
  @apply rounded border px-2 py-0.5 text-[10px];
  border-color: var(--ui-border);
  color: var(--ui-muted-foreground);
}

.meta {
  @apply mt-1 break-words;
}

.empty {
  @apply rounded border border-dashed p-4 text-center;
  border-color: var(--ui-border);
}
</style>
