<script setup lang="ts">
import { Eye, Plane, ReceiptText, RotateCcw, TrainFront } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { TravelRecord } from '../../model/travel.types';

defineProps<{ rows: readonly TravelRecord[] }>();
const emit = defineEmits<{
  view: [row: TravelRecord];
  withdraw: [row: TravelRecord];
  reimburse: [row: TravelRecord];
}>();
const formatMoney = (value: number) => value.toLocaleString('zh-CN');
</script>

<template>
  <section :class="$style.frame">
    <table>
      <thead>
        <tr>
          <th>单号</th>
          <th>目的地</th>
          <th>交通方式</th>
          <th>出差事由</th>
          <th>出发日期</th>
          <th>返回日期</th>
          <th>天数</th>
          <th>预算</th>
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
              :class="$style.code"
              type="button"
              @click="emit('view', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td>
            <strong>{{ row.destination }}</strong>
          </td>
          <td>
            <span :class="$style.transport"
              ><Plane
                v-if="row.transport === '飞机'"
                :size="13"
                :stroke-width="1.8"
              /><TrainFront
                v-else
                :size="13"
                :stroke-width="1.8"
              />{{ row.transport }}</span
            >
          </td>
          <td>{{ row.reason }}</td>
          <td>{{ row.departureDate }}</td>
          <td>{{ row.returnDate }}</td>
          <td>
            <strong>{{ row.days }}</strong>
          </td>
          <td>
            <strong :class="$style.money">{{ formatMoney(row.budget) }}</strong>
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
                  :stroke-width="1.8"
              /></UiButton>
              <UiButton
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
              <UiButton
                v-if="row.status === '已完成'"
                variant="ghost"
                size="icon-sm"
                title="提交报销"
                aria-label="提交报销"
                @click="emit('reimburse', row)"
                ><ReceiptText
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
      暂无匹配的出差申请
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
  width: 120px;
}
th:nth-child(2) {
  width: 115px;
}
th:nth-child(3) {
  width: 110px;
}
th:nth-child(4) {
  width: 330px;
}
th:nth-child(5),
th:nth-child(6) {
  width: 120px;
}
th:nth-child(7) {
  width: 65px;
}
th:nth-child(8) {
  width: 95px;
}
th:nth-child(9) {
  width: 95px;
}
th:nth-child(10) {
  width: 105px;
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.transport {
  @apply inline-flex items-center gap-1;
}
.money {
  color: #ff8b00;
}
.status {
  @apply rounded px-2 py-1 text-[10px] font-medium;
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='待审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已报销'] {
  color: #6554c0;
  background: #eae6ff;
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
