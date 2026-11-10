<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { invoicesModel } from '@/features/engineering-projects/model/invoices.model';
</script>

<template>
  <ReferenceListPageSurface :model="invoicesModel" />
</template>
