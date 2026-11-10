<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceMaterialReturnModel } from '@/features/outsourcing/model/outsource-material-return.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceMaterialReturnModel" />
</template>
