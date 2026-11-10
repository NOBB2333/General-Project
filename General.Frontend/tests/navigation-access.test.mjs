import assert from 'node:assert/strict';
import test from 'node:test';

import { canAccess } from '../src/directives/permission.ts';
import { adaptBackendMenuTree, getResolvedPages, resolveAuthorizedHomePath } from '../src/navigation/page-registry.ts';
import { decideRouteAccess } from '../src/navigation/route-policy.ts';
import {
  clearCurrentNavigation,
  isRouteAuthorized,
  navigationState,
  setCurrentNavigation,
} from '../src/navigation/menu-store.ts';

const platformWorkspace = {
  name: 'PlatformWorkspace',
  path: '/platform/workspace',
  component: '/platform/workspace/index',
  redirect: null,
  meta: { title: '平台总览', icon: 'carbon:workspace', affixTab: true, link: null },
  children: [],
};

const platformOrganization = {
  name: 'PlatformOrganization',
  path: '/platform/organization',
  component: '/platform/organization/index',
  redirect: null,
  meta: { title: '组织架构', icon: 'lucide:building-2', affixTab: false, link: null },
  children: [],
};

const platformSettings = {
  name: 'PlatformSettings',
  path: '/platform/settings',
  component: '/platform/settings/index',
  redirect: null,
  meta: { title: '系统设置', icon: 'lucide:settings', link: null },
  children: [],
};

const platformRoles = {
  name: 'PlatformRoles',
  path: '/platform/roles',
  component: '/platform/roles/index',
  redirect: null,
  meta: { title: '角色权限', icon: 'lucide:key-round', affixTab: false, link: null },
  children: [],
};

const platformMenus = {
  name: 'PlatformMenus',
  path: '/platform/menus',
  component: '/platform/menus/index',
  redirect: null,
  meta: { title: '菜单管理', icon: 'lucide:waypoints', affixTab: false, link: null },
  children: [],
};

const platformDicts = {
  name: 'PlatformDicts',
  path: '/platform/dicts',
  component: '/platform/dicts/index',
  redirect: null,
  meta: { title: '字典管理', icon: 'lucide:list-tree', affixTab: false, link: null },
  children: [],
};

const platformFiles = {
  name: 'PlatformFiles',
  path: '/platform/files',
  component: '/platform/files/index',
  redirect: null,
  meta: { title: '文件管理', icon: 'lucide:files', affixTab: false, link: null },
  children: [],
};

const platformTenants = {
  name: 'PlatformTenants',
  path: '/platform/tenants',
  component: '/platform/tenants/index',
  redirect: null,
  meta: { title: '租户管理', icon: 'lucide:building-2', affixTab: false, link: null },
  children: [],
};

const projectWorkspace = {
  name: 'ProjectWorkspace',
  path: '/project/workspace',
  component: '/project/workspace/index',
  redirect: null,
  meta: { title: '项目工作台', icon: 'lucide:layout-dashboard', affixTab: false, link: null },
  children: [],
};

const projectList = {
  name: 'ProjectList',
  path: '/project/projects',
  component: '/project/projects/index',
  redirect: null,
  meta: { title: '项目中心', icon: 'lucide:folder-open-dot', affixTab: false, link: null },
  children: [],
};

const projectDetail = {
  name: 'ProjectDetail',
  path: '/project/detail',
  component: '/project/detail/index',
  redirect: null,
  meta: { title: '项目档案', icon: 'lucide:panel-right-open', affixTab: false, link: null },
  children: [],
};

const projectTasks = {
  name: 'ProjectTasks',
  path: '/project/tasks',
  component: '/project/tasks/index',
  redirect: null,
  meta: { title: '我的任务', icon: 'lucide:list-checks', affixTab: false, link: null },
  children: [],
};

const projectMyRelated = {
  name: 'ProjectMyRelated',
  path: '/project/my-related',
  component: '/project/my-related/index',
  redirect: null,
  meta: { title: '我的项目事项', icon: 'lucide:user-round-check', affixTab: false, link: null },
  children: [],
};

function platformTree(children = [platformWorkspace]) {
  return [
    {
      name: 'PlatformCenter',
      path: '/platform',
      component: 'BasicLayout',
      redirect: null,
      meta: { title: '平台中心', icon: 'lucide:settings-2', link: null },
      children,
    },
  ];
}

function projectTree(children = [projectWorkspace, projectList, projectDetail, projectTasks, projectMyRelated]) {
  return [
    {
      name: 'ProjectCenter',
      path: '/project',
      component: 'BasicLayout',
      redirect: null,
      meta: { title: '项目执行', icon: 'lucide:folder-kanban', link: null },
      children,
    },
  ];
}

