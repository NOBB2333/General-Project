<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { mgmtBoardModel } from '@/features/engineering-projects/model/mgmt-board.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="mgmtBoardModel" />
</template>
