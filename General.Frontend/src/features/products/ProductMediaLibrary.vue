<script setup lang="ts">
import { Download, File, FileImage, Images, Plus, RefreshCw, Search, Trash2, Upload, X } from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { toast, UiSelect } from '@/components/ui';
import type { Product } from './product.types';
import { detectMediaType, formatFileSize, roleMeta } from './product.media.types';
import type { MediaAsset, MediaRole } from './product.media.types';

interface UploadTarget {
  productId: string;
  defaultRole: MediaRole;
  replaceAssetId?: string;
}

const props = defineProps<{ products: Product[]; assets: MediaAsset[] }>();
const emit = defineEmits<{
  delete: [assetIds: Set<string>];
  add: [assets: MediaAsset[]];
  replace: [assetId: string, asset: MediaAsset];
}>();

const query = ref('');
const category = ref('');
const completeness = ref('');
const selected = ref(new Set<string>());
const viewProductId = ref('');
const uploadTarget = ref<UploadTarget | null>(null);
const uploadRole = ref<MediaRole>('main');
const uploadFiles = ref<File[]>([]);
const uploadInput = ref<HTMLInputElement | null>(null);
const previewAsset = ref<MediaAsset | null>(null);

const categories = computed(() => [...new Set(props.products.map((product) => product.category))].sort());
const categoryOptions = computed(() => [
  { value: '', label: '全部分类' },
  ...categories.value.map((value) => ({ value, label: value })),
]);
const completenessOptions = [
  { value: '', label: '完整度不限' },
  { value: 'missing-main', label: '缺主图' },
  { value: 'missing-doc', label: '缺附件' },
  { value: 'complete', label: '主图+附件齐全' },
];
const byProduct = computed(() => {
  const map = new Map<string, MediaAsset[]>();
  props.assets.forEach((asset) => map.set(asset.productId, [...(map.get(asset.productId) ?? []), asset]));
  return map;
});
const filteredProducts = computed(() =>
  props.products.filter((product) => {
    const keyword = query.value.trim().toLowerCase();
    if (keyword && ![product.id, product.name, product.model].some((value) => value.toLowerCase().includes(keyword)))
      return false;
    if (category.value && product.category !== category.value) return false;
    const assets = byProduct.value.get(product.id) ?? [];
    const hasMain = assets.some((asset) => asset.role === 'main');
    const hasAttachment = assets.some((asset) => asset.role === 'attachment');
    return !(
      (completeness.value === 'missing-main' && hasMain) ||
      (completeness.value === 'missing-doc' && hasAttachment) ||
      (completeness.value === 'complete' && (!hasMain || !hasAttachment))
    );
  }),
);
const allSelected = computed(
  () => filteredProducts.value.length > 0 && filteredProducts.value.every((product) => selected.value.has(product.id)),
);
const viewingProduct = computed(() => props.products.find((product) => product.id === viewProductId.value));
const viewingAssets = computed(() => byProduct.value.get(viewProductId.value) ?? []);
const targetProduct = computed(() => props.products.find((product) => product.id === uploadTarget.value?.productId));

function notify(value: string) {
  if (value.startsWith('请先')) toast.warning(value);
  else toast.success(value);
}

function toggleAll() {
  selected.value = allSelected.value ? new Set() : new Set(filteredProducts.value.map((product) => product.id));
}

function toggleProduct(id: string) {
  const next = new Set(selected.value);
  next.has(id) ? next.delete(id) : next.add(id);
  selected.value = next;
}

function assetIdsForSelection(role?: MediaRole) {
  const ids = new Set<string>();
  selected.value.forEach((productId) =>
    (byProduct.value.get(productId) ?? []).forEach((asset) => {
      if (!role || asset.role === role) ids.add(asset.id);
    }),
  );
  return ids;
}

function batchDelete(role?: MediaRole) {
  const ids = assetIdsForSelection(role);
  emit('delete', ids);
  notify(role === 'main' ? `已删除 ${ids.size} 张主图` : `已删除 ${selected.value.size} 个商品的全部媒体资产`);
  selected.value = new Set();
}

function openUpload(target: UploadTarget) {
  uploadTarget.value = target;
  uploadRole.value = target.defaultRole;
  uploadFiles.value = [];
}

