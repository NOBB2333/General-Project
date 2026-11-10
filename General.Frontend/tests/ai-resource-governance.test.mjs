import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('uses the existing MCP, memory, knowledge, and web search API contracts', async () => {
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

  const server = {
    argumentsJson: '[]',
    command: '',
    configJson: '{}',
    endpoint: 'https://mcp.example.com',
    environmentJson: '{}',
    headersJson: '{}',
    isEnabled: true,
    name: 'MCP 服务',
    permissionCode: 'Ai.Mcp.Manage',
    protocolVersion: '2024-11-05',
    remark: '',
    serverCode: 'mcp.main',
    sort: 10,
    timeoutSeconds: 30,
    transportType: 'StreamableHttp',
  };
  const memory = {
    content: '内容',
    contextId: null,
    contextType: null,
    importance: 50,
    isEnabled: true,
    knowledgeSourceId: null,
    memoryType: 'User',
    ownerUserId: null,
    sourceReferencesJson: '[]',
    summary: '',
    tagsJson: '[]',
    title: '个人记忆',
  };
  const searchConfig = {
    apiKey: 'secret',
    allowDomains: ['search.example.com'],
    configJson: '{}',
    denyDomains: [],
    endpoint: 'https://search.example.com/api',
    isDefault: true,
    isEnabled: true,
    maxResults: 5,
    name: '搜索服务',
    providerType: 'Custom',
    remark: '',
    timeoutSeconds: 15,
  };

  await api.getMcpServers();
  await api.createMcpServer(server);
  await api.updateMcpServer('server/1', server);
  await api.testMcpServer('server/1');
  await api.discoverMcpTools('server/1');
  await api.getMcpTools('server/1');
  await api.updateMcpToolPolicy('tool/1', { isEnabled: false, permissionCode: 'Ai.Mcp.Manage' });
  await api.invokeMcpTool('tool/1', { argumentsJson: '{}' });
  await api.getMcpInvocations({ maxResultCount: 50, status: 'Failed' });
  await api.getMemories({ includeDisabled: true, memoryType: 'User', maxResultCount: 200 });
  await api.createMemory(memory);
  await api.updateMemory('memory/1', memory);
  await api.toggleMemory('memory/1', { isEnabled: false });
  await api.deleteMemory('memory/1');
  await api.getKnowledgeSources({ maxResultCount: 200, projectId: 'project/1' });
  await api.importProjectDocuments('project/1');
  await api.importProjectMeetings('project/1');
  await api.importProjectRisks('project/1');
  await api.getWebSearchConfigs(true);
  await api.createWebSearchConfig(searchConfig);
  await api.updateWebSearchConfig('config/1', searchConfig);
  await api.deleteWebSearchConfig('config/1');
  await api.getWebSearchRecords({ maxResultCount: 50, status: 'Failed' });
  await api.runWebSearch({ configId: null, conversationId: null, maxResults: 5, messageId: null, query: '查询词' });

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url]),
    [
      ['GET', '/api/app/ai/mcp/servers'],
      ['POST', '/api/app/ai/mcp/servers'],
      ['PUT', '/api/app/ai/mcp/servers/server%2F1'],
      ['POST', '/api/app/ai/mcp/servers/server%2F1/test'],
      ['POST', '/api/app/ai/mcp/servers/server%2F1/discover-tools'],
      ['GET', '/api/app/ai/mcp/tools?serverId=server%2F1'],
      ['PUT', '/api/app/ai/mcp/tools/tool%2F1/policy'],
      ['POST', '/api/app/ai/mcp/tools/tool%2F1/invoke'],
      ['GET', '/api/app/ai/mcp/invocations?maxResultCount=50&status=Failed'],
      ['GET', '/api/app/ai/memories?includeDisabled=true&maxResultCount=200&memoryType=User'],
      ['POST', '/api/app/ai/memories'],
      ['PUT', '/api/app/ai/memories/memory%2F1'],
      ['POST', '/api/app/ai/memories/memory%2F1/toggle'],
      ['DELETE', '/api/app/ai/memories/memory%2F1'],
      ['GET', '/api/app/ai/knowledge-sources?maxResultCount=200&projectId=project%2F1'],
      ['POST', '/api/app/ai/knowledge-sources/project/project%2F1/documents/import'],
      ['POST', '/api/app/ai/knowledge-sources/project/project%2F1/meetings/import'],
      ['POST', '/api/app/ai/knowledge-sources/project/project%2F1/risks/import'],
      ['GET', '/api/app/ai/web-search/configs?includeDisabled=true'],
      ['POST', '/api/app/ai/web-search/configs'],
      ['PUT', '/api/app/ai/web-search/configs/config%2F1'],
      ['DELETE', '/api/app/ai/web-search/configs/config%2F1'],
      ['GET', '/api/app/ai/web-search/records?maxResultCount=50&status=Failed'],
      ['POST', '/api/app/ai/web-search/search'],
    ],
  );
  assert.equal(calls[1].body, JSON.stringify(server));
  assert.equal(calls[10].body, JSON.stringify(memory));
  assert.equal(calls[19].body, JSON.stringify(searchConfig));
  assert.equal(
    calls[23].body,
    JSON.stringify({
      configId: null,
      conversationId: null,
      maxResults: 5,
      messageId: null,
      query: '查询词',
    }),
  );
});

test('maps MCP, memory, and web search pages to their seeded backend permissions', () => {
  const menu = [
    {
      children: [
        {
          component: '/ai/mcp/index',
          meta: { title: 'MCP 配置' },
          name: 'AiMcp',
          path: '/ai/mcp',
        },
        {
          component: '/ai/memory/index',
          meta: { title: '记忆库' },
          name: 'AiMemory',
          path: '/ai/memory',
        },
        {
          component: '/ai/web-search/index',
          meta: { title: '联网搜索' },
          name: 'AiWebSearch',
          path: '/ai/web-search',
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

  assert.deepEqual(resolved(['Ai.Mcp.Manage']), [routeNames.aiMcp]);
  assert.deepEqual(resolved(['Ai.Memory.Manage']), [routeNames.aiMemory]);
  assert.deepEqual(resolved(['Ai.WebSearch.Manage']), [routeNames.aiWebSearch]);
  assert.deepEqual(resolved(['Ai.Mcp.Manage', 'Ai.Memory.Manage', 'Ai.WebSearch.Manage']), [
    routeNames.aiMcp,
    routeNames.aiMemory,
    routeNames.aiWebSearch,
  ]);
});
