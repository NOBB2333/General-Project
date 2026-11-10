<script setup lang="ts">
import {
  ArrowLeft,
  Barcode,
  Box,
  Check,
  ChevronRight,
  Eye,
  FileText,
  History,
  Minus,
  Plus,
  Printer,
  QrCode,
  Search,
  Trash2,
  X,
} from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import type { Product } from './product.types';

type CodeType = 'barcode' | 'qr';
type SourceType = 'product' | 'purchase' | 'sale' | 'quote';
interface PrintRecord {
  id: number;
  date: string;
  names: string;
  type: CodeType;
  size: string;
  count: number;
}

const props = defineProps<{ open: boolean; products: Product[] }>();
const emit = defineEmits<{ close: [] }>();
const codeType = ref<CodeType>('barcode');
const source = ref<SourceType>('product');
const size = ref('50x30mm');
const query = ref('');
const step = ref<'select' | 'preview'>('select');
const quantities = ref<Record<string, number>>({});
const showHistory = ref(false);
const history = ref<PrintRecord[]>([]);
const printed = ref(false);

const sourceTabs: { id: SourceType; label: string }[] = [
  { id: 'product', label: '按商品' },
  { id: 'purchase', label: '采购单' },
  { id: 'sale', label: '销售单' },
  { id: 'quote', label: '报价单' },
];
const sizes = ['40x30mm', '50x30mm', '60x40mm', '70x50mm', '80x60mm'];
const filtered = computed(() =>
  props.products.filter(
    (item) =>
      item.barcode &&
      (!query.value ||
        [item.name, item.model, item.barcode].some((value) => value.toLowerCase().includes(query.value.toLowerCase()))),
  ),
);
const selectedProducts = computed(() => props.products.filter((item) => quantities.value[item.id]));
const totalLabels = computed(() => Object.values(quantities.value).reduce((sum, quantity) => sum + quantity, 0));

function reset() {
  codeType.value = 'barcode';
  source.value = 'product';
  size.value = '50x30mm';
  query.value = '';
  step.value = 'select';
  quantities.value = {};
  showHistory.value = false;
  printed.value = false;
}
function toggle(product: Product) {
  const next = { ...quantities.value };
  if (next[product.id]) delete next[product.id];
  else next[product.id] = 1;
  quantities.value = next;
}
function adjust(id: string, amount: number) {
  const next = { ...quantities.value };
  const value = Math.max(1, Math.min(999, (next[id] || 1) + amount));
  next[id] = value;
  quantities.value = next;
}
function setQuantity(id: string, event: Event) {
  const value = Number((event.target as HTMLInputElement).value);
  quantities.value = { ...quantities.value, [id]: Math.max(1, Math.min(999, value || 1)) };
}
function clearSelection() {
  quantities.value = {};
  step.value = 'select';
}
function openPreview() {
  if (totalLabels.value) {
    step.value = 'preview';
    printed.value = false;
  }
}
function printLabels() {
  history.value = [
    {
      id: Date.now(),
      date: new Date().toLocaleString('zh-CN', { hour12: false }),
      names: selectedProducts.value.map((item) => item.name).join('、'),
      type: codeType.value,
      size: size.value,
      count: totalLabels.value,
    },
    ...history.value,
  ];
  printed.value = true;
}
function restore(record: PrintRecord) {
  const names = record.names.split('、');
  const restored: Record<string, number> = {};
  props.products
    .filter((product) => names.includes(product.name))
    .forEach((product) => {
      restored[product.id] = 1;
    });
  quantities.value = restored;
  codeType.value = record.type;
  size.value = record.size;
  showHistory.value = false;
  step.value = 'select';
}
function importSampleDocument() {
  const sample: Record<string, number> = {};
  props.products
    .filter((product) => product.barcode)
    .slice(0, 3)
    .forEach((product, index) => {
      sample[product.id] = index + 1;
    });
  quantities.value = sample;
  source.value = 'product';
}

function onKeydown(event: KeyboardEvent) {
  if (!props.open || event.key !== 'Escape') return;
  if (showHistory.value) showHistory.value = false;
  else emit('close');
}