function pickFiles(event: Event) {
  const input = event.target as HTMLInputElement;
  if (input.files) {
    const picked = Array.from(input.files);
    uploadFiles.value =
      uploadRole.value === 'main' || uploadTarget.value?.replaceAssetId
        ? picked.slice(0, 1)
        : [...uploadFiles.value, ...picked];
  }
  input.value = '';
}

function confirmUpload() {
  if (!uploadTarget.value || uploadFiles.value.length === 0) {
    notify('请先选择文件');
    return;
  }
  const now = new Date().toISOString().slice(0, 19).replace('T', ' ');
  const assets = uploadFiles.value.map((file, index): MediaAsset => {
    const mediaType = detectMediaType(file.name);
    return {
      id: `MA-${Date.now()}-${index}`,
      productId: uploadTarget.value!.productId,
      role: uploadRole.value,
      mediaType,
      fileName: file.name,
      url: mediaType === 'image' ? URL.createObjectURL(file) : '#',
      size: file.size,
      uploadedAt: now,
      uploadedBy: '当前用户',
    };
  });
  if (uploadTarget.value.replaceAssetId) emit('replace', uploadTarget.value.replaceAssetId, assets[0]!);
  else emit('add', assets);
  notify(uploadTarget.value.replaceAssetId ? '文件已替换' : `已上传 ${assets.length} 个文件`);
  uploadTarget.value = null;
}

function removeAsset(id: string) {
  emit('delete', new Set([id]));
  notify('媒体资产已删除');
}

function changeRole(role: MediaRole) {
  uploadRole.value = role;
  if (role === 'main' && uploadFiles.value.length > 1) uploadFiles.value = uploadFiles.value.slice(0, 1);
}

function roleStyleClass(role: MediaRole) {
  return { main: 'roleMain', gallery: 'roleGallery', attachment: 'roleAttachment' }[role];
}

function onKeydown(event: KeyboardEvent) {
  if (event.key !== 'Escape') return;
  if (previewAsset.value) {
    previewAsset.value = null;
  } else if (uploadTarget.value) {
    uploadTarget.value = null;
  } else if (viewProductId.value) {
    viewProductId.value = '';
  }
}

watch(
  () => props.assets.length,
  () => {
    if (viewProductId.value && viewingAssets.value.length === 0) viewProductId.value = '';
  },
);
onMounted(() => document.addEventListener('keydown', onKeydown));
onUnmounted(() => {
  document.removeEventListener('keydown', onKeydown);
});
</script>

