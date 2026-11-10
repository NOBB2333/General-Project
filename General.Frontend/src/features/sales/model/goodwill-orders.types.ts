export type GoodwillStatus = '草稿' | '待审批' | '已审批' | '发货中' | '已完成' | '已驳回';
export interface GoodwillOrder {
  id: string;
  code: string;
  customer: string;
  type: '备件赠送' | '技术资料' | '样品提供' | '质量补偿' | '客户关怀' | '其他';
  reason: string;
  items: string;
  status: GoodwillStatus;
  date: string;
  owner: string;
}
