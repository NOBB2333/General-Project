<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { collabOverviewModel } from '@/features/supplier-portal/model/collab-overview.model';
</script>

<template>
  <ReferenceListPageSurface :model="collabOverviewModel" />
</template>
