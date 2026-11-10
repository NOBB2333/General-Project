<script setup lang="ts">
import type { ProjectApi } from '@/api/core/project';
import ProjectMemberResourcesTable from './ProjectMemberResourcesTable.vue';
import ProjectScheduleCalendar from './ProjectScheduleCalendar.vue';

defineProps<{ detail: ProjectApi.Detail }>();

const emit = defineEmits<{ changed: [] }>();
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目成员、资源与排期"
  >
    <ProjectMemberResourcesTable
      :detail="detail"
      @changed="emit('changed')"
    />
    <ProjectScheduleCalendar :items="detail.calendarItems" />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply space-y-5 border-t pt-4;
}
</style>
