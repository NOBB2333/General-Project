<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { carriersModel } from '@/features/service-partners/model/carriers.model';
</script>

<template>
  <ReferenceListPageSurface :model="carriersModel" />
</template>
