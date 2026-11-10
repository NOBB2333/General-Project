<script setup lang="ts">
import { computed } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import FinanceAccountsHero from './accounts/FinanceAccountsHero.vue';
import FinanceAccountsLedger from './accounts/FinanceAccountsLedger.vue';
import OpeningBalancePanel from './accounts/OpeningBalancePanel.vue';
import { useFinanceAccountsPage } from './accounts/useFinanceAccountsPage';
import type { FinanceAccountRowAction, FinanceAccountSurface } from '../model/accounts.types';

const {
  surface,
  view,
  ageTab,
  ageTabs,
  query,
  source,
  sourceOptions,
  billRelation,
  billOptions,
  startDate,
  endDate,
  advancedOpen,
  filteredRows,
  pagedRows,
  groupedRows,
  selected,
  page,
  pageCount,
  side,
  isOpening,
  summary,
  detailRow,
  detailOpen,
  actionRow,
  actionKind,
  actionOpen,
  actionAmount,
  actionNote,
  currentOpeningRows,
  openingTotal,
  openingDrafts,
  openingState,
  importOpen,
  importFileName,
  toggleSelection,
  togglePageSelection,
  openRowAction,
  confirmRowAction,
  exportRows,
  addOpeningDraft,
  removeOpeningDraft,
  updateOpeningDraft,
  saveOpeningDrafts,
  skipOpening,
  completeOpening,
  downloadOpeningTemplate,
  confirmImport,
} = useFinanceAccountsPage();

const actionTitle = computed(() => {
  if (!actionRow.value) return '账款操作';
  if (actionKind.value === 'settle')
    return `${side.value === 'receivable' ? '登记收款' : '登记付款'} · ${actionRow.value.code}`;
  if (actionKind.value === 'remind')
    return `${side.value === 'receivable' ? '发起催收' : '发起催付'} · ${actionRow.value.code}`;
  return `冲回记录 · ${actionRow.value.code}`;
});

const actionDescription = computed(() => {
  if (actionKind.value === 'settle') return '变更仅保存在当前本地会话，不会写入外部参考站。';
  if (actionKind.value === 'remind') return '记录一条本地催收/催付任务，供页面交互验收。';
  return '冲回操作仅作用于当前本地演示数据。';
});

function updateSurface(value: FinanceAccountSurface) {
  surface.value = value;
}

