<script lang="ts" setup>
import type { UploadProps, TreeProps } from 'ant-design-vue';

import { computed, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Input, Popconfirm, Space, Table, Tag, Tree, Upload, message } from 'ant-design-vue';

import { deleteFileApi, downloadFileApi, getFileListApi, uploadFileApi, type FileApi } from '#/api/core';

defineOptions({ name: 'PlatformFilePage' });

interface FileTreeNode {
  children?: FileTreeNode[];
  isLeaf?: boolean;
  key: string;
  title: string;
}

const columns = [
  { dataIndex: 'fileName', key: 'fileName', title: '文件名' },
  { dataIndex: 'category', key: 'category', title: '分类', width: 120 },
  { dataIndex: 'contentType', key: 'contentType', title: '类型', width: 220 },
  { dataIndex: 'size', key: 'size', title: '大小', width: 120 },
  { dataIndex: 'uploadedBy', key: 'uploadedBy', title: '上传人', width: 140 },
  { dataIndex: 'storageLocation', key: 'storageLocation', title: '存储位置' },
  { dataIndex: 'uploadedAt', key: 'uploadedAt', title: '上传时间', width: 220 },
  { key: 'actions', title: '操作', width: 160 },
];

const items = ref<FileApi.FileItem[]>([]);
const loading = ref(false);
const selectedTreeKey = ref<string>('all');
const keyword = ref('');

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
  const term = keyword.value.trim().toLowerCase();
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

    if (!term) {
      return true;
    }

    return [
      item.fileName,
      category,
      item.contentType,
      item.uploadedBy || '',
      item.storageLocation,
      item.parentPath || '',
    ].some((value) => value.toLowerCase().includes(term));
  });
});

async function loadFiles() {
  loading.value = true;
  try {
    items.value = await getFileListApi();
  } finally {
    loading.value = false;
  }
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
  if (selectedTreeKey.value === 'all') {
    return { category: '未分类', parentPath: undefined };
  }

  if (selectedTreeKey.value.startsWith('category:')) {
    return {
      category: selectedTreeKey.value.replace('category:', ''),
      parentPath: undefined,
    };
  }

  if (selectedTreeKey.value.startsWith('path:')) {
    const [, category, path] = selectedTreeKey.value.split(':');
    return {
      category,
      parentPath: path || undefined,
    };
  }

  const currentFile = items.value.find((item) => `file:${item.fileKey}` === selectedTreeKey.value);
  return {
    category: normalizeCategory(currentFile?.category) || '未分类',
    parentPath: normalizePath(currentFile?.parentPath) || undefined,
  };
}

loadFiles();
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
              placeholder="按文件名/上传人/路径筛选"
            />
            <Upload :custom-request="customRequest" :show-upload-list="false">
              <Button type="primary">上传文件</Button>
            </Upload>
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
            <template v-else-if="column.key === 'uploadedBy'">
              {{ record.uploadedBy || '-' }}
            </template>
            <template v-else-if="column.key === 'storageLocation'">
              <span class="platform-file__path">{{ record.storageLocation }}</span>
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