test('keeps only exact page and catalog registrations from backend menus', () => {
  const routes = [
    ...platformTree([
      platformWorkspace,
      {
        ...platformWorkspace,
        name: 'UnregisteredPage',
        path: '/platform/unknown',
        component: '@/features/unknown.vue',
      },
      {
        ...platformWorkspace,
        component: '../../outside.vue',
      },
      {
        ...platformWorkspace,
        meta: { ...platformWorkspace.meta, link: 'https://example.com' },
      },
    ]),
    {
      name: 'DashboardCenter',
      path: '/dashboard',
      component: 'BasicLayout',
      redirect: null,
      meta: { title: '工作台' },
      children: [],
    },
  ];

  const adapted = adaptBackendMenuTree(routes, []);
  assert.equal(adapted.length, 1);
  assert.equal(adapted[0].label, '平台中心');
  assert.deepEqual(
    getResolvedPages(adapted).map((page) => page.routeName),
    ['platform-workspace'],
  );
});

test('uses an authorized local page as home when backend HomePath is unknown or external', () => {
  const pages = getResolvedPages(adaptBackendMenuTree(platformTree(), []));
  assert.equal(resolveAuthorizedHomePath('/platform/workspace', pages), '/platform/workspace');
  assert.equal(resolveAuthorizedHomePath('/legacy/workbench', pages), '/platform/workspace');
  assert.equal(resolveAuthorizedHomePath('//external.example/path', pages), '/platform/workspace');
  assert.equal(resolveAuthorizedHomePath('/platform/workspace?next=/admin', pages), '/platform/workspace');
  assert.equal(resolveAuthorizedHomePath('/platform/workspace', []), null);
});

test('registers the exact backend organization route for authenticated users', () => {
  const pages = getResolvedPages(adaptBackendMenuTree(platformTree([platformOrganization]), []));
  assert.deepEqual(
    pages.map((page) => [page.routeName, page.localPath]),
    [['platform-organization', '/platform/organization']],
  );
});

test('registers the settings workspace only for the seeded platform settings route', () => {
  const registered = getResolvedPages(adaptBackendMenuTree(platformTree([platformSettings]), []));
  assert.deepEqual(
    registered.map((page) => [page.routeName, page.localPath]),
    [['settings', '/settings']],
  );
  setCurrentNavigation(platformTree([platformSettings]), []);
  assert.equal(isRouteAuthorized('settings'), true);
  clearCurrentNavigation();
  assert.equal(isRouteAuthorized('settings'), false);

  const unregistered = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformSettings, path: '/platform/other-settings' }]), []),
  );
  assert.deepEqual(unregistered, []);
});

test('registers the exact backend role route only for the role management permission', () => {
  const granted = getResolvedPages(adaptBackendMenuTree(platformTree([platformRoles]), ['Platform.Role.Manage']));
  assert.deepEqual(
    granted.map((page) => [page.routeName, page.localPath]),
    [['platform-roles', '/platform/roles']],
  );

  const denied = getResolvedPages(adaptBackendMenuTree(platformTree([platformRoles]), []));
  assert.equal(denied.length, 0);

  const wrongComponent = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformRoles, component: '@/features/unknown.vue' }]), [
      'Platform.Role.Manage',
    ]),
  );
  assert.equal(wrongComponent.length, 0);
});

test('registers the exact backend menu route only for the menu management permission', () => {
  const granted = getResolvedPages(adaptBackendMenuTree(platformTree([platformMenus]), ['Platform.Menu.Manage']));
  assert.deepEqual(
    granted.map((page) => [page.routeName, page.localPath]),
    [['platform-menus', '/platform/menus']],
  );

  const denied = getResolvedPages(adaptBackendMenuTree(platformTree([platformMenus]), []));
  assert.equal(denied.length, 0);

  const wrongComponent = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformMenus, component: '@/features/unknown.vue' }]), [
      'Platform.Menu.Manage',
    ]),
  );
  assert.equal(wrongComponent.length, 0);
});

test('registers the exact backend dictionary route only for the dictionary management permission', () => {
  const granted = getResolvedPages(adaptBackendMenuTree(platformTree([platformDicts]), ['Platform.Dict.Manage']));
  assert.deepEqual(
    granted.map((page) => [page.routeName, page.localPath]),
    [['platform-dicts', '/platform/dicts']],
  );

  const denied = getResolvedPages(adaptBackendMenuTree(platformTree([platformDicts]), []));
  assert.equal(denied.length, 0);

  const wrongComponent = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformDicts, component: '@/features/unknown.vue' }]), [
      'Platform.Dict.Manage',
    ]),
  );
  assert.equal(wrongComponent.length, 0);
});

test('registers the exact backend file route only for the file management permission', () => {
  const granted = getResolvedPages(adaptBackendMenuTree(platformTree([platformFiles]), ['Platform.File.Manage']));
  assert.deepEqual(
    granted.map((page) => [page.routeName, page.localPath]),
    [['platform-files', '/platform/files']],
  );

  const denied = getResolvedPages(adaptBackendMenuTree(platformTree([platformFiles]), []));
  assert.equal(denied.length, 0);

  const wrongComponent = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformFiles, component: '@/features/unknown.vue' }]), [
      'Platform.File.Manage',
    ]),
  );
  assert.equal(wrongComponent.length, 0);
});

