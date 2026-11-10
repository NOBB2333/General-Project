<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { completionDocsModel } from '@/features/engineering-projects/model/completion-docs.model';
</script>

<template>
  <ReferenceListPageSurface :model="completionDocsModel" />
</template>
