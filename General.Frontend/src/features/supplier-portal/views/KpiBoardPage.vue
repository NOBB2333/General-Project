<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { kpiBoardModel } from '@/features/supplier-portal/model/kpi-board.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="kpiBoardModel" />
</template>
