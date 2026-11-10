<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import type { PageAction } from '@/components/page';
import type { BusinessApi } from '@/api/core/business';
import { getBusinessProjectsApi } from '@/api/core/business';
import type { BusinessProjectRow } from '@/features/business/model/business.models';
import BusinessProjectListSurface from './BusinessProjectListSurface.vue';
import ProjectDetailDialog from '@/features/projects-new/views/ProjectDetailDialog.vue';

const projects = ref<BusinessApi.ProjectSummary[]>([]);
const loading = ref(false);
const error = ref('');
const searchQuery = ref('');
const selectedProject = ref<BusinessApi.ProjectSummary | null>(null);
const detailOpen = ref(false);

let activeRequest: AbortController | null = null;
let searchTimer: ReturnType<typeof setTimeout> | undefined;
let requestVersion = 0;

async function loadProjects() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';

  try {
    projects.value = await getBusinessProjectsApi(
      { keyword: searchQuery.value.trim() || undefined },
      controller.signal,
    );
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    projects.value = [];
    error.value = cause instanceof Error ? cause.message : '经营项目读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function handleSearchChange(query: string) {
  searchQuery.value = query;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void loadProjects(), 250);
}

function handleAction(action: PageAction) {
  if (action.kind === 'refresh') void loadProjects();
}

function openProject(row: BusinessProjectRow) {
  const project = projects.value.find((item) => item.id === row.id);
  if (!project) return;
  selectedProject.value = project;
  detailOpen.value = true;
}

function handleDetailClose(open: boolean) {
  detailOpen.value = open;
  if (!open) selectedProject.value = null;
}

onMounted(() => void loadProjects());
onBeforeUnmount(() => {
  activeRequest?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});
</script>

<template>
  <BusinessProjectListSurface
    :error="error"
    :loading="loading"
    :projects="projects"
    remote-search
    :search-query="searchQuery"
    @action="handleAction"
    @row="openProject"
    @search-change="handleSearchChange"
  />

  <ProjectDetailDialog
    :open="detailOpen"
    :business-only="true"
    :project-id="selectedProject?.id ?? ''"
    :project-name="selectedProject?.name ?? ''"
    @update:open="handleDetailClose"
  />
</template>
