<script setup lang="ts">
import { Building2, ChevronDown, ChevronRight, UsersRound } from 'lucide-vue-next';
import type { ServiceDepartmentRow } from '../../model/service.types';

defineProps<{
  rows: ServiceDepartmentRow[];
  expandedIds: Set<string>;
  searchable: boolean;
  childParentIds: Set<string>;
}>();

const emit = defineEmits<{
  toggle: [departmentId: string];
}>();

function initial(name: string) {
  return name.slice(0, 1);
}
</script>

<template>
  <div :class="$style.tableFrame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>部门名称</th>
          <th>负责人</th>
          <th>人数</th>
          <th>状态</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-department-id="row.id"
          :data-department-level="row.level"
        >
          <td>
            <div :class="[$style.departmentCell, row.level === 2 && $style.childDepartment]">
              <button
                v-if="childParentIds.has(row.id)"
                :class="$style.expandButton"
                :aria-label="`${expandedIds.has(row.id) ? '折叠' : '展开'}${row.name}`"
                :aria-expanded="searchable || expandedIds.has(row.id)"
                @click="emit('toggle', row.id)"
              >
                <ChevronDown
                  v-if="searchable || expandedIds.has(row.id)"
                  :size="14"
                  :stroke-width="1.8"
                />
                <ChevronRight
                  v-else
                  :size="14"
                  :stroke-width="1.8"
                />
              </button>
              <span
                v-else
                :class="$style.expandSpacer"
              />
              <span :class="[$style.departmentIcon, row.level === 2 && $style.childIcon]">
                <Building2
                  :size="16"
                  :stroke-width="1.8"
                />
              </span>
              <strong>{{ row.name }}</strong>
              <span :class="$style.departmentCode">{{ row.code }}</span>
              <span
                v-if="row.level === 2"
                :class="$style.levelLabel"
                >L2</span
              >
            </div>
          </td>
          <td>
            <div :class="$style.managerCell">
              <span :class="$style.avatar">{{ initial(row.manager) }}</span>
              <span>{{ row.manager }}</span>
            </div>
          </td>
          <td>
            <span :class="$style.headcount">
              <UsersRound
                :size="13"
                :stroke-width="1.8"
              />
              {{ row.headcount }}人
            </span>
          </td>
          <td>
            <span :class="[$style.status, row.status === '筹建中' && $style.formingStatus]">
              {{ row.status }}
            </span>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td
            colspan="4"
            :class="$style.emptyState"
          >
            没有找到匹配的部门
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableFrame {
  @apply min-w-[900px] overflow-hidden border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.table {
  @apply w-full table-fixed border-collapse text-[12px];
}
.table th {
  @apply h-[43px] px-5 text-left font-semibold;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-bottom: 1px solid var(--ui-border);
}
.table th:first-child {
  @apply w-auto pl-[78px];
}
.table th:nth-child(2) {
  @apply w-[128px];
}
.table th:nth-child(3) {
  @apply w-[90px];
}
.table th:nth-child(4) {
  @apply w-[95px];
}
.table td {
  @apply h-[54px] px-5;
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.departmentCell {
  @apply flex min-w-0 items-center;
}
.childDepartment {
  @apply pl-[30px];
}
.expandButton {
  @apply mr-1 flex h-6 w-6 shrink-0 items-center justify-center rounded transition-colors;
  color: var(--ui-text-subtle);
}
.expandButton:hover {
  color: var(--ui-text);
  background: var(--ui-border-subtle);
}
.expandSpacer {
  @apply mr-1 h-6 w-6 shrink-0;
}
.departmentIcon {
  @apply mr-2 flex h-8 w-8 shrink-0 items-center justify-center rounded text-white;
  background: var(--app-primary);
}
.childIcon {
  background: #4c9aff;
}
.departmentCell strong {
  @apply truncate text-[13px] font-semibold;
  color: var(--ui-text);
}
.departmentCode,
.levelLabel {
  @apply ml-2 shrink-0 text-[10px];
  color: #97a0af;
}
.levelLabel {
  @apply ml-1;
}
.managerCell,
.headcount {
  @apply flex items-center gap-2 whitespace-nowrap;
  color: var(--ui-text-muted);
}
.avatar {
  @apply flex h-[22px] w-[22px] shrink-0 items-center justify-center rounded-full text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: #dfe1e6;
}
.headcount {
  @apply gap-1;
  color: var(--ui-text-subtle);
}
.status {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.formingStatus {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.emptyState {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
