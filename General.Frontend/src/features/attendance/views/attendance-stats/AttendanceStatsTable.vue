<script setup lang="ts">
import { Building2 } from 'lucide-vue-next';
import type { AttendanceStatsRow } from '../../model/attendance-stats.types';
defineProps<{ rows: readonly AttendanceStatsRow[] }>();
</script>

<template>
  <section :class="$style.panel">
    <h2>
      <Building2
        :size="17"
        :stroke-width="1.8"
      />部门考勤汇总表
    </h2>
    <table>
      <thead>
        <tr>
          <th>部门</th>
          <th>正常出勤</th>
          <th>迟到</th>
          <th>早退</th>
          <th>旷工</th>
          <th>出勤率</th>
          <th>平均工时</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.department"
        >
          <td>
            <strong>{{ row.department }}</strong>
          </td>
          <td>{{ row.normal }}</td>
          <td>{{ row.late }}</td>
          <td>{{ row.early }}</td>
          <td>{{ row.absent }}</td>
          <td>
            <b>{{ row.rate }}</b>
          </td>
          <td>{{ row.hours }}</td>
        </tr>
      </tbody>
    </table>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply mt-3 overflow-hidden rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.panel h2 {
  @apply flex items-center gap-2 text-[14px] font-semibold;
  color: #172b4d;
}
.panel h2 svg {
  color: #0052cc;
}
.panel table {
  @apply mt-3 w-full border-collapse text-left text-[11px];
}
.panel th {
  @apply border-b px-3 py-2 font-semibold;
  color: #6b778c;
  background: #f4f5f7;
  border-color: #dfe1e6;
}
.panel td {
  @apply border-b px-3 py-2;
  color: #42526e;
  border-color: #ebecf0;
}
.panel td strong {
  color: #172b4d;
}
.panel td b {
  color: #00875a;
}
</style>
