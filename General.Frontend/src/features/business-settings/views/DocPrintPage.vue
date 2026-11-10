<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { docPrintModel } from '@/features/business-settings/model/doc-print.model';
</script>

<template>
  <ReferenceListPageSurface :model="docPrintModel" />
</template>
