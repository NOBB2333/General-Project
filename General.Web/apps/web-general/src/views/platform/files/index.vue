<script lang="ts" setup>
import type { UploadProps, TreeProps } from 'ant-design-vue';
import type { Dayjs } from 'dayjs';

import { computed, onBeforeUnmount, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  DatePicker,
  Drawer,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Tree,
  Upload,
  message,
} from 'ant-design-vue';

import {
  batchDeleteFileApi,
  createFileStorageSourceApi,
  deleteFileApi,
  deleteFileStorageSourceApi,
  downloadFileApi,
  getFileListApi,
  getFileStorageSourcesApi,
  getFileTreeApi,
  previewFileApi,
  setDefaultFileStorageSourceApi,
  toggleFileStorageSourceApi,
  updateFileMetadataApi,
  updateFileStorageSourceApi,
  uploadFileApi,
  type FileApi,
} from '#/api/core';
import PlatformStatusTag from '#/components/platform/platform-status-tag.vue';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useDateFormatter } from '#/composables/platform/use-date-formatter';

defineOptions({ name: 'PlatformFilePage' });

const FILE_MANAGE_CODE = 'Platform.File.Manage';

const { hasAccessByCodes } = useAccess();
const canManageFiles = computed(() => hasAccessByCodes([FILE_MANAGE_CODE]));

interface FileTreeNode {
  children?: FileTreeNode[];
  isLeaf?: boolean;
  key: string;
  title: string;
}

type FileMetadataFormState = Omit<FileApi.FileMetadataInput, 'businessId' | 'businessType'> & {
  businessId: string;
  businessType: string;
};

type StorageSourceFormState = Omit<
  FileApi.StorageSourceSaveInput,
  | 'accessKeyId'
  | 'bucketName'
  | 'customDomain'
  | 'endpoint'
  | 'pathTemplate'
  | 'region'
  | 'remark'
  | 'rootPath'
  | 'secretKey'
> & {
  accessKeyId: string;
  bucketName: string;
  customDomain: string;
  endpoint: string;
  pathTemplate: string;
  region: string;
  remark: string;
  rootPath: string;
  secretKey: string;
};

const columns = [
  { dataIndex: 'fileName', key: 'fileName', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => left.fileName.localeCompare(right.fileName, 'zh-CN'), title: '文件名' },
  { dataIndex: 'storageSourceName', key: 'storageSourceName', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => (left.storageSourceName || left.storageProvider).localeCompare(right.storageSourceName || right.storageProvider, 'zh-CN'), title: '存储源', width: 150 },
  { dataIndex: 'category', key: 'category', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => left.category.localeCompare(right.category, 'zh-CN'), title: '分类', width: 120 },
  { dataIndex: 'contentType', key: 'contentType', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => left.contentType.localeCompare(right.contentType, 'zh-CN'), title: '类型', width: 220 },
  { dataIndex: 'isPublic', key: 'isPublic', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => Number(left.isPublic) - Number(right.isPublic), title: '公开性', width: 90 },
  { dataIndex: 'size', key: 'size', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => left.size - right.size, title: '大小', width: 120 },
  { dataIndex: 'uploadedBy', key: 'uploadedBy', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => (left.uploadedBy || '').localeCompare(right.uploadedBy || '', 'zh-CN'), title: '上传人', width: 140 },
  { key: 'business', title: '业务关联', width: 180 },
  { dataIndex: 'relativePath', key: 'relativePath', title: '存储位置' },
  { dataIndex: 'uploadedAt', key: 'uploadedAt', sorter: (left: FileApi.FileItem, right: FileApi.FileItem) => new Date(left.uploadedAt).getTime() - new Date(right.uploadedAt).getTime(), title: '上传时间', width: 220 },
  { key: 'actions', title: '操作', width: 210 },
];

