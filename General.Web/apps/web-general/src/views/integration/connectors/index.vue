<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';
import type { IntegrationApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Drawer,
  Form,
  Input,
  Modal,
  Select,
  Space,
  Switch,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  getIntegrationConnectorsApi,
  rotateIntegrationConnectorSecretApi,
  sendIntegrationTestMessageApi,
  saveIntegrationTenantConfigApi,
  testIntegrationConnectorApi,
  toggleIntegrationConnectorApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'IntegrationConnectorsPage' });

const CONNECTOR_ROTATE_SECRET_CODE = 'Integration.Connector.RotateSecret';

const capabilityLabelMap: Record<string, string> = {
  InteractiveCard: '互动卡片',
  MarkdownMessage: 'Markdown 消息',
  SendGroupMessage: '群通知',
  SendPersonalMessage: '个人通知',
  SyncDepartment: '部门同步',
  SyncUser: '用户同步',
  WebhookEvent: 'Webhook',
};

const columns = [
  { align: 'left', field: 'displayName', minWidth: 180, title: '连接器' },
  { field: 'providerType', title: '平台类型', width: 130 },
  { field: 'capabilities', minWidth: 260, slots: { default: 'capabilities' }, title: '能力' },
  { field: 'isConfigured', slots: { default: 'configured' }, title: '配置', width: 120 },
  { field: 'isTenantEnabled', slots: { default: 'tenantStatus' }, title: '租户启用', width: 130 },
  { field: 'lastTestStatus', slots: { default: 'lastTest' }, title: '连接测试', width: 180 },
  { field: 'remark', minWidth: 220, title: '说明' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 330 },
] satisfies VxeGridProps<IntegrationApi.ConnectorItem>['columns'];

const { hasAccessByCodes } = useAccess();
const connectors = ref<IntegrationApi.ConnectorItem[]>([]);
const loading = ref(false);
const drawerOpen = ref(false);
const sendModalOpen = ref(false);
const rotateModalOpen = ref(false);
const saving = ref(false);
const sending = ref(false);
const rotating = ref(false);
const editing = ref<IntegrationApi.ConnectorItem | null>(null);
const sendingConnector = ref<IntegrationApi.ConnectorItem | null>(null);
const rotatingConnector = ref<IntegrationApi.ConnectorItem | null>(null);
const { actionLoadingKey, runAction } = useActionLoading();
const canRotateSecret = computed(() => hasAccessByCodes([CONNECTOR_ROTATE_SECRET_CODE]));

const query = reactive<IntegrationApi.ConnectorQuery>({
  includeDisabled: true,
  keyword: '',
});

const form = reactive<IntegrationApi.TenantConfigSaveInput>({
  callbackPath: '',
  configJson: '{}',
  isEnabled: true,
  remark: '',
  secret: '',
});

const sendForm = reactive<IntegrationApi.MessageSendTestInput>({
  body: '',
  messageType: 'Text',
  targetId: '',
  targetType: 'OpenId',
});

const rotateForm = reactive<IntegrationApi.SecretRotateInput>({
  secret: '',
});

const targetTypeOptions = [
  { label: 'Open ID', value: 'OpenId' },
  { label: 'User ID', value: 'User' },
  { label: '群 Chat ID', value: 'Chat' },
  { label: 'Union ID', value: 'UnionId' },
  { label: '邮箱', value: 'Email' },
];

const messageTypeOptions = [
  { label: '文本', value: 'Text' },
  { label: '富文本', value: 'Markdown' },
  { label: '互动卡片', value: 'Card' },
];

const gridOptions = computed<VxeGridProps<IntegrationApi.ConnectorItem>>(() => ({
  columns,
  data: connectors.value,
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
}));

const totalCount = computed(() => connectors.value.length);
const configuredCount = computed(() => connectors.value.filter((item) => item.isConfigured).length);
const enabledCount = computed(() => connectors.value.filter((item) => item.isTenantEnabled).length);
const failedCount = computed(() => connectors.value.filter((item) => item.lastTestStatus === '失败').length);

