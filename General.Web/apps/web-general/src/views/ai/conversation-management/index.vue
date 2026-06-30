<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Input, Popconfirm, Select, Space, Spin, Tag, message } from 'ant-design-vue';

import {
  archiveAiConversationApi,
  deleteAiConversationApi,
  exportAiConversationApi,
  getAiConversationsApi,
  restoreAiConversationApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiConversationManagementPage' });

const router = useRouter();
const { actionLoadingKey, runAction } = useActionLoading();

const conversations = ref<AiApi.ConversationItem[]>([]);
const keyword = ref('');
const status = ref('All');
const loading = ref(false);

const statusOptions = [
  { label: '全部', value: 'All' },
  { label: '进行中', value: 'Active' },
  { label: '已归档', value: 'Archived' },
];

async function loadData() {
  loading.value = true;
  try {
    conversations.value = await getAiConversationsApi({
      keyword: keyword.value.trim() || undefined,
      status: status.value,
    });
  } finally {
    loading.value = false;
  }
}

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function getStatusColor(value?: string) {
  return value === 'Archived' ? 'default' : 'green';
}

function getStatusLabel(value?: string) {
  return value === 'Archived' ? '已归档' : '进行中';
}

function openConversation(item: AiApi.ConversationItem) {
  router.push({
    path: '/ai/conversations',
    query: { id: item.id },
  });
}

async function archiveConversation(item: AiApi.ConversationItem) {
  await runAction(`archive:${item.id}`, async () => {
    await archiveAiConversationApi(item.id);
    message.success('会话已归档');
    await loadData();
  });
}

async function restoreConversation(item: AiApi.ConversationItem) {
  await runAction(`restore:${item.id}`, async () => {
    await restoreAiConversationApi(item.id);
    message.success('会话已恢复');
    await loadData();
  });
}

async function deleteConversation(item: AiApi.ConversationItem) {
  await runAction(`delete:${item.id}`, async () => {
    await deleteAiConversationApi(item.id);
    message.success('会话已删除');
    await loadData();
  });
}

async function exportConversation(item: AiApi.ConversationItem) {
  await runAction(`export:${item.id}`, async () => {
    const result = await exportAiConversationApi(item.id);
    const blob = new Blob([result.content], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = result.fileName || `${item.title}.md`;
    link.click();
    URL.revokeObjectURL(url);
  });
}

onMounted(loadData);
</script>

<template>
  <Page title="会话管理">
    <section class="ai-session-management">
      <Card :bordered="false" class="ai-session-management__toolbar rounded-2xl">
        <div class="ai-session-management__toolbar-body">
          <Input.Search
            v-model:value="keyword"
            allow-clear
            class="ai-session-management__search"
            placeholder="搜索会话标题或摘要"
            @search="loadData"
          />
          <Select v-model:value="status" class="ai-session-management__status" :options="statusOptions" @change="loadData" />
          <Button :loading="loading" @click="loadData">刷新</Button>
        </div>
      </Card>

      <Card :bordered="false" class="rounded-2xl">
        <Spin :spinning="loading">
          <Empty v-if="conversations.length === 0" description="暂无会话" />
          <div v-else class="ai-session-management__list">
            <article v-for="item in conversations" :key="item.id" class="ai-session-management__item">
              <div class="ai-session-management__main">
                <div class="ai-session-management__title">
                  <button type="button" @click="openConversation(item)">{{ item.title }}</button>
                  <Tag :color="getStatusColor(item.status)">{{ getStatusLabel(item.status) }}</Tag>
                </div>
                <p>{{ item.lastMessageSummary || '暂无消息' }}</p>
                <div class="ai-session-management__meta">
                  <span>模式 {{ item.mode }}</span>
                  <span>模型 {{ item.modelName || '未选择' }}</span>
                  <span>消息 {{ item.messageCount }}</span>
                  <span>Token {{ item.totalTokens }}</span>
                  <span>更新 {{ formatDateTime(item.lastMessageTime || item.creationTime) }}</span>
                </div>
              </div>
              <Space wrap>
                <Button size="small" @click="openConversation(item)">打开</Button>
                <Button :loading="actionLoadingKey === `export:${item.id}`" size="small" @click="exportConversation(item)">
                  导出
                </Button>
                <Button
                  v-if="item.status === 'Archived'"
                  :loading="actionLoadingKey === `restore:${item.id}`"
                  size="small"
                  @click="restoreConversation(item)"
                >
                  恢复
                </Button>
                <Button
                  v-else
                  :loading="actionLoadingKey === `archive:${item.id}`"
                  size="small"
                  @click="archiveConversation(item)"
                >
                  归档
                </Button>
                <Popconfirm title="确认删除该会话？" @confirm="deleteConversation(item)">
                  <Button :loading="actionLoadingKey === `delete:${item.id}`" danger size="small">删除</Button>
                </Popconfirm>
              </Space>
            </article>
          </div>
        </Spin>
      </Card>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-session-management {
  display: flex;
  flex-direction: column;
  @apply gap-3;
}

.ai-session-management__toolbar-body {
  display: flex;
  @apply gap-2;
  align-items: center;
}

.ai-session-management__search {
  max-width: 420px;
}

.ai-session-management__status {
  width: 128px;
}

.ai-session-management__list {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
}

.ai-session-management__item {
  display: flex;
  @apply gap-4;
  align-items: flex-start;
  justify-content: space-between;
  @apply py-3.5;
  border-bottom: 1px solid var(--ant-color-border-secondary);
}

.ai-session-management__item:last-child {
  border-bottom: 0;
}

.ai-session-management__main {
  min-width: 0;
}

.ai-session-management__title {
  display: flex;
  @apply gap-2;
  align-items: center;
  @apply mb-1.5;
}

.ai-session-management__title button {
  @apply p-0;
  overflow: hidden;
  color: var(--ant-color-text);
  font-weight: 600;
  text-align: left;
  text-overflow: ellipsis;
  white-space: nowrap;
  cursor: pointer;
  background: transparent;
  border: 0;
}

.ai-session-management__title button:hover {
  color: var(--ant-color-primary);
}

.ai-session-management__item p {
  display: -webkit-box;
  @apply mb-2;
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.ai-session-management__meta {
  display: flex;
  flex-wrap: wrap;
  @apply gap-x-3.5 gap-y-2;
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
}

@media (max-width: 768px) {
  .ai-session-management__toolbar-body,
  .ai-session-management__item {
    flex-direction: column;
  }

  .ai-session-management__search,
  .ai-session-management__status {
    width: 100%;
    max-width: none;
  }
}
</style>
