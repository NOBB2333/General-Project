import { computed, ref, type Ref } from 'vue';

import type { UploadProps } from 'ant-design-vue';
import { message } from 'ant-design-vue';

import {
  getFileListApi,
  uploadFileApi,
  type FileApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectDocumentsOptions = {
  detail: Ref<null | ProjectApi.ProjectDetail>;
  selectedAcceptanceSprintId: Ref<string | undefined>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
};

type TaskPrdUploadRequestFactory = {
  (task: ProjectApi.TaskItem): UploadProps['customRequest'];
  files: (task: ProjectApi.TaskItem) => FileApi.FileItem[];
};

export function useProjectDocuments(options: UseProjectDocumentsOptions) {
  const {
    detail,
    selectedAcceptanceSprintId,
    selectedProjectId,
    showRequestError,
  } = options;

  const loadingProjectFiles = ref(false);
  const projectFiles = ref<FileApi.FileItem[]>([]);
  const projectFilePreviewOpen = ref(false);
  const previewingProjectFile = ref<FileApi.FileItem | null>(null);

  const charterFiles = computed(() =>
    projectFiles.value
      .filter((item) => {
        const category = item.category || '';
        const parentPath = item.parentPath || '';
        return category === 'Project/Charter' || parentPath === 'documents/charter';
      })
      .sort(
        (left, right) =>
          new Date(right.uploadedAt).getTime() -
          new Date(left.uploadedAt).getTime(),
      ),
  );

  const currentCharterFile = computed(() => charterFiles.value[0] || null);

  const acceptanceCriteriaFiles = computed(() =>
    projectFiles.value
      .filter((item) => isAcceptanceCriteriaFile(item))
      .sort(
        (left, right) =>
          new Date(right.uploadedAt).getTime() -
          new Date(left.uploadedAt).getTime(),
      ),
  );

  const currentAcceptanceCriteriaFile = computed(
    () => acceptanceCriteriaFiles.value[0] || null,
  );

  const acceptanceFiles = computed(() =>
    projectFiles.value
      .filter((item) => {
        const category = item.category || '';
        const parentPath = item.parentPath || '';
        return (
          (category.startsWith('Project/Acceptance') &&
            category !== 'Project/AcceptanceCriteria') ||
          category === 'Project/Delivery' ||
          (parentPath.startsWith('documents/acceptance') &&
            !parentPath.startsWith('documents/acceptance/criteria')) ||
          parentPath.startsWith('documents/delivery')
        );
      })
      .sort(
        (left, right) =>
          new Date(right.uploadedAt).getTime() -
          new Date(left.uploadedAt).getTime(),
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
      count: items.length,
      description:
        items[0]?.summary || '沉淀该分类下的标准文档、评论记录与知识要点。',
      items,
      name,
    }));
  });

  const featuredDocuments = computed(() =>
    (detail.value?.documents || []).slice(0, 6),
  );

  async function loadProjectFiles(projectId = selectedProjectId.value) {
    if (!projectId) {
      projectFiles.value = [];
      return;
    }

    loadingProjectFiles.value = true;
    try {
      const result = await getFileListApi({
        businessProjectId: projectId,
        businessScope: 'Project',
        maxResultCount: 100,
      });
      projectFiles.value = result.items;
    } catch (error) {
      showRequestError(error, '项目资料加载失败');
    } finally {
      loadingProjectFiles.value = false;
    }
  }

  const uploadProjectFile: UploadProps['customRequest'] = async (uploadOptions) => {
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
        category: 'Project/Document',
        isPublic: false,
        parentPath: 'documents',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      message.success('项目资料已上传');
      await loadProjectFiles(selectedProjectId.value);
      openProjectFilePreview(uploadedFile);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '项目资料上传失败');
    }
  };

  function createRequirementFileUploadRequest(
    requirement: ProjectApi.ProjectRequirement,
  ): UploadProps['customRequest'] {
    return async (uploadOptions) => {
      try {
        const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
          businessId: requirement.id,
          businessType: 'ProjectRequirement',
          category: 'Project/Requirement',
          isPublic: false,
          parentPath: `documents/requirements/${requirement.requirementCode}`,
        });
        uploadOptions.onSuccess?.({}, new XMLHttpRequest());
        message.success('需求附件已上传');
        await loadProjectFiles(requirement.projectId);
        openProjectFilePreview(uploadedFile);
      } catch (error) {
        uploadOptions.onError?.(error as Error);
        showRequestError(error, '需求附件上传失败');
      }
    };
  }

  const createTaskPrdUploadRequest: TaskPrdUploadRequestFactory = (task: ProjectApi.TaskItem) => {
    return async (uploadOptions) => {
      try {
        const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
          businessId: task.id,
          businessType: 'ProjectTask',
          category: 'Project/RequirementPrd',
          isPublic: false,
          parentPath: `documents/requirements/${task.taskCode}/prd`,
        });
        uploadOptions.onSuccess?.({}, new XMLHttpRequest());
        message.success('PRD 文档已上传');
        await loadProjectFiles(task.projectId || selectedProjectId.value);
        openProjectFilePreview(uploadedFile);
      } catch (error) {
        uploadOptions.onError?.(error as Error);
        showRequestError(error, 'PRD 文档上传失败');
      }
    };
  };

  createTaskPrdUploadRequest.files = (task: ProjectApi.TaskItem) =>
    projectFiles.value
      .filter((file) => file.businessType === 'ProjectTask' && file.businessId === task.id)
      .filter(
        (file) =>
          file.category === 'Project/RequirementPrd' ||
          (file.parentPath || '').startsWith(`documents/requirements/${task.taskCode}/prd`),
      )
      .sort(
        (left, right) =>
          (right.versionNo || 1) - (left.versionNo || 1) ||
          new Date(right.uploadedAt).getTime() - new Date(left.uploadedAt).getTime(),
      );

  const uploadCharterFile: UploadProps['customRequest'] = async (uploadOptions) => {
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
        category: 'Project/Charter',
        isPublic: false,
        parentPath: 'documents/charter',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      message.success('章程文件已上传');
      await loadProjectFiles(selectedProjectId.value);
      openProjectFilePreview(uploadedFile);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '章程文件上传失败');
    }
  };

  const uploadAcceptanceCriteriaFile: UploadProps['customRequest'] = async (uploadOptions) => {
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
        category: 'Project/AcceptanceCriteria',
        isPublic: false,
        parentPath: 'documents/acceptance/criteria',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      message.success('验收标准文件已上传');
      await loadProjectFiles(selectedProjectId.value);
      openProjectFilePreview(uploadedFile);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '验收标准文件上传失败');
    }
  };

  const uploadAcceptanceFile: UploadProps['customRequest'] = async (uploadOptions) => {
    if (!selectedProjectId.value) {
      const error = new Error('请先选择项目');
      uploadOptions.onError?.(error);
      message.warning(error.message);
      return;
    }

    try {
      const sprintId = selectedAcceptanceSprintId.value;
      const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
        businessId: sprintId || selectedProjectId.value,
        businessType: sprintId ? 'ProjectSprint' : 'Project',
        category: 'Project/AcceptanceRecord',
        isPublic: false,
        parentPath: sprintId
          ? `documents/acceptance/sprints/${sprintId}`
          : 'documents/acceptance/records',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      message.success('验收文件已上传');
      await loadProjectFiles(selectedProjectId.value);
      openProjectFilePreview(uploadedFile);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '验收文件上传失败');
    }
  };

  const uploadDeliveryFile: UploadProps['customRequest'] = async (uploadOptions) => {
    if (!selectedProjectId.value) {
      const error = new Error('请先选择项目');
      uploadOptions.onError?.(error);
      message.warning(error.message);
      return;
    }

    try {
      const sprintId = selectedAcceptanceSprintId.value;
      const uploadedFile = await uploadFileApi(uploadOptions.file as File, {
        businessId: sprintId || selectedProjectId.value,
        businessType: sprintId ? 'ProjectSprint' : 'Project',
        category: 'Project/Delivery',
        isPublic: false,
        parentPath: sprintId
          ? `documents/delivery/sprints/${sprintId}`
          : 'documents/delivery',
      });
      uploadOptions.onSuccess?.({}, new XMLHttpRequest());
      message.success('交付物文件已上传');
      await loadProjectFiles(selectedProjectId.value);
      openProjectFilePreview(uploadedFile);
    } catch (error) {
      uploadOptions.onError?.(error as Error);
      showRequestError(error, '交付物文件上传失败');
    }
  };

  function openProjectFilePreview(file: FileApi.FileItem) {
    previewingProjectFile.value = file;
    projectFilePreviewOpen.value = true;
  }

  function openDocumentWorkspace(item: ProjectApi.DocumentItem) {
    const docWindow = window.open('', '_blank');
    if (!docWindow) {
      return;
    }

    const safeTitle = escapeHtml(item.title || '未命名文档');
    const safeSummary = escapeHtml(
      item.summary || '暂无文档内容，可先在这里补充草稿。',
    );
    const safeCategory = escapeHtml(item.category || '知识文档');
    const safeVersion = escapeHtml(item.version || 'v1.0');
    const safeStatus = escapeHtml(item.status || '草稿');
    const safeOwnerName = escapeHtml(item.ownerName || '未指定负责人');
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
            <span class="tag">${safeCategory}</span>
            <span class="tag">${safeVersion}</span>
            <span class="tag">${safeStatus}</span>
            <span class="tag">${safeOwnerName}</span>
          </div>
          <div class="editor" contenteditable="true">
            <h1>${safeTitle}</h1>
            <p>${safeSummary}</p>
          </div>
        </div>
      </body>
    </html>`;
    docWindow.location.href = URL.createObjectURL(
      new Blob([html], { type: 'text/html;charset=utf-8' }),
    );
  }

  function isAcceptanceCriteriaFile(file: FileApi.FileItem) {
    const category = file.category || '';
    const parentPath = file.parentPath || '';
    return (
      category === 'Project/AcceptanceCriteria' ||
      parentPath.startsWith('documents/acceptance/criteria')
    );
  }

  function escapeHtml(value: string) {
    return value
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;')
      .replaceAll("'", '&#39;');
  }

  return {
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
  };
}
