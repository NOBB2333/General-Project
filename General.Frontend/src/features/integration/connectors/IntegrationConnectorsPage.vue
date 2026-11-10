<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { PlugZap } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiButton, UiCheckbox, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';
import { createIntegrationConnectorsApi, type IntegrationConnectorsApi } from '@/api/core/integration-connectors';
import {
  createConnectorDraft,
  getConnectorSchemaSummary,
  toConnectorDraft,
  toConnectorRow,
  toConnectorSaveInput,
  validateConnectorDraft,
  type ConnectorDraft,
  type ConnectorRow,
} from './connectors.model';

const api = createIntegrationConnectorsApi(apiClient);
const pageSize = 20;
const connectors = ref<IntegrationConnectorsApi.Connector[]>([]);
const health = ref<null | IntegrationConnectorsApi.ConnectorHealthSummary>(null);
const keyword = ref('');
const includeDisabled = ref(true);
const listSurfaceKey = ref(0);
const loading = ref(false);
const saving = ref(false);
const rotating = ref(false);
const toggling = ref(false);
const testing = ref(false);
const loadError = ref('');
const healthError = ref('');
const actionError = ref('');
const configError = ref('');
const editingConnector = ref<IntegrationConnectorsApi.Connector | null>(null);
const configDialogOpen = ref(false);
const configDraft = reactive<ConnectorDraft>(createConnectorDraft());
const rotateTarget = ref<IntegrationConnectorsApi.Connector | null>(null);
const rotateSecret = ref('');
const rotateError = ref('');
const rotateDialogOpen = ref(false);
const toggleTarget = ref<null | { connector: IntegrationConnectorsApi.Connector; enabled: boolean }>(null);
const toggleDialogOpen = ref(false);
const testTarget = ref<IntegrationConnectorsApi.Connector | null>(null);
const testDialogOpen = ref(false);
const activeRequest = ref<AbortController | null>(null);
const searchTimer = ref<number | undefined>();
const canManage = computed(() => canAccess('Integration.Connector.Manage'));
const canRotate = computed(() => canAccess('Integration.Connector.RotateSecret'));
const canTest = computed(() => canAccess('Integration.Connector.Test'));
const rows = computed<ConnectorRow[]>(() =>
  connectors.value.map((connector) =>
    toConnectorRow(
      connector,
      health.value?.items.find((item) => item.id === connector.id),
    ),
  ),
);

