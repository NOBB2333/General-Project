<script setup lang="ts">
import { Boxes, Plus, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiButton, UiDialog, UiInput, toast } from '@/components/ui';
import { cloneReferenceRows } from '../../reference';
import type { PageModel, PageRow } from '../../page.types';

const props = defineProps<{ model: PageModel }>();
const rows = ref<PageRow[]>(cloneReferenceRows(props.model.rows));
const query = ref('');
const activeRow = ref<PageRow | null>(null);
const detailOpen = ref(false);
const visibleRows = computed(() => {
  const key = query.value.trim().toLowerCase();
  return key
    ? rows.value.filter((row) =>
        Object.values(row).some((value) =>
          String(value ?? '')
            .toLowerCase()
            .includes(key),
        ),
      )
    : rows.value;
});
function value(row: PageRow, key: string) {
  const result = row[key];
  return result === undefined || result === null || result === '' ? '—' : String(result);
}
function title(row: PageRow) {
  return value(row, 'name') !== '—'
    ? value(row, 'name')
    : value(row, 'title') !== '—'
      ? value(row, 'title')
      : value(row, 'code');
}
function openRow(row: PageRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function runTool(tool: string) {
  if (/新增|新建|创建/.test(tool)) {
    rows.value = [
      {
        id: `${props.model.pageSlug}-local-${Date.now()}`,
        code: `LOCAL-${rows.value.length + 1}`,
        name: `本地${props.model.label}`,
        status: '草稿',
        owner: '张经理',
      },
      ...rows.value,
    ];
    toast.success(`${props.model.label}本地草稿已创建`);
    return;
  }
  toast.info(`${tool}仅在本地复刻页执行`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="cards"
    :data-page-route="props.model.path"
  >
    <header :class="$style.hero">
      <div>
        <p :class="$style.breadcrumb">{{ props.model.module }} / {{ props.model.label }}</p>
        <h1 :class="$style.title">{{ props.model.label }}</h1>
        <p :class="$style.description">{{ props.model.description }}</p>
      </div>
      <div :class="$style.actions">
        <UiButton
          v-for="tool in props.model.tools.slice(0, 4)"
          :key="tool"
          :variant="/新增|新建/.test(tool) ? 'primary' : 'secondary'"
          size="sm"
          @click="runTool(tool)"
          ><Plus
            v-if="/新增|新建/.test(tool)"
            :size="14"
            :stroke-width="1.8"
          />{{ tool }}</UiButton
        >
      </div>
    </header>
    <section :class="$style.toolbar">
      <label :class="$style.search"
        ><Search
          :size="14"
          :stroke-width="1.8" /><UiInput
          v-model="query"
          :placeholder="props.model.searchPlaceholder" /></label
      ><span>{{ visibleRows.length }} 项</span>
    </section>
    <section :class="$style.grid">
      <button
        v-for="row in visibleRows"
        :key="row.id"
        type="button"
        :class="$style.card"
        @click="openRow(row)"
      >
        <span :class="$style.cardIcon"
          ><Boxes
            :size="20"
            :stroke-width="1.8"
        /></span>
        <div :class="$style.cardCopy">
          <small>{{ value(row, 'code') }}</small
          ><strong>{{ title(row) }}</strong>
          <p>{{ value(row, 'status') }} · {{ value(row, 'owner') }}</p>
        </div>
      </button>
    </section>
  </main>
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? title(activeRow) : `${props.model.label}详情`"
    description="参考记录只读，本地操作只影响当前会话"
    ><dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in props.model.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ value(activeRow, column.key) }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../../styles/index.css";
.page {
  @apply min-w-0 space-y-4 px-5 py-4 text-[#172B4D];
}
.hero {
  @apply flex min-h-[96px] items-start justify-between gap-5 rounded-lg border border-[#DFE1E6] bg-white px-5 py-4;
}
.breadcrumb {
  @apply text-[11px] text-[#6B778C];
}
.title {
  @apply mt-1 text-[20px] font-semibold;
}
.description {
  @apply mt-1 max-w-[760px] text-[12px] leading-5 text-[#42526E];
}
.actions {
  @apply flex flex-wrap justify-end gap-2;
}
.toolbar {
  @apply flex items-center justify-between rounded border border-[#DFE1E6] bg-white px-3 py-2 text-[11px] text-[#6B778C];
}
.search {
  @apply flex min-w-[280px] items-center gap-2;
}
.grid {
  @apply grid grid-cols-3 gap-3;
}
.card {
  @apply flex min-h-[112px] items-start gap-3 rounded-lg border border-[#DFE1E6] bg-white p-4 text-left transition-colors hover:border-[#4C9AFF] hover:bg-[#FAFBFC];
}
.cardIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded bg-[#DEEBFF] text-[#0052CC];
}
.cardCopy {
  @apply min-w-0;
}
.cardCopy small {
  @apply text-[10px] text-[#6B778C];
}
.cardCopy strong {
  @apply mt-1 block truncate text-[13px];
}
.cardCopy p {
  @apply mt-2 text-[11px] text-[#42526E];
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
@media (max-width: 1100px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
