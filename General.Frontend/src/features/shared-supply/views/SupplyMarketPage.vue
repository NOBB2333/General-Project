<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { supplyMarketModel } from '@/features/shared-supply/model/supply-market.model';
</script>

<template>
  <ReferenceListPageSurface :model="supplyMarketModel" />
</template>
