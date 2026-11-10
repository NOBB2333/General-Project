<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { UiButton } from '@/components/ui';
import { getAuthorizedHomePath } from '@/navigation/menu-store';
import { routeNames } from '@/router/route.types';
import { logoutSession, sessionState } from '@/session/session-store';

const route = useRoute();
const router = useRouter();
const isForbidden = computed(() => route.name === routeNames.forbidden);
const canReturnHome = computed(() => Boolean(getAuthorizedHomePath(sessionState.homePath)));
const code = computed(() => (isForbidden.value ? '403' : '404'));
const title = computed(() => (isForbidden.value ? '当前账号无权访问' : '页面不存在或尚未开放'));
const description = computed(() =>
  isForbidden.value
    ? '请联系平台管理员确认当前账号的菜单和权限配置。'
    : '此地址没有对应的已开放页面，请返回工作台继续操作。',
);

async function returnToLogin() {
  try {
    await logoutSession();
  } catch {
    // logoutSession clears local credentials even when the API request fails.
  }
  await router.replace({ name: routeNames.login });
}
</script>

<template>
  <main :class="$style.page">
    <section
      :class="$style.card"
      role="alert"
    >
      <p :class="$style.code">{{ code }}</p>
      <h1>{{ title }}</h1>
      <p :class="$style.description">{{ description }}</p>
      <div :class="$style.actions">
        <UiButton
          v-if="canReturnHome"
          variant="primary"
          @click="router.replace('/')"
          >返回工作台</UiButton
        >
        <UiButton
          v-if="isForbidden"
          @click="returnToLogin"
          >退出登录</UiButton
        >
      </div>
    </section>
  </main>
</template>

<style module>
@reference "../../styles/index.css";

.page {
  @apply grid min-h-screen place-items-center p-5;
  background: var(--ui-surface-subtle);
}
.card {
  @apply w-full max-w-[480px] rounded border bg-white p-8 text-center shadow-sm;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}
.code {
  @apply m-0 text-[44px] font-semibold leading-none;
  color: var(--app-primary);
}
.card h1 {
  @apply mb-0 mt-5 text-[18px] font-semibold;
  color: var(--ui-text);
}
.description {
  @apply mb-0 mt-2 text-[13px] leading-6;
  color: var(--ui-text-subtle);
}
.actions {
  @apply mt-6 flex flex-wrap justify-center gap-2;
}
</style>
