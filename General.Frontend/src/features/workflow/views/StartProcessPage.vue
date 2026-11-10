<script setup lang="ts">
import { startProcessModel } from '@/features/workflow/model/start-process.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="startProcessModel" />
</template>
