import { createRouter, createWebHistory } from 'vue-router';
import AppShell from '@/components/layout/AppShell.vue';
import { findPageRegistrationByRouteName } from '@/navigation/page-registry';
import { getAuthorizedHomePath, isRouteAuthorized } from '@/navigation/menu-store';
import { decideRouteAccess } from '@/navigation/route-policy';
import { getAccessToken } from '@/session/token-storage';
import { restoreSession, sessionState } from '@/session/session-store';
import { featureRoutes } from './modules';
import { routeNames } from './route.types';
import { appEnvironment } from '@/config/env';
import { apiClient } from '@/api/request';
import { createAuditLogApi } from '@/api/core/audit-log';
import { getAuthorizedPageByPath } from '@/navigation/menu-store';
import { createAuthorizedPageVisit } from '@/navigation/page-visit';

const auditLogApi = createAuditLogApi(apiClient);

export const router = createRouter({
  history: createWebHistory(appEnvironment.routerBase),
  routes: [
    {
      path: '/login',
      name: routeNames.login,
      component: () => import('@/features/auth/LoginPage.vue'),
      meta: { title: '登录' },
    },
    {
      path: '/403',
      name: routeNames.forbidden,
      component: () => import('@/features/system/RouteErrorPage.vue'),
      meta: { title: '无权访问' },
    },
    {
      path: '/404',
      name: routeNames.notFound,
      component: () => import('@/features/system/RouteErrorPage.vue'),
      meta: { title: '页面不存在' },
    },
    {
      path: '/',
      component: AppShell,
      children: [
        {
          path: 'platform/cache',
          name: routeNames.platformCache,
          component: () => import('@/features/platform/cache/PlatformCachePage.vue'),
          meta: {
            appView: 'feature',
            workspace: '缓存管理',
            title: '缓存管理',
            module: '平台中心',
            pageLabel: '缓存管理',
            feature: 'platform-cache',
          },
        },
        {
          path: 'platform/scheduler',
          name: routeNames.platformScheduler,
          component: () => import('@/features/platform/scheduler/PlatformSchedulerPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '定时任务',
            title: '定时任务',
            module: '平台中心',
            pageLabel: '定时任务',
            feature: 'platform-scheduler',
          },
        },
        {
          path: 'platform/system-monitor',
          name: routeNames.platformSystemMonitor,
          component: () => import('@/features/platform/monitoring/PlatformSystemMonitorPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '系统监控',
            title: '系统监控',
            module: '平台中心',
            pageLabel: '系统监控',
            feature: 'platform-system-monitor',
          },
        },
        {
          path: 'platform/configs',
          name: routeNames.platformConfigs,
          component: () => import('@/features/platform/configs/PlatformConfigsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '配置参数',
            title: '配置参数',
            module: '平台中心',
            pageLabel: '配置参数',
            feature: 'platform-configs',
          },
        },
        {
          path: 'platform/audit-logs',
          name: routeNames.platformAuditLogs,
          component: () => import('@/features/platform/audit-logs/PlatformAuditLogsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '日志中心',
            title: '日志中心',
            module: '平台中心',
            pageLabel: '日志中心',
            feature: 'platform-audit-logs',
          },
        },
        {
          path: 'platform/workspace',
          name: routeNames.platformWorkspace,
          component: () => import('@/features/workspace/PlatformWorkspacePage.vue'),
          meta: {
            appView: 'feature',
            workspace: '平台总览',
            title: '平台总览',
            module: '平台中心',
            pageLabel: '平台总览',
            feature: 'platform-workspace',
          },
        },
        {
          path: 'platform/users',
          name: routeNames.platformUsers,
          component: () => import('@/features/platform/users/PlatformUsersPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '用户管理',
            title: '用户管理',
            module: '平台中心',
            pageLabel: '用户管理',
            feature: 'platform-users',
          },
        },
        {
          path: 'platform/organization',
          name: routeNames.platformOrganization,
          component: () => import('@/features/platform/organizations/PlatformOrganizationsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '组织架构',
            title: '组织架构',
            module: '平台中心',
            pageLabel: '组织架构',
            feature: 'platform-organizations',
          },
        },
        {
          path: 'platform/roles',
          name: routeNames.platformRoles,
          component: () => import('@/features/platform/roles/PlatformRolesPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '角色权限',
            title: '角色权限',
            module: '平台中心',
            pageLabel: '角色权限',
            feature: 'platform-roles',
          },
        },
        {
          path: 'platform/menus',
          name: routeNames.platformMenus,
          component: () => import('@/features/platform/menus/PlatformMenusPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '菜单管理',
            title: '菜单管理',
            module: '平台中心',
            pageLabel: '菜单管理',
            feature: 'platform-menus',
          },
        },
        {
          path: 'platform/dicts',
          name: routeNames.platformDicts,
          component: () => import('@/features/platform/dicts/PlatformDictsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '字典管理',
            title: '字典管理',
            module: '平台中心',
            pageLabel: '字典管理',
            feature: 'platform-dicts',
          },
        },
        {
          path: 'platform/open-api',
          name: routeNames.platformOpenApi,
          component: () => import('@/features/platform/open-api/PlatformOpenApiPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '开放接口',
            title: '开放接口',
            module: '平台中心',
            pageLabel: '开放接口',
            feature: 'platform-open-api',
          },
        },
        {
          path: 'platform/files',
          name: routeNames.platformFiles,
          component: () => import('@/features/platform/files/PlatformFilesPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '文件管理',
            title: '文件管理',
            module: '平台中心',
            pageLabel: '文件管理',
            feature: 'platform-files',
          },
        },
        {
          path: 'platform/recycle-bin',
          name: routeNames.platformRecycleBin,
          component: () => import('@/features/platform/recycle-bin/PlatformRecycleBinPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '回收中心',
            title: '回收中心',
            module: '平台中心',
            pageLabel: '回收中心',
            feature: 'platform-recycle-bin',
          },
        },
        {
          path: 'platform/update-logs',
          name: routeNames.platformUpdateLogs,
          component: () => import('@/features/platform/update-logs/PlatformUpdateLogsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '更新日志',
            title: '更新日志',
            module: '平台中心',
            pageLabel: '更新日志',
            feature: 'platform-update-logs',
          },
        },
        {
          path: 'platform/tenants',
          name: routeNames.platformTenants,
          component: () => import('@/features/platform/tenants/PlatformTenantsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '租户管理',
            title: '租户管理',
            module: '平台中心',
            pageLabel: '租户管理',
            feature: 'platform-tenants',
          },
        },
        {
          path: 'integration/connectors',
          name: routeNames.integrationConnectors,
          component: () => import('@/features/integration/connectors/IntegrationConnectorsPage.vue'),
          meta: {
            appView: 'feature',
            workspace: '连接器配置',
            title: '连接器配置',
            module: '外部集成',
            pageLabel: '连接器配置',
            feature: 'integration-connectors',
          },
        },
        {
          path: 'integration/bindings',
          name: routeNames.integrationBindings,
          component: () => import('@/features/integration/bindings/IntegrationBindingsPage.vue'),
          props: { initialTab: 'accounts' },
          meta: {
            appView: 'feature',
            workspace: '账号绑定',
            title: '账号绑定',
            module: '外部集成',
            pageLabel: '账号绑定',
            feature: 'integration-bindings',
          },
        },
        {
          path: 'integration/groups',
          name: routeNames.integrationGroups,
          component: () => import('@/features/integration/bindings/IntegrationBindingsPage.vue'),
          props: { initialTab: 'groups' },
          meta: {
            appView: 'feature',
            workspace: '群与机器人',
            title: '群与机器人',
            module: '外部集成',
            pageLabel: '群与机器人',
            feature: 'integration-bindings',
          },
        },
        {
          path: 'base/products',
          name: routeNames.products,
          component: () => import('@/features/products/ProductPage.vue'),
          meta: {
            appView: 'products',
            workspace: '商品管理',
            title: '商品管理',
            module: '基础资料',
            pageLabel: '商品管理',
            feature: 'products',
          },
        },
        {
          path: 'settings/:section?',
          name: routeNames.settings,
          component: () => import('@/features/settings/SystemSettingsPage.vue'),
          meta: {
            appView: 'settings',
            workspace: '基础配置',
            title: '系统设置',
            module: '系统设置',
            pageLabel: '基础配置',
            feature: 'settings',
          },
        },
        ...featureRoutes,
      ],
    },
    { path: '/:pathMatch(.*)*', redirect: { name: routeNames.notFound } },
  ],
  scrollBehavior: () => ({ top: 0, left: 0 }),
});

