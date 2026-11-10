<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceReturnModel } from '@/features/outsourcing/model/outsource-return.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceReturnModel" />
</template>
