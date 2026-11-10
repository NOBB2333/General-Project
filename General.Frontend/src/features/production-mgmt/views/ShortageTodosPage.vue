<script setup lang="ts">
import { ReferenceListPageSurface } from '@/components/page';
import { shortageTodosModel } from '@/features/production-mgmt/model/shortage-todos.model';
</script>

<template>
  <ReferenceListPageSurface :model="shortageTodosModel" />
</template>
