<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { projectAnalyticsModel } from '@/features/projects/model/project-analytics.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="projectAnalyticsModel" />
</template>
