import type { DefaultTheme } from 'vitepress';

import { defineConfig } from 'vitepress';

import { version } from '../../../../package.json';

export const zh = defineConfig({
  description: 'General Admin 企业级平台与项目经营管理系统',
  lang: 'zh-Hans',
  themeConfig: {
    darkModeSwitchLabel: '主题',
    darkModeSwitchTitle: '切换到深色模式',
    docFooter: {
      next: '下一页',
      prev: '上一页',
    },
    footer: {
      copyright: `Copyright © 2026-${new Date().getFullYear()} General Admin`,
      message: '面向企业平台治理、项目执行与经营分析的管理系统.',
    },
    langMenuLabel: '多语言',
    lastUpdated: {
      formatOptions: {
        dateStyle: 'short',
        timeStyle: 'medium',
      },
      text: '最后更新于',
    },
    lightModeSwitchTitle: '切换到浅色模式',
    nav: nav(),
    outline: {
      label: '页面导航',
    },
    returnToTopLabel: '回到顶部',
    sidebar: {
      '/architecture/': {
        base: '/architecture/',
        items: sidebarArchitecture(),
      },
      '/business/': {
        base: '/business/',
        items: sidebarBusiness(),
      },
      '/guide/': {
        base: '/guide/',
        items: sidebarGuide(),
      },
      '/operations/': {
        base: '/operations/',
        items: sidebarOperations(),
      },
      '/platform/': {
        base: '/platform/',
        items: sidebarPlatform(),
      },
    },
    sidebarMenuLabel: '菜单',
  },
});

function nav(): DefaultTheme.NavItem[] {
  return [
    {
      activeMatch: '^/guide/',
      link: '/guide/introduction',
      text: '指南',
    },
    {
      activeMatch: '^/platform/',
      link: '/platform/tenant-and-permission',
      text: '平台治理',
    },
    {
      activeMatch: '^/business/',
      link: '/business/project-execution',
      text: '业务模块',
    },
    {
      activeMatch: '^/(architecture|operations)/',
      text: '工程',
      items: [
        { link: '/architecture/system-overview', text: '系统架构' },
        { link: '/operations/deployment', text: '部署运维' },
      ],
    },
    {
      text: version,
      items: [
        { link: '/guide/quick-start', text: '快速开始' },
        { link: '/operations/nucbox', text: 'NUC 部署' },
      ],
    },
  ];
}

function sidebarGuide(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '开始',
      items: [
        { link: 'introduction', text: '产品介绍' },
        { link: 'quick-start', text: '快速开始' },
        { link: 'configuration', text: '配置文件' },
        { link: 'module-map', text: '模块地图' },
      ],
    },
  ];
}

function sidebarPlatform(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '平台治理',
      items: [
        { link: 'tenant-and-permission', text: '租户与权限' },
        { link: 'platform-capabilities', text: '平台功能' },
        { link: 'host-operation', text: 'Host 运维租户' },
        { link: 'audit', text: '审计链路' },
        { link: 'scheduler-monitoring', text: '定时任务与监控' },
      ],
    },
  ];
}

function sidebarBusiness(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '业务模块',
      items: [
        { link: 'project-execution', text: '项目执行' },
        { link: 'business-overview', text: '经营总览' },
        { link: 'organization-scope', text: '组织与数据范围' },
      ],
    },
  ];
}

function sidebarArchitecture(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '架构',
      items: [
        { link: 'system-overview', text: '系统架构' },
        { link: 'frontend', text: '前端工程' },
        { link: 'backend', text: '后端工程' },
      ],
    },
  ];
}

function sidebarOperations(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '部署运维',
      items: [
        { link: 'deployment', text: '构建与发布' },
        { link: 'nucbox', text: 'NUC 部署' },
        { link: 'troubleshooting', text: '排障手册' },
      ],
    },
  ];
}

export const search: DefaultTheme.AlgoliaSearchOptions['locales'] = {
  root: {
    placeholder: '搜索 General Admin 文档',
    translations: {
      button: {
        buttonAriaLabel: '搜索文档',
        buttonText: '搜索文档',
      },
      modal: {
        errorScreen: {
          helpText: '请检查网络连接或稍后重试',
          titleText: '无法获取结果',
        },
        footer: {
          closeText: '关闭',
          navigateText: '切换',
          searchByText: '搜索提供者',
          selectText: '选择',
        },
        noResultsScreen: {
          noResultsText: '没有找到相关结果',
          suggestedQueryText: '你可以尝试查询',
        },
        searchBox: {
          cancelButtonAriaLabel: '取消',
          cancelButtonText: '取消',
          resetButtonAriaLabel: '清除查询条件',
          resetButtonTitle: '清除查询条件',
        },
        startScreen: {
          noRecentSearchesText: '没有搜索历史',
          recentSearchesTitle: '搜索历史',
        },
      },
    },
  },
};
