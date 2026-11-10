<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { workCentersModel } from '@/features/manufacturing/model/work-centers.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="workCentersModel" />
</template>
