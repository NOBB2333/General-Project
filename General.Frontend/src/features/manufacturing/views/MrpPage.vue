<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { mrpModel } from '@/features/manufacturing/model/mrp.model';
</script>

<template>
  <ReferenceListPageSurface :model="mrpModel" />
</template>
