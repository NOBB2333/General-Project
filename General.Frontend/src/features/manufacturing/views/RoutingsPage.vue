<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { routingsModel } from '@/features/manufacturing/model/routings.model';
</script>

<template>
  <ReferenceListPageSurface :model="routingsModel" />
</template>
