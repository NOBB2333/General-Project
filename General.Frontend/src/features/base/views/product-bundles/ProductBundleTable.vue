<script setup lang="ts">
import { Archive, CheckCircle2, ChevronRight, Copy, Pencil, Trash2 } from 'lucide-vue-next';
import { UiTooltip } from '@/components/ui';
import type { ProductBundleRow } from '../../model/product-bundles.types';

defineProps<{ rows: readonly ProductBundleRow[] }>();
const emit = defineEmits<{
  open: [row: ProductBundleRow];
  edit: [row: ProductBundleRow];
  copy: [row: ProductBundleRow];
  toggle: [row: ProductBundleRow];
  remove: [row: ProductBundleRow];
}>();
</script>

<template>
  <div :class="$style.frame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th aria-label="展开" />
          <th>编号</th>
          <th>组合名称</th>
          <th>商品数</th>
          <th>成本合计</th>
          <th>建议售价</th>
          <th>状态</th>
          <th>引用次数</th>
          <th>创建人</th>
          <th>创建日期</th>
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
              :class="$style.expand"
              :aria-label="`查看 ${row.name}`"
              @click="emit('open', row)"
            >
              <ChevronRight
                :size="14"
                :stroke-width="1.8"
              />
            </button>
          </td>
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
            <button
              type="button"
              :class="$style.nameCell"
              @click="emit('open', row)"
            >
              <strong>{{ row.name }}</strong
              ><span>{{ row.description }}</span>
            </button>
          </td>
          <td>
            <span :class="$style.count">{{ row.productCount }}</span>
          </td>
          <td :class="$style.money">{{ row.cost }}</td>
          <td :class="$style.price">
            <strong>{{ row.suggestedPrice }}</strong
            ><small>{{ row.margin }}</small>
          </td>
          <td>
            <span :class="[$style.badge, row.status === '启用' ? $style.active : $style.inactive]">{{
              row.status
            }}</span>
          </td>
          <td>{{ row.referenceCount }}</td>
          <td>{{ row.creator }}</td>
          <td :class="$style.date">{{ row.createdAt }}</td>
          <td>
            <div :class="$style.actions">
              <UiTooltip content="编辑"
                ><button
                  type="button"
                  aria-label="编辑"
                  @click="emit('edit', row)"
                >
                  <Pencil
                    :size="13"
                    :stroke-width="1.8"
                  /></button
              ></UiTooltip>
              <UiTooltip content="复制"
                ><button
                  type="button"
                  aria-label="复制"
                  @click="emit('copy', row)"
                >
                  <Copy
                    :size="13"
                    :stroke-width="1.8"
                  /></button
              ></UiTooltip>
              <UiTooltip :content="row.status === '启用' ? '停用' : '启用'"
                ><button
                  type="button"
                  :aria-label="row.status === '启用' ? '停用' : '启用'"
                  @click="emit('toggle', row)"
                >
                  <Archive
                    v-if="row.status === '启用'"
                    :size="13"
                    :stroke-width="1.8"
                  /><CheckCircle2
                    v-else
                    :size="13"
                    :stroke-width="1.8"
                  /></button
              ></UiTooltip>
              <UiTooltip content="删除"
                ><button
                  type="button"
                  aria-label="删除"
                  @click="emit('remove', row)"
                >
                  <Trash2
                    :size="13"
                    :stroke-width="1.8"
                  /></button
              ></UiTooltip>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有商品组合
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.frame {
  @apply overflow-x-auto rounded border bg-white;
  border-color: var(--ui-border);
}
.table {
  @apply w-full min-w-[1240px] table-fixed border-collapse text-left text-[11.5px];
}
.table th {
  @apply h-10 border-b bg-[#FAFBFC] px-2 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.table th:nth-child(1) {
  @apply w-10;
}
.table th:nth-child(2) {
  @apply w-[100px];
}
.table th:nth-child(3) {
  @apply w-[330px];
}
.table th:nth-child(4) {
  @apply w-[80px];
}
.table th:nth-child(5),
.table th:nth-child(6) {
  @apply w-[115px];
}
.table th:nth-child(7),
.table th:nth-child(8),
.table th:nth-child(9) {
  @apply w-[90px];
}
.table th:nth-child(10) {
  @apply w-[110px];
}
.table th:nth-child(11) {
  @apply w-[125px];
}
.table td {
  @apply h-[60px] border-b px-2 align-middle;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  @apply border-b-0;
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.expand,
.actions button {
  @apply flex h-7 w-7 items-center justify-center rounded transition-colors;
  color: var(--ui-text-subtle);
}
.expand:hover,
.actions button:hover {
  @apply bg-[#EBECF0];
  color: var(--ui-text);
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.nameCell {
  @apply block w-full overflow-hidden text-left;
}
.nameCell strong {
  @apply block truncate text-[12px] font-semibold;
  color: var(--ui-text);
}
.nameCell span {
  @apply mt-1 block truncate text-[10.5px];
  color: var(--ui-text-subtle);
}
.count {
  @apply inline-flex min-w-6 justify-center rounded bg-[#DEEBFF] px-1.5 py-0.5 font-semibold;
  color: var(--app-primary);
}
.money,
.price strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.price small {
  @apply mt-0.5 block text-[10px];
  color: var(--ui-success);
}
.badge {
  @apply inline-flex rounded px-2 py-1 text-[10.5px] font-semibold;
}
.active {
  @apply bg-[#E3FCEF];
  color: #006644;
}
.inactive {
  @apply bg-[#F4F5F7];
  color: var(--ui-text-subtle);
}
.date {
  color: var(--ui-text-subtle);
}
.actions {
  @apply flex items-center gap-0.5;
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
