<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { AlertTriangle, ArrowUpRight, Building2, FileStack, ShieldCheck, UsersRound } from 'lucide-vue-next';
import { getWorkspaceSummaryApi } from '@/api/core/workspace';
import type { WorkspaceApi } from '@/api/core/workspace';
import { getAuthorizedPageByPath } from '@/navigation/menu-store';
import { sessionState } from '@/session/session-store';
import { UiButton } from '@/components/ui';
import { useRouter } from 'vue-router';

const router = useRouter();
const summary = ref<WorkspaceApi.Summary | null>(null);
const loading = ref(true);
const errorMessage = ref('');
const displayName = computed(() => sessionState.user?.realName?.trim() || sessionState.user?.username || '当前用户');
const metrics = computed(() => {
  if (!summary.value) return [];
  return [
    { label: '用户', value: summary.value.userCount, icon: UsersRound },
    { label: '组织', value: summary.value.organizationCount, icon: Building2 },
    { label: '角色', value: summary.value.roleCount, icon: ShieldCheck },
    { label: '在线用户', value: summary.value.onlineUserCount, icon: UsersRound },
    { label: '租户', value: summary.value.tenantCount, icon: Building2 },
    { label: '文件', value: summary.value.fileCount, icon: FileStack },
    { label: '更新公告', value: summary.value.updateLogCount, icon: FileStack },
    { label: '近七日异常', value: summary.value.exceptionLogCount, icon: AlertTriangle },
  ];
});

async function loadSummary() {
  loading.value = true;
  errorMessage.value = '';
  try {
    summary.value = await getWorkspaceSummaryApi();
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : '工作台数据加载失败，请稍后重试。';
  } finally {
    loading.value = false;
  }
}

function canOpenAttentionLink(link: null | string): boolean {
  return Boolean(link && getAuthorizedPageByPath(link)?.localPath);
}

function openAttentionLink(link: null | string) {
  const target = link ? getAuthorizedPageByPath(link)?.localPath : undefined;
  if (target) void router.push(target);
}

onMounted(() => void loadSummary());
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-label="平台总览"
    data-page-module="平台中心"
  >
    <header :class="$style.header">
      <div>
        <p :class="$style.eyebrow">GENERAL ADMIN</p>
        <h1>欢迎回来，{{ displayName }}</h1>
        <p :class="$style.description">这里显示当前账号数据范围内的平台治理概览。</p>
      </div>
      <UiButton
        :loading="loading"
        @click="loadSummary"
        >刷新数据</UiButton
      >
    </header>

    <p
      v-if="loading"
      :class="$style.stateMessage"
      role="status"
    >
      正在加载平台数据…
    </p>
    <div
      v-else-if="errorMessage"
      :class="$style.errorState"
      role="alert"
    >
      <AlertTriangle :size="18" />
      <span>{{ errorMessage }}</span>
      <UiButton @click="loadSummary">重试</UiButton>
    </div>
    <template v-else-if="summary">
      <section
        :class="$style.metricGrid"
        aria-label="平台数据概览"
      >
        <article
          v-for="metric in metrics"
          :key="metric.label"
          :class="$style.metricCard"
        >
          <span :class="$style.metricIcon"
            ><component
              :is="metric.icon"
              :size="18"
          /></span>
          <span :class="$style.metricLabel">{{ metric.label }}</span>
          <strong>{{ metric.value.toLocaleString('zh-CN') }}</strong>
        </article>
      </section>

      <section
        :class="$style.attentionCard"
        aria-labelledby="attention-title"
      >
        <div :class="$style.sectionHeading">
          <div>
            <h2 id="attention-title">需要关注</h2>
            <p>内容来自 General.Admin 平台汇总接口。</p>
          </div>
          <span :class="$style.attentionCount">{{ summary.attentionItems.length }}</span>
        </div>
        <ul
          v-if="summary.attentionItems.length"
          :class="$style.attentionList"
        >
          <li
            v-for="item in summary.attentionItems"
            :key="item.key"
            :class="$style.attentionItem"
          >
            <div>
              <strong>{{ item.title }}</strong>
              <p>{{ item.detail }}</p>
            </div>
            <button
              v-if="canOpenAttentionLink(item.link)"
              type="button"
              :class="$style.attentionLink"
              @click="openAttentionLink(item.link)"
            >
              查看 <ArrowUpRight :size="14" />
            </button>
          </li>
        </ul>
        <p
          v-else
          :class="$style.emptyMessage"
        >
          当前没有需要关注的事项。
        </p>
      </section>
    </template>
  </main>
</template>

<style module>
@reference "../../styles/index.css";

.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-5 p-5;
  color: var(--ui-text);
}
.header {
  @apply flex flex-wrap items-center justify-between gap-4 border-b pb-5;
  border-color: var(--ui-border);
}
.eyebrow {
  @apply mb-2 text-[10px] font-semibold tracking-[0.14em];
  color: var(--ui-text-subtle);
}
.header h1 {
  @apply m-0 text-[22px] font-semibold;
}
.description {
  @apply mb-0 mt-2 text-[13px];
  color: var(--ui-text-subtle);
}
.stateMessage {
  @apply m-0 rounded border bg-white p-5 text-[13px];
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.errorState {
  @apply flex flex-wrap items-center gap-3 rounded border p-4 text-[13px];
  border-color: var(--ui-danger);
  color: var(--ui-danger);
}
.metricGrid {
  @apply grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4;
}
.metricCard {
  @apply grid min-h-[122px] grid-cols-[36px_1fr] content-center gap-x-3 gap-y-1 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metricIcon {
  @apply row-span-2 grid h-9 w-9 place-items-center rounded;
  color: var(--app-primary);
  background: var(--ui-surface-subtle);
}
.metricLabel {
  @apply self-end text-[12px];
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply text-[22px] font-semibold leading-7;
}
.attentionCard {
  @apply rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.sectionHeading {
  @apply flex items-start justify-between border-b pb-4;
  border-color: var(--ui-border);
}
.sectionHeading h2 {
  @apply m-0 text-[15px] font-semibold;
}
.sectionHeading p {
  @apply mb-0 mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.attentionCount {
  @apply grid h-7 min-w-7 place-items-center rounded-full px-2 text-[12px] font-semibold;
  color: var(--app-primary);
  background: var(--ui-surface-subtle);
}
.attentionList {
  @apply m-0 list-none p-0;
}
.attentionItem {
  @apply flex items-center justify-between gap-4 border-b py-4 last:border-b-0;
  border-color: var(--ui-border);
}
.attentionItem strong {
  @apply text-[13px] font-medium;
}
.attentionItem p {
  @apply mb-0 mt-1 text-[12px] leading-5;
  color: var(--ui-text-subtle);
}
.attentionLink {
  @apply inline-flex shrink-0 items-center gap-1 text-[12px] font-medium;
  color: var(--app-primary);
}
.emptyMessage {
  @apply mb-0 mt-4 text-[13px];
  color: var(--ui-text-subtle);
}
</style>
