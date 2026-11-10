import type { Component } from 'vue';

export type MyPerformanceTab = 'overview' | 'bank' | 'ledger' | 'plans';
export type MyPerformanceTone = 'primary' | 'success' | 'warning' | 'neutral';

export interface MyPerformanceTabDefinition {
  id: MyPerformanceTab;
  label: string;
  badge?: string;
  icon: Component;
}

export interface MyPerformanceMetric {
  id: string;
  kind: 'metric';
  label: string;
  value: string;
  hint: string;
  tone: MyPerformanceTone;
  detail: readonly string[];
}

export interface MyPerformanceReward {
  id: string;
  kind: 'reward';
  label: string;
  amount: string;
  formula: string;
}

export interface MyPerformanceSummary {
  id: string;
  kind: 'summary';
  label: string;
  value: string;
  tone: MyPerformanceTone;
}

export interface MyPerformanceBankEntry {
  id: string;
  kind: 'bank';
  period: string;
  performance: string;
  credited: string;
  status: string;
}

export interface MyPerformanceLedgerEntry {
  id: string;
  kind: 'ledger';
  period: string;
  category: string;
  amount: string;
  state: string;
}

export interface MyPerformancePlan {
  id: string;
  kind: 'plan';
  name: string;
  period: string;
  description: string;
  state: string;
}

export type MyPerformanceReferenceRow =
  | MyPerformanceMetric
  | MyPerformanceReward
  | MyPerformanceSummary
  | MyPerformanceBankEntry
  | MyPerformanceLedgerEntry
  | MyPerformancePlan;

export interface MyPerformanceDetail {
  title: string;
  description: string;
  facts: readonly string[];
}
