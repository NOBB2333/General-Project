<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { rfqQuoteModel } from '@/features/supplier-portal/model/rfq-quote.model';
</script>

<template>
  <ReferenceListPageSurface :model="rfqQuoteModel" />
</template>
