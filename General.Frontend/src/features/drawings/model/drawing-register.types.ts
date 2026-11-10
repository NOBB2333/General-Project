export type DrawingHeroVariant = 'customer' | 'review' | 'change' | 'distribution';
export type DrawingRegisterLayout = 'customer' | 'filter-first' | 'tabs-first';
export type DrawingColumnPresentation = 'text' | 'primary' | 'status' | 'danger';

export interface DrawingRegisterColumn {
  key: string;
  label: string;
  presentation?: DrawingColumnPresentation;
}

export interface DrawingRegisterFilter {
  key: string;
  label: string;
  placeholder?: string;
  type: 'text' | 'date' | 'select';
  options?: readonly string[];
}

export interface DrawingRegisterTab {
  id: string;
  label: string;
  value?: string;
}

export interface DrawingRegisterMetric {
  id: string;
  label: string;
  value: string;
  tone: 'neutral' | 'blue' | 'green' | 'orange' | 'purple' | 'red';
}

export interface DrawingRegisterRow {
  id: string;
  values: Readonly<Record<string, string>>;
}

export interface DrawingRegisterFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  rows: readonly DrawingRegisterRow[];
  title: string;
  description: string;
  variant: DrawingHeroVariant;
  layout: DrawingRegisterLayout;
  primaryAction: string;
  statusKey: string;
  columns: readonly DrawingRegisterColumn[];
  filters: readonly DrawingRegisterFilter[];
  tabs: readonly DrawingRegisterTab[];
  metrics?: readonly DrawingRegisterMetric[];
}
