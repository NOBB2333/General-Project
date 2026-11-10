import { reactive } from 'vue';
import { routeNames } from '@/router/route.types';
import { navigationState } from '@/navigation/menu-store';

interface NavigationPreferencesState {
  moduleEnabled: Record<string, boolean>;
  childEnabled: Record<string, Record<string, boolean>>;
  customNames: Record<string, string>;
}

const storageKey = 'fisemap-navigation';

function defaultState(): NavigationPreferencesState {
  return { moduleEnabled: {}, childEnabled: {}, customNames: {} };
}

function readBooleanRecord(value: unknown): Record<string, boolean> {
  if (!value || typeof value !== 'object' || Array.isArray(value)) return {};
  return Object.fromEntries(
    Object.entries(value).filter((entry): entry is [string, boolean] => typeof entry[1] === 'boolean'),
  );
}

function readStoredState(): NavigationPreferencesState {
  const defaults = defaultState();
  try {
    const stored = window.localStorage.getItem(storageKey);
    if (!stored) return defaults;
    const parsed = JSON.parse(stored) as Record<string, unknown>;
    const childEntries =
      parsed.childEnabled && typeof parsed.childEnabled === 'object' && !Array.isArray(parsed.childEnabled)
        ? Object.entries(parsed.childEnabled).map(([parent, children]) => [parent, readBooleanRecord(children)])
        : [];
    const savedNames =
      parsed.customNames && typeof parsed.customNames === 'object' && !Array.isArray(parsed.customNames)
        ? Object.entries(parsed.customNames).filter(
            (entry): entry is [string, string] => typeof entry[1] === 'string' && Boolean(entry[1].trim()),
          )
        : [];
    return {
      moduleEnabled: { ...defaults.moduleEnabled, ...readBooleanRecord(parsed.moduleEnabled) },
      childEnabled: Object.fromEntries(childEntries),
      customNames: Object.fromEntries(savedNames),
    };
  } catch {
    return defaults;
  }
}

const initialState = readStoredState();
const moduleEnabled = reactive<Record<string, boolean>>(initialState.moduleEnabled);
const childEnabled = reactive<Record<string, Record<string, boolean>>>(initialState.childEnabled);
const customNames = reactive<Record<string, string>>(initialState.customNames);

function persist() {
  window.localStorage.setItem(storageKey, JSON.stringify({ moduleEnabled, childEnabled, customNames }));
}

export function useNavigationPreferences() {
  function isModuleEnabled(label: string) {
    return moduleEnabled[label] ?? true;
  }

  function isChildEnabled(parent: string, child: string) {
    return childEnabled[parent]?.[child] ?? true;
  }

  function setModuleEnabled(label: string, enabled: boolean) {
    moduleEnabled[label] = enabled;
    persist();
  }

  function setChildEnabled(parent: string, child: string, enabled: boolean) {
    if (!childEnabled[parent]) childEnabled[parent] = {};
    childEnabled[parent]![child] = enabled;
    persist();
  }

  function setCustomName(label: string, value: string) {
    const next = value.trim();
    if (next && next !== label) customNames[label] = next;
    else delete customNames[label];
    persist();
  }

  function resetNames() {
    Object.keys(customNames).forEach((key) => delete customNames[key]);
    persist();
  }

  function enableAll() {
    for (const item of navigationState.items) {
      moduleEnabled[item.label] = true;
      if (!childEnabled[item.label]) childEnabled[item.label] = {};
      for (const child of item.children) childEnabled[item.label]![child.label] = true;
    }
    persist();
  }

  function keepCoreOnly() {
    for (const item of navigationState.items) {
      const coreChildren = item.children.filter(
        (child) => child.defaultHome || child.routeName === routeNames.settings,
      );
      const isCore = item.defaultHome || item.routeName === routeNames.settings || coreChildren.length > 0;
      moduleEnabled[item.label] = isCore;
      if (!childEnabled[item.label]) childEnabled[item.label] = {};
      for (const child of item.children) {
        childEnabled[item.label]![child.label] = coreChildren.includes(child);
      }
    }
    persist();
  }

  return {
    childEnabled,
    customNames,
    moduleEnabled,
    isChildEnabled,
    isModuleEnabled,
    setChildEnabled,
    setCustomName,
    setModuleEnabled,
    resetNames,
    enableAll,
    keepCoreOnly,
  };
}
