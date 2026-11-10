import assert from 'node:assert/strict';
import test from 'node:test';
import {
  createDictDataDraft,
  createDictTypeDraft,
  displayDictText,
  getDictTagLabel,
  getDictTagTone,
  normalizeDictCode,
  resolveDictLabel,
  toDictDataSaveInput,
  toDictTypeSaveInput,
  validateDictDataDraft,
  validateDictTypeDraft,
} from '../src/features/platform/dicts/dict.model.ts';

const types = [
  {
    code: 'project_status',
    dataCount: 2,
    enabledDataCount: 1,
    id: 'type-1',
    name: '项目状态',
    remark: '',
    sort: 1,
  },
];

const rows = [
  {
    dictTypeId: 'type-1',
    id: 'data-1',
    isEnabled: true,
    label: '进行中',
    remark: '',
    sort: 1,
    tagColor: 'processing',
    value: 'active',
  },
];

test('normalizes dictionary codes and rejects invalid or duplicate type input', () => {
  assert.equal(normalizeDictCode('  Project_Status  '), 'project_status');

  const draft = createDictTypeDraft();
  Object.assign(draft, { code: ' PROJECT_STATUS ', name: '状态' });
  assert.equal(validateDictTypeDraft(draft, types), '字典编码已存在。');
  assert.equal(validateDictTypeDraft(draft, types, 'type-1'), null);

  draft.code = 'status/unsafe';
  assert.match(validateDictTypeDraft(draft, []), /仅支持/u);
  draft.code = 'new_status';
  draft.sort = '-1';
  assert.match(validateDictTypeDraft(draft, []), /排序值/u);
});

test('serializes trimmed type values and keeps an edited code immutable', () => {
  const draft = createDictTypeDraft();
  Object.assign(draft, { code: 'ignored_change', name: '  状态  ', remark: '  备注  ', sort: '3' });

  assert.deepEqual(toDictTypeSaveInput(draft, 'Project_Status'), {
    code: 'project_status',
    name: '状态',
    remark: '备注',
    sort: 3,
  });
});

test('validates dictionary values and preserves enabled, tag and sort fields', () => {
  const draft = createDictDataDraft();
  Object.assign(draft, {
    isEnabled: false,
    label: '  已关闭  ',
    remark: '  归档  ',
    sort: '8',
    tagColor: ' unknown-tone ',
    value: '  closed  ',
  });

  assert.equal(validateDictDataDraft(draft, rows), null);
  assert.deepEqual(toDictDataSaveInput(draft), {
    isEnabled: false,
    label: '已关闭',
    remark: '归档',
    sort: 8,
    tagColor: 'unknown-tone',
    value: 'closed',
  });

  draft.value = ' active ';
  assert.equal(validateDictDataDraft(draft, rows), '该字典值已存在。');
});

test('maps known tag colors to shared tones and safely falls back for empty or unknown values', () => {
  assert.equal(getDictTagTone('processing'), 'primary');
  assert.equal(getDictTagTone('blue'), 'primary');
  assert.equal(getDictTagTone('success'), 'success');
  assert.equal(getDictTagTone('gold'), 'warning');
  assert.equal(getDictTagTone('error'), 'danger');
  assert.equal(getDictTagTone('default'), 'neutral');
  assert.equal(getDictTagTone('server-defined'), 'neutral');
  assert.equal(getDictTagTone(null), 'neutral');
  assert.equal(getDictTagLabel('  '), 'default');
});

test('resolves shared dictionary labels while preserving unknown values and blank states', () => {
  const items = [
    { label: '进行中', tagColor: 'processing', value: 'active' },
    { label: '已完成', tagColor: 'success', value: 'done' },
  ];

  assert.equal(resolveDictLabel(items, 'active'), '进行中');
  assert.equal(resolveDictLabel(items, 'legacy-value'), 'legacy-value');
  assert.equal(resolveDictLabel(items, null), '—');
  assert.equal(displayDictText('  '), '—');
});
