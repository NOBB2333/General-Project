<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { materialSupplementModel } from '@/features/production-mgmt/model/material-supplement.model';
</script>

<template>
  <ReferenceListPageSurface :model="materialSupplementModel" />
</template>
