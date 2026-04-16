<script lang="ts" setup>
import type { UploadProps } from 'ant-design-vue';

import { ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Popconfirm, Space, Table, Upload, message } from 'ant-design-vue';

import { deleteFileApi, downloadFileApi, getFileListApi, uploadFileApi, type FileApi } from '#/api/core';

defineOptions({ name: 'PlatformFilePage' });

const columns = [
  { dataIndex: 'fileName', key: 'fileName', title: '文件名' },
  { dataIndex: 'contentType', key: 'contentType', title: '类型', width: 220 },
  { dataIndex: 'size', key: 'size', title: '大小', width: 120 },
  { dataIndex: 'uploadedAt', key: 'uploadedAt', title: '上传时间', width: 220 },
  { key: 'actions', title: '操作', width: 160 },
];

const items = ref<FileApi.FileItem[]>([]);
const loading = ref(false);

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
    await uploadFileApi(options.file as File);
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

loadFiles();
</script>

<template>
  <Page description="文件管理用于维护平台制度、模板、交付材料等共享文件。" title="文件管理">
    <Card :bordered="false" title="共享文件">
      <template #extra>
        <Upload :custom-request="customRequest" :show-upload-list="false">
          <Button type="primary">上传文件</Button>
        </Upload>
      </template>

      <div v-if="!loading && items.length === 0" class="platform-empty">
        <Empty description="暂无文件" />
      </div>
      <Table
        v-else
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="{ pageSize: 10 }"
        row-key="fileKey"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'size'">
            {{ formatSize(record.size) }}
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
  </Page>
</template>

<style scoped>
.platform-empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
