<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { outsourceSuppliersModel } from '@/features/outsourcing/model/outsource-suppliers.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="outsourceSuppliersModel" />
</template>
