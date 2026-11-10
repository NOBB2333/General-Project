<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectsModel } from '@/features/business-settings/model/projects.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectsModel" />
</template>
