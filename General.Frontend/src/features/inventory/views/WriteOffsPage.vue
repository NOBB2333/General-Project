<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { writeOffsModel } from '@/features/inventory/model/write-offs.model';
</script>

<template>
  <ReferenceListPageSurface :model="writeOffsModel" />
</template>
