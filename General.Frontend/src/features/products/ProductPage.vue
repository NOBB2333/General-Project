<script setup lang="ts">
import {
  toast,
  UiAlertDialog,
  UiModal,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
} from '@/components/ui';
import {
  Ban,
  Check,
  ChevronDown,
  ChevronRight,
  CircleArrowRight,
  CircleCheck,
  CircleAlert,
  Columns3,
  Copy,
  Download,
  Eye,
  EyeOff,
  FileSpreadsheet,
  Images,
  Layers3,
  MoreHorizontal,
  Package,
  PackageCheck,
  Pencil,
  Plus,
  Power,
  PowerOff,
  Printer,
  Search,
  ShieldCheck,
  Tags,
  Trash2,
  Upload,
  Users,
  X,
} from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { categories, productColumns, products as seedProducts, tempMaterials } from './product.data';
import ProductAddModal from './ProductAddModal.vue';
import ProductAliasManager from './ProductAliasManager.vue';
import ProductFeedbackPanel from './ProductFeedbackPanel.vue';
import ProductHero from './ProductHero.vue';
import ProductImportModal from './ProductImportModal.vue';
import ProductMediaManager from './ProductMediaManager.vue';
import ProductPrintModal from './ProductPrintModal.vue';
import ProductPublishModal from './ProductPublishModal.vue';
import ProductTable from './ProductTable.vue';
import ProductTrashModal from './ProductTrashModal.vue';
import type { DeletedProduct, Product, ProductDraft, ProductSaveAction } from './product.types';

type MainTab = 'products' | 'temp' | 'aliases' | 'media';
type Popover = 'batch' | 'more' | 'brand' | 'category' | 'status' | 'columns' | null;
type ModalKind =
  | 'add'
  | 'import'
  | 'print'
  | 'trash'
  | 'delete'
  | 'publish'
  | 'batch-enable'
  | 'batch-disable'
  | 'batch-delete'
  | 'group-delete'
  | null;

const rows = ref<Product[]>(structuredClone(seedProducts));
const trashRows = ref<DeletedProduct[]>([]);
const activeTab = ref<MainTab>('products');
const mainTabValues = ['products', 'temp', 'aliases', 'media'] as const satisfies readonly MainTab[];
const activeTabModel = computed<string>({
  get: () => activeTab.value,
  set: (value) => {
    const nextTab = mainTabValues.find((tab) => tab === value);
    if (nextTab) activeTab.value = nextTab;
  },
});
const activeCategory = ref('全部');
const query = ref('');
const selected = ref(new Set<string>());
const popover = ref<Popover>(null);
const modal = ref<ModalKind>(null);
const pendingProduct = ref<Product | null>(null);
const visibleColumns = ref(
  new Set(productColumns.filter((column) => column.defaultVisible).map((column) => column.key)),
);
const selectedBrands = ref<string[]>([]);
const selectedCategories = ref<string[]>([]);
const selectedStatus = ref('');
const sortKey = ref('');
const sortDirection = ref<'asc' | 'desc'>('asc');
const creatingGroup = ref(false);
const groupName = ref('');
const groupRows = ref(structuredClone(categories));
const editingGroup = ref('');
const editingGroupName = ref('');
const pendingGroupDelete = ref<{ name: string; count: number } | null>(null);
const commonCategories = ref(['PLC', '触摸屏/HMI', '电源', '配件', '柜体/箱体', '伺服驱动']);
const categoryMenuOpen = ref(false);
const editingCommonCategories = ref(false);
const draftCommonCategories = ref<string[]>([]);
const tempRows = ref(structuredClone(tempMaterials));
const tempQuery = ref('');
const tempStatus = ref('');
const selectedTemp = ref(new Set<string>());
const feedbackVisible = ref(true);
const feedbackOpen = ref(false);
const tempStatusOptions = [
  { value: '', label: '全部状态' },
  { value: '待转正', label: '待转正' },
  { value: '已转正', label: '已转正' },
  { value: '已废弃', label: '已废弃' },
];
const dangerModalOpen = computed({
  get: () => modal.value === 'delete' || modal.value === 'batch-delete' || modal.value === 'group-delete',
  set: (open: boolean) => {
    if (!open) closeConfirmation();
  },
});
const dangerModalTitle = computed(() => {
  if (modal.value === 'delete') return '删除商品';
  if (modal.value === 'batch-delete') return '批量删除商品';
  return '删除商品分组';
});
const dangerModalDescription = computed(() => {
  if (modal.value === 'delete') {
    return `确定要删除商品「${pendingProduct.value?.name}」吗？删除后可在回收站中恢复。`;
  }
  if (modal.value === 'batch-delete') {
    return `确定要删除选中的 ${selected.value.size} 个商品吗？删除后可在回收站中恢复。`;
  }
  if (pendingGroupDelete.value?.count) {
    return `确定删除分组「${pendingGroupDelete.value.name}」吗？该分组下有 ${pendingGroupDelete.value.count} 个商品，删除后这些商品将变为「未分组」（不影响商品本身）。`;
  }
  return `确定删除分组「${pendingGroupDelete.value?.name}」吗？该分组下暂无商品。`;
});

function closeConfirmation() {
  modal.value = null;
  pendingProduct.value = null;
  pendingGroupDelete.value = null;
}

const allGroups = computed(() => groupRows.value);
const hiddenCategories = computed(() =>
  groupRows.value.map((item) => item.name).filter((name) => !commonCategories.value.includes(name)),
);
const brandOptions = [
  { name: '三菱', count: 1 },
  { name: '台达', count: 0 },
  { name: '威纶通', count: 2 },
  { name: '昆仑通态', count: 1 },
  { name: '明纬', count: 1 },
  { name: '汇川技术', count: 0 },
  { name: '西门子', count: 2 },
];
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  const result = rows.value.filter((product) => {
    const matchCategory = activeCategory.value === '全部' || product.category === activeCategory.value;
    const matchKeyword =
      !keyword ||
      [product.name, product.model, product.barcode, product.brand].some((value) =>
        value.toLowerCase().includes(keyword),
      );
    const matchBrand = selectedBrands.value.length === 0 || selectedBrands.value.includes(product.brand);
    const matchFilterCategory =
      selectedCategories.value.length === 0 || selectedCategories.value.includes(product.category);
    const matchStatus = !selectedStatus.value || product.status === selectedStatus.value;
    return matchCategory && matchKeyword && matchBrand && matchFilterCategory && matchStatus;
  });
  if (!sortKey.value) return result;
  return [...result].sort((a, b) => {
    const aValue = a[sortKey.value as keyof Product];
    const bValue = b[sortKey.value as keyof Product];
    const direction = sortDirection.value === 'asc' ? 1 : -1;
    if (typeof aValue === 'number' && typeof bValue === 'number') return (aValue - bValue) * direction;
    return String(aValue).localeCompare(String(bValue), 'zh-CN') * direction;
  });
});

const activeFilters = computed(
  () => selectedBrands.value.length + selectedCategories.value.length + (selectedStatus.value ? 1 : 0),
);
const selectedProducts = computed(() => rows.value.filter((product) => selected.value.has(product.id)));
const filteredTempRows = computed(() =>
  tempRows.value.filter((item) => {
    const keyword = tempQuery.value.trim().toLowerCase();
    return (
      (!tempStatus.value || item.status === tempStatus.value) &&
      (!keyword || [item.name, item.source, item.spec].some((value) => value.toLowerCase().includes(keyword)))
    );
  }),
);

function setPopover(value: Exclude<Popover, null>) {
  popover.value = popover.value === value ? null : value;
}

function notify(message: string) {
  if (/请先|请输入|暂无|已存在|不能/.test(message)) toast.warning(message);
  else toast.success(message);
}

function toggleArray(list: string[], value: string) {
  return list.includes(value) ? list.filter((item) => item !== value) : [...list, value];
}

function toggleRow(id: string) {
  const next = new Set(selected.value);
  next.has(id) ? next.delete(id) : next.add(id);
  selected.value = next;
}

function toggleAll() {
  const everySelected = filteredRows.value.every((item) => selected.value.has(item.id));
  selected.value = everySelected ? new Set() : new Set(filteredRows.value.map((item) => item.id));
}

function applySort(key: string) {
  if (sortKey.value === key) {
    if (sortDirection.value === 'asc') sortDirection.value = 'desc';
    else {
      sortKey.value = '';
      sortDirection.value = 'asc';
    }
  } else {
    sortKey.value = key;
    sortDirection.value = 'asc';
  }
}

function askStatus(product: Product) {
  pendingProduct.value = product;
  modal.value = product.status === '启用' ? 'batch-disable' : 'batch-enable';
}

function askDelete(product: Product) {
  pendingProduct.value = product;
  modal.value = 'delete';
}

