<script setup lang="ts">
import { processDefsModel } from '@/features/workflow/model/process-defs.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="processDefsModel" />
</template>