<template>
  <div :class="$style.div">
    <div :class="$style.row">
      <label :class="$style.field"
        ><Search
          :size="13"
          :class="$style.icon" /><input
          v-model="query"
          placeholder="搜索物料编码 / 商品名称 / 型号"
          :class="$style.searchInput" /></label
      ><UiSelect
        v-model="category"
        :options="categoryOptions"
        label="筛选商品分类"
        :class="$style.select"
      /><UiSelect
        v-model="completeness"
        :options="completenessOptions"
        label="筛选媒体完整度"
        :class="$style.select"
      /><span :class="$style.label">{{ filteredProducts.length }} 个商品 · {{ assets.length }} 个资产</span>
    </div>
    <div
      v-if="selected.size"
      :class="$style.rowPrimary"
    >
      <strong :class="$style.strongLabel">已选 {{ selected.size }} 个商品</strong
      ><span :class="$style.labelCompact">· 批量操作</span
      ><button
        @click="notify(`将打包导出 ${selected.size} 个商品的全部媒体资产`)"
        :class="$style.button"
      >
        <Download
          :size="12"
          :class="$style.iconMr1"
        />打包导出</button
      ><button
        @click="batchDelete('main')"
        :class="$style.buttonWarning"
      >
        <Trash2
          :size="12"
          :class="$style.iconMr1"
        />删除选中商品的主图</button
      ><button
        @click="batchDelete()"
        :class="$style.dangerButton"
      >
        <Trash2
          :size="12"
          :class="$style.iconMr1"
        />删除全部媒体</button
      ><button
        @click="selected = new Set()"
        :class="$style.iconButton"
      >
        <X :size="12" />
      </button>
    </div>

    <div :class="$style.card">
      <table :class="$style.table">
        <thead :class="$style.tableHead">
          <tr :class="$style.tableRow">
            <th :class="$style.headerCell">
              <input
                type="checkbox"
                :checked="allSelected"
                @change="toggleAll"
                :class="$style.checkbox"
              />
            </th>
            <th :class="$style.headerCellPx3Py25">物料编码</th>
            <th :class="$style.headerCellPx3Py25">商品 / 型号</th>
            <th :class="$style.headerCellCentered">主图 <small :class="$style.caption">· 限 1 张</small></th>
            <th :class="$style.headerCellW240pxPx3Py25">副图 <small :class="$style.caption">· 可多张</small></th>
            <th :class="$style.headerCellCenteredW20Px3Py25">附件</th>
            <th :class="$style.headerCellCenteredW180pxPx3Py25">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredProducts.length === 0">
            <td
              colspan="7"
              :class="$style.tableCell"
            >
              <Images
                :size="32"
                :class="$style.iconMuted"
              />无匹配商品
            </td>
          </tr>
          <tr
            v-for="product in filteredProducts"
            :key="product.id"
            :class="[$style.productRow, selected.has(product.id) ? $style.stateSelected : '']"
          >
            <td :class="$style.tableCellCentered">
              <input
                type="checkbox"
                :checked="selected.has(product.id)"
                @change="toggleProduct(product.id)"
                :class="$style.checkbox"
              />
            </td>
            <td :class="$style.tableCellPx3Py2">
              <code :class="$style.code">{{ product.id }}</code>
            </td>
            <td :class="$style.tableCellPx3Py2">
              <strong :class="$style.strongLabelAlternative">{{ product.name }}</strong
              ><small :class="$style.captionCompact">{{ product.model }} · {{ product.brand }}</small>
            </td>
            <td :class="$style.tableCellCentered">
              <template v-if="byProduct.get(product.id)?.find((asset) => asset.role === 'main')"
                ><span :class="$style.labelAlternative"
                  ><button
                    @click="previewAsset = byProduct.get(product.id)?.find((asset) => asset.role === 'main') ?? null"
                    :class="$style.buttonH11W11"
                  >
                    <img
                      :src="byProduct.get(product.id)?.find((asset) => asset.role === 'main')?.url"
                      alt=""
                      :class="$style.image"
                    /></button
                  ><button
                    title="替换主图"
                    @click="
                      openUpload({
                        productId: product.id,
                        defaultRole: 'main',
                        replaceAssetId: byProduct.get(product.id)?.find((asset) => asset.role === 'main')?.id,
                      })
                    "
                    :class="$style.replaceButton"
                  >
                    <RefreshCw
                      :size="9"
                      :class="$style.iconPrimary"
                    /></button></span></template
              ><button
                v-else
                title="点击上传主图"
                @click="openUpload({ productId: product.id, defaultRole: 'main' })"
                :class="$style.uploadButton"
              >
                <Plus :size="14" />
              </button>
            </td>
            <td :class="$style.tableCellPx3Py2">
              <div :class="$style.rowGap1">
                <button
                  v-for="asset in (byProduct.get(product.id) ?? [])
                    .filter((item) => item.role === 'gallery')
                    .slice(0, 5)"
                  :key="asset.id"
                  :title="asset.fileName"
                  @click="previewAsset = asset"
                  :class="$style.assetButton"
                >
                  <img
                    v-if="asset.mediaType === 'image'"
                    :src="asset.url"
                    alt=""
                    :class="$style.image"
                  /><File
                    v-else
                    :size="14"
                    :class="$style.iconMAuto"
                  /></button
                ><button
                  title="继续上传副图"
                  @click="openUpload({ productId: product.id, defaultRole: 'gallery' })"
                  :class="$style.uploadButtonDashed"
                >
                  <Plus :size="12" />
                </button>
              </div>
            </td>
            <td :class="$style.tableCellCentered">
              <button
                :class="[$style.buttonStrong, $style.roleAttachment]"
                @click="
                  (byProduct.get(product.id) ?? []).some((asset) => asset.role === 'attachment')
                    ? (viewProductId = product.id)
                    : openUpload({ productId: product.id, defaultRole: 'attachment' })
                "
              >
                <Plus
                  v-if="!(byProduct.get(product.id) ?? []).some((asset) => asset.role === 'attachment')"
                  :size="10"
                />{{ (byProduct.get(product.id) ?? []).filter((asset) => asset.role === 'attachment').length }}
              </button>
            </td>
            <td :class="$style.tableCellCentered">
              <button
                @click="viewProductId = product.id"
                :class="$style.buttonMr1Px2Py1"
              >
                <Images
                  :size="11"
                  :class="$style.iconMr1"
                />查看<template v-if="(byProduct.get(product.id) ?? []).length"
                  >·{{ (byProduct.get(product.id) ?? []).length }}</template
                ></button
              ><button
                @click="
                  openUpload({
                    productId: product.id,
                    defaultRole: (byProduct.get(product.id) ?? []).some((asset) => asset.role === 'main')
                      ? 'gallery'
                      : 'main',
                  })
                "
                :class="$style.primaryButton"
              >
                <Upload
                  :size="11"
                  :class="$style.iconMr1"
                />上传
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <Teleport to="body">
    <div
      v-if="viewingProduct"
      @mousedown.self="viewProductId = ''"
      :class="$style.overlay"
    >
      <section :class="$style.section">
        <header :class="$style.header">
          <div>
            <h3 :class="$style.title">媒体资产 · {{ viewingProduct.name }}</h3>
            <p :class="$style.description">{{ viewingProduct.id }} · 共 {{ viewingAssets.length }} 个文件</p>
          </div>
          <button
            @click="viewProductId = ''"
            :class="$style.iconButtonMuted"
          >
            <X :size="16" />
          </button>
        </header>
        <div :class="$style.grid">
          <article
            v-for="asset in viewingAssets"
            :key="asset.id"
            :class="$style.assetCard"
          >
            <button
              @click="previewAsset = asset"
              :class="$style.buttonCentered"
            >
              <img
                v-if="asset.mediaType === 'image'"
                :src="asset.url"
                alt=""
                :class="$style.image"
              /><FileImage
                v-else
                :size="34"
                :class="$style.icon"
              />
            </button>
            <div :class="$style.divP2">
              <span :class="[$style.badge, $style[roleStyleClass(asset.role)]]">{{ roleMeta[asset.role].label }}</span
              ><strong
                :title="asset.fileName"
                :class="$style.strongLabelCompact"
                >{{ asset.fileName }}</strong
              ><small :class="$style.captionCompactMuted">{{ formatFileSize(asset.size) }}</small>
              <div :class="$style.rowMt2Gap1">
                <button
                  @click="openUpload({ productId: asset.productId, defaultRole: asset.role, replaceAssetId: asset.id })"
                  :class="$style.primaryButtonPrimary"
                >
                  替换</button
                ><button
                  title="删除"
                  @click="removeAsset(asset.id)"
                  :class="$style.deleteButton"
                >
                  <Trash2 :size="11" />
                </button>
              </div>
            </div>
          </article>
          <button
            @click="openUpload({ productId: viewingProduct.id, defaultRole: 'gallery' })"
            :class="$style.buttonDashed"
          >
            <Plus
              :size="22"
              :class="$style.iconMb1"
            />继续上传
          </button>
        </div>
      </section>
    </div>

    <div
      v-if="uploadTarget && targetProduct"
      @mousedown.self="uploadTarget = null"
      :class="$style.overlayFloating"
    >
      <section :class="$style.sectionW560px">
        <header :class="$style.header">
          <div>
            <h3 :class="$style.title">{{ uploadTarget.replaceAssetId ? '替换文件' : '上传补传' }}</h3>
            <p :class="$style.descriptionMuted">{{ targetProduct.id }} · {{ targetProduct.name }}</p>
          </div>
          <button
            @click="uploadTarget = null"
            :class="$style.iconButtonMutedTrailing"
          >
            <X :size="16" />
          </button>
        </header>
        <div :class="$style.div">
          <label :class="$style.fieldMb15"
            >文件类型 <b :class="$style.b">*</b
            ><small :class="$style.captionMuted">{{
              uploadRole === 'main' ? '· 主图限 1 张' : '· 可多张'
            }}</small></label
          >
          <div :class="$style.gridGrid">
            <button
              v-for="role in ['main', 'gallery', 'attachment'] as MediaRole[]"
              :key="role"
              :disabled="Boolean(uploadTarget.replaceAssetId)"
              :class="[
                $style.roleButton,
                [
                  uploadRole === role ? $style.stateSelectedPrimary : $style.stateInverted,
                  uploadRole === role ? $style[roleStyleClass(role)] : '',
                ],
              ]"
              @click="changeRole(role)"
            >
              {{ roleMeta[role].label }}
            </button>
          </div>
          <label :class="$style.fieldMb15"
            >选择文件 <b :class="$style.b">*</b
            ><small
              v-if="uploadFiles.length"
              :class="$style.captionMuted"
              >已选 {{ uploadFiles.length }} 个</small
            ></label
          ><button
            @click="uploadInput?.click()"
            :class="$style.buttonDashedCentered"
          >
            <Upload
              :size="20"
              :class="$style.iconMutedMxAutoMb1"
            />{{
              uploadRole === 'main' || uploadTarget.replaceAssetId
                ? '拖拽文件至此 或 点击选择'
                : '拖拽多个文件至此 或 点击选择'
            }}<small :class="$style.captionCompactMutedMt1"
              >支持 JPG / PNG / WEBP / PDF / DOC / XLS / MP4</small
            ></button
          ><input
            ref="uploadInput"
            type="file"
            :multiple="uploadRole !== 'main' && !uploadTarget.replaceAssetId"
            @change="pickFiles"
            :class="$style.uploadInputInput"
          />
          <div
            v-if="uploadFiles.length"
            :class="$style.cardScrollable"
          >
            <div
              v-for="(file, index) in uploadFiles"
              :key="`${file.name}-${index}`"
              :class="$style.fileRow"
            >
              <File
                :size="14"
                :class="$style.iconPrimary"
              /><span :class="$style.labelWide">{{ file.name }}</span
              ><span :class="$style.labelCompactMuted">{{ formatFileSize(file.size) }}</span
              ><button
                @click="uploadFiles.splice(index, 1)"
                :class="$style.iconButtonMutedP1"
              >
                <X :size="11" />
              </button>
            </div>
          </div>
          <div
            v-if="uploadTarget.replaceAssetId"
            :class="$style.cardWarning"
          >
            替换操作会覆盖原文件，原文件将被移入回收站，可在 30 天内恢复（演示）。
          </div>
        </div>
        <footer :class="$style.footer">
          <button
            @click="uploadTarget = null"
            :class="$style.buttonPx4Py15"
          >
            取消</button
          ><button
            @click="confirmUpload"
            :class="$style.primaryButtonPrimaryInverted"
          >
            {{
              uploadTarget.replaceAssetId
                ? '确认替换'
                : `确认上传${uploadFiles.length > 1 ? ` (${uploadFiles.length})` : ''}`
            }}
          </button>
        </footer>
      </section>
    </div>

    <div
      v-if="previewAsset"
      @click="previewAsset = null"
      :class="$style.overlayFloatingCentered"
    >
      <img
        v-if="previewAsset.mediaType === 'image'"
        :src="previewAsset.url"
        alt="媒体预览"
        @click.stop
        :class="$style.imageAlternative"
      />
      <section
        v-else
        @click.stop
        :class="$style.sectionCentered"
      >
        <File
          :size="48"
          :class="$style.iconMutedMxAutoMb3"
        /><strong>{{ previewAsset.fileName }}</strong>
        <p :class="$style.descriptionCompact">{{ formatFileSize(previewAsset.size) }}</p>
      </section>
      <button
        @click="previewAsset = null"
        :class="$style.iconButtonFloating"
      >
        <X :size="16" />
      </button>
    </div>
  </Teleport>
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

