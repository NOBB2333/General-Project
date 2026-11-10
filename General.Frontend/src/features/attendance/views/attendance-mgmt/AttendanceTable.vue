<script setup lang="ts">
import { Eye, FilePenLine } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { AttendanceRecord } from '../../model/attendance-mgmt.types';

defineProps<{ rows: readonly AttendanceRecord[]; total: number }>();
const emit = defineEmits<{ view: [row: AttendanceRecord]; correct: [row: AttendanceRecord] }>();
</script>

<template>
  <section :class="$style.frame">
    <div :class="$style.scroller">
      <table>
        <thead>
          <tr>
            <th>日期</th>
            <th>星期</th>
            <th>姓名</th>
            <th>部门</th>
            <th>规定上班</th>
            <th>规定下班</th>
            <th>实际签到</th>
            <th>实际签退</th>
            <th>工时</th>
            <th>状态</th>
            <th>迟到(分)</th>
            <th>早退(分)</th>
            <th>备注</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
            :data-status="row.status"
          >
            <td>
              <strong>{{ row.date }}</strong>
            </td>
            <td>{{ row.weekday }}</td>
            <td>
              <span :class="$style.person"
                ><i>{{ row.name.slice(0, 1) }}</i
                ><strong>{{ row.name }}</strong></span
              >
            </td>
            <td>{{ row.department }}</td>
            <td>{{ row.scheduledIn }}</td>
            <td>{{ row.scheduledOut }}</td>
            <td :class="$style.clockIn">{{ row.actualIn }}</td>
            <td :class="$style.clockIn">{{ row.actualOut }}</td>
            <td>
              <strong>{{ row.hours }}</strong>
            </td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>
              <span
                v-if="row.lateMinutes !== null"
                :class="$style.minutes"
                >{{ row.lateMinutes }}</span
              ><span v-else>-</span>
            </td>
            <td>{{ row.earlyMinutes ?? '-' }}</td>
            <td>{{ row.remark }}</td>
            <td>
              <div :class="$style.actions">
                <UiButton
                  variant="ghost"
                  size="icon-sm"
                  title="查看详情"
                  aria-label="查看详情"
                  @click="emit('view', row)"
                  ><Eye
                    :size="14"
                    :stroke-width="1.8" /></UiButton
                ><UiButton
                  v-if="row.status === '迟到'"
                  variant="ghost"
                  size="icon-sm"
                  title="提交补签"
                  aria-label="提交补签"
                  @click="emit('correct', row)"
                  ><FilePenLine
                    :size="14"
                    :stroke-width="1.8"
                /></UiButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的考勤记录
    </div>
    <footer>
      <span>共 {{ total }} 条记录</span><strong>1</strong>
    </footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.frame {
  @apply mt-3 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.scroller {
  @apply overflow-x-auto;
}
table {
  @apply min-w-[1390px] w-full border-collapse text-left text-[10px];
}
th {
  @apply h-9 whitespace-nowrap px-3 font-semibold;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
td {
  @apply h-11 border-t px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
tr[data-status='迟到'] {
  background: #fffaf5;
}
.person {
  @apply flex items-center gap-2;
}
.person i {
  @apply flex h-6 w-6 items-center justify-center rounded-full bg-[#0052CC] text-[9px] not-italic text-white;
}
.clockIn {
  @apply font-semibold;
  color: #00875a;
}
.status {
  @apply rounded px-2 py-1 text-[9px] font-medium;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='迟到'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='出差'] {
  color: #0052cc;
  background: #deebff;
}
.minutes {
  @apply rounded bg-[#FFEBE6] px-2 py-1 font-semibold text-[#DE350B];
}
.actions {
  @apply flex items-center;
}
.empty {
  @apply py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.frame > footer {
  @apply flex items-center justify-between border-t px-4 py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.frame > footer strong {
  @apply flex h-7 w-7 items-center justify-center rounded bg-[#0052CC] text-white;
}
</style>
