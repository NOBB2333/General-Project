<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Drawer, Form, Input, InputNumber, Popconfirm, Select, Space, Switch, Table, Tabs, Tag, message } from 'ant-design-vue';

import {
  createAiWebSearchConfigApi,
  deleteAiWebSearchConfigApi,
  getAiWebSearchConfigsApi,
  getAiWebSearchRecordsApi,
  runAiWebSearchApi,
  updateAiWebSearchConfigApi,
} from '#/api/core';

defineOptions({ name: 'AiWebSearchPage' });

const activeTab = ref('configs');
const configs = ref<AiApi.WebSearchConfigItem[]>([]);
const records = ref<AiApi.WebSearchRecordItem[]>([]);
const loading = ref(false);
const recordLoading = ref(false);
const drawerVisible = ref(false);
const saving = ref(false);
const editingId = ref('');
const searchLoading = ref(false);
const searchResult = ref<AiApi.WebSearchResult>();

const recordQuery = reactive<AiApi.WebSearchRecordQuery>({
  keyword: '',
  maxResultCount: 50,
  status: undefined,
});

const searchForm = reactive<AiApi.WebSearchInput>({
  configId: undefined,
  maxResults: 5,
  query: '',
});

const form = reactive<AiApi.WebSearchConfigSaveInput>({
  allowDomains: [],
  apiKey: '',
  configJson: '{}',
  denyDomains: [],
  endpoint: '',
  isDefault: false,
  isEnabled: true,
  maxResults: 5,
  name: '',
  providerType: 'Tavily',
  remark: '',
  timeoutSeconds: 15,
});

const providerOptions = [
  { label: 'Tavily', value: 'Tavily' },
  { label: 'Bing Web Search', value: 'BingWebSearch' },
  { label: 'SerpApi', value: 'SerpApi' },
  { label: '自定义 JSON', value: 'Custom' },
];

const statusOptions = [
  { label: '完成', value: 'Completed' },
  { label: '失败', value: 'Failed' },
  { label: '执行中', value: 'Running' },
];

const configOptions = computed(() => configs.value.map((item) => ({ label: `${item.name} · ${item.providerType}`, value: item.id })));

const configColumns = [
  { dataIndex: 'name', key: 'name', title: 'Provider', width: 260 },
  { dataIndex: 'providerType', key: 'providerType', title: '类型', width: 140 },
  { dataIndex: 'endpoint', key: 'endpoint', title: 'Endpoint' },
  { dataIndex: 'maxResults', key: 'maxResults', title: '结果上限', width: 100 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 170 },
];

