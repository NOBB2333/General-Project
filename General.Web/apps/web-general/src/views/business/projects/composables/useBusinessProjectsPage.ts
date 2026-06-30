import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { message } from 'ant-design-vue';

import {
  createBusinessAcceptanceApi,
  createBusinessBudgetApi,
  createBusinessChainApi,
  createBusinessContractApi,
  createBusinessInvoiceApi,
  createBusinessProcurementApi,
  createBusinessReceivableApi,
  deleteBusinessAcceptanceApi,
  deleteBusinessBudgetApi,
  deleteBusinessChainApi,
  deleteBusinessContractApi,
  deleteBusinessInvoiceApi,
  deleteBusinessProcurementApi,
  deleteBusinessReceivableApi,
  getBusinessProjectDetailApi,
  getBusinessProjectListApi,
  getUserInfoApi,
  type BusinessApi,
  updateBusinessAcceptanceApi,
  updateBusinessBudgetApi,
  updateBusinessChainApi,
  updateBusinessContractApi,
  updateBusinessInvoiceApi,
  updateBusinessProcurementApi,
  updateBusinessReceivableApi,
} from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';

export function useBusinessProjectsPage() {
  const route = useRoute();
  const router = useRouter();
  type FocusRecordType =
    | 'acceptance'
    | 'budget'
    | 'businessChain'
    | 'contract'
    | 'invoice'
    | 'procurement'
    | 'receivable';

  const listKeyword = ref('');
  const loadingList = ref(false);
  const loadingDetail = ref(false);
  const refreshingDetail = ref(false);
  const savingAcceptance = ref(false);
  const savingBudget = ref(false);
  const savingBusinessChain = ref(false);
  const savingContract = ref(false);
  const savingInvoice = ref(false);
  const savingProcurement = ref(false);
  const savingReceivable = ref(false);
  const sidebarCollapsed = ref(false);
  const currentUserId = ref('');
  const currentUserName = ref('');
  const projects = ref<BusinessApi.ProjectSummary[]>([]);
  const detail = ref<BusinessApi.ProjectDetail | null>(null);
  const selectedProjectId = ref(
    typeof route.query.projectId === 'string' ? route.query.projectId : '',
  );
  const activeTab = ref(
    typeof route.query.tab === 'string' ? route.query.tab : 'overview',
  );
  const focusRecordType = ref<FocusRecordType | ''>(
    parseFocusRecordType(route.query.focusType),
  );
  const focusRecordId = ref(
    typeof route.query.focusId === 'string' ? route.query.focusId : '',
  );
  const showBudgetForm = ref(false);
  const showAcceptanceForm = ref(false);
  const showBusinessChainForm = ref(false);
  const showContractForm = ref(false);
  const showInvoiceForm = ref(false);
  const showProcurementForm = ref(false);
  const showReceivableForm = ref(false);
  const editingAcceptanceId = ref('');
  const editingBudgetId = ref('');
  const editingBusinessChainId = ref('');
  const editingContractId = ref('');
  const editingInvoiceId = ref('');
  const editingProcurementId = ref('');
  const editingReceivableId = ref('');
  const businessDicts = useDictOptions(
    ['business_close_status', 'contract_type'],
    createDictOptionsFallback({
      business_close_status: ['经营执行中', '经营收尾中', '执行关闭待回款'],
      contract_type: ['主合同', '补充合同', '分包合同', '采购合同'],
    }),
  );

  const budgetForm = ref<BusinessApi.BudgetExecutionSaveInput>({
    adjustedAmount: 0,
    budgetCode: '',
    category: '',
    executedAmount: 0,
    sortOrder: 0,
    varianceAmount: undefined,
  });
  const acceptanceForm = ref<BusinessApi.AcceptanceSaveInput>({
    acceptanceCode: '',
    acceptanceDate: undefined,
    acceptanceResult: '通过',
    linkedContractCode: '',
    milestoneId: undefined,
    qualityInspectionCode: '',
    status: '已验收',
    title: '',
  });
  const businessChainForm = ref<BusinessApi.BusinessChainSaveInput>({
    chainCode: '',
    linkedContractCode: '',
    ownerUserId: '',
    sourceChangeCode: '',
    stage: '合同',
    status: '进行中',
    summary: '',
    title: '',
    type: '经营事项',
  });
  const contractForm = ref<BusinessApi.ContractSaveInput>({
    amount: 0,
    contractCode: '',
    counterpartyName: '',
    isRevenueContract: true,
    parentContractCode: '',
    signDate: undefined,
    sourceChangeCode: '',
    status: '执行中',
    title: '',
    type: '主合同',
  });
  const invoiceForm = ref<BusinessApi.InvoiceSaveInput>({
    amount: 0,
    invoiceCode: '',
    invoiceDate: undefined,
    linkedContractCode: '',
    receivableCode: '',
    status: '已开票',
    taxAmount: 0,
    title: '',
  });
  const procurementForm = ref<BusinessApi.ProcurementSaveInput>({
    amount: 0,
    linkedContractCode: '',
    procurementCode: '',
    signDate: undefined,
    sourceChangeCode: '',
    stage: '询价',
    status: '进行中',
    supplierName: '',
    title: '',
  });
  const receivableForm = ref<BusinessApi.ReceivableSaveInput>({
    invoiceCode: '',
    linkedContractCode: '',
    plannedDate: undefined,
    receivableCode: '',
    receivedAmount: 0,
    status: '待回款',
    title: '',
    totalAmount: 0,
  });

  let detailRequestSerial = 0;

  const currentProject = computed(
    () =>
      projects.value.find((item) => item.id === selectedProjectId.value) ??
      detail.value?.summary ??
      null,
  );

  const projectSelectOptions = computed(() =>
    projects.value.map((item) => ({
      label: `${item.projectCode} · ${item.name}`,
      value: item.id,
    })),
  );

  const ownerSelectOptions = computed(() => {
    const optionMap = new Map<string, string>();
    if (currentUserId.value) {
      optionMap.set(currentUserId.value, currentUserName.value || '当前用户');
    }
    for (const item of detail.value?.businessChains ?? []) {
      if (item.ownerUserId) {
        optionMap.set(item.ownerUserId, item.ownerName || item.ownerUserId);
      }
    }

    return [...optionMap.entries()].map(([value, label]) => ({
      label,
      value,
    }));
  });
  const contractTypeOptions = businessDicts.getOptions('contract_type');

  const contractRelationItems = computed(() => {
    if (!detail.value) {
      return [];
    }

    const incomeContracts = detail.value.contracts.filter(
      (item) => item.isRevenueContract,
    );
    const downstreamContracts = detail.value.contracts.filter(
      (item) => !item.isRevenueContract,
    );

    return incomeContracts.map((contract) => ({
      id: contract.id,
      title: contract.title,
      code: contract.contractCode,
      type: contract.type,
      status: contract.status,
      changeCode: contract.sourceChangeCode,
      linkedProcurements:
        detail.value?.procurements.filter(
          (item) => item.linkedContractCode === contract.contractCode,
        ) ?? [],
      childContracts: downstreamContracts.filter(
        (item) => item.parentContractCode === contract.contractCode,
      ),
    }));
  });

  const closingItems = computed(() => {
    if (!detail.value) {
      return [];
    }

    const totalReceivable = detail.value.receivables.reduce(
      (sum, item) => sum + (item.totalAmount ?? 0),
      0,
    );
    const receivedAmount = detail.value.receivables.reduce(
      (sum, item) => sum + (item.receivedAmount ?? 0),
      0,
    );
    const openReceivables = detail.value.receivables.filter((item) =>
      item.status.includes('待回款'),
    );
    const hasPendingProcurement = detail.value.procurements.some(
      (item) =>
        !item.status.includes('已签订') && !item.status.includes('完成'),
    );

    return [
      {
        key: 'close-status',
        label: '经营收尾状态',
        value: detail.value.summary.businessCloseStatus,
        status:
          detail.value.summary.businessCloseStatus.includes('收尾') ||
          detail.value.summary.businessCloseStatus.includes('待回款')
            ? 'warning'
            : 'normal',
      },
      {
        key: 'receivable-gap',
        label: '待回款余额',
        value: formatMoney(totalReceivable - receivedAmount),
        status: openReceivables.length > 0 ? 'warning' : 'normal',
      },
      {
        key: 'pending-receivable-count',
        label: '待回款事项',
        value: `${openReceivables.length} 项`,
        status: openReceivables.length > 0 ? 'warning' : 'normal',
      },
      {
        key: 'procurement-close',
        label: '分包采购闭环',
        value: hasPendingProcurement ? '仍有未闭环事项' : '已闭环',
        status: hasPendingProcurement ? 'warning' : 'normal',
      },
    ];
  });

  function formatMoney(value?: null | number) {
    if (typeof value !== 'number') {
      return '--';
    }

    return new Intl.NumberFormat('zh-CN', {
      maximumFractionDigits: 0,
      minimumFractionDigits: 0,
      style: 'currency',
      currency: 'CNY',
    }).format(value);
  }

  function getBusinessCloseStatusColor(status?: null | string) {
    return businessDicts.getColor(
      'business_close_status',
      status,
      status?.includes('待回款') || status?.includes('收尾') ? 'gold' : 'blue',
    );
  }

  function getCollapsedProjectLabel(name: string) {
    const compact = name.replace(/\s+/g, '');
    const asciiWords = compact.match(/[A-Za-z0-9]+/g);
    if (asciiWords && asciiWords.length > 0) {
      const tail = asciiWords.join('').slice(-3).toUpperCase();
      if (tail) {
        return tail;
      }
    }

    const digitTail = compact.match(/\d+/g)?.join('').slice(-3);
    if (digitTail) {
      return digitTail;
    }

    return compact.length <= 1 ? compact : compact.slice(0, 1);
  }

  async function loadProjects() {
    loadingList.value = true;
    try {
      const nextProjects = await getBusinessProjectListApi(
        listKeyword.value.trim() || undefined,
      );
      projects.value = nextProjects;

      if (nextProjects.length === 0) {
        selectedProjectId.value = '';
        detail.value = null;
        return;
      }

      const preferredProjectId =
        selectedProjectId.value ||
        (typeof route.query.projectId === 'string'
          ? route.query.projectId
          : '');
      const matchedProject =
        nextProjects.find((item) => item.id === preferredProjectId) ??
        nextProjects[0];

      if (matchedProject && matchedProject.id !== selectedProjectId.value) {
        selectedProjectId.value = matchedProject.id;
      }
    } catch (error) {
      showRequestError(error, '经营项目列表加载失败');
    } finally {
      loadingList.value = false;
    }
  }

  function parseFocusRecordType(value: unknown): FocusRecordType | '' {
    if (
      value === 'acceptance' ||
      value === 'budget' ||
      value === 'businessChain' ||
      value === 'contract' ||
      value === 'invoice' ||
      value === 'procurement' ||
      value === 'receivable'
    ) {
      return value;
    }

    return '';
  }

  function syncRouteFocus() {
    focusRecordType.value = parseFocusRecordType(route.query.focusType);
    focusRecordId.value =
      typeof route.query.focusId === 'string' ? route.query.focusId : '';
  }

  function isFocusedRecord(type: FocusRecordType, recordId?: string) {
    return (
      Boolean(recordId) &&
      focusRecordType.value === type &&
      focusRecordId.value === recordId
    );
  }

  function getFocusedRowClass(type: FocusRecordType, record: { id?: string }) {
    return isFocusedRecord(type, record.id)
      ? 'business-projects__focused-row'
      : '';
  }

  async function loadDetail(projectId = selectedProjectId.value) {
    if (!projectId) {
      detail.value = null;
      return;
    }

    const requestId = ++detailRequestSerial;
    if (detail.value) {
      refreshingDetail.value = true;
    } else {
      loadingDetail.value = true;
    }

    try {
      const nextDetail = await getBusinessProjectDetailApi(projectId);
      if (requestId !== detailRequestSerial) {
        return;
      }
      detail.value = nextDetail;
      syncBusinessForms();
    } catch (error) {
      if (requestId === detailRequestSerial) {
        showRequestError(error, '经营项目详情加载失败');
      }
    } finally {
      if (requestId === detailRequestSerial) {
        loadingDetail.value = false;
        refreshingDetail.value = false;
      }
    }
  }

  function selectProject(projectId: string, tab = activeTab.value) {
    activeTab.value = tab;
    selectedProjectId.value = projectId;
    focusRecordId.value = '';
    focusRecordType.value = '';
    void loadDetail(projectId);
  }

  function handleProjectChange(value: unknown) {
    if (typeof value === 'string') {
      selectProject(value);
    }
  }

  async function searchProjects() {
    await loadProjects();
    await loadDetail(selectedProjectId.value);
  }

  async function resetProjects() {
    listKeyword.value = '';
    await loadProjects();
    await loadDetail(selectedProjectId.value);
  }

  function openGlobalSearch() {
    router.push('/business/reports');
  }

  function syncBusinessForms() {
    businessChainForm.value.ownerUserId =
      businessChainForm.value.ownerUserId || currentUserId.value;
  }

  function resetBudgetForm() {
    editingBudgetId.value = '';
    budgetForm.value = {
      adjustedAmount: 0,
      budgetCode: '',
      category: '',
      executedAmount: 0,
      sortOrder: 0,
      varianceAmount: undefined,
    };
  }

  function resetAcceptanceForm() {
    editingAcceptanceId.value = '';
    acceptanceForm.value = {
      acceptanceCode: '',
      acceptanceDate: undefined,
      acceptanceResult: '通过',
      linkedContractCode: '',
      milestoneId: undefined,
      qualityInspectionCode: '',
      status: '已验收',
      title: '',
    };
  }

  function resetBusinessChainForm() {
    editingBusinessChainId.value = '';
    businessChainForm.value = {
      chainCode: '',
      linkedContractCode: '',
      ownerUserId: currentUserId.value,
      sourceChangeCode: '',
      stage: '合同',
      status: '进行中',
      summary: '',
      title: '',
      type: '经营事项',
    };
  }

  function resetContractForm() {
    editingContractId.value = '';
    contractForm.value = {
      amount: 0,
      contractCode: '',
      counterpartyName: '',
      isRevenueContract: true,
      parentContractCode: '',
      signDate: undefined,
      sourceChangeCode: '',
      status: '执行中',
      title: '',
      type: '主合同',
    };
  }

  function resetProcurementForm() {
    editingProcurementId.value = '';
    procurementForm.value = {
      amount: 0,
      linkedContractCode: '',
      procurementCode: '',
      signDate: undefined,
      sourceChangeCode: '',
      stage: '询价',
      status: '进行中',
      supplierName: '',
      title: '',
    };
  }

  function resetInvoiceForm() {
    editingInvoiceId.value = '';
    invoiceForm.value = {
      amount: 0,
      invoiceCode: '',
      invoiceDate: undefined,
      linkedContractCode: '',
      receivableCode: '',
      status: '已开票',
      taxAmount: 0,
      title: '',
    };
  }

  function resetReceivableForm() {
    editingReceivableId.value = '';
    receivableForm.value = {
      invoiceCode: '',
      linkedContractCode: '',
      plannedDate: undefined,
      receivableCode: '',
      receivedAmount: 0,
      status: '待回款',
      title: '',
      totalAmount: 0,
    };
  }

  function toggleBudgetForm() {
    if (showBudgetForm.value) {
      showBudgetForm.value = false;
      resetBudgetForm();
      return;
    }
    resetBudgetForm();
    showBudgetForm.value = true;
  }

  function toggleAcceptanceForm() {
    if (showAcceptanceForm.value) {
      showAcceptanceForm.value = false;
      resetAcceptanceForm();
      return;
    }
    resetAcceptanceForm();
    showAcceptanceForm.value = true;
  }

  function toggleBusinessChainForm() {
    if (showBusinessChainForm.value) {
      showBusinessChainForm.value = false;
      resetBusinessChainForm();
      return;
    }
    resetBusinessChainForm();
    showBusinessChainForm.value = true;
  }

  function toggleContractForm() {
    if (showContractForm.value) {
      showContractForm.value = false;
      resetContractForm();
      return;
    }
    resetContractForm();
    showContractForm.value = true;
  }

  function toggleProcurementForm() {
    if (showProcurementForm.value) {
      showProcurementForm.value = false;
      resetProcurementForm();
      return;
    }
    resetProcurementForm();
    showProcurementForm.value = true;
  }

  function toggleInvoiceForm() {
    if (showInvoiceForm.value) {
      showInvoiceForm.value = false;
      resetInvoiceForm();
      return;
    }
    resetInvoiceForm();
    showInvoiceForm.value = true;
  }

  function toggleReceivableForm() {
    if (showReceivableForm.value) {
      showReceivableForm.value = false;
      resetReceivableForm();
      return;
    }
    resetReceivableForm();
    showReceivableForm.value = true;
  }

  function toDateInputValue(value?: null | string) {
    return value ? value.slice(0, 10) : undefined;
  }

  function editBudget(record: BusinessApi.BudgetExecutionItem) {
    editingBudgetId.value = record.id;
    budgetForm.value = {
      adjustedAmount: record.adjustedAmount ?? 0,
      budgetCode: record.budgetCode,
      category: record.category,
      executedAmount: record.executedAmount ?? 0,
      sortOrder: record.sortOrder ?? 0,
      varianceAmount: record.varianceAmount ?? undefined,
    };
    showBudgetForm.value = true;
  }

  function editAcceptance(record: BusinessApi.AcceptanceItem) {
    editingAcceptanceId.value = record.id;
    acceptanceForm.value = {
      acceptanceCode: record.acceptanceCode,
      acceptanceDate: toDateInputValue(record.acceptanceDate),
      acceptanceResult: record.acceptanceResult,
      linkedContractCode: record.linkedContractCode,
      milestoneId: record.milestoneId ?? undefined,
      qualityInspectionCode: record.qualityInspectionCode,
      status: record.status,
      title: record.title,
    };
    showAcceptanceForm.value = true;
  }

  function editBusinessChain(record: BusinessApi.BusinessChainItem) {
    editingBusinessChainId.value = record.id;
    businessChainForm.value = {
      chainCode: record.chainCode,
      linkedContractCode: record.linkedContractCode,
      ownerUserId: record.ownerUserId || currentUserId.value,
      sourceChangeCode: record.sourceChangeCode,
      stage: record.stage,
      status: record.status,
      summary: record.summary,
      title: record.title,
      type: record.type,
    };
    showBusinessChainForm.value = true;
  }

  function editContract(record: BusinessApi.ContractItem) {
    editingContractId.value = record.id;
    contractForm.value = {
      amount: record.amount ?? 0,
      contractCode: record.contractCode,
      counterpartyName: record.counterpartyName,
      isRevenueContract: record.isRevenueContract,
      parentContractCode: record.parentContractCode,
      signDate: toDateInputValue(record.signDate),
      sourceChangeCode: record.sourceChangeCode,
      status: record.status,
      title: record.title,
      type: record.type,
    };
    showContractForm.value = true;
  }

  function editProcurement(record: BusinessApi.ProcurementItem) {
    editingProcurementId.value = record.id;
    procurementForm.value = {
      amount: record.amount ?? 0,
      linkedContractCode: record.linkedContractCode,
      procurementCode: record.procurementCode,
      signDate: toDateInputValue(record.signDate),
      sourceChangeCode: record.sourceChangeCode,
      stage: record.stage,
      status: record.status,
      supplierName: record.supplierName,
      title: record.title,
    };
    showProcurementForm.value = true;
  }

  function editInvoice(record: BusinessApi.InvoiceItem) {
    editingInvoiceId.value = record.id;
    invoiceForm.value = {
      amount: record.amount ?? 0,
      invoiceCode: record.invoiceCode,
      invoiceDate: toDateInputValue(record.invoiceDate),
      linkedContractCode: record.linkedContractCode,
      receivableCode: record.receivableCode,
      status: record.status,
      taxAmount: record.taxAmount ?? 0,
      title: record.title,
    };
    showInvoiceForm.value = true;
  }

  function editReceivable(record: BusinessApi.ReceivableItem) {
    editingReceivableId.value = record.id;
    receivableForm.value = {
      invoiceCode: record.invoiceCode,
      linkedContractCode: record.linkedContractCode,
      plannedDate: toDateInputValue(record.plannedDate),
      receivableCode: record.receivableCode,
      receivedAmount: record.receivedAmount ?? 0,
      status: record.status,
      title: record.title,
      totalAmount: record.totalAmount ?? 0,
    };
    showReceivableForm.value = true;
  }

  async function handleSaveBudget() {
    if (!selectedProjectId.value) return;
    savingBudget.value = true;
    try {
      const payload = {
        ...budgetForm.value,
        varianceAmount: budgetForm.value.varianceAmount ?? undefined,
      };
      if (editingBudgetId.value) {
        await updateBusinessBudgetApi(
          selectedProjectId.value,
          editingBudgetId.value,
          payload,
        );
        message.success('预算执行项已更新');
      } else {
        await createBusinessBudgetApi(selectedProjectId.value, payload);
        message.success('预算执行项已创建');
      }
      showBudgetForm.value = false;
      resetBudgetForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '预算执行项保存失败');
    } finally {
      savingBudget.value = false;
    }
  }

  async function handleSaveAcceptance() {
    if (!selectedProjectId.value) return;
    savingAcceptance.value = true;
    try {
      const payload = {
        ...acceptanceForm.value,
        acceptanceDate: acceptanceForm.value.acceptanceDate || undefined,
        milestoneId: acceptanceForm.value.milestoneId || undefined,
      };
      if (editingAcceptanceId.value) {
        await updateBusinessAcceptanceApi(
          selectedProjectId.value,
          editingAcceptanceId.value,
          payload,
        );
        message.success('验收记录已更新');
      } else {
        await createBusinessAcceptanceApi(selectedProjectId.value, payload);
        message.success('验收记录已创建');
      }
      showAcceptanceForm.value = false;
      resetAcceptanceForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '验收记录保存失败');
    } finally {
      savingAcceptance.value = false;
    }
  }

  async function handleSaveBusinessChain() {
    if (!selectedProjectId.value) return;
    savingBusinessChain.value = true;
    try {
      const payload = businessChainForm.value;
      if (editingBusinessChainId.value) {
        await updateBusinessChainApi(
          selectedProjectId.value,
          editingBusinessChainId.value,
          payload,
        );
        message.success('经营链事项已更新');
      } else {
        await createBusinessChainApi(selectedProjectId.value, payload);
        message.success('经营链事项已创建');
      }
      showBusinessChainForm.value = false;
      resetBusinessChainForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '经营链事项保存失败');
    } finally {
      savingBusinessChain.value = false;
    }
  }

  async function handleSaveContract() {
    if (!selectedProjectId.value) return;
    savingContract.value = true;
    try {
      const payload = {
        ...contractForm.value,
        signDate: contractForm.value.signDate || undefined,
      };
      if (editingContractId.value) {
        await updateBusinessContractApi(
          selectedProjectId.value,
          editingContractId.value,
          payload,
        );
        message.success('合同已更新');
      } else {
        await createBusinessContractApi(selectedProjectId.value, payload);
        message.success('合同已创建');
      }
      showContractForm.value = false;
      resetContractForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '合同保存失败');
    } finally {
      savingContract.value = false;
    }
  }

  async function handleSaveProcurement() {
    if (!selectedProjectId.value) return;
    savingProcurement.value = true;
    try {
      const payload = {
        ...procurementForm.value,
        signDate: procurementForm.value.signDate || undefined,
      };
      if (editingProcurementId.value) {
        await updateBusinessProcurementApi(
          selectedProjectId.value,
          editingProcurementId.value,
          payload,
        );
        message.success('采购事项已更新');
      } else {
        await createBusinessProcurementApi(selectedProjectId.value, payload);
        message.success('采购事项已创建');
      }
      showProcurementForm.value = false;
      resetProcurementForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '采购事项保存失败');
    } finally {
      savingProcurement.value = false;
    }
  }

  async function handleSaveInvoice() {
    if (!selectedProjectId.value) return;
    savingInvoice.value = true;
    try {
      const payload = {
        ...invoiceForm.value,
        invoiceDate: invoiceForm.value.invoiceDate || undefined,
      };
      if (editingInvoiceId.value) {
        await updateBusinessInvoiceApi(
          selectedProjectId.value,
          editingInvoiceId.value,
          payload,
        );
        message.success('发票记录已更新');
      } else {
        await createBusinessInvoiceApi(selectedProjectId.value, payload);
        message.success('发票记录已创建');
      }
      showInvoiceForm.value = false;
      resetInvoiceForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '发票记录保存失败');
    } finally {
      savingInvoice.value = false;
    }
  }

  async function handleSaveReceivable() {
    if (!selectedProjectId.value) return;
    savingReceivable.value = true;
    try {
      const payload = {
        ...receivableForm.value,
        plannedDate: receivableForm.value.plannedDate || undefined,
      };
      if (editingReceivableId.value) {
        await updateBusinessReceivableApi(
          selectedProjectId.value,
          editingReceivableId.value,
          payload,
        );
        message.success('回款计划已更新');
      } else {
        await createBusinessReceivableApi(selectedProjectId.value, payload);
        message.success('回款计划已创建');
      }
      showReceivableForm.value = false;
      resetReceivableForm();
      await loadDetail();
    } catch (error) {
      showRequestError(error, '回款计划保存失败');
    } finally {
      savingReceivable.value = false;
    }
  }

  async function deleteBusinessRecord(
    itemId: string,
    api: (projectId: string, itemId: string) => Promise<boolean>,
    successMessage: string,
    fallbackMessage: string,
  ) {
    if (!selectedProjectId.value) return;
    try {
      await api(selectedProjectId.value, itemId);
      message.success(successMessage);
      await loadDetail();
    } catch (error) {
      showRequestError(error, fallbackMessage);
    }
  }

  function handleDeleteBudget(record: BusinessApi.BudgetExecutionItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessBudgetApi,
      '预算执行项已删除',
      '预算执行项删除失败',
    );
  }

  function handleDeleteAcceptance(record: BusinessApi.AcceptanceItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessAcceptanceApi,
      '验收记录已删除',
      '验收记录删除失败',
    );
  }

  function handleDeleteBusinessChain(record: BusinessApi.BusinessChainItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessChainApi,
      '经营链事项已删除',
      '经营链事项删除失败',
    );
  }

  function handleDeleteContract(record: BusinessApi.ContractItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessContractApi,
      '合同已删除',
      '合同删除失败',
    );
  }

  function handleDeleteProcurement(record: BusinessApi.ProcurementItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessProcurementApi,
      '采购事项已删除',
      '采购事项删除失败',
    );
  }

  function handleDeleteInvoice(record: BusinessApi.InvoiceItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessInvoiceApi,
      '发票记录已删除',
      '发票记录删除失败',
    );
  }

  function handleDeleteReceivable(record: BusinessApi.ReceivableItem) {
    return deleteBusinessRecord(
      record.id,
      deleteBusinessReceivableApi,
      '回款计划已删除',
      '回款计划删除失败',
    );
  }

  function showRequestError(error: unknown, fallback: string) {
    const description = error instanceof Error ? error.message : fallback;
    message.error(description || fallback);
  }

  onMounted(async () => {
    try {
      const userInfo = await getUserInfoApi();
      currentUserId.value = userInfo.userId;
      currentUserName.value = userInfo.realName || userInfo.username;
      businessChainForm.value.ownerUserId = userInfo.userId;
      await Promise.all([businessDicts.load(), loadProjects()]);
      if (selectedProjectId.value) {
        await loadDetail(selectedProjectId.value);
      }
    } catch (error) {
      showRequestError(error, '经营项目初始化失败');
    }
  });

  watch(
    () => [
      route.query.projectId,
      route.query.tab,
      route.query.focusType,
      route.query.focusId,
    ],
    ([projectId, tab]) => {
      const nextProjectId = typeof projectId === 'string' ? projectId : '';
      const nextTab = typeof tab === 'string' ? tab : 'overview';
      syncRouteFocus();

      if (nextTab !== activeTab.value) {
        activeTab.value = nextTab;
      }

      if (nextProjectId && nextProjectId !== selectedProjectId.value) {
        selectedProjectId.value = nextProjectId;
        void loadDetail(nextProjectId);
      }
    },
  );

  return {
    activeTab,
    acceptanceForm,
    budgetForm,
    closingItems,
    businessChainForm,
    contractForm,
    contractRelationItems,
    contractTypeOptions,
    currentProject,
    detail,
    editingAcceptanceId,
    editingBudgetId,
    editingBusinessChainId,
    editingContractId,
    editingInvoiceId,
    editingProcurementId,
    editingReceivableId,
    editAcceptance,
    editBudget,
    editBusinessChain,
    editContract,
    editInvoice,
    editProcurement,
    editReceivable,
    formatMoney,
    getBusinessCloseStatusColor,
    getCollapsedProjectLabel,
    getFocusedRowClass,
    handleDeleteAcceptance,
    handleDeleteBudget,
    handleDeleteBusinessChain,
    handleDeleteContract,
    handleDeleteInvoice,
    handleDeleteProcurement,
    handleDeleteReceivable,
    handleProjectChange,
    handleSaveAcceptance,
    handleSaveBudget,
    handleSaveBusinessChain,
    handleSaveContract,
    handleSaveInvoice,
    handleSaveProcurement,
    handleSaveReceivable,
    listKeyword,
    loadingDetail,
    loadingList,
    openGlobalSearch,
    ownerSelectOptions,
    invoiceForm,
    procurementForm,
    projects,
    projectSelectOptions,
    receivableForm,
    refreshingDetail,
    resetProjects,
    resetAcceptanceForm,
    resetBudgetForm,
    resetBusinessChainForm,
    resetContractForm,
    resetInvoiceForm,
    resetProcurementForm,
    resetReceivableForm,
    savingBudget,
    savingAcceptance,
    savingBusinessChain,
    savingContract,
    savingInvoice,
    savingProcurement,
    savingReceivable,
    searchProjects,
    selectProject,
    selectedProjectId,
    showAcceptanceForm,
    showBudgetForm,
    showBusinessChainForm,
    showContractForm,
    showInvoiceForm,
    showProcurementForm,
    showReceivableForm,
    sidebarCollapsed,
    toggleAcceptanceForm,
    toggleBudgetForm,
    toggleBusinessChainForm,
    toggleContractForm,
    toggleInvoiceForm,
    toggleProcurementForm,
    toggleReceivableForm,
  };
}
