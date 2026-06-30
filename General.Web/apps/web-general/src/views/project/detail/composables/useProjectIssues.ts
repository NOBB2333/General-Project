import { computed, ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  createProjectIssueApi,
  deleteProjectIssueApi,
  updateProjectIssueApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectIssuesOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
};

export function useProjectIssues(options: UseProjectIssuesOptions) {
  const { detail, refreshSelectedProjectDetail, selectedProjectId, showRequestError } = options;

  const deletingIssueId = ref('');
  const editingIssueId = ref<string | null>(null);
  const issueFormOpen = ref(false);
  const savingIssue = ref(false);
  const issueForm = ref<ProjectApi.IssueSaveInput>(emptyIssueForm());

  const projectIssueTypeOptions = ['缺陷', '问题', '阻塞', '改进'].map((value) => ({
    label: value,
    value,
  }));

  const projectIssueLevelOptions = ['P0', 'P1', 'P2', 'P3'].map((value) => ({
    label: value,
    value,
  }));

  const projectIssueStatusOptions = ['待处理', '处理中', '已解决', '已关闭', '已取消'].map(
    (value) => ({ label: value, value })
  );

  const issueSelectOptions = computed(() =>
    (detail.value?.issues || []).map((item) => ({
      label: `${item.level} · ${item.title}`,
      value: item.id,
    }))
  );

  const issueMetrics = computed(() => {
    const items = detail.value?.issues || [];
    return [
      { label: '当前问题总数', value: items.length },
      {
        label: 'P0优先级',
        value: items.filter((item) => item.level === 'P0').length,
      },
      {
        label: '待测试验证',
        value: items.filter((item) => item.testerOwnerName).length,
      },
      {
        label: '已关联需求',
        value: items.filter((item) => item.requirementTitle).length,
      },
    ];
  });

  const workflowPreviewGroups = computed(() => {
    const issues = detail.value?.issues || [];
    if (issues.length === 0) {
      return [];
    }

    const closedKeywords = ['关闭', '完成', '解决'];
    return [
      {
        key: 'defect-flow',
        title: '缺陷处理流',
        tone: 'danger',
        steps: [
          {
            key: 'capture',
            title: '登记',
            count: issues.filter((item) => item.type.includes('缺陷')).length,
          },
          {
            key: 'analysis',
            title: '分析',
            count: issues.filter((item) => item.type.includes('缺陷') && item.productOwnerName)
              .length,
          },
          {
            key: 'fix',
            title: '修复',
            count: issues.filter((item) => item.type.includes('缺陷') && item.developerOwnerName)
              .length,
          },
          {
            key: 'verify',
            title: '验证',
            count: issues.filter((item) => item.type.includes('缺陷') && item.testerOwnerName)
              .length,
          },
          {
            key: 'done',
            title: '关闭',
            count: issues.filter(
              (item) =>
                item.type.includes('缺陷') &&
                closedKeywords.some((keyword) => item.status.includes(keyword))
            ).length,
          },
        ],
      },
      {
        key: 'issue-flow',
        title: '问题跟踪流',
        tone: 'warning',
        steps: [
          {
            key: 'raise',
            title: '提出',
            count: issues.filter((item) => item.type.includes('问题')).length,
          },
          {
            key: 'scope',
            title: '定责',
            count: issues.filter((item) => item.type.includes('问题') && item.requirementTitle)
              .length,
          },
          {
            key: 'coordination',
            title: '协同',
            count: issues.filter((item) => item.type.includes('问题') && item.productOwnerName)
              .length,
          },
          {
            key: 'tracking',
            title: '跟踪',
            count: issues.filter((item) => item.type.includes('问题') && item.developerOwnerName)
              .length,
          },
          {
            key: 'archive',
            title: '归档',
            count: issues.filter(
              (item) =>
                item.type.includes('问题') &&
                closedKeywords.some((keyword) => item.status.includes(keyword))
            ).length,
          },
        ],
      },
      {
        key: 'change-flow',
        title: '变更影响跟踪',
        tone: 'primary',
        steps: [
          { key: 'collect', title: '收集', count: issues.length },
          {
            key: 'requirement',
            title: '需求确认',
            count: issues.filter((item) => item.requirementTitle).length,
          },
          {
            key: 'product',
            title: '产品评审',
            count: issues.filter((item) => item.productOwnerName).length,
          },
          {
            key: 'develop',
            title: '进入排期',
            count: issues.filter((item) => item.developerOwnerName).length,
          },
          {
            key: 'release',
            title: '发布闭环',
            count: issues.filter((item) =>
              closedKeywords.some((keyword) => item.status.includes(keyword))
            ).length,
          },
        ],
      },
    ];
  });

  function handleOpenIssueCreate() {
    editingIssueId.value = null;
    issueForm.value = emptyIssueForm();
    issueFormOpen.value = true;
  }

  function handleOpenIssueEdit(item: ProjectApi.IssueItem) {
    editingIssueId.value = item.id;
    issueForm.value = {
      acceptanceCriteriaId: item.acceptanceCriteriaId || undefined,
      developerOwnerUserId: item.developerOwnerUserId || undefined,
      dueDate: item.dueDate ? item.dueDate.slice(0, 10) : undefined,
      level: item.level || 'P1',
      ownerUserId: item.ownerUserId,
      productOwnerUserId: item.productOwnerUserId || undefined,
      qualityChecklistId: item.qualityChecklistId || undefined,
      requirementTitle: item.requirementTitle || '',
      status: item.status || '待处理',
      testerOwnerUserId: item.testerOwnerUserId || undefined,
      title: item.title || '',
      type: item.type || '缺陷',
    };
    issueFormOpen.value = true;
  }

  function handleCloseIssueForm() {
    issueFormOpen.value = false;
    editingIssueId.value = null;
    issueForm.value = emptyIssueForm();
  }

  async function handleSaveIssue() {
    const projectId = selectedProjectId.value;
    if (!projectId) {
      return undefined;
    }

    if (!issueForm.value.title.trim()) {
      message.warning('请填写问题标题');
      return undefined;
    }

    if (!issueForm.value.ownerUserId) {
      message.warning('请选择负责人');
      return undefined;
    }

    savingIssue.value = true;
    try {
      const data = sanitizeIssueForm(issueForm.value);
      let savedIssueId = editingIssueId.value || '';
      if (editingIssueId.value) {
        await updateProjectIssueApi(editingIssueId.value, data);
        message.success('问题记录已更新');
      } else {
        savedIssueId = await createProjectIssueApi(projectId, data);
        message.success('问题记录已创建');
      }
      handleCloseIssueForm();
      await refreshSelectedProjectDetail();
      return savedIssueId;
    } catch (error) {
      showRequestError(error, editingIssueId.value ? '问题记录更新失败' : '问题记录创建失败');
      return undefined;
    } finally {
      savingIssue.value = false;
    }
  }

  async function handleDeleteIssue(issueId: string) {
    deletingIssueId.value = issueId;
    try {
      await deleteProjectIssueApi(issueId);
      message.success('问题记录已删除');
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '问题记录删除失败');
    } finally {
      deletingIssueId.value = '';
    }
  }

  function emptyIssueForm(): ProjectApi.IssueSaveInput {
    return {
      acceptanceCriteriaId: undefined,
      developerOwnerUserId: undefined,
      dueDate: undefined,
      level: 'P1',
      ownerUserId: '',
      productOwnerUserId: undefined,
      qualityChecklistId: undefined,
      requirementTitle: '',
      status: '待处理',
      testerOwnerUserId: undefined,
      title: '',
      type: '缺陷',
    };
  }

  function sanitizeIssueForm(form: ProjectApi.IssueSaveInput): ProjectApi.IssueSaveInput {
    return {
      acceptanceCriteriaId: form.acceptanceCriteriaId || undefined,
      developerOwnerUserId: form.developerOwnerUserId || undefined,
      dueDate: form.dueDate || undefined,
      level: form.level,
      ownerUserId: form.ownerUserId,
      productOwnerUserId: form.productOwnerUserId || undefined,
      qualityChecklistId: form.qualityChecklistId || undefined,
      requirementTitle: form.requirementTitle.trim(),
      status: form.status,
      testerOwnerUserId: form.testerOwnerUserId || undefined,
      title: form.title.trim(),
      type: form.type,
    };
  }

  function resetIssueForms() {
    editingIssueId.value = null;
    issueFormOpen.value = false;
    issueForm.value = emptyIssueForm();
  }

  return {
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
    resetIssueForms,
    savingIssue,
    workflowPreviewGroups,
  };
}
