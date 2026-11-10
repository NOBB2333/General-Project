<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { tasksModel } from '@/features/projects/model/tasks.model';
</script>

<template>
  <ReferenceListPageSurface :model="tasksModel" />
</template>
