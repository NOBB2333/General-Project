<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { materialRequisitionsModel } from '@/features/production-mgmt/model/material-requisitions.model';
</script>

<template>
  <ReferenceListPageSurface :model="materialRequisitionsModel" />
</template>
