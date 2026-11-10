<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { prodAnalyticsModel } from '@/features/manufacturing/model/prod-analytics.model';
</script>

<template>
  <ReferenceListPageSurface :model="prodAnalyticsModel" />
</template>
