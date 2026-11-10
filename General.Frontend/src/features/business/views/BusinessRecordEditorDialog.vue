<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import {
  createBusinessAcceptanceApi,
  createBusinessBudgetExecutionApi,
  createBusinessChainApi,
  createBusinessContractApi,
  createBusinessInvoiceApi,
  createBusinessProcurementApi,
  createBusinessReceivableApi,
  updateBusinessAcceptanceApi,
  updateBusinessBudgetExecutionApi,
  updateBusinessChainApi,
  updateBusinessContractApi,
  updateBusinessInvoiceApi,
  updateBusinessProcurementApi,
  updateBusinessReceivableApi,
} from '@/api/core/business';
import type { BusinessApi } from '@/api/core/business';
import type { BusinessLegalEntityApi } from '@/api/core/business-legal-entities';
import type { UiSelectOption } from '@/components/ui';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import {
  buildBusinessMutationInput,
  createBusinessRecordForm,
  type BusinessRecordForm,
  type BusinessRecordKind,
  type BusinessRecordTarget,
} from '../model/business-records.model';

const props = defineProps<{
  contracts: BusinessApi.ContractItem[];
  currentUserId: string;
  currentUserName: string;
  invoices: BusinessApi.InvoiceItem[];
  legalEntities: BusinessLegalEntityApi.Item[];
  kind: BusinessRecordKind;
  open: boolean;
  projectId: string;
  receivables: BusinessApi.ReceivableItem[];
  record: BusinessRecordTarget | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<BusinessRecordForm>(createBusinessRecordForm(props.kind, props.currentUserId));
const saving = ref(false);
const error = ref('');
const isEditing = computed(() => Boolean(props.record && props.record.kind === props.kind));
const title = computed(() => `${isEditing.value ? '编辑' : '新增'}${kindLabel(props.kind)}`);
const recordId = computed(() => (props.record?.kind === props.kind ? props.record.item.id : undefined));
const ownerName = computed(() =>
  props.record?.kind === 'chain' ? props.record.item.ownerName : props.currentUserName || '当前用户',
);

const contractOptions = computed<UiSelectOption[]>(() => {
  const options: UiSelectOption[] = [{ value: '', label: '不关联合同' }];
  for (const contract of props.contracts) {
    options.push({ value: contract.contractCode, label: `${contract.contractCode} · ${contract.title}` });
  }
  return options;
});

const invoiceOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不关联发票' },
  ...props.invoices.map((invoice) => ({
    value: invoice.invoiceCode,
    label: `${invoice.invoiceCode} · ${invoice.title}`,
  })),
]);

const receivableOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不关联应收' },
  ...props.receivables.map((receivable) => ({
    value: receivable.receivableCode,
    label: `${receivable.receivableCode} · ${receivable.title}`,
  })),
]);

const parentContractOptions = computed<UiSelectOption[]>(() => {
  const currentId = props.record?.kind === 'contract' ? props.record.item.id : '';
  return [
    { value: '', label: '无上级合同' },
    ...props.contracts
      .filter((contract) => contract.id !== currentId)
      .map((contract) => ({ value: contract.contractCode, label: `${contract.contractCode} · ${contract.title}` })),
  ];
});

const legalEntityOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: props.record ? '未关联主体' : '使用租户默认主体' },
  ...props.legalEntities.map((item) => ({
    value: item.id,
    label: `${item.shortName || item.name}${item.isDefault ? '（默认）' : ''}`,
  })),
]);

const contractTypeOptions: UiSelectOption[] = [
  { value: 'true', label: '收入合同' },
  { value: 'false', label: '成本合同' },
];

watch(
  () => [props.open, props.kind, props.record] as const,
  ([open]) => {
    if (!open) return;
    Object.assign(form, createBusinessRecordForm(props.kind, props.currentUserId, props.record ?? undefined));
    applyDefaultLegalEntity();
    error.value = '';
  },
  { immediate: true },
);

watch(() => props.legalEntities, applyDefaultLegalEntity);

function applyDefaultLegalEntity() {
  if (!props.open || props.record || form.legalEntityId) return;
  if (props.kind !== 'contract' && props.kind !== 'invoice') return;
  form.legalEntityId = props.legalEntities.find((item) => item.isDefault)?.id ?? '';
}

function kindLabel(kind: BusinessRecordKind): string {
  switch (kind) {
    case 'acceptance':
      return '验收记录';
    case 'budget':
      return '预算执行';
    case 'chain':
      return '经营链事项';
    case 'contract':
      return '合同';
    case 'invoice':
      return '开票记录';
    case 'procurement':
      return '采购事项';
    case 'receivable':
      return '应收记录';
  }
}

function updateOpen(open: boolean) {
  if (saving.value) return;
  emit('update:open', open);
}

