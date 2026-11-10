<script setup lang="ts">
import {
  Camera,
  Check,
  CheckCircle2,
  FileImage,
  Folder,
  ImagePlus,
  Info,
  QrCode,
  Settings2,
  SlidersHorizontal,
  Sparkles,
  Trash2,
  Upload,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiSelect } from '@/components/ui';
import type { Product } from './product.types';
import ProductMediaRuleDrawer from './ProductMediaRuleDrawer.vue';
import { detectMediaType, formatFileSize, roleMeta } from './product.media.types';
import type { MediaAsset, MediaMatchStatus, MediaRole, MediaRule, ParsedMediaFile } from './product.media.types';

const props = defineProps<{ products: Product[]; assets: MediaAsset[] }>();
const emit = defineEmits<{ commit: [assets: MediaAsset[]] }>();

type UploadState = 'idle' | 'parsing' | 'preview' | 'importing' | 'done';
const state = ref<UploadState>('idle');
const files = ref<ParsedMediaFile[]>([]);
const filter = ref<'all' | MediaMatchStatus>('all');
const ruleOpen = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const folderInput = ref<HTMLInputElement | null>(null);
const rule = ref<MediaRule>({ name: '编码 + 序号', separator: '_', primary: 'either', classifier: 'sequence' });
const result = ref({ success: 0, skipped: 0, total: 0 });

const stats = computed(() => ({
  total: files.value.length,
  matched: files.value.filter((item) => item.status === 'matched').length,
  unmatched: files.value.filter((item) => item.status === 'unmatched').length,
  duplicate: files.value.filter((item) => item.status === 'duplicate').length,
  ignored: files.value.filter((item) => item.status === 'ignored').length,
}));
const visibleFiles = computed(() =>
  filter.value === 'all' ? files.value : files.value.filter((item) => item.status === filter.value),
);
const productSelectOptions = computed(() => [
  { value: '', label: '未匹配 · 请手动选择' },
  ...props.products.map((product) => ({ value: product.id, label: `${product.id} · ${product.name}` })),
]);
const roleOptions = [
  { value: 'main', label: '主图' },
  { value: 'gallery', label: '副图' },
  { value: 'attachment', label: '附件' },
];

function inferRole(fileName: string): MediaRole {
  const lower = fileName.toLowerCase();
  const mediaType = detectMediaType(fileName);
  if (mediaType !== 'image' || /(规格书|说明书|附件|attachment|manual|datasheet)/i.test(lower)) return 'attachment';
  if (/(副图|铭牌|gallery|detail|side)/i.test(lower)) return 'gallery';
  const basename = fileName.replace(/\.[^.]+$/, '');
  const sequence = basename.match(/(?:_|-|\s)(\d+)$/)?.[1];
  return sequence && Number(sequence) >= 2 ? 'gallery' : 'main';
}

function findProduct(fileName: string) {
  const lower = fileName.toLowerCase();
  return props.products.find((product) =>
    [product.id, product.model, product.barcode].some((value) => value && lower.includes(value.toLowerCase())),
  );
}

function hasMain(productId: string) {
  return props.assets.some((asset) => asset.productId === productId && asset.role === 'main');
}

function parseFiles(fileList: FileList | File[]) {
  const incoming = Array.from(fileList);
  if (incoming.length === 0) return;
  state.value = 'parsing';
  window.setTimeout(() => {
    const parsed = incoming.map((file, index): ParsedMediaFile => {
      const product = findProduct(file.name);
      const role = inferRole(file.name);
      const mediaType = detectMediaType(file.name);
      const basename = file.name.replace(/\.[^.]+$/, '');
      const sequence = basename.match(/(?:_|-|\s)(\d+)$/)?.[1] ?? '';
      const conflict = Boolean(product && role === 'main' && hasMain(product.id));
      return {
        id: `PF-${Date.now()}-${index}`,
        fileName: file.name,
        size: file.size,
        mediaType,
        previewUrl: mediaType === 'image' ? URL.createObjectURL(file) : '',
        parsedIdentifier: product?.id ?? basename.split(rule.value.separator)[0] ?? basename,
        parsedSequence: sequence,
        role,
        matchedProductId: product?.id ?? '',
        matchConfidence: product ? (file.name.includes(product.id) ? 100 : 96) : 0,
        status: product ? (conflict ? 'duplicate' : 'matched') : 'unmatched',
        conflictNote: conflict ? '该商品已有主图' : '',
      };
    });
    files.value = [...files.value, ...parsed];
    state.value = 'preview';
  }, 420);
}

