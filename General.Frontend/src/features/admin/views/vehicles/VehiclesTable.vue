<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import type { VehicleRequest } from '../../model/vehicles.types';
defineProps<{ rows: readonly VehicleRequest[] }>();
const emit = defineEmits<{ view: [item: VehicleRequest] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>申请编号</th>
          <th>申请人</th>
          <th>用途/路线</th>
          <th>用车时间</th>
          <th>里程/车型</th>
          <th>派车</th>
          <th>状态</th>
          <th>审批</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong :class="$style.code">{{ row.code }}</strong
            ><small>{{ row.department }}</small>
          </td>
          <td>
            <span :class="$style.avatar">{{ row.applicant.slice(0, 1) }}</span
            ><strong>{{ row.applicant }}</strong>
          </td>
          <td>
            <strong>{{ row.purpose }}</strong
            ><small>{{ row.route }}</small>
          </td>
          <td>{{ row.time }}</td>
          <td>
            <strong>{{ row.mileage }}</strong
            ><small :class="$style.vehicleType">{{ row.vehicleType }}</small>
          </td>
          <td>{{ row.dispatch }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.approver }}</td>
          <td>
            <button
              aria-label="查看用车申请"
              @click="emit('view', row)"
            >
              <Eye
                :size="14"
                :stroke-width="1.8"
              />
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有用车申请
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1450px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.scroller td {
  @apply h-[70px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller td small {
  @apply mt-1 block max-w-[270px] truncate text-[8px];
  color: var(--ui-text-subtle);
}
.code {
  color: var(--app-primary) !important;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[8px] font-semibold text-white;
  background: #0052cc;
}
.vehicleType {
  @apply inline-block rounded px-2 py-1;
  color: #00875a !important;
  background: #e3fcef;
}
.status {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已派车'],
.status[data-status='已通过'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-status='已用车'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.scroller td:last-child button {
  @apply rounded p-1.5;
  color: var(--app-primary);
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
