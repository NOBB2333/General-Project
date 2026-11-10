<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { purchaseSalesModel } from '@/features/business-settings/model/purchase-sales.model';
</script>

<template>
  <ReferenceListPageSurface :model="purchaseSalesModel" />
</template>
