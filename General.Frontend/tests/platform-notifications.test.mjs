import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient } from '../src/api/client.ts';
import { createPlatformNotificationsApi } from '../src/api/core/platform-notification.ts';
import {
  formatNotificationDate,
  resolveAuthorizedNotificationPath,
} from '../src/components/layout/notification-center.model.ts';

test('requests the notification list with the backend paging and unread fields', async () => {
  let requestUrl = '';
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (input) => {
      requestUrl = String(input);
      return Response.json({ code: 0, data: [], error: null, message: 'ok' });
    },
  });
  const api = createPlatformNotificationsApi(client);

  const result = await api.getList({ maxResultCount: 20, onlyUnread: true, skipCount: 40 });

  assert.deepEqual(result, []);
  assert.equal(requestUrl, '/api/app/platform/notification/list?maxResultCount=20&onlyUnread=true&skipCount=40');
});

test('maps notification mutations to the authenticated backend routes', async () => {
  const requests = [];
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (input, init) => {
      requests.push({ method: init?.method ?? 'GET', url: String(input) });
      const data = String(input).endsWith('/unread-count') ? { count: 2 } : true;
      return Response.json({ code: 0, data, error: null, message: 'ok' });
    },
  });
  const api = createPlatformNotificationsApi(client);

  await api.getUnreadCount();
  await api.markRead('notification/a b');
  await api.markAllRead();
  await api.remove('notification/a b');

  assert.deepEqual(requests, [
    { method: 'GET', url: '/api/app/platform/notification/unread-count' },
    { method: 'POST', url: '/api/app/platform/notification/notification%2Fa%20b/read' },
    { method: 'POST', url: '/api/app/platform/notification/read-all' },
    { method: 'DELETE', url: '/api/app/platform/notification/notification%2Fa%20b' },
  ]);
});

test('allows only exact authorized local notification destinations', () => {
  const authorizedPaths = ['/platform/files'];

  assert.equal(
    resolveAuthorizedNotificationPath(
      '/platform/files?category=project#list',
      authorizedPaths,
      'https://general.invalid',
    ),
    '/platform/files?category=project#list',
  );
  assert.equal(
    resolveAuthorizedNotificationPath(
      'https://evil.invalid/platform/files',
      authorizedPaths,
      'https://general.invalid',
    ),
    null,
  );
  assert.equal(
    resolveAuthorizedNotificationPath('//evil.invalid/platform/files', authorizedPaths, 'https://general.invalid'),
    null,
  );
  assert.equal(resolveAuthorizedNotificationPath('/platform/users', authorizedPaths, 'https://general.invalid'), null);
  assert.equal(resolveAuthorizedNotificationPath('/platform/files/', authorizedPaths, 'https://general.invalid'), null);
  assert.equal(resolveAuthorizedNotificationPath('platform/files', authorizedPaths, 'https://general.invalid'), null);
  assert.equal(resolveAuthorizedNotificationPath('/platform\\files', authorizedPaths, 'https://general.invalid'), null);
});

test('uses the server display date and has a safe fallback', () => {
  assert.equal(formatNotificationDate('not-a-date', '2026-10-13 09:30'), '2026-10-13 09:30');
  assert.equal(formatNotificationDate('not-a-date', ''), '时间未知');
});
