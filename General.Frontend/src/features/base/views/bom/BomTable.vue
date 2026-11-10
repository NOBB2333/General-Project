<script setup lang="ts">
import { Ban, CheckCircle2, ChevronRight, Copy, Eye, Send, Trash2 } from 'lucide-vue-next';
import { UiTooltip } from '@/components/ui';
import type { BomRow } from '../../model/bom.types';

defineProps<{ rows: readonly BomRow[] }>();
const emit = defineEmits<{
  open: [row: BomRow];
  copy: [row: BomRow];
  transition: [row: BomRow];
  remove: [row: BomRow];
}>();

function transitionLabel(row: BomRow) {
  if (row.status === '草稿') return '提交评审';
  if (row.status === '待评审') return '通过评审';
  return '设为失效';
}

function statusTone(row: BomRow) {
  if (row.status === '已生效') return 'statusActive';
  if (row.status === '待评审') return 'statusReview';
  return 'statusDraft';
}
</script>

<template>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th aria-label="展开" />
          <th>BOM编号</th>
          <th>BOM名称</th>
          <th>产品/设备</th>
          <th>类型</th>
          <th>版本</th>
          <th>状态</th>
          <th>适用项目</th>
          <th>物料数</th>
          <th>成本</th>
          <th>更新时间</th>
          <th>创建人</th>
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
              :aria-label="`查看 ${row.code}`"
              @click="emit('open', row)"
            >
              <ChevronRight
                :size="13"
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
          <td :class="$style.name">{{ row.name }}</td>
          <td>{{ row.productCode }}</td>
          <td>
            <span :class="[$style.typeBadge, row.type === '项目' && $style.projectType]">{{ row.type }}</span>
          </td>
          <td>{{ row.version }}</td>
          <td>
            <span :class="[$style.statusBadge, $style[statusTone(row)]]">{{ row.status }}</span>
          </td>
          <td :class="$style.project">{{ row.project }}</td>
          <td>{{ row.materialCount }}</td>
          <td :class="$style.cost">{{ row.cost }}</td>
          <td :class="$style.date">{{ row.updatedAt }}</td>
          <td>{{ row.creator }}</td>
          <td>
            <div :class="$style.actions">
              <UiTooltip content="查看"
                ><button
                  type="button"
                  aria-label="查看"
                  @click="emit('open', row)"
                >
                  <Eye
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
              <UiTooltip :content="transitionLabel(row)"
                ><button
                  type="button"
                  :aria-label="transitionLabel(row)"
                  @click="emit('transition', row)"
                >
                  <Send
                    v-if="row.status === '草稿'"
                    :size="13"
                    :stroke-width="1.8"
                  /><CheckCircle2
                    v-else-if="row.status === '待评审'"
                    :size="13"
                    :stroke-width="1.8"
                  /><Ban
                    v-else
                    :size="13"
                    :stroke-width="1.8"
                  /></button
              ></UiTooltip>
              <UiTooltip
                v-if="row.status === '草稿'"
                content="删除"
                ><button
                  type="button"
                  aria-label="删除"
                  :class="$style.deleteAction"
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
      当前筛选下没有 BOM
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply min-h-[342px] overflow-x-auto;
}
.table {
  @apply w-full min-w-[1060px] table-fixed border-collapse text-left text-[11px];
}
.table th {
  @apply h-[48px] border-b bg-[#FAFBFC] px-2 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.table th:nth-child(1) {
  @apply w-8;
}
.table th:nth-child(2) {
  @apply w-[92px];
}
.table th:nth-child(3) {
  @apply w-[130px];
}
.table th:nth-child(4) {
  @apply w-[90px];
}
.table th:nth-child(5),
.table th:nth-child(6),
.table th:nth-child(7) {
  @apply w-[55px];
}
.table th:nth-child(8) {
  @apply w-[130px];
}
.table th:nth-child(9) {
  @apply w-[55px];
}
.table th:nth-child(10) {
  @apply w-[75px];
}
.table th:nth-child(11) {
  @apply w-[82px];
}
.table th:nth-child(12) {
  @apply w-[58px];
}
.table th:nth-child(13) {
  @apply w-[100px];
}
.table td {
  @apply h-[68px] border-b px-2 align-middle leading-5;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
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
  @apply text-left font-semibold;
  color: var(--app-primary);
}
.name,
.cost {
  @apply font-semibold;
  color: var(--ui-text);
}
.typeBadge,
.statusBadge {
  @apply inline-flex rounded px-1.5 py-0.5 text-[10px] font-semibold;
}
.typeBadge {
  @apply bg-[#DEEBFF];
  color: var(--app-primary);
}
.projectType {
  @apply bg-[#EAE6FF];
  color: #5243aa;
}
.statusActive {
  @apply bg-[#E3FCEF];
  color: #006644;
}
.statusReview {
  @apply bg-[#FFFAE6];
  color: #ff8b00;
}
.statusDraft {
  @apply bg-[#F4F5F7];
  color: var(--ui-text-muted);
}
.project {
  @apply break-words;
}
.date {
  color: var(--ui-text-subtle);
}
.actions {
  @apply flex items-center gap-0.5;
}
.actions .deleteAction:hover {
  @apply bg-[#FFEBE6];
  color: var(--ui-danger);
}
.empty {
  @apply py-20 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
