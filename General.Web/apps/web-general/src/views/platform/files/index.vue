<script lang="ts" setup>
import type { UploadProps, TreeProps } from 'ant-design-vue';
import type { Dayjs } from 'dayjs';

import { computed, reactive, ref } from 'vue';

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
  createFileStorageSourceApi,
  deleteFileApi,
  deleteFileStorageSourceApi,
  downloadFileApi,
  getFileListApi,
  getFileStorageSourcesApi,
  setDefaultFileStorageSourceApi,
  toggleFileStorageSourceApi,
  updateFileStorageSourceApi,
  uploadFileApi,
  type FileApi,
} from '#/api/core';

defineOptions({ name: 'PlatformFilePage' });

interface FileTreeNode {
  children?: FileTreeNode[];
  isLeaf?: boolean;
  key: string;
  title: string;
}

const columns = [
  { dataIndex: 'fileName', key: 'fileName', title: '文件名' },
  { dataIndex: 'storageSourceName', key: 'storageSourceName', title: '存储源', width: 150 },
  { dataIndex: 'category', key: 'category', title: '分类', width: 120 },
  { dataIndex: 'contentType', key: 'contentType', title: '类型', width: 220 },
  { dataIndex: 'isPublic', key: 'isPublic', title: '公开性', width: 90 },
  { dataIndex: 'size', key: 'size', title: '大小', width: 120 },
  { dataIndex: 'uploadedBy', key: 'uploadedBy', title: '上传人', width: 140 },
  { dataIndex: 'relativePath', key: 'relativePath', title: '存储位置' },
  { dataIndex: 'uploadedAt', key: 'uploadedAt', title: '上传时间', width: 220 },
  { key: 'actions', title: '操作', width: 160 },
];

const items = ref<FileApi.FileItem[]>([]);
const sources = ref<FileApi.StorageSourceItem[]>([]);
const loading = ref(false);
const sourceLoading = ref(false);
const sourceDrawerOpen = ref(false);
const sourceModalOpen = ref(false);
const editingSourceId = ref('');
const selectedTreeKey = ref<string>('all');
const categoryFilter = ref<string>();
const keyword = ref('');
const storageSourceFilter = ref<string>();
const uploadStorageSourceId = ref<string>();
const uploadedBy = ref('');
const uploadedRange = ref<[Dayjs, Dayjs] | undefined>(undefined);

const providerOptions = [
  { label: '本地存储', value: 'Local' },
  { label: 'MinIO', value: 'MinIO' },
  { label: '阿里云 OSS', value: 'AliyunOSS' },
];

