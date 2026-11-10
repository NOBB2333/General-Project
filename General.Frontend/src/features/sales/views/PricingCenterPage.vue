<script setup lang="ts">
import { AlertCircle, CircleDollarSign, Layers3, Link2 } from 'lucide-vue-next';
import { computed, reactive, ref } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  pricingBusinessRecords,
  pricingBusinessUnits,
  pricingCenterReferenceFixture,
  pricingGroups,
} from '../model/pricing-center.data';
import type { PriceDraft, PriceProduct, PricingBusinessRecord, PricingTabId } from '../model/pricing-center.types';
import PricingBusinessPanel from './pricing-center/PricingBusinessPanel.vue';
import PricingCatalog from './pricing-center/PricingCatalog.vue';
import PricingCenterHero from './pricing-center/PricingCenterHero.vue';
import PricingCenterTabs from './pricing-center/PricingCenterTabs.vue';

type SecondaryTab = Exclude<PricingTabId, 'pricing'>;
type BatchMode = 'adjust' | 'relation' | 'assign';

const rows = ref<PriceProduct[]>(pricingCenterReferenceFixture.rows.map((row) => ({ ...row })));
const activeTab = ref<PricingTabId>('pricing');
const activeGroup = ref('all');
const query = ref('');
const businessUnit = ref('');
const selectedIds = ref<string[]>([]);
const editingProduct = ref<PriceProduct | null>(null);
const editOpen = ref(false);
const batchOpen = ref(false);
const batchMode = ref<BatchMode>('adjust');
const createOpen = ref(false);
const createTab = ref<SecondaryTab>('pricegroup');

const priceDraft = reactive<PriceDraft>({ costPrice: '', minPrice: '', suggestedPrice: '', listPrice: '', reason: '' });
const batchDraft = reactive({ percentage: '5', relationMargin: '20', priceGroup: '项目专属价', reason: '' });
const createDraft = reactive({ title: '', subtitle: '', amount: '' });

const priceGroupOptions: readonly UiSelectOption[] = [
  { value: '项目专属价', label: '项目专属价' },
  { value: '小型PLC', label: '小型PLC' },
  { value: '通用变频器', label: '通用变频器' },
  { value: '触摸屏/HMI', label: '触摸屏/HMI' },
  { value: '系统集成价', label: '系统集成价' },
];

const businessRows = reactive<Record<SecondaryTab, PricingBusinessRecord[]>>({
  pricegroup: pricingBusinessRecords.pricegroup.map((row) => ({ ...row })),
  agreement: pricingBusinessRecords.agreement.map((row) => ({ ...row })),
  special: pricingBusinessRecords.special.map((row) => ({ ...row })),
  alert: pricingBusinessRecords.alert.map((row) => ({ ...row })),
  adjustment: pricingBusinessRecords.adjustment.map((row) => ({ ...row })),
  history: pricingBusinessRecords.history.map((row) => ({ ...row })),
});

const filteredRows = computed(() => {
  const group = pricingGroups.find((item) => item.id === activeGroup.value);
  const needle = query.value.trim().toLocaleLowerCase();
  return rows.value.filter((row) => {
    const matchesGroup =
      !group?.match ||
      [row.materialCode, row.name, row.spec, row.priceGroupName].some((value) => value.includes(group.match ?? ''));
    const matchesBu = !businessUnit.value || row.buId === businessUnit.value;
    const matchesQuery =
      !needle ||
      [row.materialCode, row.name, row.spec, row.brand].some((value) => value.toLocaleLowerCase().includes(needle));
    return matchesGroup && matchesBu && matchesQuery;
  });
});

const dialogCopy = computed(() => {
  if (batchMode.value === 'relation')
    return { title: '批量配置价格关系', description: '按成本价与目标毛利率重新计算建议售价。' };
  if (batchMode.value === 'assign') return { title: '分配价格组', description: '为已选择的商品统一分配价格组。' };
  return { title: '批量调价', description: '按百分比调整已选择商品的建议售价。' };
});

const createCopy: Record<SecondaryTab, string> = {
  pricegroup: '新建价格组',
  agreement: '新建框架协议价',
  special: '新建特价申请',
  alert: '配置价格预警',
  adjustment: '新建调价单',
  history: '导出价格历史',
};

function updateSelection(id: string) {
  selectedIds.value = selectedIds.value.includes(id)
    ? selectedIds.value.filter((item) => item !== id)
    : [...selectedIds.value, id];
}

function toggleAll() {
  const visibleIds = filteredRows.value.map((row) => row.id);
  const allSelected = visibleIds.length > 0 && visibleIds.every((id) => selectedIds.value.includes(id));
  selectedIds.value = allSelected
    ? selectedIds.value.filter((id) => !visibleIds.includes(id))
    : [...new Set([...selectedIds.value, ...visibleIds])];
}

function selectCurrentGroup() {
  selectedIds.value = [...new Set([...selectedIds.value, ...filteredRows.value.map((row) => row.id)])];
  toast.success(`已选择当前分组的 ${filteredRows.value.length} 件商品`);
}

