import type { Recordable, UserInfo } from '@vben/types';

import { ref } from 'vue';
import { useRouter } from 'vue-router';

import { LOGIN_PATH } from '@vben/constants';
import { preferences } from '@vben/preferences';
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores';

import { notification } from 'ant-design-vue';
import { defineStore } from 'pinia';

import {
  enterTenantOperationApi,
  exitTenantOperationApi,
  getAccessCodesApi,
  getUserInfoApi,
  loginApi,
  logoutApi,
} from '#/api';
import { $t } from '#/locales';

export const useAuthStore = defineStore('auth', () => {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const router = useRouter();

  const loginLoading = ref(false);

  function normalizeHomePath(userInfo: UserInfo) {
    const allowedAppCodes = (import.meta.env['VITE_GLOB_APP_CODES'] || '')
      .split(',')
      .map((item: string) => item.trim())
      .filter(Boolean);

    if (!userInfo.homePath || allowedAppCodes.length === 0) {
      return userInfo;
    }

    const isAllowedHomePath = allowedAppCodes.some(
      (code: string) =>
        userInfo.homePath === `/${code}` ||
        userInfo.homePath.startsWith(`/${code}/`),
    );

    if (!isAllowedHomePath) {
      userInfo.homePath = preferences.app.defaultHomePath;
    }

    return userInfo;
  }

  /**
   * 异步处理登录操作
   * Asynchronously handle the login process
   * @param params 登录表单数据
   */
  async function authLogin(
    params: Recordable<any>,
    onSuccess?: () => Promise<void> | void,
  ) {
    // 异步处理用户登录操作并获取 accessToken
    let userInfo: null | UserInfo = null;
    try {
      loginLoading.value = true;
      const { accessToken } = await loginApi(params);

      // 如果成功获取到 accessToken
      if (accessToken) {
        accessStore.setAccessToken(accessToken);

        // 获取用户信息并存储到 accessStore 中
        const [fetchUserInfoResult, accessCodes] = await Promise.all([
          fetchUserInfo(),
          getAccessCodesApi(),
        ]);

        userInfo = normalizeHomePath(fetchUserInfoResult);

        userStore.setUserInfo(userInfo);
        accessStore.setAccessCodes(accessCodes);

        if (accessStore.loginExpired) {
          accessStore.setLoginExpired(false);
        } else {
          onSuccess
            ? await onSuccess?.()
            : await router.push(
                userInfo.homePath || preferences.app.defaultHomePath,
              );
        }

        if (userInfo?.realName) {
          notification.success({
            description: `${$t('authentication.loginSuccessDesc')}:${userInfo?.realName}`,
            duration: 3,
            message: $t('authentication.loginSuccess'),
          });
        }
      }
    } finally {
      loginLoading.value = false;
    }

    return {
      userInfo,
    };
  }

  async function logout(redirect: boolean = true) {
    try {
      await logoutApi();
    } catch {
      // 不做任何处理
    }
    resetAllStores();
    accessStore.setLoginExpired(false);

    // 回登录页带上当前路由地址
    await router.replace({
      path: LOGIN_PATH,
      query: redirect
        ? {
            redirect: encodeURIComponent(router.currentRoute.value.fullPath),
          }
        : {},
    });
  }

  async function switchAccessToken(accessToken: string) {
    const previousAccessToken = accessStore.accessToken;
    const previousAccessCodes = [...accessStore.accessCodes];
    const previousAccessMenus = [...accessStore.accessMenus];
    const previousAccessRoutes = [...accessStore.accessRoutes];
    const previousIsAccessChecked = accessStore.isAccessChecked;
    const previousUserInfo = userStore.userInfo;

    accessStore.setAccessToken(accessToken);
    accessStore.setAccessCodes([]);
    accessStore.setAccessMenus([]);
    accessStore.setAccessRoutes([]);
    accessStore.setIsAccessChecked(false);

    try {
      const [userInfoResult, accessCodes] = await Promise.all([
        fetchUserInfo(),
        getAccessCodesApi(),
      ]);
      const userInfo = normalizeHomePath(userInfoResult);
      userStore.setUserInfo(userInfo);
      accessStore.setAccessCodes(accessCodes);
      return userInfo;
    } catch (error) {
      accessStore.setAccessToken(previousAccessToken);
      accessStore.setAccessCodes(previousAccessCodes);
      accessStore.setAccessMenus(previousAccessMenus);
      accessStore.setAccessRoutes(previousAccessRoutes);
      accessStore.setIsAccessChecked(previousIsAccessChecked);
      userStore.setUserInfo(previousUserInfo);
      throw error;
    }
  }

  async function enterTenantOperation(tenantId: string) {
    const { accessToken } = await enterTenantOperationApi(tenantId);
    return await switchAccessToken(accessToken);
  }

  async function exitTenantOperation() {
    const { accessToken } = await exitTenantOperationApi();
    return await switchAccessToken(accessToken);
  }

  async function fetchUserInfo() {
    const userInfo = await getUserInfoApi();
    const normalizedUserInfo = normalizeHomePath(userInfo);
    userStore.setUserInfo(normalizedUserInfo);
    return normalizedUserInfo;
  }

  function $reset() {
    loginLoading.value = false;
  }

  return {
    $reset,
    authLogin,
    enterTenantOperation,
    exitTenantOperation,
    fetchUserInfo,
    loginLoading,
    logout,
  };
});
