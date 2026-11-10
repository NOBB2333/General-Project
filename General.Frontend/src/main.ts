import { createApp } from 'vue';
import App from './App.vue';
import { getAuthorizedHomePath } from './navigation/menu-store';
import { router } from './router';
import { routeNames } from './router/route.types';
import { clearSession, restoreSession, sessionState } from './session/session-store';
import { registerUnauthorizedHandler } from './session/unauthorized';
import { vAccess } from './directives/permission';
import './styles/index.css';

const app = createApp(App);
app.use(router);
app.directive('access', vAccess);

registerUnauthorizedHandler(async () => {
  const currentRoute = router.currentRoute.value;
  const redirect = currentRoute.path === '/login' ? undefined : currentRoute.fullPath;
  clearSession();
  if (currentRoute.name !== routeNames.login) {
    await router.replace({ name: routeNames.login, query: redirect ? { redirect } : undefined });
  }
});

app.mount('#app');
void router
  .isReady()
  .then(async () => {
    const restored = await restoreSession();
    if (restored && router.currentRoute.value.name === routeNames.login) {
      const homePath = getAuthorizedHomePath(sessionState.homePath);
      await router.replace(homePath ?? { name: routeNames.forbidden });
    }
  })
  .catch(() => undefined);