const items = ref<FileApi.FileItem[]>([]);
const treeItems = ref<FileApi.TreeItem[]>([]);
const sources = ref<FileApi.StorageSourceItem[]>([]);
const loading = ref(false);
const pageIndex = ref(1);
const pageSize = ref(20);
const totalCount = ref(0);
const sourceLoading = ref(false);
const sourceDrawerOpen = ref(false);
const sourceModalOpen = ref(false);
const editingSourceId = ref('');
const metadataModalOpen = ref(false);
const metadataSaving = ref(false);
const editingFile = ref<FileApi.FileItem | null>(null);
const previewModalOpen = ref(false);
const previewLoading = ref(false);
const previewFile = ref<FileApi.FileItem | null>(null);
const previewUrl = ref('');
const selectedFileKeys = ref<string[]>([]);
const selectedTreeKey = ref<string>('all');
const categoryFilter = ref<string>();
const keyword = ref('');
const storageSourceFilter = ref<string>();
const uploadStorageSourceId = ref<string>();
const uploadedBy = ref('');
const uploadedRange = ref<[Dayjs, Dayjs] | undefined>(undefined);
const { formatDateTime } = useDateFormatter();
const { actionLoadingKey, runAction } = useActionLoading();

const providerOptions = [
  { label: '本地存储', value: 'Local' },
  { label: 'MinIO', value: 'MinIO' },
  { label: '阿里云 OSS', value: 'AliyunOSS' },
];

const sourceForm = reactive<StorageSourceFormState>({
  accessKeyId: '',
  bucketName: '',
  customDomain: '',
  endpoint: '',
  isDefault: false,
  isEnabled: true,
  isPublic: false,
  name: '',
  pathTemplate: '{yyyy}/{MM}/{dd}',
  providerName: 'Local',
  region: '',
  remark: '',
  rootPath: 'App_Data/upload-files',
  secretKey: '',
  useSsl: false,
});

const metadataForm = reactive<FileMetadataFormState>({
  businessId: '',
  businessType: '',
  fileName: '',
  isPublic: false,
});

const sourceColumns = [
  { dataIndex: 'name', key: 'name', title: '名称', width: 160 },
  { dataIndex: 'providerName', key: 'providerName', title: '类型', width: 110 },
  { dataIndex: 'bucketName', key: 'bucketName', title: 'Bucket', width: 150 },
  { dataIndex: 'endpoint', key: 'endpoint', title: 'Endpoint', ellipsis: true, width: 220 },
  { dataIndex: 'isDefault', key: 'isDefault', title: '默认', width: 80 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 260 },
];

const categoryOptions = computed(() =>
  [...new Set(treeItems.value.map((item) => normalizeCategory(item.category)))]
    .sort((left, right) => left.localeCompare(right, 'zh-CN'))
    .map((item) => ({ label: item, value: item })),
);

const enabledSourceOptions = computed(() =>
  sources.value
    .filter((source) => source.isEnabled)
    .map((source) => ({
      label: `${source.name}${source.isDefault ? '（默认）' : ''}`,
      value: source.id,
    })),
);

const sourceFilterOptions = computed(() =>
  sources.value.map((source) => ({
    label: `${source.name}${source.isEnabled ? '' : '（停用）'}`,
    value: source.id,
  })),
);

const directoryTree = computed<FileTreeNode[]>(() => {
  const categoryMap = new Map<string, Set<string>>();
  for (const item of treeItems.value) {
    const category = normalizeCategory(item.category);
    const parentPath = normalizePath(item.parentPath);
    if (!categoryMap.has(category)) {
      categoryMap.set(category, new Set<string>());
    }

    const pathSet = categoryMap.get(category)!;
    pathSet.add('');

    if (!parentPath) {
      continue;
    }

    const segments = parentPath.split('/').filter(Boolean);
    let current = '';
    for (const segment of segments) {
      current = current ? `${current}/${segment}` : segment;
      pathSet.add(current);
    }
  }

  const categoryNodes = [...categoryMap.entries()]
    .sort(([left], [right]) => left.localeCompare(right, 'zh-CN'))
    .map(([category, paths]) => createCategoryNode(category, paths));

  return categoryNodes;
});

const filteredItems = computed(() => {
  return items.value;
});

const selectedFileCount = computed(() => selectedFileKeys.value.length);

const rowSelection = computed(() => {
  if (!canManageFiles.value) {
    return undefined;
  }

  return {
    selectedRowKeys: selectedFileKeys.value,
    onChange: (keys: (number | string)[]) => {
      selectedFileKeys.value = keys.map((key) => `${key}`);
    },
  };
});

