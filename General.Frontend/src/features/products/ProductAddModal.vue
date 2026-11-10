<script setup lang="ts">
import {
  Calculator,
  Camera,
  ChevronDown,
  ClipboardPaste,
  Expand,
  ExternalLink,
  FileText,
  ImagePlus,
  Layers3,
  Minimize2,
  Plus,
  ScanLine,
  Trash2,
  X,
  Zap,
} from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { UiSelect } from '@/components/ui';
import type { ProductDraft, ProductSaveAction, UnitConversion } from './product.types';
import type { ProductSku, SpecDimension } from './product.types';
import ProductSpecEditor from './ProductSpecEditor.vue';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  close: [];
  save: [draft: ProductDraft, action: ProductSaveAction];
}>();

const categoryOptions = ['PLC', '触摸屏/HMI', '电源', '配件', '柜体/箱体', '伺服驱动', '变频器', '传感器', '设计服务'];
const subCategories: Record<string, string[]> = {
  PLC: ['PLC/CPU', 'I/O模块', '通信模块'],
  '触摸屏/HMI': ['7寸及以下', '10寸', '15寸及以上'],
  电源: ['开关电源', '导轨电源', 'UPS'],
  配件: ['线缆/连接器', '端子', '安装件'],
  伺服驱动: ['伺服驱动器', '伺服电机'],
};
const brands = ['西门子', '三菱', '威纶通', '昆仑通态', '明纬', '汇川技术', '台达', '欧姆龙', '其他'];
const units = ['台', '个', '套', '项', '根', '米', '千克', '箱', '件'];
const suppliers = ['西门子（中国）有限公司', '三菱电机自动化（中国）有限公司', '深圳市汇川技术股份有限公司'];
const departments = ['采购部', '销售部', '技术部', '项目部'];
const owners = ['张经理', '李主管', '王工'];
const configurableFields = reactive([
  { label: '采购分类', enabled: true },
  { label: '默认供应商', enabled: true },
  { label: '所属部门', enabled: true },
  { label: '负责人', enabled: true },
  { label: '商品图片', enabled: true },
  { label: '文档附件', enabled: true },
]);

function emptyDraft(): ProductDraft {
  return {
    category: '',
    subCategory: '',
    name: '',
    spec: '',
    model: '',
    brand: '',
    unit: '',
    referenceCost: 0,
    purchasePrice: 0,
    salePrice: 0,
    barcode: '',
    purchaseCategory: '',
    taxIncluded: true,
    defaultTaxRate: 13,
    defaultSupplier: '',
    department: '',
    owner: '',
    allowDecimalQty: false,
    unitConversions: [],
    images: [],
    documents: [],
    enableBatchManagement: false,
    enableTraceability: false,
    shelfLifeDays: undefined,
    batchCodeRule: 'auto',
    hasSpecs: false,
    specDimensions: [],
    skus: [],
    customFields: {},
  };
}

const mode = ref<'fast' | 'full'>('fast');
const expanded = ref(false);
const multiUnit = ref(false);
const error = ref('');
const fieldConfigOpen = ref(false);
const imagePreview = ref('');
const imageInput = ref<HTMLInputElement | null>(null);
const cameraInput = ref<HTMLInputElement | null>(null);
const documentInput = ref<HTMLInputElement | null>(null);
const form = reactive<ProductDraft>(emptyDraft());
const availableSubCategories = computed(() => subCategories[form.category] ?? []);
const toOptions = (values: readonly string[]) => values.map((value) => ({ value, label: value }));
const categorySelectOptions = [{ value: '', label: '请选择一级分类' }, ...toOptions(categoryOptions)];
const subCategorySelectOptions = computed(() => {
  const emptyLabel = form.category
    ? availableSubCategories.value.length
      ? '请选择二级分类'
      : '该分类无子分类'
    : '请先选择一级分类';
  return [{ value: '', label: emptyLabel }, ...toOptions(availableSubCategories.value)];
});
const brandSelectOptions = [{ value: '', label: '请选择品牌' }, ...toOptions(brands)];
const unitSelectOptions = [{ value: '', label: '请选择单位' }, ...toOptions(units)];
const auxiliaryUnitOptions = computed(() => [
  { value: '', label: '单位' },
  ...toOptions(units.filter((value) => value !== form.unit)),
]);
const purchaseCategoryOptions = [{ value: '', label: '请选择采购分类' }, ...toOptions(categoryOptions)];
const taxRateOptions = [13, 9, 6, 3, 0].map((value) => ({ value: String(value), label: `${value}%` }));
const supplierOptions = [{ value: '', label: '请选择默认供应商' }, ...toOptions(suppliers)];
const departmentOptions = [{ value: '', label: '请选择部门' }, ...toOptions(departments)];
const ownerOptions = [{ value: '', label: '请选择负责人' }, ...toOptions(owners)];
const batchCodeOptions = [
  { value: 'auto', label: '自动生成（BYYYYMMDD-NN）' },
  { value: 'supplier', label: '沿用供应商批次号' },
  { value: 'manual', label: '手动输入' },
];
const productSeriesOptions = [
  { value: '', label: '请选择产品系列' },
  ...toOptions(['经典系列', '高端系列', '经济系列', '定制系列']),
];
const protectionLevelOptions = [{ value: '', label: '请选择防护等级' }, ...toOptions(['IP20', 'IP55', 'IP65', 'IP67'])];
const defaultTaxRateModel = computed({
  get: () => String(form.defaultTaxRate),
  set: (value: string) => {
    form.defaultTaxRate = Number(value);
  },
});
const productSeriesModel = computed({
  get: () => String(form.customFields.product_series ?? ''),
  set: (value: string) => {
    form.customFields.product_series = value;
  },
});
const protectionLevelModel = computed({
  get: () => String(form.customFields.protection_level ?? ''),
  set: (value: string) => {
    form.customFields.protection_level = value;
  },
});

function resetForm() {
  Object.assign(form, emptyDraft());
  multiUnit.value = false;
  error.value = '';
}

