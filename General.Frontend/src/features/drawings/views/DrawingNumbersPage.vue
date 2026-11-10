<script setup lang="ts">
import {
  Archive,
  ChevronRight,
  Circle,
  File,
  Folder,
  FolderOpen,
  Link2,
  Search,
  Shield,
  TableProperties,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiButton, UiDialog, UiInput, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { drawingNumbersReferenceFixture } from '../model/drawing-numbers.data';
import type { DrawingArchiveRow } from '../model/drawing-numbers.types';
import DrawingHero from './components/DrawingHero.vue';

const mode = ref('tree');
const activeFolder = ref('mechanical');
const query = ref('');
const selected = ref<DrawingArchiveRow | null>(null);
const borrowOpen = ref(false);
const borrower = ref('');

const visibleRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return drawingNumbersReferenceFixture.rows.filter((row) => {
    const inFolder = ['all', 'mechanical'].includes(activeFolder.value) || row.folder === activeFolder.value;
    const matches =
      !normalized ||
      [row.drawingNo, row.name, row.type, row.status].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(normalized),
      );
    return inFolder && matches;
  });
});

function selectFolder(id: string) {
  activeFolder.value = id;
  selected.value = null;
}
function confirmBorrow() {
  if (!borrower.value.trim()) return;
  borrowOpen.value = false;
  toast.success(`借用申请已提交给 ${borrower.value}`);
  borrower.value = '';
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-implementation="drawing-numbers"
    data-page-route="/drawings/drawing-numbers"
  >
    <DrawingHero
      title="图号档案"
      description="按图号/项目/设备分类归档，支持权限管控与跨部门借阅"
      :icon="Archive"
      variant="archive"
    />

    <UiTabs v-model="mode">
      <UiTabsList
        :class="$style.modeTabs"
        label="图号档案视图"
      >
        <UiTabsTrigger
          value="tree"
          :class="$style.modeTab"
          ><FolderOpen
            :size="15"
            :stroke-width="1.8"
          />文件资源树</UiTabsTrigger
        >
        <UiTabsTrigger
          value="ledger"
          :class="$style.modeTab"
          ><TableProperties
            :size="15"
            :stroke-width="1.8"
          />台账模式</UiTabsTrigger
        >
      </UiTabsList>
    </UiTabs>

    <section :class="$style.workspace">
      <aside :class="$style.treePanel">
        <header>
          <h2>
            <FolderOpen
              :size="15"
              :stroke-width="1.8"
            />文件目录
          </h2>
          <button
            type="button"
            aria-label="新建文件夹"
          >
            <Folder
              :size="15"
              :stroke-width="1.8"
            />
          </button>
        </header>
        <div :class="$style.treeModes">
          <button
            type="button"
            data-active
          >
            档案模式</button
          ><button type="button">
            <Shield
              :size="13"
              :stroke-width="1.8"
            />权限管理
          </button>
        </div>
        <label :class="$style.treeSearch"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            placeholder="搜索文件夹..."
            aria-label="搜索文件夹"
        /></label>
        <nav aria-label="图纸文件夹">
          <button
            v-for="folder in drawingNumbersReferenceFixture.folders"
            :key="folder.id"
            type="button"
            :class="$style.folderButton"
            :data-active="activeFolder === folder.id || undefined"
            :data-depth="folder.depth"
            @click="selectFolder(folder.id)"
          >
            <ChevronRight
              v-if="folder.depth === 0"
              :size="12"
              :stroke-width="1.8"
            />
            <Folder
              :size="15"
              :stroke-width="1.8"
            />
            <span>{{ folder.label }}</span
            ><small>{{ folder.count }}</small>
          </button>
        </nav>
      </aside>

      <div :class="$style.archiveArea">
        <section :class="$style.archiveCard">
          <header>
            <h2>
              <Folder
                :size="16"
                :stroke-width="1.8"
              />{{
                drawingNumbersReferenceFixture.folders.find((item) => item.id === activeFolder)?.label ?? '机械图纸'
              }}
            </h2>
            <div>
              <UiButton
                size="sm"
                @click="borrowOpen = true"
                ><Link2
                  :size="14"
                  :stroke-width="1.8"
                />借用图纸</UiButton
              ><span>共 {{ visibleRows.length }} 张图纸</span>
            </div>
          </header>
          <div :class="$style.tableScroller">
            <table>
              <thead>
                <tr>
                  <th>图号</th>
                  <th>图纸名称</th>
                  <th>类型</th>
                  <th>版本</th>
                  <th>状态</th>
                  <th>PDF</th>
                  <th>CAD</th>
                  <th>3D</th>
                  <th>更新时间</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="row in visibleRows"
                  :key="row.id"
                  :data-selected="selected?.id === row.id || undefined"
                  @click="selected = row"
                >
                  <td>
                    <button type="button">
                      <File
                        :size="13"
                        :stroke-width="1.8"
                      />{{ row.drawingNo }}
                    </button>
                  </td>
                  <td>{{ row.name }}</td>
                  <td>{{ row.type }}</td>
                  <td>
                    <span :class="$style.version">{{ row.version }}</span>
                  </td>
                  <td>
                    <span
                      :class="$style.status"
                      :data-tone="row.status === '已发布' ? 'success' : row.status === '变更中' ? 'blue' : 'danger'"
                      >{{ row.status }}</span
                    >
                  </td>
                  <td>
                    <Circle
                      :class="$style.fileDot"
                      :data-available="row.pdf || undefined"
                      :size="9"
                    />
                  </td>
                  <td>
                    <Circle
                      :class="$style.fileDot"
                      :data-available="row.cad || undefined"
                      :size="9"
                    />
                  </td>
                  <td>
                    <Circle
                      :class="$style.fileDot"
                      :data-available="row.model3d || undefined"
                      :size="9"
                    />
                  </td>
                  <td>{{ row.updatedAt }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
        <section :class="$style.detailPanel">
          <template v-if="selected"
            ><header>
              <strong>{{ selected.drawingNo }} · {{ selected.name }}</strong
              ><UiButton
                size="sm"
                variant="primary"
                @click="borrowOpen = true"
                >申请借用</UiButton
              >
            </header>
            <dl>
              <div>
                <dt>图纸类型</dt>
                <dd>{{ selected.type }}</dd>
              </div>
              <div>
                <dt>当前版本</dt>
                <dd>{{ selected.version }}</dd>
              </div>
              <div>
                <dt>状态</dt>
                <dd>{{ selected.status }}</dd>
              </div>
              <div>
                <dt>更新时间</dt>
                <dd>{{ selected.updatedAt }}</dd>
              </div>
            </dl></template
          >
          <p v-else>ⓘ　点击上方图纸查看属性信息</p>
        </section>
      </div>
    </section>

    <UiDialog
      v-model:open="borrowOpen"
      title="借用图纸"
      description="借用申请只保存在当前本地会话"
      size="sm"
    >
      <form
        :class="$style.borrowForm"
        @submit.prevent="confirmBorrow"
      >
        <label
          ><span>借用人 / 部门</span
          ><UiInput
            v-model="borrower"
            placeholder="请输入借用人或部门"
            autofocus
        /></label>
        <p>图纸：{{ selected?.drawingNo ?? '请先选择图纸' }}</p>
      </form>
      <template #footer
        ><UiButton
          size="sm"
          @click="borrowOpen = false"
          >取消</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          :disabled="!borrower.trim()"
          @click="confirmBorrow"
          >提交申请</UiButton
        ></template
      >
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.modeTabs {
  @apply mt-3 gap-0 border-b bg-transparent p-0;
  border-color: var(--ui-border);
}
.modeTab {
  @apply h-[42px] gap-2 rounded-none px-5;
  background: transparent;
}
.modeTab[data-state='active'] {
  color: #0052cc;
  background: transparent;
  box-shadow: inset 0 -2px 0 #0052cc;
}
.workspace {
  @apply mt-4 grid grid-cols-[270px_minmax(0,1fr)] gap-4;
}
.treePanel,
.archiveCard,
.detailPanel {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.treePanel {
  @apply min-h-[580px];
}
.treePanel > header,
.archiveCard > header {
  @apply flex h-[52px] items-center justify-between border-b px-4;
  border-color: var(--ui-border);
}
.treePanel h2,
.archiveCard h2 {
  @apply flex items-center gap-2 text-[13px] font-semibold;
}
.treePanel > header button {
  color: var(--ui-text-subtle);
}
.treeModes {
  @apply grid grid-cols-2 gap-1 p-2;
}
.treeModes button {
  @apply flex h-8 items-center justify-center gap-1 rounded text-[11px];
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
.treeModes button[data-active] {
  @apply font-semibold text-white;
  background: #0052cc;
}
.treeSearch {
  @apply mx-2 flex h-8 items-center gap-2 rounded px-2;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
.treeSearch :global(input) {
  @apply h-7 border-0 bg-transparent p-0 shadow-none;
}
.treePanel nav {
  @apply mt-2;
}
.folderButton {
  @apply flex h-8 w-full items-center gap-2 px-3 text-left text-[11px] transition-colors;
  color: var(--ui-text-muted);
}
.folderButton:hover {
  background: #f4f5f7;
}
.folderButton[data-active] {
  color: #0052cc;
  background: #deebff;
}
.folderButton[data-depth='1'] {
  @apply pl-8;
}
.folderButton small {
  @apply ml-auto;
  color: #97a0af;
}
.archiveArea {
  @apply min-w-0;
}
.archiveCard > header h2 svg {
  color: #ff8b00;
}
.archiveCard > header > div {
  @apply flex items-center gap-3 text-[11px];
  color: var(--ui-text-subtle);
}
.tableScroller {
  @apply overflow-x-auto;
}
.tableScroller table {
  @apply w-full min-w-[900px] border-collapse text-left text-[11px];
}
.tableScroller th {
  @apply bg-[#F4F5F7] px-4 py-3 font-semibold;
  color: var(--ui-text-subtle);
}
.tableScroller td {
  @apply border-t px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.tableScroller tbody tr {
  @apply cursor-pointer transition-colors;
}
.tableScroller tbody tr:hover,
.tableScroller tbody tr[data-selected] {
  background: #f7faff;
}
.tableScroller td button {
  @apply flex items-center gap-2 font-semibold;
  color: #0052cc;
}
.version {
  @apply rounded bg-[#F4F5F7] px-2 py-1 text-[9px] font-semibold;
}
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
}
.status[data-tone='success'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-tone='blue'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-tone='danger'] {
  color: #de350b;
  background: #ffebe6;
}
.fileDot {
  fill: transparent;
  color: #c1c7d0;
}
.fileDot[data-available] {
  fill: #00875a;
  color: #00875a;
}
.detailPanel {
  @apply mt-4 min-h-[200px];
}
.detailPanel > p {
  @apply flex min-h-[200px] items-center justify-center text-[11px];
  color: #97a0af;
}
.detailPanel > header {
  @apply flex items-center justify-between border-b px-5 py-3;
  border-color: var(--ui-border);
}
.detailPanel dl {
  @apply grid grid-cols-4 gap-3 p-5;
}
.detailPanel dl div {
  @apply rounded bg-[#FAFBFC] p-3;
}
.detailPanel dt {
  @apply text-[10px];
  color: #7a869a;
}
.detailPanel dd {
  @apply mt-1 text-[12px] font-semibold;
}
.borrowForm {
  @apply grid gap-4 p-5;
}
.borrowForm label span {
  @apply mb-1.5 block text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.borrowForm p {
  @apply rounded bg-[#F4F5F7] p-3 text-[11px];
}
@media (max-width: 900px) {
  .workspace {
    @apply grid-cols-1;
  }
  .treePanel {
    @apply min-h-0;
  }
}
</style>
