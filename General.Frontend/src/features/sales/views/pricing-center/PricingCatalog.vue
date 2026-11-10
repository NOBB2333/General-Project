<script setup lang="ts">
import {
  CheckSquare2,
  ChevronRight,
  Download,
  Filter,
  FolderTree,
  Layers3,
  Package,
  Search,
  Settings2,
  SlidersHorizontal,
  Upload,
} from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiCheckbox, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PriceProduct, PricingGroupNode } from '../../model/pricing-center.types';

const props = defineProps<{
  rows: readonly PriceProduct[];
  groups: readonly PricingGroupNode[];
  activeGroup: string;
  query: string;
  businessUnit: string;
  businessUnitOptions: readonly UiSelectOption[];
  selectedIds: readonly string[];
}>();

const emit = defineEmits<{
  'update:activeGroup': [id: string];
  'update:query': [query: string];
  'update:businessUnit': [id: string];
  'toggle:row': [id: string];
  'toggle:all': [];
  'select:group': [];
  edit: [row: PriceProduct];
  bulkAdjust: [];
  configure: [];
  assignGroup: [];
  import: [];
  export: [];
}>();

const allChecked = computed(
  () => props.rows.length > 0 && props.rows.every((row) => props.selectedIds.includes(row.id)),
);
const someChecked = computed(() => props.rows.some((row) => props.selectedIds.includes(row.id)));
const checkboxState = computed(() => (allChecked.value ? true : someChecked.value ? 'indeterminate' : false));

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function margin(price: number, cost: number) {
  return cost ? `+${(((price - cost) / cost) * 100).toFixed(1)}%` : '-';
}
</script>

