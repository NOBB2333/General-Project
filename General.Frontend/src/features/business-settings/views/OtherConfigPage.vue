<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { otherConfigModel } from '@/features/business-settings/model/other-config.model';
</script>

<template>
  <ReferenceListPageSurface :model="otherConfigModel" />
</template>
