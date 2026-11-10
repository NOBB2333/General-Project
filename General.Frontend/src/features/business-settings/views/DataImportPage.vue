<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { dataImportModel } from '@/features/business-settings/model/data-import.model';
</script>

<template>
  <ReferenceListPageSurface :model="dataImportModel" />
</template>
