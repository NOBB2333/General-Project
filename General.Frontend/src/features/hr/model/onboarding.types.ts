export type MovementType = '入职' | '离职';
export type MovementFilter = '全部' | MovementType;
export type MovementStatus = '待入职' | '入职中' | '已完成' | '已取消' | '待审批' | '交接中' | '已离职' | '已撤回';

export interface OnboardingRecord {
  id: string;
  code: string;
  employeeName: string;
  type: MovementType;
  department: string;
  position: string;
  date: string;
  status: MovementStatus;
  owner: string;
  completedSteps: number;
  totalSteps: number;
}

export interface OnboardingDraft {
  employeeName: string;
  type: MovementType;
  department: string;
  position: string;
  date: string;
  owner: string;
}