/* Semantic template classes */
.div {
  @apply p-5;
}
.row {
  @apply mb-3 flex flex-wrap items-center gap-2;
}
.field {
  @apply flex w-[280px] items-center gap-1.5 rounded border border-[#DFE1E6] bg-white px-3 py-1.5;
}
.icon {
  @apply text-[#6B778C];
}
.searchInput {
  @apply min-w-0 flex-1 bg-transparent text-[12px] outline-none;
}
.select {
  @apply rounded border border-[#DFE1E6] bg-white px-2.5 py-1.5 text-[12px] text-[#42526E];
}
.label {
  @apply ml-auto text-[12px] text-[#6B778C];
}
.rowPrimary {
  @apply mb-3 flex items-center gap-2 rounded-lg border border-[#B3D4FF] bg-app-primary-soft px-3 py-2;
}
.strongLabel {
  @apply text-[12px] text-app-primary;
}
.labelCompact {
  @apply text-[11px] text-[#6B778C];
}
.button {
  @apply rounded border border-[#DFE1E6] bg-white px-2.5 py-1 text-[11px] text-[#42526E];
}
.iconMr1 {
  @apply mr-1 inline;
}
.buttonWarning {
  @apply rounded border border-[#FFE380] bg-white px-2.5 py-1 text-[11px] text-[#7A4F01];
}
.dangerButton {
  @apply rounded border border-[#FFBDAD] bg-white px-2.5 py-1 text-[11px] text-[#DE350B];
}
.iconButton {
  @apply ml-auto rounded p-1 text-[#6B778C];
}
.card {
  @apply max-h-[640px] overflow-y-auto rounded-lg border border-[#DFE1E6];
}
.table {
  @apply w-full text-[11px];
}
.tableHead {
  @apply sticky top-0 z-10 bg-[#FAFBFC];
}
.tableRow {
  @apply border-b border-[#DFE1E6] text-left font-semibold text-[#5E6C84];
}
.headerCell {
  @apply w-10 px-3 py-2.5 text-center;
}
.checkbox {
  @apply accent-app-primary;
}
.headerCellPx3Py25 {
  @apply px-3 py-2.5;
}
.headerCellCentered {
  @apply w-[78px] px-3 py-2.5 text-center;
}
.caption {
  @apply font-normal text-[#97A0AF];
}
.headerCellW240pxPx3Py25 {
  @apply w-[240px] px-3 py-2.5;
}
.headerCellCenteredW20Px3Py25 {
  @apply w-20 px-3 py-2.5 text-center;
}
.headerCellCenteredW180pxPx3Py25 {
  @apply w-[180px] px-3 py-2.5 text-center;
}
.tableCell {
  @apply py-16 text-center text-[13px] text-[#97A0AF];
}
.iconMuted {
  @apply mx-auto mb-2 text-[#C1C7D0];
}
.productRow {
  @apply border-b border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.stateSelected {
  @apply bg-app-primary-soft/30;
}
.tableCellCentered {
  @apply px-3 py-2 text-center;
}
.tableCellPx3Py2 {
  @apply px-3 py-2;
}
.code {
  @apply text-[#42526E];
}
.strongLabelAlternative {
  @apply block text-[12px] font-medium;
}
.captionCompact {
  @apply mt-0.5 block text-[10px] text-[#6B778C];
}
.labelAlternative {
  @apply relative inline-block;
}
.buttonH11W11 {
  @apply h-11 w-11 overflow-hidden rounded border border-[#DFE1E6];
}
.image {
  @apply h-full w-full object-cover;
}
.replaceButton {
  @apply absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full border border-[#DFE1E6] bg-white opacity-0 shadow group-hover:opacity-100;
}
.iconPrimary {
  @apply text-app-primary;
}
.uploadButton {
  @apply mx-auto flex h-11 w-11 items-center justify-center rounded border border-dashed border-[#FFBDAD] bg-[#FFEBE6]/40 text-[#DE350B];
}
.rowGap1 {
  @apply flex items-center gap-1;
}
.assetButton {
  @apply h-9 w-9 shrink-0 overflow-hidden rounded border border-[#DFE1E6] bg-[#F4F5F7];
}
.iconMAuto {
  @apply m-auto;
}
.uploadButtonDashed {
  @apply flex h-9 w-9 items-center justify-center rounded border border-dashed border-[#DFE1E6] text-[#97A0AF];
}
.buttonStrong {
  @apply inline-flex items-center gap-1 rounded px-2 py-0.5 font-semibold;
}
.buttonMr1Px2Py1 {
  @apply mr-1 rounded border border-[#DFE1E6] px-2 py-1 text-[#42526E];
}
.primaryButton {
  @apply rounded bg-app-primary px-2 py-1 text-white;
}
.overlay {
  @apply fixed inset-0 z-[150] flex items-center justify-center bg-black/40;
}
.section {
  @apply max-h-[80vh] w-[760px] overflow-hidden rounded-xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply flex items-center border-b border-[#DFE1E6] px-5 py-4;
}
.title {
  @apply text-[15px] font-semibold;
}
.description {
  @apply mt-0.5 text-[11px] text-[#6B778C];
}
.iconButtonMuted {
  @apply ml-auto rounded p-1.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.grid {
  @apply grid max-h-[65vh] grid-cols-4 gap-3 overflow-y-auto p-5;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.assetCard {
  @apply overflow-hidden rounded-lg border border-[#DFE1E6];
}
.buttonCentered {
  @apply flex h-28 w-full items-center justify-center bg-[#F4F5F7];
}
.divP2 {
  @apply p-2;
}
.badge {
  @apply mb-1 inline-block rounded px-1.5 py-0.5 text-[9px] font-semibold;
}
.strongLabelCompact {
  @apply block truncate text-[11px];
}
.captionCompactMuted {
  @apply text-[9px] text-[#97A0AF];
}
.rowMt2Gap1 {
  @apply mt-2 flex gap-1;
}
.primaryButtonPrimary {
  @apply flex-1 rounded border border-[#DFE1E6] px-1 py-1 text-[10px] text-app-primary;
}
.deleteButton {
  @apply rounded border border-[#FFBDAD] px-2 py-1 text-[#DE350B];
}
.buttonDashed {
  @apply flex min-h-44 flex-col items-center justify-center rounded-lg border-2 border-dashed border-[#C1C7D0] text-[11px] text-[#6B778C];
}
.iconMb1 {
  @apply mb-1;
}
.overlayFloating {
  @apply fixed inset-0 z-[170] flex items-center justify-center bg-black/40;
}
.sectionW560px {
  @apply w-[560px] overflow-hidden rounded-xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.descriptionMuted {
  @apply mt-0.5 text-[12px] text-[#6B778C];
}
.iconButtonMutedTrailing {
  @apply ml-auto rounded p-1.5 text-[#6B778C];
}
.fieldMb15 {
  @apply mb-1.5 block text-[12px] font-medium;
}
.b {
  @apply text-[#DE350B];
}
.captionMuted {
  @apply ml-2 font-normal text-[#6B778C];
}
.gridGrid {
  @apply mb-4 grid grid-cols-3 gap-1.5;
}
.roleButton {
  @apply rounded border px-2.5 py-1.5 text-[11px] disabled:cursor-not-allowed disabled:opacity-60;
}
.stateSelectedPrimary {
  @apply border-app-primary shadow-sm;
}
.stateInverted {
  @apply border-[#DFE1E6] bg-white text-[#42526E];
}
.buttonDashedCentered {
  @apply w-full rounded-lg border-2 border-dashed border-[#C1C7D0] bg-[#FAFBFC] p-5 text-center text-[12px] text-[#42526E];
}
.iconMutedMxAutoMb1 {
  @apply mx-auto mb-1 text-[#6B778C];
}
.captionCompactMutedMt1 {
  @apply mt-1 block text-[10px] text-[#97A0AF];
}
.uploadInputInput {
  @apply hidden;
}
.cardScrollable {
  @apply mt-2 max-h-40 overflow-y-auto rounded border border-[#DFE1E6];
}
.fileRow {
  @apply flex items-center gap-2 px-2.5 py-1.5 text-[11px];
}
.labelWide {
  @apply min-w-0 flex-1 truncate;
}
.labelCompactMuted {
  @apply text-[10px] text-[#97A0AF];
}
.iconButtonMutedP1 {
  @apply rounded p-1 text-[#6B778C];
}
.cardWarning {
  @apply mt-4 rounded border border-[#FFE380] bg-[#FFFAE6] p-2.5 text-[11px] leading-5 text-[#7A4F01];
}
.footer {
  @apply flex justify-end gap-2 border-t border-[#DFE1E6] bg-[#FAFBFC] px-5 py-3;
}
.buttonPx4Py15 {
  @apply rounded border border-[#DFE1E6] px-4 py-1.5 text-[12px] text-[#42526E];
}
.primaryButtonPrimaryInverted {
  @apply rounded bg-app-primary px-4 py-1.5 text-[12px] font-medium text-white;
}
.overlayFloatingCentered {
  @apply fixed inset-0 z-[190] flex items-center justify-center bg-black/70;
}
.imageAlternative {
  @apply max-h-[85vh] max-w-[90vw] rounded object-contain;
}
.sectionCentered {
  @apply rounded-xl bg-white p-10 text-center;
}
.iconMutedMxAutoMb3 {
  @apply mx-auto mb-3 text-[#6B778C];
}
.descriptionCompact {
  @apply mt-1 text-[11px] text-[#97A0AF];
}
.iconButtonFloating {
  @apply absolute right-6 top-6 rounded-full bg-white p-2 text-[#42526E];
}
</style>
