import assert from 'node:assert/strict';
import test from 'node:test';

import { createIntegrationOperationsApi } from '../src/api/core/integration-operations.ts';
import {
  containsSensitiveSyncConfig,
  createSyncJobDraft,
  deliveryStatusLabel,
  isDeliveryRetryable,
  isSyncRecordRetryable,
  isWebhookRetryable,
  messageTypeLabel,
  providerTypeLabel,
  syncDirectionLabel,
  syncModeLabel,
  syncTypeLabel,
  toDeliveryRow,
  toSyncJobRow,
  toSyncRecordRow,
  toWebhookEventRow,
  validateSyncJobDraft,
  webhookStatusLabel,
} from '../src/features/integration/operations/operations.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';
import { routeNames } from '../src/router/route.types.ts';

test('integration operations API maps query, skip reason, retry and export contracts', async () => {
  const calls = [];
  const client = {
    request: async (path, options = {}) => {
      calls.push({ options, path });
      return options.responseType === 'blob' ? new Blob(['audit']) : { items: [], totalCount: 0 };
    },
  };
  const api = createIntegrationOperationsApi(client);
  const deliveryQuery = {
    connectorCode: 'sandbox',
    createdFrom: '2026-10-01T00:00:00.000Z',
    createdTo: '2026-10-31T23:59:59.000Z',
    keyword: 'PROJECT',
    maxResultCount: 20,
    messageType: 'Text',
    skipCount: 20,
    status: 'Failed',
    targetType: 'OpenId',
  };

  await api.getDeliveryRecords(deliveryQuery);
  await api.getDeliverySummary(deliveryQuery);
  const exported = await api.exportDeliveryRecords(deliveryQuery);
  await api.retryDelivery('delivery/1');
  await api.skipDelivery('delivery/1', { reason: '人工确认后跳过' });
  await api.saveSyncJob({
    configJson: '{}',
    connectorId: '00000000-0000-0000-0000-000000000001',
    cronExpression: '',
    direction: 'Inbound',
    isEnabled: true,
    jobCode: 'SAFE_SYNC',
    mode: 'Manual',
    name: '沙箱同步',
    remark: '',
    syncType: 'User',
  });
  await api.toggleSyncJob('job/1', false);
  await api.runSyncJob('job/1', { force: false, mode: 'Manual', requestJson: '{}' });
  await api.retrySyncRecord('record/1');
  await api.skipSyncRecord('record/1', { reason: '人工确认后跳过' });
  await api.retryWebhookEvent('event/1');
  await api.skipWebhookEvent('event/1', { reason: '人工确认后跳过' });

  assert.ok(exported instanceof Blob);
  assert.equal(calls[0].path, '/app/integration/delivery-records');
  assert.deepEqual(calls[0].options.query, deliveryQuery);
  assert.equal(calls[1].path, '/app/integration/delivery-records/summary');
  assert.deepEqual(calls[1].options.query, deliveryQuery);
  assert.equal(calls[2].path, '/app/integration/delivery-records/export');
  assert.equal(calls[2].options.responseType, 'blob');
  assert.deepEqual(calls[2].options.query, deliveryQuery);
  assert.deepEqual(
    calls.slice(3).map(({ path, options }) => [options.method, path]),
    [
      ['POST', '/app/integration/delivery-records/delivery%2F1/retry'],
      ['POST', '/app/integration/delivery-records/delivery%2F1/skip'],
      ['POST', '/app/integration/sync-jobs'],
      ['POST', '/app/integration/sync-jobs/job%2F1/toggle'],
      ['POST', '/app/integration/sync-jobs/job%2F1/run'],
      ['POST', '/app/integration/sync-records/record%2F1/retry'],
      ['POST', '/app/integration/sync-records/record%2F1/skip'],
      ['POST', '/app/integration/webhook-events/event%2F1/retry'],
      ['POST', '/app/integration/webhook-events/event%2F1/skip'],
    ],
  );
  assert.deepEqual(calls[4].options.body, { reason: '人工确认后跳过' });
  assert.deepEqual(calls[7].options.body, { force: false, mode: 'Manual', requestJson: '{}' });
  assert.equal(
    Object.keys(api).some((name) => /receiveWebhook/iu.test(name)),
    false,
  );
  assert.equal(
    calls.some(({ path }) => path.includes('/webhooks/')),
    false,
  );
});

