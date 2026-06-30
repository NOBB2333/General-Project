<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi, ProjectApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Drawer, Form, Input, Select, Space, Statistic, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationConnectorsApi,
  getIntegrationGroupBindingsApi,
  getProjectListApi,
  saveIntegrationGroupBindingApi,
} from '#/api/core';

defineOptions({ name: 'IntegrationGroupsPage' });

const BINDING_MANAGE_CODE = 'Integration.Binding.Manage';

const statusOptions = [
  { label: '全部状态', value: 'All' },
  { label: '待处理', value: 'Pending' },
  { label: '已绑定', value: 'Bound' },
  { label: '冲突', value: 'Conflict' },
  { label: '已停用', value: 'Disabled' },
];
const editStatusOptions = statusOptions.filter((item) => item.value !== 'All');
const groupTypeOptions = [
  { label: '群聊', value: 'Chat' },
  { label: '机器人', value: 'Robot' },
  { label: '频道', value: 'Channel' },
];
const objectTypeOptions = [
  { label: '项目', value: 'Project' },
  { label: '组织', value: 'OrganizationUnit' },
  { label: '用户', value: 'User' },
  { label: '经营对象', value: 'Business' },
  { label: '自定义', value: 'Custom' },
];

const columns = [
  { field: 'externalGroupName', slots: { default: 'externalGroup' }, title: '外部群 / 机器人', minWidth: 240 },
  { field: 'internalObjectName', slots: { default: 'internalObject' }, title: '关联内部对象', minWidth: 240 },
  { field: 'connectorCode', slots: { default: 'connector' }, title: '连接器', width: 170 },
  { field: 'groupType', slots: { default: 'groupType' }, title: '类型', width: 110 },
  { field: 'robotCode', minWidth: 180, showOverflow: 'tooltip', title: '机器人编码' },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'remark', minWidth: 220, showOverflow: 'tooltip', title: '备注' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<IntegrationApi.GroupBindingItem>['columns'];

const { hasAccessByCodes } = useAccess();
const canManage = computed(() => hasAccessByCodes([BINDING_MANAGE_CODE]));
const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const projects = ref<ProjectApi.ProjectListItem[]>([]);
const rows = ref<IntegrationApi.GroupBindingItem[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(20);
const loading = ref(false);
const saving = ref(false);
const drawerOpen = ref(false);
const editing = ref<IntegrationApi.GroupBindingItem>();

const query = reactive<IntegrationApi.GroupBindingQuery>({
  connectorId: '',
  keyword: '',
  status: 'All',
});

const form = reactive<IntegrationApi.GroupBindingSaveInput>(createForm());

const connectorOptions = computed(() => [
  { label: '全部连接器', value: '' },
  ...connectors.value.map((item) => ({ label: `${item.displayName} · ${item.connectorCode}`, value: item.id })),
]);
const enabledConnectorOptions = computed(() =>
  connectors.value.map((item) => ({ label: `${item.displayName} · ${item.connectorCode}`, value: item.id })),
);
const projectOptions = computed(() =>
  projects.value.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id })),
);
const boundCount = computed(() => rows.value.filter((item) => item.status === 'Bound').length);
const robotCount = computed(() => rows.value.filter((item) => item.robotCode || item.groupType === 'Robot').length);
const projectGroupCount = computed(() => rows.value.filter((item) => item.internalObjectType === 'Project').length);

const metrics = computed(() => [
  { label: '群配置', value: total.value },
  { label: '已绑定', value: boundCount.value },
  { label: '项目群', value: projectGroupCount.value },
  { label: '机器人', value: robotCount.value },
]);

const gridOptions = computed<VxeGridProps<IntegrationApi.GroupBindingItem>>(() => ({
  columns,
  data: rows.value,
  emptyText: '暂无群与机器人配置',
  loading: loading.value,
  pagerConfig: {
    currentPage: page.value,
    pageSize: pageSize.value,
    pageSizes: [10, 20, 50, 100],
    total: total.value,
  },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'medium',
}));

const gridEvents: NativeVxeGridEvents<IntegrationApi.GroupBindingItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    page.value = nextPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = nextPageSize || pageSize.value;
    void loadRows();
  },
};

