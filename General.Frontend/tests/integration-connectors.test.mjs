import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createIntegrationConnectorsApi } from '../src/api/core/integration-connectors.ts';
import {
  createConnectorDraft,
  toConnectorDraft,
  toConnectorRow,
  toConnectorSaveInput,
  validateConnectorDraft,
} from '../src/features/integration/connectors/connectors.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

const connector = {
  callbackPath: '/api/app/integration/webhook/feishu',
  capabilities: ['SendPersonalMessage', 'SyncDepartment'],
  configJson: '{"appId":"test-app"}',
  configSchemaJson: JSON.stringify({
    fields: [
      { key: 'appId', secret: false },
      { key: 'appSecret', secret: true },
    ],
  }),
  connectorCode: 'feishu',
  displayName: '飞书 / Lark',
  hasRobotWebhook: false,
  hasSecret: true,
  icon: 'lucide:message-square',
  id: 'connector-1',
  isBuiltin: true,
  isConfigured: true,
  isEnabled: true,
  isTenantEnabled: true,
  lastTestMessage: '连接测试成功。',
  lastTestStatus: '成功',
  lastTestTime: '2026-10-13T09:00:00Z',
  providerType: 'Feishu',
  remark: '用于集成测试',
  sort: 10,
  tenantConfigId: 'tenant-config-1',
};

test('uses connector list, health, config, toggle, rotate, and connection-test endpoints', async () => {
  const calls = [];
  const api = createIntegrationConnectorsApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        const call = { body: init.body, method: init.method, url: String(url) };
        calls.push(call);
        return Response.json({
          code: 0,
          data:
            call.method === 'GET' && call.url.endsWith('/health')
              ? {
                  disabledCount: 0,
                  healthyCount: 1,
                  items: [],
                  totalCount: 1,
                  unconfiguredCount: 0,
                  unhealthyCount: 0,
                  warningCount: 0,
                }
              : call.method === 'GET'
                ? [connector]
                : call.url.endsWith('/test')
                  ? { message: '连接测试成功。', success: true }
                  : connector,
        });
      },
    }),
  );
  const saveInput = {
    callbackPath: connector.callbackPath,
    configJson: connector.configJson,
    isEnabled: true,
    remark: connector.remark,
    secret: '',
  };

  await api.getConnectors({ includeDisabled: false, keyword: '飞书' });
  await api.getHealth();
  await api.saveTenantConfig('connector/1', saveInput);
  await api.toggle('connector-1', { isEnabled: false });
  await api.rotateSecret('connector-1', { secret: 'temporary-test-secret' });
  await api.testConnection('connector-1');

  assert.deepEqual(calls, [
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/connectors?includeDisabled=false&keyword=%E9%A3%9E%E4%B9%A6',
    },
    { body: undefined, method: 'GET', url: '/api/app/integration/connectors/health' },
    {
      body: JSON.stringify(saveInput),
      method: 'PUT',
      url: '/api/app/integration/connectors/connector%2F1/tenant-config',
    },
    {
      body: JSON.stringify({ isEnabled: false }),
      method: 'POST',
      url: '/api/app/integration/connectors/connector-1/toggle',
    },
    {
      body: JSON.stringify({ secret: 'temporary-test-secret' }),
      method: 'POST',
      url: '/api/app/integration/connectors/connector-1/rotate-secret',
    },
    { body: undefined, method: 'POST', url: '/api/app/integration/connectors/connector-1/test' },
  ]);
});

test('connector drafts never prefill secret fields and preserve callback paths', () => {
  const draft = toConnectorDraft(connector);
  assert.equal(draft.callbackPath, connector.callbackPath);
  assert.equal(draft.secret, '');
  assert.equal(draft.robotWebhook, '');

  const row = toConnectorRow(connector);
  assert.equal(row.configuration, '已配置 / 密钥已录入');
  assert.equal('secret' in row, false);
  assert.equal('configJson' in row, false);
  assert.equal('robotWebhook' in row, false);
});

test('configuration validation keeps schema secrets and robot webhook out of ordinary JSON', () => {
  const draft = createConnectorDraft();
  draft.configJson = JSON.stringify({ appId: 'test-app' });
  assert.equal(validateConnectorDraft(draft, connector), null);

  draft.configJson = JSON.stringify({ appId: 'test-app', appSecret: 'must-not-be-plain' });
  assert.match(validateConnectorDraft(draft, connector) ?? '', /密钥字段/);

  draft.configJson = JSON.stringify({ robotWebhook: 'https://example.test?key=secret' });
  assert.match(validateConnectorDraft(draft, connector) ?? '', /单独的凭证/);

  draft.configJson = '{';
  assert.match(validateConnectorDraft(draft, connector) ?? '', /合法JSON/);
});

test('connector save distinguishes unchanged webhook credentials from explicit clearing', () => {
  const draft = createConnectorDraft();
  draft.configJson = '{"appId":"test-app"}';
  draft.callbackPath = '/callback';
  draft.secret = '';
  assert.equal('robotWebhook' in toConnectorSaveInput(draft), false);

  draft.robotWebhook = 'https://example.test/webhook?key=test-only';
  assert.equal(toConnectorSaveInput(draft).robotWebhook, draft.robotWebhook);
  draft.robotWebhook = '';
  draft.clearRobotWebhook = true;
  assert.equal(toConnectorSaveInput(draft).robotWebhook, '');
});

test('only the registered connector page is exposed under the matching backend menu and permission', () => {
  const menu = [
    {
      component: 'BasicLayout',
      name: 'IntegrationCenter',
      path: '/integration',
      children: [
        {
          component: '/integration/connectors/index',
          meta: { title: '连接器配置' },
          name: 'IntegrationConnectors',
          path: '/integration/connectors',
        },
      ],
    },
  ];

  const authorized = getResolvedPages(adaptBackendMenuTree(menu, ['Integration.Connector.View']));
  assert.deepEqual(
    authorized.map((page) => page.routeName),
    [routeNames.integrationConnectors],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, [])).map((page) => page.routeName),
    [],
  );
});

test('default menu query includes AI, Project, Integration, and Business pages alongside Platform pages', async () => {
  const menuApi = await readFile(new URL('../src/api/core/menu.ts', import.meta.url), 'utf8');
  assert.match(menuApi, /appCodes: readonly string\[\] = \['platform', 'integration', 'project', 'business', 'ai'\]/);
  assert.match(menuApi, /query: \{ appCodes: appCodes\.join\(','\) \}/);
});
