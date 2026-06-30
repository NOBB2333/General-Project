<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, Modal, Select, Space, Statistic, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationMessageTemplatesApi,
  renderIntegrationMessageTemplateApi,
  saveIntegrationMessageTemplateApi,
} from '#/api/core';

defineOptions({ name: 'IntegrationTemplatesPage' });

const messageTypeOptions = [
  { label: '全部类型', value: '' },
  { label: '文本', value: 'Text' },
  { label: 'Markdown', value: 'Markdown' },
  { label: '互动卡片', value: 'Card' },
];

const editableMessageTypeOptions = messageTypeOptions.slice(1);

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '草稿', value: 'Draft' },
  { label: '启用', value: 'Active' },
  { label: '停用', value: 'Disabled' },
];

const editableStatusOptions = statusOptions.slice(1);

const columns = [
  { field: 'templateCode', title: '模板编码', width: 180 },
  { field: 'name', slots: { default: 'name' }, title: '模板名称', minWidth: 220 },
  { field: 'messageType', slots: { default: 'messageType' }, title: '类型', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'titleTemplate', showOverflow: 'tooltip', title: '标题模板', minWidth: 240 },
  { field: 'bodyTemplate', showOverflow: 'tooltip', title: '内容模板', minWidth: 320 },
  { field: 'creationTime', slots: { default: 'creationTime' }, title: '创建时间', width: 170 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 190 },
] satisfies VxeGridProps<IntegrationApi.MessageTemplateItem>['columns'];

const templates = ref<IntegrationApi.MessageTemplateItem[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const drawerOpen = ref(false);
const saving = ref(false);
const previewOpen = ref(false);
const rendering = ref(false);
const previewResult = ref<IntegrationApi.TemplateRenderResult>();
const editingId = ref<string>();
const pageIndex = ref(1);
const pageSize = ref(20);

const query = reactive<IntegrationApi.MessageTemplateQuery>({
  keyword: '',
  messageType: '',
  status: 'All',
});

const form = reactive<IntegrationApi.MessageTemplateSaveInput>({
  bodyTemplate: '',
  cardJson: '{}',
  messageType: 'Text',
  name: '',
  previewVariablesJson: '{\n  "projectName": "智慧园区交付项目",\n  "owner": "张三",\n  "riskLevel": "高"\n}',
  remark: '',
  status: 'Draft',
  templateCode: '',
  titleTemplate: '',
  variableSchemaJson: '{\n  "projectName": "项目名称",\n  "owner": "负责人",\n  "riskLevel": "风险等级"\n}',
});

const metrics = computed(() => [
  { label: '模板总数', value: totalCount.value },
  { label: '启用模板', value: templates.value.filter((item) => item.status === 'Active').length },
  { label: '草稿模板', value: templates.value.filter((item) => item.status === 'Draft').length },
  { label: '卡片模板', value: templates.value.filter((item) => item.messageType === 'Card').length },
]);

const gridOptions = computed<VxeGridProps<IntegrationApi.MessageTemplateItem>>(() => ({
  cellConfig: { height: 56 },
  columns,
  data: templates.value,
  emptyText: '暂无消息模板',
  loading: loading.value,
  pagerConfig: {
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: totalCount.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const gridEvents: NativeVxeGridEvents<IntegrationApi.MessageTemplateItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadTemplates();
  },
};

const [TemplateGrid] = useVbenVxeGrid<IntegrationApi.MessageTemplateItem>({
  gridEvents,
  gridOptions: {},
});

function buildQuery() {
  return {
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    messageType: query.messageType || undefined,
    skipCount: (pageIndex.value - 1) * pageSize.value,
    status: query.status || 'All',
  };
}

function formatDate(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function formatJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value || '{}'), null, 2);
  } catch {
    return value || '{}';
  }
}

function messageTypeLabel(value: string) {
  return editableMessageTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Active') {
    return 'green';
  }

  if (value === 'Draft') {
    return 'blue';
  }

  return 'default';
}

function assertJson(value: string, label: string) {
  try {
    JSON.parse(value || '{}');
    return true;
  } catch {
    message.warning(`${label} 必须是合法 JSON`);
    return false;
  }
}

