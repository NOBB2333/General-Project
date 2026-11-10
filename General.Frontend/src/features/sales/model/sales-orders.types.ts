import type { ListPageRow } from '@/components/page';

export interface SalesOrderRow extends ListPageRow {
  code: string;
  contract: string;
  customer: string;
  amount: string;
  invoicePct: string;
  shipPct: string;
  collectPct: string;
  status: '草稿' | '待审批' | '已审批' | '执行中' | '已发货' | '已完成' | '已取消';
  date: string;
  warehouse: string;
  owner: string;
  updatedAt: string;
}
