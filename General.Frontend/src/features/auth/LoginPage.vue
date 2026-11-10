<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ApiRequestError } from '@/api/client';
import { UiButton, UiFormField, UiInput } from '@/components/ui';
import { getAuthorizedHomePath } from '@/navigation/menu-store';
import { loginSession, useSessionStore } from '@/session/session-store';

const route = useRoute();
const router = useRouter();
const session = useSessionStore();
const credentials = reactive({ username: '', password: '' });
const submitting = ref(false);
const errorMessage = ref('');

function safeInternalPath(value: unknown): string | undefined {
  if (typeof value !== 'string' || !value.startsWith('/') || value.startsWith('//')) return undefined;
  return value;
}

async function submitLogin() {
  errorMessage.value = '';
  submitting.value = true;
  try {
    const authenticated = await loginSession(credentials);
    if (!authenticated) return;
    await router.replace(safeInternalPath(route.query.redirect) ?? getAuthorizedHomePath(session.homePath) ?? '/403');
  } catch (error) {
    errorMessage.value =
      error instanceof ApiRequestError
        ? error.message
        : error instanceof Error
          ? error.message
          : '登录失败，请稍后重试。';
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <main :class="$style.page">
    <section
      :class="$style.card"
      aria-labelledby="login-title"
    >
      <div :class="$style.brand">
        <span
          :class="$style.brandMark"
          aria-hidden="true"
          >GA</span
        >
        <div>
          <h1 id="login-title">General Admin</h1>
          <p>企业项目管理平台</p>
        </div>
      </div>

      <div :class="$style.heading">
        <h2>登录</h2>
        <p>使用平台账号继续</p>
      </div>

      <form
        :class="$style.form"
        @submit.prevent="submitLogin"
      >
        <UiFormField
          label="用户名"
          required
        >
          <UiInput
            v-model="credentials.username"
            autocomplete="username"
            maxlength="64"
            name="username"
            placeholder="请输入用户名"
            required
          />
        </UiFormField>
        <UiFormField
          label="密码"
          required
        >
          <UiInput
            v-model="credentials.password"
            autocomplete="current-password"
            maxlength="128"
            name="password"
            placeholder="请输入密码"
            required
            type="password"
          />
        </UiFormField>
        <p
          v-if="errorMessage || session.error"
          :class="$style.error"
          role="alert"
        >
          {{ errorMessage || session.error }}
        </p>
        <UiButton
          :class="$style.submit"
          :loading="submitting"
          size="lg"
          type="submit"
          variant="primary"
        >
          登录
        </UiButton>
      </form>
    </section>
  </main>
</template>

<style module>
@reference "../../styles/index.css";
.page {
  @apply grid min-h-screen place-items-center px-4 py-8;
  background: var(--ui-surface-subtle);
}
.card {
  @apply w-full max-w-[420px] border bg-white p-7 shadow-sm sm:p-9;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}
.brand {
  @apply flex items-center gap-3;
}
.brandMark {
  @apply grid h-10 w-10 place-items-center text-[12px] font-bold;
  color: white;
  background: var(--app-primary);
  border-radius: var(--app-control-radius);
}
.brand h1,
.brand p,
.heading h2,
.heading p {
  margin: 0;
}
.brand h1 {
  @apply text-[15px] font-semibold;
  color: var(--ui-text);
}
.brand p,
.heading p {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.heading {
  @apply mb-5 mt-8;
}
.heading h2 {
  @apply text-[20px] font-semibold;
  color: var(--ui-text);
}
.heading p {
  @apply mt-1;
}
.form {
  @apply grid gap-4;
}
.error {
  @apply m-0 text-[12px] leading-5;
  color: var(--ui-danger);
}
.submit {
  width: 100%;
}
</style>
