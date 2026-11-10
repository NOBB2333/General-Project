import type { ProjectApi } from '@/api/core/project';

export interface ProjectIssueForm {
  acceptanceCriteriaId: string;
  developerOwnerUserId: string;
  dueDate: string;
  level: string;
  ownerUserId: string;
  productOwnerUserId: string;
  qualityChecklistId: string;
  requirementTitle: string;
  status: string;
  testerOwnerUserId: string;
  title: string;
  type: string;
}

export interface ProjectRequirementForm {
  acceptanceCriteria: string;
  businessOwnerUserId: string;
  changeReason: string;
  description: string;
  impactSummary: string;
  isCurrent: boolean;
  milestoneId: string;
  priority: string;
  productOwnerUserId: string;
  replacesRequirementId: string;
  requirementCode: string;
  sizeEstimate: string;
  source: string;
  status: string;
  title: string;
  type: string;
  versionNo: string;
  wbsNodeId: string;
}

export interface ProjectRaidRiskForm {
  contingencyPlan: string;
  impact: string;
  probability: string;
  responseStrategy: string;
  triggerCondition: string;
}

export function createProjectIssueForm(ownerUserId = ''): ProjectIssueForm {
  return {
    acceptanceCriteriaId: '',
    developerOwnerUserId: '',
    dueDate: '',
    level: 'P1',
    ownerUserId,
    productOwnerUserId: '',
    qualityChecklistId: '',
    requirementTitle: '',
    status: '待处理',
    testerOwnerUserId: '',
    title: '',
    type: '缺陷',
  };
}

export function projectIssueFormFromItem(item: ProjectApi.IssueItem): ProjectIssueForm {
  return {
    acceptanceCriteriaId: item.acceptanceCriteriaId ?? '',
    developerOwnerUserId: item.developerOwnerUserId ?? '',
    dueDate: dateInputValue(item.dueDate),
    level: item.level,
    ownerUserId: item.ownerUserId,
    productOwnerUserId: item.productOwnerUserId ?? '',
    qualityChecklistId: item.qualityChecklistId ?? '',
    requirementTitle: item.requirementTitle ?? '',
    status: item.status,
    testerOwnerUserId: item.testerOwnerUserId ?? '',
    title: item.title,
    type: item.type,
  };
}

export function buildProjectIssueSaveInput(form: ProjectIssueForm): ProjectApi.IssueSaveInput {
  return {
    acceptanceCriteriaId: nullableId(form.acceptanceCriteriaId),
    developerOwnerUserId: nullableId(form.developerOwnerUserId),
    dueDate: form.dueDate.trim() ? `${form.dueDate.trim()}T00:00:00` : null,
    level: form.level.trim(),
    ownerUserId: form.ownerUserId,
    productOwnerUserId: nullableId(form.productOwnerUserId),
    qualityChecklistId: nullableId(form.qualityChecklistId),
    requirementTitle: form.requirementTitle.trim(),
    status: form.status.trim(),
    testerOwnerUserId: nullableId(form.testerOwnerUserId),
    title: form.title.trim(),
    type: form.type.trim(),
  };
}

export function createProjectRequirementForm(): ProjectRequirementForm {
  return {
    acceptanceCriteria: '',
    businessOwnerUserId: '',
    changeReason: '',
    description: '',
    impactSummary: '',
    isCurrent: true,
    milestoneId: '',
    priority: 'P1',
    productOwnerUserId: '',
    replacesRequirementId: '',
    requirementCode: '',
    sizeEstimate: '',
    source: '内部',
    status: '草稿',
    title: '',
    type: '功能',
    versionNo: '1',
    wbsNodeId: '',
  };
}

export function projectRequirementFormFromItem(item: ProjectApi.RequirementItem): ProjectRequirementForm {
  return {
    acceptanceCriteria: item.acceptanceCriteria,
    businessOwnerUserId: item.businessOwnerUserId ?? '',
    changeReason: item.changeReason,
    description: item.description,
    impactSummary: item.impactSummary,
    isCurrent: item.isCurrent,
    milestoneId: item.milestoneId ?? '',
    priority: item.priority,
    productOwnerUserId: item.productOwnerUserId ?? '',
    replacesRequirementId: item.replacesRequirementId ?? '',
    requirementCode: item.requirementCode,
    sizeEstimate: item.sizeEstimate === null ? '' : String(item.sizeEstimate),
    source: item.source,
    status: item.status,
    title: item.title,
    type: item.type,
    versionNo: String(item.versionNo),
    wbsNodeId: item.wbsNodeId ?? '',
  };
}

export function buildProjectRequirementSaveInput(form: ProjectRequirementForm): ProjectApi.RequirementSaveInput {
  return {
    acceptanceCriteria: form.acceptanceCriteria.trim(),
    businessOwnerUserId: nullableId(form.businessOwnerUserId),
    changeReason: form.changeReason.trim(),
    description: form.description.trim(),
    impactSummary: form.impactSummary.trim(),
    isCurrent: form.isCurrent,
    milestoneId: nullableId(form.milestoneId),
    priority: form.priority.trim(),
    productOwnerUserId: nullableId(form.productOwnerUserId),
    replacesRequirementId: nullableId(form.replacesRequirementId),
    requirementCode: form.requirementCode.trim(),
    sizeEstimate: optionalNumber(form.sizeEstimate),
    source: form.source.trim(),
    status: form.status.trim(),
    title: form.title.trim(),
    type: form.type.trim(),
    versionNo: Number(form.versionNo),
    wbsNodeId: nullableId(form.wbsNodeId),
  };
}

export function projectRaidRiskFormFromItem(item: ProjectApi.RaidItem): ProjectRaidRiskForm {
  return {
    contingencyPlan: item.contingencyPlan,
    impact: String(item.impact),
    probability: String(item.probability),
    responseStrategy: item.responseStrategy,
    triggerCondition: item.triggerCondition,
  };
}

export function buildProjectRaidRiskUpdateInput(form: ProjectRaidRiskForm): ProjectApi.RaidRiskUpdateInput {
  return {
    contingencyPlan: form.contingencyPlan.trim(),
    impact: Number(form.impact),
    probability: Number(form.probability),
    responseStrategy: form.responseStrategy.trim(),
    triggerCondition: form.triggerCondition.trim(),
  };
}

function dateInputValue(value: null | string): string {
  return value?.slice(0, 10) ?? '';
}

function nullableId(value: string): null | string {
  const normalized = value.trim();
  return normalized ? normalized : null;
}

function optionalNumber(value: string): null | number {
  const normalized = value.trim();
  return normalized ? Number(normalized) : null;
}