function resetForm() {
  editingId.value = undefined;
  Object.assign(form, {
    bodyTemplate: '',
    cardJson: '{}',
    id: undefined,
    messageType: 'Text',
    name: '',
    previewVariablesJson: '{\n  "projectName": "智慧园区交付项目",\n  "owner": "张三",\n  "riskLevel": "高"\n}',
    remark: '',
    status: 'Draft',
    templateCode: '',
    titleTemplate: '',
    variableSchemaJson: '{\n  "projectName": "项目名称",\n  "owner": "负责人",\n  "riskLevel": "风险等级"\n}',
  });
}

function openCreate() {
  resetForm();
  drawerOpen.value = true;
}

function openEdit(item: IntegrationApi.MessageTemplateItem) {
  editingId.value = item.id;
  Object.assign(form, {
    bodyTemplate: item.bodyTemplate,
    cardJson: item.cardJson,
    id: item.id,
    messageType: item.messageType,
    name: item.name,
    previewVariablesJson: item.previewVariablesJson,
    remark: item.remark,
    status: item.status,
    templateCode: item.templateCode,
    titleTemplate: item.titleTemplate,
    variableSchemaJson: item.variableSchemaJson,
  });
  drawerOpen.value = true;
}

async function loadTemplates() {
  loading.value = true;
  try {
    const result = await getIntegrationMessageTemplatesApi(buildQuery());
    templates.value = result.items;
    totalCount.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

async function refresh(resetPage = false) {
  if (resetPage) {
    pageIndex.value = 1;
  }

  await loadTemplates();
}

async function handleSave() {
  if (!form.templateCode.trim() || !form.name.trim()) {
    message.warning('请填写模板编码和模板名称');
    return;
  }

  if (
    !assertJson(form.cardJson, '卡片 JSON') ||
    !assertJson(form.variableSchemaJson, '变量 Schema') ||
    !assertJson(form.previewVariablesJson, '预览变量')
  ) {
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationMessageTemplateApi({ ...form, id: editingId.value });
    message.success('消息模板已保存');
    drawerOpen.value = false;
    await loadTemplates();
  } finally {
    saving.value = false;
  }
}

async function openPreview(item?: IntegrationApi.MessageTemplateItem) {
  const source = item
    ? {
        bodyTemplate: item.bodyTemplate,
        cardJson: item.cardJson,
        titleTemplate: item.titleTemplate,
        variablesJson: item.previewVariablesJson,
      }
    : {
        bodyTemplate: form.bodyTemplate,
        cardJson: form.cardJson,
        titleTemplate: form.titleTemplate,
        variablesJson: form.previewVariablesJson,
      };

  if (!assertJson(source.variablesJson, '预览变量')) {
    return;
  }

  previewOpen.value = true;
  rendering.value = true;
  try {
    previewResult.value = await renderIntegrationMessageTemplateApi(source);
  } finally {
    rendering.value = false;
  }
}

onMounted(loadTemplates);
</script>

<template>
  <Page description="维护飞书、钉钉、企业微信等外部系统通知模板，统一变量口径和测试预览。" title="消息模板">
    <div class="integration-templates flex flex-col gap-4">
      <div class="integration-templates__metrics grid grid-cols-4 gap-3">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="模板列表">
        <template #extra>
          <Space>
            <Button @click="refresh(false)">刷新</Button>
            <Button type="primary" @click="openCreate">新增模板</Button>
          </Space>
        </template>

        <div class="integration-templates__filters mb-3 grid gap-3">
          <Input v-model:value="query.keyword" allow-clear placeholder="搜索编码、名称、标题、内容" @press-enter="refresh(true)" />
          <Select v-model:value="query.messageType" :options="messageTypeOptions" @change="refresh(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="refresh(true)" />
          <Button type="primary" @click="refresh(true)">查询</Button>
        </div>

        <TemplateGrid :grid-events="gridEvents" :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #name="{ row }">
            <div class="integration-templates__stack-cell">
              <strong>{{ row.name }}</strong>
              <span>{{ row.remark || '未填写说明' }}</span>
            </div>
          </template>

          <template #messageType="{ row }">
            <Tag color="blue">{{ messageTypeLabel(row.messageType) }}</Tag>
          </template>

          <template #status="{ row }">
            <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
          </template>

          <template #creationTime="{ row }">
            {{ formatDate(row.creationTime) }}
          </template>

          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openEdit(row)">编辑</Button>
              <Button size="small" type="link" @click="openPreview(row)">预览</Button>
            </Space>
          </template>
        </TemplateGrid>
      </Card>
    </div>

    <Drawer v-model:open="drawerOpen" :destroy-on-close="true" :title="editingId ? '编辑消息模板' : '新增消息模板'" width="820">
      <template #extra>
        <Space>
          <Button @click="openPreview()">预览</Button>
          <Button @click="drawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSave">保存</Button>
        </Space>
      </template>

      <Form :label-col="{ span: 5 }" :model="form" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="模板编码" required>
          <Input v-model:value="form.templateCode" :maxlength="64" placeholder="project-risk-created" />
        </Form.Item>
        <Form.Item label="模板名称" required>
          <Input v-model:value="form.name" :maxlength="128" placeholder="项目风险新增通知" />
        </Form.Item>
        <Form.Item label="消息类型">
          <Select v-model:value="form.messageType" :options="editableMessageTypeOptions" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="form.status" :options="editableStatusOptions" />
        </Form.Item>
        <Form.Item label="标题模板">
          <Input v-model:value="form.titleTemplate" :maxlength="256" placeholder="{{projectName}} 有新的风险需要处理" />
        </Form.Item>
        <Form.Item label="内容模板">
          <Input.TextArea v-model:value="form.bodyTemplate" :auto-size="{ minRows: 8, maxRows: 16 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="卡片 JSON">
          <Input.TextArea v-model:value="form.cardJson" :auto-size="{ minRows: 6, maxRows: 14 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="变量 Schema">
          <Input.TextArea v-model:value="form.variableSchemaJson" :auto-size="{ minRows: 5, maxRows: 12 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="预览变量">
          <Input.TextArea v-model:value="form.previewVariablesJson" :auto-size="{ minRows: 5, maxRows: 12 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
      </Form>
    </Drawer>

    <Modal v-model:open="previewOpen" :footer="null" :loading="rendering" title="模板预览" width="760px">
      <div v-if="previewResult" class="integration-templates__preview flex flex-col gap-4">
        <section>
          <h3>标题</h3>
          <p>{{ previewResult.renderedTitle || '-' }}</p>
        </section>
        <section>
          <h3>内容</h3>
          <pre>{{ previewResult.renderedBody || '-' }}</pre>
        </section>
        <section>
          <h3>卡片 JSON</h3>
          <pre>{{ formatJson(previewResult.renderedCardJson) }}</pre>
        </section>
        <section v-if="previewResult.missingVariables.length > 0">
          <h3>缺失变量</h3>
          <Space :size="4" wrap>
            <Tag v-for="item in previewResult.missingVariables" :key="item" color="red">{{ item }}</Tag>
          </Space>
        </section>
      </div>
    </Modal>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-templates {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-templates__metrics {
  display: grid;
   @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.integration-templates__filters {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(260px, 1fr) minmax(150px, 180px) minmax(150px, 180px) auto;
   @apply mb-3;
}

.integration-templates__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-templates__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.integration-templates__preview {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-templates__preview h3 {
   @apply mb-2;
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.integration-templates__preview p,
.integration-templates__preview pre {
  @apply m-0;
   @apply p-3;
  border: 1px solid var(--ant-color-border-secondary);
   @apply rounded-md;
  background: var(--ant-color-fill-quaternary);
  color: var(--ant-color-text);
}

.integration-templates__preview pre {
  max-height: 260px;
  overflow: auto;
  font-size: 12px;
  white-space: pre-wrap;
}

@media (max-width: 960px) {
  .integration-templates__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .integration-templates__filters {
    grid-template-columns: 1fr;
  }
}
</style>
