import { ref, type Ref } from 'vue';

import { message } from 'ant-design-vue';

import {
  createProjectTaskDependencyApi,
  type ProjectApi,
} from '#/api/core';

type UseProjectTaskDependenciesOptions = {
  refreshSelectedProjectDetail: () => Promise<void>;
  selectedProjectId: Ref<string | undefined>;
  showRequestError: (error: unknown, fallback: string) => void;
};

type TaskDependencyDirection = 'predecessor' | 'successor';

export function useProjectTaskDependencies(
  options: UseProjectTaskDependenciesOptions,
) {
  const { refreshSelectedProjectDetail, selectedProjectId, showRequestError } =
    options;

  const savingTaskDependency = ref(false);
  const taskDependencyForm = ref<ProjectApi.TaskDependencySaveInput>({
    dependencyType: 'FinishToStart',
    predecessorTaskId: '',
    successorTaskId: '',
  });
  const taskDetailDependencyForm = ref<ProjectApi.TaskDependencySaveInput>({
    dependencyType: 'FinishToStart',
    predecessorTaskId: '',
    successorTaskId: '',
  });

  const dependencyTypeOptions = [
    { label: '完成后开始', value: 'FinishToStart' },
    { label: '开始后开始', value: 'StartToStart' },
    { label: '完成后完成', value: 'FinishToFinish' },
    { label: '开始后完成', value: 'StartToFinish' },
  ];

  function resetTaskDependencyForm() {
    taskDependencyForm.value = {
      dependencyType: 'FinishToStart',
      predecessorTaskId: '',
      successorTaskId: '',
    };
    taskDetailDependencyForm.value = {
      dependencyType: 'FinishToStart',
      predecessorTaskId: '',
      successorTaskId: '',
    };
  }

  async function handleCreateTaskDependency(
    input = taskDependencyForm.value,
    successMessage = '需求依赖已创建',
  ) {
    if (!selectedProjectId.value) {
      return;
    }

    if (!input.predecessorTaskId || !input.successorTaskId) {
      message.error('请选择前置需求和后续需求');
      return;
    }

    if (input.predecessorTaskId === input.successorTaskId) {
      message.error('前置需求和后续需求不能相同');
      return;
    }

    savingTaskDependency.value = true;
    try {
      await createProjectTaskDependencyApi(
        selectedProjectId.value,
        input,
      );
      message.success(successMessage);
      await refreshSelectedProjectDetail();
    } catch (error) {
      showRequestError(error, '需求依赖创建失败');
    } finally {
      savingTaskDependency.value = false;
    }
  }

  async function handleCreateTaskDetailDependency(
    taskId: string,
    direction: TaskDependencyDirection,
  ) {
    const input: ProjectApi.TaskDependencySaveInput =
      direction === 'predecessor'
        ? {
            dependencyType: taskDetailDependencyForm.value.dependencyType,
            predecessorTaskId: taskDetailDependencyForm.value.predecessorTaskId,
            successorTaskId: taskId,
          }
        : {
            dependencyType: taskDetailDependencyForm.value.dependencyType,
            predecessorTaskId: taskId,
            successorTaskId: taskDetailDependencyForm.value.successorTaskId,
          };

    await handleCreateTaskDependency(
      input,
      direction === 'predecessor' ? '前置依赖已创建' : '后续依赖已创建',
    );
    taskDetailDependencyForm.value = {
      ...taskDetailDependencyForm.value,
      predecessorTaskId: '',
      successorTaskId: '',
    };
  }

  return {
    dependencyTypeOptions,
    handleCreateTaskDependency,
    handleCreateTaskDetailDependency,
    resetTaskDependencyForm,
    savingTaskDependency,
    taskDetailDependencyForm,
    taskDependencyForm,
  };
}
