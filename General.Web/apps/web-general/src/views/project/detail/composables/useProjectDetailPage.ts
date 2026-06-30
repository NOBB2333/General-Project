import {
  computed,
  nextTick,
  onBeforeUnmount,
  onMounted,
  ref,
  watch,
  type ComponentPublicInstance,
} from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { message } from 'ant-design-vue';

import {
  getProjectDetailSectionApi,
  getProjectListApi,
  getUserInfoApi,
  type ProjectApi,
} from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';

import {
  documentColumns,
  issueColumns,
  memberColumns,
  taskColumns,
  worklogColumns,
} from './projectDetailColumns';
import { useProjectCharter } from './useProjectCharter';
import { useProjectCommunication } from './useProjectCommunication';
import { useProjectControl } from './useProjectControl';
import { useProjectDelivery } from './useProjectDelivery';
import { useProjectDocuments } from './useProjectDocuments';
import { useProjectHealth } from './useProjectHealth';
import { useProjectIssues } from './useProjectIssues';
import { useProjectOverview } from './useProjectOverview';
import { useProjectPlan } from './useProjectPlan';
import { useProjectRequirements } from './useProjectRequirements';
import { useProjectResources } from './useProjectResources';
import { useProjectRisks } from './useProjectRisks';
import { useProjectTaskDependencies } from './useProjectTaskDependencies';

