import type { PageSurface } from '../page.types';

export type PageFamily =
  'list' | 'board' | 'cards' | 'search' | 'calendar' | 'kanban' | 'schedule' | 'target' | 'specialty';

export const PAGE_FAMILY_BY_SURFACE: Record<PageSurface, PageFamily> = {
  list: 'list',
  checklist: 'list',
  settings: 'list',
  workbench: 'list',
  onboarding: 'list',
  board: 'board',
  dashboard: 'board',
  cards: 'cards',
  search: 'search',
  calendar: 'calendar',
  'month-calendar': 'calendar',
  kanban: 'kanban',
  schedule: 'schedule',
  'login-cards': 'specialty',
  'target-board': 'target',
};

export function resolvePageFamily(surface: PageSurface): PageFamily {
  return PAGE_FAMILY_BY_SURFACE[surface];
}
