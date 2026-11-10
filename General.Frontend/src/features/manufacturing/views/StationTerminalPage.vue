<script setup lang="ts">
import { ReferenceCardsPageSurface } from '@/components/page';
import { stationTerminalModel } from '@/features/manufacturing/model/station-terminal.model';
</script>

<template>
  <ReferenceCardsPageSurface :model="stationTerminalModel" />
</template>
