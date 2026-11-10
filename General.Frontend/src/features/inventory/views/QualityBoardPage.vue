<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { qualityBoardModel } from '@/features/inventory/model/quality-board.model';
</script>

<template>
  <ReferenceListPageSurface :model="qualityBoardModel" />
</template>
