<script setup lang="ts">
import { Archive, Download, Info, Plus, Trash2, Upload } from 'lucide-vue-next';
import { UiButton, UiFormField, UiInput } from '@/components/ui';
import type { FinanceAccountSide, OpeningBalanceDraft, OpeningBalanceRow } from '../../model/accounts.types';

type DraftKey = keyof OpeningBalanceDraft;

defineProps<{
  side: FinanceAccountSide;
  rows: readonly OpeningBalanceRow[];
  drafts: readonly OpeningBalanceDraft[];
  total: number;
  state: '进行中' | '已完成' | '已跳过';
}>();

const emit = defineEmits<{
  add: [];
  remove: [index: number];
  save: [];
  download: [];
  import: [];
  skip: [];
  complete: [];
  'update:draft': [index: number, key: DraftKey, value: string];
}>();

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN', { maximumFractionDigits: 0 })}`;
}
</script>

<template>
  <section :class="$style.openingPanel">
    <header :class="$style.openingIntro">
      <div :class="$style.introHeading">
        <span :class="$style.introIcon">
          <Archive
            :size="19"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <strong>系统初始化引导 · 阶段 5「期初数据建账」</strong>
          <p>录入完成后会同步标记到系统初始化引导</p>
        </div>
      </div>
      <span :class="[$style.stateBadge, state === '进行中' ? $style.stateActive : $style.stateDone]">{{ state }}</span>
    </header>

    <article :class="$style.explain">
      <div :class="$style.explainTitle">
        <Info
          :size="16"
          :stroke-width="1.8"
        />
        <strong>什么是期初{{ side === 'receivable' ? '应收' : '应付' }}建账？</strong>
      </div>
      <p>
        在系统正式启用日（2026-01-01）之前签订、截至建账日（T-1：2025-12-31）仍有合同款未{{
          side === 'receivable' ? '收' : '付'
        }}的业务，按「合同总额 − 已{{ side === 'receivable' ? '收款' : '付款' }} =
        期初未结清」的口径在此一次性录入。系统会按{{
          side === 'receivable' ? '客户' : '供应商'
        }}汇总未结清部分，作为对账单“期初{{ side === 'receivable' ? '应收' : '应付' }}余额”和账龄分析的起点。
      </p>
      <ul>
        <li>单据日期固定为 2025-12-31，但原合同到期日需如实填写以正确计算账龄</li>
        <li>“未结清”指合同尚未{{ side === 'receivable' ? '收回' : '付出' }}的金额（含未到期部分）</li>
        <li>期初记录不生成业务凭证，已开发票部分由财务自行核对底稿</li>
      </ul>
    </article>

    <div :class="$style.summaryGrid">
      <article>
        <span>已录入笔数</span>
        <strong>{{ rows.length }}</strong>
        <small
          >{{ rows.filter((row) => row.origin === 'system').length }} 条系统预置 ·
          {{ rows.filter((row) => row.origin === 'local').length }} 条本次新增</small
        >
      </article>
      <article>
        <span>期初{{ side === 'receivable' ? '应收' : '应付' }}总额</span>
        <strong>{{ money(total) }}</strong>
        <small
          >涉及 {{ new Set(rows.map((row) => row.party)).size }} 个{{
            side === 'receivable' ? '客户' : '供应商'
          }}</small
        >
      </article>
      <article>
        <span>其中已逾期</span>
        <strong :class="$style.dangerValue">{{ money(total) }}</strong>
        <small>建账后将进入{{ side === 'receivable' ? '催收清单' : '待付任务' }}</small>
      </article>
    </div>

    <div :class="$style.sectionHeader">
      <div>
        <strong>期初{{ side === 'receivable' ? '应收' : '应付' }}明细</strong>
        <span>截止日 · 2025-12-31</span>
      </div>
      <div :class="$style.sectionActions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('download')"
        >
          <template #leading>
            <Download
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          下载模板
        </UiButton>
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('import')"
        >
          <template #leading>
            <Upload
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          批量导入
        </UiButton>
        <UiButton
          variant="subtle"
          size="sm"
          @click="emit('skip')"
          >无期初{{ side === 'receivable' ? '应收' : '应付' }} · 跳过</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('complete')"
          >完成建账</UiButton
        >
      </div>
    </div>

    <div :class="$style.tableWrap">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>来源</th>
            <th>{{ side === 'receivable' ? '客户' : '供应商' }}</th>
            <th>{{ side === 'receivable' ? '原销售单号 / 合同号' : '原采购单号 / 合同号' }}</th>
            <th>期初未结清</th>
            <th>原到期日 / 账龄</th>
            <th>{{ side === 'receivable' ? '备注' : '付款条款 / 备注' }}</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <span :class="$style.originBadge">{{ row.origin === 'system' ? '已建账' : '本次新增' }}</span>
            </td>
            <td>
              <strong>{{ row.party }}</strong>
            </td>
            <td>
              <span>{{ row.sourceCode }}</span
              ><small>{{ row.contract }}</small>
            </td>
            <td :class="$style.moneyCell">{{ money(row.amount) }}</td>
            <td>
              <span>{{ row.dueDate }}</span
              ><small>{{ row.age }}</small>
            </td>
            <td>{{ row.note }}</td>
            <td>
              <span :class="$style.systemAction">{{ row.origin === 'system' ? '系统' : '本地' }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <section :class="$style.draftSection">
      <header>
        <div>
          <strong>新增期初{{ side === 'receivable' ? '应收' : '应付' }}</strong>
          <span>填写后点击「保存」加入清单</span>
        </div>
        <UiButton
          variant="secondary"
          size="xs"
          @click="emit('add')"
        >
          <template #leading>
            <Plus
              :size="13"
              :stroke-width="1.8"
            />
          </template>
          增加一行
        </UiButton>
      </header>
      <div
        v-for="(draft, index) in drafts"
        :key="index"
        :class="$style.draftRow"
      >
        <UiFormField
          :label="`${side === 'receivable' ? '客户' : '供应商'}名称`"
          required
        >
          <UiInput
            :model-value="draft.party"
            :placeholder="side === 'receivable' ? '例如：华能电力科技' : '例如：威纶通科技'"
            @update:model-value="emit('update:draft', index, 'party', $event)"
          />
        </UiFormField>
        <UiFormField label="关联合同号">
          <UiInput
            :model-value="draft.contract"
            placeholder="HT-2025-0001"
            @update:model-value="emit('update:draft', index, 'contract', $event)"
          />
        </UiFormField>
        <UiFormField
          label="合同总额(元)"
          required
        >
          <UiInput
            :model-value="draft.totalAmount"
            type="number"
            placeholder="0"
            @update:model-value="emit('update:draft', index, 'totalAmount', $event)"
          />
        </UiFormField>
        <UiFormField :label="side === 'receivable' ? '已收款(元)' : '已付款(元)'">
          <UiInput
            :model-value="draft.settledAmount"
            type="number"
            placeholder="0"
            @update:model-value="emit('update:draft', index, 'settledAmount', $event)"
          />
        </UiFormField>
        <UiFormField label="原到期日">
          <UiInput
            :model-value="draft.dueDate"
            type="date"
            @update:model-value="emit('update:draft', index, 'dueDate', $event)"
          />
        </UiFormField>
        <UiFormField label="备注">
          <UiInput
            :model-value="draft.note"
            placeholder="可选"
            @update:model-value="emit('update:draft', index, 'note', $event)"
          />
        </UiFormField>
        <strong :class="$style.draftAmount"
          >¥{{ Math.max(0, Number(draft.totalAmount) - Number(draft.settledAmount)).toLocaleString('zh-CN') }}</strong
        >
        <button
          type="button"
          :class="$style.removeDraft"
          aria-label="删除新增行"
          @click="emit('remove', index)"
        >
          <Trash2
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </div>
      <footer :class="$style.draftFooter">
        <span>期初未结清金额将按合同总额减已{{ side === 'receivable' ? '收款' : '付款' }}自动计算</span>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('save')"
          >保存</UiButton
        >
      </footer>
    </section>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.openingPanel {
  @apply min-h-0 flex-1 overflow-auto rounded border bg-white p-4;
  border-color: var(--ui-border);
}

.openingIntro {
  @apply flex items-start justify-between gap-3 rounded border px-4 py-3;
  border-color: #b3d4ff;
  background: #deebff;
}

.introHeading {
  @apply flex items-center gap-2;
}

.introIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: white;
}