const recordColumns = [
  { dataIndex: 'query', key: 'query', title: '查询', width: 260 },
  { dataIndex: 'providerName', key: 'providerName', title: 'Provider', width: 170 },
  { dataIndex: 'resultCount', key: 'resultCount', title: '结果数', width: 90 },
  { dataIndex: 'elapsedMilliseconds', key: 'elapsedMilliseconds', title: '耗时', width: 100 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  { dataIndex: 'creationTime', key: 'creationTime', title: '时间', width: 190 },
];

async function loadConfigs() {
  loading.value = true;
  try {
    configs.value = await getAiWebSearchConfigsApi(true);
  } finally {
    loading.value = false;
  }
}

async function loadRecords() {
  recordLoading.value = true;
  try {
    records.value = await getAiWebSearchRecordsApi({
      keyword: recordQuery.keyword?.trim() || undefined,
      maxResultCount: recordQuery.maxResultCount,
      status: recordQuery.status || undefined,
    });
  } finally {
    recordLoading.value = false;
  }
}

async function loadAll() {
  await Promise.all([loadConfigs(), loadRecords()]);
}

function resetForm() {
  Object.assign(form, {
    allowDomains: [],
    apiKey: '',
    configJson: '{}',
    denyDomains: [],
    endpoint: '',
    isDefault: configs.value.length === 0,
    isEnabled: true,
    maxResults: 5,
    name: '',
    providerType: 'Tavily',
    remark: '',
    timeoutSeconds: 15,
  });
}

function openCreate() {
  editingId.value = '';
  resetForm();
  drawerVisible.value = true;
}

function openEdit(record: Record<string, any>) {
  const item = record as AiApi.WebSearchConfigItem;
  editingId.value = item.id;
  Object.assign(form, {
    allowDomains: [...item.allowDomains],
    apiKey: '',
    configJson: item.configJson || '{}',
    denyDomains: [...item.denyDomains],
    endpoint: item.endpoint,
    isDefault: item.isDefault,
    isEnabled: item.isEnabled,
    maxResults: item.maxResults,
    name: item.name,
    providerType: item.providerType,
    remark: item.remark,
    timeoutSeconds: item.timeoutSeconds,
  });
  drawerVisible.value = true;
}

function validateJson(value: string, label: string) {
  try {
    JSON.parse(value.trim() || '{}');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }
}

async function handleSubmit() {
  try {
    validateJson(form.configJson, '扩展配置');
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  saving.value = true;
  try {
    if (editingId.value) {
      await updateAiWebSearchConfigApi(editingId.value, form);
    } else {
      await createAiWebSearchConfigApi(form);
    }

    drawerVisible.value = false;
    message.success('联网搜索配置已保存');
    await loadConfigs();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(record: Record<string, any>) {
  await deleteAiWebSearchConfigApi((record as AiApi.WebSearchConfigItem).id);
  message.success('联网搜索配置已删除');
  await loadConfigs();
}

async function handleSearch() {
  if (!searchForm.query.trim()) {
    message.warning('请输入搜索内容');
    return;
  }

  searchLoading.value = true;
  try {
    searchResult.value = await runAiWebSearchApi({
      configId: searchForm.configId,
      maxResults: searchForm.maxResults,
      query: searchForm.query.trim(),
    });
    await loadRecords();
  } finally {
    searchLoading.value = false;
  }
}

onMounted(loadAll);
</script>

<template>
  <Page description="配置企业可控的联网搜索 Provider，所有搜索调用都会保存审计记录，供 AI 工具和人工测试复用。" title="联网搜索">
    <Alert
      class="ai-web-search__notice"
      message="联网搜索会把查询内容发送到外部 Provider。涉及客户、合同、人员和敏感项目数据时，必须先做脱敏或改用内部知识库。"
      show-icon
      type="warning"
    />

    <Card class="rounded-2xl" :bordered="false">
      <Tabs v-model:activeKey="activeTab">
        <Tabs.TabPane key="configs" tab="Provider 配置">
          <div class="ai-web-search__actions">
            <Button type="primary" @click="openCreate">新增 Provider</Button>
          </div>
          <Table :columns="configColumns" :data-source="configs" :loading="loading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'name'">
                <Space direction="vertical" :size="2">
                  <Space>
                    <strong>{{ record.name }}</strong>
                    <Tag v-if="record.isDefault" color="blue">默认</Tag>
                  </Space>
                  <span class="ai-web-search__muted">{{ record.remark || '-' }}</span>
                </Space>
              </template>
              <template v-else-if="column.key === 'endpoint'">
                <span class="ai-web-search__endpoint">{{ record.endpoint || '使用默认 Endpoint' }}</span>
              </template>
              <template v-else-if="column.key === 'isEnabled'">
                <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
              </template>
              <template v-else-if="column.key === 'action'">
                <Space>
                  <Button type="link" @click="openEdit(record)">编辑</Button>
                  <Popconfirm title="确定删除该 Provider？" @confirm="handleDelete(record)">
                    <Button danger type="link">删除</Button>
                  </Popconfirm>
                </Space>
              </template>
            </template>
          </Table>
        </Tabs.TabPane>

        <Tabs.TabPane key="test" tab="搜索测试">
          <div class="ai-web-search__test">
            <Form layout="vertical">
              <Form.Item label="Provider">
                <Select v-model:value="searchForm.configId" allow-clear :options="configOptions" placeholder="不选则使用默认启用配置" />
              </Form.Item>
              <Form.Item label="搜索内容">
                <Input.Search v-model:value="searchForm.query" :loading="searchLoading" enter-button="搜索" placeholder="输入要检索的问题" @search="handleSearch" />
              </Form.Item>
              <Form.Item label="结果上限">
                <InputNumber v-model:value="searchForm.maxResults" :max="20" :min="1" class="ai-web-search__number" />
              </Form.Item>
            </Form>
            <Alert
              v-if="searchResult"
              :message="searchResult.status === 'Completed' ? `搜索完成，结果数 ${searchResult.resultCount}` : searchResult.errorMessage"
              :type="searchResult.status === 'Completed' ? 'success' : 'error'"
              show-icon
            />
            <Input.TextArea
              v-if="searchResult"
              :value="searchResult.responseSummaryJson || searchResult.errorMessage"
              :auto-size="{ minRows: 8, maxRows: 16 }"
              readonly
            />
          </div>
        </Tabs.TabPane>

        <Tabs.TabPane key="records" tab="调用审计">
          <div class="ai-web-search__actions">
            <Space>
              <Input v-model:value="recordQuery.keyword" allow-clear placeholder="搜索查询或 Provider" @press-enter="loadRecords" />
              <Select v-model:value="recordQuery.status" allow-clear :options="statusOptions" placeholder="状态" class="ai-web-search__select" />
              <Button @click="loadRecords">查询</Button>
            </Space>
          </div>
          <Table :columns="recordColumns" :data-source="records" :loading="recordLoading" :pagination="{ pageSize: 20, showSizeChanger: true }" row-key="id">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'status'">
                <Tag :color="record.status === 'Completed' ? 'green' : record.status === 'Failed' ? 'red' : 'blue'">{{ record.status }}</Tag>
              </template>
              <template v-else-if="column.key === 'elapsedMilliseconds'">
                {{ record.elapsedMilliseconds }} ms
              </template>
            </template>
          </Table>
        </Tabs.TabPane>
      </Tabs>
    </Card>

    <Drawer :open="drawerVisible" :title="editingId ? '编辑联网搜索 Provider' : '新增联网搜索 Provider'" width="680" @close="drawerVisible = false">
      <Form layout="vertical">
        <Form.Item label="名称"><Input v-model:value="form.name" /></Form.Item>
        <Form.Item label="Provider 类型"><Select v-model:value="form.providerType" :options="providerOptions" /></Form.Item>
        <Form.Item label="Endpoint"><Input v-model:value="form.endpoint" placeholder="留空时使用所选 Provider 默认地址" /></Form.Item>
        <Form.Item label="API Key"><Input.Password v-model:value="form.apiKey" placeholder="编辑时留空表示不修改密钥" /></Form.Item>
        <Form.Item label="允许域名"><Select v-model:value="form.allowDomains" mode="tags" placeholder="留空表示不限制 Provider Endpoint 域名" /></Form.Item>
        <Form.Item label="拒绝域名"><Select v-model:value="form.denyDomains" mode="tags" placeholder="命中后禁止调用" /></Form.Item>
        <Form.Item label="结果上限"><InputNumber v-model:value="form.maxResults" :max="20" :min="1" class="ai-web-search__number" /></Form.Item>
        <Form.Item label="超时秒数"><InputNumber v-model:value="form.timeoutSeconds" :max="60" :min="1" class="ai-web-search__number" /></Form.Item>
        <Form.Item label="扩展配置"><Input.TextArea v-model:value="form.configJson" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="8000" /></Form.Item>
        <Form.Item label="备注"><Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" /></Form.Item>
        <Form.Item label="默认"><Switch v-model:checked="form.isDefault" /></Form.Item>
        <Form.Item label="启用"><Switch v-model:checked="form.isEnabled" /></Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="drawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </Space>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-web-search__notice {
  @apply mb-3;
}

.ai-web-search__actions {
  display: flex;
  justify-content: flex-end;
  @apply mb-3;
}

.ai-web-search__test {
  max-width: 760px;
}

.ai-web-search__muted {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
}

.ai-web-search__endpoint {
  word-break: break-all;
}

.ai-web-search__number {
  width: 160px;
}

.ai-web-search__select {
  width: 160px;
}
</style>
