<script setup lang="ts">
import { myTodosModel } from '@/features/workflow/model/my-todos.model';
import WorkflowPageSurface from './WorkflowPageSurface.vue';
</script>

<template>
  <WorkflowPageSurface :model="myTodosModel" />
</template>
