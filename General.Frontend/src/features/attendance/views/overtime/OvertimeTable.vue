<script setup lang="ts">
import { Eye, RotateCcw } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { OvertimeRecord } from '../../model/overtime.types';

defineProps<{ rows: readonly OvertimeRecord[] }>();
const emit = defineEmits<{ view: [row: OvertimeRecord]; withdraw: [row: OvertimeRecord] }>();
</script>

<template>
  <section :class="$style.frame">
    <table>
      <thead>
        <tr>
          <th>单号</th>
          <th>加班日期</th>
          <th>时段</th>
          <th>时长</th>
          <th>事由</th>
          <th>补偿方式</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('view', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td>{{ row.date }} <span :class="$style.moon">◔</span></td>
          <td>{{ row.period }}</td>
          <td>
            <strong>{{ row.hours }}h</strong>
          </td>
          <td>
            {{ row.reason }} <span :class="$style.project">项目 ({{ row.projectCount }})</span>
          </td>
          <td>
            <span
              :class="$style.compensation"
              :data-compensation="row.compensation"
              >{{ row.compensationLabel }}</span
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
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
                v-if="row.status === '待审批'"
                variant="ghost"
                size="icon-sm"
                title="撤回申请"
                aria-label="撤回申请"
                @click="emit('withdraw', row)"
                ><RotateCcw
                  :size="14"
                  :stroke-width="1.8"
              /></UiButton>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的加班申请
    </div>
    <footer>共 {{ rows.length }} 条记录</footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.frame {
  @apply mt-3 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
table {
  @apply w-full border-collapse text-left text-[11px];
}
th {
  @apply h-9 px-4 font-semibold;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
td {
  @apply h-11 border-t px-4;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
th:nth-child(1) {
  width: 125px;
}
th:nth-child(2) {
  width: 140px;
}
th:nth-child(3) {
  width: 145px;
}
th:nth-child(4) {
  width: 75px;
}
th:nth-child(5) {
  width: 520px;
}
th:nth-child(6) {
  width: 145px;
}
th:nth-child(7) {
  width: 105px;
}
th:nth-child(8) {
  width: 105px;
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.moon {
  color: #6554c0;
}
.project {
  @apply ml-1 rounded bg-[#DEEBFF] px-1.5 py-0.5 text-[9px] text-[#0052CC];
}
.compensation,
.status {
  @apply rounded px-2 py-1 text-[10px] font-medium;
}
.compensation {
  color: #00875a;
  background: #e3fcef;
}
.compensation[data-compensation='加班费'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='待审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-status='已撤回'] {
  color: #6b778c;
  background: #ebecf0;
}
.actions {
  @apply flex items-center;
}
.empty {
  @apply border-t py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.frame > footer {
  @apply border-t px-4 py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
</style>
