<script setup lang="ts">
import {
  Boxes,
  ClipboardList,
  FolderOpen,
  GitBranch,
  History,
  Network,
  PackageSearch,
  RotateCcw,
  Search,
  ShoppingCart,
} from 'lucide-vue-next';
import { computed, ref, type Component } from 'vue';
import { UiButton, UiInput, toast } from '@/components/ui';
import { drawingLinksReferenceFixture } from '../model/drawing-links.data';
import type { DrawingRecentQuery } from '../model/drawing-links.types';
import DrawingHero from './components/DrawingHero.vue';

const drawingNo = ref('');
const drawingName = ref('');
const version = ref('');
const material = ref('');
const hasSearched = ref(false);

const quickEntries: { id: string; title: string; description: string; icon: Component; tone: string }[] = [
  {
    id: 'bom',
    title: '查看图号关联 BOM',
    description: '查看当前图号被哪些 BOM 引用',
    icon: ClipboardList,
    tone: 'blue',
  },
  {
    id: 'purchase',
    title: '查看图号关联采购记录',
    description: '查看采购申请与采购订单',
    icon: ShoppingCart,
    tone: 'green',
  },
  {
    id: 'change',
    title: '查看图号变更与发布记录',
    description: '版本变更历史与正式发布记录',
    icon: History,
    tone: 'orange',
  },
  { id: 'stock', title: '查看图号库存追溯情况', description: '入库、批次和追溯记录', icon: Boxes, tone: 'purple' },
];
const results = computed(() =>
  hasSearched.value
    ? drawingLinksReferenceFixture.associations.filter((row) => {
        const query = [drawingNo.value, drawingName.value, version.value, material.value]
          .join(' ')
          .trim()
          .toLocaleLowerCase('zh-CN');
        return (
          !query ||
          [row.category, row.code, row.description, row.status].join(' ').toLocaleLowerCase('zh-CN').includes(query) ||
          drawingNo.value.startsWith('DWG-')
        );
      })
    : [],
);

