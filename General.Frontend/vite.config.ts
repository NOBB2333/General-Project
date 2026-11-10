import { cwd } from 'node:process';
import { fileURLToPath, URL } from 'node:url';
import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';

function normalizeAssetBase(value: string | undefined): string {
  const base = value?.trim() || '/';
  return base.endsWith('/') ? base : `${base}/`;
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, cwd(), '');
  const proxyTarget = env.VITE_API_PROXY_TARGET?.trim();
  // 与旧前端一致，允许本机 .NET 开发证书；远端 HTTPS 仍校验证书。
  const isLocalProxyTarget = proxyTarget
    ? ['localhost', '127.0.0.1', '[::1]'].includes(new URL(proxyTarget).hostname)
    : false;

  if (mode === 'development' && !proxyTarget) {
    console.warn('未配置 VITE_API_PROXY_TARGET：前端可以运行，但 /api 请求不会转发到后端。');
  }

  return {
    base: normalizeAssetBase(env.VITE_ASSET_BASE),
    plugins: [vue()],
    resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } },
    server: {
      host: '0.0.0.0',
      port: 5011,
      strictPort: true,
      ...(proxyTarget
        ? {
            proxy: {
              '/api': { target: proxyTarget, changeOrigin: true, secure: !isLocalProxyTarget },
              '/health': { target: proxyTarget, changeOrigin: true, secure: !isLocalProxyTarget },
            },
          }
        : {}),
    },
    preview: {
      host: '0.0.0.0',
      port: 5011,
      strictPort: true,
      proxy: {
        '/api': { target: 'http://127.0.0.1:5010', changeOrigin: true },
        '/health': { target: 'http://127.0.0.1:5010', changeOrigin: true },
      },
    },
  };
});
