export interface HrMetric {
  id: string;
  label: string;
  value: string;
  hint: string;
  tone: 'blue' | 'orange' | 'green' | 'red' | 'purple' | 'cyan';
}

export interface EmployeeMovement {
  id: string;
  name: string;
  department: string;
  position: string;
  date: string;
  type: '入职' | '离职';
}

export interface RecruitmentPosition {
  id: string;
  position: string;
  applications: number;
  interviews: number;
  offers: number;
  status: '招聘中';
}

export interface HrTrendPoint {
  month: string;
  total: number;
  regular: number;
  probation: number;
  cost: number;
}