async function loadFiles() {
  loading.value = true;
  try {
    const treeFilter = resolveTreeFilter();
    const result = await getFileListApi({
      category: treeFilter.category || categoryFilter.value || undefined,
      keyword: keyword.value || undefined,
      maxResultCount: pageSize.value,
      parentPath: treeFilter.parentPath || undefined,
      skipCount: (pageIndex.value - 1) * pageSize.value,
      storageSourceId: storageSourceFilter.value || undefined,
      uploadedBy: uploadedBy.value || undefined,
      uploadedFrom: uploadedRange.value?.[0]?.startOf('day').toISOString(),
      uploadedTo: uploadedRange.value?.[1]?.endOf('day').toISOString(),
    });
    items.value = result.items;
    totalCount.value = result.totalCount;
    const currentKeys = new Set(result.items.map((item) => item.fileKey));
    selectedFileKeys.value = selectedFileKeys.value.filter((key) => currentKeys.has(key));
  } finally {
    loading.value = false;
  }
}

async function loadTree() {
  treeItems.value = await getFileTreeApi({
    category: categoryFilter.value || undefined,
    keyword: keyword.value || undefined,
    storageSourceId: storageSourceFilter.value || undefined,
    uploadedBy: uploadedBy.value || undefined,
    uploadedFrom: uploadedRange.value?.[0]?.startOf('day').toISOString(),
    uploadedTo: uploadedRange.value?.[1]?.endOf('day').toISOString(),
  });

  if (selectedTreeKey.value !== 'all' && !buildAvailableTreeKeys(treeItems.value).has(selectedTreeKey.value)) {
    selectedTreeKey.value = 'all';
  }
}

async function loadSources(enabledOnly = false) {
  sourceLoading.value = true;
  try {
    sources.value = await getFileStorageSourcesApi(enabledOnly);
    if (!uploadStorageSourceId.value) {
      uploadStorageSourceId.value = sources.value.find((source) => source.isDefault && source.isEnabled)?.id;
    }
  } finally {
    sourceLoading.value = false;
  }
}

async function loadPage() {
  await Promise.all([
    canManageFiles.value ? loadSources() : Promise.resolve(),
    loadTree(),
    loadFiles(),
  ]);
}

const customRequest: UploadProps['customRequest'] = async (options) => {
  try {
    await uploadFileApi(options.file as File, resolveUploadPayload());
    options.onSuccess?.({}, new XMLHttpRequest());
    message.success('文件上传成功');
    pageIndex.value = 1;
    await loadTree();
    await loadFiles();
  } catch (error) {
    options.onError?.(error as Error);
    throw error;
  }
};

async function handleDelete(fileKey: string) {
  await runAction(`delete-file:${fileKey}`, async () => {
    await deleteFileApi(fileKey);
    message.success('文件已删除');
    if (items.value.length === 1 && pageIndex.value > 1) {
      pageIndex.value -= 1;
    }
    await loadTree();
    await loadFiles();
  });
}

async function handleBatchDelete() {
  if (selectedFileKeys.value.length === 0) {
    message.warning('请先选择要删除的文件');
    return;
  }

  const fileKeys = [...selectedFileKeys.value];
  await runAction('batch-delete-files', async () => {
    await batchDeleteFileApi(fileKeys);
    message.success(`已删除 ${fileKeys.length} 个文件`);
    selectedFileKeys.value = [];
    if (items.value.length <= fileKeys.length && pageIndex.value > 1) {
      pageIndex.value -= 1;
    }
    await loadTree();
    await loadFiles();
  });
}

function handleTableChange(
  pagination: { current?: number; pageSize?: number },
  _filters?: unknown,
  _sorter?: unknown,
  extra?: { action?: string },
) {
  if (extra?.action === 'sort') {
    return;
  }

  pageIndex.value = pagination.current || 1;
  pageSize.value = pagination.pageSize || pageSize.value;
  void loadFiles();
}

async function handleSearch() {
  pageIndex.value = 1;
  await loadTree();
  await loadFiles();
}

async function handleDownload(file: FileApi.FileItem) {
  const blob = await downloadFileApi(file.fileKey);
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = file.fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

function canPreview(file: FileApi.FileItem) {
  return file.contentType.startsWith('image/') ||
    file.contentType === 'application/pdf' ||
    file.contentType.startsWith('text/');
}

async function handlePreview(file: FileApi.FileItem) {
  if (!canPreview(file)) {
    message.warning('该文件类型暂不支持在线预览');
    return;
  }

  revokePreviewUrl();
  previewFile.value = file;
  previewModalOpen.value = true;
  previewLoading.value = true;
  try {
    const blob = await previewFileApi(file.fileKey);
    previewUrl.value = URL.createObjectURL(blob);
  } finally {
    previewLoading.value = false;
  }
}

function revokePreviewUrl() {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
    previewUrl.value = '';
  }
}

