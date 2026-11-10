export type FundAccountKind = '银行账户' | '承兑汇票账户' | '微信' | '支付宝' | '现金账户' | '其他';

export type FundAccountStatus = '正常' | '冻结' | '注销';

export type FundAccountTone = 'bank' | 'wechat' | 'alipay' | 'cash' | 'other';

export interface FundAccount {
  id: string;
  name: string;
  kind: FundAccountKind;
  category: string;
  status: FundAccountStatus;
  company: string;
  holder: string;
  accountNumber: string;
  branch: string;
  balance: number;
  frozenBalance: number;
  frozenNote?: string;
  openingBalance?: number;
  openingDate?: string;
  purpose: string;
  hasOpeningBalance: boolean;
  tone: FundAccountTone;
}

export interface FundAccountSummary {
  totalBalance: number;
  availableBalance: number;
  activeCount: number;
  totalCount: number;
  trend: readonly number[];
}

export interface FundAccountDraft {
  name: string;
  kind: FundAccountKind;
  category: string;
  status: FundAccountStatus;
  company: string;
  holder: string;
  accountNumber: string;
  branch: string;
  balance: string;
  purpose: string;
}

export type FundAccountAction = 'view' | 'edit' | 'opening' | 'copy' | 'toggle-status' | 'delete';
