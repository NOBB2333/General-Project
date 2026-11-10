<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { tendersModel } from '@/features/business-settings/model/tenders.model';
</script>

<template>
  <ReferenceListPageSurface :model="tendersModel" />
</template>
