<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { targetCostModel } from '@/features/engineering-projects/model/target-cost.model';
</script>

<template>
  <ReferenceListPageSurface :model="targetCostModel" />
</template>
