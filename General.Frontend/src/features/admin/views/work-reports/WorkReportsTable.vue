<script setup lang="ts">
import type { WorkReport } from '../../model/work-reports.types';
defineProps<{ rows: readonly WorkReport[] }>();
const emit = defineEmits<{ view: [report: WorkReport] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>汇报标题</th>
          <th>类型</th>
          <th>汇报周期</th>
          <th>审批人</th>
          <th>状态</th>
          <th>提交时间</th>
          <th>评分</th>
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
            <strong>{{ row.title }}</strong
            ><small v-if="row.rule">规则：{{ row.rule }}</small>
          </td>
          <td>
            <span
              :class="$style.type"
              :data-type="row.type"
              >{{ row.type }}</span
            >
          </td>
          <td>{{ row.period }}</td>
          <td>
            <span :class="$style.avatar">王</span><b>{{ row.approver }}</b>
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.submittedAt }}</td>
          <td>
            <strong :class="$style.score">{{ row.score ?? '-' }}</strong>
          </td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有工作汇报
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1160px] border-collapse text-left text-[10px];
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
.scroller td:first-child strong,
.scroller b {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller td:first-child small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.type,
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
}
.type {
  color: #0052cc;
  background: #deebff;
}
.type[data-type='项目汇报'] {
  color: #00875a;
  background: #e3fcef;
}
.type[data-type='月报'] {
  color: #6554c0;
  background: #eae6ff;
}
.type[data-type='日报'] {
  color: #ff8b00;
  background: #fff0b3;
}
.type[data-type='专项汇报'] {
  color: #de350b;
  background: #ffebe6;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[9px] font-semibold text-white;
  background: #6554c0;
}
.status {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-status='待审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='草稿'] {
  color: #6b778c;
  background: #ebecf0;
}
.score {
  color: var(--app-primary) !important;
}
.empty {
  @apply py-14 text-center;
  color: var(--ui-text-subtle);
}
</style>
