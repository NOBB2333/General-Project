import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { quoteCustomers, quoteOwners, salesQuotesReferenceFixture } from '@/features/crm/model/sales-quotes.data';
import type {
  QuoteScope,
  QuoteStatus,
  QuoteView,
  SalesQuoteDraft,
  SalesQuoteRow,
} from '@/features/crm/model/sales-quotes.types';

const quoteStatuses: readonly QuoteStatus[] = ['草稿', '待审核', '已发送', '已接受', '已拒绝', '已过期'];

export function useSalesQuotesPage() {
  const rows = ref<SalesQuoteRow[]>(cloneReferenceRows<SalesQuoteRow>(salesQuotesReferenceFixture.rows));
  const query = ref('');
  const scope = ref<QuoteScope>('all');
  const status = ref<QuoteStatus | 'all'>('all');
  const view = ref<QuoteView>('list');
  const editorOpen = ref(false);
  let nextCode = 43;

  const scopedRows = computed(() =>
    rows.value.filter((row) => {
      if (scope.value === 'all') return true;
      if (scope.value === 'following') return row.followed;
      return row.ownerScope === scope.value;
    }),
  );
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return scopedRows.value.filter((row) => {
      const matchesStatus = status.value === 'all' || row.status === status.value;
      const matchesQuery =
        !keyword ||
        [row.code, row.customer, row.project, row.owner].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        );
      return matchesStatus && matchesQuery;
    });
  });
  const scopeCounts = computed(() => ({
    all: rows.value.length,
    mine: rows.value.filter((row) => row.ownerScope === 'mine').length,
    subordinate: rows.value.filter((row) => row.ownerScope === 'subordinate').length,
    following: rows.value.filter((row) => row.followed).length,
  }));
  const statusCounts = computed(() => {
    const counts = { all: scopedRows.value.length } as Record<'all' | QuoteStatus, number>;
    quoteStatuses.forEach((item) => {
      counts[item] = scopedRows.value.filter((row) => row.status === item).length;
    });
    return counts;
  });
  const totalAmount = computed(() => rows.value.reduce((sum, row) => sum + row.amount, 0));
  const wonAmount = computed(() =>
    rows.value.filter((row) => row.status === '已接受').reduce((sum, row) => sum + row.amount, 0),
  );
  const followUpCount = computed(
    () => rows.value.filter((row) => ['草稿', '待审核', '已发送'].includes(row.status)).length,
  );
  const wonCount = computed(() => rows.value.filter((row) => row.status === '已接受').length);
  const decidedCount = computed(
    () => rows.value.filter((row) => ['已接受', '已拒绝', '已过期'].includes(row.status)).length,
  );
  const winRate = computed(() => (decidedCount.value ? Math.round((wonCount.value / decidedCount.value) * 100) : 0));

  function saveQuote(draft: SalesQuoteDraft) {
    const amount = Number(draft.amount);
    if (!draft.customer || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请选择客户并填写有效报价金额');
      return;
    }
    const owner = draft.owner || quoteOwners[0] || '张明';
    rows.value = [
      {
        id: `QUOTE-LOCAL-${nextCode}`,
        code: `QT-2026-${String(nextCode).padStart(4, '0')}`,
        customer: draft.customer,
        project: draft.project.trim() || '-',
        owner,
        ownerInitial: owner.slice(0, 1),
        ownerScope: owner === '张明' ? 'mine' : 'subordinate',
        productCount: Math.max(1, Number(draft.productCount) || 1),
        amount,
        discount: draft.discount ? Number(draft.discount) : null,
        status: draft.status,
        validUntil: draft.validUntil || '2026-03-31',
        date: '2026-02-20',
      },
      ...rows.value,
    ];
    nextCode += 1;
    editorOpen.value = false;
    toast.success('报价单已创建');
  }

  function exportQuotes() {
    const header = ['报价单号', '客户', '关联项目', '负责人', '商品数', '报价金额', '折扣', '状态', '有效期至', '日期'];
    const lines = filteredRows.value.map((row) =>
      [
        row.code,
        row.customer,
        row.project,
        row.owner,
        row.productCount,
        row.amount,
        row.discount ?? '',
        row.status,
        row.validUntil,
        row.date,
      ]
        .map((cell) => `"${String(cell).replaceAll('"', '""')}"`)
        .join(','),
    );
    const url = URL.createObjectURL(
      new Blob([`\uFEFF${[header.join(','), ...lines].join('\n')}`], { type: 'text/csv;charset=utf-8' }),
    );
    const link = document.createElement('a');
    link.href = url;
    link.download = '销售报价.csv';
    link.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${filteredRows.value.length} 条报价`);
  }

  return {
    rows,
    query,
    scope,
    status,
    view,
    editorOpen,
    scopedRows,
    filteredRows,
    scopeCounts,
    statusCounts,
    totalAmount,
    wonAmount,
    followUpCount,
    wonCount,
    winRate,
    customers: quoteCustomers,
    owners: quoteOwners,
    saveQuote,
    exportQuotes,
  };
}
