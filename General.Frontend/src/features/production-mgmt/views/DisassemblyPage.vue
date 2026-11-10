<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { disassemblyModel } from '@/features/production-mgmt/model/disassembly.model';
</script>

<template>
  <ReferenceListPageSurface :model="disassemblyModel" />
</template>
