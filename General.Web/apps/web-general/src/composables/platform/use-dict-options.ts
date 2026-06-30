import type { DictApi } from '#/api/core';

import { computed, ref } from 'vue';

import { getBatchDictItemsApi } from '#/api/core';

type DictOptionFallbackMap = Record<string, DictApi.DictOptionItem[]>;

const dictCache = new Map<string, DictApi.DictOptionItem[]>();

function normalizeFallback(values: string[]): DictApi.DictOptionItem[] {
  return values.map((value) => ({ label: value, tagColor: '', value }));
}

function resolveFallback(
  codes: string[],
  fallback?: DictOptionFallbackMap,
): Record<string, DictApi.DictOptionItem[]> {
  return Object.fromEntries(
    codes.map((code) => [code, fallback?.[code] ?? []]),
  );
}

export function createDictOptionsFallback(
  values: Record<string, string[]>,
): DictOptionFallbackMap {
  return Object.fromEntries(
    Object.entries(values).map(([code, items]) => [
      code,
      normalizeFallback(items),
    ]),
  );
}

export function getDictTagColor(
  options: DictApi.DictOptionItem[],
  value?: null | string,
  fallback = 'default',
) {
  if (!value) {
    return fallback;
  }

  return (
    options.find((item) => item.value === value || item.label === value)
      ?.tagColor || fallback
  );
}

export function useDictOptions(
  codes: string[],
  fallback?: DictOptionFallbackMap,
) {
  const loading = ref(false);
  const optionsMap = ref<Record<string, DictApi.DictOptionItem[]>>(
    resolveFallback(codes, fallback),
  );

  const setOptions = (code: string, options: DictApi.DictOptionItem[]) => {
    optionsMap.value = {
      ...optionsMap.value,
      [code]: options.length > 0 ? options : (fallback?.[code] ?? []),
    };
  };

  const load = async () => {
    const missingCodes = codes.filter((code) => !dictCache.has(code));
    if (missingCodes.length > 0) {
      loading.value = true;
      try {
        const batchItems = await getBatchDictItemsApi(missingCodes);
        for (const code of missingCodes) {
          dictCache.set(code, batchItems[code] ?? []);
        }
      } finally {
        loading.value = false;
      }
    }

    for (const code of codes) {
      setOptions(code, dictCache.get(code) ?? []);
    }
  };

  const getOptions = (code: string) =>
    computed(() => optionsMap.value[code] ?? fallback?.[code] ?? []);
  const getColor = (
    code: string,
    value?: null | string,
    colorFallback = 'default',
  ) =>
    getDictTagColor(
      optionsMap.value[code] ?? fallback?.[code] ?? [],
      value,
      colorFallback,
    );

  return {
    getColor,
    getOptions,
    load,
    loading,
    optionsMap,
  };
}
