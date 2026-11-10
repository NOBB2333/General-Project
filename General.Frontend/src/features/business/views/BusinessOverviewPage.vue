<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import type { PageAction } from '@/components/page';
import { ListPageSurface } from '@/components/page';
import type { BusinessApi } from '@/api/core/business';
import { getBusinessOverviewApi } from '@/api/core/business';
import type { BusinessSignalRow } from '@/features/business/model/business.models';
import { buildBusinessOverviewModel } from '@/features/business/model/business.models';
import ProjectDetailDialog from '@/features/projects-new/views/ProjectDetailDialog.vue';

const overview = ref<BusinessApi.Overview | null>(null);
const loading = ref(false);
const error = ref('');
const selectedProject = ref<{ id: string; name: string } | null>(null);
const detailOpen = ref(false);
const pageModel = computed(() => buildBusinessOverviewModel(overview.value, loading.value, error.value));

let activeRequest: AbortController | null = null;
let requestVersion = 0;

async function loadOverview() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';

  try {
    overview.value = await getBusinessOverviewApi(controller.signal);
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    overview.value = null;
    error.value = cause instanceof Error ? cause.message : '经营总览读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function handleAction(action: PageAction) {
  if (action.kind === 'refresh') void loadOverview();
}

function openProject(row: BusinessSignalRow) {
  selectedProject.value = { id: row.projectId, name: row.projectName };
  detailOpen.value = true;
}

function handleDetailClose(open: boolean) {
  detailOpen.value = open;
  if (!open) selectedProject.value = null;
}

onMounted(() => void loadOverview());
onBeforeUnmount(() => activeRequest?.abort());
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="handleAction"
    @row="openProject"
  >
    <template #notice>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
      <p v-else-if="overview && !overview.canViewSensitiveAmounts">
        敏感金额按当前权限由服务端脱敏；未返回的金额显示为“—”。
      </p>
    </template>
  </ListPageSurface>

  <ProjectDetailDialog
    :open="detailOpen"
    :business-only="true"
    :project-id="selectedProject?.id ?? ''"
    :project-name="selectedProject?.name ?? ''"
    @update:open="handleDetailClose"
  />
</template>
