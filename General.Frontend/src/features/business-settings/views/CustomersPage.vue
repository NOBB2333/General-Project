<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { customersModel } from '@/features/business-settings/model/customers.model';
</script>

<template>
  <ReferenceListPageSurface :model="customersModel" />
</template>
