<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceOpenDetailModel } from '@/features/outsourcing/model/outsource-open-detail.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceOpenDetailModel" />
</template>
