<script setup lang="ts">
import type { MeetingNote } from '../../model/meeting-notes.types';
defineProps<{ rows: readonly MeetingNote[] }>();
const emit = defineEmits<{ view: [note: MeetingNote] }>();
</script>
<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>编号</th>
          <th>会议主题</th>
          <th>日期/时间</th>
          <th>主持人</th>
          <th>参会人</th>
          <th>待办</th>
          <th>状态</th>
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
            <strong>{{ row.code }}</strong>
          </td>
          <td>
            <b>{{ row.title }}</b
            ><small :data-type="row.type">{{ row.type }}</small>
          </td>
          <td>
            <b>{{ row.date }}</b
            ><small>{{ row.time }}</small>
          </td>
          <td>
            <span :class="$style.avatar">{{ row.host.slice(0, 1) }}</span
            ><b>{{ row.host }}</b>
          </td>
          <td>
            <span :class="$style.people"
              ><i
                v-for="person in row.attendees"
                :key="person"
                >{{ person }}</i
              ><i v-if="row.attendeeExtra">+{{ row.attendeeExtra }}</i></span
            >
          </td>
          <td>
            <b>{{ row.todoCount }}</b
            ><em>{{ row.pendingTodoCount }}待办</em>
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
        </tr>
      </tbody>
    </table>
    <p
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有会议纪要
    </p>
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1200px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: #f4f5f7;
}
.scroller td {
  @apply h-[62px] whitespace-nowrap border-b px-4;
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
.scroller b {
  @apply font-semibold;
  color: var(--ui-text);
}
.scroller small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.scroller td:nth-child(2) small {
  @apply inline-flex rounded px-2 py-1;
  font-weight: 600;
  color: #0052cc;
  background: #deebff;
}
.scroller td:nth-child(2) small[data-type='经营分析'] {
  color: #6554c0;
  background: #eae6ff;
}
.scroller td:nth-child(2) small[data-type='技术评审'] {
  color: #0065ff;
  background: #e6fcff;
}
.scroller td:nth-child(2) small[data-type='全体会议'] {
  color: #de350b;
  background: #ffebe6;
}
.scroller td:nth-child(2) small[data-type='专题讨论'] {
  color: #ff8b00;
  background: #fff0b3;
}
.avatar {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[9px] font-semibold text-white;
  background: var(--app-primary);
}
.people {
  @apply flex;
}
.people i {
  @apply -ml-1 flex h-6 w-6 items-center justify-center rounded-full border-2 border-white text-[8px] not-italic;
  background: #ebecf0;
}
.people i:first-child {
  @apply ml-0;
}
.scroller td:nth-child(6) em {
  @apply ml-2 rounded px-2 py-1 text-[8px] not-italic;
  color: #ff8b00;
  background: #fff0b3;
}
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='已归档'] {
  color: #0052cc;
  background: #deebff;
}
.status[data-status='草稿'] {
  color: #6b778c;
  background: #ebecf0;
}
.empty {
  @apply py-14 text-center;
  color: var(--ui-text-subtle);
}
</style>
