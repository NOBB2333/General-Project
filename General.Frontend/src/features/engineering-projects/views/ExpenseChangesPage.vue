<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { expenseChangesModel } from '@/features/engineering-projects/model/expense-changes.model';
</script>

<template>
  <ReferenceListPageSurface :model="expenseChangesModel" />
</template>