const sourceForm = reactive<FileApi.StorageSourceSaveInput>({
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
  [...new Set(items.value.map((item) => normalizeCategory(item.category)))]
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
  for (const item of items.value) {
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
  return items.value.filter((item) => {
    const category = normalizeCategory(item.category);
    const pathValue = normalizePath(item.parentPath);

    if (selectedTreeKey.value !== 'all') {
      const categoryPrefix = `category:${category}`;
      const parentKey = `path:${category}:${pathValue}`;
      const fileKey = `file:${item.fileKey}`;
      const selectedPathPrefix = selectedTreeKey.value.startsWith('path:')
        ? selectedTreeKey.value.replace(`path:${category}:`, '')
        : '';

      const inTree =
        selectedTreeKey.value === categoryPrefix ||
        selectedTreeKey.value === fileKey ||
        parentKey === selectedTreeKey.value ||
        (selectedTreeKey.value.startsWith(`path:${category}:`) &&
          (pathValue === selectedPathPrefix || pathValue.startsWith(`${selectedPathPrefix}/`)));

      if (!inTree) {
        return false;
      }
    }
    return true;
  });
});

async function loadFiles() {
  loading.value = true;
  try {
    items.value = await getFileListApi({
      category: categoryFilter.value || undefined,
      keyword: keyword.value || undefined,
      storageSourceId: storageSourceFilter.value || undefined,
      uploadedBy: uploadedBy.value || undefined,
      uploadedFrom: uploadedRange.value?.[0]?.startOf('day').toISOString(),
      uploadedTo: uploadedRange.value?.[1]?.endOf('day').toISOString(),
    });
  } finally {
    loading.value = false;
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
  await Promise.all([loadSources(), loadFiles()]);
}

const customRequest: UploadProps['customRequest'] = async (options) => {
  try {
    await uploadFileApi(options.file as File, resolveUploadPayload());
    options.onSuccess?.({}, new XMLHttpRequest());
    message.success('文件上传成功');
    await loadFiles();
  } catch (error) {
    options.onError?.(error as Error);
    throw error;
  }
};

async function handleDelete(fileKey: string) {
  await deleteFileApi(fileKey);
  message.success('文件已删除');
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
  const childDirectories = [...paths]
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

  const fileNodes = items.value
    .filter((item) => normalizeCategory(item.category) === category && normalizePath(item.parentPath) === parentPath)
    .sort((left, right) => left.fileName.localeCompare(right.fileName, 'zh-CN'))
    .map((item) => ({
      isLeaf: true,
      key: `file:${item.fileKey}`,
      title: item.fileName,
    }));

  return [...childDirectories, ...fileNodes];
}

function handleTreeSelect(keys: (number | string)[]) {
  selectedTreeKey.value = `${keys[0] ?? 'all'}`;
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

  const currentFile = items.value.find((item) => `file:${item.fileKey}` === selectedTreeKey.value);
  return {
    ...basePayload,
    category: normalizeCategory(currentFile?.category) || '未分类',
    parentPath: normalizePath(currentFile?.parentPath) || undefined,
  };
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
  await setDefaultFileStorageSourceApi(item.id);
  message.success('已设为默认存储源');
  await loadSources();
}

async function toggleSource(item: FileApi.StorageSourceItem, isEnabled: boolean) {
  await toggleFileStorageSourceApi(item.id, isEnabled);
  message.success(isEnabled ? '存储源已启用' : '存储源已停用');
  await loadSources();
}

async function deleteSource(item: FileApi.StorageSourceItem) {
  await deleteFileStorageSourceApi(item.id);
  message.success('存储源已删除');
  await loadSources();
}

async function resetFilters() {
  categoryFilter.value = undefined;
  keyword.value = '';
  uploadedBy.value = '';
  uploadedRange.value = undefined;
  storageSourceFilter.value = undefined;
  await loadFiles();
}

loadPage();
</script>

<template>
  <Page description="文件管理用于维护平台制度、模板、交付材料等共享文件。" title="文件管理">
    <section class="platform-file">
      <Card :bordered="false" class="platform-file__tree" title="目录树">
        <template #extra>
          <Tag color="blue">{{ items.length }} 份文件</Tag>
        </template>

        <div v-if="!loading && items.length === 0" class="platform-empty">
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
              @pressEnter="loadFiles"
            />
            <Select
              v-model:value="categoryFilter"
              allow-clear
              placeholder="按分类筛选"
              style="width: 160px"
              :options="categoryOptions"
            />
            <Select
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
              @pressEnter="loadFiles"
            />
            <DatePicker.RangePicker v-model:value="uploadedRange" />
            <Button type="primary" @click="loadFiles">查询</Button>
            <Button @click="resetFilters">重置</Button>
            <Select
              v-model:value="uploadStorageSourceId"
              allow-clear
              placeholder="上传源"
              style="width: 180px"
              :options="enabledSourceOptions"
            />
            <Upload :custom-request="customRequest" :show-upload-list="false">
              <Button type="primary">上传文件</Button>
            </Upload>
            <Button @click="sourceDrawerOpen = true">存储源</Button>
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
          :pagination="{ pageSize: 10 }"
          row-key="fileKey"
          :scroll="{ x: 1180 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'size'">
              {{ formatSize(record.size) }}
            </template>
            <template v-else-if="column.key === 'storageSourceName'">
              <Tag>{{ record.storageSourceName || record.storageProvider || '默认配置' }}</Tag>
            </template>
            <template v-else-if="column.key === 'isPublic'">
              <Tag :color="record.isPublic ? 'success' : 'default'">
                {{ record.isPublic ? '公开' : '私有' }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'uploadedBy'">
              {{ record.uploadedBy || '-' }}
            </template>
            <template v-else-if="column.key === 'relativePath'">
              <span class="platform-file__path" :title="record.storageLocation">
                {{ resolveDisplayPath(record as FileApi.FileItem) }}
              </span>
            </template>
            <template v-else-if="column.key === 'uploadedAt'">
              {{ new Date(record.uploadedAt).toLocaleString() }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <Space>
                <Button size="small" type="link" @click="handleDownload(record as FileApi.FileItem)">
                  下载
                </Button>
                <Popconfirm title="确认删除该文件？" @confirm="handleDelete(record.fileKey)">
                  <Button danger size="small" type="link">删除</Button>
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
            <Tag :color="(record as FileApi.StorageSourceItem).isEnabled ? 'success' : 'default'">
              {{ (record as FileApi.StorageSourceItem).isEnabled ? '启用' : '停用' }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Button
                size="small"
                :disabled="(record as FileApi.StorageSourceItem).isDefault || !(record as FileApi.StorageSourceItem).isEnabled"
                @click="setDefaultSource(record as FileApi.StorageSourceItem)"
              >
                设为默认
              </Button>
              <Switch
                :checked="(record as FileApi.StorageSourceItem).isEnabled"
                :disabled="(record as FileApi.StorageSourceItem).isDefault"
                @change="(checked) => toggleSource(record as FileApi.StorageSourceItem, Boolean(checked))"
              />
              <Button size="small" @click="openSourceEdit(record as FileApi.StorageSourceItem)">编辑</Button>
              <Popconfirm title="确认删除该存储源？已被文件使用的存储源不能删除。" @confirm="deleteSource(record as FileApi.StorageSourceItem)">
                <Button danger size="small" :disabled="(record as FileApi.StorageSourceItem).isDefault">删除</Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Drawer>

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
