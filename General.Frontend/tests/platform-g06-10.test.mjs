import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createPlatformFileStorageSourcesApi } from '../src/api/core/platform-file-storage-sources.ts';
import { createPlatformRecycleBinApi } from '../src/api/core/platform-recycle-bin.ts';
import { createPlatformUpdateLogApi } from '../src/api/core/platform-update-log.ts';
import {
  toStorageSourceDraft,
  toStorageSourceRow,
  toStorageSourceSaveInput,
  validateStorageSourceDraft,
} from '../src/features/platform/files/storage-sources.model.ts';
import {
  canPermanentlyDeleteRecycleBinEntity,
  canRestoreRecycleBinEntity,
  toRecycleBinRow,
} from '../src/features/platform/recycle-bin/recycle-bin.model.ts';
import {
  createUpdateLogDraft,
  parseUpdateLogTime,
  toUpdateLogDraft,
  toUpdateLogRow,
  toUpdateLogSaveInput,
  validateUpdateLogDraft,
} from '../src/features/platform/update-logs/update-log.model.ts';
import { adaptBackendMenuTree, getResolvedPages } from '../src/navigation/page-registry.ts';

const source = {
  accessKeyId: '',
  bucketName: null,
  customDomain: null,
  endpoint: null,
  id: 'source-1',
  isDefault: true,
  isEnabled: true,
  isPublic: false,
  name: '隔离测试文件存储',
  pathTemplate: null,
  providerName: 'Local',
  region: null,
  remark: null,
  rootPath: '/tmp/general-frontend-files',
  useSsl: false,
};

const updateLog = {
  authorName: '管理员',
  creationTime: '2026-10-13T08:00:00Z',
  id: 'update-1',
  impactScope: '全部用户',
  publishedAt: '2026-10-13T08:00:00Z',
  summary: '更新摘要',
  title: '版本更新',
  version: '1.2.3',
};

test('uses the storage-source endpoints, query fields, and request DTOs from the backend contract', async () => {
  const calls = [];
  const api = createPlatformFileStorageSourcesApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        const call = { body: init.body, method: init.method, url: String(url) };
        calls.push(call);
        const data =
          call.method === 'GET'
            ? [source]
            : call.method === 'DELETE' || call.url.endsWith('/toggle') || call.url.endsWith('/default')
              ? true
              : source;
        return Response.json({ code: 0, data });
      },
    }),
  );
  const input = {
    accessKeyId: '',
    bucketName: null,
    customDomain: null,
    endpoint: null,
    isDefault: false,
    isEnabled: true,
    isPublic: false,
    name: '隔离测试文件存储',
    pathTemplate: null,
    providerName: 'Local',
    region: null,
    remark: null,
    rootPath: '/tmp/general-frontend-files',
    secretKey: '',
    useSsl: false,
  };

  await api.getList(false);
  await api.create(input);
  await api.update('source/1', input);
  await api.toggle('source-1', false);
  await api.setDefault('source-1');
  await api.delete('source-1');

  assert.deepEqual(calls, [
    { body: undefined, method: 'GET', url: '/api/app/platform/file-storage-sources?enabledOnly=false' },
    { body: JSON.stringify(input), method: 'POST', url: '/api/app/platform/file-storage-sources' },
    { body: JSON.stringify(input), method: 'PUT', url: '/api/app/platform/file-storage-sources/source%2F1' },
    {
      body: JSON.stringify({ isEnabled: false }),
      method: 'POST',
      url: '/api/app/platform/file-storage-sources/source-1/toggle',
    },
    { body: undefined, method: 'POST', url: '/api/app/platform/file-storage-sources/source-1/default' },
    { body: undefined, method: 'DELETE', url: '/api/app/platform/file-storage-sources/source-1' },
  ]);
});

test('validates local/cloud storage drafts and never maps secret fields from the response DTO', () => {
  const draft = toStorageSourceDraft(source);
  assert.equal(draft.secretKey, '');
  assert.equal(validateStorageSourceDraft(draft), null);
  const row = toStorageSourceRow(source);
  assert.equal('secretKey' in row, false);
  assert.equal('rootPath' in row, false);

  draft.providerName = 'AliyunOSS';
  assert.match(validateStorageSourceDraft(draft) ?? '', /Endpoint/);
  draft.endpoint = 'https://oss.example.test';
  draft.accessKeyId = 'test-key-id';
  draft.bucketName = 'test-bucket';
  assert.match(validateStorageSourceDraft(draft) ?? '', /SecretKey/);
  draft.secretKey = 'test-only-secret';
  assert.equal(validateStorageSourceDraft(draft), null);
  assert.equal(toStorageSourceSaveInput(draft).secretKey, 'test-only-secret');
});