.introHeading strong {
  @apply block text-[13px];
  color: #172b4d;
}

.introHeading p {
  @apply m-0 mt-0.5 text-[11px];
  color: #42526e;
}

.stateBadge {
  @apply rounded px-2 py-1 text-[11px] font-semibold;
}

.stateActive {
  color: #974f0c;
  background: #fffae6;
}

.stateDone {
  color: #006644;
  background: #e3fcef;
}

.explain {
  @apply mt-3 rounded border px-4 py-3 text-[12px];
  border-color: #ffe380;
  background: #fffae6;
  color: #42526e;
}

.explainTitle {
  @apply flex items-center gap-1.5 font-semibold;
  color: #974f0c;
}

.explain p {
  @apply m-0 mt-2 leading-5;
}

.explain ul {
  @apply m-2 grid list-disc grid-cols-2 gap-x-6 gap-y-1 pl-4 text-[11px];
}

.summaryGrid {
  @apply mt-3 grid grid-cols-3 gap-3;
}

.summaryGrid article {
  @apply rounded border bg-white px-3 py-3;
  border-color: var(--ui-border);
}

.summaryGrid span {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}

.summaryGrid strong {
  @apply mt-1 block text-[20px];
  color: var(--ui-text);
}

.summaryGrid small {
  @apply mt-1 block text-[11px];
  color: #97a0af;
}