function confirmModal() {
  if (modal.value === 'delete' && pendingProduct.value) {
    trashRows.value = [
      ...trashRows.value,
      { ...pendingProduct.value, deletedAt: new Date().toISOString().replace('T', ' ').slice(0, 16) },
    ];
    rows.value = rows.value.filter((item) => item.id !== pendingProduct.value?.id);
    notify(`已将商品「${pendingProduct.value.name}」移入回收站`);
  } else if (modal.value === 'batch-enable' || modal.value === 'batch-disable') {
    const status = modal.value === 'batch-enable' ? '启用' : '停用';
    const ids = pendingProduct.value ? new Set([pendingProduct.value.id]) : selected.value;
    rows.value = rows.value.map((item) => (ids.has(item.id) ? { ...item, status } : item));
    notify(`已${status} ${ids.size} 个商品`);
    selected.value = new Set();
  } else if (modal.value === 'batch-delete') {
    const ids = selected.value;
    const deletedAt = new Date().toISOString().replace('T', ' ').slice(0, 16);
    trashRows.value = [
      ...trashRows.value,
      ...rows.value.filter((item) => ids.has(item.id)).map((item) => ({ ...item, deletedAt })),
    ];
    rows.value = rows.value.filter((item) => !ids.has(item.id));
    notify(`已将 ${ids.size} 个商品移入回收站`);
    selected.value = new Set();
  } else if (modal.value === 'group-delete' && pendingGroupDelete.value) {
    const group = pendingGroupDelete.value;
    groupRows.value = groupRows.value.filter((item) => item.name !== group.name);
    rows.value = rows.value.map((item) => (item.category === group.name ? { ...item, category: '未分组' } : item));
    if (activeCategory.value === group.name) activeCategory.value = '全部';
    commonCategories.value = commonCategories.value.filter((name) => name !== group.name);
    notify(`已删除分组「${group.name}」`);
  } else if (modal.value === 'import') {
    const imported = [
      {
        name: '汇川 SV660N',
        model: 'SV660N-020A',
        category: '伺服驱动',
        brand: '汇川',
        spec: '2kW 伺服驱动器',
        unit: '台',
        purchasePrice: 3200,
        presetSalePrice1: 4800,
        barcode: '6901234567020',
      },
      {
        name: '欧姆龙 CP1H-X40DT',
        model: 'CP1H-X40DT-D',
        category: 'PLC',
        brand: '欧姆龙',
        spec: '24DI/16DO',
        unit: '台',
        purchasePrice: 2800,
        presetSalePrice1: 4000,
        barcode: '6901234567021',
      },
      {
        name: '信捷 TG765-XT',
        model: 'TG765-XT',
        category: '触摸屏/HMI',
        brand: '信捷',
        spec: '7寸 800×480',
        unit: '台',
        purchasePrice: 1500,
        presetSalePrice1: 2200,
        barcode: '6901234567022',
      },
      {
        name: '台达 DVP-28SV2',
        model: 'DVP28SV211T',
        category: 'PLC',
        brand: '台达',
        spec: '16DI/12DO',
        unit: '台',
        purchasePrice: 1800,
        presetSalePrice1: 2600,
        barcode: '6901234567023',
      },
      {
        name: '菲尼克斯 端子排',
        model: 'UT-4',
        category: '配件',
        brand: '菲尼克斯',
        spec: '4mm² 直通型',
        unit: '个',
        purchasePrice: 5,
        presetSalePrice1: 12,
        barcode: '6901234567025',
      },
    ].map((item, index): Product => ({
      ...seedProducts[0]!,
      ...item,
      id: `SP-IMP-${Date.now().toString().slice(-4)}${String(index).padStart(2, '0')}`,
      recentPurchasePrice: 0,
      recentSalePrice: 0,
      createdAt: new Date().toISOString().replace('T', ' ').slice(0, 16),
      owner: '',
      department: '',
      purchaseCategory: item.category,
      defaultSupplier: '',
      mainImage: '',
    }));
    rows.value = [...rows.value, ...imported];
    notify('成功导入 5 条商品数据');
  } else if (modal.value === 'print') notify(`正在生成 ${selected.value.size || rows.value.length} 个条码标签`);
  modal.value = null;
  pendingProduct.value = null;
  pendingGroupDelete.value = null;
}

function publishSelectedProducts() {
  const count = selectedProducts.value.length;
  modal.value = null;
  selected.value = new Set();
  notify(`已成功发布 ${count} 个商品到共享供应链`);
}

function restoreProduct(id: string) {
  const product = trashRows.value.find((item) => item.id === id);
  if (!product) return;
  const { deletedAt: _deletedAt, ...restored } = product;
  rows.value = [...rows.value, restored];
  trashRows.value = trashRows.value.filter((item) => item.id !== id);
  notify(`已恢复商品「${product.name}」`);
}

function permanentlyDeleteProduct(id: string) {
  const product = trashRows.value.find((item) => item.id === id);
  trashRows.value = trashRows.value.filter((item) => item.id !== id);
  if (product) notify(`已永久删除商品「${product.name}」`);
}

function clearTrash() {
  trashRows.value = [];
  notify('回收站已清空');
}

function saveProduct(draft: ProductDraft, action: ProductSaveAction) {
  const stamp = Date.now().toString();
  const product: Product = {
    id: `SP-NEW-${stamp.slice(-7)}`,
    name: draft.name.trim(),
    model: draft.model.trim(),
    category: draft.category || '未分类',
    subCategory: draft.subCategory,
    brand: draft.brand || '-',
    spec: draft.spec.trim(),
    unit: draft.unit,
    barcode: draft.barcode.trim() || `690123${stamp.slice(-7)}`,
    referenceCost: draft.referenceCost,
    purchasePrice: draft.purchasePrice,
    recentPurchasePrice: 0,
    recentSalePrice: 0,
    presetSalePrice1: draft.salePrice,
    createdAt: new Date().toISOString().replace('T', ' ').slice(0, 16),
    status: '启用',
    department: draft.department.trim(),
    owner: draft.owner.trim(),
    purchaseCategory: draft.purchaseCategory.trim() || draft.category,
    taxIncluded: draft.taxIncluded,
    defaultTaxRate: draft.defaultTaxRate,
    defaultSupplier: draft.defaultSupplier.trim(),
    mainImage: draft.images[0] ?? '',
    allowDecimalQty: draft.allowDecimalQty,
    unitConversions: draft.unitConversions.map((item) => ({ ...item })),
    documents: [...draft.documents],
    enableBatchManagement: draft.enableBatchManagement,
    enableTraceability: draft.enableTraceability,
    shelfLifeDays: draft.shelfLifeDays,
    batchCodeRule: draft.batchCodeRule,
    hasSpecs: draft.hasSpecs,
    specDimensions: draft.specDimensions.map((item) => ({
      ...item,
      values: item.values.map((value) => ({ ...value })),
    })),
    skus: draft.skus.map((item) => ({ ...item })),
    customFields: Object.fromEntries(
      Object.entries(draft.customFields).map(([key, value]) => [key, Array.isArray(value) ? [...value] : value]),
    ),
  };
  rows.value = [...rows.value, product];
  if (action === 'close') {
    modal.value = null;
    notify(`商品「${product.name}」已新增`);
  } else if (action === 'new') {
    notify('已保存，可继续新增下一个商品');
  } else {
    notify('已保存并复制为新建，可继续修改');
  }
}

function startBatch(kind: ModalKind) {
  popover.value = null;
  if (selected.value.size === 0) {
    notify('请先选择商品');
    return;
  }
  if (kind) modal.value = kind;
  else notify('该批量功能已接收当前选择');
}

function batchInfo(message: string) {
  popover.value = null;
  if (selected.value.size === 0) {
    notify('请先选择商品');
    return;
  }
  notify(`已选择 ${selected.value.size} 个商品，${message}`);
}

function batchCopy() {
  popover.value = null;
  if (selected.value.size === 0) {
    notify('请先选择商品');
    return;
  }
  const timestamp = Date.now().toString().slice(-5);
  const copies = rows.value
    .filter((item) => selected.value.has(item.id))
    .map((item, index) => ({
      ...item,
      id: `SP-COPY-${timestamp}${index}`,
      name: `${item.name} (副本)`,
      barcode: '',
      createdAt: new Date().toISOString().replace('T', ' ').slice(0, 16),
    }));
  rows.value = [...rows.value, ...copies];
  notify(`已复制 ${copies.length} 个商品`);
  selected.value = new Set();
}

function printProductList() {
  popover.value = null;
  notify('正在准备打印预览...');
  window.print();
}

function copyBarcode(product: Product) {
  if (!product.barcode) {
    notify('该商品暂无条码');
    return;
  }
  void navigator.clipboard?.writeText(product.barcode);
  notify(`已复制条码 ${product.barcode}`);
}