function onFilesChange(event: Event) {
  const input = event.target as HTMLInputElement;
  if (input.files) parseFiles(input.files);
  input.value = '';
}

function onDrop(event: DragEvent) {
  event.preventDefault();
  if (event.dataTransfer?.files) parseFiles(event.dataTransfer.files);
}

function updateProduct(item: ParsedMediaFile, productId: string) {
  item.matchedProductId = productId;
  item.matchConfidence = productId ? 100 : 0;
  item.conflictNote = productId && item.role === 'main' && hasMain(productId) ? '该商品已有主图' : '';
  item.status = !productId ? 'unmatched' : item.conflictNote ? 'duplicate' : 'matched';
}

function updateRole(item: ParsedMediaFile, role: MediaRole) {
  item.role = role;
  updateProduct(item, item.matchedProductId);
}

function removeFile(id: string) {
  const item = files.value.find((entry) => entry.id === id);
  if (item?.previewUrl) URL.revokeObjectURL(item.previewUrl);
  files.value = files.value.filter((entry) => entry.id !== id);
  if (files.value.length === 0) reset();
}

function ignoreFile(item: ParsedMediaFile) {
  item.status = item.status === 'ignored' ? (item.matchedProductId ? 'matched' : 'unmatched') : 'ignored';
}

function commit() {
  const importable = files.value.filter((item) => item.status === 'matched' && item.matchedProductId);
  state.value = 'importing';
  window.setTimeout(() => {
    const now = new Date().toISOString().slice(0, 19).replace('T', ' ');
    const assets = importable.map((item, index): MediaAsset => ({
      id: `MA-${Date.now()}-${index}`,
      productId: item.matchedProductId,
      role: item.role,
      mediaType: item.mediaType,
      fileName: item.fileName,
      url: item.previewUrl || '#',
      size: item.size,
      uploadedAt: now,
      uploadedBy: '当前用户',
    }));
    emit('commit', assets);
    result.value = { success: assets.length, skipped: files.value.length - assets.length, total: files.value.length };
    state.value = 'done';
  }, 900);
}

function reset() {
  files.value.forEach((item) => {
    if (item.previewUrl && state.value !== 'done') URL.revokeObjectURL(item.previewUrl);
  });
  files.value = [];
  filter.value = 'all';
  state.value = 'idle';
}

function saveRule(nextRule: MediaRule) {
  rule.value = nextRule;
  ruleOpen.value = false;
}

function roleStyleClass(role: MediaRole) {
  return { main: 'roleMain', gallery: 'roleGallery', attachment: 'roleAttachment' }[role];
}

function filterActiveClass(key: string) {
  return (
    {
      all: 'filterAllActive',
      matched: 'filterMatchedActive',
      unmatched: 'filterUnmatchedActive',
      duplicate: 'filterDuplicateActive',
    }[key] ?? 'filterAllActive'
  );
}

function filterCountClass(key: string) {
  return (
    {
      all: 'filterAllCount',
      matched: 'filterMatchedCount',
      unmatched: 'filterUnmatchedCount',
      duplicate: 'filterDuplicateCount',
    }[key] ?? 'filterAllCount'
  );
}
</script>

