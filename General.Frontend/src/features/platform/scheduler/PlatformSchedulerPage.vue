<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { CalendarClock } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface, type ListPageRow } from '@/components/page';
import {
  toast,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
} from '@/components/ui';
import { canAccess } from '@/directives/permission';
import PlatformSchedulerJobDialog from './PlatformSchedulerJobDialog.vue';
import PlatformSchedulerTriggerDialog from './PlatformSchedulerTriggerDialog.vue';
import { createSchedulerListModel } from './scheduler-list.model';
import { usePlatformSchedulerPage } from './usePlatformSchedulerPage';

const page = usePlatformSchedulerPage();
const canManage = computed(() => canAccess('Platform.Scheduler.Manage'));
const canExecute = computed(() => canAccess('Platform.Scheduler.Execute'));

const pageModel = computed(() => createSchedulerListModel(page, canManage.value, canExecute.value));

function handleAction(action: PageAction, selectedRows: ListPageRow[]) {
  const keys = selectedRows.map((row) => row.id);
  switch (action.id) {
    case 'reload-jobs':
      void page.loadOverview();
      break;
    case 'create-job':
      if (canManage.value) page.openCreateJob();
      break;
    case 'toggle-jobs':
      if (canManage.value) page.requestToggleJobs(keys);
      break;
    case 'clear-selected-records':
      if (canManage.value) page.requestClearRecords(keys);
      break;
    case 'run-jobs':
      if (canExecute.value) page.requestRun(keys);
      break;
    case 'reload-details':
      void page.reloadDetails();
      break;
    case 'add-trigger':
      if (canManage.value) page.openCreateTrigger();
      break;
    case 'edit-trigger':
    case 'toggle-trigger':
    case 'delete-trigger': {
      if (!canManage.value) break;
      if (keys.length !== 1) {
        toast.error('每次只能选择一个触发器执行该操作。');
        break;
      }
      const trigger = page.triggers.value.find((item) => item.triggerKey === keys[0]);
      if (!trigger) {
        toast.error('触发器状态已变化，请重新读取。');
      } else if (action.id === 'edit-trigger') {
        page.openEditTrigger(trigger.triggerKey);
      } else if (action.id === 'toggle-trigger') {
        page.requestToggleTrigger(trigger);
      } else {
        page.requestDeleteTrigger(trigger);
      }
      break;
    }
    case 'reload-records':
      void page.loadRecords();
      break;
    case 'clear-history':
      if (canManage.value && page.selectedJob.value) page.requestClearRecords([page.selectedJob.value.jobKey]);
      break;
    case 'reload-nodes':
      void page.loadClusterNodes();
      break;
  }
}

function handleRow(row: ListPageRow) {
  if (page.mode.value === 'jobs') {
    void page.loadJobDetails(row.id);
  } else if (page.mode.value === 'triggers' && canManage.value) {
    page.openEditTrigger(row.id);
  }
}

function resetRecordFilters() {
  page.recordStatus.value = '';
  page.recordMode.value = '';
  page.startedFrom.value = '';
  page.startedTo.value = '';
  void page.loadRecords(1);
}

function onKeepLastNChange(value: string) {
  page.keepLastN.value = value === '' ? 0 : Number(value);
}

