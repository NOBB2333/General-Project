<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectExpensesModel } from '@/features/projects-new/model/project-expenses.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectExpensesModel" />
</template>
