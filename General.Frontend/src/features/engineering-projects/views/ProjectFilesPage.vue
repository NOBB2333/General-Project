<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectFilesModel } from '@/features/engineering-projects/model/project-files.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectFilesModel" />
</template>
