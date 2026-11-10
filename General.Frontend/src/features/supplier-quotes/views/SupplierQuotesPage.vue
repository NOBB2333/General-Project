<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { supplierQuotesModel } from '@/features/supplier-quotes/model/supplier-quotes.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="supplierQuotesModel" />
</template>
