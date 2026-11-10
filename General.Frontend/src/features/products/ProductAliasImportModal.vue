<script setup lang="ts">
import { AlertCircle, CheckCircle2, Download, FileSpreadsheet, LoaderCircle, X } from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import type { ProductAlias } from './product.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ close: []; import: [aliases: ProductAlias[]] }>();

type ImportState = 'idle' | 'parsing' | 'preview' | 'importing' | 'done';
interface PreviewRow extends ProductAlias {
  valid: boolean;
  error: string;
}

const state = ref<ImportState>('idle');
const fileName = ref('');
const input = ref<HTMLInputElement | null>(null);
const rows = ref<PreviewRow[]>([]);
const result = ref({ success: 0, failed: 0 });
let timer = 0;
const validRows = computed(() => rows.value.filter((row) => row.valid));
const invalidRows = computed(() => rows.value.filter((row) => !row.valid));

function selectFile(event: Event) {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  target.value = '';
  if (!file) return;
  const extension = file.name.split('.').pop()?.toLowerCase();
  if (!extension || !['xlsx', 'xls', 'csv'].includes(extension)) return;
  fileName.value = file.name;
  state.value = 'parsing';
  window.clearTimeout(timer);
  timer = window.setTimeout(() => {
    rows.value = [
      {
        id: '',
        customer: '华能电力科技',
        customerAbbr: '华',
        product: '西门子 S7-1200 CPU',
        model: '6ES7214-1AG40',
        alias: '华能配电PLC',
        aliasModel: 'HN-S7-1215C',
        notes: '华能三期项目',
        valid: true,
        error: '',
      },
      {
        id: '',
        customer: '中石化仪表部',
        customerAbbr: '中',
        product: '威纶通 cMT3072',
        model: 'cMT3072',
        alias: '中石化显控终端',
        aliasModel: 'SPC-HMI-3072',
        notes: '',
        valid: true,
        error: '',
      },
      {
        id: '',
        customer: '宝钢设备部',
        customerAbbr: '宝',
        product: '三菱 FX5U-32M',
        model: 'FX5U-32MT/ES',
        alias: '宝钢辅助PLC',
        aliasModel: 'BS-FX5U-32',
        notes: '宝钢冷轧线',
        valid: true,
        error: '',
      },
      {
        id: '',
        customer: '不存在的客户',
        customerAbbr: '不',
        product: '西门子 S7-1200 CPU',
        model: '6ES7214-1AG40',
        alias: '测试别名',
        aliasModel: 'TEST-001',
        notes: '',
        valid: false,
        error: '未找到匹配客户',
      },
      {
        id: '',
        customer: '上海宏控自动化',
        customerAbbr: '上',
        product: '未知商品XYZ',
        model: 'XYZ',
        alias: '宏控测试',
        aliasModel: 'HK-XYZ',
        notes: '',
        valid: false,
        error: '未找到匹配商品',
      },
    ];
    state.value = 'preview';
  }, 750);
}

function downloadTemplate() {
  const csv = '\uFEFF客户名称*,商品名称/型号*,客户商品名称*,客户商品型号*,备注\n';
  const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '商品别名导入模板.csv';
  link.click();
  URL.revokeObjectURL(url);
}

function commit() {
  state.value = 'importing';
  window.clearTimeout(timer);
  timer = window.setTimeout(() => {
    const imported = validRows.value.map((row, index): ProductAlias => ({
      ...row,
      id: `CPA-IMP-${Date.now()}-${index}`,
    }));
    emit('import', imported);
    result.value = { success: imported.length, failed: invalidRows.value.length };
    state.value = 'done';
  }, 900);
}

function reset() {
  state.value = 'idle';
  fileName.value = '';
  rows.value = [];
  result.value = { success: 0, failed: 0 };
}

function close() {
  window.clearTimeout(timer);
  emit('close');
}

function onKeydown(event: KeyboardEvent) {
  if (props.open && event.key === 'Escape') close();
}

