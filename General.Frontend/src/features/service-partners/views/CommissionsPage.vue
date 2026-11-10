<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { commissionsModel } from '@/features/service-partners/model/commissions.model';
</script>

<template>
  <ReferenceListPageSurface :model="commissionsModel" />
</template>
