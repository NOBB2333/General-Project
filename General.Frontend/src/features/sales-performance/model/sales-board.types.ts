import type { Component } from 'vue';

export type SalesBoardSection = 'overview' | 'customers' | 'pipeline' | 'contracts' | 'ranking' | 'personal';
export type SalesBoardScope = 'mine' | 'team' | 'company';
export type SalesBoardPeriod = 'month' | 'quarter' | 'year';
export type SalesBoardMetricTone = 'blue' | 'green' | 'red' | 'orange' | 'purple' | 'cyan' | 'yellow';

export interface SalesBoardTab {
  id: SalesBoardSection;
  label: string;
  badge: string;
  icon: Component;
}

export interface SalesBoardSegment<TValue extends string> {
  id: TValue;
  label: string;
}

export interface SalesBoardMetric {
  id: string;
  label: string;
  value: string;
  unit?: string;
  trend: number;
  tone: SalesBoardMetricTone;
  icon: Component;
  detail: string;
}

export interface SalesBoardTrendPoint {
  month: string;
  performance: number;
  contracts: number;
  collections: number;
}

export interface SalesBoardRanking {
  id: string;
  rank: number;
  name: string;
  team: string;
  amount: string;
  rate: string;
}

export interface SalesBoardReferenceRow {
  id: string;
  section: SalesBoardSection;
  title: string;
  value: string;
}

export interface SalesBoardActivityRow {
  id: string;
  name: string;
  team: string;
  newCustomers: number;
  contacts: number;
  visits: number;
  calls: number;
  followUps: number;
  customers: number;
  conversion: string;
}

export interface SalesBoardPipelineStage {
  id: string;
  label: string;
  count: number;
  amount?: string;
  conversion?: string;
  tone: SalesBoardMetricTone;
}

export interface SalesBoardContractRow {
  id: string;
  code: string;
  name: string;
  customer: string;
  owner: string;
  status: string;
  amount: string;
  outstanding: string;
}

export interface SalesBoardDetailedRanking {
  id: string;
  rank: number;
  name: string;
  team: string;
  performance: string;
  contracts: string;
  collections: string;
  profit: string;
  orders: number;
  share: string;
}

export interface SalesBoardPersonalMetric {
  id: string;
  label: string;
  value: string;
  detail: string;
  trend: number;
  tone: SalesBoardMetricTone;
  icon: Component;
}
