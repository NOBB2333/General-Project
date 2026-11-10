export type ServiceDepartmentStatus = '正常' | '筹建中';

export interface ServiceDepartmentRow {
  id: string;
  parentId: string | null;
  name: string;
  code: string;
  level: 1 | 2;
  manager: string;
  headcount: number;
  status: ServiceDepartmentStatus;
}

export interface ServiceDepartmentDraft {
  name: string;
  code: string;
  manager: string;
  headcount: string;
}
