import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createIntegrationBindingsApi } from '../src/api/core/integration-bindings.ts';
import {
  bindingStatusFilterValue,
  bindingStatusLabel,
  parseExternalAccountImportJson,
  toGroupBindingRow,
} from '../src/features/integration/bindings/bindings.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('binding API calls use the current controller routes, pagination, filters, and DTO payloads', async () => {
  const calls = [];
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (url, init) => {
      calls.push({ body: init.body, method: init.method, url: String(url) });
      return Response.json({ code: 0, data: { items: [], totalCount: 0 } });
    },
  });
  const api = createIntegrationBindingsApi(client);
  const query = { connectorId: 'connector-1', keyword: 'sample', maxResultCount: 20, skipCount: 20, status: 'Pending' };

  await api.getConnectors();
  await api.getExternalAccounts(query);
  await api.getUserBindings(query);
  await api.getDepartmentBindings(query);
  await api.getGroupBindings(query);
  await api.getIdMappings(query);
  await api.saveExternalAccount({ connectorId: 'connector-1', externalUserId: 'external-1', status: 'Pending' });
  await api.importExternalAccounts({
    accounts: [{ connectorId: 'connector-1', externalUserId: 'external-2', status: 'Pending' }],
    connectorId: 'connector-1',
  });
  await api.bindUser({ externalAccountId: 'account-1', internalUserId: 'user-1', matchType: 'Manual' });
  await api.autoMatchUsers('connector-1');
  await api.resolveUserBinding('binding/1', { internalUserId: 'user-1', status: 'Bound' });
  await api.saveDepartmentBinding({
    connectorId: 'connector-1',
    externalDepartmentId: 'dept-1',
    internalOrganizationUnitId: 'organization-1',
    matchType: 'Manual',
    status: 'Bound',
  });
  await api.saveGroupBinding({
    connectorId: 'connector-1',
    externalGroupId: 'group-1',
    groupType: 'Chat',
    internalObjectId: 'project-1',
    internalObjectType: 'Project',
    status: 'Bound',
  });
  await api.saveIdMapping({
    connectorId: 'connector-1',
    externalObjectId: 'external-1',
    externalObjectType: 'ExternalUser',
    internalObjectId: 'user-1',
    internalObjectType: 'User',
    source: 'Manual',
    status: 'Bound',
  });

  assert.deepEqual(calls, [
    { body: undefined, method: 'GET', url: '/api/app/integration/bindings/connectors' },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/bindings/external-accounts?connectorId=connector-1&keyword=sample&maxResultCount=20&skipCount=20&status=Pending',
    },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/bindings/users?connectorId=connector-1&keyword=sample&maxResultCount=20&skipCount=20&status=Pending',
    },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/bindings/departments?connectorId=connector-1&keyword=sample&maxResultCount=20&skipCount=20&status=Pending',
    },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/bindings/groups?connectorId=connector-1&keyword=sample&maxResultCount=20&skipCount=20&status=Pending',
    },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/bindings/id-mappings?connectorId=connector-1&keyword=sample&maxResultCount=20&skipCount=20&status=Pending',
    },
    {
      body: JSON.stringify({ connectorId: 'connector-1', externalUserId: 'external-1', status: 'Pending' }),
      method: 'POST',
      url: '/api/app/integration/bindings/external-accounts',
    },
    {
      body: JSON.stringify({
        accounts: [{ connectorId: 'connector-1', externalUserId: 'external-2', status: 'Pending' }],
        connectorId: 'connector-1',
      }),
      method: 'POST',
      url: '/api/app/integration/bindings/external-accounts/import',
    },
    {
      body: JSON.stringify({ externalAccountId: 'account-1', internalUserId: 'user-1', matchType: 'Manual' }),
      method: 'POST',
      url: '/api/app/integration/bindings/users',
    },
    {
      body: JSON.stringify({ connectorId: 'connector-1' }),
      method: 'POST',
      url: '/api/app/integration/bindings/users/auto-match',
    },
    {
      body: JSON.stringify({ internalUserId: 'user-1', status: 'Bound' }),
      method: 'POST',
      url: '/api/app/integration/bindings/users/binding%2F1/resolve',
    },
    {
      body: JSON.stringify({
        connectorId: 'connector-1',
        externalDepartmentId: 'dept-1',
        internalOrganizationUnitId: 'organization-1',
        matchType: 'Manual',
        status: 'Bound',
      }),
      method: 'POST',
      url: '/api/app/integration/bindings/departments',
    },
    {
      body: JSON.stringify({
        connectorId: 'connector-1',
        externalGroupId: 'group-1',
        groupType: 'Chat',
        internalObjectId: 'project-1',
        internalObjectType: 'Project',
        status: 'Bound',
      }),
      method: 'POST',
      url: '/api/app/integration/bindings/groups',
    },
    {
      body: JSON.stringify({
        connectorId: 'connector-1',
        externalObjectId: 'external-1',
        externalObjectType: 'ExternalUser',
        internalObjectId: 'user-1',
        internalObjectType: 'User',
        source: 'Manual',
        status: 'Bound',
      }),
      method: 'POST',
      url: '/api/app/integration/bindings/id-mappings',
    },
  ]);
});

