<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { qualityAnalyticsModel } from '@/features/inventory/model/quality-analytics.model';
</script>

<template>
  <ReferenceListPageSurface :model="qualityAnalyticsModel" />
</template>
