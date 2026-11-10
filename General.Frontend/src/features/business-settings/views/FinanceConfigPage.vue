<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { financeConfigModel } from '@/features/business-settings/model/finance-config.model';
</script>

<template>
  <ReferenceListPageSurface :model="financeConfigModel" />
</template>
