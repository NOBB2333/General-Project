<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { reportCenterModel } from '@/features/manufacturing/model/report-center.model';
</script>

<template>
  <ReferenceListPageSurface :model="reportCenterModel" />
</template>