function openMetadataEdit(file: FileApi.FileItem) {
  editingFile.value = file;
  metadataForm.businessId = file.businessId || '';
  metadataForm.businessType = file.businessType || '';
  metadataForm.fileName = file.fileName;
  metadataForm.isPublic = file.isPublic;
  metadataModalOpen.value = true;
}

async function submitMetadata() {
  if (!editingFile.value) {
    return;
  }

  if (!metadataForm.fileName.trim()) {
    message.warning('请填写文件别名');
    return;
  }

  metadataSaving.value = true;
  try {
    await updateFileMetadataApi(editingFile.value.fileKey, {
      businessId: metadataForm.businessId?.trim() || null,
      businessType: metadataForm.businessType?.trim() || null,
      fileName: metadataForm.fileName.trim(),
      isPublic: metadataForm.isPublic,
    });
    message.success('文件元数据已更新');
    metadataModalOpen.value = false;
    await loadFiles();
  } finally {
    metadataSaving.value = false;
  }
}

function formatSize(size: number) {
  if (size < 1024) {
    return `${size} B`;
  }
  if (size < 1024 * 1024) {
    return `${(size / 1024).toFixed(1)} KB`;
  }
  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}

function normalizePath(path?: null | string) {
  const normalized = `${path || ''}`.trim();
  if (!normalized || normalized.toLowerCase() === 'null' || normalized.toLowerCase() === 'undefined') {
    return '';
  }

  return normalized
    .replaceAll('\\', '/')
    .split('/')
    .map((segment) => segment.trim())
    .filter((segment) => segment.length > 0 && segment.toLowerCase() !== 'null' && segment.toLowerCase() !== 'undefined')
    .join('/');
}

function normalizeCategory(category?: null | string) {
  const normalized = `${category || ''}`.trim();
  if (!normalized || normalized.toLowerCase() === 'null' || normalized.toLowerCase() === 'undefined') {
    return '未分类';
  }

  return normalized;
}

function resolveDisplayPath(item: FileApi.FileItem) {
  if (item.relativePath) {
    return item.relativePath;
  }

  return [normalizeCategory(item.category), normalizePath(item.parentPath), item.fileName]
    .filter(Boolean)
    .join('/');
}

function createCategoryNode(category: string, paths: Set<string>): FileTreeNode {
  return {
    children: buildDirectoryChildren(category, '', paths),
    key: `category:${category}`,
    title: category,
  };
}

function buildDirectoryChildren(category: string, parentPath: string, paths: Set<string>): FileTreeNode[] {
  return [...paths]
    .filter((path) => {
      if (!path) {
        return false;
      }
      const normalizedParent = parentPath ? `${parentPath}/` : '';
      if (!path.startsWith(normalizedParent)) {
        return false;
      }
      const remainder = path.slice(normalizedParent.length);
      return remainder.length > 0 && !remainder.includes('/');
    })
    .sort((left, right) => left.localeCompare(right, 'zh-CN'))
    .map((path) => ({
      children: buildDirectoryChildren(category, path, paths),
      key: `path:${category}:${path}`,
      title: path.split('/').pop() || path,
    }));
}

function handleTreeSelect(keys: (number | string)[]) {
  selectedTreeKey.value = `${keys[0] ?? 'all'}`;
  pageIndex.value = 1;
  void loadFiles();
}

function resolveTreeFilter() {
  if (selectedTreeKey.value.startsWith('category:')) {
    return {
      category: selectedTreeKey.value.replace('category:', ''),
      parentPath: undefined,
    };
  }

  if (selectedTreeKey.value.startsWith('path:')) {
    const [, category, path] = selectedTreeKey.value.split(':');
    return { category, parentPath: path || undefined };
  }

  return {
    category: undefined,
    parentPath: undefined,
  };
}

