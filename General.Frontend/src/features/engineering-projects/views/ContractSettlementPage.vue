<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { contractSettlementModel } from '@/features/engineering-projects/model/contract-settlement.model';
</script>

<template>
  <ReferenceListPageSurface :model="contractSettlementModel" />
</template>