function resolveHomeLocation() {
  const homePath = getAuthorizedHomePath(sessionState.homePath);
  return homePath ? { path: homePath, replace: true } : { name: routeNames.forbidden, replace: true };
}

router.beforeEach(async (to) => {
  const isLoginRoute = to.name === routeNames.login;
  if (getAccessToken() && sessionState.status !== 'authenticated') {
    try {
      await restoreSession();
    } catch {
      // A failed restore is handled as an anonymous visit below; the login page remains available.
    }
  }

  const page = findPageRegistrationByRouteName(String(to.name ?? ''));
  const decision = decideRouteAccess({
    authenticated: Boolean(getAccessToken()) && sessionState.status === 'authenticated',
    isErrorRoute: to.name === routeNames.forbidden || to.name === routeNames.notFound,
    isLoginRoute,
    isRoot: to.path === '/',
    registered: Boolean(page),
    authorized: Boolean(page && isRouteAuthorized(page.routeName)),
  });

  if (decision === 'login') {
    return { name: routeNames.login, query: { redirect: to.fullPath }, replace: true };
  }
  if (decision === 'home') return resolveHomeLocation();
  if (decision === 'not-found') return { name: routeNames.notFound, replace: true };
  if (decision === 'forbidden') return { name: routeNames.forbidden, replace: true };
  return true;
});

router.afterEach((to, from, failure) => {
  document.title = `${String(to.meta.title ?? '工作台')} · ${appEnvironment.appName}`;
  const visit = createAuthorizedPageVisit({
    authenticated: Boolean(getAccessToken()) && sessionState.status === 'authenticated',
    failed: Boolean(failure),
    page: getAuthorizedPageByPath(to.path),
    path: to.path,
    previousPath: from.path,
  });
  if (visit) {
    // 访问留痕失败不阻断页面使用，不记录请求数据或凭据。
    void auditLogApi.reportPageVisit(visit).catch(() => {});
  }
});
