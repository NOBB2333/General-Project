import type { PageAction, PageReplicationMeta } from './page.contracts';

export type StatTone = 'primary' | 'success' | 'warning' | 'danger' | 'neutral';

export interface PageColumn {
  key: string;
  title: string;
  width: number;
  presentation?: 'text' | 'status' | 'progress';
  progressTone?: 'primary' | 'warning' | 'success';
  /** When false, hidden by default in column settings */
  visible?: boolean;
  /** Prefer fixed/sticky presentation in table */
  sticky?: boolean;
}

export interface PageStat {
  key: string;
  label: string;
  value: string;
  tone: StatTone;
  hint?: string;
  /** Optional filter key applied when stat card is clicked */
  filterKey?: string;
  filterValue?: string;
}

export interface PageFilter {
  key: string;
  label: string;
  options: string[];
}

export interface PagePanel {
  key: string;
  title: string;
  items: string[];
}

export interface PageRow {
  id: string;
  [key: string]: string | number | boolean | null | undefined;
}

export type PageSurface =
  | 'list'
  | 'board'
  | 'cards'
  | 'search'
  | 'kanban'
  | 'login-cards'
  | 'target-board'
  | 'schedule'
  | 'month-calendar'
  | 'calendar'
  | 'dashboard'
  | 'checklist'
  | 'settings'
  | 'workbench'
  | 'onboarding';

export type PageRowAction =
  | 'view'
  | 'edit'
  | 'copy'
  | 'delete'
  | 'status'
  | 'export'
  | 'download'
  | 'maintenance'
  | 'correct'
  | 'withdraw'
  | 'reimburse';

export interface PageModel {
  key: string;
  module: string;
  moduleSlug: string;
  label: string;
  path: string;
  routeName: string;
  pageSlug: string;
  demoPath: string;
  surface: PageSurface;
  description: string;
  searchPlaceholder: string;
  tabs: string[];
  tools: string[];
  /** Extra tools shown in overflow menu */
  moreTools?: string[];
  filters: PageFilter[];
  columns: PageColumn[];
  stats: PageStat[];
  panels: PagePanel[];
  rows: PageRow[];
  primaryAction: string;
  /** Default page size for list pagination */
  pageSize?: number;
  /** Empty-state copy */
  emptyHint?: string;
  /** Row-level action labels */
  rowActions?: PageRowAction[];
  /** Stable action contracts for verified pages; scaffold labels remain supported temporarily. */
  actions?: PageAction[];
  /** Evidence state. Omitted legacy models are treated as structural scaffolds. */
  replication?: PageReplicationMeta;
}
