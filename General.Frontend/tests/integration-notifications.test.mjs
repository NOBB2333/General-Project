import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createIntegrationNotificationsApi } from '../src/api/core/integration-notifications.ts';
import {
  createRuleDraft,
  createTemplateDraft,
  ruleStatusLabel,
  templateStatusLabel,
  toRuleSaveInput,
  toTemplateRow,
  validateRuleDraft,
  validateTemplateDraft,
} from '../src/features/integration/notifications/notifications.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('notification API uses template and rule contracts and forces evaluation to remain read-only', async () => {
  const calls = [];
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (url, init) => {
      calls.push({ body: init.body, method: init.method, url: String(url) });
      return Response.json({ code: 0, data: { items: [], totalCount: 0 } });
    },
  });
  const api = createIntegrationNotificationsApi(client);
  const templateInput = {
    bodyTemplate: 'body',
    cardJson: '{}',
    messageType: 'Text',
    name: '项目提醒',
    previewVariablesJson: '{}',
    remark: '',
    status: 'Draft',
    templateCode: 'PROJECT_NOTICE',
    titleTemplate: 'title',
    variableSchemaJson: '{}',
  };
  const ruleInput = {
    channelType: 'Personal',
    conditionJson: '{}',
    connectorId: null,
    eventCode: 'Project.Updated',
    eventName: '项目更新',
    name: '项目更新通知',
    quietEndTime: '',
    quietStartTime: '',
    rateLimitMaxCount: 0,
    rateLimitWindowMinutes: 0,
    recipientRuleJson: '{"mode":"Direct","targetIds":[]}',
    remark: '',
    ruleCode: 'PROJECT_UPDATED',
    status: 'Active',
    targetType: 'OpenId',
    templateId: '00000000-0000-0000-0000-000000000001',
  };

  await api.getTemplates({ keyword: '项目', maxResultCount: 20, skipCount: 0, status: 'All' });
  await api.saveTemplate(templateInput);
  await api.renderTemplate({
    bodyTemplate: templateInput.bodyTemplate,
    cardJson: templateInput.cardJson,
    titleTemplate: templateInput.titleTemplate,
    variablesJson: '{}',
  });
  await api.getRules({ eventCode: 'Project.Updated', maxResultCount: 20, skipCount: 0, status: 'All' });
  await api.saveRule(ruleInput);
  await api.toggleRule('rule/1', 'Disabled');
  await api.evaluateRules({
    commitTriggered: true,
    eventCode: 'Project.Updated',
    variablesJson: '{}',
  });

  assert.deepEqual(calls, [
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/templates?keyword=%E9%A1%B9%E7%9B%AE&maxResultCount=20&skipCount=0&status=All',
    },
    { body: JSON.stringify(templateInput), method: 'POST', url: '/api/app/integration/templates' },
    {
      body: JSON.stringify({
        bodyTemplate: templateInput.bodyTemplate,
        cardJson: templateInput.cardJson,
        titleTemplate: templateInput.titleTemplate,
        variablesJson: '{}',
      }),
      method: 'POST',
      url: '/api/app/integration/templates/render',
    },
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/integration/notification-rules?eventCode=Project.Updated&maxResultCount=20&skipCount=0&status=All',
    },
    { body: JSON.stringify(ruleInput), method: 'POST', url: '/api/app/integration/notification-rules' },
    {
      body: JSON.stringify({ status: 'Disabled' }),
      method: 'POST',
      url: '/api/app/integration/notification-rules/rule%2F1/toggle',
    },
    {
      body: JSON.stringify({ commitTriggered: false, eventCode: 'Project.Updated', variablesJson: '{}' }),
      method: 'POST',
      url: '/api/app/integration/notification-rules/evaluate',
    },
  ]);
  assert.equal(
    calls.some((call) => call.url.endsWith('/dispatch')),
    false,
  );
});

test('notification models validate JSON, backend-supported frequency limits, labels, and status display', () => {
  const templateDraft = createTemplateDraft();
  templateDraft.name = '项目提醒';
  templateDraft.templateCode = 'PROJECT_NOTICE';
  assert.equal(validateTemplateDraft(templateDraft), null);
  templateDraft.cardJson = '{';
  assert.match(validateTemplateDraft(templateDraft) ?? '', /合法JSON/u);

  const ruleDraft = createRuleDraft();
  ruleDraft.name = '项目更新通知';
  ruleDraft.ruleCode = 'PROJECT_UPDATED';
  ruleDraft.eventCode = 'Project.Updated';
  ruleDraft.templateId = '00000000-0000-0000-0000-000000000001';
  assert.equal(validateRuleDraft(ruleDraft), null);
  ruleDraft.conditionJson = '[]';
  assert.match(validateRuleDraft(ruleDraft) ?? '', /根节点/u);
  ruleDraft.conditionJson = '{}';
  ruleDraft.rateLimitMaxCount = '2';
  assert.match(validateRuleDraft(ruleDraft) ?? '', /仅支持/u);
  ruleDraft.rateLimitMaxCount = '0';
  ruleDraft.quietStartTime = '22:00';
  assert.match(validateRuleDraft(ruleDraft) ?? '', /同时填写/u);
  ruleDraft.quietEndTime = '25:00';
  assert.match(validateRuleDraft(ruleDraft) ?? '', /格式/u);

  assert.equal(templateStatusLabel('Draft'), '草稿');
  assert.equal(ruleStatusLabel('Paused'), '暂停');
  assert.equal(toTemplateRow({ ...templateDraft, cardJson: '{}', creationTime: '', id: 'template-1' }).status, '草稿');
  assert.equal(toRuleSaveInput({ ...ruleDraft, rateLimitMaxCount: '0' }).rateLimitMaxCount, 0);
});

test('template and rule pages resolve only through their backend menu components and view permissions', () => {
  const menu = [
    {
      component: 'BasicLayout',
      name: 'IntegrationCenter',
      path: '/integration',
      children: [
        {
          component: '/integration/templates/index',
          meta: { title: '消息模板' },
          name: 'IntegrationTemplates',
          path: '/integration/templates',
        },
        {
          component: '/integration/rules/index',
          meta: { title: '通知规则' },
          name: 'IntegrationRules',
          path: '/integration/rules',
        },
      ],
    },
  ];

  const authorized = getResolvedPages(
    adaptBackendMenuTree(menu, ['Integration.Template.View', 'Integration.Rule.View']),
  );
  assert.deepEqual(
    authorized.map((page) => page.routeName),
    [routeNames.integrationTemplates, routeNames.integrationRules],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Integration.Template.View'])).map((page) => page.routeName),
    [routeNames.integrationTemplates],
  );
});
