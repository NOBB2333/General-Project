<script setup lang="ts">
import {
  AlertTriangle,
  ArrowLeft,
  Check,
  CircleAlert,
  CircleCheck,
  CircleX,
  Download,
  FileSpreadsheet,
  LoaderCircle,
  RotateCcw,
  Upload,
  X,
} from 'lucide-vue-next';
import { onMounted, onUnmounted, ref, watch } from 'vue';

type ImportState = 'idle' | 'parsing' | 'preview' | 'importing' | 'done';
interface PreviewRow {
  name: string;
  model: string;
  category: string;
  brand: string;
  unit: string;
  purchasePrice: number;
  presetSalePrice1: number;
  valid: boolean;
  error: string;
}

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ close: []; import: [] }>();
const fileInput = ref<HTMLInputElement | null>(null);
const state = ref<ImportState>('idle');
const fileName = ref('商品批量导入示例.xlsx');
let timer = 0;

const previewRows: PreviewRow[] = [
  {
    name: '汇川 SV660N',
    model: 'SV660N-020A',
    category: '伺服驱动',
    brand: '汇川',
    unit: '台',
    purchasePrice: 3200,
    presetSalePrice1: 4800,
    valid: true,
    error: '',
  },
  {
    name: '欧姆龙 CP1H-X40DT',
    model: 'CP1H-X40DT-D',
    category: 'PLC',
    brand: '欧姆龙',
    unit: '台',
    purchasePrice: 2800,
    presetSalePrice1: 4000,
    valid: true,
    error: '',
  },
  {
    name: '信捷 TG765-XT',
    model: 'TG765-XT',
    category: '触摸屏/HMI',
    brand: '信捷',
    unit: '台',
    purchasePrice: 1500,
    presetSalePrice1: 2200,
    valid: true,
    error: '',
  },
  {
    name: '台达 DVP-28SV2',
    model: 'DVP28SV211T',
    category: 'PLC',
    brand: '台达',
    unit: '台',
    purchasePrice: 1800,
    presetSalePrice1: 2600,
    valid: true,
    error: '',
  },
  {
    name: '',
    model: 'UNKNOWN-001',
    category: '配件',
    brand: '',
    unit: '个',
    purchasePrice: 50,
    presetSalePrice1: 100,
    valid: false,
    error: '商品名称不能为空',
  },
  {
    name: '菲尼克斯 端子排',
    model: 'UT-4',
    category: '配件',
    brand: '菲尼克斯',
    unit: '个',
    purchasePrice: 5,
    presetSalePrice1: 12,
    valid: true,
    error: '',
  },
];

function reset() {
  window.clearTimeout(timer);
  state.value = 'idle';
  fileName.value = '商品批量导入示例.xlsx';
}

