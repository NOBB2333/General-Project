<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { productsModel } from '@/features/business-settings/model/products.model';
</script>

<template>
  <ReferenceListPageSurface :model="productsModel" />
</template>