.dangerValue {
  color: var(--ui-danger) !important;
}

.sectionHeader {
  @apply mt-4 flex items-center justify-between gap-3;
}

.sectionHeader > div:first-child strong {
  @apply block text-[14px];
}

.sectionHeader > div:first-child span {
  @apply ml-2 text-[11px];
  color: var(--ui-text-subtle);
}

.sectionActions {
  @apply flex flex-wrap justify-end gap-2;
}

.tableWrap {
  @apply mt-2 overflow-auto rounded border;
  border-color: var(--ui-border);
}

.table {
  @apply w-full min-w-[1000px] border-collapse text-left text-[12px];
}

.table th {
  @apply border-b px-3 py-2 text-[11px] font-medium;
  border-color: var(--ui-border);
  background: #f4f5f7;
  color: var(--ui-text-subtle);
}

.table td {
  @apply border-b px-3 py-2 align-middle;
  border-color: var(--ui-border-subtle);
  color: var(--ui-text);
}

.table td small {
  @apply mt-0.5 block text-[10px];
  color: #97a0af;
}

.originBadge {
  @apply rounded px-2 py-1 text-[10px] font-semibold;
  color: #006644;
  background: #e3fcef;
}

.moneyCell {
  @apply whitespace-nowrap font-semibold;
}

.systemAction {
  color: var(--ui-text-subtle);
}

.draftSection {
  @apply mt-4 rounded border;
  border-color: var(--ui-border);
}

.draftSection > header {
  @apply flex items-center justify-between border-b px-3 py-2;
  border-color: var(--ui-border-subtle);
  background: #fafbfc;
}

.draftSection > header strong {
  @apply block text-[13px];
}

.draftSection > header span {
  @apply ml-2 text-[11px];
  color: var(--ui-text-subtle);
}

.draftRow {
  @apply grid items-end gap-2 border-b p-3;
  grid-template-columns:
    minmax(140px, 1.25fr) minmax(120px, 1fr) minmax(100px, 0.9fr) minmax(100px, 0.9fr) minmax(125px, 1fr)
    minmax(100px, 0.9fr) 100px 28px;
  border-color: var(--ui-border-subtle);
}

.draftAmount {
  @apply pb-2 text-[12px];
  color: var(--app-primary);
}

.removeDraft {
  @apply mb-1 flex h-7 w-7 items-center justify-center rounded;
  color: var(--ui-text-subtle);
}

.removeDraft:hover {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.draftFooter {
  @apply flex items-center justify-between gap-3 px-3 py-2;
  color: var(--ui-text-subtle);
  font-size: 11px;
}

@media (max-width: 1000px) {
  .explain ul {
    @apply grid-cols-1;
  }

  .draftRow {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .draftAmount,
  .removeDraft {
    @apply self-end;
  }
}

@media (max-width: 650px) {
  .openingPanel {
    @apply p-3;
  }

  .openingIntro,
  .sectionHeader,
  .draftFooter {
    @apply items-start flex-col;
  }

  .sectionActions {
    @apply justify-start;
  }

  .summaryGrid {
    @apply grid-cols-1;
  }

  .draftRow {
    @apply grid-cols-1;
  }
}
</style>
