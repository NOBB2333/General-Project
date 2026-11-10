<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { batchTraceModel } from '@/features/inventory/model/batch-trace.model';
</script>

<template>
  <ReferenceListPageSurface :model="batchTraceModel" />
</template>