const [ConnectorGrid] = useVbenVxeGrid<IntegrationApi.ConnectorItem>({
  gridOptions: {},
});

function getCapabilityLabel(value: string) {
  return capabilityLabelMap[value] ?? value;
}

function getTestTagColor(status: string) {
  if (status === '成功') {
    return 'green';
  }

  if (status === '失败') {
    return 'red';
  }

  return 'default';
}

async function loadConnectors() {
  loading.value = true;
  try {
    connectors.value = await getIntegrationConnectorsApi(query);
  } finally {
    loading.value = false;
  }
}

function resetForm() {
  Object.assign(form, {
    callbackPath: '',
    configJson: '{}',
    isEnabled: true,
    remark: '',
    secret: '',
  });
}

function openConfig(item: IntegrationApi.ConnectorItem) {
  editing.value = item;
  Object.assign(form, {
    callbackPath: '',
    configJson: item.configJson || '{}',
    isEnabled: item.isTenantEnabled,
    remark: item.remark,
    secret: '',
  });
  drawerOpen.value = true;
}

function openSendTest(item: IntegrationApi.ConnectorItem) {
  sendingConnector.value = item;
  Object.assign(sendForm, {
    body: '项目管理系统测试消息',
    messageType: 'Text',
    targetId: '',
    targetType: item.connectorCode === 'feishu' ? 'OpenId' : 'User',
  });
  sendModalOpen.value = true;
}

function openRotateSecret(item: IntegrationApi.ConnectorItem) {
  rotatingConnector.value = item;
  rotateForm.secret = '';
  rotateModalOpen.value = true;
}

function assertConfigJson() {
  try {
    JSON.parse(form.configJson || '{}');
  } catch {
    message.warning('连接配置必须是合法 JSON');
    return false;
  }

  return true;
}

async function handleSave() {
  if (!editing.value || !assertConfigJson()) {
    return;
  }

  saving.value = true;
  try {
    await saveIntegrationTenantConfigApi(editing.value.id, form);
    message.success('连接器配置已保存');
    drawerOpen.value = false;
    resetForm();
    await loadConnectors();
  } finally {
    saving.value = false;
  }
}

async function handleToggle(item: IntegrationApi.ConnectorItem, checked: boolean | number | string) {
  await runAction(`toggle:${item.id}`, async () => {
    await toggleIntegrationConnectorApi(item.id, { isEnabled: Boolean(checked) });
    await loadConnectors();
  });
}

async function handleTest(item: IntegrationApi.ConnectorItem) {
  await runAction(`test:${item.id}`, async () => {
    const result = await testIntegrationConnectorApi(item.id);
    if (result.success) {
      message.success(result.message);
    } else {
      message.warning(result.message);
    }

    await loadConnectors();
  });
}

async function handleSendTest() {
  if (!sendingConnector.value) {
    return;
  }

  if (!sendForm.targetId.trim()) {
    message.warning('请输入接收人 ID');
    return;
  }

  if (!sendForm.body.trim()) {
    message.warning('请输入消息内容');
    return;
  }

  sending.value = true;
  try {
    const result = await sendIntegrationTestMessageApi(sendingConnector.value.id, sendForm);
    if (result.success) {
      message.success(result.message);
      sendModalOpen.value = false;
    } else {
      message.warning(result.message || '测试消息发送失败，请到投递日志查看详情');
    }
  } finally {
    sending.value = false;
  }
}

async function handleRotateSecret() {
  if (!rotatingConnector.value) {
    return;
  }

  if (!rotateForm.secret.trim()) {
    message.warning('请输入新密钥');
    return;
  }

  rotating.value = true;
  try {
    await rotateIntegrationConnectorSecretApi(rotatingConnector.value.id, {
      secret: rotateForm.secret.trim(),
    });
    message.success('连接器密钥已轮换，请重新测试连接');
    rotateModalOpen.value = false;
    rotateForm.secret = '';
    await loadConnectors();
  } finally {
    rotating.value = false;
  }
}

onMounted(loadConnectors);
</script>

