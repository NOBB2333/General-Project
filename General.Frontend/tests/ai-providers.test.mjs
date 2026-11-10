import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createAiApi } from '../src/api/core/ai.ts';
import {
  createModelDraft,
  toModelDraft,
  toModelRow,
  toModelSaveInput,
  toProviderDraft,
  toProviderRow,
  toProviderSaveInput,
  validateModelDraft,
} from '../src/features/ai/providers/providers.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

const provider = {
  creationTime: '2026-10-14T00:00:00Z',
  endpoint: 'http://127.0.0.1:5500/v1',
  hasApiKey: true,
  id: 'provider-1',
  isDefault: true,
  isEnabled: true,
  lastModificationTime: null,
  modelCount: 1,
  name: '隔离测试供应商',
  organizationId: '',
  providerType: 'OpenAIResponses',
  remark: '本地模拟端点',
};

const model = {
  capabilities: ['streaming', 'textGeneration'],
  contextWindow: 128000,
  currency: 'USD',
  displayName: '隔离测试模型',
  fallbackModelId: null,
  fallbackModelName: '',
  id: 'model-1',
  inputTokenPrice: 0.000001,
  isDefault: true,
  isEnabled: true,
  maxOutputTokens: 4096,
  modelCode: 'local-model',
  modelType: 'standard',
  outputTokenPrice: 0.000002,
  providerId: provider.id,
  providerName: provider.name,
  remark: '',
  sort: 10,
};

test('uses AI provider and provider-management model endpoints with DTO payloads', async () => {
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
  const providerInput = {
    apiKey: 'sensitive-test-value',
    configJson: '{}',
    endpoint: provider.endpoint,
    isDefault: true,
    isEnabled: true,
    name: provider.name,
    organizationId: '',
    providerType: 'OpenAIResponses',
    remark: '',
  };
  const modelInput = {
    capabilities: ['textGeneration'],
    contextWindow: 128000,
    currency: 'USD',
    displayName: model.displayName,
    fallbackModelId: null,
    inputTokenPrice: 0,
    isDefault: true,
    isEnabled: true,
    maxOutputTokens: 4096,
    modelCode: model.modelCode,
    modelType: 'standard',
    outputTokenPrice: 0,
    providerId: provider.id,
    remark: '',
    sort: 10,
  };

  await api.getProviders();
  await api.createProvider(providerInput);
  await api.updateProvider('provider/1', providerInput);
  await api.testProvider('provider-1');
  await api.discoverProviderModels('provider-1');
  await api.syncProviderModels('provider-1');
  await api.getManageModels('provider-1');
  await api.createModel(modelInput);
  await api.updateModel('model/1', modelInput);
  await api.deleteModel('model-1');
  await api.deleteProvider('provider-1');

  assert.deepEqual(
    calls.map(({ method, url }) => [method, url]),
    [
      ['GET', '/api/app/ai/providers'],
      ['POST', '/api/app/ai/providers'],
      ['PUT', '/api/app/ai/providers/provider%2F1'],
      ['POST', '/api/app/ai/providers/provider-1/test'],
      ['POST', '/api/app/ai/providers/provider-1/models/discover'],
      ['POST', '/api/app/ai/providers/provider-1/models/sync'],
      ['GET', '/api/app/ai/models/manage?providerId=provider-1'],
      ['POST', '/api/app/ai/models'],
      ['PUT', '/api/app/ai/models/model%2F1'],
      ['DELETE', '/api/app/ai/models/model-1'],
      ['DELETE', '/api/app/ai/providers/provider-1'],
    ],
  );
  assert.equal(calls[1].body, JSON.stringify(providerInput));
  assert.equal(calls[7].body, JSON.stringify(modelInput));
  assert.equal(calls[9].body, undefined);
});

test('provider edit draft never copies API key material from a response', () => {
  const draft = toProviderDraft({ ...provider, apiKey: 'must-not-be-copied' });
  assert.equal(draft.apiKey, '');
  assert.equal(draft.name, provider.name);
  const input = toProviderSaveInput({ ...draft, apiKey: 'replacement-only' });
  assert.equal(input.apiKey, 'replacement-only');
  assert.equal('apiKey' in provider, false);
  assert.equal('apiKeyCipherText' in provider, false);
  assert.equal(toProviderRow(provider).keyStatus, '已配置');
});

test('model row status and default state come from the server DTO', () => {
  assert.equal(toModelRow(model).status, '启用');
  assert.equal(toModelRow(model).defaultStatus, '默认模型');
  const disabled = { ...model, isDefault: false, isEnabled: false };
  assert.equal(toModelRow(disabled).status, '停用');
  assert.equal(toModelRow(disabled).defaultStatus, '—');
  assert.equal(toModelDraft(model).fallbackModelId, '');
});

test('validates model limits before constructing the server DTO', () => {
  const draft = createModelDraft();
  draft.modelCode = 'test-model';
  draft.displayName = '测试模型';
  assert.equal(validateModelDraft(draft), null);
  assert.equal(toModelSaveInput(draft, provider.id).providerId, provider.id);
  draft.maxOutputTokens = '-1';
  assert.match(validateModelDraft(draft) ?? '', /非负整数/u);
});

test('maps the seeded AI provider menu only for the server-granted permission', () => {
  const menu = [
    {
      children: [
        {
          component: '/ai/providers/index',
          meta: { title: '模型供应商' },
          name: 'AiProviders',
          path: '/ai/providers',
        },
      ],
      component: 'BasicLayout',
      meta: { title: 'AI 智能能力' },
      name: 'AiCenter',
      path: '/ai',
    },
  ];

  const authorized = getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Provider.Manage']));
  assert.equal(authorized.length, 1);
  assert.equal(authorized[0].routeName, routeNames.aiProviders);
  assert.deepEqual(getResolvedPages(adaptBackendMenuTree(menu, ['Ai.Conversation.Use'])), []);
});
