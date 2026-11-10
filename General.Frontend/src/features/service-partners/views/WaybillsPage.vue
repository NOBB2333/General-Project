<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { waybillsModel } from '@/features/service-partners/model/waybills.model';
</script>

<template>
  <ReferenceListPageSurface :model="waybillsModel" />
</template>