const pageModel = computed<ListPageModel<ConnectorRow>>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'configure', kind: 'custom', label: '配置所选', requiresSelection: true },
      { id: 'toggle', kind: 'custom', label: '启用 / 停用', requiresSelection: true },
    );
  }
  if (canTest.value) actions.push({ id: 'test', kind: 'custom', label: '测试连接', requiresSelection: true });
  if (canRotate.value) actions.push({ id: 'rotate', kind: 'custom', label: '轮换密钥', requiresSelection: true });

  return {
    actions,
    columns: [
      { key: 'name', title: '连接器', width: 190, sticky: true },
      { key: 'provider', title: '平台类型', width: 130 },
      { key: 'capabilities', title: '能力', width: 300 },
      { key: 'configuration', title: '配置与凭证', width: 220 },
      { key: 'status', title: '租户状态', width: 130, presentation: 'status' },
      { key: 'health', title: '集成状态', width: 110, presentation: 'status' },
      { key: 'lastTest', title: '最近测试', width: 190 },
      { key: 'remark', title: '说明', width: 300 },
    ],
    demoPath: '/integration/connectors',
    description: '管理飞书、钉钉、企业微信及集成沙箱的连接配置和凭证状态。',
    emptyHint: loading.value
      ? '正在读取连接器…'
      : loadError.value
        ? '连接器读取失败，请刷新重试。'
        : '暂无可管理的连接器。',
    filters: [{ key: 'enabled', label: '连接器范围', options: ['全部', '仅启用'] }],
    key: '外部集成|连接器配置',
    label: '连接器配置',
    listScopes: [{ id: 'all', label: '连接器', count: health.value?.totalCount ?? connectors.value.length }],
    listTabs: [{ id: 'all', label: '全部连接器' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: '外部集成',
    moduleSlug: 'integration',
    pageSlug: 'integration-connectors',
    pageSize,
    panels: [],
    path: '/integration/connectors',
    primaryAction: '配置所选',
    replication: { status: 'implemented', referenceUrl: '/integration/connectors' },
    routeName: 'integration-connectors',
    rows: rows.value,
    searchPlaceholder: '搜索连接器名称、编码或平台类型',
    stats: [
      {
        key: 'total',
        label: '连接器',
        value: String(health.value?.totalCount ?? connectors.value.length),
        tone: 'primary',
      },
      { key: 'healthy', label: '健康', value: String(health.value?.healthyCount ?? 0), tone: 'success' },
      {
        key: 'attention',
        label: '需处理',
        value: String((health.value?.unhealthyCount ?? 0) + (health.value?.unconfiguredCount ?? 0)),
        tone: 'warning',
      },
      { key: 'disabled', label: '停用', value: String(health.value?.disabledCount ?? 0), tone: 'neutral' },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
});

const remoteFilterValues = computed(() => ({ enabled: includeDisabled.value ? '全部' : '仅启用' }));
const remotePagination = computed(() => ({
  currentPage: 1,
  pageSize,
  searchQuery: keyword.value,
  totalCount: rows.value.length,
}));
const rotateDescription = computed(() => {
  const connector = rotateTarget.value;
  return connector
    ? `轮换“${connector.displayName}”的密钥会立即清除缓存Token并使旧凭证停止生效。依赖该凭证的同步和通知可能中断，提交后需要重新测试连接。`
    : '';
});
const toggleDescription = computed(() => {
  const target = toggleTarget.value;
  if (!target) return '';
  const action = target.enabled ? '启用' : '停用';
  const note = target.enabled ? '启用后允许该租户配置参与集成调用。' : '停用后该租户配置不再参与集成调用。';
  return `确认${action}“${target.connector.displayName}”？${note}`;
});

function safeConnectorResponse(connector: IntegrationConnectorsApi.Connector): boolean {
  if ('secret' in connector || 'secretCipherText' in connector || 'robotWebhook' in connector) return false;
  try {
    const config = JSON.parse(connector.configJson || '{}') as Record<string, unknown>;
    if (Object.keys(config).some((key) => key.toLowerCase() === 'robotwebhook')) return false;
    const schema = JSON.parse(connector.configSchemaJson || '{}') as {
      fields?: Array<{ Key?: unknown; Secret?: unknown; key?: unknown; secret?: unknown }>;
    };
    const secretKeys = (schema.fields ?? []).flatMap((field) => {
      const key = typeof field.Key === 'string' ? field.Key : typeof field.key === 'string' ? field.key : '';
      return key && (field.Secret === true || field.secret === true) ? [key] : [];
    });
    return !secretKeys.some((key) =>
      Object.keys(config).some((candidate) => candidate.toLowerCase() === key.toLowerCase()),
    );
  } catch {
    return false;
  }
}

async function loadConnectors(): Promise<boolean> {
  activeRequest.value?.abort();
  const controller = new AbortController();
  activeRequest.value = controller;
  loading.value = true;
  loadError.value = '';
  healthError.value = '';
  try {
    const result = await api.getConnectors(
      { includeDisabled: includeDisabled.value, keyword: keyword.value.trim() || undefined },
      controller.signal,
    );
    if (controller.signal.aborted) return false;
    if (!result.every(safeConnectorResponse)) {
      connectors.value = [];
      loadError.value = '后端返回了未脱敏的连接器凭证字段，已停止展示配置数据；请先更新后端接口。';
      return false;
    }
    connectors.value = result;
  } catch (error) {
    if (controller.signal.aborted || (error instanceof Error && error.name === 'AbortError')) return false;
    loadError.value = error instanceof Error ? error.message : '连接器读取失败，请重试。';
    return false;
  } finally {
    if (activeRequest.value === controller) loading.value = false;
  }

  try {
    health.value = await api.getHealth(controller.signal);
  } catch (error) {
    if (controller.signal.aborted || (error instanceof Error && error.name === 'AbortError')) return false;
    healthError.value = error instanceof Error ? error.message : '集成状态读取失败。';
    health.value = null;
  }
  listSurfaceKey.value += 1;
  return true;
}

function searchConnectors(value: string) {
  keyword.value = value;
  if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
  searchTimer.value = window.setTimeout(() => void loadConnectors(), 250);
}

function changeFilter(key: string, value: string) {
  if (key !== 'enabled') return;
  includeDisabled.value = value !== '仅启用';
  void loadConnectors();
}

function selectedConnector(selectedRows: ListPageRow[]): IntegrationConnectorsApi.Connector | null {
  if (selectedRows.length !== 1) {
    toast.warning('每次请选择一个连接器。');
    return null;
  }
  return connectors.value.find((connector) => connector.id === selectedRows[0]?.id) ?? null;
}

function openConfig(connector: IntegrationConnectorsApi.Connector) {
  if (!canManage.value) return;
  editingConnector.value = connector;
  Object.assign(configDraft, toConnectorDraft(connector));
  configError.value = '';
  configDialogOpen.value = true;
}

function handleAction(action: PageAction, selectedRows: ListPageRow[]) {
  if (action.id === 'reload') {
    void loadConnectors();
    return;
  }
  const connector = selectedConnector(selectedRows);
  if (!connector) return;
  if (action.id === 'configure') openConfig(connector);
  if (action.id === 'toggle' && canManage.value) {
    toggleTarget.value = { connector, enabled: !connector.isTenantEnabled };
    toggleDialogOpen.value = true;
  }
  if (action.id === 'test' && canTest.value) {
    testTarget.value = connector;
    testDialogOpen.value = true;
  }
  if (action.id === 'rotate' && canRotate.value) {
    if (!connector.isConfigured) {
      toast.warning('请先保存该连接器配置，再轮换密钥。');
      return;
    }
    rotateTarget.value = connector;
    rotateSecret.value = '';
    rotateError.value = '';
    rotateDialogOpen.value = true;
  }
}

async function saveConfig() {
  const connector = editingConnector.value;
  if (!connector || !canManage.value || saving.value) return;
  const validation = validateConnectorDraft(configDraft, connector);
  if (validation) {
    configError.value = validation;
    return;
  }

  saving.value = true;
  configError.value = '';
  try {
    const saved = await api.saveTenantConfig(connector.id, toConnectorSaveInput(configDraft));
    if (saved.id !== connector.id || !safeConnectorResponse(saved)) {
      throw new Error('服务返回的连接器配置不符合安全契约，已停止展示。');
    }
    configDialogOpen.value = false;
    configDraft.secret = '';
    configDraft.robotWebhook = '';
    if (!(await loadConnectors())) throw new Error('配置已提交，但连接器列表读回失败，请刷新核对。');
    const persisted = connectors.value.find((item) => item.id === connector.id);
    if (
      !persisted ||
      persisted.callbackPath !== configDraft.callbackPath.trim() ||
      persisted.isTenantEnabled !== configDraft.isEnabled
    ) {
      throw new Error('配置已提交，但回读字段与本次输入不一致，请刷新核对。');
    }
    actionError.value = '';
    toast.success('连接器配置已保存并读回确认。');
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '连接器配置保存失败，请重试。';
    toast.error(actionError.value);
  } finally {
    saving.value = false;
  }
}

async function confirmToggle() {
  const target = toggleTarget.value;
  if (!target || !canManage.value || toggling.value) return;
  toggling.value = true;
  actionError.value = '';
  try {
    const result = await api.toggle(target.connector.id, { isEnabled: target.enabled });
    if (
      result.id !== target.connector.id ||
      result.isTenantEnabled !== target.enabled ||
      !safeConnectorResponse(result)
    ) {
      throw new Error('服务返回的连接器状态与本次操作不一致。');
    }
    toggleDialogOpen.value = false;
    toggleTarget.value = null;
    if (!(await loadConnectors())) throw new Error('状态已提交，但连接器列表读回失败，请刷新核对。');
    toast.success(target.enabled ? '连接器已启用并读回确认。' : '连接器已停用并读回确认。');
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '连接器状态更新失败，请重试。';
    toast.error(actionError.value);
  } finally {
    toggling.value = false;
  }
}

async function confirmTest() {
  const connector = testTarget.value;
  if (!connector || !canTest.value || testing.value) return;
  testing.value = true;
  actionError.value = '';
  try {
    const result = await api.testConnection(connector.id);
    testDialogOpen.value = false;
    if (result.success) toast.success(result.message || '连接测试成功。');
    else toast.warning(result.message || '连接测试失败。');
    if (!(await loadConnectors())) throw new Error('测试结果已提交，但连接器状态读回失败，请刷新核对。');
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '连接测试请求失败，请重试。';
    toast.error(actionError.value);
  } finally {
    testing.value = false;
  }
}

async function confirmRotate() {
  const connector = rotateTarget.value;
  if (!connector || !canRotate.value || rotating.value) return;
  if (!rotateSecret.value.trim()) {
    rotateError.value = '请输入新密钥。';
    return;
  }
  if (rotateSecret.value.length > 4096) {
    rotateError.value = '新密钥不能超过4096个字符。';
    return;
  }

  rotating.value = true;
  rotateError.value = '';
  actionError.value = '';
  try {
    const result = await api.rotateSecret(connector.id, { secret: rotateSecret.value.trim() });
    if (result.id !== connector.id || !result.hasSecret || !safeConnectorResponse(result)) {
      throw new Error('密钥轮换已提交，但服务返回结果与预期不一致，请联系管理员核对。');
    }
    rotateDialogOpen.value = false;
    rotateSecret.value = '';
    if (!(await loadConnectors())) throw new Error('密钥已轮换，但列表读回失败，请刷新核对。');
    toast.success('连接器密钥已轮换。请重新测试连接。');
  } catch (error) {
    rotateError.value = error instanceof Error ? error.message : '连接器密钥轮换失败，请重试。';
    toast.error(rotateError.value);
  } finally {
    rotating.value = false;
  }
}

function updateConfigDialog(open: boolean) {
  if (saving.value) return;
  configDialogOpen.value = open;
  if (!open) {
    configDraft.secret = '';
    configDraft.robotWebhook = '';
    configDraft.clearRobotWebhook = false;
    configError.value = '';
    editingConnector.value = null;
  }
}

function updateRotateDialog(open: boolean) {
  if (rotating.value) return;
  rotateDialogOpen.value = open;
  if (!open) {
    rotateSecret.value = '';
    rotateError.value = '';
    rotateTarget.value = null;
  }
}

function updateTestDialog(open: boolean) {
  if (testing.value) return;
  testDialogOpen.value = open;
  if (!open) testTarget.value = null;
}

function updateToggleDialog(open: boolean) {
  if (toggling.value) return;
  toggleDialogOpen.value = open;
  if (!open) toggleTarget.value = null;
}

function openRow(row: ListPageRow) {
  const connector = connectors.value.find((item) => item.id === row.id);
  if (connector && canManage.value) openConfig(connector);
}

onMounted(() => void loadConnectors());
onBeforeUnmount(() => {
  activeRequest.value?.abort();
  if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
  configDraft.secret = '';
  configDraft.robotWebhook = '';
  rotateSecret.value = '';
});
</script>

<template>
  <ListPageSurface
    :key="listSurfaceKey"
    :model="pageModel"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @filter-change="changeFilter"
    @row="openRow"
    @search-change="searchConnectors"
  >
    <template #hero-icon>
      <PlugZap
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <p>密钥不会出现在列表和配置回显中；配置保存时密钥留空会沿用原值。</p>
      <p>连接测试会用已保存凭证请求外部平台的Token接口，不会发送消息。集成沙箱的测试只在本地返回结果。</p>
      <p>本页不提供测试消息发送；真实消息投递需要在后续投递流程中明确目标并查看投递记录。</p>
      <p
        v-if="healthError"
        role="alert"
      >
        {{ healthError }}
      </p>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
      <p
        v-if="actionError"
        role="alert"
      >
        {{ actionError }}
      </p>
      <p
        v-if="!canManage"
        role="alert"
      >
        当前账号没有连接器配置管理权限。
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="configDialogOpen"
    :title="editingConnector ? `配置 ${editingConnector.displayName}` : '配置连接器'"
    description="配置内容按当前后端DTO保存。密钥单独加密，不回显。"
    size="lg"
    @update:open="updateConfigDialog"
  >
    <form @submit.prevent="saveConfig">
      <UiFormField label="租户配置状态">
        <div>
          <UiSwitch
            v-model="configDraft.isEnabled"
            label="启用此租户连接配置"
          />
          <span>{{ configDraft.isEnabled ? '启用' : '停用' }}</span>
        </div>
      </UiFormField>
      <UiFormField
        label="回调路径"
        hint="最多512个字符；保存时会保留已读取的路径。"
      >
        <UiInput
          v-model="configDraft.callbackPath"
          maxlength="512"
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField
        label="连接密钥"
        hint="留空沿用已保存密钥；如需录入多个密钥，可按配置说明填JSON对象。最多4096个字符。"
      >
        <UiInput
          v-model="configDraft.secret"
          type="password"
          autocomplete="new-password"
          :placeholder="editingConnector?.hasSecret ? '已配置，留空保持现值' : '请输入连接器密钥'"
          maxlength="4096"
        />
      </UiFormField>
      <UiFormField
        v-if="editingConnector && ['dingtalk', 'wecom'].includes(editingConnector.connectorCode.toLowerCase())"
        label="群机器人 Webhook 或 Key"
        hint="该凭证只单独写入，不会出现在普通配置JSON或列表中。留空保持现值。"
      >
        <UiInput
          v-model="configDraft.robotWebhook"
          type="password"
          autocomplete="new-password"
          :placeholder="editingConnector.hasRobotWebhook ? '已配置，留空保持现值' : '可选：输入机器人Webhook或Key'"
          maxlength="4096"
        />
        <label v-if="editingConnector.hasRobotWebhook">
          <UiCheckbox
            v-model="configDraft.clearRobotWebhook"
            label="清除机器人Webhook"
          />
          <span>清除已保存的机器人凭证</span>
        </label>
      </UiFormField>
      <UiFormField
        label="连接配置JSON"
        :hint="editingConnector ? getConnectorSchemaSummary(editingConnector.configSchemaJson) : ''"
        wide
      >
        <UiTextarea
          v-model="configDraft.configJson"
          rows="10"
          maxlength="16000"
          spellcheck="false"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        hint="最多512个字符。"
      >
        <UiTextarea
          v-model="configDraft.remark"
          rows="3"
          maxlength="512"
        />
      </UiFormField>
      <p
        v-if="configError"
        role="alert"
      >
        {{ configError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="updateConfigDialog(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="saveConfig"
        >{{ saving ? '保存中…' : '保存配置' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="toggleDialogOpen"
    :title="toggleTarget?.enabled ? '启用连接器' : '停用连接器'"
    :description="toggleDescription"
    @update:open="updateToggleDialog"
  >
    <p
      v-if="!toggleTarget?.connector.isConfigured && toggleTarget?.enabled"
      role="alert"
    >
      该连接器尚未配置凭证；启用后状态仍会显示为未配置，外部调用不会因此成功。
    </p>
    <p
      v-if="actionError"
      role="alert"
    >
      {{ actionError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="toggling"
        @click="updateToggleDialog(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="toggling"
        @click="confirmToggle"
        >{{ toggling ? '提交中…' : '确认' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="testDialogOpen"
    title="测试连接"
    :description="testTarget ? `将使用“${testTarget.displayName}”已保存的配置调用连接测试。` : ''"
    @update:open="updateTestDialog"
  >
    <p>飞书、钉钉和企业微信会请求各自平台的Token接口；沙箱连接器只返回本地测试结果。此操作不会投递消息。</p>
    <p
      v-if="actionError"
      role="alert"
    >
      {{ actionError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="testing"
        @click="updateTestDialog(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="testing"
        @click="confirmTest"
        >{{ testing ? '测试中…' : '执行连接测试' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="rotateDialogOpen"
    title="轮换连接器密钥"
    :description="rotateDescription"
    @update:open="updateRotateDialog"
  >
    <div>
      <p>提交后旧密钥立即失效，已缓存Token会被清除。新密钥不会再从列表中展示；请准备好调用方更新，再执行轮换。</p>
      <UiFormField
        label="新密钥"
        required
        hint="最多4096个字符；如需多项密钥，填写后端支持的JSON对象。"
      >
        <UiInput
          v-model="rotateSecret"
          type="password"
          autocomplete="new-password"
          maxlength="4096"
          required
        />
      </UiFormField>
      <p
        v-if="rotateError"
        role="alert"
      >
        {{ rotateError }}
      </p>
    </div>
    <template #footer>
      <UiButton
        :disabled="rotating"
        @click="updateRotateDialog(false)"
        >取消</UiButton
      >
      <UiButton
        variant="danger"
        :disabled="rotating"
        @click="confirmRotate"
        >{{ rotating ? '轮换中…' : '立即轮换' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
