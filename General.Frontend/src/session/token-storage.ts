const accessTokenKey = 'general-admin.access-token';

let cachedAccessToken: string | null = null;
let storageRead = false;
let sessionVersion = 0;

function getSessionStorage(): Storage | null {
  if (typeof window === 'undefined') return null;
  try {
    return window.sessionStorage;
  } catch {
    return null;
  }
}

export function getAccessToken(): string | null {
  if (storageRead) return cachedAccessToken;
  storageRead = true;
  try {
    cachedAccessToken = getSessionStorage()?.getItem(accessTokenKey) ?? null;
  } catch {
    cachedAccessToken = null;
  }
  return cachedAccessToken;
}

export function hasCurrentTenantScope(): boolean {
  const payloadSegment = getAccessToken()?.split('.')[1];
  if (!payloadSegment) return false;

  try {
    const base64 = payloadSegment.replaceAll('-', '+').replaceAll('_', '/');
    const paddedBase64 = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const claims = JSON.parse(atob(paddedBase64)) as Record<string, unknown>;
    return typeof claims.tenantid === 'string' && claims.tenantid.trim().length > 0;
  } catch {
    return false;
  }
}

export function setAccessToken(value: string): void {
  const token = value.trim();
  if (!token) throw new TypeError('登录响应缺少 accessToken。');

  storageRead = true;
  cachedAccessToken = token;
  sessionVersion += 1;
  try {
    getSessionStorage()?.setItem(accessTokenKey, token);
  } catch {
    // Keep the token in memory for this page when browser storage is unavailable.
  }
}

export function clearAccessToken(): void {
  storageRead = true;
  cachedAccessToken = null;
  sessionVersion += 1;
  try {
    getSessionStorage()?.removeItem(accessTokenKey);
  } catch {
    // Memory state is still cleared if browser storage is unavailable.
  }
}

export function getSessionVersion(): number {
  return sessionVersion;
}
