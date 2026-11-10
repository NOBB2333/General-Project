import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createConfigApi } from '../src/api/core/config.ts';
import {
  buildConfigSaveInput,
  canEditConfig,
  expectedConfigValue,
  toConfigRow,
} from '../src/features/platform/configs/config.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';

const config = (changes = {}) => ({
  code: 'Admin.Platform.DefaultPageSize',
  defaultValue: '20',
  description: '说明',
  groupCode: 'ui',
  groupName: '界面',
  hasTenantValue: false,
  isReadonly: false,
  name: '默认分页数',
  providerKey: 'tenant-context',
  providerName: 'G',
  value: '20',
  valueType: 'number',
  ...changes,
});

test('selects global or current-tenant writes without forwarding an arbitrary provider key', () => {
  assert.deepEqual(buildConfigSaveInput(config(), ' 0030 ', false), {
    value: '30',
    providerName: 'G',
    providerKey: null,
  });
  assert.deepEqual(buildConfigSaveInput(config(), '30', true), { value: '30', providerName: 'T' });
  assert.equal(toConfigRow(config(), true).scope, '继承全局 / 默认');
  assert.equal(toConfigRow(config({ hasTenantValue: true }), true).scope, '当前租户');
});

test('validates int32 values and permits clearable strings while respecting DTO length', () => {
  for (const value of ['-1', '1.5', '1e3', '', 'NaN', '2147483648'])
    assert.throws(() => buildConfigSaveInput(config(), value, false), /整数/);
  assert.equal(buildConfigSaveInput(config(), '2147483647', false).value, '2147483647');
  assert.equal(buildConfigSaveInput(config(), '0', false).value, '0');
  const whitelist = config({
    code: 'Admin.Platform.IntegrationWebhookIpWhitelist',
    valueType: 'string',
    defaultValue: '',
  });
  assert.equal(buildConfigSaveInput(whitelist, '', true).value, '');
  assert.equal(buildConfigSaveInput(whitelist, 'a'.repeat(512), true).value.length, 512);
  assert.throws(() => buildConfigSaveInput(whitelist, 'a'.repeat(513), true), /512/);
  const title = config({ code: 'Admin.Platform.SystemName', valueType: 'string', defaultValue: 'General Admin' });
  assert.equal(expectedConfigValue(title, buildConfigSaveInput(title, ' ', false)), 'General Admin');
});

test('does not expose unknown or mismatched configuration values and respects readonly items', () => {
  const unknown = config({
    code: 'Admin.Platform.FutureSecret',
    value: 'private-value',
    defaultValue: 'private-default',
  });
  assert.equal(canEditConfig(unknown), false);
  assert.ok(!JSON.stringify(toConfigRow(unknown, false)).includes('private'));
  assert.throws(() => buildConfigSaveInput(unknown, 'new-value', false), /安全编辑/);
  assert.equal(canEditConfig(config({ isReadonly: true })), false);
  assert.equal(canEditConfig(config({ valueType: 'password' })), false);
  assert.equal(canEditConfig(config({ code: '__proto__' })), false);
});

test('previews long table values while preserving the complete editable configuration', () => {
  const item = config({ code: 'Admin.Platform.SystemName', valueType: 'string', value: '企业管理'.repeat(20) });
  assert.equal(toConfigRow(item, false).value, `${Array.from(item.value).slice(0, 14).join('')}…`);
  assert.equal(buildConfigSaveInput(item, item.value, false).value, item.value);
  assert.equal(item.value.length, 80);
});

test('maps configuration list and authenticated PUT routes without invented paging parameters', async () => {
  const calls = [];
  const api = createConfigApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ url: String(url), method: init?.method ?? 'GET', body: init?.body });
        return Response.json({ code: 0, data: init?.method === 'PUT' ? true : [] });
      },
    }),
  );
  await api.getList();
  await api.update('Admin.Platform.DefaultPageSize', { value: '30', providerName: 'T' });
  assert.equal(calls[0].url, '/api/app/platform/config/list');
  assert.equal(calls[1].url, '/api/app/platform/config/Admin.Platform.DefaultPageSize');
  assert.equal(calls[1].method, 'PUT');
  assert.deepEqual(JSON.parse(calls[1].body), { value: '30', providerName: 'T' });
});

const configTree = (component = '/platform/configs/index') => [
  {
    name: 'PlatformCenter',
    path: '/platform',
    component: 'BasicLayout',
    children: [{ name: 'PlatformConfigs', path: '/platform/configs', component, meta: { title: '配置参数' } }],
  },
];

test('requires the exact configuration menu registration and ConfigManage grant', () => {
  assert.equal(getResolvedPages(adaptBackendMenuTree(configTree(), [])).length, 0);
  assert.equal(getResolvedPages(adaptBackendMenuTree(configTree(), ['Platform.Config.Manage'])).length, 1);
  assert.equal(getResolvedPages(adaptBackendMenuTree(configTree('/other'), ['Platform.Config.Manage'])).length, 0);
});
