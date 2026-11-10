import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import {
  tenderDepositStatuses,
  tenderMethods,
  tenderOwners,
  tenderPriorities,
  tenderResults,
  tendersReferenceFixture,
  tenderStages,
  tenderTasks,
} from '@/features/crm/model/tenders.data';
import type {
  TenderDepositStatus,
  TenderDraft,
  TenderPriority,
  TenderResult,
  TenderRow,
  TenderStage,
  TenderTab,
} from '@/features/crm/model/tenders.types';

export function useTendersPage() {
  const rows = ref<TenderRow[]>(cloneReferenceRows<TenderRow>(tendersReferenceFixture.rows));
  const query = ref('');
  const tab = ref<TenderTab>('bids');
  const stage = ref<TenderStage | 'all'>('all');
  const result = ref<TenderResult | 'all'>('all');
  const deposit = ref<TenderDepositStatus | 'all'>('all');
  const owner = ref('all');
  const method = ref<TenderRow['method'] | 'all'>('all');
  const priority = ref<TenderPriority | 'all'>('all');
  const period = ref('all');
  const editorOpen = ref(false);
  const selectedRow = ref<TenderRow | null>(null);
  let nextCode = 7;

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesQuery =
        !keyword ||
        [row.name, row.code, row.customer, row.customerShort].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return (
        matchesQuery &&
        (stage.value === 'all' || row.stage === stage.value) &&
        (result.value === 'all' || row.result === result.value) &&
        (deposit.value === 'all' || row.depositStatus === deposit.value) &&
        (owner.value === 'all' || row.owner === owner.value) &&
        (method.value === 'all' || row.method === method.value) &&
        (priority.value === 'all' || row.priority === priority.value)
      );
    });
  });

  const addedCount = computed(() => Math.max(0, rows.value.length - tendersReferenceFixture.rows.length));
  const monthCount = computed(() => 18 + addedCount.value);
  const activeCount = computed(() => 13 + addedCount.value);
  const wonCount = computed(() => 3);
  const winRate = computed(() => '60%');
  const openingCount = computed(() => 2 + addedCount.value);
  const pendingDeposit = computed(() => 1030000);

  function saveTender(draft: TenderDraft) {
    const depositAmount = Number(draft.depositAmount);
    if (!draft.name.trim() || !draft.customer.trim() || !draft.deadline || !draft.openingDate) {
      toast.warning('请填写项目、招标单位和关键日期');
      return;
    }
    if (!Number.isFinite(depositAmount) || depositAmount < 0) {
      toast.warning('请输入有效保证金金额');
      return;
    }
    const code = `BID-2026-${String(nextCode).padStart(3, '0')}`;
    rows.value = [
      {
        id: `LOCAL-${code}`,
        code,
        name: draft.name.trim(),
        customer: draft.customer.trim(),
        customerShort: draft.customer.trim().slice(0, 8),
        owner: draft.owner,
        stage: draft.stage,
        deadline: draft.deadline,
        openingDate: draft.openingDate,
        depositStatus: '待申请',
        depositAmount,
        result: '待结果',
        risks: [],
        updatedAt: '2026-09-16 10:30',
        priority: draft.priority,
        method: draft.method,
        active: true,
      },
      ...rows.value,
    ];
    nextCode += 1;
    editorOpen.value = false;
    toast.success('投标项目已创建');
  }

  function exportRows() {
    const body = filteredRows.value.map((row) => `${row.code},${row.name},${row.customer},${row.stage}`).join('\n');
    const url = URL.createObjectURL(
      new Blob([`项目编号,项目名称,招标单位,当前阶段\n${body}`], { type: 'text/csv;charset=utf-8' }),
    );
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = 'tenders.csv';
    anchor.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${filteredRows.value.length} 个投标项目`);
  }

  return {
    rows,
    query,
    tab,
    stage,
    result,
    deposit,
    owner,
    method,
    priority,
    period,
    editorOpen,
    selectedRow,
    filteredRows,
    monthCount,
    activeCount,
    wonCount,
    winRate,
    openingCount,
    pendingDeposit,
    stages: tenderStages,
    results: tenderResults,
    depositStatuses: tenderDepositStatuses,
    owners: tenderOwners,
    methods: tenderMethods,
    priorities: tenderPriorities,
    tasks: tenderTasks,
    saveTender,
    exportRows,
  };
}
