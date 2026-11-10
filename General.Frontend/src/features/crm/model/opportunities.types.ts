export type OpportunityStage = '线索' | '意向' | '方案' | '报价' | '谈判' | '赢单' | '输单';
export type OpportunityTraffic = 'green' | 'orange' | 'red';
export type OpportunityPriority = '高' | '中' | '低';
export type OpportunityOwnerScope = 'mine' | 'subordinate';
export type OpportunityScope = 'all' | 'mine' | 'subordinate' | 'following';
export type OpportunityView = 'list' | 'board';
export type OpportunitySource =
  '老客户续签' | '老客户复购' | '招标' | '客户推荐' | '展会获客' | '电话拓客' | '网络营销';

export interface OpportunityRow {
  id: string;
  code: string;
  name: string;
  customer: string;
  contact: string;
  stage: OpportunityStage;
  traffic: OpportunityTraffic;
  priority: OpportunityPriority;
  amount: number;
  probability: number;
  expectedDate: string;
  owner: string;
  ownerScope: OpportunityOwnerScope;
  source: OpportunitySource;
  followed?: boolean;
}

export interface OpportunityDraft {
  name: string;
  customer: string;
  contact: string;
  stage: OpportunityStage;
  traffic: OpportunityTraffic;
  priority: OpportunityPriority;
  amount: string;
  probability: string;
  expectedDate: string;
  owner: string;
  source: OpportunitySource;
}
