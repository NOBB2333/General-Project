<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { materialReturnsModel } from '@/features/production-mgmt/model/material-returns.model';
</script>

<template>
  <ReferenceListPageSurface :model="materialReturnsModel" />
</template>