function resetFilters() {
  selectedBrands.value = [];
  selectedCategories.value = [];
  selectedStatus.value = '';
  activeCategory.value = '全部';
  query.value = '';
}

function addGroup() {
  const name = groupName.value.trim();
  if (!name) {
    notify('请输入分组名称');
    return;
  }
  if (allGroups.value.some((group) => group.name === name)) {
    notify('分组名称已存在');
    return;
  }
  groupRows.value = [...groupRows.value, { name, count: 0, color: '#4C9AFF' }];
  groupName.value = '';
  creatingGroup.value = false;
  notify(`已新增分组「${name}」`);
}

function startRenameGroup(name: string) {
  editingGroup.value = name;
  editingGroupName.value = name;
}

function saveRenameGroup() {
  const oldName = editingGroup.value;
  const newName = editingGroupName.value.trim();
  if (!newName) {
    notify('请输入分组名称');
    return;
  }
  if (newName !== oldName && allGroups.value.some((group) => group.name === newName)) {
    notify('分组名称已存在');
    return;
  }
  if (newName !== oldName) {
    groupRows.value = groupRows.value.map((group) => (group.name === oldName ? { ...group, name: newName } : group));
    rows.value = rows.value.map((item) => (item.category === oldName ? { ...item, category: newName } : item));
    commonCategories.value = commonCategories.value.map((name) => (name === oldName ? newName : name));
    if (activeCategory.value === oldName) activeCategory.value = newName;
    notify(`分组已重命名为「${newName}」`);
  }
  editingGroup.value = '';
}

function askDeleteGroup(name: string) {
  pendingGroupDelete.value = { name, count: rows.value.filter((item) => item.category === name).length };
  modal.value = 'group-delete';
}

function toggleTemp(id: string) {
  const next = new Set(selectedTemp.value);
  next.has(id) ? next.delete(id) : next.add(id);
  selectedTemp.value = next;
}

function toggleAllTemp() {
  const everySelected =
    filteredTempRows.value.length > 0 && filteredTempRows.value.every((item) => selectedTemp.value.has(item.id));
  selectedTemp.value = everySelected ? new Set() : new Set(filteredTempRows.value.map((item) => item.id));
}

function updateTempStatus(ids: Set<string>, status: string) {
  tempRows.value = tempRows.value.map((item) =>
    ids.has(item.id) && item.status === '待转正' ? { ...item, status } : item,
  );
  selectedTemp.value = new Set();
  notify(status === '已转正' ? `已批量转正 ${ids.size} 项临时物料` : '已批量废弃');
}

function openCommonCategoryEditor() {
  draftCommonCategories.value = [...commonCategories.value];
  editingCommonCategories.value = true;
}

function toggleCommonCategory(name: string) {
  draftCommonCategories.value = draftCommonCategories.value.includes(name)
    ? draftCommonCategories.value.filter((item) => item !== name)
    : draftCommonCategories.value.length < 6
      ? [...draftCommonCategories.value, name]
      : draftCommonCategories.value;
}

function saveCommonCategories() {
  commonCategories.value = [...draftCommonCategories.value];
  editingCommonCategories.value = false;
  categoryMenuOpen.value = false;
  notify('常用分类已更新');
}

function groupStyleClass(color: string) {
  const classes: Record<string, string> = {
    '#0052CC': 'groupBlue',
    '#00875A': 'groupGreen',
    '#FF8B00': 'groupOrange',
    '#6554C0': 'groupPurple',
    '#00A3BF': 'groupCyan',
    '#FF5630': 'groupRed',
    '#36B37E': 'groupMint',
    '#FFAB00': 'groupYellow',
    '#97A0AF': 'groupGray',
    '#4C9AFF': 'groupLightBlue',
  };
  return classes[color] ?? 'groupLightBlue';
}

function tempCardStyleClass(tone: string) {
  return { warning: 'tempWarning', success: 'tempSuccess', neutral: 'tempNeutral' }[tone] ?? 'tempNeutral';
}

function closeFloating() {
  popover.value = null;
  categoryMenuOpen.value = false;
  editingCommonCategories.value = false;
}
function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') closeFloating();
}
onMounted(() => {
  document.addEventListener('mousedown', closeFloating);
  document.addEventListener('keydown', onKeydown);
});
onUnmounted(() => {
  document.removeEventListener('mousedown', closeFloating);
  document.removeEventListener('keydown', onKeydown);
});
</script>

