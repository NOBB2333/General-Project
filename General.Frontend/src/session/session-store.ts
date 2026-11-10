import { reactive } from 'vue';
import { ApiRequestError } from '@/api/client';
import { getAccessCodesApi, getCurrentUserInfoApi, loginApi, logoutApi } from '@/api/core/auth';
import type { AuthApi, SafeCurrentUserInfo } from '@/api/core/auth';
import { getCurrentMenusApi } from '@/api/core/menu';
import { clearAccessCodes, setAccessCodes } from './access-store';
import { clearAccessToken, getAccessToken, getSessionVersion, setAccessToken } from './token-storage';
import { clearCurrentNavigation, setCurrentNavigation } from '@/navigation/menu-store';

export type SessionStatus = 'anonymous' | 'authenticating' | 'authenticated' | 'error' | 'restoring';

export interface SessionState {
  contextVersion: number;
  error: string | null;
  homePath: string;
  status: SessionStatus;
  user: SafeCurrentUserInfo | null;
}

export const sessionState = reactive<SessionState>({
  contextVersion: 0,
  error: null,
  homePath: '/platform/workspace',
  status: 'anonymous',
  user: null,
});

let operationVersion = 0;
let pendingRestore: Promise<boolean> | null = null;

function toSafeUserInfo(value: AuthApi.CurrentUserInfo): SafeCurrentUserInfo {
  return Object.fromEntries(
    Object.entries(value).filter(([key]) => key.toLowerCase() !== 'token'),
  ) as SafeCurrentUserInfo;
}

function toErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : '会话处理失败，请稍后重试。';
}

function applyIdentity(
  userInfo: AuthApi.CurrentUserInfo,
  accessCodes: string[],
  menus: Awaited<ReturnType<typeof getCurrentMenusApi>>,
): void {
  const user = toSafeUserInfo(userInfo);
  sessionState.user = user;
  sessionState.homePath = user.homePath || '/platform/workspace';
  sessionState.status = 'authenticated';
  sessionState.error = null;
  setAccessCodes(accessCodes);
  setCurrentNavigation(menus, accessCodes);
}

export function clearSession(): void {
  operationVersion += 1;
  clearAccessToken();
  clearAccessCodes();
  clearCurrentNavigation();
  sessionState.status = 'anonymous';
  sessionState.user = null;
  sessionState.homePath = '/platform/workspace';
  sessionState.error = null;
  sessionState.contextVersion += 1;
}

export function useSessionStore(): SessionState {
  return sessionState;
}

export async function loginSession(input: AuthApi.LoginInput): Promise<boolean> {
  const operation = ++operationVersion;
  clearAccessToken();
  clearAccessCodes();
  clearCurrentNavigation();
  sessionState.status = 'authenticating';
  sessionState.user = null;
  sessionState.homePath = '/platform/workspace';
  sessionState.error = null;
  sessionState.contextVersion += 1;

  let loginCompleted = false;
  try {
    const loginResult = await loginApi(input);
    loginCompleted = true;
    if (operation !== operationVersion) return false;

    setAccessToken(loginResult.accessToken);
    const authenticatedSessionVersion = getSessionVersion();
    const [userInfo, accessCodes, menus] = await Promise.all([
      getCurrentUserInfoApi(),
      getAccessCodesApi(),
      getCurrentMenusApi(),
    ]);
    if (operation !== operationVersion || authenticatedSessionVersion !== getSessionVersion()) return false;

    applyIdentity(userInfo, accessCodes, menus);
    if (loginResult.homePath) sessionState.homePath = loginResult.homePath;
    return true;
  } catch (error) {
    const reportedError =
      !loginCompleted && error instanceof ApiRequestError && error.status === 403
        ? new ApiRequestError('用户名或密码错误，或账号当前不可登录。', {
            cause: error,
            code: error.code,
            status: error.status,
          })
        : error;

    if (operation === operationVersion) {
      clearAccessToken();
      clearAccessCodes();
      clearCurrentNavigation();
      sessionState.status = 'anonymous';
      sessionState.user = null;
      sessionState.homePath = '/platform/workspace';
      sessionState.error = toErrorMessage(reportedError);
    }
    throw reportedError;
  }
}

export async function replaceSessionFromOperation(result: AuthApi.LoginResult): Promise<string | null> {
  if (!result.accessToken.trim()) throw new TypeError('代操作响应缺少 accessToken。');

  const operation = ++operationVersion;
  pendingRestore = null;
  clearAccessCodes();
  clearCurrentNavigation();
  sessionState.status = 'restoring';
  sessionState.user = null;
  sessionState.homePath = result.homePath || '/platform/workspace';
  sessionState.error = null;
  setAccessToken(result.accessToken);
  const tokenVersion = getSessionVersion();
  sessionState.contextVersion += 1;

  try {
    const [userInfo, accessCodes, menus] = await Promise.all([
      getCurrentUserInfoApi(),
      getAccessCodesApi(),
      getCurrentMenusApi(),
    ]);
    if (operation !== operationVersion || tokenVersion !== getSessionVersion()) return null;

    applyIdentity(userInfo, accessCodes, menus);
    if (result.homePath) sessionState.homePath = result.homePath;
    return sessionState.homePath;
  } catch (error) {
    if (operation === operationVersion && tokenVersion === getSessionVersion()) {
      clearSession();
      sessionState.status = 'error';
      sessionState.error = toErrorMessage(error);
    }
    throw error;
  }
}

export async function restoreSession(): Promise<boolean> {
  if (sessionState.status === 'authenticated') return true;
  if (pendingRestore) return pendingRestore;
  if (!getAccessToken()) {
    if (sessionState.status !== 'anonymous') clearSession();
    return false;
  }

  const operation = ++operationVersion;
  const tokenVersion = getSessionVersion();
  sessionState.status = 'restoring';
  sessionState.error = null;

  const task = (async () => {
    try {
      const [userInfo, accessCodes, menus] = await Promise.all([
        getCurrentUserInfoApi(),
        getAccessCodesApi(),
        getCurrentMenusApi(),
      ]);
      if (operation !== operationVersion || tokenVersion !== getSessionVersion()) return false;
      applyIdentity(userInfo, accessCodes, menus);
      return true;
    } catch (error) {
      if (operation === operationVersion && tokenVersion === getSessionVersion()) {
        sessionState.status = 'error';
        sessionState.error = toErrorMessage(error);
      }
      throw error;
    }
  })();

  pendingRestore = task;
  try {
    return await task;
  } finally {
    if (pendingRestore === task) pendingRestore = null;
  }
}

export async function logoutSession(): Promise<void> {
  const hasSession = Boolean(getAccessToken());
  try {
    if (hasSession) await logoutApi();
  } finally {
    clearSession();
  }
}
