<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { vendorsSubsModel } from '@/features/engineering-projects/model/vendors-subs.model';
</script>

<template>
  <ReferenceListPageSurface :model="vendorsSubsModel" />
</template>
