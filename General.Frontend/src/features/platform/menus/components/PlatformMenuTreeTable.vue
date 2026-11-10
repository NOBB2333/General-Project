<script setup lang="ts">
import { Building2, ChevronDown, ChevronRight, FolderTree, KeyRound, PanelTop } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiSwitch } from '@/components/ui';
import { getMenuCompatibility } from '../menu.model';
import type { PlatformMenuRow } from '../menu.model';

const props = defineProps<{
  childParentIds: Set<string>;
  expandedIds: Set<string>;
  rows: PlatformMenuRow[];
  searchable: boolean;
  savingEnabledId: null | string;
}>();

const emit = defineEmits<{
  'create-child': [menu: PlatformMenuRow];
  delete: [menu: PlatformMenuRow];
  edit: [menu: PlatformMenuRow];
  toggle: [menuId: string];
  'toggle-enabled': [menu: PlatformMenuRow, isEnabled: boolean];
}>();

const iconByType = { 1: FolderTree, 2: PanelTop, 3: KeyRound } as const;

const emptyMessage = computed(() => (props.searchable ? '没有找到匹配的菜单' : '当前没有菜单数据'));

function typeLabel(type: PlatformMenuRow['type']) {
  if (type === 1) return '目录';
  if (type === 2) return '菜单';
  return '按钮';
}
</script>

<template>
  <div :class="$style.tableFrame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>菜单名称与路径</th>
          <th>类型 / 应用</th>
          <th>前端兼容</th>
          <th>启用 / 排序</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-menu-id="row.id"
          :data-menu-type="row.type"
        >
          <td>
            <div :class="[$style.departmentCell, row.depth > 0 && $style.childDepartment]">
              <button
                v-if="childParentIds.has(row.id)"
                :class="$style.expandButton"
                :aria-label="`${expandedIds.has(row.id) ? '折叠' : '展开'}${row.title}`"
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
              <span :class="[$style.departmentIcon, row.depth > 0 && $style.childIcon]">
                <component
                  :is="iconByType[row.type] ?? Building2"
                  :size="16"
                  :stroke-width="1.8"
                />
              </span>
              <div :title="row.path">
                <strong>{{ row.title }}</strong>
                <div :class="$style.departmentCode">
                  {{ row.name }}
                </div>
              </div>
              <div :class="$style.managerCell">
                <UiSwitch
                  :model-value="row.isEnabled"
                  size="sm"
                  :label="`${row.isEnabled ? '停用' : '启用'}${row.title}`"
                  :disabled="savingEnabledId === row.id"
                  @update:model-value="emit('toggle-enabled', row, $event)"
                />
                <UiButton
                  size="xs"
                  variant="ghost"
                  :aria-label="`编辑${row.title}`"
                  @click.stop="emit('edit', row)"
                  >编辑</UiButton
                >
                <UiButton
                  v-if="row.type !== 3"
                  size="xs"
                  variant="ghost"
                  :aria-label="`在${row.title}下新增子菜单`"
                  @click.stop="emit('create-child', row)"
                  >添加</UiButton
                >
                <UiButton
                  size="xs"
                  variant="danger"
                  :aria-label="`删除${row.title}`"
                  @click.stop="emit('delete', row)"
                  >删除</UiButton
                >
              </div>
            </div>
          </td>
          <td>
            <div :class="$style.managerCell">
              <span :class="$style.avatar">{{ typeLabel(row.type).slice(0, 1) }}</span>
              <span>{{ typeLabel(row.type) }} · {{ row.appCode }}</span>
            </div>
          </td>
          <td>
            <div>
              <span
                :class="[
                  $style.status,
                  getMenuCompatibility(row).kind === 'unsupported' && $style.formingStatus,
                  getMenuCompatibility(row).kind === 'external' && $style.formingStatus,
                ]"
                :title="getMenuCompatibility(row).detail"
              >
                {{ getMenuCompatibility(row).label }}
              </span>
              <div
                :class="$style.departmentCode"
                :title="
                  row.type === 3 ? row.permissionCode || row.path : row.component || row.permissionCode || row.path
                "
              >
                {{ row.type === 3 ? row.permissionCode || row.path : row.component || row.permissionCode || row.path }}
              </div>
            </div>
          </td>
          <td>
            <div>
              <span :class="[$style.status, !row.isEnabled && $style.formingStatus]">
                {{ row.isEnabled ? '启用' : '停用' }} · {{ row.order }}
              </span>
            </div>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td
            colspan="4"
            :class="$style.emptyState"
          >
            {{ emptyMessage }}
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
