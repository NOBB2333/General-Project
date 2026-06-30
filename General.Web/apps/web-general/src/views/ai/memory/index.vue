<script lang="ts" setup>
import type { AiApi, ProjectApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Drawer,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tabs,
  Tag,
  message,
} from 'ant-design-vue';

import {
  createAiMemoryApi,
  deleteAiMemoryApi,
  getAiKnowledgeSourcesApi,
  getAiMemoriesApi,
  getProjectListApi,
  importAiProjectDocumentMemoriesApi,
  importAiProjectMeetingMemoriesApi,
  importAiProjectRiskMemoriesApi,
  toggleAiMemoryApi,
  updateAiMemoryApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'AiMemoryPage' });

const memoryTypeOptions = [
  { label: '全部类型', value: '' },
  { label: '组织记忆', value: 'Organization' },
  { label: '项目记忆', value: 'Project' },
  { label: '用户记忆', value: 'User' },
  { label: '场景记忆', value: 'Scenario' },
];

const sourceTypeOptions = [
  { label: '全部来源', value: '' },
  { label: '项目文档', value: 'ProjectDocument' },
  { label: '会议纪要', value: 'ProjectMeeting' },
  { label: '风险复盘', value: 'ProjectRiskReview' },
];

const contextTypeOptions = [
  { label: '不限上下文', value: '' },
  { label: '项目', value: 'Project' },
  { label: '经营', value: 'Business' },
  { label: '风险', value: 'Risk' },
  { label: '文档', value: 'Document' },
];

const memoryColumns = [
  { dataIndex: 'title', key: 'title', title: '记忆', width: 300 },
  { dataIndex: 'memoryType', key: 'memoryType', title: '类型', width: 120 },
  { dataIndex: 'contextType', key: 'contextType', title: '上下文', width: 180 },
  { dataIndex: 'importance', key: 'importance', title: '权重', width: 90 },
  { dataIndex: 'knowledgeSourceTitle', key: 'knowledgeSourceTitle', title: '来源', width: 240 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'action', title: '操作', width: 230 },
];

const sourceColumns = [
  { dataIndex: 'title', key: 'title', title: '知识源', width: 320 },
  { dataIndex: 'sourceType', key: 'sourceType', title: '来源类型', width: 130 },
  { dataIndex: 'contextType', key: 'contextType', title: '上下文', width: 170 },
  { dataIndex: 'summary', key: 'summary', title: '摘要' },
  { dataIndex: 'lastIndexedTime', key: 'lastIndexedTime', title: '入库时间', width: 170 },
];

const memories = ref<AiApi.MemoryItem[]>([]);
const sources = ref<AiApi.KnowledgeSourceItem[]>([]);
const projects = ref<ProjectApi.ProjectListItem[]>([]);
const activeTab = ref('memories');
const loadingMemories = ref(false);
const loadingSources = ref(false);
const loadingProjects = ref(false);
const memoryDrawerVisible = ref(false);
const importDrawerVisible = ref(false);
const savingMemory = ref(false);
const editingMemoryId = ref('');
const { actionLoadingKey, runAction } = useActionLoading();

const memoryQuery = reactive<AiApi.MemoryQuery>({
  contextId: '',
  contextType: '',
  includeDisabled: true,
  keyword: '',
  maxResultCount: 200,
  memoryType: '',
});

const sourceQuery = reactive<AiApi.KnowledgeSourceQuery>({
  contextId: '',
  contextType: '',
  keyword: '',
  maxResultCount: 200,
  projectId: undefined,
  sourceType: '',
});

const memoryForm = reactive<AiApi.MemorySaveInput>({
  content: '',
  contextId: '',
  contextType: '',
  importance: 50,
  isEnabled: true,
  knowledgeSourceId: undefined,
  memoryType: 'Project',
  ownerUserId: undefined,
  sourceReferencesJson: '[]',
  summary: '',
  tagsJson: '[]',
  title: '',
});

const importForm = reactive({
  projectId: '',
});

