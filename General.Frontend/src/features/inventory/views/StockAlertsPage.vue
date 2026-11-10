<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { stockAlertsModel } from '@/features/inventory/model/stock-alerts.model';
</script>

<template>
  <ReferenceListPageSurface :model="stockAlertsModel" />
</template>
