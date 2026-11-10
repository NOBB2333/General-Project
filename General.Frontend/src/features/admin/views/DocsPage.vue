<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  Archive,
  Download,
  FileArchive,
  FileText,
  Folder,
  FolderPlus,
  HardDrive,
  Link2,
  Search,
  Upload,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { documentFolders, documentsFixture } from '../model/docs.data';
import type { DocumentDraft, DocumentRecord, DocumentType } from '../model/docs.types';
import DocumentsTable from './docs/DocumentsTable.vue';

const documents = ref<DocumentRecord[]>(cloneReferenceRows(documentsFixture.rows));
const activeFolder = ref('全部文件');
const query = ref('');
const publisher = ref('全部发布人');
const department = ref('全部部门');
const type = ref<DocumentType | '全部类型'>('全部类型');
const sort = ref('按日期');
const folderOpen = ref(false);
const uploadOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<DocumentRecord | null>(null);
const folderName = ref('');
const draft = reactive<DocumentDraft>({ name: '', description: '', type: 'DOCX', folder: '全部文件' });

const publisherOptions: readonly UiSelectOption[] = [
  '全部发布人',
  '市场部',
  '质量部',
  '张工',
  '财务部',
  '项目经理',
  '法务部',
].map((value) => ({ value, label: value }));
const departmentOptions: readonly UiSelectOption[] = [
  '全部部门',
  '市场部',
  '技术部',
  '财务部',
  '质量部',
  '项目部',
  '法务部',
].map((value) => ({ value, label: value }));
const typeOptions: readonly UiSelectOption[] = ['全部类型', 'DOCX', 'PDF', 'PPTX', 'XLSX'].map((value) => ({
  value,
  label: value,
}));
const sortOptions: readonly UiSelectOption[] = ['按日期', '按名称', '按大小', '按下载量'].map((value) => ({
  value,
  label: value,
}));

