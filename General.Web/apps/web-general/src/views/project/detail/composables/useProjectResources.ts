import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  getProjectMyRelatedApi,
  recordProjectTaskProgressApi,
  updateProjectMemberRolesApi,
  type ProjectApi,
} from '#/api/core';

const DEFAULT_TASK_STATUS = '进行中';

type UseProjectResourcesOptions = {
  currentUserId: Ref<string>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  showRequestError: (error: unknown, fallback: string) => void;
};

export function useProjectResources(options: UseProjectResourcesOptions) {
  const { currentUserId, detail, refreshSelectedProjectDetail, showRequestError } = options;

  const savingMemberRoleId = ref('');
  const editingMemberId = ref<string | null>(null);
  const memberRoleForms = ref<Record<string, string[]>>({});
  const worklogDrawerOpen = ref(false);
  const loadingMyWorklogTasks = ref(false);
  const savingWorklogEntry = ref(false);
  const savingWorklogBatch = ref(false);
  const myWorklogTasks = ref<ProjectApi.TaskItem[]>([]);
  const savingWorklogTaskId = ref('');
  const worklogBatchDate = ref(toDateKey(new Date()));
  const worklogBatchSummary = ref('');
  const worklogEntryForms = ref<Record<string, ProjectApi.TaskProgressInput>>({});

  const memberResourceRows = computed(() => {
    const current = detail.value;
    if (!current) {
      return [];
    }

    return current.members.map((member) => {
      const worklogHours = current.worklogs
        .filter((item) => item.userId === member.userId || item.userName === member.userName)
        .reduce((sum, item) => sum + item.hours, 0);
      const taskCount = current.tasks.filter((task) =>
        resolveTaskParticipantUserIds(task).includes(member.userId),
      ).length;
      const estimatedWorkHours = current.tasks
        .filter((task) => isTaskAssignedToUser(task, member.userId))
        .reduce((sum, task) => sum + (task.estimatedWorkHours || 0), 0);

      return {
        ...member,
        loadHint:
          taskCount === 0 && worklogHours === 0
            ? '暂无记录'
            : worklogHours >= 40
              ? '投入较集中'
              : '有投入记录',
        taskCount,
        estimatedWorkHours: Number(estimatedWorkHours.toFixed(1)),
        worklogHours: Number(worklogHours.toFixed(1)),
      };
    });
  });

  const currentProjectWorklogTasks = computed(() => {
    const userId = currentUserId.value;
    if (!userId) {
      return [];
    }

    return (detail.value?.tasks || []).filter((task) => isTaskAssignedToUser(task, userId));
  });

  const worklogDemandRows = computed(() => {
    const rows = new Map<string, ProjectApi.TaskItem>();
    for (const task of [...myWorklogTasks.value, ...currentProjectWorklogTasks.value]) {
      if (!['已完成', '已关闭', '已解除'].includes(task.status)) {
        rows.set(task.id, task);
      }
    }

    return [...rows.values()].sort(
      (left, right) =>
        (left.projectName || '').localeCompare(right.projectName || '', 'zh-CN') ||
        (left.plannedEndTime || '9999-12-31').localeCompare(right.plannedEndTime || '9999-12-31') ||
        left.taskCode.localeCompare(right.taskCode, 'zh-CN', { numeric: true }),
    );
  });

  const worklogDrawerSummary = computed(() => {
    const rows = worklogDemandRows.value;
    const estimatedHours = rows.reduce((sum, task) => sum + (task.estimatedWorkHours || 0), 0);
    const actualHours = rows.reduce((sum, task) => sum + (task.actualWorkHours || 0), 0);
    return {
      actualHours: Number(actualHours.toFixed(1)),
      estimatedHours: Number(estimatedHours.toFixed(1)),
      taskCount: rows.length,
    };
  });

  const filledWorklogEntryCount = computed(
    () => Object.values(worklogEntryForms.value).filter((form) => form.summary.trim()).length,
  );

  const projectRoleOptions = [
    'PM',
    'PMO',
    '产品',
    '开发',
    '测试',
    '实施',
    '运维',
    '商务',
    '财务',
    '干系人',
  ].map((value) => ({
    label: value,
    value,
  }));

  function resetResourceForms(current = detail.value) {
    memberRoleForms.value = Object.fromEntries(
      (current?.members || []).map((item) => [item.id, [...item.roleNames]]),
    );
    editingMemberId.value = null;
    worklogEntryForms.value = {};
  }

  async function handleOpenWorklogDrawer() {
    worklogBatchDate.value = toDateKey(new Date());
    worklogDrawerOpen.value = true;
    await loadMyWorklogTasks();
    syncWorklogEntryForms();
  }

  function handleCloseWorklogDrawer() {
    worklogDrawerOpen.value = false;
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
    } catch (error) {
      showRequestError(error, '成员角色更新失败');
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

  async function handleSaveWorklogEntry(taskId: string) {
    const form = worklogEntryForms.value[taskId];
    if (!form) {
      message.error('请先选择要填写进展的需求');
      return;
    }

    const payload = buildWorklogPayload(form);
    if (!payload) {
      return;
    }

    savingWorklogEntry.value = true;
    savingWorklogTaskId.value = taskId;
    try {
      await recordProjectTaskProgressApi(taskId, payload);
      message.success('工时与进展已记录');
      resetWorklogEntryForm(taskId, form.status || DEFAULT_TASK_STATUS);
      await Promise.all([refreshSelectedProjectDetail(), loadMyWorklogTasks()]);
      syncWorklogEntryForms();
    } catch (error) {
      showRequestError(error, '工时记录失败');
    } finally {
      savingWorklogEntry.value = false;
      savingWorklogTaskId.value = '';
    }
  }

  async function handleSaveFilledWorklogEntries() {
    const entries = Object.entries(worklogEntryForms.value).filter(([, form]) => form.summary.trim());
    if (entries.length === 0) {
      message.error('请至少填写一条工作进展');
      return;
    }

    const payloads: Array<[string, ProjectApi.TaskProgressInput, ProjectApi.TaskProgressInput]> = [];
    for (const [taskId, form] of entries) {
      const payload = buildWorklogPayload(form);
      if (!payload) {
        return;
      }
      payloads.push([taskId, payload, form]);
    }

    savingWorklogBatch.value = true;
    try {
      for (const [taskId, payload, form] of payloads) {
        savingWorklogTaskId.value = taskId;
        await recordProjectTaskProgressApi(taskId, payload);
        resetWorklogEntryForm(taskId, form.status || DEFAULT_TASK_STATUS);
      }
      message.success(`已保存 ${payloads.length} 条工时与进展`);
      await Promise.all([refreshSelectedProjectDetail(), loadMyWorklogTasks()]);
      syncWorklogEntryForms();
    } catch (error) {
      showRequestError(error, '批量保存工时失败');
    } finally {
      savingWorklogBatch.value = false;
      savingWorklogTaskId.value = '';
    }
  }

  function handleApplyWorklogDateToRows() {
    if (!worklogBatchDate.value) {
      message.error('请选择工作日期');
      return;
    }

    worklogEntryForms.value = Object.fromEntries(
      Object.entries(worklogEntryForms.value).map(([taskId, form]) => [
        taskId,
        {
          ...form,
          workDate: worklogBatchDate.value,
        },
      ]),
    );
    message.success('工作日期已应用到全部需求');
  }

  function handleFillEmptyWorklogSummaries() {
    const summary = worklogBatchSummary.value.trim();
    if (!summary) {
      message.error('请先填写通用工作进展');
      return;
    }

    let changedCount = 0;
    worklogEntryForms.value = Object.fromEntries(
      Object.entries(worklogEntryForms.value).map(([taskId, form]) => {
        if (form.summary.trim()) {
          return [taskId, form];
        }

        changedCount += 1;
        return [
          taskId,
          {
            ...form,
            summary,
          },
        ];
      }),
    );

    if (changedCount === 0) {
      message.info('当前没有空白进展需要填充');
      return;
    }

    message.success(`已填入 ${changedCount} 条空白进展`);
  }

  async function loadMyWorklogTasks() {
    loadingMyWorklogTasks.value = true;
    try {
      const payload = await getProjectMyRelatedApi();
      myWorklogTasks.value = payload.myTasks || [];
    } catch (error) {
      showRequestError(error, '我的需求加载失败');
      myWorklogTasks.value = [];
    } finally {
      loadingMyWorklogTasks.value = false;
    }
  }

  function syncWorklogEntryForms() {
    const current = { ...worklogEntryForms.value };
    for (const task of worklogDemandRows.value) {
      if (!current[task.id]) {
        current[task.id] = {
          ...emptyWorklogEntryForm(),
          status: task.status && task.status !== '未开始' ? task.status : DEFAULT_TASK_STATUS,
        };
      }
    }
    worklogEntryForms.value = current;
  }

  function buildWorklogPayload(form: ProjectApi.TaskProgressInput) {
    if (!form.hours || form.hours <= 0) {
      message.error('请填写大于 0 的本次工时');
      return null;
    }

    if (form.hours > 24) {
      message.error('本次工时不能超过 24 小时');
      return null;
    }

    if (!form.summary.trim()) {
      message.error('请填写本次工作进展');
      return null;
    }

    const isBlocked = form.isBlocked || form.status === '阻塞';
    if (isBlocked && !form.blockReason.trim()) {
      message.error('需求阻塞时请填写阻塞原因');
      return null;
    }

    return {
      blockReason: isBlocked ? form.blockReason.trim() : '',
      hours: form.hours,
      isBlocked,
      status: form.status || DEFAULT_TASK_STATUS,
      summary: form.summary.trim(),
      workDate: form.workDate || undefined,
    };
  }

  function resetWorklogEntryForm(taskId: string, status: string) {
    worklogEntryForms.value[taskId] = {
      ...emptyWorklogEntryForm(),
      status,
    };
  }

  function emptyWorklogEntryForm(): ProjectApi.TaskProgressInput {
    return {
      blockReason: '',
      hours: 1,
      isBlocked: false,
      status: DEFAULT_TASK_STATUS,
      summary: '',
      workDate: toDateKey(new Date()),
    };
  }

  function isTaskAssignedToUser(task: ProjectApi.TaskItem, userId: string) {
    return resolveTaskParticipantUserIds(task).includes(userId);
  }

  function resolveTaskParticipantUserIds(task: ProjectApi.TaskItem) {
    return Array.from(
      new Set(
        [
          task.ownerUserId,
          task.productOwnerUserId,
          task.developerOwnerUserId,
          task.testerOwnerUserId,
          ...(task.productOwnerUserIds || []),
          ...(task.developerOwnerUserIds || []),
          ...(task.testerOwnerUserIds || []),
        ].filter((value): value is string => Boolean(value)),
      ),
    );
  }

  function toDateKey(date: Date) {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  return {
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
    resetResourceForms,
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
  };
}