function cloneDraft(): ProductDraft {
  return {
    ...form,
    unitConversions: form.unitConversions.map((item) => ({ ...item })),
    specDimensions: form.specDimensions.map((item) => ({
      ...item,
      values: item.values.map((value) => ({ ...value })),
    })),
    skus: form.skus.map((item) => ({ ...item })),
    customFields: Object.fromEntries(
      Object.entries(form.customFields).map(([key, value]) => [key, Array.isArray(value) ? [...value] : value]),
    ),
    images: [...form.images],
    documents: [...form.documents],
  };
}

function submit(action: ProductSaveAction) {
  if (!form.name.trim()) {
    error.value = '请输入商品名称';
    return;
  }
  error.value = '';
  emit('save', cloneDraft(), action);
  if (action === 'new') resetForm();
}

function updateCategory() {
  form.subCategory = '';
  if (!form.purchaseCategory) form.purchaseCategory = form.category;
}

function selectCategory(value: string) {
  form.category = value;
  updateCategory();
}

function toggleMultiUnit() {
  multiUnit.value = !multiUnit.value;
  if (!multiUnit.value) {
    form.unitConversions = [];
    form.allowDecimalQty = false;
  }
}

function addConversion() {
  const item: UnitConversion = {
    id: `uc-${Date.now()}`,
    unit: '',
    rateType: 'fixed',
    conversionRate: 1,
    pricePerUnit: 0,
    isDefault: form.unitConversions.length === 0,
  };
  form.unitConversions.push(item);
}

function removeConversion(index: number) {
  form.unitConversions.splice(index, 1);
}

function setDefaultConversion(index: number) {
  form.unitConversions.forEach((item, itemIndex) => {
    item.isDefault = itemIndex === index;
  });
}

function updateSpecs(dimensions: SpecDimension[], skus: ProductSku[]) {
  form.specDimensions = dimensions;
  form.skus = skus;
}

function toggleSpecs() {
  form.hasSpecs = !form.hasSpecs;
  if (!form.hasSpecs) updateSpecs([], []);
}

function toggleCertification(value: string) {
  const current = form.customFields.certifications;
  const selected = Array.isArray(current) ? current : [];
  form.customFields.certifications = selected.includes(value)
    ? selected.filter((item) => item !== value)
    : [...selected, value];
}

function addImages(files: FileList | null) {
  if (!files) return;
  const urls = Array.from(files)
    .filter((file) => file.type.startsWith('image/'))
    .map((file) => URL.createObjectURL(file));
  form.images.push(...urls);
}

function onImageChange(event: Event) {
  const input = event.target as HTMLInputElement;
  addImages(input.files);
  input.value = '';
}

function onDocumentChange(event: Event) {
  const input = event.target as HTMLInputElement;
  if (input.files) form.documents.push(...Array.from(input.files).map((file) => file.name));
  input.value = '';
}

function onPaste(event: ClipboardEvent) {
  const files = Array.from(event.clipboardData?.files ?? []).filter((file) => file.type.startsWith('image/'));
  files.forEach((file) => form.images.push(URL.createObjectURL(file)));
}

function onKeydown(event: KeyboardEvent) {
  if (!props.open || event.key !== 'Escape') return;
  if (imagePreview.value) imagePreview.value = '';
  else if (fieldConfigOpen.value) fieldConfigOpen.value = false;
  else emit('close');
}

