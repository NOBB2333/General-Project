import { computed, ref, type ComputedRef, type Ref } from 'vue';

import type { UploadProps } from 'ant-design-vue';
import { message } from 'ant-design-vue';

import {
  createProjectMeetingRecordApi,
  createProjectReportApi,
  getProjectReportDraftApi,
  uploadFileApi,
  type FileApi,
  type ProjectApi,
} from '#/api/core';

export type ProjectCommunicationItem = {
  actionCount: number;
  attachmentFileId?: null | string;
  attachmentFileName: string;
  sprintId?: null | string;
  sprintName: string;
  id: string;
  kind: 'meeting' | 'report';
  ownerName: string;
  ownerUserId?: null | string;
  periodText: string;
  sortTime: string;
  status: string;
  summary: string;
  title: string;
  type: string;
};

type ProjectFileOption = {
  category: string;
  label: string;
  value: string;
};

type UseProjectCommunicationOptions = {
  currentProjectUserId: ComputedRef<string>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  formatDate: (value?: null | string) => string;
  loadProjectFiles: (projectId?: string) => Promise<void>;
  openDocumentWorkspace: (item: ProjectApi.DocumentItem) => void;
  openProjectFilePreview: (file: FileApi.FileItem) => void;
  projectFiles: Ref<FileApi.FileItem[]>;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
  toDateInputValue: (value?: null | string) => string | undefined;
  toDateTimeInputValue: (value?: null | string) => string | undefined;
};

const reportTypeOptions = ['日报', '周报', '月报', '阶段报告', '项目总结'].map(
  (value) => ({ label: value, value }),
);

const meetingStatusOptions = ['草稿', '已确认', '已归档'].map((value) => ({
  label: value,
  value,
}));

const meetingActionTypeOptions = ['行动项', '转任务', '关联任务', '关联风险', '关联问题', '生成风险', '生成问题'].map(
  (value) => ({ label: value, value }),
);

const meetingActionStatusOptions = ['未开始', '进行中', '已完成', '已关闭'].map(
  (value) => ({ label: value, value }),
);

