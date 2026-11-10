<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { projectContractsModel } from '@/features/projects-new/model/project-contracts.model';
</script>

<template>
  <ReferenceListPageSurface :model="projectContractsModel" />
</template>
