<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceStatementsModel } from '@/features/outsourcing/model/outsource-statements.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceStatementsModel" />
</template>
