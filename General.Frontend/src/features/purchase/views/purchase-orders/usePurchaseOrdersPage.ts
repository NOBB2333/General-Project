import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { purchaseOrdersReferenceFixture } from '../../model/purchase-orders.data';
import type {
  PurchaseOrderDraft,
  PurchaseOrderProgressFilter,
  PurchaseOrderRow,
  PurchaseOrderScope,
  PurchaseOrderStatus,
  PurchaseOrderView,
} from '../../model/purchase-orders.types';

const emptyDraft = (): PurchaseOrderDraft => ({
  supplier: '',
  project: '',
  buyer: '张经理',
  amount: '',
  itemCount: '1',
});

export function usePurchaseOrdersPage() {
  const rows = ref<PurchaseOrderRow[]>(cloneReferenceRows(purchaseOrdersReferenceFixture.rows));
  const activeScope = ref<PurchaseOrderScope>('all');
  const activeView = ref<PurchaseOrderView>('orders');
  const activeStatus = ref<'全部' | PurchaseOrderStatus>('全部');
  const supplier = ref('全部供应商');
  const progressFilter = ref<PurchaseOrderProgressFilter>('all');
  const query = ref('');
  const createOpen = ref(false);
  const detailOpen = ref(false);
  const paymentOpen = ref(false);
  const activeRow = ref<PurchaseOrderRow | null>(null);
  const draft = reactive<PurchaseOrderDraft>(emptyDraft());

  const scopeCounts = computed<Record<PurchaseOrderScope, number>>(() => ({
    all: rows.value.length,
    mine: rows.value.filter((row) => row.scopeGroup === 'mine').length,
    team: rows.value.filter((row) => row.scopeGroup === 'team').length,
    followed: rows.value.filter((row) => row.followed).length,
  }));

  const statusCounts = computed(() => ({
    全部: rows.value.length,
    草稿: rows.value.filter((row) => row.status === '草稿').length,
    待审核: rows.value.filter((row) => row.status === '待审核').length,
    已审核: rows.value.filter((row) => row.status === '已审核').length,
    部分到货: rows.value.filter((row) => row.status === '部分到货').length,
    已完成: rows.value.filter((row) => row.status === '已完成').length,
    已取消: rows.value.filter((row) => row.status === '已取消').length,
  }));

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesScope =
        activeScope.value === 'all' ||
        (activeScope.value === 'followed' ? row.followed : row.scopeGroup === activeScope.value);
      const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
      const matchesSupplier = supplier.value === '全部供应商' || row.supplier === supplier.value;
      const matchesProgress =
        progressFilter.value === 'all' ||
        (progressFilter.value === 'invoice-pending' && row.invoiceProgress < 100) ||
        (progressFilter.value === 'stock-pending' && row.stockProgress < 100) ||
        (progressFilter.value === 'payment-pending' && row.paymentProgress < 100);
      const text = `${row.code} ${row.supplier} ${row.project} ${row.buyer}`.toLocaleLowerCase('zh-CN');
      return (
        matchesScope && matchesStatus && matchesSupplier && matchesProgress && (!keyword || text.includes(keyword))
      );
    });
  });

  function openCreate() {
    Object.assign(draft, emptyDraft());
    createOpen.value = true;
  }

  function saveOrder() {
    const amount = Number(draft.amount.replace(/[,¥\s]/g, ''));
    if (!draft.supplier.trim() || !Number.isFinite(amount) || amount <= 0) {
      toast.warning('请选择供应商并填写有效的采购金额');
      return;
    }
    const created: PurchaseOrderRow = {
      id: `purchase-order-local-${Date.now()}`,
      code: `PO-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
      scopeGroup: 'mine',
      followed: false,
      supplier: draft.supplier,
      project: draft.project.trim() || '库存采购',
      payableRule: '按收货入库',
      buyer: draft.buyer,
      amount,
      invoiceProgress: 0,
      stockProgress: 0,
      paymentProgress: 0,
      status: '草稿',
      date: '2026-09-16',
      itemCount: Number(draft.itemCount) || 1,
    };
    rows.value = [created, ...rows.value];
    activeScope.value = 'all';
    activeStatus.value = '全部';
    supplier.value = '全部供应商';
    progressFilter.value = 'all';
    query.value = '';
    createOpen.value = false;
    toast.success(`${created.code} 已保存到当前本地会话`);
  }

  function openDetails(row: PurchaseOrderRow) {
    activeRow.value = row;
    detailOpen.value = true;
  }

  function openPayment(row: PurchaseOrderRow) {
    activeRow.value = row;
    paymentOpen.value = true;
  }

  function toggleFollow(target: PurchaseOrderRow) {
    rows.value = rows.value.map((row) => (row.id === target.id ? { ...row, followed: !row.followed } : row));
  }

  function submitPayment() {
    if (!activeRow.value) return;
    const rowId = activeRow.value.id;
    rows.value = rows.value.map((row) =>
      row.id === rowId ? { ...row, paymentProgress: Math.min(100, row.paymentProgress + 20) } : row,
    );
    activeRow.value = rows.value.find((row) => row.id === rowId) ?? null;
    paymentOpen.value = false;
    toast.success(`${activeRow.value?.code} 付款申请已更新到当前本地会话`);
  }

  function exportRows() {
    const header = [
      '订单号',
      '供应商',
      '关联项目',
      '确认方式',
      '采购员',
      '金额',
      '开票',
      '入库',
      '付款',
      '状态',
      '日期',
    ];
    const lines = filteredRows.value.map((row) => [
      row.code,
      row.supplier,
      row.project,
      row.payableRule,
      row.buyer,
      row.amount,
      `${row.invoiceProgress}%`,
      `${row.stockProgress}%`,
      `${row.paymentProgress}%`,
      row.status,
      row.date,
    ]);
    const csv = [header, ...lines]
      .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = href;
    link.download = 'purchase-orders.csv';
    link.click();
    URL.revokeObjectURL(href);
    toast.success(`已导出 ${filteredRows.value.length} 张采购订单`);
  }

  return {
    rows,
    activeScope,
    activeView,
    activeStatus,
    supplier,
    progressFilter,
    query,
    createOpen,
    detailOpen,
    paymentOpen,
    activeRow,
    draft,
    scopeCounts,
    statusCounts,
    filteredRows,
    openCreate,
    saveOrder,
    openDetails,
    openPayment,
    toggleFollow,
    submitPayment,
    exportRows,
  };
}
