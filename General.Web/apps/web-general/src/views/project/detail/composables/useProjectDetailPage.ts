import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch, type ComponentPublicInstance } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { message } from 'ant-design-vue';

import {
  createProjectAcceptanceCriteriaApi,
  createProjectQualityChecklistApi,
  createProjectQualityInspectionApi,
  createProjectTaskDependencyApi,
  getProjectDetailApi,
  getProjectListApi,
  getUserInfoApi,
  saveProjectCharterApi,
  updateProjectMemberRolesApi,
  updateProjectRaidRiskApi,
  updateProjectTaskWbsApi,
  type ProjectApi,
} from '#/api/core';

export function useProjectDetailPage() {
  const DETAIL_PATH = '/project/detail';
  const STORAGE_KEY = 'general:last-project-id';

  const route = useRoute();
  const router = useRouter();

  const activeTab = ref('overview');
  const detail = ref<null | ProjectApi.ProjectDetail>(null);
  const currentUserName = ref('');
  const currentUsername = ref('');
  const loading = ref(false);
  const loadingProjects = ref(false);
  const savingAcceptanceCriteria = ref(false);
  const savingCharter = ref(false);
  const savingMemberRoleId = ref('');
  const editingMemberId = ref<string | null>(null);
  const savingQualityChecklist = ref(false);
  const savingQualityInspection = ref(false);
  const savingRaidRiskId = ref('');
  const savingTaskDependency = ref(false);
  const savingTaskWbsId = ref('');
  const projectOptions = ref<ProjectApi.ProjectListItem[]>([]);
  const selectedProjectId = ref<string | undefined>(undefined);
  const taskScope = ref<'all' | 'related'>('all');
  const taskView = ref<'board' | 'calendar' | 'gantt' | 'list'>('list');
  const taskCalendarAnchorDate = ref<Date | null>(null);
  const tabContentElement = ref<HTMLElement | null>(null);
  const tabBodyHeight = ref<number>(520);
  let tabContentObserver: null | ResizeObserver = null;

  const charterForm = ref<ProjectApi.CharterSaveInput>({
    approvalStatus: '草稿',
    assumptions: '',
    businessCase: '',
    constraints: '',
    objectives: '',
    scopeBoundary: '',
    successCriteria: '',
  });
  const taskDependencyForm = ref<ProjectApi.TaskDependencySaveInput>({
    dependencyType: 'FinishToStart',
    predecessorTaskId: '',
    successorTaskId: '',
  });
  const acceptanceCriteriaForm = ref<ProjectApi.AcceptanceCriteriaSaveInput>({
    description: '',
    milestoneId: undefined,
    status: '待确认',
    taskId: undefined,
    title: '',
  });
  const qualityChecklistForm = ref<ProjectApi.QualityChecklistSaveInput>({
    category: '通用',
    description: '',
    title: '',
  });
  const qualityInspectionForm = ref<ProjectApi.QualityInspectionSaveInput>({
    findingSummary: '',
    inspectionResult: '通过',
    inspectionTime: undefined,
    inspectorUserId: '',
    qualityChecklistId: '',
    rectificationRequirement: '',
  });
  const memberRoleForms = ref<Record<string, string[]>>({});
  const raidRiskForms = ref<Record<string, ProjectApi.RaidRiskUpdateInput>>({});
  const taskWbsForms = ref<Record<string, ProjectApi.TaskWbsUpdateInput>>({});

  const showAcceptanceForm = ref(false);
  const showChecklistForm = ref(false);
  const showInspectionForm = ref(false);

  const taskColumns = [
    { dataIndex: 'taskCode', key: 'taskCode', title: '事项编号', width: 120 },
    { dataIndex: 'title', key: 'title', title: '事项标题', width: 280, ellipsis: true },
    { dataIndex: 'contractClause', key: 'contractClause', title: '关联合同条款', width: 180, ellipsis: true },
    { dataIndex: 'productOwnerName', key: 'productOwnerName', title: '产品', width: 100 },
    { dataIndex: 'developerOwnerName', key: 'developerOwnerName', title: '开发', width: 100 },
    { dataIndex: 'testerOwnerName', key: 'testerOwnerName', title: '测试', width: 100 },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 100 },
    { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划完成', width: 120 },
    { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  ];
  const cycleColumns = [
    { dataIndex: 'type', key: 'type', title: '类型', width: 100 },
    { dataIndex: 'name', key: 'name', title: '名称' },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
    { dataIndex: 'progress', key: 'progress', title: '进度', width: 180 },
    { dataIndex: 'endDate', key: 'endDate', title: '结束日期', width: 140 },
  ];
  const milestoneColumns = [
    { dataIndex: 'name', key: 'name', title: '里程碑' },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
    { dataIndex: 'plannedCompletionDate', key: 'plannedCompletionDate', title: '计划完成', width: 140 },
    { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  ];
  const issueColumns = [
    { dataIndex: 'title', key: 'title', title: '问题项', width: 240, ellipsis: true },
    { dataIndex: 'requirementTitle', key: 'requirementTitle', title: '需求', width: 160, ellipsis: true },
    { dataIndex: 'productOwnerName', key: 'productOwnerName', title: '产品', width: 100 },
    { dataIndex: 'developerOwnerName', key: 'developerOwnerName', title: '开发', width: 100 },
    { dataIndex: 'testerOwnerName', key: 'testerOwnerName', title: '测试', width: 100 },
    { dataIndex: 'type', key: 'type', title: '类型', width: 90 },
    { dataIndex: 'level', key: 'level', title: '等级', width: 80 },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 100 },
    { dataIndex: 'dueDate', key: 'dueDate', title: '到期', width: 120 },
    { dataIndex: 'status', key: 'status', title: '状态', width: 100 },
  ];
  const documentColumns = [
    { dataIndex: 'category', key: 'category', title: '分类', width: 100 },
    { dataIndex: 'title', key: 'title', title: '文档标题' },
    { dataIndex: 'version', key: 'version', title: '版本', width: 100 },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
    { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  ];
  const memberColumns = [
    { dataIndex: 'userName', key: 'userName', title: '成员', width: 120 },
    { dataIndex: 'organizationUnitName', key: 'organizationUnitName', title: '所属部门', width: 140 },
    { dataIndex: 'roleNames', key: 'roleNames', title: '项目角色', width: 200 },
    { dataIndex: 'accessLevel', key: 'accessLevel', title: '访问级别', width: 100 },
    { dataIndex: 'joinDate', key: 'joinDate', title: '加入时间', width: 120 },
    { dataIndex: 'actions', key: 'actions', title: '操作', width: 140, fixed: 'right' as const },
  ];
  const worklogColumns = [
    { dataIndex: 'workDate', key: 'workDate', title: '日期', width: 140 },
    { dataIndex: 'userName', key: 'userName', title: '成员', width: 120 },
    { dataIndex: 'taskTitle', key: 'taskTitle', title: '关联事项' },
    { dataIndex: 'summary', key: 'summary', title: '工作内容' },
    { dataIndex: 'hours', key: 'hours', title: '工时', width: 90 },
  ];
  const reportColumns = [
    { dataIndex: 'category', key: 'category', title: '类型', width: 100 },
    { dataIndex: 'title', key: 'title', title: '报告标题' },
    { dataIndex: 'version', key: 'version', title: '版本', width: 100 },
    { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
    { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  ];

  const tabItems = [
    { key: 'overview', label: '项目详细信息' },
    { key: 'plan', label: '计划与周期' },
    { key: 'tasks', label: '需求与任务' },
    { key: 'issues', label: '缺陷与问题' },
    { key: 'delivery', label: '验收与交付' },
    { key: 'docs', label: '文档与知识' },
    { key: 'resources', label: '人员与资源' },
  ];
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
  const kanbanOrder = ['未开始', '进行中', '阻塞', '已完成', '已关闭'];
  const dependencyTypeOptions = [
    { label: '完成后开始', value: 'FinishToStart' },
    { label: '开始后开始', value: 'StartToStart' },
    { label: '完成后完成', value: 'FinishToFinish' },
    { label: '开始后完成', value: 'StartToFinish' },
  ];
  const approvalStatusOptions = ['草稿', '待审批', '已批准', '退回'].map((value) => ({ label: value, value }));
  const acceptanceStatusOptions = ['待确认', '已确认', '已验收', '不适用'].map((value) => ({ label: value, value }));
  const qualityCategoryOptions = ['通用', '范围', '进度', '成本', '质量', '交付', '验收'].map((value) => ({ label: value, value }));
  const qualityInspectionResultOptions = ['通过', '有条件通过', '不通过', '需整改'].map((value) => ({ label: value, value }));
  const riskResponseOptions = ['规避', '转移', '减轻', '接受', '升级'].map((value) => ({ label: value, value }));
  const projectRoleOptions = ['PM', 'PMO', '产品', '开发', '测试', '实施', '运维', '商务', '财务', '干系人'].map((value) => ({
    label: value,
    value,
  }));

  const currentUserAliases = computed(() =>
    [currentUserName.value, currentUsername.value].filter((item) => item.trim().length > 0),
  );

  const taskSelectOptions = computed(() =>
    (detail.value?.tasks || []).map((item) => ({
      label: `${item.wbsCode || item.taskCode} · ${item.title}`,
      value: item.id,
    })),
  );

  const milestoneSelectOptions = computed(() =>
    (detail.value?.milestones || []).map((item) => ({
      label: item.name,
      value: item.id,
    })),
  );

  const qualityChecklistOptions = computed(() =>
    (detail.value?.qualityChecklists || []).map((item) => ({
      label: `${item.category} · ${item.title}`,
      value: item.id,
    })),
  );

  const memberSelectOptions = computed(() =>
    (detail.value?.members || []).map((item) => ({
      label: item.userName,
      value: item.userId,
    })),
  );


  const raidRiskRows = computed(() =>
    (detail.value?.raidItems || []).map((item) => {
      let form = raidRiskForms.value[item.id];
      if (!form) {
        form = {
          contingencyPlan: item.contingencyPlan || '',
          impact: item.impact || 0,
          probability: item.probability || 0,
          responseStrategy: item.responseStrategy || '',
          triggerCondition: item.triggerCondition || '',
        };
        raidRiskForms.value[item.id] = form;
      }

      return { form, item };
    }),
  );
  // TODO(PMP-risk-scoring): 当前先承接传统规则评分（概率 x 影响）。
  // 后续预留朴素贝叶斯模型入口，基于历史 RAID、缺陷、逾期和关闭结果训练风险概率。

  const taskParentOptions = computed(() =>
    (detail.value?.tasks || []).map((item) => ({
      label: `${item.wbsCode || item.taskCode} · ${item.title}`,
      value: item.id,
    })),
  );

  const taskWbsRows = computed(() =>
    (detail.value?.tasks || []).map((item) => {
      let form = taskWbsForms.value[item.id];
      if (!form) {
        form = {
          baselineEndTime: toDateInputValue(item.baselineEndTime),
          baselineStartTime: toDateInputValue(item.baselineStartTime),
          parentTaskId: item.parentTaskId || undefined,
          sortOrder: item.sortOrder || 0,
          wbsCode: item.wbsCode || '',
        };
        taskWbsForms.value[item.id] = form;
      }

      return { form, item };
    }),
  );

  const compactMetrics = computed(() => {
    const current = detail.value;
    return [
      { label: '健康度', value: resolveHealthStatus().label },
      { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
      { label: '待处理问题', value: `${current?.openIssueCount || 0}` },
      { label: '成员 / 工时', value: `${current?.memberCount || 0} / ${current?.totalWorklogHours || 0}小时` },
    ];
  });

  const reportItems = computed(() =>
    (detail.value?.documents || []).filter((item) =>
      ['日报', '周报', '月报', '阶段报告', '发布报告'].some((keyword) => item.category.includes(keyword) || item.title.includes(keyword)),
    ),
  );

  const knowledgeCollections = computed(() => {
    const docs = detail.value?.documents || [];
    const groups = new Map<string, ProjectApi.DocumentItem[]>();

    for (const doc of docs) {
      const key = doc.category || '未分类';
      const items = groups.get(key) || [];
      items.push(doc);
      groups.set(key, items);
    }

    return Array.from(groups.entries()).map(([name, items]) => ({
      name,
      count: items.length,
      description: items[0]?.summary || '沉淀该分类下的标准文档、评论记录与知识要点。',
      items,
    }));
  });

  const featuredDocuments = computed(() => (detail.value?.documents || []).slice(0, 6));

  const issueMetrics = computed(() => {
    const items = detail.value?.issues || [];
    return [
      { label: '当前问题总数', value: items.length },
      { label: '高优先级', value: items.filter((item) => item.level === '高').length },
      { label: '待测试验证', value: items.filter((item) => item.testerOwnerName).length },
      { label: '已关联需求', value: items.filter((item) => item.requirementTitle).length },
    ];
  });

  const riskAlerts = computed(() =>
    (detail.value?.raidItems || [])
      .filter((item) => item.level === '高' || item.isOverdue)
      .slice(0, 4),
  );

  const currentHealth = computed(() => resolveHealthStatus());

  const relatedTaskCount = computed(() =>
    (detail.value?.tasks || []).filter((item) => isTaskRelatedToCurrentUser(item)).length,
  );

  const relatedIssueCount = computed(() =>
    (detail.value?.issues || []).filter((item) =>
      [item.ownerName, item.productOwnerName, item.developerOwnerName, item.testerOwnerName]
        .filter((name): name is string => Boolean(name))
        .some((name) => matchesCurrentUser(name)),
    ).length,
  );

  const relatedWorklogCount = computed(() =>
    (detail.value?.worklogs || []).filter((item) => matchesCurrentUser(item.userName)).length,
  );

  const overviewDetailSections = computed(() => {
    const current = detail.value;

    return [
      {
        items: [
          { label: '项目编号', value: current?.projectCode || '-' },
          { label: '项目类型', value: current?.projectType || '-' },
          { label: '项目来源', value: current?.projectSource || '-' },
          { label: '所属部门', value: current?.organizationUnitName || '-' },
          { label: '项目经理', value: current?.managerName || '-' },
          { label: '发起人', value: current?.sponsorName || '-' },
        ],
        title: '基础信息',
      },
      {
        items: [
          { label: '计划开始', value: formatDate(current?.plannedStartDate) },
          { label: '计划结束', value: formatDate(current?.plannedEndDate) },
          { label: '合同金额', value: formatMoney(current?.contractTotalAmount) },
          { label: '预算金额', value: formatMoney(current?.budgetTotalAmount) },
          { label: '已回款', value: formatMoney(current?.receivedAmount) },
          { label: '关键项目', value: current?.isKeyProject ? '是' : '否' },
        ],
        title: '计划与经营',
      },
    ];
  });

  const healthMetrics = computed(() => {
    const current = detail.value;
    return [
      { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
      { label: '高风险事项', value: `${current?.highRiskCount || 0} 项` },
      { label: '未闭环问题', value: `${current?.openIssueCount || 0} 项` },
    ];
  });

  const collaborationMetrics = computed(() => {
    const current = detail.value;
    return [
      { label: '我的项目关系', value: current?.myRelation || '-' },
      { label: '我负责事项', value: `${relatedTaskCount.value}` },
      { label: '我跟进问题', value: `${relatedIssueCount.value}` },
      { label: '我的工时记录', value: `${relatedWorklogCount.value}` },
      { label: '成员规模', value: `${current?.memberCount || 0} 人` },
      { label: '累计工时', value: `${current?.totalWorklogHours || 0} 小时` },
    ];
  });

  const milestonePreview = computed(() => (detail.value?.milestones || []).slice(0, 3));
  const cyclePreview = computed(() => (detail.value?.cycles || []).slice(0, 3));

  const filteredTasks = computed(() => {
    const tasks = detail.value?.tasks || [];
    if (taskScope.value === 'all') {
      return tasks;
    }

    return tasks.filter((item) => isTaskRelatedToCurrentUser(item));
  });

  const kanbanColumns = computed(() =>
    kanbanOrder.map((status) => ({
      items: filteredTasks.value.filter((item) => item.status === status),
      status,
    })),
  );

  const taskTimelineDays = computed(() => {
    const items = filteredTasks.value.filter((item) => item.plannedStartTime || item.plannedEndTime);
    if (items.length === 0) {
      return [];
    }

    const start = new Date(
      Math.min(
        ...items.map((item) => new Date(item.plannedStartTime || item.plannedEndTime || new Date()).getTime()),
      ),
    );
    const end = new Date(
      Math.max(
        ...items.map((item) => new Date(item.plannedEndTime || item.plannedStartTime || new Date()).getTime()),
      ),
    );
    const diff = Math.min(21, Math.max(6, Math.ceil((end.getTime() - start.getTime()) / 86_400_000)));

    return Array.from({ length: diff + 1 }, (_, index) => {
      const date = new Date(start);
      date.setDate(start.getDate() + index);
      return date;
    });
  });

  const taskTimelineGridStyle = computed(() => ({
    gridTemplateColumns: `repeat(${Math.max(taskTimelineDays.value.length, 1)}, minmax(56px, 1fr))`,
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
        const startDate = new Date(item.plannedStartTime || item.plannedEndTime || new Date()).setHours(0, 0, 0, 0);
        const endDate = new Date(item.plannedEndTime || item.plannedStartTime || new Date()).setHours(0, 0, 0, 0);
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
      days: Array<{ date: Date; dayLabel: number; inCurrentMonth: boolean; isToday: boolean; key: string }>;
      bars: Array<{
        key: string;
        title: string;
        subtitle: string;
        color: string;
        startColumn: number;
        span: number;
      }>;
    }>;

    for (let cursor = new Date(rangeStart); cursor <= rangeEnd; cursor.setDate(cursor.getDate() + 7)) {
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
          const startColumn = Math.max(0, Math.floor((start.setHours(0, 0, 0, 0) - weekStart.setHours(0, 0, 0, 0)) / 86_400_000));
          const endColumn = Math.min(6, Math.floor((end.setHours(0, 0, 0, 0) - weekStart.setHours(0, 0, 0, 0)) / 86_400_000));
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
        label: `${weekStart.getMonth() + 1}/${weekStart.getDate()} - ${weekEnd.getMonth() + 1}/${weekEnd.getDate()}`,
        days,
        bars,
      });
    }

    return rows;
  });

  const workflowPreviewGroups = computed(() => {
    const issues = detail.value?.issues || [];
    const closedKeywords = ['关闭', '完成', '解决'];
    return [
      {
        key: 'defect-flow',
        title: '缺陷处理流',
        tone: 'danger',
        steps: [
          { key: 'capture', title: '登记', count: issues.filter((item) => item.type.includes('缺陷')).length },
          { key: 'analysis', title: '分析', count: issues.filter((item) => item.type.includes('缺陷') && item.productOwnerName).length },
          { key: 'fix', title: '修复', count: issues.filter((item) => item.type.includes('缺陷') && item.developerOwnerName).length },
          { key: 'verify', title: '验证', count: issues.filter((item) => item.type.includes('缺陷') && item.testerOwnerName).length },
          { key: 'done', title: '关闭', count: issues.filter((item) => item.type.includes('缺陷') && closedKeywords.some((keyword) => item.status.includes(keyword))).length },
        ],
      },
      {
        key: 'issue-flow',
        title: '问题跟踪流',
        tone: 'warning',
        steps: [
          { key: 'raise', title: '提出', count: issues.filter((item) => item.type.includes('问题')).length },
          { key: 'scope', title: '定责', count: issues.filter((item) => item.type.includes('问题') && item.requirementTitle).length },
          { key: 'coordination', title: '协同', count: issues.filter((item) => item.type.includes('问题') && item.productOwnerName).length },
          { key: 'tracking', title: '跟踪', count: issues.filter((item) => item.type.includes('问题') && item.developerOwnerName).length },
          { key: 'archive', title: '归档', count: issues.filter((item) => item.type.includes('问题') && closedKeywords.some((keyword) => item.status.includes(keyword))).length },
        ],
      },
      {
        key: 'change-flow',
        title: '变更评审流',
        tone: 'primary',
        steps: [
          { key: 'collect', title: '收集', count: issues.length },
          { key: 'requirement', title: '需求确认', count: issues.filter((item) => item.requirementTitle).length },
          { key: 'product', title: '产品评审', count: issues.filter((item) => item.productOwnerName).length },
          { key: 'develop', title: '开发排期', count: issues.filter((item) => item.developerOwnerName).length },
          { key: 'release', title: '发布闭环', count: issues.filter((item) => closedKeywords.some((keyword) => item.status.includes(keyword))).length },
        ],
      },
    ];
  });

  const tabBodyStyle = computed(() => ({
    minHeight: `${tabBodyHeight.value}px`,
  }));


  async function loadProjects() {
    loadingProjects.value = true;
    try {
      const [projects, userInfo] = await Promise.all([getProjectListApi(), getUserInfoApi()]);
      const currentUser = userInfo as unknown as { realName?: string; username?: string };
      projectOptions.value = projects;
      currentUserName.value = currentUser.realName || '';
      currentUsername.value = currentUser.username || '';
      syncSelectedProject();
    } finally {
      loadingProjects.value = false;
    }
  }

  async function loadDetail(projectId: string) {
    loading.value = true;
    try {
      detail.value = await getProjectDetailApi(projectId);
      syncGovernanceForms();
      taskCalendarAnchorDate.value = null;
    } finally {
      loading.value = false;
      await nextTick();
      syncTabBodyHeight();
    }
  }

  function syncGovernanceForms() {
    const current = detail.value;
    charterForm.value = {
      approvalStatus: current?.charter?.approvalStatus || '草稿',
      assumptions: current?.charter?.assumptions || '',
      businessCase: current?.charter?.businessCase || '',
      constraints: current?.charter?.constraints || '',
      objectives: current?.charter?.objectives || '',
      scopeBoundary: current?.charter?.scopeBoundary || '',
      successCriteria: current?.charter?.successCriteria || '',
    };
    taskDependencyForm.value = {
      dependencyType: 'FinishToStart',
      predecessorTaskId: '',
      successorTaskId: '',
    };
    acceptanceCriteriaForm.value = {
      description: '',
      milestoneId: undefined,
      status: '待确认',
      taskId: undefined,
      title: '',
    };
    qualityChecklistForm.value = {
      category: '通用',
      description: '',
      title: '',
    };
    qualityInspectionForm.value = {
      findingSummary: '',
      inspectionResult: '通过',
      inspectionTime: undefined,
      inspectorUserId: current?.members[0]?.userId || '',
      qualityChecklistId: current?.qualityChecklists[0]?.id || '',
      rectificationRequirement: '',
    };
    memberRoleForms.value = Object.fromEntries(
      (current?.members || []).map((item) => [item.id, [...item.roleNames]]),
    );
    raidRiskForms.value = Object.fromEntries(
      (current?.raidItems || []).map((item) => [
        item.id,
        {
          contingencyPlan: item.contingencyPlan || '',
          impact: item.impact || 0,
          probability: item.probability || 0,
          responseStrategy: item.responseStrategy || '',
          triggerCondition: item.triggerCondition || '',
        },
      ]),
    );
    taskWbsForms.value = Object.fromEntries(
      (current?.tasks || []).map((item) => [
        item.id,
        {
          baselineEndTime: toDateInputValue(item.baselineEndTime),
          baselineStartTime: toDateInputValue(item.baselineStartTime),
          parentTaskId: item.parentTaskId || undefined,
          sortOrder: item.sortOrder || 0,
          wbsCode: item.wbsCode || '',
        },
      ]),
    );
  }

  async function refreshSelectedProjectDetail() {
    if (!selectedProjectId.value) {
      return;
    }

    await loadDetail(selectedProjectId.value);
  }

  async function handleSaveCharter() {
    if (!selectedProjectId.value) {
      return;
    }

    savingCharter.value = true;
    try {
      await saveProjectCharterApi(selectedProjectId.value, charterForm.value);
      message.success('项目章程已保存');
      await refreshSelectedProjectDetail();
    } finally {
      savingCharter.value = false;
    }
  }

  async function handleCreateTaskDependency() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!taskDependencyForm.value.predecessorTaskId || !taskDependencyForm.value.successorTaskId) {
      message.error('请选择前置任务和后续任务');
      return;
    }

    savingTaskDependency.value = true;
    try {
      await createProjectTaskDependencyApi(selectedProjectId.value, taskDependencyForm.value);
      message.success('任务依赖已创建');
      await refreshSelectedProjectDetail();
    } finally {
      savingTaskDependency.value = false;
    }
  }

  async function handleCreateAcceptanceCriteria() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!acceptanceCriteriaForm.value.taskId && !acceptanceCriteriaForm.value.milestoneId) {
      message.error('验收标准必须关联任务或里程碑');
      return;
    }

    savingAcceptanceCriteria.value = true;
    try {
      await createProjectAcceptanceCriteriaApi(selectedProjectId.value, acceptanceCriteriaForm.value);
      message.success('验收标准已创建');
      await refreshSelectedProjectDetail();
    } finally {
      savingAcceptanceCriteria.value = false;
    }
  }

  async function handleCreateQualityChecklist() {
    if (!selectedProjectId.value) {
      return;
    }

    savingQualityChecklist.value = true;
    try {
      await createProjectQualityChecklistApi(selectedProjectId.value, qualityChecklistForm.value);
      message.success('质量检查单已创建');
      await refreshSelectedProjectDetail();
    } finally {
      savingQualityChecklist.value = false;
    }
  }

  async function handleCreateQualityInspection() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!qualityInspectionForm.value.qualityChecklistId || !qualityInspectionForm.value.inspectorUserId) {
      message.error('请选择检查单和检查人');
      return;
    }

    savingQualityInspection.value = true;
    try {
      await createProjectQualityInspectionApi(selectedProjectId.value, qualityInspectionForm.value);
      message.success('质量检查记录已创建');
      await refreshSelectedProjectDetail();
    } finally {
      savingQualityInspection.value = false;
    }
  }

  async function handleUpdateTaskWbs(taskId: string) {
    const form = taskWbsForms.value[taskId];
    if (!form) {
      return;
    }

    savingTaskWbsId.value = taskId;
    try {
      await updateProjectTaskWbsApi(taskId, {
        ...form,
        parentTaskId: form.parentTaskId || undefined,
      });
      message.success('WBS 信息已更新');
      await refreshSelectedProjectDetail();
    } finally {
      savingTaskWbsId.value = '';
    }
  }

  async function handleUpdateMemberRoles(projectMemberId: string) {
    const roleNames = memberRoleForms.value[projectMemberId] || [];
    if (roleNames.length === 0) {
      message.error('项目成员至少需要一个角色');
      return;
    }

    savingMemberRoleId.value = projectMemberId;
    try {
      await updateProjectMemberRolesApi(projectMemberId, { roleNames });
      message.success('成员角色已更新');
      await refreshSelectedProjectDetail();
    } finally {
      savingMemberRoleId.value = '';
    }
  }

  function handleEditMemberRoles(member: ProjectApi.MemberItem) {
    memberRoleForms.value[member.id] = [...(member.roleNames || [])];
    editingMemberId.value = member.id;
  }

  async function handleSaveMemberRoles(projectMemberId: string) {
    await handleUpdateMemberRoles(projectMemberId);
    editingMemberId.value = null;
  }

  async function handleUpdateRaidRisk(raidItemId: string) {
    const form = raidRiskForms.value[raidItemId];
    if (!form) {
      return;
    }

    savingRaidRiskId.value = raidItemId;
    try {
      await updateProjectRaidRiskApi(raidItemId, form);
      message.success('风险评分已更新');
      await refreshSelectedProjectDetail();
    } finally {
      savingRaidRiskId.value = '';
    }
  }

  function formatDate(value?: null | string) {
    return value ? new Date(value).toLocaleDateString() : '-';
  }

  function toDateInputValue(value?: null | string) {
    return value ? value.slice(0, 10) : undefined;
  }

  function formatMoney(value?: null | number) {
    return typeof value === 'number' ? value.toLocaleString('zh-CN') : '-';
  }

  function openDocumentWorkspace(item: ProjectApi.DocumentItem) {
    const docWindow = window.open('', '_blank');
    if (!docWindow) {
      return;
    }

    const safeTitle = item.title || '未命名文档';
    const safeSummary = item.summary || '暂无文档内容，可先在这里补充草稿。';
    const html = `<!doctype html>
    <html lang="zh-CN">
      <head>
        <meta charset="UTF-8" />
        <title>${safeTitle}</title>
        <style>
          body { margin: 0; font-family: Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f6f8fb; color: #1f2937; }
          .shell { max-width: 1080px; margin: 0 auto; padding: 32px; }
          .meta { display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px; }
          .tag { padding: 6px 12px; border-radius: 999px; background: #e8f0ff; color: #2563eb; font-size: 12px; }
          .editor { min-height: 72vh; padding: 28px; border-radius: 20px; background: #fff; box-shadow: 0 20px 60px rgba(15, 23, 42, 0.08); line-height: 1.75; }
          .editor[contenteditable='true']:focus { outline: 2px solid rgba(37, 99, 235, 0.25); }
          h1 { margin: 0 0 12px; font-size: 32px; }
          p { margin: 0; white-space: pre-wrap; }
        </style>
      </head>
      <body>
        <div class="shell">
          <div class="meta">
            <span class="tag">${item.category || '知识文档'}</span>
            <span class="tag">${item.version || 'v1.0'}</span>
            <span class="tag">${item.status || '草稿'}</span>
            <span class="tag">${item.ownerName || '未指定负责人'}</span>
          </div>
          <div class="editor" contenteditable="true">
            <h1>${safeTitle}</h1>
            <p>${safeSummary}</p>
          </div>
        </div>
      </body>
    </html>`;
    docWindow.location.href = URL.createObjectURL(new Blob([html], { type: 'text/html;charset=utf-8' }));
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

  function matchesCurrentUser(name: string) {
    return currentUserAliases.value.some((alias) => alias === name);
  }

  function isTaskRelatedToCurrentUser(item: ProjectApi.TaskItem) {
    return [item.ownerName, item.productOwnerName, item.developerOwnerName, item.testerOwnerName]
      .filter((name): name is string => Boolean(name))
      .some((name) => matchesCurrentUser(name));
  }

  function resolveHealthStatus() {
    const current = detail.value;
    if (!current) {
      return { color: 'default', label: '-', percent: 0 };
    }

    const riskScore = current.healthRiskScore ?? current.highRiskCount * 20 + current.openIssueCount * 10 + Math.max(0, 100 - current.taskCompletionRate);
    if (riskScore >= 70) {
      return { color: 'error', label: '高风险', percent: Math.max(20, 100 - riskScore) };
    }

    if (riskScore >= 35) {
      return { color: 'warning', label: '关注中', percent: Math.max(35, 100 - riskScore) };
    }

    return { color: 'success', label: '健康', percent: Math.max(60, 100 - riskScore) };
  }

  function resolveTaskColor(item: ProjectApi.TaskItem) {
    if (item.isBlocked) {
      return 'var(--ant-color-error)';
    }

    if (item.priority === '高') {
      return 'var(--ant-color-warning)';
    }

    if (item.status === '已完成' || item.status === '已关闭') {
      return 'var(--ant-color-success)';
    }

    return 'var(--ant-color-primary)';
  }

  function shiftTaskCalendarMonth(offset: number) {
    const base = taskCalendarReferenceDate.value;
    taskCalendarAnchorDate.value = new Date(base.getFullYear(), base.getMonth() + offset, 1);
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
    tabContentElement.value = ((element as ComponentPublicInstance | null)?.$el ?? element) as HTMLElement | null;

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

    const routeProjectId = typeof route.query.projectId === 'string' ? route.query.projectId : null;
    const storedProjectId = localStorage.getItem(STORAGE_KEY);
    const candidate = routeProjectId || storedProjectId;

    if (!candidate) {
      selectedProjectId.value = undefined;
      detail.value = null;
      return;
    }

    const exists = projectOptions.value.some((item) => item.id === candidate);
    if (!exists) {
      selectedProjectId.value = undefined;
      detail.value = null;
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    selectedProjectId.value = candidate;
    localStorage.setItem(STORAGE_KEY, candidate);

    if (routeProjectId !== candidate) {
      router.replace({
        path: DETAIL_PATH,
        query: {
          projectId: candidate,
        },
      });
      return;
    }

    loadDetail(candidate);
  }

  async function handleProjectChange(projectId: string | undefined) {
    if (!projectId) {
      return;
    }

    selectedProjectId.value = projectId;
    localStorage.setItem(STORAGE_KEY, projectId);
    await router.replace({
      path: DETAIL_PATH,
      query: {
        projectId,
      },
    });
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
    () => {
      void scheduleSyncTabBodyHeight();
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

  onMounted(loadProjects);
  onBeforeUnmount(disconnectTabObserver);


    return {
      acceptanceCriteriaForm,
      acceptanceStatusOptions,
      activeTab,
      approvalStatusOptions,
      bindTabContent,
      charterForm,
      collaborationMetrics,
      compactMetrics,
      currentHealth,
      cycleColumns,
      cyclePreview,
      dependencyTypeOptions,
      detail,
      documentColumns,
      editingMemberId,
      featuredDocuments,
      filteredTasks,
      formatDate,
      formatMoney,
      handleCreateAcceptanceCriteria,
      handleCreateQualityChecklist,
      handleCreateQualityInspection,
      handleCreateTaskDependency,
      handleEditMemberRoles,
      handleProjectChange,
      handleSaveCharter,
      handleSaveMemberRoles,
      handleUpdateRaidRisk,
      handleUpdateTaskWbs,
      healthMetrics,
      issueColumns,
      issueMetrics,
      kanbanColumns,
      knowledgeCollections,
      loading,
      loadingProjects,
      memberColumns,
      memberRoleForms,
      memberSelectOptions,
      milestoneColumns,
      milestonePreview,
      milestoneSelectOptions,
      openDocumentWorkspace,
      overviewDetailSections,
      projectOptions,
      projectRoleOptions,
      qualityCategoryOptions,
      qualityChecklistForm,
      qualityChecklistOptions,
      qualityInspectionForm,
      qualityInspectionResultOptions,
      raidRiskRows,
      reportColumns,
      reportItems,
      riskAlerts,
      riskResponseOptions,
      savingAcceptanceCriteria,
      savingCharter,
      savingMemberRoleId,
      savingQualityChecklist,
      savingQualityInspection,
      savingRaidRiskId,
      savingTaskDependency,
      savingTaskWbsId,
      selectedProjectId,
      showAcceptanceForm,
      showChecklistForm,
      showInspectionForm,
      tabBodyStyle,
      tabItems,
      taskCalendarAnchorDate,
      taskCalendarRows,
      taskCalendarTitle,
      taskColumns,
      taskDependencyForm,
      taskParentOptions,
      taskScope,
      taskScopeOptions,
      taskSelectOptions,
      taskTimelineDays,
      taskTimelineGridStyle,
      taskTimelineRows,
      taskView,
      taskViewOptions,
      taskWbsRows,
      worklogColumns,
      workflowPreviewGroups,
      shiftTaskCalendarMonth,
    };
}