export function useProjectCommunication(options: UseProjectCommunicationOptions) {
  const {
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
  } = options;

  const savingReport = ref(false);
  const savingMeetingRecord = ref(false);
  const reportDraftLoading = ref(false);
  const showReportForm = ref(false);
  const showMeetingRecordForm = ref(false);

  const emptyReportForm = (): ProjectApi.ProjectReportSaveInput => ({
    attachmentFileId: undefined,
    authorUserId: '',
    businessSummary: '',
    completedSummary: '',
    sprintId: undefined,
    hoursSummary: '',
    milestoneSummary: '',
    nextPlan: '',
    periodEnd: undefined,
    periodStart: undefined,
    reportType: '周报',
    riskIssueSummary: '',
    status: '草稿',
    title: '',
  });

  const emptyMeetingActionItemForm = (): ProjectApi.ProjectMeetingActionItemSaveInput => ({
    actionType: '行动项',
    assigneeUserId: undefined,
    dueDate: undefined,
    estimatedWorkHours: undefined,
    generatedIssueType: '执行问题',
    generatedRiskResponseStrategy: '',
    linkedIssueId: undefined,
    linkedRaidItemId: undefined,
    linkedRequirementId: undefined,
    linkedTaskId: undefined,
    priority: 'P1',
    status: '未开始',
    taskCode: '',
    title: '',
  });

  const emptyMeetingRecordForm = (): ProjectApi.ProjectMeetingRecordSaveInput => ({
    actionItems: [emptyMeetingActionItemForm()],
    attachmentFileId: undefined,
    decisions: '',
    facilitatorUserId: '',
    meetingTime: undefined,
    minutes: '',
    participantUserIds: [],
    status: '草稿',
    topic: '',
  });

  const reportForm = ref<ProjectApi.ProjectReportSaveInput>(emptyReportForm());
  const meetingRecordForm = ref<ProjectApi.ProjectMeetingRecordSaveInput>(
    emptyMeetingRecordForm(),
  );

  const projectFileOptions = computed<ProjectFileOption[]>(() =>
    projectFiles.value.map((item) => ({
      category: item.category || '项目资料',
      label: `${item.fileName} · ${item.category || '项目资料'}`,
      value: item.id,
    })),
  );

  const reportAttachmentOptions = computed(() =>
    projectFileOptions.value.filter((item) =>
      ['Project/Report', 'Project/Document'].includes(item.category),
    ),
  );

  const meetingAttachmentOptions = computed(() =>
    projectFileOptions.value.filter((item) =>
      ['Project/Meeting', 'Project/Document'].includes(item.category),
    ),
  );

  const communicationItems = computed<ProjectCommunicationItem[]>(() => {
    const reports = (detail.value?.reports || []).map((item) => ({
      actionCount: 0,
      attachmentFileId: item.attachmentFileId,
      attachmentFileName: item.attachmentFileName,
      sprintId: item.sprintId,
      sprintName: item.sprintName,
      id: item.id,
      kind: 'report' as const,
      ownerName: item.authorName || '-',
      ownerUserId: item.authorUserId,
      periodText: `${formatDate(item.periodStart)} - ${formatDate(item.periodEnd)}`,
      sortTime: item.periodEnd,
      status: item.status,
      summary:
        item.completedSummary ||
        item.riskIssueSummary ||
        item.milestoneSummary ||
        item.hoursSummary,
      title: item.title,
      type: item.reportType,
    }));
    const meetings = (detail.value?.meetingRecords || []).map((item) => ({
      actionCount: item.actionItems.length,
      attachmentFileId: item.attachmentFileId,
      attachmentFileName: item.attachmentFileName,
      sprintId: undefined,
      sprintName: '',
      id: item.id,
      kind: 'meeting' as const,
      ownerName: item.facilitatorName || '-',
      ownerUserId: item.facilitatorUserId,
      periodText: formatDate(item.meetingTime),
      sortTime: item.meetingTime,
      status: item.status,
      summary: item.decisions || item.minutes,
      title: item.topic,
      type: '会议记录',
    }));

    return [...reports, ...meetings].sort(
      (left, right) =>
        new Date(right.sortTime).getTime() - new Date(left.sortTime).getTime(),
    );
  });

  const communicationMetrics = computed(() => {
    const reports = detail.value?.reports || [];
    const meetings = detail.value?.meetingRecords || [];
    return [
      { label: '项目报告', value: reports.length },
      { label: '会议记录', value: meetings.length },
      {
        label: '会议行动项',
        value: meetings.reduce((sum, item) => sum + item.actionItems.length, 0),
      },
    ];
  });

  function resetCommunicationForms() {
    reportForm.value = {
      ...emptyReportForm(),
      authorUserId: currentProjectUserId.value,
      sprintId: detail.value?.sprints[0]?.id,
    };
    meetingRecordForm.value = {
      ...emptyMeetingRecordForm(),
      facilitatorUserId: currentProjectUserId.value,
      meetingTime: toDateTimeInputValue(new Date().toISOString()),
      participantUserIds: currentProjectUserId.value
        ? [currentProjectUserId.value]
        : [],
    };
    showReportForm.value = false;
    showMeetingRecordForm.value = false;
  }

  const uploadReportFile: UploadProps['customRequest'] = async (uploadOptions) => {
    if (!selectedProjectId.value) {
      const error = new Error('请先选择项目');
      uploadOptions.onError?.(error);
      message.warning(error.message);
      return;
    }

    try {
      const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
        businessId: selectedProjectId.value,
        businessType: 'Project',
        category: 'Project/Report',
        isPublic: false,
        parentPath: 'documents/reports',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      reportForm.value.attachmentFileId = uploadedFile.id;
      message.success('报告附件已上传');
      await loadProjectFiles(selectedProjectId.value);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '报告附件上传失败');
    }
  };

  const uploadMeetingFile: UploadProps['customRequest'] = async (uploadOptions) => {
    if (!selectedProjectId.value) {
      const error = new Error('请先选择项目');
      uploadOptions.onError?.(error);
      message.warning(error.message);
      return;
    }

    try {
      const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
        businessId: selectedProjectId.value,
        businessType: 'Project',
        category: 'Project/Meeting',
        isPublic: false,
        parentPath: 'documents/meetings',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      meetingRecordForm.value.attachmentFileId = uploadedFile.id;
      message.success('会议附件已上传');
      await loadProjectFiles(selectedProjectId.value);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '会议附件上传失败');
    }
  };

  function openCommunicationItem(item: ProjectCommunicationItem) {
    const attachment = item.attachmentFileId
      ? projectFiles.value.find((file) => file.id === item.attachmentFileId)
      : null;
    if (attachment) {
      openProjectFilePreview(attachment);
      return;
    }

    openDocumentWorkspace({
      id: item.id,
      category: item.kind === 'report' ? item.type : '会议记录',
      ownerName: item.ownerName,
      status: item.status,
      summary: item.summary,
      title: item.title,
      version: item.periodText || item.type,
    } as ProjectApi.DocumentItem);
  }

  async function handleGenerateReportDraft() {
    if (!selectedProjectId.value) {
      return;
    }

    reportDraftLoading.value = true;
    try {
      const draft = await getProjectReportDraftApi(selectedProjectId.value, {
        periodEnd: reportForm.value.periodEnd || undefined,
        periodStart: reportForm.value.periodStart || undefined,
        reportType: reportForm.value.reportType || '周报',
      });
      reportForm.value = {
        ...reportForm.value,
        completedSummary: draft.completedSummary,
        businessSummary: draft.businessSummary,
        hoursSummary: draft.hoursSummary,
        milestoneSummary: draft.milestoneSummary,
        nextPlan: draft.nextPlan,
        periodEnd: toDateInputValue(draft.periodEnd),
        periodStart: toDateInputValue(draft.periodStart),
        reportType: draft.reportType,
        riskIssueSummary: draft.riskIssueSummary,
        title: draft.title,
      };
      showMeetingRecordForm.value = false;
      showReportForm.value = true;
      message.success('报告草稿已生成');
    } catch (error) {
      showRequestError(error, '报告草稿生成失败');
    } finally {
      reportDraftLoading.value = false;
    }
  }

  function handleCreateReportDraft() {
    reportForm.value = {
      ...emptyReportForm(),
      authorUserId: currentProjectUserId.value,
      sprintId: detail.value?.sprints[0]?.id,
    };
    showMeetingRecordForm.value = false;
    showReportForm.value = true;
  }

  function handleCancelReportDraft() {
    reportForm.value = {
      ...emptyReportForm(),
      authorUserId: currentProjectUserId.value,
      sprintId: detail.value?.sprints[0]?.id,
    };
    showReportForm.value = false;
  }

  async function handleCreateReport() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!reportForm.value.authorUserId) {
      message.error('请选择报告负责人');
      return;
    }

    if (!reportForm.value.title.trim()) {
      message.error('请填写报告标题，或先生成规则草稿');
      return;
    }

    savingReport.value = true;
    try {
      await createProjectReportApi(selectedProjectId.value, {
        ...reportForm.value,
        attachmentFileId: reportForm.value.attachmentFileId || undefined,
        sprintId: reportForm.value.sprintId || undefined,
        periodEnd: reportForm.value.periodEnd || undefined,
        periodStart: reportForm.value.periodStart || undefined,
        title: reportForm.value.title.trim(),
      });
      message.success('项目报告已保存');
      handleCancelReportDraft();
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '项目报告保存失败');
    } finally {
      savingReport.value = false;
    }
  }

  function handleCreateMeetingDraft() {
    meetingRecordForm.value = {
      ...emptyMeetingRecordForm(),
      facilitatorUserId: currentProjectUserId.value,
      meetingTime: toDateTimeInputValue(new Date().toISOString()),
      participantUserIds: currentProjectUserId.value
        ? [currentProjectUserId.value]
        : [],
    };
    showReportForm.value = false;
    showMeetingRecordForm.value = true;
  }

  function handleCancelMeetingDraft() {
    meetingRecordForm.value = {
      ...emptyMeetingRecordForm(),
      facilitatorUserId: currentProjectUserId.value,
      meetingTime: toDateTimeInputValue(new Date().toISOString()),
      participantUserIds: currentProjectUserId.value
        ? [currentProjectUserId.value]
        : [],
    };
    showMeetingRecordForm.value = false;
  }

  function handleAddMeetingActionItem() {
    meetingRecordForm.value.actionItems.push(emptyMeetingActionItemForm());
  }

  function handleRemoveMeetingActionItem(index: number) {
    if (meetingRecordForm.value.actionItems.length <= 1) {
      meetingRecordForm.value.actionItems = [emptyMeetingActionItemForm()];
      return;
    }

    meetingRecordForm.value.actionItems.splice(index, 1);
  }

  async function handleCreateMeetingRecord() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!meetingRecordForm.value.topic.trim()) {
      message.error('请填写会议主题');
      return;
    }

    if (!meetingRecordForm.value.facilitatorUserId) {
      message.error('请选择主持人');
      return;
    }

    savingMeetingRecord.value = true;
    try {
      await createProjectMeetingRecordApi(selectedProjectId.value, {
        ...meetingRecordForm.value,
        actionItems: meetingRecordForm.value.actionItems
          .filter((item) => item.title.trim())
          .map((item) => ({
            ...item,
            assigneeUserId: item.assigneeUserId || undefined,
            dueDate: item.dueDate || undefined,
            estimatedWorkHours:
              item.actionType === '转任务' ? item.estimatedWorkHours : undefined,
            generatedIssueType:
              item.actionType === '生成问题'
                ? item.generatedIssueType || '执行问题'
                : '执行问题',
            generatedRiskResponseStrategy:
              item.actionType === '生成风险'
                ? item.generatedRiskResponseStrategy.trim()
                : '',
            linkedIssueId:
              item.actionType === '生成问题' ? undefined : item.linkedIssueId || undefined,
            linkedRaidItemId:
              item.actionType === '生成风险' ? undefined : item.linkedRaidItemId || undefined,
            linkedRequirementId:
              item.linkedRequirementId || undefined,
            linkedTaskId:
              item.actionType === '转任务' ||
              item.actionType === '生成风险' ||
              item.actionType === '生成问题'
                ? undefined
                : item.linkedTaskId || undefined,
            priority:
              item.actionType === '转任务' ||
              item.actionType === '生成风险' ||
              item.actionType === '生成问题'
                ? item.priority || 'P1'
                : 'P1',
            taskCode: item.actionType === '转任务' ? item.taskCode.trim() : '',
            title: item.title.trim(),
          })),
        attachmentFileId: meetingRecordForm.value.attachmentFileId || undefined,
        meetingTime: meetingRecordForm.value.meetingTime || undefined,
        participantUserIds: meetingRecordForm.value.participantUserIds,
        topic: meetingRecordForm.value.topic.trim(),
      });
      message.success('会议记录已保存');
      handleCancelMeetingDraft();
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '会议记录保存失败');
    } finally {
      savingMeetingRecord.value = false;
    }
  }

  resetCommunicationForms();

  return {
    communicationItems,
    communicationMetrics,
    handleAddMeetingActionItem,
    handleCancelMeetingDraft,
    handleCancelReportDraft,
    handleCreateMeetingDraft,
    handleCreateMeetingRecord,
    handleCreateReport,
    handleCreateReportDraft,
    handleGenerateReportDraft,
    handleRemoveMeetingActionItem,
    meetingActionStatusOptions,
    meetingActionTypeOptions,
    meetingAttachmentOptions,
    meetingRecordForm,
    meetingStatusOptions,
    openCommunicationItem,
    reportAttachmentOptions,
    reportDraftLoading,
    reportForm,
    reportTypeOptions,
    resetCommunicationForms,
    savingMeetingRecord,
    savingReport,
    showMeetingRecordForm,
    showReportForm,
    uploadMeetingFile,
    uploadReportFile,
  };
}
