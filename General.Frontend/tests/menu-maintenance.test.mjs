import assert from 'node:assert/strict';
import test from 'node:test';
import {
  createMenuDraft,
  flattenMenuTree,
  getMenuCompatibility,
  isSafeExternalLink,
  toMenuSaveInput,
  validateMenuDraft,
} from '../src/features/platform/menus/menu.model.ts';

function menu(id, values = {}) {
  return {
    affixTab: false,
    appCode: 'platform',
    children: [],
    component: null,
    hideInBreadcrumb: false,
    hideInMenu: false,
    hideInTab: false,
    icon: null,
    id,
    isEnabled: true,
    keepAlive: true,
    link: null,
    menuVisibleWithForbidden: false,
    name: id,
    order: 10,
    parentId: null,
    path: `/${id}`,
    permissionCode: null,
    redirect: null,
    title: id,
    type: 1,
    ...values,
  };
}

test('flattens the backend tree while retaining order and depth', () => {
  const tree = [
    menu('root', {
      children: [menu('page', { parentId: 'root', type: 2 }), menu('action', { parentId: 'root', type: 3 })],
    }),
  ];
  assert.deepEqual(
    flattenMenuTree(tree).map(({ id, depth }) => [id, depth]),
    [
      ['root', 0],
      ['page', 1],
      ['action', 1],
    ],
  );
});

test('reports registered pages, unknown components and buttons without executing backend component strings', () => {
  const registered = menu('PlatformMenus', {
    component: '/platform/menus/index',
    name: 'PlatformMenus',
    path: '/platform/menus',
    title: '菜单管理',
    type: 2,
  });
  const unknown = menu('UnknownPage', { component: 'https://host/remote.js', type: 2 });
  const action = menu('PermissionAction', { path: 'Platform.Menu.Manage', type: 3 });

  assert.equal(getMenuCompatibility(registered).kind, 'registered');
  assert.equal(getMenuCompatibility(unknown).kind, 'unsupported');
  assert.equal(getMenuCompatibility(action).kind, 'action');
});

test('accepts only credential-free HTTP and HTTPS external links', () => {
  assert.equal(isSafeExternalLink('https://intranet.example.test/menus'), true);
  assert.equal(isSafeExternalLink('http://127.0.0.1:8080/help'), true);
  assert.equal(isSafeExternalLink('javascript:alert(1)'), false);
  assert.equal(isSafeExternalLink('data:text/html,unsafe'), false);
  assert.equal(isSafeExternalLink('//example.test/path'), false);
  assert.equal(isSafeExternalLink('https://user:password@example.test'), false);
});

test('rejects button parents, cross-app parents and cyclic moves', () => {
  const button = menu('button', { type: 3 });
  const otherApp = menu('other-app', { appCode: 'project' });
  const root = menu('root', { children: [menu('child', { parentId: 'root' })] });

  const buttonChild = createMenuDraft(button);
  buttonChild.name = 'new-page';
  buttonChild.title = '新菜单';
  buttonChild.path = '/new-page';
  assert.match(validateMenuDraft(buttonChild, [button]), /按钮权限不能作为父级/u);

  const crossAppChild = createMenuDraft(otherApp);
  crossAppChild.name = 'new-page';
  crossAppChild.title = '新菜单';
  crossAppChild.path = '/new-page';
  crossAppChild.appCode = 'platform';
  assert.match(validateMenuDraft(crossAppChild, [otherApp]), /相同的AppCode/u);

  const moveRoot = createMenuDraft(root);
  moveRoot.name = 'root';
  moveRoot.title = '根节点';
  moveRoot.path = '/root';
  moveRoot.parentId = 'child';
  assert.match(validateMenuDraft(moveRoot, [root], 'root'), /自己的子节点/u);
});

test('serializes every route metadata field so an edit can preserve the backend entity', () => {
  const draft = createMenuDraft();
  Object.assign(draft, {
    affixTab: true,
    appCode: 'platform',
    component: '/platform/menus/index',
    hideInBreadcrumb: true,
    hideInMenu: false,
    hideInTab: true,
    icon: 'lucide:waypoints',
    isEnabled: false,
    keepAlive: false,
    link: 'https://intranet.example.test/platform',
    menuVisibleWithForbidden: true,
    name: 'PlatformMenus',
    order: '42',
    parentId: 'platform-root',
    path: '/platform/menus',
    permissionCode: 'Platform.Menu.Manage',
    redirect: '/platform/workspace',
    title: '菜单管理',
    type: 2,
  });

  assert.deepEqual(toMenuSaveInput(draft), {
    affixTab: true,
    appCode: 'platform',
    component: '/platform/menus/index',
    hideInBreadcrumb: true,
    hideInMenu: false,
    hideInTab: true,
    icon: 'lucide:waypoints',
    isEnabled: false,
    keepAlive: false,
    link: 'https://intranet.example.test/platform',
    menuVisibleWithForbidden: true,
    name: 'PlatformMenus',
    order: 42,
    parentId: 'platform-root',
    path: '/platform/menus',
    permissionCode: 'Platform.Menu.Manage',
    redirect: '/platform/workspace',
    title: '菜单管理',
    type: 2,
  });
});
