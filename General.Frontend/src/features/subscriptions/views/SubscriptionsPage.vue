<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { subscriptionsModel } from '@/features/subscriptions/model/subscriptions.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="subscriptionsModel" />
</template>
