import assert from 'node:assert/strict';
import test from 'node:test';

import {
  buildProjectCalendarMonthGrid,
  getProjectCalendarSpan,
  isProjectCalendarItemOnDate,
  isProjectCalendarItemVisibleInMonth,
  projectCalendarDatePart,
} from '../src/features/projects-new/model/project-calendar.ts';

test('calendar date keys preserve the date sent by the backend without local timezone conversion', () => {
  assert.equal(projectCalendarDatePart('2026-11-01T00:30:00+08:00'), '2026-11-01');
  assert.equal(projectCalendarDatePart('2026-11-01T02:00:00Z'), '2026-11-01');
});

test('calendar ranges include items that cross month boundaries', () => {
  const item = { startDate: '2026-10-31T00:00:00', endDate: '2026-11-02T00:00:00' };

  assert.equal(isProjectCalendarItemVisibleInMonth(item, 2026, 9), true);
  assert.equal(isProjectCalendarItemVisibleInMonth(item, 2026, 10), true);
  assert.equal(isProjectCalendarItemOnDate(item, '2026-11-01'), true);
  assert.equal(isProjectCalendarItemOnDate(item, '2026-11-03'), false);
});

test('calendar ranges handle a single endpoint and keep missing or invalid ranges explicit', () => {
  assert.deepEqual(getProjectCalendarSpan({ startDate: null, endDate: '2026-02-28T00:00:00' }), {
    endDate: '2026-02-28',
    kind: 'dated',
    startDate: '2026-02-28',
  });
  assert.deepEqual(getProjectCalendarSpan({ startDate: null, endDate: null }), { kind: 'undated' });
  assert.deepEqual(getProjectCalendarSpan({ startDate: '2026-02-30', endDate: null }), { kind: 'invalid' });
  assert.deepEqual(getProjectCalendarSpan({ startDate: '2026-11-02', endDate: '2026-11-01' }), { kind: 'invalid' });
});

test('month grid contains six Monday-first weeks with local calendar keys', () => {
  const cells = buildProjectCalendarMonthGrid(2026, 9);

  assert.equal(cells.length, 42);
  assert.equal(cells[0]?.dateKey, '2026-09-28');
  assert.equal(cells[3]?.dateKey, '2026-10-01');
  assert.equal(cells[3]?.inCurrentMonth, true);
  assert.equal(cells[0]?.inCurrentMonth, false);
});
