import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { transferLoanReferenceFixture } from '../../model/transfer-loan.data';
import type {
  TransferLoanDraft,
  TransferLoanKind,
  TransferLoanRow,
  TransferLoanStatus,
} from '../../model/transfer-loan.types';

const emptyDraft = (): TransferLoanDraft => ({
  kind: 'transfer',
  sender: '上海主仓',
  receiver: '',
  reason: '',
  productCount: '1',
  quantity: '1',
  amount: '',
});

export function useTransferLoanPage() {
  const rows = ref<TransferLoanRow[]>(cloneReferenceRows(transferLoanReferenceFixture.rows));
  const activeKind = ref<'all' | TransferLoanKind>('all');
  const activeStatus = ref<'全部' | TransferLoanStatus>('全部');
  const query = ref('');
  const dialogOpen = ref(false);
  const detailOpen = ref(false);
  const activeRow = ref<TransferLoanRow | null>(null);
  const draft = reactive<TransferLoanDraft>(emptyDraft());
  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesKind = activeKind.value === 'all' || row.kind === activeKind.value;
      const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
      const text = `${row.code} ${row.sender} ${row.receiver} ${row.reason}`.toLocaleLowerCase('zh-CN');
      return matchesKind && matchesStatus && (!keyword || text.includes(keyword));
    });
  });
  const totalAmount = computed(() => rows.value.reduce((sum, row) => sum + row.amount, 0));

  function focusRow(code: string) {
    activeKind.value = 'all';
    activeStatus.value = '全部';
    query.value = code;
  }
  function openCreate() {
    Object.assign(draft, emptyDraft());
    dialogOpen.value = true;
  }
  function saveDraft() {
    if (!draft.receiver.trim() || !draft.reason.trim()) {
      toast.warning('请填写接收方和业务原因');
      return;
    }
    const sequence = rows.value.length + 1;
    rows.value = [
      {
        id: `transfer-loan-local-${sequence}`,
        code: `TR-LOCAL-${String(sequence).padStart(4, '0')}`,
        kind: draft.kind,
        type: draft.kind === 'loan' ? '借出管理' : draft.kind === 'repricing' ? '变价调拨' : '普通调拨',
        direction: draft.kind === 'loan' ? 'out' : undefined,
        sender: draft.sender,
        receiver: draft.receiver.trim(),
        reason: draft.reason.trim(),
        productCount: Number(draft.productCount) || 1,
        quantity: Number(draft.quantity) || 1,
        amount: Number(draft.amount) || 0,
        status: '待审核',
        date: '2026-09-16',
        applicant: '张经理',
      },
      ...rows.value,
    ];
    activeKind.value = 'all';
    activeStatus.value = '全部';
    query.value = '';
    dialogOpen.value = false;
    toast.success('调拨与借出单据已保存到当前本地会话');
  }
  function openDetails(row: TransferLoanRow) {
    activeRow.value = row;
    detailOpen.value = true;
  }
  function exportRows() {
    const values = filteredRows.value.map((row) => [
      row.code,
      row.type,
      row.sender,
      row.receiver,
      row.reason,
      row.quantity,
      row.amount,
      row.status,
      row.date,
      row.applicant,
    ]);
    const csv = [['单号', '业务类型', '发出方', '接收方', '原因', '数量', '金额', '状态', '日期', '申请人'], ...values]
      .map((line) => line.map((value) => `"${String(value).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = href;
    link.download = 'inventory-transfer-loan.csv';
    link.click();
    URL.revokeObjectURL(href);
    toast.success(`已导出 ${filteredRows.value.length} 条单据`);
  }
  return {
    rows,
    activeKind,
    activeStatus,
    query,
    dialogOpen,
    detailOpen,
    activeRow,
    draft,
    filteredRows,
    totalAmount,
    focusRow,
    openCreate,
    saveDraft,
    openDetails,
    exportRows,
  };
}
