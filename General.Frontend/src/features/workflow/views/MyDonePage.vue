<script setup lang="ts">
import { myDoneModel } from '@/features/workflow/model/my-done.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="myDoneModel" />
</template>