<template>
  <div
    :class="$style.div"
    data-page-route="/base/products"
  >
    <ProductHero />

    <div :class="$style.row">
      <ShieldCheck
        :size="13"
        :class="$style.icon"
      />
      <span :class="$style.label">我的管辖范围</span>
      <span :class="$style.badge">PLC</span>
      <span :class="$style.badge">配件</span>
      <span :class="$style.badgeSuccess">西门子</span>
      <span :class="$style.badgeSuccess">三菱</span>
      <span :class="$style.badgePurple">上海主仓</span>
      <span :class="$style.labelCompact">完全管理权限 · 管理员可查看全部</span>
    </div>

    <UiTabs v-model="activeTabModel">
      <div :class="$style.rowWithBottomDivider">
        <UiTabsList
          label="商品管理视图"
          :class="$style.mainTabList"
        >
          <UiTabsTrigger
            value="products"
            :class="$style.mainTabTrigger"
          >
            <Package :size="14" /> 正式商品
            <span :class="$style.mainTabIndicator" />
          </UiTabsTrigger>
          <UiTabsTrigger
            value="temp"
            :class="[$style.mainTabTrigger, $style.mainTabTriggerWarning]"
          >
            <FileSpreadsheet :size="14" /> 临时物料清单 <span :class="$style.badgeWarning">5</span>
            <span :class="$style.mainTabIndicator" />
          </UiTabsTrigger>
          <UiTabsTrigger
            value="aliases"
            :class="$style.mainTabTrigger"
          >
            <Users :size="14" /> 商品别名
            <span :class="$style.mainTabIndicator" />
          </UiTabsTrigger>
          <UiTabsTrigger
            value="media"
            :class="$style.mainTabTrigger"
          >
            <Images :size="14" /> 商品图片 <span :class="$style.badgePrimary">NEW</span>
            <span :class="$style.mainTabIndicator" />
          </UiTabsTrigger>
        </UiTabsList>
        <div :class="$style.divTrailing">
          <label :class="$style.field">
            <Search
              :size="14"
              :class="$style.iconMuted"
            />
            <input
              v-model="query"
              placeholder="搜索商品名称、条码、型号、品牌..."
              :class="$style.searchInput"
            />
            <button
              v-if="query"
              @click="query = ''"
              :class="$style.iconButton"
            >
              <X :size="12" />
            </button>
          </label>
        </div>
      </div>

      <UiTabPanel
        value="products"
        :class="$style.mainTabPanel"
      >
        <div :class="$style.rowMt3Gap4">
          <aside :class="$style.sidePanel">
            <div :class="$style.rowMuted">
              <span :class="$style.labelStrong"
                ><Layers3
                  :size="13"
                  :class="$style.iconPrimary"
                />商品分组</span
              >
              <button
                title="新增分组"
                @click="
                  creatingGroup = true;
                  groupName = '';
                "
                :class="$style.addButton"
              >
                <Plus :size="14" />
              </button>
            </div>
            <div :class="$style.divScrollable">
              <button
                :class="[
                  $style.buttonWide,
                  activeCategory === '全部' ? $style.stateSelectedPrimary : $style.stateMuted,
                ]"
                @click="activeCategory = '全部'"
              >
                <span :class="$style.labelGap2"><Tags :size="13" />全部商品</span
                ><span :class="$style.badgeRound">{{ rows.length }}</span>
              </button>
              <div
                v-if="creatingGroup"
                :class="$style.rowGap1Px2Py15"
              >
                <input
                  v-model="groupName"
                  autofocus
                  placeholder="新分组名称"
                  @keydown.enter="addGroup"
                  @keydown.escape="creatingGroup = false"
                  :class="$style.input"
                />
                <button
                  title="确认"
                  @click="addGroup"
                  :class="$style.confirmButton"
                >
                  <Check :size="14" />
                </button>
                <button
                  title="取消"
                  @click="creatingGroup = false"
                  :class="$style.cancelButton"
                >
                  <X :size="14" />
                </button>
              </div>
              <template
                v-for="group in allGroups"
                :key="group.name"
              >
                <div
                  v-if="editingGroup === group.name"
                  :class="$style.rowGap1Px2Py15"
                >
                  <input
                    v-model="editingGroupName"
                    autofocus
                    @keydown.enter="saveRenameGroup"
                    @keydown.escape="editingGroup = ''"
                    :class="$style.input"
                  /><button
                    title="保存"
                    @click="saveRenameGroup"
                    :class="$style.confirmButton"
                  >
                    <Check :size="14" /></button
                  ><button
                    title="取消"
                    @click="editingGroup = ''"
                    :class="$style.cancelButton"
                  >
                    <X :size="14" />
                  </button>
                </div>
                <div
                  v-else
                  :class="[
                    $style.rowPy2Pl15Pr2,
                    activeCategory === group.name ? $style.stateSelectedPrimaryAlternative : $style.stateMutedMuted,
                  ]"
                >
                  <span :class="$style.labelP05"><ChevronRight :size="13" /></span
                  ><button
                    @click="activeCategory = group.name"
                    :class="$style.buttonWideGap2"
                  >
                    <span :class="[$style.labelRound, $style[groupStyleClass(group.color)]]" /><span
                      :class="[
                        $style.labelWide,
                        activeCategory === group.name ? $style.stateSelected : $style.stateDefaultAlternative,
                      ]"
                      >{{ group.name }}</span
                    ></button
                  ><span :class="$style.badgeRoundCompact">{{
                    rows.filter((item) => item.category === group.name).length
                  }}</span>
                  <div :class="$style.rowGap05">
                    <button
                      title="重命名"
                      @click="startRenameGroup(group.name)"
                      :class="$style.iconButtonMuted"
                    >
                      <Pencil :size="12" /></button
                    ><button
                      title="删除分组"
                      @click="askDeleteGroup(group.name)"
                      :class="$style.cancelButton"
                    >
                      <Trash2 :size="12" />
                    </button>
                  </div>
                </div>
              </template>
            </div>
          </aside>

          <div :class="$style.rowWide">
            <div :class="$style.rowWithBottomDivider">
              <button
                v-for="category in ['全部', ...commonCategories]"
                :key="category"
                :class="[
                  $style.categoryButton,
                  activeCategory === category ? $style.stateSelected : $style.stateDefault,
                ]"
                @click="activeCategory = category"
              >
                {{ category
                }}<span
                  v-if="activeCategory === category"
                  :class="$style.labelPrimary"
                />
              </button>
              <div
                v-if="hiddenCategories.length"
                @mousedown.stop
                :class="$style.divAlternative"
              >
                <button
                  :class="[
                    $style.buttonGap1Px4Py25,
                    hiddenCategories.includes(activeCategory) || categoryMenuOpen
                      ? $style.stateSelected
                      : $style.stateDefault,
                  ]"
                  @click="
                    categoryMenuOpen = !categoryMenuOpen;
                    editingCommonCategories = false;
                  "
                >
                  {{ hiddenCategories.includes(activeCategory) ? activeCategory : '更多' }}
                  <ChevronDown :size="13" /><span
                    v-if="hiddenCategories.includes(activeCategory)"
                    :class="$style.labelPrimary"
                  />
                </button>
                <div
                  v-if="categoryMenuOpen"
                  @mousedown.stop
                  :class="$style.card"
                >
                  <div
                    v-if="editingCommonCategories"
                    :class="$style.divW320pxP4"
                  >
                    <div :class="$style.rowMb3">
                      <strong :class="$style.strongLabel">选择常用分类</strong
                      ><span :class="$style.labelCompactMuted">已选 {{ draftCommonCategories.length }}/6</span>
                    </div>
                    <p :class="$style.description">勾选最多 6 个分类固定显示在标签栏，其余分类收入「更多」中</p>
                    <div :class="$style.list">
                      <label
                        v-for="category in groupRows"
                        :key="category.name"
                        :class="[
                          $style.categoryField,
                          draftCommonCategories.includes(category.name)
                            ? $style.stateSelectedPrimaryAlternativeAlternative
                            : draftCommonCategories.length >= 6
                              ? $style.stateDefaultFaded
                              : $style.stateMutedMuted,
                        ]"
                        ><input
                          type="checkbox"
                          :checked="draftCommonCategories.includes(category.name)"
                          :disabled="
                            !draftCommonCategories.includes(category.name) && draftCommonCategories.length >= 6
                          "
                          @change="toggleCommonCategory(category.name)"
                          :class="$style.checkbox" /><span>{{ category.name }}</span
                        ><Check
                          v-if="draftCommonCategories.includes(category.name)"
                          :size="11"
                          :class="$style.iconPrimaryTrailing"
                      /></label>
                    </div>
                    <div :class="$style.rowMutedWithTopDivider">
                      <button
                        @click="editingCommonCategories = false"
                        :class="$style.buttonPx3Py15"
                      >
                        取消</button
                      ><button
                        @click="saveCommonCategories"
                        :class="$style.primaryButton"
                      >
                        保存
                      </button>
                    </div>
                  </div>
                  <template v-else
                    ><button
                      v-for="category in hiddenCategories"
                      :key="category"
                      :class="[
                        $style.categoryButtonWide,
                        activeCategory === category ? $style.stateSelectedPrimary : '',
                      ]"
                      @click="
                        activeCategory = category;
                        categoryMenuOpen = false;
                      "
                    >
                      {{ category
                      }}<Check
                        v-if="activeCategory === category"
                        :size="14"
                      />
                    </button>
                    <div :class="$style.divMuted">
                      <button
                        @click="openCommonCategoryEditor"
                        :class="$style.primaryButtonPrimary"
                      >
                        <Columns3 :size="13" />编辑常用分类
                      </button>
                    </div></template
                  >
                </div>
              </div>
            </div>

            <section :class="$style.section">
              <div :class="$style.rowMutedWithBottomDivider">
                <button
                  @click="modal = 'add'"
                  :class="$style.primaryButtonPrimaryInverted"
                >
                  新增
                </button>
                <button
                  @click="modal = 'import'"
                  :class="$style.buttonMuted"
                >
                  <Upload
                    :size="14"
                    :class="$style.iconMutedAlternative"
                  />批量导入
                </button>
                <button
                  @click="modal = 'print'"
                  :class="$style.buttonMuted"
                >
                  <Printer
                    :size="14"
                    :class="$style.iconMutedAlternative"
                  />打印条码
                </button>

                <div
                  @mousedown.stop
                  :class="$style.divAlternative"
                >
                  <button
                    :class="[
                      $style.primaryButtonPrimaryMuted,
                      popover === 'batch'
                        ? $style.stateSelectedPrimaryAlternativeAlternativeAlternative
                        : $style.stateDefaultAlternativeAlternative,
                    ]"
                    @click="setPopover('batch')"
                  >
                    批量操作 <ChevronDown :size="14" />
                  </button>
                  <div
                    v-if="popover === 'batch'"
                    :class="$style.cardFloating"
                  >
                    <button
                      @click="startBatch('batch-enable')"
                      :class="$style.buttonWideMuted"
                    >
                      <Power
                        :size="14"
                        :class="$style.iconSuccess"
                      />批量启用
                    </button>
                    <button
                      @click="startBatch('batch-disable')"
                      :class="$style.buttonWideMuted"
                    >
                      <PowerOff
                        :size="14"
                        :class="$style.iconWarning"
                      />批量停用
                    </button>
                    <button
                      @click="startBatch('publish')"
                      :class="$style.buttonWideMuted"
                    >
                      <PackageCheck :size="14" />发布到共享供应链
                    </button>
                    <button
                      @click="batchInfo('批量修改功能开发中')"
                      :class="$style.buttonWideMuted"
                    >
                      <Package :size="14" />批量修改商品
                    </button>
                    <button
                      @click="startBatch('batch-delete')"
                      :class="$style.dangerButton"
                    >
                      <Trash2 :size="14" />批量删除商品
                    </button>
                    <button
                      @click="batchInfo('批量价格修改功能开发中')"
                      :class="$style.buttonWideMuted"
                    >
                      <Download :size="14" />批量修改价格
                    </button>
                    <button
                      @click="batchCopy"
                      :class="$style.buttonWideMuted"
                    >
                      <Copy :size="14" />批量复制商品
                    </button>
                    <button
                      @click="startBatch('print')"
                      :class="$style.buttonWideMuted"
                    >
                      <Printer :size="14" />批量打印条码
                    </button>
                  </div>
                </div>

                <div
                  @mousedown.stop
                  :class="$style.divAlternative"
                >
                  <button
                    @click="setPopover('more')"
                    :class="$style.buttonMuted"
                  >
                    更多 <ChevronDown :size="14" />
                  </button>
                  <div
                    v-if="popover === 'more'"
                    :class="$style.cardFloatingElevated"
                  >
                    <button
                      @click="
                        notify(`正在导出 ${filteredRows.length} 条商品数据...`);
                        popover = null;
                      "
                      :class="$style.buttonWideMutedWFullPx4Py25"
                    >
                      导出商品
                    </button>
                    <button
                      @click="printProductList"
                      :class="$style.buttonWideMutedWFullPx4Py25"
                    >
                      打印列表
                    </button>
                    <button
                      @click="
                        modal = 'trash';
                        popover = null;
                      "
                      :class="$style.buttonWideMutedWFullPx4Py25"
                    >
                      回收站
                    </button>
                  </div>
                </div>

                <span
                  v-if="selected.size"
                  :class="$style.labelPrimaryMl2"
                  >已选 {{ selected.size }} 项</span
                >
                <span :class="$style.labelWideAlternative" />

                <div
                  v-for="filterName in ['brand', 'category', 'status'] as const"
                  :key="filterName"
                  @mousedown.stop
                  :class="$style.divAlternative"
                >
                  <button
                    :class="[
                      $style.buttonMutedGap15Px3Py7px,
                      popover === filterName
                        ? $style.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternative
                        : $style.stateDefaultAlternativeAlternativeAlternative,
                    ]"
                    @click="setPopover(filterName)"
                  >
                    {{
                      filterName === 'brand'
                        ? selectedBrands.length
                          ? `品牌 (${selectedBrands.length})`
                          : '品牌'
                        : filterName === 'category'
                          ? selectedCategories.length
                            ? `分类 (${selectedCategories.length})`
                            : '分类'
                          : selectedStatus || '状态'
                    }}
                    <ChevronDown :size="14" />
                  </button>
                  <div
                    v-if="popover === filterName"
                    :class="[
                      $style.cardFloatingElevatedMt1Py1,
                      filterName === 'status' ? $style.stateSized : $style.stateSizedW220px,
                    ]"
                  >
                    <template v-if="filterName === 'brand'">
                      <div :class="$style.divCompact">选择品牌</div>
                      <div :class="$style.divScrollableH240pxPy1">
                        <label
                          v-for="brand in brandOptions"
                          :key="brand.name"
                          :class="$style.brandField"
                          ><input
                            type="checkbox"
                            :checked="selectedBrands.includes(brand.name)"
                            @change="selectedBrands = toggleArray(selectedBrands, brand.name)"
                            :class="$style.checkboxPrimary"
                          /><span :class="$style.labelAlternative">{{ brand.name }}</span
                          ><span :class="$style.labelCompactMutedTrailing">{{ brand.count }}</span></label
                        >
                      </div>
                    </template>
                    <template v-else-if="filterName === 'category'">
                      <div :class="$style.divCompact">选择分类</div>
                      <div :class="$style.divScrollableH240pxPy1">
                        <label
                          v-for="group in categories"
                          :key="group.name"
                          :class="$style.brandField"
                          ><input
                            type="checkbox"
                            :checked="selectedCategories.includes(group.name)"
                            @change="selectedCategories = toggleArray(selectedCategories, group.name)"
                            :class="$style.checkboxPrimary"
                          /><span :class="$style.labelAlternative">{{ group.name }}</span
                          ><span :class="$style.labelCompactMutedTrailing">{{ group.count }}</span></label
                        >
                      </div>
                    </template>
                    <template v-else>
                      <button
                        v-for="status in ['', '启用', '停用']"
                        :key="status"
                        :class="[
                          $style.statusButton,
                          selectedStatus === status
                            ? $style.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternativeAlternative
                            : $style.stateDefaultAlternative,
                        ]"
                        @click="
                          selectedStatus = status;
                          popover = null;
                        "
                      >
                        {{ status || '全部' }}
                      </button>
                    </template>
                  </div>
                </div>
              </div>

              <div :class="$style.rowH445pxPx4">
                <span :class="$style.labelMuted">共 {{ filteredRows.length }} 条记录</span>
                <button
                  v-if="activeFilters"
                  @click="resetFilters"
                  :class="$style.dangerButtonDanger"
                >
                  <X :size="12" />清除全部
                </button>
                <div
                  @mousedown.stop
                  :class="$style.divTrailingMlAuto"
                >
                  <button
                    :class="[
                      $style.buttonMutedGap15Px3Py15,
                      popover === 'columns'
                        ? $style.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternativeAlternativeAlternative
                        : $style.stateDefaultAlternativeAlternativeAlternative,
                    ]"
                    @click="setPopover('columns')"
                  >
                    <Columns3 :size="13" />列设置
                    <span
                      v-if="visibleColumns.size < productColumns.length"
                      :class="$style.badgePrimaryRound"
                      >{{ visibleColumns.size }}</span
                    >
                  </button>
                  <div
                    v-if="popover === 'columns'"
                    :class="$style.cardFloatingElevatedMt1W260px"
                  >
                    <div :class="$style.rowMutedWithBottomDividerPx4Py3">
                      <span :class="$style.labelStrongAlternative">显示列</span
                      ><button
                        @click="
                          visibleColumns = new Set(productColumns.filter((c) => c.defaultVisible).map((c) => c.key))
                        "
                        :class="$style.primaryButtonPrimaryCompact"
                      >
                        重置默认
                      </button>
                    </div>
                    <div :class="$style.divScrollablePy1">
                      <label
                        v-for="column in productColumns"
                        :key="column.key"
                        :class="[
                          $style.columnField,
                          column.alwaysVisible
                            ? $style.stateDefaultFadedAlternative
                            : $style.stateMutedMutedAlternative,
                        ]"
                      >
                        <input
                          type="checkbox"
                          :disabled="column.alwaysVisible"
                          :checked="visibleColumns.has(column.key)"
                          @change="
                            !column.alwaysVisible &&
                            (visibleColumns = new Set(
                              visibleColumns.has(column.key)
                                ? [...visibleColumns].filter((k) => k !== column.key)
                                : [...visibleColumns, column.key],
                            ))
                          "
                          :class="$style.checkboxPrimaryH14pxW14px"
                        />
                        <Eye
                          v-if="visibleColumns.has(column.key)"
                          :size="13"
                          :class="$style.iconMutedAlternative"
                        /><EyeOff
                          v-else
                          :size="13"
                          :class="$style.iconMutedAlternativeAlternative"
                        />
                        <span
                          :class="[
                            $style.labelWideAlternative,
                            visibleColumns.has(column.key)
                              ? $style.stateDefaultAlternative
                              : $style.stateMutedMutedAlternativeAlternative,
                          ]"
                          >{{ column.label }}</span
                        >
                        <span
                          v-if="column.alwaysVisible"
                          :class="$style.labelCompactMutedAlternative"
                          >必选</span
                        >
                      </label>
                    </div>
                    <div :class="$style.rowMutedWithTopDividerPx4Py3">
                      <span :class="$style.labelCompactMutedAlternativeAlternative"
                        >已显示 {{ visibleColumns.size }} / {{ productColumns.length }} 列</span
                      ><button
                        @click="popover = null"
                        :class="$style.primaryButtonPrimaryInvertedPx3Py1"
                      >
                        完成
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </section>

            <ProductTable
              :rows="filteredRows"
              :visible-columns="visibleColumns"
              :selected="selected"
              :sort-key="sortKey"
              :sort-direction="sortDirection"
              @toggle-all="toggleAll"
              @toggle-row="toggleRow"
              @sort="applySort"
              @status="askStatus"
              @remove="askDelete"
              @columns="popover = 'columns'"
              @barcode="copyBarcode"
            />
          </div>
        </div>
      </UiTabPanel>

      <UiTabPanel
        value="temp"
        :class="[$style.mainTabPanel, $style.sectionMt4]"
      >
        <div :class="$style.rowWarning">
          <CircleAlert
            :size="15"
            :class="$style.iconWarningMt05"
          />
          <div>
            <p :class="$style.descriptionWarning">临时物料说明</p>
            <p :class="$style.descriptionCompact">
              以下物料由报价过程中临时创建，尚未录入正式商品库。请确认信息后“转为正式商品”，或将无效记录标记为“废弃”。
            </p>
          </div>
        </div>
        <div :class="$style.grid">
          <div
            v-for="card in [
              {
                label: '待转正',
                value: tempRows.filter((i) => i.status === '待转正').length,
                tone: 'warning',
                icon: FileSpreadsheet,
              },
              {
                label: '已转正',
                value: tempRows.filter((i) => i.status === '已转正').length,
                tone: 'success',
                icon: CircleCheck,
              },
              {
                label: '已废弃',
                value: tempRows.filter((i) => i.status === '已废弃').length,
                tone: 'neutral',
                icon: Ban,
              },
            ]"
            :key="card.label"
            :class="$style.cardCard"
          >
            <span :class="$style.labelCompactMutedGap2"
              ><component
                :is="card.icon"
                :size="14"
                :class="$style[tempCardStyleClass(card.tone)]"
              />{{ card.label }}</span
            ><strong :class="[$style.strongLabelMt1, $style[tempCardStyleClass(card.tone)]]">{{ card.value }}</strong>
          </div>
        </div>
        <div :class="$style.cardWithBottomDivider">
          <div :class="$style.labelGap2">
            <label :class="$style.fieldW240pxGap2Px3"
              ><Search
                :size="13"
                :class="$style.iconMutedAlternative" /><input
                v-model="tempQuery"
                placeholder="搜索物料名称、来源..."
                :class="$style.searchInputWide"
            /></label>
            <UiSelect
              v-model="tempStatus"
              :options="tempStatusOptions"
              label="筛选临时物料状态"
              :class="$style.select"
            />
            <template v-if="selectedTemp.size">
              <span :class="$style.labelCompactMuted">已选 {{ selectedTemp.size }} 项</span>
              <button
                @click="updateTempStatus(selectedTemp, '已转正')"
                :class="$style.primaryButtonSuccess"
              >
                <CircleArrowRight :size="12" />批量转正
              </button>
              <button
                @click="updateTempStatus(selectedTemp, '已废弃')"
                :class="$style.buttonCompact"
              >
                <Ban :size="12" />批量废弃
              </button>
            </template>
          </div>
        </div>
        <div :class="$style.cardScrollable">
          <table :class="$style.table">
            <thead>
              <tr :class="$style.tableRow">
                <th :class="$style.headerCell">
                  <input
                    type="checkbox"
                    :checked="filteredTempRows.length > 0 && filteredTempRows.every((i) => selectedTemp.has(i.id))"
                    @change="toggleAllTemp"
                    :class="$style.checkbox"
                  />
                </th>
                <th>物料名称</th>
                <th>规格型号</th>
                <th>分类</th>
                <th>单位</th>
                <th :class="$style.headerCellRightAligned">预估单价</th>
                <th>来源报价</th>
                <th>创建人</th>
                <th>创建时间</th>
                <th>状态</th>
                <th :class="$style.headerCellCentered">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="item in filteredTempRows"
                :key="item.id"
                :class="$style.itemRow"
              >
                <td :class="$style.headerCellCentered">
                  <input
                    type="checkbox"
                    :checked="selectedTemp.has(item.id)"
                    @change="toggleTemp(item.id)"
                    :class="$style.checkbox"
                  />
                </td>
                <td :class="$style.tableCell"><span :class="$style.badgeWarningCompact">临时</span>{{ item.name }}</td>
                <td :class="$style.iconMutedAlternative">{{ item.spec }}</td>
                <td>
                  <span :class="$style.badgeCompact">{{ item.category }}</span>
                </td>
                <td>{{ item.unit }}</td>
                <td :class="$style.tableCellRightAligned">¥{{ item.estimatedPrice.toLocaleString() }}</td>
                <td>
                  <span :class="$style.labelAlternativeAlternative">{{ item.source }}</span
                  ><span :class="$style.labelCompactMutedAlternativeAlternative">{{ item.quoteNo }}</span>
                </td>
                <td>{{ item.creator }}</td>
                <td :class="$style.iconMutedAlternative">{{ item.createdAt }}</td>
                <td>
                  <span
                    :class="[
                      $style.badgeCompactPx2Py1,
                      item.status === '待转正'
                        ? $style.stateWarningWarning
                        : item.status === '已转正'
                          ? $style.stateSuccess
                          : $style.stateMutedMutedAlternativeAlternativeAlternative,
                    ]"
                    >{{ item.status }}</span
                  >
                </td>
                <td>
                  <div :class="$style.rowGap3">
                    <button
                      title="转为正式商品"
                      @click="updateTempStatus(new Set([item.id]), '已转正')"
                      :class="$style.primaryButtonPrimaryMutedAlternative"
                    >
                      <CircleArrowRight :size="14" /></button
                    ><button
                      title="废弃"
                      @click="updateTempStatus(new Set([item.id]), '已废弃')"
                      :class="$style.dangerButtonDangerMuted"
                    >
                      <Ban :size="14" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </UiTabPanel>

      <UiTabPanel
        value="aliases"
        :class="$style.mainTabPanel"
      >
        <ProductAliasManager :products="rows" />
      </UiTabPanel>

      <UiTabPanel
        value="media"
        :class="$style.mainTabPanel"
      >
        <ProductMediaManager :products="rows" />
      </UiTabPanel>
    </UiTabs>

    <ProductAddModal
      :open="modal === 'add'"
      @close="modal = null"
      @save="saveProduct"
    />
    <ProductImportModal
      :open="modal === 'import'"
      @close="modal = null"
      @import="confirmModal"
    />
    <ProductPrintModal
      :open="modal === 'print'"
      :products="rows"
      @close="modal = null"
    />
    <ProductPublishModal
      :open="modal === 'publish'"
      :products="selectedProducts"
      @close="modal = null"
      @publish="publishSelectedProducts"
    />
    <ProductTrashModal
      :open="modal === 'trash'"
      :products="trashRows"
      @close="modal = null"
      @restore="restoreProduct"
      @permanent-delete="permanentlyDeleteProduct"
      @clear="clearTrash"
    />
    <ProductFeedbackPanel
      :open="feedbackOpen"
      @close="feedbackOpen = false"
    />

    <UiModal
      :open="modal === 'batch-enable' || modal === 'batch-disable'"
      :title="modal === 'batch-enable' ? '批量启用商品' : '批量停用商品'"
      :description="
        modal === 'batch-enable'
          ? `确定要启用 ${pendingProduct ? 1 : selected.size} 个商品吗？`
          : `确定要停用 ${pendingProduct ? 1 : selected.size} 个商品吗？停用后将无法在销售/采购单中使用。`
      "
      :tone="modal === 'batch-disable' ? 'warning' : 'info'"
      confirm-label="确认"
      @close="closeConfirmation"
      @confirm="confirmModal"
    />

    <UiAlertDialog
      v-model:open="dangerModalOpen"
      :title="dangerModalTitle"
      :description="dangerModalDescription"
      confirm-label="确认删除"
      tone="danger"
      @confirm="confirmModal"
    />

    <button
      v-if="feedbackVisible"
      @click="feedbackOpen = !feedbackOpen"
      :class="$style.primaryButtonFloating"
    >
      <MoreHorizontal :size="16" />反馈<span :class="$style.badgeDanger">1</span
      ><span
        role="button"
        aria-label="隐藏反馈入口（刷新页面恢复）"
        @click.stop="
          feedbackVisible = false;
          feedbackOpen = false;
        "
        :class="$style.restoreSpan"
        ><X :size="11"
      /></span>
    </button>
  </div>