watch(
  () => props.open,
  (open) => {
    if (open) reset();
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
      @mousedown.self="close"
      :class="$style.overlay"
    >
      <section
        role="dialog"
        aria-modal="true"
        aria-label="批量导入商品别名"
        :class="$style.dialog"
      >
        <header :class="$style.header">
          <div>
            <h2 :class="$style.title">批量导入商品别名</h2>
            <p :class="$style.description">上传文件 → 数据预览 → 确认导入</p>
          </div>
          <button
            @click="close"
            :class="$style.iconButton"
          >
            <X :size="16" />
          </button>
        </header>
        <div :class="$style.row">
          <template v-if="state === 'idle'">
            <div :class="$style.rowPrimary">
              <AlertCircle
                :size="16"
                :class="$style.icon"
              />
              <div>
                <strong :class="$style.strongLabel">首次导入？请先下载模板</strong>
                <p :class="$style.descriptionMt05">
                  按照模板格式填写别名数据，带 * 的字段为必填项。客户和商品需与系统中已有数据匹配。
                </p>
                <button
                  @click="downloadTemplate"
                  :class="$style.primaryButton"
                >
                  <Download :size="13" />下载导入模板
                </button>
              </div>
            </div>
            <button
              @click="input?.click()"
              :class="$style.button"
            >
              <span :class="$style.label"
                ><FileSpreadsheet
                  :size="28"
                  :class="$style.iconMuted" /></span
              ><strong :class="$style.strongLabelAlternative"
                >将文件拖拽到此处，或 <span :class="$style.labelPrimary">点击选择文件</span></strong
              ><small :class="$style.caption">支持 Excel（.xlsx、.xls）和 CSV 格式，单次最多导入 500 条</small></button
            ><input
              ref="input"
              type="file"
              accept=".xlsx,.xls,.csv"
              @change="selectFile"
              :class="$style.inputInput"
            />
            <div :class="$style.div">
              <strong :class="$style.strongLabelMb2">模板字段说明</strong>
              <div :class="$style.rowGap15">
                <span
                  v-for="field in ['客户名称*', '商品名称/型号*', '客户商品名称*', '客户商品型号*', '备注']"
                  :key="field"
                  :class="[$style.badge, field.endsWith('*') ? $style.stateSelected : $style.stateMuted]"
                  >{{ field }}</span
                >
              </div>
            </div>
          </template>

          <div
            v-else-if="state === 'parsing' || state === 'importing'"
            :class="$style.rowCentered"
          >
            <span :class="$style.labelPrimaryRound"
              ><LoaderCircle
                :size="28"
                :class="$style.iconPrimary" /></span
            ><strong :class="$style.strongLabelAlternativeAlternative">{{
              state === 'parsing' ? '正在解析文件...' : '正在导入别名数据...'
            }}</strong
            ><small :class="$style.captionMuted">{{ fileName }}</small>
          </div>

          <template v-else-if="state === 'preview'">
            <div :class="$style.rowMb4">
              <span :class="$style.labelMuted"
                ><FileSpreadsheet
                  :size="14"
                  :class="$style.iconMr1"
                />{{ fileName }}</span
              ><span :class="$style.labelSuccess"
                ><CheckCircle2
                  :size="13"
                  :class="$style.iconMr1"
                />有效 {{ validRows.length }} 条</span
              ><span :class="$style.labelDanger"
                ><AlertCircle
                  :size="13"
                  :class="$style.iconMr1"
                />异常 {{ invalidRows.length }} 条</span
              >
            </div>
            <div :class="$style.card">
              <table :class="$style.table">
                <thead :class="$style.tableHead">
                  <tr :class="$style.tableRow">
                    <th :class="$style.headerCell">行</th>
                    <th :class="$style.headerCellW10Px2Py25">状态</th>
                    <th :class="$style.headerCellPx3Py25">客户名称</th>
                    <th :class="$style.headerCellPx3Py25">商品名称</th>
                    <th :class="$style.headerCellPx3Py25">客户商品名称</th>
                    <th :class="$style.headerCellPx3Py25">客户商品型号</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(row, index) in rows"
                    :key="index"
                    :class="[$style.rowRow, row.valid ? $style.stateDefault : $style.stateDanger]"
                  >
                    <td :class="$style.tableCell">{{ index + 1 }}</td>
                    <td :class="$style.tableCellPx2Py2">
                      <CheckCircle2
                        v-if="row.valid"
                        :size="14"
                        :class="$style.iconSuccess"
                      /><AlertCircle
                        v-else
                        :size="14"
                        :title="row.error"
                        :class="$style.iconDanger"
                      />
                    </td>
                    <td
                      :class="[
                        $style.tableCellPx3Py2,
                        !row.valid && row.error.includes('客户') ? $style.iconDanger : '',
                      ]"
                    >
                      {{ row.customer }}
                    </td>
                    <td
                      :class="[
                        $style.tableCellPx3Py2,
                        !row.valid && row.error.includes('商品') ? $style.iconDanger : '',
                      ]"
                    >
                      {{ row.product }}
                    </td>
                    <td :class="$style.tableCellPx3Py2Px3Py2">{{ row.alias }}</td>
                    <td :class="$style.tableCellPx3Py2">{{ row.aliasModel }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div :class="$style.cardWarning">
              检测到 {{ invalidRows.length }} 条数据异常，这些行将被跳过。仅导入有效数据。
            </div>
          </template>

          <div
            v-else
            :class="$style.rowCenteredColumn"
          >
            <span :class="$style.labelSuccessRound"
              ><CheckCircle2
                :size="32"
                :class="$style.iconSuccess"
            /></span>
            <h3 :class="$style.titleStrong">导入完成（部分失败）</h3>
            <p :class="$style.descriptionMt2">
              成功 <b :class="$style.iconSuccess">{{ result.success }}</b> 条 · 失败
              <b :class="$style.iconDanger">{{ result.failed }}</b> 条
            </p>
            <div :class="$style.cardDanger">行 4：未找到匹配客户<br />行 5：未找到匹配商品</div>
          </div>
        </div>
        <footer
          v-if="state === 'preview' || state === 'done'"
          :class="$style.footer"
        >
          <button
            v-if="state === 'preview'"
            @click="reset"
            :class="$style.buttonPx4Py2"
          >
            重新选择</button
          ><button
            v-if="state === 'preview'"
            @click="commit"
            :class="$style.primaryButtonPrimary"
          >
            确认导入 ({{ validRows.length }} 条)</button
          ><button
            v-else
            @click="close"
            :class="$style.primaryButtonPrimary"
          >
            完成
          </button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";

/* Semantic template classes */
.overlay {
  @apply fixed inset-0 z-[160] flex items-center justify-center bg-black/40;
}
.dialog {
  @apply flex max-h-[86vh] w-[780px] flex-col overflow-hidden rounded-xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.header {
  @apply flex items-center border-b border-[#DFE1E6] px-6 py-4;
}
.title {
  @apply text-[16px] font-semibold;
}
.description {
  @apply mt-0.5 text-[11px] text-[#6B778C];
}
.iconButton {
  @apply ml-auto rounded p-1.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.row {
  @apply min-h-0 flex-1 overflow-y-auto px-6 py-5;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.rowPrimary {
  @apply mb-5 flex items-start gap-3 rounded-lg border border-[#B3D4FF] bg-app-primary-soft/50 p-3;
}
.icon {
  @apply mt-0.5 shrink-0 text-app-primary;
}
.strongLabel {
  @apply text-[13px];
}
.descriptionMt05 {
  @apply mt-0.5 text-[12px] text-[#42526E];
}
.primaryButton {
  @apply mt-2 flex items-center gap-1.5 text-[12px] font-medium text-app-primary;
}
.button {
  @apply w-full rounded-xl border-2 border-dashed border-[#DFE1E6] p-10 text-center hover:border-[#4C9AFF] hover:bg-[#FAFBFC];
}
.label {
  @apply mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-[#F4F5F7];
}
.iconMuted {
  @apply text-[#6B778C];
}
.strongLabelAlternative {
  @apply block text-[14px];
}
.labelPrimary {
  @apply text-app-primary;
}
.caption {
  @apply mt-1 block text-[12px] text-[#6B778C];
}
.inputInput {
  @apply hidden;
}
.div {
  @apply mt-5;
}
.strongLabelMb2 {
  @apply mb-2 block text-[12px] text-[#5E6C84];
}
.rowGap15 {
  @apply flex gap-1.5;
}
.badge {
  @apply rounded px-2.5 py-1 text-[11px];
}
.stateSelected {
  @apply bg-app-primary-soft font-semibold text-app-primary;
}
.stateMuted {
  @apply bg-[#F4F5F7] text-[#42526E];
}
.rowCentered {
  @apply flex flex-col items-center justify-center py-16;
}
.labelPrimaryRound {
  @apply mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-app-primary-soft;
}
.iconPrimary {
  @apply text-app-primary;
}
.strongLabelAlternativeAlternative {
  @apply text-[14px];
}
.captionMuted {
  @apply mt-1 text-[12px] text-[#6B778C];
}
.rowMb4 {
  @apply mb-4 flex items-center;
}
.labelMuted {
  @apply rounded-lg bg-[#F4F5F7] px-3 py-1.5 text-[12px] font-medium;
}
.iconMr1 {
  @apply mr-1 inline;
}
.labelSuccess {
  @apply ml-auto text-[12px] font-medium text-[#00875A];
}
.labelDanger {
  @apply ml-3 text-[12px] font-medium text-[#DE350B];
}
.card {
  @apply max-h-[330px] overflow-y-auto rounded-lg border border-[#DFE1E6];
}
.table {
  @apply w-full text-[11px];
}
.tableHead {
  @apply sticky top-0 bg-[#F4F5F7];
}
.tableRow {
  @apply border-b border-[#DFE1E6] text-left text-[#5E6C84];
}
.headerCell {
  @apply w-10 px-3 py-2.5;
}
.headerCellW10Px2Py25 {
  @apply w-10 px-2 py-2.5;
}
.headerCellPx3Py25 {
  @apply px-3 py-2.5;
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
.tableCellPx2Py2 {
  @apply px-2 py-2;
}
.iconSuccess {
  @apply text-[#00875A];
}
.iconDanger {
  @apply text-[#DE350B];
}
.tableCellPx3Py2 {
  @apply px-3 py-2;
}
.tableCellPx3Py2Px3Py2 {
  @apply px-3 py-2 font-medium;
}
.cardWarning {
  @apply mt-3 rounded border border-[#FFE380] bg-[#FFF7E6] p-3 text-[12px] text-[#42526E];
}
.rowCenteredColumn {
  @apply flex flex-col items-center justify-center py-14;
}
.labelSuccessRound {
  @apply mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#E3FCEF];
}
.titleStrong {
  @apply text-[17px] font-semibold;
}
.descriptionMt2 {
  @apply mt-2 text-[13px] text-[#42526E];
}
.cardDanger {
  @apply mt-5 rounded-lg border border-[#FFBDAD] bg-[#FFEBE6]/40 px-5 py-3 text-[11px] text-[#DE350B];
}
.footer {
  @apply flex justify-end gap-2 border-t border-[#DFE1E6] bg-[#FAFBFC] px-6 py-4;
}
.buttonPx4Py2 {
  @apply rounded border border-[#DFE1E6] bg-white px-4 py-2 text-[12px] text-[#42526E];
}
.primaryButtonPrimary {
  @apply rounded bg-app-primary px-5 py-2 text-[12px] font-medium text-white;
}
</style>
