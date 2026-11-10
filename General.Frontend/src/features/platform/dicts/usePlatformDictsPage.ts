import { computed, ref } from 'vue';
import {
  createDictDataApi,
  createDictTypeApi,
  deleteDictDataApi,
  deleteDictTypeApi,
  getDictDataApi,
  getDictItemsBatchApi,
  getDictTypesApi,
  updateDictDataApi,
  updateDictTypeApi,
} from '@/api/core/dict';
import type { DictApi } from '@/api/core/dict';
import { toast } from '@/components/ui';
import {
  createDictDataDraft,
  createDictTypeDraft,
  displayDictText,
  normalizeDictCode,
  toDictDataSaveInput,
  toDictTypeSaveInput,
  validateDictDataDraft,
  validateDictTypeDraft,
} from './dict.model';
import type { DictDataDraft, DictTypeDraft } from './dict.model';

type FormMode = 'create' | 'edit';

export function usePlatformDictsPage() {
  const types = ref<DictApi.Type[]>([]);
  const rows = ref<DictApi.Data[]>([]);
  const selectedTypeId = ref('');
  const enabledOptionsByCode = ref<Record<string, DictApi.Item[]>>({});
  const typeSearch = ref('');
  const dataSearch = ref('');
  const isLoading = ref(false);
  const isDataLoading = ref(false);
  const typeLoadError = ref('');
  const dataLoadError = ref('');
  const optionLoadError = ref('');

  const typeFormOpen = ref(false);
  const typeFormMode = ref<FormMode>('create');
  const typeEditingId = ref<string | null>(null);
  const typeDraft = ref<DictTypeDraft>(createDictTypeDraft());
  const typeFormError = ref('');
  const typeSaving = ref(false);

  const dataFormOpen = ref(false);
  const dataFormMode = ref<FormMode>('create');
  const dataEditingId = ref<string | null>(null);
  const dataDraft = ref<DictDataDraft>(createDictDataDraft());
  const dataFormError = ref('');
  const dataSaving = ref(false);
  const savingEnabledId = ref<string | null>(null);

  const deleteTypeTarget = ref<DictApi.Type | null>(null);
  const deleteDataTarget = ref<DictApi.Data | null>(null);
  const deletingType = ref(false);
  const deletingData = ref(false);

  const selectedType = computed(() => types.value.find((type) => type.id === selectedTypeId.value) ?? null);
  const publicItems = computed(() => {
    const code = selectedType.value ? normalizeDictCode(selectedType.value.code) : '';
    return code ? (enabledOptionsByCode.value[code] ?? []) : [];
  });
  const filteredTypes = computed(() => {
    const query = typeSearch.value.trim().toLowerCase();
    if (!query) return types.value;
    return types.value.filter((type) => `${type.name} ${type.code} ${type.remark}`.toLowerCase().includes(query));
  });
  const filteredData = computed(() => {
    const query = dataSearch.value.trim().toLowerCase();
    if (!query) return rows.value;
    return rows.value.filter((row) =>
      `${row.label} ${row.value} ${row.tagColor} ${row.remark}`.toLowerCase().includes(query),
    );
  });
  const totalDataCount = computed(() => types.value.reduce((sum, type) => sum + safeCount(type.dataCount), 0));
  const enabledOptionCount = computed(() =>
    Object.values(enabledOptionsByCode.value).reduce((sum, items) => sum + items.length, 0),
  );
  const disabledDataCount = computed(() =>
    types.value.reduce(
      (sum, type) => sum + Math.max(0, safeCount(type.dataCount) - safeCount(type.enabledDataCount)),
      0,
    ),
  );
  const metrics = computed(
    () =>
      [
        { id: 'types', label: '字典类型', value: String(types.value.length), tone: 'primary' },
        { id: 'items', label: '维护项', value: String(totalDataCount.value), tone: 'purple' },
        {
          id: 'options',
          label: '公共可用项',
          value: optionLoadError.value ? '—' : String(enabledOptionCount.value),
          tone: 'success',
        },
        { id: 'disabled', label: '已停用项', value: String(disabledDataCount.value), tone: 'warning' },
      ] as const,
  );

  async function loadWorkspace(preferredCode?: string) {
    isLoading.value = true;
    typeLoadError.value = '';
    try {
      const nextTypes = (await getDictTypesApi()).map(normalizeTypeDto);
      const requestedCode = preferredCode ? normalizeDictCode(preferredCode) : '';
      const currentType = types.value.find((type) => type.id === selectedTypeId.value);
      const nextSelection =
        nextTypes.find((type) => normalizeDictCode(type.code) === requestedCode) ??
        nextTypes.find((type) => type.id === currentType?.id) ??
        nextTypes[0] ??
        null;
      types.value = nextTypes;
      selectedTypeId.value = nextSelection?.id ?? '';

      await Promise.all([loadPublicOptions(nextTypes), loadSelectedData(nextSelection)]);
    } catch (error) {
      typeLoadError.value = errorMessage(error, '字典类型读取失败，请重试。');
      toast.error(typeLoadError.value);
    } finally {
      isLoading.value = false;
    }
  }

  async function loadPublicOptions(nextTypes: readonly DictApi.Type[]) {
    optionLoadError.value = '';
    if (!nextTypes.length) {
      enabledOptionsByCode.value = {};
      return;
    }

    try {
      const response = await getDictItemsBatchApi(nextTypes.map((type) => type.code));
      enabledOptionsByCode.value = Object.fromEntries(
        Object.entries(response ?? {}).map(([code, items]) => [
          normalizeDictCode(code),
          Array.isArray(items) ? items.map(normalizeItemDto) : [],
        ]),
      );
    } catch (error) {
      enabledOptionsByCode.value = {};
      optionLoadError.value = errorMessage(error, '公共字典选项读取失败。');
      toast.warning(optionLoadError.value);
    }
  }

  async function loadSelectedData(type = selectedType.value) {
    dataLoadError.value = '';
    if (!type) {
      rows.value = [];
      return;
    }

    isDataLoading.value = true;
    try {
      const data = await getDictDataApi(type.code);
      rows.value = Array.isArray(data) ? data.map(normalizeDataDto) : [];
    } catch (error) {
      rows.value = [];
      dataLoadError.value = errorMessage(error, `“${displayDictText(type.name)}”字典项读取失败。`);
      toast.error(dataLoadError.value);
    } finally {
      isDataLoading.value = false;
    }
  }

  async function selectType(type: DictApi.Type) {
    selectedTypeId.value = type.id;
    dataSearch.value = '';
    await loadSelectedData(type);
  }

  function openCreateType() {
    typeFormMode.value = 'create';
    typeEditingId.value = null;
    typeDraft.value = createDictTypeDraft();
    typeFormError.value = '';
    typeFormOpen.value = true;
  }

  function openEditType(type: DictApi.Type) {
    typeFormMode.value = 'edit';
    typeEditingId.value = type.id;
    typeDraft.value = createDictTypeDraft(type);
    typeFormError.value = '';
    typeFormOpen.value = true;
  }

  async function saveType() {
    const validation = validateDictTypeDraft(typeDraft.value, types.value, typeEditingId.value ?? undefined);
    if (validation) {
      typeFormError.value = validation;
      toast.warning(validation);
      return;
    }

    typeSaving.value = true;
    typeFormError.value = '';
    try {
      const input = toDictTypeSaveInput(
        typeDraft.value,
        typeEditingId.value ? selectedTypeById(typeEditingId.value)?.code : undefined,
      );
      const saved = typeEditingId.value
        ? await updateDictTypeApi(typeEditingId.value, input)
        : await createDictTypeApi(input);
      typeFormOpen.value = false;
      toast.success(typeEditingId.value ? '字典类型已更新。' : '字典类型已创建。');
      await loadWorkspace(saved.code);
    } catch (error) {
      typeFormError.value = errorMessage(error, '字典类型保存失败，请检查输入或权限后重试。');
      toast.error(typeFormError.value);
    } finally {
      typeSaving.value = false;
    }
  }

  function requestDeleteType(type: DictApi.Type) {
    deleteTypeTarget.value = type;
  }

  async function confirmDeleteType() {
    const target = deleteTypeTarget.value;
    if (!target) return;
    deletingType.value = true;
    try {
      await deleteDictTypeApi(target.id);
      deleteTypeTarget.value = null;
      toast.success('字典类型及其字典项已删除。');
      await loadWorkspace();
    } catch (error) {
      toast.error(errorMessage(error, '字典类型删除失败。'));
      await loadWorkspace();
    } finally {
      deletingType.value = false;
    }
  }

  function openCreateData() {
    dataFormMode.value = 'create';
    dataEditingId.value = null;
    dataDraft.value = createDictDataDraft();
    dataFormError.value = '';
    dataFormOpen.value = true;
  }

  function openEditData(data: DictApi.Data) {
    dataFormMode.value = 'edit';
    dataEditingId.value = data.id;
    dataDraft.value = createDictDataDraft(data);
    dataFormError.value = '';
    dataFormOpen.value = true;
  }

  async function saveData() {
    const type = selectedType.value;
    if (!type) return;
    const validation = validateDictDataDraft(dataDraft.value, rows.value, dataEditingId.value ?? undefined);
    if (validation) {
      dataFormError.value = validation;
      toast.warning(validation);
      return;
    }

    dataSaving.value = true;
    dataFormError.value = '';
    try {
      const input = toDictDataSaveInput(dataDraft.value);
      if (dataEditingId.value) await updateDictDataApi(dataEditingId.value, input);
      else await createDictDataApi(type.code, input);
      dataFormOpen.value = false;
      toast.success(dataEditingId.value ? '字典项已更新。' : '字典项已创建。');
      await loadWorkspace(type.code);
    } catch (error) {
      dataFormError.value = errorMessage(error, '字典项保存失败，请检查输入或权限后重试。');
      toast.error(dataFormError.value);
    } finally {
      dataSaving.value = false;
    }
  }

  async function setDataEnabled(data: DictApi.Data, isEnabled: boolean) {
    savingEnabledId.value = data.id;
    try {
      await updateDictDataApi(data.id, { ...toDictDataSaveInput(createDictDataDraft(data)), isEnabled });
      toast.success(isEnabled ? '字典项已启用。' : '字典项已停用。');
      await loadWorkspace(selectedType.value?.code);
    } catch (error) {
      toast.error(errorMessage(error, '字典项状态更新失败。'));
      await loadWorkspace(selectedType.value?.code);
    } finally {
      savingEnabledId.value = null;
    }
  }

  function requestDeleteData(data: DictApi.Data) {
    deleteDataTarget.value = data;
  }

  async function confirmDeleteData() {
    const target = deleteDataTarget.value;
    const code = selectedType.value?.code;
    if (!target || !code) return;
    deletingData.value = true;
    try {
      await deleteDictDataApi(target.id);
      deleteDataTarget.value = null;
      toast.success('字典项已删除。');
      await loadWorkspace(code);
    } catch (error) {
      toast.error(errorMessage(error, '字典项删除失败。'));
      await loadWorkspace(code);
    } finally {
      deletingData.value = false;
    }
  }

  function selectedTypeById(id: string) {
    return types.value.find((type) => type.id === id);
  }

  return {
    confirmDeleteData,
    confirmDeleteType,
    dataDraft,
    dataFormError,
    dataFormMode,
    dataFormOpen,
    dataLoadError,
    dataSaving,
    dataSearch,
    deleteDataTarget,
    deleteTypeTarget,
    deletingData,
    deletingType,
    enabledOptionCount,
    filteredData,
    filteredTypes,
    isDataLoading,
    isLoading,
    loadPublicOptions,
    loadWorkspace,
    metrics,
    openCreateData,
    openCreateType,
    openEditData,
    openEditType,
    optionLoadError,
    publicItems,
    requestDeleteData,
    requestDeleteType,
    rows,
    saveData,
    saveType,
    savingEnabledId,
    selectType,
    selectedType,
    selectedTypeId,
    setDataEnabled,
    typeDraft,
    typeFormError,
    typeFormMode,
    typeFormOpen,
    typeLoadError,
    typeSaving,
    typeSearch,
    types,
  };
}

function normalizeTypeDto(type: DictApi.Type): DictApi.Type {
  return {
    code: safeString(type?.code),
    dataCount: safeCount(type?.dataCount),
    enabledDataCount: safeCount(type?.enabledDataCount),
    id: safeString(type?.id),
    name: safeString(type?.name),
    remark: safeString(type?.remark),
    sort: safeCount(type?.sort),
  };
}

function normalizeDataDto(data: DictApi.Data): DictApi.Data {
  return {
    dictTypeId: safeString(data?.dictTypeId),
    id: safeString(data?.id),
    isEnabled: data?.isEnabled === true,
    label: safeString(data?.label),
    remark: safeString(data?.remark),
    sort: safeCount(data?.sort),
    tagColor: safeString(data?.tagColor),
    value: safeString(data?.value),
  };
}

function normalizeItemDto(item: DictApi.Item): DictApi.Item {
  return {
    label: safeString(item?.label),
    tagColor: safeString(item?.tagColor),
    value: safeString(item?.value),
  };
}

function safeCount(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) && value >= 0 ? value : 0;
}

function safeString(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

function errorMessage(error: unknown, fallback: string): string {
  return error instanceof Error && error.message.trim() ? error.message : fallback;
}
