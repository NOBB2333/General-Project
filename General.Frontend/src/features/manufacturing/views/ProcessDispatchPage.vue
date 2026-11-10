<script setup lang="ts">
import { Grid2X2, Table2, UserRoundPlus } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import type { ProcessDispatchOperation, ProcessDispatchTableRow } from '../model/process-dispatch.types';
import ProcessDispatchHero from './process-dispatch/ProcessDispatchHero.vue';
import ProcessDispatchKanban from './process-dispatch/ProcessDispatchKanban.vue';
import ProcessDispatchMetrics from './process-dispatch/ProcessDispatchMetrics.vue';
import ProcessDispatchTable from './process-dispatch/ProcessDispatchTable.vue';
import { useProcessDispatch } from './process-dispatch/useProcessDispatch';

const router = useRouter();

const {
  tableRows,
  activeView,
  assignmentOpen,
  assignmentAssignee,
  assignmentTeam,
  plannedStartDate,
  plannedEndDate,
  assignmentTarget,
  assignmentTitle,
  assignmentDescription,
  assignmentQuantityLabel,
  currentCenterMembers,
  otherCenterMembers,
  canConfirmAssignment,
  pendingOperations,
  assignedOperations,
  pendingHours,
  workCenterLanes,
  overloadedCenters,
  navigationRequest,
  openAssignment,
  openTableAssignment,
  setAssignmentAssignee,
  updateTableAssignee,
  showOutsourceInfo,
  confirmAssignment,
} = useProcessDispatch();

function openOrder(operation: ProcessDispatchOperation | ProcessDispatchTableRow) {
  void router.push(navigationRequest(operation).path);
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="kanban"
    data-page-route="/mfg/process-dispatch"
  >
    <ProcessDispatchHero />

    <div :class="$style.viewToolbar">
      <div
        :class="$style.viewSwitch"
        role="group"
        aria-label="工序派工视图"
      >
        <UiButton
          :variant="activeView === 'kanban' ? 'primary' : 'secondary'"
          size="sm"
          :aria-pressed="activeView === 'kanban'"
          @click="activeView = 'kanban'"
        >
          <template #leading>
            <Grid2X2
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          看板
        </UiButton>
        <UiButton
          :variant="activeView === 'table' ? 'primary' : 'secondary'"
          size="sm"
          :aria-pressed="activeView === 'table'"
          @click="activeView = 'table'"
        >
          <template #leading>
            <Table2
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          表格
        </UiButton>
      </div>
    </div>

    <ProcessDispatchMetrics
      :pending-count="pendingOperations.length"
      :pending-hours="pendingHours"
      :assigned-count="assignedOperations.length"
      :overloaded-count="overloadedCenters"
    />

    <ProcessDispatchKanban
      v-if="activeView === 'kanban'"
      :pending-operations="pendingOperations"
      :pending-hours="pendingHours"
      :lanes="workCenterLanes"
      @select="openOrder"
      @assign="openAssignment"
    />
    <ProcessDispatchTable
      v-else
      :rows="tableRows"
      @select="openOrder"
      @assign="openTableAssignment"
      @update-assignee="updateTableAssignee"
      @outsource="showOutsourceInfo"
    />

    <UiDialog
      v-model:open="assignmentOpen"
      :title="assignmentTitle"
      :description="assignmentDescription"
      :overlay-class="$style.dialogOverlay"
      :content-class="$style.dialogContent"
      size="sm"
    >
      <template #icon>
        <UserRoundPlus
          :class="$style.dialogIcon"
          :size="20"
          :stroke-width="1.8"
        />
      </template>

      <div
        v-if="assignmentTarget"
        :class="$style.assignmentForm"
      >
        <div :class="$style.assignmentSummary">
          <span
            >计划数 <strong>{{ assignmentQuantityLabel }}</strong></span
          >
          <span
            >定额工时 <strong>{{ assignmentTarget.standardHours }}h</strong></span
          >
          <span
            >工单交期 <strong>{{ assignmentTarget.dueDate }}</strong></span
          >
        </div>

        <fieldset :class="$style.memberFieldset">
          <legend>派工成员 <span aria-hidden="true">*</span></legend>
          <p>本工作中心成员</p>
          <div :class="$style.memberButtons">
            <UiButton
              v-for="member in currentCenterMembers"
              :key="member"
              :variant="assignmentAssignee === member ? 'primary' : 'secondary'"
              size="sm"
              :aria-pressed="assignmentAssignee === member"
              @click="setAssignmentAssignee(member)"
            >
              {{ member }}
            </UiButton>
          </div>
          <p>其他工作中心成员</p>
          <div :class="$style.memberButtons">
            <UiButton
              v-for="member in otherCenterMembers"
              :key="member"
              :variant="assignmentAssignee === member ? 'primary' : 'secondary'"
              size="sm"
              :aria-pressed="assignmentAssignee === member"
              @click="setAssignmentAssignee(member)"
            >
              {{ member }}
            </UiButton>
          </div>
        </fieldset>

        <UiFormField label="班组">
          <UiInput
            v-model="assignmentTeam"
            aria-label="班组"
          />
        </UiFormField>

        <div :class="$style.dateGrid">
          <UiFormField label="计划开工">
            <UiInput
              v-model="plannedStartDate"
              type="date"
              aria-label="计划开工"
            />
          </UiFormField>
          <UiFormField label="计划完工">
            <UiInput
              v-model="plannedEndDate"
              type="date"
              aria-label="计划完工"
            />
          </UiFormField>
        </div>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="assignmentOpen = false"
        >
          取消
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          :disabled="!canConfirmAssignment"
          @click="confirmAssignment"
        >
          <template #leading>
            <UserRoundPlus
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          确认派工
        </UiButton>
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-[1000px] pb-5;
  color: var(--ui-text);
}

.viewToolbar {
  @apply mb-4 flex h-[38px] items-center justify-end;
}

.viewSwitch {
  @apply flex items-center gap-2;
}

.assignmentForm {
  @apply flex min-h-[400px] flex-col gap-4 p-5;
}

.dialogIcon {
  @apply mt-0.5 shrink-0;
  color: var(--app-primary);
}

.dialogContent {
  width: 520px;
}

.dialogOverlay {
  background: color-mix(in srgb, var(--app-primary-dark) 54%, transparent);
}

.assignmentSummary {
  @apply flex flex-wrap items-center gap-x-4 gap-y-2 rounded px-3 py-3 text-[11px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.assignmentSummary strong {
  @apply ml-1 font-semibold;
  color: var(--ui-text);
}

.assignmentSummary span:last-child strong {
  color: var(--ui-danger);
}

.memberFieldset {
  @apply min-w-0 border-0 p-0;
}

.memberFieldset legend {
  @apply text-[12px] font-semibold;
}

.memberFieldset legend span {
  color: var(--ui-danger);
}

.memberFieldset p {
  @apply mb-2 mt-2 text-[11px];
  color: var(--ui-text-subtle);
}

.memberButtons {
  @apply flex flex-wrap gap-2;
}

.memberButtons button {
  @apply min-h-9 px-3.5;
}

.dateGrid {
  @apply grid grid-cols-2 gap-4;
}

@media (prefers-reduced-motion: reduce) {
  .page button:active,
  .dialogContent button:active {
    transform: none;
  }
}
</style>