const [GroupGrid] = useVbenVxeGrid<IntegrationApi.GroupBindingItem>({
  gridEvents,
  gridOptions: {},
});

function createForm(): IntegrationApi.GroupBindingSaveInput {
  return {
    connectorId: '',
    externalGroupId: '',
    externalGroupName: '',
    groupType: 'Chat',
    internalObjectId: '',
    internalObjectName: '',
    internalObjectType: 'Project',
    remark: '',
    robotCode: '',
    status: 'Bound',
  };
}

function normalizeQuery() {
  return {
    connectorId: query.connectorId || undefined,
    keyword: query.keyword?.trim() || undefined,
    maxResultCount: pageSize.value,
    skipCount: (page.value - 1) * pageSize.value,
    status: query.status || 'All',
  };
}

function connectorLabel(id: string) {
  const connector = connectors.value.find((item) => item.id === id);
  return connector ? `${connector.displayName} · ${connector.connectorCode}` : '-';
}

function statusLabel(value: string) {
  return statusOptions.find((item) => item.value === value)?.label || value || '-';
}

function statusColor(value: string) {
  if (value === 'Bound') {
    return 'green';
  }
  if (value === 'Conflict') {
    return 'red';
  }
  if (value === 'Pending') {
    return 'orange';
  }
  return 'default';
}

function groupTypeLabel(value: string) {
  return groupTypeOptions.find((item) => item.value === value)?.label || value || '-';
}

async function loadMetadata() {
  const [connectorResult, projectResult] = await Promise.all([
    getIntegrationConnectorsApi({ includeDisabled: false }),
    getProjectListApi(),
  ]);
  connectors.value = connectorResult;
  projects.value = projectResult;
}

