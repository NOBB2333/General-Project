<script setup lang="ts">
import { FileText } from 'lucide-vue-next';
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel } from '@/components/page/families/list/list-page.types';
import { toast, UiButton, UiDialog, UiFormField } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { createPlatformFilesApi, type PlatformFileApi } from './platform-files.api';
import StorageSourcesDialog from './StorageSourcesDialog.vue';
import {
  isSafePreviewContentType,
  sanitizeDownloadFileName,
  toPlatformFileRow,
  type PlatformFileRow,
} from './platform-files.model';

const filesApi = createPlatformFilesApi(apiClient);
const files = ref<PlatformFileApi.Item[]>([]);
const currentPage = ref(1);
const pageSize = 20;
const totalCount = ref(0);
const searchQuery = ref('');
const loading = ref(false);
const loadError = ref('');
const uploading = ref(false);
const deleting = ref(false);
const detailFile = ref<PlatformFileApi.Item | null>(null);
const deleteTarget = ref<PlatformFileApi.Item | null>(null);
const detailOpen = ref(false);
const deleteOpen = ref(false);
const storageSourcesOpen = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const canManageFiles = computed(() => canAccess('Platform.File.Manage'));
let searchTimer: number | undefined;
let requestVersion = 0;
let activeRequest: AbortController | undefined;

