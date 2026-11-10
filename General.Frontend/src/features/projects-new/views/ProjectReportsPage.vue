<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectReportsModel } from '@/features/projects-new/model/project-reports.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectReportsModel" />
</template>
