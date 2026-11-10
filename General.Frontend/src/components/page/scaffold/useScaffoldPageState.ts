import { computed, ref, watch, type Ref } from 'vue';
import { toast } from '@/components/ui';
import type { PageFilter, PageModel, PageRow, PageRowAction } from '../page.types';
import { cloneReferenceRows } from '../reference';
import { resolveScaffoldAction } from './resolveScaffoldAction';
import { useScaffoldOverlayState } from './useScaffoldOverlayState';

const DEFAULT_ROW_ACTIONS: PageRowAction[] = ['view', 'edit', 'copy', 'delete'];

export function useScaffoldPageState(model: Ref<PageModel>) {
  const rows = ref<PageRow[]>([]);
  const selected = ref<Set<string>>(new Set());
  const activeTab = ref('全部');
  const query = ref('');
  const filterValues = ref<Record<string, string>>({});
  const {
    detailOpen,
    createOpen,
    confirmOpen,
    columnOpen,
    importOpen,
    pendingAction,
    activeRow,
    draft,
    resetOverlayState,
  } = useScaffoldOverlayState();
  const sortKey = ref('');
  const sortDir = ref<'asc' | 'desc'>('asc');
  const pageIndex = ref(1);
  const pageSize = ref(10);
  const columnVisibility = ref<Record<string, boolean>>({});
  const activeStatKey = ref<string | null>(null);

  function resetFromModel(next: PageModel) {
    rows.value = cloneReferenceRows(next.rows);
    selected.value = new Set();
    activeTab.value = next.tabs[0] ?? '全部';
    query.value = '';
    filterValues.value = Object.fromEntries(
      next.filters.map((filter: PageFilter) => [filter.key, filter.options[0] ?? '全部']),
    );
    sortKey.value = '';
    sortDir.value = 'asc';
    pageIndex.value = 1;
    pageSize.value = next.pageSize ?? 10;
    columnVisibility.value = Object.fromEntries(
      next.columns.filter((column) => column.key !== 'action').map((column) => [column.key, column.visible !== false]),
    );
    activeStatKey.value = null;
    resetOverlayState();
  }

  watch(model, (next) => resetFromModel(next), { immediate: true });

  watch(activeTab, () => {
    pageIndex.value = 1;
  });

  watch(query, () => {
    pageIndex.value = 1;
  });

  watch(
    filterValues,
    () => {
      pageIndex.value = 1;
    },
    { deep: true },
  );

  const dataColumns = computed(() =>
    model.value.columns.filter((column) => column.key !== 'action' && columnVisibility.value[column.key] !== false),
  );

  const allDataColumns = computed(() => model.value.columns.filter((column) => column.key !== 'action'));

  const rowActions = computed<PageRowAction[]>(() => model.value.rowActions ?? DEFAULT_ROW_ACTIONS);

  const filteredRows = computed(() => {
    let list = [...rows.value];
    const tab = activeTab.value;
    if (tab && !tab.startsWith('全部') && tab !== '总览' && tab !== '明细' && tab !== '趋势' && tab !== '商品库存') {
      const base = tab
        .replace(/\(.*\)$/, '')
        .replace(/\d+$/, '')
        .trim();
      const narrowed = list.filter((row) => {
        const status = String(row.status ?? '');
        const type = String(row.type ?? '');
        const category = String(row.category ?? '');
        const stage = String(row.stage ?? '');
        return (
          status.includes(base) ||
          type.includes(base) ||
          category.includes(base) ||
          stage.includes(base) ||
          tab.includes(status)
        );
      });
      if (narrowed.length) list = narrowed;
    }

    if (activeStatKey.value) {
      const stat = model.value.stats.find((item) => item.key === activeStatKey.value);
      if (stat?.filterKey && stat.filterValue) {
        list = list.filter((row) => String(row[stat.filterKey!] ?? '').includes(stat.filterValue!));
      } else if (stat && !['total', 'all', '全部'].includes(stat.key) && stat.label !== '全部') {
        const label = stat.label.replace(/\d+/g, '').trim();
        list = list.filter((row) => {
          const blob = `${row.status ?? ''} ${row.type ?? ''} ${row.category ?? ''}`;
          return blob.includes(label) || label.includes(String(row.status ?? ''));
        });
      }
    }

    const q = query.value.trim().toLowerCase();
    if (q) {
      list = list.filter((row) =>
        Object.values(row).some((value) =>
          String(value ?? '')
            .toLowerCase()
            .includes(q),
        ),
      );
    }

    for (const filter of model.value.filters) {
      const selectedValue = filterValues.value[filter.key];
      if (
        !selectedValue ||
        selectedValue.startsWith('全部') ||
        selectedValue === '所有' ||
        selectedValue === '全部时间'
      ) {
        continue;
      }
      if (filter.key === 'owner' || filter.key === 'applicant') {
        list = list.filter((row) => String(row.owner ?? row.applicant ?? '') === selectedValue);
      } else if (filter.key === 'supplier') {
        list = list.filter((row) => String(row.party ?? row.supplier ?? '').includes(selectedValue));
      } else if (filter.key === 'scope') {
        if (selectedValue === '我负责的') list = list.filter((_, index) => index % 3 === 0);
        if (selectedValue === '下属负责的') list = list.filter((_, index) => index % 3 === 1);
        if (selectedValue === '我关注的') list = list.filter((_, index) => index % 3 === 2);
      } else if (filter.key === 'time') {
        // local demo: keep all for 本月/本季, drop half for 本周
        if (selectedValue === '本周') list = list.filter((_, index) => index % 2 === 0);
        if (selectedValue === '本年') list = list;
      } else if (filter.key === 'type' || filter.key === 'dept' || filter.key === 'region' || filter.key === 'status') {
        list = list.filter((row) => {
          const blob = `${row.type ?? ''} ${row.dept ?? ''} ${row.status ?? ''} ${row.metric ?? ''} ${row.category ?? ''}`;
          return blob.includes(selectedValue) || selectedValue.includes(String(row.type ?? ''));
        });
      } else {
        list = list.filter((row) => {
          const value = String(row[filter.key] ?? row.status ?? row.type ?? '');
          return value.includes(selectedValue) || selectedValue.includes(value);
        });
      }
    }

    if (sortKey.value) {
      const key = sortKey.value;
      const dir = sortDir.value === 'asc' ? 1 : -1;
      list.sort((a, b) => String(a[key] ?? '').localeCompare(String(b[key] ?? ''), 'zh-CN') * dir);
    }
    return list;
  });

  const totalPages = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / pageSize.value)));

  const pagedRows = computed(() => {
    const start = (pageIndex.value - 1) * pageSize.value;
    return filteredRows.value.slice(start, start + pageSize.value);
  });

  watch([filteredRows, pageSize], () => {
    if (pageIndex.value > totalPages.value) pageIndex.value = totalPages.value;
  });

  const allSelected = computed(
    () => pagedRows.value.length > 0 && pagedRows.value.every((row) => selected.value.has(String(row.id))),
  );

  const selectedCount = computed(() => selected.value.size);

  const selectedRows = computed(() => rows.value.filter((row) => selected.value.has(String(row.id))));

  function notify(message: string) {
    if (/请先|至少|不能|失败/.test(message)) toast.warning(message);
    else toast.success(message);
  }

  function toggleRow(id: string) {
    const next = new Set(selected.value);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selected.value = next;
  }

  function toggleAll() {
    if (allSelected.value) {
      const next = new Set(selected.value);
      for (const row of pagedRows.value) next.delete(String(row.id));
      selected.value = next;
      return;
    }
    const next = new Set(selected.value);
    for (const row of pagedRows.value) next.add(String(row.id));
    selected.value = next;
  }

  function setSort(key: string) {
    if (sortKey.value === key) sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
    else {
      sortKey.value = key;
      sortDir.value = 'asc';
    }
  }

  function openDetail(row: PageRow) {
    activeRow.value = row;
    detailOpen.value = true;
  }

  function fillDraftFromRow(row: PageRow | null) {
    const next: Record<string, string> = {};
    for (const column of allDataColumns.value) {
      if (row) next[column.key] = String(row[column.key] ?? '');
      else if (column.key === 'status')
        next[column.key] = model.value.tabs.find((tab) => !tab.includes('全部')) ?? '草稿';
      else if (column.key === 'owner') next[column.key] = '张经理';
      else next[column.key] = '';
    }
    if (!next.name && !next.title) {
      next.name = '';
      next.title = '';
    }
    draft.value = next;
  }

  function openCreate() {
    activeRow.value = null;
    fillDraftFromRow(null);
    createOpen.value = true;
  }

  function openEdit(row: PageRow) {
    activeRow.value = row;
    fillDraftFromRow(row);
    createOpen.value = true;
  }

  function openCopy(row: PageRow) {
    activeRow.value = null;
    fillDraftFromRow(row);
    const titleKey = draft.value.name !== undefined ? 'name' : draft.value.title !== undefined ? 'title' : null;
    if (titleKey) draft.value[titleKey] = `${draft.value[titleKey] || model.value.label}（副本）`;
    if (draft.value.code) draft.value.code = `${draft.value.code}-COPY`;
    createOpen.value = true;
  }

  function askDelete(row?: PageRow) {
    if (row) {
      activeRow.value = row;
      pendingAction.value = `删除「${String(row.name || row.title || row.code || row.id)}」`;
    } else {
      activeRow.value = null;
      pendingAction.value = selectedCount.value ? `删除所选 ${selectedCount.value} 条` : '删除所选';
    }
    confirmOpen.value = true;
  }

  function runTool(tool: string) {
    const action = resolveScaffoldAction(tool, model.value);

    switch (action.kind) {
      case 'configure':
        columnOpen.value = true;
        return;
      case 'import':
        importOpen.value = true;
        return;
      case 'export':
      case 'print': {
        const count = selectedCount.value || filteredRows.value.length;
        notify(`已导出 ${count} 条「${model.value.label}」演示数据`);
        return;
      }
      case 'refresh':
      case 'reset':
        resetFromModel(model.value);
        notify(action.kind === 'reset' ? '已重置为初始演示数据' : '已刷新本地数据');
        return;
      case 'delete':
        pendingAction.value = action.label;
        confirmOpen.value = true;
        return;
      case 'create':
        openCreate();
        return;
      case 'batch':
        notify(selectedCount.value ? `已对 ${selectedCount.value} 条执行：${action.label}` : '请先勾选记录');
        return;
      default:
        notify(`已执行：${action.label}`);
    }
  }

  function createRecord() {
    const now = new Date().toISOString().slice(0, 16).replace('T', ' ');
    const title =
      draft.value.name?.trim() || draft.value.title?.trim() || draft.value.code?.trim() || `新建${model.value.label}`;

    if (activeRow.value) {
      const id = String(activeRow.value.id);
      rows.value = rows.value.map((row) => {
        if (String(row.id) !== id) return row;
        const next: PageRow = { ...row, ...draft.value, id, updatedAt: now };
        if (!next.name) next.name = title;
        if (!next.title) next.title = title;
        return next;
      });
      createOpen.value = false;
      notify('已更新本地演示记录');
      return;
    }

    const id = `${model.value.pageSlug}-${Date.now()}`;
    const next: PageRow = {
      id,
      code:
        draft.value.code ||
        `${model.value.pageSlug.slice(0, 3).toUpperCase()}-${String(rows.value.length + 1).padStart(4, '0')}`,
      name: title,
      title,
      status: draft.value.status || '草稿',
      owner: draft.value.owner || '张经理',
      type: draft.value.type || model.value.label,
      updatedAt: now,
      ...draft.value,
    };
    rows.value = [next, ...rows.value];
    createOpen.value = false;
    pageIndex.value = 1;
    notify('已保存本地演示记录');
  }

  function confirmPending() {
    const action = pendingAction.value;
    if (action.includes('删除')) {
      if (activeRow.value) {
        const id = String(activeRow.value.id);
        rows.value = rows.value.filter((row) => String(row.id) !== id);
        selected.value = new Set([...selected.value].filter((item) => item !== id));
      } else if (selected.value.size) {
        rows.value = rows.value.filter((row) => !selected.value.has(String(row.id)));
        selected.value = new Set();
      }
    }
    confirmOpen.value = false;
    activeRow.value = null;
    pendingAction.value = '';
    notify(`已确认：${action}`);
  }

  function importDemoRows() {
    const stamp = Date.now();
    const imported: PageRow[] = Array.from({ length: 3 }, (_, index) => {
      const id = `${model.value.pageSlug}-imp-${stamp}-${index + 1}`;
      const base: PageRow = {
        id,
        code: `IMP-${String(index + 1).padStart(3, '0')}`,
        name: `导入${model.value.label}-${index + 1}`,
        title: `导入${model.value.label}-${index + 1}`,
        status: '草稿',
        owner: '张经理',
        type: model.value.label,
        updatedAt: new Date().toISOString().slice(0, 16).replace('T', ' '),
      };
      for (const column of allDataColumns.value) {
        if (base[column.key] == null) {
          if (column.key.includes('amount') || column.title.includes('金额') || column.title.includes('价')) {
            base[column.key] = `¥${(1000 * (index + 1)).toLocaleString('zh-CN')}`;
          } else if (column.key === 'status') base[column.key] = '草稿';
          else if (column.key === 'owner') base[column.key] = '张经理';
          else base[column.key] = `${column.title}${index + 1}`;
        }
      }
      return base;
    });
    rows.value = [...imported, ...rows.value];
    importOpen.value = false;
    pageIndex.value = 1;
    notify(`已导入 ${imported.length} 条演示记录`);
  }

  function toggleColumn(key: string) {
    const visibleCount = Object.values(columnVisibility.value).filter(Boolean).length;
    if (columnVisibility.value[key] && visibleCount <= 2) {
      notify('至少保留两列可见');
      return;
    }
    columnVisibility.value = {
      ...columnVisibility.value,
      [key]: !columnVisibility.value[key],
    };
  }

  function showAllColumns() {
    columnVisibility.value = Object.fromEntries(allDataColumns.value.map((column) => [column.key, true]));
  }

  function resetColumns() {
    columnVisibility.value = Object.fromEntries(
      allDataColumns.value.map((column) => [column.key, column.visible !== false]),
    );
  }

  function setPage(next: number) {
    pageIndex.value = Math.min(Math.max(1, next), totalPages.value);
  }

  function setPageSize(size: number) {
    pageSize.value = size;
    pageIndex.value = 1;
  }

  function clickStat(key: string) {
    activeStatKey.value = activeStatKey.value === key ? null : key;
    pageIndex.value = 1;
  }

  function resetFilters() {
    query.value = '';
    activeTab.value = model.value.tabs[0] ?? '全部';
    filterValues.value = Object.fromEntries(
      model.value.filters.map((filter: PageFilter) => [filter.key, filter.options[0] ?? '全部']),
    );
    activeStatKey.value = null;
    sortKey.value = '';
    sortDir.value = 'asc';
    pageIndex.value = 1;
    notify('已重置筛选');
  }

  function formatCell(row: PageRow, key: string) {
    const value = row[key];
    if (value == null || value === '') return '—';
    if (key.includes('amount') || key === 'value' || key.includes('Amt') || key.includes('Price')) {
      const num = typeof value === 'number' ? value : Number(String(value).replace(/[^\d.-]/g, ''));
      if (!Number.isNaN(num) && String(value).match(/^\d/)) {
        return `¥${num.toLocaleString('zh-CN', { maximumFractionDigits: 2 })}`;
      }
    }
    return String(value);
  }

  function statusTone(status: string) {
    if (/停用|驳回|逾期|失败|危险|红|关闭|作废/.test(status)) return 'danger';
    if (/待|审批|草稿|黄|中|部分|执行中/.test(status)) return 'warning';
    if (/完成|启用|生效|成功|绿|通过|已发|已收|已结/.test(status)) return 'success';
    return 'neutral';
  }

  function primaryFieldKeys() {
    const preferred = ['name', 'title', 'code', 'customer', 'party', 'owner', 'status'];
    const keys = allDataColumns.value.map((column) => column.key);
    return preferred
      .filter((key) => keys.includes(key))
      .concat(keys.filter((key) => !preferred.includes(key)))
      .slice(0, 8);
  }

  return {
    rows,
    selected,
    activeTab,
    query,
    filterValues,
    detailOpen,
    createOpen,
    confirmOpen,
    columnOpen,
    importOpen,
    pendingAction,
    activeRow,
    draft,
    sortKey,
    sortDir,
    pageIndex,
    pageSize,
    columnVisibility,
    activeStatKey,
    filteredRows,
    pagedRows,
    totalPages,
    dataColumns,
    allDataColumns,
    rowActions,
    allSelected,
    selectedCount,
    selectedRows,
    notify,
    toggleRow,
    toggleAll,
    setSort,
    openDetail,
    openCreate,
    openEdit,
    openCopy,
    askDelete,
    runTool,
    createRecord,
    confirmPending,
    importDemoRows,
    toggleColumn,
    showAllColumns,
    resetColumns,
    setPage,
    setPageSize,
    clickStat,
    resetFilters,
    formatCell,
    statusTone,
    primaryFieldKeys,
  };
}

export type ScaffoldPageState = ReturnType<typeof useScaffoldPageState>;
