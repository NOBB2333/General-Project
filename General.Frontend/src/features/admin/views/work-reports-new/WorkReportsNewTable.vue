<script setup lang="ts">
import type { WorkReportNewMode, WorkReportNewSubmission } from '../../model/work-reports-new.types';

defineProps<{ rows: readonly WorkReportNewSubmission[]; mode: WorkReportNewMode }>();
</script>

<template>
  <div
    v-if="mode === 'summary'"
    :class="$style.scroller"
  >
    <table>
      <thead>
        <tr>
          <th>人员</th>
          <th>本周关键成果</th>
          <th>工作事项明细</th>
          <th>下周计划</th>
          <th>问题与风险</th>
          <th>AI 评分</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-submitted="row.submitted"
        >
          <td>
            <strong>{{ row.person }}</strong>
          </td>
          <template v-if="row.submitted">
            <td>{{ row.keyResults }}</td>
            <td>{{ row.details }}</td>
            <td>{{ row.nextPlan }}</td>
            <td>{{ row.risks }}</td>
            <td>
              <b>{{ row.score }}</b>
            </td>
          </template>
          <td
            v-else
            colspan="5"
          >
            <em>未提交</em>
          </td>
        </tr>
      </tbody>
    </table>
    <span :class="$style.scrollHint">↔ 左右滑动</span>
  </div>
  <div
    v-else
    :class="$style.peopleGrid"
  >
    <article
      v-for="row in rows"
      :key="row.id"
      :data-submitted="row.submitted"
    >
      <span>{{ row.person.slice(0, 1) }}</span>
      <div>
        <strong>{{ row.person }}</strong>
        <p>{{ row.submitted ? row.keyResults : '本期汇报未提交' }}</p>
      </div>
      <b>{{ row.submitted ? `${row.score} 分` : '未提交' }}</b>
    </article>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply relative overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1640px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.scroller th:first-child,
.scroller td:first-child {
  @apply sticky left-0 z-[1] w-[120px] bg-white;
}
.scroller td {
  @apply max-w-[340px] border-b px-4 py-3 align-top leading-5;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td:first-child strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller tr[data-submitted='false'] {
  background: #fffafa;
}
.scroller em {
  @apply not-italic;
  color: #ff8b00;
}
.scroller td b {
  @apply text-[13px] font-semibold;
  color: #5243aa;
}
.scrollHint {
  @apply absolute right-3 top-2 rounded-full px-2 py-1 text-[9px] font-semibold;
  color: #0052cc;
  background: #deebff;
}
.peopleGrid {
  @apply grid grid-cols-2 gap-3 p-4;
}
.peopleGrid article {
  @apply flex min-h-[88px] items-center gap-3 rounded border p-4;
  border-color: var(--ui-border);
}
.peopleGrid article > span {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-[11px] font-semibold text-white;
  background: #6554c0;
}
.peopleGrid article > div {
  @apply min-w-0 flex-1;
}
.peopleGrid strong {
  @apply text-[11px];
}
.peopleGrid p {
  @apply mt-1 truncate text-[9px];
  color: var(--ui-text-subtle);
}
.peopleGrid article > b {
  @apply text-[10px];
  color: #00875a;
}
.peopleGrid article[data-submitted='false'] > b {
  color: #ff8b00;
}
</style>
