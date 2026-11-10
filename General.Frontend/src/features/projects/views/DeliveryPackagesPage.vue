<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { deliveryPackagesModel } from '@/features/projects/model/delivery-packages.model';
</script>

<template>
  <ReferenceListPageSurface :model="deliveryPackagesModel" />
</template>
