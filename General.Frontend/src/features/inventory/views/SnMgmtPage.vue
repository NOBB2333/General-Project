<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { snMgmtModel } from '@/features/inventory/model/sn-mgmt.model';
</script>

<template>
  <ReferenceListPageSurface :model="snMgmtModel" />
</template>
