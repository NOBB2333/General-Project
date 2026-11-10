<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { serviceTicketsModel } from '@/features/service-partners/model/service-tickets.model';
</script>

<template>
  <ReferenceListPageSurface :model="serviceTicketsModel" />
</template>
