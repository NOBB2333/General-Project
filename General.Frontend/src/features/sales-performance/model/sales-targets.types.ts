import type { Component } from 'vue';
import type { TargetMetricTone } from '@/components/page';

export type SalesTargetUnit = 'yuan' | 'thousand' | 'wan' | 'million';
export type SalesTargetTab = 'overview' | 'breakdown' | 'personal' | 'team';

export interface SalesTargetSummaryDefinition {
  id: string;
  label: string;
  description: string;
  targetWan: number;
  actualWan: number;
  actualLabel: string;
  tone: TargetMetricTone;
  icon: Component;
}

export interface SalesTargetTeamRow {
  id: string;
  role: 'team';
  code: string;
  name: string;
  targetWan: number;
  actualWan: number;
  actualPrecision: 1 | 2;
  rate: number;
  memberCount: number;
}

export interface SalesTargetPersonRow {
  id: string;
  role: 'person';
  code: string;
  rank: number;
  name: string;
  teamName: string;
  targetWan: number;
  actualWan: number;
  actualPrecision: 1 | 2;
  rate: number;
}

export type SalesTargetReferenceRow = SalesTargetTeamRow | SalesTargetPersonRow;

export interface SalesTargetDraft {
  name: string;
  kind: 'team' | 'person';
  targetWan: string;
  owner: string;
}
