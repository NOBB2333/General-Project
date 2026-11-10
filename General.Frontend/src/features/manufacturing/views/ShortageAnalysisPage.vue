<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { shortageAnalysisModel } from '@/features/manufacturing/model/shortage-analysis.model';
</script>

<template>
  <ReferenceListPageSurface :model="shortageAnalysisModel" />
</template>
