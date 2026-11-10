import assert from 'node:assert/strict';
import test from 'node:test';
import { setTimeout as sleep } from 'node:timers/promises';

import { ApiRequestError, createApiClient } from '../src/api/client.ts';

test('unwraps a successful ApiResponse and sends query plus Bearer token', async () => {
  let requestUrl = '';
  let requestHeaders;
  const client = createApiClient({
    baseUrl: '/api/',
    fetcher: async (input, init) => {
      requestUrl = String(input);
      requestHeaders = new Headers(init?.headers);
      return Response.json({ code: 0, data: { totalCount: 0, items: [] }, error: null, message: 'ok' });
    },
    getAccessToken: () => 'local-test-token',
  });

  const result = await client.request('/app/user/list', {
    query: { keyword: '王 五', page: 1, ignored: null, active: false },
  });

  assert.deepEqual(result, { totalCount: 0, items: [] });
  assert.equal(requestUrl, '/api/app/user/list?keyword=%E7%8E%8B+%E4%BA%94&page=1&active=false');
  assert.equal(requestHeaders.get('Authorization'), 'Bearer local-test-token');
  assert.equal(requestHeaders.get('Accept'), 'application/json');
});

test('throws for an application failure returned with HTTP 200', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => Response.json({ code: -1, data: null, error: '业务操作失败', message: '业务操作失败' }),
  });

  await assert.rejects(client.request('/app/example'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 200);
    assert.equal(error.code, -1);
    assert.equal(error.message, '业务操作失败');
    return true;
  });
});

test('preserves the login failure message from a 403 ApiResponse', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () =>
      new Response(
        JSON.stringify({ code: -1, data: null, error: '用户名或密码错误。', message: '用户名或密码错误。' }),
        { status: 403, headers: { 'Content-Type': 'application/json' } },
      ),
  });

  await assert.rejects(client.request('/app/auth/login', { anonymous: true, method: 'POST', body: {} }), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 403);
    assert.equal(error.message, '用户名或密码错误。');
    return true;
  });
});

test('extracts ABP validation errors and member names from HTTP 400', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () =>
      Response.json(
        {
          error: {
            code: 'Volo.Abp.Validation.ValidationException',
            message: 'Volo.Abp.BusinessException: validation failed',
            validationErrors: [{ message: '用户名不能为空。', members: ['Username'] }],
          },
        },
        { status: 400 },
      ),
  });

  await assert.rejects(client.request('/app/user', { method: 'POST', body: {} }), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 400);
    assert.equal(error.message, '用户名不能为空。');
    assert.deepEqual(error.validationErrors, [{ message: '用户名不能为空。', members: ['Username'] }]);
    return true;
  });
});

test('returns a safe status message for HTTP 401 without exposing a response body', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => new Response('<private gateway response>', { status: 401 }),
  });

  await assert.rejects(client.request('/app/user/info'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 401);
    assert.equal(error.message, '登录已失效，请重新登录。');
    return true;
  });
});

test('coordinates concurrent 401 responses once for the same session', async () => {
  let sessionVersion = 1;
  let unauthorizedCalls = 0;
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => new Response('', { status: 401 }),
    getAccessToken: () => 'expired-token',
    getSessionVersion: () => sessionVersion,
    onUnauthorized: () => {
      unauthorizedCalls += 1;
      sessionVersion += 1;
    },
  });

  const results = await Promise.allSettled(Array.from({ length: 8 }, () => client.request('/app/user/info')));

  assert.equal(results.filter((result) => result.status === 'rejected').length, 8);
  assert.equal(unauthorizedCalls, 1);
});

test('does not invalidate a newer session when an older request returns 401', async () => {
  let sessionVersion = 4;
  let unauthorizedCalls = 0;
  let releaseResponse;
  const responsePending = new Promise((resolve) => {
    releaseResponse = resolve;
  });
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => responsePending,
    getAccessToken: () => 'old-token',
    getSessionVersion: () => sessionVersion,
    onUnauthorized: () => {
      unauthorizedCalls += 1;
    },
  });

  const request = client.request('/app/user/info');
  sessionVersion += 1;
  releaseResponse(new Response('', { status: 401 }));

  await assert.rejects(request, (error) => error instanceof ApiRequestError);
  assert.equal(unauthorizedCalls, 0);
});

test('uses the standard authorization message for a non-login HTTP 403', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () =>
      Response.json(
        { error: { code: 'Volo.Abp.Authorization:AbpAuthorizationException', message: 'Forbidden' } },
        { status: 403 },
      ),
  });

  await assert.rejects(client.request('/app/platform/menu/tree'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 403);
    assert.equal(error.message, '当前账号无权执行此操作。');
    return true;
  });
});

test('surfaces the public business message carried in an ABP error code', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () =>
      Response.json(
        {
          error: {
            code: '项目收尾或归档前必须维护关键里程碑。',
            message: 'An internal error occurred during your request!',
            details: null,
            data: {},
            validationErrors: null,
          },
        },
        { status: 403 },
      ),
  });

  await assert.rejects(client.request('/app/project/example/status', { method: 'PUT', body: {} }), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, 403);
    assert.equal(error.message, '项目收尾或归档前必须维护关键里程碑。');
    return true;
  });
});

