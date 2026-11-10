<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { pendingDeliveryModel } from '@/features/projects-new/model/pending-delivery.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="pendingDeliveryModel" />
</template>
