import { computed, ref, type ComputedRef, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  approveProjectChangeApi,
  createProjectBaselineApi,
  createProjectChangeRequestApi,
  createProjectPerformanceSnapshotApi,
  rejectProjectChangeApi,
  submitProjectChangeApprovalApi,
  withdrawProjectChangeApprovalApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectControlOptions = {
  changeSeverityOptions: ComputedRef<Array<{ label: string; value: string }>>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  formatDate: (value?: null | string) => string;
  formatMoney: (value?: null | number) => string;
  formatRatio: (value?: null | number) => string;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
  toDateInputValue: (value?: null | string) => string | undefined;
};

export function useProjectControl(options: UseProjectControlOptions) {
  const {
    changeSeverityOptions,
    detail,
    formatDate,
    formatMoney,
    formatRatio,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    toDateInputValue,
  } = options;

  const savingBaseline = ref(false);
  const savingChangeRequest = ref(false);
  const savingChangeWorkflowId = ref('');
  const savingPerformanceSnapshot = ref(false);
  const showBaselineForm = ref(false);
  const showChangeRequestForm = ref(false);
  const showPerformanceSnapshotForm = ref(false);
  const changeApprovalComment = ref('');

  const baselineForm = ref<ProjectApi.ProjectBaselineSaveInput>({
    budgetAtCompletion: 0,
    costBaselineSummary: '',
    createdFromChangeRequestId: undefined,
    effectiveDate: undefined,
    isActive: true,
    scheduleBaselineSummary: '',
    scopeBaselineSummary: '',
  });

  const changeRequestForm = ref<ProjectApi.ProjectChangeRequestSaveInput>({
    applicantUserId: undefined,
    changeType: '范围',
    expectedResult: '',
    impactScope: '',
    impacts: [
      {
        analysis: '',
        estimatedCostDelta: undefined,
        estimatedScheduleDeltaDays: undefined,
        impactType: '范围',
        relatedBusinessCode: '',
        relatedBudgetCode: '',
        relatedContractCode: '',
        relatedMilestoneId: undefined,
        relatedTaskId: undefined,
        severity: 'P1',
      },
    ],
    reason: '',
    requestCode: '',
    title: '',
  });

  const performanceSnapshotForm =
    ref<ProjectApi.ProjectPerformanceSnapshotSaveInput>({
      actualCost: undefined,
      earnedValue: undefined,
      plannedValue: undefined,
      projectBaselineId: undefined,
      snapshotDate: undefined,
      snapshotPeriod: 'Weekly',
    });

  const changeImpactTypeOptions = ['范围', '进度', '成本'].map((value) => ({
    label: value,
    value,
  }));

  const changeTypeOptions = ['范围', '进度', '成本'].map((value) => ({
    label: value,
    value,
  }));

  const baselineChangeOptions = computed(() =>
    (detail.value?.changeRequests || [])
      .filter((item) => item.approvalWorkflow?.status === '已通过')
      .map((item) => ({
        label: `${item.requestCode || item.title} · ${item.changeType}`,
        value: item.id,
      })),
  );

  const activeBaselineSummary = computed(() => {
    const baseline = detail.value?.activeBaseline;
    if (!baseline) {
      return [
        { label: '基线版本', value: '-' },
        { label: 'BAC', value: formatMoney(detail.value?.budgetTotalAmount) },
        { label: '生效日期', value: '-' },
      ];
    }

    return [
      { label: '基线版本', value: `V${baseline.versionNo}` },
      { label: 'BAC', value: formatMoney(baseline.budgetAtCompletion) },
      { label: '生效日期', value: formatDate(baseline.effectiveDate) },
    ];
  });

  const changeControlMetrics = computed(() => {
    const changes = detail.value?.changeRequests || [];
    const pendingStatuses = ['待评审', '审批中'];
    return [
      { label: '基线变更', value: `${changes.length}` },
      {
        label: '待评审',
        value: `${changes.filter((item) => pendingStatuses.includes(item.status)).length}`,
      },
      {
        label: '已批准',
        value: `${changes.filter((item) => item.status === '已批准').length}`,
      },
      {
        label: 'P0影响',
        value: `${changes.filter((item) => item.impacts.some((impact) => impact.severity === 'P0')).length}`,
      },
    ];
  });

  const evmMetrics = computed(() => {
    const snapshot = detail.value?.performanceSnapshot;
    return [
      { label: 'PV 计划价值', value: formatMoney(snapshot?.plannedValue) },
      { label: 'EV 挣值', value: formatMoney(snapshot?.earnedValue) },
      { label: 'AC 实际成本', value: formatMoney(snapshot?.actualCost) },
      { label: 'SPI', value: formatRatio(snapshot?.schedulePerformanceIndex) },
      { label: 'CPI', value: formatRatio(snapshot?.costPerformanceIndex) },
    ];
  });

  const performanceTrendRows = computed(() =>
    (detail.value?.performanceSnapshots || []).slice(0, 8),
  );

  function resetControlForms(current = detail.value) {
    baselineForm.value = {
      budgetAtCompletion:
        current?.activeBaseline?.budgetAtCompletion ||
        current?.budgetTotalAmount ||
        0,
      costBaselineSummary: current?.activeBaseline?.costBaselineSummary || '',
      createdFromChangeRequestId: undefined,
      effectiveDate: toDateInputValue(current?.activeBaseline?.effectiveDate),
      isActive: true,
      scheduleBaselineSummary:
        current?.activeBaseline?.scheduleBaselineSummary || '',
      scopeBaselineSummary: current?.activeBaseline?.scopeBaselineSummary || '',
    };
    changeRequestForm.value = {
      applicantUserId: current?.members[0]?.userId,
      changeType: '范围',
      expectedResult: '',
      impactScope: '',
      impacts: [
        {
          analysis: '',
          estimatedCostDelta: undefined,
          estimatedScheduleDeltaDays: undefined,
          impactType: '范围',
          relatedBusinessCode: '',
          relatedBudgetCode: '',
          relatedContractCode: '',
          relatedMilestoneId: undefined,
          relatedTaskId: undefined,
          severity: 'P1',
        },
      ],
      reason: '',
      requestCode: '',
      title: '',
    };
    changeApprovalComment.value = '';
    performanceSnapshotForm.value = {
      actualCost: undefined,
      earnedValue: undefined,
      plannedValue: undefined,
      projectBaselineId: current?.activeBaseline?.id,
      snapshotDate: toDateInputValue(new Date().toISOString()),
      snapshotPeriod: 'Weekly',
    };
  }

  function canApproveBaselineChange(changeRequest: ProjectApi.ProjectChangeRequest) {
    return changeRequest.approvalWorkflow?.status === '审批中';
  }

  function canSubmitBaselineChange(changeRequest: ProjectApi.ProjectChangeRequest) {
    return !changeRequest.approvalWorkflow || changeRequest.approvalWorkflow.status === '已撤回';
  }

  function getBaselineChangeWorkflowStatus(changeRequest: ProjectApi.ProjectChangeRequest) {
    return changeRequest.approvalWorkflow?.status || changeRequest.status || '待评审';
  }

  async function handleCreateBaseline() {
    if (!selectedProjectId.value) {
      return;
    }

    if (
      !baselineForm.value.scopeBaselineSummary ||
      !baselineForm.value.scheduleBaselineSummary
    ) {
      message.error('请填写范围基线和进度基线摘要');
      return;
    }

    savingBaseline.value = true;
    try {
      await createProjectBaselineApi(selectedProjectId.value, {
        ...baselineForm.value,
        createdFromChangeRequestId:
          baselineForm.value.createdFromChangeRequestId || undefined,
        effectiveDate: baselineForm.value.effectiveDate || undefined,
      });
      message.success('项目基线已创建');
      showBaselineForm.value = false;
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目基线创建失败');
    } finally {
      savingBaseline.value = false;
    }
  }

  function handleAddChangeImpact() {
    changeRequestForm.value.impacts.push({
      analysis: '',
      estimatedCostDelta: undefined,
      estimatedScheduleDeltaDays: undefined,
      impactType: '范围',
      relatedBusinessCode: '',
      relatedBudgetCode: '',
      relatedContractCode: '',
      relatedMilestoneId: undefined,
      relatedTaskId: undefined,
      severity: 'P1',
    });
  }

  function handleRemoveChangeImpact(index: number) {
    if (changeRequestForm.value.impacts.length <= 1) {
      message.error('基线变更至少需要一条影响分析');
      return;
    }

    changeRequestForm.value.impacts.splice(index, 1);
  }

  async function handleCreateChangeRequest() {
    if (!selectedProjectId.value) {
      return;
    }

    if (
      !changeRequestForm.value.title ||
      !changeRequestForm.value.reason ||
      !changeRequestForm.value.impactScope ||
      !changeRequestForm.value.expectedResult
    ) {
      message.error('请填写基线变更标题、原因、影响范围和预期结果');
      return;
    }

    if (
      changeRequestForm.value.impacts.some(
        (impact) => !impact.impactType || !impact.analysis,
      )
    ) {
      message.error('每条影响分析都需要填写影响类型和分析说明');
      return;
    }

    savingChangeRequest.value = true;
    try {
      const changeRequestId = await createProjectChangeRequestApi(selectedProjectId.value, {
        ...changeRequestForm.value,
        applicantUserId: changeRequestForm.value.applicantUserId || undefined,
        impacts: changeRequestForm.value.impacts.map((impact) => ({
          ...impact,
          estimatedCostDelta: impact.estimatedCostDelta ?? undefined,
          estimatedScheduleDeltaDays:
            impact.estimatedScheduleDeltaDays ?? undefined,
          relatedBudgetCode: impact.relatedBudgetCode || '',
          relatedBusinessCode: impact.relatedBusinessCode || '',
          relatedContractCode: impact.relatedContractCode || '',
          relatedMilestoneId: impact.relatedMilestoneId || undefined,
          relatedTaskId: impact.relatedTaskId || undefined,
        })),
      });
      await submitProjectChangeApprovalApi(changeRequestId, {
        comment: changeApprovalComment.value.trim() || '提交基线变更审批',
      });
      message.success('基线变更已创建并提交审批');
      showChangeRequestForm.value = false;
      changeApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '基线变更创建失败');
    } finally {
      savingChangeRequest.value = false;
    }
  }

  async function handleCreateChangeApproval(changeRequestId: string) {
    const changeRequest = detail.value?.changeRequests?.find((item) => item.id === changeRequestId);
    if (!changeRequest) {
      return;
    }

    savingChangeWorkflowId.value = changeRequestId;
    try {
      if (canApproveBaselineChange(changeRequest)) {
        await approveProjectChangeApi(changeRequestId, {
          comment: changeApprovalComment.value.trim() || '同意基线变更',
        });
        message.success('基线变更审批已通过');
      } else if (changeRequest.approvalWorkflow?.status === '已通过') {
        message.success('该基线变更已通过审批');
      } else {
        await submitProjectChangeApprovalApi(changeRequestId, {
          comment: changeApprovalComment.value.trim() || '提交基线变更审批',
        });
        message.success('基线变更审批已提交');
      }
      changeApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '基线变更审批处理失败');
    } finally {
      savingChangeWorkflowId.value = '';
    }
  }

  async function handleRejectChangeApproval(changeRequestId: string) {
    savingChangeWorkflowId.value = changeRequestId;
    try {
      await rejectProjectChangeApi(changeRequestId, {
        comment: changeApprovalComment.value.trim() || '驳回基线变更',
      });
      message.success('基线变更审批已驳回');
      changeApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '基线变更审批驳回失败');
    } finally {
      savingChangeWorkflowId.value = '';
    }
  }

  async function handleWithdrawChangeApproval(changeRequestId: string) {
    savingChangeWorkflowId.value = changeRequestId;
    try {
      await withdrawProjectChangeApprovalApi(changeRequestId, {
        comment: changeApprovalComment.value.trim() || '撤回基线变更审批',
      });
      message.success('基线变更审批已撤回');
      changeApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '基线变更审批撤回失败');
    } finally {
      savingChangeWorkflowId.value = '';
    }
  }

  async function handleCreatePerformanceSnapshot() {
    if (!selectedProjectId.value) {
      return;
    }

    savingPerformanceSnapshot.value = true;
    try {
      await createProjectPerformanceSnapshotApi(selectedProjectId.value, {
        ...performanceSnapshotForm.value,
        actualCost: performanceSnapshotForm.value.actualCost ?? undefined,
        earnedValue: performanceSnapshotForm.value.earnedValue ?? undefined,
        plannedValue: performanceSnapshotForm.value.plannedValue ?? undefined,
        projectBaselineId:
          performanceSnapshotForm.value.projectBaselineId || undefined,
        snapshotDate: performanceSnapshotForm.value.snapshotDate || undefined,
      });
      message.success('绩效快照已生成');
      showPerformanceSnapshotForm.value = false;
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '绩效快照生成失败');
    } finally {
      savingPerformanceSnapshot.value = false;
    }
  }

  return {
    activeBaselineSummary,
    baselineChangeOptions,
    baselineForm,
    canApproveBaselineChange,
    canSubmitBaselineChange,
    changeApprovalComment,
    changeControlMetrics,
    changeImpactTypeOptions,
    changeRequestForm,
    changeSeverityOptions,
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
    resetControlForms,
    savingBaseline,
    savingChangeRequest,
    savingChangeWorkflowId,
    savingPerformanceSnapshot,
    showBaselineForm,
    showChangeRequestForm,
    showPerformanceSnapshotForm,
  };
}
