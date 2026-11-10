<script setup lang="ts">
import DomainScaffoldSurface from './scaffold/DomainScaffoldSurface.vue';
import type { PageModel } from './page.types';

/**
 * @deprecated Compatibility entry for existing generated pages.
 * New scaffolds should import DomainScaffoldSurface explicitly; verified pages
 * should use a page family component or a dedicated feature SFC.
 */
defineProps<{ model: PageModel }>();
</script>

<template>
  <DomainScaffoldSurface :model="model" />
</template>
