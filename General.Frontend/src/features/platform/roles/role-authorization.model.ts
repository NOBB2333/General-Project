import type { MenuApi } from '@/api/core/menu';
import type { UiSelectOption } from '@/components/ui';

export type MenuCheckedState = boolean | 'indeterminate';
export type DataScopeMode = 'all' | 'current_org' | 'current_org_and_descendants' | 'custom' | 'self';
export type AccountScopeMode = 'all' | 'data' | 'data_and_users' | 'only_users';

export const dataScopeOptions: readonly UiSelectOption[] = [
  { value: 'current_org_and_descendants', label: '本人所属组织及下级组织' },
  { value: 'current_org', label: '仅本人所属组织' },
  { value: 'self', label: '本人所属组织节点' },
  { value: 'custom', label: '自定义组织范围' },
  { value: 'all', label: '全部组织' },
];

export const accountScopeOptions: readonly UiSelectOption[] = [
  { value: 'data_and_users', label: '数据范围内用户 + 指定用户' },
  { value: 'data', label: '仅数据范围内用户' },
  { value: 'only_users', label: '仅指定用户' },
  { value: 'all', label: '全部用户' },
];

const builtInRoleNames = new Set(['admin', 'pmo', 'pm', 'member', 'viewer']);

export function isBuiltInRoleName(name: string): boolean {
  return builtInRoleNames.has(name.trim().toLocaleLowerCase());
}

export function resolveRoleMenuSelection(
  storedMenuIds: readonly string[],
  fallbackMenuIds: readonly string[],
): string[] {
  return [...new Set(storedMenuIds.length > 0 ? storedMenuIds : fallbackMenuIds)];
}

function collectEnabledMenuIds(node: MenuApi.PermissionTreeItem): string[] {
  return [...(node.isEnabled ? [node.id] : []), ...node.children.flatMap((child) => collectEnabledMenuIds(child))];
}

function findMenuNode(tree: readonly MenuApi.PermissionTreeItem[], menuId: string): MenuApi.PermissionTreeItem | null {
  for (const node of tree) {
    if (node.id === menuId) return node;
    const child = findMenuNode(node.children, menuId);
    if (child) return child;
  }
  return null;
}

export function getRoleMenuCheckedState(
  node: MenuApi.PermissionTreeItem,
  selectedIds: ReadonlySet<string>,
): MenuCheckedState {
  const selectableIds = collectEnabledMenuIds(node);
  if (selectableIds.length === 0) return selectedIds.has(node.id);

  const selectedCount = selectableIds.filter((id) => selectedIds.has(id)).length;
  if (selectedCount === 0) return false;
  if (selectedCount === selectableIds.length) return true;
  return 'indeterminate';
}

export function toggleRoleMenuSelection(
  tree: readonly MenuApi.PermissionTreeItem[],
  selectedIds: ReadonlySet<string>,
  menuId: string,
  checked: boolean,
): Set<string> {
  const node = findMenuNode(tree, menuId);
  if (!node || !node.isEnabled) return new Set(selectedIds);

  const next = new Set(selectedIds);
  for (const id of collectEnabledMenuIds(node)) {
    if (checked) next.add(id);
    else next.delete(id);
  }
  return next;
}
