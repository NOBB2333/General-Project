import { computed, ref } from 'vue';
import {
  createMenuApi,
  deleteMenuApi,
  getMenuPermissionTreeApi,
  setMenuEnabledApi,
  updateMenuApi,
} from '@/api/core/menu';
import type { MenuApi } from '@/api/core/menu';
import { toast } from '@/components/ui';
import {
  appCodeOptions,
  createMenuDraft,
  flattenMenuTree,
  getMenuDescendantIds,
  menuTypeOptions,
  setMenuDraftType,
  toMenuDraft,
  toMenuSaveInput,
  validateMenuDraft,
} from './menu.model';
import type { MenuDraft, PlatformMenuRow } from './menu.model';

const allAppCodesOption = { value: 'all', label: '全部应用' };

export function usePlatformMenusPage() {
  const tree = ref<MenuApi.PermissionTreeItem[]>([]);
  const search = ref('');
  const appCodeFilter = ref('all');
  const expandedIds = ref(new Set<string>());
  const isLoading = ref(false);
  const loadError = ref('');
  const formOpen = ref(false);
  const formMode = ref<'create' | 'edit'>('create');
  const editingId = ref<string | null>(null);
  const draft = ref<MenuDraft>(createMenuDraft());
  const formSaving = ref(false);
  const formError = ref('');
  const deleteTarget = ref<MenuApi.PermissionTreeItem | null>(null);
  const deleteSaving = ref(false);
  const savingEnabledId = ref<string | null>(null);

  const rows = computed(() => flattenMenuTree(tree.value));
  const parentIds = computed(() => new Set(rows.value.filter((row) => row.children.length > 0).map((row) => row.id)));
  const searchable = computed(() => search.value.trim().length > 0);
  const filteredRows = computed(() => {
    const query = search.value.trim().toLocaleLowerCase();
    return rows.value.filter((row) => {
      if (appCodeFilter.value !== 'all' && row.appCode.toLowerCase() !== appCodeFilter.value) return false;
      if (!query) return true;
      return [row.appCode, row.component, row.name, row.path, row.permissionCode, row.title, row.link, row.redirect]
        .filter((value): value is string => Boolean(value))
        .some((value) => value.toLocaleLowerCase().includes(query));
    });
  });
  const visibleRows = computed(() => {
    if (searchable.value) return filteredRows.value;

    const visible: PlatformMenuRow[] = [];
    const visit = (nodes: readonly MenuApi.PermissionTreeItem[], depth: number) => {
      for (const node of nodes) {
        if (appCodeFilter.value !== 'all' && node.appCode.toLowerCase() !== appCodeFilter.value) continue;
        visible.push({ ...node, depth });
        if (expandedIds.value.has(node.id)) visit(node.children, depth + 1);
      }
    };
    visit(tree.value, 0);
    return visible;
  });
  const filterOptions = computed(() => {
    const known = new Set<string>(appCodeOptions.map((option) => option.value));
    const custom = [...new Set(rows.value.map((row) => row.appCode))].filter((code) => !known.has(code));
    return [allAppCodesOption, ...appCodeOptions, ...custom.map((value) => ({ value, label: `其他应用 · ${value}` }))];
  });
  const parentOptions = computed(() => {
    const current = editingId.value ? rows.value.find((row) => row.id === editingId.value) : undefined;
    const excluded = current ? getMenuDescendantIds(current) : new Set<string>();
    return [
      { value: '', label: '无（顶级节点）' },
      ...rows.value
        .filter(
          (row) =>
            row.type !== 3 &&
            row.id !== editingId.value &&
            !excluded.has(row.id) &&
            row.appCode.toLowerCase() === draft.value.appCode.trim().toLowerCase(),
        )
        .map((row) => ({
          value: row.id,
          label: `${'　'.repeat(row.depth)}${row.title} · ${row.name}`,
        })),
    ];
  });
  const descendantCount = computed(() => (deleteTarget.value ? getMenuDescendantIds(deleteTarget.value).size : 0));
  const metrics = computed(
    () =>
      [
        {
          id: 'catalog',
          label: '目录节点',
          value: rows.value.filter((row) => row.type === 1).length,
          icon: 'catalog',
          tone: 'primary',
        },
        { id: 'total', label: '全部节点', value: rows.value.length, icon: 'total', tone: 'purple' },
        {
          id: 'pages',
          label: '菜单页面',
          value: rows.value.filter((row) => row.type === 2).length,
          icon: 'pages',
          tone: 'success',
        },
        {
          id: 'actions',
          label: '按钮权限',
          value: rows.value.filter((row) => row.type === 3).length,
          icon: 'actions',
          tone: 'warning',
        },
      ] as const,
  );

  async function loadMenus() {
    isLoading.value = true;
    loadError.value = '';
    try {
      const nextTree = await getMenuPermissionTreeApi();
      tree.value = nextTree;
      expandedIds.value = new Set(
        flattenMenuTree(nextTree)
          .filter((row) => row.children.length > 0)
          .map((row) => row.id),
      );
    } catch (error) {
      loadError.value = errorMessage(error, '菜单读取失败，请重试。');
      toast.error(loadError.value);
    } finally {
      isLoading.value = false;
    }
  }

  function toggleExpanded(menuId: string) {
    const next = new Set(expandedIds.value);
    if (next.has(menuId)) next.delete(menuId);
    else next.add(menuId);
    expandedIds.value = next;
  }

  function expandAll() {
    expandedIds.value = new Set(parentIds.value);
  }

  function collapseAll() {
    expandedIds.value = new Set();
  }

  function openCreate(parent?: MenuApi.PermissionTreeItem) {
    formMode.value = 'create';
    editingId.value = null;
    draft.value = createMenuDraft(parent);
    formError.value = '';
    formOpen.value = true;
  }

  function openEdit(menu: MenuApi.PermissionTreeItem) {
    formMode.value = 'edit';
    editingId.value = menu.id;
    draft.value = toMenuDraft(menu);
    formError.value = '';
    formOpen.value = true;
  }

  function changeType(typeValue: string) {
    const type = Number.parseInt(typeValue, 10);
    if (type === 1 || type === 2 || type === 3) setMenuDraftType(draft.value, type);
  }

  async function saveMenu() {
    const validation = validateMenuDraft(draft.value, tree.value, editingId.value ?? undefined);
    if (validation) {
      formError.value = validation;
      toast.warning(validation);
      return;
    }

    const input = toMenuSaveInput(draft.value);
    formSaving.value = true;
    formError.value = '';
    try {
      if (editingId.value) await updateMenuApi(editingId.value, input);
      else await createMenuApi(input);
      formOpen.value = false;
      toast.success(
        formMode.value === 'create' ? '菜单已创建。刷新页面后导航将同步。' : '菜单已更新。刷新页面后导航将同步。',
      );
      await loadMenus();
    } catch (error) {
      formError.value = errorMessage(error, '菜单保存失败，请检查字段或权限后重试。');
      toast.error(formError.value);
    } finally {
      formSaving.value = false;
    }
  }

  async function setEnabled(menu: MenuApi.PermissionTreeItem, isEnabled: boolean) {
    savingEnabledId.value = menu.id;
    try {
      await setMenuEnabledApi(menu.id, isEnabled);
      toast.success(isEnabled ? '菜单已启用。刷新页面后导航将同步。' : '菜单已停用。刷新页面后导航将同步。');
      await loadMenus();
    } catch (error) {
      toast.error(errorMessage(error, '菜单状态更新失败。'));
      await loadMenus();
    } finally {
      savingEnabledId.value = null;
    }
  }

  function requestDelete(menu: MenuApi.PermissionTreeItem) {
    deleteTarget.value = menu;
  }

  async function confirmDelete() {
    if (!deleteTarget.value) return;
    const menu = deleteTarget.value;
    deleteSaving.value = true;
    try {
      await deleteMenuApi(menu.id);
      deleteTarget.value = null;
      toast.success('菜单节点及其子节点已删除。刷新页面后导航将同步。');
      await loadMenus();
    } catch (error) {
      toast.error(errorMessage(error, '菜单删除失败。'));
      await loadMenus();
    } finally {
      deleteSaving.value = false;
    }
  }

  return {
    appCodeFilter,
    changeType,
    collapseAll,
    confirmDelete,
    deleteSaving,
    deleteTarget,
    descendantCount,
    draft,
    expandAll,
    expandedIds,
    filterOptions,
    formError,
    formMode,
    formOpen,
    formSaving,
    isLoading,
    loadError,
    loadMenus,
    menuTypeOptions,
    metrics,
    openCreate,
    openEdit,
    parentIds,
    parentOptions,
    requestDelete,
    saveMenu,
    savingEnabledId,
    search,
    searchable,
    setEnabled,
    toggleExpanded,
    visibleRows,
  };
}

function errorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim() ? error.message : fallback;
}
