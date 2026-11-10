<script setup lang="ts">
import { Eye } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { GiftApplication } from '../../model/gifts.types';

defineProps<{ rows: readonly GiftApplication[] }>();
const emit = defineEmits<{ view: [application: GiftApplication] }>();

const currency = new Intl.NumberFormat('zh-CN', {
  style: 'currency',
  currency: 'CNY',
  maximumFractionDigits: 0,
});
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>编号</th>
          <th>申请标题</th>
          <th>类别</th>
          <th>赠送对象</th>
          <th>金额</th>
          <th>申请日期</th>
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
            <strong>{{ row.code }}</strong>
          </td>
          <td>
            <b>{{ row.title }}</b
            ><small>{{ row.project }}</small>
          </td>
          <td>
            <span
              :class="$style.category"
              :data-category="row.category"
              >{{ row.category }}</span
            >
          </td>
          <td>
            <span :class="$style.recipientType">{{ row.recipientType }}</span
            >{{ row.recipient }}
          </td>
          <td>
            <b>{{ currency.format(row.amount) }}</b>
          </td>
          <td>{{ row.appliedAt }}</td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <div
              :class="$style.approval"
              :data-step="row.approvalStep"
            >
              <i
                v-for="step in 3"
                :key="step"
                :data-complete="step <= row.approvalStep"
              />
            </div>
          </td>
          <td>
            <UiButton
              variant="ghost"
              size="icon-sm"
              aria-label="查看礼品申请"
              title="查看礼品申请"
              @click="emit('view', row)"
            >
              <Eye
                :size="14"
                :stroke-width="1.8"
              />
            </UiButton>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前条件下没有礼品申请
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
}

.scroller table {
  @apply w-full min-w-[1280px] border-collapse text-left text-[10px];
}

.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: #f4f5f7;
}

.scroller td {
  @apply h-[58px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.scroller td:first-child strong {
  color: var(--app-primary);
}

.scroller td:nth-child(2) b,
.scroller td:nth-child(5) b {
  @apply block font-semibold;
  color: var(--ui-text);
}

.scroller td:nth-child(2) small {
  @apply mt-1 block;
  color: var(--ui-text-subtle);
}

.category,
.recipientType,
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
}

.category {
  color: #0052cc;
  background: #deebff;
}

.category[data-category='节日礼品'] {
  color: #de350b;
  background: #ffebe6;
}

.category[data-category='员工福利'] {
  color: #6554c0;
  background: #eae6ff;
}

.category[data-category='活动礼品'] {
  color: #00875a;
  background: #e3fcef;
}

.recipientType {
  @apply mr-1.5;
  color: #0052cc;
  background: #deebff;
}

.status[data-status='待审核'] {
  color: #ff8b00;
  background: #fff0b3;
}

.status[data-status='审核中'] {
  color: #0052cc;
  background: #deebff;
}

.status[data-status='已通过'],
.status[data-status='已发放'] {
  color: #00875a;
  background: #e3fcef;
}

.status[data-status='已驳回'] {
  color: #de350b;
  background: #ffebe6;
}

.approval {
  @apply flex items-center gap-2;
}

.approval i {
  @apply relative h-4 w-4 rounded-full border;
  border-color: #dfe1e6;
}

.approval i + i::before {
  @apply absolute right-full top-1/2 h-px w-2;
  content: '';
  background: #dfe1e6;
}

.approval i[data-complete='true'] {
  border-color: #00875a;
  box-shadow: inset 0 0 0 4px white;
  background: #00875a;
}

.empty {
  @apply py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
