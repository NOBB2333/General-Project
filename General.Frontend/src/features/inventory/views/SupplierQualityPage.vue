<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { supplierQualityModel } from '@/features/inventory/model/supplier-quality.model';
</script>

<template>
  <ReferenceListPageSurface :model="supplierQualityModel" />
</template>
