export type DeviceWarrantyStatus = '在保' | '即将到期' | '已过保' | '未开始';
export interface RepairDevice {
  id: string;
  serial: string;
  name: string;
  model: string;
  specification: string;
  customer: string;
  owner: string;
  warrantyDate: string;
  warrantyStatus: DeviceWarrantyStatus;
}
export interface RepairDraft {
  contact: string;
  phone: string;
  issue: string;
  urgency: '普通' | '紧急';
}
