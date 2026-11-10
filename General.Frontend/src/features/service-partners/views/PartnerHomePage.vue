<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { partnerHomeModel } from '@/features/service-partners/model/partner-home.model';
</script>

<template>
  <ReferenceListPageSurface :model="partnerHomeModel" />
</template>
