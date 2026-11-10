import assert from 'node:assert/strict';
import test from 'node:test';

import { createApiClient, ApiRequestError } from '../src/api/client.ts';
import { createPlatformFilesApi } from '../src/features/platform/files/platform-files.api.ts';
import {
  formatFileSize,
  isSafePreviewContentType,
  sanitizeDownloadFileName,
  toPlatformFileRow,
} from '../src/features/platform/files/platform-files.model.ts';

test('requests server-side file pagination and search with the actual query names', async () => {
  let requestUrl = '';
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (input) => {
      requestUrl = String(input);
      return Response.json({ code: 0, data: { items: [], totalCount: 0 }, error: null, message: 'ok' });
    },
  });
  const api = createPlatformFilesApi(client);

  const result = await api.getList({ keyword: '项目文件', maxResultCount: 20, skipCount: 40 });

  assert.deepEqual(result, { items: [], totalCount: 0 });
  assert.equal(
    requestUrl,
    '/api/app/file/list?keyword=%E9%A1%B9%E7%9B%AE%E6%96%87%E4%BB%B6&maxResultCount=20&skipCount=40',
  );
});

test('uploads multipart fields without setting a manual content type', async () => {
  let requestHeaders;
  let requestBody;
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (_input, init) => {
      requestHeaders = new Headers(init?.headers);
      requestBody = init?.body;
      return Response.json({ code: 0, data: { id: 'file-id' }, error: null, message: 'ok' });
    },
  });
  const api = createPlatformFilesApi(client);
  const file = new File(['hello'], 'hello.txt', { type: 'text/plain' });

  const result = await api.upload({
    file,
    category: 'Project/Files',
    businessType: 'Project',
    businessId: 'project-id',
  });

  assert.equal(result.id, 'file-id');
  assert.ok(requestBody instanceof FormData);
  assert.equal(requestBody.get('File').name, 'hello.txt');
  assert.equal(requestBody.get('Category'), 'Project/Files');
  assert.equal(requestBody.get('BusinessType'), 'Project');
  assert.equal(requestBody.get('BusinessId'), 'project-id');
  assert.equal(requestHeaders.has('Content-Type'), false);
});

test('requests authenticated binary endpoints with encoded file keys', async () => {
  let requestUrl = '';
  let requestHeaders;
  const client = createApiClient({
    baseUrl: '/api',
    getAccessToken: () => 'local-file-test-token',
    fetcher: async (input, init) => {
      requestUrl = String(input);
      requestHeaders = new Headers(init?.headers);
      return new Response(new Blob(['safe file'], { type: 'application/pdf' }), {
        headers: { 'Content-Disposition': 'inline; filename="safe.pdf"', 'Content-Type': 'application/pdf' },
      });
    },
  });
  const api = createPlatformFilesApi(client);

  const result = await api.getFileContent('folder/a b', 'preview');

  assert.equal(requestUrl, '/api/app/file/preview/folder%2Fa%20b');
  assert.equal(requestHeaders.get('Authorization'), 'Bearer local-file-test-token');
  assert.equal(requestHeaders.get('Accept'), 'application/json');
  assert.equal(await result.text(), 'safe file');
});

test('does not return HTTP or ApiResponse errors as downloadable file blobs', async () => {
  const forbiddenApi = createPlatformFilesApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async () =>
        Response.json({ error: { code: 'AbpAuthorizationException', message: 'Forbidden' } }, { status: 403 }),
    }),
  );
  await assert.rejects(forbiddenApi.getFileContent('forbidden', 'download'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 403);
    return true;
  });

  const applicationErrorApi = createPlatformFilesApi(
    createApiClient({
      baseUrl: '/api',
      fetcher: async () => Response.json({ code: -1, data: null, error: '文件读取失败。', message: '文件读取失败。' }),
    }),
  );
  await assert.rejects(applicationErrorApi.getFileContent('failure', 'download'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 200);
    assert.equal(error.message, '文件读取失败。');
    return true;
  });
});

test('formats server file metadata and allows only inert preview content types', () => {
  assert.equal(formatFileSize(1024), '1.0 KB');
  assert.equal(formatFileSize(10 * 1024 * 1024), '10.0 MB');
  assert.equal(isSafePreviewContentType('application/pdf'), true);
  assert.equal(isSafePreviewContentType('image/svg+xml'), false);
  assert.equal(isSafePreviewContentType('text/html'), false);
  assert.equal(isSafePreviewContentType('text/javascript'), false);
  assert.equal(sanitizeDownloadFileName('../report.pdf'), '.._report.pdf');

  const row = toPlatformFileRow({
    businessId: null,
    businessJumpPath: null,
    businessJumpQuery: {},
    businessObjectCode: null,
    businessObjectName: null,
    businessObjectTypeName: null,
    businessProjectId: null,
    businessProjectName: '项目甲',
    businessType: 'Project',
    bucketName: null,
    category: 'Project/Files',
    contentType: 'application/pdf',
    fileKey: 'opaque-key',
    fileName: 'report.pdf',
    id: 'file-id',
    isCurrentVersion: true,
    isPublic: false,
    parentPath: null,
    previousVersionFileId: null,
    relativePath: 'Project/Files/report.pdf',
    replacedByFileId: null,
    size: 1024,
    storageLocation: '/private/path',
    storageProvider: 'Local',
    storageSourceId: null,
    storageSourceName: null,
    uploadedAt: '2026-10-13T00:00:00Z',
    uploadedBy: 'admin',
    versionNo: 2,
  });
  assert.equal(row.business, '项目甲');
  assert.equal(row.status, '当前版本');
  assert.equal('storageLocation' in row, false);
});