const projectOptions = computed(() =>
  projects.value.map((item) => ({
    label: `${item.projectCode} · ${item.name}`,
    value: item.id,
  })),
);

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function memoryTypeLabel(value: string) {
  return memoryTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

function sourceTypeLabel(value: string) {
  return sourceTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

function normalizeMemoryQuery() {
  return {
    ...memoryQuery,
    contextId: memoryQuery.contextId?.trim() || undefined,
    contextType: memoryQuery.contextType || undefined,
    keyword: memoryQuery.keyword?.trim() || undefined,
    memoryType: memoryQuery.memoryType || undefined,
  };
}

function normalizeSourceQuery() {
  return {
    ...sourceQuery,
    contextId: sourceQuery.contextId?.trim() || undefined,
    contextType: sourceQuery.contextType || undefined,
    keyword: sourceQuery.keyword?.trim() || undefined,
    projectId: sourceQuery.projectId || undefined,
    sourceType: sourceQuery.sourceType || undefined,
  };
}

function validateJsonArray(value: string, label: string) {
  let parsed: unknown;
  try {
    parsed = JSON.parse(value.trim() || '[]');
  } catch {
    throw new Error(`${label} 必须是合法 JSON`);
  }

  if (!Array.isArray(parsed)) {
    throw new Error(`${label} 必须是 JSON 数组`);
  }
}

function validateMemoryForm() {
  if (!memoryForm.title.trim() || !memoryForm.content.trim()) {
    throw new Error('请填写记忆标题和内容');
  }

  validateJsonArray(memoryForm.tagsJson, '记忆标签');
  validateJsonArray(memoryForm.sourceReferencesJson, '来源引用');
}

async function loadMemories() {
  loadingMemories.value = true;
  try {
    memories.value = await getAiMemoriesApi(normalizeMemoryQuery());
  } finally {
    loadingMemories.value = false;
  }
}

async function loadSources() {
  loadingSources.value = true;
  try {
    sources.value = await getAiKnowledgeSourcesApi(normalizeSourceQuery());
  } finally {
    loadingSources.value = false;
  }
}

async function loadProjects() {
  loadingProjects.value = true;
  try {
    projects.value = await getProjectListApi();
  } finally {
    loadingProjects.value = false;
  }
}

async function loadAll() {
  await Promise.all([loadMemories(), loadSources(), loadProjects()]);
}

function resetMemoryForm() {
  Object.assign(memoryForm, {
    content: '',
    contextId: '',
    contextType: '',
    importance: 50,
    isEnabled: true,
    knowledgeSourceId: undefined,
    memoryType: 'Project',
    ownerUserId: undefined,
    sourceReferencesJson: '[]',
    summary: '',
    tagsJson: '[]',
    title: '',
  });
}

function openCreateMemory() {
  editingMemoryId.value = '';
  resetMemoryForm();
  memoryDrawerVisible.value = true;
}

function openEditMemory(record: AiApi.MemoryItem) {
  editingMemoryId.value = record.id;
  Object.assign(memoryForm, {
    content: record.content,
    contextId: record.contextId,
    contextType: record.contextType,
    importance: record.importance,
    isEnabled: record.isEnabled,
    knowledgeSourceId: record.knowledgeSourceId,
    memoryType: record.memoryType,
    ownerUserId: record.ownerUserId,
    sourceReferencesJson: record.sourceReferencesJson || '[]',
    summary: record.summary,
    tagsJson: record.tagsJson || '[]',
    title: record.title,
  });
  memoryDrawerVisible.value = true;
}

function openEditMemoryFromTable(record: Record<string, unknown>) {
  openEditMemory(record as unknown as AiApi.MemoryItem);
}

async function handleSaveMemory() {
  try {
    validateMemoryForm();
  } catch (error) {
    message.warning((error as Error).message);
    return;
  }

  savingMemory.value = true;
  try {
    const payload = {
      ...memoryForm,
      content: memoryForm.content.trim(),
      contextId: memoryForm.contextId.trim(),
      contextType: memoryForm.contextType.trim(),
      sourceReferencesJson: memoryForm.sourceReferencesJson.trim() || '[]',
      summary: memoryForm.summary.trim(),
      tagsJson: memoryForm.tagsJson.trim() || '[]',
      title: memoryForm.title.trim(),
    };
    if (editingMemoryId.value) {
      await updateAiMemoryApi(editingMemoryId.value, payload);
      message.success('记忆已保存');
    } else {
      await createAiMemoryApi(payload);
      message.success('记忆已创建');
    }

    memoryDrawerVisible.value = false;
    await loadMemories();
  } finally {
    savingMemory.value = false;
  }
}

async function handleToggleMemory(record: AiApi.MemoryItem) {
  await toggleAiMemoryApi(record.id, { isEnabled: !record.isEnabled });
  message.success(record.isEnabled ? '记忆已停用' : '记忆已启用');
  await loadMemories();
}

async function handleToggleMemoryFromTable(record: Record<string, unknown>) {
  await handleToggleMemory(record as unknown as AiApi.MemoryItem);
}

async function handleDeleteMemory(record: AiApi.MemoryItem) {
  await deleteAiMemoryApi(record.id);
  message.success('记忆已删除');
  await loadMemories();
}

async function handleDeleteMemoryFromTable(record: Record<string, unknown>) {
  await handleDeleteMemory(record as unknown as AiApi.MemoryItem);
}

function openImportDrawer() {
  importForm.projectId = projects.value[0]?.id || '';
  importDrawerVisible.value = true;
}

async function handleImport(kind: 'documents' | 'meetings' | 'risks') {
  if (!importForm.projectId) {
    message.warning('请选择项目');
    return;
  }

  const key = `import-memory:${kind}`;
  await runAction(key, async () => {
    const result = kind === 'documents'
      ? await importAiProjectDocumentMemoriesApi(importForm.projectId)
      : kind === 'meetings'
        ? await importAiProjectMeetingMemoriesApi(importForm.projectId)
        : await importAiProjectRiskMemoriesApi(importForm.projectId);
    message.success(`入库 ${result.importedCount} 条，跳过 ${result.skippedCount} 条`);
    await Promise.all([loadMemories(), loadSources()]);
  });
}

onMounted(loadAll);
</script>

<template>
  <Page description="维护组织、项目、用户、场景记忆，并把项目文档、会议纪要和风险复盘沉淀为可引用知识源。" title="记忆库">
    <div class="ai-memory-page">
      <Card :bordered="false" class="ai-memory-page__summary rounded-2xl">
        <div class="ai-memory-page__summary-grid">
          <div class="ai-memory-page__metric">
            <span>记忆总数</span>
            <strong>{{ memories.length }}</strong>
          </div>
          <div class="ai-memory-page__metric">
            <span>启用记忆</span>
            <strong>{{ memories.filter((item) => item.isEnabled).length }}</strong>
          </div>
          <div class="ai-memory-page__metric">
            <span>知识源</span>
            <strong>{{ sources.length }}</strong>
          </div>
          <div class="ai-memory-page__metric">
            <span>项目来源</span>
            <strong>{{ sources.filter((item) => item.contextType === 'Project').length }}</strong>
          </div>
        </div>
      </Card>

      <Card :bordered="false" class="rounded-2xl">
        <Tabs v-model:activeKey="activeTab">
          <Tabs.TabPane key="memories" tab="记忆">
            <div class="ai-memory-page__toolbar">
              <div class="ai-memory-page__filters">
                <Select
                  v-model:value="memoryQuery.memoryType"
                  class="ai-memory-page__select"
                  :options="memoryTypeOptions"
                  @change="loadMemories"
                />
                <Select
                  v-model:value="memoryQuery.contextType"
                  class="ai-memory-page__select"
                  :options="contextTypeOptions"
                  @change="loadMemories"
                />
                <Input
                  v-model:value="memoryQuery.contextId"
                  allow-clear
                  class="ai-memory-page__input"
                  placeholder="上下文编号"
                  @press-enter="loadMemories"
                />
                <Input
                  v-model:value="memoryQuery.keyword"
                  allow-clear
                  class="ai-memory-page__input"
                  placeholder="搜索记忆"
                  @press-enter="loadMemories"
                />
                <Switch
                  v-model:checked="memoryQuery.includeDisabled"
                  checked-children="含停用"
                  un-checked-children="仅启用"
                  @change="loadMemories"
                />
                <Button @click="loadMemories">查询</Button>
              </div>
              <Space>
                <Button @click="openImportDrawer">项目摘要入库</Button>
                <Button type="primary" @click="openCreateMemory">新增记忆</Button>
              </Space>
            </div>
            <Table
              :columns="memoryColumns"
              :data-source="memories"
              :loading="loadingMemories"
              :pagination="{ pageSize: 20, showSizeChanger: true }"
              row-key="id"
              size="middle"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'title'">
                  <Space direction="vertical" :size="2">
                    <strong>{{ record.title }}</strong>
                    <span class="ai-memory-page__muted">{{ record.summary || record.content }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'memoryType'">
                  <Tag>{{ memoryTypeLabel(record.memoryType) }}</Tag>
                </template>
                <template v-else-if="column.key === 'contextType'">
                  <Space direction="vertical" :size="2">
                    <span>{{ record.contextType || '-' }}</span>
                    <span class="ai-memory-page__muted">{{ record.contextId || '-' }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'knowledgeSourceTitle'">
                  <span>{{ record.knowledgeSourceTitle || '-' }}</span>
                </template>
                <template v-else-if="column.key === 'isEnabled'">
                  <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
                </template>
                <template v-else-if="column.key === 'action'">
                  <Space>
                    <Button size="small" @click="openEditMemoryFromTable(record)">编辑</Button>
                    <Button size="small" @click="handleToggleMemoryFromTable(record)">
                      {{ record.isEnabled ? '停用' : '启用' }}
                    </Button>
                    <Popconfirm title="确认删除该记忆？" @confirm="handleDeleteMemoryFromTable(record)">
                      <Button danger size="small">删除</Button>
                    </Popconfirm>
                  </Space>
                </template>
              </template>
            </Table>
          </Tabs.TabPane>

          <Tabs.TabPane key="sources" tab="知识源">
            <div class="ai-memory-page__filters">
              <Select
                v-model:value="sourceQuery.sourceType"
                class="ai-memory-page__select"
                :options="sourceTypeOptions"
                @change="loadSources"
              />
              <Select
                v-model:value="sourceQuery.projectId"
                allow-clear
                class="ai-memory-page__select"
                :loading="loadingProjects"
                placeholder="按项目筛选"
                :options="projectOptions"
                @change="loadSources"
              />
              <Input
                v-model:value="sourceQuery.keyword"
                allow-clear
                class="ai-memory-page__input"
                placeholder="搜索知识源"
                @press-enter="loadSources"
              />
              <Button @click="loadSources">查询</Button>
            </div>
            <Table
              :columns="sourceColumns"
              :data-source="sources"
              :loading="loadingSources"
              :pagination="{ pageSize: 20, showSizeChanger: true }"
              row-key="id"
              size="middle"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'title'">
                  <Space direction="vertical" :size="2">
                    <strong>{{ record.title }}</strong>
                    <span class="ai-memory-page__muted">{{ record.sourceId }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'sourceType'">
                  <Tag>{{ sourceTypeLabel(record.sourceType) }}</Tag>
                </template>
                <template v-else-if="column.key === 'contextType'">
                  <Space direction="vertical" :size="2">
                    <span>{{ record.contextType || '-' }}</span>
                    <span class="ai-memory-page__muted">{{ record.contextId || '-' }}</span>
                  </Space>
                </template>
                <template v-else-if="column.key === 'summary'">
                  <span>{{ record.summary || record.contentText }}</span>
                </template>
                <template v-else-if="column.key === 'lastIndexedTime'">
                  {{ formatDate(record.lastIndexedTime) }}
                </template>
              </template>
            </Table>
          </Tabs.TabPane>
        </Tabs>
      </Card>
    </div>

    <Drawer
      v-model:open="memoryDrawerVisible"
      :title="editingMemoryId ? '编辑记忆' : '新增记忆'"
      width="820px"
    >
      <Form layout="vertical">
        <div class="ai-memory-page__form-grid">
          <Form.Item label="记忆类型" required>
            <Select v-model:value="memoryForm.memoryType" :options="memoryTypeOptions.filter((item) => item.value)" />
          </Form.Item>
          <Form.Item label="权重">
            <InputNumber v-model:value="memoryForm.importance" class="ai-memory-page__number" :max="100" :min="0" />
          </Form.Item>
          <Form.Item label="标题" required>
            <Input v-model:value="memoryForm.title" :maxlength="256" />
          </Form.Item>
          <Form.Item label="启用">
            <Switch v-model:checked="memoryForm.isEnabled" checked-children="启用" un-checked-children="停用" />
          </Form.Item>
          <Form.Item label="上下文类型">
            <Select v-model:value="memoryForm.contextType" :options="contextTypeOptions" />
          </Form.Item>
          <Form.Item label="上下文编号">
            <Input v-model:value="memoryForm.contextId" :maxlength="128" />
          </Form.Item>
        </div>
        <Form.Item label="摘要">
          <Input.TextArea v-model:value="memoryForm.summary" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="2000" />
        </Form.Item>
        <Form.Item label="内容" required>
          <Input.TextArea v-model:value="memoryForm.content" :auto-size="{ minRows: 6, maxRows: 12 }" :maxlength="8000" />
        </Form.Item>
        <Form.Item label="标签 JSON 数组">
          <Input.TextArea v-model:value="memoryForm.tagsJson" :auto-size="{ minRows: 2, maxRows: 5 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="来源引用 JSON 数组">
          <Input.TextArea v-model:value="memoryForm.sourceReferencesJson" :auto-size="{ minRows: 2, maxRows: 5 }" :maxlength="16_000" />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="memoryDrawerVisible = false">取消</Button>
          <Button :loading="savingMemory" type="primary" @click="handleSaveMemory">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer v-model:open="importDrawerVisible" title="项目摘要入库" width="620px">
      <Form layout="vertical">
        <Form.Item label="项目" required>
          <Select
            v-model:value="importForm.projectId"
            show-search
            :filter-option="false"
            :loading="loadingProjects"
            placeholder="选择项目"
            :options="projectOptions"
          />
        </Form.Item>
        <Space wrap>
          <Button
            :loading="actionLoadingKey === 'import-memory:documents'"
            type="primary"
            @click="handleImport('documents')"
          >
            导入项目文档摘要
          </Button>
          <Button
            :loading="actionLoadingKey === 'import-memory:meetings'"
            @click="handleImport('meetings')"
          >
            导入会议纪要
          </Button>
          <Button
            :loading="actionLoadingKey === 'import-memory:risks'"
            @click="handleImport('risks')"
          >
            导入风险复盘
          </Button>
        </Space>
      </Form>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.ai-memory-page {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.ai-memory-page__summary-grid {
  display: grid;
  @apply gap-4;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.ai-memory-page__metric {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  min-width: 0;
}

.ai-memory-page__metric span,
.ai-memory-page__muted {
  color: var(--ant-color-text-tertiary);
}

.ai-memory-page__metric strong {
  color: var(--ant-color-text);
  font-size: 24px;
  line-height: 32px;
}

.ai-memory-page__toolbar,
.ai-memory-page__filters {
  display: flex;
  flex-wrap: wrap;
  @apply gap-3;
  justify-content: space-between;
  @apply mb-3;
}

.ai-memory-page__filters {
  justify-content: flex-start;
}

.ai-memory-page__select,
.ai-memory-page__input {
  min-width: 220px;
}

.ai-memory-page__form-grid {
  display: grid;
  @apply gap-x-4;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.ai-memory-page__number {
  width: 100%;
}

@media (max-width: 768px) {
  .ai-memory-page__summary-grid,
  .ai-memory-page__form-grid {
    grid-template-columns: 1fr;
  }

  .ai-memory-page__select,
  .ai-memory-page__input {
    width: 100%;
  }
}
</style>
