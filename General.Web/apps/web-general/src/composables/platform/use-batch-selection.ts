import { computed, ref } from 'vue';

export function useBatchSelection<T = string>() {
  const selectedRowKeys = ref<T[]>([]);

  const rowSelection = computed(() => ({
    selectedRowKeys: selectedRowKeys.value,
    onChange: (keys: T[]) => {
      selectedRowKeys.value = keys;
    },
  }));

  function clearSelection() {
    selectedRowKeys.value = [];
  }

  return {
    clearSelection,
    rowSelection,
    selectedRowKeys,
  };
}
