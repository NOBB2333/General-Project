<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { QcOrderRecord } from '../../model/qc-orders.types';

defineProps<{ rows: readonly QcOrderRecord[] }>();
const emit = defineEmits<{ view: [row: QcOrderRecord] }>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>检验单号</th>
          <th>物料名称</th>
          <th>规格型号</th>
          <th>数量</th>
          <th>供应商</th>
          <th>来源单号</th>
          <th>到货日期</th>
          <th>状态</th>
          <th>检验结果</th>
          <th>检验员</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-failed="row.result === '不合格'"
        >
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.id }}
            </button>
          </td>
          <td>
            <strong>{{ row.material }}</strong>
          </td>
          <td>{{ row.specification }}</td>
          <td>
            <strong>{{ row.quantity }}</strong>
          </td>
          <td>{{ row.supplier }}</td>
          <td>{{ row.sourceCode }}</td>
          <td>{{ row.arrivalDate }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <span
              :class="$style.result"
              :data-result="row.result"
              >{{ row.result }}</span
            >
          </td>
          <td>{{ row.inspector }}</td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="查看检验详情"
              title="查看检验详情"
              @click="emit('view', row)"
              ><Eye :size="14"
            /></UiButton>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的检验单
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1400px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[48px] max-w-[185px] truncate whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller tr[data-failed='true'] {
  background: #fff6f5;
}
.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
  color: var(--app-primary);
}
.status,
.result {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.status[data-status='已完成'],
.result[data-result='合格'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status[data-status='待检验'] {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.status[data-status='待审核'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.result[data-result='不合格'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.result[data-result='-'] {
  color: var(--ui-text-subtle);
  background: transparent;
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
