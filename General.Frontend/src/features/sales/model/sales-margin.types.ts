export type MarginScope = 'all' | 'mine' | 'team' | 'followed';
export type MarginView = 'product' | 'order' | 'customer';
export type MarginRange = '本月' | '本季' | '半年' | '全年';
export type MarginSortDirection = 'asc' | 'desc';

export interface MarginProduct {
  id: string;
  name: string;
  sku: string;
  category: string;
  qty: number;
  revenue: number;
  cost: number;
  grossProfit: number;
  margin: number;
}

export interface MarginOrder {
  id: string;
  customer: string;
  date: string;
  type: '硬件销售' | '设计服务' | '混合订单';
  revenue: number;
  cost: number;
  grossProfit: number;
  margin: number;
  items: number;
  status: '待审核' | '执行中' | '部分发货' | '已完成' | '已取消';
}

export interface MarginCustomer {
  id: string;
  name: string;
  orderCount: number;
  revenue: number;
  cost: number;
  grossProfit: number;
  margin: number;
  lastOrder: string;
  trend: 'up' | 'down' | 'flat';
}

export interface MarginMonth {
  id: string;
  month: string;
  revenue: number;
  cost: number;
  grossProfit: number;
  margin: number;
}

export interface MarginCategory {
  id: string;
  name: string;
  value: number;
  color: string;
}

export interface MarginTotals {
  revenue: number;
  cost: number;
  grossProfit: number;
  margin: number;
  change: number;
}

export type MarginSortKey = 'revenue' | 'cost' | 'grossProfit' | 'margin';