function resolveUploadPayload() {
  const basePayload = {
    isPublic: false,
    storageSourceId: uploadStorageSourceId.value || undefined,
  };

  if (selectedTreeKey.value === 'all') {
    return { ...basePayload, category: '未分类', parentPath: undefined };
  }

  if (selectedTreeKey.value.startsWith('category:')) {
    return {
      ...basePayload,
      category: selectedTreeKey.value.replace('category:', ''),
      parentPath: undefined,
    };
  }

  if (selectedTreeKey.value.startsWith('path:')) {
    const [, category, path] = selectedTreeKey.value.split(':');
    return {
      ...basePayload,
      category,
      parentPath: path || undefined,
    };
  }

  return {
    ...basePayload,
    category: '未分类',
    parentPath: undefined,
  };
}

function buildAvailableTreeKeys(items: FileApi.TreeItem[]) {
  const keys = new Set<string>();
  for (const item of items) {
    const category = normalizeCategory(item.category);
    keys.add(`category:${category}`);

    const parentPath = normalizePath(item.parentPath);
    if (!parentPath) {
      continue;
    }

    const segments = parentPath.split('/').filter(Boolean);
    let current = '';
    for (const segment of segments) {
      current = current ? `${current}/${segment}` : segment;
      keys.add(`path:${category}:${current}`);
    }
  }

  return keys;
}

function patchSourceForm(item?: FileApi.StorageSourceItem) {
  editingSourceId.value = item?.id ?? '';
  sourceForm.accessKeyId = item?.accessKeyId ?? '';
  sourceForm.bucketName = item?.bucketName ?? '';
  sourceForm.customDomain = item?.customDomain ?? '';
  sourceForm.endpoint = item?.endpoint ?? '';
  sourceForm.isDefault = item?.isDefault ?? sources.value.length === 0;
  sourceForm.isEnabled = item?.isEnabled ?? true;
  sourceForm.isPublic = item?.isPublic ?? false;
  sourceForm.name = item?.name ?? '';
  sourceForm.pathTemplate = item?.pathTemplate ?? '{yyyy}/{MM}/{dd}';
  sourceForm.providerName = item?.providerName ?? 'Local';
  sourceForm.region = item?.region ?? '';
  sourceForm.remark = item?.remark ?? '';
  sourceForm.rootPath = item?.rootPath ?? 'App_Data/upload-files';
  sourceForm.secretKey = '';
  sourceForm.useSsl = item?.useSsl ?? false;
}

function openSourceCreate() {
  patchSourceForm();
  sourceModalOpen.value = true;
}

function openSourceEdit(item: FileApi.StorageSourceItem) {
  patchSourceForm(item);
  sourceModalOpen.value = true;
}

function validateSourceForm() {
  if (!sourceForm.name.trim() || !sourceForm.providerName.trim()) {
    message.warning('请填写存储源名称和类型');
    return false;
  }

  if (sourceForm.providerName !== 'Local') {
    if (!sourceForm.endpoint?.trim() || !sourceForm.accessKeyId?.trim() || !sourceForm.bucketName?.trim()) {
      message.warning('云存储源需要 Endpoint、AccessKey 和 Bucket');
      return false;
    }
    if (!editingSourceId.value && !sourceForm.secretKey?.trim()) {
      message.warning('新增云存储源需要填写密钥');
      return false;
    }
  }

  return true;
}

async function submitSource() {
  if (!validateSourceForm()) {
    return;
  }

  sourceLoading.value = true;
  try {
    if (editingSourceId.value) {
      await updateFileStorageSourceApi(editingSourceId.value, { ...sourceForm });
      message.success('存储源已更新');
    } else {
      await createFileStorageSourceApi({ ...sourceForm });
      message.success('存储源已新增');
    }
    sourceModalOpen.value = false;
    await loadSources();
    await loadFiles();
  } finally {
    sourceLoading.value = false;
  }
}

async function setDefaultSource(item: FileApi.StorageSourceItem) {
  await runAction(`default-source:${item.id}`, async () => {
    await setDefaultFileStorageSourceApi(item.id);
    message.success('已设为默认存储源');
    await loadSources();
  });
}

async function toggleSource(item: FileApi.StorageSourceItem, isEnabled: boolean) {
  await runAction(`toggle-source:${item.id}`, async () => {
    await toggleFileStorageSourceApi(item.id, isEnabled);
    message.success(isEnabled ? '存储源已启用' : '存储源已停用');
    await loadSources();
  });
}

