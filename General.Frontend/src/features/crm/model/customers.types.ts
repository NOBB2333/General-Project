export type CustomerType = '终端客户' | '集成商' | 'EPC';
export type CustomerLevel = 'S级（战略客户）' | 'A级（重要客户）' | 'B级（一般客户）' | 'C级（小客户）';
export type CustomerTrafficColor = 'green' | 'yellow' | 'red';
export type CustomerScope = 'all' | 'mine' | 'subordinates' | 'followed';

export interface CustomerRow {
  id: string;
  name: string;
  type: CustomerType;
  level: CustomerLevel;
  owner: string;
  ownerId: string;
  contact: string;
  phone: string;
  email: string;
  address: string;
  creditLimit: number;
  paymentDays: number;
  totalSales: number;
  receivable: number;
  tags: string[];
  followed: boolean;
  trafficColor: CustomerTrafficColor;
  trafficReason: string;
  followUpCount: number;
}

export interface CustomerDraft {
  name: string;
  type: CustomerType;
  level: CustomerLevel;
  owner: string;
  contact: string;
  phone: string;
  email: string;
  address: string;
  creditLimit: string;
  paymentDays: string;
}

export interface CustomerFollowUpDraft {
  method: string;
  content: string;
  result: string;
  nextDate: string;
}

export interface CustomerColumn {
  key:
    | 'name'
    | 'trafficLight'
    | 'type'
    | 'level'
    | 'owner'
    | 'contact'
    | 'creditLimit'
    | 'paymentDays'
    | 'totalSales'
    | 'receivable';
  label: string;
  required?: boolean;
}
