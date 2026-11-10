<script setup lang="ts">
import { computed } from 'vue';
import type { PageAction } from '@/components/page';
import { ListPageSurface } from '@/components/page';
import type { BusinessApi } from '@/api/core/business';
import { buildBusinessProjectListModel, type BusinessProjectRow } from '@/features/business/model/business.models';

const props = withDefaults(
  defineProps<{
    error: string;
    loading: boolean;
    projects: readonly BusinessApi.ProjectSummary[];
    remoteSearch?: boolean;
    searchQuery?: string;
  }>(),
  { remoteSearch: false, searchQuery: '' },
);

const emit = defineEmits<{
  action: [action: PageAction];
  row: [row: BusinessProjectRow];
  'search-change': [query: string];
}>();

const model = computed(() =>
  buildBusinessProjectListModel(
    props.projects,
    props.loading,
    props.error,
    props.remoteSearch ? Math.max(props.projects.length, 1) : 10,
  ),
);

const remotePagination = computed(() =>
  props.remoteSearch
    ? {
        currentPage: 1,
        pageSize: Math.max(props.projects.length, 1),
        searchQuery: props.searchQuery,
        totalCount: props.projects.length,
      }
    : undefined,
);

function handleAction(action: PageAction) {
  emit('action', action);
}

function handleSearch(query: string) {
  emit('search-change', query);
}
</script>

<template>
  <ListPageSurface
    :model="model"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @row="emit('row', $event)"
    @search-change="handleSearch"
  />
</template>
