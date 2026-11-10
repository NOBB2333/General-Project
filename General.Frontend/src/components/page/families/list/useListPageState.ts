import { computed, ref, toValue, watch, type MaybeRefOrGetter } from 'vue';
import { cloneReferenceRows } from '../../reference/reference-fixture.ts';
import type { ListPageModel, ListPageRow } from './list-page.types';

export function useListPageState<TRow extends ListPageRow>(model: MaybeRefOrGetter<ListPageModel<TRow>>) {
  const rows = ref<TRow[]>([]);
  const query = ref('');
  const activeScopeId = ref('');
  const activeViewId = ref('');
  const activeTabId = ref('');
  const filterValues = ref<Record<string, string>>({});
  const selected = ref(new Set<string>());
  const pageNumber = ref(1);

  function resetFromModel(next: ListPageModel<TRow>) {
    rows.value = cloneReferenceRows(next.rows);
    query.value = '';
    activeScopeId.value = next.listScopes[0]?.id ?? '';
    activeViewId.value = next.listViews[0]?.id ?? '';
    activeTabId.value = next.listTabs[0]?.id ?? '';
    filterValues.value = Object.fromEntries(next.filters.map((filter) => [filter.key, filter.options[0] ?? '']));
    selected.value = new Set();
    pageNumber.value = 1;
  }

  watch(
    () => toValue(model),
    (next, previous) => {
      if (!previous || previous.key !== next.key || previous.path !== next.path) {
        resetFromModel(next);
        return;
      }
      // 同页刷新只替换数据，保留用户的搜索与筛选；删除的行不再保留选择。
      rows.value = cloneReferenceRows(next.rows);
      const ids = new Set(next.rows.map((row) => row.id));
      selected.value = new Set([...selected.value].filter((id) => ids.has(id)));
    },
    { immediate: true },
  );

  const activeScope = computed(() => toValue(model).listScopes.find((scope) => scope.id === activeScopeId.value));
  const activeTab = computed(() => toValue(model).listTabs.find((tab) => tab.id === activeTabId.value));
  const filteredRows = computed(() => {
    const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
    return rows.value.filter((row) => {
      if (
        activeScope.value?.filterKey &&
        activeScope.value.filterValue &&
        String(row[activeScope.value.filterKey] ?? '') !== activeScope.value.filterValue
      ) {
        return false;
      }
      if (
        activeTab.value?.filterKey &&
        activeTab.value.filterValue &&
        String(row[activeTab.value.filterKey] ?? '') !== activeTab.value.filterValue
      ) {
        return false;
      }
      if (
        toValue(model).filters.some((filter) => {
          const selectedValue = filterValues.value[filter.key];
          if (!selectedValue || selectedValue.startsWith('全部') || selectedValue === '所有') return false;
          return String(row[filter.key] ?? '') !== selectedValue;
        })
      ) {
        return false;
      }
      return (
        !normalized ||
        Object.values(row).some((value) =>
          String(value ?? '')
            .toLocaleLowerCase('zh-CN')
            .includes(normalized),
        )
      );
    });
  });

  const pageCount = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / toValue(model).pageSize)));
  const visibleRows = computed(() => {
    const start = (pageNumber.value - 1) * toValue(model).pageSize;
    return filteredRows.value.slice(start, start + toValue(model).pageSize);
  });
  watch(query, () => {
    pageNumber.value = 1;
  });
  watch(pageCount, (count) => {
    pageNumber.value = Math.min(pageNumber.value, count);
  });
  const selectedCount = computed(() => selected.value.size);
  const allVisibleSelected = computed(
    () => visibleRows.value.length > 0 && visibleRows.value.every((row) => selected.value.has(row.id)),
  );

  function toggleRow(id: string) {
    const next = new Set(selected.value);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selected.value = next;
  }

  function toggleAllVisible() {
    const next = new Set(selected.value);
    if (allVisibleSelected.value) visibleRows.value.forEach((row) => next.delete(row.id));
    else visibleRows.value.forEach((row) => next.add(row.id));
    selected.value = next;
  }

  function setActiveTab(id: string) {
    activeTabId.value = id;
    pageNumber.value = 1;
  }

  function setActiveScope(id: string) {
    activeScopeId.value = id;
    pageNumber.value = 1;
  }

  function setActiveView(id: string) {
    activeViewId.value = id;
  }

  function setFilter(key: string, value: string) {
    filterValues.value = { ...filterValues.value, [key]: value };
    pageNumber.value = 1;
  }

  return {
    rows,
    query,
    activeScopeId,
    activeViewId,
    activeTabId,
    filterValues,
    selected,
    filteredRows,
    visibleRows,
    selectedCount,
    allVisibleSelected,
    pageNumber,
    pageCount,
    toggleRow,
    toggleAllVisible,
    setActiveScope,
    setActiveView,
    setActiveTab,
    setFilter,
  };
}
