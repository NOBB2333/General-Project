export type EquipmentStatus = '运行中' | '维修中' | '闲置' | '已报废';
export interface MaintEquipment {
  id: string;
  code: string;
  name: string;
  model: string;
  manufacturer: string;
  category: string;
  location: string;
  status: EquipmentStatus;
  owner: string;
  lastMaintenance: string;
  nextMaintenance: string;
  overdue?: boolean;
  assetValue: number;
}
export interface MaintenanceOrder {
  id: string;
  code: string;
  equipment: string;
  kind: '维修' | '保养';
  description: string;
  owner: string;
  status: '待处理' | '进行中' | '已完成';
  cost: number;
}
export interface MaintEquipmentDraft {
  code: string;
  name: string;
  model: string;
  manufacturer: string;
  category: string;
  location: string;
  owner: string;
  assetValue: string;
}
export interface MaintenanceDraft {
  equipment: string;
  kind: '维修' | '保养';
  description: string;
  owner: string;
}
