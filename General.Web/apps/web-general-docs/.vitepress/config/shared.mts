import type { PwaOptions } from '@vite-pwa/vitepress';
import type { HeadConfig } from 'vitepress';

import { resolve } from 'node:path';

import {
  viteArchiverPlugin,
  viteVxeTableImportsPlugin,
} from '@vben/vite-config';

import {
  GitChangelog,
  GitChangelogMarkdownSection,
} from '@nolebase/vitepress-plugin-git-changelog/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig, postcssIsolateStyles } from 'vitepress';
import {
  groupIconMdPlugin,
  groupIconVitePlugin,
} from 'vitepress-plugin-group-icons';

import { demoPreviewPlugin } from './plugins/demo-preview';
import { search as zhSearch } from './zh.mts';

export const shared = defineConfig({
  appearance: 'dark',
  head: head(),
  markdown: {
    preConfig(md) {
      md.use(demoPreviewPlugin);
      md.use(groupIconMdPlugin);
    },
  },
  pwa: pwa(),
  srcDir: 'src',
  themeConfig: {
    i18nRouting: true,
    logo: '/favicon.ico',
    search: {
      options: {
        locales: {
          ...zhSearch,
        },
      },
      provider: 'local',
    },
    siteTitle: 'General Admin',
    socialLinks: [],
  },
  title: 'General Admin',
  vite: {
    build: {
      chunkSizeWarningLimit: Infinity,
      minify: 'terser',
    },
    css: {
      postcss: {
        plugins: [postcssIsolateStyles({ includeFiles: [/vp-doc\.css/] })],
      },
      preprocessorOptions: {
        scss: {
          api: 'modern',
        },
      },
    },
    json: {
      stringify: true,
    },
    plugins: [
      tailwindcss(),
      GitChangelog({
        mapAuthors: [
          {
            mapByNameAliases: ['Vben'],
            name: 'vben',
            username: 'anncwb',
          },
          {
            name: 'vince',
            username: 'vince292007',
          },
          {
            name: 'Li Kui',
            username: 'likui628',
          },
        ],
        repoURL: () => 'https://general-admin.local/General-admin',
      }),
      GitChangelogMarkdownSection(),
      viteArchiverPlugin({ outputDir: '.vitepress' }),
      groupIconVitePlugin(),
      await viteVxeTableImportsPlugin(),
    ],
    server: {
      fs: {
        allow: ['../..'],
      },
      host: true,
      port: 6173,
    },

    ssr: {
      external: ['@vue/repl'],
    },
  },
});

function head(): HeadConfig[] {
  return [
    ['meta', { content: 'General Admin Team', name: 'author' }],
    [
      'meta',
      {
        content: 'general admin, abp, dotnet, vue, vitepress',
        name: 'keywords',
      },
    ],
    ['link', { href: '/favicon.ico', rel: 'icon', type: 'image/svg+xml' }],
    [
      'meta',
      {
        content:
          'width=device-width,initial-scale=1,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no',
        name: 'viewport',
      },
    ],
    ['meta', { content: 'general admin docs', name: 'keywords' }],
    ['link', { href: '/favicon.ico', rel: 'icon' }],
    // [
    //   'script',
    //   {
    //     src: 'https://cdn.tailwindcss.com',
    //   },
    // ],
  ];
}

function pwa(): PwaOptions {
  return {
    includeManifestIcons: false,
    manifest: {
        description:
        'General Admin is an enterprise platform governance, project delivery, and business analytics system.',
      icons: [
        {
          sizes: '192x192',
          src: '/favicon.ico',
          type: 'image/png',
        },
        {
          sizes: '512x512',
          src: '/favicon.ico',
          type: 'image/png',
        },
      ],
      id: '/',
      name: 'General Admin Docs',
      short_name: 'general_admin_docs',
      theme_color: '#ffffff',
    },
    outDir: resolve(process.cwd(), '.vitepress/dist'),
    registerType: 'autoUpdate',
    workbox: {
      globPatterns: ['**/*.{css,js,html,svg,png,ico,txt,woff2}'],
      maximumFileSizeToCacheInBytes: 5 * 1024 * 1024,
    },
  };
}
