<script setup lang="ts">
import { FilePlus2, PackagePlus } from 'lucide-vue-next';
import { computed, reactive, ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  SalesContractCreatePayload,
  SalesContractCreateStatus,
} from '@/features/sales/model/sales-contracts.types';

const props = defineProps<{
  open: boolean;
  nextCode: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [payload: SalesContractCreatePayload];
}>();

const categoryOptions: UiSelectOption[] = [
  { value: '销售合同', label: '销售合同' },
  { value: '服务合同', label: '服务合同' },
  { value: '框架协议', label: '框架协议' },
  { value: '技术合同', label: '技术合同' },
  { value: '采购合同', label: '采购合同' },
];
const customerOptions: UiSelectOption[] = [
  { value: '', label: '请选择或新建客户...' },
  { value: '国电南瑞', label: '国电南瑞' },
  { value: '上海智远工业', label: '上海智远工业' },
  { value: '杭州临安智控设备', label: '杭州临安智控设备' },
];
const paymentOptions: UiSelectOption[] = [
  { value: '预付30%+发货30%+验收40%', label: '预付30%+发货30%+验收40%' },
  { value: '预付50%+验收50%', label: '预付50%+验收50%' },
  { value: '预付100%', label: '预付100%' },
  { value: '货到付款', label: '货到付款' },
  { value: '月结30天', label: '月结30天' },
];

const draft = reactive({
  title: '',
  category: '销售合同',
  customer: '',
  amount: '',
  signDate: '2026-08-16',
  paymentTerms: paymentOptions[0]?.value ?? '',
  deliveryDays: '21',
  warrantyMonths: '12',
  note: '',
  limitOrderAmount: false,
  allowAffiliates: false,
  allowOffCatalog: false,
  requireApproval: false,
});
const previewVisible = ref(false);
const validationVisible = ref(false);
const formattedAmount = computed(() => {
  const value = Number(draft.amount.replace(/[^\d.]/g, ''));
  return Number.isFinite(value) && value > 0 ? `¥${new Intl.NumberFormat('zh-CN').format(value)}` : '¥0';
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      title: '',
      category: '销售合同',
      customer: '',
      amount: '',
      signDate: '2026-08-16',
      paymentTerms: paymentOptions[0]?.value ?? '',
      deliveryDays: '21',
      warrantyMonths: '12',
      note: '',
      limitOrderAmount: false,
      allowAffiliates: false,
      allowOffCatalog: false,
      requireApproval: false,
    });
    previewVisible.value = false;
    validationVisible.value = false;
  },
);