watch(
  () => props.open,
  (open) => {
    if (open) resetForm();
    else {
      fieldConfigOpen.value = false;
      imagePreview.value = '';
      expanded.value = false;
    }
  },
);
watch(
  () => form.name,
  () => {
    if (error.value) error.value = '';
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
        :class="[
          $style.dialog,
          expanded ? $style.stateSized : mode === 'full' ? $style.stateSizedW820px : $style.stateSizedH4593pxW820px,
        ]"
        role="dialog"
        aria-modal="true"
        aria-label="新增商品"
      >
        <header :class="$style.header">
          <h2 :class="$style.title">新增商品</h2>
          <button
            :title="expanded ? '还原' : '最大化'"
            @click="expanded = !expanded"
            :class="$style.iconButton"
          >
            <component
              :is="expanded ? Minimize2 : Expand"
              :size="16"
              :stroke-width="1.8"
            />
          </button>
          <button
            title="关闭"
            @click="emit('close')"
            :class="$style.closeButton"
          >
            <X
              :size="16"
              :stroke-width="1.8"
            />
          </button>
        </header>

        <div :class="$style.row">
          <div :class="$style.rowMb5">
            <span :class="[$style.label, mode === 'fast' ? $style.stateSuccess : $style.stateSelected]" />
            <h3 :class="$style.titleStrong">{{ mode === 'fast' ? '核心字段' : '基本信息' }}</h3>
            <span
              v-if="mode === 'fast'"
              :class="$style.badge"
              >仅 3 项必填</span
            >
            <div :class="$style.rowTrailing">
              <div :class="$style.rowRound">
                <button
                  title="只填 3 个核心字段,其它稍后补充"
                  :class="[$style.iconButtonRound, mode === 'fast' ? $style.stateSuccessSuccess : $style.stateMuted]"
                  @click="mode = 'fast'"
                >
                  <Zap
                    :size="12"
                    :stroke-width="1.8"
                  />极速
                </button>
                <button
                  title="显示价格 / 税率 / 批次 / 规格等全部字段"
                  :class="[
                    $style.iconButtonRoundPx25Py1,
                    mode === 'full' ? $style.stateSelectedPrimary : $style.stateMuted,
                  ]"
                  @click="mode = 'full'"
                >
                  完整
                </button>
              </div>
              <button
                @click="fieldConfigOpen = true"
                :class="$style.primaryButton"
              >
                <ExternalLink
                  :size="14"
                  :stroke-width="1.8"
                />字段配置
              </button>
            </div>
          </div>

          <div :class="$style.list">
            <div :class="mode === 'full' ? $style.stateDefault : ''">
              <label :class="$style.field"
                ><span :class="$style.labelRightAligned">一级分类：</span
                ><span :class="$style.labelWide"
                  ><UiSelect
                    :model-value="form.category"
                    :options="categorySelectOptions"
                    label="选择一级分类"
                    :class="$style.select"
                    @update:model-value="selectCategory" /></span
              ></label>
              <label
                v-if="mode === 'full'"
                :class="$style.field"
                ><span :class="$style.labelRightAligned">二级分类：</span
                ><span :class="$style.labelWide"
                  ><UiSelect
                    v-model="form.subCategory"
                    :options="subCategorySelectOptions"
                    label="选择二级分类"
                    :disabled="!form.category"
                    :class="$style.selectPrimary" /></span
              ></label>
            </div>
            <div :class="mode === 'full' ? $style.stateDefault : ''">
              <label :class="$style.field"
                ><span :class="$style.labelRightAligned"><b :class="$style.b">*</b> 商品名称：</span
                ><input
                  v-model="form.name"
                  :class="[$style.input, error ? $style.stateDanger : $style.stateSelectedPrimaryAlternative]"
                  placeholder="请输入商品名称"
              /></label>
              <label
                v-if="mode === 'full'"
                :class="$style.field"
                ><span :class="$style.labelRightAligned">规格：</span
                ><input
                  v-model="form.spec"
                  placeholder="请输入规格"
                  :class="$style.inputPrimary"
              /></label>
            </div>
            <p
              v-if="error"
              :class="$style.description"
            >
              {{ error }}
            </p>

            <div
              v-if="mode === 'full'"
              :class="$style.stateDefault"
            >
              <label :class="$style.field"
                ><span :class="$style.labelRightAligned">型号：</span
                ><input
                  v-model="form.model"
                  placeholder="请输入型号"
                  :class="$style.inputPrimary"
              /></label>
              <label :class="$style.field"
                ><span :class="$style.labelRightAligned">品牌：</span
                ><span :class="$style.labelWide"
                  ><UiSelect
                    v-model="form.brand"
                    :options="brandSelectOptions"
                    label="选择品牌"
                    :class="$style.select" /></span
              ></label>
            </div>

            <div :class="mode === 'full' ? $style.stateDefault : ''">
              <label :class="$style.field"
                ><span :class="$style.labelRightAligned">单位：</span
                ><span :class="$style.labelWideAlternative"
                  ><UiSelect
                    v-model="form.unit"
                    :options="unitSelectOptions"
                    label="选择单位"
                    :class="$style.select" /></span
                ><button
                  v-if="mode === 'full' && form.unit"
                  type="button"
                  :class="[
                    $style.button,
                    multiUnit ? $style.stateSelectedPrimaryAlternativeAlternative : $style.stateMutedMuted,
                  ]"
                  @click="toggleMultiUnit"
                >
                  <Calculator :size="12" />多单位
                </button></label
              >
              <label
                v-if="mode === 'full'"
                :class="$style.field"
                ><span :class="$style.labelRightAligned">参考成本价：</span
                ><span :class="$style.labelWide"
                  ><input
                    v-model.number="form.referenceCost"
                    min="0"
                    type="number"
                    placeholder="0"
                    :class="$style.inputPrimaryWide" /><Calculator
                    :size="13"
                    :stroke-width="1.8"
                    :class="$style.decorationFloating" /></span
              ></label>
            </div>

            <div
              v-if="mode === 'full' && multiUnit && form.unit"
              :class="$style.card"
            >
              <div :class="$style.rowMb3">
                <Calculator
                  :size="14"
                  :class="$style.icon"
                /><strong :class="$style.strongLabel">多单位换算</strong
                ><span :class="$style.labelCompact">（主单位：{{ form.unit }}）</span
                ><label :class="$style.fieldCompact"
                  ><input
                    v-model="form.allowDecimalQty"
                    type="checkbox"
                    :class="$style.checkbox"
                  />允许小数数量</label
                ><button
                  @click="addConversion"
                  :class="$style.primaryButtonPrimary"
                >
                  <Plus :size="12" />添加辅助单位
                </button>
              </div>
              <div
                v-if="form.unitConversions.length === 0"
                :class="$style.div"
              >
                暂无辅助单位，请添加并设置换算关系。
              </div>
              <div
                v-for="(item, index) in form.unitConversions"
                :key="item.id"
                :class="$style.itemCard"
              >
                <div :class="$style.rowGap2">
                  <span :class="$style.labelAlternative">1 {{ form.unit }} =</span
                  ><input
                    v-model.number="item.conversionRate"
                    min="0"
                    type="number"
                    :class="$style.inputPrimaryH8W20Px2"
                  /><UiSelect
                    v-model="item.unit"
                    :options="auxiliaryUnitOptions"
                    label="选择辅助单位"
                    size="sm"
                    :class="$style.selectH8W20Px2"
                  /><span :class="$style.labelMl2">单价：</span
                  ><input
                    v-model.number="item.pricePerUnit"
                    min="0"
                    type="number"
                    :class="$style.inputPrimaryH8W24Px2"
                  /><label :class="$style.fieldTrailing"
                    ><input
                      type="radio"
                      name="default-unit"
                      :checked="item.isDefault"
                      @change="setDefaultConversion(index)"
                    />默认</label
                  ><button
                    title="删除辅助单位"
                    @click="removeConversion(index)"
                    :class="$style.deleteButton"
                  >
                    <Trash2 :size="13" />
                  </button>
                </div>
              </div>
            </div>

            <template v-if="mode === 'full'">
              <div :class="$style.grid">
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">采购价：</span
                  ><span :class="$style.labelWideAlternative"
                    ><input
                      v-model.number="form.purchasePrice"
                      min="0"
                      type="number"
                      placeholder="0"
                      :class="$style.inputPrimaryWide" /><Calculator
                      :size="13"
                      :stroke-width="1.8"
                      :class="$style.decorationFloating" /></span
                ></label>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">预设售价1：</span
                  ><span :class="$style.labelWideAlternative"
                    ><input
                      v-model.number="form.salePrice"
                      min="0"
                      type="number"
                      placeholder="0"
                      :class="$style.inputPrimaryWide" /><Calculator
                      :size="13"
                      :stroke-width="1.8"
                      :class="$style.decorationFloating" /></span
                ></label>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">条码：</span
                  ><span :class="$style.labelWideAlternative"
                    ><input
                      v-model="form.barcode"
                      placeholder="请输入条码或使用扫码枪扫描"
                      :class="$style.inputPrimaryWide" /><ScanLine
                      :size="15"
                      :stroke-width="1.8"
                      :class="$style.decorationFloatingMuted" /></span
                ></label>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">采购分类：</span
                  ><span :class="$style.labelWideAlternative"
                    ><UiSelect
                      v-model="form.purchaseCategory"
                      :options="purchaseCategoryOptions"
                      label="选择采购分类"
                      :class="$style.selectPrimaryWide" /></span
                ></label>
                <div :class="$style.field">
                  <span :class="$style.labelRightAligned">是否含税：</span
                  ><span :class="$style.labelWideH36pxGap3"
                    ><button
                      type="button"
                      :class="[
                        $style.buttonH36pxPx4,
                        form.taxIncluded
                          ? $style.stateSelectedPrimaryAlternativeAlternativeAlternative
                          : $style.stateMutedMutedAlternative,
                      ]"
                      @click="form.taxIncluded = true"
                    >
                      含税</button
                    ><button
                      type="button"
                      :class="[
                        $style.buttonH36pxPx4,
                        !form.taxIncluded
                          ? $style.stateSelectedPrimaryAlternativeAlternativeAlternative
                          : $style.stateMutedMutedAlternative,
                      ]"
                      @click="form.taxIncluded = false"
                    >
                      不含税
                    </button></span
                  >
                </div>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">默认税率：</span
                  ><span :class="$style.labelWide"
                    ><UiSelect
                      v-model="defaultTaxRateModel"
                      :options="taxRateOptions"
                      label="选择默认税率"
                      :class="$style.selectPrimaryWide" /></span
                ></label>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">默认供应商：</span
                  ><span :class="$style.labelWideAlternative"
                    ><UiSelect
                      v-model="form.defaultSupplier"
                      :options="supplierOptions"
                      label="选择默认供应商"
                      :class="$style.selectPrimaryWide" /></span
                ></label>
                <span aria-hidden="true" />
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">所属部门：</span
                  ><span :class="$style.labelWideAlternative"
                    ><UiSelect
                      v-model="form.department"
                      :options="departmentOptions"
                      label="选择所属部门"
                      :class="$style.selectPrimaryWide" /></span
                ></label>
                <label :class="$style.field"
                  ><span :class="$style.labelRightAligned">负责人：</span
                  ><span :class="$style.labelWideAlternative"
                    ><UiSelect
                      v-model="form.owner"
                      :options="ownerOptions"
                      label="选择负责人"
                      :class="$style.selectPrimaryWide" /></span
                ></label>
              </div>

              <section :class="$style.section">
                <header :class="$style.headerMb4Gap2">
                  <i :class="$style.i" /><strong :class="$style.strongLabelAlternative">批次与追溯管理</strong>
                </header>
                <div :class="$style.gridGrid">
                  <div :class="$style.rowMuted">
                    <div>
                      <strong :class="$style.strongLabelAlternativeAlternative">启用批次管理</strong
                      ><small :class="$style.caption">原材料和关键物料建议启用，入库时自动分配批次码</small>
                    </div>
                    <button
                      type="button"
                      role="switch"
                      :aria-checked="form.enableBatchManagement"
                      :class="[
                        $style.iconButtonRoundH16pxW8,
                        form.enableBatchManagement
                          ? $style.stateSelected
                          : $style.stateMutedMutedAlternativeAlternative,
                      ]"
                      @click="form.enableBatchManagement = !form.enableBatchManagement"
                    >
                      <i
                        :class="[
                          $style.decorationFloatingRound,
                          form.enableBatchManagement ? $style.stateTransformed : $style.stateTransformedAlternative,
                        ]"
                      />
                    </button>
                  </div>
                  <div :class="$style.rowMuted">
                    <div>
                      <strong :class="$style.strongLabelAlternativeAlternative">启用追溯管理</strong
                      ><small :class="$style.caption">成品和关键半成品建议启用，出库时自动生成追溯码</small>
                    </div>
                    <button
                      type="button"
                      role="switch"
                      :aria-checked="form.enableTraceability"
                      :class="[
                        $style.iconButtonRoundH16pxW8,
                        form.enableTraceability ? $style.stateSelected : $style.stateMutedMutedAlternativeAlternative,
                      ]"
                      @click="form.enableTraceability = !form.enableTraceability"
                    >
                      <i
                        :class="[
                          $style.decorationFloatingRound,
                          form.enableTraceability ? $style.stateTransformed : $style.stateTransformedAlternative,
                        ]"
                      />
                    </button>
                  </div>
                </div>
                <div
                  v-if="form.enableBatchManagement"
                  :class="$style.gridGridMt3GridCols3Gap4"
                >
                  <label
                    ><span :class="$style.labelCompactStrong">批次编码规则</span
                    ><UiSelect
                      v-model="form.batchCodeRule"
                      :options="batchCodeOptions"
                      label="选择批次编码规则"
                      :class="$style.selectWide" /></label
                  ><label
                    ><span :class="$style.labelCompactStrong">保质期（天）</span
                    ><input
                      v-model.number="form.shelfLifeDays"
                      min="0"
                      type="number"
                      placeholder="不填则无保质期限制"
                      :class="$style.inputWide"
                  /></label>
                  <p :class="$style.descriptionCompact">
                    {{ form.shelfLifeDays ? `到期前 ${Math.min(30, form.shelfLifeDays)} 天自动预警` : '无保质期限制' }}
                  </p>
                </div>
              </section>

              <section :class="$style.section">
                <header :class="$style.headerMb4">
                  <i :class="$style.iPurple" /><Layers3
                    :size="16"
                    :class="$style.icon"
                  /><strong :class="$style.strongLabelAlternative">规格管理</strong
                  ><span :class="$style.labelMuted">启用多规格</span
                  ><button
                    type="button"
                    role="switch"
                    :aria-checked="form.hasSpecs"
                    :class="[
                      $style.iconButtonRoundH16pxW8,
                      form.hasSpecs ? $style.statePurple : $style.stateMutedMutedAlternativeAlternative,
                    ]"
                    @click="toggleSpecs"
                  >
                    <i
                      :class="[
                        $style.decorationFloatingRound,
                        form.hasSpecs ? $style.stateTransformed : $style.stateTransformedAlternative,
                      ]"
                    />
                  </button>
                </header>
                <ProductSpecEditor
                  v-if="form.hasSpecs"
                  :dimensions="form.specDimensions"
                  :skus="form.skus"
                  :base-price="form.salePrice"
                  :base-cost="form.referenceCost"
                  @update="updateSpecs"
                />
              </section>

              <section :class="$style.section">
                <header :class="$style.headerMb4">
                  <i :class="$style.iPurple" /><strong :class="$style.strongLabelAlternative">自定义字段</strong
                  ><span :class="$style.badgePurple">5</span
                  ><button
                    @click="fieldConfigOpen = true"
                    :class="$style.primaryButtonPrimaryCompact"
                  >
                    <ExternalLink :size="11" />管理字段
                  </button>
                </header>
                <div :class="$style.grid">
                  <label :class="$style.field"
                    ><span :class="$style.labelRightAligned">产品系列：</span
                    ><UiSelect
                      v-model="productSeriesModel"
                      :options="productSeriesOptions"
                      label="选择产品系列"
                      :class="$style.selectWideH9Px3"
                  /></label>
                  <div :class="$style.field">
                    <span :class="$style.labelRightAligned">认证标准：</span>
                    <div :class="$style.rowWide">
                      <label
                        v-for="value in ['CE', 'UL', 'ISO 9001', '防爆认证']"
                        :key="value"
                        :class="$style.valueField"
                        ><input
                          type="checkbox"
                          :checked="
                            Array.isArray(form.customFields.certifications) &&
                            form.customFields.certifications.includes(value)
                          "
                          @change="toggleCertification(value)"
                          :class="$style.checkbox"
                        />{{ value }}</label
                      >
                    </div>
                  </div>
                  <label :class="$style.field"
                    ><span :class="$style.labelRightAligned">防护等级：</span
                    ><UiSelect
                      v-model="protectionLevelModel"
                      :options="protectionLevelOptions"
                      label="选择防护等级"
                      :class="$style.selectWideH9Px3" /></label
                  ><label :class="$style.field"
                    ><span :class="$style.labelRightAligned">工作温度：</span
                    ><input
                      :value="String(form.customFields.temp_range ?? '')"
                      placeholder="如：-20℃ ~ +60℃"
                      @input="form.customFields.temp_range = ($event.target as HTMLInputElement).value"
                      :class="$style.inputWideH9Px3" /></label
                  ><label :class="$style.field"
                    ><span :class="$style.labelRightAligned">质保期(月)：</span
                    ><input
                      :value="String(form.customFields.warranty_months ?? '')"
                      type="number"
                      placeholder="请输入质保月数"
                      @input="form.customFields.warranty_months = ($event.target as HTMLInputElement).value"
                      :class="$style.inputWideH9Px3"
                  /></label>
                </div>
              </section>

              <div :class="$style.rowGap3Pt1">
                <span :class="$style.labelRightAlignedW72pxPt2">图片：</span>
                <div :class="$style.rowWideMuted">
                  <div :class="$style.rowGap3">
                    <div
                      v-for="(image, index) in form.images"
                      :key="image"
                      :class="$style.imageCard"
                    >
                      <img
                        :src="image"
                        alt=""
                        :class="$style.image"
                      />
                      <div :class="$style.rowFloating">
                        <button
                          title="预览"
                          @click="imagePreview = image"
                          :class="$style.previewButton"
                        >
                          <Expand :size="12" /></button
                        ><button
                          title="删除"
                          @click="form.images.splice(index, 1)"
                          :class="$style.deleteButtonDanger"
                        >
                          <X :size="12" />
                        </button>
                      </div>
                      <span
                        v-if="index === 0"
                        :class="$style.labelPrimary"
                        >主图</span
                      >
                    </div>
                    <button
                      @click="imageInput?.click()"
                      :class="$style.buttonPrimary"
                    >
                      <ImagePlus
                        :size="24"
                        :class="$style.iconMb1"
                      />上传图片</button
                    ><button
                      @click="cameraInput?.click()"
                      :class="$style.buttonPrimary"
                    >
                      <Camera
                        :size="24"
                        :class="$style.iconMb1"
                      />拍照上传</button
                    ><button
                      @click="($event.currentTarget as HTMLButtonElement).focus()"
                      @paste="onPaste"
                      :class="$style.buttonPrimary"
                    >
                      <ClipboardPaste
                        :size="24"
                        :class="$style.iconMb1"
                      />Ctrl+V 粘贴
                    </button>
                  </div>
                  <input
                    ref="imageInput"
                    type="file"
                    accept="image/*"
                    multiple
                    @change="onImageChange"
                    :class="$style.imageInputInput"
                  /><input
                    ref="cameraInput"
                    type="file"
                    accept="image/*"
                    capture="environment"
                    @change="onImageChange"
                    :class="$style.imageInputInput"
                  />
                  <p :class="$style.descriptionCompactMuted">第一张图片将作为商品主图。支持上传、拍照和粘贴图片。</p>
                </div>
              </div>
              <div :class="$style.rowGap3Gap3">
                <span :class="$style.labelRightAlignedW72pxPt2">文档：</span>
                <div :class="$style.rowWideMuted">
                  <div :class="$style.rowGap3Gap3Gap3">
                    <button
                      @click="documentInput?.click()"
                      :class="$style.buttonPrimary"
                    >
                      <FileText
                        :size="24"
                        :class="$style.iconMb1"
                      />上传文档
                    </button>
                    <div
                      v-for="(document, index) in form.documents"
                      :key="`${document}-${index}`"
                      :class="$style.documentRow"
                    >
                      <FileText
                        :size="30"
                        :class="$style.iconMuted"
                      /><button
                        title="删除文档"
                        @click="form.documents.splice(index, 1)"
                        :class="$style.deleteButtonFloating"
                      >
                        <X :size="10" /></button
                      ><span :class="$style.labelCentered">{{ document }}</span>
                    </div>
                  </div>
                  <input
                    ref="documentInput"
                    type="file"
                    multiple
                    accept=".pdf,.doc,.docx,.xls,.xlsx,.txt"
                    @change="onDocumentChange"
                    :class="$style.imageInputInput"
                  />
                  <p :class="$style.descriptionCompactMuted">支持 pdf、docx、xlsx、txt 格式，单个文件不超过 10M</p>
                </div>
              </div>
            </template>

            <div
              v-else
              :class="$style.cardSuccess"
            >
              <div :class="$style.rowGap3Gap3Gap3Gap3">
                <span :class="$style.labelRound"><Zap :size="14" /></span>
                <div>
                  <h4 :class="$style.titleStrongAlternative">已开启极速模式 — 3 项就能建好</h4>
                  <p :class="$style.descriptionCompactMutedMt1">
                    采购价 / 税率 / 供应商 / 批次 / 多规格 / 图片等可以稍后在<span
                      :class="$style.labelPrimaryAlternative"
                      >详情页</span
                    >或第一次<span :class="$style.labelPrimaryAlternative">开单 / 入库</span>时补充，不影响现在保存。
                  </p>
                  <button
                    @click="mode = 'full'"
                    :class="$style.primaryButtonPrimaryCompactMt2Gap1Px25"
                  >
                    展开完整字段 <ChevronDown :size="12" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <footer :class="$style.footer">
          <button
            @click="submit('close')"
            :class="$style.primaryButtonPrimaryInverted"
          >
            保存
          </button>
          <button
            @click="submit('new')"
            :class="$style.buttonMuted"
          >
            保存并新增
          </button>
          <button
            @click="submit('copy')"
            :class="$style.buttonMuted"
          >
            保存并复制
          </button>
          <button
            @click="emit('close')"
            :class="$style.buttonMuted"
          >
            关闭
          </button>
        </footer>
      </div>
    </div>

    <div
      v-if="open && fieldConfigOpen"
      @mousedown.self="fieldConfigOpen = false"
      :class="$style.overlayFloating"
    >
      <section
        role="dialog"
        aria-modal="true"
        aria-label="字段配置"
        :class="$style.dialogW480px"
      >
        <header :class="$style.headerWithBottomDivider">
          <div>
            <h3 :class="$style.titleStrong">商品字段配置</h3>
            <p :class="$style.descriptionCompactMutedMt1Mt1">控制完整模式中的扩展字段</p>
          </div>
          <button
            @click="fieldConfigOpen = false"
            :class="$style.iconButtonMuted"
          >
            <X :size="16" />
          </button>
        </header>
        <div :class="$style.listP5">
          <label
            v-for="field in configurableFields"
            :key="field.label"
            :class="$style.fieldField"
            ><span>{{ field.label }}</span
            ><input
              v-model="field.enabled"
              type="checkbox"
              :class="$style.checkboxPrimary"
          /></label>
        </div>
        <footer :class="$style.footerMuted">
          <button
            @click="fieldConfigOpen = false"
            :class="$style.primaryButtonPrimaryInvertedPx4Py2"
          >
            完成
          </button>
        </footer>
      </section>
    </div>
    <div
      v-if="open && imagePreview"
      @click="imagePreview = ''"
      :class="$style.overlayFloatingCentered"
    >
      <img
        :src="imagePreview"
        alt="图片预览"
        @click.stop
        :class="$style.imageAlternative"
      /><button
        title="关闭预览"
        @click="imagePreview = ''"
        :class="$style.closeButtonFloating"
      >
        <X :size="17" />
      </button>
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
  @apply flex flex-col overflow-hidden rounded-lg border border-[#DFE1E6] bg-white shadow-2xl transition-[width,height] duration-150;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.stateSized {
  @apply h-[calc(100vh-40px)] max-h-[calc(100vh-40px)] w-[calc(100vw-40px)];
}
.stateSizedW820px {
  @apply max-h-[calc(84vh-0.4px)] w-[820px];
}
.stateSizedH4593pxW820px {
  @apply h-[459.3px] w-[820px];
}
.header {
  @apply flex shrink-0 items-center border-b border-[#DFE1E6] px-6 pb-4 pt-5;
}
.title {
  @apply text-[17px] font-semibold text-[#172B4D];
}
.iconButton {
  @apply ml-auto rounded p-1.5 text-[#6B778C] transition-colors hover:bg-[#F4F5F7];
}
.closeButton {
  @apply ml-2 rounded p-1.5 text-[#6B778C] transition-colors hover:bg-[#F4F5F7];
}
.row {
  @apply min-h-0 flex-1 overflow-y-auto px-6 py-5;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.rowMb5 {
  @apply mb-5 flex items-center;
}
.label {
  @apply mr-2 h-5 w-[3px] rounded-full;
}
.stateSuccess {
  @apply bg-[#00875A];
}
.stateSelected {
  @apply bg-app-primary;
}
.titleStrong {
  @apply text-[15px] font-semibold;
}
.badge {
  @apply ml-2 rounded bg-[#E3FCEF] px-1.5 py-0.5 text-[10px] font-semibold text-[#00875A];
}
.rowTrailing {
  @apply ml-auto flex items-center gap-3;
}
.rowRound {
  @apply flex items-center rounded-full bg-[#F4F5F7] p-0.5 text-[12px] font-semibold;
}
.iconButtonRound {
  @apply flex items-center gap-1 rounded-full px-2.5 py-1 transition-colors;
}
.stateSuccessSuccess {
  @apply bg-white text-[#00875A] shadow-sm;
}
.stateMuted {
  @apply text-[#6B778C];
}
.iconButtonRoundPx25Py1 {
  @apply rounded-full px-2.5 py-1 transition-colors;
}
.stateSelectedPrimary {
  @apply bg-white text-app-primary shadow-sm;
}
.primaryButton {
  @apply flex items-center gap-1.5 text-[13px] text-[#6B778C] transition-colors hover:text-app-primary;
}
.list {
  @apply space-y-4;
}
.stateDefault {
  @apply grid grid-cols-2 gap-x-6;
}
.field {
  @apply flex items-center gap-3;
}
.labelRightAligned {
  @apply w-[72px] shrink-0 whitespace-nowrap text-right text-[13px] text-[#5E6C84];
}
.labelWide {
  @apply relative flex-1;
}
.select {
  @apply h-[36px] w-full appearance-none rounded border border-[#DFE1E6] bg-white px-3 text-[13px] outline-none focus:border-[#4C9AFF];
}
.decoration {
  @apply absolute right-3 top-[11px] text-[#6B778C];
}
.selectPrimary {
  @apply h-[36px] w-full appearance-none rounded border border-[#DFE1E6] bg-white px-3 text-[13px] outline-none disabled:bg-[#F4F5F7] disabled:text-[#97A0AF] focus:border-[#4C9AFF];
}
.b {
  @apply text-[#FF5630];
}
.input {
  @apply h-[36px] min-w-0 flex-1 rounded border px-3 text-[13px] outline-none;
}
.stateDanger {
  @apply border-[#FF5630];
}
.stateSelectedPrimaryAlternative {
  @apply border-[#DFE1E6] focus:border-[#4C9AFF];
}
.inputPrimary {
  @apply h-[36px] min-w-0 flex-1 rounded border border-[#DFE1E6] px-3 text-[13px] outline-none focus:border-[#4C9AFF];
}
.description {
  @apply ml-[84px] -mt-2 text-[11px] text-[#DE350B];
}
.labelWideAlternative {
  @apply relative min-w-0 flex-1;
}
.button {
  @apply flex shrink-0 items-center gap-1 rounded border px-2.5 py-1.5 text-[11px] font-medium;
}
.stateSelectedPrimaryAlternativeAlternative {
  @apply border-[#4C9AFF] bg-app-primary-soft text-app-primary;
}
.stateMutedMuted {
  @apply border-[#DFE1E6] text-[#6B778C];
}
.inputPrimaryWide {
  @apply h-[36px] w-full rounded border border-[#DFE1E6] px-3 pr-8 text-[13px] outline-none focus:border-[#4C9AFF];
}
.decorationFloating {
  @apply absolute right-3 top-[11px] text-[#97A0AF];
}
.card {
  @apply rounded-lg border border-[#DFE1E6] bg-[#FAFBFC] p-4;
}
.rowMb3 {
  @apply mb-3 flex items-center;
}
.icon {
  @apply mr-2 text-[#6554C0];
}
.strongLabel {
  @apply text-[13px];
}
.labelCompact {
  @apply ml-1 text-[11px] text-[#97A0AF];
}
.fieldCompact {
  @apply ml-auto flex items-center gap-1.5 text-[11px] text-[#5E6C84];
}
.checkbox {
  @apply accent-app-primary;
}
.primaryButtonPrimary {
  @apply ml-3 flex items-center gap-1 rounded px-2.5 py-1 text-[11px] font-medium text-app-primary hover:bg-app-primary-soft;
}
.div {
  @apply py-3 text-center text-[11px] text-[#97A0AF];
}
.itemCard {
  @apply mb-2 rounded border border-[#EBECF0] bg-white px-3 py-2.5 last:mb-0;
}
.rowGap2 {
  @apply flex items-center gap-2 text-[12px];
}
.labelAlternative {
  @apply shrink-0 text-[#5E6C84];
}
.inputPrimaryH8W20Px2 {
  @apply h-8 w-20 rounded border border-[#DFE1E6] px-2 outline-none focus:border-[#4C9AFF];
}
.selectH8W20Px2 {
  @apply h-8 w-20 rounded border border-[#DFE1E6] bg-white px-2 outline-none;
}
.labelMl2 {
  @apply ml-2 text-[#5E6C84];
}
.inputPrimaryH8W24Px2 {
  @apply h-8 w-24 rounded border border-[#DFE1E6] px-2 outline-none focus:border-[#4C9AFF];
}
.fieldTrailing {
  @apply ml-auto flex items-center gap-1;
}
.deleteButton {
  @apply rounded p-1 text-[#97A0AF] hover:bg-[#FFEBE6] hover:text-[#DE350B];
}
.grid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4;
}
.decorationFloatingMuted {
  @apply absolute right-3 top-[10px] text-[#6B778C];
}
.selectPrimaryWide {
  @apply h-[38px] w-full appearance-none rounded border border-[#DFE1E6] bg-white px-3 pr-8 text-[13px] outline-none focus:border-[#4C9AFF];
}
.decorationFloatingMutedAlternative {
  @apply absolute right-3 top-[12px] text-[#6B778C];
}
.labelWideH36pxGap3 {
  @apply flex h-[36px] flex-1 items-center gap-3;
}
.buttonH36pxPx4 {
  @apply h-[36px] rounded border px-4 text-[13px] font-medium transition-colors;
}
.stateSelectedPrimaryAlternativeAlternativeAlternative {
  @apply border-[#4C9AFF] bg-[#F4F9FF] text-app-primary;
}
.stateMutedMutedAlternative {
  @apply border-[#DFE1E6] bg-white text-[#6B778C];
}
.section {
  @apply mt-6;
}
.headerMb4Gap2 {
  @apply mb-4 flex items-center gap-2;
}
.i {
  @apply h-5 w-[3px] rounded-full bg-[#00875A];
}
.strongLabelAlternative {
  @apply text-[15px];
}
.gridGrid {
  @apply grid grid-cols-2 gap-4;
}
.rowMuted {
  @apply flex items-center justify-between rounded-lg border border-[#DFE1E6] bg-[#FAFBFC] p-3;
}
.strongLabelAlternativeAlternative {
  @apply block text-[13px] font-medium;
}
.caption {
  @apply mt-0.5 block text-[11px] text-[#6B778C];
}
.iconButtonRoundH16pxW8 {
  @apply relative h-[16px] w-8 rounded-full transition-colors;
}
.stateMutedMutedAlternativeAlternative {
  @apply bg-[#C1C7D0];
}
.decorationFloatingRound {
  @apply absolute top-[1px] h-[14px] w-[14px] rounded-full bg-white shadow transition-transform;
}
.stateTransformed {
  @apply translate-x-[13px];
}
.stateTransformedAlternative {
  @apply translate-x-[1px];
}
.gridGridMt3GridCols3Gap4 {
  @apply mt-3 grid grid-cols-3 gap-4;
}
.labelCompactStrong {
  @apply mb-1 block text-[11px] font-semibold text-[#5E6C84];
}
.selectWide {
  @apply h-9 w-full rounded border border-[#DFE1E6] bg-white px-3 text-[12px];
}
.inputWide {
  @apply h-9 w-full rounded border border-[#DFE1E6] px-3 text-[12px];
}
.descriptionCompact {
  @apply self-end pb-2 text-[11px] text-[#97A0AF];
}
.headerMb4 {
  @apply mb-4 flex items-center;
}
.iPurple {
  @apply mr-2 h-5 w-[3px] rounded-full bg-[#6554C0];
}
.labelMuted {
  @apply ml-auto mr-2 text-[12px] text-[#6B778C];
}
.statePurple {
  @apply bg-[#6554C0];
}
.badgePurple {
  @apply ml-2 rounded bg-[#EAE6FF] px-1.5 py-0.5 text-[10px] font-semibold text-[#6554C0];
}
.primaryButtonPrimaryCompact {
  @apply ml-auto flex items-center gap-1 text-[11px] text-[#6B778C] hover:text-app-primary;
}
.selectWideH9Px3 {
  @apply h-9 min-w-0 flex-1 rounded border border-[#DFE1E6] bg-white px-3 text-[12px];
}
.rowWide {
  @apply flex min-h-9 flex-1 flex-wrap items-center gap-1 rounded border border-[#DFE1E6] bg-white px-2;
}
.valueField {
  @apply flex items-center gap-1 rounded bg-[#F4F5F7] px-1.5 py-0.5 text-[10px];
}
.inputWideH9Px3 {
  @apply h-9 min-w-0 flex-1 rounded border border-[#DFE1E6] px-3 text-[12px];
}
.rowGap3Pt1 {
  @apply flex gap-3 pt-1;
}
.labelRightAlignedW72pxPt2 {
  @apply w-[72px] shrink-0 pt-2 text-right text-[13px] text-[#5E6C84];
}
.rowWideMuted {
  @apply flex-1 rounded-lg border border-[#DFE1E6] bg-[#FAFBFC] p-4;
}
.rowGap3 {
  @apply flex flex-wrap items-start gap-3;
}
.imageCard {
  @apply relative h-[100px] w-[100px] overflow-hidden rounded-lg border border-[#DFE1E6];
}
.image {
  @apply h-full w-full object-cover;
}
.rowFloating {
  @apply absolute inset-0 flex items-center justify-center gap-1 bg-black/0 transition-colors group-hover:bg-black/30;
}
.previewButton {
  @apply rounded-full bg-white/90 p-1.5 text-app-primary opacity-0 group-hover:opacity-100;
}
.deleteButtonDanger {
  @apply rounded-full bg-white/90 p-1.5 text-[#DE350B] opacity-0 group-hover:opacity-100;
}
.labelPrimary {
  @apply absolute inset-x-0 bottom-0 bg-app-primary/80 py-0.5 text-center text-[9px] font-semibold text-white;
}
.buttonPrimary {
  @apply flex h-[100px] w-[100px] flex-col items-center justify-center rounded-lg border-2 border-dashed border-[#C1C7D0] text-[10px] text-[#97A0AF] hover:border-[#4C9AFF] hover:text-[#4C9AFF];
}
.iconMb1 {
  @apply mb-1;
}
.imageInputInput {
  @apply hidden;
}
.descriptionCompactMuted {
  @apply mt-3 text-[11px] text-[#97A0AF];
}
.rowGap3Gap3 {
  @apply flex gap-3;
}
.rowGap3Gap3Gap3 {
  @apply flex flex-wrap gap-3;
}
.documentRow {
  @apply relative flex h-[100px] w-[100px] flex-col items-center justify-center rounded-lg border border-[#DFE1E6] bg-white px-2;
}
.iconMuted {
  @apply text-[#C1C7D0];
}
.deleteButtonFloating {
  @apply absolute right-1 top-1 rounded-full bg-[#5E6C84] p-1 text-white;
}
.labelCentered {
  @apply mt-2 w-full truncate text-center text-[9px];
}
.cardSuccess {
  @apply rounded-lg border border-[#B3F5E5] bg-gradient-to-r p-4;
}
.rowGap3Gap3Gap3Gap3 {
  @apply flex items-start gap-3;
}
.labelRound {
  @apply flex h-6 w-6 items-center justify-center rounded-full bg-white text-[#00A878];
}
.titleStrongAlternative {
  @apply text-[13px] font-semibold;
}
.descriptionCompactMutedMt1 {
  @apply mt-1 text-[11px] leading-5 text-[#6B778C];
}
.labelPrimaryAlternative {
  @apply font-medium text-app-primary;
}
.primaryButtonPrimaryCompactMt2Gap1Px25 {
  @apply relative top-[1.5px] -ml-1 mt-2 flex items-center gap-1 rounded border border-[#B3D4FF] bg-white px-2.5 py-1 text-[11px] font-medium text-app-primary;
}
.footer {
  @apply flex shrink-0 items-center justify-end gap-3 border-t border-[#DFE1E6] bg-[#FAFBFC] px-6 py-4;
}
.primaryButtonPrimaryInverted {
  @apply rounded bg-app-primary px-5 py-2 text-[13px] font-medium text-white transition-colors hover:bg-app-primary-hover active:scale-[.97];
}
.buttonMuted {
  @apply rounded border border-[#DFE1E6] bg-white px-5 py-2 text-[13px] font-medium text-[#42526E] transition-colors hover:bg-[#F4F5F7];
}
.overlayFloating {
  @apply fixed inset-0 z-[180] flex items-center justify-center bg-black/40;
}
.dialogW480px {
  @apply w-[480px] overflow-hidden rounded-xl bg-white shadow-2xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.headerWithBottomDivider {
  @apply flex items-center border-b border-[#DFE1E6] px-6 py-4;
}
.descriptionCompactMutedMt1Mt1 {
  @apply mt-1 text-[11px] text-[#6B778C];
}
.iconButtonMuted {
  @apply ml-auto rounded p-1.5 text-[#6B778C] hover:bg-[#F4F5F7];
}
.listP5 {
  @apply space-y-2 p-5;
}
.fieldField {
  @apply flex items-center rounded border border-[#EBECF0] px-3 py-2.5 text-[12px] hover:bg-[#FAFBFC];
}
.checkboxPrimary {
  @apply ml-auto accent-app-primary;
}
.footerMuted {
  @apply flex justify-end border-t border-[#DFE1E6] bg-[#FAFBFC] px-5 py-3;
}
.primaryButtonPrimaryInvertedPx4Py2 {
  @apply rounded bg-app-primary px-4 py-2 text-[12px] text-white;
}
.overlayFloatingCentered {
  @apply fixed inset-0 z-[190] flex items-center justify-center bg-black/70;
}
.imageAlternative {
  @apply max-h-[85vh] max-w-[90vw] rounded-lg object-contain shadow-2xl;
}
.closeButtonFloating {
  @apply absolute right-6 top-6 rounded-full bg-white p-2 text-[#42526E];
}
</style>
