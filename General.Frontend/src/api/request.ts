import { appEnvironment } from '@/config/env';
import { createApiClient } from './client';
import { getAccessToken, getSessionVersion } from '@/session/token-storage';
import { notifyUnauthorized } from '@/session/unauthorized';

export const apiClient = createApiClient({
  baseUrl: appEnvironment.apiBaseUrl,
  getAccessToken,
  getSessionVersion,
  onUnauthorized: notifyUnauthorized,
});