test('uses the recycle-bin paging, restore, and single-object permanent-delete routes', async () => {
  const calls = [];
  const api = createPlatformRecycleBinApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ method: init.method, url: String(url) });
        return Response.json({ code: 0, data: init.method === 'GET' ? { items: [], totalCount: 0 } : true });
      },
    }),
  );

  await api.getItems({ entityType: 'file', maxResultCount: 20, skipCount: 20 });
  await api.restore('update-log', 'id/1');
  await api.deletePermanently('file', 'id/1');

  assert.deepEqual(calls, [
    { method: 'GET', url: '/api/app/platform/recycle-bin/items?entityType=file&maxResultCount=20&skipCount=20' },
    { method: 'POST', url: '/api/app/platform/recycle-bin/update-log/id%2F1/restore' },
    { method: 'DELETE', url: '/api/app/platform/recycle-bin/file/id%2F1' },
  ]);
  assert.equal(canRestoreRecycleBinEntity('scheduled-trigger'), true);
  assert.equal(canRestoreRecycleBinEntity('unknown'), false);
  assert.equal(canPermanentlyDeleteRecycleBinEntity('FILE'), true);
  assert.equal(canPermanentlyDeleteRecycleBinEntity('update-log'), false);
  assert.equal(
    toRecycleBinRow({
      deletionTime: null,
      deleterId: null,
      deleterName: null,
      displayName: '版本更新',
      entityType: 'update-log',
      id: 'id-1',
      originalLocation: '',
    }).id,
    'update-log:id-1',
  );
});

test('uses update-log list and write endpoints with the server DTO fields', async () => {
  const calls = [];
  const api = createPlatformUpdateLogApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async (url, init) => {
        calls.push({ body: init.body, method: init.method, url: String(url) });
        return Response.json({ code: 0, data: init.method === 'GET' ? { items: [updateLog], totalCount: 1 } : true });
      },
    }),
  );
  const input = {
    impactScope: '全部用户',
    publishedAt: '2026-10-13T08:00:00.000Z',
    summary: '更新摘要',
    title: '版本更新',
    version: '1.2.3',
  };

  await api.getPaged({ keyword: '版本', maxResultCount: 20, skipCount: 0 });
  await api.create(input);
  await api.update('update/1', input);
  await api.delete('update-1');

  assert.deepEqual(calls, [
    {
      body: undefined,
      method: 'GET',
      url: '/api/app/update-log/paged?keyword=%E7%89%88%E6%9C%AC&maxResultCount=20&skipCount=0',
    },
    { body: JSON.stringify(input), method: 'POST', url: '/api/app/update-log' },
    { body: JSON.stringify(input), method: 'PUT', url: '/api/app/update-log/update%2F1' },
    { body: undefined, method: 'DELETE', url: '/api/app/update-log/update-1' },
  ]);
});

test('validates update-log fields, converts local time to UTC, and maps only displayed fields', () => {
  const draft = createUpdateLogDraft(new Date('2026-10-13T08:00:00Z'));
  Object.assign(draft, { impactScope: ' 全部用户 ', summary: ' 更新内容 ', title: ' 版本更新 ', version: ' 1.2.3 ' });
  assert.equal(validateUpdateLogDraft(draft), null);
  const input = toUpdateLogSaveInput(draft);
  assert.equal(input.publishedAt, new Date(draft.publishedAt).toISOString());
  assert.equal(input.impactScope, '全部用户');
  assert.equal(input.title, '版本更新');

  draft.title = ' ';
  assert.equal(validateUpdateLogDraft(draft), '请输入更新标题。');
  const editDraft = toUpdateLogDraft(updateLog);
  assert.equal(editDraft.title, updateLog.title);
  const row = toUpdateLogRow(updateLog);
  assert.equal(row.authorName, '管理员');
  assert.equal('creationTime' in row, false);

  const sqliteTimestamp = '2026-10-13T09:48:00';
  assert.equal(parseUpdateLogTime(sqliteTimestamp).toISOString(), '2026-10-13T09:48:00.000Z');
  assert.equal(
    new Date(toUpdateLogDraft({ ...updateLog, publishedAt: sqliteTimestamp }).publishedAt).toISOString(),
    '2026-10-13T09:48:00.000Z',
  );
});

test('maps the recycle-bin permission and permits authenticated update-log reads without an extra permission code', () => {
  const menu = [
    {
      name: 'PlatformCenter',
      path: '/platform',
      component: 'BasicLayout',
      children: [
        { name: 'PlatformRecycleBin', path: '/platform/recycle-bin', component: '/platform/recycle-bin/index' },
        { name: 'PlatformUpdateLogs', path: '/platform/update-logs', component: '/platform/update-logs/index' },
      ],
    },
  ];

  const noManagePermission = getResolvedPages(adaptBackendMenuTree(menu, []));
  assert.deepEqual(
    noManagePermission.map((page) => page.routeName),
    ['platform-update-logs'],
  );
  const withRecyclePermission = getResolvedPages(adaptBackendMenuTree(menu, ['Platform.RecycleBin.Manage']));
  assert.deepEqual(
    withRecyclePermission.map((page) => page.routeName).toSorted(),
    ['platform-recycle-bin', 'platform-update-logs'].toSorted(),
  );
});