test('reports malformed successful JSON as a protocol error', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => new Response('<html>gateway</html>', { status: 200 }),
  });

  await assert.rejects(client.request('/app/user/info'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.message, '服务端返回了无法识别的响应格式。');
    return true;
  });
});

test('wraps network failures without leaking the underlying error message', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      throw new Error('private network detail');
    },
  });

  await assert.rejects(client.request('/app/user/info'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.status, undefined);
    assert.equal(error.message, '网络请求失败，请检查连接后重试。');
    return true;
  });
});

test('reports an AbortSignal timeout distinctly from a network failure', async () => {
  const timeoutError = new DOMException('timed out', 'TimeoutError');
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      throw timeoutError;
    },
  });

  await assert.rejects(client.request('/app/user/info'), (error) => {
    assert.ok(error instanceof ApiRequestError);
    assert.equal(error.message, '请求超时，请稍后重试。');
    return true;
  });
});

test('passes AbortError through without converting it to a network error', async () => {
  const abortError = new DOMException('aborted', 'AbortError');
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      throw abortError;
    },
  });

  await assert.rejects(client.request('/app/user/info'), (error) => error === abortError);
});

test('does not set multipart Content-Type and returns file responses as Blob', async () => {
  let uploadHeaders;
  let uploadBody;
  const uploadClient = createApiClient({
    baseUrl: '/api',
    fetcher: async (_input, init) => {
      uploadHeaders = new Headers(init?.headers);
      uploadBody = init?.body;
      return Response.json({ code: 0, data: true, error: null, message: 'ok' });
    },
  });
  const formData = new FormData();
  formData.set('file', new Blob(['file contents']), 'report.txt');

  assert.equal(await uploadClient.request('/app/file/upload', { method: 'POST', body: formData }), true);
  assert.equal(uploadBody, formData);
  assert.equal(uploadHeaders.has('Content-Type'), false);

  const fileClient = createApiClient({
    baseUrl: '/api',
    fetcher: async () => new Response('downloaded bytes', { headers: { 'Content-Type': 'application/octet-stream' } }),
  });
  const blob = await fileClient.request('/app/file/download/key', { responseType: 'blob' });
  assert.ok(blob instanceof Blob);
  assert.equal(await blob.text(), 'downloaded bytes');
});

test('does not retry a failed write request', async () => {
  let calls = 0;
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      calls += 1;
      return new Response('gateway unavailable', { status: 503 });
    },
  });

  await assert.rejects(client.request('/app/user', { method: 'POST', body: { userName: 'test' } }));
  assert.equal(calls, 1);
});

test('removes an explicit Authorization header for anonymous requests', async () => {
  let headers;
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (_input, init) => {
      headers = new Headers(init?.headers);
      return Response.json({ code: 0, data: true, error: null, message: 'ok' });
    },
  });

  await client.request('/app/auth/login', {
    anonymous: true,
    headers: { Authorization: 'Bearer stale-token' },
  });
  assert.equal(headers.has('Authorization'), false);
});

test('rejects absolute API URLs to prevent sending credentials outside the configured base', async () => {
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      throw new Error('fetch should not be called');
    },
  });

  await assert.rejects(client.request('https://example.com/collect-token'), TypeError);
});

for (const stage of ['headers', 'json', 'blob']) {
  test(`bounds stalled ${stage} responses without retrying the request`, async () => {
    let calls = 0;
    const client = createApiClient({
      baseUrl: '/api',
      timeoutMs: 20,
      fetcher: async (_input, init) => {
        calls += 1;
        if (stage === 'headers')
          return new Promise((_resolve, reject) => {
            init.signal.addEventListener('abort', () => reject(init.signal.reason), { once: true });
          });
        return new Response(
          new ReadableStream({
            start(controller) {
              init.signal.addEventListener('abort', () => controller.error(init.signal.reason), { once: true });
            },
          }),
        );
      },
    });
    await Promise.all([
      assert.rejects(
        client.request('/app/example', { responseType: stage === 'blob' ? 'blob' : 'json' }),
        (error) => error instanceof ApiRequestError && error.message === '请求超时，请稍后重试。',
      ),
      sleep(80),
    ]);
    assert.equal(calls, 1);
  });
}

test('preserves caller cancellation when combining it with the request deadline', async () => {
  const controller = new AbortController();
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async (_input, init) =>
      new Promise((_resolve, reject) => {
        init.signal.addEventListener('abort', () => reject(init.signal.reason), { once: true });
      }),
  });
  const request = client.request('/app/example', { signal: controller.signal });
  controller.abort();
  await assert.rejects(request, (error) => error === controller.signal.reason && error.name === 'AbortError');
});

test('rejects invalid deadlines before dispatching any HTTP request', async () => {
  let calls = 0;
  const client = createApiClient({
    baseUrl: '/api',
    fetcher: async () => {
      calls += 1;
      return Response.json(true);
    },
  });
  for (const timeoutMs of [0, -1, 1.5, NaN, Infinity, 2147483648])
    await assert.rejects(client.request('/app/example', { timeoutMs }), TypeError);
  assert.equal(calls, 0);
});
