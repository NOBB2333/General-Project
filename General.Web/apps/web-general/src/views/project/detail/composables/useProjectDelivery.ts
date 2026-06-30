import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  createProjectAcceptanceCriteriaApi,
  createProjectAcceptanceRecordApi,
  createProjectQualityChecklistApi,
  createProjectQualityInspectionApi,
  type FileApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectDeliveryOptions = {
  acceptanceFiles: Ref<FileApi.FileItem[]>;
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
  toDateInputValue: (value?: null | string) => string | undefined;
};

export function useProjectDelivery(options: UseProjectDeliveryOptions) {
  const {
    acceptanceFiles,
    detail,
    refreshSelectedProjectDetail,
    selectedProjectId,
    showRequestError,
    toDateInputValue,
  } = options;

  const savingAcceptanceCriteria = ref(false);
  const savingAcceptanceRecord = ref(false);
  const savingQualityChecklist = ref(false);
  const savingQualityInspection = ref(false);
  const showAcceptanceForm = ref(false);
  const showAcceptanceRecordForm = ref(false);
  const showChecklistForm = ref(false);
  const showInspectionForm = ref(false);

  const acceptanceCriteriaForm = ref<ProjectApi.AcceptanceCriteriaSaveInput>({
    description: '',
    milestoneId: undefined,
    status: '待确认',
    taskId: undefined,
    title: '',
  });

  const acceptanceRecordForm = ref<ProjectApi.AcceptanceRecordSaveInput>({
    acceptanceCriteriaIds: [],
    acceptanceFileId: undefined,
    acceptanceResult: '通过',
    acceptanceTime: undefined,
    acceptorUserId: '',
    createIssueWhenRejected: false,
    issueDueDate: undefined,
    issueLevel: 'P0',
    issueOwnerUserId: undefined,
    milestoneId: undefined,
    rectificationRequirement: '',
    sprintId: undefined,
    requirementIds: [],
    remark: '',
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

  const acceptanceStatusOptions = ['待确认', '已确认', '已验收', '不适用'].map(
    (value) => ({ label: value, value }),
  );

  const acceptanceResultOptions = ['通过', '有条件通过', '不通过'].map(
    (value) => ({ label: value, value }),
  );

  const qualityCategoryOptions = [
    '通用',
    '范围',
    '进度',
    '成本',
    '质量',
    '交付',
    '验收',
  ].map((value) => ({ label: value, value }));

  const qualityInspectionResultOptions = [
    '通过',
    '有条件通过',
    '不通过',
    '需整改',
  ].map((value) => ({ label: value, value }));

  const issueLevelOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({
    label: value,
    value,
  }));

  const qualityChecklistOptions = computed(() =>
    (detail.value?.qualityChecklists || []).map((item) => ({
      label: `${item.category} · ${item.title}`,
      value: item.id,
    })),
  );

  const acceptanceCriteriaOptions = computed(() =>
    (detail.value?.acceptanceCriteria || []).map((item) => ({
      label: `${item.status} · ${item.title}`,
      value: item.id,
    })),
  );

  const acceptanceFileOptions = computed(() =>
    acceptanceFiles.value.map((item) => ({
      label: `${item.fileName} · ${item.category || '验收文件'}`,
      value: item.id,
    })),
  );

  function resetDeliveryForms(current = detail.value) {
    acceptanceCriteriaForm.value = {
      description: '',
      milestoneId: undefined,
      status: '待确认',
      taskId: undefined,
      title: '',
    };
    acceptanceRecordForm.value = {
      acceptanceCriteriaIds: [],
      acceptanceFileId: undefined,
      acceptanceResult: '通过',
      acceptanceTime: toDateInputValue(new Date().toISOString()),
      acceptorUserId: current?.members[0]?.userId || '',
      createIssueWhenRejected: false,
      issueDueDate: undefined,
      issueLevel: 'P0',
      issueOwnerUserId: current?.members[0]?.userId,
      milestoneId: current?.milestones[0]?.id,
      rectificationRequirement: '',
      sprintId: undefined,
      requirementIds: [],
      remark: '',
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
  }

  async function handleCreateAcceptanceCriteria() {
    if (!selectedProjectId.value) {
      return;
    }

    if (
      !acceptanceCriteriaForm.value.taskId &&
      !acceptanceCriteriaForm.value.milestoneId
    ) {
      message.error('验收标准必须关联任务或里程碑');
      return;
    }

    savingAcceptanceCriteria.value = true;
    try {
      await createProjectAcceptanceCriteriaApi(
        selectedProjectId.value,
        acceptanceCriteriaForm.value,
      );
      message.success('验收标准已创建');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '验收标准创建失败');
    } finally {
      savingAcceptanceCriteria.value = false;
    }
  }

  async function handleCreateAcceptanceRecord() {
    if (!selectedProjectId.value) {
      return;
    }

    if (!acceptanceRecordForm.value.title) {
      message.error('请填写验收记录标题');
      return;
    }

    if (acceptanceRecordForm.value.acceptanceCriteriaIds.length === 0) {
      message.error('验收记录必须至少关联一条验收标准');
      return;
    }

    if (!acceptanceRecordForm.value.acceptorUserId) {
      message.error('请选择验收人');
      return;
    }

    if (
      acceptanceRecordForm.value.acceptanceResult === '不通过' &&
      !acceptanceRecordForm.value.rectificationRequirement.trim()
    ) {
      message.error('验收不通过时必须填写整改要求');
      return;
    }

    if (
      acceptanceRecordForm.value.createIssueWhenRejected &&
      acceptanceRecordForm.value.acceptanceResult !== '不通过'
    ) {
      message.error('只有验收不通过时才能生成问题记录');
      return;
    }

    if (
      acceptanceRecordForm.value.createIssueWhenRejected &&
      !acceptanceRecordForm.value.issueOwnerUserId
    ) {
      message.error('请选择问题责任人');
      return;
    }

    savingAcceptanceRecord.value = true;
    try {
      await createProjectAcceptanceRecordApi(selectedProjectId.value, {
        ...acceptanceRecordForm.value,
        acceptanceFileId:
          acceptanceRecordForm.value.acceptanceFileId || undefined,
        acceptanceTime: acceptanceRecordForm.value.acceptanceTime || undefined,
        issueDueDate: acceptanceRecordForm.value.issueDueDate || undefined,
        issueOwnerUserId:
          acceptanceRecordForm.value.issueOwnerUserId || undefined,
        milestoneId: acceptanceRecordForm.value.milestoneId || undefined,
        sprintId: acceptanceRecordForm.value.sprintId || undefined,
        requirementIds: acceptanceRecordForm.value.requirementIds || [],
      });
      message.success('验收记录已创建');
      showAcceptanceRecordForm.value = false;
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '验收记录创建失败');
    } finally {
      savingAcceptanceRecord.value = false;
    }
  }

  async function handleCreateQualityChecklist() {
    if (!selectedProjectId.value) {
      return;
    }

    savingQualityChecklist.value = true;
    try {
      await createProjectQualityChecklistApi(
        selectedProjectId.value,
        qualityChecklistForm.value,
      );
      message.success('质量检查单已创建');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '质量检查单创建失败');
    } finally {
      savingQualityChecklist.value = false;
    }
  }

  async function handleCreateQualityInspection() {
    if (!selectedProjectId.value) {
      return;
    }

    if (
      !qualityInspectionForm.value.qualityChecklistId ||
      !qualityInspectionForm.value.inspectorUserId
    ) {
      message.error('请选择检查单和检查人');
      return;
    }

    savingQualityInspection.value = true;
    try {
      await createProjectQualityInspectionApi(
        selectedProjectId.value,
        qualityInspectionForm.value,
      );
      message.success('质量检查记录已创建');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '质量检查记录创建失败');
    } finally {
      savingQualityInspection.value = false;
    }
  }

  return {
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
    resetDeliveryForms,
    savingAcceptanceCriteria,
    savingAcceptanceRecord,
    savingQualityChecklist,
    savingQualityInspection,
    showAcceptanceForm,
    showAcceptanceRecordForm,
    showChecklistForm,
    showInspectionForm,
  };
}
