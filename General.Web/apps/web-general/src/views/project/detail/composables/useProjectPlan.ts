import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  createProjectSprintApi,
  createProjectMilestoneApi,
  deleteProjectSprintApi,
  deleteProjectMilestoneApi,
  updateProjectSprintApi,
  updateProjectMilestoneApi,
  updateProjectTaskWbsApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectPlanOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
  switchProjectDetailTab: (tabKey: string) => void;
  toDateInputValue: (value?: null | string) => string | undefined;
};

type MilestoneCloseAdviceAction = {
  label: string;
  tabKey: string;
};

type MilestoneCloseAdvice = {
  actions: MilestoneCloseAdviceAction[];
  missingItems: string[];
  message: string;
  milestoneName: string;
};

type MilestoneCloseCondition = {
  count: number;
  done: boolean;
  key: string;
  label: string;
  targetTabKey: string;
};

type MilestoneCloseState = {
  acceptanceCriteria: ProjectApi.AcceptanceCriteriaItem[];
  acceptanceRecordCount: number;
  conditions: MilestoneCloseCondition[];
  confirmedAcceptanceCriteriaCount: number;
  sprintName: string;
};

export function useProjectPlan(options: UseProjectPlanOptions) {
  const {
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    switchProjectDetailTab,
    toDateInputValue,
  } = options;

  const savingSprint = ref(false);
  const deletingSprintId = ref('');
  const savingMilestone = ref(false);
  const deletingMilestoneId = ref('');
  const editingSprintId = ref<string | null>(null);
  const editingMilestoneId = ref<string | null>(null);
  const savingTaskWbsId = ref('');
  const milestoneCloseAdvice = ref<MilestoneCloseAdvice | null>(null);
  const milestoneStatusForms = ref<
    Record<string, ProjectApi.ProjectMilestoneStatusUpdateInput>
  >({});
  const taskWbsForms = ref<Record<string, ProjectApi.TaskWbsUpdateInput>>({});
  const showSprintForm = ref(false);
  const showMilestoneForm = ref(false);

  const emptySprintForm = (): ProjectApi.SprintSaveInput => ({
    endDate: undefined,
    name: '',
    ownerUserId: '',
    progress: 0,
    startDate: undefined,
    status: '规划中',
    summary: '',
  });
  const sprintForm = ref<ProjectApi.SprintSaveInput>(emptySprintForm());

  const emptyMilestoneForm = (): ProjectApi.ProjectMilestoneSaveInput => ({
    actualCompletionDate: undefined,
    sprintId: undefined,
    name: '',
    ownerUserId: '',
    plannedCompletionDate: toDateInputValue(new Date().toISOString()) ?? '',
    status: '未开始',
  });
  const milestoneForm = ref<ProjectApi.ProjectMilestoneSaveInput>(
    emptyMilestoneForm(),
  );

  const sprintColumns = [
    { field: 'sprintCode', slots: { default: 'sprintCode' }, title: '排期编号', width: 130 },
    { align: 'left' as const, field: 'name', slots: { default: 'name' }, title: '排期名称', width: 220 },
    { field: 'ownerName', slots: { default: 'ownerName' }, title: '负责人', width: 120 },
    { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
    { field: 'progress', slots: { default: 'progress' }, title: '进度', width: 180 },
    { field: 'tasks', slots: { default: 'tasks' }, title: '需求', width: 120 },
    { field: 'overdueTaskCount', slots: { default: 'overdueTaskCount' }, title: '逾期需求', width: 100 },
    { field: 'riskCount', slots: { default: 'riskCount' }, title: '风险', width: 90 },
    { field: 'openIssueCount', slots: { default: 'openIssueCount' }, title: '问题', width: 90 },
    { field: 'reportCount', slots: { default: 'reportCount' }, title: '报告', width: 120 },
    { field: 'startDate', slots: { default: 'startDate' }, title: '开始日期', width: 140 },
    { field: 'endDate', slots: { default: 'endDate' }, title: '结束日期', width: 140 },
    { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 150 },
  ];
  const milestoneColumns = [
    { slots: { content: 'wbs' }, type: 'expand' as const, width: 48 },
    { align: 'left' as const, field: 'name', slots: { default: 'name' }, title: '里程碑', width: 220 },
    { field: 'ownerName', slots: { default: 'ownerName' }, title: '负责人', width: 120 },
    { field: 'sprintName', slots: { default: 'sprintName' }, title: '所属排期', width: 180 },
    { field: 'plannedCompletionDate', slots: { default: 'plannedCompletionDate' }, title: '计划完成', width: 140 },
    { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
    { field: 'actualCompletionDate', slots: { default: 'actualCompletionDate' }, title: '实际完成', width: 140 },
    { align: 'left' as const, field: 'closeConditions', slots: { default: 'closeConditions' }, title: '验收/关闭条件', width: 260 },
    { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 180 },
  ];
  const sprintStatusOptions = ['规划中', '进行中', '已完成', '已关闭', '已取消'].map(
    (value) => ({ label: value, value }),
  );

  const milestoneSelectOptions = computed(() =>
    (detail.value?.milestones || []).map((item) => ({
      label: item.name,
      value: item.id,
    })),
  );

  const sprintOptions = computed(() =>
    (detail.value?.sprints || []).map((item) => ({
      label: `${formatSprintCode(item.sprintCode)} · ${formatSprintName(item.name)}`,
      value: item.id,
    })),
  );

  const sprintRows = computed(() => detail.value?.sprints || []);

  const wbsNodeOptions = computed(() =>
    (detail.value?.wbsNodes || []).map((item) => ({
      label: `${item.wbsCode} · ${item.name}`,
      value: item.id,
      wbsCode: item.wbsCode,
    })),
  );

  const taskWbsRows = computed(() =>
    (detail.value?.tasks || []).map((item) => {
      let form = taskWbsForms.value[item.id];
      if (!form) {
        form = buildTaskWbsForm(item);
        taskWbsForms.value[item.id] = form;
      }

      return { form, item };
    }),
  );

  const milestoneFormSprintName = computed(() =>
    resolveMilestoneSprintName({
      sprintId: milestoneForm.value.sprintId || null,
    }),
  );

  const editingMilestoneCloseState = computed(() => {
    if (!editingMilestoneId.value) {
      return null;
    }

    const milestone = detail.value?.milestones.find(
      (item) => item.id === editingMilestoneId.value,
    );
    return milestone ? buildMilestoneCloseState(milestone) : null;
  });

  const milestoneRows = computed(() =>
    (detail.value?.milestones || []).map((item) => {
      let form = milestoneStatusForms.value[item.id];
      if (!form) {
        form = {
          actualCompletionDate: toDateInputValue(item.actualCompletionDate),
          sprintId: item.sprintId || undefined,
          status: item.status,
        };
        milestoneStatusForms.value[item.id] = form;
      }

      return {
        closeState: buildMilestoneCloseState(item),
        form,
        id: item.id,
        item,
      };
    }),
  );

  const milestonePreview = computed(() =>
    (detail.value?.milestones || []).slice(0, 3),
  );
  const sprintPreview = computed(() =>
    (detail.value?.sprints || []).slice(0, 3),
  );

  function resetPlanForms(current = detail.value) {
    const firstSprintId = current?.sprints[0]?.id;
    editingSprintId.value = null;
    showSprintForm.value = false;
    sprintForm.value = {
      ...emptySprintForm(),
      ownerUserId: current?.members[0]?.userId || '',
    };
    editingMilestoneId.value = null;
    showMilestoneForm.value = false;
    milestoneForm.value = {
      ...emptyMilestoneForm(),
      sprintId: firstSprintId,
      ownerUserId: current?.members[0]?.userId || '',
    };
    milestoneStatusForms.value = {};
    taskWbsForms.value = Object.fromEntries(
      (current?.tasks || []).map((item) => [item.id, buildTaskWbsForm(item)]),
    );
  }

  function handleCreateSprintDraft() {
    editingSprintId.value = null;
    sprintForm.value = {
      ...emptySprintForm(),
      name: resolveNextSprintName(),
      ownerUserId: detail.value?.members[0]?.userId || '',
    };
    showSprintForm.value = true;
  }

  function handleEditSprint(item: ProjectApi.SprintItem) {
    editingSprintId.value = item.id;
    sprintForm.value = {
      endDate: toDateInputValue(item.endDate),
      name: item.name,
      ownerUserId: item.ownerUserId,
      progress: item.progress || 0,
      startDate: toDateInputValue(item.startDate),
      status: item.status || '规划中',
      summary: item.summary || '',
    };
    showSprintForm.value = true;
  }

  function handleCancelSprintEdit() {
    editingSprintId.value = null;
    showSprintForm.value = false;
    sprintForm.value = {
      ...emptySprintForm(),
      ownerUserId: detail.value?.members[0]?.userId || '',
    };
  }

  function resolveNextSprintName() {
    const count = detail.value?.sprints.length || 0;
    return `第 ${count + 1} 排期`;
  }

  async function handleSaveSprint() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!sprintForm.value.name.trim()) {
      message.error('请填写排期名称');
      return;
    }

    if (!sprintForm.value.ownerUserId) {
      message.error('请选择负责人');
      return;
    }

    savingSprint.value = true;
    try {
      const payload = normalizeSprintForm();
      if (editingSprintId.value) {
        await updateProjectSprintApi(editingSprintId.value, payload);
        message.success('排期已更新');
      } else {
        await createProjectSprintApi(selectedProjectId.value, payload);
        message.success('排期已创建');
      }

      handleCancelSprintEdit();
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(
        error,
        editingSprintId.value
          ? '排期更新失败'
          : '排期创建失败',
      );
    } finally {
      savingSprint.value = false;
    }
  }

  async function handleDeleteSprint(sprintId: string) {
    deletingSprintId.value = sprintId;
    try {
      await deleteProjectSprintApi(sprintId);
      message.success('排期已删除');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '排期删除失败');
    } finally {
      deletingSprintId.value = '';
    }
  }

  function handleCreateMilestoneDraft() {
    editingMilestoneId.value = null;
    milestoneForm.value = {
      ...emptyMilestoneForm(),
      sprintId: detail.value?.sprints[0]?.id,
      ownerUserId: detail.value?.members[0]?.userId || '',
    };
    showMilestoneForm.value = true;
  }

  function handleEditMilestone(item: ProjectApi.WorkspaceMilestoneItem) {
    editingMilestoneId.value = item.id;
    milestoneForm.value = {
      actualCompletionDate: toDateInputValue(item.actualCompletionDate),
      sprintId: item.sprintId || undefined,
      name: item.name,
      ownerUserId: item.ownerUserId,
      plannedCompletionDate: toDateInputValue(item.plannedCompletionDate) ?? '',
      status: item.status || '未开始',
    };
    showMilestoneForm.value = true;
  }

  function handleCancelMilestoneEdit() {
    editingMilestoneId.value = null;
    showMilestoneForm.value = false;
    milestoneForm.value = {
      ...emptyMilestoneForm(),
      sprintId: detail.value?.sprints[0]?.id,
      ownerUserId: detail.value?.members[0]?.userId || '',
    };
  }

  async function handleSaveMilestone() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!milestoneForm.value.name.trim()) {
      message.error('请填写里程碑名称');
      return;
    }

    if (!milestoneForm.value.ownerUserId) {
      message.error('请选择负责人');
      return;
    }

    if (!milestoneForm.value.plannedCompletionDate) {
      message.error('请选择计划完成日期');
      return;
    }

    savingMilestone.value = true;
    try {
      const payload = normalizeMilestoneForm();
      if (editingMilestoneId.value) {
        await updateProjectMilestoneApi(editingMilestoneId.value, payload);
        message.success('里程碑已更新');
      } else {
        await createProjectMilestoneApi(selectedProjectId.value, payload);
        message.success('里程碑已创建');
      }

      handleCancelMilestoneEdit();
      await refreshSelectedProjectDetail();
    } catch (error) {
      if (
        editingMilestoneId.value &&
        milestoneForm.value.status === '已关闭' &&
        showMilestoneCloseAdvice(error, editingMilestoneId.value)
      ) {
        return;
      }

      showRequestError(
        error,
        editingMilestoneId.value ? '里程碑更新失败' : '里程碑创建失败',
      );
    } finally {
      savingMilestone.value = false;
    }
  }

  async function handleDeleteMilestone(milestoneId: string) {
    deletingMilestoneId.value = milestoneId;
    try {
      await deleteProjectMilestoneApi(milestoneId);
      message.success('里程碑已删除');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '里程碑删除失败');
    } finally {
      deletingMilestoneId.value = '';
    }
  }

  async function handleUpdateTaskWbs(taskId: string) {
    const form = taskWbsForms.value[taskId];
    if (!form) {
      return;
    }

    savingTaskWbsId.value = taskId;
    try {
      const selectedWbsNode = (detail.value?.wbsNodes || []).find(
        (item) => item.id === form.wbsNodeId,
      );
      await updateProjectTaskWbsApi(taskId, {
        ...form,
        parentTaskId: form.parentTaskId || undefined,
        requirementId: form.requirementId || undefined,
        wbsCode: selectedWbsNode?.wbsCode || form.wbsCode || '',
        wbsNodeId: form.wbsNodeId || undefined,
      });
      message.success('WBS 信息已更新');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, 'WBS 信息更新失败');
    } finally {
      savingTaskWbsId.value = '';
    }
  }

  function normalizeSprintForm(): ProjectApi.SprintSaveInput {
    const current = sprintForm.value;
    return {
      endDate: current.endDate || undefined,
      name: current.name.trim(),
      ownerUserId: current.ownerUserId,
      progress: current.progress ?? 0,
      startDate: current.startDate || undefined,
      status: current.status || '规划中',
      summary: current.summary?.trim() || '',
    };
  }

  function normalizeMilestoneForm(): ProjectApi.ProjectMilestoneSaveInput {
    const current = milestoneForm.value;
    return {
      actualCompletionDate: current.actualCompletionDate || undefined,
      sprintId: current.sprintId || undefined,
      name: current.name.trim(),
      ownerUserId: current.ownerUserId,
      plannedCompletionDate: current.plannedCompletionDate,
      status: current.status || '未开始',
    };
  }

  function buildTaskWbsForm(item: ProjectApi.TaskItem): ProjectApi.TaskWbsUpdateInput {
    return {
      baselineEndTime: toDateInputValue(item.baselineEndTime),
      baselineStartTime: toDateInputValue(item.baselineStartTime),
      sprintId: item.sprintId || undefined,
      parentTaskId: item.parentTaskId || undefined,
      requirementId: item.requirementId || undefined,
      sortOrder: item.sortOrder || 0,
      wbsCode: item.wbsCode || '',
      wbsNodeId: item.wbsNodeId || undefined,
    };
  }

  function buildMilestoneCloseState(
    item: Pick<ProjectApi.WorkspaceMilestoneItem, 'sprintId' | 'id'>,
  ): MilestoneCloseState {
    const acceptanceCriteria = (detail.value?.acceptanceCriteria || []).filter(
      (criteria) => criteria.milestoneId === item.id,
    );
    const confirmedAcceptanceCriteriaCount = acceptanceCriteria.filter((criteria) =>
      ['不适用', '已确认', '已验收'].includes(criteria.status),
    ).length;
    const acceptanceRecordCount = (detail.value?.acceptanceRecords || []).filter(
      (record) => record.milestoneId === item.id,
    ).length;
    const qualityInspectionCount = detail.value?.qualityInspections.length || 0;
    const acceptanceEvidenceFileCount = (detail.value?.documents || []).filter((file) =>
      ['Project/AcceptanceRecord', 'Project/Delivery'].includes(file.category),
    ).length;

    return {
      acceptanceCriteria,
      acceptanceRecordCount,
      conditions: [
        {
          count: confirmedAcceptanceCriteriaCount,
          done: confirmedAcceptanceCriteriaCount > 0,
          key: 'acceptanceCriteria',
          label: '验收标准',
          targetTabKey: 'delivery',
        },
        {
          count: qualityInspectionCount,
          done: qualityInspectionCount > 0,
          key: 'qualityInspection',
          label: '质量检查',
          targetTabKey: 'delivery',
        },
        {
          count: acceptanceEvidenceFileCount,
          done: acceptanceEvidenceFileCount > 0,
          key: 'acceptanceEvidence',
          label: '验收/交付文件',
          targetTabKey: 'docs',
        },
      ],
      confirmedAcceptanceCriteriaCount,
      sprintName: resolveMilestoneSprintName(item),
    };
  }

  function resolveMilestoneSprintName(item: Pick<ProjectApi.WorkspaceMilestoneItem, 'sprintId'>) {
    const sprintId = item.sprintId || undefined;
    if (!sprintId) {
      return '未关联排期';
    }

    const sprint = (detail.value?.sprints || []).find((current) => current.id === sprintId);
    return sprint
      ? `${formatSprintCode(sprint.sprintCode)} · ${formatSprintName(sprint.name)}`
      : '未关联排期';
  }

  function formatSprintCode(code?: null | string) {
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

  function clearMilestoneCloseAdvice() {
    milestoneCloseAdvice.value = null;
  }

  function extractErrorMessage(error: unknown) {
    return error instanceof Error ? error.message : '';
  }

  function showMilestoneCloseAdvice(error: unknown, milestoneId: string) {
    const description = extractErrorMessage(error);
    const missingItems: string[] = [];
    const actions: MilestoneCloseAdviceAction[] = [];

    if (description.includes('验收标准')) {
      missingItems.push('维护并确认关联验收标准');
      actions.push({ label: '去验收与交付', tabKey: 'delivery' });
    }

    if (description.includes('质量检查')) {
      missingItems.push('完成至少一次质量检查');
      actions.push({ label: '去验收与交付', tabKey: 'delivery' });
    }

    if (description.includes('验收记录') || description.includes('交付物')) {
      missingItems.push('归档验收记录文件或交付物文件');
      actions.push({ label: '去文档与知识', tabKey: 'docs' });
      actions.push({ label: '去验收与交付', tabKey: 'delivery' });
    }

    if (missingItems.length === 0) {
      return false;
    }

    const milestoneName =
      detail.value?.milestones.find((item) => item.id === milestoneId)?.name ||
      '当前里程碑';
    const uniqueActions = actions.filter(
      (action, index, list) =>
        list.findIndex((item) => item.tabKey === action.tabKey) === index,
    );
    milestoneCloseAdvice.value = {
      actions: uniqueActions,
      message: description || '里程碑关闭条件未满足。',
      milestoneName,
      missingItems,
    };
    return true;
  }

  function handleMilestoneCloseAdviceAction(tabKey: string) {
    switchProjectDetailTab(tabKey);
  }

  return {
    clearMilestoneCloseAdvice,
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
    resetPlanForms,
    savingSprint,
    sprintColumns,
    sprintRows,
    savingMilestone,
    savingTaskWbsId,
    showSprintForm,
    showMilestoneForm,
    taskWbsRows,
    wbsNodeOptions,
  };
}
