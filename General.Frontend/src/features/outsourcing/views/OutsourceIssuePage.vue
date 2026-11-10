<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { outsourceIssueModel } from '@/features/outsourcing/model/outsource-issue.model';
</script>

<template>
  <ReferenceListPageSurface :model="outsourceIssueModel" />
</template>
