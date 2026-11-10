<script setup lang="ts">
import { FileText } from 'lucide-vue-next';
import type { SealApplication } from '../../model/seals.types';

defineProps<{ rows: readonly SealApplication[] }>();
const emit = defineEmits<{ view: [application: SealApplication] }>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>申请编号</th>
          <th>公司抬头</th>
          <th>申请人</th>
          <th>印章类型</th>
          <th>文件名称</th>
          <th>用途</th>
          <th>状态</th>
          <th>申请日期</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          tabindex="0"
          @click="emit('view', row)"
          @keydown.enter="emit('view', row)"
        >
          <td>
            <strong>{{ row.code }}</strong
            ><em
              v-if="row.urgency"
              :data-urgency="row.urgency"
              >{{ row.urgency }}</em
            >
          </td>
          <td>
            <b>{{ row.company }}</b>
          </td>
          <td>
            <span :class="$style.avatar">{{ row.applicant.slice(0, 1) }}</span
            ><span :class="$style.person"
              ><b>{{ row.applicant }}</b
              ><small>{{ row.department }}</small></span
            >
          </td>
          <td>
            <span
              :class="$style.sealType"
              :data-type="row.sealType"
              ><FileText
                :size="13"
                :stroke-width="1.8"
              />{{ row.sealType }}</span
            >
          </td>
          <td>
            <b>{{ row.fileName }}</b
            ><small>{{ row.fileType }} · {{ row.copies }}份</small>
          </td>
          <td>{{ row.purpose }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.appliedAt }}</td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有用章申请
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1320px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: #f4f5f7;
}
.scroller td {
  @apply h-[58px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller tbody tr {
  @apply cursor-pointer outline-none transition-colors;
}
.scroller tbody tr:hover {
  background: var(--ui-surface-hover);
}
.scroller tbody tr:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}
.scroller td:first-child strong {
  color: var(--app-primary);
}
.scroller td b {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller td small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.scroller td:first-child em {
  @apply ml-2 rounded px-1.5 py-0.5 text-[8px] not-italic;
  color: #ff8b00;
  background: #fff0b3;
}
.scroller td:first-child em[data-urgency='特急'] {
  color: #de350b;
  background: #ffebe6;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[9px] font-semibold text-white;
  background: var(--app-primary);
}
.person {
  @apply inline-block align-middle;
}
.sealType {
  @apply inline-flex min-w-[126px] items-center gap-1.5 rounded px-2 py-1 font-semibold;
  color: #0052cc;
  background: #deebff;
}
.sealType[data-type='公章'] {
  color: #de350b;
  background: #ffebe6;
}
.sealType[data-type='财务专用章'] {
  color: #ff8b00;
  background: #fff0b3;
}
.sealType[data-type='部门章'] {
  color: #6554c0;
  background: #eae6ff;
}
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已通过'],
.status[data-status='审批中'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已盖章'],
.status[data-status='已归还'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.empty {
  @apply py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
