function normalizePathBase(value: string | undefined): string {
  const path = value?.trim() || '/';
  if (/^https?:\/\//i.test(path)) {
    return '/';
  }

  const normalized = path.replace(/^\/+|\/+$/g, '');
  return normalized ? `/${normalized}/` : '/';
}

function normalizeApiBase(value: string | undefined): string {
  const base = value?.trim() || '';
  if (!base) {
    return '';
  }

  if (/^https?:\/\//i.test(base)) {
    return base.replace(/\/+$/g, '');
  }

  return `/${base.replace(/^\/+|\/+$/g, '')}`;
}

const rawApiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const rawProxyTarget = import.meta.env.VITE_API_PROXY_TARGET;

export const appEnvironment = Object.freeze({
  appName: import.meta.env.VITE_APP_NAME?.trim() || 'General Admin',
  assetBase: import.meta.env.BASE_URL,
  routerBase: normalizePathBase(import.meta.env.VITE_ROUTER_BASE),
  apiBaseUrl: normalizeApiBase(rawApiBaseUrl),
  isApiBaseConfigured: Boolean(rawApiBaseUrl?.trim()),
  isDevelopment: import.meta.env.DEV,
  isDevelopmentProxyConfigured: !import.meta.env.DEV || Boolean(rawProxyTarget?.trim()),
});

export function getEnvironmentWarning(): string | undefined {
  if (!appEnvironment.isApiBaseConfigured) {
    return '接口地址尚未配置。请复制 .env.example 为 .env.local 并设置 VITE_API_BASE_URL；当前页面不会发送接口请求。';
  }

  if (!appEnvironment.isDevelopmentProxyConfigured) {
    return '开发代理地址尚未配置。请在 .env.local 中设置 VITE_API_PROXY_TARGET；当前页面不会发送接口请求。';
  }

  return undefined;
}