export function useProjectDetailPage() {
  const DETAIL_PATH = '/project/detail';
  const STORAGE_KEY = 'general:last-project-id';

  const route = useRoute();
  const router = useRouter();

  const activeTab = ref('overview');
  const detail = ref<null | ProjectApi.ProjectDetail>(null);
  const loadedDetailSections = ref(new Set<ProjectApi.ProjectDetailSection>());
  const loadingDetailSections = new Set<ProjectApi.ProjectDetailSection>();
  let detailLoadVersion = 0;
  let openingWorklogFromRoute = false;
  const currentUserId = ref('');
  const currentUserName = ref('');
  const currentUsername = ref('');
  const loading = ref(false);
  const loadingProjects = ref(false);
  const projectOptions = ref<ProjectApi.ProjectListItem[]>([]);
  const selectedProjectId = ref<string | undefined>(undefined);
  const selectedAcceptanceSprintId = ref<string | undefined>(undefined);
  const projectDocuments = useProjectDocuments({
    detail,
    selectedAcceptanceSprintId,
    selectedProjectId,
    showRequestError,
  });
  const {
    acceptanceCriteriaFiles,
    acceptanceFiles,
    charterFiles,
    createRequirementFileUploadRequest,
    createTaskPrdUploadRequest,
    currentAcceptanceCriteriaFile,
    currentCharterFile,
    featuredDocuments,
    knowledgeCollections,
    loadProjectFiles,
    loadingProjectFiles,
    openDocumentWorkspace,
    openProjectFilePreview,
    previewingProjectFile,
    projectFilePreviewOpen,
    projectFiles,
    uploadAcceptanceCriteriaFile,
    uploadAcceptanceFile,
    uploadCharterFile,
    uploadDeliveryFile,
    uploadProjectFile,
  } = projectDocuments;
  const tabContentElement = ref<HTMLElement | null>(null);
  const tabBodyHeight = ref<number>(520);
  let tabContentObserver: null | ResizeObserver = null;
  const detailDicts = useDictOptions(
    ['change_status', 'task_priority', 'task_status'],
    createDictOptionsFallback({
      change_status: ['待评审', '审批中', '已批准', '已驳回', '已关闭'],
      task_priority: ['P0', 'P1', 'P2', 'P3'],
      task_status: ['未开始', '进行中', '阻塞', '已完成', '已关闭'],
    }),
  );
  const taskPriorityOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({
    label: value,
    value,
  }));
  const demandPriorityOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({
    label: value,
    value,
  }));
  const changeSeverityOptions = computed(() =>
    ['P0', 'P1', 'P2', 'P3'].map((value) => ({
      label: value,
      value,
    })),
  );

  const tabItems = [
    { key: 'overview', label: '项目详细信息' },
    { key: 'plan', label: '目标与计划' },
    { key: 'tasks', label: '需求与排期' },
    { key: 'issues', label: '缺陷与问题' },
    { key: 'control', label: '项目控制' },
    { key: 'delivery', label: '验收与交付' },
    { key: 'docs', label: '文档与知识' },
    { key: 'resources', label: '人员与资源' },
  ];
  const detailSectionFields = {
    control: [
      'activeBaseline',
      'budgetTotalAmount',
      'changeRequests',
      'milestones',
      'performanceSnapshot',
      'performanceSnapshots',
      'taskDependencies',
      'taskScheduleImpacts',
      'tasks',
    ],
    delivery: [
      'acceptanceCriteria',
      'acceptanceRecords',
      'milestones',
      'qualityChecklists',
      'qualityInspections',
      'requirements',
      'sprints',
      'tasks',
    ],
    docs: [
      'documents',
      'issues',
      'meetingRecords',
      'raidItems',
      'reports',
      'requirements',
      'sprints',
      'tasks',
    ],
    issues: [
      'acceptanceCriteria',
      'issues',
      'qualityChecklists',
      'requirements',
      'tasks',
    ],
    overview: [
      'activeBaseline',
      'calendarItems',
      'charter',
      'documents',
      'healthBreakdowns',
      'healthCalculatedTime',
      'healthDataSource',
      'healthReasons',
      'healthRiskScore',
      'healthStatus',
      'issues',
      'milestones',
      'performanceSnapshot',
      'performanceSnapshots',
      'raidItems',
      'sprints',
      'tasks',
      'totalWorklogHours',
      'worklogs',
    ],
    plan: [
      'acceptanceCriteria',
      'acceptanceRecords',
      'documents',
      'milestones',
      'meetingRecords',
      'qualityInspections',
      'reports',
      'sprints',
      'tasks',
      'wbsNodes',
    ],
    resources: [
      'calendarItems',
      'sprints',
      'tasks',
      'totalWorklogHours',
      'worklogs',
    ],
    tasks: [
      'calendarItems',
      'issues',
      'raidItems',
      'requirements',
      'sprints',
      'taskDependencies',
      'taskScheduleImpacts',
      'tasks',
      'totalWorklogHours',
      'wbsNodes',
      'worklogs',
    ],
  } satisfies Record<
    ProjectApi.ProjectDetailSection,
    Array<keyof ProjectApi.ProjectDetail>
  >;
  const detailSharedFields = [
    'budgetTotalAmount',
    'contractTotalAmount',
    'description',
    'highRiskCount',
    'id',
    'isKeyProject',
    'managerName',
    'members',
    'myRelation',
    'name',
    'organizationUnitName',
    'plannedEndDate',
    'plannedStartDate',
    'priority',
    'projectCode',
    'projectSource',
    'projectType',
    'receivedAmount',
    'sponsorName',
    'status',
  ] satisfies Array<keyof ProjectApi.ProjectDetail>;
  const detailMetricFields = [
    'highRiskCount',
    'memberCount',
    'openIssueCount',
    'taskCompletionRate',
    'totalWorklogHours',
  ] satisfies Array<keyof ProjectApi.ProjectDetail>;
  const currentUserAliases = computed(() =>
    [currentUserName.value, currentUsername.value].filter(
      (item) => item.trim().length > 0,
    ),
  );

  const {
    currentHealth,
    handleHealthAction,
    healthMetrics,
    healthReasonItems,
    healthRuleInfo,
  } = useProjectHealth({
    detail,
    formatDate,
    switchTab: switchProjectDetailTab,
  });

  const tabBodyStyle = computed(() => ({
    minHeight: `${tabBodyHeight.value}px`,
  }));

  async function loadProjects() {
    loadingProjects.value = true;
    try {
      const [projects, userInfo] = await Promise.all([
        getProjectListApi(),
        getUserInfoApi(),
      ]);
      const currentUser = userInfo as unknown as {
        realName?: string;
        userId?: string;
        username?: string;
      };
      projectOptions.value = projects;
      currentUserId.value = currentUser.userId || '';
      currentUserName.value = currentUser.realName || '';
      currentUsername.value = currentUser.username || '';
      syncSelectedProject();
    } catch (error) {
      showRequestError(error, '项目列表加载失败');
    } finally {
      loadingProjects.value = false;
    }
  }

  function resolveDetailSection(tabKey = activeTab.value) {
    return tabItems.some((item) => item.key === tabKey)
      ? (tabKey as ProjectApi.ProjectDetailSection)
      : 'overview';
  }

  function resetDetailCache() {
    detailLoadVersion += 1;
    detail.value = null;
    loadedDetailSections.value = new Set();
    loadingDetailSections.clear();
  }

  function mergeDetailSection(
    section: ProjectApi.ProjectDetailSection,
    nextDetail: ProjectApi.ProjectDetail,
  ) {
    const merged = {
      ...(detail.value || nextDetail),
    } as ProjectApi.ProjectDetail;

    for (const field of detailSharedFields) {
      merged[field] = nextDetail[field] as never;
    }

    if (section === 'overview' || detail.value === null) {
      for (const field of detailMetricFields) {
        merged[field] = nextDetail[field] as never;
      }
    }

    for (const field of detailSectionFields[section]) {
      merged[field] = nextDetail[field] as never;
    }

    detail.value = merged;
  }

  async function ensureDetailSectionLoaded(
    section = resolveDetailSection(),
    options: { force?: boolean } = {},
  ) {
    if (!selectedProjectId.value) {
      return;
    }
    const projectId = selectedProjectId.value;
    const requestVersion = detailLoadVersion;

    if (
      !options.force &&
      (loadedDetailSections.value.has(section) || loadingDetailSections.has(section))
    ) {
      return;
    }

    loadingDetailSections.add(section);
    loading.value = true;
    try {
      const nextDetail = await getProjectDetailSectionApi(section, projectId);
      if (requestVersion !== detailLoadVersion || selectedProjectId.value !== projectId) {
        return;
      }
      mergeDetailSection(section, nextDetail);
      loadedDetailSections.value = new Set(loadedDetailSections.value).add(section);
      syncGovernanceForms();
    } catch (error) {
      showRequestError(error, '项目详情加载失败');
    } finally {
      if (requestVersion === detailLoadVersion) {
        loadingDetailSections.delete(section);
        loading.value = loadingDetailSections.size > 0;
      }
      await nextTick();
      syncTabBodyHeight();
    }
  }

  async function loadCurrentDetailSection(projectId: string) {
    resetDetailCache();
    selectedProjectId.value = projectId;
    loading.value = true;
    try {
      const section = resolveDetailSection();
      const sections: ProjectApi.ProjectDetailSection[] =
        section === 'overview' ? [section] : ['overview', section];
      await Promise.all([
        ...sections.map((item) => ensureDetailSectionLoaded(item, { force: true })),
        loadProjectFiles(projectId),
      ]);
      await openWorklogDrawerFromRoute();
    } finally {
      loading.value = loadingDetailSections.size > 0;
      await nextTick();
      syncTabBodyHeight();
    }
  }

  function syncGovernanceForms() {
    const current = detail.value;
    projectCharter.resetCharterForm(current);
    projectCommunication.resetCommunicationForms();
    projectPlan.resetPlanForms(current);
    projectRequirements.resetRequirementForms();
    projectDelivery.resetDeliveryForms(current);
    projectControl.resetControlForms(current);
    projectResources.resetResourceForms(current);
    projectRisks.resetRiskForms(current);
    projectIssues.resetIssueForms();
    projectTaskDependencies.resetTaskDependencyForm();
  }

  async function refreshSelectedProjectDetail() {
    if (!selectedProjectId.value) {
      return;
    }

    await ensureDetailSectionLoaded(resolveDetailSection(), { force: true });
  }

  const projectCharter = useProjectCharter({
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
  });
  const {
    canApproveCharterApproval,
    canSubmitCharterApproval,
    canWithdrawCharterApproval,
    charterApprovalComment,
    charterApprovalRecords,
    charterApprovalStatus,
    charterApprovalWorkflow,
    charterForm,
    handleApproveCharterApproval,
    handleRejectCharterApproval,
    handleSaveCharter,
    handleSubmitCharterApproval,
    handleWithdrawCharterApproval,
    processingCharterApproval,
    savingCharter,
  } = projectCharter;

  const projectPlan = useProjectPlan({
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    switchProjectDetailTab,
    toDateInputValue,
  });
  const {
    sprintForm,
    sprintPreview,
    sprintOptions,
    sprintStatusOptions,
    deletingSprintId,
    deletingMilestoneId,
    editingMilestoneCloseState,
    editingSprintId,
    editingMilestoneId,
    handleCancelSprintEdit,
    handleCancelMilestoneEdit,
    handleCreateSprintDraft,
    handleCreateMilestoneDraft,
    handleDeleteSprint,
    handleDeleteMilestone,
    handleEditSprint,
    handleEditMilestone,
    handleMilestoneCloseAdviceAction,
    handleSaveSprint,
    handleSaveMilestone,
    handleUpdateTaskWbs,
    milestoneColumns,
    milestoneCloseAdvice,
    milestoneForm,
    milestoneFormSprintName,
    milestonePreview,
    milestoneRows,
    milestoneSelectOptions,
    sprintColumns,
    sprintRows,
    savingSprint,
    savingMilestone,
    savingTaskWbsId,
    showSprintForm,
    showMilestoneForm,
    taskWbsRows,
    wbsNodeOptions,
  } = projectPlan;

  const projectRequirements = useProjectRequirements({
    currentUserAliases,
    currentUserId,
    detail,
    getTaskPriorityColor: (priority, fallback) =>
      detailDicts.getColor('task_priority', priority, fallback),
    parseDate,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    taskStatusOptions: detailDicts.getOptions('task_status'),
    toDateKey,
  });
  const {
    editingRequirementId,
    editingTaskId,
    filteredRequirements,
    filteredTasks,
    handleCancelRequirementEdit,
    handleCancelTaskEdit,
    handleCancelTaskFromRequirement,
    handleCreateRequirementDraft,
    handleCreateTaskDraft,
    handleCreateTaskFromRequirement,
    handleChangeTaskStatus,
    handleEditTask,
    handleEditRequirement,
    handleCloseTaskDetail,
    handleOpenTaskDetail,
    handleOpenTaskFromRequirement,
    handleRecordTaskProgress,
    handleSaveRequirement,
    handleSaveTask,
    kanbanColumns,
    relatedTaskCount,
    requirementForm,
    requirementMetrics,
    requirementPriorityOptions,
    requirementReplacementOptions,
    requirementSelectOptions,
    requirementSourceOptions,
    requirementStatusFilter,
    requirementStatusOptions,
    requirementTypeOptions,
    requirementWbsFilter,
    savingRequirement,
    savingTask,
    savingTaskFromRequirement,
    savingTaskProgress,
    selectedTaskDetail,
    selectedTaskDependencies,
    selectedTaskRequirement,
    selectedTaskWorklogs,
    selectedTaskSprintMetrics,
    shiftTaskCalendarMonth,
    splittingRequirementId,
    showTaskInlineForm,
    taskCalendarAnchorDate,
    taskCalendarRows,
    taskCalendarTitle,
    taskFromRequirementForm,
    taskForm,
    taskParentOptions,
    taskProgressForm,
    taskProgressStatusOptions,
    taskSprintFilter,
    taskSprintMetrics,
    taskSprintOptions,
    taskScope,
    taskScopeOptions,
    taskSelectOptions,
    taskTimelineDays,
    taskTimelineGridStyle,
    taskTimelineRows,
    taskView,
    taskViewOptions,
    taskWbsNodeOptions,
  } = projectRequirements;

  const projectOverview = useProjectOverview({
    currentHealth,
    currentUserAliases,
    currentUserId,
    detail,
    formatDate,
    formatMoney,
    formatRatio,
    relatedTaskCount,
  });
  const {
    collaborationMetrics,
    compactMetrics,
    currentProjectUserId,
    memberSelectOptions,
    overviewDetailSections,
  } = projectOverview;

  const projectTaskDependencies = useProjectTaskDependencies({
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
  });
  const {
    dependencyTypeOptions,
    handleCreateTaskDependency,
    handleCreateTaskDetailDependency,
    savingTaskDependency,
    taskDetailDependencyForm,
    taskDependencyForm,
  } = projectTaskDependencies;

  const projectDelivery = useProjectDelivery({
    acceptanceFiles,
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    toDateInputValue,
  });
  const {
    acceptanceCriteriaForm,
    acceptanceCriteriaOptions,
    acceptanceFileOptions,
    acceptanceRecordForm,
    acceptanceResultOptions,
    acceptanceStatusOptions,
    handleCreateAcceptanceCriteria,
    handleCreateAcceptanceRecord,
    handleCreateQualityChecklist,
    handleCreateQualityInspection,
    issueLevelOptions,
    qualityCategoryOptions,
    qualityChecklistForm,
    qualityChecklistOptions,
    qualityInspectionForm,
    qualityInspectionResultOptions,
    savingAcceptanceCriteria,
    savingAcceptanceRecord,
    savingQualityChecklist,
    savingQualityInspection,
    showAcceptanceForm,
    showAcceptanceRecordForm,
    showChecklistForm,
    showInspectionForm,
  } = projectDelivery;

  const projectControl = useProjectControl({
    changeSeverityOptions,
    detail,
    formatDate,
    formatMoney,
    formatRatio,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    toDateInputValue,
  });
  const {
    activeBaselineSummary,
    baselineChangeOptions,
    baselineForm,
    canApproveBaselineChange,
    canSubmitBaselineChange,
    changeApprovalComment,
    changeControlMetrics,
    changeImpactTypeOptions,
    changeRequestForm,
    changeTypeOptions,
    evmMetrics,
    getBaselineChangeWorkflowStatus,
    handleAddChangeImpact,
    handleCreateBaseline,
    handleCreateChangeApproval,
    handleCreateChangeRequest,
    handleCreatePerformanceSnapshot,
    handleRejectChangeApproval,
    handleRemoveChangeImpact,
    handleWithdrawChangeApproval,
    performanceSnapshotForm,
    performanceTrendRows,
    savingBaseline,
    savingChangeRequest,
    savingChangeWorkflowId,
    savingPerformanceSnapshot,
    showBaselineForm,
    showChangeRequestForm,
    showPerformanceSnapshotForm,
  } = projectControl;

  const projectResources = useProjectResources({
    currentUserId,
    detail,
    refreshSelectedProjectDetail,
    showRequestError,
  });
  const {
    editingMemberId,
    filledWorklogEntryCount,
    handleApplyWorklogDateToRows,
    handleCloseWorklogDrawer,
    handleEditMemberRoles,
    handleFillEmptyWorklogSummaries,
    handleOpenWorklogDrawer,
    handleSaveFilledWorklogEntries,
    handleSaveMemberRoles,
    handleSaveWorklogEntry,
    loadingMyWorklogTasks,
    memberResourceRows,
    memberRoleForms,
    projectRoleOptions,
    savingMemberRoleId,
    savingWorklogBatch,
    savingWorklogEntry,
    savingWorklogTaskId,
    worklogBatchDate,
    worklogBatchSummary,
    worklogDemandRows,
    worklogDrawerSummary,
    worklogDrawerOpen,
    worklogEntryForms,
  } = projectResources;

  const projectRisks = useProjectRisks({
    detail,
    refreshSelectedProjectDetail,
    showRequestError,
  });
  const {
    activeRiskOptions,
    handleUpdateRaidRisk,
    raidRiskRows,
    riskAlerts,
    riskResponseOptions,
    savingRaidRiskId,
  } = projectRisks;

  const projectIssues = useProjectIssues({
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
  });
  const {
    deletingIssueId,
    editingIssueId,
    handleCloseIssueForm,
    handleDeleteIssue,
    handleOpenIssueCreate,
    handleOpenIssueEdit,
    handleSaveIssue,
    issueForm,
    issueFormOpen,
    issueMetrics,
    issueSelectOptions,
    projectIssueLevelOptions,
    projectIssueStatusOptions,
    projectIssueTypeOptions,
    savingIssue,
    workflowPreviewGroups,
  } = projectIssues;

  const projectCommunication = useProjectCommunication({
    currentProjectUserId,
    detail,
    formatDate,
    loadProjectFiles,
    openDocumentWorkspace,
    openProjectFilePreview,
    projectFiles,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    toDateInputValue,
    toDateTimeInputValue,
  });

  function formatDate(value?: null | string) {
    return value ? new Date(value).toLocaleDateString() : '-';
  }

  function toDateInputValue(value?: null | string) {
    return value ? value.slice(0, 10) : undefined;
  }

  function toDateTimeInputValue(value?: null | string) {
    return value ? value.slice(0, 16) : undefined;
  }

  function formatMoney(value?: null | number) {
    return typeof value === 'number' ? value.toLocaleString('zh-CN') : '-';
  }

  function formatRatio(value?: null | number) {
    return typeof value === 'number' ? value.toFixed(2) : '-';
  }

  function formatHours(value?: null | number) {
    return typeof value === 'number' ? `${value.toFixed(1)} 小时` : '-';
  }

  function showRequestError(error: unknown, fallback: string) {
    const description = error instanceof Error ? error.message : fallback;
    message.error(description || fallback);
  }

  function parseDate(value?: null | string) {
    if (!value) {
      return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  function toDateKey(date: Date) {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  function getChangeStatusColor(status?: null | string) {
    return detailDicts.getColor('change_status', status, 'processing');
  }

  function getPriorityColor(priority?: null | string) {
    if (priority === 'P0') {
      return 'error';
    }

    if (priority === 'P1') {
      return 'warning';
    }

    if (priority === 'P2') {
      return 'blue';
    }

    if (priority === 'P3') {
      return 'default';
    }

    return detailDicts.getColor('task_priority', priority, 'warning');
  }

  function getTaskStatusColor(status?: null | string, fallback = 'blue') {
    return detailDicts.getColor('task_status', status, fallback);
  }

  function getApprovalStatusColor(status?: null | string) {
    if (status === '已批准') {
      return 'success';
    }

    if (status === '待审批') {
      return 'processing';
    }

    if (status === '退回' || status === '已驳回') {
      return 'error';
    }

    return 'default';
  }

  function disconnectTabObserver() {
    tabContentObserver?.disconnect();
    tabContentObserver = null;
  }

  function syncTabBodyHeight() {
    if (!tabContentElement.value) {
      return;
    }

    tabBodyHeight.value = Math.max(520, tabContentElement.value.scrollHeight);
  }

  async function scheduleSyncTabBodyHeight() {
    await nextTick();
    if (typeof requestAnimationFrame === 'undefined') {
      syncTabBodyHeight();
      return;
    }

    requestAnimationFrame(() => syncTabBodyHeight());
  }

  function bindTabContent(element: ComponentPublicInstance | Element | null) {
    disconnectTabObserver();
    tabContentElement.value = ((element as ComponentPublicInstance | null)
      ?.$el ?? element) as HTMLElement | null;

    if (!tabContentElement.value) {
      return;
    }

    if (typeof ResizeObserver !== 'undefined') {
      tabContentObserver = new ResizeObserver(() => {
        void scheduleSyncTabBodyHeight();
      });
      tabContentObserver.observe(tabContentElement.value);
    }

    void scheduleSyncTabBodyHeight();
  }

  function syncSelectedProject() {
    if (route.path !== DETAIL_PATH) {
      return;
    }

    const routeTab =
      typeof route.query.tab === 'string' ? route.query.tab : null;
    if (routeTab && tabItems.some((item) => item.key === routeTab)) {
      activeTab.value = routeTab;
    }

    const routeProjectId =
      typeof route.query.projectId === 'string' ? route.query.projectId : null;
    const storedProjectId = localStorage.getItem(STORAGE_KEY);
    const routeProjectExists = Boolean(
      routeProjectId && projectOptions.value.some((item) => item.id === routeProjectId),
    );
    const fallbackProjectId =
      storedProjectId && projectOptions.value.some((item) => item.id === storedProjectId)
        ? storedProjectId
        : projectOptions.value[0]?.id;
    const candidate = routeProjectExists ? routeProjectId : fallbackProjectId;

    if (!candidate) {
      selectedProjectId.value = undefined;
      resetDetailCache();
      return;
    }

    const exists = projectOptions.value.some((item) => item.id === candidate);
    if (!exists) {
      selectedProjectId.value = undefined;
      resetDetailCache();
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    selectedProjectId.value = candidate;
    localStorage.setItem(STORAGE_KEY, candidate);

    if (routeProjectId) {
      router.replace({
        path: DETAIL_PATH,
        query: {
          ...route.query,
          projectId: undefined,
        },
      });
      return;
    }

    void loadCurrentDetailSection(candidate);
  }

  async function openWorklogDrawerFromRoute() {
    if (
      route.path !== DETAIL_PATH ||
      route.query.tab !== 'resources' ||
      route.query.worklog !== '1' ||
      activeTab.value !== 'resources' ||
      worklogDrawerOpen.value ||
      openingWorklogFromRoute
    ) {
      return;
    }

    openingWorklogFromRoute = true;
    try {
      await ensureDetailSectionLoaded('resources');
      await handleOpenWorklogDrawer();
      await router.replace({
        path: DETAIL_PATH,
        query: {
          ...route.query,
          worklog: undefined,
        },
      });
    } finally {
      openingWorklogFromRoute = false;
    }
  }

  async function handleProjectChange(projectId: string | undefined) {
    if (!projectId) {
      return;
    }

    selectedProjectId.value = projectId;
    localStorage.setItem(STORAGE_KEY, projectId);
    resetDetailCache();
    await loadCurrentDetailSection(projectId);
  }

  function switchProjectDetailTab(tabKey: string) {
    activeTab.value = tabKey;
  }

  watch(
    () => route.fullPath,
    () => {
      if (route.path === DETAIL_PATH && projectOptions.value.length > 0) {
        syncSelectedProject();
      }
    },
  );

  watch(
    () => activeTab.value,
    (tabKey) => {
      void ensureDetailSectionLoaded(resolveDetailSection(tabKey));
      void openWorklogDrawerFromRoute();
      void scheduleSyncTabBodyHeight();
    },
    { flush: 'post' },
  );

  watch(
    () => route.query.worklog,
    () => {
      void openWorklogDrawerFromRoute();
    },
    { flush: 'post' },
  );

  watch(
    () => taskView.value,
    () => {
      void scheduleSyncTabBodyHeight();
    },
    { flush: 'post' },
  );

  onMounted(async () => {
    await Promise.all([detailDicts.load(), loadProjects()]);
  });
  onBeforeUnmount(disconnectTabObserver);

  return {
    acceptanceCriteriaForm,
    acceptanceCriteriaFiles,
    acceptanceCriteriaOptions,
    acceptanceFileOptions,
    acceptanceFiles,
    acceptanceRecordForm,
    acceptanceResultOptions,
    acceptanceStatusOptions,
    activeTab,
    activeBaselineSummary,
    baselineChangeOptions,
    baselineForm,
    bindTabContent,
    changeApprovalComment,
    changeControlMetrics,
    changeImpactTypeOptions,
    changeRequestForm,
    changeSeverityOptions,
    changeTypeOptions,
    canApproveBaselineChange,
    canApproveCharterApproval,
    canSubmitBaselineChange,
    canSubmitCharterApproval,
    canWithdrawCharterApproval,
    charterApprovalComment,
    charterApprovalRecords,
    charterApprovalStatus,
    charterApprovalWorkflow,
    charterForm,
    charterFiles,
    collaborationMetrics,
    compactMetrics,
    currentCharterFile,
    currentAcceptanceCriteriaFile,
    currentHealth,
    sprintForm,
    sprintPreview,
    sprintOptions,
    sprintStatusOptions,
    deletingSprintId,
    deletingIssueId,
    deletingMilestoneId,
    dependencyTypeOptions,
    detail,
    documentColumns,
    editingMilestoneCloseState,
    editingSprintId,
    editingIssueId,
    editingMilestoneId,
    editingRequirementId,
    editingTaskId,
    editingMemberId,
    featuredDocuments,
    filteredRequirements,
    filteredTasks,
    formatDate,
    formatMoney,
    formatRatio,
    formatHours,
    createRequirementFileUploadRequest,
    createTaskPrdUploadRequest,
    evmMetrics,
    getBaselineChangeWorkflowStatus,
    getChangeStatusColor,
    getApprovalStatusColor,
    getPriorityColor,
    getTaskStatusColor,
    handleAddChangeImpact,
    handleApplyWorklogDateToRows,
    handleCreateAcceptanceCriteria,
    handleCreateAcceptanceRecord,
    handleCreateBaseline,
    handleCreateChangeApproval,
    handleCreateChangeRequest,
    handleCreateSprintDraft,
    handleCreateMilestoneDraft,
    handleCreatePerformanceSnapshot,
    handleCreateQualityChecklist,
    handleCreateQualityInspection,
    handleCreateRequirementDraft,
    handleCreateTaskDraft,
    handleCreateTaskDependency,
    handleCreateTaskDetailDependency,
    handleCreateTaskFromRequirement,
    handleChangeTaskStatus,
    handleCancelSprintEdit,
    handleCancelMilestoneEdit,
    handleCancelRequirementEdit,
    handleCancelTaskEdit,
    handleCancelTaskFromRequirement,
    handleCloseIssueForm,
    handleDeleteSprint,
    handleDeleteIssue,
    handleDeleteMilestone,
    handleEditSprint,
    handleEditMilestone,
    handleMilestoneCloseAdviceAction,
    handleEditRequirement,
    handleEditTask,
    handleFillEmptyWorklogSummaries,
    handleCloseTaskDetail,
    handleCloseWorklogDrawer,
    handleOpenTaskDetail,
    handleOpenIssueCreate,
    handleOpenIssueEdit,
    handleOpenTaskFromRequirement,
    handleOpenWorklogDrawer,
    handleRecordTaskProgress,
    handleApproveCharterApproval,
    handleEditMemberRoles,
    handleHealthAction,
    handleProjectChange,
    handleRejectChangeApproval,
    handleRejectCharterApproval,
    handleRemoveChangeImpact,
    handleSaveCharter,
    handleSubmitCharterApproval,
    handleWithdrawChangeApproval,
    handleWithdrawCharterApproval,
    handleSaveSprint,
    handleSaveIssue,
    handleSaveMilestone,
    handleSaveRequirement,
    handleSaveTask,
    handleSaveMemberRoles,
    handleSaveWorklogEntry,
    handleUpdateRaidRisk,
    handleUpdateTaskWbs,
    healthMetrics,
    healthReasonItems,
    healthRuleInfo,
    issueColumns,
    issueForm,
    issueFormOpen,
    issueSelectOptions,
    issueLevelOptions,
    issueMetrics,
    kanbanColumns,
    knowledgeCollections,
    loading,
    loadingProjectFiles,
    loadingProjects,
    loadingMyWorklogTasks,
    memberColumns,
    memberResourceRows,
    memberRoleForms,
    memberSelectOptions,
    milestoneColumns,
    milestoneCloseAdvice,
    milestoneForm,
    milestoneFormSprintName,
    milestonePreview,
    milestoneRows,
    milestoneSelectOptions,
    openDocumentWorkspace,
    openProjectFilePreview,
    overviewDetailSections,
    performanceSnapshotForm,
    performanceTrendRows,
    previewingProjectFile,
    projectFilePreviewOpen,
    projectFiles,
    projectOptions,
    projectIssueLevelOptions,
    projectIssueStatusOptions,
    projectIssueTypeOptions,
    processingCharterApproval,
    projectRoleOptions,
    qualityCategoryOptions,
    qualityChecklistForm,
    qualityChecklistOptions,
    qualityInspectionForm,
    qualityInspectionResultOptions,
    raidRiskRows,
    activeRiskOptions,
    refreshSelectedProjectDetail,
    requirementForm,
    requirementMetrics,
    requirementPriorityOptions,
    requirementReplacementOptions,
    requirementSourceOptions,
    requirementStatusFilter,
    requirementStatusOptions,
    requirementSelectOptions,
    requirementTypeOptions,
    requirementWbsFilter,
    riskAlerts,
    riskResponseOptions,
    savingAcceptanceCriteria,
    savingAcceptanceRecord,
    savingBaseline,
    savingChangeWorkflowId,
    savingChangeRequest,
    savingCharter,
    savingSprint,
    sprintColumns,
    sprintRows,
    savingMilestone,
    savingMemberRoleId,
    savingWorklogEntry,
    savingPerformanceSnapshot,
    savingQualityChecklist,
    savingQualityInspection,
    savingIssue,
    savingRaidRiskId,
    savingRequirement,
    savingTask,
    savingTaskFromRequirement,
    savingTaskProgress,
    savingTaskDependency,
    savingTaskWbsId,
    selectedTaskDetail,
    selectedTaskDependencies,
    selectedTaskRequirement,
    selectedTaskWorklogs,
    selectedTaskSprintMetrics,
    selectedProjectId,
    selectedAcceptanceSprintId,
    splittingRequirementId,
    showTaskInlineForm,
    showAcceptanceForm,
    showAcceptanceRecordForm,
    showBaselineForm,
    showChangeRequestForm,
    showSprintForm,
    showMilestoneForm,
    showChecklistForm,
    showInspectionForm,
    showPerformanceSnapshotForm,
    tabBodyStyle,
    tabItems,
    taskCalendarAnchorDate,
    taskCalendarRows,
    taskCalendarTitle,
    taskColumns,
    taskDetailDependencyForm,
    taskDependencyForm,
    taskFromRequirementForm,
    taskForm,
    taskParentOptions,
    taskProgressForm,
    taskProgressStatusOptions,
    taskPriorityOptions,
    demandPriorityOptions,
    taskSprintFilter,
    taskSprintMetrics,
    taskSprintOptions,
    taskScope,
    taskScopeOptions,
    taskSelectOptions,
    taskTimelineDays,
    taskTimelineGridStyle,
    taskTimelineRows,
    taskView,
    taskViewOptions,
    taskWbsNodeOptions,
    taskWbsRows,
    uploadAcceptanceFile,
    uploadAcceptanceCriteriaFile,
    uploadCharterFile,
    uploadDeliveryFile,
    uploadProjectFile,
    ...projectCommunication,
    worklogColumns,
    worklogBatchDate,
    worklogBatchSummary,
    worklogDemandRows,
    worklogDrawerSummary,
    worklogDrawerOpen,
    worklogEntryForms,
    filledWorklogEntryCount,
    handleSaveFilledWorklogEntries,
    savingWorklogBatch,
    savingWorklogTaskId,
    workflowPreviewGroups,
    wbsNodeOptions,
    shiftTaskCalendarMonth,
    switchProjectDetailTab,
  };
}