const rows = computed(() => files.value.map(toPlatformFileRow));
const pageModel = computed<ListPageModel<PlatformFileRow>>(() => ({
  actions: [
    { id: 'refresh', label: loading.value ? '刷新中' : '刷新', kind: 'refresh' },
    ...(canManageFiles.value
      ? [
          {
            id: 'upload',
            label: uploading.value ? '上传中' : '上传文件',
            kind: 'custom' as const,
            tone: 'primary' as const,
          },
          { id: 'storage-sources', label: '管理存储源', kind: 'custom' as const },
        ]
      : []),
  ],
  columns: [
    { key: 'fileName', title: '文件名称', width: 220 },
    { key: 'category', title: '分类', width: 120 },
    { key: 'relativePath', title: '文件路径', width: 240 },
    { key: 'business', title: '关联项目 / 业务', width: 170 },
    { key: 'contentType', title: '文件类型', width: 200 },
    { key: 'size', title: '大小', width: 100 },
    { key: 'uploadedBy', title: '上传人', width: 110 },
    { key: 'uploadedAt', title: '上传时间', width: 170 },
    { key: 'version', title: '版本', width: 110 },
    { key: 'status', title: '版本状态', width: 110, presentation: 'status' },
  ],
  demoPath: '/platform/files',
  description: '查看有权访问的平台文件；上传、下载、预览和删除均由文件服务校验权限。',
  emptyHint: loading.value ? '正在加载文件…' : loadError.value ? '文件加载失败，请通过刷新重试。' : '暂无可查看文件。',
  filters: [],
  key: '平台中心|文件管理',
  label: '文件管理',
  listScopes: [{ id: 'all', label: '全部文件' }],
  listTabs: [{ id: 'all', label: '全部' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-files',
  pageSize,
  panels: [],
  path: '/platform/files',
  primaryAction: '上传文件',
  replication: { status: 'implemented', referenceUrl: '/eng-projects/project-files' },
  routeName: 'platform-files',
  rows: rows.value,
  searchPlaceholder: '搜索文件名 / 类型 / 路径',
  stats: [
    { key: 'total', label: '文件总数', value: String(totalCount.value), tone: 'primary' },
    { key: 'current-page', label: '当前页', value: String(files.value.length), tone: 'neutral' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

async function loadFiles() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await filesApi.getList(
      {
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        skipCount: (currentPage.value - 1) * pageSize,
      },
      controller.signal,
    );
    if (version !== requestVersion) return;

    totalCount.value = result.totalCount;
    const lastPage = Math.max(1, Math.ceil(result.totalCount / pageSize));
    if (currentPage.value > lastPage) {
      currentPage.value = lastPage;
      await loadFiles();
      return;
    }
    files.value = result.items;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return;
    loadError.value = error instanceof Error ? error.message : '文件列表加载失败。';
    toast.error(loadError.value);
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function updateSearch(query: string) {
  searchQuery.value = query;
}

function changePage(page: number) {
  if (page < 1 || page === currentPage.value) return;
  currentPage.value = page;
  void loadFiles();
}

function runAction(actionId: string) {
  if (actionId === 'refresh') {
    void loadFiles();
    return;
  }
  if (actionId === 'upload' && canManageFiles.value && !uploading.value) {
    fileInput.value?.click();
    return;
  }
  if (actionId === 'storage-sources' && canManageFiles.value) {
    storageSourcesOpen.value = true;
  }
}

async function uploadSelectedFile(event: Event) {
  const input = event.currentTarget;
  if (!(input instanceof HTMLInputElement)) return;
  const file = input.files?.[0];
  if (!file || uploading.value || !canManageFiles.value) return;

  uploading.value = true;
  try {
    await filesApi.upload({ file });
    toast.success('文件上传成功。');
    currentPage.value = 1;
    await loadFiles();
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '文件上传失败，请重试。');
  } finally {
    uploading.value = false;
    input.value = '';
  }
}

function openFileDetail(row: PlatformFileRow) {
  detailFile.value = files.value.find((file) => file.id === row.id) ?? null;
  detailOpen.value = detailFile.value !== null;
}

function downloadFile(file: PlatformFileApi.Item) {
  void filesApi
    .getFileContent(file.fileKey, 'download')
    .then((blob) => {
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = sanitizeDownloadFileName(file.fileName);
      anchor.rel = 'noopener';
      anchor.click();
      window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
    })
    .catch((error: unknown) => {
      toast.error(error instanceof Error ? error.message : '文件下载失败，请重试。');
    });
}

function previewFile(file: PlatformFileApi.Item) {
  if (!isSafePreviewContentType(file.contentType)) {
    toast.warning('该文件类型暂不支持安全预览，请下载后查看。');
    return;
  }
  const previewWindow = window.open('about:blank', '_blank');
  if (!previewWindow) {
    toast.warning('浏览器阻止了预览窗口，请允许弹出窗口后重试。');
    return;
  }
  previewWindow.opener = null;

  void filesApi
    .getFileContent(file.fileKey, 'preview')
    .then((blob) => {
      if (!isSafePreviewContentType(blob.type || file.contentType)) {
        previewWindow.close();
        toast.warning('服务端返回的文件类型不支持安全预览，请下载后查看。');
        return;
      }
      const url = URL.createObjectURL(blob);
      previewWindow.location.replace(url);
      window.setTimeout(() => URL.revokeObjectURL(url), 15 * 60 * 1000);
    })
    .catch((error: unknown) => {
      previewWindow.close();
      toast.error(error instanceof Error ? error.message : '文件预览失败，请重试。');
    });
}

function requestDelete(file: PlatformFileApi.Item) {
  if (!canManageFiles.value || deleting.value) return;
  detailOpen.value = false;
  deleteTarget.value = file;
  deleteOpen.value = true;
}

async function confirmDelete() {
  const target = deleteTarget.value;
  if (!target || !canManageFiles.value || deleting.value) return;
  deleting.value = true;
  try {
    await filesApi.deleteFile(target.fileKey);
    deleteOpen.value = false;
    detailFile.value = null;
    deleteTarget.value = null;
    toast.success('文件已移入回收中心。');
    await loadFiles();
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '文件删除失败，请重试。');
  } finally {
    deleting.value = false;
  }
}

watch(searchQuery, () => {
  currentPage.value = 1;
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  searchTimer = window.setTimeout(() => {
    searchTimer = undefined;
    void loadFiles();
  }, 300);
});

onMounted(() => void loadFiles());
onBeforeUnmount(() => {
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  activeRequest?.abort();
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :remote-pagination="{
      currentPage,
      pageSize,
      searchQuery,
      totalCount,
    }"
    @action="(action) => runAction(action.id)"
    @page-change="changePage"
    @row="openFileDetail"
    @search-change="updateSearch"
  >
    <template #hero-icon>
      <FileText
        :size="20"
        :stroke-width="1.8"
      />
    </template>
  </ListPageSurface>

  <input
    ref="fileInput"
    type="file"
    hidden
    aria-label="选择要上传的文件"
    @change="uploadSelectedFile"
  />

  <UiDialog
    v-model:open="detailOpen"
    :title="detailFile?.fileName ?? '文件详情'"
    description="文件读取和下载权限由服务端按文件关联及当前身份校验。"
  >
    <div v-if="detailFile">
      <UiFormField label="分类"
        ><span>{{ detailFile.category }}</span></UiFormField
      >
      <UiFormField label="文件路径"
        ><span>{{ detailFile.relativePath }}</span></UiFormField
      >
      <UiFormField label="关联业务"
        ><span>{{ detailFile.businessObjectName ?? detailFile.businessProjectName ?? '—' }}</span></UiFormField
      >
      <UiFormField label="内容类型"
        ><span>{{ detailFile.contentType }}</span></UiFormField
      >
      <UiFormField label="大小"
        ><span>{{ detailFile.size }} 字节</span></UiFormField
      >
      <UiFormField label="版本"
        ><span
          >第 {{ detailFile.versionNo }} 版{{ detailFile.isCurrentVersion ? '（当前版本）' : '（历史版本）' }}</span
        ></UiFormField
      >
      <UiFormField label="上传人"
        ><span>{{ detailFile.uploadedBy ?? '—' }}</span></UiFormField
      >
      <UiFormField label="上传时间"
        ><span>{{ detailFile.uploadedAt }}</span></UiFormField
      >
    </div>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="!detailFile || !isSafePreviewContentType(detailFile.contentType)"
        @click="detailFile && previewFile(detailFile)"
      >
        预览
      </UiButton>
      <UiButton
        variant="secondary"
        :disabled="!detailFile"
        @click="detailFile && downloadFile(detailFile)"
      >
        下载
      </UiButton>
      <UiButton
        v-if="canManageFiles"
        variant="danger"
        :disabled="!detailFile || deleting"
        @click="detailFile && requestDelete(detailFile)"
      >
        移入回收中心
      </UiButton>
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="deleteOpen"
    title="移入回收中心"
    description="文件记录将进入回收中心；若删除的是项目章程或验收标准的当前版本，服务端会恢复其上一版本。"
  >
    <UiFormField label="待移入文件"
      ><span>{{ deleteTarget?.fileName ?? '—' }}</span></UiFormField
    >
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="deleting"
        @click="deleteOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="danger"
        :disabled="deleting || !deleteTarget"
        @click="confirmDelete"
      >
        {{ deleting ? '处理中…' : '确认移入' }}
      </UiButton>
    </template>
  </UiDialog>

  <StorageSourcesDialog v-model:open="storageSourcesOpen" />
</template>
