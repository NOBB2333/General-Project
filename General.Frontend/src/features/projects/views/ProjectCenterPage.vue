<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { projectCenterModel } from '@/features/projects/model/project-center.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="projectCenterModel" />
</template>
