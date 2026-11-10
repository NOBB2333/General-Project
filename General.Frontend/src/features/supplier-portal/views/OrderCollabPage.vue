<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { orderCollabModel } from '@/features/supplier-portal/model/order-collab.model';
</script>

<template>
  <ReferenceListPageSurface :model="orderCollabModel" />
</template>