test('registers the exact backend tenant route only for tenant view permission', () => {
  const granted = getResolvedPages(adaptBackendMenuTree(platformTree([platformTenants]), ['Platform.Tenant.View']));
  assert.deepEqual(
    granted.map((page) => [page.routeName, page.localPath]),
    [['platform-tenants', '/platform/tenants']],
  );

  const denied = getResolvedPages(adaptBackendMenuTree(platformTree([platformTenants]), []));
  assert.equal(denied.length, 0);

  const wrongComponent = getResolvedPages(
    adaptBackendMenuTree(platformTree([{ ...platformTenants, component: '@/features/unknown.vue' }]), [
      'Platform.Tenant.View',
    ]),
  );
  assert.equal(wrongComponent.length, 0);
});

test('prefers the explicitly registered default home page over an affix tab', () => {
  const pages = [
    {
      affixTab: true,
      defaultHome: false,
      localPath: '/first',
      children: [],
      icon: null,
      kind: 'page',
      label: '首个',
      routeName: 'first',
    },
    {
      affixTab: false,
      defaultHome: true,
      localPath: '/home',
      children: [],
      icon: null,
      kind: 'page',
      label: '首页',
      routeName: 'home',
    },
  ];

  assert.equal(resolveAuthorizedHomePath('/missing', pages), '/home');
});

test('maps General.Admin project menus to the migrated Chao project pages', () => {
  const pages = getResolvedPages(adaptBackendMenuTree(projectTree(), []));
  assert.deepEqual(
    pages.map((page) => [page.routeName, page.localPath]),
    [
      ['page.projects-new.project-board', '/projects-new/project-board'],
      ['page.projects-new.project-list', '/projects-new/project-list'],
      ['page.projects-new.project-list', '/projects-new/project-list'],
      ['page.projects-new.my-tasks', '/projects-new/my-tasks'],
      ['page.projects-new.my-projects', '/projects-new/my-projects'],
    ],
  );
});

test('resolves backend project HomePath aliases only through authorized project menu entries', () => {
  const pages = getResolvedPages(adaptBackendMenuTree(projectTree(), []));
  assert.equal(resolveAuthorizedHomePath('/project/pmo-overview', pages), '/projects-new/project-board');
  assert.equal(resolveAuthorizedHomePath('/project/pm-dashboard', pages), '/projects-new/project-board');
  assert.equal(resolveAuthorizedHomePath('/project/my-related', pages), '/projects-new/my-projects');
  assert.equal(resolveAuthorizedHomePath('/project/projects', pages), '/projects-new/project-list');

  const listOnly = getResolvedPages(adaptBackendMenuTree(projectTree([projectList]), []));
  assert.equal(resolveAuthorizedHomePath('/project/pmo-overview', listOnly), '/projects-new/project-list');
});

test('does not keep navigation state when a session is replaced by another account', () => {
  setCurrentNavigation(platformTree(), []);
  assert.equal(isRouteAuthorized('platform-workspace'), true);
  assert.equal(navigationState.status, 'ready');

  clearCurrentNavigation();
  setCurrentNavigation([], []);

  assert.equal(isRouteAuthorized('platform-workspace'), false);
  assert.equal(navigationState.pages.length, 0);
  assert.equal(navigationState.items.length, 0);
  assert.equal(resolveAuthorizedHomePath('/platform/workspace', navigationState.pages), null);
});

test('requires every access code for a guarded button', () => {
  assert.equal(canAccess('Platform.User.Manage', ['Platform.User.Manage']), true);
  assert.equal(canAccess('platform.user.manage', ['Platform.User.Manage']), true);
  assert.equal(canAccess(['Platform.User.Manage', 'Platform.Role.Manage'], ['Platform.User.Manage']), false);
  assert.equal(canAccess('Platform.User.Manage', []), false);
});

test('route policy covers login, whitelist, permission and not-found decisions', () => {
  const base = {
    authenticated: true,
    isErrorRoute: false,
    isLoginRoute: false,
    isRoot: false,
    registered: true,
    authorized: true,
  };

  assert.equal(decideRouteAccess({ ...base, authenticated: false }), 'login');
  assert.equal(decideRouteAccess({ ...base, authenticated: false, isLoginRoute: true }), 'allow-login');
  assert.equal(decideRouteAccess({ ...base, isRoot: true }), 'home');
  assert.equal(decideRouteAccess({ ...base, isLoginRoute: true }), 'home');
  assert.equal(decideRouteAccess({ ...base, isErrorRoute: true }), 'allow');
  assert.equal(decideRouteAccess({ ...base, registered: false }), 'not-found');
  assert.equal(decideRouteAccess({ ...base, authorized: false }), 'forbidden');
  assert.equal(decideRouteAccess(base), 'allow');
});
