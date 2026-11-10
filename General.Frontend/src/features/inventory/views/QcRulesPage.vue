<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { qcRulesModel } from '@/features/inventory/model/qc-rules.model';
</script>

<template>
  <ReferenceListPageSurface :model="qcRulesModel" />
</template>
