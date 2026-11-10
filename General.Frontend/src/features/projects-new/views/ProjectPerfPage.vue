<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectPerfModel } from '@/features/projects-new/model/project-perf.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectPerfModel" />
</template>
