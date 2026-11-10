<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { subcontractModel } from '@/features/engineering-projects/model/subcontract.model';
</script>

<template>
  <ReferenceListPageSurface :model="subcontractModel" />
</template>
