import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { customersReferenceFixture } from '@/features/crm/model/customers.data';
import { customerColumns, customerOwners } from '@/features/crm/model/customers.model';
import type {
  CustomerColumn,
  CustomerDraft,
  CustomerFollowUpDraft,
  CustomerRow,
  CustomerScope,
  CustomerTrafficColor,
} from '@/features/crm/model/customers.types';

type ConfirmationKind = 'recycle' | 'purge' | 'clear';
export type CustomerToolbarAction = 'create' | 'import' | 'scan' | 'export' | 'recycle-bin' | 'analytics';

export function useCustomersPage() {
  const rows = ref<CustomerRow[]>(cloneReferenceRows<CustomerRow>(customersReferenceFixture.rows));
  const recycledRows = ref<CustomerRow[]>([]);
  const scope = ref<CustomerScope>('all');
  const activeType = ref('全部');
  const query = ref('');
  const owner = ref('');
  const tag = ref('');
  const receivableOnly = ref(false);
  const analyticsOpen = ref(false);
  const visibleColumns = ref<Set<CustomerColumn['key']>>(new Set(customerColumns.map((column) => column.key)));
  const activeCustomer = ref<CustomerRow | null>(null);
  const editingCustomer = ref<CustomerRow | null>(null);
  const detailOpen = ref(false);
  const editorOpen = ref(false);
  const followUpOpen = ref(false);
  const recycleOpen = ref(false);
  const confirmationOpen = ref(false);
  const intakeOpen = ref(false);
  const intakeMode = ref<'import' | 'scan'>('import');
  const confirmationKind = ref<ConfirmationKind>('recycle');
  const pendingCustomer = ref<CustomerRow | null>(null);
  let nextLocalCustomer = 1;

  const filteredRows = computed(() =>
    rows.value.filter((row) => {
      const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
      const matchesScope =
        scope.value === 'all' ||
        (scope.value === 'mine' && row.ownerId === 'u1') ||
        (scope.value === 'subordinates' && row.ownerId !== 'u1') ||
        (scope.value === 'followed' && row.followed);
      const matchesType = activeType.value === '全部' || row.type === activeType.value;
      const matchesKeyword =
        !keyword ||
        row.name.toLocaleLowerCase('zh-CN').includes(keyword) ||
        row.contact.toLocaleLowerCase('zh-CN').includes(keyword) ||
        row.phone.includes(keyword);
      return (
        matchesScope &&
        matchesType &&
        matchesKeyword &&
        (!owner.value || row.owner === owner.value) &&
        (!tag.value || row.tags.includes(tag.value)) &&
        (!receivableOnly.value || row.receivable > 0)
      );
    }),
  );

  const totalSales = computed(() => rows.value.reduce((sum, row) => sum + row.totalSales, 0));
  const totalReceivable = computed(() => rows.value.reduce((sum, row) => sum + row.receivable, 0));
  const receivableShare = computed(() =>
    totalSales.value > 0 ? `${((totalReceivable.value / totalSales.value) * 100).toFixed(1)}%` : '0.0%',
  );
  const averageTerms = computed(() =>
    rows.value.length ? Math.round(rows.value.reduce((sum, row) => sum + row.paymentDays, 0) / rows.value.length) : 0,
  );
  const salesLabel = computed(() => `¥${(totalSales.value / 10000).toFixed(0)}万`);
  const receivableLabel = computed(() => `¥${(totalReceivable.value / 10000).toFixed(1)}万`);
  const topOwners = computed(() =>
    customerOwners
      .map((name) => ({
        name,
        customers: rows.value.filter((row) => row.owner === name).length,
        sales: rows.value.filter((row) => row.owner === name).reduce((sum, row) => sum + row.totalSales, 0),
      }))
      .filter((item) => item.customers)
      .sort((left, right) => right.sales - left.sales),
  );
  const confirmationTitle = computed(() => {
    if (confirmationKind.value === 'clear') return '清空回收站';
    if (confirmationKind.value === 'purge') return '彻底删除客户';
    return '删除客户';
  });
  const confirmationDescription = computed(() => {
    if (confirmationKind.value === 'clear') return '确定要清空回收站吗？此操作仅影响当前本地会话，但无法撤销。';
    if (confirmationKind.value === 'purge') return `确定要彻底删除「${pendingCustomer.value?.name ?? ''}」吗？`;
    return `确定要将「${pendingCustomer.value?.name ?? ''}」移入回收站吗？`;
  });

  function openDetails(customer: CustomerRow) {
    activeCustomer.value = customer;
    detailOpen.value = true;
  }

  function openEditor(customer: CustomerRow | null = null) {
    detailOpen.value = false;
    editingCustomer.value = customer;
    editorOpen.value = true;
  }

  function openFollowUp(customer: CustomerRow) {
    detailOpen.value = false;
    activeCustomer.value = customer;
    followUpOpen.value = true;
  }

  function saveCustomer(draft: CustomerDraft) {
    if (!draft.name.trim() || !draft.contact.trim() || !draft.phone.trim()) {
      toast.warning('请填写客户名称、联系人和联系电话');
      return;
    }
    const ownerId = `u${Math.max(1, customerOwners.indexOf(draft.owner as (typeof customerOwners)[number]) + 1)}`;
    if (editingCustomer.value) {
      rows.value = rows.value.map((row) =>
        row.id === editingCustomer.value?.id
          ? {
              ...row,
              ...draft,
              ownerId,
              creditLimit: Number(draft.creditLimit) || 0,
              paymentDays: Number(draft.paymentDays) || 0,
            }
          : row,
      );
      toast.success(`已更新客户「${draft.name.trim()}」`);
    } else {
      rows.value = [createLocalCustomer(draft), ...rows.value];
      toast.success(`已新增客户「${draft.name.trim()}」`);
    }
    editorOpen.value = false;
  }

  function createLocalCustomer(draft: CustomerDraft): CustomerRow {
    const ownerId = `u${Math.max(1, customerOwners.indexOf(draft.owner as (typeof customerOwners)[number]) + 1)}`;
    const customer: CustomerRow = {
      id: `C-LOCAL-${String(nextLocalCustomer).padStart(3, '0')}`,
      ...draft,
      name: draft.name.trim(),
      contact: draft.contact.trim(),
      phone: draft.phone.trim(),
      ownerId,
      creditLimit: Number(draft.creditLimit) || 0,
      paymentDays: Number(draft.paymentDays) || 0,
      totalSales: 0,
      receivable: 0,
      tags: ['新客户'],
      followed: false,
      trafficColor: 'green',
      trafficReason: '新建客户，暂无风险',
      followUpCount: 0,
    };
    nextLocalCustomer += 1;
    return customer;
  }

  function importCustomers(drafts: CustomerDraft[]) {
    if (!drafts.length) return;
    rows.value = [...drafts.map(createLocalCustomer), ...rows.value];
    intakeOpen.value = false;
    toast.success(`已从本地 CSV 导入 ${drafts.length} 条客户`);
  }

  function createScannedCustomer(draft: CustomerDraft) {
    rows.value = [createLocalCustomer(draft), ...rows.value];
    intakeOpen.value = false;
    toast.success(`已根据本地名片创建「${draft.name.trim()}」`);
  }

  function saveFollowUp(draft: CustomerFollowUpDraft) {
    if (!activeCustomer.value || !draft.content.trim()) {
      toast.warning('请填写跟进内容');
      return;
    }
    const customerId = activeCustomer.value.id;
    rows.value = rows.value.map((row) =>
      row.id === customerId ? { ...row, followUpCount: row.followUpCount + 1 } : row,
    );
    followUpOpen.value = false;
    toast.success(`已为「${activeCustomer.value.name}」保存${draft.method}跟进`);
  }

  function cycleTraffic(customer: CustomerRow) {
    const next: Record<CustomerTrafficColor, CustomerTrafficColor> = { green: 'yellow', yellow: 'red', red: 'green' };
    rows.value = rows.value.map((row) =>
      row.id === customer.id ? { ...row, trafficColor: next[row.trafficColor], trafficReason: '本地手动调整' } : row,
    );
    toast.success(`已更新「${customer.name}」红绿灯`);
  }

  function toggleColumn(key: CustomerColumn['key']) {
    if (key === 'name') return;
    const next = new Set(visibleColumns.value);
    next.has(key) ? next.delete(key) : next.add(key);
    visibleColumns.value = next;
  }

  function requestConfirmation(kind: ConfirmationKind, customer: CustomerRow | null = null) {
    confirmationKind.value = kind;
    pendingCustomer.value = customer;
    confirmationOpen.value = true;
  }

  function confirmDestructiveAction() {
    if (confirmationKind.value === 'clear') {
      recycledRows.value = [];
      toast.success('回收站已清空');
    } else if (confirmationKind.value === 'purge' && pendingCustomer.value) {
      recycledRows.value = recycledRows.value.filter((row) => row.id !== pendingCustomer.value?.id);
      toast.success('客户已从本地回收站删除');
    } else if (pendingCustomer.value) {
      rows.value = rows.value.filter((row) => row.id !== pendingCustomer.value?.id);
      recycledRows.value = [pendingCustomer.value, ...recycledRows.value];
      toast.success(`已将「${pendingCustomer.value.name}」移入回收站`);
    }
    confirmationOpen.value = false;
    pendingCustomer.value = null;
  }

  function restoreCustomer(customer: CustomerRow) {
    recycledRows.value = recycledRows.value.filter((row) => row.id !== customer.id);
    rows.value = [...rows.value, customer];
    toast.success(`已恢复「${customer.name}」`);
  }

  function exportCustomers() {
    const headers = [
      '客户名称',
      '类型',
      '级别',
      '负责人',
      '联系人',
      '电话',
      '信用额度',
      '账期',
      '累计销售',
      '应收余额',
    ];
    const values = filteredRows.value.map((row) => [
      row.name,
      row.type,
      row.level,
      row.owner,
      row.contact,
      row.phone,
      row.creditLimit,
      row.paymentDays,
      row.totalSales,
      row.receivable,
    ]);
    const csv = [headers, ...values]
      .map((line) => line.map((value) => `"${String(value).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'crm-customers.csv';
    link.click();
    URL.revokeObjectURL(url);
    toast.success(`已导出 ${filteredRows.value.length} 条客户数据`);
  }

  function runToolbarAction(id: CustomerToolbarAction) {
    if (id === 'create') openEditor();
    else if (id === 'export') exportCustomers();
    else if (id === 'recycle-bin') recycleOpen.value = true;
    else if (id === 'analytics') analyticsOpen.value = !analyticsOpen.value;
    else {
      intakeMode.value = id;
      intakeOpen.value = true;
    }
  }

  return {
    rows,
    recycledRows,
    scope,
    activeType,
    query,
    owner,
    tag,
    receivableOnly,
    analyticsOpen,
    visibleColumns,
    activeCustomer,
    editingCustomer,
    detailOpen,
    editorOpen,
    followUpOpen,
    recycleOpen,
    confirmationOpen,
    intakeOpen,
    intakeMode,
    confirmationKind,
    filteredRows,
    totalSales,
    totalReceivable,
    receivableShare,
    averageTerms,
    salesLabel,
    receivableLabel,
    topOwners,
    confirmationTitle,
    confirmationDescription,
    openDetails,
    openEditor,
    openFollowUp,
    saveCustomer,
    saveFollowUp,
    cycleTraffic,
    toggleColumn,
    requestConfirmation,
    confirmDestructiveAction,
    restoreCustomer,
    importCustomers,
    createScannedCustomer,
    runToolbarAction,
  };
}
