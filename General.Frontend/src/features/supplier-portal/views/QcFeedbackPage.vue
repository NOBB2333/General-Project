<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { qcFeedbackModel } from '@/features/supplier-portal/model/qc-feedback.model';
</script>

<template>
  <ReferenceListPageSurface :model="qcFeedbackModel" />
</template>
