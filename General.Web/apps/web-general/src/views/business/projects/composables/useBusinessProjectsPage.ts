import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import {
  getBusinessProjectDetailApi,
  getBusinessProjectListApi,
  type BusinessApi,
} from '#/api/core';

export function useBusinessProjectsPage() {
  const route = useRoute();
  const router = useRouter();

  const listKeyword = ref('');
  const loadingList = ref(false);
  const loadingDetail = ref(false);
  const refreshingDetail = ref(false);
  const sidebarCollapsed = ref(false);
  const projects = ref<BusinessApi.ProjectSummary[]>([]);
  const detail = ref<BusinessApi.ProjectDetail | null>(null);
  const selectedProjectId = ref(typeof route.query.projectId === 'string' ? route.query.projectId : '');
  const activeTab = ref(typeof route.query.tab === 'string' ? route.query.tab : 'overview');

  let detailRequestSerial = 0;

  const currentProject = computed(() =>
    projects.value.find((item) => item.id === selectedProjectId.value) ?? detail.value?.summary ?? null,
  );

  const contractRelationItems = computed(() => {
    if (!detail.value) {
      return [];
    }

    const incomeContracts = detail.value.contracts.filter((item) => item.isRevenueContract);
    const downstreamContracts = detail.value.contracts.filter((item) => !item.isRevenueContract);

    return incomeContracts.map((contract) => ({
      id: contract.id,
      title: contract.title,
      code: contract.contractCode,
      type: contract.type,
      status: contract.status,
      changeCode: contract.sourceChangeCode,
      linkedProcurements: detail.value?.procurements.filter((item) => item.linkedContractCode === contract.contractCode) ?? [],
      childContracts: downstreamContracts.filter((item) => item.parentContractCode === contract.contractCode),
    }));
  });

  const closingItems = computed(() => {
    if (!detail.value) {
      return [];
    }

    const totalReceivable = detail.value.receivables.reduce((sum, item) => sum + (item.totalAmount ?? 0), 0);
    const receivedAmount = detail.value.receivables.reduce((sum, item) => sum + (item.receivedAmount ?? 0), 0);
    const openReceivables = detail.value.receivables.filter((item) => item.status.includes('待回款'));
    const hasPendingProcurement = detail.value.procurements.some((item) => !item.status.includes('已签订') && !item.status.includes('完成'));

    return [
      {
        key: 'close-status',
        label: '经营收尾状态',
        value: detail.value.summary.businessCloseStatus,
        status: detail.value.summary.businessCloseStatus.includes('收尾') || detail.value.summary.businessCloseStatus.includes('待回款') ? 'warning' : 'normal',
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
      const nextProjects = await getBusinessProjectListApi(listKeyword.value.trim() || undefined);
      projects.value = nextProjects;

      if (nextProjects.length === 0) {
        selectedProjectId.value = '';
        detail.value = null;
        return;
      }

      const preferredProjectId =
        selectedProjectId.value ||
        (typeof route.query.projectId === 'string' ? route.query.projectId : '');
      const matchedProject = nextProjects.find((item) => item.id === preferredProjectId) ?? nextProjects[0];

      if (matchedProject && matchedProject.id !== selectedProjectId.value) {
        selectedProjectId.value = matchedProject.id;
      }
    } finally {
      loadingList.value = false;
    }
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


  onMounted(async () => {
    await loadProjects();
    if (selectedProjectId.value) {
      await loadDetail(selectedProjectId.value);
    }
  });


    return {
      activeTab,
      closingItems,
      contractRelationItems,
      currentProject,
      detail,
      formatMoney,
      getCollapsedProjectLabel,
      handleProjectChange,
      listKeyword,
      loadingDetail,
      loadingList,
      openGlobalSearch,
      projects,
      refreshingDetail,
      resetProjects,
      searchProjects,
      selectProject,
      selectedProjectId,
      sidebarCollapsed,
    };
}
