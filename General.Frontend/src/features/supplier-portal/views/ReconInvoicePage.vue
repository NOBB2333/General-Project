<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { reconInvoiceModel } from '@/features/supplier-portal/model/recon-invoice.model';
</script>

<template>
  <ReferenceListPageSurface :model="reconInvoiceModel" />
</template>
