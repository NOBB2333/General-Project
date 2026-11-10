<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { actualCostModel } from '@/features/engineering-projects/model/actual-cost.model';
</script>

<template>
  <ReferenceListPageSurface :model="actualCostModel" />
</template>
