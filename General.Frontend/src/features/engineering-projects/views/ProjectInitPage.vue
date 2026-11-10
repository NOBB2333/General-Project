<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectInitModel } from '@/features/engineering-projects/model/project-init.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectInitModel" />
</template>
