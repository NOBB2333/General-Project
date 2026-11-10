<script setup lang="ts">
import { SearchPageSurface } from '@/components/page';
import { materialSearchReferenceFixture } from '@/features/base/model/material-search.data';
import { materialSearchModel } from '@/features/base/model/material-search.model';
</script>

<template>
  <SearchPageSurface
    :model="materialSearchModel"
    :rows="materialSearchReferenceFixture.rows"
  />
</template>
