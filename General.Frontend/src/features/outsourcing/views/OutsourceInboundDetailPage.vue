<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceInboundDetailModel } from '@/features/outsourcing/model/outsource-inbound-detail.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceInboundDetailModel" />
</template>
