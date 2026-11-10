<script setup lang="ts">
import { BadgeCheck, Info, RefreshCw } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows, ListPageSurface, normalizeListPageModel } from '@/components/page';
import type { ListPageRow, PageAction } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { warrantyReferenceFixture } from '@/features/service/model/warranty.data';
import { warrantyModel } from '@/features/service/model/warranty.model';

const rows = ref(cloneReferenceRows(warrantyReferenceFixture.rows));
const pageModel = computed(() => normalizeListPageModel(warrantyModel, rows.value));
const activeRow = ref<ListPageRow | null>(null);
const detailOpen = ref(false);
const detailTitle = computed(() => (activeRow.value ? cellValue(activeRow.value, 'code') : '质保详情'));
function openRow(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function runAction(action: PageAction) {
  toast.success(action.label.includes('巡检') ? '质保数据已完成本地巡检' : `${action.label}已在本地演示会话执行`);
}
function cellValue(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openRow"
  >
    <template #hero-icon
      ><span :class="$style.heroIcon"
        ><BadgeCheck
          :size="22"
          :stroke-width="1.8" /></span
    ></template>
    <template #notice
      ><div :class="$style.notice">
        <Info
          :size="15"
          :stroke-width="1.8"
        /><span>质保卡由本地参考数据生成，点击立即巡检只刷新当前页面状态。</span
        ><UiButton
          variant="ghost"
          size="sm"
          aria-label="刷新质保数据"
          title="刷新质保数据"
          @click="toast.info('质保数据已刷新')"
          ><RefreshCw
            :size="14"
            :stroke-width="1.8"
        /></UiButton></div
    ></template>
  </ListPageSurface>
  <UiDialog
    v-model:open="detailOpen"
    :title="detailTitle"
    description="质保记录只读详情"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in warrantyModel.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ cellValue(activeRow, column.key) }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg bg-[#E3FCEF] text-[#00875A];
}
.notice {
  @apply flex items-center gap-2 rounded border border-[#B3D4FF] bg-[#DEEBFF]/40 px-3 py-2 text-[12px] text-[#42526E];
}
.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 text-[12px];
}
.detailGrid dt {
  @apply text-[#6B778C];
}
.detailGrid dd {
  @apply mt-1 font-medium text-[#172B4D];
}
</style>