onMounted(() => void page.loadOverview());
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="handleAction"
    @row="handleRow"
  >
    <template #hero-icon>
      <CalendarClock
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <div>
        <template v-if="page.mode.value === 'jobs'">
          <p>手动执行会立即调用真实任务处理器，即使任务主开关停用也会运行；请先确认处理器影响范围。</p>
          <p>读取触发器时，若触发器为空，后端会补建默认触发器。</p>
          <p v-if="!canManage || !canExecute">
            当前账号：{{ canManage ? '可管理任务' : '只读任务' }}，{{ canExecute ? '可手动执行' : '不可手动执行' }}。
          </p>
          <UiButton
            size="sm"
            :loading="page.nodesLoading.value"
            :disabled="page.nodesLoading.value"
            @click="page.loadClusterNodes"
            >读取集群节点</UiButton
          >
          <span>打开节点列表会更新本机心跳并清理超过 7 天的节点记录。</span>
        </template>

        <template v-else>
          <UiButton
            size="sm"
            @click="page.backToJobs"
            >返回任务列表</UiButton
          >
          <template v-if="page.selectedJob.value && (page.mode.value === 'triggers' || page.mode.value === 'records')">
            <p>
              当前任务：{{ page.selectedJob.value.title }}（{{ page.selectedJob.value.jobKey }}）；处理器：{{
                page.selectedJob.value.handlerKey
              }}；主开关：{{ page.selectedJob.value.isEnabled ? '启用' : '停用' }}。
            </p>
            <p>{{ page.selectedJob.value.description || '此任务没有补充说明。' }}</p>
            <UiTabs
              :model-value="page.detailTab.value"
              @update:model-value="(value) => page.setDetailTab(value as 'records' | 'triggers')"
            >
              <UiTabsList label="任务管理区域">
                <UiTabsTrigger value="triggers">触发器（{{ page.triggers.value.length }}）</UiTabsTrigger>
                <UiTabsTrigger value="records">执行记录（{{ page.recordTotalCount.value }}）</UiTabsTrigger>
              </UiTabsList>
            </UiTabs>
            <UiButton
              v-if="canManage && !page.selectedJob.value.isRunning"
              size="sm"
              @click="page.openEditJob()"
              >编辑任务</UiButton
            >
            <UiButton
              v-if="canExecute"
              size="sm"
              variant="primary"
              @click="page.requestRun([page.selectedJob.value.jobKey])"
              >立即执行</UiButton
            >
            <UiButton
              v-if="canExecute && page.selectedJob.value.isRunning"
              size="sm"
              @click="page.requestCancelJob(page.selectedJob.value.jobKey)"
              >发送取消信号</UiButton
            >
            <UiButton
              v-if="canManage"
              size="sm"
              variant="danger"
              @click="page.requestDeleteJob(page.selectedJob.value.jobKey)"
              >删除任务及全部历史</UiButton
            >
          </template>
        </template>

        <template v-if="page.mode.value === 'records' && page.selectedJob.value">
          <form @submit.prevent="page.loadRecords(1)">
            <UiSelect
              v-model="page.recordStatus.value"
              label="执行状态"
              size="sm"
              :options="[
                { value: '', label: '全部状态' },
                { value: 'Running', label: '执行中' },
                { value: 'Success', label: '成功' },
                { value: 'Failed', label: '失败' },
                { value: 'Skipped', label: '跳过' },
                { value: 'Cancelled', label: '已取消' },
              ]"
            />
            <UiSelect
              v-model="page.recordMode.value"
              label="触发方式"
              size="sm"
              :options="[
                { value: '', label: '全部方式' },
                { value: 'manual', label: '手动' },
                { value: 'auto', label: '自动' },
              ]"
            />
            <label>
              开始日期
              <UiInput
                v-model="page.startedFrom.value"
                type="date"
                aria-label="执行记录开始日期"
              />
            </label>
            <label>
              结束日期
              <UiInput
                v-model="page.startedTo.value"
                type="date"
                aria-label="执行记录结束日期"
              />
            </label>
            <UiButton
              size="sm"
              variant="primary"
              type="submit"
              :loading="page.recordsLoading.value"
              >查询</UiButton
            >
            <UiButton
              size="sm"
              type="button"
              @click="resetRecordFilters"
              >重置</UiButton
            >
            <UiButton
              v-if="canManage"
              size="sm"
              variant="danger"
              @click="page.requestClearRecords([page.selectedJob.value.jobKey])"
              >清理执行记录</UiButton
            >
          </form>
          <p
            v-if="page.recordsError.value"
            role="alert"
          >
            {{ page.recordsError.value }}
          </p>
          <div>
            <UiButton
              size="sm"
              :disabled="page.recordPage.value <= 1 || page.recordsLoading.value"
              @click="page.loadRecords(page.recordPage.value - 1)"
              >上一页</UiButton
            >
            <span
              >第 {{ page.recordPage.value }} / {{ page.recordPageCount.value }} 页，共
              {{ page.recordTotalCount.value }} 条。</span
            >
            <UiButton
              size="sm"
              :disabled="page.recordPage.value >= page.recordPageCount.value || page.recordsLoading.value"
              @click="page.loadRecords(page.recordPage.value + 1)"
              >下一页</UiButton
            >
          </div>
        </template>

        <template v-if="page.mode.value === 'nodes'">
          <p>节点状态由最近心跳推导；超过 90 秒未心跳会显示为离线。</p>
          <p>读取节点会刷新当前实例心跳，并清理 7 天前的节点记录；此读取不是纯查询。</p>
          <p
            v-if="page.nodesError.value"
            role="alert"
          >
            {{ page.nodesError.value }}
          </p>
        </template>

        <p
          v-if="page.overviewError.value && page.mode.value === 'jobs'"
          role="alert"
        >
          {{ page.overviewError.value }}
        </p>
        <p
          v-if="page.detailsError.value && page.mode.value === 'triggers'"
          role="alert"
        >
          {{ page.detailsError.value }}
        </p>
      </div>
    </template>
  </ListPageSurface>

  <PlatformSchedulerJobDialog
    :open="page.jobDialogOpen.value"
    :job="page.editingJob.value"
    :handlers="page.handlers.value"
    :loading="page.savingJob.value"
    @cancel="page.closeJobDialog"
    @save="page.saveJob"
  />

  <PlatformSchedulerTriggerDialog
    :open="page.triggerDialogOpen.value"
    :trigger="page.editingTrigger.value"
    :loading="page.savingTrigger.value"
    @cancel="page.closeTriggerDialog"
    @save="page.saveTrigger"
  />

  <UiDialog
    :open="page.confirmationOpen.value"
    :title="page.confirmationTitle.value"
    :description="page.confirmationDescription.value"
    size="md"
    layer="top"
    @update:open="page.closeConfirmation"
  >
    <div
      v-if="page.confirmation.value?.kind === 'clear-records'"
      :class="$style.confirmationForm"
    >
      <UiFormField label="每个任务保留最新记录数（0–10000）">
        <UiInput
          type="number"
          min="0"
          max="10000"
          step="1"
          :model-value="page.keepLastN.value"
          @update:model-value="onKeepLastNChange"
        />
      </UiFormField>
      <p>超出保留数量的较早记录将被物理删除。</p>
    </div>
    <template #footer>
      <UiButton
        :disabled="page.confirming.value"
        @click="page.closeConfirmation(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="page.confirming.value"
        :disabled="page.confirming.value"
        @click="page.confirmAction"
        >{{ page.confirmationButtonLabel() }}</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.confirmationForm {
  @apply grid gap-3 p-5;
}
</style>
