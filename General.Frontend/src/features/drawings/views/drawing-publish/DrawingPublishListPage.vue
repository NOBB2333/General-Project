<script setup lang="ts">
import { FileUp, Send } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows, ListPageSurface } from '@/components/page';
import { toast } from '@/components/ui';
import { drawingPublishReferenceFixture } from '@/features/drawings/model/drawing-publish.data';
import { drawingPublishModel } from '@/features/drawings/model/drawing-publish.model';
import type {
  DrawingPublishFilters,
  DrawingPublishHistoryRow,
  DrawingPublishRow,
  DrawingPublishStatus,
} from '@/features/drawings/model/drawing-publish.types';
import DrawingPublishDialogs from './DrawingPublishDialogs.vue';
import DrawingPublishFilterPanel from './DrawingPublishFilters.vue';

const rows = ref<DrawingPublishRow[]>(cloneReferenceRows(drawingPublishReferenceFixture.rows));
const historyRows = ref<DrawingPublishHistoryRow[]>(cloneReferenceRows(drawingPublishReferenceFixture.recentRows));
const appliedFilters = ref<DrawingPublishFilters>(emptyFilters());
const activeRowId = ref('');
const detailOpen = ref(false);
const historyOpen = ref(false);
const settingsOpen = ref(false);
const confirmOpen = ref(false);
const settingsDate = ref('');

const activeRow = computed(() => rows.value.find((row) => row.id === activeRowId.value) ?? null);
const filteredRows = computed(() => rows.value.filter((row) => matchesFilters(row, appliedFilters.value)));
const pageModel = computed(() => ({ ...drawingPublishModel, rows: filteredRows.value }));

function emptyFilters(): DrawingPublishFilters {
  return {
    drawingNo: '',
    name: '',
    pendingVersion: '',
    publishStatus: '',
    reviewStatus: '',
    currentVersion: '',
    publishedFrom: '',
    publishedTo: '',
    effectiveFrom: '',
    effectiveTo: '',
    majorChange: '',
    hasImpact: '',
  };
}

function includes(value: string, query: string) {
  return !query.trim() || value.toLocaleLowerCase('zh-CN').includes(query.trim().toLocaleLowerCase('zh-CN'));
}

function matchesFilters(row: DrawingPublishRow, filters: DrawingPublishFilters) {
  if (!includes(row.drawingNo, filters.drawingNo) || !includes(row.name, filters.name)) return false;
  if (!includes(row.pendingVersion, filters.pendingVersion) || !includes(row.currentVersion, filters.currentVersion))
    return false;
  if (filters.publishStatus && row.publishStatus !== filters.publishStatus) return false;
  if (filters.reviewStatus && row.reviewStatus !== filters.reviewStatus) return false;
  if (filters.majorChange && row.majorChange !== filters.majorChange) return false;
  if (filters.hasImpact && (Number(row.impactCount) > 0 ? '是' : '否') !== filters.hasImpact) return false;
  if (filters.publishedFrom && (!row.publishedAt || row.publishedAt < filters.publishedFrom)) return false;
  if (filters.publishedTo && (!row.publishedAt || row.publishedAt > filters.publishedTo)) return false;
  if (filters.effectiveFrom && row.effectiveDate < filters.effectiveFrom) return false;
  return !filters.effectiveTo || row.effectiveDate <= filters.effectiveTo;
}

function applyFilters(filters: DrawingPublishFilters) {
  appliedFilters.value = { ...filters };
  toast.success(`查询完成，共 ${filteredRows.value.length} 条图纸`);
}

function resetFilters() {
  appliedFilters.value = emptyFilters();
}

function openDetails(row: DrawingPublishRow) {
  activeRowId.value = row.id;
  detailOpen.value = true;
}

function updateStatus(status: DrawingPublishStatus) {
  const row = activeRow.value;
  if (!row) return;
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, publishStatus: status } : item));
}

function inspectDrawing() {
  updateStatus('检查通过');
  toast.success('检查结果已保存到当前本地会话');
}

function openSettings() {
  settingsDate.value = activeRow.value?.effectiveDate ?? '';
  detailOpen.value = false;
  settingsOpen.value = true;
}

function saveSettings() {
  const row = activeRow.value;
  if (!row || !settingsDate.value) return;
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, effectiveDate: settingsDate.value } : item));
  settingsOpen.value = false;
  toast.success('发布参数已保存到当前本地会话');
}