function save(status: SalesContractCreateStatus) {
  validationVisible.value = true;
  if (!draft.title.trim() || !draft.customer || !draft.amount.trim()) {
    toast.warning('请填写合同标题、客户和合同金额');
    return;
  }
  emit('save', { ...draft, title: draft.title.trim(), amount: draft.amount.trim(), note: draft.note.trim(), status });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建销售合同"
    description="合同基本信息、订货约束、收入规则与商务条款"
    size="xl"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon">
        <FilePlus2
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>

    <form
      :class="$style.editor"
      @submit.prevent="save('草稿')"
    >
      <section :class="$style.formSection">
        <h3>合同基本信息</h3>
        <div :class="$style.formGrid">
          <UiFormField
            label="合同标题"
            required
            wide
            :error="validationVisible && !draft.title.trim() ? '请输入合同标题' : undefined"
          >
            <UiInput
              v-model="draft.title"
              placeholder="例：华能电力DCS控制系统采购合同"
              :invalid="validationVisible && !draft.title.trim()"
            />
          </UiFormField>
          <UiFormField
            label="合同分类"
            required
          >
            <UiSelect
              v-model="draft.category"
              :options="categoryOptions"
              label="合同分类"
            />
          </UiFormField>
          <UiFormField label="合同编号">
            <UiInput
              :model-value="nextCode"
              disabled
            />
          </UiFormField>
          <UiFormField label="签订日期">
            <UiInput
              v-model="draft.signDate"
              type="date"
            />
          </UiFormField>
          <UiFormField
            label="客户"
            required
            :error="validationVisible && !draft.customer ? '请选择客户' : undefined"
          >
            <UiSelect
              v-model="draft.customer"
              :options="customerOptions"
              label="客户"
            />
          </UiFormField>
          <UiFormField
            label="含税总额"
            required
            :error="validationVisible && !draft.amount.trim() ? '请输入合同金额' : undefined"
          >
            <UiInput
              v-model="draft.amount"
              placeholder="例如：580000"
              :invalid="validationVisible && !draft.amount.trim()"
            />
          </UiFormField>
          <UiFormField
            label="备注"
            wide
          >
            <UiTextarea
              v-model="draft.note"
              rows="3"
              placeholder="补充合同背景或执行说明"
            />
          </UiFormField>
        </div>
      </section>

      <section :class="$style.formSection">
        <div :class="$style.sectionHeading">
          <div>
            <h3>物料明细</h3>
            <p>0 项</p>
          </div>
          <UiButton
            variant="secondary"
            size="sm"
            @click="toast.info('本地物料目录已打开，可在合同保存后继续维护')"
          >
            <template #leading>
              <PackagePlus
                :size="14"
                :stroke-width="1.8"
              />
            </template>
            从目录选择
          </UiButton>
        </div>
        <div :class="$style.materialEmpty">暂未添加物料</div>
      </section>

      <section :class="$style.formSection">
        <h3>订货约束与授权</h3>
        <div :class="$style.switchList">
          <label>
            <span><strong>限制合同总下单金额</strong><small>累计含税金额不得超过合同上限</small></span>
            <UiSwitch
              v-model="draft.limitOrderAmount"
              size="sm"
              label="限制合同总下单金额"
            />
          </label>
          <label>
            <span><strong>分/子公司或关联公司受用</strong><small>允许授权关联主体引用本合同下单</small></span>
            <UiSwitch
              v-model="draft.allowAffiliates"
              size="sm"
              label="关联公司受用"
            />
          </label>
          <label>
            <span><strong>容许添加协议外商品下单</strong><small>协议外商品订单将进入审批流程</small></span>
            <UiSwitch
              v-model="draft.allowOffCatalog"
              size="sm"
              label="容许协议外商品"
            />
          </label>
          <label>
            <span><strong>框架和额度内下单需要审批</strong><small>额度与清单范围内仍需审批</small></span>
            <UiSwitch
              v-model="draft.requireApproval"
              size="sm"
              label="额度内下单审批"
            />
          </label>
        </div>
      </section>

      <section :class="$style.formSection">
        <h3>合同条款</h3>
        <div :class="$style.formGrid">
          <UiFormField
            label="付款条件"
            wide
          >
            <UiSelect
              v-model="draft.paymentTerms"
              :options="paymentOptions"
              label="付款条件"
            />
          </UiFormField>
          <UiFormField label="交货周期（天）">
            <UiInput
              v-model="draft.deliveryDays"
              type="number"
            />
          </UiFormField>
          <UiFormField label="质保期（月）">
            <UiInput
              v-model="draft.warrantyMonths"
              type="number"
            />
          </UiFormField>
        </div>
      </section>

      <section
        v-if="previewVisible"
        :class="$style.preview"
      >
        <h3>合同概要</h3>
        <dl>
          <div>
            <dt>合同编号</dt>
            <dd>{{ nextCode }}</dd>
          </div>
          <div>
            <dt>合同分类</dt>
            <dd>{{ draft.category }}</dd>
          </div>
          <div>
            <dt>客户</dt>
            <dd>{{ draft.customer || '-' }}</dd>
          </div>
          <div>
            <dt>含税总额</dt>
            <dd>{{ formattedAmount }}</dd>
          </div>
        </dl>
      </section>
    </form>

    <template #footer>
      <UiButton
        variant="secondary"
        @click="previewVisible = !previewVisible"
        >{{ previewVisible ? '收起预览' : '预览合同' }}</UiButton
      >
      <UiButton
        variant="secondary"
        @click="save('草稿')"
        >保存草稿</UiButton
      >
      <UiButton
        variant="primary"
        @click="save('待审批')"
        >提交合同</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.editor {
  @apply space-y-4 p-5;
}

.formSection {
  @apply rounded border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.formSection h3,
.preview h3 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.formGrid {
  @apply mt-4 grid grid-cols-2 gap-4;
}

.sectionHeading {
  @apply flex items-center justify-between gap-3;
}

.sectionHeading p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.materialEmpty {
  @apply mt-4 flex min-h-20 items-center justify-center rounded border border-dashed text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.switchList {
  @apply mt-3 grid grid-cols-2 gap-3;
}

.switchList > label {
  @apply flex min-h-16 items-center justify-between gap-4 rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.switchList strong {
  @apply block text-[12px] font-semibold;
  color: var(--ui-text);
}

.switchList small {
  @apply mt-1 block text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.preview {
  @apply rounded border p-4;
  color: var(--app-primary);
  border-color: var(--app-primary-border);
  background: var(--app-primary-soft);
}

.preview dl {
  @apply mt-3 grid grid-cols-4 gap-3;
}

.preview dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.preview dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}
</style>
