import { computed, ref, watch, type Ref } from 'vue';
import { toast } from '@/components/ui';
import { cloneReferenceRows } from '../../reference';
import type { PageRow } from '../../page.types';
import type { SearchPageModel } from './search-page.types';

export function useSearchPageState(model: Ref<SearchPageModel>, referenceRows: Ref<readonly PageRow[]>) {
  const catalogRows = ref<PageRow[]>([]);
  const recentRows = ref<PageRow[]>([]);
  const query = ref('');
  const submittedQuery = ref('');
  const activeRow = ref<PageRow | null>(null);
  const detailOpen = ref(false);

  function resetFromModel(next: SearchPageModel, nextRows: readonly PageRow[]) {
    catalogRows.value = cloneReferenceRows(nextRows);
    const recentIds = new Set(next.initialRecentIds);
    recentRows.value = cloneReferenceRows(nextRows.filter((row) => recentIds.has(String(row.id))));
    query.value = '';
    submittedQuery.value = '';
    activeRow.value = null;
    detailOpen.value = false;
  }

  watch(
    () => [model.value, referenceRows.value] as const,
    ([next, nextRows]) => resetFromModel(next, nextRows),
    { immediate: true },
  );

  const hasSubmitted = computed(() => Boolean(submittedQuery.value));

  const visibleRows = computed(() => {
    if (!submittedQuery.value) return recentRows.value;
    const normalized = submittedQuery.value.toLocaleLowerCase('zh-CN');
    return catalogRows.value.filter((row) =>
      Object.values(row).some((value) =>
        String(value ?? '')
          .toLocaleLowerCase('zh-CN')
          .includes(normalized),
      ),
    );
  });

  const resultTitle = computed(() =>
    hasSubmitted.value ? `搜索结果 · ${visibleRows.value.length}` : model.value.copy.recentTitle,
  );

  function submitSearch() {
    const next = query.value.trim();
    if (!next) {
      submittedQuery.value = '';
      toast.warning(model.value.copy.queryRequiredMessage);
      return;
    }
    submittedQuery.value = next;
    toast.success(`找到 ${visibleRows.value.length} 条匹配${model.value.copy.entityLabel}`);
  }

  function resetSearch() {
    query.value = '';
    submittedQuery.value = '';
  }

  function submitSuggestion(value: string) {
    query.value = value;
    submitSearch();
  }

  function clearHistory() {
    recentRows.value = [];
    resetSearch();
    toast.info('已清空本地最近查看记录');
  }

  function openDetail(row: PageRow) {
    activeRow.value = row;
    detailOpen.value = true;
    recentRows.value = [row, ...recentRows.value.filter((item) => String(item.id) !== String(row.id))];
  }

  return {
    query,
    activeRow,
    detailOpen,
    hasSubmitted,
    visibleRows,
    resultTitle,
    submitSearch,
    submitSuggestion,
    resetSearch,
    clearHistory,
    openDetail,
  };
}
