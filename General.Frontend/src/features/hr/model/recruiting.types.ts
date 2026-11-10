export type RecruitingSection = '岗位说明' | '招聘岗位' | '招聘管理';
export type PositionStatus = '使用中' | '已停用';

export interface RecruitingPosition {
  id: string;
  code: string;
  name: string;
  department: string;
  level: string;
  approved: number;
  staffed: number;
  salaryRange: string;
  reportsTo: string;
  status: PositionStatus;
  updatedAt: string;
  applications: number;
  interviews: number;
  offers: number;
}

export interface RecruitingPositionDraft {
  name: string;
  department: string;
  level: string;
  approved: string;
  staffed: string;
  salaryRange: string;
  reportsTo: string;
}