<template>
  <div
    v-if="state === 'idle'"
    :class="$style.div"
  >
    <div :class="$style.row">
      <span :class="$style.label"
        ><Sparkles
          :size="20"
          :class="$style.icon"
      /></span>
      <div :class="$style.rowWide">
        <h3 :class="$style.title">批量上传 · 自动匹配商品 · 智能归类</h3>
        <p :class="$style.description">
          支持一次拖入数百张图片或文档，系统按文件名自动识别物料编码 / 型号 / 条码 / 客户别名，并归类到「主图 / 副图 /
          铭牌 / 规格书 / 说明书」等角色卡槽。
        </p>
      </div>
    </div>
    <div
      @click="fileInput?.click()"
      @dragover.prevent
      @drop="onDrop"
      :class="$style.card"
    >
      <div :class="$style.rowColumn">
        <span :class="$style.labelRound"
          ><Upload
            :size="28"
            :class="$style.icon"
        /></span>
        <div>
          <h3 :class="$style.titleStrong">拖拽文件到此处，或点击选择</h3>
          <p :class="$style.descriptionMuted">支持 JPG / PNG / WEBP / HEIC / PDF / DOC / XLS / MP4，单文件≤50MB</p>
        </div>
        <div :class="$style.rowMt2Gap2">
          <button
            @click.stop="fileInput?.click()"
            :class="$style.primaryButton"
          >
            <FileImage :size="14" />选择文件</button
          ><button
            @click.stop="folderInput?.click()"
            :class="$style.button"
          >
            <Folder :size="14" />选择整个文件夹
          </button>
        </div>
      </div>
    </div>
    <input
      ref="fileInput"
      type="file"
      multiple
      accept="image/*,application/pdf,.doc,.docx,.xls,.xlsx,video/*"
      @change="onFilesChange"
      :class="$style.fileInputInput"
    /><input
      ref="folderInput"
      type="file"
      multiple
      webkitdirectory
      @change="onFilesChange"
      :class="$style.fileInputInput"
    />

    <section :class="$style.section">
      <header :class="$style.header">
        <ImagePlus
          :size="13"
          :class="$style.icon"
        /><strong :class="$style.strongLabel">当前命名规则</strong
        ><span :class="$style.labelCompact">· 所有上传文件按此规则解析</span
        ><button
          @click="ruleOpen = true"
          :class="$style.primaryButtonPrimary"
        >
          <Settings2 :size="12" />编辑规则
        </button>
      </header>
      <div :class="$style.rowGap4Px4Py4">
        <div :class="$style.rowWide">
          <strong :class="$style.strongLabel">{{ rule.name }}</strong>
          <div :class="$style.rowMt15Gap1">
            <span :class="$style.badge">{{
              rule.primary === 'code' ? '物料编码' : rule.primary === 'model' ? '产品型号' : '编码或型号'
            }}</span
            ><span :class="$style.labelMuted">{{ rule.separator }}</span
            ><span :class="$style.badgePurple">{{ rule.classifier === 'sequence' ? '序号' : '类型关键词' }}</span>
          </div>
        </div>
        <div :class="$style.divRightAligned">
          <small :class="$style.caption">示例文件名</small
          ><code :class="$style.code"
            >SP-2025-00001{{ rule.separator }}{{ rule.classifier === 'sequence' ? '1' : '副图' }}.jpg</code
          >
        </div>
      </div>
      <div :class="$style.divMuted">
        <div :class="$style.divCompact">三种文件类型</div>
        <div :class="$style.rowGap2">
          <div
            v-for="role in ['main', 'gallery', 'attachment'] as MediaRole[]"
            :key="role"
            :class="$style.roleRow"
          >
            <span :class="[$style.badgeCompact, $style[roleStyleClass(role)]]">{{ roleMeta[role].label }}</span
            ><span :class="$style.labelCompactMuted">{{ role === 'main' ? '每个商品限 1 张' : '可多份' }}</span>
          </div>
        </div>
      </div>
      <div :class="$style.rowWarning">
        <Info
          :size="12"
          :class="$style.iconWarning"
        />系统使用上述这一条规则解析所有上传文件。不符合规则的文件需在预览页手动改派。
      </div>
    </section>
    <div :class="$style.grid">
      <div
        v-for="item in [
          { icon: QrCode, title: '扫码上传', desc: '生成商品二维码 → 手机扫码直传，铭牌/包装现拍现传' },
          { icon: Camera, title: '拍照即识别', desc: '上传铭牌照片，AI 自动 OCR 型号并匹配商品' },
          { icon: Sparkles, title: 'AI 视觉相似', desc: '未命名照片按外观匹配候选商品，置信度排序' },
        ]"
        :key="item.title"
        :class="$style.itemCard"
      >
        <span :class="$style.badgeWarning">即将上线</span>
        <div :class="$style.rowMb1Gap2">
          <span :class="$style.labelPrimary"
            ><component
              :is="item.icon"
              :size="16" /></span
          ><strong :class="$style.strongLabelAlternative">{{ item.title }}</strong>
        </div>
        <p :class="$style.descriptionCompact">{{ item.desc }}</p>
      </div>
    </div>
  </div>

  <div
    v-else-if="state === 'parsing' || state === 'importing'"
    :class="$style.rowCentered"
  >
    <span :class="$style.labelPrimaryRound" />
    <h3 :class="$style.titleStrong">{{ state === 'parsing' ? '正在解析文件...' : '正在导入媒体资产...' }}</h3>
    <p :class="$style.descriptionMuted">
      {{ state === 'parsing' ? '识别文件名、匹配商品并检查主图冲突' : `共 ${files.length} 个文件` }}
    </p>
  </div>

  <div
    v-else-if="state === 'done'"
    :class="$style.rowCenteredColumn"
  >
    <span :class="$style.labelSuccess"
      ><CheckCircle2
        :size="32"
        :class="$style.iconSuccess"
    /></span>
    <h3 :class="$style.titleStrongMb1">批量导入完成</h3>
    <p :class="$style.descriptionMb5">
      成功 <b :class="$style.iconSuccess">{{ result.success }}</b> 个 · 跳过
      <b :class="$style.iconWarning">{{ result.skipped }}</b> 个 · 共 {{ result.total }} 个
    </p>
    <div :class="$style.rowGap2Gap2">
      <button
        @click="reset"
        :class="$style.primaryButtonPrimaryInverted"
      >
        <Upload :size="14" />继续上传</button
      ><button
        @click="reset"
        :class="$style.buttonPx4Py2"
      >
        返回
      </button>
    </div>
  </div>

  <div
    v-else
    :class="$style.divP5"
  >
    <div :class="$style.gridGrid">
      <button
        v-for="card in [
          { key: 'all', label: '全部', count: stats.total },
          { key: 'matched', label: '已匹配', count: stats.matched },
          { key: 'unmatched', label: '未匹配', count: stats.unmatched },
          { key: 'duplicate', label: '冲突', count: stats.duplicate },
        ]"
        :key="card.key"
        :class="[
          $style.cardButton,
          [
            filter === card.key ? $style.stateSelected : $style.stateInverted,
            filter === card.key ? $style[filterActiveClass(card.key)] : '',
          ],
        ]"
        @click="filter = card.key as typeof filter"
      >
        <span>{{ card.label }}</span
        ><b :class="[$style.b, $style[filterCountClass(card.key)]]">{{ card.count }}</b>
      </button>
    </div>
    <div :class="$style.rowMb3Gap2">
      <button
        @click="fileInput?.click()"
        :class="$style.buttonMuted"
      >
        <Upload
          :size="13"
          :class="$style.iconMr1"
        />继续添加</button
      ><button
        @click="reset"
        :class="$style.buttonMuted"
      >
        <Trash2
          :size="13"
          :class="$style.iconMr1"
        />清空</button
      ><button
        @click="ruleOpen = true"
        :class="$style.buttonMuted"
      >
        <SlidersHorizontal
          :size="13"
          :class="$style.iconMr1"
        />命名规则</button
      ><span :class="$style.labelMutedTrailing"
        >可导入 <b :class="$style.iconSuccess">{{ stats.matched }}</b> / 跳过
        <b :class="$style.bDanger">{{ stats.unmatched + stats.duplicate }}</b></span
      ><button
        :disabled="stats.matched === 0"
        @click="commit"
        :class="$style.primaryButtonSuccess"
      >
        <Check
          :size="13"
          :class="$style.iconMr1"
        />确认导入 ({{ stats.matched }})
      </button>
    </div>
    <div :class="$style.cardScrollable">
      <table :class="$style.table">
        <colgroup>
          <col :class="$style.col" />
          <col />
          <col />
          <col :class="$style.colW28" />
          <col :class="$style.colW28" />
          <col :class="$style.col" />
        </colgroup>
        <thead :class="$style.tableHead">
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">预览</th>
            <th :class="$style.headerCell">文件名 / 解析结果</th>
            <th :class="$style.headerCell">匹配商品</th>
            <th :class="$style.headerCell">角色</th>
            <th :class="$style.headerCellCentered">策略 / 置信度</th>
            <th :class="$style.headerCellCentered">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="visibleFiles.length === 0">
            <td
              colspan="6"
              :class="$style.tableCell"
            >
              无符合条件的文件
            </td>
          </tr>
          <tr
            v-for="item in visibleFiles"
            :key="item.id"
            :class="$style.itemRow"
          >
            <td :class="$style.tableCellP3">
              <img
                v-if="item.previewUrl"
                :src="item.previewUrl"
                alt=""
                :class="$style.image"
              /><span
                v-else
                :class="$style.labelCentered"
                ><FileImage
                  :size="16"
                  :class="$style.iconMuted"
              /></span>
            </td>
            <td :class="$style.tableCellP3">
              <strong :class="$style.strongLabelAlternativeAlternative">{{ item.fileName }}</strong
              ><span :class="$style.labelCompactMutedMt1"
                >{{ item.parsedIdentifier
                }}<template v-if="item.parsedSequence"> · 序号 {{ item.parsedSequence }}</template> ·
                {{ formatFileSize(item.size) }}</span
              ><span
                v-if="item.conflictNote"
                :class="$style.badgePurpleCompact"
                >{{ item.conflictNote }}</span
              >
            </td>
            <td :class="$style.tableCellP3">
              <UiSelect
                :model-value="item.matchedProductId"
                :options="productSelectOptions"
                label="选择匹配商品"
                :class="$style.select"
                @update:model-value="updateProduct(item, $event)"
              />
            </td>
            <td :class="$style.tableCellP3">
              <UiSelect
                :model-value="item.role"
                :options="roleOptions"
                label="选择媒体角色"
                :class="$style.select"
                @update:model-value="updateRole(item, $event as MediaRole)"
              />
            </td>
            <td :class="$style.tableCellCentered">
              <span
                :class="[
                  $style.labelPx15Py05,
                  item.status === 'matched'
                    ? $style.stateSuccess
                    : item.status === 'duplicate'
                      ? $style.statePurple
                      : item.status === 'ignored'
                        ? $style.stateMuted
                        : $style.stateDanger,
                ]"
                >{{
                  item.status === 'matched'
                    ? `自动 · ${item.matchConfidence}%`
                    : item.status === 'duplicate'
                      ? '冲突'
                      : item.status === 'ignored'
                        ? '已忽略'
                        : '未匹配'
                }}</span
              >
            </td>
            <td :class="$style.tableCellCentered">
              <button
                @click="ignoreFile(item)"
                :class="$style.buttonMutedMr1Px1Py05"
              >
                {{ item.status === 'ignored' ? '恢复' : '忽略' }}</button
              ><button
                title="移除"
                @click="removeFile(item.id)"
                :class="$style.removeButton"
              >
                <Trash2 :size="12" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <ProductMediaRuleDrawer
    :open="ruleOpen"
    :rule="rule"
    @close="ruleOpen = false"
    @save="saveRule"
  />
