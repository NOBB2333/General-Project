<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { materialPurchaseModel } from '@/features/engineering-projects/model/material-purchase.model';
</script>

<template>
  <ReferenceListPageSurface :model="materialPurchaseModel" />
</template>
