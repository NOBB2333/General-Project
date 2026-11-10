<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue';
import { getBusinessLegalEntitiesApi } from '@/api/core/business-legal-entities';
import type { BusinessLegalEntityApi } from '@/api/core/business-legal-entities';
import {
  deleteBusinessAcceptanceApi,
  deleteBusinessBudgetExecutionApi,
  deleteBusinessChainApi,
  deleteBusinessContractApi,
  deleteBusinessInvoiceApi,
  deleteBusinessProcurementApi,
  deleteBusinessReceivableApi,
} from '@/api/core/business';
import type { BusinessApi } from '@/api/core/business';
import { UiAlertDialog, UiButton, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';
import {
  buildBusinessRecordRows,
  type BusinessRecordKind,
  type BusinessRecordRow,
  type BusinessRecordTarget,
} from '../model/business-records.model';
import BusinessRecordEditorDialog from './BusinessRecordEditorDialog.vue';

const props = defineProps<{
  currentUserId: string;
  currentUserName: string;
  detail: BusinessApi.ProjectDetail;
  projectId: string;
}>();

const emit = defineEmits<{
  changed: [];
}>();

const tabs: Array<{ kind: BusinessRecordKind; label: string }> = [
  { kind: 'budget', label: '预算执行' },
  { kind: 'chain', label: '经营链' },
  { kind: 'contract', label: '合同' },
  { kind: 'invoice', label: '开票' },
  { kind: 'procurement', label: '采购' },
  { kind: 'receivable', label: '应收' },
  { kind: 'acceptance', label: '验收' },
];
const activeKind = ref<BusinessRecordKind>('budget');
const editorKind = ref<BusinessRecordKind>('budget');
const editorOpen = ref(false);
const editorTarget = ref<BusinessRecordTarget | null>(null);
const deleteTarget = ref<BusinessRecordRow | null>(null);
const legalEntities = ref<BusinessLegalEntityApi.Item[]>([]);
const legalEntityLoadError = ref('');
const deleteError = ref('');
const deleting = ref(false);
const canManage = computed(() => hasAccessCode('Business.Project.Manage'));
const rows = computed(() => buildBusinessRecordRows(props.detail, activeKind.value));
const activeLabel = computed(() => tabs.find((tab) => tab.kind === activeKind.value)?.label ?? '经营记录');
const isAmountKind = computed(() =>
  ['budget', 'contract', 'invoice', 'procurement', 'receivable'].includes(activeKind.value),
);
const canCreate = computed(() => canManage.value && (!isAmountKind.value || props.detail.canViewSensitiveAmounts));

onMounted(async () => {
  if (!hasAccessCode('Business.LegalEntity.View')) return;
  try {
    legalEntities.value = await getBusinessLegalEntitiesApi();
  } catch (cause) {
    legalEntityLoadError.value =
      cause instanceof Error ? cause.message : '公司主体暂不可用，本次可继续使用租户默认主体。';
  }
});

function setActiveKind(value: string) {
  if (tabs.some((tab) => tab.kind === value)) activeKind.value = value as BusinessRecordKind;
}

function rowCanEdit(row: BusinessRecordRow): boolean {
  if (!canManage.value) return false;
  if (!['budget', 'contract', 'invoice', 'procurement', 'receivable'].includes(row.kind)) return true;
  if (!props.detail.canViewSensitiveAmounts) return false;

  switch (row.target.kind) {
    case 'budget':
      return row.target.item.adjustedAmount !== null && row.target.item.executedAmount !== null;
    case 'contract':
      return row.target.item.amount !== null;
    case 'procurement':
      return row.target.item.amount !== null;
    case 'invoice':
      return row.target.item.amount !== null && row.target.item.taxAmount !== null;
    case 'receivable':
      return row.target.item.totalAmount !== null && row.target.item.receivedAmount !== null;
    case 'acceptance':
    case 'chain':
      return true;
  }
}

function openCreate() {
  if (!canCreate.value) return;
  editorKind.value = activeKind.value;
  editorTarget.value = null;
  editorOpen.value = true;
}

function openEdit(row: BusinessRecordRow) {
  if (!rowCanEdit(row)) return;
  editorKind.value = row.kind;
  editorTarget.value = row.target;
  editorOpen.value = true;
}

function requestDelete(row: BusinessRecordRow) {
  if (!canManage.value) return;
  deleteError.value = '';
  deleteTarget.value = row;
}

function setDeleteOpen(open: boolean) {
  if (deleting.value) return;
  if (!open) {
    void nextTick(() => {
      if (deleting.value) return;
      deleteTarget.value = null;
      deleteError.value = '';
    });
  }
}

async function deleteSelected() {
  const target = deleteTarget.value?.target;
  if (!target || deleting.value) return;
  deleting.value = true;
  deleteError.value = '';
  try {
    switch (target.kind) {
      case 'acceptance':
        await deleteBusinessAcceptanceApi(props.projectId, target.item.id);
        break;
      case 'budget':
        await deleteBusinessBudgetExecutionApi(props.projectId, target.item.id);
        break;
      case 'chain':
        await deleteBusinessChainApi(props.projectId, target.item.id);
        break;
      case 'contract':
        await deleteBusinessContractApi(props.projectId, target.item.id);
        break;
      case 'invoice':
        await deleteBusinessInvoiceApi(props.projectId, target.item.id);
        break;
      case 'procurement':
        await deleteBusinessProcurementApi(props.projectId, target.item.id);
        break;
      case 'receivable':
        await deleteBusinessReceivableApi(props.projectId, target.item.id);
        break;
    }
    deleteTarget.value = null;
    emit('changed');
  } catch (cause) {
    deleteError.value = cause instanceof Error ? cause.message : '记录删除失败，请重试。';
  } finally {
    deleting.value = false;
  }
}

function confirmTitle(row: BusinessRecordRow | null): string {
  return row ? `删除${activeLabel.value}记录` : '删除经营记录';
}

function confirmDescription(row: BusinessRecordRow | null): string {
  if (!row) return '此操作会删除当前项目的经营记录。';
  return row.kind === 'contract'
    ? `确认删除合同“${row.code} ${row.primary}”？已被其它经营记录引用的合同不能删除。`
    : `确认删除“${row.code} ${row.primary}”？此操作不能撤销。`;
}
</script>

<template>
  <section aria-label="项目经营记录维护">
    <header>
      <div>
        <h3>经营记录</h3>
        <p>数据来自项目经营详情；保存后会重新读取服务端记录。</p>
      </div>
      <UiButton
        v-if="canCreate"
        variant="primary"
        size="sm"
        @click="openCreate"
      >
        新增{{ activeLabel }}
      </UiButton>
    </header>

    <p
      v-if="canManage && isAmountKind && !detail.canViewSensitiveAmounts"
      role="note"
    >
      当前账号没有金额查看权限，预算、合同、开票、采购和应收的写入入口已隐藏，以免覆盖服务端脱敏金额。
    </p>
    <p
      v-if="legalEntityLoadError"
      role="status"
    >
      {{ legalEntityLoadError }}；新增记录仍可由服务端采用租户默认主体。
    </p>

    <UiTabs
      :model-value="activeKind"
      @update:model-value="setActiveKind"
    >
      <UiTabsList label="项目经营记录类型">
        <UiTabsTrigger
          v-for="tab in tabs"
          :key="tab.kind"
          :value="tab.kind"
        >
          {{ tab.label }}（{{ buildBusinessRecordRows(detail, tab.kind).length }}）
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div v-if="rows.length">
      <article
        v-for="row in rows"
        :key="row.id"
      >
        <div>
          <p>
            {{ row.code }}<span v-if="row.status"> · {{ row.status }}</span>
          </p>
          <p>{{ row.primary }}</p>
          <p>{{ row.secondary }}</p>
        </div>
        <div v-if="canManage">
          <UiButton
            v-if="rowCanEdit(row)"
            variant="secondary"
            size="sm"
            @click="openEdit(row)"
          >
            编辑
          </UiButton>
          <span
            v-else-if="['budget', 'contract', 'invoice', 'procurement', 'receivable'].includes(row.kind)"
            role="note"
          >
            金额不可见，不能编辑
          </span>
          <UiButton
            variant="danger"
            size="sm"
            @click="requestDelete(row)"
          >
            删除
          </UiButton>
        </div>
      </article>
    </div>
    <p v-else>当前项目没有{{ activeLabel }}记录。</p>

    <BusinessRecordEditorDialog
      v-model:open="editorOpen"
      :contracts="detail.contracts"
      :invoices="detail.invoices"
      :legal-entities="legalEntities"
      :receivables="detail.receivables"
      :current-user-id="currentUserId"
      :current-user-name="currentUserName"
      :kind="editorKind"
      :project-id="projectId"
      :record="editorTarget"
      @saved="emit('changed')"
    />

    <UiAlertDialog
      :open="Boolean(deleteTarget)"
      :title="confirmTitle(deleteTarget)"
      :description="confirmDescription(deleteTarget)"
      :confirm-label="deleting ? '正在删除…' : '确认删除'"
      :cancel-label="deleting ? '请稍候' : '取消'"
      @update:open="setDeleteOpen"
      @confirm="deleteSelected"
    >
      <p
        v-if="deleteError"
        role="alert"
      >
        {{ deleteError }}
      </p>
    </UiAlertDialog>
  </section>
</template>
