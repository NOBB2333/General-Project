<script setup lang="ts">
import { computed } from 'vue';
import { Eye, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { RecruitingPosition, RecruitingSection } from '../../model/recruiting.types';

const props = defineProps<{
  rows: readonly RecruitingPosition[];
  allRows: readonly RecruitingPosition[];
  section: RecruitingSection;
  department: string;
  query: string;
}>();
const emit = defineEmits<{
  'update:department': [value: string];
  'update:query': [value: string];
  create: [];
  open: [row: RecruitingPosition];
}>();
const departmentOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部部门' },
  ...[...new Set(props.allRows.map((row) => row.department))].map((value) => ({ value, label: value })),
]);
const sectionHint = computed(() => {
  if (props.section === '招聘岗位') return '仅显示正在使用的招聘岗位';
  if (props.section === '招聘管理') return '汇总岗位投递、面试与 Offer 进展';
  return '岗位说明与编制信息';
});
const vacancy = (row: RecruitingPosition) => Math.max(row.approved - row.staffed, 0);
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <div :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><UiInput
          :model-value="query"
          placeholder="搜索岗位..."
          aria-label="搜索岗位"
          @update:model-value="emit('update:query', $event)"
        />
      </div>
      <UiSelect
        :model-value="department"
        :options="departmentOptions"
        label="岗位部门"
        size="sm"
        @update:model-value="emit('update:department', $event)"
      />
      <span :class="$style.modeHint">{{ sectionHint }}</span>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新增岗位</UiButton
      >
    </div>
    <div :class="$style.tableWrap">
      <table>
        <thead>
          <tr>
            <th>岗位名称</th>
            <th>部门</th>
            <th>职级</th>
            <th>编制/在岗</th>
            <th>薪资范围</th>
            <th>汇报对象</th>
            <th>状态</th>
            <th>更新时间</th>
            <th>操作</th>
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
                :class="$style.position"
                @click="emit('open', row)"
              >
                <strong>{{ row.name }}</strong>
                <small>{{ row.code }}</small>
              </button>
            </td>
            <td>{{ row.department }}</td>
            <td>{{ row.level }}</td>
            <td>
              <span :class="$style.headcount">{{ row.staffed }}/{{ row.approved }}</span
              ><small
                v-if="vacancy(row)"
                :class="$style.vacancy"
                >缺{{ vacancy(row) }}</small
              >
            </td>
            <td>{{ row.salaryRange }}</td>
            <td>{{ row.reportsTo }}</td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>{{ row.updatedAt }}</td>
            <td>
              <button
                type="button"
                :class="$style.view"
                @click="emit('open', row)"
              >
                <Eye
                  :size="13"
                  :stroke-width="1.8"
                />查看
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <p
        v-if="!rows.length"
        :class="$style.empty"
      >
        当前筛选下没有岗位
      </p>
    </div>
    <footer :class="$style.footer">
      共 {{ rows.length }} 个岗位
      <span v-if="section === '招聘管理'"
        >· 投递 {{ rows.reduce((sum, row) => sum + row.applications, 0) }} · 面试
        {{ rows.reduce((sum, row) => sum + row.interviews, 0) }} · Offer
        {{ rows.reduce((sum, row) => sum + row.offers, 0) }}</span
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
.toolbar {
  @apply flex h-[56px] items-center gap-2 px-4;
  background: var(--ui-surface-subtle);
}
.search {
  @apply relative w-[290px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-10 -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 pl-9;
}
.modeHint {
  @apply ml-2 text-[10px];
  color: var(--ui-text-subtle);
}
.toolbar > button:last-child {
  @apply ml-auto;
}
.tableWrap {
  @apply overflow-x-auto border-t;
  border-color: var(--ui-border);
}
.tableWrap table {
  @apply w-full min-w-[1180px] border-collapse text-left text-[11px];
}
.tableWrap th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
}
.tableWrap td {
  @apply h-[58px] border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableWrap tr:hover td {
  background: color-mix(in srgb, var(--ui-surface-hover) 55%, white);
}
.position {
  @apply flex flex-col items-start text-left;
}
.position strong {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}
.position:hover strong,
.view {
  color: var(--app-primary);
}
.position small {
  @apply mt-0.5 text-[9px];
  color: #97a0af;
}
.headcount {
  @apply font-medium;
}
.vacancy {
  @apply ml-1 text-[9px] font-semibold;
  color: var(--ui-danger);
}
.status {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status[data-status='已停用'] {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.view {
  @apply inline-flex items-center gap-1 font-semibold;
}
.view:hover {
  text-decoration: underline;
}
.empty {
  @apply m-0 py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex h-10 items-center justify-end px-4 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
</style>
