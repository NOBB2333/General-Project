<script setup lang="ts">
import { computed, ref } from 'vue';
import { Download, Upload, UserPlus } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { employeeFilesFixture } from '../model/employee-files.data';
import type { EmployeeDraft, EmployeeRow, EmployeeStatusFilter } from '../model/employee-files.types';
import EmployeeFilesHero from './employee-files/EmployeeFilesHero.vue';
import EmployeeFormDialog from './employee-files/EmployeeFormDialog.vue';
import EmployeeSummary from './employee-files/EmployeeSummary.vue';
import EmployeeWorkspace from './employee-files/EmployeeWorkspace.vue';

const rows = ref<EmployeeRow[]>(cloneReferenceRows(employeeFilesFixture.rows));
const status = ref<EmployeeStatusFilter>('全部');
const department = ref('');
const education = ref('');
const query = ref('');
const formOpen = ref(false);
const importOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<EmployeeRow | null>(null);
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value
    .filter(
      (row) =>
        (status.value === '全部' || row.status === status.value) &&
        (!department.value || row.department === department.value) &&
        (!education.value || row.education === education.value) &&
        (!normalized ||
          `${row.code} ${row.name} ${row.department} ${row.position}`.toLocaleLowerCase('zh-CN').includes(normalized)),
    )
    .sort((a, b) => b.joinedAt.localeCompare(a.joinedAt));
});
function togglePayroll(row: EmployeeRow) {
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, payroll: !item.payroll } : item));
}
function setPayroll(mode: 'active' | 'all' | 'none') {
  rows.value = rows.value.map((row) => ({
    ...row,
    payroll: mode === 'all' || (mode === 'active' && row.status !== '已离职'),
  }));
  toast.success('薪资参与范围已更新');
}
function openRow(row: EmployeeRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function saveEmployee(draft: EmployeeDraft) {
  const code = `EMP-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`;
  rows.value = [
    {
      id: `employee-${Date.now()}`,
      code,
      name: draft.name.trim(),
      gender: draft.gender,
      age: Number(draft.age),
      department: draft.department,
      position: draft.position.trim(),
      level: draft.level.trim(),
      employmentType: '正式',
      status: '试用期',
      salary: Number(draft.salary),
      payroll: true,
      joinedAt: draft.joinedAt,
      contractExpiresAt: '2029-09-15',
      phone: draft.phone.trim() || '-',
      education: '本科',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  status.value = '全部';
  toast.success(`${code} 已加入本地员工档案`);
}
function confirmImport() {
  rows.value = [
    {
      id: `employee-import-${Date.now()}`,
      code: 'EMP-IMPORT-013',
      name: '本地导入员工',
      gender: '女',
      age: 26,
      department: '人事行政部',
      position: '行政专员',
      level: 'P3',
      employmentType: '正式',
      status: '试用期',
      salary: 12800,
      payroll: true,
      joinedAt: '2026-09-16',
      contractExpiresAt: '2029-09-15',
      phone: '138-0013-1313',
      education: '本科',
    },
    ...rows.value,
  ];
  importOpen.value = false;
  status.value = '全部';
  toast.success('已导入 1 条本地员工档案');
}
function exportRows() {
  const header = ['工号', '姓名', '部门', '岗位', '职级', '状态', '月薪', '入职日期', '合同到期', '联系方式'];
  const body = filteredRows.value.map((row) => [
    row.code,
    row.name,
    row.department,
    row.position,
    row.level,
    row.status,
    row.salary,
    row.joinedAt,
    row.contractExpiresAt,
    row.phone,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '员工档案.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条员工档案`);
}
const money = (value: number) => `¥${value.toLocaleString('zh-CN')}`;
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/hr/employee-files"
    data-page-label="员工档案"
    data-page-module="人事管理"
    data-employee-files-page
    :data-status="status"
    :data-result-count="filteredRows.length"
    :data-total-count="rows.length"
  >
    <EmployeeFilesHero />
    <div :class="$style.actions">
      <UiButton
        size="sm"
        @click="exportRows"
        ><template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出</UiButton
      ><UiButton
        size="sm"
        @click="importOpen = true"
        ><template #leading
          ><Upload
            :size="14"
            :stroke-width="1.8" /></template
        >批量导入</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="formOpen = true"
        ><template #leading
          ><UserPlus
            :size="14"
            :stroke-width="1.8" /></template
        >新增员工</UiButton
      >
    </div>
    <EmployeeSummary />
    <EmployeeWorkspace
      :rows="filteredRows"
      :all-rows="rows"
      :status="status"
      :department="department"
      :education="education"
      :query="query"
      @update:status="status = $event"
      @update:department="department = $event"
      @update:education="education = $event"
      @update:query="query = $event"
      @toggle-payroll="togglePayroll"
      @set-payroll="setPayroll"
      @open="openRow"
      @export="exportRows"
      @import="importOpen = true"
      @create="formOpen = true"
    />
  </main>
  <EmployeeFormDialog
    v-model:open="formOpen"
    @save="saveEmployee"
  />
  <UiDialog
    v-model:open="importOpen"
    title="批量导入员工"
    description="预览数据仅保存到当前本地会话"
    size="md"
    ><div :class="$style.importPreview">
      <strong>员工导入预览</strong>
      <p>EMP-IMPORT-013 · 本地导入员工 · 人事行政部 · 行政专员</p>
      <span>校验通过 1 条 · 异常 0 条</span>
    </div>
    <template #footer
      ><UiButton
        size="sm"
        @click="importOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="confirmImport"
        >确认导入 1 条</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow ? `${activeRow.code} · ${activeRow.name}` : '员工详情'"
    :description="activeRow ? `${activeRow.department} · ${activeRow.position} · ${activeRow.status}` : undefined"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>职级 / 用工类型</dt>
        <dd>{{ activeRow.level }} · {{ activeRow.employmentType }}</dd>
      </div>
      <div>
        <dt>税前月薪</dt>
        <dd>{{ money(activeRow.salary) }}</dd>
      </div>
      <div>
        <dt>入职日期</dt>
        <dd>{{ activeRow.joinedAt }}</dd>
      </div>
      <div>
        <dt>合同到期</dt>
        <dd>{{ activeRow.contractExpiresAt }}</dd>
      </div>
      <div>
        <dt>联系方式</dt>
        <dd>{{ activeRow.phone }}</dd>
      </div>
      <div>
        <dt>参与薪资计算</dt>
        <dd>{{ activeRow.payroll ? '参与' : '不参与' }}</dd>
      </div>
    </dl>
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
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.actions {
  @apply flex h-[66px] items-center justify-end gap-2;
}
.importPreview {
  @apply p-5;
}
.importPreview strong {
  @apply text-[13px];
}
.importPreview p {
  @apply mt-3 rounded border p-3 text-[11px];
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.importPreview span {
  @apply text-[10px];
  color: #00875a;
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details > div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[12px] font-semibold;
}
</style>
