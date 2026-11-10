<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { partnerReconModel } from '@/features/service-partners/model/partner-recon.model';
</script>

<template>
  <ReferenceListPageSurface :model="partnerReconModel" />
</template>
