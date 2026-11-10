import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createOnlineUserApi } from '../src/api/core/online-user.ts';
import { createSystemMonitorApi } from '../src/api/core/system-monitor.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import {
  browserLabel,
  formatMonitorBytes,
  formatMonitorNumber,
  formatMonitoringDate,
  isOwnOnlineAccount,
  toMonitorRows,
  toOnlineUserRow,
} from '../src/features/platform/monitoring/monitoring.model.ts';

const online = (changes = {}) => ({
  browser: 'Mozilla Chrome/100 Safari/537 Edg/100',
  canForceLogout: true,
  currentTenantId: null,
  device: 'Mac',
  id: '00000000-0000-0000-0000-000000000001',
  ipAddress: 'private-client-address',
  lastAccessedAt: '2026-10-13T00:00:00',
  signedInAt: null,
  tenantName: null,
  userId: '00000000-0000-0000-0000-000000000002',
  userName: 'host.admin',
  ...changes,
});

test('projects online rows without raw client addresses or user agents and preserves UTC semantics', () => {
  const row = toOnlineUserRow(online());
  assert.equal(row.browser, 'Edge');
  assert.equal(row.device, 'macOS');
  assert.equal(toOnlineUserRow(online({ device: 'Web' })).device, '网页');
  assert.equal(toOnlineUserRow(online({ device: '__proto__' })).device, '未识别');
  assert.equal(row.tenant, '宿主');
  assert.ok(!JSON.stringify(row).includes('private-client-address'));
  assert.ok(!JSON.stringify(row).includes('Mozilla'));
  assert.equal(formatMonitoringDate('2026-10-13T00:00:00'), formatMonitoringDate('2026-10-13T00:00:00Z'));
  assert.equal(formatMonitoringDate('2026-10-13T08:00:00+08:00'), formatMonitoringDate('2026-10-13T00:00:00Z'));
  assert.equal(formatMonitoringDate(null), '暂无记录');
  assert.equal(formatMonitoringDate('invalid'), '暂无记录');
  assert.equal(browserLabel('unknown browser'), '未识别');
  const longName = online({ userName: 'a'.repeat(64) });
  assert.equal(toOnlineUserRow(longName).account, longName.userName);
  assert.ok(toOnlineUserRow(longName).name.endsWith('…'));
});

test('does not confuse a same-named tenant account with the current host account', () => {
  assert.equal(isOwnOnlineAccount(online(), 'HOST.ADMIN', false), true);
  assert.equal(isOwnOnlineAccount(online({ currentTenantId: 'tenant' }), 'host.admin', false), false);
  assert.equal(isOwnOnlineAccount(online({ currentTenantId: 'tenant' }), 'host.admin', true), true);
  assert.equal(isOwnOnlineAccount(online(), 'another.user', false), false);
});

test('keeps zero metrics distinct from unavailable values and displays binary byte units', () => {
  assert.equal(formatMonitorNumber(0, '%'), '0%');
  assert.equal(formatMonitorNumber(null, '%'), '未提供');
  assert.equal(formatMonitorNumber(NaN), '未提供');
  assert.equal(formatMonitorBytes(0), '0 B');
  assert.equal(formatMonitorBytes(1024), '1 KiB');
  assert.equal(formatMonitorBytes(1024 ** 3), '1 GiB');
  assert.equal(formatMonitorBytes(-1), '未提供');
});

test('uses backend metric percentages and preserves complete snapshot details', () => {
  const rows = toMonitorRows({
    systemCpuUsagePercent: null,
    processCpuUsagePercent: 0,
    processorCount: 8,
    cpuTimeSeconds: 1.5,
    systemMemoryUsagePercent: 37.5,
    processMemoryUsagePercent: 12.25,
    memoryUsageNote: '后端内存口径',
    totalMemoryBytes: 1024 ** 3,
    systemUsedMemoryBytes: 1,
    availableMemoryBytes: 1,
    processMemoryDisplayDenominatorBytes: 1024,
    workingSetBytes: 1024,
    managedMemoryBytes: 100,
    uptimeMinutes: 2,
    processStartTime: '2026-10-13T00:00:00Z',
    threadCount: 5,
    handleCount: null,
    environmentName: 'Development',
    frameworkDescription: '.NET',
    machineName: 'a'.repeat(64),
    osDescription: '系统',
    osArchitecture: 'Arm64',
    processArchitecture: 'Arm64',
    disks: [{ name: '/long/mount/path', driveFormat: 'APFS', freeSpaceBytes: 0, totalSizeBytes: 1024, type: 'Fixed' }],
  });
  const find = (id) => rows.find((row) => row.id === id);
  assert.equal(find('system-cpu').fullValue, '未提供');
  assert.equal(find('process-cpu').fullValue, '0%');
  assert.equal(find('system-memory-percent').fullValue, '37.5%');
  assert.equal(find('system-memory-percent').note, '后端内存口径');
  assert.equal(find('process-memory-percent').fullValue, '12.25%');
  assert.equal(find('machine').fullValue, 'a'.repeat(64));
  assert.ok(find('machine').value.endsWith('…'));
  assert.equal(find('disk-0-free').fullValue, '0 B');
  assert.equal(find('environment').fullValue, '开发');
});

test('requests real monitoring routes and revokes by userId rather than profile id', async () => {
  const calls = [];
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (url, init) => {
      calls.push({ url: String(url), method: init.method, body: init.body });
      return Response.json({ code: 0, data: init.method === 'POST' ? true : [], error: null, message: 'ok' });
    },
  });
  const api = createOnlineUserApi(client);
  await api.getList();
  await api.forceLogout(online().userId);
  await createSystemMonitorApi(client).getSnapshot();
  assert.deepEqual(
    calls.map((item) => item.url),
    ['/api/app/online-user/list', `/api/app/online-user/${online().userId}/force-logout`, '/api/app/system-monitor'],
  );
  assert.equal(calls[1].method, 'POST');
  assert.equal(calls[1].body, undefined);
  assert.throws(() => api.forceLogout('../other-route'), /有效账号/);
  assert.equal(calls.length, 3);
});

const tree = (
  component = '/platform/system-monitor/index',
  name = 'PlatformSystemMonitor',
  path = '/platform/system-monitor',
) => [
  {
    name: 'PlatformCenter',
    path: '/platform',
    component: 'BasicLayout',
    children: [{ name, path, component, meta: { title: '系统监控' } }],
  },
];

test('requires the exact monitor menu and permission and keeps removed online routes closed', () => {
  assert.equal(getResolvedPages(adaptBackendMenuTree(tree(), [])).length, 0);
  assert.equal(getResolvedPages(adaptBackendMenuTree(tree(), ['Platform.SystemMonitor.View'])).length, 1);
  assert.equal(getResolvedPages(adaptBackendMenuTree(tree('/other'), ['Platform.SystemMonitor.View'])).length, 0);
  assert.equal(
    getResolvedPages(
      adaptBackendMenuTree(tree('/platform/online-users/index', 'PlatformOnlineUsers', '/platform/online-users'), [
        'Platform.User.Manage',
      ]),
    ).length,
    0,
  );
});
