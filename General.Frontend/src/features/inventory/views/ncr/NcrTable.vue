<script setup lang="ts">
import { Eye, Play } from 'lucide-vue-next';
import { UiButton, UiCheckbox } from '@/components/ui';
import type { NcrRecord } from '../../model/ncr.types';

const props = defineProps<{ rows: readonly NcrRecord[]; selectedIds: readonly string[] }>();
const emit = defineEmits<{
  view: [row: NcrRecord];
  execute: [row: NcrRecord];
  toggle: [id: string, selected: boolean];
  'toggle-all': [selected: boolean];
}>();
function selected(id: string) {
  return props.selectedIds.includes(id);
}
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>
            <UiCheckbox
              :model-value="rows.length > 0 && rows.every((row) => selected(row.id))"
              label="选择全部处理单"
              @update:model-value="emit('toggle-all', $event === true)"
            />
          </th>
          <th>处理单号</th>
          <th>来源检验单</th>
          <th>供应商</th>
          <th>不合格数</th>
          <th>紧急度</th>
          <th>责任方</th>
          <th>处理方式</th>
          <th>状态</th>
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
            <UiCheckbox
              :model-value="selected(row.id)"
              :label="`选择 ${row.id}`"
              @update:model-value="emit('toggle', row.id, $event === true)"
            />
          </td>
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.id }}
            </button>
          </td>
          <td>
            <a
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.inspectionCode }}</a
            >
          </td>
          <td>{{ row.supplier }}</td>
          <td>
            <strong :class="$style.failed">{{ row.failedQuantity }}</strong>
          </td>
          <td>
            <span
              :class="$style.tag"
              :data-kind="row.urgency"
              >{{ row.urgency }}</span
            >
          </td>
          <td>
            <span
              :class="$style.tag"
              :data-kind="row.responsibility"
              >{{ row.responsibility }}</span
            >
          </td>
          <td>
            <span
              :class="$style.tag"
              :data-kind="row.resolution"
              >{{ row.resolution }}</span
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.createdAt }}</td>
          <td>
            <div :class="$style.actions">
              <UiButton
                variant="ghost"
                size="xs"
                @click="emit('view', row)"
                ><Eye :size="13" />查看</UiButton
              ><UiButton
                v-if="row.status === '待执行'"
                variant="ghost"
                size="xs"
                @click="emit('execute', row)"
                ><Play :size="13" />执行</UiButton
              >
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的处理单
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1380px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[48px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
  color: var(--app-primary);
}
.scroller a {
  color: var(--app-primary);
}
.failed {
  color: var(--ui-danger);
}
.tag,
.status {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.tag[data-kind='紧急'],
.tag[data-kind='复检'],
.tag[data-kind='换货'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.tag[data-kind='重大'],
.tag[data-kind='退货'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.tag[data-kind='供应商'],
.tag[data-kind='特采'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.tag[data-kind='内部'],
.tag[data-kind='返工'] {
  color: #6554c0;
  background: #eae6ff;
}
.tag[data-kind='运输'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.tag[data-kind='让步接收'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status[data-status='待提交'],
.status[data-status='已关闭'] {
  color: var(--ui-text-muted);
}
.status[data-status='待审批'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='待执行'],
.status[data-status='执行中'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='已完成'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.actions {
  @apply flex items-center gap-1;
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
