<script setup lang="ts">
import { Settings2 } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows, ListPageSurface, normalizeListPageModel } from '@/components/page';
import type { ListPageRow, PageAction, PageModel } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';

const props = defineProps<{ model: PageModel }>();
const rows = ref<ListPageRow[]>(cloneReferenceRows(props.model.rows));
const pageModel = computed(() => normalizeListPageModel(props.model, rows.value));
const activeRow = ref<ListPageRow | null>(null);
const detailOpen = ref(false);

function valueOf(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function openRow(row: ListPageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction) {
  toast.info(`${action.label}已在本地复刻会话执行`);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openRow"
  >
    <template #hero-icon>
      <span :class="$style.heroIcon"
        ><Settings2
          :size="20"
          :stroke-width="1.8"
      /></span>
    </template>
  </ListPageSurface>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? valueOf(activeRow, 'name') : `${props.model.label}详情`"
    description="配置记录只读详情"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in props.model.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ valueOf(activeRow, column.key) }}</dd>
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
  @apply flex h-10 w-10 items-center justify-center rounded-lg bg-[#F4F5F7] text-[#42526E];
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
