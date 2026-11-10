<script setup lang="ts">
import { computed, ref } from 'vue';
import { ArrowDownUp, Calculator, Download, Filter, Search, Upload, UserPlus } from 'lucide-vue-next';
import {
  UiButton,
  UiInput,
  UiSelect,
  UiSwitch,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  type UiSelectOption,
} from '@/components/ui';
import type { EmployeeRow, EmployeeStatusFilter } from '../../model/employee-files.types';
const props = defineProps<{
  rows: readonly EmployeeRow[];
  allRows: readonly EmployeeRow[];
  status: EmployeeStatusFilter;
  department: string;
  education: string;
  query: string;
}>();
const emit = defineEmits<{
  'update:status': [value: EmployeeStatusFilter];
  'update:department': [value: string];
  'update:education': [value: string];
  'update:query': [value: string];
  togglePayroll: [row: EmployeeRow];
  setPayroll: [mode: 'active' | 'all' | 'none'];
  open: [row: EmployeeRow];
  export: [];
  import: [];
  create: [];
}>();
const advancedOpen = ref(false);
const statuses: readonly EmployeeStatusFilter[] = ['全部', '在职', '试用期', '休假中', '已离职'];
const departmentOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部部门' },
  ...[...new Set(props.allRows.map((row) => row.department))].map((value) => ({ value, label: value })),
]);
const educationOptions: readonly UiSelectOption[] = [
  { value: '', label: '全部学历' },
  { value: '大专', label: '大专' },
  { value: '本科', label: '本科' },
  { value: '硕士', label: '硕士' },
];
const statusCount = (status: EmployeeStatusFilter) =>
  status === '全部' ? props.allRows.length : props.allRows.filter((row) => row.status === status).length;
