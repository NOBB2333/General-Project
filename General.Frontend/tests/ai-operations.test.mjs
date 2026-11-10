import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('uses the backend AI job, usage, and writeback endpoints and DTO payloads', async () => {
  const calls = [];
  const api = createAiApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ body: init.body, method: init.method, url: String(url) });
        return Response.json({ code: 0, data: [] });
      },
    }),
  );
  const job = {
    assistantCode: '',
    contextScopeJson: '{"maxItems":8}',
    description: '周度项目摘要',
    isEnabled: true,
    jobCode: 'weekly-summary',
    jobType: 'ProjectWeeklySummary',
    modelId: null,
    name: '项目周报',
    outputTargetJson: '{}',
    promptTemplateCode: '',
    schedulerJobKey: 'ai-automation-job',
  };
  const writeback = {
    actionCode: 'Project.Task.Create',
    businessModule: 'Project',
    conversationId: null,
    diffPreviewJson: '{"status":"Ready"}',
    messageId: null,
    payloadJson: '{"title":"新任务"}',
    projectId: null,
    sourceType: 'Ai',
    summary: '新增任务草稿',
    targetEntityId: '',
    targetEntityType: 'ProjectTask',
  };

  await api.getJobs({ includeDisabled: true, keyword: 'weekly', maxResultCount: 25 });
  await api.createJob(job);
  await api.updateJob('job/1', job);
  await api.toggleJob('job/1', { isEnabled: false });
  await api.deleteJob('job/1');
  await api.runJob('job/1');
  await api.getJobRuns({ aiJobId: 'job/1', maxResultCount: 50, status: 'Failed' });
  await api.getUsageDashboard({ createdFrom: '2026-10-01', maxResultCount: 500, source: 'ModelChat' });
  await api.getUsageRecords({ createdTo: '2026-10-14', status: 'Failed' });
  await api.getWritebacks({ maxResultCount: 20, skipCount: 20, status: 'Pending' });
  await api.createWriteback(writeback);
  await api.approveWriteback('request/1');
  await api.rejectWriteback('request/1', { reason: '缺少目标负责人' });
  await api.cancelWriteback('request/1');

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url]),
    [
      ['GET', '/api/app/ai/jobs?includeDisabled=true&keyword=weekly&maxResultCount=25'],
      ['POST', '/api/app/ai/jobs'],
      ['PUT', '/api/app/ai/jobs/job%2F1'],
      ['POST', '/api/app/ai/jobs/job%2F1/toggle'],
      ['DELETE', '/api/app/ai/jobs/job%2F1'],
      ['POST', '/api/app/ai/jobs/job%2F1/run'],
      ['GET', '/api/app/ai/job-runs?aiJobId=job%2F1&maxResultCount=50&status=Failed'],
      ['GET', '/api/app/ai/usage/dashboard?createdFrom=2026-10-01&maxResultCount=500&source=ModelChat'],
      ['GET', '/api/app/ai/usage/records?createdTo=2026-10-14&status=Failed'],
      ['GET', '/api/app/ai/writebacks?maxResultCount=20&skipCount=20&status=Pending'],
      ['POST', '/api/app/ai/writebacks'],
      ['POST', '/api/app/ai/writebacks/request%2F1/approve'],
      ['POST', '/api/app/ai/writebacks/request%2F1/reject'],
      ['POST', '/api/app/ai/writebacks/request%2F1/cancel'],
    ],
  );
  assert.equal(calls[1].body, JSON.stringify(job));
  assert.equal(calls[2].body, JSON.stringify(job));
  assert.equal(calls[3].body, JSON.stringify({ isEnabled: false }));
  assert.equal(calls[10].body, JSON.stringify(writeback));
  assert.equal(calls[12].body, JSON.stringify({ reason: '缺少目标负责人' }));
});

test('maps job, usage, and writeback menu pages to their matching backend permissions', () => {
  const menu = [
    {
      children: [
        { component: '/ai/jobs/index', meta: { title: '自动化任务' }, name: 'AiJobs', path: '/ai/jobs' },
        { component: '/ai/usage/index', meta: { title: '用量与成本' }, name: 'AiUsage', path: '/ai/usage' },
        {
          component: '/ai/writebacks/index',
          meta: { title: '写回确认' },
          name: 'AiWritebacks',
          path: '/ai/writebacks',
        },
      ],
      component: 'BasicLayout',
      meta: { title: 'AI 智能能力' },
      name: 'AiCenter',
      path: '/ai',
    },
  ];
  const resolved = (permissions) =>
    getResolvedPages(adaptBackendMenuTree(menu, permissions)).map((page) => page.routeName);

  assert.deepEqual(resolved(['Ai.Job.Manage']), [routeNames.aiJobs]);
  assert.deepEqual(resolved(['Ai.Usage.View']), [routeNames.aiUsage]);
  assert.deepEqual(resolved(['Ai.Writeback.View']), [routeNames.aiWritebacks]);
  assert.deepEqual(resolved(['Ai.Job.Run']), []);
  assert.deepEqual(resolved(['Ai.Job.Manage', 'Ai.Usage.View', 'Ai.Writeback.View']), [
    routeNames.aiJobs,
    routeNames.aiUsage,
    routeNames.aiWritebacks,
  ]);
});
