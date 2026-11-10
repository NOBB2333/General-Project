<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { otherExpensesModel } from '@/features/engineering-projects/model/other-expenses.model';
</script>

<template>
  <ReferenceListPageSurface :model="otherExpensesModel" />
</template>
