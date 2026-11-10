<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { agentsModel } from '@/features/service-partners/model/agents.model';
</script>

<template>
  <ReferenceListPageSurface :model="agentsModel" />
</template>
