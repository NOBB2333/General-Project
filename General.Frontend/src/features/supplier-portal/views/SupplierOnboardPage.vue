<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { supplierOnboardModel } from '@/features/supplier-portal/model/supplier-onboard.model';
</script>

<template>
  <ReferenceListPageSurface :model="supplierOnboardModel" />
</template>
