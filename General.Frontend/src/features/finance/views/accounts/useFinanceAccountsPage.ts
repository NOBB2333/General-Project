import { computed, reactive, ref, watch } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { financeAccountsReferenceFixture, openingBalancesReferenceFixture } from '../../model/accounts.data';
import type {
  FinanceAccountRow,
  FinanceAccountRowAction,
  FinanceAccountSide,
  FinanceAccountSummary,
  FinanceAccountSurface,
  FinanceAccountView,
  OpeningBalanceDraft,
  OpeningBalanceRow,
} from '../../model/accounts.types';

const emptyOpeningDraft = (): OpeningBalanceDraft => ({
  party: '',
  contract: '',
  totalAmount: '',
  settledAmount: '0',
  dueDate: '2025-12-31',
  note: '',
});

function compactMoney(value: number) {
  return `¥${(value / 10_000).toFixed(1)}万`;
}

function escapeCsv(value: string | number) {
  return `"${String(value).replace(/"/g, '""')}"`;
}

export function useFinanceAccountsPage() {
  const rows = ref<FinanceAccountRow[]>(cloneReferenceRows(financeAccountsReferenceFixture.rows));
  const openingRows = ref<OpeningBalanceRow[]>(cloneReferenceRows(openingBalancesReferenceFixture.rows));
  const surface = ref<FinanceAccountSurface>('receivable');
  const view = ref<FinanceAccountView>('list');
  const ageTab = ref('all');
  const query = ref('');
  const source = ref('all');
  const billRelation = ref('all');
  const startDate = ref('');
  const endDate = ref('');
  const advancedOpen = ref(false);
  const selected = ref<Set<string>>(new Set());
  const page = ref(1);

  const detailRow = ref<FinanceAccountRow | null>(null);
  const detailOpen = ref(false);
  const actionRow = ref<FinanceAccountRow | null>(null);
  const actionKind = ref<FinanceAccountRowAction>('settle');
  const actionOpen = ref(false);
  const actionAmount = ref('');
  const actionNote = ref('');

  const openingDrafts = ref<OpeningBalanceDraft[]>([emptyOpeningDraft()]);
  const openingState = reactive<Record<FinanceAccountSide, '进行中' | '已完成' | '已跳过'>>({
    receivable: '进行中',
    payable: '进行中',
  });
  const importOpen = ref(false);
  const importFileName = ref('');

  const side = computed<FinanceAccountSide>(() =>
    surface.value === 'payable' || surface.value === 'opening-payable' ? 'payable' : 'receivable',
  );
  const isOpening = computed(() => surface.value.startsWith('opening-'));

  const sideRows = computed(() => rows.value.filter((row) => row.side === side.value));
  const activeRows = computed(() =>
    sideRows.value.filter((row) => (side.value === 'receivable' ? row.status !== '已完成' : row.status !== '已结清')),
  );
  const settledRows = computed(() =>
    sideRows.value.filter((row) => (side.value === 'receivable' ? row.status === '已完成' : row.status === '已结清')),
  );

  const ageTabs = computed(() =>
    side.value === 'receivable'
      ? [
          { value: 'all', label: '全部', count: 25 },
          { value: '未到期', label: '未到期', count: 14 },
          { value: '逾期1-30天', label: '逾期1-30天', count: 5 },
          { value: '逾期31-60天', label: '逾期31-60天', count: 2 },
          { value: '逾期61-90天', label: '逾期61-90天', count: 2, risk: true },
          { value: '逾期90天+', label: '逾期90天+', count: 2, danger: true },
          { value: 'settled', label: '已结清', count: 2 },
        ]
      : [
          { value: 'all', label: '全部' },
          { value: '未到期', label: '未到期' },
          { value: '1-30天', label: '1-30天' },
          { value: '31-60天', label: '31-60天' },
          { value: '61-90天', label: '61-90天' },
          { value: '逾期', label: '90天以上', count: 29, danger: true },
          { value: 'settled', label: '已结清', count: 5 },
        ],
  );

  const sourceOptions = computed(() => {
    const labels = [...new Set(activeRows.value.map((row) => row.source))];
    return [{ value: 'all', label: '全部来源' }, ...labels.map((label) => ({ value: label, label }))];
  });

  const billOptions = computed(() =>
    side.value === 'receivable'
      ? [
          { value: 'all', label: '全部票据关系' },
          { value: '退票转入', label: '退票转入' },
          { value: '票据收款', label: '票据收款' },
        ]
      : [
          { value: 'all', label: '全部核销状态' },
          { value: '无记录', label: '无记录' },
          { value: '已核销', label: '已核销' },
        ],
  );

  const filteredRows = computed(() => {
    let result = ageTab.value === 'settled' ? settledRows.value : activeRows.value;
    if (ageTab.value !== 'all' && ageTab.value !== 'settled') {
      result = result.filter((row) => row.status === ageTab.value);
    }
    if (source.value !== 'all') result = result.filter((row) => row.source === source.value);
    if (billRelation.value !== 'all') result = result.filter((row) => row.bill === billRelation.value);
    if (startDate.value) result = result.filter((row) => row.dueDate >= startDate.value);
    if (endDate.value) result = result.filter((row) => row.dueDate <= endDate.value);
    const keyword = query.value.trim().toLowerCase();
    if (keyword) {
      result = result.filter((row) =>
        [row.code, row.sourceCode, row.party, row.contact, row.contract, row.project]
          .join(' ')
          .toLowerCase()
          .includes(keyword),
      );
    }
    return result;
  });

  const pageSize = computed(() => (side.value === 'payable' ? 15 : 10));
  const pageCount = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / pageSize.value)));
  const pagedRows = computed(() => {
    const offset = (page.value - 1) * pageSize.value;
    return filteredRows.value.slice(offset, offset + pageSize.value);
  });

  const summary = computed<FinanceAccountSummary>(() => {
    const openRows = activeRows.value.filter((row) => row.amount > 0);
    const openTotal = openRows.reduce((total, row) => total + row.amount - row.settled, 0);
    if (side.value === 'payable') {
      return {
        totalLabel: '应付总额',
        totalValue: compactMoney(openTotal),
        totalHint: `${openRows.length} 笔待付`,
        overdueLabel: '逾期金额',
        overdueValue: compactMoney(openTotal),
        overdueHint: `${openRows.length} 笔逾期 · 0 笔 7 天内到期`,
        countLabel: '已结清',
        countValue: String(settledRows.value.length),
        countHint: `共 ${sideRows.value.length} 笔`,
        cycleLabel: '平均付款周期',
        cycleValue: '4天',
        cycleHint: '基于已付款记录',
      };
    }
    const overdueRows = activeRows.value.filter((row) => row.status.startsWith('逾期'));
    const overdueTotal = overdueRows.reduce((total, row) => total + row.amount - row.settled, 0);
    const highRisk = overdueRows
      .filter((row) => row.status === '逾期61-90天' || row.status === '逾期90天+')
      .reduce((total, row) => total + row.amount - row.settled, 0);
    return {
      totalLabel: '应收总额',
      totalValue: compactMoney(openTotal),
      totalHint: '共 25 笔未结清',
      overdueLabel: '逾期金额',
      overdueValue: compactMoney(overdueTotal),
      overdueHint: `占比 ${openTotal ? Math.round((overdueTotal / openTotal) * 100) : 0}%`,
      countLabel: '逾期笔数',
      countValue: String(overdueRows.length),
      countHint: `高风险 ${compactMoney(highRisk)}`,
      cycleLabel: '平均回款周期',
      cycleValue: '10天',
      cycleHint: '基于已核销记录',
    };
  });

  const groupedRows = computed(() => {
    const keyFor = (row: FinanceAccountRow) => {
      if (view.value === 'party') return row.party;
      if (view.value === 'contract') return row.contract || '未关联合同';
      return row.project || '未关联项目';
    };
    const groups = new Map<string, FinanceAccountRow[]>();
    for (const row of filteredRows.value) {
      const key = keyFor(row);
      groups.set(key, [...(groups.get(key) ?? []), row]);
    }
    return [...groups].map(([label, items]) => ({
      label,
      count: items.length,
      amount: items.reduce((total, item) => total + item.amount, 0),
      settled: items.reduce((total, item) => total + item.settled, 0),
      open: items.reduce((total, item) => total + item.amount - item.settled, 0),
    }));
  });

  const currentOpeningRows = computed(() => openingRows.value.filter((row) => row.side === side.value));
  const openingTotal = computed(() => currentOpeningRows.value.reduce((total, row) => total + row.amount, 0));

  watch([surface, ageTab, query, source, billRelation, startDate, endDate], () => {
    page.value = 1;
    selected.value = new Set();
  });

  watch(surface, () => {
    ageTab.value = 'all';
    source.value = 'all';
    billRelation.value = 'all';
    view.value = 'list';
  });

  watch(pageCount, (count) => {
    if (page.value > count) page.value = count;
  });

  function toggleSelection(id: string) {
    const next = new Set(selected.value);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selected.value = next;
  }

  function togglePageSelection() {
    const allSelected = pagedRows.value.every((row) => selected.value.has(row.id));
    const next = new Set(selected.value);
    for (const row of pagedRows.value) {
      if (allSelected) next.delete(row.id);
      else next.add(row.id);
    }
    selected.value = next;
  }

  function openDetails(row: FinanceAccountRow) {
    detailRow.value = row;
    detailOpen.value = true;
  }

  function openRowAction(kind: FinanceAccountRowAction, row: FinanceAccountRow) {
    if (kind === 'view') {
      openDetails(row);
      return;
    }
    if (kind === 'copy') {
      rows.value = [
        ...rows.value,
        {
          ...row,
          id: `finance-account-local-${Date.now()}`,
          code: `${row.code}-COPY`,
          source: '本地复制',
        },
      ];
      toast.success(`已复制 ${row.code}`);
      return;
    }
    actionRow.value = row;
    actionKind.value = kind;
    actionAmount.value = String(Math.max(0, row.amount - row.settled));
    actionNote.value = '';
    actionOpen.value = true;
  }

  function confirmRowAction() {
    const row = actionRow.value;
    if (!row) return;
    if (actionKind.value === 'settle') {
      const amount = Math.max(0, Number(actionAmount.value) || 0);
      row.settled = Math.min(row.amount, row.settled + amount);
      if (row.settled >= row.amount) row.status = row.side === 'receivable' ? '已完成' : '已结清';
      toast.success(`${row.side === 'receivable' ? '收款' : '付款'}登记成功`);
    } else if (actionKind.value === 'reverse') {
      row.amount = -Math.abs(row.amount);
      row.settled = 0;
      toast.success(`${row.code} 已在本地冲回`);
    } else {
      toast.success(`已记录 ${row.code} 的${row.side === 'receivable' ? '催收' : '催付'}任务`);
    }
    actionOpen.value = false;
  }

  function exportRows() {
    const exportRows = selected.value.size
      ? filteredRows.value.filter((row) => selected.value.has(row.id))
      : filteredRows.value;
    const content = [
      ['单号', '来源单号', '来源', '往来对象', '合同', '项目', '金额', '已结算', '到期日', '状态'],
      ...exportRows.map((row) => [
        row.code,
        row.sourceCode,
        row.source,
        row.party,
        row.contract,
        row.project,
        row.amount,
        row.settled,
        row.dueDate,
        row.status,
      ]),
    ]
      .map((line) => line.map(escapeCsv).join(','))
      .join('\n');
    const url = URL.createObjectURL(new Blob([`\uFEFF${content}`], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = `${side.value === 'receivable' ? 'receivables' : 'payables'}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${exportRows.length} 条账款`);
  }

  function addOpeningDraft() {
    openingDrafts.value.push(emptyOpeningDraft());
  }

  function updateOpeningDraft(index: number, key: keyof OpeningBalanceDraft, value: string) {
    const draft = openingDrafts.value[index];
    if (!draft) return;
    openingDrafts.value[index] = { ...draft, [key]: value };
  }

  function removeOpeningDraft(index: number) {
    if (openingDrafts.value.length === 1) openingDrafts.value = [emptyOpeningDraft()];
    else openingDrafts.value.splice(index, 1);
  }

  function saveOpeningDrafts() {
    const validDrafts = openingDrafts.value.filter(
      (draft) => draft.party.trim() && Number(draft.totalAmount) - Number(draft.settledAmount) > 0,
    );
    if (!validDrafts.length) {
      toast.warning(`请填写${side.value === 'receivable' ? '客户' : '供应商'}和有效金额`);
      return;
    }
    const localRows = validDrafts.map<OpeningBalanceRow>((draft, index) => ({
      id: `opening-balance-local-${Date.now()}-${index}`,
      side: side.value,
      party: draft.party.trim(),
      sourceCode: `${side.value === 'receivable' ? 'QC-AR' : 'QC-AP'}-LOCAL-${String(index + 1).padStart(3, '0')}`,
      contract: draft.contract.trim() || '-',
      amount: Number(draft.totalAmount) - Number(draft.settledAmount),
      dueDate: draft.dueDate,
      age: '待计算',
      note: draft.note.trim() || '本次新增',
      origin: 'local',
    }));
    openingRows.value.push(...localRows);
    openingDrafts.value = [emptyOpeningDraft()];
    toast.success(`已保存 ${localRows.length} 条期初${side.value === 'receivable' ? '应收' : '应付'}`);
  }

  function skipOpening() {
    openingState[side.value] = '已跳过';
    toast.success(`已在本地标记无期初${side.value === 'receivable' ? '应收' : '应付'}`);
  }

  function completeOpening() {
    openingState[side.value] = '已完成';
    toast.success(`期初${side.value === 'receivable' ? '应收' : '应付'}建账已在本地完成`);
  }

  function downloadOpeningTemplate() {
    const content = '\uFEFF往来对象,关联合同号,合同总额,已结算金额,原到期日,备注\n';
    const url = URL.createObjectURL(new Blob([content], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = `opening-${side.value}-template.csv`;
    link.click();
    URL.revokeObjectURL(url);
    toast.success('期初建账模板已下载');
  }

  function confirmImport() {
    if (!importFileName.value) {
      toast.warning('请先选择导入文件');
      return;
    }
    openingRows.value.push({
      id: `opening-balance-import-${Date.now()}`,
      side: side.value,
      party: side.value === 'receivable' ? '本地导入客户' : '本地导入供应商',
      sourceCode: side.value === 'receivable' ? 'QC-AR-LOCAL-IMPORT' : 'QC-AP-LOCAL-IMPORT',
      contract: 'LOCAL-CONTRACT',
      amount: 50_000,
      dueDate: '2025-12-31',
      age: '待计算',
      note: `导入自 ${importFileName.value}`,
      origin: 'local',
    });
    importOpen.value = false;
    toast.success('已在本地导入 1 条期初记录');
  }

  return {
    rows,
    openingRows,
    surface,
    view,
    ageTab,
    query,
    source,
    billRelation,
    startDate,
    endDate,
    advancedOpen,
    selected,
    page,
    side,
    isOpening,
    ageTabs,
    sourceOptions,
    billOptions,
    filteredRows,
    pagedRows,
    pageCount,
    summary,
    groupedRows,
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
    openDetails,
    openRowAction,
    confirmRowAction,
    exportRows,
    addOpeningDraft,
    updateOpeningDraft,
    removeOpeningDraft,
    saveOpeningDrafts,
    skipOpening,
    completeOpening,
    downloadOpeningTemplate,
    confirmImport,
  };
}
