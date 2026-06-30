import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      optimizeDeps: {
        include: ['@flyfish-group/file-viewer3'],
      },
      server: {
        proxy: {
          '/api': {
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            secure: false,
            target: 'https://localhost:44392/api',
            ws: true,
          },
        },
      },
    },
  };
});
