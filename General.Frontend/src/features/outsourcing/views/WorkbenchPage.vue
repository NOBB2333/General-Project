<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { workbenchModel } from '@/features/outsourcing/model/workbench.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="workbenchModel" />
</template>
