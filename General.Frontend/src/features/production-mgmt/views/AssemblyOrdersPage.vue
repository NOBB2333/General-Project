<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { assemblyOrdersModel } from '@/features/production-mgmt/model/assembly-orders.model';
</script>

<template>
  <ReferenceListPageSurface :model="assemblyOrdersModel" />
</template>
