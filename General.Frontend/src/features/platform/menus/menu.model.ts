import type { MenuApi } from '@/api/core/menu';
import { menuCatalogRegistry, pageRegistry } from '../../../navigation/page-registry.ts';

export interface PlatformMenuRow extends MenuApi.PermissionTreeItem {
  depth: number;
}

export interface MenuDraft {
  affixTab: boolean;
  appCode: string;
  component: string;
  hideInBreadcrumb: boolean;
  hideInMenu: boolean;
  hideInTab: boolean;
  icon: string;
  isEnabled: boolean;
  keepAlive: boolean;
  link: string;
  menuVisibleWithForbidden: boolean;
  name: string;
  order: string;
  parentId: string;
  path: string;
  permissionCode: string;
  redirect: string;
  title: string;
  type: MenuApi.PermissionTreeItem['type'];
}

export interface MenuCompatibility {
  kind: 'action' | 'external' | 'registered' | 'unsupported';
  label: string;
  detail: string;
}

export const appCodeOptions = [
  { value: 'platform', label: '平台中心 · platform' },
  { value: 'project', label: '项目执行 · project' },
  { value: 'business', label: '经营管理 · business' },
  { value: 'integration', label: '外部集成 · integration' },
  { value: 'ai', label: 'AI 管理 · ai' },
] as const;

export const menuTypeOptions = [
  { value: '1', label: '目录（Catalog）' },
  { value: '2', label: '菜单页（Menu）' },
  { value: '3', label: '按钮权限（Button）' },
] as const;

export function createMenuDraft(parent?: MenuApi.PermissionTreeItem): MenuDraft {
  return {
    affixTab: false,
    appCode: parent?.appCode ?? 'platform',
    component: '',
    hideInBreadcrumb: false,
    hideInMenu: false,
    hideInTab: false,
    icon: '',
    isEnabled: true,
    keepAlive: true,
    link: '',
    menuVisibleWithForbidden: false,
    name: '',
    order: '10',
    parentId: parent?.id ?? '',
    path: '',
    permissionCode: '',
    redirect: '',
    title: '',
    type: 2,
  };
}

export function toMenuDraft(menu: MenuApi.PermissionTreeItem): MenuDraft {
  return {
    affixTab: menu.affixTab,
    appCode: menu.appCode,
    component: menu.component ?? '',
    hideInBreadcrumb: menu.hideInBreadcrumb,
    hideInMenu: menu.hideInMenu,
    hideInTab: menu.hideInTab,
    icon: menu.icon ?? '',
    isEnabled: menu.isEnabled,
    keepAlive: menu.keepAlive,
    link: menu.link ?? '',
    menuVisibleWithForbidden: menu.menuVisibleWithForbidden,
    name: menu.name,
    order: String(menu.order),
    parentId: menu.parentId ?? '',
    path: menu.path,
    permissionCode: menu.permissionCode ?? '',
    redirect: menu.redirect ?? '',
    title: menu.title,
    type: menu.type,
  };
}

export function toMenuSaveInput(draft: MenuDraft): MenuApi.SaveInput {
  return {
    affixTab: draft.affixTab,
    appCode: draft.appCode.trim(),
    component: draft.type === 1 ? 'BasicLayout' : draft.type === 3 ? null : normalizeOptional(draft.component),
    hideInBreadcrumb: draft.hideInBreadcrumb,
    hideInMenu: draft.hideInMenu,
    hideInTab: draft.hideInTab,
    icon: normalizeOptional(draft.icon),
    isEnabled: draft.isEnabled,
    keepAlive: draft.keepAlive,
    link: draft.link.trim(),
    menuVisibleWithForbidden: draft.menuVisibleWithForbidden,
    name: draft.name.trim(),
    order: Number.parseInt(draft.order, 10),
    parentId: draft.parentId || null,
    path: draft.path.trim(),
    permissionCode: normalizeOptional(draft.permissionCode),
    redirect: draft.redirect.trim(),
    title: draft.title.trim(),
    type: draft.type,
  };
}

export function setMenuDraftType(draft: MenuDraft, type: MenuApi.PermissionTreeItem['type']): void {
  draft.type = type;
  if (type === 1) {
    draft.component = 'BasicLayout';
    draft.keepAlive = true;
    draft.hideInBreadcrumb = false;
    draft.hideInMenu = false;
    draft.hideInTab = false;
    draft.link = '';
    draft.redirect = '';
  } else if (type === 3) {
    draft.component = '';
    draft.keepAlive = false;
    draft.hideInBreadcrumb = true;
    draft.hideInMenu = true;
    draft.hideInTab = true;
    draft.link = '';
    draft.redirect = '';
  } else {
    draft.component = '';
    draft.keepAlive = true;
    draft.hideInBreadcrumb = false;
    draft.hideInMenu = false;
    draft.hideInTab = false;
  }
}

export function flattenMenuTree(menus: readonly MenuApi.PermissionTreeItem[], depth = 0): PlatformMenuRow[] {
  return menus.flatMap((menu) => [{ ...menu, depth }, ...flattenMenuTree(menu.children, depth + 1)]);
}

