<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { shipCollabModel } from '@/features/supplier-portal/model/ship-collab.model';
</script>

<template>
  <ReferenceListPageSurface :model="shipCollabModel" />
</template>
