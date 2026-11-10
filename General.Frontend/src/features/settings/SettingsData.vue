<script setup lang="ts">
import { Database } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import { isRouteAuthorized } from '@/navigation/menu-store';
import { routeNames } from '@/router/route.types';
import { UiButton } from '@/components/ui';
import SettingsPanel from './SettingsPanel.vue';

const router = useRouter();

function openRoute(routeName: string) {
  if (!isRouteAuthorized(routeName)) return;
  void router.push({ name: routeName });
}
</script>

<template>
  <SettingsPanel
    title="数据管理"
    description="数据库与本地文件备份/恢复属于部署运维，请在停止服务后按部署包中的 DATA-MANAGEMENT.md 运行 data-management.py。恢复先进入隔离暂存位置；浏览器不会直接读取或写入宿主数据库。"
    min-height="empty"
  >
    <template #actions>
      <UiButton
        v-if="isRouteAuthorized(routeNames.platformFiles)"
        size="sm"
        @click="openRoute(routeNames.platformFiles)"
      >
        文件管理
      </UiButton>
      <UiButton
        v-if="isRouteAuthorized(routeNames.platformRecycleBin)"
        size="sm"
        @click="openRoute(routeNames.platformRecycleBin)"
      >
        回收中心
      </UiButton>
    </template>
    <div :class="$style.emptyState">
      <span :class="$style.emptyIcon"
        ><Database
          :size="22"
          :stroke-width="1.8"
      /></span>
      <strong :class="$style.emptyTitle">部署数据运维</strong>
      <p :class="$style.emptyDescription">仅在主机侧管理数据库与本地文件备份</p>
    </div>
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.emptyState {
  @apply flex min-h-[490px] flex-col items-center justify-center text-center;
}
.emptyIcon {
  @apply flex h-14 w-14 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.emptyTitle {
  @apply mt-4 block text-[14px];
}
.emptyDescription {
  @apply mt-2 text-[12px];
  color: var(--ui-text-subtle);
}
</style>