</template>

<style module>
@reference "../../styles/index.css";
.groupBlue {
  background-color: #0052cc;
}
.groupGreen {
  background-color: #00875a;
}
.groupOrange {
  background-color: #ff8b00;
}
.groupPurple {
  background-color: #6554c0;
}
.groupCyan {
  background-color: #00a3bf;
}
.groupRed {
  background-color: #ff5630;
}
.groupMint {
  background-color: #36b37e;
}
.groupYellow {
  background-color: #ffab00;
}
.groupGray {
  background-color: #97a0af;
}
.groupLightBlue {
  background-color: #4c9aff;
}

.tempWarning {
  color: #ff8b00;
}
.tempSuccess {
  color: #00875a;
}
.tempNeutral {
  color: #6b778c;
}

/* Semantic template classes */
.div {
  @apply mx-auto max-w-[1400px];
}
.row {
  @apply mb-3 mt-4 flex items-center gap-2 rounded-lg border border-[#B3D4FF] bg-app-primary-soft/60 px-3.5 py-2;
}
.icon {
  @apply shrink-0 text-app-primary;
}
.label {
  @apply text-[11px] font-medium text-app-primary;
}
.badge {
  @apply rounded bg-app-primary-soft px-2 py-0.5 text-[10px] font-medium text-app-primary;
}
.badgeSuccess {
  @apply rounded bg-[#E3FCEF] px-2 py-0.5 text-[10px] font-medium text-[#006644];
}
.badgePurple {
  @apply rounded bg-[#EAE6FF] px-2 py-0.5 text-[10px] font-medium text-[#5243AA];
}
.labelCompact {
  @apply ml-auto shrink-0 text-[10px] text-[#97A0AF];
}
.rowWithBottomDivider {
  @apply flex items-center border-b border-[#DFE1E6];
}
.mainTabList {
  @apply gap-0;
}
.mainTabTrigger {
  @apply relative min-h-0 rounded-none px-5 py-2.5 text-[13px] font-normal;
  color: #5e6c84;
  background: transparent;
}
.mainTabTrigger:hover {
  color: #42526e;
  background: transparent;
}
.mainTabTrigger[data-state='active'] {
  @apply font-semibold;
  color: var(--app-primary);
  background: transparent;
}
.mainTabTriggerWarning[data-state='active'] {
  color: #ff8b00;
}
.mainTabIndicator {
  @apply absolute inset-x-0 bottom-0 h-0.5 opacity-0;
  background: var(--app-primary);
}
.mainTabTrigger[data-state='active'] .mainTabIndicator {
  @apply opacity-100;
}
.mainTabTriggerWarning .mainTabIndicator {
  background: #ff8b00;
}
.mainTabPanel {
  @apply min-w-0;
}
.stateSelected {
  @apply font-semibold text-app-primary;
}
.stateDefault {
  @apply text-[#5E6C84] hover:text-[#42526E];
}
.labelPrimary {
  @apply absolute inset-x-0 bottom-0 h-0.5 bg-app-primary;
}
.badgeWarning {
  @apply ml-1 rounded-full bg-[#FFFAE6] px-1.5 py-0.5 text-[9px] font-bold text-[#FF8B00];
}
.badgePrimary {
  @apply ml-1 rounded-full bg-app-primary-soft px-1.5 py-0.5 text-[9px] font-bold text-app-primary;
}
.divTrailing {
  @apply ml-auto pb-1.5;
}
.field {
  @apply flex w-[280px] items-center gap-2 rounded bg-[#F4F5F7] px-3 py-1.5;
}
.iconMuted {
  @apply shrink-0 text-[#6B778C];
}
.searchInput {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] text-[#172B4D] outline-none placeholder:text-[#97A0AF];
}
.iconButton {
  @apply rounded p-0.5 text-[#6B778C] hover:bg-[#EBECF0];
}
.rowMt3Gap4 {
  @apply mt-3 flex items-start gap-4;
}
.sidePanel {
  @apply w-52 shrink-0 self-start overflow-hidden rounded-lg border border-[#DFE1E6] bg-white;
}
.rowMuted {
  @apply flex items-center justify-between border-b border-[#EBECF0] bg-[#FAFBFC] px-3 py-2.5;
}
.labelStrong {
  @apply flex items-center gap-1.5 text-[12px] font-semibold text-[#172B4D];
}
.iconPrimary {
  @apply text-app-primary;
}
.addButton {
  @apply rounded p-1 text-app-primary transition-colors hover:bg-[#EBECF0];
}
.divScrollable {
  @apply max-h-[600px] overflow-y-auto py-1;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.buttonWide {
  @apply flex w-full items-center justify-between px-3 py-2 text-[13px] transition-colors;
}
.stateSelectedPrimary {
  @apply bg-app-primary-soft font-semibold text-app-primary;
}
.stateMuted {
  @apply text-[#42526E] hover:bg-[#F4F5F7];
}
.labelGap2 {
  @apply flex items-center gap-2;
}
.badgeRound {
  @apply rounded-full bg-[#EBECF0] px-1.5 text-[10px] font-semibold text-[#5E6C84];
}
.rowGap1Px2Py15 {
  @apply flex items-center gap-1 px-2 py-1.5;
}
.input {
  @apply min-w-0 flex-1 rounded border border-[#4C9AFF] px-2 py-1 text-[12px] outline-none;
}
.confirmButton {
  @apply rounded p-1 text-[#00875A] hover:bg-[#E3FCEF];
}
.cancelButton {
  @apply rounded p-1 text-[#DE350B] hover:bg-[#FFEBE6];
}
.rowPy2Pl15Pr2 {
  @apply flex items-center py-2 pl-1.5 pr-2 text-[13px] transition-colors;
}
.stateSelectedPrimaryAlternative {
  @apply bg-app-primary-soft;
}
.stateMutedMuted {
  @apply hover:bg-[#F4F5F7];
}
.labelP05 {
  @apply shrink-0 p-0.5 text-transparent;
}
.buttonWideGap2 {
  @apply flex min-w-0 flex-1 items-center gap-2 text-left;
}
.labelRound {
  @apply h-2 w-2 shrink-0 rounded-full;
}
.labelWide {
  @apply min-w-0 flex-1 truncate;
}
.stateDefaultAlternative {
  @apply text-[#172B4D];
}
.badgeRoundCompact {
  @apply shrink-0 rounded-full bg-[#EBECF0] px-1.5 text-[10px] font-semibold text-[#5E6C84] group-hover:hidden;
}
.rowGap05 {
  @apply hidden shrink-0 items-center gap-0.5 group-hover:flex;
}
.iconButtonMuted {
  @apply rounded p-1 text-[#6B778C] hover:bg-[#EBECF0];
}
.rowWide {
  @apply min-w-0 flex-1;
}
.categoryButton {
  @apply relative px-4 py-2.5 text-[13px] transition-colors;
}
.divAlternative {
  @apply relative;
}
.buttonGap1Px4Py25 {
  @apply relative flex items-center gap-1 px-4 py-2.5 text-[13px];
}
.card {
  @apply absolute left-0 top-full z-30 mt-1 min-w-[240px] rounded-lg border border-[#DFE1E6] bg-white shadow-xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.divW320pxP4 {
  @apply w-[320px] p-4;
}
.rowMb3 {
  @apply mb-3 flex items-center;
}
.strongLabel {
  @apply text-[13px];
}
.labelCompactMuted {
  @apply ml-auto text-[11px] text-[#6B778C];
}
.description {
  @apply mb-3 text-[11px] text-[#6B778C];
}
.list {
  @apply max-h-[280px] space-y-1 overflow-y-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.categoryField {
  @apply flex cursor-pointer items-center gap-2.5 rounded-lg px-3 py-2 text-[13px];
}
.stateSelectedPrimaryAlternativeAlternative {
  @apply bg-app-primary-soft/50;
}
.stateDefaultFaded {
  @apply cursor-not-allowed opacity-40;
}
.checkbox {
  @apply accent-app-primary;
}
.iconPrimaryTrailing {
  @apply ml-auto text-app-primary;
}
.rowMutedWithTopDivider {
  @apply mt-4 flex justify-end gap-2 border-t border-[#EBECF0] pt-3;
}
.buttonPx3Py15 {
  @apply rounded border border-[#DFE1E6] px-3 py-1.5 text-[12px] text-[#42526E];
}
.primaryButton {
  @apply rounded bg-app-primary px-3 py-1.5 text-[12px] text-white;
}
.categoryButtonWide {
  @apply flex w-full items-center justify-between px-4 py-2.5 text-left text-[13px] hover:bg-[#F4F5F7];
}
.divMuted {
  @apply border-t border-[#EBECF0];
}
.primaryButtonPrimary {
  @apply flex w-full items-center gap-2 px-4 py-2.5 text-[12px] font-medium text-app-primary hover:bg-[#F4F5F7];
}
.section {
  @apply rounded-t-lg border-x border-t border-[#DFE1E6] bg-white;
}
.rowMutedWithBottomDivider {
  @apply flex items-center gap-3 border-b border-[#EBECF0] px-4 py-[9.5px];
}
.primaryButtonPrimaryInverted {
  @apply rounded bg-app-primary px-4 py-[7px] text-[13px] text-white transition-colors hover:bg-app-primary-hover active:scale-[.97];
}
.buttonMuted {
  @apply flex items-center gap-1.5 rounded border border-[#DFE1E6] px-4 py-[7px] text-[13px] text-[#42526E] transition-colors hover:bg-[#F4F5F7];
}
.iconMutedAlternative {
  @apply text-[#6B778C];
}
.primaryButtonPrimaryMuted {
  @apply flex items-center gap-1.5 rounded border px-4 py-[7px] text-[13px] text-app-primary transition-colors hover:bg-[#F4F5F7];
}
.stateSelectedPrimaryAlternativeAlternativeAlternative {
  @apply border-[#4C9AFF] bg-white;
}
.stateDefaultAlternativeAlternative {
  @apply border-[#DFE1E6];
}
.cardFloating {
  @apply absolute left-0 top-full z-30 mt-1 min-w-[180px] rounded-lg border border-[#DFE1E6] bg-white py-1 shadow-lg;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.buttonWideMuted {
  @apply flex h-[51px] w-full items-center gap-2 px-4 py-2.5 text-left text-[13px] text-[#172B4D] hover:bg-[#F4F5F7];
}
.iconSuccess {
  @apply text-[#00875A];
}
.iconWarning {
  @apply text-[#FF8B00];
}
.dangerButton {
  @apply flex h-[51px] w-full items-center gap-2 px-4 py-2.5 text-left text-[13px] text-[#DE350B] hover:bg-[#FFEBE6];
}
.cardFloatingElevated {
  @apply absolute left-0 top-full z-30 mt-1 min-w-[140px] rounded-lg border border-[#DFE1E6] bg-white py-1 shadow-lg;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.buttonWideMutedWFullPx4Py25 {
  @apply w-full px-4 py-2.5 text-left text-[13px] hover:bg-[#F4F5F7];
}
.labelPrimaryMl2 {
  @apply ml-2 text-[12px] font-medium text-app-primary;
}
.labelWideAlternative {
  @apply flex-1;
}
.buttonMutedGap15Px3Py7px {
  @apply flex items-center gap-1.5 rounded border px-3 py-[7px] text-[13px] transition-colors hover:bg-[#F4F5F7];
}
.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternative {
  @apply border-[#4C9AFF] bg-white ring-2 ring-[#4C9AFF]/20;
}
.stateDefaultAlternativeAlternativeAlternative {
  @apply border-[#DFE1E6] text-[#42526E];
}
.cardFloatingElevatedMt1Py1 {
  @apply absolute right-0 top-full z-30 mt-1 rounded-lg border border-[#DFE1E6] bg-white py-1 shadow-lg;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.stateSized {
  @apply w-[160px];
}
.stateSizedW220px {
  @apply w-[220px];
}
.divCompact {
  @apply border-b border-[#EBECF0] px-3 py-2 text-[11px] font-semibold text-[#5E6C84];
}
.divScrollableH240pxPy1 {
  @apply h-[240px] overflow-y-auto py-1;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.brandField {
  @apply flex cursor-pointer items-center gap-2 px-3 py-2 text-[12px] hover:bg-[#F4F5F7];
}
.checkboxPrimary {
  @apply h-4 w-4 accent-app-primary;
}
.labelAlternative {
  @apply font-medium;
}
.labelCompactMutedTrailing {
  @apply ml-auto text-[10px] text-[#97A0AF];
}
.statusButton {
  @apply block h-[37px] w-full px-3 py-2.5 text-left text-[13px] transition-colors hover:bg-[#F4F5F7];
}
.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternativeAlternative {
  @apply bg-app-primary-soft text-app-primary;
}
.rowH445pxPx4 {
  @apply flex h-[44.5px] items-center px-4;
}
.labelMuted {
  @apply text-[12px] text-[#6B778C];
}
.dangerButtonDanger {
  @apply ml-3 flex items-center gap-1 text-[11px] text-[#6B778C] hover:text-[#DE350B];
}
.divTrailingMlAuto {
  @apply relative ml-auto translate-y-px;
}
.buttonMutedGap15Px3Py15 {
  @apply flex items-center gap-1.5 rounded border px-3 py-1.5 text-[12px] font-medium transition-colors hover:bg-[#F4F5F7];
}
.stateSelectedPrimaryAlternativeAlternativeAlternativeAlternativeAlternativeAlternative {
  @apply border-[#4C9AFF] text-app-primary ring-2 ring-[#4C9AFF]/20;
}
.badgePrimaryRound {
  @apply ml-1 rounded-full bg-app-primary px-1.5 py-0.5 text-[9px] font-bold text-white;
}
.cardFloatingElevatedMt1W260px {
  @apply absolute right-0 top-full z-40 mt-1 w-[260px] rounded-lg border border-[#DFE1E6] bg-white shadow-xl;
  animation: productPopoverIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.rowMutedWithBottomDividerPx4Py3 {
  @apply flex items-center justify-between border-b border-[#EBECF0] px-4 py-3;
}
.labelStrongAlternative {
  @apply text-[13px] font-semibold;
}
.primaryButtonPrimaryCompact {
  @apply text-[11px] text-app-primary hover:text-app-primary-hover;
}
.divScrollablePy1 {
  @apply max-h-[320px] overflow-y-auto py-1;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.columnField {
  @apply flex items-center gap-3 px-4 py-2.5 text-[13px];
}
.stateDefaultFadedAlternative {
  @apply cursor-not-allowed opacity-60;
}
.stateMutedMutedAlternative {
  @apply cursor-pointer hover:bg-[#F4F5F7];
}
.checkboxPrimaryH14pxW14px {
  @apply h-[14px] w-[14px] accent-app-primary;
}
.iconMutedAlternativeAlternative {
  @apply text-[#C1C7D0];
}
.stateMutedMutedAlternativeAlternative {
  @apply text-[#97A0AF];
}
.labelCompactMutedAlternative {
  @apply text-[9px] text-[#97A0AF];
}
.rowMutedWithTopDividerPx4Py3 {
  @apply flex items-center justify-between border-t border-[#EBECF0] bg-[#FAFBFC] px-4 py-3;
}
.labelCompactMutedAlternativeAlternative {
  @apply text-[10px] text-[#97A0AF];
}
.primaryButtonPrimaryInvertedPx3Py1 {
  @apply rounded bg-app-primary px-3 py-1 text-[12px] text-white hover:bg-app-primary-hover;
}
.sectionMt4 {
  @apply mt-4;
}
.rowWarning {
  @apply mb-4 flex items-start gap-3 rounded-lg border border-[#FFE380] bg-[#FFFAE6] p-4;
}
.iconWarningMt05 {
  @apply mt-0.5 shrink-0 text-[#FF8B00];
}
.descriptionWarning {
  @apply text-[12px] font-semibold text-[#FF8B00];
}
.descriptionCompact {
  @apply mt-0.5 text-[11px] leading-6;
}
.grid {
  @apply mb-4 grid grid-cols-3 gap-4;
}
.cardCard {
  @apply rounded-lg border border-[#DFE1E6] bg-white p-4;
}
.labelCompactMutedGap2 {
  @apply flex items-center gap-2 text-[11px] text-[#6B778C];
}
.strongLabelMt1 {
  @apply mt-1 block text-[22px];
}
.cardWithBottomDivider {
  @apply rounded-t-lg border border-b-0 border-[#DFE1E6] bg-white px-4 py-3;
}
.fieldW240pxGap2Px3 {
  @apply flex w-[240px] items-center gap-2 rounded border border-[#DFE1E6] bg-white px-3 py-1.5;
}
.searchInputWide {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none placeholder:text-[#97A0AF];
}
.select {
  @apply rounded border border-[#DFE1E6] bg-white px-3 py-[7px] text-[12px] outline-none;
}
.primaryButtonSuccess {
  @apply flex items-center gap-1 rounded bg-[#00875A] px-3 py-1.5 text-[11px] text-white hover:bg-[#006644];
}
.buttonCompact {
  @apply flex items-center gap-1 rounded border border-[#DFE1E6] px-3 py-1.5 text-[11px] text-[#6B778C] hover:bg-[#F4F5F7];
}
.cardScrollable {
  @apply overflow-x-auto rounded-b-lg border border-[#DFE1E6] bg-white;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full min-w-[1250px] border-collapse text-[12px];
}
.tableRow {
  @apply h-[42px] border-b border-[#DFE1E6] bg-[#F4F5F7] text-left text-[11px] font-semibold text-[#5E6C84];
}
.headerCell {
  @apply w-12 text-center;
}
.headerCellRightAligned {
  @apply text-right;
}
.headerCellCentered {
  @apply text-center;
}
.itemRow {
  @apply h-[58px] border-b border-[#F4F5F7] hover:bg-[#FAFBFC];
}
.tableCell {
  @apply font-semibold;
}
.badgeWarningCompact {
  @apply mr-1.5 rounded bg-[#FFFAE6] px-1 py-0.5 text-[8px] text-[#FF8B00];
}
.badgeCompact {
  @apply rounded bg-[#F4F5F7] px-2 py-1 text-[10px];
}
.tableCellRightAligned {
  @apply text-right font-medium;
}
.labelAlternativeAlternative {
  @apply block;
}
.badgeCompactPx2Py1 {
  @apply rounded px-2 py-1 text-[10px] font-medium;
}
.stateWarningWarning {
  @apply bg-[#FFFAE6] text-[#FF8B00];
}
.stateSuccess {
  @apply bg-[#E3FCEF] text-[#00875A];
}
.stateMutedMutedAlternativeAlternativeAlternative {
  @apply bg-[#F4F5F7] text-[#6B778C];
}
.rowGap3 {
  @apply flex justify-center gap-3;
}
.primaryButtonPrimaryMutedAlternative {
  @apply text-[#6B778C] hover:text-app-primary;
}
.dangerButtonDangerMuted {
  @apply text-[#6B778C] hover:text-[#DE350B];
}
.primaryButtonFloating {
  @apply fixed bottom-6 right-6 z-[60] flex h-[40.5px] items-center gap-2 rounded-full bg-gradient-to-r py-3 pl-4 pr-5 font-medium text-white shadow-[0_8px_24px_rgba(20,184,166,.45)] transition-transform hover:scale-[1.04] active:scale-[.98];
}
.badgeDanger {
  @apply absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full bg-[#FF5630] text-[10px];
}
.restoreSpan {
  @apply absolute -left-1.5 -top-1.5 flex h-5 w-5 items-center justify-center rounded-full bg-white text-[#0E9488] opacity-0 shadow-md transition-opacity group-hover:opacity-100;
}
</style>
