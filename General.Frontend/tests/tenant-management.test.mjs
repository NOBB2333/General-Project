import assert from 'node:assert/strict';
import test from 'node:test';
import {
  createTenantDraft,
  filterTenants,
  getTenantMetrics,
  toTenantSaveInput,
  validateTenantDraft,
} from '../src/features/platform/tenants/tenant.model.ts';
import {
  getTenantMenuCheckedState,
  getTenantMenuUnknownCount,
  getTenantEndpointUnknownKeys,
  toggleTenantMenuSelection,
  toTenantAuthorizationInput,
} from '../src/features/platform/tenants/tenant-authorization.model.ts';

const tenantRows = [
  {
    adminEmail: 'admin@alpha.local',
    adminUserId: 'user-alpha',
    adminUserName: 'alpha.admin',
    apiBlacklist: ['Platform.Project.Manage'],
    creationTime: '2026-01-01T00:00:00Z',
    defaultConnectionStringDisplay: null,
    hasDefaultConnectionString: false,
    hasExplicitAuthorization: true,
    id: 'tenant-alpha',
    isActive: true,
    name: 'Alpha',
    remark: '项目团队',
  },
  {
    adminEmail: null,
    adminUserId: null,
    adminUserName: null,
    apiBlacklist: [],
    creationTime: '2026-01-02T00:00:00Z',
    defaultConnectionStringDisplay: '已配置（SQLite · tenant.db）',
    hasDefaultConnectionString: true,
    hasExplicitAuthorization: false,
    id: 'tenant-beta',
    isActive: false,
    name: 'Beta',
    remark: null,
  },
];

const menuTree = [
  {
    children: [
      { children: [], id: 'enabled-child', isEnabled: true },
      { children: [], id: 'disabled-child', isEnabled: false },
    ],
    id: 'enabled-root',
    isEnabled: true,
  },
];

test('validates tenant names and creation administrator credentials against backend constraints', () => {
  const draft = createTenantDraft();
  assert.match(validateTenantDraft(draft, 'create'), /租户名称/u);

  Object.assign(draft, { name: '新租户', adminUserName: '1bad', adminPassword: 'secret' });
  assert.match(validateTenantDraft(draft, 'create'), /管理员账号/u);

  draft.adminUserName = 'tenant.admin';
  draft.adminPassword = '';
  assert.match(validateTenantDraft(draft, 'create'), /初始密码/u);

  draft.adminPassword = 'initial-password';
  assert.equal(validateTenantDraft(draft, 'create'), null);
  draft.name = 'x'.repeat(65);
  assert.match(validateTenantDraft(draft, 'create'), /64/u);
});

test('serializes create and edit tenant DTOs without sending empty connection strings or create-only fields', () => {
  const createDraft = createTenantDraft();
  Object.assign(createDraft, {
    adminEmail: ' admin@example.test ',
    adminPassword: 'secret',
    adminUserName: ' tenant.admin ',
    name: ' 新租户 ',
    remark: ' 说明 ',
  });
  assert.deepEqual(toTenantSaveInput(createDraft, 'create'), {
    adminEmail: 'admin@example.test',
    adminPassword: 'secret',
    adminUserId: null,
    adminUserName: 'tenant.admin',
    name: '新租户',
    remark: '说明',
  });

  const editDraft = createTenantDraft(tenantRows[0]);
  editDraft.name = ' Alpha Updated ';
  const editInput = toTenantSaveInput(editDraft, 'edit');
  assert.deepEqual(editInput, {
    adminUserId: 'user-alpha',
    name: 'Alpha Updated',
    remark: '项目团队',
  });
  assert.equal('adminPassword' in editInput, false);
  assert.equal('defaultConnectionString' in editInput, false);
});

test('filters the full tenant collection locally and computes accurate overview metrics', () => {
  assert.deepEqual(
    filterTenants(tenantRows, 'ALPHA', 'all').map((row) => row.id),
    ['tenant-alpha'],
  );
  assert.deepEqual(filterTenants(tenantRows, 'admin@beta', 'all'), []);
  assert.deepEqual(
    filterTenants(tenantRows, 'beta', 'inactive').map((row) => row.id),
    ['tenant-beta'],
  );
  assert.deepEqual(
    getTenantMetrics(tenantRows).map(({ id, value }) => [id, value]),
    [
      ['total', 2],
      ['active', 1],
      ['inactive', 1],
      ['connection', 1],
    ],
  );
});

test('tenant menu tree toggles enabled descendants while preserving unknown and disabled values', () => {
  const existing = new Set(['legacy-menu-id', 'disabled-child']);
  const selected = toggleTenantMenuSelection(menuTree, existing, 'enabled-root', true);

  assert.deepEqual(selected, new Set(['legacy-menu-id', 'disabled-child', 'enabled-root', 'enabled-child']));
  assert.equal(getTenantMenuCheckedState(menuTree[0], selected), true);
  assert.equal(getTenantMenuUnknownCount(menuTree, selected), 2);

  const deselected = toggleTenantMenuSelection(menuTree, selected, 'enabled-root', false);
  assert.deepEqual(deselected, new Set(['legacy-menu-id', 'disabled-child']));
});

test('tenant authorization serialization preserves unknown API restrictions, current status and admin assignment', () => {
  const current = {
    adminUserId: 'admin-1',
    apiBlacklist: ['legacy.endpoint', 'Platform.Project.Manage'],
    isActive: false,
    menuIds: ['legacy-menu'],
    remark: 'old',
  };
  const apiGroups = [{ groupName: '项目', items: [{ key: 'Platform.Project.Manage', label: '项目管理' }] }];
  assert.deepEqual(getTenantEndpointUnknownKeys(apiGroups, new Set(current.apiBlacklist)), ['legacy.endpoint']);

  const input = toTenantAuthorizationInput(
    current,
    new Set(['menu-b', 'menu-a']),
    new Set(['legacy.endpoint', 'Platform.Project.Manage']),
    '  new note  ',
    'admin-2',
  );
  assert.deepEqual(input, {
    adminUserId: 'admin-2',
    apiBlacklist: ['legacy.endpoint', 'Platform.Project.Manage'],
    isActive: false,
    menuIds: ['menu-a', 'menu-b'],
    remark: 'new note',
  });
});
