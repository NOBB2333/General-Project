<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { shelfLifeModel } from '@/features/food-beverage/model/shelf-life.model';
</script>

<template>
  <ReferenceListPageSurface :model="shelfLifeModel" />
</template>