test('binding model maps backend statuses and keeps duplicate identifiers out of batch imports', () => {
  assert.equal(bindingStatusLabel('Conflict'), '待处理冲突');
  assert.equal(bindingStatusFilterValue('存在冲突'), 'Conflict');
  assert.equal(
    toGroupBindingRow(
      {
        connectorCode: 'feishu',
        connectorId: 'connector-1',
        externalGroupId: 'group-1',
        externalGroupName: '项目群',
        groupType: 'Chat',
        id: 'binding-1',
        internalObjectId: 'project-1',
        internalObjectName: '项目一',
        internalObjectType: 'Project',
        providerType: 'Feishu',
        remark: '',
        robotCode: '',
        status: 'Bound',
      },
      '飞书 · feishu',
    ).related,
    'Project · 项目一',
  );

  const parsed = parseExternalAccountImportJson(
    '[{"externalUserId":"user-1","displayName":"成员"},{"externalUserId":"user-1"}]',
    'connector-1',
  );
  assert.deepEqual(parsed.accounts, []);
  assert.match(parsed.error ?? '', /重复/);
});

test('batch import validation rejects unknown fields and malformed external account IDs', () => {
  assert.match(
    parseExternalAccountImportJson('[{"externalUserId":"u1","secret":"x"}]', 'c1').error ?? '',
    /不支持的字段/,
  );
  assert.match(parseExternalAccountImportJson('[{"displayName":"成员"}]', 'c1').error ?? '', /缺少外部用户ID/);
  assert.match(parseExternalAccountImportJson('{', 'c1').error ?? '', /合法JSON数组/);
  assert.equal(
    parseExternalAccountImportJson('[{"externalUserId":"u1","displayName":"成员"}]', 'c1').accounts[0]?.status,
    'Pending',
  );
});

test('binding and group backend menus resolve only with the binding view permission', () => {
  const menu = [
    {
      component: 'BasicLayout',
      name: 'IntegrationCenter',
      path: '/integration',
      children: [
        {
          component: '/integration/bindings/index',
          meta: { title: '账号绑定' },
          name: 'IntegrationBindings',
          path: '/integration/bindings',
        },
        {
          component: '/integration/groups/index',
          meta: { title: '群与机器人' },
          name: 'IntegrationGroups',
          path: '/integration/groups',
        },
      ],
    },
  ];

  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Integration.Binding.View'])).map((page) => page.routeName),
    [routeNames.integrationBindings, routeNames.integrationGroups],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, [])).map((page) => page.routeName),
    [],
  );
});
