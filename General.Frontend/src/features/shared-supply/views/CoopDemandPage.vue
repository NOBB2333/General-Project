<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { coopDemandModel } from '@/features/shared-supply/model/coop-demand.model';
</script>

<template>
  <ReferenceListPageSurface :model="coopDemandModel" />
</template>
