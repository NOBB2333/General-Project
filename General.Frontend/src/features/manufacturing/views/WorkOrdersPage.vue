<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { workOrdersModel } from '@/features/manufacturing/model/work-orders.model';
</script>

<template>
  <ReferenceListPageSurface :model="workOrdersModel" />
</template>
