<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from 'vue';

import { Button, Empty, Modal, Skeleton, message } from 'ant-design-vue';
import { FileViewer } from '@flyfish-group/file-viewer3';
import '@flyfish-group/file-viewer3/dist/file-viewer3.css';
import { marked } from 'marked';

import { previewFileApi, type FileApi } from '#/api/core';
import { downloadPlatformFile } from './download-file';

const props = defineProps<{
  file?: FileApi.FileItem | null;
  open: boolean;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();

const loading = ref(false);
const downloading = ref(false);
const previewFileObject = ref<File | null>(null);
const previewHtml = ref('');

const title = computed(() => props.file?.fileName || '文件预览');
const isRichTextPreview = computed(() => isHtmlFile(props.file) || isMarkdownFile(props.file));

watch(
  () => [props.open, props.file?.fileKey] as const,
  async ([open]) => {
    if (!open || !props.file) {
      clearPreviewFile();
      return;
    }

    loading.value = true;
    previewFileObject.value = null;
    previewHtml.value = '';
    try {
      const blob = await previewFileApi(props.file.fileKey);
      const normalizedBlob = new Blob([blob], {
        type: blob.type || props.file.contentType || 'application/octet-stream',
      });
      if (isHtmlFile(props.file)) {
        previewHtml.value = buildHtmlPreview(await normalizedBlob.text());
        return;
      }

      if (isMarkdownFile(props.file)) {
        previewHtml.value = buildMarkdownPreview(await normalizedBlob.text());
        return;
      }

      previewFileObject.value = new File([normalizedBlob], props.file.fileName, {
        type: normalizedBlob.type,
      });
    } catch (error) {
      const description = error instanceof Error ? error.message : '文件预览加载失败';
      message.error(description || '文件预览加载失败');
    } finally {
      loading.value = false;
    }
  },
);

onBeforeUnmount(clearPreviewFile);

function handleOpenChange(value: boolean) {
  emit('update:open', value);
  if (!value) {
    clearPreviewFile();
  }
}

function clearPreviewFile() {
  previewFileObject.value = null;
  previewHtml.value = '';
  loading.value = false;
  downloading.value = false;
}

async function handleDownload() {
  if (!props.file) {
    return;
  }

  downloading.value = true;
  try {
    await downloadPlatformFile(props.file);
  } catch (error) {
    const description = error instanceof Error ? error.message : '文件下载失败';
    message.error(description || '文件下载失败');
  } finally {
    downloading.value = false;
  }
}

function isHtmlFile(file?: FileApi.FileItem | null) {
  if (!file) {
    return false;
  }

  const contentType = (file.contentType || '').toLowerCase();
  const fileName = (file.fileName || '').toLowerCase();
  return contentType === 'text/html' || /\.html?$/i.test(fileName);
}

function isMarkdownFile(file?: FileApi.FileItem | null) {
  if (!file) {
    return false;
  }

  const contentType = (file.contentType || '').toLowerCase();
  const fileName = (file.fileName || '').toLowerCase();
  return contentType === 'text/markdown' || /\.(md|markdown|mdown|mkd)$/i.test(fileName);
}

function buildHtmlPreview(content: string) {
  return content;
}

function buildMarkdownPreview(content: string) {
  const rendered = marked.parse(content, {
    async: false,
    breaks: true,
    gfm: true,
  });
  return `<!doctype html>
<html>
<head>
  <meta charset="utf-8" />
  <style>
@reference "@vben/tailwind-config/theme";
    body {
      margin: 0;
      @apply p-8;
      color: #1f2937;
      background: #fff;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      line-height: 1.72;
    }
    .markdown-body {
      max-width: 920px;
      margin: 0 auto;
    }
    h1, h2, h3, h4 {
      margin: 1.4em 0 0.6em;
      color: #111827;
      line-height: 1.3;
    }
    h1 {
      padding-bottom: 0.35em;
      border-bottom: 1px solid #e5e7eb;
      font-size: 28px;
    }
    h2 {
      padding-bottom: 0.3em;
      border-bottom: 1px solid #eef2f7;
      font-size: 22px;
    }
    p, ul, ol, blockquote, pre, table {
      @apply mb-4;
    }
    code {
      @apply px-1 py-0.5;
      background: #f3f4f6;
      @apply rounded;
      font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
      font-size: 0.92em;
    }
    pre {
      overflow: auto;
      @apply px-4 py-3.5;
      background: #0f172a;
      @apply rounded-lg;
    }
    pre code {
      padding: 0;
      color: #e5e7eb;
      background: transparent;
    }
    blockquote {
      @apply pl-3.5;
      color: #4b5563;
      border-left: 4px solid #dbeafe;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th, td {
      @apply px-2.5 py-2;
      border: 1px solid #e5e7eb;
      text-align: left;
    }
    th {
      background: #f8fafc;
    }
    img {
      max-width: 100%;
    }
  </style>
</head>
<body>
  <main class="markdown-body">${rendered}</main>
</body>
</html>`;
}
</script>

<template>
  <Modal
    :open="open"
    :title="title"
    width="min(1120px, calc(100vw - 48px))"
    wrap-class-name="file-preview-modal"
    @update:open="handleOpenChange"
  >
    <template #footer>
      <Button @click="handleOpenChange(false)">关闭</Button>
      <Button :loading="downloading" type="primary" @click="handleDownload">下载</Button>
    </template>
    <div class="file-preview-modal__body">
      <Skeleton v-if="loading" active :paragraph="{ rows: 8 }" />
      <iframe
        v-else-if="isRichTextPreview"
        class="file-preview-modal__html"
        sandbox=""
        :srcdoc="previewHtml"
        title="文件预览"
      />
      <FileViewer v-else-if="previewFileObject" :file="previewFileObject" />
      <Empty v-else description="暂无可预览内容" />
    </div>
  </Modal>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.file-preview-modal__body {
  height: min(78vh, 760px);
  min-height: 520px;
  overflow: hidden;
  background: #f5f7fb;
}

.file-preview-modal__body :deep(.file-viewer) {
  height: 100%;
}

.file-preview-modal__html {
  width: 100%;
  height: 100%;
  background: #fff;
  border: 0;
}

</style>
