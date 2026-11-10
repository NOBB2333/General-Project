<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectCollectionsModel } from '@/features/projects-new/model/project-collections.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectCollectionsModel" />
</template>
