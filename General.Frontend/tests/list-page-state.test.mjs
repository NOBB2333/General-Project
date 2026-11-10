import assert from 'node:assert/strict';
import test from 'node:test';
import { effectScope, nextTick, shallowRef } from 'vue';
import { useListPageState } from '../src/components/page/families/list/useListPageState.ts';

const listModel = () => ({
  key: 'platform|configs',
  path: '/platform/configs',
  pageSize: 1,
  listScopes: [{ id: 'all' }],
  listViews: [{ id: 'list' }],
  listTabs: [{ id: 'all' }],
  filters: [{ key: 'group', options: ['全部', 'A', 'B'] }],
  rows: [
    { id: '1', name: 'Alpha', group: 'A' },
    { id: '2', name: 'Beta', group: 'B' },
    { id: '3', name: 'Gamma', group: 'A' },
  ],
});

test('keeps same-page search and filters after readback but clears state on another page', async () => {
  const scope = effectScope();
  const model = shallowRef(listModel());
  const state = scope.run(() => useListPageState(model));
  try {
    state.query.value = 'alpha';
    state.setFilter('group', 'A');
    state.toggleRow('2');
    await nextTick();
    model.value = {
      ...model.value,
      rows: [
        { id: '1', name: 'Alpha updated', group: 'A' },
        { id: '3', name: 'Gamma', group: 'A' },
      ],
    };
    await nextTick();
    assert.equal(state.query.value, 'alpha');
    assert.equal(state.filterValues.value.group, 'A');
    assert.equal(state.visibleRows.value[0].name, 'Alpha updated');
    assert.equal(state.selectedCount.value, 0);
    model.value = { ...listModel(), key: 'other', path: '/other' };
    await nextTick();
    assert.equal(state.query.value, '');
    assert.equal(state.filterValues.value.group, '全部');
    assert.equal(state.pageNumber.value, 1);
  } finally {
    scope.stop();
  }
});

test('returns to a valid page when search or refreshed data narrows a local list', async () => {
  const scope = effectScope();
  const model = shallowRef(listModel());
  const state = scope.run(() => useListPageState(model));
  try {
    state.pageNumber.value = 3;
    state.query.value = 'beta';
    await nextTick();
    assert.equal(state.pageNumber.value, 1);
    assert.equal(state.visibleRows.value[0].name, 'Beta');
    state.query.value = '';
    await nextTick();
    state.pageNumber.value = 3;
    model.value = { ...model.value, rows: [{ id: '1', name: 'Alpha', group: 'A' }] };
    await nextTick();
    assert.equal(state.pageNumber.value, 1);
    assert.equal(state.pageCount.value, 1);
  } finally {
    scope.stop();
  }
});
