import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createPlatformSchedulerApi } from '../src/api/core/platform-scheduler.ts';
import {
  classifySchedulerRunMessage,
  schedulerRecordStatusLabel,
  toSchedulerJobRow,
} from '../src/features/platform/scheduler/scheduler.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

function responseFor(url) {
  const pathname = new URL(url, 'http://localhost').pathname;
  if (pathname.endsWith('/dashboard')) {
    return { enabledCount: 1, failedLast24Hours: 2, runningCount: 1, slowLast24Hours: 3, totalCount: 4 };
  }
  if (pathname.endsWith('/records')) return { items: [], totalCount: 0 };
  if (pathname.endsWith('/batch/run')) return [{ jobKey: 'tenant health', message: 'ok', success: true }];
  if (pathname.endsWith('/list') || pathname.endsWith('/handlers') || pathname.endsWith('/cluster-nodes')) return [];
  if (pathname.endsWith('/triggers')) return [];
  if (pathname.endsWith('/toggle') || pathname.endsWith('/run') || pathname.endsWith('/cancel')) return 'ok';
  return true;
}

test('uses the scheduler controller routes, HTTP methods, payloads, query names, and encoded route keys', async () => {
  const calls = [];
  const api = createPlatformSchedulerApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ body: init.body, method: init.method, url: String(url) });
        return Response.json({ code: 0, data: responseFor(url) });
      },
    }),
  );
  const job = {
    cronExpression: '0 0 2 * * ?',
    description: '隔离验证任务',
    handlerKey: 'tenant-health-check',
    isEnabled: false,
    jobKey: 'tenant health',
    title: '租户健康检查',
  };
  const trigger = {
    cronExpression: '0 0 3 * * ?',
    description: '隔离触发器',
    isEnabled: false,
    title: '隔离触发器',
    triggerKey: 'manual/check',
  };

  await api.getJobs();
  await api.getHandlers();
  await api.getDashboard();
  await api.getClusterNodes();
  await api.createJob(job);
  await api.updateJob(job.jobKey, job);
  await api.toggleJob(job.jobKey, true);
  await api.toggleJobs([job.jobKey, 'other'], false);
  await api.runJob(job.jobKey);
  await api.runJobs([job.jobKey]);
  await api.cancelJob(job.jobKey);
  await api.deleteJob(job.jobKey);
  await api.getRecords(job.jobKey, {
    maxResultCount: 50,
    skipCount: 100,
    startedFrom: '2026-10-01',
    startedTo: '2026-10-13',
    status: 'Failed',
    triggerMode: 'manual',
  });
  await api.clearRecords(job.jobKey, 10);
  await api.clearRecordsBatch([job.jobKey, 'other'], 100);
  await api.getTriggers(job.jobKey);
  await api.createTrigger(job.jobKey, trigger);
  await api.updateTrigger(job.jobKey, trigger.triggerKey, trigger);
  await api.toggleTrigger(job.jobKey, trigger.triggerKey, true);
  await api.deleteTrigger(job.jobKey, trigger.triggerKey);

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url.replace('/api', '')]),
    [
      ['GET', '/app/platform/scheduler/list'],
      ['GET', '/app/platform/scheduler/handlers'],
      ['GET', '/app/platform/scheduler/dashboard'],
      ['GET', '/app/platform/scheduler/cluster-nodes'],
      ['POST', '/app/platform/scheduler'],
      ['PUT', '/app/platform/scheduler/tenant%20health'],
      ['POST', '/app/platform/scheduler/tenant%20health/toggle'],
      ['POST', '/app/platform/scheduler/batch/toggle'],
      ['POST', '/app/platform/scheduler/tenant%20health/run'],
      ['POST', '/app/platform/scheduler/batch/run'],
      ['POST', '/app/platform/scheduler/tenant%20health/cancel'],
      ['DELETE', '/app/platform/scheduler/tenant%20health'],
      [
        'GET',
        '/app/platform/scheduler/tenant%20health/records?maxResultCount=50&skipCount=100&startedFrom=2026-10-01&startedTo=2026-10-13&status=Failed&triggerMode=manual',
      ],
      ['DELETE', '/app/platform/scheduler/tenant%20health/records?keepLastN=10'],
      ['POST', '/app/platform/scheduler/batch/records/clear'],
      ['GET', '/app/platform/scheduler/tenant%20health/triggers'],
      ['POST', '/app/platform/scheduler/tenant%20health/triggers'],
      ['PUT', '/app/platform/scheduler/tenant%20health/triggers/manual%2Fcheck'],
      ['POST', '/app/platform/scheduler/tenant%20health/triggers/manual%2Fcheck/toggle'],
      ['DELETE', '/app/platform/scheduler/tenant%20health/triggers/manual%2Fcheck'],
    ],
  );
  assert.equal(calls[4].body, JSON.stringify(job));
  assert.equal(calls[7].body, JSON.stringify({ isEnabled: false, jobKeys: ['tenant health', 'other'] }));
  assert.equal(calls[9].body, JSON.stringify({ jobKeys: ['tenant health'] }));
  assert.equal(calls[14].body, JSON.stringify({ jobKeys: ['tenant health', 'other'], keepLastN: 100 }));
  assert.equal(calls[16].body, JSON.stringify(trigger));
});

test('projects scheduler status and reports a run as successful only when the backend says so', () => {
  const job = {
    cronExpression: '0 0 2 * * ?',
    description: '',
    handlerKey: 'tenant-health-check',
    isEnabled: false,
    isRunning: true,
    jobKey: 'tenant-health-check',
    lastRunResult: '',
    lastRunTime: null,
    lockExpirationTime: null,
    nextRunTime: null,
    runningInstanceId: null,
    title: '租户健康检查',
    triggerCount: 1,
  };
  assert.deepEqual(toSchedulerJobRow(job), {
    cronExpression: job.cronExpression,
    enabledState: '停用',
    handlerKey: job.handlerKey,
    id: job.jobKey,
    lastRunTime: '—',
    nextRunTime: '—',
    runningState: '执行中',
    title: job.title,
    triggerCount: 1,
  });
  assert.equal(classifySchedulerRunMessage('手动执行成功：检查完成'), 'success');
  assert.equal(classifySchedulerRunMessage('手动执行失败：连接不可用'), 'failure');
  assert.equal(classifySchedulerRunMessage('手动执行已取消。'), 'cancelled');
  assert.equal(classifySchedulerRunMessage('定时任务正在执行中，已跳过本次触发。'), 'skipped');
  assert.equal(classifySchedulerRunMessage('未知服务响应'), 'unknown');
  assert.equal(schedulerRecordStatusLabel('Failed'), '失败');
});

const schedulerMenu = (component = '/platform/scheduler/index') => [
  {
    name: 'PlatformCenter',
    path: '/platform',
    component: 'BasicLayout',
    children: [{ name: 'PlatformScheduler', path: '/platform/scheduler', component, meta: { title: '定时任务' } }],
  },
];

test('registers the scheduler page against the backend menu and requires view permission', () => {
  assert.equal(getResolvedPages(adaptBackendMenuTree(schedulerMenu(), [])).length, 0);
  const resolved = getResolvedPages(adaptBackendMenuTree(schedulerMenu(), ['Platform.Scheduler.View']));
  assert.equal(resolved.length, 1);
  assert.equal(resolved[0].routeName, routeNames.platformScheduler);
  assert.equal(resolved[0].localPath, '/platform/scheduler');
  assert.equal(getResolvedPages(adaptBackendMenuTree(schedulerMenu('/other'), ['Platform.Scheduler.View'])).length, 0);
});
