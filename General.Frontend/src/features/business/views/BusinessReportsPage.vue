<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import type { BusinessApi } from '@/api/core/business';
import { exportBusinessReportApi, getBusinessReportApi } from '@/api/core/business';
import { UiButton, UiFormField, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';
import type { BusinessReportSection } from '@/features/business/model/business.models';
import {
  buildBusinessReportListModel,
  buildBusinessReportQuery,
  businessReportSectionLabel,
} from '@/features/business/model/business.models';
import { ListPageSurface } from '@/components/page';
import type { PageAction } from '@/components/page';

const sections: readonly BusinessReportSection[] = [
  'projects',
  'profit',
  'contracts',
  'budgets',
  'procurements',
  'acceptances',
  'invoices',
  'receivables',
  'warnings',
];

type BusinessReportForm = Record<keyof BusinessApi.ReportQuery, string>;

const form = reactive<BusinessReportForm>({
  businessCloseStatus: '',
  contractType: '',
  counterpartyName: '',
  keyword: '',
  plannedDateFrom: '',
  plannedDateTo: '',
  projectCode: '',
  receivableStatus: '',
  signDateFrom: '',
  signDateTo: '',
  supplierName: '',
  warningLevel: '',
});
const report = ref<BusinessApi.Report | null>(null);
const activeSection = ref<BusinessReportSection>('projects');
const loading = ref(false);
const exporting = ref(false);
const error = ref('');
const exportError = ref('');
const appliedQuery = ref<BusinessApi.ReportQuery>({});
const canExport = computed(() => hasAccessCode('Business.Report.Export'));
const pageModel = computed(() =>
  buildBusinessReportListModel(report.value, activeSection.value, loading.value, error.value),
);

let activeRequest: AbortController | null = null;
let activeExport: AbortController | null = null;
let requestVersion = 0;

const sectionLedgerKinds: Record<BusinessReportSection, BusinessApi.ReportLedgerKind> = {
  acceptances: 'acceptance',
  budgets: 'budget',
  contracts: 'contract',
  invoices: 'invoice',
  profit: 'profit',
  procurements: 'procurement',
  projects: 'project',
  receivables: 'receivable',
  warnings: 'warning',
};

const sectionExportTitles: Record<BusinessReportSection, string> = {
  acceptances: '验收记录',
  budgets: '预算台账',
  contracts: '合同台账',
  invoices: '开票记录',
  profit: '利润分析',
  procurements: '采购台账',
  projects: '项目经营',
  receivables: '回款记录',
  warnings: '经营预警',
};

async function loadReport(query = appliedQuery.value) {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  loading.value = true;
  error.value = '';
  report.value = null;

  try {
    report.value = await getBusinessReportApi(query, controller.signal);
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '经营报表查询失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

function submitQuery() {
  appliedQuery.value = buildBusinessReportQuery(form);
  void loadReport(appliedQuery.value);
}

function handleAction(action: PageAction) {
  if (action.kind === 'refresh') void loadReport(appliedQuery.value);
}

function updateSection(value: string) {
  if (sections.includes(value as BusinessReportSection)) activeSection.value = value as BusinessReportSection;
}

async function exportReport() {
  if (!canExport.value || exporting.value || loading.value || !report.value) return;
  const section = activeSection.value;
  activeExport?.abort();
  const controller = new AbortController();
  activeExport = controller;
  exporting.value = true;
  exportError.value = '';

  try {
    const blob = await exportBusinessReportApi(
      { ...appliedQuery.value, ledgerKind: sectionLedgerKinds[section] },
      controller.signal,
    );
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `经营${sectionExportTitles[section]}.csv`;
    anchor.hidden = true;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    window.setTimeout(() => URL.revokeObjectURL(url), 0);
  } catch (cause) {
    if (!controller.signal.aborted) {
      exportError.value = cause instanceof Error ? cause.message : '经营报表导出失败，请重试。';
    }
  } finally {
    if (activeExport === controller) activeExport = null;
    exporting.value = false;
  }
}

onMounted(() => void loadReport());
onBeforeUnmount(() => {
  activeRequest?.abort();
  activeExport?.abort();
});
</script>

<template>
  <div>
    <header>
      <h1>报表中心</h1>
      <p>筛选条件由经营报表接口处理；结果范围和金额脱敏遵循后端权限。</p>
    </header>

    <form
      :class="$style.reportFilters"
      @submit.prevent="submitQuery"
    >
      <UiFormField label="关键字">
        <UiInput
          v-model="form.keyword"
          placeholder="项目、合同、单据或事项"
        />
      </UiFormField>
      <UiFormField label="项目编号">
        <UiInput v-model="form.projectCode" />
      </UiFormField>
      <UiFormField label="合同方">
        <UiInput v-model="form.counterpartyName" />
      </UiFormField>
      <UiFormField label="供应商">
        <UiInput v-model="form.supplierName" />
      </UiFormField>
      <UiFormField label="经营收尾状态">
        <UiInput v-model="form.businessCloseStatus" />
      </UiFormField>
      <UiFormField label="合同类型">
        <UiInput v-model="form.contractType" />
      </UiFormField>
      <UiFormField label="应收状态">
        <UiInput v-model="form.receivableStatus" />
      </UiFormField>
      <UiFormField label="预警级别">
        <UiInput v-model="form.warningLevel" />
      </UiFormField>
      <UiFormField label="签约日期起">
        <UiInput
          v-model="form.signDateFrom"
          type="date"
        />
      </UiFormField>
      <UiFormField label="签约日期止">
        <UiInput
          v-model="form.signDateTo"
          type="date"
        />
      </UiFormField>
      <UiFormField label="计划日期起">
        <UiInput
          v-model="form.plannedDateFrom"
          type="date"
        />
      </UiFormField>
      <UiFormField label="计划日期止">
        <UiInput
          v-model="form.plannedDateTo"
          type="date"
        />
      </UiFormField>
      <div :class="$style.queryAction">
        <UiButton
          variant="primary"
          type="submit"
          :disabled="loading"
        >
          查询
        </UiButton>
        <UiButton
          v-if="canExport"
          variant="secondary"
          type="button"
          :disabled="loading || exporting || !report"
          @click="exportReport"
        >
          {{ exporting ? '正在导出…' : '导出CSV' }}
        </UiButton>
      </div>
    </form>

    <UiTabs
      :model-value="activeSection"
      @update:model-value="updateSection"
    >
      <UiTabsList label="经营报表类别">
        <UiTabsTrigger
          v-for="section in sections"
          :key="section"
          :value="section"
        >
          {{ businessReportSectionLabel(section) }}
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <p
      v-if="error"
      role="alert"
      aria-live="polite"
    >
      {{ error }}
    </p>
    <p
      v-if="exportError"
      role="alert"
      aria-live="polite"
    >
      {{ exportError }}
    </p>
    <ListPageSurface
      :model="pageModel"
      @action="handleAction"
    />
  </div>
</template>

<style module>
@reference "../../../styles/index.css";

.reportFilters {
  @apply grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4;
}

.queryAction {
  @apply flex items-end;
}
</style>