watch(
  () => props.open,
  (open) => {
    if (open) reset();
  },
);
onMounted(() => document.addEventListener('keydown', onKeydown));
onUnmounted(() => document.removeEventListener('keydown', onKeydown));
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      @mousedown.self="emit('close')"
      :class="$style.overlay"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="商品条码打印"
        :class="$style.dialog"
      >
        <header :class="$style.header">
          <div :class="$style.row">
            <span :class="$style.label"
              ><QrCode
                v-if="codeType === 'qr'"
                :size="20" /><Barcode
                v-else
                :size="20"
            /></span>
            <div>
              <h2 :class="$style.title">商品{{ codeType === 'qr' ? '二维码' : '条码' }}打印</h2>
              <p :class="$style.description">
                {{ step === 'select' ? '选择物料并设置打印数量' : `预览 ${totalLabels} 张标签 · ${size}` }}
              </p>
            </div>
          </div>
          <div :class="$style.rowGap2">
            <button
              :class="[$style.button, showHistory ? $style.stateSelected : $style.stateMuted]"
              @click="showHistory = !showHistory"
            >
              <History :size="13" />打印记录<span
                v-if="history.length"
                :class="$style.badge"
                >{{ history.length }}</span
              >
            </button>
            <div :class="$style.rowAlternative">
              <button
                :class="[
                  $style.buttonGap15Px3Py15,
                  codeType === 'barcode' ? $style.stateSelectedPrimary : $style.stateMutedMuted,
                ]"
                @click="codeType = 'barcode'"
              >
                <Barcode :size="13" />条形码</button
              ><button
                :class="[
                  $style.buttonGap15Px3Py15,
                  codeType === 'qr' ? $style.stateSelectedPrimary : $style.stateMutedMuted,
                ]"
                @click="codeType = 'qr'"
              >
                <QrCode :size="13" />二维码
              </button>
            </div>
            <button
              @click="emit('close')"
              :class="$style.iconButton"
            >
              <X :size="18" />
            </button>
          </div>
        </header>

        <section
          v-if="showHistory"
          :class="$style.section"
        >
          <div :class="$style.rowMuted">
            <History
              :size="14"
              :class="$style.icon"
            /><strong :class="$style.strongLabel">打印历史记录</strong
            ><span :class="$style.labelCompact">最近 {{ history.length }} 条</span
            ><button
              v-if="history.length"
              @click="history = []"
              :class="$style.dangerButton"
            >
              清空全部
            </button>
          </div>
          <div
            v-if="!history.length"
            :class="$style.div"
          >
            <History
              :size="24"
              :class="$style.iconMuted"
            />
            <p :class="$style.descriptionMuted">暂无打印记录</p>
          </div>
          <div
            v-else
            :class="$style.list"
          >
            <div
              v-for="record in history"
              :key="record.id"
              :class="$style.recordRow"
            >
              <span
                :class="[
                  $style.labelCentered,
                  record.type === 'qr' ? $style.statePurple : $style.stateSelectedPrimaryAlternative,
                ]"
                ><QrCode
                  v-if="record.type === 'qr'"
                  :size="14" /><Barcode
                  v-else
                  :size="14"
              /></span>
              <div :class="$style.rowWide">
                <p :class="$style.descriptionAlternative">{{ record.names }}</p>
                <p :class="$style.descriptionCompact">
                  {{ record.date }} | {{ record.type === 'qr' ? '二维码' : '条形码' }} | {{ record.size }} |
                  {{ record.count }} 张
                </p>
              </div>
              <button
                @click="restore(record)"
                :class="$style.primaryButton"
              >
                恢复</button
              ><button
                @click="history = history.filter((item) => item.id !== record.id)"
                :class="$style.dangerButtonDanger"
              >
                <Trash2 :size="12" />
              </button>
            </div>
          </div>
        </section>

        <div :class="$style.rowMutedWithBottomDivider">
          <button
            :class="[
              $style.iconButtonRound,
              step === 'select' ? $style.stateSelectedPrimaryInverted : $style.stateInverted,
            ]"
            @click="step = 'select'"
          >
            <span
              :class="[
                $style.labelRound,
                step === 'select' ? $style.stateInvertedAlternative : $style.stateMutedMutedAlternative,
              ]"
              >1</span
            >选择物料</button
          ><ChevronRight
            :size="14"
            :class="$style.iconMutedMx2"
          /><button
            :class="[
              $style.iconButtonRound,
              step === 'preview' ? $style.stateSelectedPrimaryInverted : $style.stateInverted,
            ]"
            @click="openPreview"
          >
            <span
              :class="[
                $style.labelRound,
                step === 'preview' ? $style.stateInvertedAlternative : $style.stateMutedMutedAlternative,
              ]"
              >2</span
            >预览打印
          </button>
          <div :class="$style.rowWideAlternative" />
          <div
            v-if="selectedProducts.length"
            :class="$style.rowMutedGap2"
          >
            已选 <strong :class="$style.strongLabelAlternative">{{ selectedProducts.length }}</strong> 种 / 共
            <strong :class="$style.strongLabelAlternative">{{ totalLabels }}</strong> 张<button
              @click="clearSelection"
              :class="$style.dangerButtonDangerCompact"
            >
              清空
            </button>
          </div>
        </div>

        <div
          v-if="step === 'select'"
          :class="$style.rowWideAlternativeAlternative"
        >
          <aside :class="$style.sidePanel">
            <div :class="$style.grid">
              <button
                v-for="tab in sourceTabs"
                :key="tab.id"
                :class="[$style.tabButton, source === tab.id ? $style.stateSelectedPrimaryMuted : $style.stateDefault]"
                @click="
                  source = tab.id;
                  query = '';
                "
              >
                <component
                  :is="tab.id === 'product' ? Box : FileText"
                  :size="12"
                />{{ tab.label
                }}<span
                  v-if="source === tab.id"
                  :class="$style.labelPrimary"
                />
              </button>
            </div>
            <template v-if="source === 'product'"
              ><label :class="$style.field"
                ><Search
                  :size="14"
                  :class="$style.iconMutedAlternative" /><input
                  v-model="query"
                  placeholder="搜索商品名称、型号、条码..."
                  :class="$style.searchInput"
              /></label>
              <div :class="$style.listScrollable">
                <button
                  v-for="product in filtered"
                  :key="product.id"
                  :class="[
                    $style.productButton,
                    quantities[product.id] ? $style.stateSuccess : $style.stateMutedMutedWithTopDivider,
                  ]"
                  @click="toggle(product)"
                >
                  <span
                    :class="[
                      $style.labelCenteredH7W7,
                      quantities[product.id]
                        ? $style.stateSuccessSuccess
                        : $style.stateMutedMutedAlternativeAlternative,
                    ]"
                    ><Check
                      v-if="quantities[product.id]"
                      :size="13" /><Box
                      v-else
                      :size="13" /></span
                  ><span :class="$style.rowWide"
                    ><strong :class="$style.strongLabelAlternativeAlternative">{{ product.name }}</strong
                    ><small :class="$style.caption">{{ product.model }} | {{ product.barcode }}</small></span
                  ><span
                    v-if="quantities[product.id]"
                    :class="$style.badgeSuccess"
                    >×{{ quantities[product.id] }}</span
                  >
                </button>
              </div></template
            >
            <div
              v-else
              :class="$style.rowCentered"
            >
              <FileText
                :size="34"
                :class="$style.iconMutedMb3"
              />
              <p :class="$style.descriptionAlternativeAlternative">
                选择{{ source === 'purchase' ? '采购单' : source === 'sale' ? '销售单' : '报价单' }}
              </p>
              <p :class="$style.descriptionCompactMuted">可按单据批量导入其中的商品与数量</p>
              <button
                @click="importSampleDocument"
                :class="$style.primaryButtonPrimary"
              >
                导入示例单据
              </button>
            </div>
          </aside>

          <section :class="$style.sectionColumn">
            <div :class="$style.rowMutedWithBottomDividerPx4Py25">
              <strong :class="$style.strongLabelCompact">标签尺寸:</strong>
              <div :class="$style.rowGap1">
                <button
                  v-for="option in sizes"
                  :key="option"
                  :class="[$style.optionButton, size === option ? $style.stateSelected : $style.stateMuted]"
                  @click="size = option"
                >
                  {{ option }}
                </button>
              </div>
            </div>
            <div
              v-if="!selectedProducts.length"
              :class="$style.rowCenteredColumn"
            >
              <span :class="$style.labelCenteredMuted"
                ><QrCode
                  v-if="codeType === 'qr'"
                  :size="28" /><Barcode
                  v-else
                  :size="28"
              /></span>
              <h3 :class="$style.titleMuted">暂无待打印标签</h3>
              <p :class="$style.descriptionMutedMt1">从左侧选择商品或导入订单</p>
            </div>
            <div
              v-else
              :class="$style.listScrollableWide"
            >
              <div
                v-for="product in selectedProducts"
                :key="product.id"
                :class="$style.productRow"
              >
                <span :class="$style.labelCenteredMutedH52pxW100px"
                  ><QrCode
                    v-if="codeType === 'qr'"
                    :size="44" /><Barcode
                    v-else
                    :size="58"
                /></span>
                <div :class="$style.rowWide">
                  <strong :class="$style.strongLabelAlternativeAlternativeAlternative">{{ product.name }}</strong
                  ><span :class="$style.labelCompactMuted"
                    >{{ product.model }}{{ product.spec ? ` | ${product.spec}` : '' }}</span
                  ><span :class="$style.labelCompactMutedAlternative">{{ product.barcode }}</span>
                </div>
                <div :class="$style.rowGap1Gap1">
                  <button
                    @click="adjust(product.id, -1)"
                    :class="$style.buttonCentered"
                  >
                    <Minus :size="12" /></button
                  ><input
                    :value="quantities[product.id]"
                    @change="setQuantity(product.id, $event)"
                    :class="$style.input"
                  /><button
                    @click="adjust(product.id, 1)"
                    :class="$style.buttonCentered"
                  >
                    <Plus :size="12" />
                  </button>
                </div>
                <button
                  @click="toggle(product)"
                  :class="$style.dangerButtonDangerMuted"
                >
                  <Trash2 :size="13" />
                </button>
              </div>
            </div>
          </section>
        </div>

        <div
          v-else
          :class="$style.rowColumn"
        >
          <div :class="$style.rowMutedWithBottomDividerPx5Py25">
            <div>
              <strong :class="$style.strongLabelAlternativeAlternativeAlternativeAlternative">打印预览</strong
              ><span :class="$style.labelCompactMutedMl2">标签尺寸：{{ size }}</span>
            </div>
            <span :class="$style.description">共 {{ totalLabels }} 张</span>
          </div>
          <div :class="$style.rowScrollable">
            <div :class="$style.gridGrid">
              <template
                v-for="product in selectedProducts"
                :key="product.id"
                ><div
                  v-for="number in quantities[product.id]"
                  :key="`${product.id}-${number}`"
                  :class="$style.numberRow"
                >
                  <strong :class="$style.strongLabelAlternativeAlternativeAlternativeAlternativeAlternative">{{
                    product.name
                  }}</strong
                  ><QrCode
                    v-if="codeType === 'qr'"
                    :size="66"
                    :class="$style.iconMy2"
                  />
                  <div
                    v-else
                    :class="$style.rowCenteredMy3H12W150px"
                  />
                  <span :class="$style.labelCompactAlternative">{{ product.barcode }}</span
                  ><span :class="$style.labelCompactMutedAlternativeAlternative">{{ product.model }}</span>
                </div></template
              >
            </div>
          </div>
        </div>

        <footer :class="$style.footer">
          <Printer
            :size="15"
            :class="$style.icon"
          /><span :class="$style.labelMuted"
            >总计 <strong :class="$style.strongLabelAlternative">{{ selectedProducts.length }}</strong> 种商品 /
            <strong :class="$style.strongLabelAlternative">{{ totalLabels }}</strong> 张标签</span
          ><template v-if="step === 'select'"
            ><button
              @click="emit('close')"
              :class="$style.buttonMuted"
            >
              取消</button
            ><button
              :disabled="!totalLabels"
              @click="openPreview"
              :class="$style.primaryButtonPrimaryCentered"
            >
              <Eye :size="13" />预览标签
            </button></template
          ><template v-else
            ><button
              @click="step = 'select'"
              :class="$style.buttonTrailing"
            >
              <ArrowLeft :size="13" />返回调整</button
            ><button
              @click="printLabels"
              :class="$style.primaryButtonPrimaryInverted"
            >
              <Printer :size="13" />{{ printed ? '已发送打印任务' : `打印 ${totalLabels} 张标签` }}
            </button></template
          >
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.overlay {
  @apply fixed inset-0 z-[100] flex items-center justify-center bg-black/50;
}
.dialog {
  @apply flex max-h-[92vh] min-h-[713.8px] w-[1060px] flex-col overflow-hidden rounded-2xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply flex shrink-0 items-center justify-between border-b border-[#DFE1E6] bg-gradient-to-r px-6 py-4;
}
.row {
  @apply flex items-center gap-3;
}
.label {
  @apply flex h-10 w-10 items-center justify-center rounded-xl bg-app-primary-soft text-app-primary;
}
.title {
  @apply text-[17px] font-bold;
}
.description {
  @apply text-[11px] text-[#6B778C];
}
.rowGap2 {
  @apply flex items-center gap-2;
}
.button {
  @apply flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-[12px] font-medium transition-colors;
}
.stateSelected {
  @apply border-app-primary bg-app-primary-soft text-app-primary;
}
.stateMuted {
  @apply border-[#DFE1E6] text-[#6B778C] hover:bg-[#F4F5F7];
}
.badge {
  @apply rounded bg-app-primary/10 px-1 text-[9px] font-bold text-app-primary;
}
.rowAlternative {
  @apply flex overflow-hidden rounded-lg border border-[#DFE1E6];
}
.buttonGap15Px3Py15 {
  @apply flex items-center gap-1.5 px-3 py-1.5 text-[12px] font-medium;
}
.stateSelectedPrimary {
  @apply bg-app-primary text-white;
}
.stateMutedMuted {
  @apply text-[#42526E] hover:bg-[#F4F5F7];
}
.iconButton {
  @apply rounded-lg p-1.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.section {
  @apply flex max-h-[260px] shrink-0 flex-col border-b border-[#EBECF0] bg-[#FAFBFC];
}
.rowMuted {
  @apply flex items-center border-b border-[#EBECF0] px-5 py-2.5;
}
.icon {
  @apply mr-2 text-[#6B778C];
}
.strongLabel {
  @apply text-[13px];
}
.labelCompact {
  @apply ml-2 text-[11px] text-[#97A0AF];
}
.dangerButton {
  @apply ml-auto text-[11px] text-[#FF5630];
}
.div {
  @apply py-8 text-center;
}
.iconMuted {
  @apply mx-auto mb-2 text-[#C1C7D0];
}
.descriptionMuted {
  @apply text-[12px] text-[#97A0AF];
}
.list {
  @apply space-y-1.5 overflow-y-auto px-4 py-2;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.recordRow {
  @apply flex items-center gap-3 rounded-lg border border-[#EBECF0] bg-white px-3 py-2.5 hover:border-[#B3D4FF];
}
.labelCentered {
  @apply flex h-8 w-8 items-center justify-center rounded-lg;
}
.statePurple {
  @apply bg-[#EAE6FF] text-[#5243AA];
}
.stateSelectedPrimaryAlternative {
  @apply bg-app-primary-soft text-app-primary;
}
.rowWide {
  @apply min-w-0 flex-1;
}
.descriptionAlternative {
  @apply truncate text-[12px] font-medium;
}
.descriptionCompact {
  @apply text-[10px] text-[#97A0AF];
}
.primaryButton {
  @apply rounded-md bg-app-primary px-2.5 py-1 text-[10px] font-medium text-white opacity-0 group-hover:opacity-100;
}
.dangerButtonDanger {
  @apply rounded p-1 text-[#97A0AF] opacity-0 hover:bg-[#FFEBE6] hover:text-[#FF5630] group-hover:opacity-100;
}
.rowMutedWithBottomDivider {
  @apply flex shrink-0 items-center border-b border-[#EBECF0] bg-[#FAFBFC] px-6 py-2.5;
}
.iconButtonRound {
  @apply flex items-center gap-2 rounded-full px-4 py-1.5 text-[12px] font-semibold;
}
.stateSelectedPrimaryInverted {
  @apply bg-app-primary text-white shadow-sm;
}
.stateInverted {
  @apply border border-[#DFE1E6] bg-white text-[#42526E];
}
.labelRound {
  @apply flex h-5 w-5 items-center justify-center rounded-full;
}
.stateInvertedAlternative {
  @apply bg-white/20;
}
.stateMutedMutedAlternative {
  @apply bg-[#F4F5F7];
}
.iconMutedMx2 {
  @apply mx-2 text-[#C1C7D0];
}
.rowWideAlternative {
  @apply flex-1;
}
.rowMutedGap2 {
  @apply flex items-center gap-2 text-[12px] text-[#6B778C];
}
.strongLabelAlternative {
  @apply text-[#172B4D];
}
.dangerButtonDangerCompact {
  @apply ml-1 text-[11px] text-[#FF5630];
}
.rowWideAlternativeAlternative {
  @apply flex min-h-0 flex-1 overflow-hidden;
}
.sidePanel {
  @apply flex w-[380px] shrink-0 flex-col border-r border-[#EBECF0] bg-white;
}
.grid {
  @apply grid h-[37px] grid-cols-4 border-b border-[#EBECF0] text-[11px];
}
.tabButton {
  @apply relative flex items-center justify-center gap-1 py-2.5 font-semibold text-[#6B778C];
}
.stateSelectedPrimaryMuted {
  @apply bg-[#FAFBFC] text-app-primary;
}
.stateDefault {
  @apply hover:text-[#172B4D];
}
.labelPrimary {
  @apply absolute inset-x-0 bottom-0 h-0.5 bg-app-primary;
}
.field {
  @apply mx-3 mt-2.5 flex items-center gap-2 rounded-lg bg-[#F4F5F7] px-3 py-2;
}
.iconMutedAlternative {
  @apply text-[#97A0AF];
}
.searchInput {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none placeholder:text-[#97A0AF];
}
.listScrollable {
  @apply mt-2.5 flex-1 space-y-0.5 overflow-y-auto px-3 pb-3;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.productButton {
  @apply flex w-full items-center gap-2.5 rounded-lg border px-3 py-2.5 text-left;
}
.stateSuccess {
  @apply border-[#ABF5D1] bg-[#E3FCEF]/60;
}
.stateMutedMutedWithTopDivider {
  @apply border-transparent hover:bg-[#F4F5F7];
}
.labelCenteredH7W7 {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded-md;
}
.stateSuccessSuccess {
  @apply bg-[#00875A] text-white;
}
.stateMutedMutedAlternativeAlternative {
  @apply bg-[#F4F5F7] text-[#97A0AF];
}
.strongLabelAlternativeAlternative {
  @apply block truncate text-[12px] font-medium;
}
.caption {
  @apply block truncate text-[10px] text-[#97A0AF];
}
.badgeSuccess {
  @apply rounded bg-[#00875A]/10 px-1.5 py-0.5 text-[10px] font-bold text-[#00875A];
}
.rowCentered {
  @apply flex flex-1 flex-col items-center justify-center px-8 text-center;
}
.iconMutedMb3 {
  @apply mb-3 text-[#C1C7D0];
}
.descriptionAlternativeAlternative {
  @apply text-[13px] font-medium;
}
.descriptionCompactMuted {
  @apply mt-1 text-[11px] text-[#97A0AF];
}
.primaryButtonPrimary {
  @apply mt-4 rounded bg-app-primary px-4 py-2 text-[11px] text-white;
}
.sectionColumn {
  @apply flex min-w-0 flex-1 flex-col bg-[#FAFBFC];
}
.rowMutedWithBottomDividerPx4Py25 {
  @apply flex items-center border-b border-[#EBECF0] bg-white px-4 py-2.5;
}
.strongLabelCompact {
  @apply mr-3 text-[11px] text-[#5E6C84];
}
.rowGap1 {
  @apply flex gap-1;
}
.optionButton {
  @apply rounded border px-2 py-1 text-[11px] font-medium;
}
.rowCenteredColumn {
  @apply flex flex-1 flex-col items-center justify-center;
}
.labelCenteredMuted {
  @apply mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-[#F4F5F7] text-[#C1C7D0];
}
.titleMuted {
  @apply text-[14px] font-medium text-[#97A0AF];
}
.descriptionMutedMt1 {
  @apply mt-1 text-[12px] text-[#C1C7D0];
}
.listScrollableWide {
  @apply flex-1 space-y-2 overflow-y-auto px-4 py-3;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.productRow {
  @apply flex items-center gap-3 rounded-xl border border-[#EBECF0] bg-white p-3 hover:border-[#B3D4FF];
}
.labelCenteredMutedH52pxW100px {
  @apply flex h-[52px] w-[100px] shrink-0 items-center justify-center overflow-hidden rounded-lg border border-[#F4F5F7] bg-[#FAFBFC];
}
.strongLabelAlternativeAlternativeAlternative {
  @apply block truncate text-[13px] font-medium;
}
.labelCompactMuted {
  @apply block truncate text-[11px] text-[#6B778C];
}
.labelCompactMutedAlternative {
  @apply block font-mono text-[10px] text-[#97A0AF];
}
.rowGap1Gap1 {
  @apply flex shrink-0 items-center gap-1;
}
.buttonCentered {
  @apply flex h-7 w-7 items-center justify-center rounded-md border border-[#DFE1E6] text-[#6B778C] hover:bg-[#F4F5F7];
}
.input {
  @apply h-7 w-12 rounded-md border border-[#DFE1E6] text-center text-[13px] font-semibold outline-none focus:border-[#4C9AFF];
}
.dangerButtonDangerMuted {
  @apply rounded-lg p-1.5 text-[#97A0AF] hover:bg-[#FFEBE6] hover:text-[#FF5630];
}
.rowColumn {
  @apply flex min-h-0 flex-1 flex-col bg-[#F4F5F7];
}
.rowMutedWithBottomDividerPx5Py25 {
  @apply flex items-center justify-between border-b border-[#EBECF0] bg-white px-5 py-2.5;
}
.strongLabelAlternativeAlternativeAlternativeAlternative {
  @apply text-[12px];
}
.labelCompactMutedMl2 {
  @apply ml-2 text-[11px] text-[#6B778C];
}
.rowScrollable {
  @apply flex-1 overflow-auto p-6;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.gridGrid {
  @apply mx-auto grid max-w-[760px] grid-cols-3 gap-4;
}
.numberRow {
  @apply flex min-h-[145px] flex-col items-center justify-center rounded border border-[#C1C7D0] bg-white p-3 text-center shadow-sm;
}
.strongLabelAlternativeAlternativeAlternativeAlternativeAlternative {
  @apply text-[12px];
}
.iconMy2 {
  @apply my-2;
}
.rowCenteredMy3H12W150px {
  @apply my-3 flex h-12 w-[150px] items-center justify-center bg-[repeating-linear-gradient(90deg,#172B4D_0_2px,transparent_2px_4px,#172B4D_4px_5px,transparent_5px_8px)];
}
.labelCompactAlternative {
  @apply font-mono text-[10px];
}
.labelCompactMutedAlternativeAlternative {
  @apply text-[9px] text-[#6B778C];
}
.footer {
  @apply flex shrink-0 items-center border-t border-[#DFE1E6] bg-white px-6 py-4;
}
.labelMuted {
  @apply text-[12px] text-[#6B778C];
}
.buttonMuted {
  @apply ml-auto rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#42526E] hover:bg-[#F4F5F7];
}
.primaryButtonPrimaryCentered {
  @apply ml-2 flex min-w-[106.3px] items-center justify-center gap-1.5 rounded bg-app-primary px-5 py-2 text-[13px] text-white disabled:bg-[#A5B8E8];
}
.buttonTrailing {
  @apply ml-auto flex items-center gap-1.5 rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#42526E];
}
.primaryButtonPrimaryInverted {
  @apply ml-2 flex items-center gap-1.5 rounded bg-app-primary px-5 py-2 text-[13px] text-white;
}
</style>
