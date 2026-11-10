<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { agencyMarketModel } from '@/features/projects-new/model/agency-market.model';
</script>

<template>
  <ReferenceListPageSurface :model="agencyMarketModel" />
</template>
