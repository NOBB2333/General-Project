import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createPlatformOpenApiApi } from '../src/api/core/platform-open-api.ts';
import {
  createOpenApiApplicationDraft,
  toOpenApiApplicationRow,
  toOpenApiApplicationSaveInput,
  toggleOpenApiScope,
  validateOpenApiApplicationDraft,
} from '../src/features/platform/open-api/open-api.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';

const application = {
  appId: 'ga_demo_123',
  creationTime: '2026-10-01T00:00:00Z',
  id: 'application-1',
  isEnabled: true,
  lastModificationTime: null,
  name: '同步服务',
  remark: '',
  scopes: ['dict:read'],
};

test('maps OpenAPI application fields and masks secret availability as a generic one-time-only status', () => {
  assert.deepEqual(toOpenApiApplicationRow(application), {
    appId: 'ga_demo_123',
    createdAt: toOpenApiApplicationRow(application).createdAt,
    id: 'application-1',
    name: '同步服务',
    remark: '',
    scopes: 'dict:read',
    status: '启用',
  });
  assert.equal('appSecret' in toOpenApiApplicationRow(application), false);
});

test('limits authorization choices to a selected scope and requires a nonempty application name and scope', () => {
  const draft = createOpenApiApplicationDraft();
  draft.name = '同步服务';
  assert.equal(validateOpenApiApplicationDraft(draft), null);
  assert.deepEqual(toggleOpenApiScope(['dict:read'], '*', true), ['*']);
  assert.deepEqual(toggleOpenApiScope(['*'], 'dict:read', true), ['dict:read']);
  assert.deepEqual(toggleOpenApiScope(['dict:read'], 'dict:read', false), []);
  draft.scopes = [];
  assert.equal(validateOpenApiApplicationDraft(draft), '至少选择一个授权范围。');
  draft.scopes = ['dict:read'];
  draft.name = '  ';
  assert.equal(validateOpenApiApplicationDraft(draft), '请输入应用名称。');
});

test('trims the save payload and preserves existing unrecognized scopes until explicitly removed', () => {
  const draft = createOpenApiApplicationDraft();
  Object.assign(draft, {
    name: ' 同步服务 ',
    remark: '  订单同步  ',
    scopes: ['dict:read', 'legacy:read', 'dict:read'],
  });
  assert.deepEqual(toOpenApiApplicationSaveInput(draft), {
    isEnabled: true,
    name: '同步服务',
    remark: '订单同步',
    scopes: ['dict:read', 'legacy:read'],
  });
});

test('uses the exact protected application endpoints and request DTO fields', async () => {
  const calls = [];
  const api = createPlatformOpenApiApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        const call = { body: init.body, method: init.method, url: String(url) };
        calls.push(call);
        const data =
          call.method === 'GET'
            ? { items: [application], totalCount: 1 }
            : call.method === 'DELETE'
              ? true
              : { ...application, appSecret: 'gas_unit-test-only-secret' };
        return Response.json({ code: 0, data });
      },
    }),
  );
  const input = { isEnabled: false, name: ' 同步服务 ', remark: '', scopes: ['dict:read'] };
  await api.getApplications({ keyword: 'ga_demo_123', maxResultCount: 10, skipCount: 0 });
  const created = await api.createApplication(input);
  await api.updateApplication('id/with slash', input);
  await api.resetApplicationSecret('application-1');
  await api.deleteApplication('application-1');

  assert.equal(created.appSecret, 'gas_unit-test-only-secret');
  assert.deepEqual(calls, [
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/platform/open-api/applications/paged?keyword=ga_demo_123&maxResultCount=10&skipCount=0',
    },
    {
      body: JSON.stringify(input),
      method: 'POST',
      url: '/api/app/platform/open-api/applications',
    },
    {
      body: JSON.stringify(input),
      method: 'PUT',
      url: '/api/app/platform/open-api/applications/id%2Fwith%20slash',
    },
    {
      body: undefined,
      method: 'POST',
      url: '/api/app/platform/open-api/applications/application-1/reset-secret',
    },
    {
      body: undefined,
      method: 'DELETE',
      url: '/api/app/platform/open-api/applications/application-1',
    },
  ]);
});

test('resolves the OpenAPI page only for the backend menu path and manage permission', () => {
  const menu = [
    {
      name: 'PlatformCenter',
      path: '/platform',
      component: 'BasicLayout',
      children: [{ name: 'PlatformOpenApi', path: '/platform/open-api', component: '/platform/open-api/index' }],
    },
  ];
  assert.equal(getResolvedPages(adaptBackendMenuTree(menu, [])).length, 0);
  assert.equal(getResolvedPages(adaptBackendMenuTree(menu, ['Platform.OpenApi.Manage'])).length, 1);
  assert.equal(getResolvedPages(adaptBackendMenuTree(menu, ['Platform.Dict.Manage'])).length, 0);
  const wrongComponent = structuredClone(menu);
  wrongComponent[0].children[0].component = '/platform/open-api/unsafe';
  assert.equal(getResolvedPages(adaptBackendMenuTree(wrongComponent, ['Platform.OpenApi.Manage'])).length, 0);
});
