import type { Component } from 'vue';

export interface TargetPageUnit {
  value: string;
  label: string;
}

export interface TargetPageTab {
  value: string;
  label: string;
  icon: Component;
}

export type TargetMetricTone = 'primary' | 'purple' | 'success';

export interface TargetSummaryMetric {
  id: string;
  label: string;
  description: string;
  rate: number;
  target: string;
  actualLabel: string;
  actual: string;
  tone: TargetMetricTone;
  icon: Component;
}

export interface TargetPageSurfaceModel {
  module: string;
  label: string;
  path: string;
  title: string;
  description: string;
  primaryActionLabel: string;
  periodLabel: string;
  participantLabel: string;
  units: readonly TargetPageUnit[];
  tabs: readonly TargetPageTab[];
  summaries: readonly TargetSummaryMetric[];
}
