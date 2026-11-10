<script setup lang="ts">
import type { ProjectApi } from '@/api/core/project';
import ProjectChangeRequestsPanel from './ProjectChangeRequestsPanel.vue';
import ProjectCharterPanel from './ProjectCharterPanel.vue';
import ProjectStatusActions from './ProjectStatusActions.vue';

defineProps<{
  detail: ProjectApi.Detail;
  projectId: string;
}>();

const emit = defineEmits<{ changed: [] }>();
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目章程与治理"
  >
    <ProjectCharterPanel
      :project-id="projectId"
      @changed="emit('changed')"
    />
    <ProjectChangeRequestsPanel
      :project-id="projectId"
      :requests="detail.changeRequests"
      @changed="emit('changed')"
    />
    <ProjectStatusActions
      :project-id="projectId"
      :status="detail.status"
      @changed="emit('changed')"
    />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply space-y-5 border-t pt-4;
}
</style>
