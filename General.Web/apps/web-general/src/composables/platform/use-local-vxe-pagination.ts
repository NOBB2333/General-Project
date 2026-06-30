import { computed, ref, watch } from 'vue';

export function useLocalVxePagination<T>(
  rows: { readonly value: T[] },
  options: { defaultPageSize?: number; pageSizes?: number[] } = {},
) {
  const pageIndex = ref(1);
  const pageSize = ref(options.defaultPageSize ?? 10);
  const pageSizes = options.pageSizes ?? [10, 20, 50, 100];

  const total = computed(() => rows.value.length);
  const pagedRows = computed(() => {
    const start = (pageIndex.value - 1) * pageSize.value;
    return rows.value.slice(start, start + pageSize.value);
  });

  const pagerConfig = computed(() => ({
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    pageSizes,
    total: total.value,
  }));

  function handlePageChange(event: {
    currentPage?: number;
    pageSize?: number;
  }) {
    if (event.pageSize && event.pageSize !== pageSize.value) {
      pageSize.value = event.pageSize;
      pageIndex.value = 1;
      return;
    }

    pageIndex.value = event.currentPage || 1;
  }

  function resetPage() {
    pageIndex.value = 1;
  }

  watch(rows, () => {
    pageIndex.value = 1;
  });

  watch(total, () => {
    const maxPage = Math.max(1, Math.ceil(total.value / pageSize.value));
    if (pageIndex.value > maxPage) {
      pageIndex.value = maxPage;
    }
  });

  return {
    handlePageChange,
    pageIndex,
    pageSize,
    pagedRows,
    pagerConfig,
    resetPage,
    total,
  };
}
