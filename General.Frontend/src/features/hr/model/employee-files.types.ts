export type EmployeeStatus = '在职' | '试用期' | '休假中' | '已离职';
export type EmployeeStatusFilter = '全部' | EmployeeStatus;

export interface EmployeeRow {
  id: string;
  code: string;
  name: string;
  gender: '男' | '女';
  age: number;
  department: string;
  position: string;
  level: string;
  employmentType: '正式';
  status: EmployeeStatus;
  salary: number;
  payroll: boolean;
  joinedAt: string;
  contractExpiresAt: string;
  phone: string;
  education: '大专' | '本科' | '硕士';
}

export interface EmployeeDraft {
  name: string;
  gender: '男' | '女';
  age: string;
  department: string;
  position: string;
  level: string;
  salary: string;
  joinedAt: string;
  phone: string;
}
