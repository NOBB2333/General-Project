<script setup lang="ts">
import { Eye, Paperclip } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { PendingQcRecord } from '../../model/pending-qc.types';

defineProps<{ rows: readonly PendingQcRecord[] }>();
const emit = defineEmits<{ view: [row: PendingQcRecord] }>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>待检单号</th>
          <th>物料名称</th>
          <th>规格型号</th>
          <th>数量</th>
          <th>批次号</th>
          <th>供应商</th>
          <th>合同号</th>
          <th>到货日期</th>
          <th>状态</th>
          <th>关联检验单</th>
          <th>检验结果</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-emphasis="row.status === '部分合格'"
        >
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.id }}</button
            ><span
              v-if="row.stage"
              :class="$style.stage"
              :data-stage="row.stage"
              >{{ row.stage }}</span
            ><span
              v-if="row.attachmentCount"
              :class="$style.attachment"
              ><Paperclip :size="10" />{{ row.attachmentCount }}</span
            >
          </td>
          <td>
            <strong>{{ row.material }}</strong>
          </td>
          <td>{{ row.specification }}</td>
          <td>
            <strong>{{ row.quantity }}</strong>
          </td>
          <td>{{ row.batchCode }}</td>
          <td>{{ row.supplier }}</td>
          <td>{{ row.contractCode }}</td>
          <td>{{ row.arrivalDate }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <a
              href="#"
              @click.prevent="emit('view', row)"
              >{{ row.inspectionCode }}</a
            >
          </td>
          <td>
            <span
              v-for="result in row.result"
              :key="result"
              :class="$style.result"
              :data-result="result"
              >{{ result }}</span
            >
          </td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="查看待检详情"
              title="查看待检详情"
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
      暂无匹配的待检记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1510px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[55px] max-w-[165px] truncate border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller tr[data-emphasis='true'] {
  background: #fffaf5;
}
.codeButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
  color: var(--app-primary);
}
.stage,
.status,
.result {
  @apply ml-1 inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.stage {
  color: white;
  background: var(--ui-warning);
}
.stage[data-stage='特采审批中'] {
  background: #6554c0;
}
.attachment {
  @apply ml-1 inline-flex items-center gap-0.5 text-[9px];
  color: var(--ui-text-subtle);
}
.status[data-status='部分合格'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已入库'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.result {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.result[data-result='不合格'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.result[data-result*='特采'] {
  color: #6554c0;
  background: #eae6ff;
}
.scroller a {
  color: var(--app-primary);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
