import { ref } from 'vue';
import type { PageRow } from '../page.types';

export function useScaffoldOverlayState() {
  const detailOpen = ref(false);
  const createOpen = ref(false);
  const confirmOpen = ref(false);
  const columnOpen = ref(false);
  const importOpen = ref(false);
  const pendingAction = ref('');
  const activeRow = ref<PageRow | null>(null);
  const draft = ref<Record<string, string>>({});

  function resetOverlayState() {
    detailOpen.value = false;
    createOpen.value = false;
    confirmOpen.value = false;
    columnOpen.value = false;
    importOpen.value = false;
    pendingAction.value = '';
    activeRow.value = null;
    draft.value = {};
  }

  return {
    detailOpen,
    createOpen,
    confirmOpen,
    columnOpen,
    importOpen,
    pendingAction,
    activeRow,
    draft,
    resetOverlayState,
  };
}
