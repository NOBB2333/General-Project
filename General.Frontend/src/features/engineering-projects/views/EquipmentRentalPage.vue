<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { equipmentRentalModel } from '@/features/engineering-projects/model/equipment-rental.model';
</script>

<template>
  <ReferenceListPageSurface :model="equipmentRentalModel" />
</template>
