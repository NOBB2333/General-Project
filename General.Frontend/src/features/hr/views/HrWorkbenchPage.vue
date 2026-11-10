<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { Target, Users } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import { employeeMovements, hrMetrics, hrTrend, recruitmentPositions } from '../model/hr-workbench.data';
import type { EmployeeMovement, HrMetric } from '../model/hr-workbench.types';
import EmployeeMovementsPanel from './hr-workbench/EmployeeMovementsPanel.vue';
import HrMetricGrid from './hr-workbench/HrMetricGrid.vue';
import HrTrendPanel from './hr-workbench/HrTrendPanel.vue';
import HrWorkbenchHero from './hr-workbench/HrWorkbenchHero.vue';

const router = useRouter();
const detailOpen = ref(false);
const detailTitle = ref('');
const detailDescription = ref('');
function inspectMetric(metric: HrMetric) {
  detailTitle.value = metric.label;
  detailDescription.value = `${metric.value} · ${metric.hint}`;
  detailOpen.value = true;
}
function inspectMovement(item: EmployeeMovement) {
  detailTitle.value = `${item.name} · ${item.type}`;
  detailDescription.value = `${item.department} · ${item.position} · ${item.date}`;
  detailOpen.value = true;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/hr-workbench"
    data-page-label="人事工作台"
    data-page-module="人事管理"
    data-hr-workbench-page
  >
    <HrWorkbenchHero />
    <div :class="$style.actions">
      <UiButton
        size="sm"
        @click="router.push('/hr/employee-files')"
        ><template #leading
          ><Users
            :size="14"
            :stroke-width="1.8" /></template
        >员工档案</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="router.push('/hr/recruiting')"
        ><template #leading
          ><Target
            :size="14"
            :stroke-width="1.8" /></template
        >招聘岗位</UiButton
      >
    </div>
    <HrMetricGrid
      :metrics="hrMetrics"
      @inspect="inspectMetric"
    />
    <section :class="$style.dashboard">
      <HrTrendPanel :points="hrTrend" />
      <aside>
        <EmployeeMovementsPanel
          :movements="employeeMovements"
          @open="inspectMovement"
          @view-all="router.push('/hr/employee-files')"
        />
        <section :class="$style.departments">
          <header><h2>部门人员分布</h2></header>
          <p
            v-for="item in [
              { name: '工程部', count: 12 },
              { name: '销售部', count: 8 },
              { name: '生产部', count: 7 },
              { name: '财务部', count: 5 },
            ]"
            :key="item.name"
          >
            <span>{{ item.name }}</span
            ><strong>{{ item.count }}人</strong>
          </p>
        </section>
      </aside>
    </section>
    <section :class="$style.recruitment">
      <header>
        <h2>招聘动态分析</h2>
        <span>招聘漏斗 · 简历投递 60 · 筛选通过 36 · 面试中 20 · Offer沟通 3 · 已录用 5</span>
      </header>
      <table>
        <thead>
          <tr>
            <th>岗位</th>
            <th>投递</th>
            <th>面试</th>
            <th>Offer</th>
            <th>状态</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in recruitmentPositions"
            :key="row.id"
          >
            <td>{{ row.position }}</td>
            <td>{{ row.applications }}</td>
            <td>{{ row.interviews }}</td>
            <td>{{ row.offers }}</td>
            <td>
              <span>{{ row.status }}</span>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </main>
  <UiDialog
    v-model:open="detailOpen"
    :title="detailTitle"
    :description="detailDescription"
    size="sm"
    ><div :class="$style.detail">该数据来自 2026-03-15 人力资源工作台快照。</div>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1160px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.actions {
  @apply flex h-[66px] items-center justify-end gap-2;
}
.dashboard {
  @apply mt-4 grid grid-cols-[2fr_1fr] items-start gap-4;
}
.dashboard > aside {
  @apply grid gap-4;
}
.departments,
.recruitment {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.departments header,
.recruitment header {
  @apply flex min-h-[54px] items-center justify-between border-b px-5;
  border-color: var(--ui-border);
}
.departments h2,
.recruitment h2 {
  @apply m-0 text-[14px];
}
.departments p {
  @apply m-0 flex h-10 items-center justify-between border-b px-5 text-[10px] last:border-b-0;
  border-color: var(--ui-border-subtle);
}
.departments strong {
  color: var(--ui-text-muted);
}
.recruitment {
  @apply mt-4;
}
.recruitment header span {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.recruitment table {
  @apply w-full border-collapse text-[10px];
}
.recruitment th,
.recruitment td {
  @apply border-b px-5 py-3 text-left;
  border-color: var(--ui-border-subtle);
}
.recruitment th {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
.recruitment td span {
  @apply rounded-full px-2 py-1 text-[9px];
  color: #00875a;
  background: #e3fcef;
}
.detail {
  @apply p-5 text-[12px];
  color: var(--ui-text-muted);
}
</style>
