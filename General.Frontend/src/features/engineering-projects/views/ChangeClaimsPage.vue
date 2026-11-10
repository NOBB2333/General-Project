<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { changeClaimsModel } from '@/features/engineering-projects/model/change-claims.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="changeClaimsModel" />
</template>