function requestPublish() {
  detailOpen.value = false;
  confirmOpen.value = true;
}

function confirmPublish() {
  const row = activeRow.value;
  if (!row) return;
  updateStatus('已发布');
  historyRows.value = [
    {
      id: `history-local-${Date.now()}`,
      drawingNo: row.drawingNo,
      name: row.name,
      publishedVersion: row.pendingVersion,
      previousVersion: row.currentVersion,
      publishedAt: '2026-08-16 21:00',
      publisher: '张经理',
      effectiveDate: row.effectiveDate,
    },
    ...historyRows.value,
  ];
  confirmOpen.value = false;
  toast.success(`${row.drawingNo} 已发布到当前本地会话`);
}

function exportRows() {
  const keys = drawingPublishModel.columns.filter((column) => column.key !== 'action').map((column) => column.key);
  const csv = [
    drawingPublishModel.columns.filter((column) => column.key !== 'action').map((column) => column.title),
    ...filteredRows.value.map((row) => keys.map((key) => String(row[key] ?? ''))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'drawing-publish.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${filteredRows.value.length} 条图纸发布记录`);
}

function refreshRows() {
  rows.value = cloneReferenceRows(drawingPublishReferenceFixture.rows);
  historyRows.value = cloneReferenceRows(drawingPublishReferenceFixture.recentRows);
  appliedFilters.value = emptyFilters();
  toast.success('图纸发布列表已刷新');
}
</script>

<template>
  <ListPageSurface
    :class="$style.surface"
    :model="pageModel"
    @row="openDetails"
  >
    <template #hero-icon
      ><span :class="$style.heroIcon"
        ><Send
          :size="22"
          :stroke-width="1.8" /></span
    ></template>
    <template #hero-art>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <span :class="$style.versionTop">v3.0</span><span :class="$style.versionMiddle">v2.5</span
        ><span :class="$style.versionBottom">v2.0</span>
        <span :class="$style.artCircle"
          ><FileUp
            :size="42"
            :stroke-width="1.4"
        /></span>
      </div>
    </template>
    <template #notice>
      <DrawingPublishFilterPanel
        @search="applyFilters"
        @reset="resetFilters"
        @history="historyOpen = true"
        @export="exportRows"
        @refresh="refreshRows"
      />
    </template>
  </ListPageSurface>

  <DrawingPublishDialogs
    :active-row="activeRow"
    :history-rows="historyRows"
    :detail-open="detailOpen"
    :history-open="historyOpen"
    :settings-open="settingsOpen"
    :confirm-open="confirmOpen"
    :settings-date="settingsDate"
    @update:detail-open="detailOpen = $event"
    @update:history-open="historyOpen = $event"
    @update:settings-open="settingsOpen = $event"
    @update:confirm-open="confirmOpen = $event"
    @update:settings-date="settingsDate = $event"
    @check="inspectDrawing"
    @settings="openSettings"
    @save-settings="saveSettings"
    @publish="requestPublish"
    @confirm-publish="confirmPublish"
  />
</template>

<style module>
@reference "../../../../styles/index.css";

.surface > :global(nav[aria-label='数据范围']),
.surface > :global(section:empty) {
  display: none;
}
.surface :global(label:has(input[aria-label='搜索图号、图纸名称或版本...'])) {
  display: none;
}
.heroIcon {
  @apply inline-flex h-11 w-11 items-center justify-center rounded border;
  color: white;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 16%);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[44%] overflow-hidden;
}
.heroArt::before,
.heroArt::after {
  content: '';
  @apply absolute rounded-full border;
  border-color: rgb(255 255 255 / 18%);
}
.heroArt::before {
  width: 210px;
  height: 210px;
  right: 150px;
  top: -45px;
}
.heroArt::after {
  width: 130px;
  height: 130px;
  right: 190px;
  top: -4px;
}
.artCircle {
  @apply absolute right-[218px] top-[29px] flex h-16 w-16 items-center justify-center rounded-full bg-white;
  color: #22a873;
}
.versionTop,
.versionMiddle,
.versionBottom {
  @apply absolute rounded-full px-3 py-1 text-[10px] font-semibold;
  color: #275c48;
  background: #f4ea79;
}
.versionTop {
  right: 82px;
  top: 10px;
}
.versionMiddle {
  right: 40px;
  top: 62px;
  background: #c9f3df;
}
.versionBottom {
  right: 88px;
  top: 108px;
  background: #e5e9ed;
}
</style>
