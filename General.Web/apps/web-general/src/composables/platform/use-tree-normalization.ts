export interface PlatformTreeNode {
  children?: PlatformTreeNode[];
  key: string;
  title: string;
  value?: string;
}

export function normalizePlatformTree<T extends { children?: T[]; id: string }>(
  items: T[],
  resolveTitle: (item: T) => string,
): PlatformTreeNode[] {
  return items.map((item) => ({
    children: normalizePlatformTree(item.children || [], resolveTitle),
    key: item.id,
    title: resolveTitle(item),
  }));
}

export function normalizePlatformTreeSelect<T extends { children?: T[]; id: string }>(
  items: T[],
  resolveTitle: (item: T) => string,
): PlatformTreeNode[] {
  return items.map((item) => ({
    children: normalizePlatformTreeSelect(item.children || [], resolveTitle),
    key: item.id,
    title: resolveTitle(item),
    value: item.id,
  }));
}

export function collectPlatformLeafKeys(nodes: PlatformTreeNode[]): Set<string> {
  const keys = new Set<string>();
  for (const node of nodes) {
    if (node.children && node.children.length > 0) {
      for (const key of collectPlatformLeafKeys(node.children)) {
        keys.add(key);
      }
    } else {
      keys.add(node.key);
    }
  }
  return keys;
}
