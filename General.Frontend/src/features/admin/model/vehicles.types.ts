export type VehicleStatus = '待审批' | '已派车' | '已用车' | '审批中' | '已通过' | '已驳回' | '进行中';
export interface VehicleRequest {
  id: string;
  code: string;
  applicant: string;
  department: string;
  purpose: string;
  route: string;
  time: string;
  mileage: string;
  vehicleType: string;
  dispatch: string;
  status: VehicleStatus;
  approver: string;
}
export interface VehicleDraft {
  purpose: string;
  route: string;
  time: string;
  mileage: string;
  vehicleType: string;
}
export interface VehicleRecord {
  id: string;
  plate: string;
  type: string;
  driver: string;
  status: '可用' | '使用中' | '维修中';
  nextInspection: string;
}
