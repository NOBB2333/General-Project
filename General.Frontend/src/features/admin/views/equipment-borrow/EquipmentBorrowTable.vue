<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import type { EquipmentBorrow } from '../../model/equipment-borrow.types';
defineProps<{ rows: readonly EquipmentBorrow[] }>();
const emit = defineEmits<{ view: [item: EquipmentBorrow] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>借用编号</th>
          <th>申请人</th>
          <th>借用物品</th>
          <th>类型</th>
          <th>数量</th>
          <th>关联业务</th>
          <th>借用日期</th>
          <th>归还日期</th>
          <th>状态</th>
          <th>审批进度</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong :class="$style.code">{{ row.code }}</strong>
          </td>
          <td>
            <span :class="$style.avatar">{{ row.applicant.slice(0, 1) }}</span
            ><strong>{{ row.applicant }}</strong
            ><small>{{ row.department }}</small>
          </td>
          <td>
            <strong>{{ row.item }}</strong
            ><small>{{ row.assetCode }}</small>
          </td>
          <td>
            <span :class="$style.type">{{ row.type }}</span>
          </td>
          <td>{{ row.quantity }}</td>
          <td>
            <span :class="$style.business">{{ row.business }}</span>
          </td>
          <td>{{ row.borrowedAt }}</td>
          <td>
            <strong :data-overdue="Boolean(row.overdueHint)">{{ row.dueAt }}</strong
            ><small
              v-if="row.overdueHint"
              :class="$style.overdue"
              >{{ row.overdueHint }}</small
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>{{ row.approvers }}</td>
          <td>
            <button
              aria-label="查看借用记录"
              @click="emit('view', row)"
            >
              <Eye
                :size="14"
                :stroke-width="1.8"
              />查看
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有借用记录
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1520px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.scroller td {
  @apply h-[62px] whitespace-nowrap border-b px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td strong {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller td small {
  @apply mt-1 block text-[8px];
  color: var(--ui-text-subtle);
}
.code {
  color: var(--app-primary) !important;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[8px] font-semibold text-white;
  background: #0052cc;
}
.type,
.status {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: #6554c0;
  background: #eae6ff;
}
.status {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='已归还'] {
  color: #6b778c;
  background: #ebecf0;
}
.status[data-status='逾期未还'],
.status[data-status='损坏报废'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-status='使用中'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='待审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.business {
  @apply block max-w-[210px] truncate;
}
.scroller strong[data-overdue='true'],
.overdue {
  color: #de350b !important;
}
.scroller td:last-child button {
  @apply inline-flex items-center gap-1 font-semibold;
  color: var(--app-primary);
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
