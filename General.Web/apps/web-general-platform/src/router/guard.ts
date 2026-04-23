import type { Router } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { startProgress, stopProgress } from '@vben/utils';

import { accessRoutes, coreRouteNames } from '#/router/routes';
import { useAuthStore } from '#/store';
import { reportPageVisitApi } from '#/api/core/audit-log';

import { generateAccess } from './access';

const FALLBACK_NOT_FOUND_ROUTE_NAME = 'FallbackNotFound';

function buildLoginRedirect(fullPath: string) {
  return {
    path: LOGIN_PATH,
    query:
      fullPath === preferences.app.defaultHomePath
        ? {}
        : { redirect: encodeURIComponent(fullPath) },
    replace: true,
  };
}

function resetAccessState() {
  const accessStore = useAccessStore();
  accessStore.setAccessCodes([]);
  accessStore.setAccessMenus([]);
  accessStore.setAccessRoutes([]);
  accessStore.setAccessToken(null);
  accessStore.setRefreshToken(null);
  accessStore.setIsAccessChecked(false);
  accessStore.setLoginExpired(false);
}

/**
 * 通用守卫配置
 * @param router
 */
function setupCommonGuard(router: Router) {
  // 记录已经加载的页面
  const loadedPaths = new Set<string>();

  router.beforeEach((to) => {
    to.meta.loaded = loadedPaths.has(to.path);

    // 页面加载进度条
    if (!to.meta.loaded && preferences.transition.progress) {
      startProgress();
    }
    return true;
  });

  router.afterEach((to) => {
    // 记录页面是否加载,如果已经加载，后续的页面切换动画等效果不在重复执行

    loadedPaths.add(to.path);

    // 关闭页面加载进度条
    if (preferences.transition.progress) {
      stopProgress();
    }

    // 菜单访问埋点：仅对已认证的业务页面上报
    const accessStore = useAccessStore();
    if (
      accessStore.accessToken &&
      !coreRouteNames.includes(to.name as string) &&
      !to.meta?.ignoreAccess
    ) {
      const title = (to.meta?.title as string | undefined) ?? to.path;
      reportPageVisitApi({ menuPath: to.path, menuTitle: title }).catch(() => {});
    }
  });
}

/**
 * 权限访问守卫配置
 * @param router
 */
function setupAccessGuard(router: Router) {
  router.beforeEach(async (to, from) => {
    const accessStore = useAccessStore();
    const userStore = useUserStore();
    const authStore = useAuthStore();

    // 基本路由，这些路由不需要进入权限拦截
    if (coreRouteNames.includes(to.name as string)) {
      if (
        to.path === LOGIN_PATH &&
        accessStore.accessToken &&
        accessStore.isAccessChecked &&
        userStore.userInfo?.homePath
      ) {
        return decodeURIComponent(
          (to.query?.redirect as string) ||
            userStore.userInfo?.homePath ||
            preferences.app.defaultHomePath,
        );
      }
      return true;
    }

    // accessToken 检查
    if (!accessStore.accessToken) {
      // 明确声明忽略权限访问权限，则可以访问
      if (to.meta.ignoreAccess) {
        return true;
      }

      // 没有访问权限，跳转登录页面
      if (to.fullPath !== LOGIN_PATH) {
        return buildLoginRedirect(to.fullPath);
      }
      return to;
    }

    const shouldRegenerateAccess =
      !accessStore.isAccessChecked ||
      to.name === FALLBACK_NOT_FOUND_ROUTE_NAME;

    if (!shouldRegenerateAccess) {
      return true;
    }

    try {
      const userInfo = userStore.userInfo || (await authStore.fetchUserInfo());
      const userRoles = userInfo.roles ?? [];

      const { accessibleMenus, accessibleRoutes } = await generateAccess({
        roles: userRoles,
        router,
        routes: accessRoutes,
      });

      accessStore.setAccessMenus(accessibleMenus);
      accessStore.setAccessRoutes(accessibleRoutes);
      accessStore.setIsAccessChecked(true);
      const redirectPath = (from.query.redirect ??
        (to.path === preferences.app.defaultHomePath
          ? userInfo.homePath || preferences.app.defaultHomePath
          : to.fullPath)) as string;

      return {
        ...router.resolve(decodeURIComponent(redirectPath)),
        replace: true,
      };
    } catch (error) {
      console.error('Failed to build dynamic routes.', error);
      resetAccessState();
      return buildLoginRedirect(to.fullPath);
    }
  });
}

/**
 * 项目守卫配置
 * @param router
 */
function createRouterGuard(router: Router) {
  /** 通用 */
  setupCommonGuard(router);
  /** 权限访问 */
  setupAccessGuard(router);
}

export { createRouterGuard };
