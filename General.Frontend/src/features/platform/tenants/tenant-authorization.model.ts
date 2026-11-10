import type { MenuApi } from '@/api/core/menu';
import type { TenantApi } from '@/api/core/tenant';

export type CheckedState = boolean | 'indeterminate';

function sortStrings(values: Iterable<string>): string[] {
  const sorted = [...values];
  // 这里排序的是新建的私有数组，不会改变调用方集合。
  // oxlint-disable-next-line unicorn(no-array-sort)
  sorted.sort((left, right) => left.localeCompare(right));
  return sorted;
}

function findMenuNode(tree: readonly MenuApi.PermissionTreeItem[], id: string): MenuApi.PermissionTreeItem | null {
  for (const node of tree) {
    if (node.id === id) return node;
    const nested = findMenuNode(node.children, id);
    if (nested) return nested;
  }
  return null;
}

function collectEnabledIds(node: MenuApi.PermissionTreeItem): string[] {
  if (!node.isEnabled) return [];
  return [node.id, ...node.children.flatMap(collectEnabledIds)];
}

export function toggleTenantMenuSelection(
  tree: readonly MenuApi.PermissionTreeItem[],
  selectedIds: ReadonlySet<string>,
  id: string,
  selected: boolean,
): Set<string> {
  const node = findMenuNode(tree, id);
  if (!node || !node.isEnabled) return new Set(selectedIds);

  const next = new Set(selectedIds);
  for (const menuId of collectEnabledIds(node)) {
    if (selected) next.add(menuId);
    else next.delete(menuId);
  }
  return next;
}

export function getTenantMenuCheckedState(
  node: MenuApi.PermissionTreeItem,
  selectedIds: ReadonlySet<string>,
): CheckedState {
  if (!node.isEnabled) return false;
  const enabledIds = collectEnabledIds(node);
  const selectedCount = enabledIds.filter((id) => selectedIds.has(id)).length;
  if (selectedCount === 0) return false;
  return selectedCount === enabledIds.length ? true : 'indeterminate';
}

export function getKnownTenantMenuIds(tree: readonly MenuApi.PermissionTreeItem[]): Set<string> {
  return new Set(tree.flatMap(collectEnabledIds));
}

export function getTenantMenuUnknownCount(
  tree: readonly MenuApi.PermissionTreeItem[],
  selectedIds: ReadonlySet<string>,
): number {
  const known = getKnownTenantMenuIds(tree);
  return [...selectedIds].filter((id) => !known.has(id)).length;
}

export function toggleTenantApiBlacklist(
  selectedKeys: ReadonlySet<string>,
  key: string,
  selected: boolean,
): Set<string> {
  const next = new Set(selectedKeys);
  if (selected) next.add(key);
  else next.delete(key);
  return next;
}

export function getKnownTenantEndpointKeys(groups: readonly MenuApi.EndpointGroup[]): Set<string> {
  const result = new Set<string>();
  for (const group of groups) {
    for (const item of group.items) {
      result.add(item.key);
      if (item.capabilityKey) result.add(item.capabilityKey);
    }
  }
  return result;
}

export function getTenantEndpointUnknownKeys(
  groups: readonly MenuApi.EndpointGroup[],
  selectedKeys: ReadonlySet<string>,
): string[] {
  const known = getKnownTenantEndpointKeys(groups);
  return sortStrings([...selectedKeys].filter((key) => !known.has(key)));
}

export function toTenantAuthorizationInput(
  current: TenantApi.Authorization,
  selectedMenuIds: ReadonlySet<string>,
  selectedApiBlacklist: ReadonlySet<string>,
  remark: string,
  adminUserId = current.adminUserId,
): TenantApi.Authorization {
  return {
    adminUserId,
    apiBlacklist: sortStrings(selectedApiBlacklist),
    isActive: current.isActive,
    menuIds: sortStrings(selectedMenuIds),
    remark: remark.trim() || null,
  };
}
