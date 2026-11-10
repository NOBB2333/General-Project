import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('uses the assistant, prompt-template, skill, and persona contracts', async () => {
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
  const assistantInput = {
    assistantCode: 'project-helper',
    configJson: '{}',
    defaultMode: 'General',
    defaultModelId: null,
    description: '',
    isEnabled: true,
    name: '项目助手',
    roleType: 'Project',
    sort: 10,
    systemPrompt: '受管理权限保护',
    toolCodes: [],
  };
  const promptInput = {
    assistantCode: 'project-helper',
    category: '项目管理',
    description: '',
    inputSchemaJson: '{}',
    isEnabled: true,
    mode: 'General',
    name: '项目周报',
    outputSchemaJson: '{}',
    promptContent: '受管理权限保护',
    scenario: 'Weekly',
    sort: 10,
    templateCode: 'project-weekly',
    toolCodes: [],
    version: 1,
  };
  const skillInput = {
    category: '项目管理',
    defaultMode: 'General',
    description: '',
    inputSchemaJson: '{}',
    instruction: '受管理权限保护',
    isEnabled: true,
    name: '项目分析',
    outputSchemaJson: '{}',
    skillCode: 'project-analysis',
    sort: 10,
    toolCodes: [],
  };
  const personaInput = {
    boundaryPrompt: '',
    configJson: '{}',
    description: '',
    isEnabled: true,
    name: '项目经理',
    personaCode: 'project-manager',
    personalityPrompt: '受管理权限保护',
    roleType: 'Project',
    sort: 10,
    stylePrompt: '',
  };

  await api.getAssistants();
  await api.createAssistant(assistantInput);
  await api.updateAssistant('assistant-1', assistantInput);
  await api.deleteAssistant('assistant-1');
  await api.getPromptTemplates({ includeDisabled: true, keyword: '周报' });
  await api.getUsablePromptTemplates();
  await api.createPromptTemplate(promptInput);
  await api.updatePromptTemplate('template-1', promptInput);
  await api.dryRunPromptTemplate('template-1', {
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: 'None',
    inputJson: '{}',
  });
  await api.createConversationFromTemplate('template-1', {
    contextId: '',
    contextSnapshotJson: '{}',
    contextType: 'None',
    inputJson: '{}',
    modelId: null,
    title: '',
  });
  await api.deletePromptTemplate('template-1');
  await api.getSkills({ includeDisabled: true });
  await api.getUsableSkills();
  await api.createSkill(skillInput);
  await api.updateSkill('skill-1', skillInput);
  await api.deleteSkill('skill-1');
  await api.getPersonas({ includeDisabled: true });
  await api.createPersona(personaInput);
  await api.updatePersona('persona-1', personaInput);
  await api.deletePersona('persona-1');

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url]),
    [
      ['GET', '/api/app/ai/assistants?includeDisabled=true'],
      ['POST', '/api/app/ai/assistants'],
      ['PUT', '/api/app/ai/assistants/assistant-1'],
      ['DELETE', '/api/app/ai/assistants/assistant-1'],
      ['GET', '/api/app/ai/prompt-templates?includeDisabled=true&keyword=%E5%91%A8%E6%8A%A5'],
      ['GET', '/api/app/ai/prompt-templates/available'],
      ['POST', '/api/app/ai/prompt-templates'],
      ['PUT', '/api/app/ai/prompt-templates/template-1'],
      ['POST', '/api/app/ai/prompt-templates/template-1/dry-run'],
      ['POST', '/api/app/ai/prompt-templates/template-1/conversations'],
      ['DELETE', '/api/app/ai/prompt-templates/template-1'],
      ['GET', '/api/app/ai/skills?includeDisabled=true'],
      ['GET', '/api/app/ai/skills/available'],
      ['POST', '/api/app/ai/skills'],
      ['PUT', '/api/app/ai/skills/skill-1'],
      ['DELETE', '/api/app/ai/skills/skill-1'],
      ['GET', '/api/app/ai/personas?includeDisabled=true'],
      ['POST', '/api/app/ai/personas'],
      ['PUT', '/api/app/ai/personas/persona-1'],
      ['DELETE', '/api/app/ai/personas/persona-1'],
    ],
  );
  assert.equal(calls[1].body, JSON.stringify(assistantInput));
  assert.equal(calls[6].body, JSON.stringify(promptInput));
  assert.equal(calls[13].body, JSON.stringify(skillInput));
  assert.equal(calls[17].body, JSON.stringify(personaInput));
});

test('maps each AI admin page only for its matching backend permission', () => {
  const menu = [
    {
      children: [
        {
          component: '/ai/assistants/index',
          meta: { title: '助手角色' },
          name: 'AiAssistants',
          path: '/ai/assistants',
        },
        { component: '/ai/prompts/index', meta: { title: '场景模板' }, name: 'AiPrompts', path: '/ai/prompts' },
        {
          component: '/ai/agent-design/index',
          meta: { title: 'Agent 编排' },
          name: 'AiAgentDesign',
          path: '/ai/agent-design',
        },
      ],
      component: 'BasicLayout',
      meta: { title: 'AI 智能能力' },
      name: 'AiCenter',
      path: '/ai',
    },
  ];

  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Assistant.Manage'])).map((page) => page.routeName),
    [routeNames.aiAssistants],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Prompt.Manage'])).map((page) => page.routeName),
    [routeNames.aiPrompts],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Agent.Manage'])).map((page) => page.routeName),
    [routeNames.aiAgentDesign],
  );
});
