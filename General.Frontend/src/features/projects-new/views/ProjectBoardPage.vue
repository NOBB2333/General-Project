<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import type { PageModel } from '@/components/page';
import { ReferenceBoardPageSurface } from '@/components/page';
import { UiButton } from '@/components/ui';
import {
  getProjectMyRelatedDashboardApi,
  getProjectPmDashboardApi,
  getProjectPmoDashboardApi,
  getProjectWorkspaceApi,
} from '@/api/core/project';
import {
  buildManagerDashboardPageModel,
  buildMyRelatedPageModel,
  buildWorkspacePageModel,
} from '@/features/projects-new/model/project-api-page.models';
import { sessionState } from '@/session/session-store';

const props = withDefaults(defineProps<{ mode?: 'my-related' | 'role-home' }>(), { mode: 'role-home' });
const pageModel = ref<PageModel | null>(null);
const loading = ref(false);
const error = ref('');
let activeRequest: AbortController | null = null;
let requestVersion = 0;

async function loadDashboard() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';

  try {
    if (props.mode === 'my-related' || sessionState.homePath === '/project/my-related') {
      const dashboard = await getProjectMyRelatedDashboardApi(controller.signal);
      if (currentRequest === requestVersion) pageModel.value = buildMyRelatedPageModel(dashboard);
    } else if (sessionState.homePath === '/project/pmo-overview') {
      const dashboard = await getProjectPmoDashboardApi(controller.signal);
      if (currentRequest === requestVersion) pageModel.value = buildManagerDashboardPageModel(dashboard);
    } else if (sessionState.homePath === '/project/pm-dashboard') {
      const dashboard = await getProjectPmDashboardApi(controller.signal);
      if (currentRequest === requestVersion) pageModel.value = buildManagerDashboardPageModel(dashboard);
    } else {
      const workspace = await getProjectWorkspaceApi(controller.signal);
      if (currentRequest === requestVersion) pageModel.value = buildWorkspacePageModel(workspace);
    }
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '项目工作台读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

onMounted(() => void loadDashboard());

onBeforeUnmount(() => {
  requestVersion += 1;
  activeRequest?.abort();
});
</script>

<template>
  <div
    v-if="loading && !pageModel"
    role="status"
    aria-live="polite"
  >
    正在读取项目数据…
  </div>
  <div
    v-else-if="error"
    role="alert"
  >
    <span>{{ error }}</span>
    <UiButton
      variant="secondary"
      size="sm"
      @click="loadDashboard"
    >
      重试
    </UiButton>
  </div>
  <ReferenceBoardPageSurface
    v-else-if="pageModel"
    :model="pageModel"
  />
</template>
