import assert from 'node:assert/strict';
import { afterEach, test } from 'node:test';
import { clearAccessToken, hasCurrentTenantScope, setAccessToken } from '../src/session/token-storage.ts';

function createToken(claims) {
  return `header.${Buffer.from(JSON.stringify(claims)).toString('base64url')}.signature`;
}

afterEach(() => clearAccessToken());

test('recognizes a tenant-scoped token without returning its tenant identifier', () => {
  setAccessToken(createToken({ tenantid: 'tenant-id' }));
  assert.equal(hasCurrentTenantScope(), true);
});

test('does not classify host-scoped or invalid tokens as tenant-scoped', () => {
  setAccessToken(createToken({ sub: 'admin-id' }));
  assert.equal(hasCurrentTenantScope(), false);
  setAccessToken('invalid-token');
  assert.equal(hasCurrentTenantScope(), false);
});