function openEdit(row: PriceProduct) {
  editingProduct.value = row;
  priceDraft.costPrice = String(row.costPrice);
  priceDraft.minPrice = String(row.minPrice);
  priceDraft.suggestedPrice = String(row.suggestedPrice);
  priceDraft.listPrice = String(row.listPrice);
  priceDraft.reason = '';
  editOpen.value = true;
}

function saveProduct() {
  if (!editingProduct.value) return;
  const target = rows.value.find((row) => row.id === editingProduct.value?.id);
  if (!target) return;
  target.costPrice = Number(priceDraft.costPrice) || target.costPrice;
  target.minPrice = Number(priceDraft.minPrice) || target.minPrice;
  target.suggestedPrice = Number(priceDraft.suggestedPrice) || target.suggestedPrice;
  target.listPrice = Number(priceDraft.listPrice) || target.listPrice;
  target.updatedAt = '2026-09-16';
  target.updatedBy = '张经理';
  editOpen.value = false;
  toast.success(`${target.materialCode} 的价格已更新`);
}

function openBatch(mode: BatchMode) {
  if (!selectedIds.value.length) {
    toast.warning('请先选择需要处理的商品');
    return;
  }
  batchMode.value = mode;
  batchOpen.value = true;
}

function applyBatch() {
  const selected = rows.value.filter((row) => selectedIds.value.includes(row.id));
  if (batchMode.value === 'adjust') {
    const rate = Number(batchDraft.percentage) / 100;
    selected.forEach((row) => {
      row.suggestedPrice = Math.round(row.suggestedPrice * (1 + rate));
    });
  } else if (batchMode.value === 'relation') {
    const margin = Number(batchDraft.relationMargin) / 100;
    selected.forEach((row) => {
      row.suggestedPrice = Math.round(row.costPrice * (1 + margin));
    });
  } else {
    selected.forEach((row) => {
      row.priceGroupName = batchDraft.priceGroup;
    });
  }
  batchOpen.value = false;
  toast.success(`已更新 ${selected.length} 件商品`);
}

function openCreate(tab: SecondaryTab) {
  if (tab === 'history') {
    exportBusinessHistory();
    return;
  }
  createTab.value = tab;
  createDraft.title = '';
  createDraft.subtitle = '';
  createDraft.amount = '';
  createOpen.value = true;
}

function saveBusinessRecord() {
  const sequence = businessRows[createTab.value].length + 1;
  businessRows[createTab.value].unshift({
    id: `LOCAL-${createTab.value}-${sequence}`,
    code: `LOCAL-${String(sequence).padStart(3, '0')}`,
    title: createDraft.title || '本地新增记录',
    subtitle: createDraft.subtitle || '当前本地复刻会话创建',
    amount: createDraft.amount || '待配置',
    status: createTab.value === 'alert' ? '低风险' : '草稿',
    updatedAt: '2026-09-16',
  });
  createOpen.value = false;
  toast.success(`${createCopy[createTab.value]}已保存到本地`);
}

function exportRows() {
  downloadCsv(
    '商品定价.csv',
    ['物料编码', '商品名称', '规格型号', 'BU', '价格组', '成本价', '最低售价', '建议售价', '目录价'],
    filteredRows.value.map((row) => [
      row.materialCode,
      row.name,
      row.spec,
      row.buName,
      row.priceGroupName,
      row.costPrice,
      row.minPrice,
      row.suggestedPrice,
      row.listPrice,
    ]),
  );
}

function exportBusinessHistory() {
  downloadCsv(
    '价格历史.csv',
    ['编号', '商品', '业务说明', '价格', '状态', '日期'],
    businessRows.history.map((row) => [row.code, row.title, row.subtitle, row.amount, row.status, row.updatedAt]),
  );
}

