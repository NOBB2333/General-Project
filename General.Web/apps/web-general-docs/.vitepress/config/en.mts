import type { DefaultTheme } from 'vitepress';

import { defineConfig } from 'vitepress';

import { version } from '../../../../package.json';

export const en = defineConfig({
  description: 'General Admin enterprise platform documentation',
  lang: 'en-US',
  themeConfig: {
    darkModeSwitchLabel: 'Theme',
    darkModeSwitchTitle: 'Switch to Dark Mode',
    docFooter: {
      next: 'Next Page',
      prev: 'Previous Page',
    },
    footer: {
      copyright: `Copyright © 2026-${new Date().getFullYear()} General Admin`,
      message: 'Enterprise platform governance, project delivery, and business analytics.',
    },
    langMenuLabel: 'Language',
    lastUpdated: {
      formatOptions: {
        dateStyle: 'short',
        timeStyle: 'medium',
      },
      text: 'Last updated on',
    },
    lightModeSwitchTitle: 'Switch to Light Mode',
    nav: nav(),
    outline: {
      label: 'Navigate',
    },
    returnToTopLabel: 'Back to top',
    sidebar: {
      '/en/guide/': { base: '/en/guide/', items: sidebarGuide() },
    },
  },
});

function nav(): DefaultTheme.NavItem[] {
  return [
    {
      activeMatch: '^/en/guide/',
      link: '/en/guide/introduction',
      text: 'Guide',
    },
    {
      link: '/',
      text: '中文',
    },
    {
      text: version,
      items: [{ link: '/en/guide/introduction', text: 'Overview' }],
    },
  ];
}

function sidebarGuide(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: 'General Admin',
      items: [
        { link: 'introduction', text: 'Introduction' },
      ],
    },
  ];
}
