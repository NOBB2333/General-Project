import type { PlatformCacheApi } from '@/api/core/platform-cache';
import type { ListPageRow } from '@/components/page';

export interface CacheRow extends ListPageRow {
  area: string;
  description: string;
  id: string;
  version: string;
}

export function toCacheRow(area: PlatformCacheApi.Area): CacheRow {
  return {
    area: area.area,
    description: area.description,
    id: area.area,
    version: area.version,
  };
}

export function hasConfirmedCacheRefresh(
  previous: PlatformCacheApi.Area,
  refreshedAreas: readonly PlatformCacheApi.Area[],
): boolean {
  const refreshed = refreshedAreas.find((area) => area.area.toLocaleLowerCase() === previous.area.toLocaleLowerCase());
  return Boolean(refreshed && refreshed.version && refreshed.version !== previous.version);
}