function downloadCsv(filename: string, header: readonly string[], body: readonly (readonly (string | number)[])[]) {
  const escape = (value: string | number) => `"${String(value).replaceAll('"', '""')}"`;
  const csv = [header, ...body].map((line) => line.map(escape).join(',')).join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-route="/sales/pricing-center"
    data-pricing-center-page
    :data-active-tab="activeTab"
    :data-result-count="activeTab === 'pricing' ? filteredRows.length : businessRows[activeTab].length"
    :data-selected-count="selectedIds.length"
  >
    <PricingCenterHero />
    <PricingCenterTabs v-model="activeTab" />

    <div
      v-if="activeTab === 'pricing'"
      :class="$style.draftNotice"
    >
      <span>1</span>
      <p>您有 <strong>1 个待提交的调价草稿</strong>，请前往「价格调整记录」tab 查看并提交审批</p>
      <button
        type="button"
        @click="activeTab = 'adjustment'"
      >
        立即查看
      </button>
    </div>

    <PricingCatalog
      v-if="activeTab === 'pricing'"
      :rows="filteredRows"
      :groups="pricingGroups"
      :active-group="activeGroup"
      :query="query"
      :business-unit="businessUnit"
      :business-unit-options="pricingBusinessUnits"
      :selected-ids="selectedIds"
      @update:active-group="activeGroup = $event"
      @update:query="query = $event"
      @update:business-unit="businessUnit = $event"
      @toggle:row="updateSelection"
      @toggle:all="toggleAll"
      @select:group="selectCurrentGroup"
      @edit="openEdit"
      @bulk-adjust="openBatch('adjust')"
      @configure="openBatch('relation')"
      @assign-group="openBatch('assign')"
      @import="toast.info('批量导入仅在本地演示：请选择 CSV 模板后导入')"
      @export="exportRows"
    />

    <PricingBusinessPanel
      v-else
      :tab="activeTab"
      :rows="businessRows[activeTab]"
      @create="openCreate"
    />
  </main>

  <UiDialog
    v-model:open="editOpen"
    :title="editingProduct ? `编辑价格 · ${editingProduct.materialCode}` : '编辑价格'"
    :description="editingProduct?.name"
    size="lg"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="saveProduct"
    >
      <UiFormField label="成本价"
        ><UiInput
          v-model="priceDraft.costPrice"
          type="number"
      /></UiFormField>
      <UiFormField label="最低售价"
        ><UiInput
          v-model="priceDraft.minPrice"
          type="number"
      /></UiFormField>
      <UiFormField label="建议售价"
        ><UiInput
          v-model="priceDraft.suggestedPrice"
          type="number"
      /></UiFormField>
      <UiFormField label="目录价"
        ><UiInput
          v-model="priceDraft.listPrice"
          type="number"
      /></UiFormField>
      <UiFormField
        label="调整原因"
        wide
        ><UiTextarea
          v-model="priceDraft.reason"
          rows="3"
          placeholder="请输入本次价格调整原因"
      /></UiFormField>
    </form>
    <template #footer>
      <UiButton @click="editOpen = false">取消</UiButton>
      <UiButton
        variant="primary"
        @click="saveProduct"
        >保存价格</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="batchOpen"
    :title="dialogCopy.title"
    :description="`${dialogCopy.description} 当前已选择 ${selectedIds.length} 件商品。`"
  >
    <div :class="$style.batchBody">
      <UiFormField
        v-if="batchMode === 'adjust'"
        label="建议售价调整百分比"
        hint="正数为上调，负数为下调"
        ><UiInput
          v-model="batchDraft.percentage"
          type="number"
      /></UiFormField>
      <UiFormField
        v-else-if="batchMode === 'relation'"
        label="目标成本毛利率（%）"
        ><UiInput
          v-model="batchDraft.relationMargin"
          type="number"
      /></UiFormField>
      <UiFormField
        v-else
        label="价格组"
        ><UiSelect
          v-model="batchDraft.priceGroup"
          label="价格组"
          :options="priceGroupOptions"
      /></UiFormField>
      <UiFormField label="操作说明"
        ><UiTextarea
          v-model="batchDraft.reason"
          rows="3"
          placeholder="请输入批量操作说明"
      /></UiFormField>
    </div>
    <template #icon>
      <span :class="$style.dialogIcon">
        <Layers3
          v-if="batchMode === 'assign'"
          :size="18"
          :stroke-width="1.8"
        />
        <Link2
          v-else-if="batchMode === 'relation'"
          :size="18"
          :stroke-width="1.8"
        />
        <CircleDollarSign
          v-else
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #footer>
      <UiButton @click="batchOpen = false">取消</UiButton>
      <UiButton
        variant="primary"
        @click="applyBatch"
        >确认更新</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    :title="createCopy[createTab]"
    description="记录仅保存在当前本地复刻会话中。"
  >
    <div :class="$style.batchBody">
      <UiFormField
        label="名称"
        required
        ><UiInput
          v-model="createDraft.title"
          placeholder="请输入名称"
      /></UiFormField>
      <UiFormField label="业务说明"
        ><UiTextarea
          v-model="createDraft.subtitle"
          rows="3"
          placeholder="请输入业务说明"
      /></UiFormField>
      <UiFormField label="价格 / 指标"
        ><UiInput
          v-model="createDraft.amount"
          placeholder="例如：¥120,000"
      /></UiFormField>
    </div>
    <template #icon
      ><span :class="$style.dialogIcon"
        ><AlertCircle
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <template #footer>
      <UiButton @click="createOpen = false">取消</UiButton>
      <UiButton
        variant="primary"
        @click="saveBusinessRecord"
        >保存</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto w-full max-w-[1400px] pb-10;
}
.draftNotice {
  @apply my-4 flex min-h-[46px] items-center gap-2 rounded border px-4 text-[12px];
  color: #974f0c;
  border-color: #ffe380;
  background: #fffae6;
}
.draftNotice > span {
  @apply flex h-5 w-5 shrink-0 items-center justify-center rounded-full font-semibold;
  color: #b65c02;
  background: #fff0b3;
}
.draftNotice p {
  @apply min-w-0 flex-1;
}
.draftNotice button {
  @apply shrink-0 font-semibold hover:underline;
  color: #b65c02;
}
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.batchBody {
  @apply grid gap-4 p-5;
}
.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
@media (max-width: 1100px) {
  .page {
    @apply pb-6;
  }
}
</style>
