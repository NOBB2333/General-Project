import assert from 'node:assert/strict';
import test from 'node:test';

import {
  getRoleMenuCheckedState,
  isBuiltInRoleName,
  resolveRoleMenuSelection,
  toggleRoleMenuSelection,
} from '../src/features/platform/roles/role-authorization.model.ts';

function menu(id, children = [], isEnabled = true) {
  return {
    appCode: 'platform',
    children,
    component: null,
    icon: null,
    id,
    isEnabled,
    name: id,
    order: 1,
    parentId: null,
    path: `/${id}`,
    permissionCode: null,
    title: id,
    type: 2,
  };
}

test('protects built-in role names without hiding custom roles', () => {
  assert.equal(isBuiltInRoleName('ADMIN'), true);
  assert.equal(isBuiltInRoleName(' viewer '), true);
  assert.equal(isBuiltInRoleName('project-operator'), false);
});

test('uses the role authorization fallback menus only when direct mappings are empty', () => {
  assert.deepEqual(resolveRoleMenuSelection([], ['default-a', 'default-b']), ['default-a', 'default-b']);
  assert.deepEqual(resolveRoleMenuSelection(['stored-a'], ['default-a']), ['stored-a']);
});

test('cascades enabled menu selections and preserves disabled grants', () => {
  const tree = [menu('catalog', [menu('page', [menu('button')]), menu('disabled-page', [], false)])];
  const existing = new Set(['disabled-page']);
  const selected = toggleRoleMenuSelection(tree, existing, 'catalog', true);

  assert.deepEqual(selected, new Set(['button', 'catalog', 'disabled-page', 'page']));
  assert.equal(getRoleMenuCheckedState(tree[0], selected), true);

  const deselected = toggleRoleMenuSelection(tree, selected, 'catalog', false);
  assert.deepEqual([...deselected], ['disabled-page']);
  assert.equal(getRoleMenuCheckedState(tree[0], deselected), false);
});

test('does not change grants by toggling a missing or disabled menu node', () => {
  const tree = [menu('disabled-catalog', [menu('disabled-child')], false)];
  const original = new Set(['disabled-catalog']);

  assert.deepEqual([...toggleRoleMenuSelection(tree, original, 'disabled-catalog', false)], ['disabled-catalog']);
  assert.deepEqual([...toggleRoleMenuSelection(tree, original, 'missing', true)], ['disabled-catalog']);
});
