import type { ProjectApi } from '@/api/core/project';

export interface ProjectQualityChecklistForm {
  category: string;
  description: string;
  title: string;
}

export interface ProjectQualityInspectionForm {
  findingSummary: string;
  inspectionResult: string;
  inspectionTime: string;
  inspectorUserId: string;
  qualityChecklistId: string;
  rectificationRequirement: string;
}

export function createProjectQualityChecklistForm(): ProjectQualityChecklistForm {
  return {
    category: '通用',
    description: '',
    title: '',
  };
}

export function buildProjectQualityChecklistSaveInput(
  form: ProjectQualityChecklistForm,
): ProjectApi.QualityChecklistSaveInput {
  return {
    category: form.category.trim(),
    description: form.description.trim(),
    title: form.title.trim(),
  };
}

export function createProjectQualityInspectionForm(): ProjectQualityInspectionForm {
  return {
    findingSummary: '',
    inspectionResult: '通过',
    inspectionTime: '',
    inspectorUserId: '',
    qualityChecklistId: '',
    rectificationRequirement: '',
  };
}

export function buildProjectQualityInspectionSaveInput(
  form: ProjectQualityInspectionForm,
): ProjectApi.QualityInspectionSaveInput {
  const inspectionTime = form.inspectionTime.trim();
  return {
    findingSummary: form.findingSummary.trim(),
    inspectionResult: form.inspectionResult.trim(),
    inspectionTime: inspectionTime ? (inspectionTime.length === 16 ? `${inspectionTime}:00` : inspectionTime) : null,
    inspectorUserId: form.inspectorUserId,
    qualityChecklistId: form.qualityChecklistId,
    rectificationRequirement: form.rectificationRequirement.trim(),
  };
}
