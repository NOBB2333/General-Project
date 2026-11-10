<script setup lang="ts">
import { Building2, ChevronDown, ChevronRight, FolderPlus, Pencil, Trash2, Waypoints } from 'lucide-vue-next';
import { computed } from 'vue';
import type { OrganizationApi } from '@/api/core/organization';

const props = defineProps<{
  canManage: boolean;
  expandedIds: ReadonlySet<string>;
  loading: boolean;
  searchable: boolean;
  searchTerm: string;
  rows: readonly OrganizationApi.TreeItem[];
  selectedId: string;
}>();

const emit = defineEmits<{
  createChild: [unit: OrganizationApi.TreeItem];
  delete: [unit: OrganizationApi.TreeItem];
  edit: [unit: OrganizationApi.TreeItem];
  move: [unit: OrganizationApi.TreeItem];
  select: [unit: OrganizationApi.TreeItem];
  toggle: [organizationUnitId: string];
}>();

interface VisibleOrganizationRow {
  depth: number;
  unit: OrganizationApi.TreeItem;
}

const visibleRows = computed<VisibleOrganizationRow[]>(() => {
  const result: VisibleOrganizationRow[] = [];
  const query = props.searchTerm.trim().toLocaleLowerCase();
  if (props.searchable && query) {
    function appendMatches(units: readonly OrganizationApi.TreeItem[], depth: number) {
      for (const unit of units) {
        if ([unit.displayName, unit.code].some((value) => value.toLocaleLowerCase().includes(query))) {
          result.push({ depth, unit });
        }
        appendMatches(unit.children, depth + 1);
      }
    }
    appendMatches(props.rows, 0);
    return result;
  }

  function append(units: readonly OrganizationApi.TreeItem[], depth: number) {
    for (const unit of units) {
      result.push({ depth, unit });
      if (unit.children.length > 0 && props.expandedIds.has(unit.id)) append(unit.children, depth + 1);
    }
  }
  append(props.rows, 0);
  return result;
});

function depthClass(depth: number) {
  return `depth${Math.min(depth, 8)}`;
}
</script>

<template>
  <div
    :class="$style.tableFrame"
    :aria-busy="loading"
  >
    <table :class="$style.table">
      <thead>
        <tr>
          <th>组织名称</th>
          <th>直接成员</th>
          <th>组织成员</th>
          <th>数据范围</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="{ depth, unit } in visibleRows"
          :key="unit.id"
          :aria-selected="selectedId === unit.id"
          :class="selectedId === unit.id && $style.selectedRow"
          :data-organization-id="unit.id"
          :data-organization-level="depth"
          tabindex="0"
          @click="emit('select', unit)"
          @keydown.enter.prevent="emit('select', unit)"
          @keydown.space.prevent="emit('select', unit)"
        >
          <td>
            <div :class="[$style.organizationCell, $style[depthClass(depth)]]">
              <button
                v-if="unit.children.length > 0"
                type="button"
                :class="$style.expandButton"
                :aria-label="`${expandedIds.has(unit.id) ? '折叠' : '展开'}${unit.displayName}`"
                :aria-expanded="expandedIds.has(unit.id)"
                @click.stop="emit('toggle', unit.id)"
              >
                <ChevronDown
                  v-if="expandedIds.has(unit.id)"
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
              <span :class="[$style.organizationIcon, depth > 0 && $style.childIcon]">
                <Building2
                  :size="16"
                  :stroke-width="1.8"
                />
              </span>
              <span :class="$style.organizationCopy">
                <strong>{{ unit.displayName }}</strong>
                <small>{{ unit.code }}</small>
              </span>
            </div>
          </td>
          <td>
            <span :class="$style.memberCount">{{ unit.directMemberCount.toLocaleString('zh-CN') }} 人</span>
          </td>
          <td>
            <span :class="$style.memberCount">{{ unit.memberCount.toLocaleString('zh-CN') }} 人</span>
          </td>
          <td>
            <span :class="[$style.scopeBadge, unit.disabled && $style.readOnlyBadge]">
              {{ unit.disabled ? '只读' : '可管理' }}
            </span>
          </td>
          <td>
            <div :class="$style.rowActions">
              <button
                type="button"
                title="新增下级组织"
                :aria-label="`在${unit.displayName}下新增组织`"
                :disabled="loading || !canManage || unit.disabled"
                @click.stop="emit('createChild', unit)"
              >
                <FolderPlus
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <button
                type="button"
                title="编辑组织名称"
                :aria-label="`编辑${unit.displayName}`"
                :disabled="loading || !canManage || unit.disabled"
                @click.stop="emit('edit', unit)"
              >
                <Pencil
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <button
                type="button"
                title="移动组织"
                :aria-label="`移动${unit.displayName}`"
                :disabled="loading || !canManage || unit.disabled"
                @click.stop="emit('move', unit)"
              >
                <Waypoints
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
              <button
                type="button"
                title="删除组织"
                :aria-label="`删除${unit.displayName}`"
                :disabled="loading || !canManage || unit.disabled"
                @click.stop="emit('delete', unit)"
              >
                <Trash2
                  :size="15"
                  :stroke-width="1.8"
                />
              </button>
            </div>
          </td>
        </tr>
        <tr v-if="loading">
          <td
            colspan="5"
            :class="$style.emptyState"
            role="status"
          >
            正在读取组织架构…
          </td>
        </tr>
        <tr v-else-if="rows.length === 0">
          <td
            colspan="5"
            :class="$style.emptyState"
          >
            暂无组织架构
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../styles/index.css";

.tableFrame {
  @apply min-w-[900px] overflow-x-auto border bg-white;
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
.table th:nth-child(2),
.table th:nth-child(3) {
  @apply w-[105px];
}
.table th:nth-child(4) {
  @apply w-[100px];
}
.table th:nth-child(5) {
  @apply w-[180px];
}
.table td {
  @apply h-[54px] px-5;
  color: var(--ui-text-muted);
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover,
.selectedRow {
  background: var(--ui-surface-hover);
}
.organizationCell {
  @apply flex min-w-0 items-center;
}
.depth0 {
  padding-left: 0;
}
.depth1 {
  padding-left: 30px;
}
.depth2 {
  padding-left: 60px;
}
.depth3 {
  padding-left: 90px;
}
.depth4 {
  padding-left: 120px;
}
.depth5 {
  padding-left: 150px;
}
.depth6 {
  padding-left: 180px;
}
.depth7 {
  padding-left: 210px;
}
.depth8 {
  padding-left: 240px;
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
.organizationIcon {
  @apply mr-2 flex h-8 w-8 shrink-0 items-center justify-center rounded text-white;
  background: var(--app-primary);
}
.childIcon {
  background: #4c9aff;
}
.organizationCopy {
  @apply flex min-w-0 flex-col;
}
.organizationCopy strong {
  @apply truncate text-[13px] font-semibold;
  color: var(--ui-text);
}
.organizationCopy small {
  @apply mt-0.5 truncate text-[10px];
  color: #97a0af;
}
.memberCount {
  @apply whitespace-nowrap;
  color: var(--ui-text-subtle);
}
.scopeBadge {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.readOnlyBadge {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.rowActions {
  @apply flex items-center gap-2;
}
.rowActions button {
  @apply rounded p-1 transition-colors disabled:cursor-not-allowed disabled:opacity-40;
  color: var(--ui-text-subtle);
}
.rowActions button:hover:not(:disabled) {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.emptyState {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
