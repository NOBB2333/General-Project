export type UserAccountStatus = '启用' | '禁用' | '待激活';
export type UserRole = '管理员' | '项目经理' | '销售经理' | '财务' | '普通员工' | '仓库管理员';
export type UsersRolesTab = 'accounts' | 'roles' | 'permissions';

export interface UserAccountRow {
  id: string;
  name: string;
  jobTitle: string;
  account: string;
  email: string;
  department: string;
  role: UserRole;
  status: UserAccountStatus;
  lastLogin: string;
}

export interface UserAccountDraft {
  name: string;
  jobTitle: string;
  account: string;
  email: string;
  department: string;
  role: UserRole;
}

export interface UserRoleSummary {
  id: string;
  name: UserRole;
  description: string;
  memberCount: number;
  permissionCount: number;
  tone: 'orange' | 'blue' | 'purple' | 'green' | 'gray';
}
