<script setup lang="ts">
import { processCategoriesModel } from '@/features/workflow/model/process-categories.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="processCategoriesModel" />
</template>
