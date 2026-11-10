<script setup lang="ts">
import { CalendarDays, UserRound } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { MaterialRequest } from '../../model/material-pickup.types';
defineProps<{ rows: readonly MaterialRequest[] }>();
const emit = defineEmits<{ view: [item: MaterialRequest]; process: [item: MaterialRequest] }>();
</script>
<template>
  <div :class="$style.list">
    <article
      v-for="row in rows"
      :key="row.id"
      :data-status="row.status"
      @dblclick="emit('view', row)"
    >
      <header>
        <button @click="emit('view', row)">{{ row.code }}</button><span :data-status="row.status">{{ row.status }}</span
        ><UiButton
          v-if="row.status === '待审批' || row.status === '已通过'"
          size="sm"
          :variant="row.status === '待审批' ? 'secondary' : 'primary'"
          @click="emit('process', row)"
          >{{ row.status === '待审批' ? '审批' : '确认领取' }}</UiButton
        >
      </header>
      <p :class="$style.meta">
        <UserRound
          :size="12"
          :stroke-width="1.8"
        />{{ row.applicant }}　{{ row.department }}　<CalendarDays
          :size="12"
          :stroke-width="1.8"
        />{{ row.date }}
      </p>
      <strong>申请事由: {{ row.reason }}</strong>
      <div :class="$style.items">
        <span
          v-for="line in row.items"
          :key="line.name"
          >◇ {{ line.name }} ×{{ line.quantity }}</span
        >
      </div>
      <small v-if="row.comment">审批意见: {{ row.comment }}</small>
    </article>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有领取申请
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.list {
  @apply space-y-3;
}
.list article {
  @apply rounded border bg-white p-5;
  border-color: var(--ui-border);
}
.list article[data-status='已驳回'] {
  border-color: #ffbdad;
}
.list header {
  @apply flex items-center gap-2;
}
.list header > button {
  @apply text-[13px] font-semibold;
  color: var(--app-primary);
}
.list header > span {
  @apply rounded-full px-2 py-1 text-[8px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.list header > span[data-status='已领取'] {
  color: #0052cc;
  background: #deebff;
}
.list header > span[data-status='待审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.list header > span[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.list header > button:last-child {
  @apply ml-auto;
}
.meta {
  @apply mt-3 flex items-center gap-1 text-[9px];
  color: var(--ui-text-subtle);
}
.list article > strong {
  @apply mt-3 block text-[10px] font-medium;
}
.items {
  @apply mt-2 flex gap-2;
}
.items span {
  @apply rounded px-2 py-1 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.list article > small {
  @apply mt-2 block text-[9px];
  color: #de350b;
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
</style>
