<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { getDictItemsApi } from '@/api/core/dict';
import { updateProjectStatusApi } from '@/api/core/project';
import { UiAlertDialog, UiButton, UiDialog, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

const props = defineProps<{ projectId: string; status: string }>();
const emit = defineEmits<{ changed: [] }>();

const statuses = ref<UiSelectOption[]>([]);
const selectedStatus = ref('');
const dialogOpen = ref(false);
const confirmCloseOpen = ref(false);
const loading = ref(false);
const error = ref('');
const notice = ref('');

const canManage = computed(() => hasAccessCode('Project.Governance.Manage'));
const willClose = computed(() => ['已完成', '已关闭', '已解除'].includes(selectedStatus.value));
const canSave = computed(
  () =>
    Boolean(selectedStatus.value) &&
    selectedStatus.value !== props.status &&
    !loading.value &&
    statuses.value.length > 0,
);
const selectOptions = computed(() => {
  if (props.status && !statuses.value.some((item) => item.value === props.status)) {
    return [{ value: props.status, label: props.status }, ...statuses.value];
  }
  return statuses.value;
});

async function loadStatuses() {
  try {
    const items = await getDictItemsApi('project_status');
    statuses.value = items.map((item) => ({ value: item.value, label: item.label || item.value }));
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目状态字典读取失败。';
  }
}

onMounted(() => void loadStatuses());
watch(
  () => props.status,
  (status) => (selectedStatus.value = status),
  { immediate: true },
);

function openDialog() {
  selectedStatus.value = props.status;
  error.value = '';
  notice.value = '';
  dialogOpen.value = true;
}

function requestSave() {
  if (!canSave.value) return;
  if (willClose.value) {
    confirmCloseOpen.value = true;
    return;
  }
  void saveStatus();
}

async function saveStatus() {
  loading.value = true;
  error.value = '';
  notice.value = '';
  try {
    await updateProjectStatusApi(props.projectId, { status: selectedStatus.value });
    notice.value = `项目状态已更新为“${selectedStatus.value}”。`;
    dialogOpen.value = false;
    confirmCloseOpen.value = false;
    emit('changed');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '项目状态更新失败，请重试。';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目生命周期"
  >
    <div>
      <h3 :class="$style.heading">项目状态</h3>
      <p>当前状态：{{ status }}。状态变更由后端规则校验。</p>
      <p
        v-if="notice"
        role="status"
      >
        {{ notice }}
      </p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
    </div>
    <UiButton
      v-if="canManage"
      variant="secondary"
      @click="openDialog"
      >变更状态</UiButton
    >

    <UiDialog
      :open="dialogOpen"
      title="变更项目状态"
      description="进入执行需要章程审批通过；收尾状态需要项目验收条件全部满足。"
      size="md"
      @update:open="dialogOpen = $event"
    >
      <UiSelect
        v-model="selectedStatus"
        label="目标状态"
        :options="selectOptions"
        :disabled="loading || !statuses.length"
      />
      <p
        v-if="!statuses.length"
        role="status"
      >
        状态字典没有可用项，暂时无法变更。
      </p>
      <p
        v-if="willClose"
        role="note"
      >
        收尾前，服务端会检查关键里程碑、确认的验收标准、通过的验收记录及项目关联验收文件。
      </p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="loading"
          @click="dialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          :disabled="!canSave"
          :loading="loading"
          @click="requestSave"
          >保存状态</UiButton
        >
      </template>
    </UiDialog>

    <UiAlertDialog
      :open="confirmCloseOpen"
      title="确认项目收尾"
      description="服务端将校验验收条件；若条件未满足，项目状态保持不变并显示具体原因。"
      confirm-label="确认收尾"
      tone="warning"
      @update:open="confirmCloseOpen = $event"
      @confirm="saveStatus"
    />
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply flex flex-wrap items-center justify-between gap-3 border-t pt-4;
}

.heading {
  @apply font-semibold;
}
</style>