const payrollCount = computed(() => props.allRows.filter((row) => row.payroll).length);
const payrollTotal = computed(() =>
  props.allRows.filter((row) => row.payroll).reduce((sum, row) => sum + row.salary, 0),
);
const money = (value: number) => `¥${value.toLocaleString('zh-CN')}`;
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.tabsRow">
      <UiTabs
        :model-value="status"
        @update:model-value="emit('update:status', $event as EmployeeStatusFilter)"
        ><UiTabsList label="员工状态"
          ><UiTabsTrigger
            v-for="item in statuses"
            :key="item"
            :value="item"
            >{{ item }} <small>({{ statusCount(item) }})</small></UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
    </header>
    <div :class="$style.filters">
      <Filter
        :size="15"
        :stroke-width="1.8"
      /><UiSelect
        :model-value="department"
        :options="departmentOptions"
        label="员工部门"
        @update:model-value="emit('update:department', $event)"
      /><UiSelect
        :model-value="education"
        :options="educationOptions"
        label="员工学历"
        @update:model-value="emit('update:education', $event)"
      /><UiButton
        size="sm"
        @click="advancedOpen = !advancedOpen"
        ><template #leading
          ><Filter
            :size="13"
            :stroke-width="1.8" /></template
        >高级筛选</UiButton
      ><UiButton size="sm"
        ><template #leading
          ><ArrowDownUp
            :size="13"
            :stroke-width="1.8" /></template
        >入职日期↓</UiButton
      >
      <div :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><UiInput
          :model-value="query"
          placeholder="搜索姓名/工号/部门/岗位..."
          aria-label="搜索员工"
          @update:model-value="emit('update:query', $event)"
        />
      </div>
    </div>
    <div
      v-if="advancedOpen"
      :class="$style.advanced"
    >
      当前高级条件：正式员工 · 合同有效 · 参与薪资计算
    </div>
    <div :class="$style.payroll">
      <span :class="$style.payrollIcon"
        ><Calculator
          :size="17"
          :stroke-width="1.8"
      /></span>
      <p>
        <strong>本月薪资计算范围</strong>已选 <b>{{ payrollCount }}</b> / {{ allRows.length }} 人参与 · 预估工资总额
        <em>{{ money(payrollTotal) }}</em>
      </p>
      <span :class="$style.batchLabel">批量设置：</span
      ><UiButton
        size="xs"
        @click="emit('setPayroll', 'active')"
        >仅在职参与</UiButton
      ><UiButton
        size="xs"
        @click="emit('setPayroll', 'all')"
        >全部参与</UiButton
      ><UiButton
        size="xs"
        @click="emit('setPayroll', 'none')"
        >全部不参与</UiButton
      >
    </div>
    <div :class="$style.tableWrap">
      <table>
        <thead>
          <tr>
            <th>工号</th>
            <th>姓名</th>
            <th>部门</th>
            <th>岗位</th>
            <th>职级</th>
            <th>用工类型</th>
            <th>状态</th>
            <th>月薪(税前)</th>
            <th>参与薪资计算</th>
            <th>入职日期</th>
            <th>合同到期</th>
            <th>联系方式</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <button
                type="button"
                :class="$style.code"
                @click="emit('open', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>
              <span :class="$style.person"
                ><b>{{ row.name.slice(0, 1) }}</b
                ><span
                  ><strong>{{ row.name }}</strong
                  ><small>{{ row.gender }} · {{ row.age }}岁</small></span
                ></span
              >
            </td>
            <td>{{ row.department }}</td>
            <td>{{ row.position }}</td>
            <td>
              <span :class="$style.level">{{ row.level }}</span>
            </td>
            <td>
              <span :class="$style.type">{{ row.employmentType }}</span>
            </td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>{{ money(row.salary) }}</td>
            <td>
              <span :class="$style.switchCell"
                ><UiSwitch
                  :model-value="row.payroll"
                  size="sm"
                  tone="success"
                  :label="`${row.name}参与薪资计算`"
                  @update:model-value="emit('togglePayroll', row)"
                />{{ row.payroll ? '参与' : '不参与' }}</span
              >
            </td>
            <td>{{ row.joinedAt }}</td>
            <td>{{ row.contractExpiresAt }}</td>
            <td>{{ row.phone }}</td>
          </tr>
        </tbody>
      </table>
      <p
        v-if="!rows.length"
        :class="$style.empty"
      >
        当前筛选下没有员工
      </p>
    </div>
    <footer :class="$style.footer">
      <span>共 {{ rows.length }} 条记录</span
      ><UiButton
        size="sm"
        @click="emit('export')"
        ><template #leading
          ><Download
            :size="13"
            :stroke-width="1.8" /></template
        >导出</UiButton
      ><UiButton
        size="sm"
        @click="emit('import')"
        ><template #leading
          ><Upload
            :size="13"
            :stroke-width="1.8" /></template
        >批量导入</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
        ><template #leading
          ><UserPlus
            :size="13"
            :stroke-width="1.8" /></template
        >新增员工</UiButton
      >
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.tabsRow {
  @apply h-[50px] border-b px-3;
  border-color: var(--ui-border);
}
.tabsRow [role='tablist'] {
  @apply h-full;
}
.tabsRow [role='tab'] {
  @apply h-full px-3 text-[11px];
}
.tabsRow small {
  @apply ml-1 text-[8px];
  color: #97a0af;
}
.filters {
  @apply flex h-[54px] items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.filters > div {
  @apply w-[120px];
}
.search {
  @apply ml-auto flex w-[230px] items-center gap-2 rounded px-3;
  background: var(--ui-surface-hover);
}
.search > div {
  @apply w-full;
}
.search input {
  @apply border-0 bg-transparent px-0;
}
.advanced {
  @apply border-b px-5 py-2 text-[9px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: #fff7d6;
}
.payroll {
  @apply flex h-[58px] items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
  background: var(--app-primary-soft);
}
.payrollIcon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: var(--app-primary);
  background: white;
}
.payroll p {
  @apply m-0 flex flex-1 flex-col text-[9px];
  color: var(--ui-text-subtle);
}
.payroll p strong {
  @apply text-[11px];
  color: var(--ui-text);
}
.payroll b,
.payroll em {
  @apply not-italic;
  color: #00875a;
}
.batchLabel {
  @apply ml-auto text-[9px];
  color: #97a0af;
}
.tableWrap {
  @apply overflow-x-auto;
}
.tableWrap table {
  @apply min-w-[1370px] w-full border-collapse text-[9px];
}
.tableWrap th,
.tableWrap td {
  @apply h-[56px] border-b px-3 text-left whitespace-nowrap;
  border-color: var(--ui-border-subtle);
}
.tableWrap th {
  @apply h-10 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.person {
  @apply flex items-center gap-2;
}
.person > b {
  @apply flex h-7 w-7 items-center justify-center rounded-full text-[10px] text-white;
  background: var(--app-primary);
}
.person > span {
  @apply flex flex-col;
}
.person strong {
  @apply text-[10px];
}
.person small {
  @apply mt-0.5 text-[8px];
  color: #97a0af;
}
.level {
  @apply rounded px-2 py-0.5;
  background: var(--ui-surface-hover);
}
.type {
  @apply rounded px-2 py-0.5;
  color: #00875a;
  background: #e3fcef;
}
.status {
  @apply rounded px-2 py-0.5;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='试用期'] {
  color: #0c66e4;
  background: #deebff;
}
.status[data-status='休假中'] {
  color: #ff8b00;
  background: #fff7d6;
}
.status[data-status='已离职'] {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.switchCell {
  @apply flex items-center gap-2;
  color: #00875a;
}
.empty {
  @apply p-10 text-center text-[11px];
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex items-center justify-end gap-2 border-t px-4 py-3;
  border-color: var(--ui-border);
}
.footer > span {
  @apply mr-auto text-[9px];
  color: var(--ui-text-subtle);
}
</style>
