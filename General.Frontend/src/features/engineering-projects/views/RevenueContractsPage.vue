<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { revenueContractsModel } from '@/features/engineering-projects/model/revenue-contracts.model';
</script>

<template>
  <ReferenceListPageSurface :model="revenueContractsModel" />
</template>
