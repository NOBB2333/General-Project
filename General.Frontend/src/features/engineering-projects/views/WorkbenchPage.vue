<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { workbenchModel } from '@/features/engineering-projects/model/workbench.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="workbenchModel" />
</template>
