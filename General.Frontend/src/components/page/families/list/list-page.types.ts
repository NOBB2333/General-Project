import type { PageAction, PageReplicationMeta } from '../../page.contracts';
import type { PageColumn, PageFilter, PageModel, PageStat } from '../../page.types';

export interface ListPageRow {
  id: string;
  [key: string]: string | number | boolean | null | undefined;
}

export interface ListPageTab {
  id: string;
  label: string;
  filterKey?: string;
  filterValue?: string;
}

export interface ListPageScope extends ListPageTab {
  count?: number;
}

export interface ListPageModel<TRow extends ListPageRow = ListPageRow> extends Omit<
  PageModel,
  'surface' | 'rows' | 'columns' | 'stats' | 'filters' | 'actions' | 'replication'
> {
  surface: 'list';
  rows: TRow[];
  columns: PageColumn[];
  stats: PageStat[];
  filters: PageFilter[];
  actions: readonly PageAction[];
  listScopes: readonly ListPageScope[];
  listViews: readonly ListPageTab[];
  listTabs: readonly ListPageTab[];
  pageSize: number;
  emptyHint: string;
  replication: PageReplicationMeta;
}
