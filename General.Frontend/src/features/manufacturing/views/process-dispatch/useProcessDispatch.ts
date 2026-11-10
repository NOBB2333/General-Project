import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import {
  processDispatchMemberGroups,
  processDispatchReferenceFixture,
  processDispatchTableRows,
  processDispatchWorkCenters,
} from '../../model/process-dispatch.data';
import type {
  ProcessDispatchLane,
  ProcessDispatchNavigationRequest,
  ProcessDispatchOperation,
  ProcessDispatchTableRow,
  ProcessDispatchView,
} from '../../model/process-dispatch.types';

function cloneTableRow(row: ProcessDispatchTableRow): ProcessDispatchTableRow {
  return { ...row, candidateAssignees: [...row.candidateAssignees] };
}

function membersForCenter(center: string) {
  return processDispatchMemberGroups.find((group) => group.center === center)?.members ?? [];
}

export function useProcessDispatch() {
  const operations = ref<ProcessDispatchOperation[]>(cloneReferenceRows(processDispatchReferenceFixture.rows));
  const tableRows = ref<ProcessDispatchTableRow[]>(processDispatchTableRows.map(cloneTableRow));
  const activeView = ref<ProcessDispatchView>('kanban');
  const assignmentOpen = ref(false);
  const assignmentOperationId = ref('');
  const assignmentTableRowId = ref('');
  const assignmentAssignee = ref('');
  const assignmentTeam = ref('');
  const plannedStartDate = ref('2026-07-25');
  const plannedEndDate = ref('2026-07-19');

  const pendingOperations = computed(() => operations.value.filter((operation) => operation.lane === 'pending'));
  const assignedOperations = computed(() => operations.value.filter((operation) => operation.lane === 'assigned'));
  const pendingHours = computed(() =>
    pendingOperations.value.reduce((total, operation) => total + operation.standardHours, 0),
  );

  const workCenterLanes = computed<ProcessDispatchLane[]>(() =>
    processDispatchWorkCenters.map((center) => {
      const centerOperations = assignedOperations.value.filter((operation) => operation.center === center.name);
      const scheduledHours =
        center.baseHours + centerOperations.reduce((total, operation) => total + operation.standardHours, 0);
      return {
        ...center,
        scheduledHours,
        loadPercent: Math.round((scheduledHours / center.capacityHours) * 100),
        operations: centerOperations,
      };
    }),
  );

  const overloadedCenters = computed(
    () => workCenterLanes.value.filter((center) => center.scheduledHours > center.capacityHours).length,
  );
  const assignmentOperation = computed(
    () => operations.value.find((operation) => operation.id === assignmentOperationId.value) ?? null,
  );
  const assignmentTableRow = computed(
    () => tableRows.value.find((row) => row.id === assignmentTableRowId.value) ?? null,
  );
  const assignmentTarget = computed(() => assignmentTableRow.value ?? assignmentOperation.value);
  const assignmentCenter = computed(() => assignmentTarget.value?.center ?? '');
  const currentCenterMembers = computed(() =>
    assignmentTarget.value ? [...membersForCenter(assignmentCenter.value)] : [],
  );
  const otherCenterMembers = computed(() => {
    return processDispatchMemberGroups
      .filter((group) => group.center !== assignmentCenter.value)
      .flatMap((group) => group.members)
      .filter((member) => !currentCenterMembers.value.includes(member));
  });
  const assignmentTitle = computed(() => {
    const target = assignmentTarget.value;
    if (!target) return '派工';
    return 'action' in target
      ? target.action === '改派'
        ? '改派工序'
        : '派工'
      : target.lane === 'assigned'
        ? '改派工序'
        : '派工';
  });
  const assignmentDescription = computed(() => {
    const target = assignmentTarget.value;
    return target ? `${target.code} · 工序${target.operationNo} ${target.operationName} · ${target.center}` : '';
  });
  const assignmentQuantityLabel = computed(() => {
    const target = assignmentTarget.value;
    if (!target) return '';
    return `${target.quantity}${'unit' in target ? ` ${target.unit}` : ''}`;
  });
  const canConfirmAssignment = computed(
    () =>
      Boolean(assignmentTarget.value) &&
      Boolean(
        assignmentAssignee.value && assignmentTeam.value.trim() && plannedStartDate.value && plannedEndDate.value,
      ),
  );

  function navigationRequest(
    operation: ProcessDispatchOperation | ProcessDispatchTableRow,
  ): ProcessDispatchNavigationRequest {
    return {
      path: `/mfg/orders/${operation.code}`,
      workOrderCode: operation.code,
      operationId: 'linkedOperationId' in operation ? (operation.linkedOperationId ?? operation.id) : operation.id,
    };
  }

  function openAssignment(operation: ProcessDispatchOperation) {
    const row = tableRows.value.find((item) => item.linkedOperationId === operation.id);
    assignmentOperationId.value = operation.id;
    assignmentTableRowId.value = row?.id ?? '';
    assignmentTeam.value = operation.center || row?.center || '';
    assignmentAssignee.value = operation.assignee || membersForCenter(assignmentTeam.value)[0] || '';
    plannedStartDate.value = '2026-07-25';
    plannedEndDate.value = operation.dueDate;
    assignmentOpen.value = true;
  }

  function openTableAssignment(row: ProcessDispatchTableRow) {
    if (row.action === '外协') return;
    assignmentTableRowId.value = row.id;
    assignmentOperationId.value = row.linkedOperationId ?? '';
    assignmentTeam.value = row.center;
    assignmentAssignee.value = row.assignee || row.candidateAssignees[0] || '';
    plannedStartDate.value = '2026-07-25';
    plannedEndDate.value = row.dueDate;
    assignmentOpen.value = true;
  }

  function setAssignmentAssignee(assignee: string) {
    assignmentAssignee.value = assignee;
  }

  function updateTableAssignee(row: ProcessDispatchTableRow, assignee: string) {
    row.assignee = assignee;
    if (!row.linkedOperationId) return;
    const operation = operations.value.find((item) => item.id === row.linkedOperationId);
    if (operation?.lane === 'assigned') {
      operation.assignee = assignee;
      operation.owner = assignee;
    }
  }

  function showOutsourceInfo(row: ProcessDispatchTableRow) {
    toast.info(`${row.code} 工序${row.operationNo} 的委外进度请在委外管理中维护`);
  }

  function confirmAssignment() {
    const operation = assignmentOperation.value;
    const row = assignmentTableRow.value;
    const target = assignmentTarget.value;
    if (!target || !canConfirmAssignment.value) return;

    if (operation) {
      operation.lane = 'assigned';
      operation.center = assignmentCenter.value;
      operation.assignee = assignmentAssignee.value;
      operation.owner = assignmentAssignee.value;
      operation.status = '已派/在制';
      operation.type = '按工作中心已派工序';
      operation.value = '在制';
    }

    if (row) {
      row.center = assignmentCenter.value;
      row.assignee = assignmentAssignee.value;
      row.candidateAssignees = [...new Set([assignmentAssignee.value, ...membersForCenter(assignmentCenter.value)])];
      if (row.status === '待派工') row.status = '进行中';
      row.action = '改派';
    }

    assignmentOpen.value = false;
    toast.success(`${target.code} 工序${target.operationNo} 已派给 ${assignmentAssignee.value}`);
  }

  return {
    operations,
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
  };
}
