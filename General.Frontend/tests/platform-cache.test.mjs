import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createPlatformCacheApi } from '../src/api/core/platform-cache.ts';
import { hasConfirmedCacheRefresh, toCacheRow } from '../src/features/platform/cache/cache.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';

const cacheArea = (changes = {}) => ({
  area: 'authorization',
  description: '角色、租户和接口访问授权缓存',
  version: '1',
  ...changes,
});

test('maps the backend cache DTO directly and confirms only a version change for the same area', () => {
  const area = cacheArea();
  assert.deepEqual(toCacheRow(area), {
    area: area.area,
    description: area.description,
    id: area.area,
    version: area.version,
  });
  assert.equal(hasConfirmedCacheRefresh(area, [cacheArea({ version: '2' })]), true);
  assert.equal(hasConfirmedCacheRefresh(area, [cacheArea()]), false);
  assert.equal(hasConfirmedCacheRefresh(area, [cacheArea({ area: 'dict', version: '2' })]), false);
});

test('uses the exact cache list and per-area refresh contract', async () => {
  const calls = [];
  const api = createPlatformCacheApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ body: init.body, method: init.method, url: String(url) });
        return Response.json({ code: 0, data: init.method === 'POST' ? true : [cacheArea()] });
      },
    }),
  );
  await api.getAreas();
  await api.refresh('authorization');
  assert.deepEqual(calls, [
    { body: undefined, method: 'GET', url: '/api/app/platform/cache/areas' },
    {
      body: JSON.stringify({ area: 'authorization' }),
      method: 'POST',
      url: '/api/app/platform/cache/refresh',
    },
  ]);
});

const cacheTree = (component = '/platform/cache/index') => [
  {
    name: 'PlatformCenter',
    path: '/platform',
    component: 'BasicLayout',
    children: [{ name: 'PlatformCache', path: '/platform/cache', component, meta: { title: '缓存管理' } }],
  },
];

test('requires the backend cache menu contract and Platform.Cache.Manage permission', () => {
  assert.equal(getResolvedPages(adaptBackendMenuTree(cacheTree(), [])).length, 0);
  assert.equal(getResolvedPages(adaptBackendMenuTree(cacheTree(), ['Platform.Cache.Manage'])).length, 1);
  assert.equal(getResolvedPages(adaptBackendMenuTree(cacheTree('/other'), ['Platform.Cache.Manage'])).length, 0);
});
