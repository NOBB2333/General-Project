import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('uses existing Agent, command, artifact, tool, and invocation contracts', async () => {
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
  const command = {
    agentCode: '',
    commandCode: 'weekly.report',
    configJson: '{}',
    description: '',
    exampleInput: '',
    inputPlaceholder: '',
    isEnabled: true,
    mode: 'General',
    name: '周报',
    promptTemplateCode: '',
    skillCode: '',
    sort: 0,
    slashName: '/周报',
  };
  const agent = {
    agentCode: 'project.assistant',
    defaultModelId: null,
    description: '',
    isEnabled: true,
    name: '项目助手',
    personaCode: '',
    roleType: 'ProjectManager',
    runtimeConfigJson: '{}',
    skillCodes: [],
    sort: 0,
    systemPrompt: '帮助管理项目。',
    toolCodes: [],
  };
  const artifact = {
    artifactCode: 'project.summary',
    category: 'Project',
    description: '',
    examplePayloadJson: '{}',
    isEnabled: true,
    jsonSchema: '{}',
    name: '项目摘要',
    rendererType: 'metric-card',
    sort: 0,
    systemHint: '',
  };

  await api.getCommands({ includeDisabled: true, keyword: '周报' });
  await api.createCommand(command);
  await api.updateCommand('command/1', command);
  await api.deleteCommand('command/1');
  await api.getAgents({ includeDisabled: true });
  await api.createAgent(agent);
  await api.updateAgent('agent/1', agent);
  await api.buildAgentRunPlan('agent/1', { inputJson: '{}' });
  await api.deleteAgent('agent/1');
  await api.getArtifacts({ category: 'Project', includeDisabled: true });
  await api.createArtifact(artifact);
  await api.updateArtifact('artifact/1', artifact);
  await api.deleteArtifact('artifact/1');
  await api.getTools();
  await api.getToolInvocations({ maxResultCount: 100, status: 'Failed' });

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url]),
    [
      ['GET', '/api/app/ai/commands?includeDisabled=true&keyword=%E5%91%A8%E6%8A%A5'],
      ['POST', '/api/app/ai/commands'],
      ['PUT', '/api/app/ai/commands/command%2F1'],
      ['DELETE', '/api/app/ai/commands/command%2F1'],
      ['GET', '/api/app/ai/agents?includeDisabled=true'],
      ['POST', '/api/app/ai/agents'],
      ['PUT', '/api/app/ai/agents/agent%2F1'],
      ['POST', '/api/app/ai/agents/agent%2F1/run-plan'],
      ['DELETE', '/api/app/ai/agents/agent%2F1'],
      ['GET', '/api/app/ai/artifacts?category=Project&includeDisabled=true'],
      ['POST', '/api/app/ai/artifacts'],
      ['PUT', '/api/app/ai/artifacts/artifact%2F1'],
      ['DELETE', '/api/app/ai/artifacts/artifact%2F1'],
      ['GET', '/api/app/ai/tools'],
      ['GET', '/api/app/ai/tool-invocations?maxResultCount=100&status=Failed'],
    ],
  );
  assert.equal(calls[1].body, JSON.stringify(command));
  assert.equal(calls[5].body, JSON.stringify(agent));
  assert.equal(calls[7].body, JSON.stringify({ inputJson: '{}' }));
  assert.equal(calls[10].body, JSON.stringify(artifact));
  assert.equal('invokeTool' in api, false);
});

test('maps Agent and artifact pages to their separate backend permissions', () => {
  const menu = [
    {
      children: [
        {
          component: '/ai/agent-design/index',
          meta: { title: 'Agent 编排' },
          name: 'AiAgentDesign',
          path: '/ai/agent-design',
        },
        {
          component: '/ai/artifacts/index',
          meta: { title: '安全构件' },
          name: 'AiArtifacts',
          path: '/ai/artifacts',
        },
      ],
      component: 'BasicLayout',
      meta: { title: 'AI 智能能力' },
      name: 'AiCenter',
      path: '/ai',
    },
  ];

  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Agent.Manage'])).map((page) => page.routeName),
    [routeNames.aiAgentDesign],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Artifact.Manage'])).map((page) => page.routeName),
    [routeNames.aiArtifacts],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Agent.Manage', 'Ai.Artifact.Manage'])).map(
      (page) => page.routeName,
    ),
    [routeNames.aiAgentDesign, routeNames.aiArtifacts],
  );
});
