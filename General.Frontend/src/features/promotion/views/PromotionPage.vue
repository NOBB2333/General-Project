<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { promotionModel } from '@/features/promotion/model/promotion.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="promotionModel" />
</template>
