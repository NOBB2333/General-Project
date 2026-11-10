<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { prodWorkbenchModel } from '@/features/manufacturing/model/prod-workbench.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="prodWorkbenchModel" />
</template>
