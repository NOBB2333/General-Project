<script setup lang="ts">
import { computed, ref } from 'vue';
import { Plus } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { onboardingFixture } from '../model/onboarding.data';
import type { MovementFilter, OnboardingDraft, OnboardingRecord } from '../model/onboarding.types';
import OnboardingFormDialog from './onboarding/OnboardingFormDialog.vue';
import OnboardingHero from './onboarding/OnboardingHero.vue';
import OnboardingSummary from './onboarding/OnboardingSummary.vue';
import OnboardingWorkspace from './onboarding/OnboardingWorkspace.vue';

const rows = ref<OnboardingRecord[]>(cloneReferenceRows(onboardingFixture.rows));
const type = ref<MovementFilter>('全部');
const department = ref('');
const position = ref('');
const status = ref('');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<OnboardingRecord | null>(null);
const summary = computed(() => ({
  onboarding: rows.value.filter((row) => row.type === '入职').length,
  waiting: rows.value.filter((row) => row.status === '待入职').length,
  offboarding: rows.value.filter((row) => row.type === '离职').length,
  processing: rows.value.filter((row) => row.type === '离职' && ['待审批', '交接中'].includes(row.status)).length,
  month: rows.value.filter((row) => row.type === '入职' && row.date.startsWith('2026-03')).length,
  pending: rows.value.filter((row) => ['待入职', '待审批', '交接中'].includes(row.status)).length,
}));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (type.value === '全部' || row.type === type.value) &&
      (!department.value || row.department === department.value) &&
      (!position.value || row.position === position.value) &&
      (!status.value || row.status === status.value) &&
      (!normalized ||
        `${row.code} ${row.employeeName} ${row.department} ${row.position}`
          .toLocaleLowerCase('zh-CN')
          .includes(normalized)),
  );
});
function openRecord(row: OnboardingRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function saveRecord(draft: OnboardingDraft) {
  const sequence = rows.value.filter((row) => row.type === draft.type).length + 1;
  const code = `${draft.type === '入职' ? 'OB' : 'OF'}-LOCAL-${String(sequence).padStart(3, '0')}`;
  rows.value = [
    {
      id: `movement-${Date.now()}`,
      code,
      employeeName: draft.employeeName.trim(),
      type: draft.type,
      department: draft.department,
      position: draft.position.trim(),
      date: draft.date,
      status: draft.type === '入职' ? '待入职' : '待审批',
      owner: draft.owner.trim(),
      completedSteps: 0,
      totalSteps: 8,
    },
    ...rows.value,
  ];
  formOpen.value = false;
  type.value = '全部';
  toast.success(`${code} 已加入本地办理记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/hr/onboarding"
    data-page-label="入职离职"
    data-page-module="人事管理"
    data-onboarding-page
    :data-type="type"
    :data-result-count="filteredRows.length"
    :data-total-count="rows.length"
  >
    <OnboardingHero />
    <div :class="$style.actions">
      <UiButton
        variant="primary"
        size="sm"
        @click="formOpen = true"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新增记录</UiButton
      >
    </div>
    <OnboardingSummary v-bind="summary" />
    <OnboardingWorkspace
      :rows="filteredRows"
      :all-rows="rows"
      :type="type"
      :department="department"
      :position="position"
      :status="status"
      :query="query"
      @update:type="type = $event"
      @update:department="department = $event"
      @update:position="position = $event"
      @update:status="status = $event"
      @update:query="query = $event"
      @open="openRecord"
    />
  </main>
  <OnboardingFormDialog
    v-model:open="formOpen"
    @save="saveRecord"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? `${activeRow.code} · ${activeRow.employeeName}` : '办理详情'"
    :description="activeRow ? `${activeRow.type} · ${activeRow.department} · ${activeRow.position}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>办理日期</dt>
        <dd>{{ activeRow.date }}</dd>
      </div>
      <div>
        <dt>当前状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>经办人</dt>
        <dd>{{ activeRow.owner }}</dd>
      </div>
      <div>
        <dt>办理进度</dt>
        <dd>{{ activeRow.completedSteps }} / {{ activeRow.totalSteps }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.actions {
  @apply flex h-[50px] items-end justify-end;
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details div {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[10px] font-semibold;
  color: var(--ui-text-subtle);
}
.details dd {
  @apply m-0 mt-2 text-[13px] font-medium;
  color: var(--ui-text);
}
</style>
