<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceReconModel } from '@/features/outsourcing/model/outsource-recon.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceReconModel" />
</template>
