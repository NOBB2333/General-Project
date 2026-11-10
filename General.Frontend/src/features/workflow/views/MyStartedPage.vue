<script setup lang="ts">
import { myStartedModel } from '@/features/workflow/model/my-started.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="myStartedModel" />
</template>
