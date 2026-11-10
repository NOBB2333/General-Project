import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import { toConversationMessage, toConversationListItem } from '../src/features/ai/conversations/conversations.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('uses the conversation model endpoint and conversation, plan, archive, export, and delete contracts', async () => {
  const calls = [];
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (input, init) => {
      calls.push({ body: init.body, method: init.method, url: String(input) });
      return Response.json({ code: 0, data: { id: 'conversation-1', messages: [] } });
    },
  });
  const api = createAiApi(client);

  await api.getModels();
  await api.getConversations({ status: 'All', keyword: '计划' });
  await api.getConversation('conversation-1');
  await api.createConversation({
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: 'None',
    modelId: null,
    mode: 'General',
    title: '测试会话',
  });
  await api.confirmPlan('conversation-1', { confirmed: true, messageId: 'message-1' });
  await api.togglePlanStep('conversation-1', { isDone: true, messageId: 'message-1', stepId: 'step-1' });
  await api.createPlanDraft('conversation-1', { messageId: 'message-1' });
  await api.archiveConversation('conversation-1');
  await api.restoreConversation('conversation-1');
  await api.exportConversation('conversation-1');
  await api.deleteConversation('conversation-1');

  assert.equal(calls[0].url, '/api/app/ai/models');
  assert.equal(calls[1].url, '/api/app/ai/conversations?keyword=%E8%AE%A1%E5%88%92&status=All');
  assert.equal(calls[3].method, 'POST');
  assert.deepEqual(JSON.parse(calls[3].body), {
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: 'None',
    modelId: null,
    mode: 'General',
    title: '测试会话',
  });
  assert.equal(calls[4].url, '/api/app/ai/conversations/conversation-1/plan/confirm');
  assert.equal(calls[5].url, '/api/app/ai/conversations/conversation-1/plan/steps/toggle');
  assert.equal(calls[6].url, '/api/app/ai/conversations/conversation-1/plan/draft');
  assert.equal(calls[7].url, '/api/app/ai/conversations/conversation-1/archive');
  assert.equal(calls[8].url, '/api/app/ai/conversations/conversation-1/restore');
  assert.equal(calls[9].url, '/api/app/ai/conversations/conversation-1/export');
  assert.equal(calls[10].method, 'DELETE');
});

test('parses chunked NDJSON events, forwards cancellation, and returns the persisted completion result', async () => {
  const encoder = new TextEncoder();
  const events = [
    { event: 'userMessage', userMessage: { id: 'message-user' } },
    { event: 'delta', delta: '你好' },
    { event: 'completed', result: { conversation: { id: 'conversation-1' }, messages: [{ id: 'message-user' }] } },
  ];
  const bytes = encoder.encode(events.map((event) => JSON.stringify(event)).join('\n') + '\n');
  const observed = [];
  let requestInit;
  const client = createApiClient({
    baseUrl: '/api',
    getAccessToken: () => 'isolated-token',
    fetcher: async (_input, init) => {
      requestInit = init;
      return new Response(
        new ReadableStream({
          start(controller) {
            controller.enqueue(bytes.slice(0, 17));
            controller.enqueue(bytes.slice(17, 53));
            controller.enqueue(bytes.slice(53));
            controller.close();
          },
        }),
        { headers: { 'Content-Type': 'application/x-ndjson; charset=utf-8' } },
      );
    },
  });
  const api = createAiApi(client);
  const controller = new AbortController();
  const result = await api.streamMessage(
    'conversation-1',
    { content: '你好', modelId: null },
    { signal: controller.signal, onEvent: (event) => observed.push(event) },
  );

  assert.equal(requestInit.method, 'POST');
  assert.equal(requestInit.headers.get('Authorization'), 'Bearer isolated-token');
  assert.equal(requestInit.headers.get('Accept'), 'application/x-ndjson');
  assert.equal(requestInit.signal.aborted, false);
  assert.deepEqual(
    observed.map((event) => event.event),
    ['userMessage', 'delta', 'completed'],
  );
  assert.equal(observed[1].delta, '你好');
  assert.equal(result.messages[0].id, 'message-user');
});

test('maps only persisted plan, run, tool, citation, and token fields from AI message metadata', () => {
  const message = toConversationMessage({
    citations: ['Project:1 项目详情'],
    completionTokens: 5,
    content: '目标：完成项目计划',
    conversationId: 'conversation-1',
    creationTime: '2026-10-14T00:00:00Z',
    errorMessage: '',
    id: 'message-1',
    metadataJson: JSON.stringify({
      structured: {
        confirmed: false,
        goal: '完成项目计划',
        mode: 'Plan',
        steps: [{ expectedOutput: 'WBS', id: 'step-1', isDone: false, title: '拆分工作' }],
        type: 'plan',
      },
      runSteps: [{ code: 'model', status: 'Completed', summary: '模型完成输出。', title: '模型推理' }],
      toolInvocations: [
        {
          elapsedMilliseconds: 32,
          errorMessage: '',
          invocationId: 'tool-1',
          status: 'Completed',
          toolCode: 'project.query',
        },
      ],
    }),
    modelId: 'model-1',
    modelName: '项目模型',
    promptTokens: 9,
    providerId: 'provider-1',
    providerName: '测试供应商',
    role: 'assistant',
    sequenceNo: 2,
    status: 'Completed',
    totalTokens: 14,
  });
  const item = toConversationListItem({
    archivedTime: null,
    completionTokens: 5,
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: 'None',
    creationTime: '2026-10-14T00:00:00Z',
    id: 'conversation-1',
    lastMessageSummary: '完成项目计划',
    lastMessageTime: null,
    messageCount: 2,
    modelId: 'model-1',
    modelName: '项目模型',
    mode: 'Plan',
    promptTokens: 9,
    status: 'Archived',
    title: '项目计划',
    totalTokens: 14,
  });

  assert.equal(message.kind, 'plan');
  assert.equal(message.aiPlan.confirmed, false);
  assert.equal(message.aiPlan.steps[0].id, 'step-1');
  assert.equal(message.aiRunSteps[0].status, '已完成');
  assert.equal(message.aiToolInvocations[0].toolCode, 'project.query');
  assert.deepEqual(message.citations, ['Project:1 项目详情']);
  assert.match(message.tokenSummary, /输入 9 · 输出 5 · 合计 14/u);
  assert.equal(item.status, 'Archived');
  assert.equal(item.pinned, false);
});

test('maps the two seeded conversation pages only for Ai.Conversation.Use', () => {
  const menu = [
    {
      children: [
        {
          component: '/ai/conversations/index',
          meta: { title: 'AI 对话' },
          name: 'AiConversations',
          path: '/ai/conversations',
        },
        {
          component: '/ai/conversation-management/index',
          meta: { title: '会话管理' },
          name: 'AiConversationManagement',
          path: '/ai/conversation-management',
        },
      ],
      component: 'BasicLayout',
      meta: { title: 'AI 智能能力' },
      name: 'AiCenter',
      path: '/ai',
    },
  ];
  const authorized = getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Conversation.Use']));

  assert.deepEqual(
    authorized.map((page) => page.routeName),
    [routeNames.aiConversations, routeNames.aiConversationManagement],
  );
  assert.deepEqual(getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Provider.Manage'])), []);
});
