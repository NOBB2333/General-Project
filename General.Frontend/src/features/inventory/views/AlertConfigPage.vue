<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { alertConfigModel } from '@/features/inventory/model/alert-config.model';
</script>

<template>
  <ReferenceListPageSurface :model="alertConfigModel" />
</template>
