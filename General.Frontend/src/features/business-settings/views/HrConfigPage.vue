<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { hrConfigModel } from '@/features/business-settings/model/hr-config.model';
</script>

<template>
  <ReferenceListPageSurface :model="hrConfigModel" />
</template>