function chooseFile() {
  fileInput.value?.click();
}
function downloadTemplate() {
  const fields = ['商品名称*', '型号*', '分类*', '品牌', '规格', '单位*', '采购价', '预设售价1', '条码'];
  const url = URL.createObjectURL(new Blob(['\uFEFF' + fields.join(',') + '\n'], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '商品导入模板.csv';
  link.click();
  URL.revokeObjectURL(url);
}
function beginParsing(file?: File) {
  if (file && !/\.(xlsx|xls|csv)$/i.test(file.name)) return;
  if (file) fileName.value = file.name;
  state.value = 'parsing';
  timer = window.setTimeout(() => {
    state.value = 'preview';
  }, 650);
}
function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (file) beginParsing(file);
  input.value = '';
}
function onDrop(event: DragEvent) {
  event.preventDefault();
  const file = event.dataTransfer?.files[0];
  if (file) beginParsing(file);
}
function importRows() {
  state.value = 'importing';
  timer = window.setTimeout(() => {
    state.value = 'done';
  }, 750);
}
function finish() {
  emit('import');
}

function onKeydown(event: KeyboardEvent) {
  if (props.open && event.key === 'Escape') emit('close');
}

watch(
  () => props.open,
  (open) => {
    if (open) reset();
    else window.clearTimeout(timer);
  },
);
onMounted(() => document.addEventListener('keydown', onKeydown));
onUnmounted(() => {
  window.clearTimeout(timer);
  document.removeEventListener('keydown', onKeydown);
});
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      @mousedown.self="(state === 'idle' || state === 'done') && emit('close')"
      :class="$style.overlay"
    >
      <div
        :class="[$style.dialog, state === 'preview' || state === 'done' ? $style.modalWide : $style.modalCompact]"
        role="dialog"
        aria-modal="true"
        aria-label="批量导入商品"
      >
        <header :class="$style.header">
          <span :class="$style.label"><Upload :size="18" /></span>
          <div>
            <h2 :class="$style.title">批量导入商品</h2>
            <p :class="$style.description">支持 .xlsx、.xls、.csv 格式文件</p>
          </div>
          <button
            @click="emit('close')"
            :class="$style.iconButton"
          >
            <X :size="16" />
          </button>
        </header>

        <div :class="$style.div">
          <div :class="$style.row">
            <template
              v-for="(step, index) in [
                { n: 1, label: '上传文件' },
                { n: 2, label: '数据预览' },
                { n: 3, label: '导入结果' },
              ]"
              :key="step.n"
            >
              <div :class="$style.rowGap25">
                <span
                  :class="[
                    $style.labelRound,
                    (state === 'idle' || state === 'parsing'
                      ? 1
                      : state === 'preview' || state === 'importing'
                        ? 2
                        : 3) === step.n
                      ? $style.stateSelected
                      : ((state === 'preview' || state === 'importing' || state === 'done') && step.n === 1) ||
                          (state === 'done' && step.n === 2)
                        ? $style.stateSuccess
                        : $style.stateMuted,
                  ]"
                >
                  <Check
                    v-if="
                      ((state === 'preview' || state === 'importing' || state === 'done') && step.n === 1) ||
                      (state === 'done' && step.n === 2)
                    "
                    :size="14"
                  />
                  <template v-else>{{ step.n }}</template> </span
                ><span
                  :class="[
                    $style.labelAlternative,
                    (state === 'idle' || state === 'parsing'
                      ? 1
                      : state === 'preview' || state === 'importing'
                        ? 2
                        : 3) === step.n
                      ? $style.stateSelectedPrimary
                      : $style.stateMutedMuted,
                  ]"
                  >{{ step.label }}</span
                >
              </div>
              <span
                v-if="index < 2"
                :class="[
                  $style.labelWide,
                  ((state === 'preview' || state === 'importing' || state === 'done') && index === 0) ||
                  (state === 'done' && index === 1)
                    ? $style.stateSuccessSuccess
                    : $style.stateMutedMutedAlternative,
                ]"
              />
            </template>
          </div>
        </div>

        <div :class="$style.rowScrollable">
          <template v-if="state === 'idle'">
            <div :class="$style.rowPrimary">
              <CircleAlert
                :size="16"
                :class="$style.icon"
              />
              <div>
                <h3 :class="$style.titleAlternative">首次导入？请先下载模板</h3>
                <p :class="$style.descriptionMt05">按照模板格式填写商品数据，带 * 的字段为必填项。</p>
                <button
                  @click="downloadTemplate"
                  :class="$style.primaryButton"
                >
                  <Download :size="13" />下载导入模板
                </button>
              </div>
            </div>
            <input
              ref="fileInput"
              type="file"
              accept=".xlsx,.xls,.csv"
              @change="onFileChange"
              :class="$style.fileInputInput"
            />
            <button
              @click="chooseFile"
              @dragover.prevent
              @drop="onDrop"
              :class="$style.button"
            >
              <span :class="$style.labelRoundCentered"><FileSpreadsheet :size="28" /></span
              ><strong :class="$style.strongLabel"
                >将文件拖拽到此处，或 <span :class="$style.labelPrimary">点击选择文件</span></strong
              ><span :class="$style.labelMuted">支持 Excel（.xlsx、.xls）和 CSV 格式，单次最多导入 500 条</span>
            </button>
            <div :class="$style.divMt5">
              <h4 :class="$style.titleStrong">模板字段说明</h4>
              <div :class="$style.rowGap15">
                <span
                  v-for="field in [
                    '商品名称*',
                    '型号*',
                    '分类*',
                    '品牌',
                    '规格',
                    '单位*',
                    '采购价',
                    '预设售价1',
                    '条码',
                  ]"
                  :key="field"
                  :class="[
                    $style.badge,
                    field.endsWith('*')
                      ? $style.stateSelectedPrimaryStrong
                      : $style.stateMutedMutedAlternativeAlternative,
                  ]"
                  >{{ field }}</span
                >
              </div>
            </div>
          </template>

          <div
            v-else-if="state === 'parsing'"
            :class="$style.rowCentered"
          >
            <span :class="$style.labelPrimaryRound"
              ><LoaderCircle
                :size="28"
                :class="$style.iconAlternative"
            /></span>
            <h3 :class="$style.strongLabel">正在解析文件...</h3>
            <p :class="$style.labelMuted">{{ fileName }}</p>
          </div>

          <template v-else-if="state === 'preview'">
            <div :class="$style.rowMb4">
              <span :class="$style.labelMutedGap2Px3Py15"
                ><FileSpreadsheet
                  :size="14"
                  :class="$style.stateMutedMuted"
                />{{ fileName }}</span
              >
              <div :class="$style.rowGap3">
                <span :class="$style.labelSuccess"><CircleCheck :size="13" />有效 5 条</span
                ><span :class="$style.labelDanger"><CircleX :size="13" />异常 1 条</span>
              </div>
            </div>
            <div :class="$style.card">
              <table :class="$style.table">
                <thead>
                  <tr :class="$style.tableRow">
                    <th :class="$style.headerCell">行</th>
                    <th :class="$style.headerCellCentered">状态</th>
                    <th :class="$style.headerCellPx3Py25">商品名称</th>
                    <th :class="$style.headerCellPx3Py25">型号</th>
                    <th :class="$style.headerCellPx3Py25">分类</th>
                    <th :class="$style.headerCellPx3Py25">品牌</th>
                    <th :class="$style.headerCellPx3Py25">单位</th>
                    <th :class="$style.headerCellRightAligned">采购价</th>
                    <th :class="$style.headerCellRightAligned">预设售价</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(row, index) in previewRows"
                    :key="index"
                    :class="[$style.rowRow, row.valid ? $style.stateDefault : $style.stateDanger]"
                  >
                    <td :class="$style.tableCell">{{ index + 1 }}</td>
                    <td :class="$style.tableCellCentered">
                      <CircleCheck
                        v-if="row.valid"
                        :size="14"
                        :class="$style.iconSuccess"
                      /><CircleX
                        v-else
                        :size="14"
                        :title="row.error"
                        :class="$style.iconDanger"
                      />
                    </td>
                    <td :class="[$style.tableCellPx3Py2, !row.valid ? $style.stateDangerDanger : '']">
                      {{ row.name || '（空）' }}
                    </td>
                    <td :class="$style.tableCellPx3Py2Px3Py2">{{ row.model }}</td>
                    <td :class="$style.tableCellPx3Py2Px3Py2">{{ row.category }}</td>
                    <td :class="$style.tableCell">{{ row.brand || '-' }}</td>
                    <td :class="$style.tableCellPx3Py2Px3Py2">{{ row.unit }}</td>
                    <td :class="$style.tableCellRightAligned">¥{{ row.purchasePrice.toLocaleString() }}</td>
                    <td :class="$style.tableCellRightAligned">¥{{ row.presetSalePrice1.toLocaleString() }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div :class="$style.rowWarning">
              <AlertTriangle
                :size="14"
                :class="$style.iconWarning"
              />
              <p :class="$style.descriptionAlternative">检测到 1 条数据异常，这些行将被跳过。仅导入有效数据。</p>
            </div>
          </template>

          <div
            v-else-if="state === 'importing'"
            :class="$style.rowCentered"
          >
            <span :class="$style.labelPrimaryRound"
              ><LoaderCircle
                :size="28"
                :class="$style.iconAlternative"
            /></span>
            <h3 :class="$style.strongLabel">正在导入商品数据...</h3>
            <p :class="$style.labelMuted">共 5 条有效数据</p>
          </div>

          <template v-else>
            <div :class="$style.divCentered">
              <span :class="$style.labelWarning"><AlertTriangle :size="32" /></span>
              <h3 :class="$style.titleStrongAlternative">导入完成（部分失败）</h3>
              <p :class="$style.descriptionMuted">5 条导入成功，1 条导入失败</p>
              <div :class="$style.rowCenteredMt5Gap6">
                <div :class="$style.cardSuccess">
                  <strong :class="$style.strongLabelSuccess">5</strong><span :class="$style.description">成功导入</span>
                </div>
                <div :class="$style.cardDanger">
                  <strong :class="$style.strongLabelDanger">1</strong><span :class="$style.description">导入失败</span>
                </div>
                <div :class="$style.cardMuted">
                  <strong :class="$style.strongLabelAlternative">6</strong><span :class="$style.description">总计</span>
                </div>
              </div>
            </div>
            <div :class="$style.divMt5">
              <h4 :class="$style.titleDanger"><CircleX :size="14" />失败详情（1 条）</h4>
              <div :class="$style.cardDangerPx4Py25">行 5：商品名称不能为空</div>
              <p :class="$style.descriptionCompact">请修正上述问题后重新导入失败的数据行。</p>
            </div>
          </template>
        </div>

        <footer :class="$style.footer">
          <template v-if="state === 'idle'"
            ><button
              @click="downloadTemplate"
              :class="$style.primaryButtonPrimary"
            >
              <Download :size="13" />下载模板</button
            ><button
              @click="emit('close')"
              :class="$style.buttonPx4Py2"
            >
              取消
            </button></template
          >
          <template v-else-if="state === 'preview'"
            ><button
              @click="reset"
              :class="$style.buttonGap15"
            >
              <ArrowLeft :size="14" />上一步
            </button>
            <div :class="$style.rowGap2">
              <button
                @click="emit('close')"
                :class="$style.buttonPx4Py2Px4Py2"
              >
                取消</button
              ><button
                @click="importRows"
                :class="$style.primaryButtonPrimaryInverted"
              >
                确认导入 (5 条)
              </button>
            </div></template
          >
          <template v-else-if="state === 'parsing' || state === 'importing'"
            ><span /><button
              disabled
              :class="$style.buttonMuted"
            >
              处理中...
            </button></template
          >
          <template v-else
            ><button
              @click="reset"
              :class="$style.primaryButtonPrimaryGap15"
            >
              <RotateCcw :size="14" />重新导入</button
            ><button
              @click="finish"
              :class="$style.primaryButtonPrimaryInverted"
            >
              完成
            </button></template
          >
        </footer>
      </div>
    </div>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";
.modalCompact {
  width: 580px;
}

.modalWide {
  width: 720px;
}

/* Semantic template classes */
.overlay {
  @apply fixed inset-0 z-[100] flex items-center justify-center bg-black/50;
}
.dialog {
  @apply flex max-h-[85vh] flex-col overflow-hidden rounded-lg border border-[#DFE1E6] bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply flex shrink-0 items-center border-b border-[#DFE1E6] px-6 pb-4 pt-5;
}
.label {
  @apply mr-3 flex h-9 w-9 items-center justify-center rounded-lg bg-app-primary-soft text-app-primary;
}
.title {
  @apply text-[16px] font-semibold;
}
.description {
  @apply text-[12px] text-[#6B778C];
}
.iconButton {
  @apply ml-auto rounded p-1.5 text-[#6B778C] transition-colors hover:bg-[#F4F5F7];
}
.div {
  @apply shrink-0 border-b border-[#EBECF0] px-6 pb-3 pt-5;
}
.row {
  @apply flex items-center;
}
.rowGap25 {
  @apply flex items-center gap-2.5;
}
.labelRound {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[12px] font-bold transition-colors;
}
.stateSelected {
  @apply bg-app-primary text-white;
}
.stateSuccess {
  @apply bg-[#00875A] text-white;
}
.stateMuted {
  @apply bg-[#EBECF0] text-[#6B778C];
}
.labelAlternative {
  @apply whitespace-nowrap text-[13px];
}
.stateSelectedPrimary {
  @apply font-semibold text-app-primary;
}
.stateMutedMuted {
  @apply text-[#6B778C];
}
.labelWide {
  @apply mx-4 h-0.5 flex-1 rounded;
}
.stateSuccessSuccess {
  @apply bg-[#00875A];
}
.stateMutedMutedAlternative {
  @apply bg-[#EBECF0];
}
.rowScrollable {
  @apply flex-1 overflow-y-auto px-6 py-5;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.rowPrimary {
  @apply mb-5 flex items-start gap-3 rounded-lg border border-[#B3D4FF] bg-app-primary-soft/50 p-3;
}
.icon {
  @apply mt-0.5 shrink-0 text-app-primary;
}
.titleAlternative {
  @apply text-[13px] font-medium;
}
.descriptionMt05 {
  @apply mt-0.5 text-[12px] text-[#42526E];
}
.primaryButton {
  @apply mt-2 flex items-center gap-1.5 text-[12px] font-medium text-app-primary transition-colors hover:text-app-primary-hover;
}
.fileInputInput {
  @apply hidden;
}
.button {
  @apply flex w-full flex-col items-center rounded-xl border-2 border-dashed border-[#DFE1E6] p-10 text-center transition-colors hover:border-[#4C9AFF] hover:bg-[#FAFBFC];
}
.labelRoundCentered {
  @apply mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-[#F4F5F7] text-[#6B778C];
}
.strongLabel {
  @apply text-[14px] font-medium;
}
.labelPrimary {
  @apply text-app-primary;
}
.labelMuted {
  @apply mt-1 text-[12px] text-[#6B778C];
}
.divMt5 {
  @apply mt-5;
}
.titleStrong {
  @apply mb-2 text-[12px] font-semibold text-[#5E6C84];
}
.rowGap15 {
  @apply flex flex-wrap gap-1.5;
}
.badge {
  @apply rounded px-2.5 py-1 text-[11px];
}
.stateSelectedPrimaryStrong {
  @apply bg-app-primary-soft font-semibold text-app-primary;
}
.stateMutedMutedAlternativeAlternative {
  @apply bg-[#F4F5F7] text-[#42526E];
}
.rowCentered {
  @apply flex flex-col items-center justify-center py-16;
}
.labelPrimaryRound {
  @apply mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-app-primary-soft text-app-primary;
}
.iconAlternative {
}
.rowMb4 {
  @apply mb-4 flex items-center justify-between;
}
.labelMutedGap2Px3Py15 {
  @apply flex items-center gap-2 rounded-lg bg-[#F4F5F7] px-3 py-1.5 text-[12px] font-medium;
}
.rowGap3 {
  @apply flex items-center gap-3;
}
.labelSuccess {
  @apply flex items-center gap-1 text-[12px] font-medium text-[#00875A];
}
.labelDanger {
  @apply flex items-center gap-1 text-[12px] font-medium text-[#DE350B];
}
.card {
  @apply max-h-[320px] overflow-auto rounded-lg border border-[#DFE1E6];
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply min-w-[700px] border-collapse text-[12px];
}
.tableRow {
  @apply border-b border-[#DFE1E6] bg-[#F4F5F7] text-left text-[11px] font-semibold text-[#5E6C84];
}
.headerCell {
  @apply w-9 px-3 py-2.5;
}
.headerCellCentered {
  @apply w-9 px-2 py-2.5 text-center;
}
.headerCellPx3Py25 {
  @apply px-3 py-2.5;
}
.headerCellRightAligned {
  @apply px-3 py-2.5 text-right;
}
.rowRow {
  @apply border-b border-[#F4F5F7];
}
.stateDefault {
  @apply hover:bg-[#FAFBFC];
}
.stateDanger {
  @apply bg-[#FFEBE6]/30;
}
.tableCell {
  @apply px-3 py-2 text-[#6B778C];
}
.tableCellCentered {
  @apply px-2 py-2 text-center;
}
.iconSuccess {
  @apply mx-auto text-[#00875A];
}
.iconDanger {
  @apply mx-auto text-[#DE350B];
}
.tableCellPx3Py2 {
  @apply px-3 py-2 font-medium;
}
.stateDangerDanger {
  @apply italic text-[#DE350B];
}
.tableCellPx3Py2Px3Py2 {
  @apply px-3 py-2;
}
.tableCellRightAligned {
  @apply px-3 py-2 text-right;
}
.rowWarning {
  @apply mt-3 flex items-start gap-2 rounded border border-[#FFE380] bg-[#FFF7E6] p-3;
}
.iconWarning {
  @apply mt-0.5 shrink-0 text-[#FF8B00];
}
.descriptionAlternative {
  @apply text-[12px] text-[#42526E];
}
.divCentered {
  @apply py-6 text-center;
}
.labelWarning {
  @apply mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#FFF7E6] text-[#FF8B00];
}
.titleStrongAlternative {
  @apply text-[18px] font-semibold;
}
.descriptionMuted {
  @apply mt-2 text-[13px] text-[#6B778C];
}
.rowCenteredMt5Gap6 {
  @apply mt-5 flex items-center justify-center gap-6;
}
.cardSuccess {
  @apply rounded-lg border border-[#ABF5D1] bg-[#E3FCEF]/50 px-5 py-3;
}
.strongLabelSuccess {
  @apply block text-[28px] text-[#00875A];
}
.cardDanger {
  @apply rounded-lg border border-[#FFBDAD] bg-[#FFEBE6]/50 px-5 py-3;
}
.strongLabelDanger {
  @apply block text-[28px] text-[#DE350B];
}
.cardMuted {
  @apply rounded-lg border border-[#DFE1E6] bg-[#F4F5F7] px-5 py-3;
}
.strongLabelAlternative {
  @apply block text-[28px] text-[#172B4D];
}
.titleDanger {
  @apply mb-2 flex items-center gap-1.5 text-[13px] font-semibold text-[#DE350B];
}
.cardDangerPx4Py25 {
  @apply rounded-lg border border-[#FFBDAD] bg-[#FFEBE6]/10 px-4 py-2.5 text-[12px] text-[#42526E];
}
.descriptionCompact {
  @apply mt-2 text-[11px] text-[#6B778C];
}
.footer {
  @apply flex shrink-0 items-center justify-between border-t border-[#DFE1E6] bg-[#FAFBFC] px-6 py-4;
}
.primaryButtonPrimary {
  @apply flex items-center gap-1.5 text-[12px] font-medium text-app-primary;
}
.buttonPx4Py2 {
  @apply rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#42526E] transition-colors hover:bg-white;
}
.buttonGap15 {
  @apply flex items-center gap-1.5 text-[13px] text-[#42526E];
}
.rowGap2 {
  @apply flex gap-2;
}
.buttonPx4Py2Px4Py2 {
  @apply rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#42526E];
}
.primaryButtonPrimaryInverted {
  @apply rounded bg-app-primary px-5 py-2 text-[13px] text-white transition-colors hover:bg-app-primary-hover;
}
.buttonMuted {
  @apply cursor-not-allowed rounded border border-[#DFE1E6] px-4 py-2 text-[13px] text-[#97A0AF];
}
.primaryButtonPrimaryGap15 {
  @apply flex items-center gap-1.5 text-[13px] font-medium text-app-primary;
}
</style>
