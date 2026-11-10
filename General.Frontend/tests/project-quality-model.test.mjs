import assert from 'node:assert/strict';
import test from 'node:test';

import {
  buildProjectQualityChecklistSaveInput,
  buildProjectQualityInspectionSaveInput,
  createProjectQualityChecklistForm,
  createProjectQualityInspectionForm,
} from '../src/features/projects-new/model/project-quality.ts';

test('quality checklist form trims the fields accepted by the backend', () => {
  const form = createProjectQualityChecklistForm();
  form.title = '  发布前检查  ';
  form.category = '  发布  ';
  form.description = '  核对发布准备情况  ';

  assert.deepEqual(buildProjectQualityChecklistSaveInput(form), {
    category: '发布',
    description: '核对发布准备情况',
    title: '发布前检查',
  });
});

test('quality inspection input preserves local time and checklist links without changing record text', () => {
  const form = createProjectQualityInspectionForm();
  form.qualityChecklistId = 'checklist-1';
  form.inspectorUserId = 'member-1';
  form.inspectionTime = '2026-10-14T04:30';
  form.inspectionResult = '  未通过  ';
  form.findingSummary = '  验收环境尚未配置  ';
  form.rectificationRequirement = '  补齐配置后复检  ';

  assert.deepEqual(buildProjectQualityInspectionSaveInput(form), {
    findingSummary: '验收环境尚未配置',
    inspectionResult: '未通过',
    inspectionTime: '2026-10-14T04:30:00',
    inspectorUserId: 'member-1',
    qualityChecklistId: 'checklist-1',
    rectificationRequirement: '补齐配置后复检',
  });

  form.inspectionTime = '';
  assert.equal(buildProjectQualityInspectionSaveInput(form).inspectionTime, null);
});