<template>
  <section :class="$style.catalog">
    <aside :class="$style.groups">
      <header>
        <FolderTree
          :size="16"
          :stroke-width="1.8"
        /><strong>商品分组</strong>
      </header>
      <nav aria-label="商品分组">
        <button
          v-for="(group, index) in groups"
          :key="group.id"
          type="button"
          :class="[$style.group, activeGroup === group.id && $style.groupActive, index === 0 && $style.allGroup]"
          :aria-pressed="activeGroup === group.id"
          @click="emit('update:activeGroup', group.id)"
        >
          <Package
            v-if="index === 0"
            :size="14"
            :stroke-width="1.8"
          />
          <ChevronRight
            v-else
            :size="13"
            :stroke-width="1.8"
          />
          <span>{{ group.label }}</span
          ><b>{{ group.count }}</b>
        </button>
      </nav>
      <footer>
        <UiButton
          variant="subtle"
          size="sm"
          @click="emit('select:group')"
          ><template #leading
            ><CheckSquare2
              :size="14"
              :stroke-width="1.8" /></template
          >选中当前分组商品</UiButton
        >
      </footer>
    </aside>

    <div :class="$style.content">
      <header :class="$style.toolbar">
        <label :class="$style.searchField">
          <Search
            :size="15"
            :stroke-width="1.8"
          />
          <UiInput
            :model-value="query"
            placeholder="搜索物料编码、商品名称、规格型号…"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
        <Filter
          :size="15"
          :stroke-width="1.8"
          :class="$style.filterIcon"
        />
        <UiSelect
          :model-value="businessUnit"
          :options="businessUnitOptions"
          label="BU 筛选"
          size="sm"
          @update:model-value="emit('update:businessUnit', $event)"
        />
        <div :class="$style.actions">
          <UiButton
            :class="$style.adjustButton"
            size="sm"
            @click="emit('bulkAdjust')"
          >
            <template #leading
              ><SlidersHorizontal
                :size="14"
                :stroke-width="1.8" /></template
            >批量调价
          </UiButton>
          <UiButton
            :class="$style.relationButton"
            size="sm"
            @click="emit('configure')"
          >
            <template #leading
              ><Settings2
                :size="14"
                :stroke-width="1.8" /></template
            >批量配置价格关系
          </UiButton>
          <UiButton
            :class="$style.assignButton"
            size="sm"
            @click="emit('assignGroup')"
          >
            <template #leading
              ><Layers3
                :size="14"
                :stroke-width="1.8" /></template
            >分配价格组
          </UiButton>
          <i />
          <UiButton
            size="sm"
            @click="emit('import')"
            ><template #leading
              ><Upload
                :size="14"
                :stroke-width="1.8" /></template
            >批量导入</UiButton
          >
          <UiButton
            size="sm"
            @click="emit('export')"
            ><template #leading
              ><Download
                :size="14"
                :stroke-width="1.8" /></template
            >导出</UiButton
          >
        </div>
      </header>
      <div
        v-if="selectedIds.length"
        :class="$style.selection"
      >
        已选择 <strong>{{ selectedIds.length }}</strong> 件商品
      </div>
      <p :class="$style.resultCount">
        共 <strong>{{ rows.length }}</strong> 条记录
      </p>
      <div :class="$style.scroller">
        <table :class="$style.table">
          <thead>
            <tr>
              <th>
                <UiCheckbox
                  :model-value="checkboxState"
                  label="选择当前结果"
                  @update:model-value="emit('toggle:all')"
                />
              </th>
              <th>物料编码</th>
              <th>商品名称</th>
              <th>规格型号</th>
              <th>单位</th>
              <th>BU</th>
              <th>价格组</th>
              <th :class="$style.numeric">成本价</th>
              <th :class="$style.numeric">最低售价</th>
              <th :class="$style.numeric">建议售价</th>
              <th :class="$style.numeric">目录价</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in rows"
              :key="row.id"
              :class="selectedIds.includes(row.id) && $style.selectedRow"
            >
              <td>
                <UiCheckbox
                  :model-value="selectedIds.includes(row.id)"
                  :label="`选择 ${row.name}`"
                  @update:model-value="emit('toggle:row', row.id)"
                />
              </td>
              <td :class="$style.code">{{ row.materialCode }}</td>
              <td :class="$style.name">{{ row.name }}</td>
              <td :class="$style.spec">{{ row.spec }}</td>
              <td>{{ row.unit }}</td>
              <td>{{ row.buName }}</td>
              <td>{{ row.priceGroupName }}</td>
              <td :class="$style.numeric">{{ money(row.costPrice) }}</td>
              <td :class="$style.numeric">
                <strong>{{ money(row.minPrice) }}</strong
                ><small>{{ margin(row.minPrice, row.costPrice) }}</small>
              </td>
              <td :class="$style.numeric">
                <strong>{{ money(row.suggestedPrice) }}</strong
                ><small>{{ margin(row.suggestedPrice, row.costPrice) }}</small>
              </td>
              <td :class="$style.numeric">
                <strong>{{ money(row.listPrice) }}</strong
                ><small>{{ margin(row.listPrice, row.costPrice) }}</small>
              </td>
              <td>
                <span :class="[$style.status, row.status === '生效中' ? $style.activeStatus : $style.pendingStatus]">{{
                  row.status
                }}</span>
              </td>
              <td>
                <button
                  type="button"
                  :class="$style.editButton"
                  @click="emit('edit', row)"
                >
                  编辑
                </button>
              </td>
            </tr>
            <tr v-if="!rows.length">
              <td
                colspan="13"
                :class="$style.empty"
              >
                暂无匹配的商品数据
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.catalog {
  @apply flex min-h-[520px] gap-4;
}
.groups {
  @apply flex w-[255px] shrink-0 flex-col overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.groups > header {
  @apply flex h-[50px] items-center gap-2 border-b px-3 text-[13px];
  color: var(--ui-text);
  border-color: var(--ui-border-subtle);
}
.groups nav {
  @apply flex-1 px-1.5 py-1.5;
}
.group {
  @apply flex w-full items-center gap-1.5 rounded px-2 py-1.5 text-left text-[12px] transition-colors;
  color: var(--ui-text-muted);
}
.group:hover {
  background: var(--ui-surface-hover);
}
.group span {
  @apply min-w-0 flex-1 truncate;
}
.group b {
  @apply text-[10px] font-medium tabular-nums;
  color: #97a0af;
}
.allGroup {
  @apply mb-2 border-b pb-2;
  border-radius: 0;
  border-color: var(--ui-border-subtle);
}
.groupActive {
  @apply font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.groups footer {
  @apply border-t p-3;
  border-color: var(--ui-border-subtle);
}
.groups footer button {
  @apply w-full;
  color: var(--app-primary);
  border: 1px solid #b3d4ff;
  background: #f4f8ff;
}
.content {
  @apply min-w-0 flex-1 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[60px] items-center gap-2 border-b px-4 py-2;
  border-color: var(--ui-border-subtle);
}
.searchField {
  @apply relative block w-[255px] shrink-0;
}
.searchField > svg {
  @apply pointer-events-none absolute left-2.5 top-1/2 z-[1] -translate-y-1/2;
  color: #97a0af;
}
.searchField input {
  @apply h-8 pl-8 text-[12px];
}
.filterIcon {
  color: #97a0af;
}
.actions {
  @apply ml-auto flex items-center gap-2;
}
.actions > i {
  @apply h-6 w-px;
  background: var(--ui-border);
}
.adjustButton {
  color: #0052cc;
  border-color: #b3d4ff;
  background: #f4f8ff;
}
.relationButton {
  color: #00875a;
  border-color: #abf5d1;
  background: #e3fcef;
}
.assignButton {
  color: #6554c0;
  border-color: #c0b6f2;
  background: #eae6ff;
}
.selection {
  @apply border-b px-4 py-2 text-[12px];
  color: var(--app-primary);
  border-color: #b3d4ff;
  background: #f4f8ff;
}
.resultCount {
  @apply border-b px-4 py-2 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.resultCount strong {
  color: var(--ui-text-muted);
}
.scroller {
  @apply max-h-[590px] overflow-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.table {
  @apply w-full border-collapse text-left text-[11px];
  min-width: 1190px;
  color: var(--ui-text-muted);
}
.table thead {
  @apply sticky top-0 z-[1];
  background: #f7f8f9;
}
.table th {
  @apply h-10 whitespace-nowrap px-3 py-2.5 font-semibold;
  color: #5e6c84;
}
.table th:first-child,
.table td:first-child {
  @apply w-10 pl-4 pr-2;
}
.table td {
  @apply border-t px-3 py-2.5 align-middle;
  border-color: var(--ui-border-subtle);
}
.table tbody tr {
  @apply transition-colors;
}
.table tbody tr:hover {
  background: #fafbfc;
}
.selectedRow {
  background: #f4f8ff;
}
.code {
  @apply whitespace-nowrap font-mono;
}
.name {
  @apply min-w-[205px] font-semibold;
  color: var(--ui-text);
}
.spec {
  @apply min-w-[175px] whitespace-nowrap;
  color: #6b778c;
}
.numeric {
  @apply whitespace-nowrap text-right tabular-nums;
}
.numeric strong {
  @apply block text-[11px] font-semibold;
  color: var(--ui-text);
}
.numeric small {
  @apply block text-[10px] font-semibold;
  color: var(--ui-success);
}
.status {
  @apply inline-flex whitespace-nowrap rounded-full px-2 py-0.5 text-[10px] font-semibold;
}
.activeStatus {
  color: var(--ui-success);
  background: #e3fcef;
}
.pendingStatus {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.editButton {
  @apply font-medium hover:underline;
  color: var(--app-primary);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: #97a0af;
}
@media (max-width: 1250px) {
  .toolbar {
    @apply flex-wrap;
  }
  .actions {
    @apply ml-0 w-full;
  }
}
</style>
