import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createAuditLogApi } from '../src/api/core/audit-log.ts';
import {
  auditPath,
  buildAuditQuery,
  parseAuditTime,
  toAuditLogRow,
} from '../src/features/platform/audit-logs/audit-log.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { createAuthorizedPageVisit } from '../src/navigation/page-visit.ts';

test('maps local inclusive date boundaries and real backend paging fields', () => {
  const query = buildAuditQuery(
    { category: 'pagevisit', startDate: '2026-10-13', endDate: '2026-10-13' },
    '  smoke  ',
    3,
    20,
  );
  assert.equal(query.category, 'pagevisit');
  assert.equal(query.keyword, 'smoke');
  assert.equal(query.skipCount, 40);
  assert.equal(query.maxResultCount, 20);
  assert.equal(new Date(query.startTime).getHours(), 0);
  assert.equal(new Date(query.endTime).getHours(), 23);
  assert.equal(new Date(query.endTime).getMilliseconds(), 999);
  assert.throws(
    () => buildAuditQuery({ category: '', startDate: '2026-10-14', endDate: '2026-10-13' }, '', 1, 20),
    /开始日期/,
  );
  assert.throws(() => buildAuditQuery({ category: '', startDate: '2026-02-30', endDate: '' }, '', 1, 20), /有效日期/);
  assert.throws(() => buildAuditQuery({ category: 'unknown', startDate: '', endDate: '' }, '', 1, 20), /类别/);
});

test('uses paged and dashboard APIs and only sends page title and path for visits', async () => {
  const requests = [];
  const api = createAuditLogApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        requests.push({ url: String(url), method: init?.method ?? 'GET', body: init?.body });
        return Response.json({ code: 0, data: true });
      },
    }),
  );
  const query = buildAuditQuery({ category: 'operation', startDate: '', endDate: '' }, '操作', 2, 20);
  await api.getPaged(query);
  await api.getDashboard({ ...query, maxResultCount: 200, skipCount: 0 });
  await api.reportPageVisit({ menuPath: '/platform/users', menuTitle: '用户管理' });
  assert.equal(new URL(requests[0].url, 'https://test.invalid').searchParams.get('skipCount'), '20');
  assert.ok(requests[0].url.startsWith('/api/app/audit-log/paged?'));
  assert.ok(requests[1].url.startsWith('/api/app/audit-log/dashboard?'));
  assert.equal(requests[2].url, '/api/app/audit-log/page-visit');
  assert.equal(requests[2].method, 'POST');
  assert.deepEqual(JSON.parse(requests[2].body), { menuPath: '/platform/users', menuTitle: '用户管理' });
});

test('projects only safe audit details and never renders query secrets or exception text', () => {
  const row = toAuditLogRow({
    id: 'log',
    executionTime: 'invalid',
    executionDuration: 12,
    hasException: true,
    url: '/api/app/example?token=private#password',
    exceptionMessage: 'password=private',
    userName: 'tester',
    httpStatusCode: null,
    operationSessionId: 'private-session',
  });
  assert.equal(row.path, '/api/app/example');
  assert.equal(row.status, '未记录');
  assert.equal(row.time, '未记录');
  assert.ok(!JSON.stringify(row).includes('private'));
  assert.equal(auditPath('https://user:private@server.invalid/path?key=private'), '/path');
  assert.equal(auditPath('key=private'), '未记录');
});

test('reports only successful authenticated navigation to an authorized local page', () => {
  const input = {
    authenticated: true,
    failed: false,
    path: '/platform/users',
    previousPath: '/platform/workspace',
    page: { localPath: '/platform/users', label: '用户管理' },
  };
  assert.deepEqual(createAuthorizedPageVisit(input), { menuPath: '/platform/users', menuTitle: '用户管理' });
  for (const change of [
    { authenticated: false },
    { failed: true },
    { page: undefined },
    { previousPath: input.path },
    { path: `${input.path}?token=private` },
  ])
    assert.equal(createAuthorizedPageVisit({ ...input, ...change }), null);
});

const tree = (child) => [{ name: 'PlatformCenter', path: '/platform', component: 'BasicLayout', children: [child] }];

test('preserves UTC instants when SQLite returns audit timestamps without timezone suffixes', () => {
  assert.equal(parseAuditTime('2026-10-13T02:11:52.50822').toISOString(), '2026-10-13T02:11:52.508Z');
  assert.equal(parseAuditTime('2026-10-13T10:11:52.508+08:00').toISOString(), '2026-10-13T02:11:52.508Z');
  assert.equal(parseAuditTime('2026-10-13T02:11:52.508Z').toISOString(), '2026-10-13T02:11:52.508Z');
});

test('opens the audit seed menu only with the exact component and AuditLogView grant', () => {
  const node = {
    name: 'PlatformAuditLogs',
    path: '/platform/audit-logs',
    component: '/platform/audit-logs/index',
    meta: { title: '日志中心' },
  };
  assert.equal(getResolvedPages(adaptBackendMenuTree(tree(node), [])).length, 0);
  assert.equal(getResolvedPages(adaptBackendMenuTree(tree(node), ['Platform.AuditLog.View'])).length, 1);
  assert.equal(
    getResolvedPages(adaptBackendMenuTree(tree({ ...node, component: '/other' }), ['Platform.AuditLog.View'])).length,
    0,
  );
});
