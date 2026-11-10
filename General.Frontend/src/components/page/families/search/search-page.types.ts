import type { PageAction } from '../../page.contracts';
import type { PageModel } from '../../page.types';

export interface SearchPageCopy {
  heading: string;
  description: string;
  inputLabel: string;
  entityLabel: string;
  recentTitle: string;
  recentEmptyTitle: string;
  recentEmptyDescription: string;
  notFoundTitle: string;
  notFoundDescription: string;
  detailCodeLabel: string;
  queryRequiredMessage: string;
}

export interface SearchPageResultFields {
  code: string;
  title: string;
  category?: string;
  timestamp?: string;
  categoryFallback: string;
}

export interface SearchPageModel extends Omit<PageModel, 'surface' | 'actions'> {
  surface: 'search';
  actions: readonly PageAction[];
  copy: SearchPageCopy;
  resultFields: SearchPageResultFields;
  suggestions: readonly string[];
  initialRecentIds: readonly string[];
}
