<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceOrdersModel } from '@/features/outsourcing/model/outsource-orders.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceOrdersModel" />
</template>