</template>

<style module>
@reference "../../styles/index.css";
.roleMain {
  color: #0052cc;
  background-color: #deebff;
}
.roleGallery {
  color: #5243aa;
  background-color: #eae6ff;
}
.roleAttachment {
  color: #00875a;
  background-color: #e3fcef;
}

.filterAllActive {
  background-color: #f4f5f7;
}
.filterMatchedActive {
  background-color: #e3fcef;
}
.filterUnmatchedActive {
  background-color: #ffebe6;
}
.filterDuplicateActive {
  background-color: #eae6ff;
}
.filterAllCount {
  color: #42526e;
}
.filterMatchedCount {
  color: #00875a;
}
.filterUnmatchedCount {
  color: #de350b;
}
.filterDuplicateCount {
  color: #5243aa;
}

/* Semantic template classes */
.div {
  @apply p-6;
}
.row {
  @apply mb-5 flex h-[73.7px] items-start gap-3 rounded-lg border border-[#B3D4FF] bg-gradient-to-r p-4;
}
.label {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white;
}
.icon {
  @apply text-app-primary;
}
.rowWide {
  @apply min-w-0 flex-1;
}
.title {
  @apply mb-1 text-[14px] font-semibold;
}
.description {
  @apply text-[12px] leading-5 text-[#42526E];
}
.card {
  @apply cursor-pointer rounded-xl border-2 border-dashed border-[#C1C7D0] bg-[#FAFBFC] p-10 text-center transition-colors hover:border-[#4C9AFF] hover:bg-app-primary-soft/20;
}
.rowColumn {
  @apply flex flex-col items-center gap-3;
}
.labelRound {
  @apply flex h-16 w-16 items-center justify-center rounded-full border border-[#DFE1E6] bg-white;
}
.titleStrong {
  @apply text-[15px] font-semibold;
}
.descriptionMuted {
  @apply mt-1 text-[12px] text-[#6B778C];
}
.rowMt2Gap2 {
  @apply mt-2 flex gap-2;
}
.primaryButton {
  @apply flex items-center gap-1.5 rounded bg-app-primary px-4 py-2 text-[13px] font-medium text-white hover:bg-app-primary-hover;
}
.button {
  @apply flex items-center gap-1.5 rounded border border-[#DFE1E6] bg-white px-4 py-2 text-[13px] text-[#42526E] hover:bg-[#F4F5F7];
}
.fileInputInput {
  @apply hidden;
}
.section {
  @apply mt-5 overflow-hidden rounded-lg border border-[#DFE1E6] bg-white;
}
.header {
  @apply flex items-center gap-2 border-b border-[#DFE1E6] bg-[#FAFBFC] px-4 py-3;
}
.strongLabel {
  @apply text-[13px];
}
.labelCompact {
  @apply text-[11px] text-[#6B778C];
}
.primaryButtonPrimary {
  @apply ml-auto flex items-center gap-1 rounded bg-app-primary px-2.5 py-1 text-[11px] font-medium text-white hover:bg-app-primary-hover;
}
.rowGap4Px4Py4 {
  @apply flex items-center gap-4 px-4 py-4;
}
.rowMt15Gap1 {
  @apply mt-1.5 flex items-center gap-1;
}
.badge {
  @apply rounded bg-app-primary-soft px-2 py-0.5 text-[11px] font-semibold text-app-primary-hover;
}
.labelMuted {
  @apply mx-1 text-[12px] text-[#97A0AF];
}
.badgePurple {
  @apply rounded bg-[#EAE6FF] px-2 py-0.5 text-[11px] font-semibold text-[#403294];
}
.divRightAligned {
  @apply shrink-0 text-right;
}
.caption {
  @apply block text-[10px] text-[#97A0AF];
}
.code {
  @apply mt-1 inline-block rounded bg-[#F4F5F7] px-2.5 py-1.5 text-[12px] font-semibold text-app-primary;
}
.divMuted {
  @apply border-t border-[#F4F5F7] bg-[#FAFBFC]/40 px-4 py-3;
}
.divCompact {
  @apply mb-1.5 text-[11px] font-semibold text-[#42526E];
}
.rowGap2 {
  @apply flex flex-wrap gap-2;
}
.roleRow {
  @apply flex items-center gap-1.5 rounded border border-[#DFE1E6] bg-white px-2 py-1;
}
.badgeCompact {
  @apply rounded px-1.5 py-0.5 text-[10px] font-semibold;
}
.labelCompactMuted {
  @apply text-[10px] text-[#6B778C];
}
.rowWarning {
  @apply flex items-center gap-2 border-t border-[#FFE380] bg-[#FFFAE6] px-4 py-2.5 text-[11px] text-[#7A4F01];
}
.iconWarning {
  @apply text-[#FF8B00];
}
.grid {
  @apply mt-5 grid grid-cols-3 gap-3;
}
.itemCard {
  @apply relative rounded-lg border border-dashed border-[#DFE1E6] bg-white p-3 transition-colors hover:bg-[#FAFBFC];
}
.badgeWarning {
  @apply absolute right-2 top-2 rounded bg-[#FFFAE6] px-1.5 py-0.5 text-[9px] font-semibold text-[#7A4F01];
}
.rowMb1Gap2 {
  @apply mb-1 flex items-center gap-2;
}
.labelPrimary {
  @apply flex h-8 w-8 items-center justify-center rounded-lg bg-app-primary-soft text-app-primary;
}
.strongLabelAlternative {
  @apply text-[12px];
}
.descriptionCompact {
  @apply text-[11px] leading-[1.5] text-[#6B778C];
}
.rowCentered {
  @apply flex min-h-[430px] flex-col items-center justify-center;
}
.labelPrimaryRound {
  @apply mb-4 h-12 w-12 rounded-full border-4 border-app-primary-soft border-t-[#0052CC];
}
.rowCenteredColumn {
  @apply flex min-h-[430px] flex-col items-center justify-center p-10;
}
.labelSuccess {
  @apply mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#E3FCEF];
}
.iconSuccess {
  @apply text-[#00875A];
}
.titleStrongMb1 {
  @apply mb-1 text-[17px] font-semibold;
}
.descriptionMb5 {
  @apply mb-5 text-[13px] text-[#42526E];
}
.rowGap2Gap2 {
  @apply flex gap-2;
}
.primaryButtonPrimaryInverted {
  @apply flex items-center gap-1.5 rounded bg-app-primary px-4 py-2 text-[13px] text-white;
}
.buttonPx4Py2 {
  @apply rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#42526E];
}
.divP5 {
  @apply p-5;
}
.gridGrid {
  @apply mb-4 grid grid-cols-4 gap-2;
}
.cardButton {
  @apply flex items-center justify-between rounded-lg border px-3 py-2 text-[11px] font-medium;
}
.stateSelected {
  @apply border-app-primary shadow-sm;
}
.stateInverted {
  @apply border-[#DFE1E6] bg-white;
}
.b {
  @apply text-[18px];
}
.rowMb3Gap2 {
  @apply mb-3 flex items-center gap-2;
}
.buttonMuted {
  @apply rounded border border-[#DFE1E6] px-3 py-1.5 text-[12px] text-[#42526E] hover:bg-[#F4F5F7];
}
.iconMr1 {
  @apply mr-1 inline;
}
.labelMutedTrailing {
  @apply ml-auto text-[12px] text-[#6B778C];
}
.bDanger {
  @apply text-[#DE350B];
}
.primaryButtonSuccess {
  @apply rounded bg-[#00875A] px-4 py-1.5 text-[12px] font-medium text-white disabled:cursor-not-allowed disabled:opacity-40;
}
.cardScrollable {
  @apply max-h-[520px] overflow-y-auto rounded-lg border border-[#DFE1E6];
}
.table {
  @apply w-full table-fixed text-[11px];
}
.col {
  @apply w-16;
}
.colW28 {
  @apply w-28;
}
.tableHead {
  @apply sticky top-0 z-10 bg-[#FAFBFC];
}
.tableRow {
  @apply border-b border-[#DFE1E6] text-left text-[#5E6C84];
}
.headerCell {
  @apply px-3 py-2.5;
}
.headerCellCentered {
  @apply px-3 py-2.5 text-center;
}
.tableCell {
  @apply py-12 text-center text-[13px] text-[#97A0AF];
}
.itemRow {
  @apply border-t border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.tableCellP3 {
  @apply p-3;
}
.image {
  @apply h-10 w-10 rounded object-cover;
}
.labelCentered {
  @apply flex h-10 w-10 items-center justify-center rounded bg-[#F4F5F7];
}
.iconMuted {
  @apply text-[#6B778C];
}
.strongLabelAlternativeAlternative {
  @apply block truncate text-[12px];
}
.labelCompactMutedMt1 {
  @apply mt-1 block text-[10px] text-[#6B778C];
}
.badgePurpleCompact {
  @apply mt-1 inline-block rounded bg-[#EAE6FF] px-1.5 py-0.5 text-[9px] text-[#5243AA];
}
.select {
  @apply h-8 w-full rounded border border-[#DFE1E6] bg-white px-2 text-[11px];
}
.tableCellCentered {
  @apply p-3 text-center;
}
.labelPx15Py05 {
  @apply rounded px-1.5 py-0.5;
}
.stateSuccess {
  @apply bg-[#E3FCEF] text-[#00875A];
}
.statePurple {
  @apply bg-[#EAE6FF] text-[#5243AA];
}
.stateMuted {
  @apply bg-[#F4F5F7] text-[#6B778C];
}
.stateDanger {
  @apply bg-[#FFEBE6] text-[#DE350B];
}
.buttonMutedMr1Px1Py05 {
  @apply mr-1 rounded px-1 py-0.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.removeButton {
  @apply rounded p-1 text-[#6B778C] hover:bg-[#FFEBE6] hover:text-[#DE350B];
}
</style>