async function loadRows(resetPage = false) {
  if (resetPage) {
    page.value = 1;
  }
  loading.value = true;
  try {
    const result = await getIntegrationGroupBindingsApi(normalizeQuery());
    rows.value = result.items;
    total.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

function applyProject(projectId: string) {
  const project = projects.value.find((item) => item.id === projectId);
  if (!project) {
    return;
  }
  form.internalObjectId = project.id;
  form.internalObjectName = `${project.projectCode} · ${project.name}`;
}

function openDrawer(item?: IntegrationApi.GroupBindingItem) {
  editing.value = item;
  Object.assign(form, createForm(), item ? {
    connectorId: item.connectorId,
    externalGroupId: item.externalGroupId,
    externalGroupName: item.externalGroupName,
    groupType: item.groupType || 'Chat',
    internalObjectId: item.internalObjectId,
    internalObjectName: item.internalObjectName,
    internalObjectType: item.internalObjectType || 'Project',
    remark: item.remark,
    robotCode: item.robotCode,
    status: item.status || 'Bound',
  } : { connectorId: query.connectorId || enabledConnectorOptions.value[0]?.value || '' });
  drawerOpen.value = true;
}

async function submit() {
  if (!form.connectorId) {
    message.warning('请选择连接器');
    return;
  }
  if (!form.externalGroupId.trim()) {
    message.warning('请填写外部群 ID 或机器人 Webhook 标识');
    return;
  }
  if (!form.internalObjectType.trim() || !form.internalObjectId.trim()) {
    message.warning('请填写内部对象类型和 ID');
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationGroupBindingApi(form);
    message.success('群与机器人配置已保存');
    drawerOpen.value = false;
    await loadRows();
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await loadMetadata();
  await loadRows();
});
</script>

<template>
  <Page description="维护项目群、群机器人和内部对象的绑定关系，供通知规则、项目群周报和外部消息投递统一引用。" title="群与机器人">
    <div class="integration-groups flex flex-col gap-4">
      <Alert
        message="群与机器人配置只维护内部对象到外部群、机器人或频道的映射；真实消息发送仍统一经过连接器、通知规则和投递日志。"
        show-icon
        type="info"
      />

      <div class="integration-groups__metrics grid grid-cols-4 gap-3">
        <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl" size="small">
          <Statistic :title="metric.label" :value="metric.value" />
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="群与机器人配置">
        <div class="integration-groups__toolbar mb-3 flex items-center justify-between gap-3">
          <Input.Search v-model:value="query.keyword" allow-clear placeholder="搜索外部群、内部对象、机器人" @search="loadRows(true)" />
          <Select v-model:value="query.connectorId" :options="connectorOptions" @change="loadRows(true)" />
          <Select v-model:value="query.status" :options="statusOptions" @change="loadRows(true)" />
          <Space>
            <Button @click="loadRows(true)">刷新</Button>
            <Button v-if="canManage" type="primary" @click="openDrawer()">新增配置</Button>
          </Space>
        </div>

        <GroupGrid :grid-events="gridEvents" :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #externalGroup="{ row }">
            <div class="integration-groups__stack-cell">
              <strong>{{ row.externalGroupName || row.externalGroupId }}</strong>
              <span>{{ row.externalGroupId }}</span>
            </div>
          </template>
          <template #internalObject="{ row }">
            <div class="integration-groups__stack-cell">
              <strong>{{ row.internalObjectName || row.internalObjectId }}</strong>
              <span>{{ row.internalObjectType }} · {{ row.internalObjectId }}</span>
            </div>
          </template>
          <template #connector="{ row }">
            {{ connectorLabel(row.connectorId) }}
          </template>
          <template #groupType="{ row }">
            {{ groupTypeLabel(row.groupType) }}
          </template>
          <template #status="{ row }">
            <Tag :color="statusColor(row.status)">{{ statusLabel(row.status) }}</Tag>
          </template>
          <template #actions="{ row }">
            <Button v-if="canManage" size="small" type="link" @click="openDrawer(row)">编辑</Button>
          </template>
        </GroupGrid>
      </Card>
    </div>

    <Drawer v-model:open="drawerOpen" :destroy-on-close="true" :title="editing ? '编辑群与机器人配置' : '新增群与机器人配置'" width="720">
      <template #extra>
        <Space>
          <Button @click="drawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="submit">保存</Button>
        </Space>
      </template>
      <Form layout="vertical">
        <Form.Item label="连接器" required>
          <Select v-model:value="form.connectorId" :disabled="!!editing" :options="enabledConnectorOptions" />
        </Form.Item>
        <Form.Item label="外部群 ID / 机器人 Webhook 标识" required>
          <Input v-model:value="form.externalGroupId" :disabled="!!editing" :maxlength="256" />
        </Form.Item>
        <Form.Item label="外部群名称">
          <Input v-model:value="form.externalGroupName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="类型">
          <Select v-model:value="form.groupType" :options="groupTypeOptions" />
        </Form.Item>
        <Form.Item label="关联对象类型">
          <Select v-model:value="form.internalObjectType" :options="objectTypeOptions" />
        </Form.Item>
        <Form.Item v-if="form.internalObjectType === 'Project'" label="选择项目">
          <Select :options="projectOptions" allow-clear show-search @change="(value) => applyProject(value as string)" />
        </Form.Item>
        <Form.Item label="内部对象 ID" required>
          <Input v-model:value="form.internalObjectId" :maxlength="128" />
        </Form.Item>
        <Form.Item label="内部对象名称">
          <Input v-model:value="form.internalObjectName" :maxlength="128" />
        </Form.Item>
        <Form.Item label="机器人编码">
          <Input v-model:value="form.robotCode" :maxlength="128" placeholder="机器人 Key、Webhook Key 或应用机器人编码" />
        </Form.Item>
        <Form.Item label="状态">
          <Select v-model:value="form.status" :options="editStatusOptions" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 3, maxRows: 6 }" :maxlength="512" />
        </Form.Item>
      </Form>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-groups {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-groups__metrics {
  display: grid;
   @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.integration-groups__toolbar {
  display: grid;
   @apply gap-3;
  grid-template-columns: minmax(240px, 1fr) minmax(220px, 0.8fr) minmax(160px, 0.5fr) auto;
   @apply mb-3;
}

.integration-groups__stack-cell {
  display: flex;
  flex-direction: column;
   @apply gap-0.5;
  min-width: 0;
}

.integration-groups__stack-cell span {
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 1120px) {
  .integration-groups__metrics,
  .integration-groups__toolbar {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .integration-groups__metrics,
  .integration-groups__toolbar {
    grid-template-columns: 1fr;
  }
}
</style>
