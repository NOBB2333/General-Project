import { computed, ref, type ComputedRef, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  createProjectRequirementApi,
  createProjectTaskFromRequirementApi,
  createProjectTaskFromWbsApi,
  recordProjectTaskProgressApi,
  updateProjectRequirementApi,
  updateProjectTaskApi,
  type ProjectApi,
} from '#/api/core';

type TaskStatusOption = {
  value: string;
};

const DEFAULT_TASK_STATUS = '进行中';
const DEFAULT_TASK_PRIORITY = 'P1';

type UseProjectRequirementsOptions = {
  currentUserAliases: ComputedRef<string[]>;
  currentUserId: Ref<string>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  getTaskPriorityColor: (priority?: null | string, fallback?: string) => string;
  parseDate: (value?: null | string) => Date | null;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
  taskStatusOptions: ComputedRef<TaskStatusOption[]>;
  toDateKey: (date: Date) => string;
};

export function useProjectRequirements(options: UseProjectRequirementsOptions) {
  const {
    currentUserAliases,
    currentUserId,
    detail,
    getTaskPriorityColor,
    parseDate,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    taskStatusOptions,
    toDateKey,
  } = options;

  const savingRequirement = ref(false);
  const savingTask = ref(false);
  const savingTaskFromRequirement = ref(false);
  const savingTaskProgress = ref(false);
  const editingRequirementId = ref<string | null>(null);
  const editingTaskId = ref<string | null>(null);
  const splittingRequirementId = ref<string | null>(null);
  const showTaskInlineForm = ref(false);
  const taskScope = ref<'all' | 'related'>('all');
  const taskView = ref<'board' | 'calendar' | 'gantt' | 'list'>('list');
  const taskSprintFilter = ref('current');
  const requirementStatusFilter = ref('');
  const requirementWbsFilter = ref('');
  const taskCalendarAnchorDate = ref<Date | null>(null);
  const selectedTaskDetailId = ref<string | null>(null);

  const emptyTaskProgressForm = (): ProjectApi.TaskProgressInput => ({
    blockReason: '',
    hours: 1,
    isBlocked: false,
    status: DEFAULT_TASK_STATUS,
    summary: '',
    workDate: toDateKey(new Date()),
  });

  const emptyRequirementForm = (): ProjectApi.ProjectRequirementSaveInput => ({
    acceptanceCriteria: '',
    businessOwnerUserId: undefined,
    changeReason: '',
    description: '',
    impactSummary: '',
    isCurrent: true,
    milestoneId: undefined,
    priority: DEFAULT_TASK_PRIORITY,
    productOwnerUserId: undefined,
    replacesRequirementId: undefined,
    requirementCode: '',
    sizeEstimate: undefined,
    source: '内部',
    status: '草稿',
    title: '',
    type: '功能',
    versionNo: 1,
    wbsNodeId: undefined,
  });

  const requirementForm = ref<ProjectApi.ProjectRequirementSaveInput>(emptyRequirementForm());

  const emptyTaskForm = (): ProjectApi.TaskSaveInput => ({
    sprintId: undefined,
    developerOwnerUserId: undefined,
    developerOwnerUserIds: [],
    description: '',
    estimatedWorkHours: undefined,
    isOutOfContract: false,
    ownerUserId: '',
    plannedEndTime: undefined,
    plannedStartTime: undefined,
    priority: DEFAULT_TASK_PRIORITY,
    productOwnerUserId: undefined,
    productOwnerUserIds: [],
    status: '未开始',
    taskCode: '',
    testerOwnerUserId: undefined,
    testerOwnerUserIds: [],
    title: '',
    wbsNodeId: undefined,
  });

  const taskForm = ref<ProjectApi.TaskSaveInput>(emptyTaskForm());

  const emptyTaskFromRequirementForm = (): ProjectApi.TaskFromRequirementSaveInput => ({
    developerOwnerUserId: undefined,
    developerOwnerUserIds: [],
    description: '',
    estimatedWorkHours: undefined,
    isOutOfContract: false,
    ownerUserId: '',
    plannedEndTime: undefined,
    plannedStartTime: undefined,
    priority: DEFAULT_TASK_PRIORITY,
    productOwnerUserId: undefined,
    productOwnerUserIds: [],
    taskCode: '',
    testerOwnerUserId: undefined,
    testerOwnerUserIds: [],
    title: '',
  });

  const taskFromRequirementForm = ref<ProjectApi.TaskFromRequirementSaveInput>(
    emptyTaskFromRequirementForm()
  );
  const taskProgressForm = ref<ProjectApi.TaskProgressInput>(emptyTaskProgressForm());

  const taskScopeOptions = [
    { key: 'all', label: '全部' },
    { key: 'related', label: '与我相关' },
  ] as const;

  const taskViewOptions = [
    { key: 'list', label: '列表' },
    { key: 'board', label: '看板' },
    { key: 'gantt', label: '甘特' },
    { key: 'calendar', label: '日历' },
  ] as const;

  const sprints = computed(() => detail.value?.sprints || []);

  const taskSprintOptions = computed(() => [
    { label: '全部排期', value: 'all' },
    { label: '当前排期', value: 'current' },
    { label: '未进排期', value: 'unscheduled' },
    ...sprints.value.map((item) => ({
      label: `${resolveSprintCode(item)} · ${formatSprintName(item.name)} · ${item.status}`,
      value: item.id,
    })),
  ]);

  const taskProgressStatusOptions = computed(() =>
    taskStatusOptions.value.map((item) => ({
      label: item.value,
      value: item.value,
    }))
  );

  const requirementStatusOptions = [
    '草稿',
    '待确认',
    '已确认',
    '开发中',
    '验收中',
    '已关闭',
    '已取消',
  ].map((value) => ({ label: value, value }));

  const requirementPriorityOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({
    label: value,
    value,
  }));

  const requirementSourceOptions = ['客户', '内部', '合同', '问题', '变更', '会议行动项'].map(
    (value) => ({ label: value, value })
  );

  const requirementTypeOptions = ['功能', '非功能', '集成', '数据', '流程', '合规'].map(
    (value) => ({ label: value, value })
  );

  const kanbanOrder = computed(() => taskStatusOptions.value.map((item) => item.value));

  const taskSelectOptions = computed(() =>
    (detail.value?.tasks || []).map((item) => ({
      label: `${item.wbsCode || item.taskCode} · ${item.title}`,
      value: item.id,
    }))
  );

  const taskWbsNodeOptions = computed(() =>
    (detail.value?.wbsNodes || [])
      .filter((item) => item.type === '工作包')
      .map((item) => ({
        label: `${item.wbsCode} · ${item.name}`,
        value: item.id,
      })),
  );

  const currentSprints = computed(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const sprintItems = sprints.value;
    const activeByDate = sprintItems.filter((item) => {
      if (!item.startDate || !item.endDate) {
        return false;
      }
      const startDate = new Date(item.startDate);
      const endDate = new Date(item.endDate);
      startDate.setHours(0, 0, 0, 0);
      endDate.setHours(23, 59, 59, 999);
      return startDate <= today && endDate >= today;
    });

    if (activeByDate.length > 0) {
      return activeByDate;
    }

    const activeByStatus = sprintItems.filter((item) => ['进行中', '执行中'].includes(item.status));
    if (activeByStatus.length > 0) {
      return activeByStatus;
    }

    return sprintItems
      .filter((item) => !['已关闭', '已取消'].includes(item.status))
      .sort((left, right) => {
        const leftDate = new Date(left.startDate || left.endDate || '').getTime();
        const rightDate = new Date(right.startDate || right.endDate || '').getTime();
        return (
          Math.abs((Number.isNaN(leftDate) ? Date.now() : leftDate) - today.getTime()) -
          Math.abs((Number.isNaN(rightDate) ? Date.now() : rightDate) - today.getTime())
        );
      })
      .slice(0, 1);
  });

  const currentSprintIds = computed(
    () => new Set(currentSprints.value.map((item) => item.id)),
  );

  const taskParentOptions = computed(() =>
    (detail.value?.tasks || []).map((item) => ({
      label: `${item.wbsCode || item.taskCode} · ${item.title}`,
      value: item.id,
    }))
  );

  const requirementSelectOptions = computed(() =>
    (detail.value?.requirements || []).map((item) => ({
      label: `${item.requirementCode} · ${item.title}`,
      value: item.id,
      wbsNodeId: item.wbsNodeId || undefined,
    }))
  );

  const requirementReplacementOptions = computed(() =>
    (detail.value?.requirements || [])
      .filter((item) => item.id !== editingRequirementId.value)
      .map((item) => ({
        label: `${item.requirementCode} V${item.versionNo} · ${item.title}`,
        value: item.id,
      }))
  );

  const filteredRequirements = computed(() =>
    (detail.value?.requirements || [])
      .filter(
        (item) => !requirementStatusFilter.value || item.status === requirementStatusFilter.value
      )
      .filter(
        (item) => !requirementWbsFilter.value || item.wbsNodeId === requirementWbsFilter.value
      )
      .sort(
        (left, right) =>
          (left.wbsNodeName || '').localeCompare(right.wbsNodeName || '', 'zh-CN') ||
          left.requirementCode.localeCompare(right.requirementCode, 'zh-CN', {
            numeric: true,
          }) ||
          right.versionNo - left.versionNo
      )
  );

  const requirementMetrics = computed(() => {
    const requirements = detail.value?.requirements || [];
    return [
      { label: '需求总数', value: requirements.length },
      {
        label: '当前有效',
        value: requirements.filter((item) => item.isCurrent).length,
      },
      {
        label: '已确认',
        value: requirements.filter((item) => ['已确认', '开发中', '验收中'].includes(item.status))
          .length,
      },
      {
        label: '已关联 WBS',
        value: requirements.filter((item) => item.wbsNodeId).length,
      },
    ];
  });

  const relatedTaskCount = computed(
    () => (detail.value?.tasks || []).filter((item) => isTaskRelatedToCurrentUser(item)).length
  );

  const selectedTaskDetail = computed(() => {
    if (!selectedTaskDetailId.value) {
      return null;
    }

    return (
      (detail.value?.tasks || []).find((item) => item.id === selectedTaskDetailId.value) || null
    );
  });

  const selectedTaskRequirement = computed(() => {
    const task = selectedTaskDetail.value;
    if (!task?.requirementId) {
      return null;
    }

    return (
      (detail.value?.requirements || []).find((item) => item.id === task.requirementId) || null
    );
  });

  const selectedTaskDependencies = computed(() => {
    const taskId = selectedTaskDetail.value?.id;
    if (!taskId) {
      return [];
    }

    return (detail.value?.taskDependencies || [])
      .filter((item) => item.predecessorTaskId === taskId || item.successorTaskId === taskId)
      .map((item) => ({
        ...item,
        direction:
          item.predecessorTaskId === taskId ? ('后续需求' as const) : ('前置需求' as const),
        relatedTaskTitle:
          item.predecessorTaskId === taskId ? item.successorTaskTitle : item.predecessorTaskTitle,
      }));
  });

  const selectedTaskWorklogs = computed(() => {
    const taskId = selectedTaskDetail.value?.id;
    if (!taskId) {
      return [];
    }

    return (detail.value?.worklogs || []).filter((item) => item.taskId === taskId);
  });

  const filteredTasks = computed(() => {
    const tasks = detail.value?.tasks || [];
    const scopedTasks =
      taskScope.value === 'all' ? tasks : tasks.filter((item) => isTaskRelatedToCurrentUser(item));

    if (taskSprintFilter.value === 'all') {
      return scopedTasks;
    }

    if (taskSprintFilter.value === 'unscheduled') {
      return scopedTasks.filter((item) => !item.sprintId);
    }

    if (taskSprintFilter.value === 'current') {
      return scopedTasks.filter(
        (item) => item.sprintId && currentSprintIds.value.has(item.sprintId)
      );
    }

    return scopedTasks.filter((item) => item.sprintId === taskSprintFilter.value);
  });

  const taskSprintMetrics = computed(() => {
    const tasks = detail.value?.tasks || [];
    return [
      {
        label: '已进排期',
        value: tasks.filter((item) => item.sprintId).length,
      },
      {
        label: '未进排期',
        value: tasks.filter((item) => !item.sprintId).length,
      },
      {
        label: '当前排期需求',
        value: tasks.filter(
          (item) => item.sprintId && currentSprintIds.value.has(item.sprintId)
        ).length,
      },
    ];
  });

  const selectedTaskSprintMetrics = computed(() => {
    const tasks = filteredTasks.value;
    const closedCount = tasks.filter((item) => ['已完成', '已关闭'].includes(item.status)).length;
    const estimatedHours = tasks.reduce((sum, item) => sum + (item.estimatedWorkHours || 0), 0);
    const actualHours = tasks.reduce((sum, item) => sum + (item.actualWorkHours || 0), 0);
    const blockedCount = tasks.filter((item) => item.isBlocked).length;
    const overdueCount = tasks.filter((item) => {
      if (!item.plannedEndTime || ['已完成', '已关闭'].includes(item.status)) {
        return false;
      }

      const endDate = new Date(item.plannedEndTime);
      endDate.setHours(23, 59, 59, 999);
      return endDate < new Date();
    }).length;

    return [
      { label: '需求数量', value: tasks.length },
      { label: '已完成', value: closedCount },
      { label: '预计工时', value: formatHoursValue(estimatedHours) },
      { label: '已填报工时', value: formatHoursValue(actualHours) },
      { label: '阻塞 / 逾期', value: `${blockedCount} / ${overdueCount}` },
    ];
  });

  const kanbanColumns = computed(() =>
    kanbanOrder.value.map((status) => ({
      items: filteredTasks.value.filter((item) => item.status === status),
      status,
    }))
  );

  const taskTimelineDays = computed(() => {
    const items = filteredTasks.value.filter(
      (item) => item.plannedStartTime || item.plannedEndTime
    );
    if (items.length === 0) {
      return [];
    }

    const start = new Date(
      Math.min(
        ...items.map((item) =>
          new Date(item.plannedStartTime || item.plannedEndTime || new Date()).getTime()
        )
      )
    );
    const end = new Date(
      Math.max(
        ...items.map((item) =>
          new Date(item.plannedEndTime || item.plannedStartTime || new Date()).getTime()
        )
      )
    );
    const diff = Math.min(
      21,
      Math.max(6, Math.ceil((end.getTime() - start.getTime()) / 86_400_000))
    );

    return Array.from({ length: diff + 1 }, (_, index) => {
      const date = new Date(start);
      date.setDate(start.getDate() + index);
      return date;
    });
  });

  const taskTimelineGridStyle = computed(() => ({
    gridTemplateColumns: `repeat(${Math.max(taskTimelineDays.value.length, 1)}, minmax(56px, 1fr))`,
    '--timeline-columns': `${Math.max(taskTimelineDays.value.length, 1)}`,
  }));

  const taskTimelineRows = computed(() => {
    const days = taskTimelineDays.value;
    if (days.length === 0) {
      return [];
    }

    const firstDay = days[0]?.setHours(0, 0, 0, 0) ?? Date.now();
    const lastDay = days[days.length - 1]?.setHours(0, 0, 0, 0) ?? firstDay;

    return filteredTasks.value
      .filter((item) => item.plannedStartTime || item.plannedEndTime)
      .map((item) => {
        const startDate = new Date(
          item.plannedStartTime || item.plannedEndTime || new Date()
        ).setHours(0, 0, 0, 0);
        const endDate = new Date(
          item.plannedEndTime || item.plannedStartTime || new Date()
        ).setHours(0, 0, 0, 0);
        const clampedStart = Math.max(startDate, firstDay);
        const clampedEnd = Math.min(endDate, lastDay);
        return {
          ...item,
          color: resolveTaskColor(item),
          span: Math.max(1, Math.floor((clampedEnd - clampedStart) / 86_400_000) + 1),
          startOffset: Math.floor((clampedStart - firstDay) / 86_400_000),
        };
      });
  });

  const taskCalendarReferenceDate = computed(() => {
    if (taskCalendarAnchorDate.value) {
      return taskCalendarAnchorDate.value;
    }

    const firstTaskDate = filteredTasks.value
      .flatMap((item) => [item.plannedStartTime, item.plannedEndTime])
      .find((value) => Boolean(value));

    return parseDate(firstTaskDate) || new Date();
  });

  const taskCalendarTitle = computed(() => {
    const current = taskCalendarReferenceDate.value;
    return `${current.getFullYear()} 年 ${current.getMonth() + 1} 月`;
  });

  const taskCalendarRows = computed(() => {
    const current = taskCalendarReferenceDate.value;
    const monthStart = new Date(current.getFullYear(), current.getMonth(), 1);
    const monthEnd = new Date(current.getFullYear(), current.getMonth() + 1, 0);
    const rangeStart = new Date(monthStart);
    rangeStart.setDate(monthStart.getDate() - monthStart.getDay());
    const rangeEnd = new Date(monthEnd);
    rangeEnd.setDate(monthEnd.getDate() + (6 - monthEnd.getDay()));
    const todayKey = toDateKey(new Date());
    const rows = [] as Array<{
      key: string;
      label: string;
      days: Array<{
        date: Date;
        dayLabel: number;
        inCurrentMonth: boolean;
        isToday: boolean;
        key: string;
      }>;
      bars: Array<{
        key: string;
        title: string;
        subtitle: string;
        color: string;
        startColumn: number;
        span: number;
      }>;
    }>;

    for (
      let cursor = new Date(rangeStart);
      cursor <= rangeEnd;
      cursor.setDate(cursor.getDate() + 7)
    ) {
      const weekStart = new Date(cursor);
      const weekEnd = new Date(cursor);
      weekEnd.setDate(weekStart.getDate() + 6);
      const days = Array.from({ length: 7 }, (_, index) => {
        const date = new Date(weekStart);
        date.setDate(weekStart.getDate() + index);
        return {
          date,
          dayLabel: date.getDate(),
          inCurrentMonth: date.getMonth() === current.getMonth(),
          isToday: toDateKey(date) === todayKey,
          key: toDateKey(date),
        };
      });

      const bars = filteredTasks.value
        .filter((item) => {
          const start = parseDate(item.plannedStartTime || item.plannedEndTime);
          const end = parseDate(item.plannedEndTime || item.plannedStartTime);
          return start && end && start <= weekEnd && end >= weekStart;
        })
        .map((item) => {
          const start = parseDate(item.plannedStartTime || item.plannedEndTime) || weekStart;
          const end = parseDate(item.plannedEndTime || item.plannedStartTime) || weekEnd;
          const startColumn = Math.max(0, Math.floor((startOfDay(start) - startOfDay(weekStart)) / 86_400_000));
          const endColumn = Math.min(
            6,
            Math.floor((startOfDay(end) - startOfDay(weekStart)) / 86_400_000)
          );
          return {
            key: item.id,
            title: item.title,
            subtitle: `${item.taskCode} · ${item.ownerName}`,
            color: resolveTaskColor(item),
            startColumn: Math.max(0, startColumn),
            span: Math.max(1, endColumn - Math.max(0, startColumn) + 1),
          };
        })
        .sort((left, right) => left.startColumn - right.startColumn || right.span - left.span);

      rows.push({
        key: `${toDateKey(weekStart)}-${toDateKey(weekEnd)}`,
        label: `${weekStart.getMonth() + 1}/${weekStart.getDate()} - ${
          weekEnd.getMonth() + 1
        }/${weekEnd.getDate()}`,
        days,
        bars,
      });
    }

    return rows;
  });

  function resetRequirementForms() {
    editingRequirementId.value = null;
    editingTaskId.value = null;
    splittingRequirementId.value = null;
    showTaskInlineForm.value = false;
    requirementForm.value = emptyRequirementForm();
    taskForm.value = emptyTaskForm();
    taskFromRequirementForm.value = emptyTaskFromRequirementForm();
    taskProgressForm.value = emptyTaskProgressForm();
    requirementStatusFilter.value = '';
    requirementWbsFilter.value = '';
    taskCalendarAnchorDate.value = null;
    taskSprintFilter.value = 'current';
    selectedTaskDetailId.value = null;
  }

  function handleOpenTaskDetail(item: ProjectApi.TaskItem) {
    selectedTaskDetailId.value = item.id;
    taskProgressForm.value = {
      ...emptyTaskProgressForm(),
      isBlocked: item.isBlocked,
      status: !item.status || item.status === '未开始' ? DEFAULT_TASK_STATUS : item.status,
    };
  }

  function handleCloseTaskDetail() {
    selectedTaskDetailId.value = null;
    taskProgressForm.value = emptyTaskProgressForm();
  }

  function handleCreateTaskDraft(wbsNodeId?: string) {
    editingTaskId.value = null;
    const wbsNode = (detail.value?.wbsNodes || []).find((item) => item.id === wbsNodeId);
    taskForm.value = {
      ...emptyTaskForm(),
      sprintId: wbsNode?.sprintId || undefined,
      ownerUserId: wbsNode?.ownerUserId || detail.value?.members[0]?.userId || '',
      productOwnerUserId: wbsNode?.ownerUserId || undefined,
      productOwnerUserIds: wbsNode?.ownerUserId ? [wbsNode.ownerUserId] : [],
      taskCode: '',
      title: wbsNode?.name || '',
      wbsNodeId,
    };
    showTaskInlineForm.value = true;
  }

  function handleEditTask(item: ProjectApi.TaskItem) {
    editingTaskId.value = item.id;
    taskForm.value = {
      sprintId: item.sprintId || undefined,
      developerOwnerUserId: item.developerOwnerUserId || undefined,
      developerOwnerUserIds: item.developerOwnerUserIds?.length
        ? [...item.developerOwnerUserIds]
        : item.developerOwnerUserId
          ? [item.developerOwnerUserId]
          : [],
      estimatedWorkHours: item.estimatedWorkHours ?? undefined,
      description: item.description || '',
      isOutOfContract: item.isOutOfContract,
      ownerUserId: item.ownerUserId,
      plannedEndTime: toDateInputValue(item.plannedEndTime),
      plannedStartTime: toDateInputValue(item.plannedStartTime),
      priority: resolveTaskPriority(item.priority),
      productOwnerUserId: item.productOwnerUserId || undefined,
      productOwnerUserIds: item.productOwnerUserIds?.length
        ? [...item.productOwnerUserIds]
        : item.productOwnerUserId
          ? [item.productOwnerUserId]
          : [],
      status: item.status || '未开始',
      taskCode: item.taskCode,
      testerOwnerUserId: item.testerOwnerUserId || undefined,
      testerOwnerUserIds: item.testerOwnerUserIds?.length
        ? [...item.testerOwnerUserIds]
        : item.testerOwnerUserId
          ? [item.testerOwnerUserId]
          : [],
      title: item.title,
      wbsNodeId: item.wbsNodeId || undefined,
    };
    showTaskInlineForm.value = true;
  }

  function handleCancelTaskEdit() {
    editingTaskId.value = null;
    showTaskInlineForm.value = false;
    taskForm.value = emptyTaskForm();
  }

  function handleCreateRequirementDraft(wbsNodeId?: string) {
    editingRequirementId.value = null;
    const wbsNode = (detail.value?.wbsNodes || []).find((item) => item.id === wbsNodeId);
    requirementForm.value = {
      ...emptyRequirementForm(),
      requirementCode: resolveNextRequirementCode(),
      wbsNodeId,
      milestoneId: wbsNode?.milestoneId || undefined,
      productOwnerUserId: detail.value?.members[0]?.userId,
    };
  }

  function handleEditRequirement(item: ProjectApi.ProjectRequirement) {
    editingRequirementId.value = item.id;
    requirementForm.value = {
      acceptanceCriteria: item.acceptanceCriteria || '',
      businessOwnerUserId: item.businessOwnerUserId || undefined,
      changeReason: item.changeReason || '',
      description: item.description || '',
      impactSummary: item.impactSummary || '',
      isCurrent: item.isCurrent,
      milestoneId: item.milestoneId || undefined,
      priority: resolveTaskPriority(item.priority),
      productOwnerUserId: item.productOwnerUserId || undefined,
      replacesRequirementId: item.replacesRequirementId || undefined,
      requirementCode: item.requirementCode,
      sizeEstimate: item.sizeEstimate ?? undefined,
      source: item.source || '内部',
      status: item.status || '草稿',
      title: item.title,
      type: item.type || '功能',
      versionNo: item.versionNo || 1,
      wbsNodeId: item.wbsNodeId || undefined,
    };
  }

  function handleCancelRequirementEdit() {
    editingRequirementId.value = null;
    requirementForm.value = emptyRequirementForm();
  }

  function handleOpenTaskFromRequirement(item: ProjectApi.ProjectRequirement) {
    splittingRequirementId.value = item.id;
    taskFromRequirementForm.value = {
      ...emptyTaskFromRequirementForm(),
      estimatedWorkHours: item.sizeEstimate ?? undefined,
      ownerUserId: item.productOwnerUserId || detail.value?.members[0]?.userId || '',
      priority: resolveTaskPriority(item.priority),
      productOwnerUserId: item.productOwnerUserId || undefined,
      productOwnerUserIds: item.productOwnerUserId ? [item.productOwnerUserId] : [],
      taskCode: resolveNextTaskCode(),
      title: item.title,
    };
  }

  function handleCancelTaskFromRequirement() {
    splittingRequirementId.value = null;
    taskFromRequirementForm.value = emptyTaskFromRequirementForm();
  }

  async function handleSaveRequirement() {
    if (!selectedProjectId.value) {
      return false;
    }

    if (!requirementForm.value.requirementCode.trim() || !requirementForm.value.title.trim()) {
      message.error('请填写需求编号和标题');
      return false;
    }

    savingRequirement.value = true;
    try {
      const payload = normalizeRequirementForm();
      if (editingRequirementId.value) {
        await updateProjectRequirementApi(editingRequirementId.value, payload);
        message.success('需求已更新');
      } else {
        await createProjectRequirementApi(selectedProjectId.value, payload);
        message.success('需求已创建');
      }

      editingRequirementId.value = null;
      requirementForm.value = emptyRequirementForm();
      await refreshSelectedProjectDetail();
      return true;
    } catch (error) {
      showRequestError(error, editingRequirementId.value ? '需求更新失败' : '需求创建失败');
      return false;
    } finally {
      savingRequirement.value = false;
    }
  }

  async function handleSaveTask() {
    if (!selectedProjectId.value) {
      return false;
    }

    if (!taskForm.value.title.trim()) {
      message.error('请填写需求标题');
      return false;
    }

    if (!taskForm.value.ownerUserId) {
      message.error('请选择负责人');
      return false;
    }

    if (!taskForm.value.wbsNodeId && !editingTaskId.value) {
      message.error('请选择所属 WBS 工作包');
      return false;
    }

    savingTask.value = true;
    try {
      const payload = normalizeTaskForm();
      if (editingTaskId.value) {
        await updateProjectTaskApi(editingTaskId.value, payload);
        message.success('需求已更新');
      } else {
        await createProjectTaskFromWbsApi(
          selectedProjectId.value,
          payload.wbsNodeId || '',
          {
            sprintId: payload.sprintId,
            developerOwnerUserId: payload.developerOwnerUserId,
            developerOwnerUserIds: payload.developerOwnerUserIds,
            description: payload.description,
            estimatedWorkHours: payload.estimatedWorkHours,
            isOutOfContract: payload.isOutOfContract,
            ownerUserId: payload.ownerUserId,
            plannedEndTime: payload.plannedEndTime,
            plannedStartTime: payload.plannedStartTime,
            priority: payload.priority,
            productOwnerUserId: payload.productOwnerUserId,
            productOwnerUserIds: payload.productOwnerUserIds,
            taskCode: '',
            testerOwnerUserId: payload.testerOwnerUserId,
            testerOwnerUserIds: payload.testerOwnerUserIds,
            title: payload.title,
          },
        );
        message.success('需求已创建');
      }

      handleCancelTaskEdit();
      await refreshSelectedProjectDetail();
      return true;
    } catch (error) {
      showRequestError(error, editingTaskId.value ? '需求更新失败' : '需求创建失败');
      return false;
    } finally {
      savingTask.value = false;
    }
  }

  async function handleCreateTaskFromRequirement() {
    if (!selectedProjectId.value || !splittingRequirementId.value) {
      return;
    }

    if (!taskFromRequirementForm.value.title.trim()) {
      message.error('请填写需求标题');
      return;
    }

    if (!taskFromRequirementForm.value.ownerUserId) {
      message.error('请选择需求负责人');
      return;
    }

    savingTaskFromRequirement.value = true;
    try {
      await createProjectTaskFromRequirementApi(
        selectedProjectId.value,
        splittingRequirementId.value,
        {
          ...taskFromRequirementForm.value,
          estimatedWorkHours: taskFromRequirementForm.value.estimatedWorkHours ?? undefined,
          plannedEndTime: taskFromRequirementForm.value.plannedEndTime || undefined,
          plannedStartTime: taskFromRequirementForm.value.plannedStartTime || undefined,
          taskCode: taskFromRequirementForm.value.taskCode.trim(),
          description: taskFromRequirementForm.value.description?.trim() || '',
          isOutOfContract: taskFromRequirementForm.value.isOutOfContract,
          title: taskFromRequirementForm.value.title.trim(),
        }
      );
      message.success('需求执行项已创建');
      handleCancelTaskFromRequirement();
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '需求执行项创建失败');
    } finally {
      savingTaskFromRequirement.value = false;
    }
  }

  async function handleRecordTaskProgress() {
    const task = selectedTaskDetail.value;
    if (!task) {
      return;
    }

    if (!taskProgressForm.value.hours || taskProgressForm.value.hours <= 0) {
      message.error('请填写大于 0 的本次工时');
      return;
    }

    if (taskProgressForm.value.hours > 24) {
      message.error('本次工时不能超过 24 小时');
      return;
    }

    if (!taskProgressForm.value.summary.trim()) {
      message.error('请填写本次进展说明');
      return;
    }

    const isBlocked = taskProgressForm.value.isBlocked || taskProgressForm.value.status === '阻塞';
    if (isBlocked && !taskProgressForm.value.blockReason.trim()) {
      message.error('需求阻塞时请填写阻塞原因');
      return;
    }

    savingTaskProgress.value = true;
    try {
      await recordProjectTaskProgressApi(task.id, {
        blockReason: isBlocked ? taskProgressForm.value.blockReason.trim() : '',
        hours: taskProgressForm.value.hours,
        isBlocked,
        status: taskProgressForm.value.status || DEFAULT_TASK_STATUS,
        summary: taskProgressForm.value.summary.trim(),
        workDate: taskProgressForm.value.workDate || undefined,
      });
      message.success('需求进展已记录');
      await refreshSelectedProjectDetail();
      selectedTaskDetailId.value = task.id;
      taskProgressForm.value = {
        ...emptyTaskProgressForm(),
        status: taskProgressForm.value.status || DEFAULT_TASK_STATUS,
      };
    } catch (error) {
      showRequestError(error, '需求进展记录失败');
    } finally {
      savingTaskProgress.value = false;
    }
  }

  function normalizeRequirementForm(): ProjectApi.ProjectRequirementSaveInput {
    const current = requirementForm.value;
    return {
      acceptanceCriteria: current.acceptanceCriteria?.trim() || '',
      businessOwnerUserId: current.businessOwnerUserId || undefined,
      changeReason: current.changeReason?.trim() || '',
      description: current.description?.trim() || '',
      impactSummary: current.impactSummary?.trim() || '',
      isCurrent: current.isCurrent,
      milestoneId: current.milestoneId || undefined,
      priority: resolveTaskPriority(current.priority),
      productOwnerUserId: current.productOwnerUserId || undefined,
      replacesRequirementId: current.replacesRequirementId || undefined,
      requirementCode: current.requirementCode.trim(),
      sizeEstimate: current.sizeEstimate ?? undefined,
      source: current.source || '内部',
      status: current.status || '草稿',
      title: current.title.trim(),
      type: current.type || '功能',
      versionNo: current.versionNo || 1,
      wbsNodeId: current.wbsNodeId || undefined,
    };
  }

  function normalizeTaskForm(): ProjectApi.TaskSaveInput {
    const current = taskForm.value;
    return {
      sprintId: current.sprintId || undefined,
      developerOwnerUserId: current.developerOwnerUserId || undefined,
      developerOwnerUserIds: current.developerOwnerUserIds || [],
      description: current.description?.trim() || '',
      estimatedWorkHours: current.estimatedWorkHours ?? undefined,
      isOutOfContract: current.isOutOfContract,
      ownerUserId: current.ownerUserId,
      plannedEndTime: current.plannedEndTime || undefined,
      plannedStartTime: current.plannedStartTime || undefined,
      priority: resolveTaskPriority(current.priority),
      productOwnerUserId: current.productOwnerUserId || undefined,
      productOwnerUserIds: current.productOwnerUserIds || [],
      status: current.status || '未开始',
      taskCode: editingTaskId.value ? current.taskCode.trim() : '',
      testerOwnerUserId: current.testerOwnerUserId || undefined,
      testerOwnerUserIds: current.testerOwnerUserIds || [],
      title: current.title.trim(),
      wbsNodeId: current.wbsNodeId || undefined,
    };
  }

  function toDateInputValue(value?: null | string) {
    return value ? value.slice(0, 10) : undefined;
  }

  function resolveNextRequirementCode() {
    const numbers = (detail.value?.requirements || [])
      .map((item) => item.requirementCode.match(/REQ-(\d+)$/i)?.[1])
      .filter((item): item is string => Boolean(item))
      .map((item) => Number(item));
    const next = numbers.length > 0 ? Math.max(...numbers) + 1 : 1;
    return `REQ-${String(next).padStart(3, '0')}`;
  }

  function resolveNextTaskCode() {
    const numbers = (detail.value?.tasks || [])
      .map((item) => item.taskCode.match(/TASK-(\d+)$/i)?.[1])
      .filter((item): item is string => Boolean(item))
      .map((item) => Number(item));
    const next = numbers.length > 0 ? Math.max(...numbers) + 1 : 1;
    return `TASK-${String(next).padStart(3, '0')}`;
  }

  function resolveSprintCode(item: ProjectApi.SprintItem) {
    const code = item.sprintCode;
    if (!code) {
      return '未编号';
    }

    const matched = code.match(/^SPR[-_]?(\d+)$/i);
    if (matched?.[1]) {
      return `排期-${matched[1].padStart(3, '0')}`;
    }

    return code.replace(/Sprint/gi, '排期');
  }

  function formatSprintName(name?: null | string) {
    return (name || '')
      .replace(/第\s*(\d+)\s*迭代/g, '第 $1 排期')
      .replace(/Sprint/gi, '排期');
  }

  function isTaskRelatedToCurrentUser(item: ProjectApi.TaskItem) {
    const userId = currentUserId.value;
    if (
      userId &&
      [
        item.ownerUserId,
        item.productOwnerUserId,
        item.developerOwnerUserId,
        item.testerOwnerUserId,
        ...(item.productOwnerUserIds || []),
        ...(item.developerOwnerUserIds || []),
        ...(item.testerOwnerUserIds || []),
      ].includes(userId)
    ) {
      return true;
    }

    return [
      item.ownerName,
      item.productOwnerName,
      item.developerOwnerName,
      item.testerOwnerName,
      ...(item.productOwnerNames || []),
      ...(item.developerOwnerNames || []),
      ...(item.testerOwnerNames || []),
    ]
      .filter((name): name is string => Boolean(name))
      .some((name) => currentUserAliases.value.some((alias) => alias === name));
  }

  function resolveTaskColor(item: ProjectApi.TaskItem) {
    if (item.isBlocked) {
      return 'var(--ant-color-error)';
    }

    if (item.priority === 'P0') {
      return `var(--ant-color-${getTaskPriorityColor(item.priority, 'warning')})`;
    }

    if (item.status === '已完成' || item.status === '已关闭') {
      return 'var(--ant-color-success)';
    }

    return 'var(--ant-color-primary)';
  }

  function formatHoursValue(value: number) {
    return `${Number(value || 0).toFixed(1)} 小时`;
  }

  function startOfDay(date: Date) {
    const next = new Date(date);
    next.setHours(0, 0, 0, 0);
    return next.getTime();
  }

  function resolveTaskPriority(priority?: null | string) {
    return priority === 'P0' || priority === 'P1' || priority === 'P2' || priority === 'P3'
      ? priority
      : DEFAULT_TASK_PRIORITY;
  }

  async function handleChangeTaskStatus(item: ProjectApi.TaskItem, status: string) {
    if (item.status === status) {
      return;
    }

    savingTask.value = true;
    try {
      await updateProjectTaskApi(item.id, {
        sprintId: item.sprintId || undefined,
        developerOwnerUserId: item.developerOwnerUserId || undefined,
        developerOwnerUserIds: item.developerOwnerUserIds || [],
        description: item.description || '',
        estimatedWorkHours: item.estimatedWorkHours ?? undefined,
        isOutOfContract: item.isOutOfContract,
        ownerUserId: item.ownerUserId,
        plannedEndTime: toDateInputValue(item.plannedEndTime),
        plannedStartTime: toDateInputValue(item.plannedStartTime),
        priority: resolveTaskPriority(item.priority),
        productOwnerUserId: item.productOwnerUserId || undefined,
        productOwnerUserIds: item.productOwnerUserIds || [],
        status,
        taskCode: item.taskCode,
        testerOwnerUserId: item.testerOwnerUserId || undefined,
        testerOwnerUserIds: item.testerOwnerUserIds || [],
        title: item.title,
        wbsNodeId: item.wbsNodeId || undefined,
      });
      message.success('需求状态已更新');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '需求状态更新失败');
    } finally {
      savingTask.value = false;
    }
  }

  function shiftTaskCalendarMonth(offset: number) {
    const base = taskCalendarReferenceDate.value;
    taskCalendarAnchorDate.value = new Date(base.getFullYear(), base.getMonth() + offset, 1);
  }

  return {
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
    handleEditTask,
    handleChangeTaskStatus,
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
    resetRequirementForms,
    savingRequirement,
    savingTask,
    savingTaskFromRequirement,
    savingTaskProgress,
    selectedTaskDetail,
    selectedTaskDependencies,
    selectedTaskRequirement,
    selectedTaskWorklogs,
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
    selectedTaskSprintMetrics,
    taskScope,
    taskScopeOptions,
    taskSelectOptions,
    taskWbsNodeOptions,
    taskTimelineDays,
    taskTimelineGridStyle,
    taskTimelineRows,
    taskView,
    taskViewOptions,
  };
}
