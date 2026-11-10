<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceStockModel } from '@/features/outsourcing/model/outsource-stock.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceStockModel" />
</template>