const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return documents.value.filter(
    (item) =>
      (activeFolder.value === '全部文件' || activeFolder.value === '最近使用' || item.folder === activeFolder.value) &&
      (publisher.value === '全部发布人' || item.publisher === publisher.value) &&
      (department.value === '全部部门' || item.department === department.value) &&
      (type.value === '全部类型' || item.type === type.value) &&
      (!needle || `${item.name} ${item.description} ${item.publisher}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});

function openUpload() {
  Object.assign(draft, { name: '', description: '', type: 'DOCX', folder: activeFolder.value });
  uploadOpen.value = true;
}
function saveFolder() {
  if (!folderName.value.trim()) return toast.warning('请输入文件夹名称');
  activeFolder.value = folderName.value.trim();
  folderOpen.value = false;
  toast.success('文件夹已在本地创建');
}
function saveDocument() {
  if (!draft.name.trim()) return toast.warning('请输入文件名');
  documents.value.unshift({
    id: `doc-local-${documents.value.length + 1}`,
    name: draft.name.trim(),
    description: draft.description.trim() || '本地上传文件',
    type: draft.type,
    size: '1.0 MB',
    publisher: '张经理',
    department: '行政部',
    updatedAt: '2026-09-17',
    version: 'V1.0',
    downloads: 0,
    references: 0,
    folder: draft.folder || '全部文件',
  });
  activeFolder.value = '全部文件';
  uploadOpen.value = false;
  toast.success('文件已上传到本地文档库');
}
function openDetail(document: DocumentRecord) {
  selected.value = document;
  detailOpen.value = true;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/docs"
    data-page-label="文档中心"
    data-page-module="行政管理"
    data-docs-page
    :data-result-count="filtered.length"
    :data-total-count="documents.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 文档中心</nav>
        <section>
          <span
            ><Folder
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>文档中心</h1>
            <p>公司文档归档、版本管控、权限分配与跨业务模块文件引用</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i /><i /><i /><strong>2026年档</strong><em>归档</em>
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <div>
          <span>文档数量</span><strong>24<small>份</small></strong>
          <p>全部归档文件</p>
        </div>
        <FileText
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div>
          <span>占用空间</span><strong>157.7 MB</strong>
          <p>已存储容量</p>
        </div>
        <HardDrive
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div>
          <span>下载次数</span><strong>670<small>次</small></strong>
          <p>累计下载量</p>
        </div>
        <Download
          :size="18"
          :stroke-width="1.8"
        />
      </article>
      <article>
        <div>
          <span>引用次数</span><strong>31<small>次</small></strong>
          <p>跨业务模块引用</p>
        </div>
        <Link2
          :size="18"
          :stroke-width="1.8"
        />
      </article>
    </section>
    <section :class="$style.workspace">
      <aside :class="$style.folderPanel">
        <header>
          <strong>文件夹</strong
          ><FolderPlus
            :size="14"
            :stroke-width="1.8"
          />
        </header>
        <div>
          <button
            v-for="folder in documentFolders"
            :key="folder.name"
            :data-active="activeFolder === folder.name"
            :data-depth="folder.depth"
            @click="activeFolder = folder.name"
          >
            <Folder
              :size="14"
              :stroke-width="1.8"
            /><span>{{ folder.name }}</span
            ><em>{{ folder.count }}</em>
          </button>
        </div>
      </aside>
      <div :class="$style.documents">
        <header :class="$style.toolbar">
          <label :class="$style.search"
            ><Search
              :size="15"
              :stroke-width="1.8" /><UiInput
              v-model="query"
              aria-label="搜索文件"
              placeholder="搜索文件名、描述、上传者..."
          /></label>
          <UiSelect
            v-model="publisher"
            label="发布人"
            :options="publisherOptions"
            :class="$style.filter"
          />
          <UiSelect
            v-model="department"
            label="部门"
            :options="departmentOptions"
            :class="$style.filter"
          />
          <UiSelect
            v-model="type"
            label="类型"
            :options="typeOptions"
            :class="$style.filter"
          />
          <UiButton
            variant="secondary"
            size="sm"
            ><template #leading
              ><Archive
                :size="14"
                :stroke-width="1.8" /></template
            >高级</UiButton
          >
          <UiButton
            variant="secondary"
            size="sm"
            @click="folderOpen = true"
            ><template #leading
              ><FolderPlus
                :size="14"
                :stroke-width="1.8" /></template
            >新建文件夹</UiButton
          >
          <UiButton
            variant="primary"
            size="sm"
            @click="openUpload"
            ><template #leading
              ><Upload
                :size="14"
                :stroke-width="1.8" /></template
            >上传文件</UiButton
          >
        </header>
        <div :class="$style.listHeader">
          <strong
            >{{ activeFolder }} <span>共 {{ filtered.length }} 个</span></strong
          ><UiSelect
            v-model="sort"
            label="排序"
            :options="sortOptions"
            :class="$style.sort"
          />
        </div>
        <DocumentsTable
          :rows="filtered"
          @view="openDetail"
        />
      </div>
    </section>
  </main>
  <UiDialog
    v-model:open="folderOpen"
    title="新建文件夹"
    description="文件夹仅保存在当前本地复刻会话"
    size="sm"
  >
    <div :class="$style.dialogBody">
      <UiFormField
        label="文件夹名称"
        required
        ><UiInput
          v-model="folderName"
          aria-label="文件夹名称"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="folderOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveFolder"
        >创建</UiButton
      ></template
    >
  </UiDialog>
  <UiDialog
    v-model:open="uploadOpen"
    title="上传文件"
    description="创建本地文件记录，不会向参考站上传"
    size="md"
  >
    <div :class="$style.uploadForm">
      <UiFormField
        label="文件名"
        required
        ><UiInput
          v-model="draft.name"
          aria-label="文件名" /></UiFormField
      ><UiFormField label="描述"
        ><UiInput
          v-model="draft.description"
          aria-label="文件描述" /></UiFormField
      ><UiFormField label="文件类型"
        ><UiSelect
          v-model="draft.type"
          label="上传文件类型"
          :options="typeOptions.slice(1)" /></UiFormField
      ><UiFormField label="归档文件夹"
        ><UiInput
          v-model="draft.folder"
          aria-label="归档文件夹"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="uploadOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveDocument"
        >确认上传</UiButton
      ></template
    >
  </UiDialog>
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.name || '文件详情'"
    :description="selected?.description"
    size="md"
  >
    <div
      v-if="selected"
      :class="$style.detail"
    >
      <FileArchive
        :size="30"
        :stroke-width="1.8"
      />
      <dl>
        <div>
          <dt>文件类型</dt>
          <dd>{{ selected.type }}</dd>
        </div>
        <div>
          <dt>版本</dt>
          <dd>{{ selected.version }}</dd>
        </div>
        <div>
          <dt>上传者</dt>
          <dd>{{ selected.publisher }}</dd>
        </div>
        <div>
          <dt>更新时间</dt>
          <dd>{{ selected.updatedAt }}</dd>
        </div>
      </dl>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-3 flex min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #a84109, #d16524 58%, #f28c00);
}
.hero > div:first-child {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.78);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.28);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[54%];
  background: linear-gradient(90deg, transparent, rgba(255, 211, 135, 0.5));
}
.heroArt i {
  @apply absolute bottom-[-12px] h-16 w-40 border-2;
  border-color: #8d3c13;
  background: #f5b34d;
}
.heroArt i:nth-child(1) {
  right: 7%;
}
.heroArt i:nth-child(2) {
  right: 21%;
  bottom: -25px;
}
.heroArt i:nth-child(3) {
  right: 36%;
  bottom: -36px;
  background: #df638b;
}
.heroArt strong {
  @apply absolute right-[10%] top-8 text-[12px];
  color: #8d3c13;
}
.heroArt em {
  @apply absolute right-[8%] top-0 px-5 py-2 text-[9px] not-italic;
  color: #784010;
  background: #ffd76a;
}
.metrics {
  @apply mb-3 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[112px] items-start rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > div {
  @apply flex-1;
}
.metrics article > svg {
  @apply rounded p-2;
  box-sizing: content-box;
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(2) > svg {
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(3) > svg {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(4) > svg {
  color: #6554c0;
  background: #eae6ff;
}
.metrics span,
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply my-3 block text-[20px] font-semibold;
}
.metrics small {
  @apply ml-1 text-[9px];
  color: var(--ui-text-subtle);
}
.workspace {
  @apply flex items-start gap-4;
}
.folderPanel {
  @apply w-[330px] shrink-0 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.folderPanel header {
  @apply flex h-12 items-center border-b px-4;
  border-color: var(--ui-border);
}
.folderPanel header strong {
  @apply mr-auto text-[12px];
}
.folderPanel > div {
  @apply max-h-[540px] overflow-y-auto p-2;
}
.folderPanel button {
  @apply flex h-9 w-full items-center gap-2 rounded px-3 text-[10px];
  color: var(--ui-text-muted);
}
.folderPanel button[data-depth='1'] {
  @apply pl-8;
}
.folderPanel button:hover {
  background: var(--ui-surface-hover);
}
.folderPanel button[data-active='true'] {
  @apply font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.folderPanel button span {
  @apply min-w-0 flex-1 text-left;
}
.folderPanel button em {
  @apply not-italic;
  color: var(--ui-text-subtle);
}
.documents {
  @apply min-w-0 flex-1;
}
.toolbar {
  @apply flex h-16 items-center gap-2 rounded border bg-white px-3;
  border-color: var(--ui-border);
}
.search {
  @apply flex min-w-[220px] flex-1 items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.filter {
  @apply w-[118px];
}
.listHeader {
  @apply flex h-14 items-center;
}
.listHeader strong {
  @apply text-[11px];
}
.listHeader strong span {
  @apply ml-1 font-normal;
  color: var(--ui-text-subtle);
}
.sort {
  @apply ml-auto w-[100px];
}
.dialogBody {
  @apply p-5;
}
.uploadForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detail {
  @apply p-5;
}
.detail > svg {
  @apply mb-4 rounded p-3;
  box-sizing: content-box;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.detail dl {
  @apply grid grid-cols-2 gap-3;
}
.detail dl div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[11px] font-medium;
}
</style>
