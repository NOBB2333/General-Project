<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { partSwapModel } from '@/features/production-mgmt/model/part-swap.model';
</script>

<template>
  <ReferenceListPageSurface :model="partSwapModel" />
</template>
