<script setup lang="ts">
import { BadgeCheck, ShieldCheck } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { CreditLimitRow, PendingCreditRequest } from '../../model/credit-limits.types';

defineProps<{
  open: boolean;
  row: CreditLimitRow | null;
  request: PendingCreditRequest | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  freeze: [row: CreditLimitRow];
  approve: [request: PendingCreditRequest];
}>();

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row ? `${row.customer} · 授信详情` : request ? `${request.customer} · 授信审批` : '授信详情'"
    :description="
      row
        ? `${row.customerType} · ${row.riskStatus} · ${row.freezeStatus}`
        : request
          ? `${request.id} · 待审批`
          : undefined
    "
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="row"
      :class="$style.details"
    >
      <div>
        <dt>授信额度</dt>
        <dd>{{ money(row.limit) }}</dd>
      </div>
      <div>
        <dt>已用额度</dt>
        <dd>{{ money(row.used) }} · {{ Math.round((row.used / row.limit) * 100) }}%</dd>
      </div>
      <div>
        <dt>可用额度</dt>
        <dd :class="$style.available">{{ money(row.limit - row.used) }}</dd>
      </div>
      <div>
        <dt>超期金额 / 笔数</dt>
        <dd :class="row.overdueAmount ? $style.danger : undefined">
          {{ money(row.overdueAmount) }} / {{ row.overdueCount }} 笔
        </dd>
      </div>
      <div>
        <dt>风险状态</dt>
        <dd>{{ row.riskStatus }}</dd>
      </div>
      <div>
        <dt>负责人</dt>
        <dd>{{ row.owner }}</dd>
      </div>
    </dl>

    <dl
      v-else-if="request"
      :class="$style.details"
    >
      <div>
        <dt>申请客户</dt>
        <dd>{{ request.customer }}</dd>
      </div>
      <div>
        <dt>客户类型</dt>
        <dd>{{ request.customerType }}</dd>
      </div>
      <div>
        <dt>当前额度</dt>
        <dd>{{ money(request.currentLimit) }}</dd>
      </div>
      <div>
        <dt>申请额度</dt>
        <dd :class="$style.available">{{ money(request.requestedLimit) }}</dd>
      </div>
      <div :class="$style.wide">
        <dt>申请原因</dt>
        <dd>{{ request.reason }}</dd>
      </div>
      <div>
        <dt>申请人</dt>
        <dd>{{ request.owner }}</dd>
      </div>
      <div>
        <dt>申请日期</dt>
        <dd>{{ request.requestedAt }}</dd>
      </div>
    </dl>

    <template #icon>
      <span :class="$style.dialogIcon">
        <ShieldCheck
          v-if="row"
          :size="18"
          :stroke-width="1.8"
        />
        <BadgeCheck
          v-else
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >关闭</UiButton
      >
      <UiButton
        v-if="row"
        :variant="row.freezeStatus === '已冻结' ? 'primary' : 'danger'"
        size="sm"
        @click="emit('freeze', row)"
      >
        {{ row.freezeStatus === '已冻结' ? '解除冻结' : '冻结授信' }}
      </UiButton>
      <UiButton
        v-if="request"
        variant="primary"
        size="sm"
        @click="emit('approve', request)"
        >批准授信</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.details {
  @apply m-0 grid grid-cols-2 gap-4 p-5;
}

.details > div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.details dt,
.details dd {
  @apply m-0;
}

.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.details dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.wide {
  @apply col-span-2;
}

.available {
  color: var(--ui-success) !important;
}

.danger {
  color: var(--ui-danger) !important;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