async function save() {
  saving.value = true;
  error.value = '';
  try {
    const request = buildBusinessMutationInput(props.kind, form, props.currentUserId);
    switch (request.kind) {
      case 'acceptance':
        if (recordId.value) await updateBusinessAcceptanceApi(props.projectId, recordId.value, request.input);
        else await createBusinessAcceptanceApi(props.projectId, request.input);
        break;
      case 'budget':
        if (recordId.value) await updateBusinessBudgetExecutionApi(props.projectId, recordId.value, request.input);
        else await createBusinessBudgetExecutionApi(props.projectId, request.input);
        break;
      case 'chain':
        if (recordId.value) await updateBusinessChainApi(props.projectId, recordId.value, request.input);
        else await createBusinessChainApi(props.projectId, request.input);
        break;
      case 'contract':
        if (recordId.value) await updateBusinessContractApi(props.projectId, recordId.value, request.input);
        else await createBusinessContractApi(props.projectId, request.input);
        break;
      case 'invoice':
        if (recordId.value) await updateBusinessInvoiceApi(props.projectId, recordId.value, request.input);
        else await createBusinessInvoiceApi(props.projectId, request.input);
        break;
      case 'procurement':
        if (recordId.value) await updateBusinessProcurementApi(props.projectId, recordId.value, request.input);
        else await createBusinessProcurementApi(props.projectId, request.input);
        break;
      case 'receivable':
        if (recordId.value) await updateBusinessReceivableApi(props.projectId, recordId.value, request.input);
        else await createBusinessReceivableApi(props.projectId, request.input);
        break;
    }

    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '经营记录保存失败，请重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="title"
    description="保存后重新读取当前项目经营详情；必填项和关联范围由服务端再次校验。"
    size="lg"
    @update:open="updateOpen"
  >
    <form @submit.prevent="save">
      <template v-if="kind === 'budget'">
        <UiFormField
          label="预算编号"
          required
        >
          <UiInput
            v-model="form.budgetCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="预算类别"
          required
        >
          <UiInput
            v-model="form.category"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="调整后预算"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.adjustedAmount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="已执行预算"
          required
        >
          <UiInput
            v-model="form.executedAmount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="排序值"
          required
        >
          <UiInput
            v-model="form.sortOrder"
            :disabled="saving"
            step="1"
            type="number"
            required
          />
        </UiFormField>
        <p>预算差异额由服务端按调整后预算减已执行预算计算，超预算时允许为负。</p>
      </template>

      <template v-else-if="kind === 'chain'">
        <UiFormField
          label="经营链编号"
          required
        >
          <UiInput
            v-model="form.chainCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="事项名称"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="事项类型"
          required
        >
          <UiInput
            v-model="form.type"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="阶段"
          required
        >
          <UiInput
            v-model="form.stage"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="负责人"
          hint="新记录默认使用当前登录用户；编辑时保留原负责人。"
        >
          <UiInput
            :model-value="ownerName"
            disabled
          />
        </UiFormField>
        <UiFormField label="关联合同">
          <UiSelect
            v-model="form.linkedContractCode"
            label="关联合同"
            :options="contractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="来源变更编号"
          hint="可留空；填写时服务端会检查其属于当前项目。"
        >
          <UiInput
            v-model="form.sourceChangeCode"
            :disabled="saving"
            maxlength="64"
          />
        </UiFormField>
        <UiFormField label="事项摘要">
          <UiTextarea
            v-model="form.summary"
            :disabled="saving"
            maxlength="512"
            rows="3"
          />
        </UiFormField>
      </template>

      <template v-else-if="kind === 'contract'">
        <UiFormField
          label="合同编号"
          required
        >
          <UiInput
            v-model="form.contractCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="合同名称"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="合同类型"
          required
        >
          <UiInput
            v-model="form.type"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="合同方"
          required
        >
          <UiInput
            v-model="form.counterpartyName"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          v-if="legalEntities.length"
          label="公司主体"
          hint="新建时默认采用租户默认主体；保存后关联主体和名称快照。"
        >
          <UiSelect
            v-model="form.legalEntityId"
            label="公司主体"
            :options="legalEntityOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="合同金额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.amount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="合同性质"
          required
        >
          <UiSelect
            v-model="form.isRevenueContract"
            label="合同性质"
            :options="contractTypeOptions"
            :disabled="saving"
            required
          />
        </UiFormField>
        <UiFormField
          label="签约日期"
          hint="留空时由服务端按当天记录。"
        >
          <UiInput
            v-model="form.signDate"
            :disabled="saving"
            type="date"
          />
        </UiFormField>
        <UiFormField
          label="合同状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField label="上级合同">
          <UiSelect
            v-model="form.parentContractCode"
            label="上级合同"
            :options="parentContractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="来源变更编号"
          hint="可留空；填写时服务端会检查其属于当前项目。"
        >
          <UiInput
            v-model="form.sourceChangeCode"
            :disabled="saving"
            maxlength="64"
          />
        </UiFormField>
      </template>

      <template v-else-if="kind === 'procurement'">
        <UiFormField
          label="采购编号"
          required
        >
          <UiInput
            v-model="form.procurementCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="采购事项"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="供应商"
          required
        >
          <UiInput
            v-model="form.supplierName"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="采购金额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.amount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="采购阶段"
          required
        >
          <UiInput
            v-model="form.stage"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="采购状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="签约日期"
          hint="留空时由服务端按当天记录。"
        >
          <UiInput
            v-model="form.signDate"
            :disabled="saving"
            type="date"
          />
        </UiFormField>
        <UiFormField label="关联合同">
          <UiSelect
            v-model="form.linkedContractCode"
            label="关联合同"
            :options="contractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="来源变更编号"
          hint="可留空；填写时服务端会检查其属于当前项目。"
        >
          <UiInput
            v-model="form.sourceChangeCode"
            :disabled="saving"
            maxlength="64"
          />
        </UiFormField>
      </template>

      <template v-else-if="kind === 'invoice'">
        <UiFormField
          label="发票编号"
          required
        >
          <UiInput
            v-model="form.invoiceCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="发票事项"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          v-if="legalEntities.length"
          label="公司主体"
          hint="新建时默认采用租户默认主体；保存后关联主体和名称快照。"
        >
          <UiSelect
            v-model="form.legalEntityId"
            label="公司主体"
            :options="legalEntityOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="发票金额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.amount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="税额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.taxAmount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="开票日期"
          hint="留空时由服务端按当天记录。"
        >
          <UiInput
            v-model="form.invoiceDate"
            :disabled="saving"
            type="date"
          />
        </UiFormField>
        <UiFormField
          label="发票状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField label="关联合同">
          <UiSelect
            v-model="form.linkedContractCode"
            label="关联合同"
            :options="contractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="关联应收">
          <UiSelect
            v-model="form.receivableCode"
            label="关联应收"
            :options="receivableOptions"
            :disabled="saving"
          />
        </UiFormField>
      </template>

      <template v-else-if="kind === 'receivable'">
        <UiFormField
          label="应收编号"
          required
        >
          <UiInput
            v-model="form.receivableCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="应收事项"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="应收金额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.totalAmount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="已回金额"
          required
          hint="金额按输入数值传给服务端，不做前端舍入。"
        >
          <UiInput
            v-model="form.receivedAmount"
            :disabled="saving"
            min="0"
            step="any"
            type="number"
            required
          />
        </UiFormField>
        <UiFormField
          label="计划回款日期"
          hint="留空时由服务端按当天记录。"
        >
          <UiInput
            v-model="form.plannedDate"
            :disabled="saving"
            type="date"
          />
        </UiFormField>
        <UiFormField
          label="应收状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField label="关联合同">
          <UiSelect
            v-model="form.linkedContractCode"
            label="关联合同"
            :options="contractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="关联发票">
          <UiSelect
            v-model="form.invoiceCode"
            label="关联发票"
            :options="invoiceOptions"
            :disabled="saving"
          />
        </UiFormField>
      </template>

      <template v-else-if="kind === 'acceptance'">
        <UiFormField
          label="验收编号"
          required
        >
          <UiInput
            v-model="form.acceptanceCode"
            :disabled="saving"
            maxlength="64"
            required
          />
        </UiFormField>
        <UiFormField
          label="验收事项"
          required
        >
          <UiInput
            v-model="form.title"
            :disabled="saving"
            maxlength="128"
            required
          />
        </UiFormField>
        <UiFormField
          label="验收结果"
          required
        >
          <UiInput
            v-model="form.acceptanceResult"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="验收状态"
          required
        >
          <UiInput
            v-model="form.status"
            :disabled="saving"
            maxlength="32"
            required
          />
        </UiFormField>
        <UiFormField
          label="验收日期"
          hint="留空时由服务端按当天记录。"
        >
          <UiInput
            v-model="form.acceptanceDate"
            :disabled="saving"
            type="date"
          />
        </UiFormField>
        <UiFormField label="关联合同">
          <UiSelect
            v-model="form.linkedContractCode"
            label="关联合同"
            :options="contractOptions"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="里程碑 ID"
          hint="可留空；填写时服务端会检查该里程碑属于当前项目。"
        >
          <UiInput
            v-model="form.milestoneId"
            :disabled="saving"
            maxlength="36"
            pattern="[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"
            placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
          />
        </UiFormField>
        <UiFormField
          label="质量检查编号"
          hint="当前后端将此字段作为可选文本保存，没有独立检查编号实体。"
        >
          <UiInput
            v-model="form.qualityInspectionCode"
            :disabled="saving"
            maxlength="64"
          />
        </UiFormField>
      </template>

      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
      <UiButton
        type="submit"
        variant="primary"
        :disabled="saving"
      >
        {{ saving ? '正在保存…' : '保存' }}
      </UiButton>
    </form>
  </UiDialog>
</template>
