<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { pendingReviewModel } from '@/features/projects-new/model/pending-review.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="pendingReviewModel" />
</template>
