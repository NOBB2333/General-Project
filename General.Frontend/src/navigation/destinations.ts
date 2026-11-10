/**
 * Runtime navigation destinations — single source for sidebar path lookup.
 * Generated from menu.inventory.json; do not drive page UI from this file.
 */
import inventoryJson from './menu.inventory.json';

export type PageSpecial = 'products' | 'settings' | 'workbench' | 'onboarding' | 'calendar' | 'arap' | null;

export interface MenuDestination {
  module: string;
  moduleSlug: string;
  label: string;
  kind: 'leaf' | 'top';
  path: string;
  routeName: string;
  pageSlug: string;
  special: PageSpecial;
  badge: number | string | null;
  demoPath?: string;
}

export interface MenuInventory {
  generatedAt: string;
  count: number;
  destinations: MenuDestination[];
}

const inventory = inventoryJson as MenuInventory;

export function getMenuInventory(): MenuInventory {
  return inventory;
}

export function getAllDestinations(): MenuDestination[] {
  return inventory.destinations;
}

export function findDestinationByLabels(mainLabel: string, childLabel?: string): MenuDestination | null {
  if (!childLabel) {
    return inventory.destinations.find((item) => item.module === mainLabel && item.label === mainLabel) ?? null;
  }
  return inventory.destinations.find((item) => item.module === mainLabel && item.label === childLabel) ?? null;
}

export function findDestinationByPath(path: string): MenuDestination | null {
  return inventory.destinations.find((item) => item.path === path) ?? null;
}
