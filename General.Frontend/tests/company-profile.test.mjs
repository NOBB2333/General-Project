import assert from 'node:assert/strict';
import test from 'node:test';
import { createApiClient } from '../src/api/client.ts';
import { createCompanyProfileApi } from '../src/api/core/company-profile.ts';

const profile = {
  address: '示例路 1 号',
  city: '杭州',
  creditCode: '91330100MA12345678',
  email: 'office@example.com',
  fullName: '示例科技有限公司',
  hasLogo: true,
  industry: '软件',
  introduction: '企业简介',
  legalPerson: '张三',
  phone: '0571-12345678',
  platformName: '项目管理平台',
  province: '浙江省',
  scale: '100-499人',
  shortName: '示例科技',
  slogan: '协同交付',
  website: 'https://example.com',
};

function createApi(fetcher) {
  return createCompanyProfileApi(createApiClient({ baseUrl: '/api', fetcher }));
}

test('reads tenant company profile from the profile endpoint', async () => {
  const calls = [];
  const api = createApi(async (url, init) => {
    calls.push({ method: init.method, url: String(url) });
    return Response.json({ code: 0, data: profile });
  });

  assert.deepEqual(await api.get(), profile);
  assert.deepEqual(calls, [{ method: 'GET', url: '/api/app/platform/company-profile' }]);
});

test('updates profile fields with the matching JSON contract', async () => {
  let request;
  const api = createApi(async (url, init) => {
    request = { body: init.body, method: init.method, url: String(url) };
    return Response.json({ code: 0, data: true });
  });
  const { hasLogo: _hasLogo, ...input } = profile;

  assert.equal(await api.update(input), true);
  assert.deepEqual(request, {
    body: JSON.stringify(input),
    method: 'PUT',
    url: '/api/app/platform/company-profile',
  });
});

test('uploads a logo as multipart form data and reads it as an image blob', async () => {
  const calls = [];
  const api = createApi(async (url, init) => {
    calls.push({ body: init.body, headers: init.headers, method: init.method, url: String(url) });
    if (init.method === 'POST') return Response.json({ code: 0, data: true });
    return new Response(new Uint8Array([137, 80, 78, 71]), {
      headers: { 'Content-Type': 'image/png' },
    });
  });
  const logo = new File([new Uint8Array([137, 80, 78, 71])], 'company.png', { type: 'image/png' });

  assert.equal(await api.uploadLogo(logo), true);
  const downloadedLogo = await api.getLogo();

  assert.equal(downloadedLogo?.type, 'image/png');
  assert.equal(calls[0].url, '/api/app/platform/company-profile/logo');
  assert.equal(calls[0].method, 'POST');
  assert.equal(calls[0].body instanceof FormData, true);
  assert.equal(calls[0].body.get('file').name, 'company.png');
  assert.equal(new Headers(calls[0].headers).has('Content-Type'), false);
  assert.equal(calls[1].url, '/api/app/platform/company-profile/logo');
  assert.equal(calls[1].method, 'GET');
});
