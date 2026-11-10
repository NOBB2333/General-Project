<script setup lang="ts">
import { computed, ref } from 'vue';
import { BriefcaseBusiness, FileText, UsersRound } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabs, UiTabsList, UiTabsTrigger, toast } from '@/components/ui';
import { recruitingFixture } from '../model/recruiting.data';
import type { RecruitingPosition, RecruitingPositionDraft, RecruitingSection } from '../model/recruiting.types';
import RecruitingFormDialog from './recruiting/RecruitingFormDialog.vue';
import RecruitingHero from './recruiting/RecruitingHero.vue';
import RecruitingSummary from './recruiting/RecruitingSummary.vue';
import RecruitingWorkspace from './recruiting/RecruitingWorkspace.vue';

const rows = ref<RecruitingPosition[]>(cloneReferenceRows(recruitingFixture.rows));
const section = ref<RecruitingSection>(recruitingFixture.defaults.section);
const department = ref(recruitingFixture.defaults.department);
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<RecruitingPosition | null>(null);
const summary = computed(() => ({
  total: rows.value.length,
  active: rows.value.filter((row) => row.status === '使用中').length,
  approved: rows.value.reduce((sum, row) => sum + row.approved, 0),
  vacancies: rows.value.reduce((sum, row) => sum + Math.max(row.approved - row.staffed, 0), 0),
}));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    if (section.value === '招聘岗位' && row.status !== '使用中') return false;
    if (section.value === '招聘管理' && (row.status !== '使用中' || row.approved <= row.staffed)) return false;
    return (
      (!department.value || row.department === department.value) &&
      (!normalized || `${row.code} ${row.name} ${row.department}`.toLocaleLowerCase('zh-CN').includes(normalized))
    );
  });
});

function setSection(value: string) {
  section.value = value as RecruitingSection;
}

function openPosition(row: RecruitingPosition) {
  activeRow.value = row;
  detailOpen.value = true;
}

function savePosition(draft: RecruitingPositionDraft) {
  const sequence = rows.value.length + 1;
  const code = `JD-LOCAL-${String(sequence).padStart(3, '0')}`;
  rows.value = [
    {
      id: `position-${Date.now()}`,
      code,
      name: draft.name.trim(),
      department: draft.department,
      level: draft.level.trim(),
      approved: Number(draft.approved),
      staffed: Number(draft.staffed),
      salaryRange: draft.salaryRange.trim(),
      reportsTo: draft.reportsTo.trim(),
      status: '使用中',
      updatedAt: '2026-09-16',
      applications: 0,
      interviews: 0,
      offers: 0,
    },
    ...rows.value,
  ];
  formOpen.value = false;
  section.value = '岗位说明';
  toast.success(`${code} 已加入本地岗位说明`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/hr/recruiting"
    data-page-label="招聘管理"
    data-page-module="人事管理"
    data-recruiting-page
    :data-section="section"
    :data-result-count="filteredRows.length"
    :data-total-count="rows.length"
  >
    <RecruitingHero />
    <nav :class="$style.sectionNav">
      <UiTabs
        :model-value="section"
        @update:model-value="setSection"
        ><UiTabsList label="招聘业务视图"
          ><UiTabsTrigger value="岗位说明"
            ><FileText
              :size="14"
              :stroke-width="1.8"
            />岗位说明</UiTabsTrigger
          ><UiTabsTrigger value="招聘岗位"
            ><BriefcaseBusiness
              :size="14"
              :stroke-width="1.8"
            />招聘岗位</UiTabsTrigger
          ><UiTabsTrigger value="招聘管理"
            ><UsersRound
              :size="14"
              :stroke-width="1.8"
            />招聘管理</UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
    </nav>
    <RecruitingSummary
      :total="summary.total"
      :active="summary.active"
      :approved="summary.approved"
      :vacancies="summary.vacancies"
    />
    <RecruitingWorkspace
      :rows="filteredRows"
      :all-rows="rows"
      :section="section"
      :department="department"
      :query="query"
      @update:department="department = $event"
      @update:query="query = $event"
      @create="formOpen = true"
      @open="openPosition"
    />
  </main>
  <RecruitingFormDialog
    v-model:open="formOpen"
    @save="savePosition"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? `${activeRow.name} · ${activeRow.code}` : '岗位详情'"
    :description="activeRow ? `${activeRow.department} · ${activeRow.level} · ${activeRow.status}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>编制 / 在岗</dt>
        <dd>{{ activeRow.approved }} / {{ activeRow.staffed }}</dd>
      </div>
      <div>
        <dt>薪资范围</dt>
        <dd>{{ activeRow.salaryRange }}</dd>
      </div>
      <div>
        <dt>汇报对象</dt>
        <dd>{{ activeRow.reportsTo }}</dd>
      </div>
      <div>
        <dt>更新时间</dt>
        <dd>{{ activeRow.updatedAt }}</dd>
      </div>
      <div>
        <dt>候选人漏斗</dt>
        <dd>投递 {{ activeRow.applications }} · 面试 {{ activeRow.interviews }} · Offer {{ activeRow.offers }}</dd>
      </div>
      <div>
        <dt>当前缺编</dt>
        <dd>{{ Math.max(activeRow.approved - activeRow.staffed, 0) }} 人</dd>
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
.sectionNav {
  @apply mt-4 inline-flex rounded border bg-white;
  border-color: var(--ui-border);
}
.sectionNav [role='tab'] {
  @apply min-h-9 min-w-[108px];
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