test('operation row projections hide unredacted request and configuration fields', () => {
  const delivery = {
    attemptCount: 2,
    businessKey: 'secret-business-key',
    businessModule: 'Project',
    connectorCode: 'sandbox',
    connectorId: 'connector-id',
    creationTime: '2026-10-13T10:00:00Z',
    deadLetterReason: 'private reason',
    durationMilliseconds: 10,
    errorMessage: 'private error',
    externalMessageId: 'private-message-id',
    id: 'delivery-id',
    lastAttemptTime: null,
    maxRetryCount: 5,
    messageType: 'Text',
    nextRetryTime: null,
    payloadPreview: 'private payload',
    providerType: 'Sandbox',
    requestJson: '{"secret":"hidden"}',
    responseJson: '{"private":"hidden"}',
    sourceType: 'ProjectEvent',
    status: 'Failed',
    targetId: 'private-target',
    targetType: 'OpenId',
  };
  const syncJob = {
    configJson: '{"token":"hidden"}',
    connectorCode: 'sandbox',
    connectorId: 'connector-id',
    cronExpression: '',
    direction: 'Inbound',
    id: 'job-id',
    isEnabled: true,
    jobCode: 'SAFE_SYNC',
    lastRunError: 'private error',
    lastRunStatus: 'Failed',
    lastRunSummary: 'private summary',
    lastRunTime: null,
    mode: 'Manual',
    name: '沙箱同步',
    providerType: 'Sandbox',
    remark: '',
    syncType: 'User',
  };
  const syncRecord = {
    conflictCount: 1,
    conflictJson: '{"private":"hidden"}',
    connectorCode: 'sandbox',
    connectorId: 'connector-id',
    creationTime: '2026-10-13T10:00:00Z',
    createdCount: 1,
    durationMilliseconds: 10,
    errorMessage: 'private error',
    failedCount: 0,
    finishedTime: null,
    id: 'record-id',
    mode: 'Manual',
    providerType: 'Sandbox',
    requestJson: '{"private":"hidden"}',
    resultJson: '{"private":"hidden"}',
    retryCount: 0,
    skippedCount: 0,
    startedTime: null,
    status: 'Succeeded',
    summary: 'private summary',
    syncJobId: 'job-id',
    syncType: 'User',
    totalCount: 1,
    updatedCount: 0,
  };
  const webhook = {
    actionCode: 'private-action',
    businessKey: 'private-key',
    connectorCode: 'sandbox',
    connectorId: 'connector-id',
    creationTime: '2026-10-13T10:00:00Z',
    dedupeKey: 'private-dedupe',
    errorMessage: 'private error',
    eventId: 'private-event-id',
    eventType: 'user.updated',
    headersJson: '{"authorization":"hidden"}',
    id: 'event-id',
    normalizedPayloadJson: '{"private":"hidden"}',
    providerType: 'Sandbox',
    rawBody: 'private body',
    rawPayloadJson: '{"private":"hidden"}',
    resolvedTime: null,
    retryCount: 0,
    signatureStatus: 'Valid',
    sourceIp: '127.0.0.1',
    status: 'Failed',
  };

  const rows = [
    toDeliveryRow(delivery),
    toSyncJobRow(syncJob),
    toSyncRecordRow(syncRecord),
    toWebhookEventRow(webhook),
  ];
  const serializedRows = JSON.stringify(rows);
  for (const privateValue of [
    'secret-business-key',
    'private reason',
    'private error',
    'private payload',
    'private-target',
    'hidden',
    'private summary',
    'private body',
  ]) {
    assert.equal(serializedRows.includes(privateValue), false);
  }
  const hiddenKeys = [
    'businessKey',
    'configJson',
    'conflictJson',
    'deadLetterReason',
    'errorMessage',
    'headersJson',
    'lastRunError',
    'lastRunSummary',
    'normalizedPayloadJson',
    'payloadPreview',
    'rawBody',
    'rawPayloadJson',
    'requestJson',
    'responseJson',
    'resultJson',
    'summary',
    'targetId',
  ];
  for (const row of rows) {
    for (const key of hiddenKeys) assert.equal(key in row, false, `${key} is not part of a safe row`);
  }
  assert.equal(deliveryStatusLabel('Failed'), '失败');
  assert.equal(webhookStatusLabel('Processed'), '已处理');
  assert.equal(syncTypeLabel('User'), '用户');
  assert.equal(syncTypeLabel('Custom'), '自定义');
  assert.equal(syncDirectionLabel('Inbound'), '导入');
  assert.equal(syncModeLabel('Manual'), '手动');
  assert.equal(providerTypeLabel('Feishu'), '飞书');
  assert.equal(messageTypeLabel('Text'), '文本');
  assert.equal(isDeliveryRetryable(delivery), true);
  assert.equal(isSyncRecordRetryable(syncRecord), false);
  assert.equal(isWebhookRetryable(webhook), true);
});

test('sync job config rejects sensitive keys and route access follows backend view permissions', () => {
  const draft = createSyncJobDraft();
  draft.name = '沙箱同步';
  draft.jobCode = 'SAFE_SYNC';
  draft.connectorId = '00000000-0000-0000-0000-000000000001';
  assert.equal(validateSyncJobDraft(draft), null);
  draft.configJson = `{"mapping":"${'x'.repeat(16_000)}"}`;
  assert.match(validateSyncJobDraft(draft) ?? '', /16000/u);
  draft.configJson = '{"mapping":{"accessToken":"must-not-save"}}';
  assert.equal(containsSensitiveSyncConfig(draft.configJson), true);
  assert.match(validateSyncJobDraft(draft) ?? '', /凭证/u);

  const menu = [
    {
      component: 'BasicLayout',
      name: 'IntegrationCenter',
      path: '/integration',
      children: [
        {
          component: '/integration/delivery-records/index',
          meta: { title: '投递日志' },
          name: 'IntegrationDeliveryRecords',
          path: '/integration/delivery-records',
        },
        {
          component: '/integration/sync-jobs/index',
          meta: { title: '同步任务' },
          name: 'IntegrationSyncJobs',
          path: '/integration/sync-jobs',
        },
        {
          component: '/integration/webhook-events/index',
          meta: { title: 'Webhook 事件' },
          name: 'IntegrationWebhookEvents',
          path: '/integration/webhook-events',
        },
      ],
    },
  ];
  const allPages = getResolvedPages(
    adaptBackendMenuTree(menu, ['Integration.Delivery.View', 'Integration.Sync.View', 'Integration.Webhook.View']),
  );
  assert.deepEqual(
    allPages.map((page) => page.routeName),
    [routeNames.integrationDeliveryRecords, routeNames.integrationSyncJobs, routeNames.integrationWebhookEvents],
  );
  assert.deepEqual(
    getResolvedPages(adaptBackendMenuTree(menu, ['Integration.Delivery.View'])).map((page) => page.routeName),
    [routeNames.integrationDeliveryRecords],
  );
});
