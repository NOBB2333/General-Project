import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import {
  customerMaterialCustomers,
  customerMaterialsReferenceFixture,
} from '@/features/crm/model/customer-materials.data';
import type {
  CustomerMaterialDraft,
  CustomerMaterialImportMode,
  CustomerMaterialRow,
  CustomerMaterialToolbarAction,
} from '@/features/crm/model/customer-materials.types';

export function useCustomerMaterialsPage() {
  const rows = ref<CustomerMaterialRow[]>(
    cloneReferenceRows<CustomerMaterialRow>(customerMaterialsReferenceFixture.rows),
  );
  const query = ref('');
  const customer = ref('');
  const editorOpen = ref(false);
  const editingRow = ref<CustomerMaterialRow | null>(null);
  const importOpen = ref(false);
  const importMode = ref<CustomerMaterialImportMode>('file');
  const deleteOpen = ref(false);
  const pendingDelete = ref<CustomerMaterialRow | null>(null);
  let nextLocalNumber = 1;

  const filteredRows = computed(() => {
    const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      const matchesCustomer = !customer.value || row.customer === customer.value;
      const matchesKeyword =
        !keyword ||
        [
          row.customer,
          row.internalCode,
          row.internalName,
          row.internalSpec,
          row.customerCode,
          row.customerName,
          row.remark,
        ].some((value) => value.toLocaleLowerCase('zh-CN').includes(keyword));
      return matchesCustomer && matchesKeyword;
    });
  });
  const customerCount = computed(() => new Set(rows.value.map((row) => row.customer)).size);
  const materialCount = computed(() => new Set(rows.value.map((row) => row.internalCode)).size);
  const availableCustomers = computed(() => [
    ...new Set([...customerMaterialCustomers, ...rows.value.map((row) => row.customer)]),
  ]);

  function openEditor(row: CustomerMaterialRow | null = null) {
    editingRow.value = row;
    editorOpen.value = true;
  }

  function saveRow(draft: CustomerMaterialDraft) {
    if (
      !draft.customer.trim() ||
      !draft.internalCode.trim() ||
      !draft.internalName.trim() ||
      !draft.customerCode.trim() ||
      !draft.customerName.trim()
    ) {
      toast.warning('请填写客户、我方物料和客户物料的必填信息');
      return;
    }
    if (editingRow.value) {
      rows.value = rows.value.map((row) =>
        row.id === editingRow.value?.id
          ? {
              ...row,
              ...normalizeDraft(draft),
              maintainedAt: new Date().toISOString().slice(0, 16).replace('T', ' '),
              maintainedBy: '张经理',
            }
          : row,
      );
      toast.success('已更新「' + draft.customerCode.trim() + '」对照');
    } else {
      rows.value = [createLocalRow(draft), ...rows.value];
      toast.success('已新增「' + draft.customerCode.trim() + '」对照');
    }
    editorOpen.value = false;
  }

  function normalizeDraft(draft: CustomerMaterialDraft) {
    return {
      customer: draft.customer.trim(),
      internalCode: draft.internalCode.trim(),
      internalName: draft.internalName.trim(),
      internalSpec: draft.internalSpec.trim() || '-',
      unit: draft.unit.trim() || '台',
      customerCode: draft.customerCode.trim(),
      customerName: draft.customerName.trim(),
      remark: draft.remark.trim() || '-',
    };
  }

  function createLocalRow(draft: CustomerMaterialDraft): CustomerMaterialRow {
    const row: CustomerMaterialRow = {
      id: 'CM-LOCAL-' + String(nextLocalNumber).padStart(3, '0'),
      ...normalizeDraft(draft),
      maintainedAt: new Date().toISOString().slice(0, 16).replace('T', ' '),
      maintainedBy: '张经理',
    };
    nextLocalNumber += 1;
    return row;
  }

  function requestDelete(row: CustomerMaterialRow) {
    pendingDelete.value = row;
    deleteOpen.value = true;
  }

  function confirmDelete() {
    if (!pendingDelete.value) return;
    rows.value = rows.value.filter((row) => row.id !== pendingDelete.value?.id);
    toast.success('已删除「' + pendingDelete.value.customerCode + '」对照');
    deleteOpen.value = false;
    pendingDelete.value = null;
  }

  function importRows(drafts: CustomerMaterialDraft[]) {
    if (!drafts.length) return;
    rows.value = [...drafts.map(createLocalRow), ...rows.value];
    importOpen.value = false;
    toast.success('已导入 ' + drafts.length + ' 条客户物料对照');
  }

  function exportRows() {
    const header = [
      '客户名称',
      '我方物料编码',
      '我方物料名称',
      '规格型号',
      '单位',
      '客户物料编码',
      '客户物料名称',
      '备注',
      '最近维护',
      '维护人',
    ];
    const values = filteredRows.value.map((row) => [
      row.customer,
      row.internalCode,
      row.internalName,
      row.internalSpec,
      row.unit,
      row.customerCode,
      row.customerName,
      row.remark,
      row.maintainedAt,
      row.maintainedBy,
    ]);
    const csv = [header, ...values]
      .map((line) => line.map((value) => '"' + String(value).replace(/"/g, '""') + '"').join(','))
      .join('\n');
    const url = URL.createObjectURL(new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'customer-material-mappings.csv';
    link.click();
    URL.revokeObjectURL(url);
    toast.success('已导出 ' + filteredRows.value.length + ' 条物料对照');
  }

  function runToolbarAction(action: CustomerMaterialToolbarAction) {
    if (action === 'create') openEditor();
    else if (action === 'export') exportRows();
    else {
      importMode.value = action === 'import-file' ? 'file' : 'paste';
      importOpen.value = true;
    }
  }

  return {
    rows,
    query,
    customer,
    editorOpen,
    editingRow,
    importOpen,
    importMode,
    deleteOpen,
    pendingDelete,
    filteredRows,
    customerCount,
    materialCount,
    availableCustomers,
    openEditor,
    saveRow,
    requestDelete,
    confirmDelete,
    importRows,
    runToolbarAction,
  };
}