function runSearch() {
  if (![drawingNo.value, drawingName.value, version.value, material.value].some((value) => value.trim())) return;
  hasSearched.value = true;
  toast.success(`查询完成，共找到 ${results.value.length} 项关联`);
}
function resetSearch() {
  drawingNo.value = '';
  drawingName.value = '';
  version.value = '';
  material.value = '';
  hasSearched.value = false;
}
function repeatQuery(row: DrawingRecentQuery) {
  drawingNo.value = row.drawingNo;
  drawingName.value = row.name;
  runSearch();
}
function runQuick(id: string) {
  if (!drawingNo.value) drawingNo.value = 'DWG-2025-001';
  hasSearched.value = true;
  toast.success(`已定位${quickEntries.find((item) => item.id === id)?.title ?? '关联记录'}`);
}
function exportResults() {
  if (!results.value.length) return;
  const csv = [
    '分类,业务编号,关联说明,状态',
    ...results.value.map((row) => [row.category, row.code, row.description, row.status].join(',')),
  ].join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'drawing-associations.csv';
  link.click();
  URL.revokeObjectURL(link.href);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-implementation="drawing-links"
    data-page-route="/drawings/drawing-links"
  >
    <DrawingHero
      title="图纸关联查询"
      description="围绕图号和版本查看关联物料、BOM、采购、库存、项目、售后和变更发布记录"
      :icon="Network"
      variant="association"
    />

    <section :class="$style.searchCard">
      <div :class="$style.searchGrid">
        <label
          ><span>图号</span
          ><UiInput
            v-model="drawingNo"
            placeholder="请输入图号"
        /></label>
        <label
          ><span>图纸名称</span
          ><UiInput
            v-model="drawingName"
            placeholder="请输入图纸名称"
        /></label>
        <label
          ><span>当前版本</span
          ><UiInput
            v-model="version"
            placeholder="请输入版本号"
        /></label>
        <label
          ><span>关联物料</span
          ><UiInput
            v-model="material"
            placeholder="请输入物料编码或名称"
        /></label>
      </div>
      <footer>
        <UiButton
          variant="link"
          size="sm"
          >展开高级筛选</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          :disabled="![drawingNo, drawingName, version, material].some((value) => value.trim())"
          @click="runSearch"
          ><Search
            :size="14"
            :stroke-width="1.8"
          />查询</UiButton
        ><UiButton
          size="sm"
          @click="resetSearch"
          ><RotateCcw
            :size="14"
            :stroke-width="1.8"
          />重置</UiButton
        ><UiButton
          size="sm"
          :disabled="!results.length"
          @click="exportResults"
          >导出当前结果</UiButton
        >
      </footer>
    </section>

    <section :class="$style.resultCard">
      <template v-if="!hasSearched"
        ><FolderOpen
          :size="38"
          :stroke-width="1.6"
        />
        <h2>请输入图号或相关条件后进行查询</h2>
        <p>支持按图号、图纸名称、版本、物料、采购单号、BOM 编号等条件进行关联查询</p></template
      >
      <template v-else
        ><header>
          <h2>{{ drawingNo || drawingName }} 的关联结果</h2>
          <span>共 {{ results.length }} 项</span>
        </header>
        <div :class="$style.resultGrid">
          <button
            v-for="row in results"
            :key="row.id"
            type="button"
          >
            <span>{{ row.category }}</span
            ><b>{{ row.code }}</b
            ><small>{{ row.description }}</small
            ><em>{{ row.status }}</em>
          </button>
        </div>
        <p v-if="!results.length">没有找到匹配的关联记录</p></template
      >
    </section>

    <section :class="$style.quickSection">
      <h2>高频查询入口</h2>
      <div :class="$style.quickGrid">
        <button
          v-for="entry in quickEntries"
          :key="entry.id"
          type="button"
          :data-tone="entry.tone"
          @click="runQuick(entry.id)"
        >
          <span
            ><component
              :is="entry.icon"
              :size="18"
              :stroke-width="1.8"
          /></span>
          <div>
            <strong>{{ entry.title }}</strong
            ><small>{{ entry.description }}</small>
          </div>
        </button>
      </div>
    </section>

    <section :class="$style.recentSection">
      <h2>最近查询记录</h2>
      <div :class="$style.tableCard">
        <table>
          <thead>
            <tr>
              <th>图号</th>
              <th>图纸名称</th>
              <th>查询时间</th>
              <th>来源</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in drawingLinksReferenceFixture.rows"
              :key="row.id"
            >
              <td>{{ row.drawingNo }}</td>
              <td>{{ row.name }}</td>
              <td>{{ row.queriedAt }}</td>
              <td>
                <span>{{ row.source }}</span>
              </td>
              <td>
                <button
                  type="button"
                  @click="repeatQuery(row)"
                >
                  重新查询
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.searchCard,
.resultCard,
.tableCard {
  @apply mt-4 rounded border bg-white;
  border-color: var(--ui-border);
}
.searchGrid {
  @apply grid grid-cols-4 gap-4 p-4 pb-2;
}
.searchGrid label span {
  @apply mb-1.5 block text-[10px] font-semibold;
  color: var(--ui-text-subtle);
}
.searchCard footer {
  @apply flex items-center justify-end gap-2 px-4 pb-4;
}
.searchCard footer > :first-child {
  @apply mr-auto;
}
.resultCard {
  @apply flex min-h-[150px] flex-col items-center justify-center p-6 text-center;
  color: #c1c7d0;
}
.resultCard h2 {
  @apply mt-2 text-[13px] font-semibold;
  color: var(--ui-text);
}
.resultCard > p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.resultCard > header {
  @apply flex w-full items-center justify-between border-b pb-3 text-left;
  color: var(--ui-text);
  border-color: var(--ui-border-subtle);
}
.resultCard > header h2 {
  @apply m-0;
}
.resultCard > header span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.resultGrid {
  @apply mt-4 grid w-full grid-cols-5 gap-3;
}
.resultGrid button {
  @apply flex min-h-[100px] flex-col items-start rounded border p-3 text-left transition-colors;
  border-color: var(--ui-border);
}
.resultGrid button:hover {
  border-color: #4c9aff;
  background: #f7faff;
}
.resultGrid button span {
  @apply text-[9px] font-semibold;
  color: #6554c0;
}
.resultGrid button b {
  @apply mt-1 text-[11px];
  color: #0052cc;
}
.resultGrid button small {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.resultGrid button em {
  @apply mt-auto pt-2 text-[9px] not-italic;
  color: #00875a;
}
.quickSection,
.recentSection {
  @apply mt-5;
}
.quickSection > h2,
.recentSection > h2 {
  @apply mb-3 text-[14px] font-semibold;
}
.quickGrid {
  @apply grid grid-cols-4 gap-4;
}
.quickGrid button {
  @apply flex min-h-[68px] items-center gap-3 rounded border bg-white px-4 text-left transition-colors;
  border-color: var(--ui-border);
}
.quickGrid button:hover {
  border-color: #4c9aff;
  background: #f7faff;
}
.quickGrid button > span {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.quickGrid button[data-tone='green'] > span {
  color: #00875a;
  background: #e3fcef;
}
.quickGrid button[data-tone='orange'] > span {
  color: #ff8b00;
  background: #fffae6;
}
.quickGrid button[data-tone='purple'] > span {
  color: #6554c0;
  background: #eae6ff;
}
.quickGrid button div {
  @apply min-w-0;
}
.quickGrid strong {
  @apply block text-[11px];
}
.quickGrid small {
  @apply mt-1 block truncate text-[10px];
  color: var(--ui-text-subtle);
}
.tableCard {
  @apply overflow-hidden;
}
.tableCard table {
  @apply w-full border-collapse text-left text-[11px];
}
.tableCard th {
  @apply bg-[#F4F5F7] px-5 py-3 font-semibold;
  color: var(--ui-text-subtle);
}
.tableCard td {
  @apply border-t px-5 py-3;
  border-color: var(--ui-border-subtle);
}
.tableCard td:first-child {
  @apply font-semibold;
  color: #0052cc;
}
.tableCard td span {
  @apply rounded bg-[#F4F5F7] px-2 py-1 text-[9px];
}
.tableCard td button {
  @apply font-semibold;
  color: #0052cc;
}
@media (max-width: 1000px) {
  .searchGrid,
  .quickGrid {
    @apply grid-cols-2;
  }
  .resultGrid {
    @apply grid-cols-3;
  }
}
</style>
