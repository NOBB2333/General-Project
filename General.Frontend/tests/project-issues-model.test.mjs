import assert from 'node:assert/strict';
import test from 'node:test';

import {
  buildProjectIssueSaveInput,
  buildProjectRaidRiskUpdateInput,
  buildProjectRequirementSaveInput,
  createProjectIssueForm,
  createProjectRequirementForm,
  projectIssueFormFromItem,
  projectRaidRiskFormFromItem,
  projectRequirementFormFromItem,
} from '../src/features/projects-new/model/project-issues.ts';

test('issue form maps nullable relations and date-only input to the backend date shape', () => {
  const form = createProjectIssueForm('member-1');
  form.title = '  无法保存  ';
  form.dueDate = '2026-10-31';
  form.acceptanceCriteriaId = 'criteria-1';

  assert.deepEqual(buildProjectIssueSaveInput(form), {
    acceptanceCriteriaId: 'criteria-1',
    developerOwnerUserId: null,
    dueDate: '2026-10-31T00:00:00',
    level: 'P1',
    ownerUserId: 'member-1',
    productOwnerUserId: null,
    qualityChecklistId: null,
    requirementTitle: '',
    status: '待处理',
    testerOwnerUserId: null,
    title: '无法保存',
    type: '缺陷',
  });

  const hydrated = projectIssueFormFromItem({
    acceptanceCriteriaId: null,
    developerOwnerUserId: null,
    dueDate: '2026-10-31T00:00:00',
    id: 'issue-1',
    level: 'P1',
    ownerUserId: 'member-1',
    productOwnerUserId: null,
    qualityChecklistId: null,
    requirementTitle: null,
    status: '待处理',
    testerOwnerUserId: null,
    title: '无法保存',
    type: '缺陷',
  });
  assert.equal(hydrated.ownerUserId, 'member-1');
  assert.equal(hydrated.dueDate, '2026-10-31');
});

test('requirement editor round trips versions, project links, and optional estimates', () => {
  const item = {
    acceptanceCriteria: '可正常登录',
    businessOwnerName: '业务负责人',
    businessOwnerUserId: 'user-1',
    changeReason: '范围调整',
    description: '统一登录入口',
    id: 'req-2',
    impactSummary: '影响登录流程',
    isCurrent: true,
    milestoneId: 'milestone-1',
    milestoneName: '上线准备',
    priority: 'P1',
    productOwnerName: '产品负责人',
    productOwnerUserId: 'user-2',
    projectId: 'project-1',
    replacesRequirementId: 'req-1',
    replacesRequirementTitle: '旧版登录',
    requirementCode: 'REQ-002',
    sizeEstimate: 3.5,
    source: '访谈',
    status: '已确认',
    title: '统一登录',
    type: '功能',
    versionNo: 2,
    wbsNodeId: 'wbs-1',
    wbsNodeName: '认证模块',
  };

  const form = projectRequirementFormFromItem(item);
  assert.deepEqual(buildProjectRequirementSaveInput(form), {
    acceptanceCriteria: '可正常登录',
    businessOwnerUserId: 'user-1',
    changeReason: '范围调整',
    description: '统一登录入口',
    impactSummary: '影响登录流程',
    isCurrent: true,
    milestoneId: 'milestone-1',
    priority: 'P1',
    productOwnerUserId: 'user-2',
    replacesRequirementId: 'req-1',
    requirementCode: 'REQ-002',
    sizeEstimate: 3.5,
    source: '访谈',
    status: '已确认',
    title: '统一登录',
    type: '功能',
    versionNo: 2,
    wbsNodeId: 'wbs-1',
  });

  const blankForm = createProjectRequirementForm();
  assert.equal(buildProjectRequirementSaveInput(blankForm).sizeEstimate, null);
});

test('RAID risk form sends the editable assessment only and hydrates backend score inputs', () => {
  const item = {
    contingencyPlan: '切换备用方案',
    id: 'raid-1',
    impact: 4,
    isOverdue: false,
    level: 'P1',
    ownerName: '项目经理',
    plannedResolveDate: null,
    probability: 3,
    projectId: 'project-1',
    projectName: '项目',
    responseStrategy: '降低风险',
    riskScore: 12,
    status: '处理中',
    title: '外部依赖延期',
    triggerCondition: '供应商未按期交付',
    type: '风险',
  };

  assert.deepEqual(buildProjectRaidRiskUpdateInput(projectRaidRiskFormFromItem(item)), {
    contingencyPlan: '切换备用方案',
    impact: 4,
    probability: 3,
    responseStrategy: '降低风险',
    triggerCondition: '供应商未按期交付',
  });
  assert.equal(item.riskScore, 12);
});
