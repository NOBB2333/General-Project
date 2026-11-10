import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { paymentFlowReferenceFixture } from '../../model/payment-flow.data';
import type {
  PaymentAllocationDraft,
  PaymentFlowDraft,
  PaymentFlowRow,
  PaymentFlowScope,
} from '../../model/payment-flow.types';

const emptyPaymentDraft = (): PaymentFlowDraft => ({
  supplier: '',
  date: '2026-09-16',
  method: '银行转账',
  account: '招商银行 (…0810 001)',
  amount: '',
});

const emptyAllocationDraft = (): PaymentAllocationDraft => ({
  orderCode: '',
  amount: '',
  note: '',
});

export function usePaymentFlowPage() {
  const rows = ref<PaymentFlowRow[]>(cloneReferenceRows(paymentFlowReferenceFixture.rows));
  const activeScope = ref<PaymentFlowScope>('all');
  const supplier = ref('全部供应商');
  const query = ref('');
  const paymentDialogOpen = ref(false);
  const allocationDialogOpen = ref(false);
  const activeRow = ref<PaymentFlowRow | null>(null);
  const paymentDraft = reactive<PaymentFlowDraft>(emptyPaymentDraft());
  const allocationDraft = reactive<PaymentAllocationDraft>(emptyAllocationDraft());

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesScope =
        activeScope.value === 'all' ||
        (activeScope.value === 'allocated' ? row.status === 'allocated' : row.status !== 'allocated');
      const matchesSupplier = supplier.value === '全部供应商' || row.supplier === supplier.value;
      const haystack = `${row.code} ${row.supplier} ${row.account} ${row.method} ${row.operator}`.toLocaleLowerCase(
        'zh-CN',
      );
      return matchesScope && matchesSupplier && (!keyword || haystack.includes(keyword));
    });
  });

  const totals = computed(() => {
    const total = rows.value.reduce((sum, row) => sum + row.amount, 0);
    const allocated = rows.value.reduce((sum, row) => sum + row.allocatedAmount, 0);
    return {
      total,
      allocated,
      unallocated: total - allocated,
      incomplete: rows.value.filter((row) => row.status !== 'allocated').length,
      allocatedCount: rows.value.filter((row) => row.status === 'allocated').length,
    };
  });

  function openPayment() {
    Object.assign(paymentDraft, emptyPaymentDraft());
    paymentDialogOpen.value = true;
  }

  function savePayment() {
    const amount = Number(paymentDraft.amount.replace(/[,¥\s]/g, ''));
    if (!paymentDraft.supplier.trim() || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请选择供应商并填写有效的付款金额');
      return;
    }
    const created: PaymentFlowRow = {
      id: `payment-flow-local-${Date.now()}`,
      code: `PF-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
      imported: false,
      supplier: paymentDraft.supplier,
      date: paymentDraft.date,
      method: paymentDraft.method,
      account: paymentDraft.account,
      amount,
      allocatedAmount: 0,
      status: 'unallocated',
      operator: '张经理',
    };
    rows.value = [created, ...rows.value];
    activeScope.value = 'all';
    supplier.value = '全部供应商';
    query.value = '';
    paymentDialogOpen.value = false;
    toast.success(`${created.code} 已保存到当前本地会话`);
  }

  function openAllocation(row: PaymentFlowRow) {
    activeRow.value = row;
    Object.assign(allocationDraft, {
      orderCode: '',
      amount: String(row.amount - row.allocatedAmount || row.amount),
      note: '',
    });
    allocationDialogOpen.value = true;
  }

  function saveAllocation() {
    if (!activeRow.value) return;
    const amount = Number(allocationDraft.amount.replace(/[,¥\s]/g, ''));
    if (!allocationDraft.orderCode.trim() || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请填写采购订单号和有效的分配金额');
      return;
    }
    const target = activeRow.value;
    const nextAllocated = Math.min(
      target.status === 'allocated' ? amount : target.allocatedAmount + amount,
      target.amount,
    );
    rows.value = rows.value.map((row) =>
      row.id === target.id
        ? {
            ...row,
            allocatedAmount: nextAllocated,
            status: nextAllocated >= row.amount ? 'allocated' : 'partial',
            assignmentMode: '手动分配',
          }
        : row,
    );
    activeRow.value = rows.value.find((row) => row.id === target.id) ?? null;
    allocationDialogOpen.value = false;
    toast.success(`${target.code} 已分配到 ${allocationDraft.orderCode.trim()}`);
  }

  function exportRows() {
    const header = ['流水号', '供应商', '付款日期', '方式', '付款账户', '付款金额', '已分配', '未分配', '经办人'];
    const lines = filteredRows.value.map((row) => [
      row.code,
      row.supplier,
      row.date,
      row.method,
      row.account,
      row.amount,
      row.allocatedAmount,
      row.amount - row.allocatedAmount,
      row.operator,
    ]);
    const csv = [header, ...lines]
      .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = href;
    link.download = 'purchase-payment-flow.csv';
    link.click();
    URL.revokeObjectURL(href);
    toast.success(`已导出 ${filteredRows.value.length} 笔付款流水`);
  }

  return {
    rows,
    activeScope,
    supplier,
    query,
    paymentDialogOpen,
    allocationDialogOpen,
    activeRow,
    paymentDraft,
    allocationDraft,
    filteredRows,
    totals,
    openPayment,
    savePayment,
    openAllocation,
    saveAllocation,
    exportRows,
  };
}
