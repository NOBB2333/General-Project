import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      server: {
        proxy: {
          '/api': {
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            // 当前开发阶段先沿用模板 mock 服务。
            // 后续切换到 ABP 后端时，可以改成 https://localhost:44392 并移除 rewrite。
            target: 'http://localhost:5320/api',
            ws: true,
          },
        },
      },
    },
  };
});
