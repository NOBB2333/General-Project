<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { thirdPartyModel } from '@/features/service-partners/model/third-party.model';
</script>

<template>
  <ReferenceListPageSurface :model="thirdPartyModel" />
</template>
