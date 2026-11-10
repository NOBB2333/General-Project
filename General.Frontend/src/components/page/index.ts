export { default as DomainSurface } from './DomainSurface.vue';
export { default as DomainScaffoldSurface } from './scaffold/DomainScaffoldSurface.vue';
export { usePageListState } from './usePageListState';
export { useScaffoldPageState } from './scaffold/useScaffoldPageState';
export {
  CatalogListPage,
  ListPageSurface,
  PAGE_FAMILY_BY_SURFACE,
  ReferenceListPageSurface,
  normalizeListPageModel,
  resolvePageFamily,
  useListPageState,
} from './families';
export { CatalogBoardPage, CatalogCardsPage, ReferenceBoardPageSurface, ReferenceCardsPageSurface } from './families';
export { SearchPageSurface, useSearchPageState } from './families';
export { TargetPageSurface } from './families';
export { cloneReferenceRows, defineReferenceFixture } from './reference';
export type { PageAction, PageActionKind, PageReplicationMeta, PageReplicationStatus } from './page.contracts';
export type { ListPageModel, ListPageRow, ListPageScope, ListPageTab, PageFamily } from './families';
export type { SearchPageModel } from './families';
export type {
  TargetMetricTone,
  TargetPageSurfaceModel,
  TargetPageTab,
  TargetPageUnit,
  TargetSummaryMetric,
} from './families';
export type { PageReferenceFixture, ReferenceViewport } from './reference';
export type {
  PageColumn,
  PageFilter,
  PageModel,
  PagePanel,
  PageRow,
  PageStat,
  PageSurface,
  StatTone,
} from './page.types';
