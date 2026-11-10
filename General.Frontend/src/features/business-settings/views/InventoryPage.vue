<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { inventoryModel } from '@/features/business-settings/model/inventory.model';
</script>

<template>
  <ReferenceListPageSurface :model="inventoryModel" />
</template>
