<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { perfPlansModel } from '@/features/sales-performance/model/perf-plans.model';
</script>

<template>
  <ReferenceListPageSurface :model="perfPlansModel" />
</template>
