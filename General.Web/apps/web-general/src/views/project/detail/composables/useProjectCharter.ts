import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  approveProjectCharterApi,
  rejectProjectCharterApi,
  saveProjectCharterApi,
  submitProjectCharterApprovalApi,
  withdrawProjectCharterApprovalApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectCharterOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
};

export function useProjectCharter(options: UseProjectCharterOptions) {
  const {
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
  } = options;

  const savingCharter = ref(false);
  const processingCharterApproval = ref('');
  const charterApprovalComment = ref('');
  const charterForm = ref<ProjectApi.CharterSaveInput>({
    approvalStatus: '草稿',
    assumptions: '',
    businessCase: '',
    constraints: '',
    objectives: '',
    scopeBoundary: '',
    successCriteria: '',
  });

  const charterApprovalWorkflow = computed(
    () => detail.value?.charter?.approvalWorkflow ?? null,
  );
  const charterApprovalRecords = computed(() =>
    [...(charterApprovalWorkflow.value?.approvalRecords ?? [])].sort(
      (left, right) =>
        new Date(right.creationTime).getTime() -
        new Date(left.creationTime).getTime(),
    ),
  );
  const charterApprovalStatus = computed(
    () =>
      detail.value?.charter?.approvalStatus ||
      charterForm.value.approvalStatus ||
      '草稿',
  );
  const canSubmitCharterApproval = computed(
    () =>
      Boolean(selectedProjectId.value) &&
      charterApprovalStatus.value !== '待审批' &&
      charterApprovalStatus.value !== '已批准',
  );
  const canWithdrawCharterApproval = computed(
    () =>
      Boolean(selectedProjectId.value) &&
      charterApprovalStatus.value === '待审批',
  );
  const canApproveCharterApproval = computed(
    () =>
      Boolean(selectedProjectId.value) &&
      charterApprovalStatus.value === '待审批',
  );

  function resetCharterForm(current: null | ProjectApi.ProjectDetail) {
    charterForm.value = {
      approvalStatus: current?.charter?.approvalStatus || '草稿',
      assumptions: current?.charter?.assumptions || '',
      businessCase: current?.charter?.businessCase || '',
      constraints: current?.charter?.constraints || '',
      objectives: current?.charter?.objectives || '',
      scopeBoundary: current?.charter?.scopeBoundary || '',
      successCriteria: current?.charter?.successCriteria || '',
    };
  }

  async function handleSaveCharter() {
    if (!selectedProjectId.value) {
      return;
    }

    savingCharter.value = true;
    try {
      const approvalStatus =
        detail.value?.charter?.approvalStatus === '已批准'
          ? '已批准'
          : charterForm.value.approvalStatus === '退回'
            ? '退回'
            : '草稿';
      await saveProjectCharterApi(selectedProjectId.value, {
        assumptions: normalizeCharterSummary(charterForm.value.assumptions),
        businessCase: normalizeCharterSummary(charterForm.value.businessCase),
        constraints: normalizeCharterSummary(charterForm.value.constraints),
        objectives: normalizeCharterSummary(charterForm.value.objectives),
        scopeBoundary: normalizeCharterSummary(charterForm.value.scopeBoundary),
        successCriteria: normalizeCharterSummary(charterForm.value.successCriteria),
        approvalStatus,
      });
      message.success('项目章程已保存');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目章程保存失败');
    } finally {
      savingCharter.value = false;
    }
  }

  async function handleSubmitCharterApproval() {
    if (!selectedProjectId.value) {
      return;
    }

    processingCharterApproval.value = 'submit';
    try {
      await submitProjectCharterApprovalApi(selectedProjectId.value, {
        comment: charterApprovalComment.value.trim() || '提交项目章程审批',
      });
      message.success('项目章程已提交审批');
      charterApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目章程提交审批失败');
    } finally {
      processingCharterApproval.value = '';
    }
  }

  async function handleApproveCharterApproval() {
    if (!selectedProjectId.value) {
      return;
    }

    processingCharterApproval.value = 'approve';
    try {
      await approveProjectCharterApi(selectedProjectId.value, {
        comment: charterApprovalComment.value.trim() || '同意项目章程',
      });
      message.success('项目章程已通过审批');
      charterApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目章程审批通过失败');
    } finally {
      processingCharterApproval.value = '';
    }
  }

  async function handleRejectCharterApproval() {
    if (!selectedProjectId.value) {
      return;
    }

    processingCharterApproval.value = 'reject';
    try {
      await rejectProjectCharterApi(selectedProjectId.value, {
        comment: charterApprovalComment.value.trim() || '退回项目章程',
      });
      message.success('项目章程已退回');
      charterApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目章程退回失败');
    } finally {
      processingCharterApproval.value = '';
    }
  }

  async function handleWithdrawCharterApproval() {
    if (!selectedProjectId.value) {
      return;
    }

    processingCharterApproval.value = 'withdraw';
    try {
      await withdrawProjectCharterApprovalApi(selectedProjectId.value, {
        comment: charterApprovalComment.value.trim() || '撤回项目章程审批',
      });
      message.success('项目章程审批已撤回');
      charterApprovalComment.value = '';
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目章程审批撤回失败');
    } finally {
      processingCharterApproval.value = '';
    }
  }

  function normalizeCharterSummary(value?: null | string) {
    return value?.trim() || '';
  }

  return {
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
    resetCharterForm,
    savingCharter,
  };
}