<template>
  <Page auto-content-height>
    <div class="integration-connectors flex flex-col gap-4">
      <div class="integration-connectors__metrics grid grid-cols-4 gap-3 max-[960px]:grid-cols-2 max-[640px]:grid-cols-1">
        <Card :bordered="false" class="rounded-2xl" size="small">
          <div class="integration-connectors__metric-label">连接器</div>
          <div class="integration-connectors__metric-value">{{ totalCount }}</div>
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small">
          <div class="integration-connectors__metric-label">已配置</div>
          <div class="integration-connectors__metric-value">{{ configuredCount }}</div>
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small">
          <div class="integration-connectors__metric-label">已启用</div>
          <div class="integration-connectors__metric-value">{{ enabledCount }}</div>
        </Card>
        <Card :bordered="false" class="rounded-2xl" size="small">
          <div class="integration-connectors__metric-label">测试失败</div>
          <div class="integration-connectors__metric-value integration-connectors__metric-value--danger">
            {{ failedCount }}
          </div>
        </Card>
      </div>

      <Card :bordered="false" class="rounded-2xl" title="连接器配置">
        <template #extra>
          <Space>
            <Input.Search
              v-model:value="query.keyword"
              allow-clear
              class="integration-connectors__search"
              placeholder="搜索连接器"
              @search="loadConnectors"
            />
            <Switch v-model:checked="query.includeDisabled" checked-children="全部" un-checked-children="启用" @change="loadConnectors" />
            <Button :loading="loading" @click="loadConnectors">刷新</Button>
          </Space>
        </template>

        <Alert
          class="integration-connectors__alert mb-3"
          message="当前已落地连接器基座、配置加密、能力声明、飞书/钉钉/企业微信 Token 校验、消息发送、部门/用户同步适配器和投递日志；真实租户联调、Webhook 验签和事件回流按第六期清单继续验收。"
          show-icon
          type="info"
        />

        <ConnectorGrid :grid-options="gridOptions" class="app-vxe-card-grid">
          <template #capabilities="{ row }">
            <Space :size="4" wrap>
              <Tag v-for="item in row.capabilities" :key="item" color="blue">{{ getCapabilityLabel(item) }}</Tag>
            </Space>
          </template>

          <template #configured="{ row }">
            <Space :size="4">
              <Tag :color="row.isConfigured ? 'green' : 'default'">{{ row.isConfigured ? '已配置' : '未配置' }}</Tag>
              <Tag :color="row.hasSecret ? 'cyan' : 'default'">{{ row.hasSecret ? '有密钥' : '无密钥' }}</Tag>
            </Space>
          </template>

          <template #tenantStatus="{ row }">
            <Switch
              :checked="row.isTenantEnabled"
              :loading="actionLoadingKey === `toggle:${row.id}`"
              checked-children="启用"
              un-checked-children="停用"
              @change="(checked) => handleToggle(row, checked)"
            />
          </template>

          <template #lastTest="{ row }">
            <Space direction="vertical" :size="2">
              <Tag :color="getTestTagColor(row.lastTestStatus)">{{ row.lastTestStatus || '未测试' }}</Tag>
              <span class="integration-connectors__test-message">{{ row.lastTestMessage }}</span>
            </Space>
          </template>

          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openConfig(row)">配置</Button>
              <Button
                :loading="actionLoadingKey === `test:${row.id}`"
                size="small"
                type="link"
                @click="handleTest(row)"
              >
                测试
              </Button>
              <Button v-if="canRotateSecret && row.isConfigured" size="small" type="link" @click="openRotateSecret(row)">
                轮换密钥
              </Button>
              <Button size="small" type="link" @click="openSendTest(row)">测试发送</Button>
            </Space>
          </template>
        </ConnectorGrid>
      </Card>
    </div>

    <Drawer
      v-model:open="drawerOpen"
      :destroy-on-close="true"
      :title="editing ? `配置 ${editing.displayName}` : '配置连接器'"
      width="720"
    >
      <template #extra>
        <Space>
          <Button @click="drawerOpen = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSave">保存</Button>
        </Space>
      </template>

      <Form :label-col="{ span: 5 }" :model="form" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="启用配置">
          <Switch v-model:checked="form.isEnabled" />
        </Form.Item>
        <Form.Item label="回调路径">
          <Input v-model:value="form.callbackPath" :maxlength="512" placeholder="/api/app/integration/webhook/feishu" />
        </Form.Item>
        <Form.Item label="连接密钥">
          <Input.Password
            v-model:value="form.secret"
            :maxlength="4096"
            :placeholder="editing?.hasSecret ? '不填写则沿用原密钥' : '请输入 AppSecret 或 Token'"
          />
        </Form.Item>
        <Form.Item label="连接配置">
          <Input.TextArea v-model:value="form.configJson" :auto-size="{ minRows: 10, maxRows: 18 }" :maxlength="16_000" />
        </Form.Item>
        <Form.Item label="配置 Schema">
          <Input.TextArea :value="editing?.configSchemaJson || '{}'" :auto-size="{ minRows: 8, maxRows: 14 }" disabled />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="form.remark" :auto-size="{ minRows: 2, maxRows: 4 }" :maxlength="512" />
        </Form.Item>
      </Form>
    </Drawer>

    <Drawer
      v-model:open="sendModalOpen"
      destroy-on-close
      :title="sendingConnector ? `测试发送 · ${sendingConnector.displayName}` : '测试发送'"
      width="640px"
    >
      <Form :label-col="{ span: 5 }" :model="sendForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="接收类型" required>
          <Select v-model:value="sendForm.targetType" :options="targetTypeOptions" />
        </Form.Item>
        <Form.Item label="接收人 ID" required>
          <Input v-model:value="sendForm.targetId" :maxlength="256" placeholder="Open ID / User ID / Chat ID / 邮箱" />
        </Form.Item>
        <Form.Item label="消息类型">
          <Select v-model:value="sendForm.messageType" :options="messageTypeOptions" />
        </Form.Item>
        <Form.Item label="消息内容" required>
          <Input.TextArea v-model:value="sendForm.body" :auto-size="{ minRows: 5, maxRows: 10 }" :maxlength="64_000" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="sendModalOpen = false">取消</Button>
          <Button :loading="sending" type="primary" @click="handleSendTest">发送测试</Button>
        </div>
      </template>
    </Drawer>

    <Modal
      v-model:open="rotateModalOpen"
      :confirm-loading="rotating"
      :title="rotatingConnector ? `轮换密钥 · ${rotatingConnector.displayName}` : '轮换密钥'"
      ok-text="确认轮换"
      width="560px"
      @ok="handleRotateSecret"
    >
      <Alert
        class="integration-connectors__rotate-alert mb-4"
        message="轮换后会清空已缓存的访问 Token，连接器会进入待重测状态；请在企业平台侧同步更新密钥后再执行连接测试。"
        show-icon
        type="warning"
      />
      <Form :label-col="{ span: 5 }" :model="rotateForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="新密钥" required>
          <Input.Password v-model:value="rotateForm.secret" :maxlength="4096" placeholder="请输入新的 AppSecret 或 Token" />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.integration-connectors {
  display: flex;
  flex-direction: column;
   @apply gap-4;
}

.integration-connectors__metrics {
  display: grid;
   @apply gap-3;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.integration-connectors__metric-label {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.integration-connectors__metric-value {
  @apply mt-1.5;
  color: var(--ant-color-text);
  font-size: 24px;
  font-weight: 600;
  line-height: 1.2;
}

.integration-connectors__metric-value--danger {
  color: var(--ant-color-error);
}

.integration-connectors__search {
  width: 220px;
}

.integration-connectors__alert {
   @apply mb-3;
}

.integration-connectors__rotate-alert {
   @apply mb-4;
}

.integration-connectors__test-message {
  display: inline-block;
  max-width: 150px;
  overflow: hidden;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  line-height: 1.4;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 960px) {
  .integration-connectors__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .integration-connectors__metrics {
    grid-template-columns: 1fr;
  }

  .integration-connectors__search {
    width: 160px;
  }
}
</style>