function updateImportFile(event: Event) {
  const input = event.target as HTMLInputElement;
  importFileName.value = input.files?.[0]?.name ?? '';
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-label="应收应付"
    data-page-module="财务管理"
    data-page-layout="finance-ar-ap"
    data-page-route="/finance/accounts"
  >
    <FinanceAccountsHero
      :model-value="surface"
      :summary="summary"
      :is-opening="isOpening"
      @update:model-value="updateSurface"
    />

    <OpeningBalancePanel
      v-if="isOpening"
      :side="side"
      :rows="currentOpeningRows"
      :drafts="openingDrafts"
      :total="openingTotal"
      :state="openingState[side]"
      @add="addOpeningDraft"
      @remove="removeOpeningDraft"
      @save="saveOpeningDrafts"
      @download="downloadOpeningTemplate"
      @import="importOpen = true"
      @skip="skipOpening"
      @complete="completeOpening"
      @update:draft="updateOpeningDraft"
    />

    <FinanceAccountsLedger
      v-else
      :side="side"
      :view="view"
      :age-tab="ageTab"
      :age-tabs="ageTabs"
      :query="query"
      :source="source"
      :source-options="sourceOptions"
      :bill-relation="billRelation"
      :bill-options="billOptions"
      :start-date="startDate"
      :end-date="endDate"
      :advanced-open="advancedOpen"
      :filtered-rows="filteredRows"
      :paged-rows="pagedRows"
      :grouped-rows="groupedRows"
      :selected="selected"
      :page="page"
      :page-count="pageCount"
      @update:view="view = $event"
      @update:age-tab="ageTab = $event"
      @update:query="query = $event"
      @update:source="source = $event"
      @update:bill-relation="billRelation = $event"
      @update:start-date="startDate = $event"
      @update:end-date="endDate = $event"
      @update:advanced-open="advancedOpen = $event"
      @update:page="page = $event"
      @toggle-selection="toggleSelection"
      @toggle-page-selection="togglePageSelection"
      @row-action="openRowAction"
      @export="exportRows"
    />

    <UiDialog
      v-model:open="detailOpen"
      :title="detailRow?.code ?? '账款详情'"
      :description="detailRow ? `${detailRow.party} · ${detailRow.status}` : undefined"
      size="lg"
    >
      <dl
        v-if="detailRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>来源单号</dt>
          <dd>{{ detailRow.sourceCode }}</dd>
        </div>
        <div>
          <dt>来源</dt>
          <dd>{{ detailRow.source }}</dd>
        </div>
        <div>
          <dt>{{ side === 'receivable' ? '客户' : '收款方' }}</dt>
          <dd>{{ detailRow.party }}</dd>
        </div>
        <div>
          <dt>{{ detailRow.contactRole }}</dt>
          <dd>{{ detailRow.contact }}</dd>
        </div>
        <div>
          <dt>合同/项目</dt>
          <dd>{{ detailRow.contract }} {{ detailRow.project }}</dd>
        </div>
        <div>
          <dt>到期日</dt>
          <dd>{{ detailRow.dueDate }} {{ detailRow.terms }}</dd>
        </div>
        <div>
          <dt>{{ side === 'receivable' ? '应收金额' : '应付金额' }}</dt>
          <dd>{{ detailRow.amount.toLocaleString('zh-CN') }}</dd>
        </div>
        <div>
          <dt>{{ side === 'receivable' ? '已收金额' : '已付金额' }}</dt>
          <dd>{{ detailRow.settled.toLocaleString('zh-CN') }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ detailRow.status }}</dd>
        </div>
        <div>
          <dt>{{ side === 'receivable' ? '发票' : '付款审核' }}</dt>
          <dd>{{ detailRow.invoice }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="actionOpen"
      :title="actionTitle"
      :description="actionDescription"
      size="md"
    >
      <div :class="$style.actionForm">
        <UiFormField
          v-if="actionKind === 'settle'"
          :label="side === 'receivable' ? '本次收款金额' : '本次付款金额'"
          required
        >
          <UiInput
            v-model="actionAmount"
            type="number"
            placeholder="0"
          />
        </UiFormField>
        <UiFormField
          v-else
          label="备注"
        >
          <UiInput
            v-model="actionNote"
            placeholder="可选，记录本次处理说明"
          />
        </UiFormField>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="actionOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="confirmRowAction"
          >确认</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="importOpen"
      :title="`批量导入期初${side === 'receivable' ? '应收' : '应付'}`"
      description="只在本地演示站读取文件并创建本地记录"
      size="sm"
    >
      <div :class="$style.importForm">
        <label :class="$style.filePicker">
          <span>选择 CSV 文件</span>
          <input
            type="file"
            accept=".csv,.xlsx,.xls"
            @change="updateImportFile"
          />
        </label>
        <p v-if="importFileName">已选择：{{ importFileName }}</p>
        <p v-else>示例导入会在本地新增 1 条期初记录，不会上传到外部站点。</p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="importOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="confirmImport"
          >确认导入</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply flex h-full min-h-0 flex-col gap-3 overflow-hidden;
  background: #fafbfc;
}

.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}

.detailGrid dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}

.actionForm {
  @apply p-5;
}

.importForm {
  @apply p-5;
}

.filePicker {
  @apply flex cursor-pointer flex-col gap-2 rounded border border-dashed p-5 text-center text-[12px];
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}

.filePicker:hover {
  border-color: var(--ui-focus);
  background: var(--ui-surface-hover);
}

.filePicker input {
  @apply text-[11px];
}

.importForm p {
  @apply mt-2 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}

@media (max-width: 900px) {
  .page {
    @apply overflow-auto;
  }
}
</style>
