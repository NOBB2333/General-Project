<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectInvoicingModel } from '@/features/projects-new/model/project-invoicing.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectInvoicingModel" />
</template>
