<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { timesheetsModel } from '@/features/projects/model/timesheets.model';
</script>

<template>
  <ReferenceListPageSurface :model="timesheetsModel" />
</template>
