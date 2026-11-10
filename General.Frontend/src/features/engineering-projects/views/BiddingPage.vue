<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { biddingModel } from '@/features/engineering-projects/model/bidding.model';
</script>

<template>
  <ReferenceListPageSurface :model="biddingModel" />
</template>