export function getMenuDescendantIds(menu: MenuApi.PermissionTreeItem): Set<string> {
  const descendants = new Set<string>();
  const visit = (node: MenuApi.PermissionTreeItem) => {
    for (const child of node.children) {
      descendants.add(child.id);
      visit(child);
    }
  };
  visit(menu);
  return descendants;
}

export function getMenuCompatibility(menu: MenuApi.PermissionTreeItem | MenuDraft): MenuCompatibility {
  if (menu.type === 3) {
    return { kind: 'action', label: '按钮权限', detail: '只关联权限码，不会注册为导航路由。' };
  }

  if (menu.link?.trim()) {
    return isSafeExternalLink(menu.link)
      ? { kind: 'external', label: '外链已校验', detail: '允许 HTTP(S)，当前前端不会自动打开外链。' }
      : { kind: 'unsupported', label: '外链不安全', detail: '仅允许无账号信息的 HTTP 或 HTTPS 绝对地址。' };
  }

  if (menu.redirect?.trim()) {
    return { kind: 'unsupported', label: '含重定向', detail: '当前页面白名单不装配带重定向的菜单。' };
  }

  if (menu.type === 1) {
    const registered = menuCatalogRegistry.some(
      (entry) =>
        entry.backendName === menu.name && entry.backendPath === menu.path && entry.backendComponent === menu.component,
    );
    return registered
      ? { kind: 'registered', label: '目录已登记', detail: '目录匹配本地导航目录白名单。' }
      : { kind: 'unsupported', label: '目录未登记', detail: '目录元数据会保存，但不会自动加入导航。' };
  }

  const registered = pageRegistry.some(
    (entry) =>
      entry.backendName === menu.name && entry.backendPath === menu.path && entry.backendComponent === menu.component,
  );
  return registered
    ? { kind: 'registered', label: '页面已登记', detail: '名称、路径与组件均匹配本地页面白名单。' }
    : { kind: 'unsupported', label: '页面未接入', detail: '后端组件不会被动态加载，需先在本地注册页面。' };
}

export function isSafeExternalLink(value: string): boolean {
  if (!value.trim() || value.trim().startsWith('//')) return false;

  try {
    const parsed = new URL(value.trim());
    return (
      (parsed.protocol === 'http:' || parsed.protocol === 'https:') &&
      Boolean(parsed.hostname) &&
      !parsed.username &&
      !parsed.password
    );
  } catch {
    return false;
  }
}

export function validateMenuDraft(
  draft: MenuDraft,
  menus: readonly MenuApi.PermissionTreeItem[],
  editingId?: string,
): null | string {
  const name = draft.name.trim();
  const title = draft.title.trim();
  const appCode = draft.appCode.trim();
  const path = draft.path.trim();
  const order = Number.parseInt(draft.order, 10);

  if (!name || name.length > 64) return '菜单名称为必填项，最多64个字符。';
  if (!title || title.length > 128) return '菜单标题为必填项，最多128个字符。';
  if (!appCode || appCode.length > 32) return 'AppCode为必填项，最多32个字符。';
  if (!path || path.length > 256) return '路径为必填项，最多256个字符。';
  if (draft.component.length > 256) return '组件标识最多256个字符。';
  if (draft.icon.length > 128) return '图标标识最多128个字符。';
  if (draft.permissionCode.length > 128) return '权限码最多128个字符。';
  if (draft.redirect.length > 256) return '重定向路径最多256个字符。';
  if (draft.link.length > 512) return '外链最多512个字符。';
  if (!Number.isSafeInteger(order)) return '排序必须是有效整数。';
  if (menus.some((menu) => menu.id !== editingId && menu.name === name)) return '菜单名称已存在。';
  if (draft.type !== 3 && !isSafeInternalPath(path)) return '目录或菜单路径必须是安全的站内绝对路径。';
  if (draft.redirect.trim() && !isSafeInternalPath(draft.redirect.trim())) return '重定向只允许安全的站内绝对路径。';
  if (draft.link.trim() && !isSafeExternalLink(draft.link)) return '外链仅允许无账号信息的 HTTP 或 HTTPS 绝对地址。';

  const flat = flattenMenuTree(menus);
  const parent = draft.parentId ? flat.find((menu) => menu.id === draft.parentId) : undefined;
  if (draft.parentId && !parent) return '父级菜单不存在，请刷新后重新选择。';
  if (parent?.type === 3) return '按钮权限不能作为父级菜单。';
  if (parent && parent.appCode.toLowerCase() !== appCode.toLowerCase()) return '父级菜单必须属于相同的AppCode。';
  if (editingId && draft.parentId === editingId) return '菜单不能把自己设为父级。';
  if (editingId && parent && getMenuDescendantIds(flat.find((menu) => menu.id === editingId)!).has(parent.id)) {
    return '不能把菜单移动到自己的子节点下面。';
  }

  return null;
}

function normalizeOptional(value: string): null | string {
  const normalized = value.trim();
  return normalized || null;
}

function isSafeInternalPath(path: string): boolean {
  return (
    path.startsWith('/') &&
    !path.startsWith('//') &&
    !path.includes('\\') &&
    !path.includes('?') &&
    !path.includes('#') &&
    // oxlint-disable-next-line no-control-regex -- Reject ASCII control characters in backend menu paths.
    !/[\u0000-\u001f]/u.test(path)
  );
}