async function deleteSource(item: FileApi.StorageSourceItem) {
  await runAction(`delete-source:${item.id}`, async () => {
    await deleteFileStorageSourceApi(item.id);
    message.success('存储源已删除');
    await loadSources();
  });
}

async function resetFilters() {
  categoryFilter.value = undefined;
  keyword.value = '';
  uploadedBy.value = '';
  uploadedRange.value = undefined;
  storageSourceFilter.value = undefined;
  selectedTreeKey.value = 'all';
  pageIndex.value = 1;
  await loadTree();
  await loadFiles();
}

loadPage();

onBeforeUnmount(revokePreviewUrl);
</script>

<template>
  <Page description="文件管理用于维护平台制度、模板、交付材料等共享文件。" title="文件管理">
    <section class="platform-file">
      <Card :bordered="false" class="platform-file__tree" title="目录树">
        <template #extra>
          <Tag color="blue">{{ totalCount }} 份文件</Tag>
        </template>

        <div v-if="!loading && directoryTree.length === 0" class="platform-empty">
          <Empty description="暂无文件" />
        </div>
        <Tree
          v-else
          block-node
          default-expand-all
          :selected-keys="selectedTreeKey === 'all' ? [] : [selectedTreeKey]"
          :tree-data="directoryTree as TreeProps['treeData']"
          @select="handleTreeSelect"
        />
      </Card>

      <Card :bordered="false" class="platform-file__table" title="文件清单">
        <template #extra>
          <Space>
            <Input
              v-model:value="keyword"
              allow-clear
              placeholder="按文件名/类型/路径筛选"
              @pressEnter="handleSearch"
            />
            <Select
              v-model:value="categoryFilter"
              allow-clear
              placeholder="按分类筛选"
              style="width: 160px"
              :options="categoryOptions"
            />
            <Select
              v-if="canManageFiles"
              v-model:value="storageSourceFilter"
              allow-clear
              placeholder="按存储源筛选"
              style="width: 180px"
              :options="sourceFilterOptions"
            />
            <Input
              v-model:value="uploadedBy"
              allow-clear
              placeholder="按上传人筛选"
              @pressEnter="handleSearch"
            />
            <DatePicker.RangePicker v-model:value="uploadedRange" />
            <Button type="primary" @click="handleSearch">查询</Button>
            <Button @click="resetFilters">重置</Button>
            <Select
              v-if="canManageFiles"
              v-model:value="uploadStorageSourceId"
              allow-clear
              placeholder="上传源"
              style="width: 180px"
              :options="enabledSourceOptions"
            />
            <Upload v-if="canManageFiles" :custom-request="customRequest" :show-upload-list="false">
              <Button type="primary">上传文件</Button>
            </Upload>
            <Popconfirm
              v-if="canManageFiles"
              :disabled="selectedFileCount === 0"
              :title="`确认删除选中的 ${selectedFileCount} 个文件？`"
              @confirm="handleBatchDelete"
            >
              <Button
                danger
                :disabled="selectedFileCount === 0"
                :loading="actionLoadingKey === 'batch-delete-files'"
              >
                批量删除
              </Button>
            </Popconfirm>
            <Button v-if="canManageFiles" @click="sourceDrawerOpen = true">存储源</Button>
          </Space>
        </template>

        <div v-if="!loading && filteredItems.length === 0" class="platform-empty">
          <Empty description="当前目录暂无文件" />
        </div>
        <Table
          v-else
          :columns="columns"
          :data-source="filteredItems"
          :loading="loading"
          :pagination="{
            current: pageIndex,
            pageSize,
            showSizeChanger: true,
            showTotal: (total: number) => `共 ${total} 条`,
            total: totalCount,
          }"
          :row-selection="rowSelection"
          row-key="fileKey"
          :scroll="{ x: 1420 }"
          @change="handleTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'size'">
              {{ formatSize(record.size) }}
            </template>
            <template v-else-if="column.key === 'storageSourceName'">
              <Tag>{{ record.storageSourceName || record.storageProvider || '默认配置' }}</Tag>
            </template>
            <template v-else-if="column.key === 'isPublic'">
              <PlatformStatusTag :value="record.isPublic" true-label="公开" false-label="私有" />
            </template>
            <template v-else-if="column.key === 'uploadedBy'">
              {{ record.uploadedBy || '-' }}
            </template>
            <template v-else-if="column.key === 'business'">
              <span v-if="record.businessType || record.businessId">
                {{ [record.businessType, record.businessId].filter(Boolean).join(' / ') }}
              </span>
              <span v-else>-</span>
            </template>
            <template v-else-if="column.key === 'relativePath'">
              <span class="platform-file__path" :title="record.storageLocation">
                {{ resolveDisplayPath(record as FileApi.FileItem) }}
              </span>
            </template>
            <template v-else-if="column.key === 'uploadedAt'">
              {{ formatDateTime(record.uploadedAt) }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <Space>
                <Button
                  :disabled="!canPreview(record as FileApi.FileItem)"
                  size="small"
                  type="link"
                  @click="handlePreview(record as FileApi.FileItem)"
                >
                  预览
                </Button>
                <Button size="small" type="link" @click="handleDownload(record as FileApi.FileItem)">
                  下载
                </Button>
                <Button
                  v-if="canManageFiles"
                  size="small"
                  type="link"
                  @click="openMetadataEdit(record as FileApi.FileItem)"
                >
                  编辑
                </Button>
                <Popconfirm
                  v-if="canManageFiles"
                  title="确认删除该文件？"
                  @confirm="handleDelete(record.fileKey)"
                >
                  <Button
                    danger
                    :loading="actionLoadingKey === `delete-file:${(record as FileApi.FileItem).fileKey}`"
                    size="small"
                    type="link"
                  >
                    删除
                  </Button>
                </Popconfirm>
              </Space>
            </template>
          </template>
        </Table>
      </Card>
    </section>

    <Drawer v-model:open="sourceDrawerOpen" title="文件存储源" width="1040">
      <template #extra>
        <Space>
          <Button :loading="sourceLoading" @click="loadSources()">刷新</Button>
          <Button type="primary" @click="openSourceCreate">新增存储源</Button>
        </Space>
      </template>

      <Table
        :columns="sourceColumns"
        :data-source="sources"
        :loading="sourceLoading"
        :pagination="{ pageSize: 8, showSizeChanger: false }"
        row-key="id"
        :scroll="{ x: 1080 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'providerName'">
            <Tag>{{ (record as FileApi.StorageSourceItem).providerName }}</Tag>
          </template>
          <template v-else-if="column.key === 'isDefault'">
            <Tag v-if="(record as FileApi.StorageSourceItem).isDefault" color="blue">默认</Tag>
            <span v-else>-</span>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <PlatformStatusTag :value="(record as FileApi.StorageSourceItem).isEnabled" />
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Button
                size="small"
                :disabled="(record as FileApi.StorageSourceItem).isDefault || !(record as FileApi.StorageSourceItem).isEnabled"
                :loading="actionLoadingKey === `default-source:${(record as FileApi.StorageSourceItem).id}`"
                @click="setDefaultSource(record as FileApi.StorageSourceItem)"
              >
                设为默认
              </Button>
              <Switch
                :checked="(record as FileApi.StorageSourceItem).isEnabled"
                :disabled="(record as FileApi.StorageSourceItem).isDefault"
                :loading="actionLoadingKey === `toggle-source:${(record as FileApi.StorageSourceItem).id}`"
                @change="(checked) => toggleSource(record as FileApi.StorageSourceItem, Boolean(checked))"
              />
              <Button size="small" @click="openSourceEdit(record as FileApi.StorageSourceItem)">编辑</Button>
              <Popconfirm title="确认删除该存储源？已被文件使用的存储源不能删除。" @confirm="deleteSource(record as FileApi.StorageSourceItem)">
                <Button
                  danger
                  :disabled="(record as FileApi.StorageSourceItem).isDefault"
                  :loading="actionLoadingKey === `delete-source:${(record as FileApi.StorageSourceItem).id}`"
                  size="small"
                >
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Drawer>

    <Modal
      v-model:open="metadataModalOpen"
      :confirm-loading="metadataSaving"
      title="编辑文件元数据"
      width="560px"
      @ok="submitMetadata"
    >
      <Form layout="vertical">
        <Form.Item label="文件别名" required>
          <Input v-model:value="metadataForm.fileName" :maxlength="256" />
        </Form.Item>
        <Form.Item label="公开文件">
          <Switch v-model:checked="metadataForm.isPublic" />
        </Form.Item>
        <Form.Item label="业务类型">
          <Input v-model:value="metadataForm.businessType" :maxlength="64" placeholder="例如 Project / Contract" />
        </Form.Item>
        <Form.Item label="业务标识">
          <Input v-model:value="metadataForm.businessId" :maxlength="128" />
        </Form.Item>
      </Form>
    </Modal>

    <Modal
      v-model:open="previewModalOpen"
      :footer="null"
      :title="previewFile?.fileName || '文件预览'"
      width="880px"
      @cancel="revokePreviewUrl"
    >
      <div v-if="previewLoading" class="platform-empty">正在加载预览...</div>
      <img
        v-else-if="previewFile?.contentType.startsWith('image/') && previewUrl"
        alt=""
        class="platform-file__preview-image"
        :src="previewUrl"
      />
      <iframe
        v-else-if="previewUrl"
        class="platform-file__preview-frame"
        :src="previewUrl"
        title="文件预览"
      />
      <Empty v-else description="无法加载预览" />
    </Modal>

    <Modal
      v-model:open="sourceModalOpen"
      :confirm-loading="sourceLoading"
      :title="editingSourceId ? '编辑存储源' : '新增存储源'"
      width="720px"
      @ok="submitSource"
    >
      <Form layout="vertical">
        <Form.Item label="名称" required>
          <Input v-model:value="sourceForm.name" placeholder="例如 本地默认 / MinIO 私有化" />
        </Form.Item>
        <Form.Item label="类型" required>
          <Select v-model:value="sourceForm.providerName" :options="providerOptions" />
        </Form.Item>
        <Form.Item v-if="sourceForm.providerName === 'Local'" label="本地根路径">
          <Input v-model:value="sourceForm.rootPath" placeholder="App_Data/upload-files" />
        </Form.Item>
        <template v-else>
          <Form.Item label="Endpoint" required>
            <Input v-model:value="sourceForm.endpoint" placeholder="例如 localhost:9000 或 oss-cn-hangzhou.aliyuncs.com" />
          </Form.Item>
          <Form.Item label="Bucket" required>
            <Input v-model:value="sourceForm.bucketName" placeholder="Bucket 名称" />
          </Form.Item>
          <Form.Item label="AccessKey" required>
            <Input v-model:value="sourceForm.accessKeyId" placeholder="AccessKeyId / AccessKey" />
          </Form.Item>
          <Form.Item :label="editingSourceId ? 'SecretKey（留空不修改）' : 'SecretKey'" :required="!editingSourceId">
            <Input.Password v-model:value="sourceForm.secretKey" placeholder="SecretKey / AccessKeySecret" />
          </Form.Item>
          <Form.Item label="Region">
            <Input v-model:value="sourceForm.region" placeholder="可选" />
          </Form.Item>
          <Form.Item label="CustomDomain">
            <Input v-model:value="sourceForm.customDomain" placeholder="可选，例如 https://files.example.com" />
          </Form.Item>
          <Form.Item label="启用 SSL/HTTPS">
            <Switch v-model:checked="sourceForm.useSsl" />
          </Form.Item>
        </template>
        <Form.Item label="路径模板">
          <Input v-model:value="sourceForm.pathTemplate" placeholder="{yyyy}/{MM}/{dd}" />
        </Form.Item>
        <Form.Item label="默认源">
          <Switch v-model:checked="sourceForm.isDefault" />
        </Form.Item>
        <Form.Item label="启用">
          <Switch v-model:checked="sourceForm.isEnabled" />
        </Form.Item>
        <Form.Item label="默认公开">
          <Switch v-model:checked="sourceForm.isPublic" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="sourceForm.remark" :auto-size="{ minRows: 2, maxRows: 4 }" />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-file {
  display: grid;
  grid-template-columns: 280px minmax(0, 1fr);
  gap: 16px;
}

.platform-file__tree,
.platform-file__table {
  min-height: 640px;
}

.platform-file__path {
  display: inline-block;
  max-width: 320px;
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-file__preview-frame {
  width: 100%;
  height: 70vh;
  border: 0;
}

.platform-file__preview-image {
  display: block;
  max-width: 100%;
  max-height: 70vh;
  margin: 0 auto;
  object-fit: contain;
}

.platform-empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}

@media (width <= 960px) {
  .platform-file {
    grid-template-columns: 1fr;
  }
}
</style>
